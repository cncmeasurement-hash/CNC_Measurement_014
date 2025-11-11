using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System.Drawing;

namespace _014.Analyzers.Data
{
    /// <summary>
    /// İşlenmiş yüzey bilgilerini tutar
    /// Surface Processor tarafından oluşturulan her yüzey için kayıt
    /// </summary>
    public class ProcessedSurface
    {
        /// <summary>
        /// Yüzey index numarası (0'dan başlar)
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Yüzey adı (örn: "Surface_0", "Surface_1")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Orijinal Brep'teki entity index
        /// </summary>
        public int OriginalEntityIndex { get; set; }

        /// <summary>
        /// Orijinal Brep'teki face index
        /// </summary>
        public int OriginalFaceIndex { get; set; }

        /// <summary>
        /// Yeni oluşturulan Surface entity
        /// </summary>
        public Entity SurfaceEntity { get; set; }

        /// <summary>
        /// Normal vektör (birim vektör)
        /// </summary>
        public Vector3D Normal { get; set; }

        /// <summary>
        /// Yüzey merkez noktası
        /// </summary>
        public Point3D Center { get; set; }

        /// <summary>
        /// Yüzey tipi: "TOP (Z+)", "BOTTOM (Z-)", "LEFT (X-)", vb.
        /// </summary>
        public string SurfaceType { get; set; }

        /// <summary>
        /// Grup: "Alt Yüzey", "Dik", "Eğik"
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Ok rengi (🔴🟡🔵)
        /// </summary>
        public Color ArrowColor { get; set; }

        /// <summary>
        /// Yüzey layer adı
        /// </summary>
        public string LayerName { get; set; }

        /// <summary>
        /// Seçilebilir mi? (Alt yüzeyler seçilemez)
        /// </summary>
        public bool IsSelectable { get; set; }

        /// <summary>
        /// Ok entity (normal vektör)
        /// </summary>
        public Entity ArrowEntity { get; set; }

        /// <summary>
        /// Etiket entity
        /// </summary>
        public Entity LabelEntity { get; set; }

        public ProcessedSurface()
        {
            Name = string.Empty;
            SurfaceType = string.Empty;
            Group = string.Empty;
            LayerName = string.Empty;
            IsSelectable = true;
        }
    }
}
