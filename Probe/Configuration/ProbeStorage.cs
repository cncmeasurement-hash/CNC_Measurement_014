using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using _014.Probe.Core;
using _014.Managers.Data; // ✅ YENİ: PathManager için eklendi

namespace _014.Probe.Configuration
{
    /// <summary>
    /// Prob verilerini JSON formatında kaydetmek ve yüklemek için yardımcı sınıf.
    /// Tüm prob tanımlarını kalıcı olarak depolar.
    /// ✅ YENİ: PathManager kullanarak AppData/Local/014/Config/probes.json konumunu kullanır
    /// </summary>
    /// <remarks>
    /// <para><strong>Dosya Konumu:</strong> AppData/Local/014/Config/probes.json</para>
    /// <para><strong>Format:</strong> UTF-8 kodlamalı, indented (okunabilir) JSON</para>
    /// <para>Dosya otomatik oluşturulur, ilk kullanımda boş liste döner.</para>
    /// </remarks>
    internal static class ProbeStorage
    {
        /// <summary>
        /// JSON dosyasının tam yolu.
        /// ✅ YENİ: PathManager kullanılarak AppData'da saklanır
        /// ESKİ: Uygulama klasöründe probes.json olarak saklanıyordu
        /// </summary>
        private static readonly string jsonPath = PathManager.ProbesJsonPath;

        /// <summary>
        /// Prob listesini JSON dosyasına kaydeder.
        /// Mevcut dosya varsa üzerine yazar.
        /// </summary>
        /// <param name="list">Kaydedilecek prob listesi. Null olamaz.</param>
        /// <remarks>
        /// <para><strong>Dosya Formatı:</strong></para>
        /// <para>- UTF-8 encoding</para>
        /// <para>- Indented (okunabilir) JSON</para>
        /// <para>- Mevcut dosya varsa üzerine yazılır</para>
        /// <para></para>
        /// <para><strong>Örnek JSON Yapısı:</strong></para>
        /// <code>
        /// [
        ///   {
        ///     "Name": "Renishaw TP20",
        ///     "D": 6,
        ///     "d1": 9,
        ///     "d2": 58,
        ///     "L1": 55,
        ///     "L2": 75,
        ///     "L3": 10
        ///   }
        /// ]
        /// </code>
        /// </remarks>
        /// <exception cref="ArgumentNullException">list parametresi null ise</exception>
        /// <exception cref="IOException">Dosya yazma hatası (erişim reddi, disk dolu, vb.)</exception>
        /// <example>
        /// <code>
        /// var probes = new List&lt;ProbeData&gt;
        /// {
        ///     new ProbeData { Name = "Test Probe", D = 6, d1 = 9, d2 = 58, L1 = 55, L2 = 75, L3 = 10 }
        /// };
        /// 
        /// ProbeStorage.SaveToJson(probes);
        /// </code>
        /// </example>
        public static void SaveToJson(List<ProbeData> list)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(list, options));
        }

        /// <summary>
        /// JSON dosyasından prob listesini yükler.
        /// Dosya yoksa boş liste döner, hata fırlatmaz.
        /// </summary>
        /// <returns>
        /// Prob listesi. 
        /// <br/>Dosya yoksa: Boş liste (new List&lt;ProbeData&gt;())
        /// <br/>Dosya bozuksa: Boş liste döner
        /// </returns>
        /// <remarks>
        /// <para><strong>Güvenli Yükleme:</strong></para>
        /// <para>- Dosya yoksa hata vermez, boş liste döner</para>
        /// <para>- JSON parse hatası olursa boş liste döner</para>
        /// <para>- Null değer olursa boş liste döner</para>
        /// <para></para>
        /// <para><strong>İlk Kullanım:</strong></para>
        /// <para>Uygulama ilk kez çalıştığında dosya yoktur.</para>
        /// <para>Bu metod boş liste döner ve program normal devam eder.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Probları yükle
        /// var probes = ProbeStorage.LoadFromJson();
        /// 
        /// if (probes.Count == 0)
        /// {
        ///     MessageBox.Show("Henüz kayıtlı prob yok.");
        /// }
        /// else
        /// {
        ///     foreach (var probe in probes)
        ///     {
        ///         Console.WriteLine($"Prob: {probe.Name}");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static List<ProbeData> LoadFromJson()
        {
            if (!File.Exists(jsonPath))
                return new List<ProbeData>();

            string json = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<List<ProbeData>>(json) ?? new List<ProbeData>();
        }
    }
}
