using System;
using System.Windows.Forms;
using _014.Probe.Configuration;

namespace _014
{
    public partial class password : Form
    {
        public password()
        {
            InitializeComponent();
            LoadExistingSettings();
        }

        /// <summary>
        /// Form yüklendiğinde mevcut ayarları yükle
        /// </summary>
        private void LoadExistingSettings()
        {
            try
            {
                var settings = ProbeLogoStorage.LoadSettings();

                // Custom text varsa göster
                if (settings.HasCustomText)
                {
                    textBox_WebText.Text = settings.CustomWebText;
                    System.Diagnostics.Debug.WriteLine($"✅ Loaded Custom Text: {settings.CustomWebText}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Ayarlar yüklenemedi: {ex.Message}");
            }
        }

        /// <summary>
        /// KAYDET butonu - Sadece text'i kaydet
        /// </summary>
        private void button_SaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                string webText = textBox_WebText.Text;

                // Ayarları oluştur - SADECE TEXT!
                var settings = new ProbeLogoSettings
                {
                    CustomWebText = webText
                };

                // Kaydet
                ProbeLogoStorage.SaveSettings(settings);

                System.Diagnostics.Debug.WriteLine($"✅ Text ayarları kaydedildi: {webText}");

                // Başarı mesajı
                MessageBox.Show(
                    $"✅ Text başarıyla kaydedildi!\n\n" +
                    $"Web Site: {(settings.HasCustomText ? webText : "Boş")}",
                    "Başarılı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Kayıt hatası: {ex.Message}");
                MessageBox.Show(
                    $"❌ Kayıt hatası:\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// İPTAL butonu
        /// </summary>
        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}