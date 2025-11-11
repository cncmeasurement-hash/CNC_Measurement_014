using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace _014
{
    /// <summary>
    /// LENGTH MEASUREMENT - ENABLE/DISABLE PARTIAL CLASS 2/3
    /// Event subscriptions burada
    /// </summary>
    public partial class LengthMeasurementAnalyzer
    {
        // ═══════════════════════════════════════════════════════════
        // ENABLE / DISABLE
        // ═══════════════════════════════════════════════════════════

        public void Enable(bool enable)
        {
            if (enable)
            {
                // ✅ MODU AKTİF ET
                isEnabled = true;
                selectedPoints.Clear();
                ClearVisuals();

                // ✅ LAYER OLUŞTUR (Yoksa)
                if (!design.Layers.Contains("LengthMeasurement"))
                {
                    design.Layers.Add(new devDept.Eyeshot.Layer("LengthMeasurement")
                    {
                        Color = Color.Yellow
                    });
                    System.Diagnostics.Debug.WriteLine("✅ 'LengthMeasurement' layer oluşturuldu!");
                }

                // ✅ YENİ: SAĞ TIK MENÜSÜNÜ OLUŞTUR
                CreateSnapContextMenu();

                // ✅ Ölçüm formunu göster ve sıfırla
                if (measurementForm != null)
                {
                    measurementForm.ResetValues();
                    measurementForm.Show();
                }

                // ✅ PERFORMANCE: Değişkenleri başlat
                lastMouseMoveTime = DateTime.MinValue;
                var camera = design.Viewports[0].Camera;
                lastCameraState = $"{camera.Target.X},{camera.Target.Y},{camera.Target.Z}|{camera.Distance}";
                isViewportStable = true;

                // Mouse event'lerini bağla (STANDART WINDOWS FORMS)
                design.MouseClick += Design_MouseClick;
                design.MouseMove += Design_MouseMove; // ✅ YENİ!
                design.KeyDown += Design_KeyDown;

                // Cursor değiştir
                design.Cursor = Cursors.Cross;

                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("📏 LENGTH MEASUREMENT MOD AKTİF (OPTIMIZED)");
                System.Diagnostics.Debug.WriteLine("✅ Throttling: 50ms");
                System.Diagnostics.Debug.WriteLine("✅ Viewport tracking: Aktif");
                System.Diagnostics.Debug.WriteLine("✅ İlk noktayı seçin (Snap: EndPoint/MidPoint)");
                System.Diagnostics.Debug.WriteLine("✅ Sağ tık → Snap seçenekleri");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            else
            {
                // ⛔ MODU KAPAT
                isEnabled = false;
                selectedPoints.Clear();
                ClearVisuals();

                // ✅ YENİ: SAĞ TIK MENÜSÜNÜ KALDIR
                if (snapContextMenu != null)
                {
                    snapContextMenu.Dispose();
                    snapContextMenu = null;
                }

                // ✅ Ölçüm formunu gizle
                if (measurementForm != null)
                {
                    measurementForm.Hide();
                }

                // Event'leri kaldır
                design.MouseClick -= Design_MouseClick;
                design.MouseMove -= Design_MouseMove; // ✅ YENİ!
                design.KeyDown -= Design_KeyDown;

                // Cursor'u geri al
                design.Cursor = Cursors.Default;

                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("⛔ LENGTH MEASUREMENT MOD KAPALI");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }

            design.Invalidate();
        }

        // ═══════════════════════════════════════════════════════════
        // ESC TUŞU İLE ÇIKIŞ
        // ═══════════════════════════════════════════════════════════

    }
}
