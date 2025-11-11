using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _014.CNC.Machine;

namespace _014
{
    public partial class Form_CNC_Machines : Form
    {
        private List<MachineData> machines = new List<MachineData>();
        private int selectedIndex = -1;
        private bool isEditMode = false;
        private ContextMenuStrip gridContextMenu;

        public Form_CNC_Machines()
        {
            InitializeComponent();
            InitializeForm();
            LoadMachines();
        }

        private void InitializeForm()
        {
            cmb_cnc_control_system.Items.AddRange(new string[] {
                "Heidenhain", "Siemens", "Fanuc", "Mazak", "Haas", "Okuma", "Brother", "Mitsubishi"
            });

            // ✅ # KOLONUNU SİL
            RemoveHashColumn();

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;

            // ✅ SIRA NUMARASI İÇİN RowPostPaint EVENT'İ
            dataGridView1.RowPostPaint += DataGridView1_RowPostPaint;

            dataGridView1.CellClick += DataGridView1_CellClick;
            dataGridView1.KeyDown += DataGridView1_KeyDown;

            CreateContextMenu();
        }

        /// <summary>
        /// ✅ # KOLONUNU TAMAMEN SİL
        /// </summary>
        private void RemoveHashColumn()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Başlangıç: {dataGridView1.Columns.Count} kolon");

                // Tüm kolonları kontrol et
                for (int i = dataGridView1.Columns.Count - 1; i >= 0; i--)
                {
                    var col = dataGridView1.Columns[i];
                    System.Diagnostics.Debug.WriteLine($"  [{i}] Name='{col.Name}' Header='{col.HeaderText}'");

                    // # kolonunu veya boş kolonları sil
                    if (col.HeaderText == "#" || string.IsNullOrWhiteSpace(col.HeaderText))
                    {
                        System.Diagnostics.Debug.WriteLine($"  🗑️ Siliniyor: [{i}] '{col.HeaderText}'");
                        dataGridView1.Columns.RemoveAt(i);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Temizlik bitti. Kalan: {dataGridView1.Columns.Count} kolon");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RemoveHashColumn hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ SIRA NUMARASINI RowHeader'a YAZ
        /// </summary>
        private void DataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(
                e.RowBounds.Left,
                e.RowBounds.Top,
                grid.RowHeadersWidth,
                e.RowBounds.Height
            );

            e.Graphics.DrawString(
                rowIdx,
                this.Font,
                SystemBrushes.ControlText,
                headerBounds,
                centerFormat
            );
        }

        private void CreateContextMenu()
        {
            gridContextMenu = new ContextMenuStrip();

            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem
            {
                Text = "🗑️ Sil",
                ShortcutKeys = Keys.Delete
            };
            deleteMenuItem.Click += DeleteMenuItem_Click;

            ToolStripMenuItem editMenuItem = new ToolStripMenuItem
            {
                Text = "✏️ Düzenle"
            };
            editMenuItem.Click += EditMenuItem_Click;

            gridContextMenu.Items.Add(editMenuItem);
            gridContextMenu.Items.Add(new ToolStripSeparator());
            gridContextMenu.Items.Add(deleteMenuItem);

            dataGridView1.ContextMenuStrip = gridContextMenu;
        }

        private void LoadMachines()
        {
            try
            {
                machines = MachineStorage.LoadFromJson();
                RefreshGrid();
                System.Diagnostics.Debug.WriteLine($"✅ {machines.Count} makine yüklendi");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Makine listesi yüklenirken hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshGrid()
        {
            dataGridView1.Rows.Clear();

            // ✅ SADECE 2 KOLON: Name, Control System
            foreach (var machine in machines)
            {
                dataGridView1.Rows.Add(
                    machine.MachineName,      // Name
                    machine.ControlSystem     // Control System
                );
            }

            System.Diagnostics.Debug.WriteLine($"📊 Grid yenilendi: {machines.Count} kayıt");
        }

        private void btn_new_machine_save_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show("⚠️ Makine adı boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Focus();
                    return;
                }

                if (cmb_cnc_control_system.SelectedIndex == -1)
                {
                    MessageBox.Show("⚠️ Kontrol sistemi seçiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmb_cnc_control_system.Focus();
                    return;
                }

                string machineName = textBox1.Text.Trim();
                string controlSystem = cmb_cnc_control_system.SelectedItem.ToString();

                if (isEditMode && selectedIndex >= 0)
                {
                    bool duplicateExists = machines
                        .Where((m, index) => index != selectedIndex)
                        .Any(m => m.MachineName.Equals(machineName, StringComparison.OrdinalIgnoreCase));

                    if (duplicateExists)
                    {
                        MessageBox.Show($"⚠️ '{machineName}' isimli makine zaten var!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBox1.Focus();
                        return;
                    }

                    machines[selectedIndex].MachineName = machineName;
                    machines[selectedIndex].ControlSystem = controlSystem;
                    MachineStorage.SaveToJson(machines);
                    RefreshGrid();
                    ClearForm();
                    MessageBox.Show("✅ Makine güncellendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    bool duplicateExists = machines.Any(m => m.MachineName.Equals(machineName, StringComparison.OrdinalIgnoreCase));

                    if (duplicateExists)
                    {
                        MessageBox.Show($"⚠️ '{machineName}' isimli makine zaten var!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBox1.Focus();
                        return;
                    }

                    var newMachine = new MachineData
                    {
                        MachineName = machineName,
                        ControlSystem = controlSystem,
                        Coordinates = "X, Y, Z",
                        ToolNumber = 1
                    };

                    machines.Add(newMachine);
                    MachineStorage.SaveToJson(machines);
                    RefreshGrid();
                    ClearForm();
                    MessageBox.Show("✅ Makine eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.RowIndex < machines.Count)
                {
                    selectedIndex = e.RowIndex;
                    var machine = machines[selectedIndex];
                    isEditMode = true;
                    textBox1.Text = machine.MachineName;
                    cmb_cnc_control_system.SelectedItem = machine.ControlSystem;
                }
            }
            catch { }
        }

        private void DataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedMachine();
                e.Handled = true;
            }
        }

        private void EditMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int rowIndex = dataGridView1.SelectedRows[0].Index;
                if (rowIndex >= 0 && rowIndex < machines.Count)
                {
                    selectedIndex = rowIndex;
                    var machine = machines[selectedIndex];
                    isEditMode = true;
                    textBox1.Text = machine.MachineName;
                    cmb_cnc_control_system.SelectedItem = machine.ControlSystem;
                    textBox1.Focus();
                }
            }
        }

        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedMachine();
        }

        private void DeleteSelectedMachine()
        {
            try
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("⚠️ Silinecek makineyi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int rowIndex = dataGridView1.SelectedRows[0].Index;

                if (rowIndex >= 0 && rowIndex < machines.Count)
                {
                    var machine = machines[rowIndex];

                    var result = MessageBox.Show(
                        $"'{machine.MachineName}' makinesini silmek istediğinize emin misiniz?\n\nBu işlem geri alınamaz!",
                        "Silme Onayı",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        machines.RemoveAt(rowIndex);
                        MachineStorage.SaveToJson(machines);
                        RefreshGrid();
                        ClearForm();
                        MessageBox.Show("✅ Makine silindi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Silme hatası:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            textBox1.Clear();
            cmb_cnc_control_system.SelectedIndex = -1;
            selectedIndex = -1;
            isEditMode = false;
            textBox1.Focus();
        }
    }
}