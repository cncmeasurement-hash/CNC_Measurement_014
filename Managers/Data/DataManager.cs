using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _014.Analyzers.Data;
using _014.Probe.Core;

namespace _014.Managers.Data
{
    /// <summary>
    /// JSON ve veri yönetimi
    /// ✅ YENİ: Surface cache artık AppData/Local/014/Cache/surface_cache.json konumunda
    /// </summary>
    public class DataManager
    {
        private List<SurfaceData> surfaceDataList;
        private string jsonFilePath;

        // ✅ YENI: Seçili prob bilgisi
        private ProbeData selectedProbe;

        public DataManager()
        {
            surfaceDataList = new List<SurfaceData>();
            // ✅ YENİ: Default olarak cache path kullan
            jsonFilePath = PathManager.SurfaceCacheJsonPath;
            selectedProbe = null;
        }

        // ✅ YENI: Seçili probu kaydet
        public void SetSelectedProbe(ProbeData probe)
        {
            selectedProbe = probe;
            System.Diagnostics.Debug.WriteLine($"✅ Seçili prob kaydedildi: {probe?.Name}, D={probe?.D}");
        }

        // ✅ YENI: Seçili probu al
        public ProbeData GetSelectedProbe()
        {
            return selectedProbe;
        }

        public List<SurfaceData> GetSurfaceDataList()
        {
            return surfaceDataList;
        }

        public void ClearSurfaceData()
        {
            surfaceDataList.Clear();
        }

        public void AddSurfaceData(SurfaceData data)
        {
            surfaceDataList.Add(data);
        }

        public string GetJsonFilePath()
        {
            return jsonFilePath;
        }

