using _014.Managers.Data;
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
    /// UI bileşenlerini yöneten sınıf
    /// ✅ V5: Resize + Ondalık format + Modern font + Dik/Eğik
    /// ✅ PARTIAL CLASS 1/4: Ana sınıf, Field'lar, Constructor, Properties
    /// </summary>
    public partial class UIManager
    {
        private Design design;
        private Form parentForm;
        
        // UI Components
        private Label coordinateLabel;
        private DataGridView pointsGridView;
        private DataTable pointsDataTable;
        private DataGridView surfacesGridView;
        private DataTable surfacesDataTable;
        
        // Grid taşıma ve boyutlandırma
        private bool isDragging = false;
        private bool isResizing = false;
        private DrawingPoint dragStartPoint;
        private DrawingSize dragStartSize;
        private ResizeMode resizeMode = ResizeMode.None;
        
        private enum ResizeMode
        {
            None,
            Right,
            Bottom,
            BottomRight
        }
        
        // Dependencies
        private DataManager dataManager;
        private MarkerManager markerManager;
        private SurfaceAnalyzer surfaceAnalyzer;
        
        public UIManager(Design design, Form parent, DataManager dataManager)
        {
            this.design = design;
            this.parentForm = parent;
            this.dataManager = dataManager;
            
            CreateCoordinateLabel();
            CreatePointsDataGrid();
            CreateSurfacesDataGrid();
        }
        
        public void SetMarkerManager(MarkerManager marker)
        {
            this.markerManager = marker;
        }

        public void SetSurfaceAnalyzer(SurfaceAnalyzer analyzer)
        {
            this.surfaceAnalyzer = analyzer;
        }
        
        // Properties
        public Label CoordinateLabel => coordinateLabel;
        public DataGridView PointsGridView => pointsGridView;
        public DataTable PointsDataTable => pointsDataTable;
        public DataGridView SurfacesGridView => surfacesGridView;
        public DataTable SurfacesDataTable => surfacesDataTable;
    }
}
