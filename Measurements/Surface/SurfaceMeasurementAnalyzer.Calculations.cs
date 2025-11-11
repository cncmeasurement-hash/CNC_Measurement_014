using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 3/3: Ölçüm hesaplamaları ve rapor gösterimi
    /// </summary>
    public partial class SurfaceMeasurementAnalyzer
    {
        // ═══════════════════════════════════════════════════════════
        // CYLINDRICAL SURFACE MEASUREMENTS
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// ✅ Silindirik yüzey ölçümü
        /// </summary>
        private void MeasureCylindricalSurface(Surface surface)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📐 Silindirik yüzey ölçülüyor...");

                // ✅ TÜM PROPERTY'LERİ LİSTELE (DEBUG)
                System.Diagnostics.Debug.WriteLine("🔍 Surface property'leri:");
                foreach (var prop in surface.GetType().GetProperties())
                {
                    System.Diagnostics.Debug.WriteLine($"   • {prop.Name} ({prop.PropertyType.Name})");
                }

                // ✅ MESH'TEN ÇAP HESAPLA (EN GÜVENLİ YÖNTEM)
                double diameter = CalculateCylinderDiameterFromMesh(surface);

                // ✅ MERKEZ NOKTA (BoundingBox'tan)
                Point3D center = surface.BoxMin + (surface.BoxMax - surface.BoxMin) * 0.5;

                System.Diagnostics.Debug.WriteLine($"   📏 Çap (mesh'ten): {diameter:F2} mm");
                System.Diagnostics.Debug.WriteLine($"   📍 Merkez: ({center.X:F2}, {center.Y:F2}, {center.Z:F2})");

                // ✅ Yüzey alanı hesapla
                double surfaceArea = CalculateSurfaceArea(surface);

                System.Diagnostics.Debug.WriteLine($"   📐 Yüzey alanı: {surfaceArea:F2} mm²");

                // ✅ Rapor oluştur ve göster
                ShowCylindricalReport(diameter, diameter, center, surfaceArea);

                System.Diagnostics.Debug.WriteLine("✅ Silindirik yüzey ölçümü tamamlandı!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Silindirik yüzey ölçüm hatası: {ex.Message}");

                MessageBox.Show(
                    $"❌ Ölçüm hatası:\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// ✅ Mesh'ten silindir çapı hesapla (EN GÜVENLİ YÖNTEM)
        /// </summary>
        private double CalculateCylinderDiameterFromMesh(Surface surface)
        {
            try
            {
                // Surface'i mesh'e çevir
                Mesh mesh = surface.ConvertToMesh(0.1);

                if (mesh == null || mesh.Vertices == null || mesh.Vertices.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Mesh oluşturulamadı!");
                    return 0;
                }

                // ✅ Tüm vertex'lerin merkeze uzaklıklarını hesapla
                double minRadius = double.MaxValue;
                double maxRadius = 0;

                // Merkez nokta
                Point3D center = surface.BoxMin + (surface.BoxMax - surface.BoxMin) * 0.5;

                foreach (var vertex in mesh.Vertices)
                {
                    // XY düzleminde merkeze uzaklık (Z hariç)
                    double dx = vertex.X - center.X;
                    double dy = vertex.Y - center.Y;
                    double distanceFromAxis = Math.Sqrt(dx * dx + dy * dy);

                    if (distanceFromAxis > maxRadius)
                        maxRadius = distanceFromAxis;

                    if (distanceFromAxis < minRadius && distanceFromAxis > 0.001)
                        minRadius = distanceFromAxis;
                }

                // Ortalama yarıçap
                double averageRadius = (minRadius + maxRadius) / 2.0;
                double diameter = averageRadius * 2.0;

                System.Diagnostics.Debug.WriteLine($"   🔍 Min Yarıçap: {minRadius:F2} mm");
                System.Diagnostics.Debug.WriteLine($"   🔍 Max Yarıçap: {maxRadius:F2} mm");
                System.Diagnostics.Debug.WriteLine($"   🔍 Ortalama Yarıçap: {averageRadius:F2} mm");

                return diameter;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Mesh'ten çap hesaplama hatası: {ex.Message}");
                return 0;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // CONICAL SURFACE MEASUREMENTS
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// ✅ Konik yüzey ölçümü
        /// </summary>

        // ═══════════════════════════════════════════════════════════
        // SURFACE AREA CALCULATION
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// ✅ Yüzey alanı hesapla (Mesh'e çevirip)
        /// </summary>
        private double CalculateSurfaceArea(Surface surface)
        {
            try
            {
                // Surface'i mesh'e çevir
                Mesh mesh = surface.ConvertToMesh(0.1);

                if (mesh == null || mesh.Triangles == null || mesh.Triangles.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Mesh oluşturulamadı!");
                    return 0;
                }

                double totalArea = 0;

                // Her triangle'ın alanını topla
                foreach (var triangle in mesh.Triangles)
                {
                    Point3D p1 = mesh.Vertices[triangle.V1];
                    Point3D p2 = mesh.Vertices[triangle.V2];
                    Point3D p3 = mesh.Vertices[triangle.V3];

                    // Triangle alanı: 0.5 * |cross product|
                    Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
                    Vector3D v2 = new Vector3D(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);

                    Vector3D cross = Vector3D.Cross(v1, v2);
                    double area = cross.Length / 2.0;

                    totalArea += area;
                }

                return totalArea;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Yüzey alanı hesaplama hatası: {ex.Message}");
                return 0;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // REPORT DISPLAY
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// ✅ Silindirik yüzey raporu göster (FORM'DA)
        /// </summary>
        private void ShowCylindricalReport(double minDiameter, double maxDiameter, Point3D center, double surfaceArea)
        {
            // Debug log
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("    📏 SİLİNDİRİK YÜZEY ÖLÇÜMÜ");
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine("📊 ÇAPLAR:");
            sb.AppendLine($"   • Çap: {minDiameter:F2} mm");
            sb.AppendLine();
            sb.AppendLine("📍 MERKEZ KOORDİNATLARI:");
            sb.AppendLine($"   • X: {center.X:F2} mm");
            sb.AppendLine($"   • Y: {center.Y:F2} mm");
            sb.AppendLine($"   • Z: {center.Z:F2} mm");
            sb.AppendLine();
            sb.AppendLine("📐 YÜZEY ALANI:");
            sb.AppendLine($"   • Alan: {surfaceArea:F2} mm²");
            sb.AppendLine();
            sb.AppendLine("🔵 Tip: Silindirik");
            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════");

            System.Diagnostics.Debug.WriteLine(sb.ToString());
        }

    }
}