        public SurfaceData GetSurfaceByTag(string tag)
        {
            try
            {
                string indexStr = tag.Replace("SURFACE_LABEL_", "")
                                    .Replace("FACE_NORMAL_", "");

                if (int.TryParse(indexStr, out int index))
                {
                    return surfaceDataList.FirstOrDefault(s => s.Index == index);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public SurfaceData GetSurfaceByIndex(int index)
        {
            return surfaceDataList.FirstOrDefault(s => s.Index == index);
        }

        /// <summary>
        /// Surface verilerini JSON'a kaydet
        /// ✅ YENİ: stepFileName parametresi ile dinamik dosya adı
        /// ✅ YENİ: Desktop yerine Cache klasörüne tek dosya olarak kaydeder
        /// ❌ ESKİ: Desktop'a timestamp ile her seferinde yeni dosya oluşturuyordu
        /// </summary>
        /// <param name="surfacesList">Kaydedilecek yüzey listesi</param>
        /// <param name="stepFileName">STEP dosya adı (uzantısız, opsiyonel)</param>
        public bool SaveToJson(List<object> surfacesList, string stepFileName = null, double clearancePlane = 0)
        {
            try
            {
                var jsonData = new
                {
                    clearancePlane = Math.Round(clearancePlane, 2),
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    totalSurfaces = surfacesList.Count,
                    surfaces = surfacesList
                };

                string jsonString = System.Text.Json.JsonSerializer.Serialize(jsonData, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // ✅ YENİ: Dinamik dosya adı
                if (string.IsNullOrEmpty(stepFileName))
                {
                    // Default: surface_cache.json (eski davranış)
                    jsonFilePath = PathManager.SurfaceCacheJsonPath;
                }
                else
                {
                    // Dinamik: 777_surface_cache.json
                    jsonFilePath = PathManager.GetSurfaceCacheJsonPath(stepFileName);
                }

                // ✅ YENİ EKLEME: Eski dosya varsa önce sil
                if (File.Exists(jsonFilePath))
                {
                    File.Delete(jsonFilePath);
                    System.Diagnostics.Debug.WriteLine($"🗑️ Eski surface cache silindi: {jsonFilePath}");
                }

                File.WriteAllText(jsonFilePath, jsonString);

                System.Diagnostics.Debug.WriteLine($"💾 Yeni surface cache kaydedildi: {jsonFilePath}");
                System.Diagnostics.Debug.WriteLine($"   {surfacesList.Count} yüzey kaydedildi");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Surface cache kayıt hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ✅ JSON dosyasından yüzey verilerini yükle
        /// </summary>
        public bool LoadFromJson(string jsonPath)
        {
            try
            {
                if (!File.Exists(jsonPath))
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ JSON dosyası bulunamadı: {jsonPath}");
                    return false;
                }

                string jsonString = File.ReadAllText(jsonPath);

                using (var document = System.Text.Json.JsonDocument.Parse(jsonString))
                {
                    var root = document.RootElement;

                    if (!root.TryGetProperty("surfaces", out var surfacesArray))
                    {
                        System.Diagnostics.Debug.WriteLine("❌ JSON'da 'surfaces' alanı bulunamadı!");
                        return false;
                    }

                    // Mevcut listeyi temizle
                    surfaceDataList.Clear();

                    int loadedCount = 0;

                    foreach (var surfaceElement in surfacesArray.EnumerateArray())
                    {
                        try
                        {
                            // JSON'dan verileri oku
                            int index = surfaceElement.GetProperty("index").GetInt32();
                            string name = surfaceElement.GetProperty("name").GetString();
                            int entityIndex = surfaceElement.GetProperty("entityIndex").GetInt32();
                            int faceIndex = surfaceElement.GetProperty("faceIndex").GetInt32();
                            string surfaceType = surfaceElement.GetProperty("type").GetString();
                            string group = surfaceElement.GetProperty("group").GetString();

                            // Normal vektör
                            var normalObj = surfaceElement.GetProperty("normal");
                            double normalX = normalObj.GetProperty("x").GetDouble();
                            double normalY = normalObj.GetProperty("y").GetDouble();
                            double normalZ = normalObj.GetProperty("z").GetDouble();
                            var normal = new devDept.Geometry.Vector3D(normalX, normalY, normalZ);

                            // Merkez nokta
                            var centerObj = surfaceElement.GetProperty("center");
                            double centerX = centerObj.GetProperty("x").GetDouble();
                            double centerY = centerObj.GetProperty("y").GetDouble();
                            double centerZ = centerObj.GetProperty("z").GetDouble();
                            var center = new devDept.Geometry.Point3D(centerX, centerY, centerZ);

                            // SurfaceData oluştur
                            var surfaceData = new SurfaceData
                            {
                                Index = index,
                                Name = name,
                                EntityIndex = entityIndex,
                                FaceIndex = faceIndex,
                                Normal = normal,
                                Center = center,
                                SurfaceType = surfaceType,
                                Group = group,
                                IsLabelVisible = true,
                                IsArrowVisible = true,
                                IsSelectable = surfaceType != "BOTTOM (Z-)"
                            };

                            surfaceDataList.Add(surfaceData);
                            loadedCount++;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Yüzey parse hatası: {ex.Message}");
                            continue;
                        }
                    }

                    jsonFilePath = jsonPath;
                    System.Diagnostics.Debug.WriteLine($"✅ JSON'dan {loadedCount} yüzey yüklendi: {jsonPath}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ JSON yükleme hatası: {ex.Message}");
                return false;
            }
        }

        public SurfaceData FindSurfaceAtPoint(devDept.Geometry.Point3D point)
        {
            try
            {
                if (surfaceDataList == null || surfaceDataList.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ surfaceDataList boş!");
                    return null;
                }

                double minDistance = double.MaxValue;
                SurfaceData closestSurface = null;

                foreach (var surface in surfaceDataList)
                {
                    double dx = point.X - surface.Center.X;
                    double dy = point.Y - surface.Center.Y;
                    double dz = point.Z - surface.Center.Z;
                    double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestSurface = surface;
                    }
                }

                if (closestSurface != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🎯 En yakın yüzey: {closestSurface.Name}, Mesafe: {minDistance:F2}mm");
                    return closestSurface;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ FindSurfaceAtPoint hatası: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Entity Index'e göre yüzey verisi döndürür
        /// Ridge Width için kullanılır - Dikey yüzey kontrolü
        /// </summary>
        /// <param name="entityIndex">design.Entities[index]</param>
        /// <returns>SurfaceData veya null</returns>
        public SurfaceData GetSurfaceByEntityIndex(int entityIndex)
        {
            try
            {
                var surface = surfaceDataList.FirstOrDefault(s => s.EntityIndex == entityIndex);

                if (surface != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Entity[{entityIndex}] bulundu: {surface.SurfaceType} - {surface.Group}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Entity[{entityIndex}] DataManager'da yok");
                }

                return surface;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetSurfaceByEntityIndex hatası: {ex.Message}");
                return null;
            }
        }
    }
}
