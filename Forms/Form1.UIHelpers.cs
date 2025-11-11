using _014;
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
        private void DrawComboBoxItemRightAligned(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null) return;

            // Arka planı çiz
            e.DrawBackground();

            // Text'i al
            string text = comboBox.Items[e.Index].ToString();

            // Text formatı: Sağa yaslanmış
            TextFormatFlags flags = TextFormatFlags.Right | TextFormatFlags.VerticalCenter;

            // Renk: Seçili item için farklı renk
            Color textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? SystemColors.HighlightText
                : e.ForeColor;

            // Text'i çiz (sağa yaslanmış)
            TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, textColor, flags);

            // Focus rectangle çiz
            e.DrawFocusRectangle();
        }

        private void HideOldSnapMenuItems()
        {
            // End Point
            if (endPointToolStripMenuItem != null)
                endPointToolStripMenuItem.Visible = false;

            // Mid Point  
            if (midPointToolStripMenuItem != null)
                midPointToolStripMenuItem.Visible = false;

            // Point to Curve
            if (pointOnCurveToolStripMenuItem != null)
                pointOnCurveToolStripMenuItem.Visible = false;

            // Center
            if (centerToolStripMenuItem != null)
                centerToolStripMenuItem.Visible = false;

            // Close to Face
            if (closeToFaceToolStripMenuItem != null)
                closeToFaceToolStripMenuItem.Visible = false;

            Debug.WriteLine("✅ Eski snap menü itemleri gizlendi");
        }
    }
}
