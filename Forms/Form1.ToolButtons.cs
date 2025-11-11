using _014.Probe.CMM;
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
        private void cMMProbePathTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Test 1: Basit 3 nokta
            CMM_ProbePathTest.Test_SimpleThreePoints(design1);

            // VEYA Test 2: Daire ölçümü - yorum satırını kaldırın
            // CMM_ProbePathTest.Test_CircleMeasurement(design1);

            // VEYA Test 3: Grid tarama - yorum satırını kaldırın
            // CMM_ProbePathTest.Test_GridScan(design1);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
        }

        private void pictureBoxplay_Click(object sender, EventArgs e)
        {
            try
            {
                if (toolpathManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ toolpathManager NULL!");
                    MessageBox.Show("Please generate toolpath first!", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Simülasyon çalışıyor mu kontrol et
                if (toolpathManager.IsSimulationRunning)
                {
                    // PAUSE ⏸️
                    toolpathManager.PauseSimulation();
                    System.Diagnostics.Debug.WriteLine("⏸️ Simülasyon duraklatıldı (pictureBoxplay)");
                }
                else
                {
                    // PLAY ▶️
                    // Eğer hiç başlatılmamışsa StartSimulation çağır
                    // Eğer duraklıysa ResumeSimulation çağır

                    // İlk kez mi başlatılıyor kontrol et (animationPath var mı?)
                    // Bunun için toolpathManager'dan bir kontrol gerekebilir
                    // Şimdilik StartSimulation + ResumeSimulation kombinasyonu kullanalım

                    toolpathManager.ResumeSimulation();

                    // Eğer resume çalışmadıysa (henüz başlatılmamış), start dene
                    if (!toolpathManager.IsSimulationRunning)
                    {
                        toolpathManager.StartSimulation();
                    }

                    System.Diagnostics.Debug.WriteLine("▶️ Simülasyon başlatıldı/devam ediyor (pictureBoxplay)");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ pictureBoxplay_Click hatası: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Simulation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox_creat_toolpath_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎯 CREATE TOOLPATH butonu tıklandı!");

                if (toolpathManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ toolpathManager NULL!");
                    MessageBox.Show("Toolpath manager not initialized!", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Toolpath oluştur
                toolpathManager.GenerateToolpath();

                System.Diagnostics.Debug.WriteLine("✅ Toolpath oluşturuldu (pictureBox_creat_toolpath)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Toolpath oluşturma hatası: {ex.Message}");
                MessageBox.Show($"Error creating toolpath: {ex.Message}", "Toolpath Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox24_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎯 CREATE TOOLPATH butonu tıklandı!");

                if (toolpathManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ toolpathManager NULL!");
                    MessageBox.Show("Toolpath manager not initialized!", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Toolpath oluştur
                toolpathManager.GenerateToolpath();

                System.Diagnostics.Debug.WriteLine("✅ Toolpath oluşturuldu (pictureBox_creat_toolpath)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Toolpath oluşturma hatası: {ex.Message}");
                MessageBox.Show($"Error creating toolpath: {ex.Message}", "Toolpath Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void pictureBox_Creat_GCODE_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎯 G-CODE GENERATOR butonu tıklandı!");

                if (treeViewManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ treeViewManager NULL!");
                    MessageBox.Show("TreeView manager not initialized!", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // TreeView'den değerleri al
                string machineName = treeViewManager.SelectedMachine;
                string probeName = treeViewManager.SelectedProbe;
                double zSafety = treeViewManager.ZSafetyDistance;
                int retract = treeViewManager.RetractDistance;

                // G-CODE oluştur
                string gcode = GenerateGCode(machineName, probeName, zSafety, retract);

                // Sonucu göster
                MessageBox.Show(gcode, "Generated G-CODE", MessageBoxButtons.OK, MessageBoxIcon.Information);

                System.Diagnostics.Debug.WriteLine("✅ G-CODE oluşturuldu (pictureBox_Creat_GCODE)");
                System.Diagnostics.Debug.WriteLine($"Machine: {machineName}");
                System.Diagnostics.Debug.WriteLine($"Probe: {probeName}");
                System.Diagnostics.Debug.WriteLine($"Z Safety: {zSafety} mm");
                System.Diagnostics.Debug.WriteLine($"Retract: {retract} mm");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ G-CODE oluşturma hatası: {ex.Message}");
                MessageBox.Show($"Error generating G-CODE: {ex.Message}", "G-CODE Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (toolpathManager == null)
                {
                    return;
                }

                // TrackBar değerini simülasyon hızına dönüştür (2x FASTER!)
                int trackValue = trackBar1.Value;
                double speed;

                switch (trackValue)
                {
                    case 0: speed = 0.5; break;
                    case 1: speed = 1.0; break;
                    case 2: speed = 1.5; break;
                    case 3: speed = 2.0; break;
                    case 4: speed = 3.0; break;
                    case 5: speed = 4.0; break;
                    case 6: speed = 6.0; break;
                    case 7: speed = 8.0; break;
                    case 8: speed = 10.0; break;
                    case 9: speed = 15.0; break;
                    case 10: speed = 20.0; break;
                    default: speed = 1.0; break;
                }

                // Simülasyon hızını ayarla
                toolpathManager.SetSimulationSpeed(speed);

                System.Diagnostics.Debug.WriteLine($"🎚️ Simülasyon hızı değişti: {speed}x (TrackBar: {trackValue})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ TrackBar hata: {ex.Message}");
            }
        }

        private void pictureBox22_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("⏹️ STOP butonu tıklandı!");

                if (toolpathManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ toolpathManager NULL!");
                    return;
                }

                // Simülasyonu durdur ve sona erdir
                toolpathManager.StopSimulation();

                System.Diagnostics.Debug.WriteLine("✅ Simülasyon durduruldu ve sona erdirildi (pictureBox22)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ STOP butonu hatası: {ex.Message}");
                MessageBox.Show($"Error stopping simulation: {ex.Message}", "Stop Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox_CMM_point_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎯 CMM Point butonu tıklandı!");

                if (treeViewManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ treeViewManager NULL!");
                    MessageBox.Show("TreeView manager not initialized!", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ✅ Angle Measurement modunu kapat
                angleMeasurementManager?.Disable();

                // Point Probing modunu aktif et
                treeViewManager.AddProbingPoint();

                System.Diagnostics.Debug.WriteLine("✅ Point Probing modu aktif edildi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CMM Point butonu hatası: {ex.Message}");
                MessageBox.Show($"Error activating Point Probing: {ex.Message}", "CMM Point Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            // ✅ Point Probing modunu kapat
            selectionManager?.DisablePointProbing();
            
            // ✅ Angle Measurement modunu kapat
            angleMeasurementManager?.Disable();
            
            // Ridge Width modunu aktif et
            ridgeWidthHandler?.CreateNewRidgeWidthGroup();

        }
    }
}
