using _014.Probe.Configuration;
using _014.Probe.Core;
using _014.Probe.Visualization;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace _014
{
    /// <summary>
    /// Probe (ölçüm ucu) oluşturma ve yönetme formu - ANA FORM
    /// Partial class 1/4: Form event'leri, grid işlemleri, animasyon kontrolleri
    /// </summary>
    public partial class Form_New_Prob : Form
    {
        // ═══════════════════════════════════════════════════════════
        // FIELDS (Tüm partial class'lar tarafından paylaşılır)
        // ═══════════════════════════════════════════════════════════

        private List<ProbeData> probes = new List<ProbeData>();
        private ContextMenuStrip gridContextMenu;
        private bool suppressValueChange = false;
        private ProbeColorAnimator probeAnimator;

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR VE INITIALIZATION
        // ═══════════════════════════════════════════════════════════

        public Form_New_Prob()
        {
            InitializeComponent();
            InitializeGridSettings();
            LoadProbesToGrid();

            // ⭐ Render modunu ayarla (Rendering.cs'de tanımlı)
            InitializeRenderSettings();

            // ⭐ Animatör başlat
            probeAnimator = new ProbeColorAnimator(design_new_probe);

            this.Load += Form_New_Prob_Load;

            // Numeric değer değişim event'leri
            numeric_new_probe_D.ValueChanged += numeric_ValueChanged;
            numeric_new_probe_d1.ValueChanged += numeric_ValueChanged;
            numeric_new_probe_d2.ValueChanged += numeric_ValueChanged;
            numeric_new_probe_L1.ValueChanged += numeric_ValueChanged;
            numeric_new_probe_L2.ValueChanged += numeric_ValueChanged;
            numeric_new_probe_L3.ValueChanged += numeric_ValueChanged;

            dataGrid_new_prob.CellClick += DataGrid_new_prob_CellClick;

            // ✅ CheckBox ve TrackBar event handler'ları
            checkBox_lamp_prob_2.CheckedChanged += checkBox_lamp_prob_2_CheckedChanged;
            trackBar_lamp_probe.ValueChanged += trackBar_lamp_probe_ValueChanged;

            // ✅ TrackBar ayarları
            trackBar_lamp_probe.Minimum = 25;    // 25ms (çok hızlı ⚡)
            trackBar_lamp_probe.Maximum = 500;   // 500ms (yavaş 🐢)
            trackBar_lamp_probe.Value = 100;     // Varsayılan: 100ms
            trackBar_lamp_probe.TickFrequency = 25;
            trackBar_lamp_probe.Enabled = false; // Başlangıçta kapalı
        }

        // ═══════════════════════════════════════════════════════════
        // PUBLIC METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Şu anda gösterilen prob parametrelerini döndürür
        /// </summary>
        public ProbeData GetCurrentProbe()
        {
            try
            {
                return new ProbeData
                {
                    Name = txt_new_probe_name.Text,
                    D = numeric_new_probe_D.Value,
                    d1 = numeric_new_probe_d1.Value,
                    d2 = numeric_new_probe_d2.Value,
                    L1 = numeric_new_probe_L1.Value,
                    L2 = numeric_new_probe_L2.Value,
                    L3 = numeric_new_probe_L3.Value
                };
            }
            catch
            {
                return null;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // FORM EVENTS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Form yüklendiğinde çalışır
        /// </summary>
        private void Form_New_Prob_Load(object sender, EventArgs e)
        {
            // ⭐ RENDER MODUNU AKTİF ET
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Render modu aktif ediliyor...");

                var designType = design_new_probe.GetType();
                var displayModeProp = designType.GetProperty("DisplayMode");

                if (displayModeProp != null && displayModeProp.CanWrite)
                {
                    var renderedValue = displayType.Rendered;
                    displayModeProp.SetValue(design_new_probe, renderedValue);
                    System.Diagnostics.Debug.WriteLine("✅ RENDER MODU AKTİF! (Property)");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ DisplayMode property bulunamadı veya read-only");
                }

                design_new_probe.Rendered.ShowEdges = true;
                design_new_probe.Rendered.EdgeThickness = 0.1f;
                System.Diagnostics.Debug.WriteLine($"✅ EdgeThickness = {design_new_probe.Rendered.EdgeThickness}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Render hatası: {ex.Message}");
            }

            // Probe'u oluştur (ProbeBuilder.cs'de tanımlı)
            UpdateProbeWithHole();

            // ⭐ 200ms sonra animasyonu otomatik başlat
            System.Windows.Forms.Timer startTimer = new System.Windows.Forms.Timer();
            startTimer.Interval = 200;
            startTimer.Tick += (s, ev) =>
            {
                startTimer.Stop();
                System.Diagnostics.Debug.WriteLine("🎬 Form_New_Prob: Animasyon başlatma kontrolü...");
                if (checkBox_lamp_prob_2.Checked)
                {
                    probeAnimator.StartAnimation();
                    System.Diagnostics.Debug.WriteLine("✅ CheckBox işaretli - Animasyon başlatıldı");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⏸️ CheckBox boş - Animasyon başlatılmadı");
                }
            };
            startTimer.Start();
        }

        /// <summary>
        /// Form kapanırken animasyonu durdur
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (probeAnimator != null)
            {
                probeAnimator.StopAnimation();
                probeAnimator.Dispose();
                probeAnimator = null;
                System.Diagnostics.Debug.WriteLine("⛔ Form kapanıyor - Animasyon durduruldu!");
            }

            base.OnFormClosing(e);
        }

        // ═══════════════════════════════════════════════════════════
        // NUMERIC VALUE CHANGE EVENTS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Numeric değerler değiştiğinde probe'u güncelle
        /// </summary>
        private void numeric_ValueChanged(object sender, EventArgs e)
        {
            if (suppressValueChange)
                return;

            // Animasyonu durdur
            if (probeAnimator != null && probeAnimator.IsAnimating)
            {
                probeAnimator.StopAnimation();
            }

            UpdateProbeWithHole();

            // 100ms bekle, sonra animasyonu yeniden başlat
            System.Windows.Forms.Timer restartTimer = new System.Windows.Forms.Timer();
            restartTimer.Interval = 100;
            restartTimer.Tick += (s, ev) =>
            {
                restartTimer.Stop();
                if (probeAnimator != null && checkBox_lamp_prob_2.Checked)
                {
                    probeAnimator.StartAnimation();
                }
            };
            restartTimer.Start();
        }

        // ═══════════════════════════════════════════════════════════
        // DATAGRID EVENTS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Grid'de satıra tıklandığında
        /// </summary>
        private void DataGrid_new_prob_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGrid_new_prob.Rows.Count)
                return;

            // Animasyonu durdur
            if (probeAnimator != null && probeAnimator.IsAnimating)
            {
                probeAnimator.StopAnimation();
            }

            suppressValueChange = true;

            try
            {
                var row = dataGrid_new_prob.Rows[e.RowIndex];

                txt_new_probe_name.Text = row.Cells[0].Value?.ToString();
                numeric_new_probe_D.Value = Convert.ToDecimal(row.Cells[1].Value);
                numeric_new_probe_d1.Value = Convert.ToDecimal(row.Cells[2].Value);
                numeric_new_probe_d2.Value = Convert.ToDecimal(row.Cells[3].Value);
                numeric_new_probe_L1.Value = Convert.ToDecimal(row.Cells[4].Value);
                numeric_new_probe_L2.Value = Convert.ToDecimal(row.Cells[5].Value);
                numeric_new_probe_L3.Value = Convert.ToDecimal(row.Cells[6].Value);
            }
            finally
            {
                suppressValueChange = false;
            }

            UpdateProbeWithHole();

            // 100ms bekle, sonra animasyonu yeniden başlat
            System.Windows.Forms.Timer restartTimer = new System.Windows.Forms.Timer();
            restartTimer.Interval = 100;
            restartTimer.Tick += (s, ev) =>
            {
                restartTimer.Stop();
                if (probeAnimator != null && checkBox_lamp_prob_2.Checked)
                {
                    probeAnimator.StartAnimation();
                }
            };
            restartTimer.Start();
        }

        private void DataGrid_new_prob_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit = dataGrid_new_prob.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0)
                {
                    dataGrid_new_prob.ClearSelection();
                    dataGrid_new_prob.Rows[hit.RowIndex].Selected = true;
                    gridContextMenu.Show(dataGrid_new_prob, e.Location);
                }
            }
        }

        private void DataGrid_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();
            var centerFormat = new System.Drawing.StringFormat()
            {
                Alignment = System.Drawing.StringAlignment.Center,
                LineAlignment = System.Drawing.StringAlignment.Center
            };
            var headerBounds = new System.Drawing.Rectangle(e.RowBounds.Left, e.RowBounds.Top,
                                                            grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, System.Drawing.SystemBrushes.ControlText,
                                  headerBounds, centerFormat);
        }

        // ═══════════════════════════════════════════════════════════
        // GRID MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        private void InitializeGridSettings()
        {
            dataGrid_new_prob.AllowUserToAddRows = false;
            dataGrid_new_prob.RowPostPaint += DataGrid_RowPostPaint;

            gridContextMenu = new ContextMenuStrip();
            var deleteItem = new ToolStripMenuItem("Sil");
            deleteItem.Click += DeleteSelectedRow;
            gridContextMenu.Items.Add(deleteItem);

            dataGrid_new_prob.MouseDown += DataGrid_new_prob_MouseDown;
        }

        private void DeleteSelectedRow(object sender, EventArgs e)
        {
            if (dataGrid_new_prob.SelectedRows.Count > 0)
            {
                int rowIndex = dataGrid_new_prob.SelectedRows[0].Index;

                if (MessageBox.Show("Seçili satır silinsin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (rowIndex >= 0 && rowIndex < probes.Count)
                    {
                        probes.RemoveAt(rowIndex);
                        UpdateGrid();
                        ProbeStorage.SaveToJson(probes);
                    }
                }
            }
        }

        private void LoadProbesToGrid()
        {
            probes = ProbeStorage.LoadFromJson();
            UpdateGrid();
        }

        private void UpdateGrid()
        {
            dataGrid_new_prob.Rows.Clear();
            foreach (var p in probes)
            {
                dataGrid_new_prob.Rows.Add(p.Name, p.D, p.d1, p.d2, p.L1, p.L2, p.L3);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // SAVE BUTTON
        // ═══════════════════════════════════════════════════════════

        private void btn_new_probe_save_Click(object sender, EventArgs e)
        {
            string name = txt_new_probe_name.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Lütfen Name alanını doldurunuz.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var incoming = new ProbeData
            {
                Name = name,
                D = numeric_new_probe_D.Value,
                d1 = numeric_new_probe_d1.Value,
                d2 = numeric_new_probe_d2.Value,
                L1 = numeric_new_probe_L1.Value,
                L2 = numeric_new_probe_L2.Value,
                L3 = numeric_new_probe_L3.Value
            };

            int idx = probes.FindIndex(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (idx >= 0)
            {
                var dr = MessageBox.Show(
                    "Bu isimde kayıt mevcut. Üzerine yazmak istiyor musunuz?",
                    "Onay",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dr == DialogResult.No)
                    return;

                probes[idx] = incoming;
            }
            else
            {
                probes.Add(incoming);
            }

            UpdateGrid();
            ProbeStorage.SaveToJson(probes);
            SelectRowByName(name);
        }

        private void SelectRowByName(string name)
        {
            foreach (DataGridViewRow r in dataGrid_new_prob.Rows)
            {
                var cellName = r.Cells[0].Value?.ToString();
                if (string.Equals(cellName, name, StringComparison.OrdinalIgnoreCase))
                {
                    r.Selected = true;
                    dataGrid_new_prob.CurrentCell = r.Cells[0];
                    dataGrid_new_prob.FirstDisplayedScrollingRowIndex = r.Index;
                    break;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ANIMATION CONTROL EVENTS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Lamp Probe CheckBox değiştiğinde
        /// </summary>
        private void checkBox_lamp_prob_2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_lamp_prob_2.Checked)
            {
                if (probeAnimator != null)
                {
                    probeAnimator.StartAnimation();
                    System.Diagnostics.Debug.WriteLine("✅ Animasyon başlatıldı (CheckBox)");
                }
                trackBar_lamp_probe.Enabled = true;
            }
            else
            {
                if (probeAnimator != null && probeAnimator.IsAnimating)
                {
                    probeAnimator.StopAnimation();
                    System.Diagnostics.Debug.WriteLine("⏸️ Animasyon durduruldu");
                }
                trackBar_lamp_probe.Enabled = false;
            }
        }

        /// <summary>
        /// Animasyon hızı değiştiğinde
        /// </summary>
        private void trackBar_lamp_probe_ValueChanged(object sender, EventArgs e)
        {
            int speedMs = trackBar_lamp_probe.Value;

            if (probeAnimator != null)
            {
                probeAnimator.UpdateSpeed(speedMs);
                System.Diagnostics.Debug.WriteLine($"🎨 Animasyon hızı değişti: {speedMs}ms");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PROBE MESH EXPORT (CollisionDetector için)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// ✅ Ekranda gösterilen probe mesh'ini döndürür (KOPYA)
        /// CollisionDetector'ın çarpışma kontrolü için kullanılır
        /// </summary>
        /// <returns>Probe mesh'inin kopyası (clone) veya null</returns>
        public Mesh GetCurrentProbeMesh()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 GetCurrentProbeMesh() çağrıldı...");

                // Block'ları kontrol et
                if (design_new_probe.Blocks == null || design_new_probe.Blocks.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Hiç block yok!");
                    return null;
                }

                // Son eklenen probe block'unu bul
                devDept.Eyeshot.Block probeBlock = null;
                foreach (var block in design_new_probe.Blocks)
                {
                    if (block.Name != null && block.Name.StartsWith("ProbeBlock_"))
                    {
                        probeBlock = block;
                        break; // İlk bulduğunu al (en son eklenen olmalı)
                    }
                }

                if (probeBlock == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ ProbeBlock bulunamadı!");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"✅ ProbeBlock bulundu: {probeBlock.Name}");
                System.Diagnostics.Debug.WriteLine($"   Entity sayısı: {probeBlock.Entities.Count}");

                // Block içindeki tüm entity'leri tek mesh'e birleştir
                Mesh combinedMesh = null;

                foreach (var entity in probeBlock.Entities)
                {
                    Mesh meshPart = null;

                    if (entity is Mesh mesh)
                    {
                        meshPart = mesh;
                    }
                    else if (entity is Solid solid)
                    {
                        // Solid'i Mesh'e çevir
                        meshPart = solid.ConvertToMesh();
                    }

                    if (meshPart != null)
                    {
                        if (combinedMesh == null)
                        {
                            combinedMesh = (Mesh)meshPart.Clone(); // İlk parça
                        }
                        else
                        {
                            combinedMesh.MergeWith((Mesh)meshPart.Clone()); // Diğer parçalar
                        }
                    }
                }

                if (combinedMesh == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Mesh birleştirilemedi!");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Probe mesh hazır! Vertex sayısı: {combinedMesh.Vertices.Length}");
                return combinedMesh; // Zaten clone yapıldı
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetCurrentProbeMesh hatası: {ex.Message}");
                return null;
            }
        }
    }
}