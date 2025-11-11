using System.Windows.Forms;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Drawing;
using System.Data;
using _014.Managers.Data;

namespace _014
{
    /// <summary>
    /// Marker ve ok yönetimi
    /// PARTIAL CLASS 1/3: Ana yapı, fields, constructor, temizleme
    /// </summary>
    public partial class MarkerManager
    {
        // ═══════════════════════════════════════════════════════════
        // FIELDS
        // ═══════════════════════════════════════════════════════════
        private Design design;
        private Entity selectedMarker = null;
        private int selectedMarkerIndex = -1;
        private DataTable pointsDataTable;
        private DataManager dataManager;
        private DataGridView pointsGridView;
        
        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════
        public MarkerManager(Design design, DataTable pointsDataTable, 
                           DataManager dataManager, DataGridView pointsGrid)
        {
            this.design = design;
            this.pointsDataTable = pointsDataTable;
            this.dataManager = dataManager;
            this.pointsGridView = pointsGrid;
        }
        
        // ═══════════════════════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════════════════════
        public Entity SelectedMarker => selectedMarker;
        public int SelectedMarkerIndex => selectedMarkerIndex;
        
        // ═══════════════════════════════════════════════════════════
        // CLEAR ALL
        // ═══════════════════════════════════════════════════════════
        public void ClearPointMarkers()
        {
            // Marker'ları, ok'ları ve normal çizgilerini temizle
            for (int i = design.Entities.Count - 1; i >= 0; i--)
            {
                if (design.Entities[i].EntityData is string tag &&
                    (tag.StartsWith("POINT_MARKER") || 
                     tag.StartsWith("NORMAL_ARROW") ||
                     tag.StartsWith("SURFACE_NORMAL_LINE")))
                {
                    design.Entities.RemoveAt(i);
                }
            }

            // DataGrid'i temizle
            if (pointsDataTable != null)
            {
                pointsDataTable.Clear();
            }

            // Seçimi temizle
            selectedMarker = null;
            selectedMarkerIndex = -1;

            design.Invalidate();
        }
    }
}
