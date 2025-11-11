using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DrawingPoint = System.Drawing.Point;
using DrawingSize = System.Drawing.Size;

namespace _014
{
    /// <summary>
    /// UIManager - POINTS GRID
    /// ✅ PARTIAL CLASS 3/4: Points DataGrid oluşturma, event'ler, işlemler
    /// </summary>
    public partial class UIManager
    {
        private void CreatePointsDataGrid()
        {
            // ✅ YENİ KOLON YAPISI - Checkbox eklendi
            pointsDataTable = new DataTable();
            pointsDataTable.Columns.Add("", typeof(int));           // Boş (sıra no)
            pointsDataTable.Columns.Add("☑", typeof(bool));         // ✅ CHECKBOX
            pointsDataTable.Columns.Add("Yüzey No", typeof(string));
            pointsDataTable.Columns.Add("X", typeof(string));       // ✅ String format
            pointsDataTable.Columns.Add("Y", typeof(string));       // ✅ String format
            pointsDataTable.Columns.Add("Z", typeof(string));       // ✅ String format
            pointsDataTable.Columns.Add("Nx", typeof(string));      // ✅ String format
            pointsDataTable.Columns.Add("Ny", typeof(string));      // ✅ String format
            pointsDataTable.Columns.Add("Nz", typeof(string));      // ✅ String format

            pointsGridView = new DataGridView();
            pointsGridView.DataSource = pointsDataTable;
            pointsGridView.ReadOnly = false; // ✅ Checkbox için false
            pointsGridView.AllowUserToAddRows = false;
            pointsGridView.AllowUserToDeleteRows = false;
            pointsGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            pointsGridView.RowHeadersVisible = false;
            pointsGridView.BackgroundColor = Color.White;
            pointsGridView.BorderStyle = BorderStyle.Fixed3D;

            // ✅ MODERN FONT - Segoe UI (Surfaces ile aynı)
            pointsGridView.Font = new Font("Segoe UI", 9);
            pointsGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            pointsGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(70, 130, 180); // Steel Blue
            pointsGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            
            // ✅ AÇIK GRİ / BEYAZ (Surfaces ile aynı)
            pointsGridView.DefaultCellStyle.BackColor = Color.White;
            pointsGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            // ✅ SATIR YÜKSEKLIĞI (Surfaces ile aynı)
            pointsGridView.RowTemplate.Height = 24;

            // ✅ DEFAULT KONUM - Sağ üst (biraz daha büyük)
            int gridWidth = 480;
            int gridHeight = 200;
            pointsGridView.Location = new DrawingPoint(design.Width - gridWidth - 10, 10);
            pointsGridView.Size = new DrawingSize(gridWidth, gridHeight);
            
            // ✅ Anchor kaldırıldı - taşınabilir olsun
            pointsGridView.Visible = false;
            pointsGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // ✅ Event'ler
            pointsGridView.SelectionChanged += PointsGridView_SelectionChanged;
            pointsGridView.KeyDown += PointsGridView_KeyDown;
            pointsGridView.MouseDown += PointsGridView_MouseDown;
            pointsGridView.MouseMove += PointsGridView_MouseMove; // ✅ YENİ - Taşıma/Resize
            pointsGridView.MouseUp += PointsGridView_MouseUp;     // ✅ YENİ
            pointsGridView.DataBindingComplete += PointsGridView_DataBindingComplete; // ✅ YENİ - Kolon ayarları

            CreateGridContextMenu();

            design.Controls.Add(pointsGridView);
            pointsGridView.BringToFront();
        }

        private void CreateGridContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem deleteItem = new ToolStripMenuItem("🗑️ Sil (Delete)");
            deleteItem.ShortcutKeyDisplayString = "Del";
            deleteItem.Click += (s, e) => DeleteSelectedGridRow();

            ToolStripMenuItem deleteAllItem = new ToolStripMenuItem("🗑️ Tümünü Sil");
            deleteAllItem.Click += (s, e) => DeleteAllPoints();

