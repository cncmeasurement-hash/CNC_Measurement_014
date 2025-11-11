using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System.Drawing;

namespace _014.Probe.CMM
{
    /// <summary>
    /// CMM Probe Path Görselleştirici - Basitleştirilmiş Versiyon
    /// Sadece Line entity'leri kullanır (Mesh sorunları yok)
    /// </summary>
    public class CMM_PathVisualizer
    {
        // ═══════════════════════════════════════════════════════════
        // FIELDS
        // ═══════════════════════════════════════════════════════════

        private Design design;
        private const string LAYER_NAME = "CMM_ProbePath";

        // Renkler
        private readonly Color colorPath = Color.Blue;           // Yol çizgisi
        private readonly Color colorProbePoint = Color.Red;      // Probe noktası
        private readonly Color colorApproach = Color.Green;      // Yaklaşma yönü

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        public CMM_PathVisualizer(Design design)
        {
            this.design = design;
            CreateLayer();
        }

        // ═══════════════════════════════════════════════════════════
        // PUBLIC METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Probe yolunu çiz
        /// </summary>
        public void DrawPath(CMM_ProbePath path)
        {
            if (path == null || path.Points.Count == 0)
                return;

            // Önce layer'ı temizle
            Clear();

            // 1. Noktalar arası çizgileri çiz
            DrawConnectingLines(path);

            // 2. Her probe noktasını çiz (basit çizgilerle X işareti)
            DrawProbePoints(path);

            // 3. Yaklaşma yönlerini çiz (oklar)
            DrawApproachDirections(path);

            // Ekranı yenile
            design.Invalidate();
        }

        /// <summary>
        /// Görselleştirmeyi temizle
        /// </summary>
        public void Clear()
        {
            if (design.Layers.Contains(LAYER_NAME))
            {
                // Layer'daki tüm entity'leri sil
                for (int i = design.Entities.Count - 1; i >= 0; i--)
                {
                    if (design.Entities[i].LayerName == LAYER_NAME)
                    {
                        design.Entities.RemoveAt(i);
                    }
                }
            }
            design.Invalidate();
        }

