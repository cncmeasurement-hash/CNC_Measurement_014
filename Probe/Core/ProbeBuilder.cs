using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;

namespace _014.Probe.Core
{
    /// <summary>
    /// ✅ PROBE BUILDER - STATIC CLASS
    /// 
    /// GÖREV:
    /// TreeViewManager'dan alınan ProbeData ile probe mesh'i oluşturur.
    /// Form_New_Prob.ProbeBuilder.cs'deki DOĞRU formülleri kullanır.
    /// 
    /// KULLANIM:
    /// var probeData = treeViewManager.GetSelectedProbeData();
    /// Mesh probeMesh = ProbeBuilder.CreateProbeMesh(probeData);
    /// 
    /// FORMÜLLER (Form_New_Prob'dan):
    /// - sapRadius = (D / 1.85) / 2.0
    /// - sapLength = L1 - R
    /// - altYaricap = sapRadius
    /// - Konik: Translate(0, 0, L1 - L3)
    /// - Gövde: Translate(0, 0, L1)
    /// </summary>
    public static class ProbeBuilder
    {
        /// <summary>
        /// ✅ Probe mesh'ini oluştur (Top + Sap + Konik + Gövde)
        /// Form_New_Prob.ProbeBuilder.cs'deki DOĞRU formülleri kullanır
        /// </summary>
        public static Mesh CreateProbeMesh(ProbeData probeData)
        {
            try
            {
                // Parametreleri double'a çevir
                double D = (double)probeData.D;
                double d1 = (double)probeData.d1;
                double d2 = (double)probeData.d2;
                double L1 = (double)probeData.L1;
                double L2 = (double)probeData.L2;
                double L3 = (double)probeData.L3;

                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🔧 PROBE MESH OLUŞTURULUYOR (ProbeBuilder)...");
                System.Diagnostics.Debug.WriteLine($"   Parametreler: D={D}, d1={d1}, d2={d2}, L1={L1}, L2={L2}, L3={L3}");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                // 1️⃣ KÜRE
                var sphere = CreateSphereMesh(D);
                if (sphere == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Küre oluşturulamadı!");
                    return null;
                }

                // 2️⃣ SAP
                var shaft = CreateShaftMesh(D, L1);
                if (shaft == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Sap oluşturulamadı!");
                    return null;
                }

                // 3️⃣ KONİK
                var cone = CreateConeMesh(D, d1, L1, L3);
                if (cone == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Konik oluşturulamadı!");
                    return null;
                }

                // 4️⃣ GÖVDE
                var body = CreateBodyMesh(d2, L1, L2);
                if (body == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Gövde oluşturulamadı!");
                    return null;
                }

                // 5️⃣ HEPSİNİ BİRLEŞTİR
                sphere.MergeWith(shaft);
                sphere.MergeWith(cone);
                sphere.MergeWith(body);

                System.Diagnostics.Debug.WriteLine("✅ Probe mesh tamamlandı! (ProbeBuilder)");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                return sphere;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ProbeBuilder hatası: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 🔴 Küre mesh'i oluştur
        /// Form_New_Prob formülü: sphere.Translate(0, 0, R)
        /// </summary>
        private static Mesh CreateSphereMesh(double D)
        {
            if (D <= 0)
                return null;

            double R = D / 2.0;
            var sphere = Mesh.CreateSphere(R, 40, 40);  // ✅ YÜKSEK KALİTE: 40x40 (smooth surface)
            sphere.Translate(0, 0, R); // ✅ Form_New_Prob formülü
            return sphere;
        }

        /// <summary>
        /// ⚪ Sap mesh'i oluştur
        /// Form_New_Prob formülleri:
        /// - sapRadius = (D / 1.85) / 2.0
        /// - sapLength = L1 - R
        /// - shaft.Translate(0, 0, R)
        /// </summary>
        private static Mesh CreateShaftMesh(double D, double L1)
        {
            if (L1 <= 0 || D <= 0)
                return null;

            double R = D / 2.0;
            double sapRadius = D / 1.85 / 2.0; // ✅ Form_New_Prob formülü
            double sapLength = L1 - R;           // ✅ Form_New_Prob formülü

            if (sapLength <= 0)
                return null;

            var shaft = Mesh.CreateCylinder(sapRadius, sapLength, 8);  // ✅ ÇOK HAFİF: 8 kenar
            shaft.Translate(0, 0, R); // ✅ Form_New_Prob formülü
            return shaft;
        }

        /// <summary>
        /// ⚪ Konik mesh'i oluştur
        /// Form_New_Prob formülleri:
        /// - altYaricap = (D / 1.85) / 2.0 (sapRadius ile aynı)
        /// - ustYaricap = d1 / 2.0
        /// - cone.Translate(0, 0, L1 - L3)
        /// </summary>
        private static Mesh CreateConeMesh(double D, double d1, double L1, double L3)
        {
            if (L3 <= 0 || D <= 0 || d1 <= 0)
                return null;

            double altYaricap = D / 1.85 / 2.0; // ✅ Form_New_Prob formülü (sapRadius)
            double ustYaricap = d1 / 2.0;

            var cone = Mesh.CreateCone(altYaricap, ustYaricap, L3, 8);  // ✅ ÇOK HAFİF: 8 kenar
            cone.Translate(0, 0, L1 - L3); // ✅ Form_New_Prob formülü
            return cone;
        }

        /// <summary>
        /// 🟡 Gövde mesh'i oluştur
        /// Form_New_Prob formülü:
        /// - body.Translate(0, 0, L1)
        /// </summary>
        private static Mesh CreateBodyMesh(double d2, double L1, double L2)
        {
            if (L2 <= 0 || d2 <= 0)
                return null;

            double bodyRadius = d2 / 2.0;

            var body = Mesh.CreateCylinder(bodyRadius, L2, 16);  // ✅ ÇOK HAFİF: 16 kenar
            body.Translate(0, 0, L1); // ✅ Form_New_Prob formülü
            return body;
        }
    }
}
