using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace _014
{
    /// <summary>
    /// LENGTH MEASUREMENT - MOUSE MOVE HANDLING
    /// PARTIAL CLASS 4B/6: Mouse move event, throttling, snap detection
    /// </summary>
    public partial class LengthMeasurementAnalyzer
    {
        // ═══════════════════════════════════════════════════════════
        // MOUSE MOVE EVENT
        // ═══════════════════════════════════════════════════════════

        private void Design_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isEnabled || !snapEnabled) return;

            try
            {
                // ✅ OPTİMİZASYON 1: THROTTLING
                // MouseMove çok sık tetikleniyor - 50ms'de bir çalıştır
                TimeSpan timeSinceLastMove = DateTime.Now - lastMouseMoveTime;
                if (timeSinceLastMove.TotalMilliseconds < MOUSE_MOVE_THROTTLE_MS)
                {
                    return; // Çok erken, atla
                }
                lastMouseMoveTime = DateTime.Now;

                // ✅ OPTİMİZASYON 2: VIEWPORT DEĞİŞİKLİĞİ KONTROLÜ
                // Parça dönüyor/zoom yapılıyorsa snap hesaplama
                var camera = design.Viewports[0].Camera;
                string currentCameraState = $"{camera.Target.X},{camera.Target.Y},{camera.Target.Z}|{camera.Distance}";

                if (!string.IsNullOrEmpty(lastCameraState) && currentCameraState != lastCameraState)
                {
                    // Viewport değişiyor (döndürme/zoom)
                    lastCameraState = currentCameraState;
                    isViewportStable = false;

                    // Snap marker'ı kaldır
                    RemoveTempSnapMarker();
                    hoveredSnapPoint = null;
                    hoveredEntity = null;

                    return; // Snap hesaplama yapma, performansı koru
                }
                else
                {
                    // Viewport sabit
                    lastCameraState = currentCameraState;
                    isViewportStable = true;
                }

                // Önceki snap marker'ı temizle
                RemoveTempSnapMarker();

                hoveredSnapPoint = null;
                hoveredEntity = null;

                Point3D bestSnapPoint = null;
                Entity bestEntity = null;
                double minDistance = double.MaxValue;

                // ✅ HYBRID YAKLAŞIM: Önce GetEntityUnderMouseCursor dene (hızlı)
                // Başarısız olursa tüm entity'lerde ara (güvenli)

                int entityIndex = design.GetEntityUnderMouseCursor(e.Location, true);

                if (entityIndex != -1)
                {
                    // ✅ HIZLI YOL: GetEntityUnderMouseCursor başarılı
                    Entity entity = design.Entities[entityIndex];

                    if (entity is Mesh || entity is Surface)
                    {
                        Point3D snapPoint = FindNearestSnapPoint(entity, e.Location);

                        if (snapPoint != null)
                        {
                            // Snap noktasının ekran mesafesini hesapla
                            var viewport = design.Viewports[0];
                            Point3D screenPt = viewport.WorldToScreen(snapPoint);
                            double screenY = viewport.Size.Height - screenPt.Y;

                            double dx = screenPt.X - e.Location.X;
                            double dy = screenY - e.Location.Y;
                            double screenDist = Math.Sqrt(dx * dx + dy * dy);

                            if (screenDist < snapTolerance)
                            {
                                bestSnapPoint = snapPoint;
                                bestEntity = entity;
                                minDistance = screenDist;

                                System.Diagnostics.Debug.WriteLine($"📍 SNAP (hızlı): {minDistance:F1} px");
                            }
                        }
                    }
                }
                else
                {
                    // ⚠️ YEDEK YOL: GetEntityUnderMouseCursor başarısız - tüm entity'lerde ara
                    System.Diagnostics.Debug.WriteLine("⚠️ GetEntityUnderMouseCursor başarısız, tüm entity'lerde aranıyor...");

                    foreach (Entity entity in design.Entities)
                    {
                        // ✅ OPTİMİZASYON 3: GÖRÜNÜRLÜK KONTROLÜ
                        // Görünmez entity'leri atla (performans)
                        if (!entity.Visible) continue;

                        // Layer görünürlük kontrolü
                        if (!string.IsNullOrEmpty(entity.LayerName) &&
                            design.Layers.Contains(entity.LayerName) &&
                            !design.Layers[entity.LayerName].Visible)
                            continue;

                        // Sadece Mesh ve Surface ile çalış
                        if (!(entity is Mesh || entity is Surface)) continue;

                        // Bu entity için en yakın snap noktasını bul
                        Point3D snapPoint = FindNearestSnapPoint(entity, e.Location);

                        if (snapPoint != null)
                        {
                            // Snap noktasının ekran mesafesini hesapla
                            var viewport = design.Viewports[0];
                            Point3D screenPt = viewport.WorldToScreen(snapPoint);
                            double screenY = viewport.Size.Height - screenPt.Y;

                            double dx = screenPt.X - e.Location.X;
                            double dy = screenY - e.Location.Y;
                            double screenDist = Math.Sqrt(dx * dx + dy * dy);

                            // En yakın olanı tut
                            if (screenDist < minDistance)
                            {
                                minDistance = screenDist;
                                bestSnapPoint = snapPoint;
                                bestEntity = entity;
                            }
                        }
                    }

                    if (bestSnapPoint != null && minDistance < snapTolerance)
                    {
                        System.Diagnostics.Debug.WriteLine($"📍 SNAP (yedek): {minDistance:F1} px");
                    }
                }

                // ✅ Snap bulundu mu?
                if (bestSnapPoint != null && minDistance < snapTolerance)
                {
                    hoveredSnapPoint = bestSnapPoint;
                    hoveredEntity = bestEntity;

                    // Snap marker göster (küçük yeşil küre)
                    ShowSnapPreview(bestSnapPoint);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ MouseMove hatası: {ex.Message}");
            }
        }
    }
}
