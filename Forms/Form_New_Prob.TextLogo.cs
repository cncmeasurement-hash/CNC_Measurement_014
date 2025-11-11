using _014.Probe.Visualization;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;

namespace _014
{
    /// <summary>
    /// Form_New_Prob - TEXT & LOGO
    /// Partial class 4/4: Text oyma ve logo işlemleri
    /// </summary>
    public partial class Form_New_Prob
    {
        // ═══════════════════════════════════════════════════════════
        // TEXT ENGRAVING
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Gövdeye LOGO ve CUSTOM TEXT ekler - Password formundan ayarlar alınır
        /// ESKİ: CNC, Measurement, www.cncmeasurement.com (KALDIRILDI)
        /// YENİ: Logo (ortada) + Custom Text (altta)
        /// </summary>
        private Solid AddTextEngraving(Solid bodySolid, double d2, double L1, double L2)
        {
            if (bodySolid == null)
                return bodySolid;

            try
            {
                System.Diagnostics.Debug.WriteLine("🖼️ Logo ve Custom Text ekleniyor...");

                double cylinderRadius = d2 / 2.0;

                // ============================================
                // 🎨 LOGO ve CUSTOM TEXT EKLE (Password formundan)
                // ============================================
                ProbeLogoManager.AddLogoAndText(design_new_probe, cylinderRadius, L1, L2);

                System.Diagnostics.Debug.WriteLine("✅ Logo ve Custom Text ekleme tamamlandı!");

                return bodySolid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Logo/Text ekleme hatası: {ex.Message}");
            }

            return bodySolid;
        }

        // ═══════════════════════════════════════════════════════════
        // MESH WRAPPING HELPER
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mesh'i silindir yüzeyine sarar
        /// Text'in silindir etrafına yapışması için kullanılır
        /// </summary>
        private void WrapMeshToCylinder(Mesh mesh, double cylinderRadius)
        {
            for (int i = 0; i < mesh.Vertices.Length; i++)
            {
                Point3D vertex = mesh.Vertices[i];
                double currentX = vertex.X;
                double currentY = vertex.Y;
                double currentZ = vertex.Z;
                double distanceFromCenter = Math.Sqrt(currentX * currentX + currentY * currentY);

                if (distanceFromCenter > 0.001)
                {
                    double angle = Math.Atan2(currentY, currentX);
                    double newX = cylinderRadius * Math.Cos(angle);
                    double newY = cylinderRadius * Math.Sin(angle);
                    mesh.Vertices[i] = new Point3D(newX, newY, currentZ);
                }
            }
        }
    }
}
