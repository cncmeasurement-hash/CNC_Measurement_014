using _014.Analyzers.Data;
using _014.Managers.Data;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace _014.Handlers.Selection
{
    /// <summary>
    /// Nokta seçim işlemlerini yönetir.
    /// 3D model üzerinde nokta seçimi ve yüzey renk yönetimi sağlar.
    /// </summary>
    public class PointSelectionHandler
    {
        private Design design;
        private DataManager dataManager;
        private MarkerManager markerManager;
        private UIManager uiManager;

        // 🎨 Yüzey renk yönetimi için

        public PointSelectionHandler(Design design, DataManager dataManager, MarkerManager markerManager, UIManager uiManager)
        {
            this.design = design;
            this.dataManager = dataManager;
            this.markerManager = markerManager;
            this.uiManager = uiManager;
        }

        /// <summary>
        /// Nokta seçim modunu aktif/pasif eder.
        /// </summary>
        /// <param name="enable">True: aktif, False: pasif</param>
        public void Enable(bool enable)
        {
            if (enable)
            {
                design.MouseClick += Design_MouseClick_Point;
                design.KeyDown += Design_KeyDown_Point;

                // ✅ Face selection mode'u aktifleştir (sadece yüzey seçimi için)
                design.ActionMode = actionType.SelectVisibleByPick;
                design.SelectionFilterMode = selectionFilterType.Face;

                design.Cursor = Cursors.Cross;
                uiManager.CoordinateLabel.Visible = true;
                uiManager.PointsGridView.Visible = true;

                // Yüzey grid'ini de göster (eğer doldurulmuşsa)
                if (dataManager.GetSurfaceDataList().Count > 0)
                {
                    uiManager.SurfacesGridView.Visible = true;
                    uiManager.SurfacesGridView.BringToFront();
                }

                // ❌ BİLGİ MESAJI KALDIRILDI!
            }
            else
            {
                design.MouseClick -= Design_MouseClick_Point;
                design.KeyDown -= Design_KeyDown_Point;

                // ✅ Face selection mode'u kapat
                design.ActionMode = actionType.None;
                design.SelectionFilterMode = selectionFilterType.Face;

                design.Cursor = Cursors.Default;
                uiManager.CoordinateLabel.Visible = false;
                uiManager.PointsGridView.Visible = false;
                uiManager.SurfacesGridView.Visible = false;

                // 🎨 Seçimi temizle
                design.Entities.ClearSelection();

                design.Invalidate();
            }
        }

        /// <summary>
        /// Mouse click event handler.
        /// Sol tıklama ile nokta seçimi yapar ve yüzey rengini değiştirir.
        /// </summary>
        private void Design_MouseClick_Point(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            try
            {
                // ScreenToWorld ile 3D nokta al
                var viewport = design.Viewports[0];
                List<System.Drawing.Point> mousePoints = new List<System.Drawing.Point>
                {
                    new System.Drawing.Point(e.X, e.Y)
                };
                Point3D[] worldPoints = viewport.ScreenToWorld(mousePoints);

                // ✅ NULL KONTROLÜ
                if (worldPoints == null || worldPoints.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ worldPoints null veya boş");
                    return;
                }

                Point3D clickedPoint = worldPoints[0];

                // ✅ NULL KONTROLÜ - clickedPoint
                if (clickedPoint == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ clickedPoint null");
                    return;
                }

                // Mouse altındaki entity'yi bul (manuel kontrol)
                Entity clickedEntity = null;
                double minScreenDistance = double.MaxValue;

                foreach (var entity in design.Entities)
                {
                    if (!entity.Visible)
                        continue;

                    // Marker veya arrow değilse
                    if (entity.EntityData is string tag &&
                        (tag.StartsWith("POINT_MARKER_") || tag.StartsWith("NORMAL_ARROW_")))
                        continue;

                    // Entity'nin merkez noktasını ekrana project et
                    if (entity.BoxMin != null && entity.BoxMax != null)
                    {
                        Point3D entityCenter = new Point3D(
                            (entity.BoxMin.X + entity.BoxMax.X) / 2.0,
                            (entity.BoxMin.Y + entity.BoxMax.Y) / 2.0,
                            (entity.BoxMin.Z + entity.BoxMax.Z) / 2.0
                        );

                        Point3D screenPt = viewport.WorldToScreen(entityCenter);
                        double dx = screenPt.X - e.X;
                        double dy = screenPt.Y - e.Y;
                        double screenDist = Math.Sqrt(dx * dx + dy * dy);

                        if (screenDist < minScreenDistance)
                        {
                            minScreenDistance = screenDist;
                            clickedEntity = entity;
                        }
                    }
                }

                // ✅ ENTITY BULUNAMADIYSA SESSIZCE GERİ DÖN
                if (clickedEntity == null)
                {
                    System.Diagnostics.Debug.WriteLine("ℹ️ Boş alana tıklandı, işlem yapılmadı");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"🎯 Entity bulundu (ekran mesafesi: {minScreenDistance:F1}px)");

                // Bu entity'nin hangi yüzeye ait olduğunu bul
                SurfaceData foundSurface = FindSurfaceByEntity(clickedEntity);

                if (foundSurface != null)
                {
                    Vector3D surfaceNormal = foundSurface.Normal;
                    string surfaceName = foundSurface.Name;

                    System.Diagnostics.Debug.WriteLine($"✅ DOĞRU YÜZEY: {surfaceName}");

                    // 🎨 Yüzey zaten Eyeshot tarafından seçilmiş durumda (Face selection mode)
                    // SelectSurfaceByIndex çağırmaya gerek yok!

                    // ✅ DataGrid'e ekle
                    uiManager.PointsDataTable.Rows.Add(
                        uiManager.PointsDataTable.Rows.Count + 1,  // # (sıra no)
                        false,                                      // ☑ Checkbox (unchecked)
                        surfaceName,                                // Yüzey No
                        clickedPoint.X.ToString("0.000"),          // X (string format)
                        clickedPoint.Y.ToString("0.000"),          // Y
                        clickedPoint.Z.ToString("0.000"),          // Z
                        surfaceNormal.X.ToString("0.000"),         // Nx
                        surfaceNormal.Y.ToString("0.000"),         // Ny
                        surfaceNormal.Z.ToString("0.000")          // Nz
                    );

                    // Marker ekle
                    markerManager.AddPointMarker(clickedPoint, foundSurface);

                    design.Invalidate();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Entity için yüzey bulunamadı");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Nokta seçim hatası: {ex.Message}");
                // ✅ KULLANICIYA HATA MESAJI GÖSTERME - sadece log'la
            }
        }

        /// Önceki seçili yüzeyi eski rengine döndürür.
        /// </summary>
        private void HighlightSurface(Entity entity)
        {
            try
            {
                // Önceki yüzeyi eski rengine döndür
                RestoreLastSurfaceColor();

                // Yeni yüzeyin orijinal rengini kaydet

                // Yüzeyi SARI yap
                entity.Color = Color.Yellow;
                entity.ColorMethod = colorMethodType.byEntity;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Renk değiştirme hatası: {ex.Message}");
            }
        }

        /// </summary>
        private void SelectSurfaceByIndex(int entityIndex)
        {
            try
            {
                // Önceki seçimi temizle
                design.Entities.ClearSelection();

                // Entity index'i kontrol et
                if (entityIndex < 0 || entityIndex >= design.Entities.Count)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Geçersiz entity index: {entityIndex}");
                    return;
                }

                // Entity'yi bul ve seç
                var entity = design.Entities[entityIndex];

                if (entity == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Entity null (index: {entityIndex})");
                    return;
                }

                // Eyeshot'a seçtir (otomatik sarı yapacak)
                entity.Selected = true;
                design.Invalidate();

                System.Diagnostics.Debug.WriteLine($"🎨 Yüzey seçildi (index: {entityIndex})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Yüzey seçme hatası: {ex.Message}");
            }
        }

        /// </summary>
        private void HighlightSurfaceByIndex(int entityIndex)
        {
            try
            {
                // Entity index'i kontrol et
                if (entityIndex < 0 || entityIndex >= design.Entities.Count)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Geçersiz entity index: {entityIndex}");
                    return;
                }

                // Entity'yi bul
                var entity = design.Entities[entityIndex];

                if (entity == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Entity null (index: {entityIndex})");
                    return;
                }

                // Önceki yüzeyi eski rengine döndür
                RestoreLastSurfaceColor();

                // Yeni yüzeyin orijinal rengini kaydet

                // Yüzeyi SARI yap
                entity.Color = Color.Yellow;
                entity.ColorMethod = colorMethodType.byEntity;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Renk değiştirme hatası: {ex.Message}");
            }
        }


        /// </summary>
        private void CreateHighlightOverlay(SurfaceData surface)
        {
            // Bu metod artık kullanılmıyor - basit renk değiştirme kullanıyoruz
        }

        /// </summary>
        private void RemoveHighlightOverlay()
        {
            try
            {
                {
                    System.Diagnostics.Debug.WriteLine("🗑️ Highlight overlay kaldırıldı");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Overlay kaldırma hatası: {ex.Message}");
            }
        }

        /// </summary>
        private void RestoreLastSurfaceColor()
        {
            try
            {
                {


                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Renk geri yükleme hatası: {ex.Message}");
            }
        }

        /// </summary>
        private SurfaceData FindSurfaceByEntity(Entity entity)
        {
            try
            {
                var surfaces = dataManager.GetSurfaceDataList();

                // Entity index'ini bul
                int entityIndex = -1;
                for (int i = 0; i < design.Entities.Count; i++)
                {
                    if (design.Entities[i] == entity)
                    {
                        entityIndex = i;
                        break;
                    }
                }

                if (entityIndex == -1)
                    return null;

                System.Diagnostics.Debug.WriteLine($"🔍 Entity index: {entityIndex}");

                // SurfaceData'da bu entity index'i olan yüzeyi bul
                foreach (var surface in surfaces)
                {
                    if (surface.EntityIndex == entityIndex)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Eşleşen yüzey bulundu: {surface.Name}");
                        return surface;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"❌ Entity index {entityIndex} için yüzey bulunamadı");

                // Plan B: Entity'nin merkezine en yakın yüzeyi bul
                if (entity.BoxMin != null && entity.BoxMax != null)
                {
                    Point3D entityCenter = new Point3D(
                        (entity.BoxMin.X + entity.BoxMax.X) / 2.0,
                        (entity.BoxMin.Y + entity.BoxMax.Y) / 2.0,
                        (entity.BoxMin.Z + entity.BoxMax.Z) / 2.0
                    );

                    return FindClosestSurfaceToPoint(entityCenter);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ FindSurfaceByEntity error: {ex.Message}");
                return null;
            }
        }

        /// </summary>
        private SurfaceData FindClosestSurfaceToPoint(Point3D point)
        {
            var surfaces = dataManager.GetSurfaceDataList();
            double minDistance = double.MaxValue;
            SurfaceData closest = null;

            foreach (var surface in surfaces)
            {
                double distance = Point3D.Distance(point, surface.Center);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = surface;
                }
            }

            return closest;
        }

        /// Delete tuşu ile seçili marker silinir.
        /// </summary>
        private void Design_KeyDown_Point(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                // TODO: Seçili marker'ı sil
            }
        }
    }
}