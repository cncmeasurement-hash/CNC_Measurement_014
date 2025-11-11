using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _014.Utilities.UI;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// RidgeWidthHandler - Toolpath Generation
    /// Toolpath oluşturma ve veri erişimi
    /// </summary>
    public partial class RidgeWidthHandler
    {
        public void SetInstructionPanel(InstructionPanel panel)
        {
            instructionPanel = panel;
            System.Diagnostics.Debug.WriteLine("✅ RidgeWidthHandler: InstructionPanel set edildi");
        }

        public void RegenerateAllToolpaths()
        {
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine("🔄 RIDGE WIDTH TOOLPATH'LERİ YENİDEN OLUŞTURULUYOR");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            
            try
            {
                // TreeView'den tüm Ridge Width gruplarını al
                var groups = treeViewManager.GetAllRidgeWidthGroups();
                
                if (groups == null || groups.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Ridge Width grubu bulunamadı");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"📊 {groups.Count} Ridge Width grubu bulundu");
                
                // Her grup için toolpath oluştur
                int successCount = 0;
                foreach (var group in groups)
                {
                    try
                    {
                        // Gruptan nokta ve normal verilerini çıkar
                        if (group.Nodes.Count < 3)  // Point 1, Point 2, Ölçüm Sonucu
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Grup eksik: {group.Text}");
                            continue;
                        }
                        
                        // Grup verilerinden noktaları ve normalleri al
                        // ⚠️ NOT: Bu kısım artık groupPoints ve groupNormals Dictionary'lerinden alınmalı
                        // ⚠️ Şu anda bu metod kullanılmıyor - Toolpath generation ToolpathManager'da yapılıyor
                        
                        // Placeholder - gerçek implementasyon için groupPoints/groupNormals kullanılmalı
                        Point3D? point1 = null;
                        Point3D? point2 = null;
                        
                        if (point1 == null || point2 == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Nokta verileri bulunamadı: {group.Text}");
                            System.Diagnostics.Debug.WriteLine($"   NOT: Bu metod artık kullanılmıyor. ToolpathManager kullanın.");
                            continue;
                        }
                        
                        // Toolpath oluştur (normal'ler grup tag'inden çıkarılmalı)
                        // CreateRidgeWidthToolpath(point1.Value, normal1, point2.Value, normal2, probeD, retract);
                        
                        successCount++;
                        System.Diagnostics.Debug.WriteLine($"✅ Toolpath oluşturuldu: {group.Text}");
                    }
                    catch (Exception groupEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Grup toolpath hatası ({group.Text}): {groupEx.Message}");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ {successCount}/{groups.Count} grup için toolpath yenilendi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RegenerateAllToolpaths hatası: {ex.Message}");
            }
            
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
        }

        public List<Point3D> GetMarkerPositions()
        {
            var allMarkerPositions = new List<Point3D>();
            
            try
            {
                // Probe diameter'ı al
                double probeDiameter = 4.0; // Default
                if (treeViewManager != null)
                {
                    var selectedProbe = treeViewManager.GetSelectedProbeData();
                    probeDiameter = (double)(selectedProbe?.D ?? 4.0m);
                }
                
                double offset = probeDiameter / 2.0;
                
                // Tüm grupları sırası ile dolaş
                foreach (var kvp in groupPoints.OrderBy(x => x.Key))
                {
                    int groupId = kvp.Key;
                    List<Point3D> contactPoints = kvp.Value;
                    List<Vector3D> normals = groupNormals.ContainsKey(groupId) ? groupNormals[groupId] : null;
                    
                    if (contactPoints != null && normals != null && contactPoints.Count == normals.Count)
                    {
                        // Her nokta için marker pozisyonunu hesapla (Point Probing pattern'i)
                        for (int i = 0; i < contactPoints.Count; i++)
                        {
                            Point3D contactPoint = contactPoints[i];
                            Vector3D normal = normals[i];
                            
                            // ✅ Marker pozisyonu = temas noktası + (D/2 × normal)
                            Point3D markerPosition = new Point3D(
                                contactPoint.X + normal.X * offset,
                                contactPoint.Y + normal.Y * offset,
                                contactPoint.Z + normal.Z * offset
                            );
                            
                            allMarkerPositions.Add(markerPosition);
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"  📍 Ridge Width Grup {groupId}: {contactPoints.Count} marker pozisyonu (D/2 offset uygulandı)");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"📊 Ridge Width Toplam: {allMarkerPositions.Count} marker pozisyonu");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RidgeWidth GetMarkerPositions hatası: {ex.Message}");
            }
            
            return allMarkerPositions;
        }

        public List<Vector3D> GetNormals()
        {
            var allNormals = new List<Vector3D>();
            
            try
            {
                // Tüm grupları sırası ile dolaş
                foreach (var kvp in groupNormals.OrderBy(x => x.Key))
                {
                    int groupId = kvp.Key;
                    List<Vector3D> normals = kvp.Value;
                    
                    if (normals != null && normals.Count > 0)
                    {
                        allNormals.AddRange(normals);
                        System.Diagnostics.Debug.WriteLine($"  📐 Ridge Width Grup {groupId}: {normals.Count} normal vektör");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"📊 Ridge Width Toplam: {allNormals.Count} normal vektör");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RidgeWidth GetNormals hatası: {ex.Message}");
            }
            
            return allNormals;
        }
    }
}
