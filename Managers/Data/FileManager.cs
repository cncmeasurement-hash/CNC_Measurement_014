using System;
using System.IO;
using System.Windows.Forms;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;

namespace _014.Managers.Data
{
    /// <summary>
    /// Dosya işlemleri için yardımcı sınıf.
    /// Eyeshot sahne dosyalarını (.eye) açma ve kaydetme işlemlerini yönetir.
    /// </summary>
    internal static class FileManager
    {
        /// <summary>
        /// Eyeshot sahne dosyasını (.eye) açar ve Design kontrolüne yükler.
        /// Kullanıcıya dosya seçme dialogu gösterilir.
        /// </summary>
        /// <param name="design">Eyeshot Design kontrolü. Dosyanın yükleneceği sahne.</param>
        /// <returns>
        /// Başarılı açılışta: Açılan dosyanın tam yolu (örn: "C:\Projects\probe1.eye")
        /// <br/>Hata durumunda: Boş string ("")
        /// <br/>İptal edilirse: Boş string ("")
        /// </returns>
        /// <remarks>
        /// <para><strong>İşlem Adımları:</strong></para>
        /// <para>1. OpenFileDialog gösterilir (filtre: *.eye)</para>
        /// <para>2. Kullanıcı dosya seçer</para>
        /// <para>3. Dosya Design kontrolüne yüklenir</para>
        /// <para>4. Otomatik ZoomFit yapılır (tüm model ekrana sığdırılır)</para>
        /// <para>5. Sahne yenilenir</para>
        /// <para></para>
        /// <para><strong>Hata Yönetimi:</strong></para>
        /// <para>Dosya açılamazsa (bozuk, erişim hatası, vb.) kullanıcıya MessageBox ile bilgi verilir.</para>
        /// <para>Hata durumunda boş string döner, program çökmez.</para>
        /// </remarks>
        /// <exception cref="Exception">
        /// Dosya okuma hatası, format hatası veya erişim reddi durumlarında fırlatılır.
        /// Ancak bu exception yakalanır ve kullanıcıya MessageBox ile gösterilir.
        /// </exception>
        /// <example>
        /// <code>
        /// // Dosya aç ve sonucu kontrol et
        /// string filePath = FileManager.OpenFile(design1);
        /// 
        /// if (!string.IsNullOrEmpty(filePath))
        /// {
        ///     currentFilePath = filePath;
        ///     MessageBox.Show($"Dosya açıldı: {filePath}");
        /// }
        /// else
        /// {
        ///     MessageBox.Show("Dosya açılamadı veya iptal edildi.");
        /// }
        /// </code>
        /// </example>
        public static string OpenFile(Design design)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Eyeshot Scene (*.eye)|*.eye";
                ofd.Title = "Bir dosya seçin";

                if (ofd.ShowDialog() != DialogResult.OK)
                    return string.Empty;

                // ✅ Try-catch bloğu ile hata yönetimi
                try
                {
                    design.OpenFile(ofd.FileName);
                    design.ZoomFit();
                    design.Invalidate();

                    return ofd.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Dosya açılırken hata oluştu:\n{ex.Message}",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    return string.Empty;
                }
            }
        }
    }
}