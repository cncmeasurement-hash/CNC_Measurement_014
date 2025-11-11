using System;
using System.IO;

namespace _014.Managers.Data
{
    /// <summary>
    /// Tüm JSON dosya yollarını merkezi olarak yönetir
    /// AppData/Local/014 klasör yapısını oluşturur
    /// </summary>
    public static class PathManager
    {
        // Ana AppData klasörü
        private static readonly string AppDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "014"
        );

        // Alt klasörler
        public static readonly string ConfigFolder = Path.Combine(AppDataRoot, "Config");
        public static readonly string CacheFolder = Path.Combine(AppDataRoot, "Cache");
        public static readonly string ProjectsFolder = Path.Combine(AppDataRoot, "Projects");

        // JSON dosya yolları
        public static readonly string MachinesJsonPath = Path.Combine(ConfigFolder, "machines.json");
        public static readonly string ProbesJsonPath = Path.Combine(ConfigFolder, "probes.json");
        public static readonly string LogoSettingsJsonPath = Path.Combine(ConfigFolder, "logo_settings.json");
        public static readonly string SurfaceCacheJsonPath = Path.Combine(CacheFolder, "surface_cache.json");
        public static readonly string AutoSaveProjectPath = Path.Combine(ProjectsFolder, "AutoSave.cncproj");

        /// <summary>
        /// Dinamik surface cache JSON path oluşturur
        /// Örnek: 777.step → 777_surface_cache.json
        /// </summary>
        /// <param name="stepFileName">STEP dosya adı (uzantısız)</param>
        /// <returns>Tam dosya yolu</returns>
        public static string GetSurfaceCacheJsonPath(string stepFileName)
        {
            return Path.Combine(CacheFolder, $"{stepFileName}_surface_cache.json");
        }

        /// <summary>
        /// Tüm klasörleri oluşturur (yoksa)
        /// Program başlangıcında bir kez çağrılmalı
        /// </summary>
        public static void EnsureDirectoriesExist()
        {
            try
            {
                Directory.CreateDirectory(ConfigFolder);
                Directory.CreateDirectory(CacheFolder);
                Directory.CreateDirectory(ProjectsFolder);

                System.Diagnostics.Debug.WriteLine("✅ PathManager: Klasörler hazır");
                System.Diagnostics.Debug.WriteLine($"   Config: {ConfigFolder}");
                System.Diagnostics.Debug.WriteLine($"   Cache: {CacheFolder}");
                System.Diagnostics.Debug.WriteLine($"   Projects: {ProjectsFolder}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ PathManager klasör oluşturma hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Eski dosyaları yeni konuma taşır (ilk çalıştırmada)
        /// </summary>
        public static void MigrateOldFiles()
        {
            string oldAppDir = AppDomain.CurrentDomain.BaseDirectory;

            // Eski dosyaları tara ve taşı
            MigrateFile(Path.Combine(oldAppDir, "machines.json"), MachinesJsonPath);
            MigrateFile(Path.Combine(oldAppDir, "probes.json"), ProbesJsonPath);
            MigrateFile(Path.Combine(oldAppDir, "logo_settings.json"), LogoSettingsJsonPath);

            System.Diagnostics.Debug.WriteLine("✅ PathManager: Eski dosyalar migrate edildi");
        }

        private static void MigrateFile(string oldPath, string newPath)
        {
            try
            {
                // Eski dosya var mı?
                if (File.Exists(oldPath))
                {
                    // Yeni dosya yoksa taşı
                    if (!File.Exists(newPath))
                    {
                        File.Copy(oldPath, newPath);
                        System.Diagnostics.Debug.WriteLine($"   📦 Taşındı: {Path.GetFileName(oldPath)}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"   ⚠️ Migration hatası: {ex.Message}");
                // Hata olsa da devam et
            }
        }
    }
}
