using _014;
using devDept;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
namespace _014
{
    public partial class CNC_Measurement : Form
    {
        private void InitializeLanguageEvents()
        {
            englishToolStripMenuItem.Click += (s, e) => SetLanguage("English");
            turkishToolStripMenuItem.Click += (s, e) => SetLanguage("Turkish");
            russianToolStripMenuItem.Click += (s, e) => SetLanguage("Russian");
        }

        private void SetLanguage(string language)
        {
            // Radio button effect - sadece seçilen dil checked
            englishToolStripMenuItem.Checked = (language == "English");
            turkishToolStripMenuItem.Checked = (language == "Turkish");
            russianToolStripMenuItem.Checked = (language == "Russian");

            // Kaydet
            Properties.Settings.Default.SelectedLanguage = language;
            Properties.Settings.Default.Save();

            UpdateUI(language);

            Debug.WriteLine($"✅ Dil değiştirildi ve kaydedildi: {language}");

            // İleride dil dosyası yüklemesi buraya eklenebilir
            // LoadLanguageFile(language);
        }

        private void UpdateUI(string language)
        {
            if (language == "Turkish")
            {
                fileToolStripMenuItem.Text = "Dosya";
                languageToolStripMenuItem.Text = "Dil";
                viewToolStripMenuItem.Text = "Görünüm";
                probingToolStripMenuItem.Text = "Prob";
                // ... diğer menüler ...
            }
            else if (language == "Russian")
            {
                fileToolStripMenuItem.Text = "Файл";
                languageToolStripMenuItem.Text = "Язык";
                viewToolStripMenuItem.Text = "Вид";
                probingToolStripMenuItem.Text = "Зондирование";
                // ... diğer menüler ...
            }
            else // English
            {
                fileToolStripMenuItem.Text = "File";
                languageToolStripMenuItem.Text = "Language";
                viewToolStripMenuItem.Text = "View";
                probingToolStripMenuItem.Text = "Probing";
                // ... diğer menüler ...
            }

            this.Invalidate();
        }

        private void LoadSavedLanguage()
        {
            string savedLanguage = Properties.Settings.Default.SelectedLanguage;

            // Null, boş veya geçersiz ise default: English
            if (string.IsNullOrEmpty(savedLanguage) ||
                (savedLanguage != "English" && savedLanguage != "Turkish" && savedLanguage != "Russian"))
            {
                savedLanguage = "English";
                Properties.Settings.Default.SelectedLanguage = "English";
                Properties.Settings.Default.Save();
            }

            // Dili ayarla
            SetLanguage(savedLanguage);

            Debug.WriteLine($"✅ Kaydedilmiş dil yüklendi: {savedLanguage}");
        }
    }
}
