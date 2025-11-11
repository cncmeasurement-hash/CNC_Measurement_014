using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace _014
{
    /// <summary>
    /// CYLINDRICAL ANALYZER - VISUALIZATION
    /// PARTIAL CLASS 3/3: Visualization (markers, lines)
    /// </summary>
    public partial class CylindricalAnalyzer
    {
        // ═══════════════════════════════════════════════════════════
        // VISUALIZATION METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Silindir görselleştirme (Point + Kesik çizgi)
        /// </summary>
        private List<Entity> CreateCylindricalVisualization(CylindricalAxisInfo info, int index)
        {
            var entities = new List<Entity>();

            // Renk seçimi (HOLE vs BOSS)
            System.Drawing.Color pointColor, lineColor;

            switch (info.Type)
            {
                case CylinderType.Hole:
                    pointColor = System.Drawing.Color.Red;
                    lineColor = System.Drawing.Color.Red;
                    break;
                case CylinderType.Boss:
                    pointColor = System.Drawing.Color.Blue;
                    lineColor = System.Drawing.Color.Blue;
                    break;
                default:
                    pointColor = System.Drawing.Color.Gray;
                    lineColor = System.Drawing.Color.Gray;
                    break;
            }

            // ═══════════════════════════════════════════════════════
            // 1. ALT MERKEZ (Point)
            // ═══════════════════════════════════════════════════════
            devDept.Eyeshot.Entities.Point bottomPt = new devDept.Eyeshot.Entities.Point(info.BottomCenter);
            bottomPt.Color = pointColor;
            bottomPt.ColorMethod = colorMethodType.byEntity;
            bottomPt.LayerName = ANALYSIS_LAYER;
            entities.Add(bottomPt);

            // ═══════════════════════════════════════════════════════
            // 2. ÜST MERKEZ (Point)
            // ═══════════════════════════════════════════════════════
            devDept.Eyeshot.Entities.Point topPt = new devDept.Eyeshot.Entities.Point(info.TopCenter);
            topPt.Color = pointColor;
            topPt.ColorMethod = colorMethodType.byEntity;
            topPt.LayerName = ANALYSIS_LAYER;
            entities.Add(topPt);

            // ═══════════════════════════════════════════════════════
            // 3. EKSEN ÇİZGİSİ (Kesikli çizgi - MANUEL SEGMENTLER)
            // ═══════════════════════════════════════════════════════
            
            double totalLength = info.BottomCenter.DistanceTo(info.TopCenter);

            // ✅ HOLE için direction TERS! (delik içe doğru)
            Vector3D direction;
            if (info.Type == CylinderType.Hole)
            {
                // HOLE: BottomCenter → TopCenter TERS yönde (içe doğru)
                direction = new Vector3D(info.BottomCenter.X - info.TopCenter.X,
                                        info.BottomCenter.Y - info.TopCenter.Y,
                                        info.BottomCenter.Z - info.TopCenter.Z);
                System.Diagnostics.Debug.WriteLine("   🔴 HOLE: Direction TERS çevrildi (içe doğru)");
            }
            else
            {
                // BOSS: Normal direction (dışa doğru)
                direction = new Vector3D(info.TopCenter.X - info.BottomCenter.X,
                                        info.TopCenter.Y - info.BottomCenter.Y,
                                        info.TopCenter.Z - info.BottomCenter.Z);
                System.Diagnostics.Debug.WriteLine("   🔵 BOSS: Direction normal (dışa doğru)");
            }
            direction.Normalize();

            // Kesik çizgi pattern: 5mm çizgi, 3mm boşluk
            double segmentLength = 5.0;
            double gapLength = 3.0;
            double patternLength = segmentLength + gapLength;
            int segmentCount = (int)(totalLength / patternLength);

            for (int i = 0; i <= segmentCount; i++)
            {
                double startDist = i * patternLength;
                double endDist = startDist + segmentLength;

                if (startDist >= totalLength) break;
                if (endDist > totalLength) endDist = totalLength;

                Point3D segmentStart = new Point3D(
                    info.BottomCenter.X + direction.X * startDist,
                    info.BottomCenter.Y + direction.Y * startDist,
                    info.BottomCenter.Z + direction.Z * startDist
                );

                Point3D segmentEnd = new Point3D(
                    info.BottomCenter.X + direction.X * endDist,
                    info.BottomCenter.Y + direction.Y * endDist,
                    info.BottomCenter.Z + direction.Z * endDist
                );

                Line segment = new Line(segmentStart, segmentEnd);
                segment.Color = lineColor;
                segment.ColorMethod = colorMethodType.byEntity;
                segment.LineWeight = 1;
                segment.Selectable = false;
                segment.LayerName = ANALYSIS_LAYER;
                entities.Add(segment);
            }

            System.Diagnostics.Debug.WriteLine($"   🎨 Görselleştirme oluşturuldu:");
            System.Diagnostics.Debug.WriteLine($"      Bottom: ({info.BottomCenter.X:F2}, {info.BottomCenter.Y:F2}, {info.BottomCenter.Z:F2})");
            System.Diagnostics.Debug.WriteLine($"      Top: ({info.TopCenter.X:F2}, {info.TopCenter.Y:F2}, {info.TopCenter.Z:F2})");
            System.Diagnostics.Debug.WriteLine($"      Renk: {pointColor.Name}");
            System.Diagnostics.Debug.WriteLine($"      Segment sayısı: {segmentCount + 1}");

            return entities;
        }
    }
}
