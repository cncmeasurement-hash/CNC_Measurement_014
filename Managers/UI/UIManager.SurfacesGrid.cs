using devDept.Eyeshot.Control;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DrawingPoint = System.Drawing.Point;
using DrawingSize = System.Drawing.Size;

namespace _014
{
    /// <summary>
    /// UIManager - SURFACES GRID
    /// ✅ PARTIAL CLASS 4/4: Surfaces DataGrid oluşturma, event'ler, populate
    /// </summary>
    public partial class UIManager
    {
        private void CreateSurfacesDataGrid()
        {
            surfacesDataTable = new DataTable();
            surfacesDataTable.Columns.Add("", typeof(int));
            surfacesDataTable.Columns.Add("📝", typeof(string));
            surfacesDataTable.Columns.Add("➡️", typeof(string));
            surfacesDataTable.Columns.Add("Yüzey", typeof(string));
            surfacesDataTable.Columns.Add("Nx", typeof(string));  // ✅ String formatı için
            surfacesDataTable.Columns.Add("Ny", typeof(string));  // ✅ String formatı için
            surfacesDataTable.Columns.Add("Nz", typeof(string));  // ✅ String formatı için
            surfacesDataTable.Columns.Add("Grup", typeof(string));

            surfacesGridView = new DataGridView();
            surfacesGridView.DataSource = surfacesDataTable;
            surfacesGridView.ReadOnly = false;
            surfacesGridView.AllowUserToAddRows = false;
            surfacesGridView.AllowUserToDeleteRows = false;
            surfacesGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            surfacesGridView.RowHeadersVisible = false;
            surfacesGridView.BackgroundColor = Color.White;
            surfacesGridView.BorderStyle = BorderStyle.Fixed3D;

            // ✅ MODERN FONT - Segoe UI
            surfacesGridView.Font = new Font("Segoe UI", 9);
            surfacesGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            surfacesGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(70, 130, 180);
            surfacesGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            
            // ✅ AÇIK GRİ / BEYAZ
            surfacesGridView.DefaultCellStyle.BackColor = Color.White;
            surfacesGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            surfacesGridView.RowTemplate.Height = 24;
            
            // Default konum
            int gridWidth = 323;  // %15 küçültüldü (380 -> 323)
            int gridHeight = 160;
            surfacesGridView.Size = new DrawingSize(gridWidth, gridHeight);
            surfacesGridView.Location = new DrawingPoint(
                design.Width - gridWidth - 10,
                design.Height - gridHeight - 10
            );
            
            surfacesGridView.Visible = false;
            surfacesGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            
            design.Controls.Add(surfacesGridView);
            surfacesGridView.BringToFront();

            // Event'ler
            surfacesGridView.SelectionChanged += SurfacesGridView_SelectionChanged;
            surfacesGridView.CellContentClick += SurfacesGridView_CellContentClick;
            surfacesGridView.ColumnHeaderMouseClick += SurfacesGridView_ColumnHeaderMouseClick;
            surfacesGridView.DataBindingComplete += SurfacesGridView_DataBindingComplete;
            
            // ✅ TAŞıMA + RESIZE EVENT'LERİ
            surfacesGridView.MouseDown += SurfacesGridView_MouseDown;
            surfacesGridView.MouseMove += SurfacesGridView_MouseMove;
            surfacesGridView.MouseUp += SurfacesGridView_MouseUp;
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ HEADER TıKLAMA
        // ═══════════════════════════════════════════════════════════

        private void SurfacesGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 1) // Etiket
                {
                    bool allVisible = dataManager.GetSurfaceDataList().TrueForAll(s => s.IsLabelVisible);
                    bool newState = !allVisible;
                    
                    foreach (var surface in dataManager.GetSurfaceDataList())
                    {
                        surface.IsLabelVisible = newState;
                        surfaceAnalyzer?.ToggleLabelVisibility(surface.Index, newState);
                    }
                    
                    RefreshSurfacesGrid();
                    System.Diagnostics.Debug.WriteLine($"📝 TÜM ETİKETLER: {(newState ? "AÇIK" : "KAPALI")}");
                }
                else if (e.ColumnIndex == 2) // Ok
                {
                    bool allVisible = dataManager.GetSurfaceDataList().TrueForAll(s => s.IsArrowVisible);
                    bool newState = !allVisible;
                    
                    foreach (var surface in dataManager.GetSurfaceDataList())
                    {
                        surface.IsArrowVisible = newState;
                        surfaceAnalyzer?.ToggleArrowVisibility(surface.Index, newState);
                    }
                    
                    RefreshSurfacesGrid();
                    System.Diagnostics.Debug.WriteLine($"➡️ TÜM OKLAR: {(newState ? "AÇIK" : "KAPALI")}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ColumnHeaderMouseClick hatası: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ GRİD TAŞıMA + RESIZE
        // ═══════════════════════════════════════════════════════════

        private void SurfacesGridView_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button != MouseButtons.Left)
                    return;

                var hitTest = surfacesGridView.HitTest(e.X, e.Y);
                
                // Header → Taşıma
                if (hitTest.Type == DataGridViewHitTestType.ColumnHeader)
                {
                    isDragging = true;
                    dragStartPoint = e.Location;
                    surfacesGridView.Cursor = Cursors.SizeAll;
                    return;
                }

                // ✅ RESIZE - Kenarlardan çek
                int resizeMargin = 8;
                bool atRight = e.X >= surfacesGridView.Width - resizeMargin;
                bool atBottom = e.Y >= surfacesGridView.Height - resizeMargin;

                if (atRight && atBottom)
                {
                    isResizing = true;
                    resizeMode = ResizeMode.BottomRight;
                    dragStartPoint = e.Location;
                    dragStartSize = surfacesGridView.Size;
                    surfacesGridView.Cursor = Cursors.SizeNWSE;
                }
                else if (atRight)
                {
                    isResizing = true;
                    resizeMode = ResizeMode.Right;
                    dragStartPoint = e.Location;
                    dragStartSize = surfacesGridView.Size;
                    surfacesGridView.Cursor = Cursors.SizeWE;
                }
                else if (atBottom)
                {
                    isResizing = true;
                    resizeMode = ResizeMode.Bottom;
                    dragStartPoint = e.Location;
                    dragStartSize = surfacesGridView.Size;
                    surfacesGridView.Cursor = Cursors.SizeNS;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ MouseDown hatası: {ex.Message}");
            }
        }

