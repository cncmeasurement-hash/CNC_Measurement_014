using _014.CNC.Machine;
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
        private void LoadCNCMachines()
        {
            try
            {
                // Önceki seçimi sakla
                string previousSelection = cmb_form1_cnc_machine.SelectedItem?.ToString();

                // JSON'dan makineleri yükle
                List<MachineData> machines = MachineStorage.LoadFromJson();

                // ComboBox'ı temizle
                cmb_form1_cnc_machine.Items.Clear();

                // Makine isimlerini ekle
                foreach (var machine in machines)
                {
                    cmb_form1_cnc_machine.Items.Add(machine.MachineName);
                }

                Debug.WriteLine($"✅ {machines.Count} CNC makinesi yüklendi (Sol panel dropdown)");

                // Önceki seçimi geri yükle (eğer hala listede varsa)
                if (!string.IsNullOrEmpty(previousSelection) && cmb_form1_cnc_machine.Items.Contains(previousSelection))
                {
                    cmb_form1_cnc_machine.SelectedItem = previousSelection;
                }
                // Yoksa ilk makineyi seç
                else if (cmb_form1_cnc_machine.Items.Count > 0)
                {
                    cmb_form1_cnc_machine.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ CNC makineleri yüklenirken hata: {ex.Message}");
                MessageBox.Show($"CNC makineleri yüklenirken hata:\n{ex.Message}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmb_form1_cnc_machine_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmb_form1_cnc_machine.SelectedIndex >= 0)
                {
                    string selectedMachine = cmb_form1_cnc_machine.SelectedItem.ToString();

                    // TreeViewManager'a seçili makineyi bildir
                    if (treeViewManager != null)
                    {
                        treeViewManager.SetSelectedMachine(selectedMachine);
                    }

                    Debug.WriteLine($"✅ CNC Machine seçildi: {selectedMachine}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ CNC Machine seçimi hatası: {ex.Message}");
            }
        }

        private void InitializeCNCMachineContextMenu()
        {
            // Context menu oluştur
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            // "Add Machine" menü öğesi
            ToolStripMenuItem addMachineItem = new ToolStripMenuItem
            {
                Text = "Add Machine",
                Image = null // İsterseniz icon ekleyebilirsiniz
            };
            addMachineItem.Click += (s, e) => OpenCNCMachineForm();

            // Menüye ekle
            contextMenu.Items.Add(addMachineItem);

            // ComboBox'a context menu'yü ata
            cmb_form1_cnc_machine.ContextMenuStrip = contextMenu;

            Debug.WriteLine("✅ CNC Machine context menu oluşturuldu");
        }

        private void OpenCNCMachineForm()
        {
            try
            {
                Form_CNC_Machines machineForm = new Form_CNC_Machines();
                machineForm.ShowDialog(); // Modal olarak aç

                // Form kapandıktan sonra makine listesini yeniden yükle
                LoadCNCMachines();

                Debug.WriteLine("✅ CNC Machines formu kapatıldı ve liste yenilendi");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ CNC Machines formu açma hatası: {ex.Message}");
                MessageBox.Show($"Form açılırken hata:\n{ex.Message}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmb_form1_cnc_machine_DrawItem(object sender, DrawItemEventArgs e)
        {
            DrawComboBoxItemRightAligned(sender, e);
        }
    }
}