        /// <summary>
        /// Görünürlüğü değiştir
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (design.Layers.Contains(LAYER_NAME))
            {
                design.Layers[LAYER_NAME].Visible = visible;
                design.Invalidate();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE METHODS - ÇİZİM
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Noktalar arası bağlantı çizgileri
        /// </summary>
        private void DrawConnectingLines(CMM_ProbePath path)
        {
            for (int i = 1; i < path.Points.Count; i++)
            {
                Point3D p1 = path.Points[i - 1].Position;
                Point3D p2 = path.Points[i].Position;

                Line line = new Line(p1, p2);
                line.Color = colorPath;
                line.ColorMethod = colorMethodType.byEntity;
                line.LineWeight = 2;
                line.LayerName = LAYER_NAME;

                design.Entities.Add(line);
            }
        }

        /// <summary>
        /// Probe noktalarını çiz (basit X işareti)
        /// </summary>
        private void DrawProbePoints(CMM_ProbePath path)
        {
            foreach (var point in path.Points)
            {
                // X işareti çiz (2 çapraz çizgi)
                double size = 2.0;  // 2mm boyut

                // Çapraz 1: sol-üst -> sağ-alt
                Point3D p1 = new Point3D(point.Position.X - size, point.Position.Y + size, point.Position.Z);
                Point3D p2 = new Point3D(point.Position.X + size, point.Position.Y - size, point.Position.Z);
                Line cross1 = new Line(p1, p2);
                cross1.Color = colorProbePoint;
                cross1.ColorMethod = colorMethodType.byEntity;
                cross1.LineWeight = 3;
                cross1.LayerName = LAYER_NAME;
                design.Entities.Add(cross1);

                // Çapraz 2: sol-alt -> sağ-üst
                Point3D p3 = new Point3D(point.Position.X - size, point.Position.Y - size, point.Position.Z);
                Point3D p4 = new Point3D(point.Position.X + size, point.Position.Y + size, point.Position.Z);
                Line cross2 = new Line(p3, p4);
                cross2.Color = colorProbePoint;
                cross2.ColorMethod = colorMethodType.byEntity;
                cross2.LineWeight = 3;
                cross2.LayerName = LAYER_NAME;
                design.Entities.Add(cross2);

                // Merkez nokta (küçük yuvarlak için 4 çizgi)
                double dotSize = 0.5;
                DrawSmallCircle(point.Position, dotSize, colorProbePoint);
            }
        }

        /// <summary>
        /// Yaklaşma yönlerini çiz (basit oklar)
        /// </summary>
        private void DrawApproachDirections(CMM_ProbePath path)
        {
            foreach (var point in path.Points)
            {
                // Yaklaşma yönü oku (5mm uzunluk)
                double arrowLength = 5.0;
                Vector3D direction = point.ApproachDirection;
                direction.Normalize();
                direction = direction * arrowLength;
                
                Point3D arrowEnd = point.Position - direction;

                // Ok gövdesi
                Line arrow = new Line(point.Position, arrowEnd);
                arrow.Color = colorApproach;
                arrow.ColorMethod = colorMethodType.byEntity;
                arrow.LineWeight = 2;
                arrow.LayerName = LAYER_NAME;
                design.Entities.Add(arrow);

                // Ok başı (basit V şekli)
                double arrowheadSize = 1.5;
                Vector3D perpendicular1 = GetPerpendicularVector(direction);
                perpendicular1.Normalize();
                perpendicular1 = perpendicular1 * arrowheadSize;

                Vector3D perpendicular2 = Vector3D.Cross(direction, perpendicular1);
                perpendicular2.Normalize();
                perpendicular2 = perpendicular2 * arrowheadSize;

                // Ok başı çizgileri
                Point3D arrowTip1 = arrowEnd + direction * 0.3 + perpendicular1;
                Point3D arrowTip2 = arrowEnd + direction * 0.3 + perpendicular2;
                Point3D arrowTip3 = arrowEnd + direction * 0.3 - perpendicular1;
                Point3D arrowTip4 = arrowEnd + direction * 0.3 - perpendicular2;

                Line arrowLine1 = new Line(arrowEnd, arrowTip1);
                Line arrowLine2 = new Line(arrowEnd, arrowTip2);
                Line arrowLine3 = new Line(arrowEnd, arrowTip3);
                Line arrowLine4 = new Line(arrowEnd, arrowTip4);

                foreach (var line in new[] { arrowLine1, arrowLine2, arrowLine3, arrowLine4 })
                {
                    line.Color = colorApproach;
                    line.ColorMethod = colorMethodType.byEntity;
                    line.LineWeight = 2;
                    line.LayerName = LAYER_NAME;
                    design.Entities.Add(line);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE METHODS - YARDIMCI
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Küçük daire çiz (8 çizgiyle yaklaşık daire)
        /// </summary>
        private void DrawSmallCircle(Point3D center, double radius, Color color)
        {
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                double angle1 = 2 * Math.PI * i / segments;
                double angle2 = 2 * Math.PI * (i + 1) / segments;

                Point3D p1 = new Point3D(
                    center.X + radius * Math.Cos(angle1),
                    center.Y + radius * Math.Sin(angle1),
                    center.Z
                );

                Point3D p2 = new Point3D(
                    center.X + radius * Math.Cos(angle2),
                    center.Y + radius * Math.Sin(angle2),
                    center.Z
                );

                Line segment = new Line(p1, p2);
                segment.Color = color;
                segment.ColorMethod = colorMethodType.byEntity;
                segment.LineWeight = 2;
                segment.LayerName = LAYER_NAME;
                design.Entities.Add(segment);
            }
        }

        /// <summary>
        /// Bir vektöre dik vektör bul
        /// </summary>
        private Vector3D GetPerpendicularVector(Vector3D v)
        {
            // Eğer Z ekseni ile paralel değilse, Z ile cross product al
            if (Math.Abs(v.Z) < 0.9)
            {
                return Vector3D.Cross(v, Vector3D.AxisZ);
            }
            else
            {
                // Z ile paralelse, X ile cross product al
                return Vector3D.Cross(v, Vector3D.AxisX);
            }
        }

        /// <summary>
        /// Layer oluştur
        /// </summary>
        private void CreateLayer()
        {
            if (!design.Layers.Contains(LAYER_NAME))
            {
                Layer layer = new Layer(LAYER_NAME);
                layer.Color = colorPath;
                layer.Visible = true;
                design.Layers.Add(layer);
            }
        }
    }
}
