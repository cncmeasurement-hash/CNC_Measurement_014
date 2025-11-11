using _014.Probe.Visualization;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Graphics;
using System;
using System.Drawing;

namespace _014
{
    /// <summary>
    /// Form_New_Prob - PROBE BUILDER
    /// Partial class 2/4: Probe oluşturma metodları (küre, sap, konik, gövde, delik)
    /// </summary>
    public partial class Form_New_Prob
    {
        // ═══════════════════════════════════════════════════════════
        // ANA PROBE OLUŞTURMA METODU
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// TEK SEFERDE: Probe oluştur + Silindir çıkar + Renklendir + Ekrana çiz
        /// </summary>
        private void UpdateProbeWithHole()
        {
            double D = (double)numeric_new_probe_D.Value;
            double d1 = (double)numeric_new_probe_d1.Value;
            double d2 = (double)numeric_new_probe_d2.Value;
            double L1 = (double)numeric_new_probe_L1.Value;
            double L2 = (double)numeric_new_probe_L2.Value;
            double L3 = (double)numeric_new_probe_L3.Value;

            design_new_probe.SuspendLayout();

            try
            {
                design_new_probe.Entities.Clear();
                design_new_probe.Blocks.Clear();

                // 1️⃣ PROBE PARÇALARINI AYRI AYRI OLUŞTUR
                var sphere = CreateSphereSolid(D);
                var shaft = CreateShaftSolid(D, L1);
                var cone = CreateConeSolid(D, d1, L1, L3);
                var body = CreateBodySolid(d2, L1, L2);

                // 2️⃣ SİLİNDİR OLUŞTUR (d2-1)
                var holeCylinder = CreateHoleCylinder(d2, L1, L2);

                // 3️⃣ GÖVDEYE KÖŞE KIRMA UYGULA (basitleştirilmiş)
                if (body != null)
                {
                    body = ApplyFilletToBody(body, d2);
                }

                // 3.5️⃣ GÖVDEYE TEXT OYMA EKLE (delik açılmadan önce)
                if (body != null)
                {
                    body = AddTextEngraving(body, d2, L1, L2);
                }

                // 4️⃣ GÖVDEDEN SİLİNDİRİ ÇIKAR
                Solid bodyWithHole = null;
                if (body != null && holeCylinder != null)
                {
                    var result = Solid.Difference(body, holeCylinder);
                    if (result != null && result.Length > 0)
                        bodyWithHole = result[0];
                }

                // 5️⃣ BLOCK OLUŞTUR VE RENKLENDIR
                string blockName = "ProbeBlock_" + Guid.NewGuid().ToString("N");
                var block = new devDept.Eyeshot.Block(blockName);

                // 🔴 Küre - Kırmızı
                if (sphere != null)
                {
                    sphere.ColorMethod = colorMethodType.byEntity;
                    sphere.Color = Color.Red;
                    block.Entities.Add(sphere);
                }

                // ⚪ Sap - Beyaz
                if (shaft != null)
                {
                    shaft.ColorMethod = colorMethodType.byEntity;
                    shaft.Color = Color.White;
                    block.Entities.Add(shaft);
                }

                // ⚪ Konik geçiş - Beyaz
                if (cone != null)
                {
                    cone.ColorMethod = colorMethodType.byEntity;
                    cone.Color = Color.White;
                    block.Entities.Add(cone);
                }

                // 🟡 Gövde (delikli) - ÇOK PARLAK ALTIN SARISI
                if (bodyWithHole != null)
                {
                    bodyWithHole.ColorMethod = colorMethodType.byEntity;
                    bodyWithHole.Color = Color.FromArgb(255, 255, 215, 0); // Parlak altın (Gold)
                    block.Entities.Add(bodyWithHole);
                }

                // 🟢 DELİK İÇİNİ YEŞİLE BOYA
                if (holeCylinder != null)
                {
                    holeCylinder.ColorMethod = colorMethodType.byEntity;
                    holeCylinder.Color = Color.Lime;
                    block.Entities.Add(holeCylinder);
                }

                // 6️⃣ BLOCK'U SAHNEYE EKLE
                design_new_probe.Blocks.Add(block);

                var blockRef = new BlockReference(
                    new Translation(0, 0, 0),
                    blockName);

                design_new_probe.Entities.Add(blockRef);

                // 7️⃣ YÜKSEK KALİTE RENDER AYARLARI (Rendering.cs'de de var)
                design_new_probe.Rendered.ShadowMode = devDept.Graphics.shadowType.Realistic;
                design_new_probe.Rendered.ShowEdges = true;
                design_new_probe.Rendered.EdgeThickness = 0.1f;

                design_new_probe.Background.TopColor = Color.FromArgb(240, 248, 255);
                design_new_probe.Background.BottomColor = Color.FromArgb(200, 220, 240);
                design_new_probe.Background.StyleMode = devDept.Graphics.backgroundStyleType.LinearGradient;

                // ✅ 8️⃣ LOGO VE CUSTOM TEXT EKLE (TextLogo.cs'de detaylı metod var)
                try
                {
                    double cylinderRadius = d2 / 2.0;
                    double L1_val = Convert.ToDouble(numeric_new_probe_L1.Value);
                    double L2_val = Convert.ToDouble(numeric_new_probe_L2.Value);

                    System.Diagnostics.Debug.WriteLine($"🎨 Logo ekleniyor: R={cylinderRadius:F2}, L1={L1_val:F2}, L2={L2_val:F2}");

                    ProbeLogoManager.AddLogoAndText(
                        design_new_probe,
                        cylinderRadius,
                        L1_val,
                        L2_val
                    );

                    System.Diagnostics.Debug.WriteLine("✅ Logo ekleme tamamlandı!");
                }
                catch (Exception logoEx)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Logo ekleme hatası: {logoEx.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hata: {ex.Message}");
            }
            finally
            {
                design_new_probe.ResumeLayout();
                design_new_probe.Entities.Regen();
                design_new_probe.Invalidate();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PROBE PARÇALARI OLUŞTURMA METODLARı
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// 🔴 Küre oluştur - Yüksek kalite mesh
        /// </summary>
        private Solid CreateSphereSolid(double D)
        {
            double R = D / 2.0;
            var sphere = Mesh.CreateSphere(R, 64, 64); // 64 segment (yüksek kalite)
            sphere.Translate(0, 0, R);
            return sphere.ConvertToSolid();
        }

        /// <summary>
        /// ⚪ Sap oluştur - Yüksek kalite mesh
        /// </summary>
        private Solid CreateShaftSolid(double D, double L1)
        {
            if (L1 <= 0 || D <= 0)
                return null;

            double R = D / 2.0;
            double sapRadius = (D / 1.85) / 2.0;
            double sapLength = L1 - (D / 2);

            if (sapLength <= 0)
                return null;

            var shaft = Mesh.CreateCylinder(sapRadius, sapLength, 64); // 64 segment
            shaft.Translate(0, 0, R);
            return shaft.ConvertToSolid();
        }

        /// <summary>
        /// ⚪ Konik geçiş oluştur
        /// </summary>
        private Solid CreateConeSolid(double D, double d1, double L1, double L3)
        {
            if (L3 <= 0 || D <= 0 || d1 <= 0)
                return null;

            double altYaricap = (D / 1.85) / 2.0;
            double ustYaricap = d1 / 2.0;

            var cone = Mesh.CreateCone(altYaricap, ustYaricap, L3, 64);
            cone.Translate(0, 0, L1 - L3);
            return cone.ConvertToSolid();
        }

        /// <summary>
        /// 🔷 Gövde oluştur - Yüksek kaliteli mesh
        /// </summary>
        private Solid CreateBodySolid(double d2, double L1, double L2)
        {
            if (L2 <= 0 || d2 <= 0)
                return null;

            double bodyRadius = (d2 / 2.0) - 0.1; // 0.1mm küçült (text oyma efekti için)

            System.Diagnostics.Debug.WriteLine($"🎯 Gövde yarıçapı: {bodyRadius:F2}mm (0.1mm küçültüldü - text oyma efekti)");

            // Yüksek kaliteli mesh (128 segment - daha pürüzsüz)
            var body = Mesh.CreateCylinder(bodyRadius, L2, 128);
            body.Translate(0, 0, L1);

            return body.ConvertToSolid();
        }

        /// <summary>
        /// 🟢 Delik silindiri oluştur (d2-1) - Yüksek kaliteli mesh
        /// </summary>
        private Solid CreateHoleCylinder(double d2, double L1, double L2)
        {
            double radius1 = d2 / 2.0;
            double height1 = L2 / 12;

            var blackCylinder = Mesh.CreateCylinder(radius1, height1, 128); // 128 segment

            double radius2 = (d2 - 1) / 2.0;
            double height2 = L1 + L2;

            var whiteCylinder = Mesh.CreateCylinder(radius2, height2, 128); // 128 segment

            var blackSolid = blackCylinder.ConvertToSolid();
            var whiteSolid = whiteCylinder.ConvertToSolid();

            var result = Solid.Difference(blackSolid, whiteSolid);

            if (result != null && result.Length > 0)
            {
                double offsetZ = L1 + (L2 / 5.0);
                result[0].Translate(0, 0, offsetZ);
                return result[0];
            }

            return null;
        }

        /// <summary>
        /// 🔧 Gövdeye köşe kırma (fillet) uygula - BASİTLEŞTİRİLMİŞ
        /// ✅ Edges ve FilletEdges metodları kaldırıldı (Eyeshot 2025'te çalışmıyor)
        /// </summary>
        private Solid ApplyFilletToBody(Solid body, double d2)
        {
            // ℹ️ Not: Eyeshot 2025.3.457'de Solid.Edges ve FilletEdges metodları 
            // farklı çalışıyor veya deprecated. Bu yüzden köşe kırma devre dışı.
            // Eğer fillet gerekiyorsa, Eyeshot dokümantasyonundan 
            // yeni API'yi kontrol et veya mesh üzerinde manuel işlem yap.

            return body; // Orijinal gövdeyi döndür
        }
    }
}