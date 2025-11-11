using devDept.Eyeshot;
using devDept.Graphics;
using System;
using System.Drawing;

namespace _014
{
    /// <summary>
    /// Form_New_Prob - RENDERING
    /// Partial class 3/4: Render ayarları ve kamera kontrolü
    /// </summary>
    public partial class Form_New_Prob
    {
        // ═══════════════════════════════════════════════════════════
        // RENDER SETTINGS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Design kontrolü için render ayarlarını başlatır
        /// Constructor'dan çağrılır
        /// </summary>
        private void InitializeRenderSettings()
        {
            // Render modu aktif
            design_new_probe.Rendered.ShadowMode = devDept.Graphics.shadowType.Realistic;
            design_new_probe.Rendered.ShowEdges = true;
            design_new_probe.Rendered.EdgeThickness = 0.1f; // Çok çok ince kenarlar

            // Arka plan gradyanı
            design_new_probe.Background.TopColor = Color.FromArgb(240, 248, 255); // Açık mavi
            design_new_probe.Background.BottomColor = Color.FromArgb(200, 220, 240); // Koyu mavi
            design_new_probe.Background.StyleMode = devDept.Graphics.backgroundStyleType.LinearGradient;

            // ✅ ORTHOGRAPHIC MODU
            design_new_probe.Camera.ProjectionMode = devDept.Eyeshot.projectionType.Orthographic;

            System.Diagnostics.Debug.WriteLine("✅ Render ayarları başlatıldı");
            System.Diagnostics.Debug.WriteLine("✅ Orthographic modu ayarlandı");
        }

        // ═══════════════════════════════════════════════════════════
        // CAMERA CONTROL (ViewManager.cs tarzında)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Probe görünümü için kamera açısını ve projeksiyon modunu ayarla
        /// ViewManager.cs tarzında - Orthographic + Trimetric
        /// Form_Load ve UpdateProbeWithHole'dan çağrılır
        /// </summary>
        private void SetProbeViewCamera()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎥 Kamera ayarları yapılıyor...");

                // 1️⃣ ORTHOGRAPHIC PROJEKSIYON MODU
                design_new_probe.Camera.ProjectionMode = devDept.Eyeshot.projectionType.Orthographic;
                System.Diagnostics.Debug.WriteLine("✅ Orthographic mod aktif");

                // 2️⃣ Trimetric görünüm açısı
                design_new_probe.Viewports[0].SetView(viewType.Trimetric);
                System.Diagnostics.Debug.WriteLine("✅ Trimetric view ayarlandı");

                // 3️⃣ Ekrana sığdır
                design_new_probe.ZoomFit();
                System.Diagnostics.Debug.WriteLine("✅ ZoomFit yapıldı");

                // 🔥 CRITICAL: SetView ve ZoomFit sonrası Orthographic'i yeniden zorla!
                design_new_probe.Camera.ProjectionMode = devDept.Eyeshot.projectionType.Orthographic;
                System.Diagnostics.Debug.WriteLine("🔒 Orthographic modu kilitledi!");

                // 4️⃣ Ekranı yenile
                design_new_probe.Invalidate();
                System.Diagnostics.Debug.WriteLine("🎥 Kamera ayarları tamamlandı!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Kamera ayarlama hatası: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ALTERNATIVE CAMERA VIEWS (İsterseniz kullanabilirsiniz)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Front view (Ön görünüm)
        /// </summary>
        private void SetFrontView()
        {
            try
            {
                design_new_probe.Viewports[0].SetView(viewType.Front);
                design_new_probe.Camera.ProjectionMode = devDept.Eyeshot.projectionType.Orthographic;
                design_new_probe.ZoomFit();
                design_new_probe.Invalidate();
                System.Diagnostics.Debug.WriteLine("📐 Front view ayarlandı");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Front view hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Top view (Üst görünüm)
        /// </summary>
        private void SetTopView()
        {
            try
            {
                design_new_probe.Viewports[0].SetView(viewType.Top);
                design_new_probe.Camera.ProjectionMode = devDept.Eyeshot.projectionType.Orthographic;
                design_new_probe.ZoomFit();
                design_new_probe.Invalidate();
                System.Diagnostics.Debug.WriteLine("📐 Top view ayarlandı");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Top view hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Isometric view (İzometrik görünüm)
        /// </summary>
        private void SetIsometricView()
        {
            try
            {
                design_new_probe.Viewports[0].SetView(viewType.Isometric);
                design_new_probe.Camera.ProjectionMode = devDept.Eyeshot.projectionType.Orthographic;
                design_new_probe.ZoomFit();
                design_new_probe.Invalidate();
                System.Diagnostics.Debug.WriteLine("📐 Isometric view ayarlandı");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Isometric view hatası: {ex.Message}");
            }
        }
    }
}