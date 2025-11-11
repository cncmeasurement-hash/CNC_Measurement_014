namespace _014.Managers.Selection
{
    /// <summary>
    /// 3D sahne üzerinde yapılabilecek seçim modlarını tanımlar.
    /// Kullanıcının farklı seçim tiplerini aktif etmesini sağlar.
    /// </summary>
    /// <remarks>
    /// <para>Bu enum, SelectionManager tarafından aktif seçim modunu belirlemek için kullanılır.</para>
    /// <para>Aynı anda yalnızca bir mod aktif olabilir.</para>
    /// </remarks>
    public enum SelectionMode
    {
        /// <summary>
        /// Hiçbir seçim modu aktif değil.
        /// Varsayılan durum, kullanıcı yalnızca görüntüleme yapabilir.
        /// </summary>
        None = 0,

        /// <summary>
        /// Yüzey (Face) seçim modu.
        /// Kullanıcı 3D modeller üzerinde tek bir yüzey seçebilir.
        /// Normal vektörü ve yüzey merkezi hesaplanır.
        /// </summary>
        Face = 1,

        /// <summary>
        /// Tam entity seçim modu.
        /// Kullanıcı tüm 3D nesneyi (mesh, solid, vb.) seçebilir.
        /// Yüzey bazlı değil, tüm nesne seçilir.
        /// </summary>
        Entity = 2,

        /// <summary>
        /// Nokta seçim modu.
        /// Kullanıcı 3D modeller üzerinde belirli noktaları işaretleyebilir.
        /// Her nokta için koordinat ve normal vektörü kaydedilir.
        /// </summary>
        Point = 3
    }
}