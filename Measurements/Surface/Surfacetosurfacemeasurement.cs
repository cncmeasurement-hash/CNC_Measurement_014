using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Drawing;
using System.Windows.Forms;
using Plane = devDept.Geometry.Plane;
// Type aliases
using Point3D = devDept.Geometry.Point3D;
using Vector3D = devDept.Geometry.Vector3D;

namespace _014.Measurements.Surface
{
    /// <summary>
    /// SURFACE TO SURFACE MEASUREMENT
    /// ✅ FIXED: FaceSelectionHandler yöntemi kullanılıyor!
    /// entity.Selected ile seçim kontrolü (GetEntityUnderMouseCursor YOK!)
    /// </summary>
    public class SurfaceToSurfaceMeasurement
    {
        private Design design;
        private bool isEnabled = false;

        // Seçilen surface'ler
        private SurfaceInfo selectedSurface1 = null;
        private SurfaceInfo selectedSurface2 = null;

        // Son seçilen entity (duplicate önleme)
        private Entity lastSelectedEntity = null;

        // Renkler
        private Color surface1Color = Color.FromArgb(100, Color.Yellow);
        private Color surface2Color = Color.FromArgb(100, Color.Cyan);

        // ════════════════════════════════════════════════════════
        // SURFACE INFO CLASS
        // ════════════════════════════════════════════════════════
        private class SurfaceInfo
        {
            public Point3D Center { get; set; }
            public Vector3D Normal { get; set; }
            public string SurfaceType { get; set; }
            public Plane ReferencePlane { get; set; }
            public Entity Entity { get; set; }  // Referans için
        }

        // ════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ════════════════════════════════════════════════════════
        public SurfaceToSurfaceMeasurement(Design design)
        {
            this.design = design;
        }

        // ════════════════════════════════════════════════════════
        // ENABLE/DISABLE
        // ════════════════════════════════════════════════════════
        public void Enable()
        {
            if (isEnabled) return;

            isEnabled = true;

            // ✅ FaceSelectionHandler ile AYNI AYARLAR
            design.ActionMode = actionType.SelectVisibleByPick;
            design.SelectionFilterMode = selectionFilterType.Face;
            design.Cursor = Cursors.Hand;

            // ✅ MouseClick event bağla
            design.MouseClick += Design_MouseClick;

            lastSelectedEntity = null;

            System.Diagnostics.Debug.WriteLine("✅ Surface-to-Surface Measurement AKTIF!");
            System.Diagnostics.Debug.WriteLine("✅ Face Selection Mode AKTİF!");
            System.Diagnostics.Debug.WriteLine("📍 1. yüzeye tıklayın");
        }

        public void Disable()
        {
            if (!isEnabled) return;

            isEnabled = false;

            // ✅ FaceSelectionHandler ile AYNI KAPANIŞ
            design.ActionMode = actionType.None;
            design.SelectionFilterMode = selectionFilterType.Entity;
            design.Cursor = Cursors.Default;
            design.Entities.ClearSelection();
            design.Invalidate();

            // ✅ Event bağlantısını kes
            design.MouseClick -= Design_MouseClick;

            selectedSurface1 = null;
            selectedSurface2 = null;
            lastSelectedEntity = null;

            System.Diagnostics.Debug.WriteLine("⏸️ Surface-to-Surface Measurement KAPALI");
        }

