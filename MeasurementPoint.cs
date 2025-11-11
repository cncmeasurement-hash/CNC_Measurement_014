using devDept.Geometry;
using System;

namespace _014.Managers.Data
{
    /// <summary>
    /// Tek bir measurement noktasının tüm bilgilerini içeren veri yapısı
    /// Point Probing, Ridge Width, Angle Measurement gibi tüm modlar için kullanılır
    /// JSON serialization için hazır
    /// </summary>
    public class MeasurementPoint
    {
        // ═══════════════════════════════════════════════════════════
        // TEMEL BİLGİLER
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Measurement modu: "PointProbing", "RidgeWidth", "Angle"
        /// </summary>
        public string MeasurementMode { get; set; }

        /// <summary>
        /// Hangi gruba ait (TreeView'deki grup ID)
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Grup adı (örn: "Probing - Point 1", "Ridge Width 1")
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Grup içindeki nokta indexi (0, 1, 2, ...)
        /// </summary>
        public int PointIndex { get; set; }

        // ═══════════════════════════════════════════════════════════
        // PROBE BİLGİLERİ
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Kullanılan probe adı (örn: "Renishaw TP20")
        /// </summary>
        public string ProbeName { get; set; }

        /// <summary>
        /// Probe çapı (mm)
        /// </summary>
        public double ProbeDiameter { get; set; }

        // ═══════════════════════════════════════════════════════════
        // KOORDİNAT BİLGİLERİ
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Nokta koordinatı (X, Y, Z) - Yüzeye temas noktası
        /// </summary>
        public Point3D Position { get; set; }

        /// <summary>
        /// Marker görsel pozisyonu (X, Y, Z) - Ekranda gösterilen konum
        /// </summary>
        public Point3D MarkerPosition { get; set; }

        /// <summary>
        /// Yüzey normal vektörü (X, Y, Z) - Probe yaklaşma yönü
        /// </summary>
        public Vector3D SurfaceNormal { get; set; }

        // ═══════════════════════════════════════════════════════════
        // YÜZEY BİLGİLERİ
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Hangi yüzey index'i (Face ID)
        /// </summary>
        public int SurfaceIndex { get; set; }

        /// <summary>
        /// Yüzey rengi (Ridge Width için: "Yellow", "Red", "Green", "Blue")
        /// </summary>
        public string SurfaceColor { get; set; }

        // ═══════════════════════════════════════════════════════════
        // CNC PARAMETRELERİ
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Retract mesafesi (mm) - Geri çekilme mesafesi
        /// </summary>
        public double RetractDistance { get; set; }

        /// <summary>
        /// Z Safety / Clearance Plane (mm) - Güvenli yükseklik
        /// </summary>
        public double ZSafety { get; set; }

        // ═══════════════════════════════════════════════════════════
        // TOOLPATH BİLGİLERİ
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Yaklaşma noktası (X, Y, Z) - Probe'un güvenli yaklaşma noktası
        /// </summary>
        public Point3D ApproachPoint { get; set; }

        /// <summary>
        /// Temas noktası (X, Y, Z) - Gerçek ölçüm noktası
        /// </summary>
        public Point3D TouchPoint { get; set; }

        // ═══════════════════════════════════════════════════════════
        // METADATA
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Nokta ne zaman oluşturuldu
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Nokta hala aktif mi (silinmedi mi)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Ek notlar (kullanıcı girişi için)
        /// </summary>
        public string Notes { get; set; }

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Yeni bir MeasurementPoint oluşturur
        /// </summary>
        public MeasurementPoint()
        {
            CreatedAt = DateTime.Now;
            IsActive = true;
            
            // Default değerler
            Position = new Point3D(0, 0, 0);
            MarkerPosition = new Point3D(0, 0, 0);
            SurfaceNormal = new Vector3D(0, 0, 1);
            ApproachPoint = new Point3D(0, 0, 0);
            TouchPoint = new Point3D(0, 0, 0);
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Debug için string representation
        /// </summary>
        public override string ToString()
        {
            return $"{MeasurementMode} - Group {GroupId} - Point {PointIndex} - ({Position.X:F3}, {Position.Y:F3}, {Position.Z:F3})";
        }
    }
}
