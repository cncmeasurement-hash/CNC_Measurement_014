using _014.Analyzers.Data;
using _014.Managers.Data;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace _014
{
    /// <summary>
    /// Yüzey analizi ve normal hesaplama
    /// ✅ V5: Ok renkleri düzeltildi + Yeşil boyama düzeltildi
    /// </summary>
    public partial class SurfaceAnalyzer
    {
        private Design design;
        private DataManager dataManager;
        
        public SurfaceAnalyzer(Design design, DataManager dataManager)
        {
            this.design = design;
            this.dataManager = dataManager;
        }
        public void ShowSelectedFaceNormals()
        {
            System.Diagnostics.Debug.WriteLine("🔵 ShowSelectedFaceNormals() - RENKLİ OKLAR + YEŞİL YÜZEY");

            if (design.Entities == null || design.Entities.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Hiç entity yok!", "Uyarı");
                return;
            }

            dataManager.ClearSurfaceData();

            int normalCount = 0;
            int faceCount = 0;
            int globalSurfaceIndex = 0;

            var surfacesList = new List<object>();

            for (int entityIdx = 0; entityIdx < design.Entities.Count; entityIdx++)
            {
                var entity = design.Entities[entityIdx];

                if (entity is Brep brep && brep.Faces != null)
                {
                    System.Diagnostics.Debug.WriteLine($"📦 Brep[{entityIdx}]: {brep.Faces.Length} face");

                    for (int faceIdx = 0; faceIdx < brep.Faces.Length; faceIdx++)
                    {
                        try
                        {
                            var face = brep.Faces[faceIdx];
                            faceCount++;

                            Mesh faceMesh = face.ConvertToMesh();

                            if (faceMesh == null || faceMesh.Vertices == null || faceMesh.Vertices.Length == 0)
                            {
                                continue;
                            }

                            Point3D center = new Point3D(0, 0, 0);
                            foreach (var v in faceMesh.Vertices)
                            {
                                center.X += v.X;
                                center.Y += v.Y;
                                center.Z += v.Z;
                            }
                            center.X /= faceMesh.Vertices.Length;
                            center.Y /= faceMesh.Vertices.Length;
                            center.Z /= faceMesh.Vertices.Length;

                            if (faceMesh.Triangles != null && faceMesh.Triangles.Length > 0)
                            {
                                var tri = faceMesh.Triangles[0];
                                Point3D v0 = faceMesh.Vertices[tri.V1];
                                Point3D v1 = faceMesh.Vertices[tri.V2];
                                Point3D v2 = faceMesh.Vertices[tri.V3];

                                Vector3D edge1 = new Vector3D(v1.X - v0.X, v1.Y - v0.Y, v1.Z - v0.Z);
                                Vector3D edge2 = new Vector3D(v2.X - v0.X, v2.Y - v0.Y, v2.Z - v0.Z);
                                Vector3D normal = Vector3D.Cross(edge1, edge2);
                                normal.Normalize();

                                string surfaceType = DetermineSurfaceType(normal);
                                string group = DetermineGroup(surfaceType);
                                bool isSelectable = (surfaceType != "BOTTOM (Z-)");

                                surfacesList.Add(new
                                {
                                    index = globalSurfaceIndex,
                                    name = $"Surface_{globalSurfaceIndex}",
                                    type = surfaceType,
                                    group = group,
                                    entityIndex = entityIdx,
                                    faceIndex = faceIdx,
                                    normal = new
                                    {
                                        x = Math.Round(normal.X, 6),
                                        y = Math.Round(normal.Y, 6),
                                        z = Math.Round(normal.Z, 6)
                                    },
                                    center = new
                                    {
                                        x = Math.Round(center.X, 3),
                                        y = Math.Round(center.Y, 3),
                                        z = Math.Round(center.Z, 3)
                                    }
                                });

                                dataManager.AddSurfaceData(new SurfaceData
                                {
                                    Index = globalSurfaceIndex,
                                    Name = $"Surface_{globalSurfaceIndex}",
                                    EntityIndex = entityIdx,
                                    FaceIndex = faceIdx,
                                    Normal = normal,
                                    Center = center,
                                    SurfaceType = surfaceType,
                                    Group = group,
                                    IsLabelVisible = true,
                                    IsArrowVisible = true,
                                    IsSelectable = isSelectable
                                });

                                // ✅ OK ÇİZ - RENKLİ!
                                double arrowLength = 50.0;
                                Point3D arrowEnd = new Point3D(
                                    center.X + normal.X * arrowLength,
                                    center.Y + normal.Y * arrowLength,
                                    center.Z + normal.Z * arrowLength
                                );

                                Line normalArrow = new Line(center, arrowEnd);
                                
                                // ✅ RENK - GRUBA GÖRE
                                if (group == "Alt Yüzey")
                                {
                                    normalArrow.Color = Color.Red; // 🔴 KIRMIZI
                                }
                                else if (group == "Dik")
                                {
                                    normalArrow.Color = Color.Yellow; // 🟡 SARI
                                }
                                else // Eğik
                                {
                                    normalArrow.Color = Color.Blue; // 🔵 MAVİ
                                }

                                normalArrow.LineWeight = 3.0f;
                                normalArrow.ColorMethod = colorMethodType.byEntity; // ✅ ÖNEMLİ!
                                normalArrow.EntityData = $"FACE_NORMAL_{globalSurfaceIndex}";

                                design.Entities.Add(normalArrow);
                                normalCount++;

                                // ✅ ETİKET EKLE
                                AddSurfaceLabel(center, normal, globalSurfaceIndex, surfaceType, group);

                                // ✅ Z- YÜZEYLERİ YEŞİL BOYA
                                if (group == "Alt Yüzey")
                                {
                                    PaintFaceGreen(entityIdx, faceIdx);
                                }

                                globalSurfaceIndex++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"  ❌ Face[{faceIdx}] Hata: {ex.Message}");
                        }
                    }
                }
            }

            design.Invalidate();
            dataManager.SaveToJson(surfacesList);

            System.Diagnostics.Debug.WriteLine($"📊 SONUÇ: {normalCount} renkli ok + etiket");

            if (normalCount > 0)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"✅ {normalCount} yüzey etiketlendi!\n\n" +
                    $"🎨 Renk Sistemi:\n" +
                    $"  • 🟢 YEŞİL Yüzey: Alt yüzeyler (Z-)\n" +
                    $"  • 🔴 KIRMIZI Ok: Alt yüzeyler\n" +
                    $"  • 🟡 SARI Ok: Dik yüzeyler\n" +
                    $"  • 🔵 MAVİ Ok: Eğik yüzeyler\n\n" +
                    $"Toplam {faceCount} yüzey bulundu.",
                    "Başarılı",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information
                );
            }
        }

        /// <summary>
        /// ✅ DÜZELTME: Yüzeyi yeşil renge boyar
        /// </summary>
        private void PaintFaceGreen(int entityIndex, int faceIndex)
        {
            try
            {
                var entity = design.Entities[entityIndex];
                
                if (entity is Brep brep && brep.Faces != null && faceIndex < brep.Faces.Length)
                {
                    var face = brep.Faces[faceIndex];
                    var mesh = face.ConvertToMesh();
                    
                    if (mesh != null)
                    {
                        mesh.Color = Color.Lime; // 🟢 PARLAK YEŞİL
                        mesh.ColorMethod = colorMethodType.byEntity;
                        mesh.EntityData = $"GREEN_FACE_{entityIndex}_{faceIndex}";
                        
                        design.Entities.Add(mesh);
                        
                        System.Diagnostics.Debug.WriteLine($"  🟢 Yüzey yeşile boyandı: Entity[{entityIndex}] Face[{faceIndex}]");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"  ❌ Yeşil boyama hatası: {ex.Message}");
            }
        }

        private string DetermineSurfaceType(Vector3D normal)
        {
            double threshold = 0.9;

            if (normal.Z > threshold)
                return "TOP (Z+)";
            if (normal.Z < -threshold)
                return "BOTTOM (Z-)";
            if (normal.X > threshold)
                return "RIGHT (X+)";
            if (normal.X < -threshold)
                return "LEFT (X-)";
            if (normal.Y > threshold)
                return "FRONT (Y+)";
            if (normal.Y < -threshold)
                return "BACK (Y-)";

            return "INCLINED";
        }

        /// <summary>
        /// ✅ GÜNCELLEME: Sadece 2 grup - Dik ve Eğik
        /// </summary>
        private string DetermineGroup(string surfaceType)
        {
            switch (surfaceType)
            {
                case "BOTTOM (Z-)":
                    return "Alt Yüzey"; // İçerde kullanım için
                
                case "TOP (Z+)":
                case "RIGHT (X+)":
                case "LEFT (X-)":
                case "FRONT (Y+)":
                case "BACK (Y-)":
                    return "Dik"; // ✅ KULLANICIYA "DİK" GÖSTER
                
                case "INCLINED":
                default:
                    return "Eğik"; // ✅ KULLANICIYA "EĞİK" GÖSTER
            }
        }


        /// <summary>
        /// ✅ YENİ: Sadece Planar (Düzlemsel) Surface'leri analiz eder
        /// Brep'e ihtiyaç duymaz, direkt Surface'lerle çalışır
        /// </summary>
        /// <summary>
        /// Planar Surface'leri analiz eder ve JSON'a kaydeder
        /// </summary>
        /// <param name="stepFileName">STEP dosya adı (uzantısız, opsiyonel)</param>
        public void AnalyzePlanarSurfaces(string stepFileName = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🔄 PLANAR SURFACE ANALİZİ BAŞLIYOR...");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                
                dataManager.ClearSurfaceData();
                
                int totalSurfaces = 0;
                int planarCount = 0;
                int globalSurfaceIndex = 0;
                var surfacesList = new List<object>();
                
                // 1. Tüm Surface'leri tara
                for (int entityIdx = 0; entityIdx < design.Entities.Count; entityIdx++)
                {
                    var entity = design.Entities[entityIdx];
                    
                    if (entity is Surface surface)
                    {
                        totalSurfaces++;
                        
                        // 2. SADECE Planar olanları işle
                        if (surface is PlanarSurface)
                        {
                            planarCount++;
                            System.Diagnostics.Debug.WriteLine($"📦 Planar Surface[{entityIdx}]");
                            
                            try
                            {
                                // 3. Mesh'e çevir (Normal hesabı için)
                                Mesh surfaceMesh = surface.ConvertToMesh();
                                
                                if (surfaceMesh == null || surfaceMesh.Vertices == null || surfaceMesh.Vertices.Length == 0)
                                    continue;
                                
                                // 4. Merkez nokta hesapla
                                Point3D center = new Point3D(0, 0, 0);
                                foreach (var v in surfaceMesh.Vertices)
                                {
                                    center.X += v.X;
                                    center.Y += v.Y;
                                    center.Z += v.Z;
                                }
                                center.X /= surfaceMesh.Vertices.Length;
                                center.Y /= surfaceMesh.Vertices.Length;
                                center.Z /= surfaceMesh.Vertices.Length;
                                
                                // 5. Normal hesapla (İlk triangle'dan)
                                Vector3D normal = new Vector3D(0, 0, 1); // Default
                                
                                if (surfaceMesh.Triangles != null && surfaceMesh.Triangles.Length > 0)
                                {
                                    var tri = surfaceMesh.Triangles[0];
                                    Point3D v0 = surfaceMesh.Vertices[tri.V1];
                                    Point3D v1 = surfaceMesh.Vertices[tri.V2];
                                    Point3D v2 = surfaceMesh.Vertices[tri.V3];
                                    
                                    Vector3D edge1 = new Vector3D(v1.X - v0.X, v1.Y - v0.Y, v1.Z - v0.Z);
                                    Vector3D edge2 = new Vector3D(v2.X - v0.X, v2.Y - v0.Y, v2.Z - v0.Z);
                                    normal = Vector3D.Cross(edge1, edge2);
                                    normal.Normalize();
                                }
                                
                                // 6. SurfaceType ve Group belirle
                                string surfType = DetermineSurfaceType(normal);
                                string group = DetermineGroup(surfType);
                                bool isSelectable = (surfType != "BOTTOM (Z-)");
                                
                                // 7. Alan hesapla
                                double area = 0;
                                try
                                {
                                    Point3D centroid;
                                    area = surface.GetArea(out centroid);
                                }
                                catch { area = 0; }
                                
                                // 8. BoundingBox
                                Point3D boxMin = surface.BoxMin;
                                Point3D boxMax = surface.BoxMax;
                                
                                // 9. JSON için data
                                surfacesList.Add(new
                                {
                                    index = globalSurfaceIndex,
                                    name = $"Surface_{globalSurfaceIndex}",
                                    type = surfType,
                                    group = group,
                                    geometryType = "Planar",
                                    entityIndex = entityIdx,
                                    faceIndex = 0,
                                    area = Math.Round(area, 2),
                                    normal = new { x = Math.Round(normal.X, 3), y = Math.Round(normal.Y, 3), z = Math.Round(normal.Z, 3) },
                                    center = new { x = Math.Round(center.X, 2), y = Math.Round(center.Y, 2), z = Math.Round(center.Z, 2) },
                                    boundingBox = new
                                    {
                                        min = new { x = Math.Round(boxMin.X, 2), y = Math.Round(boxMin.Y, 2), z = Math.Round(boxMin.Z, 2) },
                                        max = new { x = Math.Round(boxMax.X, 2), y = Math.Round(boxMax.Y, 2), z = Math.Round(boxMax.Z, 2) }
                                    }
                                });
                                
                                // 10. DataManager'a ekle
                                dataManager.AddSurfaceData(new SurfaceData
                                {
                                    Index = globalSurfaceIndex,
                                    Name = $"Surface_{globalSurfaceIndex}",
                                    EntityIndex = entityIdx,
                                    FaceIndex = 0,
                                    Normal = normal,
                                    Center = center,
                                    SurfaceType = surfType,
                                    Group = group,
                                    IsLabelVisible = true,
                                    IsArrowVisible = true,
                                    IsSelectable = isSelectable
                                });
                                
                                // 11. Debug output
                                System.Diagnostics.Debug.WriteLine($"  ✅ {surfType} - {group}");
                                System.Diagnostics.Debug.WriteLine($"     Area: {area:F2} mm²");
                                System.Diagnostics.Debug.WriteLine($"     Normal: ({normal.X:F3}, {normal.Y:F3}, {normal.Z:F3})");
                                System.Diagnostics.Debug.WriteLine($"     Center: ({center.X:F2}, {center.Y:F2}, {center.Z:F2})");
                                
                                globalSurfaceIndex++;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"  ❌ Surface[{entityIdx}] Hata: {ex.Message}");
                            }
                        }
                    }
                }
                
                // 12. JSON'a kaydet
                dataManager.SaveToJson(surfacesList, stepFileName);
                
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("📊 PLANAR ANALİZ SONUCU");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine($"📊 Toplam Surface: {totalSurfaces}");
                System.Diagnostics.Debug.WriteLine($"📊 Planar Surface: {planarCount}");
                System.Diagnostics.Debug.WriteLine($"✅ Analiz edilen: {globalSurfaceIndex}");
                System.Diagnostics.Debug.WriteLine($"💾 JSON kaydedildi: Desktop/surface_normals_*.json");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AnalyzePlanarSurfaces hatası: {ex.Message}");
            }
        }
    }
}
