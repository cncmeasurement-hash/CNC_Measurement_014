using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 4/6: UI - TreeView initialization, rendering, dropdowns, forms
    /// </summary>
    public partial class TreeViewManager
    {
        /// <summary>
        /// TreeView'i başlangıç durumuna getirir
        /// </summary>
        private void InitializeTreeView()
        {
            treeView.Nodes.Clear();

            // TreeView görünüm ayarları
            treeView.CheckBoxes = false;  // ✅ CheckBox sistemini KAPATTIK
            treeView.ShowLines = true;
            treeView.ShowPlusMinus = true;
            treeView.ShowRootLines = true;
            treeView.HideSelection = false;
            treeView.FullRowSelect = true;
            treeView.Font = new Font("Segoe UI", 9F);
            treeView.DrawMode = TreeViewDrawMode.OwnerDrawText; 
            treeView.DrawNode += TreeView_DrawNode;

            // İki nokta hizalama için padding (Space karakteri ile)
            int labelWidth = 15;

            // ✅ KALDIRILDI - Machine Name artık gösterilmiyor
            /*
            // Machine Name
            string machineLabel = "Machine Name".PadRight(labelWidth);
            machineNode = new TreeNode($"{machineLabel}: {SelectedMachine} ▼")
            {
                Tag = "MACHINE",
                ForeColor = Color.Black
            };

            var machineInfoNode = new TreeNode("   [Sağ Tık: CNC Machines]")
            {
                Tag = "MACHINE_INFO",
                ForeColor = Color.Black,
                NodeFont = new Font("Segoe UI", 8F, FontStyle.Italic)
            };
            machineNode.Nodes.Add(machineInfoNode);
            */

            // ✅ KALDIRILDI - Probe Name artık gösterilmiyor
            /*
            // Probe Name
            string probeLabel = "Probe Name".PadRight(labelWidth);
            probeNode = new TreeNode($"{probeLabel}: {SelectedProbe} ▼")
            {
                Tag = "PROBE",
                ForeColor = Color.Black
            };

            var probeInfoNode = new TreeNode("   [Sağ Tık: Add Probe]")
            {
                Tag = "PROBE_INFO",
                ForeColor = Color.Black,
                NodeFont = new Font("Segoe UI", 8F, FontStyle.Italic)
            };
            probeNode.Nodes.Add(probeInfoNode);
            */

            // ✅ KALDIRILDI - Z Safety artık gösterilmiyor
            /*
            // Z Safety
            string zSafetyLabel = "Z Safety".PadRight(labelWidth);
            zSafetyNode = new TreeNode($"{zSafetyLabel}: {ZSafetyDistance} mm")
            {
                Tag = "ZSAFETY",
                ForeColor = Color.Black
            };
            zSafetyNode.ContextMenuStrip = zSafetyContextMenu;  // ✅ Sağ tık menüsü
            */

            // ✅ KALDIRILDI - Retract artık gösterilmiyor
            /*
            // Retract (YENİ)
            string retractLabel = "Retract".PadRight(labelWidth);
            retractNode = new TreeNode($"{retractLabel}: {RetractDistance} mm")
            {
                Tag = "RETRACT",
                ForeColor = Color.Black
            };
            */

            // Probing
            methodNode = new TreeNode("CNC Measurement")
            {
                Tag = "PROBING",
                ForeColor = Color.Black,
                NodeFont = new Font("Segoe UI", 9F, FontStyle.Italic)
            };
            methodNode.ContextMenuStrip = probingContextMenu;  // ✅ Sağ tık menüsü

            // Toolpath (YENİ)
            string toolpathLabel = "Toolpath".PadRight(labelWidth);
            toolpathNode = new TreeNode($"{toolpathLabel}: -")
            {
                Tag = "TOOLPATH",
                ForeColor = Color.Black,
                NodeFont = new Font("Segoe UI", 9F, FontStyle.Italic)
            };
            
            
            // Toolpath altına Stop butonu
            var stopSimulationNode = new TreeNode("   [Stop Simulation]")
            {
                Tag = "STOP_SIMULATION",
                ForeColor = Color.Red,
                NodeFont = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            toolpathNode.Nodes.Add(stopSimulationNode);

            // Generate G-CODE
            generateNode = new TreeNode("[Generate G-CODE]")
            {
                Tag = "GENERATE",
                ForeColor = Color.Black,
                NodeFont = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            // rootNode.Nodes.Add(machineNode); // ✅ KALDIRILDI - Machine Name artık gösterilmiyor
            // rootNode.Nodes.Add(probeNode); // ✅ KALDIRILDI - Probe Name artık gösterilmiyor
            // rootNode.Nodes.Add(zSafetyNode); // ✅ KALDIRILDI - Z Safety artık gösterilmiyor
            // rootNode.Nodes.Add(retractNode); // ✅ KALDIRILDI - Retract artık gösterilmiyor
            // rootNode.Nodes.Add(toolpathNode);  // ✅ GİZLENDİ - Toolpath artık gösterilmiyor
            // rootNode.Nodes.Add(generateNode);  // ✅ GİZLENDİ - Generate G-CODE artık gösterilmiyor

            treeView.Nodes.Add(methodNode);
            methodNode.Expand();
        }

        /// <summary>
        /// Context Menu'leri oluştur (Sağ tık menüleri)
        /// </summary>
        private void InitializeContextMenus()
        {
            // Machine Context Menu - CNC Machines formunu aç
            machineContextMenu = new ContextMenuStrip();
            machineContextMenu.Items.Add("🏭 CNC Machines", null, (s, e) => OpenCNCMachinesForm());

            // Probe Context Menu - Form_New_Prob formunu aç
            probeContextMenu = new ContextMenuStrip();
            probeContextMenu.Items.Add("🔧 Add Probe", null, (s, e) => OpenAddProbeForm());

            // Z Safety Context Menu - Clearance Plane göster/gizle
            zSafetyContextMenu = new ContextMenuStrip();
            
            // Test için Opening event
            zSafetyContextMenu.Opening += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("🎯 Z Safety context menu açıldı!");
            };
            
            zSafetyContextMenu.Items.Add("👁️ Toggle Clearance Plane", null, (s, e) => ToggleClearancePlane());

            // Probing Context Menu - 11 alt seçenek
            probingContextMenu = new ContextMenuStrip();
            
            // Ana menü item: "Add Probing"
            var addProbingItem = new ToolStripMenuItem("➕ Add Probing");
            
            // Alt seçenekler (numara yok, temiz)
            addProbingItem.DropDownItems.Add("Point", null, (s, e) => AddProbingPoint());
            addProbingItem.DropDownItems.Add("Hole", null, (s, e) => AddProbingHole());
            addProbingItem.DropDownItems.Add("Boss", null, (s, e) => AddProbingBoss());
            addProbingItem.DropDownItems.Add("Slot Width", null, (s, e) => AddProbingSlotWidth());
            addProbingItem.DropDownItems.Add("Ridge Width", null, (s, e) => AddProbingRidgeWidth());
            addProbingItem.DropDownItems.Add("Rectangle In", null, (s, e) => AddProbingRectangleIn());
            addProbingItem.DropDownItems.Add("Rectangle Out", null, (s, e) => AddProbingRectangleOut());
            addProbingItem.DropDownItems.Add("Two Holes", null, (s, e) => AddProbingTwoHoles());
            addProbingItem.DropDownItems.Add("Four Holes", null, (s, e) => AddProbingFourHoles());
            addProbingItem.DropDownItems.Add("Angle", null, (s, e) => AddProbingAngle());
            addProbingItem.DropDownItems.Add("Plane", null, (s, e) => AddProbingPlane());
            
            probingContextMenu.Items.Add(addProbingItem);
        }

        /// <summary>
        /// CNC Machines formunu aç
        /// </summary>
        private void OpenCNCMachinesForm()
        {
            try
            {
                var machinesForm = new Form_CNC_Machines();
                machinesForm.ShowDialog();
                
                // Form kapandıktan sonra makineleri yeniden yükle
                LoadMachinesFromJson();
                
                // ✅ KALDIRILDI - Machine Name artık TreeView'de gösterilmiyor
                /*
                // TreeView'i güncelle
                if (machines.Count > 0)
                {
                    string machineLabel = "Machine Name".PadRight(15);
                    machineNode.Text = $"{machineLabel}: {SelectedMachine} ▼";
                }
                */
                
                System.Diagnostics.Debug.WriteLine("✅ CNC Machines formu kapatıldı, TreeView güncellendi");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening CNC Machines form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"❌ CNC Machines form hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Form_New_Prob formunu aç (Probe ekleme formu)
        /// </summary>
        private void OpenAddProbeForm()
        {
            try
            {
                var probeForm = new Form_New_Prob();
                probeForm.ShowDialog();
                
                // Form kapandıktan sonra probe'ları yeniden yükle
                LoadProbesFromJson();
                
                // ✅ KALDIRILDI - Probe Name artık TreeView'de gösterilmiyor
                /*
                // TreeView'i güncelle
                if (probes.Count > 0)
                {
                    string probeLabel = "Probe Name".PadRight(15);
                    probeNode.Text = $"{probeLabel}: {SelectedProbe} ▼";
                }
                */
                
                System.Diagnostics.Debug.WriteLine("✅ Add Probe formu kapatıldı, TreeView güncellendi");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Probe form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"❌ Add Probe form hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Custom TreeView node çizimi - Label'lar bold, değerler normal
        /// </summary>
        private void TreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // Arka plan
            if (e.Node.IsSelected)
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);
            }

            // Node text'i al
            string text = e.Node.Text;
            Color textColor = e.Node.IsSelected ? Color.White : e.Node.ForeColor;

            // ✅ YENİ: Point koordinatları için özel çizim (İki farklı font)
            if (text.StartsWith("Point ") && text.Contains("X="))
            {
                try
                {
                    // "Point 1:" ve koordinatları ayır
                    int coordStart = text.IndexOf("X=");
                    string pointLabel = text.Substring(0, coordStart).TrimEnd();
                    string coordinates = text.Substring(coordStart);

                    // Label'ı Segoe UI ile çiz
                    Font labelFont = new Font("Segoe UI", 9F, FontStyle.Regular);
                    SizeF labelSize = e.Graphics.MeasureString(pointLabel + "  ", labelFont);
                    e.Graphics.DrawString(pointLabel + "  ", labelFont, new SolidBrush(textColor), e.Bounds.X, e.Bounds.Y + 1);

                    // Koordinatları Monospac821 BT ile çiz
                    Font coordFont = new Font("Monospac821 BT", 9F, FontStyle.Regular);
                    e.Graphics.DrawString(coordinates, coordFont, new SolidBrush(textColor), 
                        e.Bounds.X + labelSize.Width, e.Bounds.Y + 1);
                }
                catch
                {
                    // Hata durumunda normal çizim
                    Font font = e.Node.NodeFont ?? treeView.Font;
                    e.Graphics.DrawString(text, font, new SolidBrush(textColor), e.Bounds.X, e.Bounds.Y + 1);
                }
            }
            // İki nokta üst üste varsa, öncesini bold, sonrasını normal yap
            else if (text.Contains(" : "))
            {
                string[] parts = text.Split(new[] { " : " }, 2, StringSplitOptions.None);
                string labelPart = parts[0];
                string valuePart = " : " + parts[1];

                // Label kısmını bold olarak çiz (Segoe UI)
                Font boldFont = new Font("Segoe UI", 9F, FontStyle.Bold);
                SizeF labelSize = e.Graphics.MeasureString(labelPart, boldFont);
                e.Graphics.DrawString(labelPart, boldFont, new SolidBrush(textColor), e.Bounds.X, e.Bounds.Y + 1);

                // Değer kısmını normal olarak çiz (Segoe UI)
                Font normalFont = new Font("Segoe UI", 9F, FontStyle.Regular);
                e.Graphics.DrawString(valuePart, normalFont, new SolidBrush(textColor), 
                    e.Bounds.X + labelSize.Width, e.Bounds.Y + 1);
            }
            else
            {
                // Normal çizim (root node, generate button, vb.)
                Font font = e.Node.NodeFont ?? treeView.Font;
                e.Graphics.DrawString(text, font, new SolidBrush(textColor), e.Bounds.X, e.Bounds.Y + 1);
            }
        }

        /// <summary>
        /// Event handler'ları başlat
        /// </summary>
        private void InitializeEvents()
        {
            // Node click eventi
            treeView.NodeMouseClick += TreeView_NodeMouseClick;

            // Double click eventi (Z Safety için inline edit)
            treeView.NodeMouseDoubleClick += TreeView_NodeMouseDoubleClick;

            // Context menu için
            treeView.NodeMouseClick += TreeView_NodeMouseRightClick;
            
            // ✅ KeyDown event (DELETE tuşu için)
            treeView.KeyDown += TreeView_KeyDown;
            
            // ✅ AfterSelect event (Marker highlight için)
            treeView.AfterSelect += TreeView_AfterSelect;
        }

        /// <summary>
        /// Node'a tıklandığında
        /// </summary>
        private void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            string tag = e.Node.Tag?.ToString();

            // ✅ Grup altındaki noktalara tıklandı
            if (e.Node.Parent != null && e.Node.Parent.Tag?.ToString().StartsWith("PROBE_GROUP_") == true)
            {
                if (e.Node.Tag is Point3D)
                {
                    ToggleProbePointSelection(e.Node);
                    return;
                }
            }

            switch (tag)
            {
                case "MACHINE":
                    ShowMachineDropdown(e.Node);
                    break;

                case "PROBE":
                    ShowProbeDropdown(e.Node);
                    break;

                case "GENERATE":
                    OnGenerateGCodeClicked?.Invoke(this, EventArgs.Empty);
                    break;
                    
                case "STOP_SIMULATION":  // YENİ: Simülasyonu Durdur
                    OnStopSimulationClicked?.Invoke(this, EventArgs.Empty);
                    break;
                    
                case "CLEAR_ALL_POINTS":  // ✅ YENİ: Grup için Clear All
                    ClearGroupPoints(e.Node.Parent);
                    break;
                    
                case "SELECT_ALL":  // ✅ YENİ: Grup için Select All
                    ToggleGroupSelectAll(e.Node.Parent);
                    break;
            }
        }

        /// <summary>
        /// Machine dropdown göster
        /// </summary>
        private void ShowMachineDropdown(TreeNode node)
        {
            // Mevcut ComboBox'ı temizle
            if (machineComboBox != null && treeView.Controls.Contains(machineComboBox))
            {
                treeView.Controls.Remove(machineComboBox);
                machineComboBox.Dispose();
            }

            // Yeni ComboBox oluştur
            machineComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = treeView.Font,
                Width = 200
            };

            machineComboBox.Items.AddRange(machines.ToArray());
            machineComboBox.SelectedItem = SelectedMachine;

            // Node'un konumunu al
            Rectangle nodeBounds = node.Bounds;
            machineComboBox.Location = new Point(nodeBounds.X + 150, nodeBounds.Y);

            // Event ekle
            machineComboBox.SelectedIndexChanged += (s, e) =>
            {
                SelectedMachine = machineComboBox.SelectedItem.ToString();
                // ✅ KALDIRILDI - Machine Name artık TreeView'de gösterilmiyor
                /*
                string machineLabel = "Machine Name".PadRight(15);
                machineNode.Text = $"{machineLabel}: {SelectedMachine} ▼";
                */
                treeView.Controls.Remove(machineComboBox);
                machineComboBox.Dispose();
            };

            machineComboBox.Leave += (s, e) =>
            {
                treeView.Controls.Remove(machineComboBox);
                machineComboBox.Dispose();
            };

            treeView.Controls.Add(machineComboBox);
            machineComboBox.Focus();
            machineComboBox.DroppedDown = true;
        }

        /// <summary>
        /// Probe dropdown göster
        /// </summary>
        private void ShowProbeDropdown(TreeNode node)
        {
            // Mevcut ComboBox'ı temizle
            if (probeComboBox != null && treeView.Controls.Contains(probeComboBox))
            {
                treeView.Controls.Remove(probeComboBox);
                probeComboBox.Dispose();
            }

            // Yeni ComboBox oluştur
            probeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = treeView.Font,
                Width = 200
            };

            probeComboBox.Items.AddRange(probes.ToArray());
            probeComboBox.SelectedItem = SelectedProbe;

            // Node'un konumunu al
            Rectangle nodeBounds = node.Bounds;
            probeComboBox.Location = new Point(nodeBounds.X + 120, nodeBounds.Y);

            // Event ekle
            probeComboBox.SelectedIndexChanged += (s, e) =>
            {
                SelectedProbe = probeComboBox.SelectedItem.ToString();
                // ✅ KALDIRILDI - Probe Name artık TreeView'de gösterilmiyor
                /*
                string probeLabel = "Probe Name".PadRight(15);
                probeNode.Text = $"{probeLabel}: {SelectedProbe} ▼";
                */
                
                // ✅ YENİ: Probe değişti → Marker'ları güncelle
                OnProbeChanged?.Invoke(this, EventArgs.Empty);
                
                treeView.Controls.Remove(probeComboBox);
                probeComboBox.Dispose();
            };

            probeComboBox.Leave += (s, e) =>
            {
                treeView.Controls.Remove(probeComboBox);
                probeComboBox.Dispose();
            };

            treeView.Controls.Add(probeComboBox);
            probeComboBox.Focus();
            probeComboBox.DroppedDown = true;
        }

        /// <summary>
        /// Double click - ✅ KALDIRILDI (Retract artık TreeView'de yok)
        /// </summary>
        private void TreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Z Safety KALDIRILDI - Artık Clearance Plane'den otomatik
            // Retract KALDIRILDI - Artık TreeView'de gösterilmiyor
        }

        /// <summary>
        /// <summary>
        /// ✅ KALDIRILDI: Z Safety artık Clearance Plane'den otomatik alınıyor
        /// </summary>

        /// <summary>
        /// ✅ KALDIRILDI: Retract artık TreeView'de gösterilmiyor
        /// </summary>
        /*
        private void ShowRetractTextBox(TreeNode node)
        {
            // ... kod kaldırıldı ...
        }
        */
    }
}
