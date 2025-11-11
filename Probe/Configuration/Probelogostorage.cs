using System;
using System.IO;
using System.Text.Json;
using _014.Managers.Data; // ✅ YENİ: PathManager için eklendi

namespace _014.Probe.Configuration
{
    /// <summary>
    /// Sadece custom text ayarlarını JSON formatında kaydetme/yükleme (LOGO YOK!)
    /// ✅ YENİ: PathManager kullanarak AppData/Local/014/Config/logo_settings.json konumunu kullanır
    /// </summary>
    internal static class ProbeLogoStorage
    {
        /// <summary>
        /// JSON dosyasının tam yolu.
        /// ✅ YENİ: PathManager kullanılarak AppData'da saklanır
        /// ESKİ: Uygulama klasöründe logo_settings.json olarak saklanıyordu
        /// </summary>
        private static readonly string jsonPath = PathManager.LogoSettingsJsonPath;

        /// <summary>
        /// Text ayarlarını JSON dosyasına kaydeder.
        /// </summary>
        public static void SaveSettings(ProbeLogoSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(jsonPath, json);

                System.Diagnostics.Debug.WriteLine($"✅ Text ayarları kaydedildi: {jsonPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Text ayarları kaydetme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// JSON dosyasından text ayarlarını yükler.
        /// Dosya yoksa veya hata varsa default ayarlar döner.
        /// </summary>
        public static ProbeLogoSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(jsonPath))
                {
                    System.Diagnostics.Debug.WriteLine("ℹ️ Text ayarları dosyası yok, default kullanılıyor.");
                    return ProbeLogoSettings.Default();
                }

                string json = File.ReadAllText(jsonPath);
                var settings = JsonSerializer.Deserialize<ProbeLogoSettings>(json);

                if (settings == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Text ayarları parse edilemedi, default kullanılıyor.");
                    return ProbeLogoSettings.Default();
                }

                // ✅ SADECE HasCustomText (HasLogo YOK!)
                System.Diagnostics.Debug.WriteLine($"✅ Text ayarları yüklendi: Text={settings.HasCustomText}");
                return settings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Text ayarları yükleme hatası: {ex.Message}");
                return ProbeLogoSettings.Default();
            }
        }

        /// <summary>
        /// Ayarlar dosyasını siler (fabrika ayarlarına dön).
        /// </summary>
        public static void ResetSettings()
        {
            try
            {
                if (File.Exists(jsonPath))
                {
                    File.Delete(jsonPath);
                    System.Diagnostics.Debug.WriteLine("✅ Text ayarları sıfırlandı.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Text ayarları sıfırlama hatası: {ex.Message}");
            }
        }
    }
}
