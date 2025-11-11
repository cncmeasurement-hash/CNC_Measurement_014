using devDept.Eyeshot.Control;
using devDept.Geometry;
using System;
using System.Windows.Forms;

namespace _014.Probe.CMM
{
    /// <summary>
    /// CMM Probe Path Test Sınıfı
    /// Form1'den çağrılacak basit test örnekleri
    /// </summary>
    public static class CMM_ProbePathTest
    {
        /// <summary>
        /// TEST 1: Basit 3 noktalı probe yolu
        /// Form1'e eklenecek test metodu
        /// </summary>
        public static void Test_SimpleThreePoints(Design design)
        {
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  CMM PROBE PATH TEST - Basit 3 Nokta");
            Console.WriteLine("═══════════════════════════════════════════════════════");

            // 1. Probe yolu oluştur
            CMM_ProbePath path = new CMM_ProbePath("Test - 3 Nokta");
            path.Type = MeasurementType.Surface;

            // 2. 3 nokta ekle (üçgen şeklinde)
            path.AddPoint(new Point3D(0, 0, 0), Vector3D.AxisZ);       // Merkez
            path.AddPoint(new Point3D(10, 0, 0), Vector3D.AxisZ);      // Sağ
            path.AddPoint(new Point3D(5, 8.66, 0), Vector3D.AxisZ);    // Üst (60 derece)

            // 3. Hesapla
            path.Calculate();

            Console.WriteLine($"✅ Yol oluşturuldu: {path.Points.Count} nokta");
            Console.WriteLine($"   Toplam mesafe: {path.TotalDistance:F2} mm");
            Console.WriteLine($"   Tahmini süre: {path.EstimatedTime:F1} saniye");

            // 4. Görselleştir
            CMM_PathVisualizer visualizer = new CMM_PathVisualizer(design);
            visualizer.DrawPath(path);

            Console.WriteLine("✅ Görselleştirme tamamlandı");
            Console.WriteLine("   Mavi çizgiler: Yol");
            Console.WriteLine("   Kırmızı küreler: Probe noktaları");
            Console.WriteLine("   Yeşil oklar: Yaklaşma yönleri");

            // 5. Kamerayı ayarla
            design.ZoomFit();
            design.Invalidate();

            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  TEST BAŞARILI! ✅");
            Console.WriteLine("═══════════════════════════════════════════════════════");
        }

        /// <summary>
        /// TEST 2: Daire ölçümü (8 noktalı)
        /// </summary>
        public static void Test_CircleMeasurement(Design design)
        {
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  CMM PROBE PATH TEST - Daire Ölçümü (8 nokta)");
            Console.WriteLine("═══════════════════════════════════════════════════════");

            // 1. Probe yolu oluştur
            CMM_ProbePath path = new CMM_ProbePath("Test - Daire Ölçümü");
            path.Type = MeasurementType.Circle;

            // 2. Daire üzerinde 8 nokta (25mm yarıçap)
            double radius = 25.0;
            int pointCount = 8;

            for (int i = 0; i < pointCount; i++)
            {
                double angle = 2 * Math.PI * i / pointCount;
                double x = radius * Math.Cos(angle);
                double y = radius * Math.Sin(angle);

                Point3D position = new Point3D(x, y, 0);
                
                // Yaklaşma yönü: merkezden dışa (radyal)
                Vector3D approach = new Vector3D(-x, -y, 0);
                approach.Normalize();

                path.AddPoint(position, approach);
            }

            // İlk noktayı tekrar ekle (döngüyü kapat)
            path.AddPoint(path.Points[0].Position, path.Points[0].ApproachDirection);

            // 3. Hesapla
            path.Calculate();

            Console.WriteLine($"✅ Daire yolu oluşturuldu: {path.Points.Count} nokta");
            Console.WriteLine($"   Yarıçap: {radius:F2} mm");
            Console.WriteLine($"   Çevre: {2 * Math.PI * radius:F2} mm");
            Console.WriteLine($"   Toplam mesafe: {path.TotalDistance:F2} mm");
            Console.WriteLine($"   Tahmini süre: {path.EstimatedTime:F1} saniye");

            // 4. Görselleştir
            CMM_PathVisualizer visualizer = new CMM_PathVisualizer(design);
            visualizer.DrawPath(path);

            Console.WriteLine("✅ Görselleştirme tamamlandı");

            // 5. Kamerayı ayarla
            design.ZoomFit();
            design.Invalidate();

            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  TEST BAŞARILI! ✅");
            Console.WriteLine("═══════════════════════════════════════════════════════");
        }

