using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace _014
{
    /// <summary>
    /// Form1 - WINDOW MANAGEMENT
    /// PARTIAL CLASS 2/5: Window menüsü ve pencere düzenleme işlemleri
    /// </summary>
    public partial class CNC_Measurement
    {
        // ═══════════════════════════════════════════════════════════
        // WINDOW MENÜSÜ FONKSİYONLARI
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Window menüsü açılırken dinamik olarak pencere listesini ekler
        /// </summary>
        private void windowToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            UpdateWindowMenu();
        }

        /// <summary>
        /// Window menüsüne açık pencereleri dinamik olarak ekler
        /// </summary>
        private void UpdateWindowMenu()
        {
            // Separator'dan sonraki tüm dinamik menü öğelerini temizle
            int separatorIndex = windowToolStripMenuItem.DropDownItems.IndexOf(toolStripSeparator6);

            // Separator'dan sonraki tüm öğeleri sil
            while (windowToolStripMenuItem.DropDownItems.Count > separatorIndex + 1)
            {
                windowToolStripMenuItem.DropDownItems.RemoveAt(separatorIndex + 1);
            }

            // Açık pencereleri ekle
            for (int i = 0; i < openWindows.Count; i++)
            {
                var window = openWindows[i];
                var menuItem = new ToolStripMenuItem();

                // Menü öğesi metnini ayarla
                string displayText = window.Text;
                if (string.IsNullOrEmpty(displayText) || displayText == "CNC Measurement")
                    displayText = $"Window {i + 1}";

                // Numaralandırma ekle
                menuItem.Text = $"{i + 1}. {displayText}";

                // Aktif pencereyi işaretle
                if (window == this)
                {
                    menuItem.Checked = true;
                    menuItem.Font = new Font(menuItem.Font, FontStyle.Bold);
                }

                // Click event'i ekle
                var targetWindow = window; // Closure için
                menuItem.Click += (s, ev) =>
                {
                    // Minimize edilmişse normal yap
                    if (targetWindow.WindowState == FormWindowState.Minimized)
                        targetWindow.WindowState = FormWindowState.Normal;

                    // Pencereyi öne getir
                    targetWindow.BringToFront();
                    targetWindow.Activate();
                };

                windowToolStripMenuItem.DropDownItems.Add(menuItem);
            }

            // Eğer hiç pencere yoksa bilgi mesajı ekle
            if (openWindows.Count == 0)
            {
                var noWindowItem = new ToolStripMenuItem("(No windows open)");
                noWindowItem.Enabled = false;
                windowToolStripMenuItem.DropDownItems.Add(noWindowItem);
            }
        }

        /// <summary>
        /// Cascade - Pencereleri kademeli olarak düzenler
        /// </summary>
        private void cascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openWindows.Count <= 1)
            {
                MessageBox.Show("En az 2 pencere açık olmalı!", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Ekran boyutlarını al
            var screen = Screen.FromControl(this);
            int startX = screen.WorkingArea.X + 50;
            int startY = screen.WorkingArea.Y + 50;
            int offsetX = 30;
            int offsetY = 30;
            int width = screen.WorkingArea.Width - 200;
            int height = screen.WorkingArea.Height - 200;

            // Her pencereyi kademeli olarak yerleştir
            for (int i = 0; i < openWindows.Count; i++)
            {
                var window = openWindows[i];
                window.WindowState = FormWindowState.Normal;
                window.SetBounds(
                    startX + (i * offsetX),
                    startY + (i * offsetY),
                    width,
                    height
                );
            }

            MessageBox.Show($"{openWindows.Count} pencere cascade düzenlendi!", "Başarılı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Tile Horizontal - Pencereleri yatay olarak böl
        /// </summary>
        private void tileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openWindows.Count <= 1)
            {
                MessageBox.Show("En az 2 pencere açık olmalı!", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var screen = Screen.FromControl(this);
            int windowHeight = screen.WorkingArea.Height / openWindows.Count;
            int width = screen.WorkingArea.Width;
            int startY = screen.WorkingArea.Y;

            for (int i = 0; i < openWindows.Count; i++)
            {
                var window = openWindows[i];
                window.WindowState = FormWindowState.Normal;
                window.SetBounds(
                    screen.WorkingArea.X,
                    startY + (i * windowHeight),
                    width,
                    windowHeight
                );
            }

            MessageBox.Show($"{openWindows.Count} pencere yatay döşendi!", "Başarılı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Tile Vertical - Pencereleri dikey olarak böl
        /// </summary>
        private void tileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openWindows.Count <= 1)
            {
                MessageBox.Show("En az 2 pencere açık olmalı!", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var screen = Screen.FromControl(this);
            int windowWidth = screen.WorkingArea.Width / openWindows.Count;
            int height = screen.WorkingArea.Height;
            int startX = screen.WorkingArea.X;

            for (int i = 0; i < openWindows.Count; i++)
            {
                var window = openWindows[i];
                window.WindowState = FormWindowState.Normal;
                window.SetBounds(
                    startX + (i * windowWidth),
                    screen.WorkingArea.Y,
                    windowWidth,
                    height
                );
            }

            MessageBox.Show($"{openWindows.Count} pencere dikey döşendi!", "Başarılı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Tüm pencereleri kapat (Window menüsünden)
        /// </summary>
        private void closeAllWindowsToolStripMenuItem_Click(object sender, EventArgs e)
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
                // Liste kopyasını oluştur (çünkü Close() çağrısı listeyi değiştirir)
                var windowsToClose = new List<CNC_Measurement>(openWindows);
                foreach (var window in windowsToClose)
                {
                    window.Close();
                }
            }
        }
    }
}
