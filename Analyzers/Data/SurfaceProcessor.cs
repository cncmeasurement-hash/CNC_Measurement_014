using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Eyeshot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using _014.Managers.Data;

namespace _014.Analyzers.Data
{
    /// <summary>
    /// SURFACE PROCESSOR (REFACTORED)
    /// ✅ Cylindrical analiz CylindricalAnalyzer.cs'ye taşındı
    /// ✅ Sadece Brep → Surface dönüşümü ve raporlama
    /// ✅ Otomatik eksen analizi kaldırıldı (manuel menü kontrolü)
    /// </summary>
    public class SurfaceProcessor
    {
        private Design design;
        private DataManager dataManager;
        private string importedFileName;

        public SurfaceProcessor(Design designControl, DataManager dataManager)
        {
            design = designControl;
            this.dataManager = dataManager;
            importedFileName = "";
        }

        /// <summary>
        /// Import edilen modeli işle
        /// ✅ SADECE Brep → Surface dönüşümü
        /// ❌ Artık otomatik eksen analizi YOK!
        /// </summary>
        public void ProcessImportedModel(string fileName)
        {
            importedFileName = Path.GetFileNameWithoutExtension(fileName);

            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine($"🚀 SURFACE PROCESSOR BAŞLADI: {importedFileName}");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

            try
            {
                var startTime = DateTime.Now;

                int toplamBrep = 0;
                int toplamSurface = 0;
                int basarili = 0;
                int basarisiz = 0;
                List<Entity> silinecekler = new List<Entity>();
                List<Surface> yeniSurfacelar = new List<Surface>();

                // Tip sayaçları
                int dikYuzeyCount = 0;
                int silindirikCount = 0;
                int konikCount = 0;
                int kureselCount = 0;
                int toroidalCount = 0;
                int nurbsCount = 0;
                int digerCount = 0;

                List<Entity> entities = new List<Entity>(design.Entities);

                foreach (Entity entity in entities)
                {
                    if (entity is Brep brep && brep.Faces != null)
                    {
                        toplamBrep++;
                        int faceSayisi = brep.Faces.Length;

                        System.Diagnostics.Debug.WriteLine($"🔧 Brep #{toplamBrep} bulundu");
                        System.Diagnostics.Debug.WriteLine($"   Face sayısı: {faceSayisi}");

                        try
                        {
                            Surface[] surfaces = brep.ConvertToSurfaces();

                            if (surfaces != null && surfaces.Length > 0)
                            {
                                basarili++;
                                toplamSurface += surfaces.Length;

                                System.Diagnostics.Debug.WriteLine($"   ✅ ConvertToSurfaces() BAŞARILI! {surfaces.Length} Surface");

                                // Surface'leri ekle ve tipleri say
                                foreach (Surface surface in surfaces)
                                {
                                    string tipAdi = surface.GetType().Name;

                                    // Tip sayaçlarını güncelle
                                    switch (tipAdi)
                                    {
                                        case "PlanarSurface":
                                            dikYuzeyCount++;
                                            break;
                                        case "CylindricalSurface":
                                            silindirikCount++;
                                            break;
                                        case "SphericalSurface":
                                            kureselCount++;
                                            break;
                                        case "ToroidalSurface":
                                            toroidalCount++;
                                            break;
                                        case "NurbsSurface":
                                        case "BSplineSurface":
                                            nurbsCount++;
                                            break;
                                        default:
                                            digerCount++;
                                            break;
                                    }

                                    surface.Color = Color.Tan;
                                    surface.ColorMethod = colorMethodType.byEntity;

                                    design.Entities.Add(surface);
                                    yeniSurfacelar.Add(surface);
                                }

                                // Brep'i sil
                                silinecekler.Add(brep);
                            }
                            else
                            {
                                basarisiz++;
                                System.Diagnostics.Debug.WriteLine($"   ❌ ConvertToSurfaces() BOŞ DÖNDÜ!");
                            }
                        }
                        catch (Exception ex)
                        {
                            basarisiz++;
                            System.Diagnostics.Debug.WriteLine($"   ❌ HATA: {ex.Message}");
                        }
                    }
                }

                // Brep'leri sil
                foreach (Entity entity in silinecekler)
                {
                    design.Entities.Remove(entity);
                }

                design.Entities.Regen();
                design.Invalidate();

                // Analysis layer oluştur (menüden analiz için hazır olsun)
                if (!design.Layers.Contains("Surface_Analysis"))
                {
                    Layer analysisLayer = new Layer("Surface_Analysis");
                    analysisLayer.LineWeight = 1;
                    analysisLayer.Color = Color.DarkOrange;
                    design.Layers.Add(analysisLayer);
                    System.Diagnostics.Debug.WriteLine("✅ 'Surface_Analysis' layer oluşturuldu!");
                }

                var duration = (DateTime.Now - startTime).TotalSeconds;

                // ✅ SADELEŞTIRILMIŞ RAPOR (eksen analizi yok!)
                ShowSummaryReport(toplamBrep, basarili, basarisiz, toplamSurface, silinecekler.Count, duration,
                    dikYuzeyCount, silindirikCount, konikCount, kureselCount, toroidalCount, nurbsCount, digerCount);

                System.Diagnostics.Debug.WriteLine("✅ SURFACE PROCESSOR TAMAMLANDI!");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ HATA: {ex.Message}");
                MessageBox.Show(
                    $"Surface işleme hatası!\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Özet rapor göster
        /// ✅ Sadece surface tipi sayıları
        /// ❌ Eksen analizi bilgisi yok (manuel yapılacak)
        /// </summary>
        private void ShowSummaryReport(int toplamBrep, int basarili, int basarisiz, int toplamSurface, int silinenBrep, double duration,
            int dikYuzey, int silindirik, int konik, int kuresel, int toroidal, int nurbs, int diger)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("📊 SURFACE PROCESSOR RAPORU");
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"📂 Dosya: {importedFileName}");
            sb.AppendLine($"⏱️ Süre: {duration:F2} saniye");
            sb.AppendLine();
            sb.AppendLine("─── BREP → SURFACE DÖNÜŞÜMÜ ───");
            sb.AppendLine($"🔧 Toplam Brep: {toplamBrep}");
            sb.AppendLine($"✅ Başarılı: {basarili}");
            sb.AppendLine($"❌ Başarısız: {basarisiz}");
            sb.AppendLine($"✨ Oluşturulan Surface: {toplamSurface}");
            sb.AppendLine($"🗑️ Silinen Brep: {silinenBrep}");
            sb.AppendLine();
            sb.AppendLine("─── SURFACE TİPLERİ ───");
            sb.AppendLine($"📐 Düzlemsel (Planar): {dikYuzey}");
            sb.AppendLine($"🔵 Silindirik: {silindirik}");
            sb.AppendLine($"🔶 Konik: {konik}");
            sb.AppendLine($"⚪ Küresel: {kuresel}");
            sb.AppendLine($"🍩 Toroidal: {toroidal}");
            sb.AppendLine($"🌀 NURBS/Freeform: {nurbs}");
            sb.AppendLine($"❓ Diğer: {diger}");
            sb.AppendLine();
            sb.AppendLine("💡 NOT: Eksen analizi için Measurement menüsünü kullanın:");
            sb.AppendLine("   • Hole axis → Delik eksenler");
            sb.AppendLine("   • Boss axis → Çıkıntı eksenler");
            sb.AppendLine("═══════════════════════════════════════");

            string raporMetni = sb.ToString();
            System.Diagnostics.Debug.WriteLine(raporMetni);

        }
    }
}