using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 2/3: Cylinder type detection (Hole vs Boss)
    /// </summary>
    public partial class CylindricalAnalyzer
    {
        // ═══════════════════════════════════════════════════════════
        // HOLE vs BOSS DETECTION
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// ✅ YENİ: EYESHOT YÖNTEMİ - Normal vektör yönüne göre HOLE/BOSS belirle
        /// </summary>
        private CylinderType DetermineCylinderType(Surface surface, CylindricalAxisInfo info, SimpleBoundingBox modelBox)
        {
            System.Diagnostics.Debug.WriteLine($"   🔍 EYESHOT Yöntemi ile tespit başlıyor...");
            System.Diagnostics.Debug.WriteLine($"      Axis: ({info.Axis.X:F3}, {info.Axis.Y:F3}, {info.Axis.Z:F3})");
            System.Diagnostics.Debug.WriteLine($"      Center: ({info.BottomCenter.X:F2}, {info.BottomCenter.Y:F2}, {info.BottomCenter.Z:F2})");
            System.Diagnostics.Debug.WriteLine($"      Radius: {info.Radius:F2} mm");

            try
            {
                // ✅ EYESHOT YÖNTEMİ: Normal vektör yönüne bak
                bool outwardNormal = HasOutwardNormal(surface, info.Axis, info.BottomCenter);

                if (outwardNormal)
                {
                    System.Diagnostics.Debug.WriteLine($"      ✅ SONUÇ: BOSS (çıkıntı)");
                    return CylinderType.Boss;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"      ✅ SONUÇ: HOLE (delik)");
                    return CylinderType.Hole;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"      ❌ Tespit hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"      ⚠️ Varsayılan: Unknown");
                return CylinderType.Unknown;
            }
        }

        /// <summary>
        /// ✅ EYESHOT YÖNTEMİ: Yüzeyin normal vektörü dışa mı içe mi bakıyor?
        /// Normal DIŞA bakıyorsa → true (BOSS)
        /// Normal İÇE bakıyorsa → false (HOLE)
        /// </summary>
        private bool HasOutwardNormal(Surface surface, Vector3D axis, Point3D center)
        {
            try
            {
                // 1. Yüzeyin bir noktasında normal vektörü hesapla
                Vector3D normal = surface.NormalAt(surface.DomainU.Low, surface.DomainV.Low);

                // 2. O noktanın koordinatını al
                Point3D pt = surface.PointAt(surface.DomainU.Low, surface.DomainV.Low);

                // 3. Bu noktayı eksen üzerine projekte et
                Point3D ptAlongAxis = pt.ProjectTo(new Segment3D(center, center + axis));

                // 4. Yarıçap vektörü hesapla (merkez → nokta)
                Vector3D radiusVec = new Vector3D(ptAlongAxis, pt);
                radiusVec.Normalize();

                // 5. Normal ile yarıçap vektörü ZIT yönde mi?
                if (Vector3D.AreOpposite(radiusVec, normal))
                {
                    System.Diagnostics.Debug.WriteLine("      ✅ Normal İÇE bakıyor → HOLE");
                    return false;  // İçe bakıyor → HOLE
                }

                System.Diagnostics.Debug.WriteLine("      ✅ Normal DIŞA bakıyor → BOSS");
                return true;  // Dışa bakıyor → BOSS
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"      ❌ HasOutwardNormal hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"      Stack: {ex.StackTrace}");

                // Hata durumunda varsayılan: BOSS
                return true;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Model bounding box hesapla
        /// </summary>
        private SimpleBoundingBox CalculateModelBoundingBox(List<Entity> entities)
        {
            var box = new SimpleBoundingBox();

            foreach (Entity entity in entities)
            {
                if (entity.BoxMin != null && entity.BoxMax != null)
                {
                    box.MinX = Math.Min(box.MinX, entity.BoxMin.X);
                    box.MinY = Math.Min(box.MinY, entity.BoxMin.Y);
                    box.MinZ = Math.Min(box.MinZ, entity.BoxMin.Z);

                    box.MaxX = Math.Max(box.MaxX, entity.BoxMax.X);
                    box.MaxY = Math.Max(box.MaxY, entity.BoxMax.Y);
                    box.MaxZ = Math.Max(box.MaxZ, entity.BoxMax.Z);
                }
            }

            System.Diagnostics.Debug.WriteLine($"📦 Model Bounding Box:");
            System.Diagnostics.Debug.WriteLine($"   Min: ({box.MinX:F2}, {box.MinY:F2}, {box.MinZ:F2})");
            System.Diagnostics.Debug.WriteLine($"   Max: ({box.MaxX:F2}, {box.MaxY:F2}, {box.MaxZ:F2})");

            return box;
        }

        /// <summary>
        /// Silindir ekseni bilgilerini hesapla
        /// ✅ ESKİ PLANE YÖNTEMİ - Reflection ile Surface.Plane property'lerini oku
        /// </summary>
        private CylindricalAxisInfo GetCylindricalAxisInfo(Surface surface)
        {
            try
            {
                var planeProperty = surface.GetType().GetProperty("Plane");
                if (planeProperty == null) return null;

                var plane = planeProperty.GetValue(surface);
                if (plane == null) return null;

                var originProperty = plane.GetType().GetProperty("Origin");
                var axisZProperty = plane.GetType().GetProperty("AxisZ");
                if (originProperty == null || axisZProperty == null) return null;

                Point3D origin = (Point3D)originProperty.GetValue(plane);
                Vector3D axis = (Vector3D)axisZProperty.GetValue(plane);
                if (axis == null) return null;

                axis.Normalize();

                var radiusProperty = surface.GetType().GetProperty("Radius");
                if (radiusProperty == null) return null;
                double radius = (double)radiusProperty.GetValue(surface);

                var domainVProperty = surface.GetType().GetProperty("DomainV");
                if (domainVProperty == null) return null;

                var domainV = domainVProperty.GetValue(surface);
                if (domainV == null) return null;

                var minProperty = domainV.GetType().GetProperty("Min");
                var maxProperty = domainV.GetType().GetProperty("Max");
                if (minProperty == null || maxProperty == null) return null;

                double minV = (double)minProperty.GetValue(domainV);
                double maxV = (double)maxProperty.GetValue(domainV);

                // Swap kontrolü
                if (minV > maxV)
                {
                    System.Diagnostics.Debug.WriteLine($"   🔄 Eksen ters! Min={minV:F2}, Max={maxV:F2} → Swap yapılıyor");
                    double temp = minV;
                    minV = maxV;
                    maxV = temp;
                }

                double height = Math.Abs(maxV - minV);

                Point3D bottomCenter = new Point3D(
                    origin.X + axis.X * minV,
                    origin.Y + axis.Y * minV,
                    origin.Z + axis.Z * minV
                );

                Point3D topCenter = new Point3D(
                    origin.X + axis.X * maxV,
                    origin.Y + axis.Y * maxV,
                    origin.Z + axis.Z * maxV
                );

                return new CylindricalAxisInfo
                {
                    BottomCenter = bottomCenter,
                    TopCenter = topCenter,
                    Axis = axis,
                    Radius = radius,
                    Height = height,
                    Type = CylinderType.Unknown
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"   ❌ GetCylindricalAxisInfo hatası: {ex.Message}");
                return null;
            }
        }
    }
}
