using devDept.Geometry;
using System;

namespace _014.Probe.CMM
{
    /// <summary>
    /// CMM Probe Tek Dokunma Noktası
    /// Basit yapı - sadece pozisyon ve yaklaşma yönü
    /// </summary>
    public class CMM_ProbePoint
    {
        // ═══════════════════════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Probe'un dokunacağı 3D pozisyon
        /// </summary>
        public Point3D Position { get; set; }

        /// <summary>
        /// Probe yaklaşma yönü (normal vektör)
        /// Örnek: Vector3D.AxisZ (yukarıdan yaklaş)
        /// </summary>
        public Vector3D ApproachDirection { get; set; }

        /// <summary>
        /// Probe hızı (mm/min)
        /// Tipik CMM hızı: 50-200 mm/min
        /// </summary>
        public double ProbeSpeed { get; set; }

        /// <summary>
        /// Nokta tipi (ölçüm amacı)
        /// </summary>
        public ProbePointType Type { get; set; }

        /// <summary>
        /// Sıra numarası
        /// </summary>
        public int Index { get; set; }

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTORS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Boş constructor
        /// </summary>
        public CMM_ProbePoint()
        {
            Position = Point3D.Origin;
            ApproachDirection = Vector3D.AxisZ;  // Default: yukarıdan yaklaş
            ProbeSpeed = 100.0;  // Default: 100 mm/min
            Type = ProbePointType.Surface;
        }

        /// <summary>
        /// Pozisyon ve yön ile oluştur
        /// </summary>
        public CMM_ProbePoint(Point3D position, Vector3D approachDirection)
        {
            Position = position;
            ApproachDirection = approachDirection;
            ApproachDirection.Normalize();  // Birim vektöre çevir
            ProbeSpeed = 100.0;
            Type = ProbePointType.Surface;
        }

        /// <summary>
        /// Tam parametreli constructor
        /// </summary>
        public CMM_ProbePoint(Point3D position, Vector3D approachDirection, double speed, ProbePointType type)
        {
            Position = position;
            ApproachDirection = approachDirection;
            ApproachDirection.Normalize();
            ProbeSpeed = speed;
            Type = type;
        }

        // ═══════════════════════════════════════════════════════════
        // METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Güvenli yaklaşma noktası (probe başlangıç pozisyonu)
        /// Hedef noktadan belirli mesafe geri
        /// </summary>
        public Point3D GetSafeApproachPoint(double safeDistance = 5.0)
        {
            // Yaklaşma yönünün tersine git
            Vector3D offset = ApproachDirection * -safeDistance;
            return Position + offset;
        }

        /// <summary>
        /// String gösterimi
        /// </summary>
        public override string ToString()
        {
            return $"ProbePoint #{Index}: Pos({Position.X:F2}, {Position.Y:F2}, {Position.Z:F2}) " +
                   $"Dir({ApproachDirection.X:F2}, {ApproachDirection.Y:F2}, {ApproachDirection.Z:F2}) " +
                   $"Speed:{ProbeSpeed:F0} mm/min Type:{Type}";
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PROBE POINT TYPE ENUM
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Probe noktası tipleri
    /// </summary>
    public enum ProbePointType
    {
        /// <summary>
        /// Yüzey ölçümü
        /// </summary>
        Surface,

        /// <summary>
        /// Kenar/köşe ölçümü
        /// </summary>
        Edge,

        /// <summary>
        /// Delik merkezi ölçümü
        /// </summary>
        Hole,

        /// <summary>
        /// Daire/arc ölçümü
        /// </summary>
        Circle,

        /// <summary>
        /// Referans noktası (sıfır noktası)
        /// </summary>
        Reference
    }
}
