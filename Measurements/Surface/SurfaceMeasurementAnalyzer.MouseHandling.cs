using devDept.Eyeshot.Entities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 2/3: Mouse ve Keyboard event handling
    /// </summary>
    public partial class SurfaceMeasurementAnalyzer
    {
        // ═══════════════════════════════════════════════════════════
        // KEYBOARD EVENTS
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// ✅ KeyDown - ESC tuşu ile moddan çık
        /// </summary>
        private void Design_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isEnabled)
                return;

            // ESC tuşuna basıldı mı?
            if (e.KeyCode == Keys.Escape)
            {
                System.Diagnostics.Debug.WriteLine("⌨️ ESC tuşuna basıldı - Mod kapatılıyor...");
                Enable(false);

                // ✅ Form1'e bildir (Mod kapandı)
                OnDisabled?.Invoke();

                e.Handled = true;  // Event'i işle
            }
        }

        // ═══════════════════════════════════════════════════════════
        // SELECTION EVENTS
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// ✅ SelectionChanged - Yanlış tipleri otomatik deselect et (hover engellemesi)
        /// </summary>
        private void Design_SelectionChanged(object sender, EventArgs e)
        {
            if (!isEnabled)
                return;

            try
            {
                // Seçili entity'leri kontrol et
                var selectedEntities = design.Entities.Where(ent => ent.Selected).ToList();

                foreach (var entity in selectedEntities)
                {
                    // Surface mi?
                    if (entity is Surface surface)
                    {
                        string surfaceType = surface.GetType().Name;

                        // Yanlış tip mi?
                        if (surfaceType != "CylindricalSurface")
                        {
                            // ✅ Hemen deselect et (hover iptal)
                            entity.Selected = false;
                            System.Diagnostics.Debug.WriteLine($"⛔ '{surfaceType}' deselect edildi (hover engellendi)");
                        }
                    }
                    else
                    {
                        // Surface değil - deselect
                        entity.Selected = false;
                    }
                }

                design.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SelectionChanged hatası: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MOUSE EVENTS
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// ✅ Mouse click event handler
        /// NurbsNormalHandler pattern'i - Manuel filtreleme
        /// </summary>
        private void Design_MouseClick(object sender, MouseEventArgs e)
        {
            // Mod aktif değilse çık
            if (!isEnabled)
                return;

            // Sol tık değilse çık
            if (e.Button != MouseButtons.Left)
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🖱️ Yüzeye tıklandı!");

                // ✅ Mouse altındaki entity'yi al (NurbsNormalHandler gibi)
                int entityIndex = design.GetEntityUnderMouseCursor(e.Location, true);

                if (entityIndex == -1)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Mouse altında entity yok");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }

                Entity entity = design.Entities[entityIndex];

                System.Diagnostics.Debug.WriteLine($"📦 Entity bulundu: {entity.GetType().Name} (Index: {entityIndex})");

                // ✅ Surface mi kontrol et
                if (!(entity is Surface surface))
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Bu Surface değil, atlanıyor...");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }

                // ✅ MANUEL FİLTRE: Tip kontrolü (NurbsNormalHandler pattern'i)
                string surfaceType = surface.GetType().Name;
                System.Diagnostics.Debug.WriteLine($"🔍 Surface tipi: {surfaceType}");

                // ✅ Sadece Cylindrical kabul et
                if (surfaceType != "CylindricalSurface")
                {
                    System.Diagnostics.Debug.WriteLine($"⛔ '{surfaceType}' tipi desteklenmiyor, atlanıyor...");
                    System.Diagnostics.Debug.WriteLine("   ℹ️ Sadece Cylindrical yüzeyler ölçülebilir.");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;  // ✅ Sessizce atla (NurbsNormalHandler gibi)
                }

                // ✅ Doğru tip - Önceki seçimi iptal et
                if (lastSelectedSurface != null)
                {
                    lastSelectedSurface.Selected = false;
                }

                // Yeni seçimi kaydet
                lastSelectedSurface = surface;
                surface.Selected = true;
                design.Invalidate();

                // Ölçüm yap
                if (surfaceType == "CylindricalSurface")
                {
                    System.Diagnostics.Debug.WriteLine("✅ Silindirik yüzey ölçülüyor...");
                    MeasureCylindricalSurface(surface);
                }

                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Mouse click hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }
        }
    }
}
