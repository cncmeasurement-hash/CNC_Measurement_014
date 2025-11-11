using System;

namespace _014.Probe.Visualization
{
    /// <summary>
    /// Eyeshot layer isimlerini merkezi olarak yöneten sınıf.
    /// Tüm projede kullanılan layer isimleri buradan alınır.
    /// </summary>
    /// <remarks>
    /// <para>Bu sınıf layer isimlerinde tutarlılık sağlar ve yazım hatalarını önler.</para>
    /// <para>Layer ismini değiştirmek için tek bir yeri güncellemeniz yeterlidir.</para>
    /// </remarks>
    public static class ProbeLayerNames
    {
        /// <summary>
        /// Prob modeli layer'ı.
        /// Küre, sap, konik geçiş ve gövdenin tümü bu layer'da oluşturulur.
        /// </summary>
        /// <remarks>
        /// Tüm prob elemanları tek bir layer'da gruplandırılmıştır.
        /// Bu sayede probe ile ilgili tüm işlemler (göster/gizle/kilitle) tek bir layer üzerinden yapılabilir.
        /// </remarks>
        public const string Probe = "Layer_Probe";

        /// <summary>
        /// Referans silindir layer'ı.
        /// Probe'dan Boolean Difference ile çıkarılan referans silindir bu layer'dadır.
        /// </summary>
        /// <remarks>
        /// Referans silindir ayrı bir layer'da tutulur çünkü probe'dan bağımsız bir elemandır.
        /// Gerektiğinde gizlenebilir veya silinebilir.
        /// </remarks>
        public const string RefCylinder = "Layer_RefCylinder";
    }
}