            contextMenu.Items.Add(deleteItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(deleteAllItem);

            pointsGridView.ContextMenuStrip = contextMenu;
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ YENİ - POINTS GRID KOLON AYARLARI
        // ═══════════════════════════════════════════════════════════

        private void PointsGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (pointsGridView.Columns.Count < 9)
                    return;

                // ✅ KOLON GENİŞLİKLERİ
                pointsGridView.Columns[0].Width = 25;   // # (boş header)
                pointsGridView.Columns[1].Width = 40;   // ☑ Checkbox
                pointsGridView.Columns[2].Width = 90;   // Yüzey No
                pointsGridView.Columns[3].Width = 70;   // X
                pointsGridView.Columns[4].Width = 70;   // Y
                pointsGridView.Columns[5].Width = 70;   // Z
                pointsGridView.Columns[6].Width = 60;   // Nx
                pointsGridView.Columns[7].Width = 60;   // Ny
                pointsGridView.Columns[8].Width = 60;   // Nz

                // ✅ HİZALAMA
                pointsGridView.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                pointsGridView.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                
                // ✅ SAYILAR SAĞ HİZALI
                pointsGridView.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                pointsGridView.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                pointsGridView.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                pointsGridView.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                pointsGridView.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                pointsGridView.Columns[8].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                // ✅ READONLY - Checkbox hariç her şey
                for (int i = 0; i < 9; i++)
                {
                    if (i != 1) // Checkbox dışında
                        pointsGridView.Columns[i].ReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ PointsGridView_DataBindingComplete hatası: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ YENİ - POINTS GRID TAŞıMA + RESIZE
        // ═══════════════════════════════════════════════════════════

        private bool isPointsDragging = false;
        private bool isPointsResizing = false;
        private DrawingPoint pointsDragStartPoint;
        private DrawingSize pointsDragStartSize;
        private ResizeMode pointsResizeMode = ResizeMode.None;

        private void PointsGridView_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Cursor değiştirme
                if (!isPointsDragging && !isPointsResizing)
                {
                    int resizeMargin = 8;
                    bool atRight = e.X >= pointsGridView.Width - resizeMargin;
                    bool atBottom = e.Y >= pointsGridView.Height - resizeMargin;

                    if (atRight && atBottom)
                        pointsGridView.Cursor = Cursors.SizeNWSE;
                    else if (atRight)
                        pointsGridView.Cursor = Cursors.SizeWE;
                    else if (atBottom)
                        pointsGridView.Cursor = Cursors.SizeNS;
                    else
                    {
                        var hitTest = pointsGridView.HitTest(e.X, e.Y);
                        if (hitTest.Type == DataGridViewHitTestType.ColumnHeader)
                            pointsGridView.Cursor = Cursors.SizeAll;
                        else
                            pointsGridView.Cursor = Cursors.Default;
                    }
                }

                // Taşıma
                if (isPointsDragging)
                {
                    DrawingPoint newLocation = pointsGridView.Location;
                    newLocation.X += e.X - pointsDragStartPoint.X;
                    newLocation.Y += e.Y - pointsDragStartPoint.Y;
                    
                    if (newLocation.X < 0) newLocation.X = 0;
                    if (newLocation.Y < 0) newLocation.Y = 0;
                    if (newLocation.X + pointsGridView.Width > design.Width)
                        newLocation.X = design.Width - pointsGridView.Width;
                    if (newLocation.Y + pointsGridView.Height > design.Height)
                        newLocation.Y = design.Height - pointsGridView.Height;
                    
                    pointsGridView.Location = newLocation;
                }

                // Resize
                if (isPointsResizing)
                {
                    int deltaX = e.X - pointsDragStartPoint.X;
                    int deltaY = e.Y - pointsDragStartPoint.Y;

                    int newWidth = pointsDragStartSize.Width;
                    int newHeight = pointsDragStartSize.Height;

                    if (pointsResizeMode == ResizeMode.Right || pointsResizeMode == ResizeMode.BottomRight)
                    {
                        newWidth = Math.Max(350, pointsDragStartSize.Width + deltaX);
                    }

                    if (pointsResizeMode == ResizeMode.Bottom || pointsResizeMode == ResizeMode.BottomRight)
                    {
                        newHeight = Math.Max(120, pointsDragStartSize.Height + deltaY);
                    }

                    pointsGridView.Size = new DrawingSize(newWidth, newHeight);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ PointsGridView_MouseMove hatası: {ex.Message}");
            }
        }

        private void PointsGridView_MouseUp(object sender, MouseEventArgs e)
        {
            isPointsDragging = false;
            isPointsResizing = false;
            pointsResizeMode = ResizeMode.None;
            pointsGridView.Cursor = Cursors.Default;
        }

        private void PointsGridView_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // Sağ tıklama menüsü (mevcut özellik)
                if (e.Button == MouseButtons.Right)
                {
                    var hitTest = pointsGridView.HitTest(e.X, e.Y);
                    if (hitTest.RowIndex >= 0)
                    {
                        pointsGridView.ClearSelection();
                        pointsGridView.Rows[hitTest.RowIndex].Selected = true;
                    }
                    return;
                }

                // ✅ SOL TıKLAMA - Taşıma ve Resize
                if (e.Button == MouseButtons.Left)
                {
                    var hitTest = pointsGridView.HitTest(e.X, e.Y);
                    
                    // Header → Taşıma
                    if (hitTest.Type == DataGridViewHitTestType.ColumnHeader)
                    {
                        isPointsDragging = true;
                        pointsDragStartPoint = e.Location;
                        pointsGridView.Cursor = Cursors.SizeAll;
                        return;
                    }

                    // Kenarlardan → Resize
                    int resizeMargin = 8;
                    bool atRight = e.X >= pointsGridView.Width - resizeMargin;
                    bool atBottom = e.Y >= pointsGridView.Height - resizeMargin;

                    if (atRight && atBottom)
                    {
                        isPointsResizing = true;
                        pointsResizeMode = ResizeMode.BottomRight;
                        pointsDragStartPoint = e.Location;
                        pointsDragStartSize = pointsGridView.Size;
                        pointsGridView.Cursor = Cursors.SizeNWSE;
                    }
                    else if (atRight)
                    {
                        isPointsResizing = true;
                        pointsResizeMode = ResizeMode.Right;
                        pointsDragStartPoint = e.Location;
                        pointsDragStartSize = pointsGridView.Size;
                        pointsGridView.Cursor = Cursors.SizeWE;
                    }
                    else if (atBottom)
                    {
                        isPointsResizing = true;
                        pointsResizeMode = ResizeMode.Bottom;
                        pointsDragStartPoint = e.Location;
                        pointsDragStartSize = pointsGridView.Size;
                        pointsGridView.Cursor = Cursors.SizeNS;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PointsGridView_MouseDown error: {ex.Message}");
            }
        }

        private void DeleteSelectedGridRow()
        {
            try
            {
                if (pointsGridView.SelectedRows.Count > 0)
                {
                    int selectedIndex = pointsGridView.SelectedRows[0].Index;
                    markerManager.DeleteMarkerByIndex(selectedIndex);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteSelectedGridRow error: {ex.Message}");
                MessageBox.Show($"Silme hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteAllPoints()
        {
            try
            {
                var result = MessageBox.Show(
                    "Tüm işaretli noktaları silmek istediğinizden emin misiniz?",
                    "Tümünü Sil",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    markerManager.ClearPointMarkers();
                    MessageBox.Show("Tüm noktalar silindi!", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteAllPoints error: {ex.Message}");
                MessageBox.Show($"Silme hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PointsGridView_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (pointsGridView.SelectedRows.Count == 0)
                    return;

                int selectedIndex = pointsGridView.SelectedRows[0].Index;

                foreach (Entity ent in design.Entities)
                {
                    if (ent.EntityData is string tag && tag.StartsWith("POINT_MARKER"))
                    {
                        ent.Color = Color.Red;
                        ent.ColorMethod = colorMethodType.byEntity;
                    }
                }

                foreach (Entity ent in design.Entities)
                {
                    if (ent.EntityData is string tag && tag == $"POINT_MARKER_{selectedIndex}")
                    {
                        ent.Color = Color.Yellow;
                        ent.ColorMethod = colorMethodType.byEntity;
                        markerManager.SelectMarker(ent);
                        break;
                    }
                }

                design.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PointsGridView_SelectionChanged error: {ex.Message}");
            }
        }

        private void PointsGridView_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete && pointsGridView.SelectedRows.Count > 0)
                {
                    int selectedIndex = pointsGridView.SelectedRows[0].Index;
                    markerManager.DeleteMarkerByIndex(selectedIndex);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PointsGridView_KeyDown error: {ex.Message}");
            }
        }
    }
}
