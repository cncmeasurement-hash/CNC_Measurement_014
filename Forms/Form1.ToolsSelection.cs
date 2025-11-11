using System;
using System.Drawing;
using System.Windows.Forms;
using _014.Managers.Selection;

namespace _014
{
    /// <summary>
    /// Form1 - TOOLS & SELECTION
    /// PARTIAL CLASS 4/5: Probe, selection, animation ve diğer tool işlemleri
    /// </summary>
    public partial class CNC_Measurement
    {
        // ═══════════════════════════════════════════════════════════
        // PROBE İŞLEMLERİ
        // ═══════════════════════════════════════════════════════════

        // ✅ KALDIRILDI - Sol panelden Probe dropdown ile erişilebilir
        /*
        private void addNewProbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_New_Prob formNewProb = new Form_New_Prob();
            formNewProb.ShowDialog();

            // ✅ Form kapandığında seçili probu DataManager'a kaydet
            var selectedProbe = formNewProb.GetCurrentProbe();
            if (selectedProbe != null)
            {
                selectionManager.GetDataManager().SetSelectedProbe(selectedProbe);
                System.Diagnostics.Debug.WriteLine($"✅ Seçili prob kaydedildi: {selectedProbe.Name}, D={selectedProbe.D}mm");
            }
        }
        */

        // ═══════════════════════════════════════════════════════════
        // SELECTION İŞLEMLERİ
        // ═══════════════════════════════════════════════════════════

        private void selectFacesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isActive = selectionManager.IsEnabled() &&
                           selectionManager.GetCurrentMode() == SelectionManager.SelectionMode.Face;

            if (isActive)
            {
                selectionManager.EnableFaceSelection(false);
            }
            else
            {
                selectionManager.EnableFaceSelection(true);
            }
        }

        // ✅ YENİ: Nokta seçimi
        private void selectPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isActive = selectionManager.IsEnabled() &&
                           selectionManager.GetCurrentMode() == SelectionManager.SelectionMode.Point;

            if (isActive)
            {
                selectionManager.EnablePointSelection(false);
            }
            else
            {
                selectionManager.EnablePointSelection(true);
            }
        }

        // ✅ YENİ: Point marker'ları temizle
        private void clearPointMarkersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectionManager.ClearPointMarkers();
        }

        private void selectEntitiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isActive = selectionManager.IsEnabled() &&
                           selectionManager.GetCurrentMode() == SelectionManager.SelectionMode.Entity;

            if (isActive)
            {
                selectionManager.EnableEntitySelection(false);
            }
            else
            {
                selectionManager.EnableEntitySelection(true);
            }
        }

        private void showSelectionInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectionManager.ShowSelectedFacesInfo();
        }

        private void changeSelectionColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = Color.Red;
                cd.FullOpen = true;

                if (cd.ShowDialog() == DialogResult.OK)
                {
                    selectionManager.ChangeSelectedFacesColor(cd.Color);
                }
            }
        }

        private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectionManager.ClearSelection();
            MessageBox.Show("Seçimler temizlendi.", "Bilgi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ✅ YENİ: Seçili yüzeylerin normallerini göster
        private void showFaceNormalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectionManager != null)
            {
                selectionManager.ShowSelectedFaceNormals();
            }
            else
            {
                MessageBox.Show("Selection Manager başlatılmadı!", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ANIMATION İŞLEMLERİ
        // ═══════════════════════════════════════════════════════════

        private void startAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (probeAnimator.IsAnimating)
            {
                probeAnimator.StopAnimation();
                MessageBox.Show("Prob animasyonu durduruldu.", "Animasyon",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                int greenCount = probeAnimator.CountGreenEntities();
                probeAnimator.StartAnimation();
                MessageBox.Show($"Prob animasyonu başlatıldı!\nYeşil entity: {greenCount}",
                    "Animasyon", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // DİĞER TOOL İŞLEMLERİ
        // ═══════════════════════════════════════════════════════════

        private void passwordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var passwordForm = new password();
                passwordForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ KALDIRILDI - Sol panelden CNC Machine dropdown ile erişilebilir
        /*
        private void macineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🏭 CNC Machines formu açılıyor...");
                var machinesForm = new Form_CNC_Machines();
                machinesForm.ShowDialog();
                System.Diagnostics.Debug.WriteLine("✅ CNC Machines formu kapatıldı");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ CNC Machines formu açılırken hata:\n\n{ex.Message}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"❌ CNC Machines hatası: {ex.Message}");
            }
        }
        */

        // ═══════════════════════════════════════════════════════════
        // DUPLICATE EVENT HANDLERS (Aynı metodları çağırıyor)
        // ═══════════════════════════════════════════════════════════

        private void toSurfaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectFacesToolStripMenuItem_Click(sender, e);
        }

        private void selectEntitiesToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            selectEntitiesToolStripMenuItem_Click(sender, e);
        }

        private void showSelectionInfoToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            showSelectionInfoToolStripMenuItem_Click(sender, e);
        }

        private void changeSelectionColorToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            changeSelectionColorToolStripMenuItem_Click(sender, e);
        }

        private void clearSelectionToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            clearSelectionToolStripMenuItem_Click(sender, e);
        }
    }
}
