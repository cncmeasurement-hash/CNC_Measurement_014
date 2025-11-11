using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Windows.Forms;

namespace _014
{
    /// <summary>
    /// ✅ SİLİNDİRİK VE KONİK YÜZEY ÖLÇÜM ANALİZİ
    /// PARTIAL CLASS 1/3: Ana yapı, fields, constructor, enable/disable
    /// </summary>
    public partial class SurfaceMeasurementAnalyzer
    {
        // ═══════════════════════════════════════════════════════════
        // FIELDS
        // ═══════════════════════════════════════════════════════════
        private Design design;
        private bool isEnabled = false;
        private Surface lastSelectedSurface = null;

        // ✅ Callback - ESC ile kapatıldığında Form1'e bildir
        public Action? OnDisabled { get; set; }

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════
        public SurfaceMeasurementAnalyzer(Design designControl)
        {
            design = designControl;

            // Mouse click event'ini bağla
            design.MouseClick += Design_MouseClick;

            // ✅ KeyDown event - ESC tuşu ile çıkış
            design.KeyDown += Design_KeyDown;

            // ✅ SelectionChanged event - Yanlış tipleri hemen deselect et
            design.SelectionChanged += Design_SelectionChanged;
        }

        // ═══════════════════════════════════════════════════════════
        // ENABLE / DISABLE
        // ═══════════════════════════════════════════════════════════
        public void Enable(bool enable)
        {
            isEnabled = enable;

            if (enable)
            {
                // ✅ Sadece seçim modunu aktif et (NurbsNormalHandler gibi)
                design.ActionMode = devDept.Eyeshot.actionType.SelectVisibleByPick;
                design.Cursor = Cursors.Hand;

                // ❌ SelectionFilterMode KULLANILMIYOR (manuel filtreleme)

                // ✅ Debug log
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("📏 DIAMETER ÖLÇÜM MODU AKTİF");
                System.Diagnostics.Debug.WriteLine("   ✅ SelectionChanged: Sadece Cylindrical hover");
                System.Diagnostics.Debug.WriteLine("   ✅ Yanlış tipler otomatik deselect");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            else
            {
                // ⛔ Modu kapat (NurbsNormalHandler pattern'i)
                design.ActionMode = devDept.Eyeshot.actionType.None;
                design.Cursor = Cursors.Default;
                design.Entities.ClearSelection();
                design.Invalidate();

                lastSelectedSurface = null;

                // ✅ Debug log (MessageBox YOK!)
                System.Diagnostics.Debug.WriteLine("⛔ DIAMETER ÖLÇÜM MODU KAPANDI");
            }
        }
    }
}