        /// <summary>
        /// TEST 3: Grid (Izgara) Taraması
        /// </summary>
        public static void Test_GridScan(Design design)
        {
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  CMM PROBE PATH TEST - Grid Tarama");
            Console.WriteLine("═══════════════════════════════════════════════════════");

            // 1. Probe yolu oluştur
            CMM_ProbePath path = new CMM_ProbePath("Test - Grid Tarama");
            path.Type = MeasurementType.Surface;

            // 2. 5x5 grid oluştur (50mm x 50mm alan)
            int rows = 5;
            int cols = 5;
            double spacing = 12.5;  // 12.5mm aralık

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    double x = col * spacing;
                    double y = row * spacing;

                    Point3D position = new Point3D(x, y, 0);
                    path.AddPoint(position, Vector3D.AxisZ);  // Yukarıdan yaklaş
                }
            }

            // 3. Hesapla
            path.Calculate();

            Console.WriteLine($"✅ Grid yolu oluşturuldu: {rows}x{cols} = {path.Points.Count} nokta");
            Console.WriteLine($"   Alan: {(rows - 1) * spacing:F1} x {(cols - 1) * spacing:F1} mm");
            Console.WriteLine($"   Nokta aralığı: {spacing:F2} mm");
            Console.WriteLine($"   Toplam mesafe: {path.TotalDistance:F2} mm");
            Console.WriteLine($"   Tahmini süre: {path.EstimatedTime:F1} saniye");

            // 4. Görselleştir
            CMM_PathVisualizer visualizer = new CMM_PathVisualizer(design);
            visualizer.DrawPath(path);

            Console.WriteLine("✅ Görselleştirme tamamlandı");

            // 5. Kamerayı ayarla
            design.ZoomFit();
            design.Invalidate();

            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  TEST BAŞARILI! ✅");
            Console.WriteLine("═══════════════════════════════════════════════════════");
        }

        /// <summary>
        /// Tüm testleri göster (menü için)
        /// </summary>
        public static void ShowTestMenu(Design design, Form parentForm)
        {
            string message = "CMM Probe Path Test Menüsü:\n\n" +
                           "1 - Basit 3 Nokta\n" +
                           "2 - Daire Ölçümü (8 nokta)\n" +
                           "3 - Grid Tarama (5x5)\n\n" +
                           "Hangi testi çalıştırmak istersiniz?";

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                message, 
                "CMM Probe Test", 
                "1", 
                -1, -1
            );

            switch (input)
            {
                case "1":
                    Test_SimpleThreePoints(design);
                    break;
                case "2":
                    Test_CircleMeasurement(design);
                    break;
                case "3":
                    Test_GridScan(design);
                    break;
                default:
                    MessageBox.Show("Geçersiz seçim!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }
    }
}

// ═══════════════════════════════════════════════════════════
// FORM1'E EKLENECEK KOD ÖRNEĞI
// ═══════════════════════════════════════════════════════════
/*
 * Form1.cs içinde bir menü item click event'ine şunu ekleyin:
 * 
 * private void testCMMProbePathToolStripMenuItem_Click(object sender, EventArgs e)
 * {
 *     // Test 1: Basit 3 nokta
 *     CMM_ProbePathTest.Test_SimpleThreePoints(design1);
 *     
 *     // VEYA Test 2: Daire ölçümü
 *     // CMM_ProbePathTest.Test_CircleMeasurement(design1);
 *     
 *     // VEYA Test 3: Grid tarama
 *     // CMM_ProbePathTest.Test_GridScan(design1);
 *     
 *     // VEYA Menü göster
 *     // CMM_ProbePathTest.ShowTestMenu(design1, this);
 * }
 */
