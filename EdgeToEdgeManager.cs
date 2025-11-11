using _014.Handlers.EdgeToEdge;
using _014.Managers.Data;
using _014.Utilities.UI;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;  // ✅ Design için
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Point = System.Drawing.Point;  // ✅ Point belirsizliğini çöz

namespace _014
{
    /// <summary>
    /// Edge to Edge measurement manager
    /// İki edge (kenar) arasındaki mesafeyi, açıyı ve bilgileri hesaplar
    /// </summary>
    public class EdgeToEdgeManager
    {
        // ═══════════════════════════════════════════════════════════
        // FIELDS
        // ═══════════════════════════════════════════════════════════

        private readonly Design design;
        private readonly Form parentForm;
        private readonly DataManager? dataManager;

        private const string MEASUREMENT_LAYER_NAME = "MeasurementLines";

        // Seçim durumu
        private int selectionCount = 0;
        private bool isActive = false;

        // Seçilen edge'ler (ICurve olarak - hem Surface hem Brep için)
        private ICurve? edge1 = null;
        private ICurve? edge2 = null;
        private Entity? entity1 = null;  // Surface veya Brep
        private Entity? entity2 = null;  // Surface veya Brep

        // Orijinal renkler (highlight için)
        private Color originalColor1;
        private Color originalColor2;

        // Highlight çizgileri (sadece edge'i vurgulamak için)
        private Line? highlightLine1 = null;
        private Line? highlightLine2 = null;

        // ⭐ HOVER PREVIEW (Mouse hareket ederken)
        private Entity? hoveredEntity = null;     // Hover edilen entity
        private ICurve? hoveredEdge = null;       // Hover edilen edge
        private Color originalHoverColor;         // Hover entity'nin orijinal rengi

        // Ölçüm sonuçları
        private double distance = 0;
        private double angle = 0;
        private double edge1Length = 0;
        private double edge2Length = 0;
        private Point3D closestPoint1 = Point3D.Origin;
        private Point3D closestPoint2 = Point3D.Origin;

        // Görselleştirme
        private Line? distanceLine = null;

        // Paneller
        private InstructionPanel? instructionPanel;
        private EdgeToEdgeInfoPanel? infoPanel;

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        public EdgeToEdgeManager(Design designControl, Form parentForm, DataManager? dataManager = null)
        {
            design = designControl ?? throw new ArgumentNullException(nameof(designControl));
            this.parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
            this.dataManager = dataManager;
        }

        // ═══════════════════════════════════════════════════════════
        // PUBLIC PROPERTIES
        // ═══════════════════════════════════════════════════════════

        public bool IsActive => isActive;

        // ═══════════════════════════════════════════════════════════
        // PUBLIC METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Edge to Edge modunu aktif et
        /// </summary>
        public void Enable(InstructionPanel? instructionPanel)
        {
            if (isActive) return;

            isActive = true;
            this.instructionPanel = instructionPanel;

            // Seçimi sıfırla
            ResetSelection();

            // Mouse event ekle
            design.MouseMove += OnMouseMove;   // ⭐ HOVER PREVIEW İÇİN!
            design.MouseClick += OnMouseClick;

            // Cursor değiştir
            design.Cursor = Cursors.Hand;

            // InstructionPanel güncelle
            if (instructionPanel != null && !instructionPanel.IsDisposed)
            {
                instructionPanel.UpdatePanel(
                    InstructionTexts.TITLE_EDGE_TO_EDGE,
                    InstructionTexts.EDGE_TO_EDGE
                );
                instructionPanel.Show();
                instructionPanel.BringToFront();
            }

            // EdgeToEdgeInfoPanel oluştur
            if (infoPanel == null || infoPanel.IsDisposed)
            {
                infoPanel = new EdgeToEdgeInfoPanel(parentForm);
            }
            infoPanel.ShowWaitingMessage();
            infoPanel.Show();

            Debug.WriteLine("✅ Edge to Edge AKTIF - İlk edge seçimi bekleniyor!");
        }

