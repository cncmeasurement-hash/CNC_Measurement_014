using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _014.Managers.Data;
using _014.Managers.Selection;
using _014.Utilities.Collision;
using _014.Utilities.UI;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// RIDGE_WIDTH ölçüm sistemi
    /// İki nokta seçerek ridge (sırt/kenar) genişliğini ölçer
    /// </summary>
    public partial class RidgeWidthHandler
    {
        private TreeViewManager treeViewManager;
        private Design design;
        private DataManager dataManager;
        private InstructionPanel instructionPanel;  // ✅ YENİ: InstructionPanel referansı
        private SelectionManager selectionManager;  // ✅ YENİ: SelectionManager referansı
        private bool isPointSelectionActive = false;  // Nokta seçimi aktif mi?
        private int selectedPointCount = 0;           // Kaç nokta seçildi?
        
        // ✅ YENİ: Probe collision için
        private ImportToMeshForCollision meshConverter;
        private CollisionDetector collisionDetector;
        
        // Marker için
        private List<Point3D> selectedPoints = new List<Point3D>();
        private List<Entity> pointMarkers = new List<Entity>();
        private const string MARKER_LAYER_NAME = "RidgeWidthPoints";
        private const string PROBE_LAYER_NAME = "RidgeWidthProbe";  // ✅ YENİ: Probe mesh için layer
        
        // ✅ YENİ: Her grup için ayrı List'ler (Point Probing pattern'i)
        private Dictionary<int, List<Point3D>> groupPoints = new Dictionary<int, List<Point3D>>();
        private Dictionary<int, List<Vector3D>> groupNormals = new Dictionary<int, List<Vector3D>>();
        
        // TreeView için
        private TreeNode currentGroupNode = null;     // Aktif Ridge Width grubu
        private int currentGroupNumber = -1;          // ✅ YENİ: Aktif grup numarası (-1 = grup yok)
        
        // ✅ EKSEN BAZLI SAYAÇLAR (Her eksen kendi basamağını tutar)
        private int xAxisCounter = 0;  // X eksenindeki ölçüm sayısı
        private int yAxisCounter = 0;  // Y eksenindeki ölçüm sayısı
        private int zAxisCounter = 0;  // Z eksenindeki ölçüm sayısı
        
        // İlk seçilen yüzey için
        private Vector3D firstSelectedNormal = null;  // İlk seçilen yüzeyin normal vektörü
        private int? firstSelectedEntityIndex = null;  // İlk seçilen yüzeyin EntityIndex'i
        private Vector3D secondSelectedNormal = null;  // ✅ İkinci seçilen yüzeyin normal vektörü
        private Dictionary<int, Color> originalColors = new Dictionary<int, Color>(); // Yüzeylerin orijinal renkleri

        public RidgeWidthHandler(TreeViewManager tvm, Design designControl, DataManager dataMgr, ImportToMeshForCollision meshConv, InstructionPanel instPanel, SelectionManager selMgr)
        {
            treeViewManager = tvm;
            design = designControl;
            dataManager = dataMgr;
            meshConverter = meshConv;
            instructionPanel = instPanel;  // ✅ YENİ: InstructionPanel'i kaydet
            selectionManager = selMgr;  // ✅ YENİ: SelectionManager'ı kaydet
            
            // ✅ YENİ: CollisionDetector oluştur
            collisionDetector = new CollisionDetector(design, meshConverter);
            
            // ✅ YENİ: KeyDown event'i - ESC tuşu için
            design.KeyDown += Design_KeyDown;
            
            // ✅ YENİ: Probe layer'ını oluştur
            InitializeProbeLayer();
            
            // ✅ YENİ: Measurement layer'ını oluştur
            EnsureMeasurementLayerExists();
            
            System.Diagnostics.Debug.WriteLine("✅ RidgeWidthHandler oluşturuldu (CollisionDetector hazır)");
        }
    }
}
