using System;
using System.Collections.Generic;
using System.Text;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System.Drawing;

namespace _014.Analyzers.SurfaceAnal
{
    /// <summary>
    /// ✅ NURBS YÜZEY ANALİZCİSİ
    /// SARI RENKLİ SERBEST FORM YÜZEYLERİN NORMAL VEKTÖRLERİNİ HESAPLAR
    /// Her tıklanan noktada normal vektörünü gösterir
    /// </summary>
    public class NurbsSurfaceAnalyzer
    {
        // ═══════════════════════════════════════════════════════════
        // TRIANGLE YÜZEY BİLGİSİ SINIFI
        // ═══════════════════════════════════════════════════════════
        public class TriangleFaceInfo
        {
            public Point3D P1 { get; set; }         // 1. köşe
            public Point3D P2 { get; set; }         // 2. köşe
            public Point3D P3 { get; set; }         // 3. köşe
            public Vector3D Normal { get; set; }    // Normal vektör
            public Point3D Center { get; set; }     // Merkez nokta
        }

        // ═══════════════════════════════════════════════════════════
        // TRİANGLE'DAN YÜZEY BİLGİSİNİ AL
        // ═══════════════════════════════════════════════════════════
        public static TriangleFaceInfo GetTriangleFaceInfo(
            Mesh mesh,
            int triangleIndex,
            Point3D clickedPoint)
        {
            try
            {
                if (mesh == null || mesh.Triangles == null ||
                    triangleIndex < 0 || triangleIndex >= mesh.Triangles.Length)
                {
                    return null;
                }

                // Triangle'ın köşelerini al
                IndexTriangle tri = mesh.Triangles[triangleIndex];

                Point3D p1 = mesh.Vertices[tri.V1];
                Point3D p2 = mesh.Vertices[tri.V2];
                Point3D p3 = mesh.Vertices[tri.V3];

                // Normal hesapla (3 köşeden)
                Vector3D normal = CalculateNormal(p1, p2, p3);

                // Merkez hesapla
                Point3D center = new Point3D(
                    (p1.X + p2.X + p3.X) / 3.0,
                    (p1.Y + p2.Y + p3.Y) / 3.0,
                    (p1.Z + p2.Z + p3.Z) / 3.0
                );

                return new TriangleFaceInfo
                {
                    P1 = p1,
                    P2 = p2,
                    P3 = p3,
                    Normal = normal,
                    Center = center
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetTriangleFaceInfo hatası: {ex.Message}");
                return null;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // NORMAL HESAPLA (3 NOKTADAN) - CROSS PRODUCT
        // ═══════════════════════════════════════════════════════════
        public static Vector3D CalculateNormal(Point3D p1, Point3D p2, Point3D p3)
        {
            try
            {
                // İki kenar vektörü
                Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
                Vector3D v2 = new Vector3D(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);

                // Cross product = Normal
                Vector3D normal = Vector3D.Cross(v1, v2);

                // Normalize et
                normal.Normalize();

                return normal;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CalculateNormal hatası: {ex.Message}");
                return new Vector3D(0, 0, 1); // Varsayılan Z yukarı
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MARKER VE NORMAL ÇİZGİSİ OLUŞTUR
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Tıklanan noktada mor marker + sarı normal çizgisi oluşturur
        /// </summary>
        /// <param name="clickedPoint">Tıklanan 3D nokta</param>
        /// <param name="normal">Normal vektör</param>
        /// <param name="lineLength">Normal çizgisi uzunluğu (mm)</param>
        /// <param name="markerSize">Marker boyutu (mm)</param>
        /// <returns>Entity listesi (marker + çizgi + ok)</returns>
        public static List<Entity> CreateMarkerAndNormalLine(
            Point3D clickedPoint,
            Vector3D normal,
            double lineLength = 30.0,
            double markerSize = 3.0)
        {
            List<Entity> entities = new List<Entity>();

            try
            {
                // 🟣 MOR MARKER (küre)
                Mesh markerSphere = Mesh.CreateSphere(markerSize, 20, 20);

                // ✅ Normal vektör yönünde 3mm kaydir
                Point3D offsetPoint = new Point3D(
                    clickedPoint.X + normal.X * 3.0,
                    clickedPoint.Y + normal.Y * 3.0,
                    clickedPoint.Z + normal.Z * 3.0
                );
                markerSphere.Translate(offsetPoint.X, offsetPoint.Y, offsetPoint.Z);
                markerSphere.Color = Color.Purple;
                markerSphere.ColorMethod = colorMethodType.byEntity;
                markerSphere.Selectable = false;
                // LayerName kaldırıldı - varsayılan layer kullanılacak

                entities.Add(markerSphere);


                // ═══════════════════════════════════════════════════════════
                // ✅ YENİ: 1. NORMAL YÖNÜNDE 10MM ÇİZGİ (MAVİ)
                // ═══════════════════════════════════════════════════════════

                Point3D normalEnd10mm = new Point3D(
                    clickedPoint.X + normal.X * 10.0,
                    clickedPoint.Y + normal.Y * 10.0,
                    clickedPoint.Z + normal.Z * 10.0
                );

                Line normalLine10mm = new Line(clickedPoint, normalEnd10mm);
                normalLine10mm.Color = Color.Blue;  // Mavi renk
                normalLine10mm.ColorMethod = colorMethodType.byEntity;
                normalLine10mm.LineWeight = 3;
                normalLine10mm.Selectable = false;

                entities.Add(normalLine10mm);

                // ═══════════════════════════════════════════════════════════
                // ✅ YENİ: 2. +Z YÖNÜNDE 100MM ÇİZGİ (KIRMIZI)
                // ═══════════════════════════════════════════════════════════

                // 10mm çizginin bittiği noktadan başlayarak +Z yönünde 100mm
                Point3D zEnd100mm = new Point3D(
                    normalEnd10mm.X,          // X değişmez
                    normalEnd10mm.Y,          // Y değişmez
                    normalEnd10mm.Z + 100.0   // Z yönünde +100mm
                );

                Line zLine100mm = new Line(normalEnd10mm, zEnd100mm);
                zLine100mm.Color = Color.Red;  // Kırmızı renk
                zLine100mm.ColorMethod = colorMethodType.byEntity;
                zLine100mm.LineWeight = 3;
                zLine100mm.Selectable = false;

                entities.Add(zLine100mm);


                System.Diagnostics.Debug.WriteLine($"✅ Marker + 10mm Normal + 100mm Z oluşturuldu: {entities.Count} entity");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateMarkerAndNormalLine hatası: {ex.Message}");
            }

            return entities;
        }

        // ═══════════════════════════════════════════════════════════
        // SURFACE TİPİNİ KONTROL ET
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Entity NURBS veya herhangi bir Surface mi kontrol eder
        /// ✅ TÜM Surface tipleri kabul edilir (test için)
        /// </summary>
        public static bool IsNurbsOrFreeformSurface(Entity entity)
        {
            try
            {
                // Surface veya Mesh ise kabul et
                if (entity is Surface || entity is Mesh)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ IsNurbsOrFreeformSurface hatası: {ex.Message}");
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // NORMAL BİLGİLERİNİ METNE ÇEVİR
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Normal bilgilerini Debug output için metin oluşturur
        /// </summary>
        public static string GetNormalInfoText(TriangleFaceInfo faceInfo)
        {
            if (faceInfo == null) return "Bilgi yok";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"📐 Normal Vektör:");
            sb.AppendLine($"   X: {faceInfo.Normal.X:F3}");
            sb.AppendLine($"   Y: {faceInfo.Normal.Y:F3}");
            sb.AppendLine($"   Z: {faceInfo.Normal.Z:F3}");
            sb.AppendLine();
            sb.AppendLine($"📍 Merkez:");
            sb.AppendLine($"   ({faceInfo.Center.X:F2}, {faceInfo.Center.Y:F2}, {faceInfo.Center.Z:F2})");

            return sb.ToString();
        }
    }
}