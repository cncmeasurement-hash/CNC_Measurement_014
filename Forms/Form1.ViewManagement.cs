using System;
using _014.Utilities.UI;

namespace _014
{
    /// <summary>
    /// Form1 - VIEW MANAGEMENT
    /// PARTIAL CLASS 5/5: View modları ve kamera işlemleri
    /// ✅ UPDATED: InstructionPanel toggle eklendi
    /// </summary>
    public partial class CNC_Measurement
    {
        // ═══════════════════════════════════════════════════════════
        // DISPLAY MODE İŞLEMLERİ
        // ═══════════════════════════════════════════════════════════

        private void wireFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewManager.SetWireframeMode();
        }

        private void shadedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewManager.SetShadedMode();
        }

        private void renderedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewManager.SetRenderedMode();
        }

        private void hiddenLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewManager.SetHiddenLineMode();
        }

        // ═══════════════════════════════════════════════════════════
        // CAMERA VIEW İŞLEMLERİ
        // ═══════════════════════════════════════════════════════════

        private void izometricToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewManager.SetIsometricView();
            design1.ZoomFit();
        }

        private void upToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewManager.SetTopView();
            design1.ZoomFit();
        }

        private void bottomToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            viewManager.SetBottomView();
            design1.ZoomFit();
        }

        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewManager.SetLeftView();
            design1.ZoomFit();
        }

        private void rigthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewManager.SetRightView();
            design1.ZoomFit();
        }

        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewManager.SetBackView();
            design1.ZoomFit();
        }

        private void bottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewManager.SetFrontView();
            design1.ZoomFit();
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ YENİ: INSTRUCTION PANEL TOGGLE
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// View → Instruction Panel menü click handler
        /// Panel'i göster/gizle toggle
        /// </summary>
        private void instructionPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (instructionPanel == null || instructionPanel.IsDisposed)
                {
                    // Panel yoksa oluştur
                    instructionPanel = new InstructionPanel(design1);
                    instructionPanel.Owner = this;
                    instructionPanel.Show();
                    instructionPanel.UpdateInstruction(InstructionTexts.WELCOME);

                    System.Diagnostics.Debug.WriteLine("✅ InstructionPanel gösterildi");
                }
                else
                {
                    // Panel varsa toggle yap
                    if (instructionPanel.Visible)
                    {
                        // Görünürse gizle
                        instructionPanel.Hide();
                        System.Diagnostics.Debug.WriteLine("🔒 InstructionPanel gizlendi");
                    }
                    else
                    {
                        // Gizliyse göster
                        instructionPanel.Show();
                        instructionPanel.BringToFront();
                        System.Diagnostics.Debug.WriteLine("🔓 InstructionPanel gösterildi");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ InstructionPanel toggle hatası: {ex.Message}");
            }
        }

    }
}
