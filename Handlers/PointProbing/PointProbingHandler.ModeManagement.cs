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
    public partial class PointProbingHandler
    {
        public void Enable(bool enable)
        {
            isEnabled = enable;

            if (enable)
            {
                // ✅ Seçim modunu aktif et
                design.ActionMode = actionType.SelectVisibleByPick;
                design.Cursor = Cursors.Cross;

                // Design control'e focus ver (ESC tuşu hemen çalışsın)
                design.Focus();

                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🎯 POINT PROBING MODU AKTİF");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            else
            {
                // ⛔ Modu kapat
                design.ActionMode = actionType.None;
                design.Cursor = Cursors.Default;
                
                // ✅ SEÇİLİ YÜZEYLERIN RENGİNİ ORİJİNALE DÖNDÜR
                design.Entities.ClearSelection();
                design.Invalidate();
                
                System.Diagnostics.Debug.WriteLine("⛔ POINT PROBING MODU PASİF");
            }
        }
    }
}
