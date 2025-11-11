using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using _014.Managers.Data; // ✅ YENİ: PathManager için eklendi

namespace _014.CNC.Machine
{
    /// <summary>
    /// Makine verilerini JSON formatında kaydetme/yükleme
    /// ✅ YENİ: PathManager kullanarak AppData/Local/014/Config/machines.json konumunu kullanır
    /// </summary>
    internal static class MachineStorage
    {
        // ✅ YENİ: PathManager.MachinesJsonPath kullan
        // ESKİ: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "machines.json")
        private static readonly string jsonPath = PathManager.MachinesJsonPath;

        /// <summary>
        /// Makine listesini JSON'a kaydet
        /// </summary>
        public static void SaveToJson(List<MachineData> machines)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(machines, options);
                File.WriteAllText(jsonPath, json);

                System.Diagnostics.Debug.WriteLine($"✅ {machines.Count} makine kaydedildi: {jsonPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Kaydetme hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// JSON'dan makine listesini yükle
        /// </summary>
        public static List<MachineData> LoadFromJson()
        {
            try
            {
                if (!File.Exists(jsonPath))
                {
                    System.Diagnostics.Debug.WriteLine("ℹ️ machines.json bulunamadı, boş liste dönüyor");
                    return new List<MachineData>();
                }

                string json = File.ReadAllText(jsonPath);
                var machines = JsonSerializer.Deserialize<List<MachineData>>(json);

                System.Diagnostics.Debug.WriteLine($"✅ {machines?.Count ?? 0} makine yüklendi");
                return machines ?? new List<MachineData>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Yükleme hatası: {ex.Message}");
                return new List<MachineData>();
            }
        }
    }
}
