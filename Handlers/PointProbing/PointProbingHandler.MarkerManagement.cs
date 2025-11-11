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
        public void UpdateAllMarkerSizes()
        {
            try
            {
                if (treeViewManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ TreeViewManager null, marker güncellenemiyor");
                    return;
                }
                
                // Yeni probe diameter'ı ve retract'ı al
                double newDiameter = treeViewManager.GetSelectedProbeDiameter();
                double retractDistance = treeViewManager.RetractDistance;
                
                System.Diagnostics.Debug.WriteLine($"🔄 Marker güncelleme başladı: Yeni çap = {newDiameter}mm, Retract = {retractDistance}mm");
                
                // Eski marker'ları, normal line'ları ve Z line'ları sil
                foreach (var marker in pointMarkers)
                {
                    design.Entities.Remove(marker);
                }
                pointMarkers.Clear();
                
                foreach (var line in normalLines)
                {
                    design.Entities.Remove(line);
                }
                normalLines.Clear();
                
                foreach (var zLine in zLines)
                {
                    design.Entities.Remove(zLine);
                }
                zLines.Clear();
                
                // Yeni marker'ları, normal line'ları ve Z line'ları oluştur
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    Point3D contactPoint = selectedPoints[i];
                    Vector3D normal = pointNormals[i];
                    
                    // Marker konumunu hesapla: Temas noktası + (normal * D/2)
                    double offset = newDiameter / 2.0;
                    Point3D markerPosition = new Point3D(
                        contactPoint.X + normal.X * offset,
                        contactPoint.Y + normal.Y * offset,
                        contactPoint.Z + normal.Z * offset
                    );
                    
                    var newMarker = CreateSphereMarker(markerPosition, newDiameter, Color.Red);
                    
                    // ✅ YENİ: Grup tag'ini marker'a ekle
                    if (groupId > 0)
                    {
                        newMarker.EntityData = $"PointProbing_{groupId}_Marker";
                        System.Diagnostics.Debug.WriteLine($"  ✅ Marker'a grup tag'i eklendi: PointProbing_{groupId}_Marker");
                    }
                    
                    pointMarkers.Add(newMarker);
                    design.Entities.Add(newMarker);
                    
                    // Normal line oluştur (uzunluk = Retract değeri)
                    var newLine = CreateNormalLine(markerPosition, normal, retractDistance, Color.Blue);
                    normalLines.Add(newLine);
                    design.Entities.Add(newLine);
                    
                    // Z+ çizgisi oluştur (uzunluk = Retract değeri)
                    Point3D normalLineEnd = new Point3D(
                        markerPosition.X + normal.X * retractDistance,
                        markerPosition.Y + normal.Y * retractDistance,
                        markerPosition.Z + normal.Z * retractDistance
                    );
                    Vector3D zDirection = new Vector3D(0, 0, 1);
                    var newZLine = CreateNormalLine(normalLineEnd, zDirection, retractDistance, Color.Green);
                    zLines.Add(newZLine);
                    design.Entities.Add(newZLine);
                }
                
                // Yenile
                design.Entities.Regen();
                design.Invalidate();
                
                System.Diagnostics.Debug.WriteLine($"✅ {pointMarkers.Count} marker + {normalLines.Count} normal line + {zLines.Count} Z line güncellendi! (Çap: Ø{newDiameter}mm, Retract: {retractDistance}mm)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ UpdateAllMarkerSizes hatası: {ex.Message}");
            }
        }

        public void HighlightMarker(Point3D point)
        {
            try
            {
                // Önce önceki highlight'ı temizle
                ClearHighlight();
                
                // Koordinata göre marker'ı bul
                int markerIndex = -1;
                
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    Point3D p = selectedPoints[i];
                    
                    // Koordinatları karşılaştır
                    if (Math.Abs(p.X - point.X) < 0.01 &&
                        Math.Abs(p.Y - point.Y) < 0.01 &&
                        Math.Abs(p.Z - point.Z) < 0.01)
                    {
                        markerIndex = i;
                        break;
                    }
                }
                
                // Marker bulunamadıysa çık
                if (markerIndex == -1 || markerIndex >= pointMarkers.Count)
                {
                    return;
                }
                
                // Marker'ı highlight et
                Entity marker = pointMarkers[markerIndex];
                marker.Color = Color.Yellow;  // 🟡 SARI
                highlightedMarker = marker;
                
                design.Entities.Regen();
                design.Invalidate();
                
                System.Diagnostics.Debug.WriteLine($"✅ Marker highlight edildi: X={point.X:F2}, Y={point.Y:F2}, Z={point.Z:F2}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ HighlightMarker hatası: {ex.Message}");
            }
        }

        public void ClearHighlight()
        {
            try
            {
                if (highlightedMarker != null)
                {
                    highlightedMarker.Color = Color.Red;  // 🔴 KIRMIZI
                    highlightedMarker = null;
                    
                    design.Entities.Regen();
                    design.Invalidate();
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Highlight temizlendi");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ClearHighlight hatası: {ex.Message}");
            }
        }
    }
}
