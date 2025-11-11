namespace _014.Analyzers.Data
{
    /// <summary>
    /// Yüzey ana tipleri - Eyeshot Face sınıflarına karşılık gelir
    /// </summary>
    public enum SurfaceMainType
    {
        /// <summary>
        /// Bilinmeyen tip
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Düz yüzey (PlaneFace)
        /// </summary>
        Planar = 1,

        /// <summary>
        /// Silindir yüzeyi (CylindricalFace)
        /// </summary>
        Cylindrical = 2,

        /// <summary>
        /// Küre yüzeyi (SphericalFace)
        /// </summary>
        Spherical = 4,

        /// <summary>
        /// Torus yüzeyi (ToroidalFace)
        /// </summary>
        Toroidal = 5,

        /// <summary>
        /// NURBS serbest form yüzey (NurbsFace)
        /// </summary>
        NURBS = 6,

        /// <summary>
        /// Döndürme yüzeyi (RevolutionFace)
        /// </summary>
        Revolution = 7,

        /// <summary>
        /// Ekstrüzyon yüzeyi (ExtrusionFace)
        /// </summary>
        Extrusion = 8
    }

    /// <summary>
    /// Yüzey grupları - CNC işlenebilirlik ve ölçüm stratejisine göre
    /// </summary>
    public enum SurfaceGroup
    {
        /// <summary>
        /// Alt yüzeyler (Z-) - Yeşil, seçilemez
        /// </summary>
        BottomSurface = 0,

        /// <summary>
        /// Dik yüzeyler (Z+, X±, Y±) - Sarı, kolay erişim
        /// </summary>
        VerticalSurface = 1,

        /// <summary>
        /// Eğik düz yüzeyler - Mavi, orta zorluk
        /// </summary>
        InclinedPlanar = 2,

        /// <summary>
        /// Silindirik yüzeyler - Mor, özel prob
        /// </summary>
        Cylindrical = 3,

        /// <summary>
        /// Küresel yüzeyler - Kahverengi, çok zor
        /// </summary>
        Spherical = 5,

        /// <summary>
        /// Toroidal yüzeyler - Koyu mor, çok zor
        /// </summary>
        Toroidal = 6,

        /// <summary>
        /// Kompleks yüzeyler (NURBS, Revolve, Extrude) - Siyah, scanning
        /// </summary>
        Complex = 7
    }

    /// <summary>
    /// Düzlemsel yüzey yönleri
    /// </summary>
    public enum PlanarOrientation
    {
        /// <summary>
        /// Yukarı bakan (Z+)
        /// </summary>
        Top = 0,

        /// <summary>
        /// Aşağı bakan (Z-) - Seçilemez!
        /// </summary>
        Bottom = 1,

        /// <summary>
        /// Ön yüz (Y+)
        /// </summary>
        Front = 2,

        /// <summary>
        /// Arka yüz (Y-)
        /// </summary>
        Back = 3,

        /// <summary>
        /// Sağ yüz (X+)
        /// </summary>
        Right = 4,

        /// <summary>
        /// Sol yüz (X-)
        /// </summary>
        Left = 5,

        /// <summary>
        /// Eğik yüzey
        /// </summary>
        Inclined = 6
    }

    /// <summary>
    /// CNC erişilebilirlik seviyeleri
    /// </summary>
    public enum CNCAccessibility
    {
        /// <summary>
        /// Erişilemez (örn: alt yüzey)
        /// </summary>
        Inaccessible = 0,

        /// <summary>
        /// Kolay erişim (dik yüzeyler)
        /// </summary>
        Easy = 1,

        /// <summary>
        /// Orta zorluk (eğik yüzeyler)
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Zor (silindirik, konik)
        /// </summary>
        Hard = 3,

        /// <summary>
        /// Çok zor (küre, torus, NURBS)
        /// </summary>
        VeryHard = 4
    }
}
