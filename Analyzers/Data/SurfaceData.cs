using devDept.Geometry;

namespace _014.Analyzers.Data
{
    /// <summary>
    /// Yüzey verilerini tutan sınıf
    /// </summary>
    public class SurfaceData
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public int EntityIndex { get; set; }
        public int FaceIndex { get; set; }
        public Vector3D Normal { get; set; }
        public Point3D Center { get; set; }
        public string SurfaceType { get; set; }
        public string Group { get; set; }
        public bool IsLabelVisible { get; set; } = true;
        public bool IsArrowVisible { get; set; } = true;
        public bool IsSelectable { get; set; } = true;
    }
}
