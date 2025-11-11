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
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Ridge Width modu aktifse ve TreeView değişikliği olduysa
            if (ridgeWidthHandler != null && ridgeWidthHandler.IsPointSelectionActive())
            {
                // Ridge Width modundan çık
                ridgeWidthHandler.DisablePointSelection();

                // Kullanıcıya bilgi ver
                MessageBox.Show(
                    "TreeView seçimi değişti!\n\n" +
                    "Ridge Width modu otomatik olarak kapatıldı.\n" +
                    "Yeni ölçüm başlatabilirsiniz.",
                    "Ridge Width - Mod Kapatıldı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                System.Diagnostics.Debug.WriteLine("⚠️ Ridge Width modu kapatıldı (TreeView değişikliği)");
            }
        }

        private void TreeViewManager_OnGenerateGCodeClicked(object sender, EventArgs e)
        {
            try
            {
                // TreeView'den değerleri al
                string machineName = treeViewManager.SelectedMachine;
                string probeName = treeViewManager.SelectedProbe;
                double zSafety = treeViewManager.ZSafetyDistance;
                int retract = treeViewManager.RetractDistance;

                // G-CODE oluştur (şimdilik basit bir örnek)
                string gcode = GenerateGCode(machineName, probeName, zSafety, retract);

                // Sonucu göster
                MessageBox.Show(gcode, "Generated G-CODE", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Debug.WriteLine("✅ G-CODE generated successfully");
                Debug.WriteLine($"Machine: {machineName}");
                Debug.WriteLine($"Probe: {probeName}");
                Debug.WriteLine($"Z Safety: {zSafety} mm");
                Debug.WriteLine($"Retract: {retract} mm");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating G-CODE: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"❌ G-CODE generation error: {ex.Message}");
            }
        }

        private string GenerateGCode(string machineName, string probeName, double zSafety, int retract)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("; Generated G-CODE");
            sb.AppendLine($"; Machine: {machineName}");
            sb.AppendLine($"; Probe: {probeName}");
            sb.AppendLine($"; Z Safety Distance: {zSafety} mm");
            sb.AppendLine($"; Retract Distance: {retract} mm");
            sb.AppendLine("; Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine();
            sb.AppendLine("G90 ; Absolute positioning");
            sb.AppendLine("G21 ; Units in mm");
            sb.AppendLine($"G0 Z{zSafety} ; Move to safety height");
            sb.AppendLine("M0 ; Program stop");

            return sb.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Boş ise kontrol etme
                if (string.IsNullOrWhiteSpace(txt_Form1_Retract.Text))
                {
                    return;
                }

                // Değeri kontrol et (1-10 mm arası tam sayı)
                if (int.TryParse(txt_Form1_Retract.Text, out int value))
                {
                    // 1-10 mm arası kontrol
                    if (value >= 1 && value <= 10)
                    {
                        // ✅ YENİ: Retract değiştiğinde SADECE AKTİF GRUBU temizle
                        if (selectionManager != null)
                        {
                            selectionManager.ClearActiveGroupPoints();
                            Debug.WriteLine("✅ Retract değişti - Aktif grup temizlendi");
                        }

                        // TreeView'deki Retract değerini güncelle
                        if (treeViewManager != null)
                        {
                            treeViewManager.UpdateRetractFromTextBox(value);
                        }

                        Debug.WriteLine($"✅ Retract TextBox güncellendi: {value} mm");
                    }
                    else if (value < 1)
                    {
                        Debug.WriteLine($"⚠️ Retract minimum 1 mm olmalı");
                    }
                    else if (value > 10)
                    {
                        Debug.WriteLine($"⚠️ Retract maksimum 10 mm olmalı");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Retract TextBox hatası: {ex.Message}");
            }
        }

        private void txt_Form1_Retract_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Sadece rakam, Backspace, Delete ve kontrol tuşlarına izin ver
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // Geçersiz karakteri engelle
                return;
            }

            // İki haneden fazla girişi engelle
            TextBox textBox = sender as TextBox;
            if (textBox != null && char.IsDigit(e.KeyChar))
            {
                // Seçili metin varsa, silinecek - izin ver
                if (textBox.SelectionLength > 0)
                {
                    return;
                }

                // Mevcut metin + yeni karakter = 2 haneden fazla mı?
                string futureText = textBox.Text + e.KeyChar;
                if (futureText.Length > 2)
                {
                    e.Handled = true; // 2 haneden fazla girişi engelle
                }
            }
        }

        private void txt_Form1_Retract_Leave(object sender, EventArgs e)
        {
            try
            {
                // Boş ise varsayılan değer
                if (string.IsNullOrWhiteSpace(txt_Form1_Retract.Text))
                {
                    txt_Form1_Retract.Text = "3";
                    Debug.WriteLine("✅ Retract boş bırakıldı, varsayılan değer: 3 mm");
                    return;
                }

                // Sayı mı kontrol et
                if (int.TryParse(txt_Form1_Retract.Text, out int value))
                {
                    // 1'den küçükse → 1 yap
                    if (value < 1)
                    {
                        txt_Form1_Retract.Text = "1";
                        Debug.WriteLine("✅ Retract < 1, düzeltildi: 1 mm");
                    }
                    // 10'dan büyükse → 10 yap
                    else if (value > 10)
                    {
                        txt_Form1_Retract.Text = "10";
                        Debug.WriteLine("✅ Retract > 10, düzeltildi: 10 mm");
                    }
                }
                else
                {
                    // Geçersiz değerse varsayılan
                    txt_Form1_Retract.Text = "3";
                    Debug.WriteLine("✅ Retract geçersiz, varsayılan değer: 3 mm");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Retract Leave hatası: {ex.Message}");
                txt_Form1_Retract.Text = "3";
            }
        }
    }
}
