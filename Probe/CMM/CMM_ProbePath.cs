using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _014.Probe.CMM
{
    /// <summary>
    /// CMM Probe Yolu
    /// Probe noktalarının sıralı listesi + basit hesaplamalar
    /// </summary>
    public class CMM_ProbePath
    {
        // ═══════════════════════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Probe noktaları listesi
        /// </summary>
        public List<CMM_ProbePoint> Points { get; set; }

        /// <summary>
        /// Yol adı
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ölçüm tipi
        /// </summary>
        public MeasurementType Type { get; set; }

        /// <summary>
        /// Toplam mesafe (mm)
        /// </summary>
        public double TotalDistance { get; private set; }

        /// <summary>
        /// Tahmini süre (saniye)
        /// </summary>
        public double EstimatedTime { get; private set; }

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTORS
        // ═══════════════════════════════════════════════════════════

        public CMM_ProbePath()
        {
            Points = new List<CMM_ProbePoint>();
            Name = "Untitled Path";
            Type = MeasurementType.Surface;
        }

        public CMM_ProbePath(string name)
        {
            Points = new List<CMM_ProbePoint>();
            Name = name;
            Type = MeasurementType.Surface;
        }

        // ═══════════════════════════════════════════════════════════
        // METHODS - NOKTA EKLEME
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Probe noktası ekle
        /// </summary>
        public void AddPoint(CMM_ProbePoint point)
        {
            point.Index = Points.Count;
            Points.Add(point);
        }

        /// <summary>
        /// Pozisyon ve yön ile nokta ekle
        /// </summary>
        public void AddPoint(Point3D position, Vector3D approachDirection)
        {
            var point = new CMM_ProbePoint(position, approachDirection);
            AddPoint(point);
        }

        /// <summary>
        /// Sadece pozisyon ile nokta ekle (Z ekseni yukarıdan yaklaşım)
        /// </summary>
        public void AddPoint(Point3D position)
        {
            AddPoint(position, Vector3D.AxisZ);
        }

        // ═══════════════════════════════════════════════════════════
        // METHODS - HESAPLAMALAR
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Toplam mesafeyi hesapla
        /// </summary>
        public void CalculateTotalDistance()
        {
            TotalDistance = 0;

            for (int i = 1; i < Points.Count; i++)
            {
                Point3D p1 = Points[i - 1].Position;
                Point3D p2 = Points[i].Position;
                TotalDistance += p1.DistanceTo(p2);
            }
        }

        /// <summary>
        /// Tahmini süreyi hesapla
        /// </summary>
        public void CalculateEstimatedTime()
        {
            EstimatedTime = 0;

            for (int i = 1; i < Points.Count; i++)
            {
                Point3D p1 = Points[i - 1].Position;
                Point3D p2 = Points[i].Position;
                double distance = p1.DistanceTo(p2);
                
                // Ortalama hız (mm/min)
                double avgSpeed = (Points[i - 1].ProbeSpeed + Points[i].ProbeSpeed) / 2.0;
                
                if (avgSpeed > 0)
                {
                    // Süre = Mesafe / Hız (dakika cinsinden)
                    EstimatedTime += distance / avgSpeed * 60.0;  // Saniyeye çevir
                }
            }

            // Her probe için ek bekleme süresi (probe temas + geri çekilme)
            EstimatedTime += Points.Count * 2.0;  // Her nokta için 2 saniye ekstra
        }

        /// <summary>
        /// Tüm hesaplamaları yap
        /// </summary>
        public void Calculate()
        {
            CalculateTotalDistance();
            CalculateEstimatedTime();
        }

        // ═══════════════════════════════════════════════════════════
        // METHODS - YARDIMCI
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Merkez noktasını hesapla
        /// </summary>
        public Point3D GetCenterPoint()
        {
            if (Points.Count == 0)
                return Point3D.Origin;

            double avgX = Points.Average(p => p.Position.X);
            double avgY = Points.Average(p => p.Position.Y);
            double avgZ = Points.Average(p => p.Position.Z);

            return new Point3D(avgX, avgY, avgZ);
        }

        /// <summary>
        /// Yolu temizle
        /// </summary>
        public void Clear()
        {
            Points.Clear();
            TotalDistance = 0;
            EstimatedTime = 0;
        }

        /// <summary>
        /// String gösterimi
        /// </summary>
        public override string ToString()
        {
            return $"CMM_ProbePath: {Name}, {Points.Count} points, " +
                   $"Distance: {TotalDistance:F2}mm, Time: {EstimatedTime:F1}s";
        }
    }

    // ═══════════════════════════════════════════════════════════
    // MEASUREMENT TYPE ENUM
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Ölçüm tipleri
    /// </summary>
    public enum MeasurementType
    {
        /// <summary>
        /// Yüzey taraması (grid)
        /// </summary>
        Surface,

        /// <summary>
        /// Daire/delik ölçümü
        /// </summary>
        Circle,

        /// <summary>
        /// Kenar/çizgi ölçümü
        /// </summary>
        Line,

        /// <summary>
        /// Nokta ölçümü
        /// </summary>
        Point,

        /// <summary>
        /// Özel/manuel yol
        /// </summary>
        Custom
    }
}
