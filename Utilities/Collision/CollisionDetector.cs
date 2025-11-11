using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace _014.Utilities.Collision
{
    /// <summary>
    /// BOŞ CollisionDetector
    /// Sadece probe'u klonlayıp X=0, Y=0, Z=0'a yerleştirir
    /// </summary>
    public class CollisionDetector
    {
        private Design design;
        private ImportToMeshForCollision meshConverter;
        
        public CollisionDetector(Design design, ImportToMeshForCollision meshConverter)
        {
            this.design = design;
            this.meshConverter = meshConverter;
        }
        
        /// <summary>
        /// Probe'u klonla ve X=0, Y=0, Z=0'a yerleştir
        /// Her zaman false döner (çarpışma kontrolü yok)
        /// </summary>
        public (bool collision, Mesh displayedProbe) CheckCollisionAtPoint(
            Mesh probeMesh,
            Point3D contactPoint,
            Vector3D normal,
            double probeDiameter,
            double retractDistance,
            double zSafetyDistance,
            bool showVisuals)
        {
            try
            {
                Console.WriteLine("🔵 CheckCollisionAtPoint ÇAĞRILDI!");
                
                if (probeMesh == null)
                {
                    Console.WriteLine("❌ probeMesh NULL!");
                    return (false, null);
                }
                
                // 1. Probe'u klonla
                Mesh clonedProbe = (Mesh)probeMesh.Clone();
                
                // 2. Tıklanan noktaya taşı + Z'de -D/2
                double R = probeDiameter / 2.0;  // Yarıçap
                clonedProbe.Translate(
                    contactPoint.X, 
                    contactPoint.Y, 
                    contactPoint.Z - R  // Z ekseninde -D/2 aşağı
                );
                
                // 3. Normal yönünde D/2 kadar kaydır (BİRİNCİ İŞLEM)
                clonedProbe.Translate(
                    normal.X * R,
                    normal.Y * R,
                    normal.Z * R
                );
                
                // 4. Normal yönünde D*0.1 kadar kaydır (İKİNCİ İŞLEM)
                double offset = probeDiameter * 0.1;
                clonedProbe.Translate(
                    normal.X * offset,
                    normal.Y * offset,
                    normal.Z * offset
                );
                
                // 5. Yeşil probe'u ekrana ekle
                clonedProbe.Visible = false;  // ✅ ÖNCE GÖRÜNMEZ YAP (FLASH ÖNLEME!)
                clonedProbe.Color = Color.Lime;
                clonedProbe.ColorMethod = colorMethodType.byEntity;
                clonedProbe.LayerName = "ProbePoints";
                design.Entities.Add(clonedProbe);
                design.Invalidate();
                Console.WriteLine("✅ Yeşil probe ekrana eklendi!");
                
                // 6. Part mesh'lerini al
                List<Mesh> partMeshes = meshConverter.GetMeshesForCollision();
                
                // 7. Renk dizisi
                Color[] colors = new Color[]
                {
                    Color.Blue,
                    Color.Red,
                    Color.Yellow,
                    Color.Magenta,
                    Color.Cyan,
                    Color.Orange,
                    Color.Pink,
                    Color.Brown,
                    Color.Purple,
                    Color.Gold
                };
                
                // 8. DÖNGÜ: 1mm kaydır + çarpışma kontrolü (retractDistance kadar)
                int stepCount = (int)retractDistance;  // Kaç adım yapılacak
                for (int i = 0; i < stepCount; i++)
                {
                    // 1mm kaydır
                    clonedProbe.Translate(
                        normal.X * 1.0,
                        normal.Y * 1.0,
                        normal.Z * 1.0
                    );
                    
                    // Renk değiştir (renk dizisi sınırını aşmamak için modulo kullan)
                    clonedProbe.Color = colors[i % colors.Length];
                    design.Invalidate();
                    Console.WriteLine($"✅ Probe {i+1}mm kaydırıldı (Toplam: {i+1}mm / {stepCount}mm)");
                    
                    // Çarpışma kontrolü
                    foreach (Mesh partMesh in partMeshes)
                    {
                        try
                        {
                            // Mesh kontrolü
                            if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                continue;
                            
                            if (clonedProbe.Vertices == null || clonedProbe.Vertices.Length == 0)
                                continue;
                            
                            CollisionDetection cd = new CollisionDetection(
                                new Entity[] { clonedProbe },
                                new Entity[] { partMesh },
                                null
                            );
                            
                            cd.CheckMethod = collisionCheckType.SubdivisionTree;
                            cd.DoWork();
                            
                            if (cd.Result != null && cd.Result.Length > 0)
                            {
                                Console.WriteLine($"💥 ÇARPIŞMA TESPİT EDİLDİ! ({i+1}mm konumunda)");
                                return (true, clonedProbe);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Mesh collision hatası: {ex.Message}");
                        }
                    }
                }
                
                // 9. Z EKSENİNDE DÖNGÜ: zSafetyDistance + L1'e kadar çık (Probe tamamen yukarıda)
                Color[] zColors = new Color[] 
                { 
                    Color.White, 
                    Color.LightGray, 
                    Color.Silver 
                };
                
                // Probe'un toplam uzunluğunu hesapla (L1)
                double probeHeight = clonedProbe.BoxMax.Z - clonedProbe.BoxMin.Z;
                
                // Probe'un mevcut Z pozisyonunu al
                double currentZ = clonedProbe.Vertices[0].Z;
                
                // Hedef Z = Güvenlik mesafesi + Probe uzunluğu
                double targetZ = zSafetyDistance + probeHeight;
                
                // Güvenlik mesafesine kadar kalan mesafeyi hesapla
                double remainingZ = targetZ - currentZ;
                
                // Kaç adım gerekli? (50mm adımlarla)
                int zStepCount = (int)Math.Ceiling(remainingZ / 50.0);
                
                Console.WriteLine($"📊 Probe mevcut Z: {currentZ:F2}mm, Probe uzunluğu: {probeHeight:F2}mm");
                Console.WriteLine($"📊 Hedef Z: {zSafetyDistance}mm + {probeHeight:F2}mm = {targetZ:F2}mm, Kalan: {remainingZ:F2}mm, Adım: {zStepCount}");
                
                for (int z = 0; z < zStepCount; z++)
                {
                    // Son adım mı?
                    bool isLastStep = z == zStepCount - 1;
                    
                    // Son adımda: kalan mesafeyi kullan, diğer adımlarda 50mm
                    double stepSize = isLastStep ? targetZ - currentZ : 50.0;
                    
                    // Z ekseninde stepSize kadar kaldır
                    clonedProbe.Translate(0, 0, stepSize);
                    currentZ += stepSize;
                    clonedProbe.Color = zColors[z % zColors.Length];
                    design.Invalidate();
                    Console.WriteLine($"✅ Probe Z ekseninde {stepSize:F2}mm kaldırıldı (Şu an: Z={currentZ:F2}mm / Hedef: {targetZ:F2}mm)");
                    
                    // Çarpışma kontrolü
                    foreach (Mesh partMesh in partMeshes)
                    {
                        try
                        {
                            // Mesh kontrolü
                            if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                continue;
                            
                            if (clonedProbe.Vertices == null || clonedProbe.Vertices.Length == 0)
                                continue;
                            
                            CollisionDetection cd = new CollisionDetection(
                                new Entity[] { clonedProbe },
                                new Entity[] { partMesh },
                                null
                            );
                            
                            cd.CheckMethod = collisionCheckType.SubdivisionTree;
                            cd.DoWork();
                            
                            if (cd.Result != null && cd.Result.Length > 0)
                            {
                                Console.WriteLine($"💥 ÇARPIŞMA TESPİT EDİLDİ! (Z+{(z+1)*50}mm konumunda)");
                                return (true, clonedProbe);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Mesh collision hatası: {ex.Message}");
                        }
                    }
                }
                
                Console.WriteLine("✅ CollisionDetection TAMAM - Çarpışma yok");
                return (false, clonedProbe);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GENEL HATA: {ex.Message}");
                Console.WriteLine($"❌ Stack: {ex.StackTrace}");
                return (false, null);
            }
        }
    }
}
