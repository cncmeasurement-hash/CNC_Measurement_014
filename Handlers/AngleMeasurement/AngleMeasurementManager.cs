using _014.Managers.Data;
using _014.Probe.Core;  // ✅ YENİ: ProbeBuilder için
using _014.Utilities.Collision;  // ✅ YENİ: ImportToMeshForCollision için
using _014.Utilities.UI;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace _014.Handlers.AngleMeasurement
{
    /// <summary>
    /// ANGLE MEASUREMENT MANAGER
    /// Düzlemsel yüzey üzerinde 2 nokta seçerek açı ölçümü yapar
    /// 2 nokta arası çizginin X/Y ekseni ile yaptığı açıyı hesaplar
    /// </summary>
    public partial class AngleMeasurementManager
    {
        // ═══════════════════════════════════════════════════════════
        // FIELDS
        // ═══════════════════════════════════════════════════════════

        private readonly Design design;
        private readonly Form parentForm;
        private readonly DataManager? dataManager;
        private readonly TreeViewManager? treeViewManager;  // ✅ YENİ: TreeViewManager referansı
        private readonly ImportToMeshForCollision? meshConverter;  // ✅ YENİ: Çarpışma kontrolü için

        private const string MEASUREMENT_LAYER_NAME = "MeasurementLines";
        private const string PROBE_LAYER_NAME = "AngleMeasurementProbe";  // ✅ YENİ: Probe mesh'leri için layer

        // Mod durumu
        private bool isActive = false;
        private int selectionStep = 0; // 0=başla, 1=yüzey seç, 2=1.nokta seç, 3=2.nokta seç

        // ✅ YENİ: TreeView grup yönetimi
        private TreeNode? currentGroupNode = null;  // TreeView işlemleri için (AddPoint, AddResult)
        private int _currentGroupId = -1;  // RemoveGroup için grup ID'si

        // Seçilen yüzey
        private Entity? selectedFace = null;
        private Color originalFaceColor;
        private Plane? facePlane = null; // Yüzeyin düzlemi

        // Seçilen noktalar
        private Point3D? point1 = null;
        private Point3D? point2 = null;

        // Açı hesaplama
        private double calculatedAngle = 0;
        private string referenceAxis = "X"; // Varsayılan X ekseni
        private Vector3D lineVector;        // 2 nokta arası vektör

        // Görselleştirme
        private Mesh? marker1 = null;      // 1. nokta marker (MESH - KÜRE)
        private Mesh? marker2 = null;      // 2. nokta marker (MESH - KÜRE)
        private Mesh? probe1 = null;      // 1. nokta probe mesh
        private Mesh? probe2 = null;      // 2. nokta probe mesh
        private Line? measurementLine = null; // 2 nokta arası çizgi
        private Arc? angleArc = null;       // Açı gösterimi (yay)

        // UI
        private InstructionPanel? instructionPanel;
        // InfoPanel - sonraki adımda eklenecek

        // Orijinal renkler (restore için)
        private Dictionary<int, Color> originalColors = new Dictionary<int, Color>();

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        public AngleMeasurementManager(Design designControl, Form parentForm, DataManager? dataManager = null, TreeViewManager? treeViewManager = null, ImportToMeshForCollision? meshConverter = null)
        {
            design = designControl ?? throw new ArgumentNullException(nameof(designControl));
            this.parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
            this.dataManager = dataManager;
            this.treeViewManager = treeViewManager;
            this.meshConverter = meshConverter;

            Debug.WriteLine("✅ AngleMeasurementManager oluşturuldu");
        }

        // ═══════════════════════════════════════════════════════════
        // PUBLIC PROPERTIES
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Angle Measurement modu aktif mi?
        /// </summary>
        public bool IsActive => isActive;

        // ═══════════════════════════════════════════════════════════
        // PUBLIC METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Angle Measurement modunu aktif et
        /// </summary>
        public void Enable(InstructionPanel? instructionPanel)
        {
            if (isActive) return;

            isActive = true;
            this.instructionPanel = instructionPanel;
            selectionStep = 1;  // Yüzey seçimi

            // Seçimi sıfırla
            ResetSelection();

            // Mouse event'leri bağla
            design.SelectionChanged += design_SelectionChanged;
            design.MouseClick += design_MouseClick;
            design.KeyDown += Design_KeyDown;

            // Cursor değiştir
            design.Cursor = Cursors.Hand;

            // Design control'e focus ver (ESC tuşu hemen çalışsın)
            design.Focus();

            // InstructionPanel güncelle
            if (instructionPanel != null && !instructionPanel.IsDisposed)
            {
                instructionPanel.UpdatePanel(
                    InstructionTexts.TITLE_ANGLE_MEASUREMENT,
                    InstructionTexts.ANGLE_MEASUREMENT
                );
                Debug.WriteLine("📋 InstructionPanel güncellendi: Angle Measurement modu");
            }

            // ✅ YENİ: Layer'ları oluştur (RidgeWidth'den öğrenildi)
            InitializeLayers();

            // ✅ YENİ: TreeView'de yeni Angle Measurement grubu oluştur
            if (treeViewManager != null)
            {
                TreeNode groupNode = treeViewManager.CreateNewAngleMeasurementGroup();
                
                // Grup ID'sini TreeNode Tag'inden al
                if (groupNode != null)
                {
                    // TreeView işlemleri için
                    currentGroupNode = groupNode;
                    
                    // RemoveGroup için grup ID'sini parse et
                    string tag = groupNode.Tag?.ToString() ?? "";
                    if (tag.StartsWith("ANGLE_"))
                    {
                        _currentGroupId = int.Parse(tag.Replace("ANGLE_", ""));
                        Debug.WriteLine($"✅ Angle grup oluşturuldu: ID={_currentGroupId}");
                    }
                }
                else
                {
                    Debug.WriteLine("❌ TreeView'de Angle Measurement grubu oluşturulamadı");
                }
            }

            // Planar yüzeyleri sarıya çevir
            HighlightPlanarSurfaces();

            Debug.WriteLine("✅ Angle Measurement AKTIF - Düzlemsel yüzey seçimi bekleniyor");
        }

        /// <summary>
        /// Angle Measurement modunu pasif et
        /// </summary>
        public void Disable()
        {
            if (!isActive) return;

            isActive = false;

            // Mouse event'leri kopar
            design.SelectionChanged -= design_SelectionChanged;
            design.MouseClick -= design_MouseClick;
            design.KeyDown -= Design_KeyDown;

            // Yüzeyleri restore et
            RestoreAllSurfaces();

            // InstructionPanel güncelle
            if (instructionPanel != null && !instructionPanel.IsDisposed)
            {
                instructionPanel.UpdatePanel(
                    InstructionTexts.TITLE_MAIN_MENU,
                    InstructionTexts.WELCOME
                );
                Debug.WriteLine("📋 InstructionPanel Main Menu'ye döndürüldü");
            }

            // Cursor normale döndür
            design.Cursor = Cursors.Default;

            Debug.WriteLine("❌ Angle Measurement PASİF");
        }

        /// <summary>
        /// Seçimleri sıfırla ve temizle
        /// </summary>
        public void ResetSelection()
        {
            // TODO: Implement
            // - Yüzey rengini eski haline getir
            // - Marker'ları sil
            // - Çizgileri sil
            // - Arc'ı sil
            // - Değişkenleri sıfırla

            selectionStep = 0;
            selectedFace = null;
            facePlane = null;
            point1 = null;
            point2 = null;
            calculatedAngle = 0;
            
            // ✅ Probe ve marker field'larını sıfırla
            marker1 = null;
            marker2 = null;
            probe1 = null;
            probe2 = null;

            Debug.WriteLine("🔄 Angle Measurement - Seçimler temizlendi");
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE METHODS - EVENT HANDLERS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Yüzey seçimi değiştiğinde
        /// </summary>
        private void design_SelectionChanged(object sender, EventArgs e)
        {
            // TODO: Yüzey seçimi kontrolü
            // selectionStep == 1 ise yüzey seç
        }

        /// <summary>
        /// Mouse tıklaması
        /// </summary>
        private void design_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isActive) return;
            
            try
            {
                Debug.WriteLine("═══════════════════════════════════════");
                Debug.WriteLine("🖱️ ANGLE MEASUREMENT: Mouse tıklandı");
                Debug.WriteLine($"   Adım: {selectionStep} (1=yüzey, 2=1.nokta, 3=2.nokta)");
                
                // ═══════════════════════════════════════════════════════════
                // ADIM 1: Entity Seçimi
                // ═══════════════════════════════════════════════════════════
                int entityIndex = design.GetEntityUnderMouseCursor(e.Location, true);
                
                if (entityIndex == -1)
                {
                    Debug.WriteLine("❌ Hiçbir entity tıklanmadı");
                    Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }
                
                // ═══════════════════════════════════════════════════════════
                // ADIM 2: Entity'yi Al ve IFace Kontrolü
                // ═══════════════════════════════════════════════════════════
                Entity entity = design.Entities[entityIndex];
                Debug.WriteLine($"📦 Entity bulundu: {entity.GetType().Name} (Index: {entityIndex})");
                
                if (!(entity is IFace faceEntity))
                {
                    Debug.WriteLine("⚠️ Entity IFace değil (Marker veya başka bir şey)");
                    Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }
                
                // ═══════════════════════════════════════════════════════════
                // ADIM 3: Tıklanan Noktayı Bul
                // ═══════════════════════════════════════════════════════════
                Point3D clickedPoint;
                int triangleIndex;
                double distance = design.FindClosestTriangle(
                    faceEntity,
                    e.Location,
                    out clickedPoint,
                    out triangleIndex
                );
                
                if (distance == double.MaxValue || triangleIndex == -1)
                {
                    Debug.WriteLine("⚠️ Tıklanan nokta bulunamadı");
                    Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }
                
                Debug.WriteLine($"✅ Nokta bulundu: ({clickedPoint.X:F3}, {clickedPoint.Y:F3}, {clickedPoint.Z:F3})");
                
                // ═══════════════════════════════════════════════════════════
                // ADIM 4: DataManager'dan Yüzey Bilgisi Al
                // ═══════════════════════════════════════════════════════════
                if (dataManager == null)
                {
                    Debug.WriteLine("⚠️ DataManager yok!");
                    Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }
                
                var surfaceData = dataManager.GetSurfaceByEntityIndex(entityIndex);
                
                if (surfaceData == null)
                {
                    Debug.WriteLine("⚠️ Bu yüzey analiz edilmemiş veya bulunamadı");
                    Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }
                
                // ═══════════════════════════════════════════════════════════
                // ADIM 5: PLANAR YÜZEY KONTROLÜ (BOTTOM Z- HARİÇ)
                // ═══════════════════════════════════════════════════════════
                // Sadece düzlemsel yüzeyler kabul edilir
                bool isPlanar = surfaceData.SurfaceType == "RIGHT (X+)" ||
                                surfaceData.SurfaceType == "LEFT (X-)" ||
                                surfaceData.SurfaceType == "FRONT (Y+)" ||
                                surfaceData.SurfaceType == "BACK (Y-)" ||
                                surfaceData.SurfaceType == "TOP (Z+)" ||
                                // surfaceData.SurfaceType == "BOTTOM (Z-)" ||  // ❌ ÇIKARILDI - Z- yüzeyleri seçilemez
                                surfaceData.SurfaceType == "INCLINED";
                
                if (!isPlanar)
                {
                    Debug.WriteLine("⛔ SADECE DÜZLEMSEL (PLANAR) YÜZEYLER SEÇİLEBİLİR!");
                    Debug.WriteLine($"   Bu yüzey: {surfaceData.SurfaceType}");
                    Debug.WriteLine("   Lütfen sarı renkli yüzeylerden birini seçin!");
                    Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }
                
                Debug.WriteLine($"✅ PLANAR YÜZEY SEÇİLDİ: {surfaceData.SurfaceType}");
                Debug.WriteLine($"   Normal: ({surfaceData.Normal.X:F3}, {surfaceData.Normal.Y:F3}, {surfaceData.Normal.Z:F3})");
                
                // ═══════════════════════════════════════════════════════════
                // Probe Diameter Al (Marker kaydırma için)
                // ═══════════════════════════════════════════════════════════
                double probeDiameter = 6.0;  // Default
                if (treeViewManager != null)
                {
                    probeDiameter = treeViewManager.GetSelectedProbeDiameter();
                    Debug.WriteLine($"   Probe Diameter: {probeDiameter:F3}mm (Marker offset: {probeDiameter / 2.0:F3}mm)");
                }
                else
                {
                    Debug.WriteLine("   ⚠️ TreeViewManager null, default diameter kullanılıyor: 6.0mm");
                }
                
                // ═══════════════════════════════════════════════════════════
                // ADIM 6: BİRİNCİ NOKTA SEÇİMİ
                // ═══════════════════════════════════════════════════════════
                if (point1 == null)
                {
                    // İlk nokta seçiliyor
                    point1 = clickedPoint;
                    selectedFace = entity;
                    originalFaceColor = entity.Color;
                    
                    // ✅ YENİ: Yüzey düzlemini oluştur (açı hesaplama için gerekli)
                    facePlane = new Plane(point1, surfaceData.Normal);
                    
                    Debug.WriteLine($"✅ BİRİNCİ NOKTA SEÇİLDİ: ({point1.X:F3}, {point1.Y:F3}, {point1.Z:F3})");
                    Debug.WriteLine($"   Entity Index: {entityIndex}");
                    Debug.WriteLine($"   Yüzey Düzlemi oluşturuldu: Normal = ({surfaceData.Normal.X:F3}, {surfaceData.Normal.Y:F3}, {surfaceData.Normal.Z:F3})");
                    
                    // ADIM 7: Marker Ekle (Kaydırılmış pozisyonda - SADECE GÖRSEL)
                    // Unique isim ver: currentGroupNode.Text'ten grup numarasını al
                    string groupNumber = currentGroupNode?.Text?.Replace("Angle ", "") ?? "0";
                    string markerName = $"AngleMarker_{groupNumber}_Point1";
                    marker1 = AddMarker(point1, Color.Red, markerName, surfaceData.Normal, probeDiameter);
                    
                    // ═══════════════════════════════════════════════════════════
                    // ✅ YENİ: PROBE MESH OLUŞTUR VE YERLEŞTİR
                    // ═══════════════════════════════════════════════════════════
                    if (treeViewManager != null)
                    {
                        try
                        {
                            Debug.WriteLine("═══════════════════════════════════════");
                            Debug.WriteLine("🔧 PROBE MESH YERLEŞTİRME BAŞLIYOR...");
                            
                            // 1. Seçili probe'u al
                            var selectedProbe = treeViewManager.GetSelectedProbeData();
                            if (selectedProbe == null)
                            {
                                Debug.WriteLine("❌ Seçili probe bulunamadı!");
                            }
                            else
                            {
                                Debug.WriteLine($"   Seçili Probe: D={selectedProbe.D}mm");
                                
                                // 2. Probe mesh'ini oluştur
                                Mesh probeMesh = _014.Probe.Core.ProbeBuilder.CreateProbeMesh(selectedProbe);
                                if (probeMesh == null)
                                {
                                    Debug.WriteLine("❌ ProbeBuilder.CreateProbeMesh() null döndü!");
                                }
                                else
                                {
                                    Debug.WriteLine($"✅ Probe mesh oluşturuldu (Vertex: {probeMesh.Vertices.Length})");
                                    
                                    // 3. Mesh'in klonunu al
                                    Mesh displayProbe = (Mesh)probeMesh.Clone();
                                    Debug.WriteLine("✅ Probe mesh klonlandı");
                                    
                                    // 4. ADIM 1: X0Y0Z0'da başla (zaten origin'de)
                                    Debug.WriteLine("   Başlangıç: X=0, Y=0, Z=0");
                                    
                                    // 5. ADIM 2: Kullanıcının seçtiği koordinatlara kaydır
                                    displayProbe.Translate(point1.X, point1.Y, point1.Z);
                                    Debug.WriteLine($"   Kullanıcı noktasına kaydırıldı: ({point1.X:F3}, {point1.Y:F3}, {point1.Z:F3})");
                                    
                                    // 6. ADIM 3: Z- yönünde D/2 kaydır
                                    double probeRadius = (double)selectedProbe.D / 2.0;
                                    displayProbe.Translate(0, 0, -probeRadius);
                                    Debug.WriteLine($"   Z- yönünde kaydırıldı: -D/2 = {-probeRadius:F3}mm");
                                    
                                    // 7. ADIM 4: Normal yönünde D*0.6mm kaydır
                                    double offset = (double)selectedProbe.D * 0.6;
                                    displayProbe.Translate(
                                        surfaceData.Normal.X * offset,
                                        surfaceData.Normal.Y * offset,
                                        surfaceData.Normal.Z * offset
                                    );
                                    Debug.WriteLine($"   Normal yönünde kaydırıldı: D*0.6 = {offset:F3}mm");
                                    Debug.WriteLine($"   Normal: ({surfaceData.Normal.X:F3}, {surfaceData.Normal.Y:F3}, {surfaceData.Normal.Z:F3})");
                                    
                                    // 8. Probe özelliklerini ayarla (SADECE ÇARPIŞMA İÇİN)
                                    displayProbe.Visible = false;  // ✅ PROBE GÖRÜNMEZ!
                                    displayProbe.Color = Color.Blue;  // ✅ MAVİ PROBE
                                    displayProbe.ColorMethod = colorMethodType.byEntity;
                                    displayProbe.LayerName = PROBE_LAYER_NAME;
                                    Debug.WriteLine($"   Probe renk: BEYAZ, Layer: {PROBE_LAYER_NAME}");
                                    
                                    // 9. Probe'u field'a kaydet (çarpışma kontrolü için)
                                    probe1 = displayProbe;
                                    
                                    Debug.WriteLine("✅ PROBE MESH OLUŞTURULDU (EKRANDA GÖRÜNMİYOR - SADECE ÇARPIŞMA KONTROLÜ)!");
                                    Debug.WriteLine($"   Final Pozisyon: Kullanıcı noktası + Z-({probeRadius:F3}mm) + Normal*{offset:F3}mm");
                                    
                                    // ═══════════════════════════════════════════════════════════
                                    // ✅ PROBE'U GEÇİCİ OLARAK EKLE (Eyeshot CollisionDetection için gerekli)
                                    // ═══════════════════════════════════════════════════════════
                                    design.Entities.Add(displayProbe);
                                    
                                    design.Invalidate();
                                    
                                    // ═══════════════════════════════════════════════════════════
                                    // ✅ YENİ: ÇARPIŞMA KONTROLÜ (BİRİNCİ NOKTA)
                                    // ═══════════════════════════════════════════════════════════
                                    if (meshConverter != null)
                                    {
                                        Debug.WriteLine("═══════════════════════════════════════");
                                        Debug.WriteLine("🔍 ÇARPIŞMA KONTROLÜ BAŞLADI (BİRİNCİ NOKTA)...");
                                        
                                        List<Mesh> partMeshes = meshConverter.GetMeshesForCollision();
                                        Debug.WriteLine($"📦 Kontrol edilecek mesh sayısı: {partMeshes.Count}");
                                        
                                        bool hasCollision = false;
                                        foreach (Mesh partMesh in partMeshes)
                                        {
                                            // Mesh validasyonu
                                            if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                                continue;
                                            
                                            try
                                            {
                                                // Eyeshot CollisionDetection
                                                CollisionDetection cd = new CollisionDetection(
                                                    new Entity[] { displayProbe },  // Yerleştirilmiş probe
                                                    new Entity[] { partMesh },      // Parça mesh
                                                    null
                                                );
                                                
                                                cd.CheckMethod = collisionCheckType.SubdivisionTree;
                                                cd.DoWork();
                                                
                                                if (cd.Result != null && cd.Result.Length > 0)
                                                {
                                                    hasCollision = true;
                                                    Debug.WriteLine("💥 ÇARPIŞMA TESPİT EDİLDİ!");
                                                    break;
                                                }
                                            }
                                            catch (Exception collisionEx)
                                            {
                                                Debug.WriteLine($"❌ Çarpışma kontrolü hatası: {collisionEx.Message}");
                                            }
                                        }
                                        
                                        if (hasCollision)
                                        {
                                            Debug.WriteLine("═══════════════════════════════════════");
                                            Debug.WriteLine("⛔ ÇARPIŞMA VAR (BİRİNCİ NOKTA - İLK KONUM)");
                                            Debug.WriteLine("═══════════════════════════════════════");
                                            
                                            // ✅ 1. PROBE'U GÖRÜNÜR YAP (Çarpışma yerini göster!)
                                            if (probe1 != null)
                                            {
                                                probe1.Visible = true;
                                                Debug.WriteLine("👁️ Probe1 görünür yapıldı (Çarpışma gösterimi)");
                                            }
                                            
                                            design.Invalidate();
                                            Application.DoEvents();  // UI thread'i güncelle
                                            
                                            // ✅ 2. MessageBox göster
                                            MessageBox.Show(
                                                "⚠️ ÇARPIŞMA TESPİT EDİLDİ!\n\n" +
                                                "Probe parça ile çarpışıyor (Birinci Nokta - İlk Konum).",
                                                "Angle Measurement - Çarpışma Uyarısı",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Warning
                                            );
                                            
                                            // ✅ 3. Kullanıcı OK'e tıkladı - Probe ve marker'ı sil
                                            if (probe1 != null)
                                            {
                                                design.Entities.Remove(probe1);
                                                probe1 = null;
                                            }
                                            if (marker1 != null)
                                            {
                                                design.Entities.Remove(marker1);
                                                marker1 = null;
                                            }
                                            Debug.WriteLine("🗑️ Probe1 ve Marker1 silindi (Birinci Nokta - Çarpışma)");
                                            
                                            // ✅ 4. TreeView + DataManager'dan grubu sil
                                            if (_currentGroupId != -1 && treeViewManager != null)
                                            {
                                                treeViewManager.RemoveGroup(_currentGroupId);
                                                Debug.WriteLine($"🗑️ TreeView + DataManager'dan grup silindi: ID={_currentGroupId}");
                                                _currentGroupId = -1;
                                            }
                                            currentGroupNode = null;
                                            
                                            // ✅ 5. Ekranı güncelle
                                            design.Entities.Regen();
                                            design.Invalidate();
                                            
                                            // ✅ 6. Moddan çık
                                            Disable();
                                            Debug.WriteLine("⛔ Angle Measurement modu kapatıldı (Birinci Nokta - Çarpışma)");
                                            
                                            Debug.WriteLine("═══════════════════════════════════════");
                                            return;  // İşlemi iptal et
                                        }
                                        else
                                        {
                                            Debug.WriteLine("✅ ÇARPIŞMA YOK (İLK KONUM - D*0.6)");
                                        }
                                        
                                        // ═══════════════════════════════════════════════════════════
                                        // ✅ RETRACT DÖNGÜSÜ (1MM ADIMLARLA ÇARPIŞMA KONTROLÜ)
                                        // ═══════════════════════════════════════════════════════════
                                        if (treeViewManager != null)
                                        {
                                            double retractDistance = treeViewManager.RetractDistance;
                                            int stepCount = (int)retractDistance;
                                            
                                            Debug.WriteLine("═══════════════════════════════════════");
                                            Debug.WriteLine($"🔁 RETRACT DÖNGÜSÜ BAŞLIYOR: {stepCount} adım (1mm → {stepCount}mm)");
                                            Debug.WriteLine("═══════════════════════════════════════");
                                            
                                            bool hasCollisionInLoop = false;
                                            int collisionStep = 0;
                                            
                                            for (int i = 0; i < stepCount; i++)
                                            {
                                                // Her adımda 1mm kaydır (normal yönünde)
                                                displayProbe.Translate(
                                                    surfaceData.Normal.X * 1.0,
                                                    surfaceData.Normal.Y * 1.0,
                                                    surfaceData.Normal.Z * 1.0
                                                );
                                                design.Invalidate();
                                                
                                                Debug.WriteLine($"   🔍 Adım {i + 1}/{stepCount}: +{i + 1}mm konumda kontrol (Toplam: D*0.6 + {i + 1}mm)");
                                                
                                                // Çarpışma kontrolü
                                                foreach (Mesh partMesh in partMeshes)
                                                {
                                                    // Mesh validasyonu
                                                    if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                                        continue;
                                                    
                                                    try
                                                    {
                                                        // Eyeshot CollisionDetection
                                                        CollisionDetection cd = new CollisionDetection(
                                                            new Entity[] { displayProbe },  // Kaydırılmış probe
                                                            new Entity[] { partMesh },      // Parça mesh
                                                            null
                                                        );
                                                        
                                                        cd.CheckMethod = collisionCheckType.SubdivisionTree;
                                                        cd.DoWork();
                                                        
                                                        if (cd.Result != null && cd.Result.Length > 0)
                                                        {
                                                            hasCollisionInLoop = true;
                                                            collisionStep = i + 1;
                                                            Debug.WriteLine($"   💥 ÇARPIŞMA TESPİT EDİLDİ! (+{i + 1}mm konumda)");
                                                            break;
                                                        }
                                                    }
                                                    catch (Exception collisionEx)
                                                    {
                                                        Debug.WriteLine($"   ❌ Döngü çarpışma kontrolü hatası (Adım {i + 1}): {collisionEx.Message}");
                                                    }
                                                }
                                                
                                                // Çarpışma varsa döngüden çık
                                                if (hasCollisionInLoop)
                                                    break;
                                            }
                                            
                                            // ═══════════════════════════════════════════════════════════
                                            // DÖNGÜ SONRASI KONTROL
                                            // ═══════════════════════════════════════════════════════════
                                            if (hasCollisionInLoop)
                                            {
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                Debug.WriteLine($"⛔ ÇARPIŞMA VAR (BİRİNCİ NOKTA - +{collisionStep}MM KONUM)");
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                
                                                // ✅ 1. Önce marker ve probe'u sil
                                                if (probe1 != null)
                                                {
                                                    design.Entities.Remove(probe1);
                                                    probe1 = null;
                                                }
                                                if (marker1 != null)
                                                {
                                                    design.Entities.Remove(marker1);
                                                    marker1 = null;
                                                }
                                                Debug.WriteLine($"🗑️ Probe1 ve Marker1 silindi (Birinci Nokta - +{collisionStep}mm Çarpışma)");
                                                
                                                // ✅ 2. TreeView + DataManager'dan grubu sil
                                                if (_currentGroupId != -1 && treeViewManager != null)
                                                {
                                                    treeViewManager.RemoveGroup(_currentGroupId);
                                                    Debug.WriteLine($"🗑️ TreeView + DataManager'dan grup silindi: ID={_currentGroupId}");
                                                    _currentGroupId = -1;
                                                }
                                                currentGroupNode = null;
                                                
                                                // ✅ 3. Ekranı ANINDA güncelle ve bekle
                                                design.Entities.Regen();  // Entity listesini yeniden oluştur
                                                design.Invalidate();
                                                Application.DoEvents();  // UI thread'i güncelle
                                                System.Threading.Thread.Sleep(50);  // 50ms bekle - Ekran güncellemesi için
                                                
                                                // ✅ 4. SONRA MessageBox göster
                                                MessageBox.Show(
                                                    $"⚠️ ÇARPIŞMA TESPİT EDİLDİ!\n\n" +
                                                    $"Probe normal yönünde +{collisionStep}mm konumda parça ile çarpışıyor (Birinci Nokta).",
                                                    "Angle Measurement - Çarpışma Uyarısı",
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Warning
                                                );
                                                
                                                // ✅ 5. Moddan çık
                                                Disable();
                                                Debug.WriteLine($"⛔ Angle Measurement modu kapatıldı (Birinci Nokta - +{collisionStep}mm Çarpışma)");
                                                
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                return;  // İşlemi iptal et
                                            }
                                            else
                                            {
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                Debug.WriteLine($"✅ TÜM RETRACT KONTROLÜ TAMAM - ÇARPIŞMA YOK ({stepCount} adım)");
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                
                                                // ═══════════════════════════════════════════════════════════
                                                // ✅ Z+ YÖNÜNDEKİ ÇARPIŞMA KONTROLÜ (50-CLEARANCE PLANE)
                                                // ═══════════════════════════════════════════════════════════
                                                
                                                // Clearance Plane değerini Form1'den al
                                                double clearancePlaneValue = 350.0; // Varsayılan
                                                if (parentForm is CNC_Measurement form1)
                                                {
                                                    if (double.TryParse(form1.txt_form1_Clerance.Text, out double parsedValue))
                                                    {
                                                        clearancePlaneValue = parsedValue;
                                                    }
                                                }
                                                int zStepMax = (int)clearancePlaneValue;
                                                
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                Debug.WriteLine($"🔍 Z+ YÖNÜNDE DÖNGÜ BAŞLIYOR (50mm → {zStepMax}mm adımlarla)...");
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                
                                                bool hasCollisionZPlus = false;
                                                int collisionZStep = 0;
                                                
                                                for (int zStep = 50; zStep <= zStepMax; zStep += 50)
                                                {
                                                    // 50mm Z+ yönünde hareket
                                                    displayProbe.Translate(0, 0, 50.0);
                                                    design.Invalidate();
                                                    
                                                    Debug.WriteLine($"   🔍 Z+ Adım: {zStep}mm yukarı çıkıldı (Toplam Z+{zStep}mm)");
                                                    
                                                    // Çarpışma kontrolü
                                                    foreach (Mesh partMesh in partMeshes)
                                                    {
                                                        // Mesh validasyonu
                                                        if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                                            continue;
                                                        
                                                        try
                                                        {
                                                            // Eyeshot CollisionDetection
                                                            CollisionDetection cdZPlus = new CollisionDetection(
                                                                new Entity[] { displayProbe },  // Z+ konumundaki probe
                                                                new Entity[] { partMesh },      // Parça mesh
                                                                null
                                                            );
                                                            
                                                            cdZPlus.CheckMethod = collisionCheckType.SubdivisionTree;
                                                            cdZPlus.DoWork();
                                                            
                                                            if (cdZPlus.Result != null && cdZPlus.Result.Length > 0)
                                                            {
                                                                hasCollisionZPlus = true;
                                                                collisionZStep = zStep;
                                                                Debug.WriteLine($"   💥 ÇARPIŞMA TESPİT EDİLDİ! (Z+{zStep}mm konumda)");
                                                                break;
                                                            }
                                                        }
                                                        catch (Exception collisionExZ)
                                                        {
                                                            Debug.WriteLine($"   ❌ Z+ çarpışma kontrolü hatası (Z+{zStep}mm): {collisionExZ.Message}");
                                                        }
                                                    }
                                                    
                                                    // Çarpışma varsa döngüden çık
                                                    if (hasCollisionZPlus)
                                                        break;
                                                }
                                                
                                                // ═══════════════════════════════════════════════════════════
                                                // Z+ DÖNGÜ SONRASI KONTROL
                                                // ═══════════════════════════════════════════════════════════
                                                if (hasCollisionZPlus)
                                                {
                                                    Debug.WriteLine("═══════════════════════════════════════");
                                                    Debug.WriteLine($"⛔ ÇARPIŞMA VAR (BİRİNCİ NOKTA - Z+{collisionZStep}MM KONUM)");
                                                    Debug.WriteLine("═══════════════════════════════════════");
                                                    
                                                    // ✅ 1. Önce marker ve probe'u sil
                                                    if (probe1 != null)
                                                    {
                                                        design.Entities.Remove(probe1);
                                                        probe1 = null;
                                                    }
                                                    if (marker1 != null)
                                                    {
                                                        design.Entities.Remove(marker1);
                                                        marker1 = null;
                                                    }
                                                    Debug.WriteLine($"🗑️ Probe1 ve Marker1 silindi (Birinci Nokta - Z+{collisionZStep}mm Çarpışma)");
                                                    
                                                    // ✅ 2. TreeView + DataManager'dan grubu sil
                                                    if (_currentGroupId != -1 && treeViewManager != null)
                                                    {
                                                        treeViewManager.RemoveGroup(_currentGroupId);
                                                        Debug.WriteLine($"🗑️ TreeView + DataManager'dan grup silindi: ID={_currentGroupId}");
                                                        _currentGroupId = -1;
                                                    }
                                                    
                                                    // ✅ 3. Ekranı ANINDA güncelle ve bekle
                                                    design.Entities.Regen();  // Entity listesini yeniden oluştur
                                                    design.Invalidate();
                                                    Application.DoEvents();  // UI thread'i güncelle
                                                    System.Threading.Thread.Sleep(50);  // 50ms bekle - Ekran güncellemesi için
                                                    
                                                    // ✅ 4. SONRA MessageBox göster
                                                    MessageBox.Show(
                                                        $"⚠️ ÇARPIŞMA TESPİT EDİLDİ!\n\n" +
                                                        $"Probe Z+ yönünde {collisionZStep}mm konumda parça ile çarpışıyor (Birinci Nokta).",
                                                        "Angle Measurement - Çarpışma Uyarısı",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Warning
                                                    );
                                                    
                                                    // ✅ 5. Moddan çık
                                                    Disable();
                                                    Debug.WriteLine($"⛔ Angle Measurement modu kapatıldı (Birinci Nokta - Z+{collisionZStep}mm Çarpışma)");
                                                    
                                                    Debug.WriteLine("═══════════════════════════════════════");
                                                    return;  // İşlemi iptal et
                                                }
                                                else
                                                {
                                                    Debug.WriteLine("═══════════════════════════════════════");
                                                    Debug.WriteLine($"✅ Z+ KONTROLÜ TAMAMLANDI - ÇARPIŞMA YOK ({zStepMax}mm kontrol edildi)");
                                                    Debug.WriteLine("═══════════════════════════════════════");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Debug.WriteLine("⚠️ treeViewManager null - Retract döngüsü yapılamadı");
                                        }
                                    }
                                    else
                                    {
                                        Debug.WriteLine("⚠️ meshConverter null - Çarpışma kontrolü yapılamadı");
                                    }
                                    // ═══════════════════════════════════════════════════════════
                                }
                            }
                            Debug.WriteLine("═══════════════════════════════════════");
                        }
                        catch (Exception probeEx)
                        {
                            Debug.WriteLine($"❌ Probe mesh ekleme hatası: {probeEx.Message}");
                            Debug.WriteLine($"   StackTrace: {probeEx.StackTrace}");
                            Debug.WriteLine("═══════════════════════════════════════");
                        }
                    }
                    // ═══════════════════════════════════════════════════════════
                    
                    // ✅ ÇARPIŞMA KONTROLÜ BİTTİ - PROBE'U SİL (EKRANDAN KALDIR)
                    if (probe1 != null)
                    {
                        design.Entities.Remove(probe1);
                        // probe1 = null; yapma - ikinci nokta için hala lazım olabilir
                        Debug.WriteLine("🗑️ Probe1 ekrandan silindi (Çarpışma yok - Görünürlük kapatıldı)");
                    }
                    design.Invalidate();
                    
                    // ✅ YENİ: TreeView'e 1. noktayı ekle
                    if (treeViewManager != null && currentGroupNode != null)
                    {
                        treeViewManager.AddPointToAngleMeasurementGroup(currentGroupNode, point1, 1);
                        Debug.WriteLine("✅ TreeView'a nokta eklendi: Point 1");
                        
                        // ═══════════════════════════════════════════════════════════
                        // ✅ YENİ: MeasurementDataManager'a ekle
                        // ═══════════════════════════════════════════════════════════
                        
                        if (_currentGroupId > 0 && dataManager != null)
                        {
                            // Değişkenleri al
                            ProbeData? selectedProbe = treeViewManager.GetSelectedProbeData();
                            double retractDistance = treeViewManager.RetractDistance;
                            double zSafetyDistance = treeViewManager.ZSafetyDistance;
                            
                            // Marker pozisyonunu hesapla (visualPosition - normal yönünde D/2 offset)
                            double markerOffset = probeDiameter / 2.0;
                            Point3D markerPosition = new Point3D(
                                point1.X + surfaceData.Normal.X * markerOffset,
                                point1.Y + surfaceData.Normal.Y * markerOffset,
                                point1.Z + surfaceData.Normal.Z * markerOffset
                            );
                            
                            // MeasurementPoint oluştur
                            var measurementPoint = new MeasurementPoint
                            {
                                MeasurementMode = "Angle",
                                GroupId = _currentGroupId,
                                PointIndex = 0,  // İlk nokta
                                Position = point1,
                                MarkerPosition = markerPosition,
                                SurfaceNormal = surfaceData.Normal,
                                ProbeName = selectedProbe?.Name ?? "Unknown",
                                ProbeDiameter = probeDiameter,
                                RetractDistance = retractDistance,
                                ZSafety = zSafetyDistance,
                                ApproachPoint = new Point3D(
                                    markerPosition.X + surfaceData.Normal.X * retractDistance,
                                    markerPosition.Y + surfaceData.Normal.Y * retractDistance,
                                    markerPosition.Z + surfaceData.Normal.Z * retractDistance
                                ),
                                TouchPoint = point1,
                                CreatedAt = DateTime.Now,
                                IsActive = true,
                                Notes = ""
                            };
                            
                            // MeasurementDataManager'a ekle
                            bool success = MeasurementDataManager.Instance.AddPoint(_currentGroupId, measurementPoint);
                            
                            if (success)
                            {
                                Debug.WriteLine($"✅ DataManager'a nokta eklendi (Angle): Group={_currentGroupId}, Point #1");
                            }
                            else
                            {
                                Debug.WriteLine($"❌ DataManager'a nokta eklenemedi!");
                            }
                        }
                        
                        Debug.WriteLine("✅ 1. nokta TreeView'e eklendi");
                    }
                    
                    // ADIM 8: Diğer yüzeyleri orijinal renge döndür (sadece seçilen sarı kalsın)
                    RestoreNonSelectedSurface(entityIndex);
                    
                    // InstructionPanel güncelle
                    if (instructionPanel != null && !instructionPanel.IsDisposed)
                    {
                        instructionPanel.UpdatePanel(
                            InstructionTexts.TITLE_ANGLE_MEASUREMENT,
                            "Pick second point on the SAME surface..."
                        );
                    }
                    
                    Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }
                
                // ═══════════════════════════════════════════════════════════
                // ADIM 9: İKİNCİ NOKTA SEÇİMİ - AYNI YÜZEY KONTROLÜ
                // ═══════════════════════════════════════════════════════════
                if (point2 == null)
                {
                    // İkinci nokta seçiliyor - AYNI YÜZEY OLMALI!
                    int firstEntityIndex = design.Entities.IndexOf(selectedFace);
                    
                    if (entityIndex != firstEntityIndex)
                    {
                        Debug.WriteLine("⛔ İKİNCİ NOKTA AYNI YÜZEYDEN SEÇİLMELİ!");
                        Debug.WriteLine($"   İlk seçilen entity: {firstEntityIndex}");
                        Debug.WriteLine($"   Tıkladığınız entity: {entityIndex}");
                        Debug.WriteLine("   Lütfen aynı sarı yüzey üzerinden ikinci noktayı seçin!");
                        Debug.WriteLine("═══════════════════════════════════════");
                        return;
                    }
                    
                    // Aynı yüzey → İkinci nokta kabul edildi
                    point2 = clickedPoint;
                    
                    Debug.WriteLine($"✅ İKİNCİ NOKTA SEÇİLDİ: ({point2.X:F3}, {point2.Y:F3}, {point2.Z:F3})");
                    
                    // ADIM 7: Marker Ekle (Kaydırılmış pozisyonda - SADECE GÖRSEL)
                    // Unique isim ver: currentGroupNode.Text'ten grup numarasını al
                    string groupNumber = currentGroupNode?.Text?.Replace("Angle ", "") ?? "0";
                    string markerName = $"AngleMarker_{groupNumber}_Point2";
                    marker2 = AddMarker(point2, Color.Blue, markerName, surfaceData.Normal, probeDiameter);
                    
                    // ═══════════════════════════════════════════════════════════
                    // ✅ YENİ: PROBE MESH OLUŞTUR VE YERLEŞTİR (İKİNCİ NOKTA)
                    // ═══════════════════════════════════════════════════════════
                    if (treeViewManager != null)
                    {
                        try
                        {
                            Debug.WriteLine("═══════════════════════════════════════");
                            Debug.WriteLine("🔧 PROBE MESH YERLEŞTİRME BAŞLIYOR (İKİNCİ NOKTA)...");
                            
                            // 1. Seçili probe'u al
                            var selectedProbe = treeViewManager.GetSelectedProbeData();
                            if (selectedProbe == null)
                            {
                                Debug.WriteLine("❌ Seçili probe bulunamadı!");
                            }
                            else
                            {
                                Debug.WriteLine($"   Seçili Probe: D={selectedProbe.D}mm");
                                
                                // 2. Probe mesh'ini oluştur
                                Mesh probeMesh = _014.Probe.Core.ProbeBuilder.CreateProbeMesh(selectedProbe);
                                if (probeMesh == null)
                                {
                                    Debug.WriteLine("❌ ProbeBuilder.CreateProbeMesh() null döndü!");
                                }
                                else
                                {
                                    Debug.WriteLine($"✅ Probe mesh oluşturuldu (Vertex: {probeMesh.Vertices.Length})");
                                    
                                    // 3. Mesh'in klonunu al
                                    Mesh displayProbe = (Mesh)probeMesh.Clone();
                                    Debug.WriteLine("✅ Probe mesh klonlandı");
                                    
                                    // 4. ADIM 1: X0Y0Z0'da başla (zaten origin'de)
                                    Debug.WriteLine("   Başlangıç: X=0, Y=0, Z=0");
                                    
                                    // 5. ADIM 2: Kullanıcının seçtiği koordinatlara kaydır (İKİNCİ NOKTA!)
                                    displayProbe.Translate(point2.X, point2.Y, point2.Z);
                                    Debug.WriteLine($"   Kullanıcı noktasına kaydırıldı: ({point2.X:F3}, {point2.Y:F3}, {point2.Z:F3})");
                                    
                                    // 6. ADIM 3: Z- yönünde D/2 kaydır
                                    double probeRadius = (double)selectedProbe.D / 2.0;
                                    displayProbe.Translate(0, 0, -probeRadius);
                                    Debug.WriteLine($"   Z- yönünde kaydırıldı: -D/2 = {-probeRadius:F3}mm");
                                    
                                    // 7. ADIM 4: Normal yönünde D*0.6mm kaydır
                                    double offset = (double)selectedProbe.D * 0.6;
                                    displayProbe.Translate(
                                        surfaceData.Normal.X * offset,
                                        surfaceData.Normal.Y * offset,
                                        surfaceData.Normal.Z * offset
                                    );
                                    Debug.WriteLine($"   Normal yönünde kaydırıldı: D*0.6 = {offset:F3}mm");
                                    Debug.WriteLine($"   Normal: ({surfaceData.Normal.X:F3}, {surfaceData.Normal.Y:F3}, {surfaceData.Normal.Z:F3})");
                                    
                                    // 8. Probe özelliklerini ayarla (SADECE ÇARPIŞMA İÇİN)
                                    displayProbe.Visible = false;  // ✅ PROBE GÖRÜNMEZ!
                                    displayProbe.Color = Color.Blue;  // ✅ MAVİ PROBE
                                    displayProbe.ColorMethod = colorMethodType.byEntity;
                                    displayProbe.LayerName = PROBE_LAYER_NAME;
                                    Debug.WriteLine($"   Probe renk: BEYAZ, Layer: {PROBE_LAYER_NAME}");
                                    
                                    // 9. Probe'u field'a kaydet (çarpışma kontrolü için)
                                    probe2 = displayProbe;
                                    
                                    Debug.WriteLine("✅ PROBE MESH OLUŞTURULDU (İKİNCİ NOKTA - EKRANDA GÖRÜNMİYOR - SADECE ÇARPIŞMA KONTROLÜ)!");
                                    Debug.WriteLine($"   Final Pozisyon: Kullanıcı noktası + Z-({probeRadius:F3}mm) + Normal*{offset:F3}mm");
                                    
                                    // ═══════════════════════════════════════════════════════════
                                    // ✅ PROBE'U GEÇİCİ OLARAK EKLE (Eyeshot CollisionDetection için gerekli)
                                    // ═══════════════════════════════════════════════════════════
                                    design.Entities.Add(displayProbe);
                                    
                                    design.Invalidate();
                                    
                                    // ═══════════════════════════════════════════════════════════
                                    // ✅ YENİ: ÇARPIŞMA KONTROLÜ (İKİNCİ NOKTA)
                                    // ═══════════════════════════════════════════════════════════
                                    if (meshConverter != null)
                                    {
                                        Debug.WriteLine("═══════════════════════════════════════");
                                        Debug.WriteLine("🔍 ÇARPIŞMA KONTROLÜ BAŞLADI (İKİNCİ NOKTA)...");
                                        
                                        List<Mesh> partMeshes = meshConverter.GetMeshesForCollision();
                                        Debug.WriteLine($"📦 Kontrol edilecek mesh sayısı: {partMeshes.Count}");
                                        
                                        bool hasCollision = false;
                                        foreach (Mesh partMesh in partMeshes)
                                        {
                                            // Mesh validasyonu
                                            if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                                continue;
                                            
                                            try
                                            {
                                                // Eyeshot CollisionDetection
                                                CollisionDetection cd = new CollisionDetection(
                                                    new Entity[] { displayProbe },  // Yerleştirilmiş probe
                                                    new Entity[] { partMesh },      // Parça mesh
                                                    null
                                                );
                                                
                                                cd.CheckMethod = collisionCheckType.SubdivisionTree;
                                                cd.DoWork();
                                                
                                                if (cd.Result != null && cd.Result.Length > 0)
                                                {
                                                    hasCollision = true;
                                                    Debug.WriteLine("💥 ÇARPIŞMA TESPİT EDİLDİ!");
                                                    break;
                                                }
                                            }
                                            catch (Exception collisionEx)
                                            {
                                                Debug.WriteLine($"❌ Çarpışma kontrolü hatası: {collisionEx.Message}");
                                            }
                                        }
                                        
                                        if (hasCollision)
                                        {
                                            Debug.WriteLine("═══════════════════════════════════════");
                                            Debug.WriteLine("⛔ ÇARPIŞMA VAR (İKİNCİ NOKTA)");
                                            Debug.WriteLine("═══════════════════════════════════════");
                                            
                                            // ✅ 1. PROBE'LARI GÖRÜNÜR YAP (Çarpışma yerini göster!)
                                            if (probe2 != null)
                                            {
                                                probe2.Visible = true;
                                                Debug.WriteLine("👁️ Probe2 görünür yapıldı (Çarpışma gösterimi)");
                                            }
                                            
                                            design.Invalidate();
                                            Application.DoEvents();  // UI thread'i güncelle
                                            
                                            // ✅ 2. MessageBox göster
                                            MessageBox.Show(
                                                "⚠️ ÇARPIŞMA TESPİT EDİLDİ!\n\n" +
                                                "Probe parça ile çarpışıyor (İkinci Nokta).",
                                                "Angle Measurement - Çarpışma Uyarısı",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Warning
                                            );
                                            
                                            // ✅ 3. Kullanıcı OK'e tıkladı - Probe ve marker'ları sil
                                            if (probe1 != null)
                                            {
                                                design.Entities.Remove(probe1);
                                                probe1 = null;
                                            }
                                            if (probe2 != null)
                                            {
                                                design.Entities.Remove(probe2);
                                                probe2 = null;
                                            }
                                            if (marker1 != null)
                                            {
                                                design.Entities.Remove(marker1);
                                                marker1 = null;
                                            }
                                            if (marker2 != null)
                                            {
                                                design.Entities.Remove(marker2);
                                                marker2 = null;
                                            }
                                            Debug.WriteLine("🗑️ Probe1, Probe2, Marker1, Marker2 silindi (İkinci Nokta - Çarpışma)");
                                            
                                            // ✅ 4. TreeView + DataManager'dan grubu sil
                                            if (_currentGroupId != -1 && treeViewManager != null)
                                            {
                                                treeViewManager.RemoveGroup(_currentGroupId);
                                                Debug.WriteLine($"🗑️ TreeView + DataManager'dan grup silindi: ID={_currentGroupId}");
                                                _currentGroupId = -1;
                                            }
                                            
                                            // ✅ 5. Ekranı güncelle
                                            design.Entities.Regen();
                                            design.Invalidate();
                                            
                                            // ✅ 6. Moddan çık
                                            Disable();
                                            Debug.WriteLine("⛔ Angle Measurement modu kapatıldı (İkinci Nokta - Çarpışma)");
                                            
                                            Debug.WriteLine("═══════════════════════════════════════");
                                            return;  // İşlemi iptal et
                                        }
                                        else
                                        {
                                            Debug.WriteLine("✅ ÇARPIŞMA YOK (İLK KONUM - D*0.6)");
                                        }
                                        
                                        // ═══════════════════════════════════════════════════════════
                                        // ✅ RETRACT DÖNGÜSÜ (1MM ADIMLARLA ÇARPIŞMA KONTROLÜ)
                                        // ═══════════════════════════════════════════════════════════
                                        if (treeViewManager != null)
                                        {
                                            double retractDistance = treeViewManager.RetractDistance;
                                            int stepCount = (int)retractDistance;
                                            
                                            Debug.WriteLine("═══════════════════════════════════════");
                                            Debug.WriteLine($"🔁 RETRACT DÖNGÜSÜ BAŞLIYOR: {stepCount} adım (1mm → {stepCount}mm)");
                                            Debug.WriteLine("═══════════════════════════════════════");
                                            
                                            bool hasCollisionInLoop = false;
                                            int collisionStep = 0;
                                            
                                            for (int i = 0; i < stepCount; i++)
                                            {
                                                // Her adımda 1mm kaydır (normal yönünde)
                                                displayProbe.Translate(
                                                    surfaceData.Normal.X * 1.0,
                                                    surfaceData.Normal.Y * 1.0,
                                                    surfaceData.Normal.Z * 1.0
                                                );
                                                design.Invalidate();
                                                
                                                Debug.WriteLine($"   🔍 Adım {i + 1}/{stepCount}: +{i + 1}mm konumda kontrol (Toplam: D*0.6 + {i + 1}mm)");
                                                
                                                // Çarpışma kontrolü
                                                foreach (Mesh partMesh in partMeshes)
                                                {
                                                    // Mesh validasyonu
                                                    if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                                        continue;
                                                    
                                                    try
                                                    {
                                                        // Eyeshot CollisionDetection
                                                        CollisionDetection cd = new CollisionDetection(
                                                            new Entity[] { displayProbe },  // Kaydırılmış probe
                                                            new Entity[] { partMesh },      // Parça mesh
                                                            null
                                                        );
                                                        
                                                        cd.CheckMethod = collisionCheckType.SubdivisionTree;
                                                        cd.DoWork();
                                                        
                                                        if (cd.Result != null && cd.Result.Length > 0)
                                                        {
                                                            hasCollisionInLoop = true;
                                                            collisionStep = i + 1;
                                                            Debug.WriteLine($"   💥 ÇARPIŞMA TESPİT EDİLDİ! (+{i + 1}mm konumda)");
                                                            break;
                                                        }
                                                    }
                                                    catch (Exception collisionEx)
                                                    {
                                                        Debug.WriteLine($"   ❌ Döngü çarpışma kontrolü hatası (Adım {i + 1}): {collisionEx.Message}");
                                                    }
                                                }
                                                
                                                // Çarpışma varsa döngüden çık
                                                if (hasCollisionInLoop)
                                                    break;
                                            }
                                            
                                            // ═══════════════════════════════════════════════════════════
                                            // DÖNGÜ SONRASI KONTROL
                                            // ═══════════════════════════════════════════════════════════
                                            if (hasCollisionInLoop)
                                            {
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                Debug.WriteLine($"⛔ ÇARPIŞMA VAR (İKİNCİ NOKTA - +{collisionStep}MM KONUM)");
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                
                                                // ✅ 1. Önce marker ve probe'ları sil
                                                if (probe1 != null)
                                                {
                                                    design.Entities.Remove(probe1);
                                                    probe1 = null;
                                                }
                                                if (probe2 != null)
                                                {
                                                    design.Entities.Remove(probe2);
                                                    probe2 = null;
                                                }
                                                if (marker1 != null)
                                                {
                                                    design.Entities.Remove(marker1);
                                                    marker1 = null;
                                                }
                                                if (marker2 != null)
                                                {
                                                    design.Entities.Remove(marker2);
                                                    marker2 = null;
                                                }
                                                Debug.WriteLine($"🗑️ Probe1, Probe2, Marker1, Marker2 silindi (İkinci Nokta - +{collisionStep}mm Çarpışma)");
                                                
                                                // ✅ 2. TreeView + DataManager'dan grubu sil
                                                if (_currentGroupId != -1 && treeViewManager != null)
                                                {
                                                    treeViewManager.RemoveGroup(_currentGroupId);
                                                    Debug.WriteLine($"🗑️ TreeView + DataManager'dan grup silindi: ID={_currentGroupId}");
                                                    _currentGroupId = -1;
                                                }
                                                currentGroupNode = null;
                                                
                                                // ✅ 3. Ekranı ANINDA güncelle ve bekle
                                                design.Entities.Regen();  // Entity listesini yeniden oluştur
                                                design.Invalidate();
                                                Application.DoEvents();  // UI thread'i güncelle
                                                System.Threading.Thread.Sleep(50);  // 50ms bekle - Ekran güncellemesi için
                                                
                                                // ✅ 4. SONRA MessageBox göster
                                                MessageBox.Show(
                                                    $"⚠️ ÇARPIŞMA TESPİT EDİLDİ!\n\n" +
                                                    $"Probe normal yönünde +{collisionStep}mm konumda parça ile çarpışıyor (İkinci Nokta).",
                                                    "Angle Measurement - Çarpışma Uyarısı",
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Warning
                                                );
                                                
                                                // ✅ 5. Moddan çık
                                                Disable();
                                                Debug.WriteLine($"⛔ Angle Measurement modu kapatıldı (İkinci Nokta - +{collisionStep}mm Çarpışma)");
                                                
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                return;  // İşlemi iptal et
                                            }
                                            else
                                            {
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                Debug.WriteLine($"✅ TÜM RETRACT KONTROLÜ TAMAM - ÇARPIŞMA YOK ({stepCount} adım)");
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                
                                                // ═══════════════════════════════════════════════════════════
                                                // ✅ Z+ YÖNÜNDEKİ ÇARPIŞMA KONTROLÜ (50-CLEARANCE PLANE)
                                                // ═══════════════════════════════════════════════════════════
                                                
                                                // Clearance Plane değerini Form1'den al
                                                double clearancePlaneValue = 350.0; // Varsayılan
                                                if (parentForm is CNC_Measurement form1)
                                                {
                                                    if (double.TryParse(form1.txt_form1_Clerance.Text, out double parsedValue))
                                                    {
                                                        clearancePlaneValue = parsedValue;
                                                    }
                                                }
                                                int zStepMax = (int)clearancePlaneValue;
                                                
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                Debug.WriteLine($"🔍 Z+ YÖNÜNDE DÖNGÜ BAŞLIYOR (50mm → {zStepMax}mm adımlarla)...");
                                                Debug.WriteLine("═══════════════════════════════════════");
                                                
                                                bool hasCollisionZPlus = false;
                                                int collisionZStep = 0;
                                                
                                                for (int zStep = 50; zStep <= zStepMax; zStep += 50)
                                                {
                                                    // 50mm Z+ yönünde hareket
                                                    displayProbe.Translate(0, 0, 50.0);
                                                    design.Invalidate();
                                                    
                                                    Debug.WriteLine($"   🔍 Z+ Adım: {zStep}mm yukarı çıkıldı (Toplam Z+{zStep}mm)");
                                                    
                                                    // Çarpışma kontrolü
                                                    foreach (Mesh partMesh in partMeshes)
                                                    {
                                                        // Mesh validasyonu
                                                        if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                                            continue;
                                                        
                                                        try
                                                        {
                                                            // Eyeshot CollisionDetection
                                                            CollisionDetection cdZPlus = new CollisionDetection(
                                                                new Entity[] { displayProbe },  // Z+ konumundaki probe
                                                                new Entity[] { partMesh },      // Parça mesh
                                                                null
                                                            );
                                                            
                                                            cdZPlus.CheckMethod = collisionCheckType.SubdivisionTree;
                                                            cdZPlus.DoWork();
                                                            
                                                            if (cdZPlus.Result != null && cdZPlus.Result.Length > 0)
                                                            {
                                                                hasCollisionZPlus = true;
                                                                collisionZStep = zStep;
                                                                Debug.WriteLine($"   💥 ÇARPIŞMA TESPİT EDİLDİ! (Z+{zStep}mm konumda)");
                                                                break;
                                                            }
                                                        }
                                                        catch (Exception collisionExZ)
                                                        {
                                                            Debug.WriteLine($"   ❌ Z+ çarpışma kontrolü hatası (Z+{zStep}mm): {collisionExZ.Message}");
                                                        }
                                                    }
                                                    
                                                    // Çarpışma varsa döngüden çık
                                                    if (hasCollisionZPlus)
                                                        break;
                                                }
                                                
                                                // ═══════════════════════════════════════════════════════════
                                                // Z+ DÖNGÜ SONRASI KONTROL
                                                // ═══════════════════════════════════════════════════════════
                                                if (hasCollisionZPlus)
                                                {
                                                    Debug.WriteLine("═══════════════════════════════════════");
                                                    Debug.WriteLine($"⛔ ÇARPIŞMA VAR (İKİNCİ NOKTA - Z+{collisionZStep}MM KONUM)");
                                                    Debug.WriteLine("═══════════════════════════════════════");
                                                    
                                                    // ✅ 1. Önce marker ve probe'ları sil
                                                    if (probe1 != null)
                                                    {
                                                        design.Entities.Remove(probe1);
                                                        probe1 = null;
                                                    }
                                                    if (probe2 != null)
                                                    {
                                                        design.Entities.Remove(probe2);
                                                        probe2 = null;
                                                    }
                                                    if (marker1 != null)
                                                    {
                                                        design.Entities.Remove(marker1);
                                                        marker1 = null;
                                                    }
                                                    if (marker2 != null)
                                                    {
                                                        design.Entities.Remove(marker2);
                                                        marker2 = null;
                                                    }
                                                    Debug.WriteLine($"🗑️ Probe1, Probe2, Marker1, Marker2 silindi (İkinci Nokta - Z+{collisionZStep}mm Çarpışma)");
                                                    
                                                    // ✅ 2. TreeView + DataManager'dan grubu sil
                                                    if (_currentGroupId != -1 && treeViewManager != null)
                                                    {
                                                        treeViewManager.RemoveGroup(_currentGroupId);
                                                        Debug.WriteLine($"🗑️ TreeView + DataManager'dan grup silindi: ID={_currentGroupId}");
                                                        _currentGroupId = -1;
                                                    }
                                                    currentGroupNode = null;
                                                    
                                                    // ✅ 3. Ekranı ANINDA güncelle ve bekle
                                                    design.Entities.Regen();  // Entity listesini yeniden oluştur
                                                    design.Invalidate();
                                                    Application.DoEvents();  // UI thread'i güncelle
                                                    System.Threading.Thread.Sleep(50);  // 50ms bekle - Ekran güncellemesi için
                                                    
                                                    // ✅ 4. SONRA MessageBox göster
                                                    MessageBox.Show(
                                                        $"⚠️ ÇARPIŞMA TESPİT EDİLDİ!\n\n" +
                                                        $"Probe Z+ yönünde {collisionZStep}mm konumda parça ile çarpışıyor (İkinci Nokta).",
                                                        "Angle Measurement - Çarpışma Uyarısı",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Warning
                                                    );
                                                    
                                                    // ✅ 5. Moddan çık
                                                    Disable();
                                                    Debug.WriteLine($"⛔ Angle Measurement modu kapatıldı (İkinci Nokta - Z+{collisionZStep}mm Çarpışma)");
                                                    
                                                    Debug.WriteLine("═══════════════════════════════════════");
                                                    return;  // İşlemi iptal et
                                                }
                                                else
                                                {
                                                    Debug.WriteLine("═══════════════════════════════════════");
                                                    Debug.WriteLine($"✅ Z+ KONTROLÜ TAMAMLANDI - ÇARPIŞMA YOK ({zStepMax}mm kontrol edildi)");
                                                    Debug.WriteLine("═══════════════════════════════════════");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Debug.WriteLine("⚠️ treeViewManager null - Retract döngüsü yapılamadı");
                                        }
                                        
                                        Debug.WriteLine("═══════════════════════════════════════");
                                    }
                                    else
                                    {
                                        Debug.WriteLine("⚠️ meshConverter null - Çarpışma kontrolü yapılamadı");
                                    }
                                    // ═══════════════════════════════════════════════════════════
                                }
                            }
                            Debug.WriteLine("═══════════════════════════════════════");
                        }
                        catch (Exception probeEx)
                        {
                            Debug.WriteLine($"❌ Probe mesh ekleme hatası (İKİNCİ NOKTA): {probeEx.Message}");
                            Debug.WriteLine($"   StackTrace: {probeEx.StackTrace}");
                            Debug.WriteLine("═══════════════════════════════════════");
                        }
                    }
                    // ═══════════════════════════════════════════════════════════
                    
                    // ✅ ÇARPIŞMA KONTROLÜ BİTTİ - PROBE'U SİL (EKRANDAN KALDIR)
                    if (probe2 != null)
                    {
                        design.Entities.Remove(probe2);
                        probe2 = null;
                        Debug.WriteLine("🗑️ Probe2 ekrandan silindi (Çarpışma yok - Görünürlük kapatıldı)");
                    }
                    // Probe1'i de temizle (artık işimiz bitti)
                    if (probe1 != null)
                    {
                        design.Entities.Remove(probe1);
                        probe1 = null;
                        Debug.WriteLine("🗑️ Probe1 ekrandan silindi (Temizlik)");
                    }
                    design.Invalidate();
                    
                    // ✅ YENİ: TreeView'e 2. noktayı ekle
                    if (treeViewManager != null && currentGroupNode != null)
                    {
                        treeViewManager.AddPointToAngleMeasurementGroup(currentGroupNode, point2, 2);
                        Debug.WriteLine("✅ TreeView'a nokta eklendi: Point 2");
                        
                        // ═══════════════════════════════════════════════════════════
                        // ✅ YENİ: MeasurementDataManager'a ekle
                        // ═══════════════════════════════════════════════════════════
                        
                        if (_currentGroupId > 0 && dataManager != null)
                        {
                            // Değişkenleri al
                            ProbeData? selectedProbe = treeViewManager.GetSelectedProbeData();
                            double retractDistance = treeViewManager.RetractDistance;
                            double zSafetyDistance = treeViewManager.ZSafetyDistance;
                            
                            // Marker pozisyonunu hesapla (visualPosition - normal yönünde D/2 offset)
                            double markerOffset = probeDiameter / 2.0;
                            Point3D markerPosition = new Point3D(
                                point2.X + surfaceData.Normal.X * markerOffset,
                                point2.Y + surfaceData.Normal.Y * markerOffset,
                                point2.Z + surfaceData.Normal.Z * markerOffset
                            );
                            
                            // MeasurementPoint oluştur
                            var measurementPoint = new MeasurementPoint
                            {
                                MeasurementMode = "Angle",
                                GroupId = _currentGroupId,
                                PointIndex = 1,  // İkinci nokta
                                Position = point2,
                                MarkerPosition = markerPosition,
                                SurfaceNormal = surfaceData.Normal,
                                ProbeName = selectedProbe?.Name ?? "Unknown",
                                ProbeDiameter = probeDiameter,
                                RetractDistance = retractDistance,
                                ZSafety = zSafetyDistance,
                                ApproachPoint = new Point3D(
                                    markerPosition.X + surfaceData.Normal.X * retractDistance,
                                    markerPosition.Y + surfaceData.Normal.Y * retractDistance,
                                    markerPosition.Z + surfaceData.Normal.Z * retractDistance
                                ),
                                TouchPoint = point2,
                                CreatedAt = DateTime.Now,
                                IsActive = true,
                                Notes = ""
                            };
                            
                            // MeasurementDataManager'a ekle
                            bool success = MeasurementDataManager.Instance.AddPoint(_currentGroupId, measurementPoint);
                            
                            if (success)
                            {
                                Debug.WriteLine($"✅ DataManager'a nokta eklendi (Angle): Group={_currentGroupId}, Point #2");
                            }
                            else
                            {
                                Debug.WriteLine($"❌ DataManager'a nokta eklenemedi!");
                            }
                        }
                        
                        Debug.WriteLine("✅ 2. nokta TreeView'e eklendi");
                    }
                    
                    // İki nokta arası çizgi çiz
                    DrawLineBetweenPoints(point1, point2);
                    
                    // ✅ YENİ: Açı hesapla
                    CalculateAngle();
                    
                    Debug.WriteLine("✅ İki nokta seçildi - Açı hesaplandı!");
                    Debug.WriteLine("═══════════════════════════════════════");
                    
                    // Modu kapat (geçici - sonra kaldırılacak)
                    Disable();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ design_MouseClick hatası: {ex.Message}");
                Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                Debug.WriteLine("═══════════════════════════════════════");
            }
        }

        /// <summary>
        /// Klavye tuşu basıldığında - ESC ile çıkış
        /// </summary>
        private void Design_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Disable();
                Debug.WriteLine("⛔ ESC tuşu: Angle Measurement modu kapatıldı");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE METHODS - CALCULATIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// İki nokta arasındaki çizginin açısını hesaplar (düzleme iz düşümü ile 2D)
        /// </summary>
        private void CalculateAngle()
        {
            try
            {
                if (point1 == null || point2 == null || facePlane == null)
                {
                    Debug.WriteLine("❌ CalculateAngle: Noktalar veya yüzey düzlemi null!");
                    return;
                }
                
                Debug.WriteLine("═══════════════════════════════════════");
                Debug.WriteLine("📐 AÇI HESAPLAMA BAŞLIYOR (İz Düşümü ile 2D)...");
                Debug.WriteLine($"   Point1 (3D): ({point1.X:F3}, {point1.Y:F3}, {point1.Z:F3})");
                Debug.WriteLine($"   Point2 (3D): ({point2.X:F3}, {point2.Y:F3}, {point2.Z:F3})");
                
                // ✅ Yüzeyin normal vektörü
                Vector3D normal = facePlane.AxisZ;
                Debug.WriteLine($"   Normal Vektör: ({normal.X:F3}, {normal.Y:F3}, {normal.Z:F3})");
                
                string planeType = "";
                string referenceAxisName = "";
                double angle2D = 0;
                
                // ═══════════════════════════════════════════════════════════
                // Y EKSENİ DİK (Y- ve Y+ yüzeyleri: BACK/FRONT)
                // ═══════════════════════════════════════════════════════════
                if (Math.Abs(normal.Y) > 0.9)
                {
                    planeType = "YZ Plane";
                    referenceAxisName = "Y Axis";
                    
                    // İz düşümü: YZ düzlemine (X=0, Y, Z)
                    Debug.WriteLine($"   İz Düşümü: YZ düzlemi (X=0)");
                    Debug.WriteLine($"   Point1 (YZ): (X=0, Y={point1.Y:F3}, Z={point1.Z:F3})");
                    Debug.WriteLine($"   Point2 (YZ): (X=0, Y={point2.Y:F3}, Z={point2.Z:F3})");
                    
                    // Vektör: (ΔY, ΔZ)
                    double deltaY = point2.Y - point1.Y;
                    double deltaZ = point2.Z - point1.Z;
                    Debug.WriteLine($"   Vektör: (ΔY={deltaY:F3}, ΔZ={deltaZ:F3})");
                    
                    // Referans: Y ekseni = (0, 1, 0)
                    Debug.WriteLine($"   Referans: Y ekseni (yatay)");
                    
                    // Açı: Y ekseninden çizgiye doğru
                    angle2D = Math.Atan2(deltaZ, deltaY) * (180.0 / Math.PI);
                    
                    // Mutlak değer al (pozitif açı)
                    angle2D = Math.Abs(angle2D);
                }
                // ═══════════════════════════════════════════════════════════
                // X EKSENİ DİK (X- ve X+ yüzeyleri: LEFT/RIGHT)
                // ═══════════════════════════════════════════════════════════
                else if (Math.Abs(normal.X) > 0.9)
                {
                    planeType = "XZ Plane";
                    referenceAxisName = "X Axis";
                    
                    // İz düşümü: XZ düzlemine (X, Y=0, Z)
                    Debug.WriteLine($"   İz Düşümü: XZ düzlemi (Y=0)");
                    Debug.WriteLine($"   Point1 (XZ): (X={point1.X:F3}, Y=0, Z={point1.Z:F3})");
                    Debug.WriteLine($"   Point2 (XZ): (X={point2.X:F3}, Y=0, Z={point2.Z:F3})");
                    
                    // Vektör: (ΔX, ΔZ)
                    double deltaX = point2.X - point1.X;
                    double deltaZ = point2.Z - point1.Z;
                    Debug.WriteLine($"   Vektör: (ΔX={deltaX:F3}, ΔZ={deltaZ:F3})");
                    
                    // Referans: Y ekseni
                    Debug.WriteLine($"   Referans: Y ekseni");
                    
                    // Açı: Y ekseninden çizgiye doğru
                    angle2D = Math.Atan2(deltaZ, deltaX) * (180.0 / Math.PI);
                    angle2D = Math.Abs(angle2D);
                }
                // ═══════════════════════════════════════════════════════════
                // Z EKSENİ DİK (Z+ ve Z- yüzeyleri: TOP/BOTTOM)
                // ═══════════════════════════════════════════════════════════
                else if (Math.Abs(normal.Z) > 0.9)
                {
                    planeType = "XY Plane";
                    
                    // İz düşümü: XY düzlemine (X, Y, Z=0)
                    Debug.WriteLine($"   İz Düşümü: XY düzlemi (Z=0)");
                    Debug.WriteLine($"   Point1 (XY): (X={point1.X:F3}, Y={point1.Y:F3}, Z=0)");
                    Debug.WriteLine($"   Point2 (XY): (X={point2.X:F3}, Y={point2.Y:F3}, Z=0)");
                    
                    // Vektör: (ΔX, ΔY)
                    double deltaX = point2.X - point1.X;
                    double deltaY = point2.Y - point1.Y;
                    Debug.WriteLine($"   Vektör: (ΔX={deltaX:F3}, ΔY={deltaY:F3})");
                    
                    // ✅ Büyük delta kontrolü ile referans belirleme
                    if (Math.Abs(deltaX) > Math.Abs(deltaY))
                    {
                        // X değişimi büyük → Yatay çizgi → X referans
                        referenceAxisName = "X Axis";
                        Debug.WriteLine($"   |ΔX|={Math.Abs(deltaX):F3} > |ΔY|={Math.Abs(deltaY):F3} → Yatay çizgi");
                        Debug.WriteLine($"   Referans: X Axis (yatay)");
                        angle2D = 0.0;
                    }
                    else
                    {
                        // Y değişimi büyük → Y eksenine paralel → Y referans
                        referenceAxisName = "Y Axis";
                        Debug.WriteLine($"   |ΔY|={Math.Abs(deltaY):F3} > |ΔX|={Math.Abs(deltaX):F3} → Y eksenine paralel");
                        Debug.WriteLine($"   Referans: Y Axis (dikey)");
                        angle2D = 0.0;
                    }
                }
                // ═══════════════════════════════════════════════════════════
                // EĞİK YÜZEY (INCLINED)
                // ═══════════════════════════════════════════════════════════
                else
                {
                    planeType = "INCLINED";
                    
                    Debug.WriteLine($"   Düzlem: EĞİK");
                    
                    // ═══════════════════════════════════════════════════════════
                    // EĞİK YÜZEY ÖZEL DURUM 1: X ≈ 0 && Y ≠ 0 && Z ≠ 0 (YZ düzlemi)
                    // ═══════════════════════════════════════════════════════════
                    if (Math.Abs(normal.X) < 0.1 && Math.Abs(normal.Y) > 0.1 && Math.Abs(normal.Z) > 0.1)
                    {
                        referenceAxisName = "Y Axis";
                        
                        // İz düşümü: YZ düzlemine (X=0, Y, Z)
                        Debug.WriteLine($"   Eğik yüzey tipi: YZ düzlemi (X ≈ 0)");
                        Debug.WriteLine($"   İz Düşümü: YZ düzlemi (X=0)");
                        Debug.WriteLine($"   Point1 (YZ): (X=0, Y={point1.Y:F3}, Z={point1.Z:F3})");
                        Debug.WriteLine($"   Point2 (YZ): (X=0, Y={point2.Y:F3}, Z={point2.Z:F3})");
                        
                        // Vektör: (ΔY, ΔZ)
                        double deltaY = point2.Y - point1.Y;
                        double deltaZ = point2.Z - point1.Z;
                        Debug.WriteLine($"   Vektör: (ΔY={deltaY:F3}, ΔZ={deltaZ:F3})");
                        
                        // Referans: Y Axis
                        Debug.WriteLine($"   Referans: Y Axis");
                        
                        // Açı: Y ekseninden Z'ye doğru
                        angle2D = Math.Atan2(Math.Abs(deltaZ), Math.Abs(deltaY)) * (180.0 / Math.PI);
                    }
                    // ═══════════════════════════════════════════════════════════
                    // EĞİK YÜZEY ÖZEL DURUM 2: Y ≈ 0 && X ≠ 0 && Z ≠ 0 (XZ düzlemi)
                    // ═══════════════════════════════════════════════════════════
                    else if (Math.Abs(normal.Y) < 0.1 && Math.Abs(normal.X) > 0.1 && Math.Abs(normal.Z) > 0.1)
                    {
                        referenceAxisName = "X Axis";
                        
                        // İz düşümü: XZ düzlemine (X, Y=0, Z)
                        Debug.WriteLine($"   Eğik yüzey tipi: XZ düzlemi (Y ≈ 0)");
                        Debug.WriteLine($"   İz Düşümü: XZ düzlemi (Y=0)");
                        Debug.WriteLine($"   Point1 (XZ): (X={point1.X:F3}, Y=0, Z={point1.Z:F3})");
                        Debug.WriteLine($"   Point2 (XZ): (X={point2.X:F3}, Y=0, Z={point2.Z:F3})");
                        
                        // Vektör: (ΔX, ΔZ)
                        double deltaX = point2.X - point1.X;
                        double deltaZ = point2.Z - point1.Z;
                        Debug.WriteLine($"   Vektör: (ΔX={deltaX:F3}, ΔZ={deltaZ:F3})");
                        
                        // Referans: X Axis
                        Debug.WriteLine($"   Referans: X Axis");
                        
                        // Açı: X ekseninden Z'ye doğru
                        angle2D = Math.Atan2(Math.Abs(deltaZ), Math.Abs(deltaX)) * (180.0 / Math.PI);
                    }
                    // ═══════════════════════════════════════════════════════════
                    // EĞİK YÜZEY ÖZEL DURUM 3: Z ≈ 0 && X ≠ 0 && Y ≠ 0 (XY düzlemi)
                    // ═══════════════════════════════════════════════════════════
                    else if (Math.Abs(normal.Z) < 0.1 && Math.Abs(normal.X) > 0.1 && Math.Abs(normal.Y) > 0.1)
                    {
                        referenceAxisName = "Y Axis";
                        
                        // İz düşümü: XY düzlemine (X, Y, Z=0)
                        Debug.WriteLine($"   Eğik yüzey tipi: XY düzlemi (Z ≈ 0)");
                        Debug.WriteLine($"   İz Düşümü: XY düzlemi (Z=0)");
                        Debug.WriteLine($"   Point1 (XY): (X={point1.X:F3}, Y={point1.Y:F3}, Z=0)");
                        Debug.WriteLine($"   Point2 (XY): (X={point2.X:F3}, Y={point2.Y:F3}, Z=0)");
                        
                        // Vektör: (ΔX, ΔY)
                        double deltaX = point2.X - point1.X;
                        double deltaY = point2.Y - point1.Y;
                        Debug.WriteLine($"   Vektör: (ΔX={deltaX:F3}, ΔY={deltaY:F3})");
                        
                        // Referans: Y Axis (dikey)
                        Debug.WriteLine($"   Referans: Y Axis");
                        
                        // Açı: Y ekseninden çizgiye doğru
                        angle2D = Math.Atan2(Math.Abs(deltaX), Math.Abs(deltaY)) * (180.0 / Math.PI);
                    }
                    // ═══════════════════════════════════════════════════════════
                    // EĞİK YÜZEY GENEL DURUM: X ≠ 0 && Y ≠ 0 && Z ≠ 0
                    // Çizgi vektörü ile yüzey normal vektörü arasındaki açı
                    // ═══════════════════════════════════════════════════════════
                    else
                    {
                        planeType = "INCLINED (General)";
                        referenceAxisName = $"Surface Normal ({normal.X:F3}, {normal.Y:F3}, {normal.Z:F3})";
                        
                        Debug.WriteLine($"   Eğik yüzey tipi: Genel (Çizgi ↔ Normal açısı)");
                        
                        // 1. Çizgi vektörü
                        Vector3D lineVector = new Vector3D(
                            point2.X - point1.X,
                            point2.Y - point1.Y,
                            point2.Z - point1.Z
                        );
                        lineVector.Normalize();
                        
                        Debug.WriteLine($"   Çizgi vektörü: ({lineVector.X:F3}, {lineVector.Y:F3}, {lineVector.Z:F3})");
                        Debug.WriteLine($"   Yüzey normal: ({normal.X:F3}, {normal.Y:F3}, {normal.Z:F3})");
                        
                        // 2. Dot product
                        double dotProduct = lineVector.X * normal.X + 
                                          lineVector.Y * normal.Y + 
                                          lineVector.Z * normal.Z;
                        
                        Debug.WriteLine($"   Dot Product: {dotProduct:F3}");
                        
                        // 3. Açı hesapla (çizgi ile normal arasındaki açı)
                        angle2D = Math.Acos(Math.Abs(dotProduct)) * (180.0 / Math.PI);
                        
                        Debug.WriteLine($"   Referans: Surface Normal");
                    }
                }
                
                calculatedAngle = angle2D;
                
                Debug.WriteLine($"   ✅ Açı (Derece): {calculatedAngle:F3}°");
                Debug.WriteLine($"   ✅ Referans: {referenceAxisName}");
                Debug.WriteLine("═══════════════════════════════════════");
                
                // ✅ TreeView'e sonucu ekle
                if (treeViewManager != null && currentGroupNode != null)
                {
                    treeViewManager.AddResultToAngleMeasurementGroup(currentGroupNode, calculatedAngle, referenceAxisName);
                    Debug.WriteLine("✅ Açı sonucu TreeView'e eklendi");
                }
                
                // ✅ MessageBox ile göster
                MessageBox.Show(
                    $"📐 ANGLE MEASUREMENT\n\n" +
                    $"Plane: {planeType}\n" +
                    $"Reference: {referenceAxisName}\n" +
                    $"Angle: {calculatedAngle:F3}°",
                    "Angle Measurement - Result",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ CalculateAngle hatası: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE METHODS - VISUALIZATION
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Tüm planar yüzeyleri sarıya çevir
        /// </summary>
        private void HighlightPlanarSurfaces()
        {
            if (dataManager == null) return;

            var allSurfaces = dataManager.GetSurfaceDataList();

            // TÜM PLANAR yüzeyleri filtrele (BOTTOM Z- HARİÇ)
            var planarSurfaces = allSurfaces
                .Where(s => 
                    s.SurfaceType == "RIGHT (X+)" ||
                    s.SurfaceType == "LEFT (X-)" ||
                    s.SurfaceType == "FRONT (Y+)" ||
                    s.SurfaceType == "BACK (Y-)" ||
                    s.SurfaceType == "TOP (Z+)" ||
                    // s.SurfaceType == "BOTTOM (Z-)" ||  // ❌ ÇIKARILDI - Z- yüzeyleri artık sarıya boyanmayacak
                    s.SurfaceType == "INCLINED"
                )
                .ToList();

            foreach (var surface in planarSurfaces)
            {
                var entity = design.Entities[surface.EntityIndex];

                // Orijinal rengi kaydet
                if (!originalColors.ContainsKey(surface.EntityIndex))
                {
                    originalColors[surface.EntityIndex] = entity.Color;
                }

                // Sarıya çevir
                entity.ColorMethod = colorMethodType.byEntity;
                entity.Color = Color.Yellow;
            }

            design.Invalidate();
            Debug.WriteLine($"🎨 {planarSurfaces.Count} planar yüzey sarıya çevrildi (BOTTOM Z- hariç)");
        }

        /// <summary>
        /// Tüm yüzeyleri orijinal renge döndür
        /// </summary>
        private void RestoreAllSurfaces()
        {
            try
            {
                Debug.WriteLine("═══════════════════════════════════════");
                Debug.WriteLine("🔄 Tüm planar yüzeyler orijinal renge döndürülüyor...");
                
                int restoredCount = 0;
                
                foreach (var kvp in originalColors)
                {
                    int entityIndex = kvp.Key;
                    Color originalColor = kvp.Value;
                    
                    if (entityIndex >= 0 && entityIndex < design.Entities.Count)
                    {
                        Entity entity = design.Entities[entityIndex];
                        entity.Color = originalColor;
                        entity.ColorMethod = colorMethodType.byEntity;
                        restoredCount++;
                    }
                }
                
                originalColors.Clear();
                design.Invalidate();
                
                Debug.WriteLine($"✅ {restoredCount} yüzey orijinal renge döndürüldü");
                Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ RestoreAllSurfaces hatası: {ex.Message}");
                Debug.WriteLine("═══════════════════════════════════════");
            }
        }

        /// <summary>
        /// Layer'ları oluştur (RidgeWidth'den öğrenildi)
        /// </summary>
        private void InitializeLayers()
        {
            try
            {
                // ═══════════════════════════════════════════════════════════
                // LAYER 1: AngleMeasurementMarkers (Marker'lar için)
                // ═══════════════════════════════════════════════════════════
                const string MARKER_LAYER = "AngleMeasurementMarkers";
                bool markerLayerExists = false;
                
                foreach (Layer layer in design.Layers)
                {
                    if (layer.Name == MARKER_LAYER)
                    {
                        markerLayerExists = true;
                        break;
                    }
                }

                if (!markerLayerExists)
                {
                    Layer markerLayer = new Layer(MARKER_LAYER);
                    markerLayer.Color = Color.Red;
                    markerLayer.Visible = true;
                    markerLayer.LineWeight = 1.0f;
                    
                    design.Layers.Add(markerLayer);
                    Debug.WriteLine($"✅ Layer oluşturuldu: {MARKER_LAYER}");
                }
                
                // ═══════════════════════════════════════════════════════════
                // LAYER 2: AngleMeasurementLines (Çizgiler için)
                // ═══════════════════════════════════════════════════════════
                const string LINE_LAYER = "AngleMeasurementLines";
                bool lineLayerExists = false;
                
                foreach (Layer layer in design.Layers)
                {
                    if (layer.Name == LINE_LAYER)
                    {
                        lineLayerExists = true;
                        break;
                    }
                }

                if (!lineLayerExists)
                {
                    Layer lineLayer = new Layer(LINE_LAYER);
                    lineLayer.Color = Color.Green;
                    lineLayer.Visible = true;
                    lineLayer.LineWeight = 2.0f;
                    
                    design.Layers.Add(lineLayer);
                    Debug.WriteLine($"✅ Layer oluşturuldu: {LINE_LAYER}");
                }
                
                // ═══════════════════════════════════════════════════════════
                // LAYER 3: AngleMeasurementProbe (Probe mesh'leri için)
                // ═══════════════════════════════════════════════════════════
                bool probeLayerExists = false;
                
                foreach (Layer layer in design.Layers)
                {
                    if (layer.Name == PROBE_LAYER_NAME)
                    {
                        probeLayerExists = true;
                        break;
                    }
                }

                if (!probeLayerExists)
                {
                    Layer probeLayer = new Layer(PROBE_LAYER_NAME);
                    probeLayer.Color = Color.White;
                    probeLayer.Visible = true;
                    probeLayer.LineWeight = 1.0f;
                    
                    design.Layers.Add(probeLayer);
                    Debug.WriteLine($"✅ Layer oluşturuldu: {PROBE_LAYER_NAME}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Layer oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Seçilen yüzey hariç diğer tüm yüzeyleri orijinal renge döndür
        /// </summary>
        private void RestoreNonSelectedSurface(int selectedEntityIndex)
        {
            try
            {
                Debug.WriteLine("═══════════════════════════════════════");
                Debug.WriteLine("🔄 Seçilen yüzey hariç diğerleri orijinal renge döndürülüyor...");
                
                int restoredCount = 0;
                
                foreach (var kvp in originalColors)
                {
                    int entityIndex = kvp.Key;
                    
                    // Seçilen yüzeyi atla - o sarı kalacak
                    if (entityIndex == selectedEntityIndex)
                    {
                        continue;
                    }
                    
                    Color originalColor = kvp.Value;
                    
                    if (entityIndex >= 0 && entityIndex < design.Entities.Count)
                    {
                        Entity entity = design.Entities[entityIndex];
                        entity.Color = originalColor;
                        entity.ColorMethod = colorMethodType.byEntity;
                        restoredCount++;
                    }
                }
                
                design.Invalidate();
                
                Debug.WriteLine($"✅ {restoredCount} yüzey orijinal renge döndürüldü (Seçilen hariç)");
                Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ RestoreNonSelectedSurface hatası: {ex.Message}");
                Debug.WriteLine("═══════════════════════════════════════");
            }
        }

        /// <summary>
        /// Marker ekle (kırmızı veya mavi KÜRE - Mesh.CreateSphere kullan!)
        /// Marker, yüzey normalinde probe çapı/2 kadar kaydırılır (SADECE GÖRSEL)
        /// </summary>
        /// <param name="position">Marker pozisyonu</param>
        /// <param name="markerColor">Marker rengi</param>
        /// <param name="markerName">Unique marker ismi (örn: "AngleMarker_1_Point1")</param>
        /// <param name="surfaceNormal">Yüzey normal vektörü</param>
        /// <param name="probeDiameter">Probe çapı</param>
        /// <returns>Oluşturulan Mesh marker</returns>
        private Mesh? AddMarker(Point3D position, Color markerColor, string markerName, Vector3D? surfaceNormal = null, double probeDiameter = 0)
        {
            try
            {
                // ═══════════════════════════════════════════════════════════
                // MARKER POZİSYONU HESAPLA
                // ═══════════════════════════════════════════════════════════
                Point3D markerPosition = position;  // Orijinal pozisyon (hesaplamalar için)
                Point3D visualPosition = position;  // Görsel pozisyon (ekranda gösterim için)
                
                // Eğer normal ve probe diameter verilmişse, marker'ı kaydır (SADECE GÖRSEL)
                if (surfaceNormal != null && probeDiameter > 0)
                {
                    double offset = probeDiameter / 2.0;
                    visualPosition = new Point3D(
                        position.X + surfaceNormal.X * offset,
                        position.Y + surfaceNormal.Y * offset,
                        position.Z + surfaceNormal.Z * offset
                    );
                    
                    Debug.WriteLine($"   Marker kaydırma: Offset={offset:F3}mm, Normal=({surfaceNormal.X:F3}, {surfaceNormal.Y:F3}, {surfaceNormal.Z:F3})");
                    Debug.WriteLine($"   Orijinal nokta: ({position.X:F3}, {position.Y:F3}, {position.Z:F3})");
                    Debug.WriteLine($"   Görsel pozisyon: ({visualPosition.X:F3}, {visualPosition.Y:F3}, {visualPosition.Z:F3})");
                }
                
                // ═══════════════════════════════════════════════════════════
                // MARKER OLUŞTUR (Kaydırılmış pozisyonda)
                // ═══════════════════════════════════════════════════════════
                // Marker çapı = probe diameter (kullanıcının seçtiği probe çapı)
                double markerDiameter = probeDiameter > 0 ? probeDiameter : 6.0;  // Default 6mm
                double radius = markerDiameter / 2.0;
                
                Mesh sphere = Mesh.CreateSphere(radius, 16, 16);  // Point Probing'deki gibi 16x16
                
                // Kaydırılmış pozisyona taşı (SADECE GÖRSEL)
                sphere.Translate(visualPosition.X, visualPosition.Y, visualPosition.Z);
                
                // Renk ayarla
                sphere.Color = markerColor;
                sphere.ColorMethod = colorMethodType.byEntity;
                sphere.LayerName = "AngleMeasurementMarkers";
                
                // Design'a ekle
                design.Entities.Add(sphere);
                
                design.Invalidate();
                
                Debug.WriteLine($"✅ Marker (KÜRE) eklendi: İsim=[{markerName}], Ø{markerDiameter:F3}mm, Görsel=({visualPosition.X:F3}, {visualPosition.Y:F3}, {visualPosition.Z:F3}) - Renk: {markerColor.Name}");
                
                // ✅ OLUŞTURULAN MARKER'I DÖNDÜR
                return sphere;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ AddMarker hatası: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// İki nokta arasında çizgi çiz
        /// </summary>
        private void DrawLineBetweenPoints(Point3D p1, Point3D p2)
        {
            try
            {
                // Çizgi oluştur
                Line line = new Line(p1, p2)
                {
                    Color = Color.Green,
                    ColorMethod = colorMethodType.byEntity,
                    LineWeight = 2.0f
                };
                
                // Design'a ekle
                design.Entities.Add(line, "AngleMeasurementLines");
                measurementLine = line;
                
                design.Invalidate();
                
                double lineLength = p1.DistanceTo(p2);
                Debug.WriteLine($"✅ Çizgi çizildi: {p1} → {p2} (Uzunluk: {lineLength:F3} mm)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ DrawLineBetweenPoints hatası: {ex.Message}");
            }
        }
    }
}
