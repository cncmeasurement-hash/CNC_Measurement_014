using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _014.Managers.Selection;
using _014.Managers.Data;  // ✅ YENİ: MeasurementDataManager için
using _014.Probe.Core;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace _014.Managers.Toolpath
{
    /// <summary>
    /// Toolpath (Takım Yolu) yöneticisi
    /// Probe noktalarından G-code takım yolu oluşturur
    /// </summary>
    public class ToolpathManager
    {
        private TreeNode toolpathNode;
        private SelectionManager selectionManager;
        private TreeViewManager treeViewManager;
        private Design design;
        
        // Animasyon için değişkenler
        private System.Windows.Forms.Timer animationTimer;
        private List<Point3D> animationPath;
        private List<Color> animationColors;
        private List<int> animationSpeeds; // ms cinsinden bekleme süreleri
        private int currentPathIndex;
        private Mesh simulationProbe;
        private bool isAnimating;
        private bool isSimulating;  // 🆕 YENİ: Simülasyon aktif mi? (Toolpath generation engellemek için)
        private double simulationSpeed = 1.0;  // 🆕 YENİ: Simülasyon hızı (default 1.0x)
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ToolpathManager(TreeNode toolpathNode, SelectionManager selectionManager, TreeViewManager treeViewMgr = null)
        {
            this.toolpathNode = toolpathNode;
            this.selectionManager = selectionManager;
            treeViewManager = treeViewMgr;
            design = selectionManager?.GetDesign();
            
            // Animasyon timer'ını başlat
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Tick += AnimationTimer_Tick;
            
            System.Diagnostics.Debug.WriteLine("✅ ToolpathManager oluşturuldu");
        }
        
        /// <summary>
        /// Toolpath oluştur - MeasurementDataManager'dan veri alır
        /// ✅ YENİ: MeasurementDataManager entegrasyonu
        /// </summary>
        public void GenerateToolpath()
        {
            // ═══════════════════════════════════════════════════════════
            // KONTROL: Simülasyon çalışıyor mu?
            // ═══════════════════════════════════════════════════════════
            if (isSimulating)
            {
                MessageBox.Show(
                    "⚠️ SİMÜLASYON ÇALIŞIRKEN TOOLPATH OLUŞTURAMAZSINIZ!\n\n" +
                    "Önce simülasyonu durdurun:\n" +
                    "• [Stop Simulation] butonuna basın\n" +
                    "• Simülasyon bitene kadar bekleyin\n\n" +
                    "Sonra tekrar toolpath oluşturun.",
                    "Simülasyon Aktif",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                System.Diagnostics.Debug.WriteLine("⚠️ Toolpath generation engellendi - Simülasyon aktif!");
                return;
            }
            
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine("📊 TOOLPATH GENERATION BAŞLADI (MeasurementDataManager)");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            
            try
            {
                if (design == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Design null!");
                    return;
                }
                
                // 1. Toolpath layer'ını oluştur/kontrol et
                if (!design.Layers.Contains("Toolpath"))
                {
                    var toolpathLayer = new Layer("Toolpath");
                    toolpathLayer.Color = Color.Green;
                    toolpathLayer.LineWeight = 0.6f;
                    design.Layers.Add(toolpathLayer);
                    System.Diagnostics.Debug.WriteLine("✅ Toolpath layer oluşturuldu");
                }
                
                // 2. Eski toolpath çizgilerini temizle
                ClearOldToolpath();
                
                // ✅ 3. MeasurementDataManager'dan TÜM grupları al
                var dataManager = MeasurementDataManager.Instance;
                var allGroups = dataManager.GetAllGroups();
                
                if (allGroups == null || allGroups.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Hiç measurement grubu yok!");
                    toolpathNode.Text = "Toolpath      : No Groups";
                    toolpathNode.ForeColor = Color.Red;
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"📊 Toplam {allGroups.Count} grup bulundu");
                
                // 4. HER GRUP İÇİN MARKER/APPROACH/SAFE POİNT HESAPLA
                List<Point3D> allMarkers = new List<Point3D>();
                List<Point3D> allApproaches = new List<Point3D>();
                List<Point3D> allSafePoints = new List<Point3D>();
                
                foreach (var group in allGroups.OrderBy(g => g.GroupId))
                {
                    System.Diagnostics.Debug.WriteLine($"  📁 Grup işleniyor: {group.GroupName} ({group.MeasurementMode})");
                    
                    var activePoints = group.Points.Where(p => p.IsActive).ToList();
                    
                    if (activePoints.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ⚠️ Grup boş: {group.GroupName}");
                        continue;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"  📍 {activePoints.Count} nokta bulundu");
                    
                    foreach (var point in activePoints.OrderBy(p => p.PointIndex))
                    {
                        Point3D markerPosition = point.MarkerPosition;
                        Vector3D normal = point.SurfaceNormal;
                        double retract = point.RetractDistance;
                        double zSafety = point.ZSafety;
                        
                        Point3D approachPoint = new Point3D(
                            markerPosition.X + normal.X * retract,
                            markerPosition.Y + normal.Y * retract,
                            markerPosition.Z + normal.Z * retract
                        );
                        
                        Point3D safePoint = new Point3D(
                            approachPoint.X,
                            approachPoint.Y,
                            zSafety
                        );
                        
                        allMarkers.Add(markerPosition);
                        allApproaches.Add(approachPoint);
                        allSafePoints.Add(safePoint);
                        
                        System.Diagnostics.Debug.WriteLine($"    🔵 Point {point.PointIndex}:");
                        System.Diagnostics.Debug.WriteLine($"       Marker: ({markerPosition.X:F2},{markerPosition.Y:F2},{markerPosition.Z:F2})");
                        System.Diagnostics.Debug.WriteLine($"       Approach: ({approachPoint.X:F2},{approachPoint.Y:F2},{approachPoint.Z:F2})");
                        System.Diagnostics.Debug.WriteLine($"       Safe: ({safePoint.X:F2},{safePoint.Y:F2},{safePoint.Z:F2})");
                    }
                }
                
                if (allMarkers.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Hiç aktif nokta yok!");
                    toolpathNode.Text = "Toolpath      : No Points";
                    toolpathNode.ForeColor = Color.Red;
                    return;
                }
                
                // 5. TOOLPATH ÇİZGİLERİNİ OLUŞTUR
                System.Diagnostics.Debug.WriteLine("  ═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("  🎨 TOOLPATH ÇİZGİLERİ OLUŞTURULUYOR...");
                System.Diagnostics.Debug.WriteLine("  ═══════════════════════════════════════");
                
                int lineCount = 0;
                double retractDistance = treeViewManager != null ? treeViewManager.RetractDistance : 3.0;
                
                for (int i = 0; i < allMarkers.Count; i++)
                {
                    Point3D marker = allMarkers[i];
                    Point3D approach = allApproaches[i];
                    Point3D safe = allSafePoints[i];
                    
                    // ÇİZGİ 1: Marker → Approach (Retract - BEYAZ)
                    Line retractLine = new Line(marker, approach);
                    retractLine.Color = Color.White;
                    retractLine.ColorMethod = colorMethodType.byEntity;
                    retractLine.LineWeight = 0.6f;
                    retractLine.LayerName = "Toolpath";
                    retractLine.EntityData = "Toolpath_Rapid";
                    design.Entities.Add(retractLine);
                    lineCount++;
                    System.Diagnostics.Debug.WriteLine($"  ✅ Line {lineCount}: Marker{i+1} → Approach{i+1}");
                    
                    // ÇİZGİ 2: Approach → Safe (Z+ - BEYAZ)
                    Line goUpLine = new Line(approach, safe);
                    goUpLine.Color = Color.White;
                    goUpLine.ColorMethod = colorMethodType.byEntity;
                    goUpLine.LineWeight = 0.6f;
                    goUpLine.LayerName = "Toolpath";
                    goUpLine.EntityData = "Toolpath_Rapid";
                    design.Entities.Add(goUpLine);
                    lineCount++;
                    System.Diagnostics.Debug.WriteLine($"  ✅ Line {lineCount}: Approach{i+1} → SafePoint{i+1}");
                    
                    // Sonraki noktaya geçiş
                    if (i < allMarkers.Count - 1)
                    {
                        Point3D nextSafe = allSafePoints[i + 1];
                        Point3D nextApproach = allApproaches[i + 1];
                        Point3D nextMarker = allMarkers[i + 1];
                        
                        // ÇİZGİ 3: Safe → Next Safe (BEYAZ)
                        Line rapidLine = new Line(safe, nextSafe);
                        rapidLine.Color = Color.White;
                        rapidLine.ColorMethod = colorMethodType.byEntity;
                        rapidLine.LineWeight = 0.6f;
                        rapidLine.LayerName = "Toolpath";
                        rapidLine.EntityData = "Toolpath_Rapid";
                        design.Entities.Add(rapidLine);
                        lineCount++;
                        System.Diagnostics.Debug.WriteLine($"  ✅ Line {lineCount}: SafePoint{i+1} → SafePoint{i+2}");
                        
                        // ÇİZGİ 4: Next Safe → Intermediate (BEYAZ)
                        Point3D intermediate = new Point3D(
                            nextApproach.X,
                            nextApproach.Y,
                            nextApproach.Z + retractDistance
                        );
                        Line fastDownLine = new Line(nextSafe, intermediate);
                        fastDownLine.Color = Color.White;
                        fastDownLine.ColorMethod = colorMethodType.byEntity;
                        fastDownLine.LineWeight = 0.6f;
                        fastDownLine.LayerName = "Toolpath";
                        fastDownLine.EntityData = "Toolpath_Rapid";
                        design.Entities.Add(fastDownLine);
                        lineCount++;
                        System.Diagnostics.Debug.WriteLine($"  ✅ Line {lineCount}: SafePoint{i+2} → Intermediate");
                        
                        // ÇİZGİ 5: Intermediate → Approach (SARI)
                        Line slowDownLine = new Line(intermediate, nextApproach);
                        slowDownLine.Color = Color.Yellow;
                        slowDownLine.ColorMethod = colorMethodType.byEntity;
                        slowDownLine.LineWeight = 0.6f;
                        slowDownLine.LayerName = "Toolpath";
                        slowDownLine.EntityData = "Toolpath_Feed";
                        design.Entities.Add(slowDownLine);
                        lineCount++;
                        System.Diagnostics.Debug.WriteLine($"  ✅ Line {lineCount}: Intermediate → Approach{i+2}");
                        
                        // ÇİZGİ 6: Approach → Target (KIRMIZI)
                        Vector3D direction = new Vector3D(
                            nextApproach.X - nextMarker.X,
                            nextApproach.Y - nextMarker.Y,
                            nextApproach.Z - nextMarker.Z
                        );
                        direction.Normalize();
                        Point3D target = new Point3D(
                            nextMarker.X + direction.X * 0.8,
                            nextMarker.Y + direction.Y * 0.8,
                            nextMarker.Z + direction.Z * 0.8
                        );
                        Line probeLine = new Line(nextApproach, target);
                        probeLine.Color = Color.Red;
                        probeLine.ColorMethod = colorMethodType.byEntity;
                        probeLine.LineWeight = 0.6f;
                        probeLine.LayerName = "Toolpath";
                        probeLine.EntityData = "Toolpath_Probe";
                        design.Entities.Add(probeLine);
                        lineCount++;
                        System.Diagnostics.Debug.WriteLine($"  ✅ Line {lineCount}: Approach{i+2} → Target");
                    }
                }
                
                design.Entities.Regen();
                design.Invalidate();
                
                System.Diagnostics.Debug.WriteLine("  ═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine($"✅ Toolpath oluşturuldu! {lineCount} çizgi eklendi");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                
                toolpathNode.Text = $"Toolpath      : {lineCount} Lines ✓";
                toolpathNode.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Toolpath oluşturma hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
                toolpathNode.Text = "Toolpath      : Error!";
                toolpathNode.ForeColor = Color.Red;
            }
        }
        
        /// <summary>
        /// Eski toolpath çizgilerini temizle
        /// </summary>
        private void ClearOldToolpath()
        {
            if (design == null) return;
            
            List<Entity> toRemove = new List<Entity>();
            foreach (Entity entity in design.Entities)
            {
                if (entity.LayerName == "Toolpath")
                {
                    toRemove.Add(entity);
                }
            }
            
            foreach (Entity entity in toRemove)
            {
                design.Entities.Remove(entity);
            }
            
            if (toRemove.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"🗑️ {toRemove.Count} eski toolpath çizgisi silindi");
            }
        }
        
        /// <summary>
        /// Toolpath'i temizle (Retract değiştiğinde çağrılır)
        /// </summary>
        public void ClearToolpath()
        {
            ClearOldToolpath();
            design?.Invalidate();
            System.Diagnostics.Debug.WriteLine("🗑️ Toolpath temizlendi (Retract değişti)");
        }
        
        /// <summary>
        /// Toolpath simülasyonunu başlat
        /// </summary>
        public void StartSimulation()
        {
            System.Diagnostics.Debug.WriteLine("🔍 StartSimulation() ÇAĞRILDI!");  // ✅ DEBUG
            
            if (isAnimating)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Simülasyon zaten çalışıyor!");
                MessageBox.Show(
                    "Simülasyon zaten çalışıyor!",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            // Önce toolpath oluşturulmuş mu kontrol et
            if (design != null)
            {
                bool hasToolpath = false;
                foreach (Entity entity in design.Entities)
                {
                    if (entity.LayerName == "Toolpath")
                    {
                        hasToolpath = true;
                        break;
                    }
                }
                
                if (!hasToolpath)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Toolpath bulunamadı!");
                    MessageBox.Show(
                        "Önce 'Generate Toolpath' butonuna tıklayarak toolpath oluşturun!",
                        "Toolpath Bulunamadı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }
            
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine("🎬 TOOLPATH SİMÜLASYONU BAŞLADI");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            
            try
            {
                // Animasyon yolunu oluştur
                BuildAnimationPath();
                
                if (animationPath == null || animationPath.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Animasyon yolu oluşturulamadı!");
                    MessageBox.Show(
                        "Animasyon yolu oluşturulamadı! Lütfen toolpath'i kontrol edin.",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                
                // Probe mesh'ini oluştur
                CreateSimulationProbe();
                
                // Animasyonu başlat
                currentPathIndex = 0;
                isAnimating = true;
                isSimulating = true;  // 🆕 YENİ: Simülasyon flag'ini aktif et (Toolpath generation engellenecek)
                
                // Hıza göre interval ayarla (base: 50ms)
                int baseInterval = 50;
                animationTimer.Interval = (int)(baseInterval / simulationSpeed);
                
                animationTimer.Start();
                
                System.Diagnostics.Debug.WriteLine($"✅ Simülasyon başlatıldı: {animationPath.Count} adım, Hız: {simulationSpeed}x, Interval: {animationTimer.Interval}ms");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Simülasyon başlatma hatası: {ex.Message}");
                MessageBox.Show(
                    $"Simülasyon başlatma hatası:\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Toolpath simülasyonunu durdur
        /// </summary>
        public void StopSimulation()
        {
            if (!isAnimating)
            {
                return;
            }
            
            animationTimer.Stop();
            isAnimating = false;
            isSimulating = false;  // 🆕 YENİ: Simülasyon flag'ini pasif et (Toolpath generation tekrar izin verilecek)
            
            // Simülasyon probe'unu temizle
            if (simulationProbe != null && design.Entities.Contains(simulationProbe))
            {
                design.Entities.Remove(simulationProbe);
                design.Invalidate();
            }
            
            System.Diagnostics.Debug.WriteLine("⏹️ Simülasyon durduruldu");
        }
        
        /// <summary>
        /// Simülasyon çalışıyor mu? (Play/Pause kontrolü için)
        /// </summary>
        public bool IsSimulationRunning
        {
            get { return animationTimer != null && animationTimer.Enabled; }
        }
        
        /// <summary>
        /// Simülasyonu duraklat (Pause)
        /// </summary>
        public void PauseSimulation()
        {
            if (animationTimer != null && animationTimer.Enabled)
            {
                animationTimer.Stop();
                System.Diagnostics.Debug.WriteLine("⏸️ Simülasyon duraklatıldı");
            }
        }
        
        /// <summary>
        /// Simülasyonu devam ettir (Resume)
        /// </summary>
        public void ResumeSimulation()
        {
            if (animationTimer != null && !animationTimer.Enabled && isAnimating)
            {
                animationTimer.Start();
                System.Diagnostics.Debug.WriteLine("▶️ Simülasyon devam ediyor");
            }
        }
        
        /// <summary>
        /// Simülasyon hızını ayarla
        /// </summary>
        public void SetSimulationSpeed(double speed)
        {
            simulationSpeed = speed;
            
            // Eğer simülasyon çalışıyorsa, interval'ı hemen güncelle
            if (isAnimating)
            {
                int baseInterval = 50;
                animationTimer.Interval = (int)(baseInterval / simulationSpeed);
                System.Diagnostics.Debug.WriteLine($"⚡ Simülasyon hızı güncellendi: {simulationSpeed}x, Yeni interval: {animationTimer.Interval}ms");
            }
        }
        
        /// <summary>
        /// Animasyon yolunu oluştur (tüm noktalar)
        /// </summary>
        private void BuildAnimationPath()
        {
            animationPath = new List<Point3D>();
            animationColors = new List<Color>();
            animationSpeeds = new List<int>();
            
            // SelectionManager'dan marker pozisyonlarını ve normalleri al
            var markerPositions = selectionManager?.GetAllProbePoints();
            var normals = selectionManager?.GetAllNormals();
            
            if (markerPositions == null || markerPositions.Count == 0)
            {
                return;
            }
            
            double retractDistance = treeViewManager?.RetractDistance ?? 3.0;
            double zSafetyDistance = treeViewManager?.ZSafetyDistance ?? 50.0;
            
            for (int i = 0; i < markerPositions.Count; i++)
            {
                Point3D marker = markerPositions[i];
                Vector3D normal = normals[i];
                
                // Approach point
                Point3D approach = new Point3D(
                    marker.X + normal.X * retractDistance,
                    marker.Y + normal.Y * retractDistance,
                    marker.Z + normal.Z * retractDistance
                );
                
                // Safe point
                Point3D safePoint = new Point3D(approach.X, approach.Y, zSafetyDistance);
                
                // 1. Marker → Approach (BEYAZ - hızlı)
                AddAnimationSegment(marker, approach, Color.White, 30);
                
                // 2. Approach → SafePoint (BEYAZ - hızlı)
                AddAnimationSegment(approach, safePoint, Color.White, 30);
                
                // Bir sonraki noktaya geçiş
                if (i < markerPositions.Count - 1)
                {
                    Point3D nextMarker = markerPositions[i + 1];
                    Vector3D nextNormal = normals[i + 1];
                    
                    Point3D nextApproach = new Point3D(
                        nextMarker.X + nextNormal.X * retractDistance,
                        nextMarker.Y + nextNormal.Y * retractDistance,
                        nextMarker.Z + nextNormal.Z * retractDistance
                    );
                    
                    Point3D nextSafePoint = new Point3D(nextApproach.X, nextApproach.Y, zSafetyDistance);
                    Point3D intermediate = new Point3D(nextApproach.X, nextApproach.Y, nextApproach.Z + retractDistance);
                    
                    // 3. SafePoint → NextSafePoint (BEYAZ - hızlı)
                    AddAnimationSegment(safePoint, nextSafePoint, Color.White, 30);
                    
                    // 4. NextSafePoint → Intermediate (BEYAZ - hızlı)
                    AddAnimationSegment(nextSafePoint, intermediate, Color.White, 30);
                    
                    // 5. Intermediate → NextApproach (YEŞİL - yavaş)
                    AddAnimationSegment(intermediate, nextApproach, Color.Green, 100);
                    
                    // 6. NextApproach → Target (CYAN - yavaş)
                    Point3D targetPoint = new Point3D(
                        nextMarker.X - nextNormal.X * 0.8,
                        nextMarker.Y - nextNormal.Y * 0.8,
                        nextMarker.Z - nextNormal.Z * 0.8
                    );
                    AddAnimationSegment(nextApproach, targetPoint, Color.Cyan, 150);
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"📊 Animasyon yolu oluşturuldu: {animationPath.Count} nokta");
        }
        
        /// <summary>
        /// İki nokta arasına animasyon segmenti ekle
        /// </summary>
        private void AddAnimationSegment(Point3D start, Point3D end, Color color, int speedMs)
        {
            int steps = 20; // Her segment 20 adıma bölünecek
            
            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                Point3D point = new Point3D(
                    start.X + (end.X - start.X) * t,
                    start.Y + (end.Y - start.Y) * t,
                    start.Z + (end.Z - start.Z) * t
                );
                
                animationPath.Add(point);
                animationColors.Add(color);
                animationSpeeds.Add(speedMs);
            }
        }
        
        /// <summary>
        /// Simülasyon için probe mesh'i oluştur
        /// </summary>
        private void CreateSimulationProbe()
        {
            try
            {
                // Probe verilerini al
                ProbeData probeData = treeViewManager?.GetSelectedProbeData();
                if (probeData == null)
                {
                    probeData = new ProbeData { Name = "Default", D = 6, d1 = 4, d2 = 40, L1 = 40, L2 = 40, L3 = 4 };
                }
                
                // Probe mesh'ini oluştur
                simulationProbe = ProbeBuilder.CreateProbeMesh(probeData);
                
                if (simulationProbe != null)
                {
                    simulationProbe.Color = Color.Orange; // Simülasyon probe'u turuncu
                    simulationProbe.ColorMethod = colorMethodType.byEntity;
                    simulationProbe.LayerName = "Toolpath"; // Toolpath layer'ına ekle
                    
                    // 🆕 YENİ: Probe'u İLK POZISYONA taşı
                    if (animationPath != null && animationPath.Count > 0)
                    {
                        Point3D firstPos = animationPath[0];
                        
                        // 🎯 KRITIK DÜZELTME: Probe KÜRE MERKEZİ toolpath üzerinde olsun
                        // Probe mesh küre merkezi (0,0,0)'da oluşuyor
                        // Marker pozisyonları probe KÜRE MERKEZİ için hesaplanmış
                        // Bu yüzden probe çapının yarısı kadar (D/2) AŞAĞIYA kaydırmalıyız
                        double probeRadius = (double)probeData.D / 2.0;  // 3mm (D=6mm için)
                        
                        simulationProbe.Translate(
                            firstPos.X, 
                            firstPos.Y, 
                            firstPos.Z - probeRadius  // 🎯 Z'den probe yarıçapı kadar çıkar
                        );
                        
                        System.Diagnostics.Debug.WriteLine($"🚀 Probe başlangıç pozisyonu: ({firstPos.X:F2}, {firstPos.Y:F2}, {firstPos.Z:F2})");
                        System.Diagnostics.Debug.WriteLine($"   ⚙️ Probe radius offset: -{probeRadius:F2}mm (Küre merkezi toolpath'te)");
                    }
                    
                    design.Entities.Add(simulationProbe);
                    design.Invalidate();
                    System.Diagnostics.Debug.WriteLine("✅ Simülasyon probe'u oluşturuldu ve konumlandırıldı");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Simülasyon probe oluşturma hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Animasyon timer tick eventi
        /// </summary>
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!isAnimating || currentPathIndex >= animationPath.Count)
            {
                StopSimulation();
                System.Diagnostics.Debug.WriteLine("✅ Simülasyon tamamlandı!");
                return;
            }
            
            try
            {
                // Mevcut pozisyonu al
                Point3D currentPos = animationPath[currentPathIndex];
                Color currentColor = animationColors[currentPathIndex];
                int currentSpeed = animationSpeeds[currentPathIndex];
                
                // Probe'u bu pozisyona taşı
                if (simulationProbe != null)
                {
                    // 🆕 KRITIK: Mesh'i silip yeniden ekleyerek ekran güncellemesini zorla
                    if (design.Entities.Contains(simulationProbe))
                    {
                        design.Entities.Remove(simulationProbe);
                    }
                    
                    // İLK POZISYON: Probe zaten başlangıç pozisyonunda
                    if (currentPathIndex == 0)
                    {
                        // İlk pozisyon - sadece renk güncelle
                        simulationProbe.Color = currentColor;
                    }
                    else
                    {
                        // Sonraki pozisyonlar - eski pozisyondan farkı hesapla ve taşı
                        Point3D prevPos = animationPath[currentPathIndex - 1];
                        double dx = currentPos.X - prevPos.X;
                        double dy = currentPos.Y - prevPos.Y;
                        double dz = currentPos.Z - prevPos.Z;
                        
                        // 🔧 DEBUG: Her 10 adımda bir pozisyon logla
                        if (currentPathIndex % 10 == 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"🚀 Probe hareket: Index={currentPathIndex}, Pos=({currentPos.X:F2}, {currentPos.Y:F2}, {currentPos.Z:F2})");
                        }
                        
                        simulationProbe.Translate(dx, dy, dz);
                        simulationProbe.Color = currentColor;
                    }
                    
                    // 🆕 KRITIK: Mesh'i tekrar ekle
                    design.Entities.Add(simulationProbe);
                    
                    // Ekranı güncelle
                    design.Invalidate();
                }
                
                // Timer aralığını güncelle (sadece hız değiştiğinde)
                int newInterval = (int)(currentSpeed / simulationSpeed);
                if (animationTimer.Interval != newInterval)
                {
                    animationTimer.Interval = newInterval;
                }
                
                // Sonraki adıma geç
                currentPathIndex++;
                
                // İlerleme göster (her 50 adımda bir)
                if (currentPathIndex % 50 == 0)
                {
                    int progress = currentPathIndex * 100 / animationPath.Count;
                    System.Diagnostics.Debug.WriteLine($"🎬 Simülasyon: {progress}%");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Animasyon hatası: {ex.Message}");
                StopSimulation();
            }
        }
        
    }
}
