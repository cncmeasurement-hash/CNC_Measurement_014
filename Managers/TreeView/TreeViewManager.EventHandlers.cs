using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _014.CNC.Machine;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 5/6: EventHandlers - Event handling, deletion, machine/probe management
    /// </summary>
    public partial class TreeViewManager
    {
        /// <summary>
        /// Sağ tık menü göster
        /// </summary>
        private void TreeView_NodeMouseRightClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            string tag = e.Node.Tag?.ToString();

            // ✅ Point node'ları için context menu
            if (e.Node.Tag is Point3D point)
            {
                ContextMenuStrip pointContextMenu = new ContextMenuStrip();
                
                ToolStripMenuItem deleteItem = new ToolStripMenuItem("🗑️ Delete Point");
                deleteItem.Click += (s, ev) =>
                {
                    DialogResult result = MessageBox.Show(
                        $"Bu probe noktasını silmek istediğinize emin misiniz?\n\n{e.Node.Text}",
                        "Probe Point Sil",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        DeleteProbePoint(e.Node, point);
                    }
                };
                
                pointContextMenu.Items.Add(deleteItem);
                pointContextMenu.Show(treeView, e.Location);
                return;
            }
            
            // ✅ YENİ: Ridge Width grupları için context menu
            if (tag == "RIDGE_WIDTH_GROUP")
            {
                ContextMenuStrip ridgeWidthGroupContextMenu = new ContextMenuStrip();
                
                ToolStripMenuItem deleteGroupItem = new ToolStripMenuItem("🗑️ Delete Group");
                deleteGroupItem.Click += (s, ev) =>
                {
                    DialogResult result = MessageBox.Show(
                        $"Bu Ridge Width grubunu ve içindeki tüm noktaları silmek istediğinize emin misiniz?\n\n{e.Node.Text}",
                        "Ridge Width Group Sil",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.Yes)
                    {
                        DeleteRidgeWidthGroup(e.Node);
                    }
                };
                
                ridgeWidthGroupContextMenu.Items.Add(deleteGroupItem);
                ridgeWidthGroupContextMenu.Show(treeView, e.Location);
                return;
            }
            
            // ✅ YENİ: Grup node'ları için context menu
            if (tag != null && tag.StartsWith("PROBE_GROUP_"))
            {
                ContextMenuStrip groupContextMenu = new ContextMenuStrip();
                
                ToolStripMenuItem deleteGroupItem = new ToolStripMenuItem("🗑️ Delete Group");
                deleteGroupItem.Click += (s, ev) =>
                {
                    DialogResult result = MessageBox.Show(
                        $"Bu grubu ve içindeki tüm probe noktalarını silmek istediğinize emin misiniz?\n\n{e.Node.Text}",
                        "Probe Group Sil",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.Yes)
                    {
                        DeleteProbeGroup(e.Node);
                    }
                };
                
                groupContextMenu.Items.Add(deleteGroupItem);
                groupContextMenu.Show(treeView, e.Location);
                return;
            }

            switch (tag)
            {
                case "MACHINE":
                case "MACHINE_INFO":
                    machineContextMenu.Show(treeView, e.Location);
                    break;

                case "PROBE":
                case "PROBE_INFO":
                    probeContextMenu.Show(treeView, e.Location);
                    break;
            }
        }

        /// <summary>
        /// ✅ TreeView'de DELETE tuşu basıldı
        /// </summary>
        private void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;
            
            try
            {
                TreeNode selectedNode = treeView.SelectedNode;
                if (selectedNode == null) return;
                
                string tag = selectedNode.Tag?.ToString();
                
                // ✅ Grup node'u seçiliyse
                if (tag != null && tag.StartsWith("PROBE_GROUP_"))
                {
                    DialogResult result = MessageBox.Show(
                        $"Bu grubu ve içindeki tüm probe noktalarını silmek istediğinize emin misiniz?\n\n{selectedNode.Text}",
                        "Probe Group Sil",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.Yes)
                    {
                        DeleteProbeGroup(selectedNode);
                        e.Handled = true;
                    }
                    return;
                }
                
                // ✅ Point node'u seçiliyse
                if (selectedNode.Tag is Point3D point)
                {
                    // Kullanıcıya sor
                    DialogResult result = MessageBox.Show(
                        $"Bu probe noktasını silmek istediğinize emin misiniz?\n\n{selectedNode.Text}",
                        "Probe Point Sil",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        DeleteProbePoint(selectedNode, point);
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ TreeView_KeyDown hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ✅ TreeView'de node seçildiğinde (Marker highlight için)
        /// </summary>
        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                TreeNode selectedNode = e.Node;
                if (selectedNode == null) return;
                
                // Eğer Point node'u seçildiyse, marker'ı highlight et
                if (selectedNode.Tag is Point3D point)
                {
                    // Parent node'un grup ID'sini al
                    TreeNode groupNode = selectedNode.Parent;
                    if (groupNode != null && groupNode.Tag?.ToString().StartsWith("PROBE_GROUP_") == true)
                    {
                        string groupTag = groupNode.Tag.ToString();
                        int groupId = int.Parse(groupTag.Replace("PROBE_GROUP_", ""));
                        
                        // Handler'dan marker'ı highlight et
                        var handler = selectionManager.GetPointProbingHandler(groupId);
                        if (handler != null)
                        {
                            handler.HighlightMarker(point);
                        }
                    }
                }
                else
                {
                    // Point dışında bir şey seçildiyse, tüm highlight'ları temizle
                    ClearAllHighlights();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ TreeView_AfterSelect hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ✅ Tüm marker highlight'larını temizle
        /// </summary>
        private void ClearAllHighlights()
        {
            try
            {
                // Tüm grupları gez
                foreach (TreeNode groupNode in probePointsGroups)
                {
                    if (groupNode.Tag?.ToString().StartsWith("PROBE_GROUP_") == true)
                    {
                        string groupTag = groupNode.Tag.ToString();
                        int groupId = int.Parse(groupTag.Replace("PROBE_GROUP_", ""));
                        
                        var handler = selectionManager.GetPointProbingHandler(groupId);
                        if (handler != null)
                        {
                            handler.ClearHighlight();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ClearAllHighlights hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ✅ Probe point'i sil (TreeView + 3D view)
        /// </summary>
        private void DeleteProbePoint(TreeNode node, Point3D point)
        {
            try
            {
                if (node == null || point == null) return;
                
                TreeNode groupNode = node.Parent;
                if (groupNode == null || !groupNode.Tag?.ToString().StartsWith("PROBE_GROUP_") == true)
                {
                    return;
                }
                
                // Grup ID'sini al
                string groupTag = groupNode.Tag.ToString();
                int groupId = int.Parse(groupTag.Replace("PROBE_GROUP_", ""));
                
                // Handler'dan sil (3D view'den marker + çizgiler)
                var handler = selectionManager.GetPointProbingHandler(groupId);
                if (handler != null)
                {
                    handler.DeletePointByCoordinate(point);
                }
                
                // TreeView'den sil
                groupNode.Nodes.Remove(node);
                
                // Kalan noktaları yeniden numaralandır
                RenumberGroupPoints(groupNode);
                
                treeView.Refresh();
                
                System.Diagnostics.Debug.WriteLine($"✅ Probe point silindi: X={point.X:F2}, Y={point.Y:F2}, Z={point.Z:F2}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DeleteProbePoint hatası: {ex.Message}");
                MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// ✅ Probe grubunu tamamen sil (TreeView + 3D view)
        /// </summary>
        private void DeleteProbeGroup(TreeNode groupNode)
        {
            try
            {
                if (groupNode == null) return;
                
                string tag = groupNode.Tag?.ToString();
                if (tag == null || !tag.StartsWith("PROBE_GROUP_"))
                {
                    return;
                }
                
                // Grup ID'sini al
                int groupId = int.Parse(tag.Replace("PROBE_GROUP_", ""));
                
                // Handler'ı al
                var handler = selectionManager.GetPointProbingHandler(groupId);
                if (handler != null)
                {
                    // Tüm marker + çizgileri sil
                    handler.ClearAllPoints();
                }
                
                // TreeView'den grup node'unu sil
                methodNode.Nodes.Remove(groupNode);
                
                // probePointsGroups listesinden kaldır
                probePointsGroups.Remove(groupNode);
                
                treeView.Refresh();
                
                System.Diagnostics.Debug.WriteLine($"✅ Probe grubu silindi: {groupNode.Text}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DeleteProbeGroup hatası: {ex.Message}");
                MessageBox.Show($"Grup silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ridge Width grubunu sil
        /// </summary>
        private void DeleteRidgeWidthGroup(TreeNode groupNode)
        {
            try
            {
                if (groupNode == null) return;
                
                string tag = groupNode.Tag?.ToString();
                if (tag != "RIDGE_WIDTH_GROUP")
                {
                    return;
                }
                
                // Grup isminden numarayı al (örn: "Ridge Width 1" -> 1)
                string groupText = groupNode.Text;
                int groupNumber = -1;
                
                // ✅ Grup numarasını parse et
                if (groupText.StartsWith("Ridge Width "))
                {
                    string numStr = groupText.Replace("Ridge Width ", "");
                    int.TryParse(numStr, out groupNumber);
                }
                
                if (groupNumber <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Geçersiz grup numarası!");
                    return;
                }
                
                // ✅ 1. TreeView'den tüm child node'ları temizle
                groupNode.Nodes.Clear();
                
                // ✅ 2. TreeView'den grup node'unu sil
                methodNode.Nodes.Remove(groupNode);
                
                // ✅ 3. Design'dan SADECE BU GRUBUN Ridge Width marker'larını ve ölçü çizgilerini sil
                var entitiesToRemove = new List<devDept.Eyeshot.Entities.Entity>();
                
                foreach (var entity in design.Entities)
                {
                    // RidgeWidthPoints, RidgeWidthMeasurements veya RidgeWidthProbe layer'ındaki entity'leri kontrol et
                    if (entity.LayerName == "RidgeWidthPoints" || 
                        entity.LayerName == "RidgeWidthMeasurements" ||
                        entity.LayerName == "RidgeWidthProbe")
                    {
                        // ✅ EntityData kontrolü - SADECE BU GRUBA AİT Mİ?
                        if (entity.EntityData != null && entity.EntityData is string entityTag)
                        {
                            if (entityTag.Contains($"RidgeWidth_{groupNumber}"))
                            {
                                entitiesToRemove.Add(entity);
                                System.Diagnostics.Debug.WriteLine($"  🗑️ Silinecek: {entity.GetType().Name} - Tag: {entityTag}");
                            }
                        }
                    }
                }
                
                // Entity'leri sil
                foreach (var entity in entitiesToRemove)
                {
                    design.Entities.Remove(entity);
                }
                
                if (entitiesToRemove.Count > 0)
                {
                    design.Invalidate();
                    System.Diagnostics.Debug.WriteLine($"✅ {entitiesToRemove.Count} entity silindi (Ridge Width layer'ları)");
                }
                
                treeView.Refresh();
                
                System.Diagnostics.Debug.WriteLine($"✅ Ridge Width grubu silindi: {groupText}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DeleteRidgeWidthGroup hatası: {ex.Message}");
                MessageBox.Show($"Ridge Width grup silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Yeni makina ekle
        /// </summary>
        private void AddNewMachine()
        {
            string newMachine = ShowInputDialog("Enter new machine name:", "Add New Machine", "New Machine");

            if (!string.IsNullOrWhiteSpace(newMachine) && !machines.Contains(newMachine))
            {
                try
                {
                    // JSON'dan mevcut listeyi yükle
                    var machineDataList = MachineStorage.LoadFromJson();
                    
                    // Yeni makineyi ekle
                    var newMachineData = new MachineData
                    {
                        MachineName = newMachine,
                        ControlSystem = "Fanuc", // Default
                        Coordinates = "X, Y, Z",
                        ToolNumber = 1
                    };
                    machineDataList.Add(newMachineData);
                    
                    // JSON'a kaydet
                    MachineStorage.SaveToJson(machineDataList);
                    
                    // TreeView listesini güncelle
                    machines.Add(newMachine);
                    
                    MessageBox.Show($"Machine '{newMachine}' added successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    System.Diagnostics.Debug.WriteLine($"✅ TreeView: Yeni makine eklendi: {newMachine}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding machine: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    System.Diagnostics.Debug.WriteLine($"❌ TreeView makine ekleme hatası: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Makina sil
        /// </summary>
        private void DeleteMachine()
        {
            if (machines.Count <= 1)
            {
                MessageBox.Show("Cannot delete the last machine!", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"Delete machine '{SelectedMachine}'?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // JSON'dan mevcut listeyi yükle
                    var machineDataList = MachineStorage.LoadFromJson();
                    
                    // Makineyi bul ve sil
                    var machineToDelete = machineDataList.FirstOrDefault(m => m.MachineName == SelectedMachine);
                    if (machineToDelete != null)
                    {
                        machineDataList.Remove(machineToDelete);
                        
                        // JSON'a kaydet
                        MachineStorage.SaveToJson(machineDataList);
                        
                        // TreeView listesini güncelle
                        machines.Remove(SelectedMachine);
                        SelectedMachine = machines[0];
                        // ✅ KALDIRILDI - Machine Name artık TreeView'de gösterilmiyor
                        /*
                        string machineLabel = "Machine Name".PadRight(15);
                        machineNode.Text = $"{machineLabel}: {SelectedMachine} ▼";
                        */
                        
                        MessageBox.Show("Machine deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        System.Diagnostics.Debug.WriteLine($"✅ TreeView: Makine silindi: {machineToDelete.MachineName}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting machine: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    System.Diagnostics.Debug.WriteLine($"❌ TreeView makine silme hatası: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Yeni probe ekle
        /// </summary>
        private void AddNewProbe()
        {
            string newProbe = ShowInputDialog("Enter new probe name:", "Add New Probe", "New Probe");

            if (!string.IsNullOrWhiteSpace(newProbe) && !probes.Contains(newProbe))
            {
                probes.Add(newProbe);
                MessageBox.Show($"Probe '{newProbe}' added successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Probe sil
        /// </summary>
        private void DeleteProbe()
        {
            if (probes.Count <= 1)
            {
                MessageBox.Show("Cannot delete the last probe!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"Delete probe '{SelectedProbe}'?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                probes.Remove(SelectedProbe);
                SelectedProbe = probes[0];
                // ✅ KALDIRILDI - Probe Name artık TreeView'de gösterilmiyor
                /*
                string probeLabel = "Probe Name".PadRight(15);
                probeNode.Text = $"{probeLabel}: {SelectedProbe} ▼";
                */
                MessageBox.Show("Probe deleted successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Basit input dialog göster
        /// </summary>
        private string ShowInputDialog(string text, string caption, string defaultValue)
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Text = text, Width = 350 };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 350, Text = defaultValue };
            Button confirmation = new Button() { Text = "OK", Left = 220, Width = 70, Top = 80, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Cancel", Left = 300, Width = 70, Top = 80, DialogResult = DialogResult.Cancel };

            confirmation.Click += (sender, e) => { prompt.Close(); };
            cancel.Click += (sender, e) => { prompt.Close(); };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        /// <summary>
        /// Clearance Plane'i göster/gizle (Z Safety sağ tık menüsünden)
        /// </summary>
        private void ToggleClearancePlane()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎯 ToggleClearancePlane çağrıldı!");
                
                // Form1'deki clearancePlaneManager'a erişim
                var form = ownerForm as CNC_Measurement;
                System.Diagnostics.Debug.WriteLine($"   Form cast: {form != null}");
                
                if (form != null)
                {
                    // Reflection ile clearancePlaneManager'a eriş
                    var fieldInfo = form.GetType().GetField("clearancePlaneManager",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    System.Diagnostics.Debug.WriteLine($"   FieldInfo bulundu: {fieldInfo != null}");
                    
                    if (fieldInfo != null)
                    {
                        var manager = fieldInfo.GetValue(form);
                        System.Diagnostics.Debug.WriteLine($"   Manager bulundu: {manager != null}");
                        
                        if (manager != null)
                        {
                            var toggleMethod = manager.GetType().GetMethod("ToggleLayerVisibility");
                            System.Diagnostics.Debug.WriteLine($"   ToggleMethod bulundu: {toggleMethod != null}");
                            
                            toggleMethod?.Invoke(manager, null);
                            System.Diagnostics.Debug.WriteLine("   ✅ ToggleLayerVisibility çağrıldı!");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("   ❌ Manager null!");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("   ❌ FieldInfo bulunamadı!");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("   ❌ Form cast başarısız!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Toggle Clearance Plane hatası: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Hata: {ex.Message}", "Clearance Plane Toggle");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PROBING METHODS (11 TYPES)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// 1. Point Probing - Tek nokta ölçümü
        /// </summary>
    }
}
