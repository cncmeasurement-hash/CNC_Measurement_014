using System;
using System.Windows.Forms;
using _014.Managers.Data; // ✅ PathManager için eklendi

namespace _014
{
    static class Program
    {
        /// <summary>
        /// Uygulamanın ana giriş noktası.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // ✅ ADIM 1: İLK ÖNCE KLASÖRLERI OLUŞTUR
                PathManager.EnsureDirectoriesExist();

                // ✅ ADIM 2: ESKİ DOSYALARI MİGRATE ET (İLK ÇALIŞTIRMADA)
                PathManager.MigrateOldFiles();

                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🚀 014 CNC Measurement BAŞLATILDI");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Program başlatma hatası:\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            Application.Run(new CNC_Measurement());
        }
    }
}
