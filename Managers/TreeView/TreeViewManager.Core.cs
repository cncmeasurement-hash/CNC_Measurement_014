using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _014.Managers.Selection;
using _014.Managers.Toolpath;
using _014.Managers.Data;
using devDept.Eyeshot.Control;
using devDept.Geometry;  // ✅ YENİ: Point3D için gerekli

namespace _014
{
    /// <summary>
    /// TreeView CNC Configuration Manager
    /// Mix yöntemi: ComboBox dropdown + Inline editing
    /// PARTIAL CLASS 1/6: Core - Fields, Constructor, Properties, Events
    /// </summary>
    public partial class TreeViewManager
    {
        private TreeView treeView;
        private Design design;
        private Form ownerForm;
        private SelectionManager selectionManager;  // ✅ YENİ: SelectionManager referansı

        // Ana node'lar
        private TreeNode rootNode;
        // private TreeNode machineNode; // ✅ KALDIRILDI - Machine Name artık gösterilmiyor
        // private TreeNode probeNode; // ✅ KALDIRILDI - Probe Name artık gösterilmiyor
        // private TreeNode zSafetyNode; // ✅ KALDIRILDI - Z Safety artık gösterilmiyor
        // private TreeNode retractNode; // ✅ KALDIRILDI - Retract artık gösterilmiyor
        private TreeNode methodNode;
        private TreeNode toolpathNode;  // YENİ: Toolpath node
        private TreeNode generateNode;
        // YENİ: ToolpathManager
        private ToolpathManager toolpathManager;
        // YENİ: MeasurementDataManager
        private MeasurementDataManager _dataManager;
        // ✅ GRUP SİSTEMİ: Probe grupları
        private List<TreeNode> probePointsGroups = new List<TreeNode>();
        private TreeNode activeProbeGroup = null;
        private int probeGroupCounter = 0;
        
        // ✅ YENİ: Static ID counter'lar (Duplicate ID hatasını önlemek için)
        private static int angleMeasurementIdCounter = 1000;  // Angle grupları için 1001'den başlar
        private static int ridgeWidthIdCounter = 2000;        // Ridge Width grupları için 2001'den başlar

        // ComboBox ve TextBox kontrolleri
        private ComboBox machineComboBox;
        private ComboBox probeComboBox;
        // zSafetyTextBox KALDIRILDI - Z Safety artık Clearance Plane'den otomatik alınıyor
        private TextBox retractTextBox;  // YENİ: Retract textbox

        // Context Menu'ler
        private ContextMenuStrip machineContextMenu;
        private ContextMenuStrip probeContextMenu;
        private ContextMenuStrip zSafetyContextMenu;  // ✅ YENİ: Z Safety context menu
        private ContextMenuStrip probingContextMenu;  // ✅ YENİ: Probing context menu
        
        // ✅ YENİ: Checkbox kontrolü için boş ImageList
        private ImageList stateImageList;

        // Veriler - Artık JSON'dan yüklenecek
        private List<string> machines = new List<string>();
        private List<string> probes = new List<string> { "Renishaw TP20", "Blum TC50", "Heidenhain TS" };

        // Seçili değerler
        public string SelectedMachine { get; private set; } = "Hermle C30";
        public string SelectedProbe { get; private set; } = "Renishaw TP20";
        public double ZSafetyDistance { get; private set; } = 50.0;
        public int RetractDistance { get; private set; } = 3;  // YENİ: Retract değeri (default 3mm)
        public double SimulationSpeed { get; private set; } = 1.0;  // 🆕 YENİ: Simülasyon hızı (default 1.0x)

        // Event'ler
        public event EventHandler OnGenerateGCodeClicked;
        public event EventHandler<double> OnZSafetyChanged;  // ✅ YENİ: Z Safety değiştiğinde
        public event EventHandler OnProbeChanged;  // ✅ YENİ: Probe değiştiğinde marker'ları güncelle
        public event EventHandler OnRetractChanged;  // ✅ YENİ: Retract değiştiğinde marker'ları güncelle
        public event EventHandler OnSimulateToolpathClicked;  // ✅ YENİ: Simülasyon başlat
        public event EventHandler OnStopSimulationClicked;  // ✅ YENİ: Simülasyon durdur
        public event EventHandler<double> OnSimulationSpeedChanged;  // 🆕 YENİ: Simülasyon hızı değişti

        public TreeViewManager(TreeView treeView, Design design, Form ownerForm, SelectionManager selectionManager = null)
        {
            this.treeView = treeView;
            this.design = design;
            this.ownerForm = ownerForm;
            this.selectionManager = selectionManager;

            // ✅ TreeView genişliğini artır (Z değerinin görünmesi için)
            treeView.Width = 400;

            LoadMachinesFromJson(); // JSON'dan makineleri yükle
            LoadProbesFromJson();   // JSON'dan probe'ları yükle
            InitializeContextMenus();  // ✅ ÖNCE: Context menu'leri oluştur
            InitializeTreeView();       // SONRA: TreeView'i oluştur ve context menu'leri assign et
            
            // YENİ: ToolpathManager oluştur
            toolpathManager = new ToolpathManager(toolpathNode, selectionManager, this);
            
            // YENİ: MeasurementDataManager singleton instance'ı al
            _dataManager = MeasurementDataManager.Instance;
            System.Diagnostics.Debug.WriteLine("✅ TreeViewManager: DataManager bağlandı");
            
            InitializeEvents();
        }
    }
}
