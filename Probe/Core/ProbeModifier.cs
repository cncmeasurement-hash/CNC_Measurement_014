using System;
using System.Drawing;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Eyeshot.Control;
using _014.Probe.Visualization;

namespace _014.Probe.Core
{
    /// <summary>
    /// Prob modelleri üzerinde Boolean işlemleri ve modifikasyonlar yapmak için yardımcı sınıf.
    /// Referans silindir ekleme ve probe'dan çıkarma işlemlerini gerçekleştirir.
    /// </summary>
    internal static class ProbeModifier
    {
        // 🔹 Sabitler (Magic numbers yerine)
        /// <summary>Geçici konumlandırma için X koordinatı (mm)</summary>
        private const double TEMP_CONSTRUCTION_X = 100.0;

        /// <summary>İç silindir çapı için offset (mm)</summary>
        private const double INNER_DIAMETER_OFFSET = 1.0;

        /// <summary>Z konumu hesaplama böleni</summary>
        private const double Z_POSITION_DIVISOR = 5.0;

        /// <summary>
        /// Probun yanına referans silindir ekler, konumlandırır ve Boolean Difference ile probe modelinden çıkarır.
        /// Referans silindir, içi boş bir silindir olup probe montaj ve ölçüm işlemlerinde referans olarak kullanılır.
        /// </summary>
        /// <param name="design">Eyeshot Design kontrolü. İşlemin yapılacağı sahne.</param>
        /// <param name="d2">Gövde çapı (mm). Referans silindirin dış çapını belirler.</param>
        /// <param name="L1">Sap uzunluğu (mm). İç boşluk yüksekliğinin hesaplanmasında kullanılır.</param>
        /// <param name="L2">Gövde uzunluğu (mm). Referans silindirin yüksekliğini ve iç boşluk boyutunu belirler.</param>
        /// <remarks>
        /// <para><strong>İşlem Adımları:</strong></para>
        /// <para>1. Eski referans silindirler temizlenir (Layer_RefCylinder)</para>
        /// <para>2. Dış silindir oluşturulur: çap=d2, yükseklik=L2/4</para>
        /// <para>3. İç boşluk oluşturulur: çap=d2-1, yükseklik=L1+L2</para>
        /// <para>4. Boolean Difference ile iç boşluk dışarı çıkarılır</para>
        /// <para>5. Silindir konumlandırılır: X-100, Z+L1+(L2/5)</para>
        /// <para>6. Probe modelinden Boolean Difference ile çıkarılır</para>
        /// <para></para>
        /// <para><strong>Formüller:</strong></para>
        /// <para>- Dış yükseklik: L2 / 4</para>
        /// <para>- İç çap: d2 - 1</para>
        /// <para>- İç yükseklik: L1 + L2</para>
        /// <para>- Z offset: L1 + (L2 / 5)</para>
        /// <para></para>
        /// <para><strong>Hata Yönetimi:</strong></para>
        /// <para>Boolean işlemleri başarısız olursa silindir tek başına gösterilir.</para>
        /// <para>Probe bulunamazsa silindir Layer_RefCylinder katmanına eklenir.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Form_New_Prob'dan gelen değerlerle referans silindir ekle
        /// ProbeModifier.AddReferenceCylinder(design1, d2: 58, L1: 55, L2: 75);
        /// </code>
        /// </example>
        public static void AddReferenceCylinder(Design design, double d2, double L1, double L2)
        {
            if (design == null)
                return;

            // 🔹 Formül değişkenleri (Form_New_Prob'dan gelen değerler)
            double bodyDiameter = d2;
            double shaftLength = L1;
            double bodyLength = L2;

            // 🔹 Eski silindirleri temizle
            for (int i = design.Entities.Count - 1; i >= 0; i--)
            {
                var ent = design.Entities[i];
                if (ent.LayerName == ProbeLayerNames.RefCylinder)
                    design.Entities.RemoveAt(i);
            }

            // =========================================================
            // 🔸 1️⃣ SİYAH SİLİNDİR (Ana gövde - d2 çapında, L2/4 boyunda)
            // =========================================================
            double outerRadius = bodyDiameter / 2.0;
            double outerHeight = bodyLength / 4.0;

            var blackCylinder = Mesh.CreateCylinder(outerRadius, outerHeight, 64);
            blackCylinder.Translate(TEMP_CONSTRUCTION_X, 0, 0);

            // =========================================================
            // 🔸 2️⃣ BEYAZ SİLİNDİR (Delik - d2-1 çapında, L1+L2 boyunda)
            // =========================================================
            double innerRadius = (bodyDiameter - INNER_DIAMETER_OFFSET) / 2.0;
            double innerHeight = shaftLength + bodyLength;

            var whiteCylinder = Mesh.CreateCylinder(innerRadius, innerHeight, 64);
            whiteCylinder.Translate(TEMP_CONSTRUCTION_X, 0, 0);

            // =========================================================
            // 🔸 3️⃣ BOOLEAN SUBTRACT (Beyaz'ı siyahtan çıkar)
            // =========================================================
            Solid resultCylinder = null;

            try
            {
                // Mesh'leri Solid'e dönüştür
                var blackSolid = blackCylinder.ConvertToSolid();
                var whiteSolid = whiteCylinder.ConvertToSolid();

                // Boolean çıkarma işlemi
                var resultSolids = Solid.Difference(blackSolid, whiteSolid);

                // Sonucu al
                if (resultSolids != null && resultSolids.Length > 0)
                {
                    resultCylinder = resultSolids[0];
                }
            }
            catch (Exception ex)
            {
                // Boolean işlemi başarısız olursa sadece siyah silindiri kullan
                System.Diagnostics.Debug.WriteLine($"Boolean işlemi hatası: {ex.Message}");
                resultCylinder = blackCylinder.ConvertToSolid();
            }

            // =========================================================
            // 🔸 4️⃣ SİLİNDİRİ KAYDIR (X-100, Z+L1+(L2/5))
            // =========================================================
            if (resultCylinder != null)
            {
                double offsetX = -TEMP_CONSTRUCTION_X;
                double offsetZ = shaftLength + bodyLength / Z_POSITION_DIVISOR;

                resultCylinder.Translate(offsetX, 0, offsetZ);

                // =========================================================
                // 🔸 5️⃣ PROBE MODELİNDEN ÇIKAR
                // =========================================================
                try
                {
                    // Probe bloğunu bul
                    Entity probeBlock = null;
                    foreach (var entity in design.Entities)
                    {
                        if (entity is BlockReference)
                        {
                            probeBlock = entity;
                            break;
                        }
                    }

                    if (probeBlock != null && probeBlock is BlockReference blockRef)
                    {
                        // Block içindeki tüm Solid'leri topla
                        var blockName = blockRef.BlockName;
                        var block = design.Blocks[blockName];

                        if (block != null)
                        {
                            // Block içindeki tüm mesh'leri solid'e çevir ve birleştir
                            Solid combinedProbe = null;

                            foreach (var ent in block.Entities)
                            {
                                if (ent is Mesh mesh)
                                {
                                    var solid = mesh.ConvertToSolid();
                                    if (combinedProbe == null)
                                        combinedProbe = solid;
                                    else
                                    {
                                        var unionResult = Solid.Union(combinedProbe, solid);
                                        if (unionResult != null && unionResult.Length > 0)
                                            combinedProbe = unionResult[0];
                                    }
                                }
                            }

                            // Probe'dan silindiri çıkar
                            if (combinedProbe != null)
                            {
                                var finalResult = Solid.Difference(combinedProbe, resultCylinder);

                                if (finalResult != null && finalResult.Length > 0)
                                {
                                    // Eski block'u temizle
                                    design.Entities.Clear();
                                    design.Blocks.Clear();

                                    // Yeni sonucu ekle
                                    string newBlockName = "ProbeBlock_" + Guid.NewGuid().ToString("N");
                                    var newBlock = new devDept.Eyeshot.Block(newBlockName);

                                    foreach (var solid in finalResult)
                                    {
                                        solid.ColorMethod = colorMethodType.byEntity;
                                        solid.Color = Color.Red;
                                        newBlock.Entities.Add(solid);
                                    }

                                    design.Blocks.Add(newBlock);

                                    var newBlockRef = new BlockReference(
                                        new Translation(0, 0, 0),
                                        newBlockName);

                                    design.Entities.Add(newBlockRef);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Probe'dan çıkarma hatası: {ex.Message}");

                    // Hata olursa en azından silindiri göster
                    if (!design.Layers.Contains(ProbeLayerNames.RefCylinder))
                        design.Layers.Add(new devDept.Eyeshot.Layer(ProbeLayerNames.RefCylinder, Color.DarkGray));

                    resultCylinder.ColorMethod = colorMethodType.byEntity;
                    resultCylinder.Color = Color.FromArgb(180, 60, 60, 60);
                    design.Entities.Add(resultCylinder, ProbeLayerNames.RefCylinder);
                }
            }

            // =========================================================
            // 🔸 6️⃣ Yenile
            // =========================================================
            design.Entities.Regen();
            design.Invalidate();
            design.Refresh();
        }
    }
}