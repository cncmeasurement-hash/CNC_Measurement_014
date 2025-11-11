using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _014.Probe.Core
{
    /// <summary>
    /// Prob parametrelerini saklayan veri sınıfı.
    /// Form_New_Prob formundan alınan değerleri tutar ve JSON serileştirme için kullanılır.
    /// </summary>
    /// <remarks>
    /// <para>Bu sınıf prob tanımlarını ProbeStorage aracılığıyla probes.json dosyasına kaydeder.</para>
    /// <para>Tüm ölçüler milimetre (mm) cinsindendir.</para>
    /// <para>Değerler Form_New_Prob sayfasında kullanıcı tarafından kontrol edilir.</para>
    /// </remarks>
    public class ProbeData
    {
        /// <summary>
        /// Prob ismi.
        /// Kullanıcının probı tanımlaması için açıklayıcı isim.
        /// </summary>
        /// <example>
        /// "Renishaw TP20", "Zeiss XXT", "Standard Probe 6mm"
        /// </example>
        public string Name { get; set; }

        /// <summary>
        /// Küre çapı (mm).
        /// Prob ucundaki ölçüm küresinin çapı.
        /// </summary>
        /// <remarks>
        /// Bu değer prob uç boyutunu belirler.
        /// Tipik değerler: 3mm, 6mm, 8mm, 10mm
        /// </remarks>
        public decimal D { get; set; }

        /// <summary>
        /// Sap üst çapı (mm).
        /// Konik geçiş bölgesinin üst kısmının çapı.
        /// </summary>
        /// <remarks>
        /// Genellikle D değerinden büyüktür.
        /// Sap ile gövde arasındaki geçiş bölgesini oluşturur.
        /// </remarks>
        public decimal d1 { get; set; }

        /// <summary>
        /// Gövde çapı (mm).
        /// Prob ana gövdesinin çapı.
        /// </summary>
        /// <remarks>
        /// En kalın bölüm. Genellikle d1'den büyüktür.
        /// Tipik değerler: 40mm - 80mm
        /// </remarks>
        public decimal d2 { get; set; }

        /// <summary>
        /// Sap uzunluğu (mm).
        /// Küre ile gövde arasındaki mesafe.
        /// </summary>
        /// <remarks>
        /// Bu değer probun toplam uzunluğunu etkiler.
        /// Uzun saplar daha derin ölçümlere ulaşmayı sağlar.
        /// </remarks>
        public decimal L1 { get; set; }

        /// <summary>
        /// Gövde uzunluğu (mm).
        /// Ana gövde parçasının boyu.
        /// </summary>
        /// <remarks>
        /// Probun en kalın kısmının uzunluğu.
        /// Mekanik dayanıklılığı etkiler.
        /// </remarks>
        public decimal L2 { get; set; }

        /// <summary>
        /// Konik geçiş uzunluğu (mm).
        /// Sap ile gövde arasındaki geçiş bölgesinin boyu.
        /// </summary>
        /// <remarks>
        /// Bu bölge d1 çapından d2 çapına konik olarak geçiş yapar.
        /// Kısa değerler daha dik açı, uzun değerler daha yumuşak geçiş sağlar.
        /// </remarks>
        public decimal L3 { get; set; }
    }
}