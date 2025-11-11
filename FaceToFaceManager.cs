using _014.Handlers.FaceToFace;
using _014.Managers.Data;
using _014.Utilities.UI;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace _014
{
    /// <summary>
    /// FACE TO FACE MEASUREMENT MANAGER
    /// ✅ Form1.cs'den ayrıldı
    /// ✅ İki yüzey arası mesafe ölçümü
    /// ✅ Silindirik yüzey çap hesaplama
    /// ✅ Alan hesaplama
    /// </summary>
    public class FaceToFaceManager
    {
        // ═══════════════════════════════════════════════════════════
        // PRIVATE FIELDS
        // ═══════════════════════════════════════════════════════════

        private readonly Design design;
        private readonly DataManager? dataManager;
        private readonly Form parentForm;
        private const string MEASUREMENT_LAYER_NAME = "MeasurementLines";

        // İlk yüzey verileri
        private Entity? face1 = null;
        private Color originalColor1;
        private double area1 = 0;
        private bool isCylindrical1 = false;
        private double diameter1 = 0;
        private Point3D center1 = Point3D.Origin;
        private Point3D topCenter1 = Point3D.Origin;
        private Point3D bottomCenter1 = Point3D.Origin;

        // İkinci yüzey verileri
        private Entity? face2 = null;
        private Color originalColor2;
        private double area2 = 0;
        private bool isCylindrical2 = false;
        private double diameter2 = 0;
        private Point3D center2 = Point3D.Origin;
        private Point3D topCenter2 = Point3D.Origin;
        private Point3D bottomCenter2 = Point3D.Origin;

        // Ölçüm verileri
        private int selectionCount = 0;
        private double distance = 0;
        private Line? distanceLine = null;

        // UI Panelleri
        private FaceToFaceInfoPanel? infoPanel;
        private InstructionPanel? instructionPanel;

        // Mod durumu
        private bool isActive = false;

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        public FaceToFaceManager(Design designControl, Form parentForm, DataManager? dataManager = null)
        {
            design = designControl ?? throw new ArgumentNullException(nameof(designControl));
            this.parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
            this.dataManager = dataManager;
        }

        // ═══════════════════════════════════════════════════════════
        // PUBLIC PROPERTIES
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Face to Face modu aktif mi?
        /// </summary>
        public bool IsActive => isActive;

        // ═══════════════════════════════════════════════════════════
        // PUBLIC METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Face to Face modunu aktif et
        /// </summary>
        public void Enable(InstructionPanel? instructionPanel)
        {
            if (isActive)
                return;

            isActive = true;
            this.instructionPanel = instructionPanel;

            // ✅ Mouse click event'ini dinle
            design.MouseClick += OnMouseClick;

            // ✅ Cursor'u değiştir
            design.Cursor = Cursors.Hand;

            // ✅ InstructionPanel'i aç
            if (instructionPanel != null && !instructionPanel.IsDisposed)
            {
                instructionPanel.UpdatePanel(
                    InstructionTexts.TITLE_FACE_TO_FACE,
                    InstructionTexts.FACE_TO_FACE
                );
                instructionPanel.Show();
                instructionPanel.BringToFront();
            }

            // ✅ FaceToFaceInfoPanel'i oluştur ve göster (sağ üst)
            if (infoPanel == null || infoPanel.IsDisposed)
            {
                infoPanel = new FaceToFaceInfoPanel(parentForm);
            }
            infoPanel.ShowWaitingMessage();
            infoPanel.Show();

            Debug.WriteLine("✅ Face to Face AKTIF - İlk yüzey seçimi bekleniyor!");
        }

        /// <summary>
        /// Face to Face modunu pasif et
        /// </summary>
        public void Disable()
        {
            if (!isActive)
                return;

            isActive = false;

            // ✅ Mouse event'i kaldır
            design.MouseClick -= OnMouseClick;

            // ✅ Cursor'u normale döndür
            design.Cursor = Cursors.Default;

            // ✅ Face to Face seçimlerini temizle
            ResetSelection();

            // ✅ Genel seçimleri temizle
            design.Entities.ClearSelection();
            design.Invalidate();

            // ✅ Panel'i welcome mesajına döndür (gizleme!)
            if (instructionPanel != null && !instructionPanel.IsDisposed)
            {
                instructionPanel.UpdatePanel(
                    InstructionTexts.TITLE_MAIN_MENU,
                    InstructionTexts.WELCOME
                );
            }

            // ✅ FaceToFaceInfoPanel'i kapat
            if (infoPanel != null && !infoPanel.IsDisposed)
            {
                infoPanel.Close();
                infoPanel = null;
            }

            Debug.WriteLine("✅ Face to Face PASİF");
        }

        /// <summary>
        /// Seçimleri temizle ve sıfırla
        /// </summary>
        public void ResetSelection()
        {
            Debug.WriteLine("🔄 Face to Face - Seçimler temizleniyor...");

            // ✅ İlk yüzeyi eski rengine döndür
            if (face1 != null)
            {
                face1.Color = originalColor1;
                face1.ColorMethod = colorMethodType.byEntity;
                face1 = null;
            }

            // ✅ İkinci yüzeyi eski rengine döndür
            if (face2 != null)
            {
                face2.Color = originalColor2;
                face2.ColorMethod = colorMethodType.byEntity;
                face2 = null;
            }

            // ✅ Mesafe çizgisini sil
            if (distanceLine != null)
            {
                design.Entities.Remove(distanceLine);
                distanceLine = null;
            }

            // ✅ Değişkenleri sıfırla
            selectionCount = 0;
            area1 = 0;
            area2 = 0;
            distance = 0;
            isCylindrical1 = false;
            isCylindrical2 = false;
            diameter1 = 0;
            diameter2 = 0;

            design.Invalidate();

            Debug.WriteLine("✅ Face to Face - Seçimler temizlendi!");
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE METHODS - EVENT HANDLERS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mouse tıklama event handler
        /// Form1.cs'deki FaceToFace_MouseClick metodundan taşındı
        /// </summary>
        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (!isActive)
                return;

            if (e.Button != MouseButtons.Left)
                return;

            try
            {
                Debug.WriteLine("═══════════════════════════════════════");
                Debug.WriteLine($"🖱️ Mouse tıklaması! Seçim: {selectionCount + 1}/2");

                // ✅ Fare altındaki entity'yi BUL
                int entityIndex = design.GetEntityUnderMouseCursor(e.Location, true);

                if (entityIndex == -1)
                {
                    Debug.WriteLine("⚠️ Fare altında hiçbir şey yok");
                    Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }

                Debug.WriteLine($"✅ Entity bulundu! Index: {entityIndex}");

                // ✅ Entity'yi AL
                Entity entity = design.Entities[entityIndex];

                Debug.WriteLine($"📦 Entity tipi: {entity.GetType().Name}");

                // ✅ FİLTRE: SADECE Surface veya Brep kabul et!
                if (!(entity is Surface) && !(entity is Brep))
                {
                    Debug.WriteLine($"⛔ '{entity.GetType().Name}' Surface veya Brep değil - ATLANACAK!");
                    Debug.WriteLine("   💡 Sadece yüzeyler (Surface/Brep) seçilebilir.");
                    Debug.WriteLine("═══════════════════════════════════════");
                    return;  // Sessizce atla
                }

                // ═══════════════════════════════════════════════════════
                // İLK YÜZEY SEÇİMİ
                // ═══════════════════════════════════════════════════════
                if (selectionCount == 0)
                {
                    face1 = entity;
                    originalColor1 = entity.Color;  // ✅ Orijinal rengi kaydet
                    face1.Color = Color.Yellow;  // SARI
                    face1.ColorMethod = colorMethodType.byEntity;

                    // ✅ SİLİNDİR KONTROLÜ YAP!
                    bool isCylindrical = false;
                    double diameter = 0;
                    Point3D center = Point3D.Origin;
                    Point3D topCenter = Point3D.Origin;
                    Point3D bottomCenter = Point3D.Origin;

                    if (entity is Surface surface)
                    {
                        if (surface is CylindricalSurface cylindricalSurf)
                        {
                            isCylindrical = true;

                            // ✅ DIAMETER MODU KODU (Mesh'ten çap hesapla)
                            diameter = CalculateCylinderDiameterFromMesh(surface);

                            // ✅ ÜST ve ALT Merkez hesapla
                            topCenter = new Point3D(
                                (surface.BoxMin.X + surface.BoxMax.X) / 2.0,
                                (surface.BoxMin.Y + surface.BoxMax.Y) / 2.0,
                                surface.BoxMax.Z
                            );

                            bottomCenter = new Point3D(
                                (surface.BoxMin.X + surface.BoxMax.X) / 2.0,
                                (surface.BoxMin.Y + surface.BoxMax.Y) / 2.0,
                                surface.BoxMin.Z
                            );

                            center = topCenter;  // Varsayılan (eski uyumluluk)

                            Debug.WriteLine($"🔵 1. Yüzey SİLİNDİRİK!");
                            Debug.WriteLine($"📏 Çap: {diameter:F2} mm");
                            Debug.WriteLine($"📍 Üst Merkez: ({topCenter.X:F2}, {topCenter.Y:F2}, {topCenter.Z:F2})");
                            Debug.WriteLine($"📍 Alt Merkez: ({bottomCenter.X:F2}, {bottomCenter.Y:F2}, {bottomCenter.Z:F2})");
                        }
                    }

                    // ✅ Değişkenleri kaydet
                    isCylindrical1 = isCylindrical;
                    diameter1 = diameter;
                    center1 = center;
                    if (isCylindrical)
                    {
                        topCenter1 = topCenter;
                        bottomCenter1 = bottomCenter;
                    }

                    // ✅ PANEL'İ GÜNCELLE
                    if (infoPanel != null && !infoPanel.IsDisposed)
                    {
                        if (isCylindrical)
                        {
                            // Silindir için özel gösterim (Üst ve Alt merkez)
                            infoPanel.UpdateSurface1Cylinder(diameter, topCenter, bottomCenter);
                        }
                        else
                        {
                            // Normal yüzey için alan
                            area1 = CalculateSurfaceArea(entity);
                            Debug.WriteLine($"📐 1. Yüzey Alanı: {area1:F2} mm²");
                            infoPanel.UpdateSurface1(area1);
                        }
                    }

                    design.Invalidate();

                    selectionCount = 1;

                    Debug.WriteLine("✅ 1. yüzey seçildi (SARI)");
                    Debug.WriteLine("📍 2. yüzeyi bekliyor...");
                    Debug.WriteLine("═══════════════════════════════════════");
                }
                // ═══════════════════════════════════════════════════════
                // İKİNCİ YÜZEY SEÇİMİ
                // ═══════════════════════════════════════════════════════
                else if (selectionCount == 1)
                {
                    face2 = entity;
                    originalColor2 = entity.Color;  // ✅ Orijinal rengi kaydet
                    face2.Color = Color.Cyan;    // CYAN
                    face2.ColorMethod = colorMethodType.byEntity;

                    // ✅ SİLİNDİR KONTROLÜ YAP! (İkinci yüzey için)
                    bool isCylindrical = false;
                    double diameter = 0;
                    Point3D center = Point3D.Origin;
                    Point3D topCenter = Point3D.Origin;
                    Point3D bottomCenter = Point3D.Origin;

                    if (entity is Surface surface2)
                    {
                        if (surface2 is CylindricalSurface cylindricalSurf2)
                        {
                            isCylindrical = true;
                            diameter = CalculateCylinderDiameterFromMesh(surface2);

                            // ✅ ÜST ve ALT Merkez hesapla
                            topCenter = new Point3D(
                                (surface2.BoxMin.X + surface2.BoxMax.X) / 2.0,
                                (surface2.BoxMin.Y + surface2.BoxMax.Y) / 2.0,
                                surface2.BoxMax.Z
                            );

                            bottomCenter = new Point3D(
                                (surface2.BoxMin.X + surface2.BoxMax.X) / 2.0,
                                (surface2.BoxMin.Y + surface2.BoxMax.Y) / 2.0,
                                surface2.BoxMin.Z
                            );

                            center = topCenter;  // Varsayılan (eski uyumluluk)

                            Debug.WriteLine($"🔵 2. Yüzey SİLİNDİRİK!");
                            Debug.WriteLine($"📏 Çap: {diameter:F2} mm");
                            Debug.WriteLine($"📍 Üst Merkez: ({topCenter.X:F2}, {topCenter.Y:F2}, {topCenter.Z:F2})");
                            Debug.WriteLine($"📍 Alt Merkez: ({bottomCenter.X:F2}, {bottomCenter.Y:F2}, {bottomCenter.Z:F2})");

                            // ✅ Değişkenleri kaydet
                            topCenter2 = topCenter;
                            bottomCenter2 = bottomCenter;
                        }
                        else
                        {
                            // Normal yüzey
                            area2 = CalculateSurfaceArea(entity);
                            Debug.WriteLine($"📐 2. Yüzey Alanı: {area2:F2} mm²");
                        }
                    }

                    // ✅ Değişkenleri kaydet
                    isCylindrical2 = isCylindrical;
                    diameter2 = diameter;
                    center2 = center;

                    design.Invalidate();

                    selectionCount = 2;

                    Debug.WriteLine("✅ 2. yüzey seçildi (CYAN)");
                    Debug.WriteLine("📏 Mesafe hesaplanıyor...");

                    double dist = 0;

                    // ✅ HER DURUMDA MinimumDistance WorkUnit kullan (P0-P1 için)
                    var minDist = new MinimumDistance(face1, face2);
                    minDist.DoWork(null, null);

                    // ✅ Result bir Segment3D döndürüyor
                    Segment3D segment = minDist.Result;
                    dist = segment.Length;

                    // ✅ P0 ve P1 arasına kırmızı çizgi çiz
                    distanceLine = new Line(segment.P0, segment.P1);
                    distanceLine.Color = Color.Red;
                    distanceLine.ColorMethod = colorMethodType.byEntity;
                    distanceLine.LineWeightMethod = colorMethodType.byEntity;

                    // ✅ Çizgiyi MeasurementLines layer'ına ekle
                    distanceLine.LayerName = MEASUREMENT_LAYER_NAME;

                    // ✅ Çizgiyi entities'e ekle
                    design.Entities.Add(distanceLine);
                    design.Entities.UpdateBoundingBox();
                    design.Invalidate();

                    Debug.WriteLine($"📍 P0: {segment.P0}");
                    Debug.WriteLine($"📍 P1: {segment.P1}");

                    Debug.WriteLine($"📏 EN YAKIN MESAFE: {dist:F3} mm");

                    // ✅ AÇI HESAPLAMA (Normal vektörler arası)
                    double angle = CalculateAngleBetweenSurfaces(face1, face2);
                    Debug.WriteLine($"📐 İKİ YÜZEY ARASI AÇI: {angle:F2}°");

                    // ✅ Mesafeyi field'a kaydet
                    distance = dist;

                    // ✅ Info Panel'i güncelle - silindir durumuna göre
                    if (infoPanel != null && !infoPanel.IsDisposed)
                    {
                        if (isCylindrical1 && isCylindrical2)
                        {
                            // ✅ İKİ SİLİNDİR + Mesafe + Açı
                            infoPanel.UpdateSurface2Cylinder(
                                diameter1, topCenter1, bottomCenter1,
                                diameter2, topCenter2, bottomCenter2,
                                dist, angle);
                        }
                        else if (isCylindrical1 && !isCylindrical2)
                        {
                            // ✅ BİRİNCİ SİLİNDİR, İKİNCİ DÜZLEM + Mesafe + Açı
                            infoPanel.UpdateMixedSurfaces(
                                true, diameter1, topCenter1, bottomCenter1,
                                false, area2,
                                dist, angle);
                        }
                        else if (!isCylindrical1 && isCylindrical2)
                        {
                            // ✅ BİRİNCİ DÜZLEM, İKİNCİ SİLİNDİR + Mesafe + Açı
                            infoPanel.UpdateMixedSurfaces(
                                false, area1,
                                true, diameter2, topCenter2, bottomCenter2,
                                dist, angle);
                        }
                        else
                        {
                            // ✅ İKİ NORMAL YÜZEY - alan göster + Mesafe + Açı
                            infoPanel.UpdateDistance(area1, area2, dist, angle);
                        }
                    }

                    design.Invalidate();

                    Debug.WriteLine("✅ Panel güncellendi!");
                    Debug.WriteLine("═══════════════════════════════════════");
                }
                // ═══════════════════════════════════════════════════════
                // 3. SEÇİM - Yeni ölçüm başlat
                // ═══════════════════════════════════════════════════════
                else
                {
                    Debug.WriteLine("✅ 3. tıklama - Yeni ölçüm başlatılıyor, reset yapılıyor!");
                    ResetSelection();

                    // ✅ Reset'ten sonra bu tıklama 1. seçim olarak sayılacak
                    // Tekrar OnMouseClick çağrılacak (recursive)
                    OnMouseClick(sender, e);
                }

                // TODO: PARÇA 4 - Yardımcı metodlar eklenecek (CalculateAngleBetweenSurfaces vb.)
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Face to Face hata: {ex.Message}");
                MessageBox.Show(
                    $"Face to Face ölçümü sırasında hata oluştu!\n\nHata: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE METHODS - CALCULATION
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Yüzey alanını hesapla
        /// Form1.cs'deki CalculateSurfaceArea metodundan taşındı
        /// </summary>
        private double CalculateSurfaceArea(Entity entity)
        {
            try
            {
                if (entity is Surface surface)
                {
                    // Surface'in GetArea() metodu
                    Point3D centroid;
                    return surface.GetArea(out centroid);
                }
                else if (entity is Brep brep)
                {
                    // Brep'in tüm face'lerinin alanlarını topla
                    double totalArea = 0;
                    if (brep.Faces != null)
                    {
                        foreach (var face in brep.Faces)
                        {
                            if (face.Surface != null)
                            {
                                try
                                {
                                    // face.Surface tipine bakılmaksızın GetArea çağır
                                    // Surface veya AnalyticSurf olabilir
                                    Point3D centroid;

                                    // Reflection kullanarak GetArea metodunu çağır
                                    var surfaceObj = face.Surface;
                                    var getAreaMethod = surfaceObj.GetType().GetMethod("GetArea");

                                    if (getAreaMethod != null)
                                    {
                                        var parameters = new object[] { null };
                                        var area = (double)getAreaMethod.Invoke(surfaceObj, parameters);
                                        centroid = (Point3D)parameters[0];
                                        totalArea += area;
                                    }
                                }
                                catch
                                {
                                    // GetArea yoksa skip
                                }
                            }
                        }
                    }
                    return totalArea;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Alan hesaplama hatası: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// İki yüzey arasındaki en yakın noktaları bul
        /// Form1.cs'deki FaceToFace_GetClosestPoints metodundan taşınacak
        /// </summary>
        private (Point3D point1, Point3D point2, double distance) GetClosestPoints(Entity face1, Entity face2)
        {
            // TODO: Metod içeriği ADIM 3'te taşınacak
            return (Point3D.Origin, Point3D.Origin, 0);
        }

        /// <summary>
        /// Mesafe hesapla ve çizgi çiz
        /// Form1.cs'deki FaceToFace_ComputeDistanceAndLine metodundan taşınacak
        /// </summary>
        private void ComputeDistanceAndLine()
        {
            // TODO: Metod içeriği ADIM 3'te taşınacak
        }

        /// <summary>
        /// Silindir çapını mesh'ten hesapla
        /// Form1.cs'deki FaceToFace_CalculateCylinderDiameterFromMesh metodundan taşındı
        /// </summary>
        private double CalculateCylinderDiameterFromMesh(Surface surface)
        {
            try
            {
                // Surface'i mesh'e çevir
                Mesh mesh = surface.ConvertToMesh(0.1);

                if (mesh == null || mesh.Vertices == null || mesh.Vertices.Length == 0)
                {
                    Debug.WriteLine("⚠️ Mesh oluşturulamadı!");
                    return 0;
                }

                // ✅ Tüm vertex'lerin merkeze uzaklıklarını hesapla
                double minRadius = double.MaxValue;
                double maxRadius = 0;

                // Merkez nokta
                Point3D center = surface.BoxMin + (surface.BoxMax - surface.BoxMin) * 0.5;

                foreach (var vertex in mesh.Vertices)
                {
                    // XY düzleminde merkeze uzaklık (Z hariç)
                    double dx = vertex.X - center.X;
                    double dy = vertex.Y - center.Y;
                    double distanceFromAxis = Math.Sqrt(dx * dx + dy * dy);

                    if (distanceFromAxis > maxRadius)
                        maxRadius = distanceFromAxis;

                    if (distanceFromAxis < minRadius && distanceFromAxis > 0.001)
                        minRadius = distanceFromAxis;
                }

                // Ortalama yarıçap
                double averageRadius = (minRadius + maxRadius) / 2.0;
                double diameter = averageRadius * 2.0;

                Debug.WriteLine($"   🔍 Min Yarıçap: {minRadius:F2} mm");
                Debug.WriteLine($"   🔍 Max Yarıçap: {maxRadius:F2} mm");
                Debug.WriteLine($"   🔍 Ortalama Yarıçap: {averageRadius:F2} mm");

                return diameter;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Mesh'ten çap hesaplama hatası: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// İki yüzey arasındaki açıyı hesapla
        /// Form1.cs'den taşındı
        /// </summary>
        private double CalculateAngleBetweenSurfaces(Entity entity1, Entity entity2)
        {
            try
            {
                Vector3D vector1 = GetSurfaceAxisOrNormal(entity1);
                Vector3D vector2 = GetSurfaceAxisOrNormal(entity2);

                if (vector1 == null || vector2 == null)
                {
                    Debug.WriteLine("❌ Vektörler alınamadı!");
                    return 0;
                }

                // Dot product hesapla
                double dotProduct = Vector3D.Dot(vector1, vector2);

                // Clamp [-1, 1] aralığına
                dotProduct = Math.Max(-1.0, Math.Min(1.0, dotProduct));

                // Açıyı hesapla (radyan → derece)
                double angleRadians = Math.Acos(dotProduct);
                double angleDegrees = angleRadians * (180.0 / Math.PI);

                // ✅ SADECE KARIŞIK (SİLİNDİR + DÜZLEM) İÇİN DÜZELTME!
                bool isCylindrical1Local = IsCylindricalOrConical(entity1);
                bool isCylindrical2Local = IsCylindricalOrConical(entity2);

                // Biri silindir, diğeri düzlem mi?
                bool isMixed = isCylindrical1Local && !isCylindrical2Local ||
                               !isCylindrical1Local && isCylindrical2Local;

                if (isMixed)
                {
                    // Silindir + Düzlem → 90 derece düzeltmesi
                    angleDegrees = Math.Abs(90.0 - angleDegrees);
                    Debug.WriteLine($"📐 Karışık açı düzeltildi (silindir+düzlem): {angleDegrees:F2}°");
                }
                else
                {
                    // İki silindir VEYA İki düzlem → Düzeltme YOK
                    Debug.WriteLine($"📐 Aynı tip açı (düzeltme yok): {angleDegrees:F2}°");
                }

                return angleDegrees;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Açı hesaplama hatası: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Entity silindirik mi kontrol et
        /// Form1.cs'den taşındı
        /// </summary>
        private bool IsCylindricalOrConical(Entity entity)
        {
            if (entity is Surface surface)
            {
                return surface is CylindricalSurface;
            }
            return false;
        }

        /// <summary>
        /// Entity'nin eksen veya normal vektörünü al
        /// Form1.cs'den taşındı
        /// </summary>
        private Vector3D GetSurfaceAxisOrNormal(Entity entity)
        {
            try
            {
                if (entity is Surface surface)
                {
                    // ✅ 1. SİLİNDİRİK YÜZEY → Eksen vektörü
                    if (surface is CylindricalSurface cylindrical)
                    {
                        Debug.WriteLine($"  📍 Silindirik yüzey - Axis kullanılıyor");
                        return cylindrical.Axis;
                    }


                    // ✅ 3. TOROIDAL YÜZEY → Eksen vektörü
                    if (surface is ToroidalSurface toroidal)
                    {
                        Debug.WriteLine($"  📍 Toroidal yüzey - Axis kullanılıyor");
                        return toroidal.Axis;
                    }

                    // ✅ 4. DÜZLEMSEL YÜZEY → Normal vektörü
                    if (surface is PlanarSurface planar)
                    {
                        Debug.WriteLine($"  📍 Düzlemsel yüzey - Normal kullanılıyor");
                        double u = (surface.DomainU.Low + surface.DomainU.High) / 2.0;
                        double v = (surface.DomainV.Low + surface.DomainV.High) / 2.0;
                        return surface.NormalAt(u, v);
                    }

                    // ✅ 5. DİĞER YÜZEYLER → Normal vektörü (fallback)
                    Debug.WriteLine($"  📍 Diğer yüzey ({surface.GetType().Name}) - Normal kullanılıyor");
                    double uMid = (surface.DomainU.Low + surface.DomainU.High) / 2.0;
                    double vMid = (surface.DomainV.Low + surface.DomainV.High) / 2.0;
                    return surface.NormalAt(uMid, vMid);
                }
                else if (entity is Brep brep)
                {
                    // Brep'in ilk face'inin vektörünü al
                    if (brep.Faces != null && brep.Faces.Length > 0)
                    {
                        var face = brep.Faces[0];
                        if (face.Surface != null)
                        {
                            var surfaceObj = face.Surface;
                            var surfaceType = surfaceObj.GetType().Name;

                            // Reflection ile tip kontrolü ve uygun vektörü al
                            if (surfaceType.Contains("Cylindrical") ||
                                surfaceType.Contains("Toroidal"))
                            {
                                // Axis property'sini al
                                var axisProperty = surfaceObj.GetType().GetProperty("Axis");
                                if (axisProperty != null)
                                {
                                    Debug.WriteLine($"  📍 Brep - {surfaceType} - Axis kullanılıyor");
                                    return (Vector3D)axisProperty.GetValue(surfaceObj);
                                }
                            }

                            // Normal kullan (Planar veya diğer)
                            Debug.WriteLine($"  📍 Brep - {surfaceType} - Normal kullanılıyor");
                            var normalAtMethod = surfaceObj.GetType().GetMethod("NormalAt");
                            if (normalAtMethod != null)
                            {
                                var domainUProp = surfaceObj.GetType().GetProperty("DomainU");
                                var domainVProp = surfaceObj.GetType().GetProperty("DomainV");

                                if (domainUProp != null && domainVProp != null)
                                {
                                    dynamic domainU = domainUProp.GetValue(surfaceObj);
                                    dynamic domainV = domainVProp.GetValue(surfaceObj);

                                    double u = (domainU.Low + domainU.High) / 2.0;
                                    double v = (domainV.Low + domainV.High) / 2.0;

                                    return (Vector3D)normalAtMethod.Invoke(surfaceObj, new object[] { u, v });
                                }
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Vektör alma hatası: {ex.Message}");
                return null;
            }
        }
    }
}
