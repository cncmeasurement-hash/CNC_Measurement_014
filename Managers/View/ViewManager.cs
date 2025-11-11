using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using System.Drawing;
using System.Windows.Forms;

namespace _014.Managers.View
{
    /// <summary>
    /// Görünüm modlarını yönetir (Wireframe, Shaded, Rendered, vb.)
    /// </summary>
    public class ViewManager
    {
        private Design design;
        private Design design2; // ✅ İkinci design (eğer varsa)
        private ToolStripMenuItem wireframeMenuItem;
        private ToolStripMenuItem shadedMenuItem;
        private ToolStripMenuItem renderedMenuItem;
        private ToolStripMenuItem hiddenLineMenuItem;

        public ViewManager(Design designControl, Design designControl2 = null)
        {
            design = designControl;
            design2 = designControl2;

            // ✅ Başlangıçta Orthographic mod
            SetOrthographicMode();
        }

        /// <summary>
        /// View menü öğelerini ayarla (Form1'den çağrılır)
        /// </summary>
        public void SetMenuItems(
            ToolStripMenuItem wireframe,
            ToolStripMenuItem shaded,
            ToolStripMenuItem rendered,
            ToolStripMenuItem hiddenLine)
        {
            wireframeMenuItem = wireframe;
            shadedMenuItem = shaded;
            renderedMenuItem = rendered;
            hiddenLineMenuItem = hiddenLine;
        }

        /// <summary>
        /// Wireframe moduna geç
        /// </summary>
        public void SetWireframeMode()
        {
            design.Viewports[0].DisplayMode = displayType.Wireframe;

            if (design2 != null)
            {
                design2.Viewports[0].DisplayMode = displayType.Wireframe;
            }
            UpdateDisplayModeButtons(wireframeMenuItem);
            design.Invalidate();

            if (design2 != null)
            {
                design2.Invalidate();
            }

            System.Diagnostics.Debug.WriteLine("✅ Wireframe moduna geçildi");
        }

        /// <summary>
        /// Shaded moduna geç
        /// </summary>
        public void SetShadedMode()
        {
            design.Viewports[0].DisplayMode = displayType.Shaded;

            if (design2 != null) design2.Viewports[0].DisplayMode = displayType.Shaded;

            if (design2 != null)
            {
                design2.Viewports[0].DisplayMode = displayType.Wireframe;
            }
            UpdateDisplayModeButtons(shadedMenuItem);
            design.Invalidate();
            if (design2 != null) design2.Invalidate();

            if (design2 != null)
            {
                design2.Invalidate();
            }

            System.Diagnostics.Debug.WriteLine("✅ Shaded moduna geçildi");
        }

        /// <summary>
        /// Rendered moduna geç
        /// </summary>
        public void SetRenderedMode()
        {
            design.Viewports[0].DisplayMode = displayType.Rendered;
            if (design2 != null) design2.Viewports[0].DisplayMode = displayType.Rendered;

            if (design2 != null)
            {
                design2.Viewports[0].DisplayMode = displayType.Wireframe;
            }
            UpdateDisplayModeButtons(renderedMenuItem);
            design.Invalidate();
            if (design2 != null) design2.Invalidate();
            if (design2 != null) design2.Invalidate();

            if (design2 != null)
            {
                design2.Invalidate();
            }

            System.Diagnostics.Debug.WriteLine("✅ Rendered moduna geçildi");
        }

        /// <summary>
        /// HiddenLine moduna geç
        /// </summary>
        public void SetHiddenLineMode()
        {
            design.Viewports[0].DisplayMode = displayType.HiddenLines;
            if (design2 != null) design2.Viewports[0].DisplayMode = displayType.HiddenLines;

            if (design2 != null)
            {
                design2.Viewports[0].DisplayMode = displayType.Wireframe;
            }
            UpdateDisplayModeButtons(hiddenLineMenuItem);
            design.Invalidate();
            if (design2 != null) design2.Invalidate();
            if (design2 != null) design2.Invalidate();
            if (design2 != null) design2.Invalidate();

            if (design2 != null)
            {
                design2.Invalidate();
            }

            System.Diagnostics.Debug.WriteLine("✅ HiddenLine moduna geçildi");
        }

