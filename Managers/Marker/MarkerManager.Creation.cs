using System;
using System.Drawing;
using System.Windows.Forms;
using _014.Analyzers.Data;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 2/3: Marker ve ok oluşturma
    /// </summary>
    public partial class MarkerManager
    {
        // ═══════════════════════════════════════════════════════════
        // MARKER CREATION
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Marker ekle - Yüzey bilgisi ile
        /// </summary>
        public void AddPointMarker(Point3D point, SurfaceData surface)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🎯 AddPointMarker (with surface): [{point.X:F3}, {point.Y:F3}, {point.Z:F3}]");
                System.Diagnostics.Debug.WriteLine($"  📍 Yüzey: {surface.Name}");
                System.Diagnostics.Debug.WriteLine($"  🧭 Normal: [{surface.Normal.X:F3}, {surface.Normal.Y:F3}, {surface.Normal.Z:F3}]");

                // Z- KONTROLÜ
                if (surface.Normal.Z < -0.01)
                {
                    System.Diagnostics.Debug.WriteLine($"  ❌ Z- NORMAL! Z={surface.Normal.Z:F3}");

                    MessageBox.Show(
                        $"❌ BU YÜZEY ÖLÇÜLEMEZ!\n\n" +
                        $"Yüzey: {surface.Name}\n" +
                        $"Normal: [{surface.Normal.X:F3}, {surface.Normal.Y:F3}, {surface.Normal.Z:F3}]\n\n" +
                        $"⚠️ Z komponenti negatif (aşağı bakıyor)!\n" +
                        $"Prob bu yüzeye erişemez.",
                        "Ölçülemez Yüzey",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"  ✅ Z+ NORMAL - OK");

                // MARKER EKLE (Çap 6mm / Yarıçap 3mm)
                double markerSize = 3.0; // Yarıçap = 3mm → Çap = 6mm
                var marker = Mesh.CreateSphere(markerSize, 8, 8);
                marker.Translate(point.X, point.Y, point.Z);
                marker.Color = Color.Red;
                marker.ColorMethod = colorMethodType.byEntity;

                int markerIndex = pointsDataTable.Rows.Count;
                marker.EntityData = $"POINT_MARKER_{markerIndex}";

                design.Entities.Add(marker);

                // NORMAL OKU EKLE
                AddNormalArrow(point, surface.Normal);

                System.Diagnostics.Debug.WriteLine($"  ✅ Marker + normal oku eklendi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AddPointMarker error: {ex.Message}");
                MessageBox.Show($"Marker eklenirken hata: {ex.Message}", "Hata");
            }
        }
        
        /// <summary>
        /// Marker ekle - Eski API (geriye uyumluluk)
        /// </summary>
        public void AddPointMarker(Point3D point)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🎯 AddPointMarker: [{point.X:F3}, {point.Y:F3}, {point.Z:F3}]");

                // YÜZEY DATA KONTROL
                if (dataManager.GetSurfaceDataList().Count == 0)
                {
                    MessageBox.Show(
                        "Önce 'Show Face Normals' ile yüzeyleri tanımlayın!",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // EN YAKIN YÜZEYİ BUL
                SurfaceData closestSurface = null;
                double minDistance = double.MaxValue;

                foreach (var surface in dataManager.GetSurfaceDataList())
                {
                    double distance = Point3D.Distance(point, surface.Center);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestSurface = surface;
                    }
                }

                if (closestSurface == null)
                {
                    MessageBox.Show("Yüzey bulunamadı!", "Hata");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"  📍 Yüzey: {closestSurface.Name}");
                System.Diagnostics.Debug.WriteLine($"  🧭 Normal: [{closestSurface.Normal.X:F3}, {closestSurface.Normal.Y:F3}, {closestSurface.Normal.Z:F3}]");

                // Z- KONTROLÜ
                if (closestSurface.Normal.Z < -0.01)
                {
                    System.Diagnostics.Debug.WriteLine($"  ❌ Z- NORMAL! Z={closestSurface.Normal.Z:F3}");

                    MessageBox.Show(
                        $"❌ BU YÜZEY ÖLÇÜLEMEZ!\n\n" +
                        $"Yüzey: {closestSurface.Name}\n" +
                        $"Normal: [{closestSurface.Normal.X:F3}, {closestSurface.Normal.Y:F3}, {closestSurface.Normal.Z:F3}]\n\n" +
                        $"⚠️ Z komponenti negatif (aşağı bakıyor)!\n" +
                        $"Prob bu yüzeye erişemez.",
                        "Ölçülemez Yüzey",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"  ✅ Z+ NORMAL - OK");

                // MARKER EKLE (Çap 6mm / Yarıçap 3mm)
                double markerSize = 3.0; // Yarıçap = 3mm → Çap = 6mm
                var marker = Mesh.CreateSphere(markerSize, 8, 8);
                marker.Translate(point.X, point.Y, point.Z);
                marker.Color = Color.Red;
                marker.ColorMethod = colorMethodType.byEntity;

                int markerIndex = pointsDataTable.Rows.Count;
                marker.EntityData = $"POINT_MARKER_{markerIndex}";

                design.Entities.Add(marker);

                // NORMAL OKU EKLE
                AddNormalArrow(point, closestSurface.Normal);

                System.Diagnostics.Debug.WriteLine($"  ✅ Marker + normal oku eklendi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AddPointMarker error: {ex.Message}");
                MessageBox.Show($"Hata: {ex.Message}", "Nokta Ekleme Hatası");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ARROW CREATION
        // ═══════════════════════════════════════════════════════════
        
        public void AddNormalArrow(Point3D point, Vector3D normal)
        {
            try
            {
                // Ok uzunluğu
                double arrowLength = 30.0; // 30mm uzunluk

                // Ok başlangıç ve bitiş noktaları
                Point3D arrowStart = point;
                Point3D arrowEnd = new Point3D(
                    point.X + normal.X * arrowLength,
                    point.Y + normal.Y * arrowLength,
                    point.Z + normal.Z * arrowLength
                );

                // Ana çizgi (gövde) - ŞİMDİLİK SADECE ÇİZGİ
                devDept.Eyeshot.Entities.Line arrowLine = new devDept.Eyeshot.Entities.Line(arrowStart, arrowEnd);
                arrowLine.Color = Color.Blue; // Mavi ok
                arrowLine.LineWeight = 3.0f; // Kalın çizgi
                arrowLine.ColorMethod = colorMethodType.byEntity;

                int markerIndex = pointsDataTable.Rows.Count;
                arrowLine.EntityData = $"NORMAL_ARROW_{markerIndex}";
                design.Entities.Add(arrowLine);

                System.Diagnostics.Debug.WriteLine($"✅ Normal oku çizildi: [{normal.X:F3}, {normal.Y:F3}, {normal.Z:F3}] uzunluk={arrowLength}mm");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddNormalArrow error: {ex.Message}");
            }
        }
    }
}
