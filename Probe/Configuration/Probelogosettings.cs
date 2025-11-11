using System;

namespace _014.Probe.Configuration
{
    /// <summary>
    /// Sadece custom text ayarları (LOGO YOK!)
    /// </summary>
    [Serializable]
    public class ProbeLogoSettings
    {
        /// <summary>
        /// Gövde alt kısmına yazılacak custom text.
        /// Boş string ise text eklenmez.
        /// </summary>
        public string CustomWebText { get; set; } = string.Empty;

        /// <summary>
        /// Custom text var mı?
        /// </summary>
        public bool HasCustomText => !string.IsNullOrEmpty(CustomWebText);

        /// <summary>
        /// Default (boş) ayarlar döndürür.
        /// </summary>
        public static ProbeLogoSettings Default()
        {
            return new ProbeLogoSettings
            {
                CustomWebText = string.Empty
            };
        }
    }
}