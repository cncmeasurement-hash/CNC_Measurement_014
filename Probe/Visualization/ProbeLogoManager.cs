using System;
using System.Drawing;
using _014.Probe.Configuration;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace _014.Probe.Visualization
{
    /// <summary>
    /// Prob gövdesine SADECE custom text ekleme (LOGO YOK!)
    /// Text silindire sarılır - ESKİ ÇALIŞAN VERSİYON
    /// </summary>
    public static class ProbeLogoManager
    {
        /// <summary>
        /// Sadece custom text ekler (Logo komple kaldırıldı)
        /// </summary>
        public static void AddLogoAndText(Design design, double cylinderRadius, double L1, double L2)
        {
            System.Diagnostics.Debug.WriteLine("🖼️ Logo ve Custom Text ekleniyor...");

            var settings = ProbeLogoStorage.LoadSettings();

            // ❌ LOGO TAMAMEN KALDIRILDI - Hız için!

            // ✅ SADECE Custom text varsa ekle
            if (settings.HasCustomText)
            {
                System.Diagnostics.Debug.WriteLine($"🔤 Custom text ekleniyor: \"{settings.CustomWebText}\"");
                AddCustomTextMesh(design, settings.CustomWebText, cylinderRadius, L1, L2);
            }

            System.Diagnostics.Debug.WriteLine($"✅ Logo ve Custom Text ekleme tamamlandı!");
        }

        /// <summary>
        /// Custom text'i mesh'e çevirip silindire sarar
        /// ESKİ ÇALIŞAN VERSİYON - AYNEN KORUNDU
        /// </summary>
        private static void AddCustomTextMesh(Design design, string customText, double cylinderRadius, double L1, double L2)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔤 Custom text ekleniyor: \"{customText}\"");

                // Text parametreleri - ESKİ ÇALIŞAN FORMÜLLER
                double textHeight = L2 / 30.0;           // Text yüksekliği
                double textZ = L1 + L2 / 7.5;          // Text Z pozisyonu

                // Text entity oluştur
                var textEntity = new Text(
                    Point3D.Origin,
                    customText,
                    textHeight
                );

                // Text hizalama ve döndürme - ESKİ ÇALIŞAN YÖNTemler
                textEntity.Alignment = Text.alignmentType.MiddleCenter;
                textEntity.Rotate(Math.PI / 2, Vector3D.AxisX, Point3D.Origin);
                textEntity.Translate(0, -cylinderRadius, textZ);

                // Text'i mesh'e çevir
                var textMeshes = textEntity.ConvertToMesh(design);

                if (textMeshes != null && textMeshes.Length > 0)
                {
                    foreach (var mesh in textMeshes)
                    {
                        // ✅ SİLİNDİRE SARMA - ESKİ ÇALIŞAN METOD
                        WrapMeshToCylinder(mesh, cylinderRadius);

                        // Renk ve stil
                        mesh.ColorMethod = colorMethodType.byEntity;
                        mesh.Color = Color.White;  // BEYAZ TEXT
                        mesh.EdgeStyle = Mesh.edgeStyleType.None;

                        // Sahneye ekle
                        design.Entities.Add(mesh);
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ Custom text eklendi: {textMeshes.Length} mesh");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Text mesh'e çevrilemedi");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Custom text hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Mesh'i silindir yüzeyine sarar
        /// ESKİ ÇALIŞAN FORMÜL - AYNEN KORUNDU
        /// </summary>
        private static void WrapMeshToCylinder(Mesh mesh, double cylinderRadius)
        {
            for (int i = 0; i < mesh.Vertices.Length; i++)
            {
                Point3D vertex = mesh.Vertices[i];

                double currentX = vertex.X;
                double currentY = vertex.Y;
                double currentZ = vertex.Z;

                // Merkeze olan mesafe
                double distanceFromCenter = Math.Sqrt(currentX * currentX + currentY * currentY);

                if (distanceFromCenter > 0.001)
                {
                    // Açı hesapla
                    double angle = Math.Atan2(currentY, currentX);

                    // Silindire sar
                    double newX = cylinderRadius * Math.Cos(angle);
                    double newY = cylinderRadius * Math.Sin(angle);

                    mesh.Vertices[i] = new Point3D(newX, newY, currentZ);
                }
            }
        }

        /// <summary>
        /// Logo ve Text'i kaldırır
        /// </summary>
        public static void RemoveLogoAndText(Design design)
        {
            try
            {
                // ProbeLogo layer'ındaki tüm entity'leri sil
                for (int i = design.Entities.Count - 1; i >= 0; i--)
                {
                    var entity = design.Entities[i];
                    if (entity.LayerName == "ProbeLogo")
                    {
                        design.Entities.RemoveAt(i);
                    }
                }

                System.Diagnostics.Debug.WriteLine("✅ Logo & Text kaldırıldı");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Logo & Text kaldırma hatası: {ex.Message}");
            }
        }
    }
}