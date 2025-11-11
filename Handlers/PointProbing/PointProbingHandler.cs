using _014.Managers.Selection;
using _014.Utilities.Collision;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace _014
{
    /// <summary>
    /// ✅ POINT PROBING HANDLER
    /// Direction Probe (NurbsNormalHandler) pattern'i ile
    /// Kullanıcı yüzeylere tıklayarak probe noktaları ekler
    /// </summary>
    public partial class PointProbingHandler
    {
        private Design design;
        private bool isEnabled = false;
        private TreeViewManager treeViewManager;  // ✅ Probe için
        private SelectionManager selectionManager;  // ✅ YENİ: ESC ile çıkış için
        private CollisionDetector collisionDetector;  // ✅ Çarpışma kontrolü için
        private ImportToMeshForCollision meshConverter;  // ✅ ADIM 1: Cache erişimi için
        private int groupId = -1;  // ✅ YENİ: Grup ID (-1 = grup yok)
        
        // Seçilen noktalar
        private List<Point3D> selectedPoints = new List<Point3D>();
        private List<Vector3D> pointNormals = new List<Vector3D>();  // ✅ YENİ: Her noktanın normal vektörü
        private List<Entity> pointMarkers = new List<Entity>();
        private List<Entity> normalLines = new List<Entity>();  // ✅ YENİ: Normal çizgileri
        private List<Entity> zLines = new List<Entity>();  // ✅ YENİ: Z+ dikey çizgileri
        
        // ✅ Highlight için
        private Entity highlightedMarker = null;  // Şu anda highlight edilen marker
        
        // TreeView update callback
        public Action<Point3D> OnPointAdded;
        
        // Layer
        private const string MARKER_LAYER_NAME = "ProbePoints";

        public PointProbingHandler(Design designControl, ImportToMeshForCollision meshConv, TreeViewManager treeViewMgr = null, SelectionManager selMgr = null, int groupIdentifier = -1)
        {
            design = designControl;
            this.meshConverter = meshConv;  // ✅ ADIM 1: Referansı sakla
            treeViewManager = treeViewMgr;
            selectionManager = selMgr;  // ✅ YENİ: SelectionManager'ı kaydet
            this.groupId = groupIdentifier;  // ✅ YENİ: Grup ID'yi kaydet
            
            // ✅ ADIM 1: CollisionDetector oluştur (meshConverter ile)
            collisionDetector = new CollisionDetector(design, meshConverter);
            
            // Mouse click event'ini bağla
            design.MouseClick += Design_MouseClick;
            
            // ✅ YENİ: KeyDown event'i - DELETE tuşu için
            design.KeyDown += Design_KeyDown;
            
            // Layer'ı oluştur
            InitializeLayer();
        }
    }
}
