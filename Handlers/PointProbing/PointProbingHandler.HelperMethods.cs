using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace _014
{
    public partial class PointProbingHandler
    {
        private Vector3D CalculateTriangleNormal(IFace faceEntity, int triangleIndex)
        {
            try
            {
                // ✅ ÖNCE: Surface ise direkt NormalAt kullan
                if (faceEntity is Surface surface)
                {
                    // Mesh'e çevirip triangle vertices al
                    Mesh mesh = surface.ConvertToMesh();
                    
                    if (mesh != null && mesh.Triangles != null && triangleIndex >= 0 && triangleIndex < mesh.Triangles.Length)
                    {
                        // Triangle'ın merkez noktasını bul
                        IndexTriangle tri = mesh.Triangles[triangleIndex];
                        Point3D p1 = mesh.Vertices[tri.V1];
                        Point3D p2 = mesh.Vertices[tri.V2];
                        Point3D p3 = mesh.Vertices[tri.V3];
                        
                        Point3D center = new Point3D(
                            (p1.X + p2.X + p3.X) / 3.0,
                            (p1.Y + p2.Y + p3.Y) / 3.0,
                            (p1.Z + p2.Z + p3.Z) / 3.0
                        );
                        
                        // ✅ Surface'de en yakın parametrik koordinatı bul
                        double u, v;
                        surface.Project(center, out u, out v);
                        
                        // ✅ Surface.NormalAt(u, v) ile direkt normal al
                        Vector3D normal = surface.NormalAt(u, v);
                        normal.Normalize();
                        
                        System.Diagnostics.Debug.WriteLine($"✅ Surface.NormalAt kullanıldı: ({normal.X:F3}, {normal.Y:F3}, {normal.Z:F3})");
                        return normal;
                    }
                }
                
                // ✅ MESH İSE: Triangle vertices'ten cross product
                if (faceEntity is Mesh mesh2)
                {
                    if (mesh2.Triangles != null && triangleIndex >= 0 && triangleIndex < mesh2.Triangles.Length)
                    {
                        // Triangle'ın 3 vertex'ini al
                        IndexTriangle tri = mesh2.Triangles[triangleIndex];
                        Point3D p1 = mesh2.Vertices[tri.V1];
                        Point3D p2 = mesh2.Vertices[tri.V2];
                        Point3D p3 = mesh2.Vertices[tri.V3];
                        
                        // Cross product ile normal hesapla
                        Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
                        Vector3D v2 = new Vector3D(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);
                        Vector3D normal = Vector3D.Cross(v1, v2);
                        normal.Normalize();
                        
                        System.Diagnostics.Debug.WriteLine($"✅ Mesh cross product kullanıldı: ({normal.X:F3}, {normal.Y:F3}, {normal.Z:F3})");
                        return normal;
                    }
                }
                
                // Varsayılan: Z ekseni yukarı
                System.Diagnostics.Debug.WriteLine("⚠️ Normal hesaplanamadı, varsayılan Z kullanılıyor");
                return new Vector3D(0, 0, 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CalculateTriangleNormal hatası: {ex.Message}");
                // Varsayılan: Z ekseni yukarı
                return new Vector3D(0, 0, 1);
            }
        }

        private Entity CreateSphereMarker(Point3D point, double diameter, Color color)
        {
            try
            {
                // Yarıçapı hesapla
                double radius = diameter / 2.0;
                
                // Küre mesh oluştur
                Mesh sphere = Mesh.CreateSphere(
                    radius,    // Yarıçap (mm)
                    16,        // Yatay detay
                    16         // Dikey detay
                );
                
                // Konumlandır
                sphere.Translate(point.X, point.Y, point.Z);
                
                // Renk ve layer
                sphere.Color = color;
                sphere.ColorMethod = colorMethodType.byEntity;
                sphere.LayerName = MARKER_LAYER_NAME;
                sphere.Selectable = true;  // ✅ Marker SEÇİLEBİLİR (kullanıcı seçip silebilsin)
                
                System.Diagnostics.Debug.WriteLine($"🔴 Küre marker oluşturuldu: Ø{diameter}mm → ({point.X:F2}, {point.Y:F2}, {point.Z:F2})");
                
                return sphere;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateSphereMarker hatası: {ex.Message}");
                
                // Hata durumunda basit bir point döndür
                var fallback = new devDept.Eyeshot.Entities.Point(point);
                fallback.Color = color;
                fallback.ColorMethod = colorMethodType.byEntity;
                fallback.LayerName = MARKER_LAYER_NAME;
                fallback.Selectable = true;  // ✅ Fallback marker da seçilebilir
                return fallback;
            }
        }

        private Entity CreateNormalLine(Point3D startPoint, Vector3D normal, double length, Color color)
        {
            try
            {
                // Bitiş noktası = Başlangıç + (Normal * Uzunluk)
                Point3D endPoint = new Point3D(
                    startPoint.X + normal.X * length,
                    startPoint.Y + normal.Y * length,
                    startPoint.Z + normal.Z * length
                );
                
                // Line oluştur
                devDept.Eyeshot.Entities.Line line = new devDept.Eyeshot.Entities.Line(startPoint, endPoint);
                line.Color = color;
                line.ColorMethod = colorMethodType.byEntity;
                line.LineWeight = 2.0f;  // Kalın çizgi
                line.LayerName = MARKER_LAYER_NAME;
                line.Selectable = false;  // ✅ Çizgi SEÇİLEMEZ (kullanıcı seçemesin/silemesin)
                
                System.Diagnostics.Debug.WriteLine($"➡️ Normal line: {length}mm [{startPoint.X:F2},{startPoint.Y:F2},{startPoint.Z:F2}] → [{endPoint.X:F2},{endPoint.Y:F2},{endPoint.Z:F2}]");
                
                return line;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateNormalLine hatası: {ex.Message}");
                // Fallback: küçük point
                var fallback = new devDept.Eyeshot.Entities.Point(startPoint);
                fallback.Color = color;
                fallback.ColorMethod = colorMethodType.byEntity;
                fallback.LayerName = MARKER_LAYER_NAME;
                fallback.Selectable = false;  // ✅ Fallback line da seçilemez
                return fallback;
            }
        }
    }
}
