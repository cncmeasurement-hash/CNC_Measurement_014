// ════════════════════════════════════════════════════════
// ✅ FACE MEASUREMENT ANALYZER - PRODUCTION READY!
// ════════════════════════════════════════════════════════
// 
// ÖZELLİKLER:
// ✅ GetEntityUnderMouseCursor (NURBS best practice!)
// ✅ FindClosestTriangle (Eyeshot built-in!)
// ✅ IFace pattern (Surface + Mesh desteği)
// ✅ Brep.Face ray casting
// ✅ Minimum/Maximum mesafe
// ✅ Açı ve paralellik hesaplama
// ✅ Coplanar grouping (BFS)
// ✅ Highlight sistemi
// 
// ÇALIŞMA DURUMU: ✅ TEST EDİLDİ VE ÇALIŞIYOR!
// TARIH: 30 Ekim 2025
// ════════════════════════════════════════════════════════

using _014.Measurements.Face;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static devDept.Eyeshot.Entities.Mesh;
using Plane = devDept.Geometry.Plane;
using Point3D = devDept.Geometry.Point3D;
using Vector3D = devDept.Geometry.Vector3D;

namespace _014
{
    /// <summary>
    /// FACE MEASUREMENT ANALYZER - FINAL VERSION
    /// 
    /// ✅ Brep.Face kullanımı
    /// ✅ Compile ready
    /// ✅ Ray casting ile face seçimi
    /// </summary>
    public partial class FaceMeasurementAnalyzer
    {
        private Design design;
        private FaceMeasurementForm measurementForm;  // ✅ Ölçüm formu
        private bool isEnabled = false;

        private Face selectedFace1 = null;
        private Face selectedFace2 = null;

        private List<Entity> visualEntities = new List<Entity>();
        private Entity measurementText = null;
        private Line minDistanceLine = null;  // ✅ YENİ: Minimum mesafe çizgisi

        private double coplanarTolerance = 1.0;
        private Color face1Color = Color.FromArgb(255, Color.Yellow);
        private Color face2Color = Color.FromArgb(255, Color.Cyan);

        // ════════════════════════════════════════════════════════
        // FACE CLASS
        // ════════════════════════════════════════════════════════
        public class Face
        {
            public List<int> TriangleIndices { get; set; } = new List<int>();
            public Vector3D Normal { get; set; }
            public Point3D Center { get; set; }
            public List<Point3D> Vertices { get; set; } = new List<Point3D>();
            public Plane Plane { get; set; }
            public Mesh SourceMesh { get; set; }

            public Brep ParentBrep { get; set; }
            public Brep.Face BrepFace { get; set; }

            // ✅ YENİ: Orijinal entity ve renk
            public Entity SourceEntity { get; set; }
            public Color OriginalColor { get; set; }
            public colorMethodType OriginalColorMethod { get; set; }
        }

        public FaceMeasurementAnalyzer(Design design)
        {
            this.design = design;
        }

        // ════════════════════════════════════════════════════════
        // ENABLE/DISABLE
        // ════════════════════════════════════════════════════════
        public void Enable()
        {
            if (isEnabled) return;
            isEnabled = true;
            selectedFace1 = null;
            selectedFace2 = null;

            // ✅ Form oluştur (ilk kez)
            if (measurementForm == null)
            {
                measurementForm = new FaceMeasurementForm();
            }

            // ✅ Form'u göster
            measurementForm.Show();
            measurementForm.ResetValues();

            // ✅ Layer oluştur
            if (!design.Layers.Contains("FaceMeasurement"))
            {
                design.Layers.Add(new devDept.Eyeshot.Layer("FaceMeasurement")
                {
                    Color = Color.Yellow,
                    Visible = true
                });
                System.Diagnostics.Debug.WriteLine("✅ 'FaceMeasurement' layer oluşturuldu!");
            }

            design.MouseClick += Design_MouseClick;
            design.SelectionFilterMode = selectionFilterType.Face;

            System.Diagnostics.Debug.WriteLine("✅ Face Measurement Analyzer AKTIF!");
            System.Diagnostics.Debug.WriteLine("📍 1. yüzeyi seçin");
        }

        public void Disable()
        {
            if (!isEnabled) return;
            isEnabled = false;
            design.MouseClick -= Design_MouseClick;
            ClearVisuals();
            selectedFace1 = null;
            selectedFace2 = null;

            // ✅ Form'u gizle
            if (measurementForm != null)
            {
                measurementForm.Hide();
            }

            System.Diagnostics.Debug.WriteLine("❌ Face Measurement Analyzer KAPALI!");
        }
    }
}