        private void SurfacesGridView_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // ✅ RESIZE CURSOR
                if (!isDragging && !isResizing)
                {
                    int resizeMargin = 8;
                    bool atRight = e.X >= surfacesGridView.Width - resizeMargin;
                    bool atBottom = e.Y >= surfacesGridView.Height - resizeMargin;

                    if (atRight && atBottom)
                        surfacesGridView.Cursor = Cursors.SizeNWSE;
                    else if (atRight)
                        surfacesGridView.Cursor = Cursors.SizeWE;
                    else if (atBottom)
                        surfacesGridView.Cursor = Cursors.SizeNS;
                    else
                        surfacesGridView.Cursor = Cursors.Default;
                }

                // TAŞıMA
                if (isDragging)
                {
                    DrawingPoint newLocation = surfacesGridView.Location;
                    newLocation.X += e.X - dragStartPoint.X;
                    newLocation.Y += e.Y - dragStartPoint.Y;
                    
                    // Sınır kontrolü
                    if (newLocation.X < 0) newLocation.X = 0;
                    if (newLocation.Y < 0) newLocation.Y = 0;
                    if (newLocation.X + surfacesGridView.Width > design.Width)
                        newLocation.X = design.Width - surfacesGridView.Width;
                    if (newLocation.Y + surfacesGridView.Height > design.Height)
                        newLocation.Y = design.Height - surfacesGridView.Height;
                    
                    surfacesGridView.Location = newLocation;
                }

                // ✅ RESIZE
                if (isResizing)
                {
                    int deltaX = e.X - dragStartPoint.X;
                    int deltaY = e.Y - dragStartPoint.Y;

                    int newWidth = dragStartSize.Width;
                    int newHeight = dragStartSize.Height;

                    if (resizeMode == ResizeMode.Right || resizeMode == ResizeMode.BottomRight)
                    {
                        newWidth = Math.Max(250, dragStartSize.Width + deltaX); // Min 250px
                    }

                    if (resizeMode == ResizeMode.Bottom || resizeMode == ResizeMode.BottomRight)
                    {
                        newHeight = Math.Max(120, dragStartSize.Height + deltaY); // Min 120px
                    }

                    surfacesGridView.Size = new DrawingSize(newWidth, newHeight);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ MouseMove hatası: {ex.Message}");
            }
        }

