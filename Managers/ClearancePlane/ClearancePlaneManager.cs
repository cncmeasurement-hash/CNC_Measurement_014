using System;
using System.Drawing;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Control;
using devDept.Geometry;

namespace _014.Managers.ClearancePlane
{
    /// <summary>
    /// Clearance Plane (Güvenlik Düzlemi) Yöneticisi
    /// Z Safety yüksekliğinde mavi, yarı şeffaf bir düzlem çizer
    /// </summary>
    public class ClearancePlaneManager
    {
        private Design design;
        private const string LAYER_NAME = "ClearancePlane";
        
        private double currentZSafety = 50.0;
        private bool isLayerVisible = true;

        /// <summary>
        /// Basit bounding box sınıfı (Eyeshot'ta BoundingBox olmadığı için)
        /// </summary>
        private class SimpleBoundingBox
        {
            public double MinX { get; set; } = double.MaxValue;
            public double MinY { get; set; } = double.MaxValue;
            public double MinZ { get; set; } = double.MaxValue;
            public double MaxX { get; set; } = double.MinValue;
            public double MaxY { get; set; } = double.MinValue;
            public double MaxZ { get; set; } = double.MinValue;

            public Point3D Min => new Point3D(MinX, MinY, MinZ);
            public Point3D Max => new Point3D(MaxX, MaxY, MaxZ);

            public bool IsEmpty => MinX >= double.MaxValue || MaxX <= double.MinValue;

            public void UpdateWithPoints(Point3D min, Point3D max)
            {
                if (min.X < MinX) MinX = min.X;
                if (min.Y < MinY) MinY = min.Y;
                if (min.Z < MinZ) MinZ = min.Z;

                if (max.X > MaxX) MaxX = max.X;
                if (max.Y > MaxY) MaxY = max.Y;
                if (max.Z > MaxZ) MaxZ = max.Z;
            }
        }

        public ClearancePlaneManager(Design design)
        {
            this.design = design;
            InitializeClearancePlaneLayer();
        }

