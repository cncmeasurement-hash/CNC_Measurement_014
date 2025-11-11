using _014.Probe.Configuration;
using _014.Probe.Core;
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
        private void cmb_form1_probe_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🔄 PROBE DEĞİŞTİ - TEMİZLİK BAŞLIYOR");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                
                // ✅ 1. TÜM MODLARDAN ÇIK
                System.Diagnostics.Debug.WriteLine("📍 Adım 1: Tüm modlardan çıkılıyor...");
                
                // Point Probing modunu kapat
                selectionManager?.DisablePointProbing();
                
                // Ridge Width modunu kapat
                if (ridgeWidthHandler != null && ridgeWidthHandler.IsPointSelectionActive())
                {
                    ridgeWidthHandler.DisablePointSelection();
                }
                
                // Angle Measurement modunu kapat
                angleMeasurementManager?.Disable();
                
                // NURBS Normal modunu kapat
                selectionManager?.DisableNurbsNormalMode();
                
                // Face to Face modunu kapat
                if (faceToFaceManager != null && faceToFaceManager.IsActive)
                {
                    faceToFaceManager.Disable();
                }
                
                // Edge to Edge modunu kapat
                if (edgeToEdgeManager != null && edgeToEdgeManager.IsActive)
                {
                    edgeToEdgeManager.Disable();
                }
                
                // Design ActionMode'u sıfırla
                design1.ActionMode = actionType.None;
                design1.Cursor = Cursors.Default;
                
                System.Diagnostics.Debug.WriteLine("✅ Tüm modlardan çıkıldı");
                
                // ✅ 2. TREEVIEW'İ TAMAMEN TEMİZLE (TÜM GRUPLARI SİL)
                System.Diagnostics.Debug.WriteLine("📍 Adım 2: TreeView tamamen temizleniyor...");
                if (treeViewManager != null)
                {
                    treeViewManager.ClearAllGroups(); // ✅ YENİ METOD - TÜM GRUPLARI SİL
                }
                System.Diagnostics.Debug.WriteLine("✅ TreeView tamamen temizlendi");
                
                // ✅ 2.5 EKRANDAKI POINT PROBING MARKER'LARINI TEMİZLE
                System.Diagnostics.Debug.WriteLine("📍 Adım 2.5: Point Probing marker'ları temizleniyor...");
                var pointProbingMarkers = new List<Entity>();
                foreach (Entity entity in design1.Entities)
                {
                    if (entity.LayerName == "ProbePoints")
                    {
                        pointProbingMarkers.Add(entity);
                    }
                }
                foreach (var entity in pointProbingMarkers)
                {
                    design1.Entities.Remove(entity);
                }
                System.Diagnostics.Debug.WriteLine($"✅ {pointProbingMarkers.Count} Point Probing marker temizlendi (Layer: ProbePoints)");
                
                // ✅ 2.6 EKRANDAKI RIDGE WIDTH MARKER'LARINI TEMİZLE
                System.Diagnostics.Debug.WriteLine("📍 Adım 2.6: Ridge Width marker'ları temizleniyor...");
                var ridgeWidthMarkers = new List<Entity>();
                foreach (Entity entity in design1.Entities)
                {
                    if (entity.LayerName == "RidgeWidthPoints" ||
                        entity.LayerName == "RidgeWidthProbe" ||
                        entity.LayerName == "RidgeWidthMeasurements")
                    {
                        ridgeWidthMarkers.Add(entity);
                    }
                }
                foreach (var entity in ridgeWidthMarkers)
                {
                    design1.Entities.Remove(entity);
                }
                System.Diagnostics.Debug.WriteLine($"✅ {ridgeWidthMarkers.Count} Ridge Width marker temizlendi (RidgeWidthPoints + RidgeWidthProbe + RidgeWidthMeasurements)");
                
                // ✅ 2.7 EKRANDAKI ANGLE MEASUREMENT ÇİZGİLERİNİ TEMİZLE
                System.Diagnostics.Debug.WriteLine("📍 Adım 2.7: Angle Measurement çizgileri temizleniyor...");
                var angleMeasurementEntities = new List<Entity>();
                foreach (Entity entity in design1.Entities)
                {
                    if (entity.LayerName == "AngleMeasurementMarkers" ||
                        entity.LayerName == "AngleMeasurementProbe" ||
                        entity.LayerName == "AngleMeasurementLines")
                    {
                        angleMeasurementEntities.Add(entity);
                    }
                }
                foreach (var entity in angleMeasurementEntities)
                {
                    design1.Entities.Remove(entity);
                }
                System.Diagnostics.Debug.WriteLine($"✅ {angleMeasurementEntities.Count} Angle Measurement entity temizlendi (AngleMeasurementMarkers + AngleMeasurementProbe + AngleMeasurementLines)");
                
                // ✅ 3. TOOLPATH'LERİ TEMİZLE
                System.Diagnostics.Debug.WriteLine("📍 Adım 3: Toolpath temizleniyor...");
                toolpathManager?.ClearToolpath();
                System.Diagnostics.Debug.WriteLine("✅ Toolpath temizlendi");
                
                // ✅ 4. EKRANI YENİLE
                System.Diagnostics.Debug.WriteLine("📍 Adım 4: Ekran yenileniyor...");
                design1.Entities.Regen();
                design1.Invalidate();
                System.Diagnostics.Debug.WriteLine("✅ Ekran yenilendi");
                
                // ✅ 5. YENİ PROBE'U AYARLA
                if (cmb_form1_probe.SelectedIndex >= 0)
                {
                    string selectedProbe = cmb_form1_probe.SelectedItem.ToString();
                    
                    // TreeViewManager'a seçili probe'u bildir
                    if (treeViewManager != null)
                    {
                        treeViewManager.SetSelectedProbe(selectedProbe);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Yeni probe seçildi: {selectedProbe}");
                }
                
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("✅ PROBE DEĞİŞİMİ TAMAMLANDI");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Probe değişim hatası: {ex.Message}");
                Debug.WriteLine($"❌ Probe seçimi hatası: {ex.Message}");
            }
        }

        private void InitializeProbeContextMenu()
        {
            // Context menu oluştur
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            // "Add Probe" menü öğesi
            ToolStripMenuItem addProbeItem = new ToolStripMenuItem
            {
                Text = "Add Probe",
                Image = null // İsterseniz icon ekleyebilirsiniz
            };
            addProbeItem.Click += (s, e) => OpenProbeForm();

            // Menüye ekle
            contextMenu.Items.Add(addProbeItem);

            // ComboBox'a context menu'yü ata
            cmb_form1_probe.ContextMenuStrip = contextMenu;

            Debug.WriteLine("✅ Probe context menu oluşturuldu");
        }

        private void OpenProbeForm()
        {
            try
            {
                Form_New_Prob probeForm = new Form_New_Prob();
                probeForm.ShowDialog(); // Modal olarak aç

                // Form kapandıktan sonra probe listesini yeniden yükle
                LoadProbes();

                Debug.WriteLine("✅ Probe formu kapatıldı ve liste yenilendi");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Probe formu açma hatası: {ex.Message}");
                MessageBox.Show($"Form açılırken hata:\n{ex.Message}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProbes()
        {
            try
            {
                // Önceki seçimi sakla
                string previousSelection = cmb_form1_probe.SelectedItem?.ToString();

                // JSON'dan probe'ları yükle
                List<ProbeData> probes = ProbeStorage.LoadFromJson();

                // ComboBox'ı temizle
                cmb_form1_probe.Items.Clear();

                // Probe isimlerini ekle
                foreach (var probe in probes)
                {
                    cmb_form1_probe.Items.Add(probe.Name); // ✅ DÜZELTİLDİ: ProbeName → Name
                }

                Debug.WriteLine($"✅ {probes.Count} probe yüklendi (Sol panel dropdown)");

                // Önceki seçimi geri yükle (eğer hala listede varsa)
                if (!string.IsNullOrEmpty(previousSelection) && cmb_form1_probe.Items.Contains(previousSelection))
                {
                    cmb_form1_probe.SelectedItem = previousSelection;
                }
                // Yoksa ilk probe'u seç
                else if (cmb_form1_probe.Items.Count > 0)
                {
                    cmb_form1_probe.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Probe'lar yüklenirken hata: {ex.Message}");
                MessageBox.Show($"Probe'lar yüklenirken hata:\n{ex.Message}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmb_form1_probe_DrawItem(object sender, DrawItemEventArgs e)
        {
            DrawComboBoxItemRightAligned(sender, e);
        }

        private void cmb_form1_probe_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }
    }
}
