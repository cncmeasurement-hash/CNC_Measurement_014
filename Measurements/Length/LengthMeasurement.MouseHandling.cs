using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Windows.Forms;

namespace _014
{
    /// <summary>
    /// LENGTH MEASUREMENT - MOUSE HANDLING (EVENTS)
    /// PARTIAL CLASS 4A/6: Keyboard ve Click event handling
    /// </summary>
    public partial class LengthMeasurementAnalyzer
    {
        // ═══════════════════════════════════════════════════════════
        // KEYBOARD EVENTS
        // ═══════════════════════════════════════════════════════════

        private void Design_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                System.Diagnostics.Debug.WriteLine("📏 ESC basıldı - Length modu kapatılıyor");
                Enable(false);
                OnDisabled?.Invoke();
                e.Handled = true;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MOUSE CLICK EVENTS
        // ═══════════════════════════════════════════════════════════

        private void Design_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isEnabled) return;

            // ✅ YENİ: SAĞ TIK → SNAP MENÜSÜNÜ GÖSTER
            if (e.Button == MouseButtons.Right)
            {
                if (snapContextMenu != null)
                {
                    snapContextMenu.Show(design, e.Location);
                    System.Diagnostics.Debug.WriteLine("📍 Snap menüsü açıldı!");
                }
                return;
            }

            // Sadece sol tıklama
            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    Point3D clickedPoint;

                    // ═══════════════════════════════════════════════════════
                    // ✅ KRİTİK: SADECE SNAP NOKTASI VARSA TIKLAMAYlzlN VER!
                    // Boşlukta (havada) tıklamayı engelle
                    // ═══════════════════════════════════════════════════════
                    if (snapEnabled && hoveredSnapPoint != null)
                    {
                        // ✅ SNAP noktası var - izin ver
                        clickedPoint = hoveredSnapPoint;
                        System.Diagnostics.Debug.WriteLine("📍 SNAP noktası kullanıldı!");

                        // Noktayı ekle
                        AddPoint(clickedPoint);
                    }
                    else if (snapEnabled && hoveredSnapPoint == null)
                    {
                        // ❌ SNAP aktif ama nokta yok - boşlukta tıklandı, REDDET!
                        System.Diagnostics.Debug.WriteLine("⚠️ Boşlukta tıklama engellendi! Lütfen bir mesh noktasına tıklayın.");
                        return; // Tıklamayı yoksay
                    }
                    else
                    {
                        // ═══════════════════════════════════════════════════
                        // Snap kapalı - eski mod (ScreenToPlane)
                        // ═══════════════════════════════════════════════════
                        Plane workPlane = Plane.XY;

                        bool success = design.ScreenToPlane(e.Location, workPlane, out clickedPoint);

                        if (!success || clickedPoint == null)
                        {
                            System.Diagnostics.Debug.WriteLine("⚠️ ScreenToPlane başarısız");
                            return;
                        }

                        System.Diagnostics.Debug.WriteLine("📍 Snap kapalı - ScreenToPlane kullanıldı");

                        // Noktayı ekle
                        AddPoint(clickedPoint);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ MouseClick hatası: {ex.Message}");
                }
            }
        }
    }
}
