using System;
using System.Collections.Generic;
using System.Drawing;
using devDept.Geometry;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Control;
using _014.Probe.Visualization;

namespace _014.Probe.Core
{
    /// <summary>
    /// Eyeshot üzerinde parametrik prob modelleri oluşturmak için yardımcı sınıf.
    /// Küre, sap, konik geçiş ve gövde parçalarından oluşan prob modelleri üretir.
    /// </summary>
    internal class ProbeCalculator
    {
        /// <summary>
        /// Eyeshot sahnesine parametrik prob modeli ekler.
        /// Prob dört ana parçadan oluşur: Küre (uç), Sap (ince silindir), Konik geçiş ve Gövde (kalın silindir).
        /// </summary>
        /// <param name="design">Eyeshot Design kontrolü. Probun çizileceği sahne.</param>
        /// <param name="D">Küre çapı (mm). Prob ucunun boyutunu belirler. Varsayılan: 6mm</param>
        /// <param name="d1">Sap üst çapı (mm). Konik geçişin üst kısmının çapı. Varsayılan: 9mm</param>
        /// <param name="d2">Gövde çapı (mm). Prob gövdesinin çapı. Varsayılan: 58mm</param>
        /// <param name="L1">Sap uzunluğu (mm). Küre ile gövde arasındaki mesafe. Varsayılan: 55mm</param>
        /// <param name="L2">Gövde uzunluğu (mm). Ana gövde parçasının boyu. Varsayılan: 75mm</param>
        /// <param name="L3">Konik geçiş uzunluğu (mm). Sap ile gövde arasındaki geçiş bölgesi. Varsayılan: 10mm</param>
        /// <param name="insertPoint">Ekleme noktası. Probun sahneye ekleneceği koordinat. Null ise (0,0,0) kullanılır.</param>
        /// <param name="fitAll">Tüm sahneyi ekrana sığdır (şu an kullanılmıyor). Varsayılan: true</param>
        /// <remarks>
        /// <para><strong>ÖNEMLİ:</strong> Bu metod çağrıldığında mevcut sahnedeki TÜM objeler silinir.</para>
        /// <para>Tüm ölçüler milimetre (mm) cinsindendir.</para>
        /// <para>Prob parçaları tek bir Block olarak oluşturulur ve BlockReference ile sahneye eklenir.</para>
        /// <para>Tüm prob elemanları (küre, sap, konik, gövde) Layer_Probe layer'ında gruplandırılır.</para>
        /// <para>Sap çapı formülü: D / 1.85 / 2</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Default değerlerle prob oluştur
        /// ProbeCalculator.AddSphere(design1);
        /// 
        /// // Özel değerlerle prob oluştur
        /// ProbeCalculator.AddSphere(design1, D: 10, d1: 15, d2: 60, L1: 80, L2: 100, L3: 15);
        /// 
        /// // Belirli bir noktaya ekle
        /// var point = new Point3D(100, 100, 0);
        /// ProbeCalculator.AddSphere(design1, insertPoint: point);
        /// </code>
        /// </example>
        public static void AddSphere(Design design,
                                     double D = 6,      // 🔹 Küre çapı (Default: 6)
                                     double d1 = 9,     // 🔹 Sap çapı (Default: 9)
                                     double d2 = 58,    // 🔹 Gövde çapı (Default: 58)
                                     double L1 = 55,    // 🔹 Sap uzunluğu (Default: 55)
                                     double L2 = 75,    // 🔹 Gövde uzunluğu (Default: 75)
                                     double L3 = 10,    // 🔹 Konik geçiş boyu (Default: 10)
                                     Point3D insertPoint = null,
                                     bool fitAll = true)
        {
            // ==========================================================
            // 0️⃣ Başlatma
            // ==========================================================

            insertPoint ??= new Point3D(0, 0, 0);

            // 🔹 1️⃣ Form üzerindeki çizimleri geçici olarak dondur
            design.SuspendLayout();

            try
            {
                // ✅ 2️⃣ Mevcut nesneleri temizle
                design.Entities.Clear();
                design.Blocks.Clear();

                // 🔹 Formül değişkenleri (Form_New_Prob'dan gelen değerler)
                double sphereDiameter = D;
                double shaftDiameter = d1;
                double bodyDiameter = d2;
                double shaftLength = L1;
                double bodyLength = L2;
                double transitionLength = L3;

                double sphereRadius = sphereDiameter / 2.0; // Küre yarıçapı

                // ==========================================================
                // 1️⃣ Layer Kontrolü - Tek layer'da tüm prob parçaları
                // ==========================================================
                if (!design.Layers.Contains(ProbeLayerNames.Probe))
                {
                    design.Layers.Add(new devDept.Eyeshot.Layer(ProbeLayerNames.Probe, Color.LightSkyBlue));
                }

                // ==========================================================
                // 2️⃣ Küre (prob ucu)
                // ==========================================================
                var sphere = Mesh.CreateSphere(sphereRadius, 48, 48);
                sphere.Translate(insertPoint.X, insertPoint.Y, insertPoint.Z + sphereRadius);
                sphere.ColorMethod = colorMethodType.byEntity;
                sphere.Color = Color.Red;

                design.Entities.Add(sphere, ProbeLayerNames.Probe);

                // ==========================================================
                // 3️⃣ Sap (ince silindir)
                // ==========================================================
                if (shaftLength > 0 && sphereDiameter > 0)
                {
                    // 🔸 Sap yarıçapı: D / 1.85 / 2 (FORMÜL AYNEN KORUNDU)
                    double smallCylinderRadius = sphereDiameter / 1.85 / 2.0;

                    // 🔸 Sap boyu: L1 - (D / 2) (FORMÜL AYNEN KORUNDU)
                    double smallCylinderLength = shaftLength - sphereDiameter / 2;
                    if (smallCylinderLength <= 0) smallCylinderLength = sphereDiameter / 2; // min uzunluk

                    // 🔸 Silindir oluştur (Z yönünde)
                    var smallCylinder = Mesh.CreateCylinder(smallCylinderRadius, smallCylinderLength, 48);

                    // 🔸 Başlangıç: kürenin üstünden başlasın (Z + R)
                    smallCylinder.Translate(insertPoint.X, insertPoint.Y, insertPoint.Z + sphereRadius);

                    // 🔸 Görünüm ayarları
                    smallCylinder.ColorMethod = colorMethodType.byEntity;
                    smallCylinder.Color = Color.White;

                    // 🔸 Sahneye ekle - Aynı layer'da
                    design.Entities.Add(smallCylinder, ProbeLayerNames.Probe);
                }

                // ==========================================================
                // 4️⃣ GÖVDE (Body) - 3 PARÇALI: ÜST + YEŞİL ANIMASYON + ALT
                // ==========================================================
                if (bodyLength > 0 && bodyDiameter > 0)
                {
                    // 🔸 Gövde yarıçapı = d2 / 2 (FORMÜL AYNEN KORUNDU)
                    double bodyCylinderRadius = bodyDiameter / 2.0;

                    // 🔸 Yeşil bölge kalınlığı (gövdenin %15'i)
                    double greenRegionHeight = bodyLength * 0.15;

                    // 🔸 Üst gövde uzunluğu (gövdenin %40'ı)
                    double topBodyLength = bodyLength * 0.40;

                    // 🔸 Alt gövde uzunluğu (gövdenin %45'i)
                    double bottomBodyLength = bodyLength * 0.45;

                    // === ÜST GÖVDE (SteelBlue) ===
                    var topBody = Mesh.CreateCylinder(bodyCylinderRadius, topBodyLength, 64);
                    topBody.Translate(insertPoint.X, insertPoint.Y, insertPoint.Z + shaftLength);
                    topBody.ColorMethod = colorMethodType.byEntity;
                    topBody.Color = Color.SteelBlue;
                    design.Entities.Add(topBody, ProbeLayerNames.Probe);

                    // === YEŞİL ANIMASYON BÖLGESİ ===
                    var greenRegion = Mesh.CreateCylinder(bodyCylinderRadius, greenRegionHeight, 64);
                    greenRegion.Translate(insertPoint.X, insertPoint.Y, insertPoint.Z + shaftLength + topBodyLength);
                    greenRegion.ColorMethod = colorMethodType.byEntity;
                    greenRegion.Color = Color.Lime; // Parlak yeşil (0, 255, 0)
                    design.Entities.Add(greenRegion, ProbeLayerNames.Probe);

                    // === ALT GÖVDE (SteelBlue) ===
                    var bottomBody = Mesh.CreateCylinder(bodyCylinderRadius, bottomBodyLength, 64);
                    bottomBody.Translate(insertPoint.X, insertPoint.Y, insertPoint.Z + shaftLength + topBodyLength + greenRegionHeight);
                    bottomBody.ColorMethod = colorMethodType.byEntity;
                    bottomBody.Color = Color.SteelBlue;
                    design.Entities.Add(bottomBody, ProbeLayerNames.Probe);
                }


                // ==========================================================
                // 5️⃣ KONİK GEÇİŞ (L3)
                // ==========================================================
                if (transitionLength > 0 && sphereDiameter > 0 && shaftDiameter > 0)
                {
                    // 🔸 Koni, sapın hemen üstünden başlıyor.
                    // 🔸 Alt çap = D / 1.85 (FORMÜL AYNEN KORUNDU)
                    // 🔸 Üst çap = d1 (FORMÜL AYNEN KORUNDU)
                    // 🔸 Boy = L3 (FORMÜL AYNEN KORUNDU)

                    double coneBottomRadius = sphereDiameter / 1.85 / 2.0;
                    double coneTopRadius = shaftDiameter / 2.0;
                    double coneHeight = transitionLength;

                    // Koni oluştur
                    var cone = Mesh.CreateCone(coneBottomRadius, coneTopRadius, coneHeight, 64);

                    // Konumlandırma: Z = (L1 - L3) (FORMÜL AYNEN KORUNDU)
                    cone.Translate(insertPoint.X, insertPoint.Y, insertPoint.Z + (shaftLength - transitionLength));

                    // Görünüm ayarları - Konik bölge YEŞİL (animasyon için)
                    cone.ColorMethod = colorMethodType.byEntity;
                    cone.Color = Color.Lime; // Parlak yeşil (0, 255, 0)

                    // Katmanına ekle - Aynı layer'da
                    design.Entities.Add(cone, ProbeLayerNames.Probe);
                }

                // ==========================================================
                // 6️⃣ PARÇALARI TEK MODEL GİBİ DAVRANAN BLOKA TOPLA
                // ==========================================================
                var parts = new List<Entity>(design.Entities);   // küre, sap, koni, gövde
                if (parts.Count > 0)
                {
                    // 1) Benzersiz isimle block oluştur
                    string blockName = "ProbeBlock_" + Guid.NewGuid().ToString("N");
                    var block = new devDept.Eyeshot.Block(blockName);

                    // 2) Parçaları block içine taşı
                    foreach (var ent in parts)
                        block.Entities.Add(ent);

                    // 3) Sahneyi temizle ve block'u tanıt
                    design.Entities.Clear();
                    design.Blocks.Clear();
                    design.Blocks.Add(block);

                    // 4) Block'u insertPoint konumunda referansla sahneye ekle
                    var tr = new Translation(insertPoint.X, insertPoint.Y, insertPoint.Z);
                    var blockRef = new BlockReference(tr, blockName);
                    blockRef.ColorMethod = colorMethodType.byEntity;

                    design.Entities.Add(blockRef);
                }
            }
            finally
            {
                // 🔹 4️⃣ Dondurmayı kaldır ve sahneyi güncelle
                design.ResumeLayout();
                design.Refresh();
                design.Invalidate();
            }
        }
    }
}