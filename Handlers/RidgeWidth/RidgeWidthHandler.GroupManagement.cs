using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _014.Utilities.UI;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// RidgeWidthHandler - Group Management
    /// Grup yönetimi ve eksen sayaçları
    /// </summary>
    public partial class RidgeWidthHandler
    {
        public void SetActiveGroup(int groupNumber)
        {
            currentGroupNumber = groupNumber;
            System.Diagnostics.Debug.WriteLine($"✅ RidgeWidthHandler: Aktif grup set edildi: {groupNumber}");
        }

        public void ClearActiveGroup()
        {
            currentGroupNumber = -1;
            firstSelectedNormal = null;       // ✅ İlk normal temizle
            secondSelectedNormal = null;      // ✅ İkinci normal temizle
            firstSelectedEntityIndex = null;  // ✅ EntityIndex temizle
            selectedPointCount = 0;           // ✅ Nokta sayacı sıfırla
            
            // ✅ NOT: groupPoints ve groupNormals Dictionary'leri KORUNUYOR!
            // Bu veriler toolpath generation için gerekli
            // groupPoints.Clear();    ← YAPMA!
            // groupNormals.Clear();   ← YAPMA!
            
            System.Diagnostics.Debug.WriteLine("✅ RidgeWidthHandler: Aktif grup temizlendi (Dictionary'ler korundu)");
        }

        public void ResetAllAxisCounters()
        {
            xAxisCounter = 0;
            yAxisCounter = 0;
            zAxisCounter = 0;
            System.Diagnostics.Debug.WriteLine("✅ RidgeWidthHandler: Eksen sayaçları sıfırlandı (Yeni dosya import edildi)");
        }

        public void CreateNewRidgeWidthGroup()
        {
            try
            {
                // ✅ YENİ: Point Probing modunu kapat (Ridge Width açılırken)
                if (selectionManager != null)
                {
                    selectionManager.DisablePointProbing();
                    System.Diagnostics.Debug.WriteLine("✅ Point Probing modu kapatıldı (Ridge Width açılıyor)");
                }
                
                if (treeViewManager == null)
                {
                    MessageBox.Show("TreeViewManager bulunamadı!", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                System.Diagnostics.Debug.WriteLine("🎯 Ridge Width seçildi - Yeni grup oluşturuluyor...");

                // ✅ TreeView'de yeni Ridge Width grubu oluştur
                TreeNode groupNode = treeViewManager.CreateNewRidgeWidthGroup();
                
                if (groupNode == null)
                {
                    MessageBox.Show("Grup oluşturulamadı!", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // ✅ Aktif grubu kaydet
                currentGroupNode = groupNode;
                selectedPointCount = 0;  // Nokta sayacını sıfırla
                
                // ✅ YENİ: Grup numarasını parse et ve set et
                string groupText = groupNode.Text;  // "Ridge Width 2"
                if (groupText.StartsWith("Ridge Width "))
                {
                    string numStr = groupText.Replace("Ridge Width ", "");
                    if (int.TryParse(numStr, out int groupNumber))
                    {
                        SetActiveGroup(groupNumber);
                    }
                }

                // ✅ Dikey yüzeyleri sarıya çevir
                HighlightVerticalSurfaces();

                // ✅ YENİ: InstructionPanel'i güncelle
                if (instructionPanel != null && !instructionPanel.IsDisposed)
                {
                    instructionPanel.UpdatePanel(
                        InstructionTexts.TITLE_RIDGE_WIDTH,
                        InstructionTexts.RIDGE_WIDTH
                    );
                    System.Diagnostics.Debug.WriteLine("📋 InstructionPanel güncellendi: Ridge Width modu");
                }

                // ✅ Nokta seçim modunu aktif et
                EnablePointSelection();

                System.Diagnostics.Debug.WriteLine($"✅ Ridge Width grubu oluşturuldu: {groupNode.Text}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateNewRidgeWidthGroup hatası: {ex.Message}");
                MessageBox.Show($"Hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