        /// <summary>
        /// Clearance Plane layer'ını oluştur
        /// </summary>
        private void InitializeClearancePlaneLayer()
        {
            try
            {
                // Layer zaten varsa çık
                bool layerExists = false;
                foreach (Layer layer in design.Layers)
                {
                    if (layer.Name == LAYER_NAME)
                    {
                        layerExists = true;
                        break;
                    }
                }

                if (!layerExists)
                {
                    Layer clearanceLayer = new Layer(LAYER_NAME);
                    clearanceLayer.Color = Color.Blue;
                    clearanceLayer.Visible = true;
                    clearanceLayer.LineWeight = 1.0f;
                    
                    design.Layers.Add(clearanceLayer);
                    System.Diagnostics.Debug.WriteLine($"✅ Layer oluşturuldu: {LAYER_NAME}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ℹ️ Layer zaten var: {LAYER_NAME}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Layer oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Z Safety değerini güncelle ve düzlemi yeniden çiz
        /// </summary>
        public void UpdateZSafety(double zSafety)
        {
            currentZSafety = zSafety;
            
            if (isLayerVisible)
            {
                DrawClearancePlane();
            }
            
            System.Diagnostics.Debug.WriteLine($"✅ Z Safety güncellendi: {zSafety} mm");
        }

        /// <summary>
        /// Layer'ı göster/gizle
        /// </summary>
        public void ToggleLayerVisibility()
        {
            try
            {
                if (design.Layers[LAYER_NAME] != null)
                {
                    isLayerVisible = !isLayerVisible;
                    design.Layers[LAYER_NAME].Visible = isLayerVisible;
                    design.Invalidate();
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Clearance Plane: {(isLayerVisible ? "Gösteriliyor" : "Gizlendi")}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Layer visibility hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Layer görünürlük durumunu döndür
        /// </summary>
        public bool IsLayerVisible => isLayerVisible;

        /// <summary>
        /// ✅ YENİ: Clearance Plane görünürlüğünü ayarla (checkbox için)
        /// </summary>
        public void SetVisibility(bool visible)
        {
            try
            {
                if (design.Layers[LAYER_NAME] != null)
                {
                    isLayerVisible = visible;
                    design.Layers[LAYER_NAME].Visible = isLayerVisible;
                    design.Invalidate();
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Clearance Plane: {(isLayerVisible ? "Gösterildi" : "Gizlendi")}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Clearance Plane visibility hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Clearance Plane'i çiz
        /// </summary>
        public void DrawClearancePlane()
        {
            try
            {
                // Önce eski düzlemi temizle
                ClearClearancePlane();

                // Model bounding box'ını hesapla
                SimpleBoundingBox modelBounds = GetModelBoundingBox();
                
                if (modelBounds.IsEmpty)
                {
                    // Model yoksa default boyut
                    modelBounds = new SimpleBoundingBox();
                    modelBounds.UpdateWithPoints(
                        new Point3D(-100, -100, 0),
                        new Point3D(100, 100, 0)
                    );
                    System.Diagnostics.Debug.WriteLine("⚠️ Model bulunamadı, default boyut kullanılıyor");
                }

                // %10 margin ekle
                double marginFactor = 1.1;
                double width = (modelBounds.Max.X - modelBounds.Min.X) * marginFactor;
                double height = (modelBounds.Max.Y - modelBounds.Min.Y) * marginFactor;
                double centerX = (modelBounds.Max.X + modelBounds.Min.X) / 2.0;
                double centerY = (modelBounds.Max.Y + modelBounds.Min.Y) / 2.0;

                // Köşe noktaları hesapla
                double x1 = centerX - width / 2.0;
                double x2 = centerX + width / 2.0;
                double y1 = centerY - height / 2.0;
                double y2 = centerY + height / 2.0;

                // Kapalı dikdörtgen yol oluştur (Z=0'da)
                Point3D[] rectanglePoints = new Point3D[]
                {
                    new Point3D(x1, y1, 0),
                    new Point3D(x2, y1, 0),
                    new Point3D(x2, y2, 0),
                    new Point3D(x1, y2, 0),
                    new Point3D(x1, y1, 0)  // Kapalı yol
                };

                LinearPath rectanglePath = new LinearPath(rectanglePoints);

                // Region oluştur (tam namespace ile)
                devDept.Eyeshot.Entities.Region clearancePlane = 
                    new devDept.Eyeshot.Entities.Region(rectanglePath, Plane.XY);

                // Z yüksekliğine taşı
                clearancePlane.Translate(0, 0, currentZSafety);

                // Mavi, %10 şeffaf (Alpha: 25 = 255 * 0.10)
                clearancePlane.Color = Color.FromArgb(25, 0, 0, 255);
                clearancePlane.ColorMethod = colorMethodType.byEntity;
                clearancePlane.LayerName = LAYER_NAME;
                clearancePlane.EntityData = "SafetyPlane";

                // Düzlemi ekle
                design.Entities.Add(clearancePlane);

                // Yazı ekle
                AddSafetyLabel(centerX, centerY, currentZSafety);

                // Yenile
                design.Entities.Regen();
                design.Invalidate();

                System.Diagnostics.Debug.WriteLine($"✅ Clearance Plane çizildi: Z={currentZSafety}mm, Boyut={width:F1}x{height:F1}mm");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Clearance Plane çizim hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// "Z Safety: XX mm" yazısını ekle
        /// </summary>
        private void AddSafetyLabel(double centerX, double centerY, double z)
        {
            try
            {
                string labelText = $"Z Safety: {currentZSafety:F0} mm";

                Text safetyText = new Text(
                    new Point3D(centerX, centerY, z + 5), // Düzlemin 5mm üstünde
                    labelText,
                    10.0 // Font height
                );

                safetyText.Color = Color.Blue;
                safetyText.ColorMethod = colorMethodType.byEntity;
                safetyText.LayerName = LAYER_NAME;
                safetyText.EntityData = "SafetyLabel";
                safetyText.Alignment = Text.alignmentType.MiddleCenter;

                design.Entities.Add(safetyText);

                System.Diagnostics.Debug.WriteLine($"✅ Yazı eklendi: {labelText}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Yazı ekleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Model bounding box'ını hesapla (Clearance Plane hariç)
        /// </summary>
        private SimpleBoundingBox GetModelBoundingBox()
        {
            SimpleBoundingBox bounds = new SimpleBoundingBox();

            foreach (Entity entity in design.Entities)
            {
                // Clearance Plane layer'ını atla
                if (entity.LayerName == LAYER_NAME)
                    continue;

                try
                {
                    if (entity.BoxMin != null && entity.BoxMax != null)
                    {
                        bounds.UpdateWithPoints(entity.BoxMin, entity.BoxMax);
                    }
                }
                catch
                {
                    // Entity bounding box vermiyorsa devam et
                }
            }

            return bounds;
        }

        /// <summary>
        /// Clearance Plane layer'ındaki tüm entity'leri sil
        /// </summary>
        private void ClearClearancePlane()
        {
            try
            {
                for (int i = design.Entities.Count - 1; i >= 0; i--)
                {
                    Entity entity = design.Entities[i];
                    if (entity.LayerName == LAYER_NAME)
                    {
                        design.Entities.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Clearance Plane temizleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Temizlik (form kapanırken çağrılabilir)
        /// </summary>
        public void Dispose()
        {
            try
            {
                ClearClearancePlane();
                design.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Dispose hatası: {ex.Message}");
            }
        }
    }
}