        /// <summary>
        /// Edge to Edge modunu pasif et
        /// </summary>
        public void Disable()
        {
            if (!isActive) return;

            isActive = false;

            // Mouse event kaldır
            design.MouseMove -= OnMouseMove;   // ⭐ HOVER PREVIEW TEMİZLE!
            design.MouseClick -= OnMouseClick;

            // Cursor normale döndür
            design.Cursor = Cursors.Default;

            // Seçimi temizle
            ResetSelection();

            // Genel seçimleri temizle
            design.Entities.ClearSelection();
            design.Invalidate();

            // Panel'i welcome mesajına döndür
            if (instructionPanel != null && !instructionPanel.IsDisposed)
            {
                instructionPanel.UpdatePanel(
                    InstructionTexts.TITLE_MAIN_MENU,
                    InstructionTexts.WELCOME
                );
            }

            // EdgeToEdgeInfoPanel'i kapat
            if (infoPanel != null && !infoPanel.IsDisposed)
            {
                infoPanel.ShowWaitingMessage();
                infoPanel.Hide();
                Debug.WriteLine("✅ Edge to Edge Info Panel temizlendi ve gizlendi");
            }

            Debug.WriteLine("❌ Edge to Edge PASİF!");
        }

        /// <summary>
        /// Seçimi sıfırla
        /// </summary>
        public void ResetSelection()
        {
            try
            {
                // Hover preview temizle
                ClearHoverPreview();

                // ✅ Entity 1 rengini geri çevir
                if (entity1 != null)
                {
                    entity1.Color = originalColor1;
                    entity1.ColorMethod = colorMethodType.byEntity;
                }

                // ✅ Highlight line 1'i sil
                if (highlightLine1 != null)
                {
                    design.Entities.Remove(highlightLine1);
                    highlightLine1 = null;
                    Debug.WriteLine("🗑️ Highlight line 1 (SARI) kaldırıldı");
                }

                // ✅ Entity 2 rengini geri çevir
                if (entity2 != null)
                {
                    entity2.Color = originalColor2;
                    entity2.ColorMethod = colorMethodType.byEntity;
                }

                // ✅ Highlight line 2'yi sil
                if (highlightLine2 != null)
                {
                    design.Entities.Remove(highlightLine2);
                    highlightLine2 = null;
                    Debug.WriteLine("🗑️ Highlight line 2 (CYAN) kaldırıldı");
                }

                // Edge'leri ve entity'leri temizle
                edge1 = null;
                edge2 = null;
                entity1 = null;
                entity2 = null;

                selectionCount = 0;
                distance = 0;
                angle = 0;
                edge1Length = 0;
                edge2Length = 0;

                // Mesafe çizgisini kaldır
                if (distanceLine != null)
                {
                    design.Entities.Remove(distanceLine);
                    distanceLine = null;
                    Debug.WriteLine("🗑️ Mesafe çizgisi kaldırıldı");
                }

                design.Invalidate();

                // Info Panel'i waiting mesajına döndür
                if (infoPanel != null && !infoPanel.IsDisposed)
                {
                    infoPanel.ShowWaitingMessage();
                }

                Debug.WriteLine("🔄 Edge to Edge seçimi sıfırlandı");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ResetSelection hatası: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE METHODS - MOUSE HANDLING
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mouse click event handler - Edge seçimi
        /// </summary>
        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (!isActive) return;
            if (e.Button != MouseButtons.Left) return;

            try
            {
                Debug.WriteLine("═══════════════════════════════════════");
                Debug.WriteLine($"🖱️ Mouse tıklaması! Seçim: {selectionCount + 1}/2");

                // Mouse altındaki entity'yi bul
                int entityIndex = design.GetEntityUnderMouseCursor(e.Location, true);

                if (entityIndex < 0)
                {
                    Debug.WriteLine("⚠️ Entity bulunamadı!");
                    return;
                }

                Entity entity = design.Entities[entityIndex];

                // ✅ SADECE Surface veya Brep kabul et!
                if (!(entity is Surface) && !(entity is Brep))
                {
                    Debug.WriteLine($"⚠️ Entity tipi desteklenmiyor: {entity.GetType().Name}");
                    MessageBox.Show(
                        "Lütfen bir Surface veya Brep seçin!\n\n" +
                        "Edge to Edge modu sadece Surface ve Brep entity'leri ile çalışır.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                Debug.WriteLine($"✅ Entity bulundu: {entity.GetType().Name}");

                // ✅ Entity'den edge'leri çıkar (Surface veya Brep)
                ICurve[] edgeCurves = ExtractEdgesFromEntity(entity);

                if (edgeCurves == null || edgeCurves.Length == 0)
                {
                    Debug.WriteLine("⚠️ Entity'den edge çıkarılamadı!");
                    MessageBox.Show(
                        $"Bu {entity.GetType().Name} entity'sinden edge (kenar) çıkarılamadı!",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                Debug.WriteLine($"✅ {edgeCurves.Length} edge bulundu!");

                // Mouse'a en yakın edge'i bul
                ICurve? closestEdge = FindClosestEdge(edgeCurves, e.Location);

                if (closestEdge == null)
                {
                    Debug.WriteLine("⚠️ En yakın edge bulunamadı!");
                    return;
                }

                Debug.WriteLine($"✅ En yakın edge bulundu!");

                // Seçim sayısına göre işlem yap
                if (selectionCount == 0)
                {
                    // İLK EDGE SEÇİMİ
                    HandleFirstEdgeSelection(entity, closestEdge);
                }
                else if (selectionCount == 1)
                {
                    // İKİNCİ EDGE SEÇİMİ
                    HandleSecondEdgeSelection(entity, closestEdge);
                }
                else
                {
                    // 3. TIKLAMADA RESET
                    Debug.WriteLine("🔄 3. tıklama - Reset!");
                    ResetSelection();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ OnMouseClick hatası: {ex.Message}");
                MessageBox.Show(
                    $"Hata oluştu:\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MOUSE MOVE EVENT - HOVER PREVIEW
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mouse hareket eventi - Hover preview için
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!isActive) return;

                // Tıklama sonrası hover'ı kapat (seçim tamamlandıysa)
                if (selectionCount >= 1)
                {
                    ClearHoverPreview();
                    return;
                }

                // Mouse altındaki entity'yi bul
                int entityIndex = design.GetEntityUnderMouseCursor(e.Location, true);

                if (entityIndex >= 0)
                {
                    Entity entity = design.Entities[entityIndex];

                    // Surface veya Brep mi kontrol et
                    if (entity is Surface || entity is Brep)
                    {
                        // Edge'leri çıkar
                        ICurve[]? edges = ExtractEdgesFromEntity(entity);

                        if (edges != null && edges.Length > 0)
                        {
                            // En yakın edge'i bul
                            ICurve? closestEdge = FindClosestEdge(edges, e.Location);

                            if (closestEdge != null)
                            {
                                // Hover preview göster
                                ShowHoverPreview(entity, closestEdge);
                                return;
                            }
                        }
                    }
                }

                // Mouse boşlukta veya uygun entity yok - hover temizle
                ClearHoverPreview();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ OnMouseMove hatası: {ex.Message}");
                // Sessizce devam et (hover preview kritik değil)
            }
        }

        /// <summary>
        /// Hover preview göster (hafif highlight)
        /// </summary>
        private void ShowHoverPreview(Entity entity, ICurve edge)
        {
            try
            {
                // Eğer aynı entity ise güncelleme yapma
                if (hoveredEntity == entity && hoveredEdge == edge)
                    return;

                // Önceki hover'ı temizle
                ClearHoverPreview();

                // Yeni hover
                hoveredEntity = entity;
                hoveredEdge = edge;
                originalHoverColor = entity.Color;

                // Hafif sarı highlight (yarı saydam)
                entity.Color = Color.FromArgb(255, 255, 200);  // Açık sarı
                entity.ColorMethod = colorMethodType.byEntity;

                design.Invalidate();

                Debug.WriteLine($"🟡 Hover: {entity.GetType().Name} (Edge uzunluk: {edge.Length():F2} mm)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ShowHoverPreview hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Hover preview temizle
        /// </summary>
        private void ClearHoverPreview()
        {
            try
            {
                if (hoveredEntity != null)
                {
                    // Orijinal renge dön
                    hoveredEntity.Color = originalHoverColor;
                    hoveredEntity.ColorMethod = colorMethodType.byEntity;

                    hoveredEntity = null;
                    hoveredEdge = null;

                    design.Invalidate();

                    Debug.WriteLine("🔲 Hover temizlendi");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ClearHoverPreview hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// İlk edge seçimini işle
        /// </summary>
        private void HandleFirstEdgeSelection(Entity entity, ICurve edge)
        {
            Debug.WriteLine("═══════════════════════════════════════");
            Debug.WriteLine("📍 İLK EDGE SEÇİMİ");

            // Hover temizle
            ClearHoverPreview();

            // Entity ve edge'i sakla
            entity1 = entity;
            edge1 = edge;
            selectionCount = 1;

            // Orijinal rengi sakla
            originalColor1 = entity.Color;

            // ⭐ HIGHLIGHT: Entity SARI + Kalın çizgi
            entity.Color = Color.FromArgb(255, 255, 0);  // Parlak sarı
            entity.ColorMethod = colorMethodType.byEntity;

            // Kalın highlight çizgisi ekle (edge üzerinde)
            highlightLine1 = new Line(edge.StartPoint, edge.EndPoint);
            highlightLine1.Color = Color.FromArgb(255, 0, 0);  // Kırmızı (daha görünür)
            highlightLine1.ColorMethod = colorMethodType.byEntity;
            highlightLine1.LineWeight = 5.0f;  // Orta kalınlık
            highlightLine1.LineWeightMethod = colorMethodType.byEntity;
            highlightLine1.LayerName = MEASUREMENT_LAYER_NAME;

            design.Entities.Add(highlightLine1, 0);
            design.Invalidate();

            Debug.WriteLine($"   🟡 SARI entity highlight + kırmızı edge çizgisi");
            Debug.WriteLine($"      • End sphere (Radius=3.0) @ ({edge.EndPoint.X:F2}, {edge.EndPoint.Y:F2}, {edge.EndPoint.Z:F2})");

            // Edge uzunluğunu hesapla
            if (edge != null)
            {
                edge1Length = edge.Length();
                Debug.WriteLine($"   📏 Edge1 uzunluğu: {edge1Length:F2} mm");
            }

            // Info panel'i güncelle
            if (infoPanel != null && !infoPanel.IsDisposed)
            {
                infoPanel.UpdateFirstEdgeInfo(edge1Length);
            }

            Debug.WriteLine("✅ İlk edge seçildi (SARI çizgi + marker'lar)");
            Debug.WriteLine("👉 İkinci edge'i seçin...");
        }

        /// <summary>
        /// İkinci edge seçimini işle
        /// </summary>
        private void HandleSecondEdgeSelection(Entity entity, ICurve edge)
        {
            Debug.WriteLine("═══════════════════════════════════════");
            Debug.WriteLine("📍 İKİNCİ EDGE SEÇİMİ");

            // Entity ve edge'i sakla
            entity2 = entity;
            edge2 = edge;
            selectionCount = 2;

            // Orijinal rengi sakla
            originalColor2 = entity.Color;

            // ⭐ HIGHLIGHT: Entity CYAN + Kalın çizgi
            entity.Color = Color.FromArgb(0, 255, 255);  // Parlak cyan
            entity.ColorMethod = colorMethodType.byEntity;

            // Kalın highlight çizgisi ekle (edge üzerinde)
            highlightLine2 = new Line(edge.StartPoint, edge.EndPoint);
            highlightLine2.Color = Color.FromArgb(255, 0, 0);  // Kırmızı (daha görünür)
            highlightLine2.ColorMethod = colorMethodType.byEntity;
            highlightLine2.LineWeight = 5.0f;  // Orta kalınlık
            highlightLine2.LineWeightMethod = colorMethodType.byEntity;
            highlightLine2.LayerName = MEASUREMENT_LAYER_NAME;

            design.Entities.Add(highlightLine2, 0);
            design.Invalidate();

            Debug.WriteLine($"   🔵 CYAN entity highlight + kırmızı edge çizgisi");

            // Edge uzunluğunu hesapla
            if (edge != null)
            {
                edge2Length = edge.Length();
                Debug.WriteLine($"   📏 Edge2 uzunluğu: {edge2Length:F2} mm");
            }

            Debug.WriteLine("✅ İkinci edge seçildi (CYAN çizgi)");

            // Mesafe ve açı hesapla
            CalculateDistanceAndAngle();

            // Kırmızı çizgi çiz
            DrawDistanceLine();

            // Info panel'i güncelle
            UpdateInfoPanel();

            Debug.WriteLine("═══════════════════════════════════════");
            Debug.WriteLine("✅ ÖLÇÜM TAMAMLANDI!");
            Debug.WriteLine("👉 3. tıklama ile reset yapabilirsiniz");
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE METHODS - EDGE EXTRACTION & FINDING
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Entity'den edge'leri çıkar (Surface veya Brep)
        /// </summary>
        private ICurve[] ExtractEdgesFromEntity(Entity entity)
        {
            try
            {
                // ✅ SURFACE: ExtractEdges() kullan
                if (entity is Surface surface)
                {
                    Debug.WriteLine("📦 Surface'den edge'ler çıkarılıyor...");
                    ICurve[] edges = surface.ExtractEdges();

                    if (edges != null && edges.Length > 0)
                    {
                        Debug.WriteLine($"   ✅ {edges.Length} edge çıkarıldı!");
                        return edges;
                    }
                    else
                    {
                        Debug.WriteLine("   ⚠️ ExtractEdges() boş döndü!");
                        return null;
                    }
                }

                // ✅ BREP: Edges dizisinden Curve'leri al
                if (entity is Brep brep)
                {
                    Debug.WriteLine("📦 Brep'den edge'ler çıkarılıyor...");

                    if (brep.Edges == null || brep.Edges.Length == 0)
                    {
                        Debug.WriteLine("   ⚠️ Brep.Edges boş!");
                        return null;
                    }

                    // Brep.Edge dizisinden ICurve dizisi oluştur
                    ICurve[] curves = new ICurve[brep.Edges.Length];
                    for (int i = 0; i < brep.Edges.Length; i++)
                    {
                        curves[i] = brep.Edges[i].Curve;
                    }

                    Debug.WriteLine($"   ✅ {curves.Length} edge çıkarıldı!");
                    return curves;
                }

                Debug.WriteLine("⚠️ Entity tipi desteklenmiyor!");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ExtractEdgesFromEntity hatası: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Mouse konumuna en yakın edge'i bul (ICurve dizisinden)
        /// </summary>
        private ICurve? FindClosestEdge(ICurve[] edges, Point mouseLocation)
        {
            try
            {
                if (edges == null || edges.Length == 0)
                    return null;

                ICurve? closestEdge = null;
                double minDistance = double.MaxValue;

                // Viewport al
                var viewport = design.Viewports[0];

                // Her edge'in orta noktasına bakalım
                foreach (var edge in edges)
                {
                    if (edge == null) continue;

                    // Edge'in orta noktası
                    Point3D midPoint = new Point3D(
                        (edge.StartPoint.X + edge.EndPoint.X) / 2.0,
                        (edge.StartPoint.Y + edge.EndPoint.Y) / 2.0,
                        (edge.StartPoint.Z + edge.EndPoint.Z) / 2.0
                    );

                    // 3D noktayı 2D ekran koordinatına çevir
                    Point3D screenPt = viewport.WorldToScreen(midPoint);
                    double screenY = viewport.Size.Height - screenPt.Y;

                    // Ekran mesafesini hesapla
                    double dx = screenPt.X - mouseLocation.X;
                    double dy = screenY - mouseLocation.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestEdge = edge;
                    }
                }

                Debug.WriteLine($"   🎯 En yakın edge bulundu! Ekran mesafesi: {minDistance:F1} px");
                return closestEdge;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ FindClosestEdge hatası: {ex.Message}");
                return null;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE METHODS - CALCULATIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// İki edge arasındaki mesafe ve açıyı hesapla
        /// </summary>
        private void CalculateDistanceAndAngle()
        {
            try
            {
                if (edge1 == null || edge2 == null) return;

                Debug.WriteLine("📐 Mesafe ve açı hesaplanıyor...");

                // ✅ ICurve'lerden Line entity'leri oluştur
                // Edge'ler herhangi bir curve tipi olabilir (Line, Arc, Circle, vs.)
                // MinimumDistance için StartPoint ve EndPoint'lerinden Line oluşturuyoruz
                Line line1 = new Line(edge1.StartPoint, edge1.EndPoint);
                Line line2 = new Line(edge2.StartPoint, edge2.EndPoint);

                // MinimumDistance kullan (Eyeshot WorkUnit)
                var minDist = new MinimumDistance(line1, line2);
                minDist.DoWork(null, null);

                // Sonucu al
                Segment3D segment = minDist.Result;
                distance = segment.Length;
                closestPoint1 = segment.P0;
                closestPoint2 = segment.P1;

                Debug.WriteLine($"   📏 Mesafe: {distance:F2} mm");
                Debug.WriteLine($"   📍 P0: ({closestPoint1.X:F2}, {closestPoint1.Y:F2}, {closestPoint1.Z:F2})");
                Debug.WriteLine($"   📍 P1: ({closestPoint2.X:F2}, {closestPoint2.Y:F2}, {closestPoint2.Z:F2})");

                // Açı hesapla (edge yönleri arası)
                angle = CalculateAngleBetweenEdges();

                Debug.WriteLine($"   📐 Açı: {angle:F2}°");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ CalculateDistanceAndAngle hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// İki edge arasındaki açıyı hesapla
        /// </summary>
        private double CalculateAngleBetweenEdges()
        {
            try
            {
                if (edge1 == null || edge2 == null) return 0;

                // Her edge'in yön vektörünü al (StartPoint → EndPoint)
                Vector3D vector1 = new Vector3D(
                    edge1.EndPoint.X - edge1.StartPoint.X,
                    edge1.EndPoint.Y - edge1.StartPoint.Y,
                    edge1.EndPoint.Z - edge1.StartPoint.Z
                );

                Vector3D vector2 = new Vector3D(
                    edge2.EndPoint.X - edge2.StartPoint.X,
                    edge2.EndPoint.Y - edge2.StartPoint.Y,
                    edge2.EndPoint.Z - edge2.StartPoint.Z
                );

                // Normalize et
                vector1.Normalize();
                vector2.Normalize();

                // Dot product
                double dotProduct = Vector3D.Dot(vector1, vector2);

                // Clamp [-1, 1]
                dotProduct = Math.Max(-1.0, Math.Min(1.0, dotProduct));

                // Açı hesapla (radyan → derece)
                double angleRadians = Math.Acos(dotProduct);
                double angleDegrees = angleRadians * (180.0 / Math.PI);

                return angleDegrees;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ CalculateAngleBetweenEdges hatası: {ex.Message}");
                return 0;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE METHODS - VISUALIZATION
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// İki en yakın nokta arasına kırmızı çizgi çiz
        /// </summary>
        private void DrawDistanceLine()
        {
            try
            {
                if (closestPoint1 == null || closestPoint2 == null) return;

                // Eski çizgiyi kaldır
                if (distanceLine != null)
                {
                    design.Entities.Remove(distanceLine);
                }

                // Yeni kırmızı çizgi
                distanceLine = new Line(closestPoint1, closestPoint2);
                distanceLine.Color = Color.Red;
                distanceLine.ColorMethod = colorMethodType.byEntity;
                distanceLine.LineWeightMethod = colorMethodType.byEntity;
                distanceLine.LayerName = MEASUREMENT_LAYER_NAME;

                design.Entities.Add(distanceLine);
                design.Invalidate();

                Debug.WriteLine("✅ Kırmızı mesafe çizgisi çizildi!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ DrawDistanceLine hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Info panel'i güncelle
        /// </summary>
        private void UpdateInfoPanel()
        {
            try
            {
                if (infoPanel == null || infoPanel.IsDisposed) return;

                // Paralel/Perpendicular durumu
                string angleStatus = "";
                if (angle < 5.0)
                    angleStatus = " (Paralel ∥)";
                else if (angle > 85.0 && angle < 95.0)
                    angleStatus = " (Dik ⊥)";

                infoPanel.UpdateMeasurementInfo(
                    edge1Length,
                    edge2Length,
                    distance,
                    angle,
                    angleStatus
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ UpdateInfoPanel hatası: {ex.Message}");
            }
        }
    }
}