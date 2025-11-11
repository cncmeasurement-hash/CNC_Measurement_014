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
    public partial class PointProbingHandler
    {
        private void Design_KeyDown(object sender, KeyEventArgs e)
        {
            // ✅ ESC tuşu basıldı mı? (Point Probing modundan çık)
            if (e.KeyCode == Keys.Escape)
            {
                if (selectionManager != null)
                {
                    selectionManager.DisablePointProbing();
                    System.Diagnostics.Debug.WriteLine("⛔ ESC tuşu: Point Probing modu kapatıldı");
                }
                return;
            }
            
            // DELETE tuşu basıldı mı?
            if (e.KeyCode != Keys.Delete) return;
            
            try
            {
                // Seçili entity'leri al
                if (design.Entities.Count == 0) return;
                
                List<int> indicesToRemove = new List<int>();
                
                // Seçili marker'ları bul
                for (int i = 0; i < pointMarkers.Count; i++)
                {
                    Entity marker = pointMarkers[i];
                    
                    // Marker seçili mi ve hala design'da var mı?
                    if (marker.Selected && design.Entities.Contains(marker))
                    {
                        indicesToRemove.Add(i);
                        System.Diagnostics.Debug.WriteLine($"🗑️ DELETE tuşu: Marker {i+1} ve çizgileri silinecek");
                    }
                }
                
                // Eğer marker bulunduysa, event'i handle et (design otomatik silmesin)
                if (indicesToRemove.Count > 0)
                {
                    e.Handled = true;  // ✅ Design otomatik silmesin, biz kontrollü sileceğiz
                }
                
                // Geriye doğru sil (index karmaşası olmasın)
                for (int i = indicesToRemove.Count - 1; i >= 0; i--)
                {
                    int index = indicesToRemove[i];
                    
                    // ✅ Silmeden önce Point3D'yi kaydet (TreeView'den silmek için)
                    Point3D pointToRemove = selectedPoints[index];
                    
                    // Marker'ı sil
                    if (design.Entities.Contains(pointMarkers[index]))
                    {
                        design.Entities.Remove(pointMarkers[index]);
                    }
                    
                    // Normal line'ı sil
                    if (index < normalLines.Count && design.Entities.Contains(normalLines[index]))
                    {
                        design.Entities.Remove(normalLines[index]);
                    }
                    
                    // Z line'ı sil
                    if (index < zLines.Count && design.Entities.Contains(zLines[index]))
                    {
                        design.Entities.Remove(zLines[index]);
                    }
                    
                    // Listelerden kaldır
                    pointMarkers.RemoveAt(index);
                    selectedPoints.RemoveAt(index);
                    pointNormals.RemoveAt(index);
                    normalLines.RemoveAt(index);
                    zLines.RemoveAt(index);
                    
                    // ✅ TreeView'den de sil
                    if (treeViewManager != null)
                    {
                        treeViewManager.RemoveProbePointFromTree(pointToRemove);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Marker {index+1} + çizgileri + TreeView node'u silindi");
                }
                
                if (indicesToRemove.Count > 0)
                {
                    design.Entities.Regen();
                    design.Invalidate();
                    System.Diagnostics.Debug.WriteLine($"✅ Toplam {indicesToRemove.Count} marker + çizgileri silindi");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Design_KeyDown hatası: {ex.Message}");
            }
        }

        private void Design_MouseClick(object sender, MouseEventArgs e)
        {
            // Mod aktif değilse çık
            if (!isEnabled)
                return;

            // Sol tık değilse çık
            if (e.Button != MouseButtons.Left)
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine("🖱️ Yüzeye tıklandı!");

                // ✅ Mouse altındaki entity'yi al
                int entityIndex = design.GetEntityUnderMouseCursor(e.Location, true);

                if (entityIndex == -1)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Mouse altında entity yok");
                    return;
                }

                Entity entity = design.Entities[entityIndex];
                System.Diagnostics.Debug.WriteLine($"📦 Entity bulundu: {entity.GetType().Name} (Index: {entityIndex})");

                // ✅ ClearancePlane layer'ını filtrele (güvenlik yüzeyi seçilmesin)
                if (entity.LayerName == "ClearancePlane")
                {
                    System.Diagnostics.Debug.WriteLine("⛔ ClearancePlane (güvenlik yüzeyi) seçilemez!");
                    return;
                }
                
                // ✅ ProbePoints layer'ını filtrele (kendi marker'larına tıklanmasın)
                if (entity.LayerName == MARKER_LAYER_NAME)
                {
                    System.Diagnostics.Debug.WriteLine("⛔ ProbePoints marker'ına tıklanamaz!");
                    return;
                }

                // ✅ Entity IFace mi kontrol et (Surface, Brep, Mesh)
                if (!(entity is IFace faceEntity))
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Entity IFace değil");
                    return;
                }

                // ✅ Eyeshot'ın FindClosestTriangle metodu
                Point3D clickedPoint;
                int triangleIndex;

                double distance = design.FindClosestTriangle(
                    faceEntity,
                    e.Location,
                    out clickedPoint,
                    out triangleIndex
                );

                // ✅ Distance kontrolü
                if (distance >= 0 && triangleIndex >= 0 && clickedPoint != null)
                {
                    System.Diagnostics.Debug.WriteLine($"📍 Probe noktası: ({clickedPoint.X:F2}, {clickedPoint.Y:F2}, {clickedPoint.Z:F2})");
                    
                    // ✅ Normal vektörünü hesapla
                    Vector3D normal = CalculateTriangleNormal(faceEntity, triangleIndex);
                    
                    System.Diagnostics.Debug.WriteLine($"↗️ Normal vektör: ({normal.X:F3}, {normal.Y:F3}, {normal.Z:F3})");
                    
                    // ✅ Z- yönündeki normal'leri filtrele (alt yüzeyler)
                    const double EPSILON = 0.001; // Tolerans
                    if (normal.Z < -EPSILON)
                    {
                        System.Diagnostics.Debug.WriteLine("⛔ ALT YÜZEY ALGILANDI!");
                        System.Diagnostics.Debug.WriteLine($"   Normal.Z = {normal.Z:F3} < -{EPSILON}");
                        System.Diagnostics.Debug.WriteLine("   → Alt yüzeylere probe yapılamaz!");
                        return;
                    }
                    
                    // Noktayı kaydet ve marker ekle
                    AddProbePoint(clickedPoint, normal);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ FindClosestTriangle başarısız");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Mouse click hatası: {ex.Message}");
            }
        }
    }
}
