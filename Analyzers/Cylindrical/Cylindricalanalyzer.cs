using _014.Managers.Data;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace _014
{
    /// <summary>
    /// SİLİNDİRİK YÜZEY ANALİZİ
    /// ✅ SurfaceProcessor'dan ayrıldı
    /// ✅ HOLE vs BOSS tespiti
    /// ✅ Point entity + Kesik çizgi görselleştirme
    /// </summary>
    public partial class CylindricalAnalyzer
    {
        private Design design;
        private DataManager dataManager;
        private const string ANALYSIS_LAYER = "Surface_Analysis";

        /// <summary>
        /// Silindir tipi
        /// </summary>
        public enum CylinderType
        {
            Unknown,    // Tespit edilemedi
            Hole,       // Delik (içe doğru)
            Boss        // Çıkıntı (dışa doğru)
        }

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        public CylindricalAnalyzer(Design designControl, DataManager dataManager = null)
        {
            this.design = designControl;
            this.dataManager = dataManager;
        }

        // ═══════════════════════════════════════════════════════════
        // PUBLIC API - MENÜDEN ÇAĞRILACAK METODLAR
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// TÜM silindirleri analiz et (HOLE + BOSS)
        /// </summary>
        public int AnalyzeAll()
        {
            return AnalyzeCylindrical(CylinderType.Unknown);  // Tümü
        }


        // ═══════════════════════════════════════════════════════════
        // ANA ANALİZ METODU
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Silindirik yüzey analizi (filter ile)
        /// </summary>
        private int AnalyzeCylindrical(CylinderType filterType)
        {
            string filterName = filterType == CylinderType.Hole ? "HOLE" :
                               filterType == CylinderType.Boss ? "BOSS" : "TÜM";

            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine($"🔵 SİLİNDİRİK YÜZEY ANALİZİ BAŞLADI ({filterName})");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

            var result = new CylinderAnalysisResult();
            List<Entity> markerEntities = new List<Entity>();
            List<Entity> entityList = new List<Entity>(design.Entities);

            // Model bounding box hesapla (hole/boss tespiti için)
            SimpleBoundingBox modelBox = CalculateModelBoundingBox(entityList);

            foreach (Entity entity in entityList)
            {
                if (entity is Surface surface && surface.GetType().Name == "CylindricalSurface")
                {
                    result.Total++;
                    System.Diagnostics.Debug.WriteLine($"🔍 Silindirik yüzey #{result.Total} bulundu");

                    try
                    {
                        var cylinderInfo = GetCylindricalAxisInfo(surface);
                        if (cylinderInfo != null)
                        {
                            // ✅ HOLE vs BOSS TESPİTİ
                            CylinderType type = DetermineCylinderType(surface, cylinderInfo, modelBox);
                            cylinderInfo.Type = type;


                            // ✅ FİLTRE UYGULA
                            bool shouldVisualize = false;

                            if (filterType == CylinderType.Unknown)
                            {
                                // Tüm silindirler
                                shouldVisualize = true;
                            }
                            else if (type == filterType)
                            {
                                // Sadece filtreye uyan
                                shouldVisualize = true;
                            }

                            // Sayaçları güncelle
                            switch (type)
                            {
                                case CylinderType.Hole:
                                    result.HoleCount++;
                                    System.Diagnostics.Debug.WriteLine($"   🔴 TİP: HOLE (Delik)");
                                    break;
                                case CylinderType.Boss:
                                    result.BossCount++;
                                    System.Diagnostics.Debug.WriteLine($"   🔵 TİP: BOSS (Çıkıntı)");
                                    break;
                                default:
                                    result.UnknownCount++;
                                    System.Diagnostics.Debug.WriteLine($"   ⚪ TİP: UNKNOWN (Belirsiz)");
                                    break;
                            }

                            // ✅ Sadece filtre ile eşleşenleri görselleştir
                            if (shouldVisualize)
                            {
                                var markers = CreateCylindricalVisualization(cylinderInfo, result.Total);
                                markerEntities.AddRange(markers);
                            }

                            result.Successful++;

                            System.Diagnostics.Debug.WriteLine($"   ✅ Bottom: ({cylinderInfo.BottomCenter.X:F2}, {cylinderInfo.BottomCenter.Y:F2}, {cylinderInfo.BottomCenter.Z:F2})");
                            System.Diagnostics.Debug.WriteLine($"   ✅ Top: ({cylinderInfo.TopCenter.X:F2}, {cylinderInfo.TopCenter.Y:F2}, {cylinderInfo.TopCenter.Z:F2})");
                            System.Diagnostics.Debug.WriteLine($"   ✅ Radius: {cylinderInfo.Radius:F2} mm");
                            System.Diagnostics.Debug.WriteLine($"   ✅ Height: {cylinderInfo.Height:F2} mm");
                        }
                        else
                        {
                            result.Failed++;
                            System.Diagnostics.Debug.WriteLine($"   ⚠️ Bilgi hesaplanamadı");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Failed++;
                        System.Diagnostics.Debug.WriteLine($"   ❌ Hata: {ex.Message}");
                    }
                }
            }

            // Marker'ları ekle
            if (markerEntities.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"\n✨ {markerEntities.Count} marker entity ekleniyor...");
                foreach (var marker in markerEntities)
                {
                    design.Entities.Add(marker);
                }
                design.Entities.Regen();
                design.Invalidate();
                System.Diagnostics.Debug.WriteLine($"✅ {markerEntities.Count} marker eklendi!");
            }

            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine($"📊 Toplam silindir: {result.Total}");
            System.Diagnostics.Debug.WriteLine($"   🔴 Hole (Delik): {result.HoleCount}");
            System.Diagnostics.Debug.WriteLine($"   🔵 Boss (Çıkıntı): {result.BossCount}");
            System.Diagnostics.Debug.WriteLine($"   ⚪ Belirsiz: {result.UnknownCount}");
            System.Diagnostics.Debug.WriteLine($"✅ Başarılı: {result.Successful}");
            System.Diagnostics.Debug.WriteLine($"❌ Başarısız: {result.Failed}");
            System.Diagnostics.Debug.WriteLine($"🎨 Görselleştirilen: {markerEntities.Count / 3} silindir ({filterName})");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

            // Filtre ile eşleşen sayıyı döndür
            if (filterType == CylinderType.Hole)
                return result.HoleCount;
            else if (filterType == CylinderType.Boss)
                return result.BossCount;
            else
                return result.Successful;
        }

        // ═══════════════════════════════════════════════════════════
        // HOLE vs BOSS TESPİTİ
        // ═══════════════════════════════════════════════════════════


        /// <summary>
        /// ✅ YENİ: EYESHOT YÖNTEMİ - Normal vektör yönüne göre HOLE/BOSS belirle
        /// Eski karmaşık scoring sistemi yerine basit ve doğru yöntem
        /// </summary>
        private class CylindricalAxisInfo
        {
            public Point3D BottomCenter { get; set; }
            public Point3D TopCenter { get; set; }
            public Vector3D Axis { get; set; }
            public double Radius { get; set; }
            public double Height { get; set; }
            public CylinderType Type { get; set; }
        }

        private class CylinderAnalysisResult
        {
            public int Total { get; set; }
            public int Successful { get; set; }
            public int Failed { get; set; }
            public int HoleCount { get; set; }
            public int BossCount { get; set; }
            public int UnknownCount { get; set; }
        }

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

            public bool IsValid => MinX < double.MaxValue && MaxX > double.MinValue;

            public void UpdateWithBox(object box)
            {
                try
                {
                    var boxType = box.GetType();
                    System.Diagnostics.Debug.WriteLine($"      Box tipi: {boxType.Name}");

                    var allProps = boxType.GetProperties();
                    System.Diagnostics.Debug.WriteLine($"      Property'ler: {string.Join(", ", allProps.Select(p => p.Name))}");

                    var xProp = boxType.GetProperty("X");
                    var yProp = boxType.GetProperty("Y");
                    var zProp = boxType.GetProperty("Z");

                    if (xProp != null && yProp != null && zProp != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"      ⚠️ Size3D tespit edildi ama min/max bilgisi yok!");
                        System.Diagnostics.Debug.WriteLine($"      X={xProp.GetValue(box)}, Y={yProp.GetValue(box)}, Z={zProp.GetValue(box)}");
                    }

                    var corner1Prop = boxType.GetProperty("Corner1");
                    var corner2Prop = boxType.GetProperty("Corner2");

                    if (corner1Prop != null && corner2Prop != null)
                    {
                        var c1 = corner1Prop.GetValue(box);
                        var c2 = corner2Prop.GetValue(box);

                        if (c1 is Point3D p1 && c2 is Point3D p2)
                        {
                            UpdateWithPoints(p1, p2);
                            System.Diagnostics.Debug.WriteLine($"      ✅ Box güncellendi (Corner): C1=({p1.X:F2},{p1.Y:F2},{p1.Z:F2}) C2=({p2.X:F2},{p2.Y:F2},{p2.Z:F2})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"      ❌ Box güncelleme hatası: {ex.Message}");
                }
            }

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
    }
}