        // ════════════════════════════════════════════════════════
        // ✅ MOUSE CLICK HANDLER - FaceSelectionHandler YÖNTEM İ!
        // ════════════════════════════════════════════════════════
        private void Design_MouseClick(object sender, MouseEventArgs e)
        {
            // ✅ FaceSelectionHandler satır 34-38
            if (!isEnabled || design.SelectionFilterMode != selectionFilterType.Face)
                return;

            if (e.Button != MouseButtons.Left)
                return;

            try
            {
                // ✅ FaceSelectionHandler satır 40-66: entity.Selected KULLAN!
                foreach (var entity in design.Entities)
                {
                    if (entity.Selected)
                    {
                        // ✅ Duplicate önleme - FaceSelectionHandler satır 55-56
                        if (entity == lastSelectedEntity)
                            continue;

                        lastSelectedEntity = entity;

                        // ✅ Surface bilgisini çıkar
                        SurfaceInfo surfInfo = ExtractSurfaceInfoFromEntity(entity);

                        if (surfInfo == null)
                        {
                            System.Diagnostics.Debug.WriteLine("⚠️ Surface bilgisi alınamadı!");
                            return;
                        }

                        // İlk veya ikinci surface
                        if (selectedSurface1 == null)
                        {
                            selectedSurface1 = surfInfo;
                            System.Diagnostics.Debug.WriteLine($"✅ 1. Surface seçildi: {surfInfo.SurfaceType}");
                            System.Diagnostics.Debug.WriteLine($"   Center: ({surfInfo.Center.X:F2}, {surfInfo.Center.Y:F2}, {surfInfo.Center.Z:F2})");
                            System.Diagnostics.Debug.WriteLine($"   Normal: ({surfInfo.Normal.X:F3}, {surfInfo.Normal.Y:F3}, {surfInfo.Normal.Z:F3})");
                            System.Diagnostics.Debug.WriteLine("📍 2. yüzeye tıklayın");
                        }
                        else
                        {
                            selectedSurface2 = surfInfo;
                            System.Diagnostics.Debug.WriteLine($"✅ 2. Surface seçildi: {surfInfo.SurfaceType}");
                            System.Diagnostics.Debug.WriteLine($"   Center: ({surfInfo.Center.X:F2}, {surfInfo.Center.Y:F2}, {surfInfo.Center.Z:F2})");
                            System.Diagnostics.Debug.WriteLine($"   Normal: ({surfInfo.Normal.X:F3}, {surfInfo.Normal.Y:F3}, {surfInfo.Normal.Z:F3})");

                            // Ölçüm yap
                            PerformMeasurement();

                            // Reset
                            selectedSurface1 = null;
                            selectedSurface2 = null;
                            lastSelectedEntity = null;
                            System.Diagnostics.Debug.WriteLine("📍 1. yüzeye tıklayın (yeni ölçüm)");
                        }

                        break; // ✅ FaceSelectionHandler satır 64
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ HATA: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
            }
        }

        // ════════════════════════════════════════════════════════
        // ✅ ENTITY'DEN SURFACE INFO ÇIKAR
        // ════════════════════════════════════════════════════════
        private SurfaceInfo ExtractSurfaceInfoFromEntity(Entity entity)
        {
            try
            {
                // ✅ BREP İŞLEME
                if (entity is Brep brep && brep.Faces != null && brep.Faces.Length > 0)
                {
                    // NOT: Şu an ilk face kullanılıyor (basit fallback)
                    // İleride: Tam hangi face seçildi bulunabilir
                    var face = brep.Faces[0];
                    return ExtractSurfaceInfoFromFace(face, entity);
                }

                System.Diagnostics.Debug.WriteLine($"⚠️ Desteklenmeyen tip: {entity.GetType().Name}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Entity extraction hatası: {ex.Message}");
                return null;
            }
        }

        // ════════════════════════════════════════════════════════
        // ✅ MESH KULLANARAK FACE BİLGİSİ ÇIKAR - SurfaceAnalyzer!
        // ════════════════════════════════════════════════════════
        private SurfaceInfo ExtractSurfaceInfoFromFace(Brep.Face face, Entity entity)
        {
            try
            {
                // ✅ SurfaceAnalyzer satır 60: Mesh'e çevir!
                Mesh faceMesh = face.ConvertToMesh();

                if (faceMesh == null || faceMesh.Vertices == null || faceMesh.Vertices.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Face mesh oluşturulamadı!");
                    return null;
                }

                // ✅ CENTER HESAPLA - SurfaceAnalyzer satır 67-76
                Point3D center = new Point3D(0, 0, 0);
                foreach (var v in faceMesh.Vertices)
                {
                    center.X += v.X;
                    center.Y += v.Y;
                    center.Z += v.Z;
                }
                center.X /= faceMesh.Vertices.Length;
                center.Y /= faceMesh.Vertices.Length;
                center.Z /= faceMesh.Vertices.Length;

                // ✅ NORMAL HESAPLA - SurfaceAnalyzer satır 78-88
                Vector3D normal = new Vector3D(0, 0, 1); // Varsayılan

                if (faceMesh.Triangles != null && faceMesh.Triangles.Length > 0)
                {
                    var tri = faceMesh.Triangles[0];
                    Point3D v0 = faceMesh.Vertices[tri.V1];
                    Point3D v1 = faceMesh.Vertices[tri.V2];
                    Point3D v2 = faceMesh.Vertices[tri.V3];

                    Vector3D edge1 = new Vector3D(v1.X - v0.X, v1.Y - v0.Y, v1.Z - v0.Z);
                    Vector3D edge2 = new Vector3D(v2.X - v0.X, v2.Y - v0.Y, v2.Z - v0.Z);
                    normal = Vector3D.Cross(edge1, edge2);
                    normal.Normalize();
                }

                // ✅ SURFACE TYPE BELİRLE - SurfaceAnalyzer satır 90
                string surfaceType = DetermineSurfaceType(normal);

                // SurfaceInfo oluştur
                SurfaceInfo info = new SurfaceInfo
                {
                    Center = center,
                    Normal = normal,
                    SurfaceType = surfaceType,
                    ReferencePlane = new Plane(center, normal),
                    Entity = entity
                };

                return info;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Face extraction hatası: {ex.Message}");
                return null;
            }
        }

        // ════════════════════════════════════════════════════════
        // ✅ SURFACE TYPE BELİRLE - SurfaceAnalyzer satır 236-254
        // ════════════════════════════════════════════════════════
        private string DetermineSurfaceType(Vector3D normal)
        {
            double threshold = 0.9;

            if (normal.Z > threshold)
                return "TOP (Z+)";
            if (normal.Z < -threshold)
                return "BOTTOM (Z-)";
            if (normal.X > threshold)
                return "RIGHT (X+)";
            if (normal.X < -threshold)
                return "LEFT (X-)";
            if (normal.Y > threshold)
                return "FRONT (Y+)";
            if (normal.Y < -threshold)
                return "BACK (Y-)";

            return "INCLINED";
        }

        // ════════════════════════════════════════════════════════
        // PERFORM MEASUREMENT
        // ════════════════════════════════════════════════════════
        private void PerformMeasurement()
        {
            if (selectedSurface1 == null || selectedSurface2 == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("📏 SURFACE ÖLÇÜMÜ BAŞLADI");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                // 1. CENTER TO CENTER DISTANCE
                double centerDistance = selectedSurface1.Center.DistanceTo(selectedSurface2.Center);

                // 2. PLANE TO PLANE DISTANCE
                double planeDistance = CalculatePlaneToPlaneDistance();

                // 3. AÇI
                double angle = AngleBetweenVectors(selectedSurface1.Normal, selectedSurface2.Normal);

                // 4. PARALELLİK
                bool isParallel = IsParallel(selectedSurface1.Normal, selectedSurface2.Normal);

                // SONUÇLAR
                System.Diagnostics.Debug.WriteLine($"📐 Merkez Mesafesi: {centerDistance:F3} mm");
                System.Diagnostics.Debug.WriteLine($"📐 Düzlem Mesafesi: {planeDistance:F3} mm");
                System.Diagnostics.Debug.WriteLine($"📐 Açı: {angle:F2}°");
                System.Diagnostics.Debug.WriteLine($"📐 Paralellik: {(isParallel ? "✅ PARALEL" : "❌ PARALEL DEĞİL")}");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                // MessageBox göster
                string message = $"📏 SURFACE TO SURFACE ÖLÇÜMÜ\n\n" +
                                 $"Surface 1: {selectedSurface1.SurfaceType}\n" +
                                 $"Surface 2: {selectedSurface2.SurfaceType}\n\n" +
                                 $"═══════════════════════════════════\n\n" +
                                 $"📐 Merkez Mesafesi: {centerDistance:F3} mm\n" +
                                 $"📐 Düzlem Mesafesi: {planeDistance:F3} mm\n" +
                                 $"📐 Açı: {angle:F2}°\n" +
                                 $"📐 Paralellik: {(isParallel ? "✅ PARALEL" : "❌ PARALEL DEĞİL")}";

                MessageBox.Show(message, "Surface Measurement Results",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ölçüm hatası: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════
        // UTILITY METHODS
        // ════════════════════════════════════════════════════════
        private double CalculatePlaneToPlaneDistance()
        {
            Vector3D toPoint = new Vector3D(
                selectedSurface1.Center.X - selectedSurface2.ReferencePlane.Origin.X,
                selectedSurface1.Center.Y - selectedSurface2.ReferencePlane.Origin.Y,
                selectedSurface1.Center.Z - selectedSurface2.ReferencePlane.Origin.Z
            );

            double distance = Math.Abs(DotProduct(toPoint, selectedSurface2.Normal));
            return distance;
        }

        private double AngleBetweenVectors(Vector3D v1, Vector3D v2)
        {
            double dot = DotProduct(v1, v2);
            double mag1 = Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);
            double mag2 = Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z);

            if (mag1 == 0 || mag2 == 0) return 0;

            double cosAngle = dot / (mag1 * mag2);
            cosAngle = Math.Max(-1, Math.Min(1, cosAngle));

            double angleRad = Math.Acos(cosAngle);
            double angleDeg = angleRad * (180.0 / Math.PI);

            return angleDeg;
        }

        private bool IsParallel(Vector3D v1, Vector3D v2)
        {
            double angle = AngleBetweenVectors(v1, v2);
            return angle < 1.0 || angle > 179.0;
        }

        private double DotProduct(Vector3D a, Vector3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }
    }
}