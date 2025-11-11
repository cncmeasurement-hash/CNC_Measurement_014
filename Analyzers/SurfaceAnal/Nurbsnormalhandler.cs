using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace _014.Analyzers.SurfaceAnal
{
    /// <summary>
    /// ✅ NURBS NORMAL HANDLER (DÜZELTİLMİŞ)
    /// FindClosestTriangle kullanarak mouse click'i handle eder
    /// 16 numaralı projeden uyarlandı
    /// </summary>
    public class NurbsNormalHandler
    {
        private Design design;
        private bool isEnabled = false;

        public NurbsNormalHandler(Design designControl)
        {
            design = designControl;

            // Mouse click event'ini bağla
            design.MouseClick += Design_MouseClick;
        }

        /// <summary>
        /// Modu aktif/pasif et
        /// </summary>
        public void Enable(bool enable)
        {
            isEnabled = enable;

            if (enable)
            {
                // ✅ Seçim modunu aktif et
                design.ActionMode = actionType.SelectVisibleByPick;

                // ✅ NOT: GetEntityUnderMouseCursor(e.Location, true) zaten 
                //    sadece görünen entity'leri buluyor, ek filtreye gerek yok

                design.Cursor = Cursors.Hand;

                System.Diagnostics.Debug.WriteLine("🟡 NURBS Normal Handler AKTİF");
                System.Diagnostics.Debug.WriteLine("   ✅ Z- normalli yüzeyler filtrelenecek");
            }
            else
            {
                // ⛔ Modu kapat
                design.ActionMode = actionType.None;
                design.Cursor = Cursors.Default;
                design.Entities.ClearSelection();
                design.Invalidate();

                System.Diagnostics.Debug.WriteLine("⛔ NURBS Normal Handler PASİF");
            }
        }

        /// <summary>
        /// ✅ Mouse click event handler
        /// 16 numaralı projeden uyarlandı - GetEntityUnderMouseCursor kullanıyor
        /// </summary>
        private void Design_MouseClick(object sender, MouseEventArgs e)
        {
            // Mod aktif değilse çık
            if (!isEnabled)
                return;

            // Sol tık değilse çık
            if (e.Button != MouseButtons.Left)
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🖱️ NURBS yüzeye tıklandı!");

                // ✅ Mouse altındaki entity'yi al (16 projesinden - TRUE parametresi ile)
                int entityIndex = design.GetEntityUnderMouseCursor(e.Location, true);

                if (entityIndex == -1)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Mouse altında entity yok");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }

                Entity entity = design.Entities[entityIndex];

                System.Diagnostics.Debug.WriteLine($"📦 Entity bulundu: {entity.GetType().Name} (Index: {entityIndex})");

                // NURBS mi kontrol et
                if (!NurbsSurfaceAnalyzer.IsNurbsOrFreeformSurface(entity))
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Bu NURBS yüzey değil, atlanıyor...");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("✅ NURBS/Freeform yüzey tespit edildi!");

                // ✅ Entity IFace mi kontrol et
                if (!(entity is IFace faceEntity))
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Entity IFace değil");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }

                // ✅ Eyeshot'ın FindClosestTriangle metodu
                Point3D clickedPoint;
                int triangleIndex;

                double distance = design.FindClosestTriangle(
                    faceEntity,
                    e.Location,
                    out clickedPoint,
                    out triangleIndex
                );

                // ✅ Distance kontrolü daha gevşek (> 0 yerine >= 0)
                if (distance >= 0 && triangleIndex >= 0 && clickedPoint != null)
                {
                    System.Diagnostics.Debug.WriteLine($"📍 Tıklanan nokta: ({clickedPoint.X:F2}, {clickedPoint.Y:F2}, {clickedPoint.Z:F2})");
                    System.Diagnostics.Debug.WriteLine($"🔺 Triangle index: {triangleIndex}");
                    System.Diagnostics.Debug.WriteLine($"📏 Distance: {distance:F2}");

                    // Normal hesapla ve marker ekle
                    ProcessNurbsClick(entity, clickedPoint, triangleIndex);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ FindClosestTriangle başarısız");
                    System.Diagnostics.Debug.WriteLine($"   Distance: {distance:F2}");
                    System.Diagnostics.Debug.WriteLine($"   Triangle index: {triangleIndex}");
                }

                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Mouse click hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// ✅ NURBS yüzeye tıklandı - marker ve normal ekle
        /// </summary>
        private void ProcessNurbsClick(Entity entity, Point3D clickedPoint, int triangleIndex)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎯 Normal hesaplanıyor...");

                // Mesh'e çevir
                Mesh mesh = null;

                if (entity is Surface surface)
                {
                    // ✅ Daha yoğun mesh için düşük tolerance
                    mesh = surface.ConvertToMesh(); // Varsayılan yerine daha yoğun
                }
                else if (entity is Mesh m)
                {
                    mesh = m;
                }

                if (mesh == null || mesh.Triangles == null || mesh.Triangles.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Mesh oluşturulamadı");
                    return;
                }

                // Triangle index kontrolü
                if (triangleIndex < 0 || triangleIndex >= mesh.Triangles.Length)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Geçersiz triangle index: {triangleIndex}");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Triangle index: {triangleIndex}");

                // Triangle bilgilerini al
                var faceInfo = NurbsSurfaceAnalyzer.GetTriangleFaceInfo(
                    mesh,
                    triangleIndex,
                    clickedPoint
                );

                if (faceInfo == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Face info hesaplanamadı");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Normal: ({faceInfo.Normal.X:F3}, {faceInfo.Normal.Y:F3}, {faceInfo.Normal.Z:F3})");

                // ═══════════════════════════════════════════════════════════
                // ✅ Z- FİLTRESİ: Normal.Z < -0.001 ise (açıkça aşağı bakıyorsa) REDDET
                // ✅ -0.001 ile +0.001 arası = YAN YÜZEY (kabul et)
                // ═══════════════════════════════════════════════════════════
                const double EPSILON = 0.001; // Tolerans

                if (faceInfo.Normal.Z < -EPSILON)
                {
                    System.Diagnostics.Debug.WriteLine("⛔ ALT YÜZEY ALGILANDI!");
                    System.Diagnostics.Debug.WriteLine($"   Normal.Z = {faceInfo.Normal.Z:F3} < -{EPSILON}");
                    System.Diagnostics.Debug.WriteLine("   ❌ Bu yüzey aşağı bakıyor, atlanıyor...");
                    return; // ✅ İşlemi durdur, marker ekleme
                }

                if (Math.Abs(faceInfo.Normal.Z) <= EPSILON)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ YAN YÜZEY (Normal.Z ≈ 0: {faceInfo.Normal.Z:F3})");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✅ ÜST YÜZEY (Normal.Z = {faceInfo.Normal.Z:F3} > 0)");
                }

                // Marker ve normal çizgisini oluştur
                var entities = NurbsSurfaceAnalyzer.CreateMarkerAndNormalLine(
                    clickedPoint,
                    faceInfo.Normal,
                    lineLength: 30.0,
                    markerSize: 3.0
                );

                // Sahneye ekle
                foreach (var ent in entities)
                {
                    design.Entities.Add(ent);
                }

                design.Entities.Regen();
                design.Invalidate();

                System.Diagnostics.Debug.WriteLine($"✅ {entities.Count} entity eklendi (marker + 10mm normal + 100mm Z)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ProcessNurbsClick hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }
        }
    }
}