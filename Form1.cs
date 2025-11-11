using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Windows.Forms;
using _014.Handlers.AngleMeasurement;
using _014.Managers.ClearancePlane;
using _014.Managers.Data;
using _014.Managers.Selection;
using _014.Managers.Toolpath;
using _014.Managers.View;
using _014.Measurements.Surface;
using _014.Probe.Visualization;
using _014.Utilities.Collision;
using _014.Utilities.FileOperations;
using _014.Utilities.UI;
using devDept;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;  // ✅ Surface, Brep için
using devDept.Eyeshot.Translators;
using devDept.Geometry;  // ✅ Point3D, Vector3D için
namespace _014
{
    /// <summary>
    /// Ana form - CNC Measurement
    /// ✅ DÜZELTİLMİŞ: InstructionPanel Shown event ile başlatılıyor
    /// ✅ YENİ: FaceMeasurementAnalyzer eklendi
    /// PARTIAL CLASS 1/5: Fields, Constructor, Form Load
    /// </summary>
    public partial class CNC_Measurement : Form
    {
        // ═══════════════════════════════════════════════════════════
        // FIELDS - TÜM FIELD'LAR BURADA TANIMLI
        // ═══════════════════════════════════════════════════════════

        private FileImporter fileImporter;
        private ImportToMeshForCollision importToMeshForCollision;  // ✅ YENİ: Collision için mesh cache
        private SelectionManager selectionManager;
        private ProbeColorAnimator probeAnimator;
        private ViewManager viewManager;
        private DataManager dataManager;  // ✅ YENİ: Shared data manager
        private SurfaceMeasurementAnalyzer surfaceMeasurementAnalyzer;
        private LengthMeasurementAnalyzer lengthAnalyzer;  // ✅ YENİ: Length measurement
        private bool isLengthModeActive = false;
        private TreeViewManager treeViewManager;  // ✅ YENİ: TreeView CNC Configuration manager
        private ClearancePlaneManager clearancePlaneManager;  // ✅ YENİ: Clearance Plane manager
        private ToolpathManager toolpathManager;  // ✅ YENİ: Toolpath manager ve simülasyon

        // ✅ Face to Face Manager (eski 20+ değişken yerine tek manager)
        private FaceToFaceManager? faceToFaceManager;

        // ✅ Edge to Edge Manager
        private EdgeToEdgeManager? edgeToEdgeManager;

        // ✅ Ridge Width Handler
        private RidgeWidthHandler ridgeWidthHandler;

        // ✅ Angle Measurement Manager
        private AngleMeasurementManager? angleMeasurementManager;



        private const string MEASUREMENT_LAYER_NAME = "MeasurementLines";
        private InstructionPanel? instructionPanel;
        private SurfaceToSurfaceMeasurement surfaceToSurfaceMeasurement;
        private bool isSurfaceToSurfaceActive = false;

        // ✅ YENİ: Clearance Plane minimum değeri (import'tan sonra ayarlanır)
        private double minimumClearancePlane = 50.0; // Varsayılan minimum


        // Açık pencere listesi (static - tüm pencereler tarafından paylaşılıyor)
        private static List<CNC_Measurement> openWindows = new List<CNC_Measurement>();

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        public CNC_Measurement()
        {
            InitializeComponent();

            // ✅ ESC tuşunu yakalamak için KeyPreview aktif
            this.KeyPreview = true;
            this.KeyDown += CNC_Measurement_KeyDown;

            // ✅ Measurement layer'ını oluştur
            CreateMeasurementLayer();

            // ✅ Eski snap menü itemlerini gizle (artık sağ tık menüsünde)
            HideOldSnapMenuItems();

            // ✅ Dil sistemi
            InitializeLanguageEvents();
            LoadSavedLanguage();

            // ✅ DataManager'ı başlat (FileImporter ve SelectionManager paylaşacak)
            dataManager = new DataManager();

            // ✅ ImportToMeshForCollision başlat (Collision için mesh cache)
            importToMeshForCollision = new ImportToMeshForCollision(design1);

            // ✅ FileImporter'a dataManager'ı geç
            fileImporter = new FileImporter(design1, this, importToMeshForCollision, dataManager);

            // Diğer yardımcı sınıfları başlat
            probeAnimator = new ProbeColorAnimator(design1);
            viewManager = new ViewManager(design1);

            // ⛔ REMOVED: FaceMeasurementAnalyzer (boşaltıldı)

            // Pencere listesine ekle
            openWindows.Add(this);

            // Pencere kapanınca listeden çıkar ve panel'i kapat
            this.FormClosed += (s, e) =>
            {
                openWindows.Remove(this);

                // ✅ Panel'i de kapat
                if (instructionPanel != null && !instructionPanel.IsDisposed)
                {
                    instructionPanel.Close();
                    instructionPanel.Dispose();
                }

                // ⛔ REMOVED: FaceMeasurementAnalyzer cleanup
            };

            // Pencere focus aldığında Window menüsünü güncelle
            this.Activated += (s, e) => UpdateWindowMenu();

            // Form Load event handler'ını ekle
            this.Load += CNC_Measurement_Load;

            // ✅ CRITICAL: Shown event ekle (InstructionPanel için)


            // ✅ CNC Machine ComboBox event (Sol panel)
            cmb_form1_cnc_machine.SelectedIndexChanged += cmb_form1_cnc_machine_SelectedIndexChanged;

            // ✅ Probe ComboBox event (Sol panel)
            cmb_form1_probe.SelectedIndexChanged += cmb_form1_probe_SelectedIndexChanged;

            // ✅ CNC Machine ComboBox sağ tık menüsü
            InitializeCNCMachineContextMenu();

            // ✅ Probe ComboBox sağ tık menüsü
            InitializeProbeContextMenu();

            // ✅ Clearance Plane TextBox event'leri
            txt_form1_Clerance.KeyPress += txt_form1_Clerance_KeyPress;
            txt_form1_Clerance.Leave += txt_form1_Clerance_Leave; // TextBox'tan çıkınca kontrol

            // ✅ ClearancePlaneManager başlat
            clearancePlaneManager = new ClearancePlaneManager(design1);
            clearancePlaneManager.DrawClearancePlane();  // İlk çizim

            // ✅ YENİ: Clearance Plane Checkbox event'i
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;

            // KALDIRILDI: AngleMeasurementManager artik Form_Load'da olusturulacak (treeViewManager'dan SONRA!)


            this.Shown += CNC_Measurement_Shown;
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            // ✅ Point Probing modunu kapat
            selectionManager?.DisablePointProbing();
            
            // Diğer modları kapat
            faceToFaceManager?.Disable();
            edgeToEdgeManager?.Disable();
            ridgeWidthHandler?.DisablePointSelection();

            // Angle Measurement modunu aktif et
            angleMeasurementManager?.Enable(instructionPanel);

            Debug.WriteLine("🎯 Angle Measurement butonu tıklandı");
        }
    }
}
