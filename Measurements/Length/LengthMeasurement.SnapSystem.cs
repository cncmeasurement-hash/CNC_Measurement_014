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
    /// LENGTH MEASUREMENT - SNAP SYSTEM PARTIAL CLASS 3/3
    /// Snap menu, tolerance, preview
    /// </summary>
    public partial class LengthMeasurementAnalyzer
    {
        // ═══════════════════════════════════════════════════════════
        // SNAP SYSTEM
        // ═══════════════════════════════════════════════════════════

        private void CreateSnapContextMenu()
        {
            snapContextMenu = new ContextMenuStrip();
            snapContextMenu.ShowCheckMargin = true;
            snapContextMenu.ShowImageMargin = false;
            
            // ✅ Checkbox'ları küçült
            snapContextMenu.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            snapContextMenu.AutoSize = true;

            // ════════════════════════════════════════════════════════
            // TÜM SNAP TİPLERİ - DÜZ LİSTE (Alt menü yok!)
            // ════════════════════════════════════════════════════════
            
            // End Point
            AddSnapMenuItemToContext(snapContextMenu, "End Point", ref snapEndPoint, true);
            
            // Mid Point
            AddSnapMenuItemToContext(snapContextMenu, "Mid Point", ref snapMidPoint, true);
            
            // Center
            AddSnapMenuItemToContext(snapContextMenu, "Center", ref snapCenter, false);
            
            // Quadrant
            AddSnapMenuItemToContext(snapContextMenu, "Quadrant", ref snapQuadrant, false);
            
            // Edge Point
            AddSnapMenuItemToContext(snapContextMenu, "Edge Point", ref snapEdgePoint, false);
            
            // Origin Point
            AddSnapMenuItemToContext(snapContextMenu, "Origin Point", ref snapOrigin, false);

            snapContextMenu.Items.Add(new ToolStripSeparator());

            // ════════════════════════════════════════════════════════
            // TOLERANS AYARLARI
            // ════════════════════════════════════════════════════════
            
            var tol5 = new ToolStripMenuItem
            {
                Text = "5 pixel",
                Checked = (snapTolerance == 5.0)
            };
            tol5.Click += (s, e) =>
            {
                snapTolerance = 5.0;
                UpdateToleranceMenu();
                System.Diagnostics.Debug.WriteLine("✅ Snap toleransı: 5px");
            };
            snapContextMenu.Items.Add(tol5);

            var tol8 = new ToolStripMenuItem
            {
                Text = "8 pixel",
                Checked = (snapTolerance == 8.0)
            };
            tol8.Click += (s, e) =>
            {
                snapTolerance = 8.0;
                UpdateToleranceMenu();
                System.Diagnostics.Debug.WriteLine("✅ Snap toleransı: 8px");
            };
            snapContextMenu.Items.Add(tol8);

            var tol10 = new ToolStripMenuItem
            {
                Text = "10 pixel",
                Checked = (snapTolerance == 10.0)
            };
            tol10.Click += (s, e) =>
            {
                snapTolerance = 10.0;
                UpdateToleranceMenu();
                System.Diagnostics.Debug.WriteLine("✅ Snap toleransı: 10px");
            };
            snapContextMenu.Items.Add(tol10);

            snapContextMenu.Items.Add(new ToolStripSeparator());

            // ════════════════════════════════════════════════════════
            // MARKER BOYUTU AYARLARI
            // ════════════════════════════════════════════════════════
            
            var marker2 = new ToolStripMenuItem
            {
                Text = "2 mm",
                Checked = (snapMarkerSize == 2.0)
            };
            marker2.Click += (s, e) =>
            {
                snapMarkerSize = 2.0;
                UpdateMarkerSizeMenu();
                System.Diagnostics.Debug.WriteLine("✅ Snap marker boyutu: 2mm");
            };
            snapContextMenu.Items.Add(marker2);

            var marker4 = new ToolStripMenuItem
            {
                Text = "4 mm",
                Checked = (snapMarkerSize == 4.0)
            };
            marker4.Click += (s, e) =>
            {
                snapMarkerSize = 4.0;
                UpdateMarkerSizeMenu();
                System.Diagnostics.Debug.WriteLine("✅ Snap marker boyutu: 4mm");
            };
            snapContextMenu.Items.Add(marker4);

            var marker6 = new ToolStripMenuItem
            {
                Text = "6 mm",
                Checked = (snapMarkerSize == 6.0)
            };
            marker6.Click += (s, e) =>
            {
                snapMarkerSize = 6.0;
                UpdateMarkerSizeMenu();
                System.Diagnostics.Debug.WriteLine("✅ Snap marker boyutu: 6mm");
            };
            snapContextMenu.Items.Add(marker6);
        }

        /// <summary>
        /// Snap menu item ekle (helper metod)
        /// </summary>
        private void AddSnapMenuItem(ToolStripMenuItem parentMenu, string text, ref bool snapFlag, bool defaultChecked)
        {
            // Local copy of ref parameter
            bool localFlag = snapFlag;

            var item = new ToolStripMenuItem
            {
                Text = text,
                CheckOnClick = true,
                Checked = defaultChecked
            };

            // Store reference to the flag in the Tag
            item.Tag = text; // For debugging

            item.CheckedChanged += (s, e) =>
            {
                var menuItem = (ToolStripMenuItem)s;

                // Update the corresponding field based on text
                if (text.Contains("EndPoint")) snapEndPoint = menuItem.Checked;
                else if (text.Contains("MidPoint")) snapMidPoint = menuItem.Checked;
                else if (text.Contains("Center")) snapCenter = menuItem.Checked;
                else if (text.Contains("Quadrant")) snapQuadrant = menuItem.Checked;
                else if (text.Contains("EdgePoint")) snapEdgePoint = menuItem.Checked;
                else if (text.Contains("Tangent")) snapTangent = menuItem.Checked;
                else if (text.Contains("Origin")) snapOrigin = menuItem.Checked;

                System.Diagnostics.Debug.WriteLine($"✅ {text}: {menuItem.Checked}");
            };

            parentMenu.DropDownItems.Add(item);
        }

        /// <summary>
        /// OVERLOAD: ContextMenuStrip'e direkt snap item ekle (alt menü olmadan)
        /// </summary>
        private void AddSnapMenuItemToContext(ContextMenuStrip contextMenu, string text, ref bool snapFlag, bool defaultChecked)
        {
            var item = new ToolStripMenuItem
            {
                Text = text,
                CheckOnClick = true,
                Checked = defaultChecked
            };

            item.Tag = text;

            item.CheckedChanged += (s, e) =>
            {
                var menuItem = (ToolStripMenuItem)s;

                // Update the corresponding field based on text
                if (text.Contains("End Point")) snapEndPoint = menuItem.Checked;
                else if (text.Contains("Mid Point")) snapMidPoint = menuItem.Checked;
                else if (text.Contains("Center")) snapCenter = menuItem.Checked;
                else if (text.Contains("Quadrant")) snapQuadrant = menuItem.Checked;
                else if (text.Contains("Edge Point")) snapEdgePoint = menuItem.Checked;
                else if (text.Contains("Origin Point")) snapOrigin = menuItem.Checked;

                System.Diagnostics.Debug.WriteLine($"✅ {text}: {menuItem.Checked}");
            };

            contextMenu.Items.Add(item);
        }

        // ════════════════════════════════════════════════════════

        /// <summary>
        /// Tolerans menüsünü güncelle (radio button effect)
        /// </summary>
        private void UpdateToleranceMenu()
        {
            if (snapContextMenu == null) return;

            foreach (ToolStripItem item in snapContextMenu.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    if (menuItem.Text.Contains("5 pixel"))
                        menuItem.Checked = (snapTolerance == 5.0);
                    else if (menuItem.Text.Contains("8 pixel"))
                        menuItem.Checked = (snapTolerance == 8.0);
                    else if (menuItem.Text.Contains("10 pixel"))
                        menuItem.Checked = (snapTolerance == 10.0);
                    else if (menuItem.Text.Contains("Snap Toleransı"))
                        menuItem.Text = $"⚙️ Snap Toleransı: {snapTolerance:F0}px";
                }
            }
        }

        /// <summary>
        /// Marker boyutu menüsünü güncelle (radio button effect)
        /// </summary>
        private void UpdateMarkerSizeMenu()
        {
            if (snapContextMenu == null) return;

            foreach (ToolStripItem item in snapContextMenu.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    if (menuItem.Text.Contains("2 mm"))
                        menuItem.Checked = (snapMarkerSize == 2.0);
                    else if (menuItem.Text.Contains("4 mm"))
                        menuItem.Checked = (snapMarkerSize == 4.0);
                    else if (menuItem.Text.Contains("6 mm"))
                        menuItem.Checked = (snapMarkerSize == 6.0);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ YENİ: MOUSE MOVE EVENT - SNAP PREVIEW (GÜNCELLENDİ)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mouse hareket ederken snap noktalarını bul ve göster
        /// Referans: SurfaceMeasurementAnalyzer.cs MouseMove sistemi
        /// </summary>
        private void ShowSnapPreview(Point3D snapPoint)
        {
            try
            {
                RemoveTempSnapMarker();

                // Yeşil küre oluştur (snapMarkerSize kullan)
                // Referans: MarkerManager.cs satır 71-82
                tempSnapMarker = Mesh.CreateSphere(snapMarkerSize, 8, 8);  // ✅ 2.0 → snapMarkerSize
                tempSnapMarker.Translate(snapPoint.X, snapPoint.Y, snapPoint.Z);
                tempSnapMarker.Color = Color.Lime;
                tempSnapMarker.ColorMethod = colorMethodType.byEntity;
                tempSnapMarker.EntityData = "TEMP_SNAP_MARKER";
                tempSnapMarker.Selectable = false;

                design.Entities.Add(tempSnapMarker);
                design.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ShowSnapPreview hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Geçici snap marker'ı temizle
        /// </summary>
        private void RemoveTempSnapMarker()
        {
            try
            {
                if (tempSnapMarker != null)
                {
                    design.Entities.Remove(tempSnapMarker);
                    tempSnapMarker = null;
                }
            }
            catch { }
        }

        // ═══════════════════════════════════════════════════════════
        // MOUSE CLICK EVENT (SNAP DESTEKLİ)
        // ═══════════════════════════════════════════════════════════

    }
}