        /// <summary>
        /// Menü öğelerinin checkmark'larını güncelle
        /// </summary>
        private void UpdateDisplayModeButtons(ToolStripMenuItem activeItem)
        {
            if (wireframeMenuItem != null) wireframeMenuItem.Checked = false;
            if (shadedMenuItem != null) shadedMenuItem.Checked = false;
            if (renderedMenuItem != null) renderedMenuItem.Checked = false;
            if (hiddenLineMenuItem != null) hiddenLineMenuItem.Checked = false;

            if (activeItem != null)
            {
                activeItem.Checked = true;
            }
        }

        /// <summary>
        /// Front view (Ön görünüm)
        /// </summary>
        public void SetFrontView()
        {
            ChangeView(viewType.Front);
            System.Diagnostics.Debug.WriteLine("📐 Front view");
        }

        /// <summary>
        /// Top view (Üst görünüm)
        /// </summary>
        public void SetTopView()
        {
            ChangeView(viewType.Top);
            System.Diagnostics.Debug.WriteLine("📐 Top view");
        }

        /// <summary>
        /// Left view (Sol görünüm)
        /// </summary>
        public void SetLeftView()
        {
            ChangeView(viewType.Left);
            System.Diagnostics.Debug.WriteLine("📐 Left view");
        }

        /// <summary>
        /// Bottom view (Alt görünüm)
        /// </summary>
        public void SetBottomView()
        {
            ChangeView(viewType.Bottom);
            System.Diagnostics.Debug.WriteLine("📐 Bottom view");
        }

        /// <summary>
        /// Back view (Arka görünüm)
        /// </summary>
        public void SetBackView()
        {
            ChangeView(viewType.Rear);
            System.Diagnostics.Debug.WriteLine("📐 Back view");
        }

        /// <summary>
        /// Right view (Sağ görünüm)
        /// </summary>
        public void SetRightView()
        {
            ChangeView(viewType.Right);
            System.Diagnostics.Debug.WriteLine("📐 Right view");
        }

        /// <summary>
        /// Isometric view (İzometrik görünüm)
        /// </summary>
        public void SetIsometricView()
        {
            ChangeView(viewType.Isometric);
            System.Diagnostics.Debug.WriteLine("📐 Isometric view");
        }


        /// <summary>
        /// <summary>
        /// Orthographic (Dik Projeksiyon) modunu ayarla
        /// </summary>
        private void SetOrthographicMode()
        {
            design.Camera.ProjectionMode = projectionType.Orthographic;

            if (design2 != null)
            {
                design2.Camera.ProjectionMode = projectionType.Orthographic;
            }

            System.Diagnostics.Debug.WriteLine("✅ Orthographic mod aktif");
        }

        /// <summary>
        /// Orthographic (Dik Projeksiyon) modunu ayarla - PUBLIC
        /// NOT: Metod adı "SetPerspectiveMode" ama aslında Orthographic ayarlıyor
        /// </summary>
        public void SetPerspectiveMode()
        {
            design.Camera.ProjectionMode = projectionType.Orthographic;

            if (design2 != null)
            {
                design2.Camera.ProjectionMode = projectionType.Orthographic;
            }

            design.Invalidate();
            if (design2 != null) design2.Invalidate();

            System.Diagnostics.Debug.WriteLine("✅ Orthographic mod aktif");
        }

        /// <summary>
        /// View değiştirme helper - Her zaman Orthographic modda kalır
        /// </summary>
        private void ChangeView(viewType view)
        {
            design.Viewports[0].SetView(view);
            design.Invalidate();

            if (design2 != null)
            {
                design2.Viewports[0].SetView(view);
                design2.Invalidate();
            }

            // ✅ View değiştikten sonra Orthographic modda kal
            SetOrthographicMode();
            // 🔥 CRITICAL: SetView sonrası Orthographic'i tekrar zorla!
            design.Camera.ProjectionMode = projectionType.Orthographic;
            if (design2 != null)
            {
                design2.Camera.ProjectionMode = projectionType.Orthographic;
            }
        }


        /// Zoom Fit - Tüm modeli ekrana sığdır
        /// </summary>
        public void ZoomFit()
        {
            design.ZoomFit();

            if (design2 != null)
            {
                design2.ZoomFit();
            }

            // ✅ Zoom sonrası Orthographic modda kal
            SetOrthographicMode();
            design.Invalidate();
            System.Diagnostics.Debug.WriteLine("🔍 Zoom Fit");
        }
    }
}