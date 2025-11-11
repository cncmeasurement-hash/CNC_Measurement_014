using System;
using System.IO;
using System.Windows.Forms;

namespace _014
{
    /// <summary>
    /// Form1 - FILE OPERATIONS
    /// PARTIAL CLASS 3/5: Dosya açma, kaydetme, import işlemleri
    /// </summary>
    public partial class CNC_Measurement
    {
        // ═══════════════════════════════════════════════════════════
        // DOSYA İŞLEMLERİ - TEMEL
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mevcut pencerede dosya aç
        /// </summary>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Eyeshot Scene (*.eye)|*.eye|All Files (*.*)|*.*";
                ofd.Title = "Dosya Aç";
                ofd.Multiselect = false;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    design1.OpenFile(ofd.FileName);
                    design1.ZoomFit();
                    design1.Invalidate();
                    this.Text = $"CNC Measurement - {Path.GetFileName(ofd.FileName)}";
                }
            }
        }

        /// <summary>
        /// Yeni pencerede dosya aç
        /// ✅ DÜZELTILDI: Show() önce çağrılıyor, sonra dosya yükleniyor
        /// </summary>
        private void openInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Eyeshot Scene (*.eye)|*.eye|All Files (*.*)|*.*";
                ofd.Title = "Yeni Pencerede Aç";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filePath in ofd.FileNames)
                    {
                        CNC_Measurement newWindow = new CNC_Measurement();
                        newWindow.Show(); // ✅ ÖNCE Show() - handle oluşsun!
                        Application.DoEvents(); // ✅ Handle'ın oluşmasını bekle

                        newWindow.design1.OpenFile(filePath);
                        newWindow.design1.ZoomFit();
                        newWindow.design1.Invalidate();
                        newWindow.Text = $"CNC Measurement - {Path.GetFileName(filePath)}";
                    }
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Eyeshot Scene (*.eye)|*.eye";
                sfd.Title = "Dosyayı Kaydet";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // ✅ ESKİ: Eyeshot dosyasını kaydet
                    design1.SaveFile(sfd.FileName);
                    this.Text = $"CNC Measurement - {Path.GetFileName(sfd.FileName)}";
                    
                    // ✅ YENİ EKLEME: Aynı isimle JSON'u da kaydet
                    try
                    {
                        string eyeFileName = Path.GetFileNameWithoutExtension(sfd.FileName);
                        string jsonPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "014",
                            "Projects",
                            eyeFileName + ".cncproj"
                        );
                        _014.Managers.Data.MeasurementDataManager.Instance.SaveToJson(jsonPath);
                        System.Diagnostics.Debug.WriteLine($"✅ Save: {eyeFileName}.eye + {eyeFileName}.cncproj kaydedildi");
                        
                        // ✅ YENİ EKLEME: Surface cache'i de kopyala
                        string cacheFolder = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "014", "Cache"
                        );
                        
                        // En son değiştirilen *_surface_cache.json dosyasını bul
                        var cacheFiles = Directory.GetFiles(cacheFolder, "*_surface_cache.json");
                        if (cacheFiles.Length > 0)
                        {
                            // En son değiştirilen dosyayı al
                            string latestCache = cacheFiles
                                .OrderByDescending(f => File.GetLastWriteTime(f))
                                .First();
                            
                            // Yeni adla kopyala
                            string newCachePath = Path.Combine(cacheFolder, eyeFileName + "_surface_cache.json");
                            File.Copy(latestCache, newCachePath, overwrite: true);
                            
                            System.Diagnostics.Debug.WriteLine($"✅ Surface cache kopyalandı: {Path.GetFileName(latestCache)} → {eyeFileName}_surface_cache.json");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ JSON kayıt hatası: {ex.Message}");
                    }
                    
                    MessageBox.Show("Dosya kaydedildi.", "Bilgi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Eyeshot Scene (*.eye)|*.eye";
                sfd.Title = "Farklı Kaydet";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // ✅ ESKİ: Eyeshot dosyasını kaydet
                    design1.SaveFile(sfd.FileName);
                    this.Text = $"CNC Measurement - {Path.GetFileName(sfd.FileName)}";
                    
                    // ✅ YENİ EKLEME: Aynı isimle JSON'u da kaydet
                    try
                    {
                        string eyeFileName = Path.GetFileNameWithoutExtension(sfd.FileName);
                        string jsonPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "014",
                            "Projects",
                            eyeFileName + ".cncproj"
                        );
                        _014.Managers.Data.MeasurementDataManager.Instance.SaveToJson(jsonPath);
                        System.Diagnostics.Debug.WriteLine($"✅ Save As: {eyeFileName}.eye + {eyeFileName}.cncproj kaydedildi");
                        
                        // ✅ YENİ EKLEME: Surface cache'i de kopyala
                        string cacheFolder = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "014", "Cache"
                        );
                        
                        // En son değiştirilen *_surface_cache.json dosyasını bul
                        var cacheFiles = Directory.GetFiles(cacheFolder, "*_surface_cache.json");
                        if (cacheFiles.Length > 0)
                        {
                            // En son değiştirilen dosyayı al
                            string latestCache = cacheFiles
                                .OrderByDescending(f => File.GetLastWriteTime(f))
                                .First();
                            
                            // Yeni adla kopyala
                            string newCachePath = Path.Combine(cacheFolder, eyeFileName + "_surface_cache.json");
                            File.Copy(latestCache, newCachePath, overwrite: true);
                            
                            System.Diagnostics.Debug.WriteLine($"✅ Surface cache kopyalandı: {Path.GetFileName(latestCache)} → {eyeFileName}_surface_cache.json");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ JSON kayıt hatası: {ex.Message}");
                    }
                    
                    MessageBox.Show("Dosya kaydedildi.", "Bilgi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Tüm pencereleri kapat (File menüsünden)
        /// </summary>
        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openWindows.Count <= 1)
            {
                MessageBox.Show("Sadece bu pencere açık.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Tüm pencereleri kapatmak istiyor musunuz?\n\nToplam {openWindows.Count} pencere açık.",
                "Tümünü Kapat",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                var windowsToClose = new List<System.Collections.Generic.List<CNC_Measurement>>(new[] { openWindows });
                foreach (var window in openWindows.ToArray())
                {
                    window.Close();
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // IMPORT STEP - MEVCUT PENCERE
        // ═══════════════════════════════════════════════════════════

        private void importSTEPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "STEP Files (*.step;*.stp)|*.step;*.stp|All Files (*.*)|*.*";
                ofd.Title = "Select STEP File";
                ofd.Multiselect = false;

                if (ofd.ShowDialog() == DialogResult.OK)
                    fileImporter.ImportSTEP(ofd.FileName);
            }
        }

        private void importAsyncSTEPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "STEP Files (*.step;*.stp)|*.step;*.stp|All Files (*.*)|*.*";
                ofd.Title = "Select STEP File (Async)";
                ofd.Multiselect = false;

                if (ofd.ShowDialog() == DialogResult.OK)
                    fileImporter.ImportSTEPAsync(ofd.FileName);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // IMPORT STEP - YENİ PENCERE
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// STEP dosyasını yeni pencerede aç
        /// ✅ DÜZELTILDI: Show() önce çağrılıyor
        /// </summary>
        private void importSTEPInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "STEP Files (*.step;*.stp)|*.step;*.stp|All Files (*.*)|*.*";
                ofd.Title = "Import STEP in New Window";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filePath in ofd.FileNames)
                    {
                        CNC_Measurement newWindow = new CNC_Measurement();
                        newWindow.Show(); // ✅ ÖNCE Show()
                        Application.DoEvents(); // ✅ Handle oluşsun
                        newWindow.Text = $"CNC Measurement - {Path.GetFileName(filePath)}";

                        newWindow.fileImporter.ImportSTEP(filePath);
                    }
                }
            }
        }

        /// <summary>
        /// Async STEP dosyasını yeni pencerede aç
        /// ✅ DÜZELTILDI: Show() önce çağrılıyor
        /// </summary>
        private void importAsyncSTEPInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "STEP Files (*.step;*.stp)|*.step;*.stp|All Files (*.*)|*.*";
                ofd.Title = "Import Async STEP in New Window";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filePath in ofd.FileNames)
                    {
                        CNC_Measurement newWindow = new CNC_Measurement();
                        newWindow.Show(); // ✅ ÖNCE Show()
                        Application.DoEvents(); // ✅ Handle oluşsun
                        newWindow.Text = $"CNC Measurement - {Path.GetFileName(filePath)}";

                        newWindow.fileImporter.ImportSTEPAsync(filePath);
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // IMPORT IGES - MEVCUT PENCERE
        // ═══════════════════════════════════════════════════════════

        private void importIGESToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "IGES Files (*.iges;*.igs)|*.iges;*.igs|All Files (*.*)|*.*";
                ofd.Title = "Select IGES File";
                ofd.Multiselect = false;

                if (ofd.ShowDialog() == DialogResult.OK)
                    fileImporter.ImportIGES(ofd.FileName);
            }
        }

        private void importAsyncIGESToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "IGES Files (*.iges;*.igs)|*.iges;*.igs|All Files (*.*)|*.*";
                ofd.Title = "Select IGES File (Async)";
                ofd.Multiselect = false;

                if (ofd.ShowDialog() == DialogResult.OK)
                    fileImporter.ImportIGESAsync(ofd.FileName);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // IMPORT IGES - YENİ PENCERE
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// IGES dosyasını yeni pencerede aç
        /// ✅ DÜZELTILDI: Show() önce çağrılıyor
        /// </summary>
        private void importIGESInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "IGES Files (*.iges;*.igs)|*.iges;*.igs|All Files (*.*)|*.*";
                ofd.Title = "Import IGES in New Window";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filePath in ofd.FileNames)
                    {
                        CNC_Measurement newWindow = new CNC_Measurement();
                        newWindow.Show(); // ✅ ÖNCE Show()
                        Application.DoEvents(); // ✅ Handle oluşsun
                        newWindow.Text = $"CNC Measurement - {Path.GetFileName(filePath)}";

                        newWindow.fileImporter.ImportIGES(filePath);
                    }
                }
            }
        }

        /// <summary>
        /// Async IGES dosyasını yeni pencerede aç
        /// ✅ DÜZELTILDI: Show() önce çağrılıyor
        /// </summary>
        private void importAsyncIGESInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "IGES Files (*.iges;*.igs)|*.iges;*.igs|All Files (*.*)|*.*";
                ofd.Title = "Import Async IGES in New Window";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filePath in ofd.FileNames)
                    {
                        CNC_Measurement newWindow = new CNC_Measurement();
                        newWindow.Show(); // ✅ ÖNCE Show()
                        Application.DoEvents(); // ✅ Handle oluşsun
                        newWindow.Text = $"CNC Measurement - {Path.GetFileName(filePath)}";

                        newWindow.fileImporter.ImportIGESAsync(filePath);
                    }
                }
            }
        }
    }
}
