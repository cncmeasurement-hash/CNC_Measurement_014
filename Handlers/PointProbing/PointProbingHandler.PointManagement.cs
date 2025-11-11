using _014.Probe.Core;
using _014.Managers.Data;  // ✅ YENİ: MeasurementDataManager için
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace _014
{
    public partial class PointProbingHandler
    {
        private void AddProbePoint(Point3D contactPoint, Vector3D normal)
        {
            try
            {
                // ═══════════════════════════════════════════════════════
                // ✅ PROBE GÖRSELLEŞTİRME (D*0.6 KAYDIRILI)
                // ═══════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("🔴 PROBE EKLENİYOR...");
                System.Diagnostics.Debug.WriteLine($"📍 Temas noktası: ({contactPoint.X:F2}, {contactPoint.Y:F2}, {contactPoint.Z:F2})");
                System.Diagnostics.Debug.WriteLine($"📐 Normal vektör: ({normal.X:F3}, {normal.Y:F3}, {normal.Z:F3})");
                
                // ✅ Probe diameter'ı al
                double probeDiameter = 6.0; // Default
                if (treeViewManager != null)
                {
                    probeDiameter = treeViewManager.GetSelectedProbeDiameter();
                }
                System.Diagnostics.Debug.WriteLine($"📐 Probe diameter: {probeDiameter:F2}mm");
                System.Diagnostics.Debug.WriteLine($"📐 Kaydırma mesafesi: {probeDiameter * 0.6:F2}mm (D*0.6)");
                
                // ✅ Retract distance'ı al (kullanıcı her seferinde farklı girebilir)
                double retractDistance = 3.0; // Default
                if (treeViewManager != null)
                {
                    retractDistance = treeViewManager.RetractDistance;
                }
                System.Diagnostics.Debug.WriteLine($"📐 Retract mesafesi: {retractDistance:F2}mm");
                
                // ✅ Z Safety mesafesini al (kullanıcı her seferinde farklı girebilir)
                double zSafetyDistance = 50.0; // Default
                if (treeViewManager != null)
                {
                    zSafetyDistance = treeViewManager.ZSafetyDistance;  // ✅ DOĞRU: ZSafetyDistance
                }
                System.Diagnostics.Debug.WriteLine($"📐 Z Safety mesafesi: {zSafetyDistance:F2}mm");
                
                // ✅ Probe mesh'ini al (TreeViewManager'dan seçili probe)
                Mesh probeMesh = null;
                if (treeViewManager != null)
                {
                    // TreeView'dan seçili probe'u al
                    ProbeData selectedProbeData = treeViewManager.GetSelectedProbeData();
                    
                    if (selectedProbeData == null)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ TreeView'dan probe verisi alınamadı!");
                        MessageBox.Show(
                            "⚠️ HATA: Probe verisi alınamadı!\n\n" +
                            "Lütfen TreeView'dan bir probe seçin.",
                            "Probe Seçimi Gerekli",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Seçili probe: {selectedProbeData.Name} (D={selectedProbeData.D}mm)");
                    
                    // ProbeBuilder ile mesh oluştur
                    probeMesh = ProbeBuilder.CreateProbeMesh(selectedProbeData);
                    
                    if (probeMesh == null)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ ProbeBuilder.CreateProbeMesh() null döndü!");
                        MessageBox.Show(
                            "⚠️ HATA: Probe mesh oluşturulamadı!",
                            "Hata",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                    System.Diagnostics.Debug.WriteLine($"✅ Probe mesh oluşturuldu (Vertex: {probeMesh.Vertices.Length})");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ TreeViewManager referansı yok!");
                    MessageBox.Show(
                        "⚠️ HATA: TreeViewManager bulunamadı!",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                
                // ✅ Probe'u ekle ve çarpışma kontrolü yap
                var (collision, displayProbe) = collisionDetector.CheckCollisionAtPoint(
                    probeMesh,        // ✅ YENİ: Probe mesh (Form_New_Prob'dan)
                    contactPoint,     // Temas noktası
                    normal,           // Normal vektör (kaydırma yönü)
                    probeDiameter,    // Probe çapı
                    retractDistance,  // Retract mesafesi
                    zSafetyDistance,  // ✅ YENİ: Z Safety mesafesi
                    true              // Probe'u ekranda göster
                );
                
                // ✅ ÇARPIŞMA YOKSA PROBE'U GÖRÜNMEZ YAP
                if (!collision && displayProbe != null)
                {
                    displayProbe.Visible = false;
                    design.Invalidate();
                    System.Diagnostics.Debug.WriteLine("✅ Probe görünmez yapıldı (Çarpışma yok)");
                }
                
                if (collision)
                {
                    // ✅ ÇARPIŞMA VAR - PROBE'U GÖRÜNÜR + MAVİ YAP!
                    if (displayProbe != null)
                    {
                        displayProbe.Visible = true;
                        displayProbe.Color = Color.Blue;
                        design.Invalidate();
                        System.Diagnostics.Debug.WriteLine("👁️ Probe görünür + mavi yapıldı (Çarpışma gösterimi)");
                    }
                    
                    // ⚠️ ÇARPIŞMA VAR - Nokta ekleme!
                    System.Diagnostics.Debug.WriteLine("");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════════");
                    System.Diagnostics.Debug.WriteLine("⛔ ÇARPIŞMA TESPİT EDİLDİ!");
                    System.Diagnostics.Debug.WriteLine("   → Nokta EKLENEMEZ!");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════════");
                    System.Diagnostics.Debug.WriteLine("");
                    
                    MessageBox.Show(
                        "⚠️ ÇARPIŞMA TESPİT EDİLDİ!\n\n" +
                        "Probe bu konumda parça ile çarpışıyor.\n" +
                        "Lütfen farklı bir nokta seçin.",
                        "Çarpışma Uyarısı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    
                    // ✅ MessageBox kapandıktan SONRA - Probe'u ekrandan sil
                    if (displayProbe != null && design.Entities.Contains(displayProbe))
                    {
                        design.Entities.Remove(displayProbe);
                        design.Invalidate();
                        System.Diagnostics.Debug.WriteLine("✅ Çarpışan probe ekrandan silindi");
                    }
                    
                    return;  // Metodu sonlandır, nokta EKLEME!
                }
                
                System.Diagnostics.Debug.WriteLine("✅ Çarpışma yok, nokta eklendi!");
                System.Diagnostics.Debug.WriteLine("");
                // ═══════════════════════════════════════════════════════
                
                // Temas noktasını ve normal'i listeye ekle
                selectedPoints.Add(contactPoint);
                pointNormals.Add(normal);
                
                // ✅ Marker konumunu hesapla: Temas noktası + (normal * D/2)
                double offset = probeDiameter / 2.0;
                Point3D markerPosition = new Point3D(
                    contactPoint.X + normal.X * offset,
                    contactPoint.Y + normal.Y * offset,
                    contactPoint.Z + normal.Z * offset
                );
                
                System.Diagnostics.Debug.WriteLine($"📍 Temas noktası: ({contactPoint.X:F2}, {contactPoint.Y:F2}, {contactPoint.Z:F2})");
                System.Diagnostics.Debug.WriteLine($"🔴 Marker konumu: ({markerPosition.X:F2}, {markerPosition.Y:F2}, {markerPosition.Z:F2}) [Offset: {offset:F2}mm]");
                
                // ═══════════════════════════════════════════════════════════
                // ✅ YENİ: MeasurementDataManager'a ekle
                // ═══════════════════════════════════════════════════════════
                
                if (groupId > 0)
                {
                    // Probe bilgisini al
                    ProbeData selectedProbe = treeViewManager?.GetSelectedProbeData();
                    if (selectedProbe != null)
                    {
                        // MeasurementPoint oluştur
                        var measurementPoint = new MeasurementPoint
                        {
                            MeasurementMode = "PointProbing",
                            GroupId = groupId,
                            PointIndex = selectedPoints.Count,
                            Position = contactPoint,
                            MarkerPosition = markerPosition,
                            SurfaceNormal = normal,
                            ProbeName = selectedProbe.Name,
                            ProbeDiameter = probeDiameter,
                            RetractDistance = retractDistance,
                            ZSafety = zSafetyDistance,
                            ApproachPoint = new Point3D(
                                markerPosition.X + normal.X * retractDistance,
                                markerPosition.Y + normal.Y * retractDistance,
                                markerPosition.Z + normal.Z * retractDistance
                            ),
                            TouchPoint = contactPoint,
                            CreatedAt = DateTime.Now,
                            IsActive = true,
                            Notes = ""
                        };
                        
                        // MeasurementDataManager'a ekle
                        bool success = MeasurementDataManager.Instance.AddPoint(groupId, measurementPoint);
                        
                        if (success)
                        {
                            System.Diagnostics.Debug.WriteLine($"✅ DataManager'a nokta eklendi: Group={groupId}, Point #{measurementPoint.PointIndex + 1}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ DataManager'a nokta eklenemedi!");
                        }
                    }
                }

                // ✅ Kırmızı küre marker oluştur (offset konumda)
                var marker = CreateSphereMarker(markerPosition, probeDiameter, Color.Red);
                
                // ✅ YENİ: Grup tag'ini marker'a ekle
                if (groupId > 0)
                {
                    marker.EntityData = $"PointProbing_{groupId}_Marker";
                    System.Diagnostics.Debug.WriteLine($"  ✅ Marker'a grup tag'i eklendi: PointProbing_{groupId}_Marker");
                }
                
                pointMarkers.Add(marker);
                design.Entities.Add(marker);
                
                // ✅ Normal çizgisi oluştur (uzunluk = Retract değeri)
                var normalLine = CreateNormalLine(markerPosition, normal, retractDistance, Color.Blue);
                normalLines.Add(normalLine);
                design.Entities.Add(normalLine);
                
                // ✅ Z+ çizgisi oluştur (uzunluk = Retract değeri)
                Point3D normalLineEnd = new Point3D(
                    markerPosition.X + normal.X * retractDistance,
                    markerPosition.Y + normal.Y * retractDistance,
                    markerPosition.Z + normal.Z * retractDistance
                );
                Vector3D zDirection = new Vector3D(0, 0, 1); // Z+ yönü
                var zLine = CreateNormalLine(normalLineEnd, zDirection, retractDistance, Color.Green);
                zLines.Add(zLine);
                design.Entities.Add(zLine);
                
                // Yenile
                design.Entities.Regen();
                design.Invalidate();
                
                System.Diagnostics.Debug.WriteLine($"✅ Probe Point #{selectedPoints.Count} eklendi! (Diameter: {probeDiameter}mm + {retractDistance}mm normal + {retractDistance}mm Z line)");
                
                // TreeView'i güncelle (temas noktası ile - G-code için)
                OnPointAdded?.Invoke(contactPoint);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AddProbePoint hatası: {ex.Message}");
            }
        }

        public void ClearAllPoints()
        {
            try
            {
                // Marker'ları sil
                foreach (var marker in pointMarkers)
                {
                    design.Entities.Remove(marker);
                }
                
                // Normal line'ları sil
                foreach (var line in normalLines)
                {
                    design.Entities.Remove(line);
                }
                
                // Z line'ları sil
                foreach (var zLine in zLines)
                {
                    design.Entities.Remove(zLine);
                }
                
                pointMarkers.Clear();
                normalLines.Clear();
                zLines.Clear();
                selectedPoints.Clear();
                pointNormals.Clear();  // ✅ Normal'leri de temizle
                
                design.Entities.Regen();
                design.Invalidate();
                
                System.Diagnostics.Debug.WriteLine("✅ Tüm probe noktaları + normal line'lar + Z line'lar temizlendi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ClearAllPoints hatası: {ex.Message}");
            }
        }

        public void DeletePointByCoordinate(Point3D point)
        {
            try
            {
                // Koordinata göre index bul
                int indexToRemove = -1;
                
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    Point3D p = selectedPoints[i];
                    
                    // Koordinatları karşılaştır
                    if (Math.Abs(p.X - point.X) < 0.01 &&
                        Math.Abs(p.Y - point.Y) < 0.01 &&
                        Math.Abs(p.Z - point.Z) < 0.01)
                    {
                        indexToRemove = i;
                        break;
                    }
                }
                
                // Bulunamadıysa çık
                if (indexToRemove == -1)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Nokta bulunamadı: X={point.X:F2}, Y={point.Y:F2}, Z={point.Z:F2}");
                    return;
                }
                
                // Marker'ı sil
                if (indexToRemove < pointMarkers.Count && design.Entities.Contains(pointMarkers[indexToRemove]))
                {
                    design.Entities.Remove(pointMarkers[indexToRemove]);
                }
                
                // Normal line'ı sil
                if (indexToRemove < normalLines.Count && design.Entities.Contains(normalLines[indexToRemove]))
                {
                    design.Entities.Remove(normalLines[indexToRemove]);
                }
                
                // Z line'ı sil
                if (indexToRemove < zLines.Count && design.Entities.Contains(zLines[indexToRemove]))
                {
                    design.Entities.Remove(zLines[indexToRemove]);
                }
                
                // Listelerden kaldır
                pointMarkers.RemoveAt(indexToRemove);
                selectedPoints.RemoveAt(indexToRemove);
                pointNormals.RemoveAt(indexToRemove);
                normalLines.RemoveAt(indexToRemove);
                zLines.RemoveAt(indexToRemove);
                
                design.Entities.Regen();
                design.Invalidate();
                
                System.Diagnostics.Debug.WriteLine($"✅ 3D view'den nokta silindi: X={point.X:F2}, Y={point.Y:F2}, Z={point.Z:F2}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DeletePointByCoordinate hatası: {ex.Message}");
            }
        }
    }
}