        private void SurfacesGridView_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            isResizing = false;
            resizeMode = ResizeMode.None;
            surfacesGridView.Cursor = Cursors.Default;
        }

        // ═══════════════════════════════════════════════════════════
        // KOLON AYARLARI
        // ═══════════════════════════════════════════════════════════

        private void SurfacesGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (surfacesGridView.Columns.Count < 8)
                    return;

                surfacesGridView.Columns[0].Width = 20;   // #
                surfacesGridView.Columns[1].Width = 28;   // 📝
                surfacesGridView.Columns[2].Width = 28;   // ➡️
                surfacesGridView.Columns[3].Width = 70;   // Yüzey
                surfacesGridView.Columns[4].Width = 45;   // Nx ✅ biraz daha geniş
                surfacesGridView.Columns[5].Width = 45;   // Ny
                surfacesGridView.Columns[6].Width = 44;   // Nz
                surfacesGridView.Columns[7].Width = 43;   // Grup

                // Hizalama
                surfacesGridView.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                surfacesGridView.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                surfacesGridView.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                
                // ✅ NX, NY, NZ SAĞ HİZALI
                surfacesGridView.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                surfacesGridView.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                surfacesGridView.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                // ReadOnly
                for (int i = 0; i < 8; i++)
                {
                    if (i != 1 && i != 2)
                        surfacesGridView.Columns[i].ReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DataBindingComplete hatası: {ex.Message}");
            }
        }

        private void SurfacesGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0)
                    return;

                var row = surfacesGridView.Rows[e.RowIndex];
                string surfaceName = row.Cells["Yüzey"].Value?.ToString();
                
                if (string.IsNullOrEmpty(surfaceName))
                    return;

                int surfaceIndex = int.Parse(surfaceName.Replace("Surface_", ""));
                var surface = dataManager.GetSurfaceByIndex(surfaceIndex);

                if (surface == null)
                    return;

                if (e.ColumnIndex == 1)
                {
                    surface.IsLabelVisible = !surface.IsLabelVisible;
                    surfaceAnalyzer?.ToggleLabelVisibility(surfaceIndex, surface.IsLabelVisible);
                    RefreshSurfacesGrid();
                }
                else if (e.ColumnIndex == 2)
                {
                    surface.IsArrowVisible = !surface.IsArrowVisible;
                    surfaceAnalyzer?.ToggleArrowVisibility(surfaceIndex, surface.IsArrowVisible);
                    RefreshSurfacesGrid();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CellContentClick hatası: {ex.Message}");
            }
        }

        private void RefreshSurfacesGrid()
        {
            try
            {
                foreach (DataGridViewRow row in surfacesGridView.Rows)
                {
                    string surfaceName = row.Cells["Yüzey"].Value?.ToString();
                    if (string.IsNullOrEmpty(surfaceName))
                        continue;

                    int surfaceIndex = int.Parse(surfaceName.Replace("Surface_", ""));
                    var surface = dataManager.GetSurfaceByIndex(surfaceIndex);

                    if (surface != null)
                    {
                        row.Cells["📝"].Value = surface.IsLabelVisible ? "💡" : "🔘";
                        row.Cells["➡️"].Value = surface.IsArrowVisible ? "💡" : "🔘";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RefreshSurfacesGrid hatası: {ex.Message}");
            }
        }

        private void SurfacesGridView_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (surfacesGridView.SelectedRows.Count == 0)
                    return;

                var selectedRow = surfacesGridView.SelectedRows[0];
                string surfaceName = selectedRow.Cells["Yüzey"].Value?.ToString();

                if (string.IsNullOrEmpty(surfaceName))
                    return;

                int surfaceIndex = int.Parse(surfaceName.Replace("Surface_", ""));
                surfaceAnalyzer?.HighlightSurface(surfaceIndex, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SelectionChanged hatası: {ex.Message}");
            }
        }

        public void SelectSurfaceInGrid(int surfaceIndex)
        {
            try
            {
                foreach (DataGridViewRow row in surfacesGridView.Rows)
                {
                    string surfaceName = row.Cells["Yüzey"].Value?.ToString();
                    if (surfaceName == $"Surface_{surfaceIndex}")
                    {
                        surfacesGridView.ClearSelection();
                        row.Selected = true;
                        surfacesGridView.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SelectSurfaceInGrid hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ GÜNCELLEME: Ondalık formatlama + Dik/Eğik
        /// </summary>
        public void PopulateSurfacesGrid()
        {
            try
            {
                surfacesDataTable.Clear();

                int rowNumber = 1;
                foreach (var surface in dataManager.GetSurfaceDataList())
                {
                    // ✅ Grup gösterimi - "Alt Yüzey" → gösterme
                    string displayGroup = surface.Group;
                    if (surface.Group == "Alt Yüzey")
                        displayGroup = ""; // Yeşil yüzey için grup gösterme
                    
                    // ✅ ONDALIK FORMAT: -0.202 (3 basamak, sıfırlarla)
                    string nx = surface.Normal.X.ToString("0.000");
                    string ny = surface.Normal.Y.ToString("0.000");
                    string nz = surface.Normal.Z.ToString("0.000");

                    surfacesDataTable.Rows.Add(
                        rowNumber++,
                        surface.IsLabelVisible ? "💡" : "🔘",
                        surface.IsArrowVisible ? "💡" : "🔘",
                        surface.Name,
                        nx,  // ✅ String olarak
                        ny,
                        nz,
                        displayGroup
                    );
                }

                surfacesGridView.Visible = true;
                RefreshSurfacesGrid();

                System.Diagnostics.Debug.WriteLine($"✅ Grid güncellendi: {dataManager.GetSurfaceDataList().Count} yüzey");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ PopulateSurfacesGrid hatası: {ex.Message}");
            }
        }
    }
}
