
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using static devDept.Eyeshot.Entities.Mesh;

using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plane = devDept.Geometry.Plane;
using Point3D = devDept.Geometry.Point3D;
using Vector3D = devDept.Geometry.Vector3D;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 3/3: Measurements and visualization
    /// </summary>
    public partial class FaceMeasurementAnalyzer
    {
        // ════════════════════════════════════════════════════════
        // MEASUREMENTS
        // ════════════════════════════════════════════════════════
        private void PerformMeasurements()
        {
            if (selectedFace1 == null || selectedFace2 == null)
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("📏 ÖLÇÜMLER");

                double minDistance = CalculateMinimumDistance(selectedFace1, selectedFace2);
                double angle = CalculateAngleBetweenFaces(selectedFace1, selectedFace2);
                double area1 = CalculateFaceArea(selectedFace1);
                double area2 = CalculateFaceArea(selectedFace2);

                System.Diagnostics.Debug.WriteLine($"📐 Min: {minDistance:F3} mm");
                System.Diagnostics.Debug.WriteLine($"📐 Açı: {angle:F2}°");
                System.Diagnostics.Debug.WriteLine($"📐 Alan1: {area1:F2} mm²");
                System.Diagnostics.Debug.WriteLine($"📐 Alan2: {area2:F2} mm²");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                DisplayMeasurements(minDistance, angle, area1, area2);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ PerformMeasurements: {ex.Message}");
            }
        }

        /// <summary>
        /// İki yüzey arasındaki GERÇEK minimum mesafeyi hesapla
        /// ✅ Tüm nokta çiftleri arasındaki en kısa mesafeyi bul (Brute Force)
        /// ✅ YENİ: Minimum mesafe noktaları arasına MAVİ ÇİZGİ çiz
        /// </summary>
        private double CalculateMinimumDistance(Face face1, Face face2)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 Minimum mesafe hesaplanıyor...");
                
                // Face'leri mesh'e çevir (geçici - sadece nokta almak için)
                List<Point3D> points1 = new List<Point3D>();
                List<Point3D> points2 = new List<Point3D>();
                
                // Face 1 noktalarını al
                if (face1.BrepFace != null)
                {
                    Mesh mesh1 = face1.BrepFace.ConvertToMesh();
                    if (mesh1 != null && mesh1.Vertices != null)
                    {
                        points1.AddRange(mesh1.Vertices);
                        System.Diagnostics.Debug.WriteLine($"   Face 1: {mesh1.Vertices.Length} nokta");
                    }
                }
                else if (face1.Vertices.Count > 0)
                {
                    points1.AddRange(face1.Vertices);
                    System.Diagnostics.Debug.WriteLine($"   Face 1: {face1.Vertices.Count} nokta (vertices)");
                }
                
                // Face 2 noktalarını al
                if (face2.BrepFace != null)
                {
                    Mesh mesh2 = face2.BrepFace.ConvertToMesh();
                    if (mesh2 != null && mesh2.Vertices != null)
                    {
                        points2.AddRange(mesh2.Vertices);
                        System.Diagnostics.Debug.WriteLine($"   Face 2: {mesh2.Vertices.Length} nokta");
                    }
                }
                else if (face2.Vertices.Count > 0)
                {
                    points2.AddRange(face2.Vertices);
                    System.Diagnostics.Debug.WriteLine($"   Face 2: {face2.Vertices.Count} nokta (vertices)");
                }
                
                // Hiç nokta yoksa merkez mesafesini döndür
                if (points1.Count == 0 || points2.Count == 0)
                {
                    double centerDist = face1.Center.DistanceTo(face2.Center);
                    System.Diagnostics.Debug.WriteLine($"   ⚠️ Nokta yok, merkez mesafesi: {centerDist:F3} mm");
                    return centerDist;
                }
                
                // ✅ BRUTE FORCE: Tüm nokta çiftleri arasındaki en kısa mesafeyi bul
                double minDistance = double.MaxValue;
                Point3D minPoint1 = points1[0];  // ✅ YENİ: En yakın nokta 1
                Point3D minPoint2 = points2[0];  // ✅ YENİ: En yakın nokta 2
                
                foreach (Point3D p1 in points1)
                {
                    foreach (Point3D p2 in points2)
                    {
                        double dist = p1.DistanceTo(p2);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            minPoint1 = p1;  // ✅ Noktaları kaydet
                            minPoint2 = p2;
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"   ✅ Minimum mesafe: {minDistance:F3} mm");
                System.Diagnostics.Debug.WriteLine($"   📍 Nokta 1: ({minPoint1.X:F2}, {minPoint1.Y:F2}, {minPoint1.Z:F2})");
                System.Diagnostics.Debug.WriteLine($"   📍 Nokta 2: ({minPoint2.X:F2}, {minPoint2.Y:F2}, {minPoint2.Z:F2})");
                
                // ═══════════════════════════════════════════════════════════
                // ✅ YENİ: MAVİ KALIN ÇİZGİ ÇİZ
                // ═══════════════════════════════════════════════════════════
                DrawMinDistanceLine(minPoint1, minPoint2);
                
                return minDistance;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"   ❌ Hata: {ex.Message}");
                // Hata durumunda merkez mesafesini döndür
                return face1.Center.DistanceTo(face2.Center);
            }
        }

        private double CalculateMaximumDistance(Face face1, Face face2)
        {
            if (face1.Vertices.Count == 0 || face2.Vertices.Count == 0)
            {
                return face1.Center.DistanceTo(face2.Center);
            }

            double maxDistance = 0;

            foreach (Point3D p1 in face1.Vertices)
            {
                foreach (Point3D p2 in face2.Vertices)
                {
                    double dist = p1.DistanceTo(p2);
                    maxDistance = Math.Max(maxDistance, dist);
                }
            }

            return maxDistance;
        }

        private double CalculateAngleBetweenFaces(Face face1, Face face2)
        {
            double dotProduct = DotProduct(face1.Normal, face2.Normal);
            dotProduct = Math.Max(-1.0, Math.Min(1.0, dotProduct));
            double angleRadians = Math.Acos(dotProduct);
            return angleRadians * (180.0 / Math.PI);
        }

        private double DistancePointToPlane(Point3D point, Plane plane, Vector3D normal)
        {
            Vector3D diff = new Vector3D(
                point.X - plane.Origin.X,
                point.Y - plane.Origin.Y,
                point.Z - plane.Origin.Z
            );
            return Math.Abs(DotProduct(normal, diff));
        }

        private double DotProduct(Vector3D v1, Vector3D v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        /// <summary>
        /// Yüzey alanını hesapla
        /// ✅ BrepFace için ConvertToMesh() kullanarak alan hesapla
        /// </summary>
        private double CalculateFaceArea(Face face)
        {
            try
            {
                // BrepFace varsa, mesh'e çevir ve alan hesapla
                if (face.BrepFace != null)
                {
                    // ✅ Geçici mesh oluştur (sadece hesaplama için!)
                    Mesh tempMesh = face.BrepFace.ConvertToMesh();
                    
                    if (tempMesh != null && tempMesh.Triangles != null && tempMesh.Triangles.Length > 0)
                    {
                        double totalArea = 0;
                        
                        foreach (IndexTriangle tri in tempMesh.Triangles)
                        {
                            Point3D p1 = tempMesh.Vertices[tri.V1];
                            Point3D p2 = tempMesh.Vertices[tri.V2];
                            Point3D p3 = tempMesh.Vertices[tri.V3];

                            // Triangle alanı = 0.5 * |AB × AC|
                            Vector3D ab = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
                            Vector3D ac = new Vector3D(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);
                            Vector3D cross = CrossProduct(ab, ac);
                            double triangleArea = 0.5 * Math.Sqrt(cross.X * cross.X + cross.Y * cross.Y + cross.Z * cross.Z);
                            totalArea += triangleArea;
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"   📐 BrepFace alanı: {totalArea:F2} mm² ({tempMesh.Triangles.Length} triangle)");
                        return totalArea;
                    }
                }

                // Mesh triangles varsa, triangle alanlarını topla
                if (face.SourceMesh != null && face.TriangleIndices.Count > 0)
                {
                    double totalArea = 0;
                    foreach (int triIdx in face.TriangleIndices)
                    {
                        IndexTriangle tri = face.SourceMesh.Triangles[triIdx];
                        Point3D p1 = face.SourceMesh.Vertices[tri.V1];
                        Point3D p2 = face.SourceMesh.Vertices[tri.V2];
                        Point3D p3 = face.SourceMesh.Vertices[tri.V3];

                        // Triangle alanı = 0.5 * |AB × AC|
                        Vector3D ab = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
                        Vector3D ac = new Vector3D(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);
                        Vector3D cross = CrossProduct(ab, ac);
                        double triangleArea = 0.5 * Math.Sqrt(cross.X * cross.X + cross.Y * cross.Y + cross.Z * cross.Z);
                        totalArea += triangleArea;
                    }
                    System.Diagnostics.Debug.WriteLine($"   📐 Mesh alanı: {totalArea:F2} mm² ({face.TriangleIndices.Count} triangle)");
                    return totalArea;
                }

                System.Diagnostics.Debug.WriteLine("   ⚠️ Alan hesaplanamadı (BrepFace yok, Mesh yok)");
                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"   ⚠️ CalculateFaceArea: {ex.Message}");
                return 0;
            }
        }

        private Vector3D CrossProduct(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(
                v1.Y * v2.Z - v1.Z * v2.Y,
                v1.Z * v2.X - v1.X * v2.Z,
                v1.X * v2.Y - v1.Y * v2.X
            );
        }

        // ════════════════════════════════════════════════════════
        // HIGHLIGHT - SURFACE KOPYALA (MESH YAPMA!)
        // ════════════════════════════════════════════════════════
        private void HighlightFace(Face face, Color color)
        {
            try
            {
                if (face.SourceEntity == null)
                {
                    System.Diagnostics.Debug.WriteLine("   ⚠️ SourceEntity yok, highlight atlandı");
                    return;
                }

                // ✅ YENİ YAKLAŞIM: ORİJİNAL ENTITY'NİN RENGİNİ DEĞİŞTİR!
                // KOPYA OLUŞTURMA YOK, ÜSTÜSTE DURUM YOK!

                Entity entity = face.SourceEntity;

                // Orijinal rengi kaydet (ilk kez)
                if (face.OriginalColor == Color.Empty || face.OriginalColor == Color.Transparent)
                {
                    face.OriginalColor = entity.Color;
                    face.OriginalColorMethod = entity.ColorMethod;
                    System.Diagnostics.Debug.WriteLine($"   💾 Orijinal renk kaydedildi: {entity.Color.Name}");
                }

                // Rengi değiştir (ORİJİNAL entity üzerinde!)
                entity.Color = color;
                entity.ColorMethod = colorMethodType.byEntity;

                System.Diagnostics.Debug.WriteLine($"   🎨 Entity rengi değiştirildi: {color.Name}");
                System.Diagnostics.Debug.WriteLine($"   ✅ Highlight tamamlandı (kopya yok, orijinal boyandı!)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Highlight: {ex.Message}");
            }
        }

        private void DisplayMeasurements(double minDist, double angle, double area1, double area2)
        {
            try
            {
                // ✅ YENİ: MessageBox yerine Form'u güncelle!
                if (measurementForm != null)
                {
                    measurementForm.UpdateMeasurements(minDist, angle, area1, area2);
                    System.Diagnostics.Debug.WriteLine($"✅ Form güncellendi: Min={minDist:F3}, Açı={angle:F2}°, Alan1={area1:F2}, Alan2={area2:F2}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ measurementForm null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Display: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════
        // ✅ YENİ: MINIMUM MESAFE ÇİZGİSİ ÇİZ
        // ════════════════════════════════════════════════════════
        /// <summary>
        /// İki nokta arasına MAVİ KALIN çizgi çiz
        /// </summary>
        private void DrawMinDistanceLine(Point3D point1, Point3D point2)
        {
            try
            {
                // Eski çizgiyi temizle
                if (minDistanceLine != null)
                {
                    design.Entities.Remove(minDistanceLine);
                    minDistanceLine = null;
                }

                // ✅ Yeni mavi kalın çizgi oluştur
                minDistanceLine = new Line(point1, point2);
                minDistanceLine.Color = Color.Blue;
                minDistanceLine.ColorMethod = colorMethodType.byEntity;
                minDistanceLine.LineWeight = 5;  // ✅ Kalın çizgi (5 pixel)
                minDistanceLine.EntityData = "MIN_DISTANCE_LINE";  // ✅ Tag ile tanımlama

                // Design'a ekle
                design.Entities.Add(minDistanceLine);
                design.Invalidate();

                System.Diagnostics.Debug.WriteLine("   🔵 Mavi minimum mesafe çizgisi çizildi!");
                System.Diagnostics.Debug.WriteLine($"   📏 Çizgi uzunluğu: {point1.DistanceTo(point2):F3} mm");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"   ⚠️ DrawMinDistanceLine: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════
        // CLEAR
        // ════════════════════════════════════════════════════════
        // ════════════════════════════════════════════════════════
        // CLEAR - ORİJİNAL RENKLERİ GERİ YÜKLE!
        // ════════════════════════════════════════════════════════
        private void ClearVisuals()
        {
            try
            {
                // ✅ Face 1 rengini geri yükle
                if (selectedFace1 != null && selectedFace1.SourceEntity != null)
                {
                    selectedFace1.SourceEntity.Color = selectedFace1.OriginalColor;
                    selectedFace1.SourceEntity.ColorMethod = selectedFace1.OriginalColorMethod;
                    System.Diagnostics.Debug.WriteLine("   🔄 Face 1 rengi geri yüklendi");
                }

                // ✅ Face 2 rengini geri yükle
                if (selectedFace2 != null && selectedFace2.SourceEntity != null)
                {
                    selectedFace2.SourceEntity.Color = selectedFace2.OriginalColor;
                    selectedFace2.SourceEntity.ColorMethod = selectedFace2.OriginalColorMethod;
                    System.Diagnostics.Debug.WriteLine("   🔄 Face 2 rengi geri yüklendi");
                }

                // Eski overlay mesh'leri sil (artık kullanılmıyor ama yine de)
                foreach (Entity entity in visualEntities)
                {
                    design.Entities.Remove(entity);
                }
                visualEntities.Clear();

                if (measurementText != null)
                {
                    design.Entities.Remove(measurementText);
                    measurementText = null;
                }

                // ✅ YENİ: Mavi minimum mesafe çizgisini temizle
                if (minDistanceLine != null)
                {
                    design.Entities.Remove(minDistanceLine);
                    minDistanceLine = null;
                    System.Diagnostics.Debug.WriteLine("   🔵 Mavi çizgi temizlendi");
                }

                design.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ ClearVisuals: {ex.Message}");
            }
        }
    }
}