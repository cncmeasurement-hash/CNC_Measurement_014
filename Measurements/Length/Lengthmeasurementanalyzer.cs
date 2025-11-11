using _014.Measurements.Length;
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
    /// ✅ LENGTH MEASUREMENT ANALYZER - PARTIAL CLASS 1/3
    /// Ana class: Fields, Constructor, Core Logic
    /// </summary>
    public partial class LengthMeasurementAnalyzer
    {
        private Design design;
        private bool isEnabled = false;
        private List<Point3D> selectedPoints = new List<Point3D>();
        private List<devDept.Eyeshot.Entities.Point> pointMarkers = new List<devDept.Eyeshot.Entities.Point>();
        private devDept.Eyeshot.Entities.Line measurementLine = null;
        private devDept.Eyeshot.Entities.Text measurementText = null;

        // ✅ YENİ: Ölçüm sonuçları için form
        private LengthMeasurementForm measurementForm = null;

        // ✅ YENİ: Snap sistemi için field'lar
        private bool snapEnabled = true;
        private double snapDistance = 10.0; // 10mm snap toleransı
        private Point3D hoveredSnapPoint = null;
        private Entity hoveredEntity = null;
        private Mesh tempSnapMarker = null;

        // ✅ YENİ: Snap tipleri kontrolü (sağ tık menüsü için)
        // ════════════════════════════════════════════════════════════
        // POINT SNAPS
        private bool snapEndPoint = true;      // Vertex/Köşe noktaları
        private bool snapMidPoint = true;      // Kenar ortası (Boundary)
        private bool snapCenter = false;       // Circle/Arc/Sphere merkezi
        private bool snapQuadrant = false;     // Circle üzerinde 4 nokta

        // CURVE SNAPS
        private bool snapEdgePoint = false;    // Kenar üzerinde herhangi bir nokta
        private bool snapTangent = false;      // Curve'e teğet

        // REFERENCE SNAPS
        private bool snapOrigin = false;       // World origin (0,0,0)

        private double snapTolerance = 8.0;    // Pixel cinsinden
        private double snapMarkerSize = 4.0;   // ✅ YENİ: Snap marker boyutu (mm)
        private ContextMenuStrip snapContextMenu;

        // ✅ YENİ: PERFORMANCE OPTİMİZASYONU
        // ════════════════════════════════════════════════════════════
        private DateTime lastMouseMoveTime = DateTime.MinValue;
        private const int MOUSE_MOVE_THROTTLE_MS = 50; // 50ms'de bir çalış
        private string lastCameraState = "";
        private bool isViewportStable = true;

        // Callback - ESC ile kapatıldığında
        public Action OnDisabled;
        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        public LengthMeasurementAnalyzer(Design design)
        {
            this.design = design;
            
            // ✅ Ölçüm formunu oluştur
            measurementForm = new LengthMeasurementForm();
        }
        private void AddPoint(Point3D point)
        {
            selectedPoints.Add(point);

            // Marker ekle (kırmızı nokta)
            var marker = new devDept.Eyeshot.Entities.Point(point)
            {
                Color = Color.Red,
                ColorMethod = colorMethodType.byEntity
            };
            pointMarkers.Add(marker);
            design.Entities.Add(marker, "LengthMeasurement");

            System.Diagnostics.Debug.WriteLine($"📍 Nokta {selectedPoints.Count} seçildi: ({point.X:F3}, {point.Y:F3}, {point.Z:F3})");

            if (selectedPoints.Count == 1)
            {
                System.Diagnostics.Debug.WriteLine("✅ İkinci noktayı seçin");
            }
            else if (selectedPoints.Count == 2)
            {
                // İki nokta seçildi - ölçümü yap
                CalculateAndDisplay();
            }

            design.Invalidate();
        }

        // ═══════════════════════════════════════════════════════════
        // MESAFE HESAPLAMA VE GÖSTERME
        // ═══════════════════════════════════════════════════════════

        private void CalculateAndDisplay()
        {
            if (selectedPoints.Count != 2) return;

            Point3D p1 = selectedPoints[0];
            Point3D p2 = selectedPoints[1];

            // Mesafe hesapla
            double distance = p1.DistanceTo(p2);

            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine($"📏 MESAFE: {distance:F3} mm");
            System.Diagnostics.Debug.WriteLine($"   Nokta 1: ({p1.X:F3}, {p1.Y:F3}, {p1.Z:F3})");
            System.Diagnostics.Debug.WriteLine($"   Nokta 2: ({p2.X:F3}, {p2.Y:F3}, {p2.Z:F3})");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

            // Çizgi çiz (iki nokta arası)
            measurementLine = new devDept.Eyeshot.Entities.Line(p1, p2)
            {
                Color = Color.Yellow,
                ColorMethod = colorMethodType.byEntity,
                LineWeight = 2
            };
            design.Entities.Add(measurementLine, "LengthMeasurement");

            // ❌ Ekran üzerindeki yazı kaldırıldı (Form kullanıyoruz)
            /*
            // Mesafe yazısını ortaya yerleştir
            Point3D midPoint = new Point3D(
                (p1.X + p2.X) / 2,
                (p1.Y + p2.Y) / 2,
                (p1.Z + p2.Z) / 2
            );

            measurementText = new devDept.Eyeshot.Entities.Text(
                midPoint,
                $"Distance: {distance:F3} mm",
                3.0  // Text height
            )
            {
                Color = Color.Cyan,
                ColorMethod = colorMethodType.byEntity
            };
            design.Entities.Add(measurementText, "LengthMeasurement");
            */

            design.Invalidate();

            // ✅ Formu güncelle (MessageBox yerine)
            double deltaX = Math.Abs(p2.X - p1.X);
            double deltaY = Math.Abs(p2.Y - p1.Y);
            double deltaZ = Math.Abs(p2.Z - p1.Z);

            // Direkt Point3D kullan (p1 ve p2 zaten Point3D)
            measurementForm.UpdateMeasurement(distance, deltaX, deltaY, deltaZ, p1, p2);

            // Reset - yeni ölçüm için hazır
            System.Diagnostics.Debug.WriteLine("✅ Yeni ölçüm için hazır. İlk noktayı seçin.");
            selectedPoints.Clear();
            pointMarkers.Clear();
        }

        // ═══════════════════════════════════════════════════════════
        // GÖRSELLERİ TEMİZLE
        // ═══════════════════════════════════════════════════════════

        private void ClearVisuals()
        {
            // Snap marker'ı temizle
            RemoveTempSnapMarker();

            // Layer'daki tüm entity'leri tek tek sil
            for (int i = design.Entities.Count - 1; i >= 0; i--)
            {
                if (design.Entities[i].LayerName == "LengthMeasurement")
                {
                    design.Entities.RemoveAt(i);
                }
            }

            pointMarkers.Clear();
            measurementLine = null;
            measurementText = null;
        }
    }
}