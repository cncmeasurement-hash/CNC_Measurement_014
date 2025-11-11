using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;

namespace _014
{
    /// <summary>
    /// LENGTH MEASUREMENT - SNAP DETECTION
    /// PARTIAL CLASS 4C/6: Snap point detection (7 types)
    /// </summary>
    public partial class LengthMeasurementAnalyzer
    {
        // ═══════════════════════════════════════════════════════════
        // SNAP POINT DETECTION
        // ═══════════════════════════════════════════════════════════

        private Point3D FindNearestSnapPoint(Entity entity, System.Drawing.Point mouseLocation)
        {
            try
            {
                Mesh mesh = null;

                // Entity'yi Mesh'e çevir
                if (entity is Surface surface)
                {
                    mesh = surface.ConvertToMesh();
                }
                else if (entity is Mesh m)
                {
                    mesh = m;
                }

                if (mesh == null || mesh.Vertices == null || mesh.Vertices.Length == 0)
                    return null;

                var viewport = design.Viewports[0];
                double minScreenDistance = double.MaxValue;
                Point3D nearestPoint = null;
                string snapType = "Unknown"; // ✅ YENİ: Hangi snap tipini buldu?

                // ═══════════════════════════════════════════════════════
                // ✅ DÜZELTME: Snap toleransı 10 → 8 pixel (daha hassas)
                // ENDPOINT SNAP - Tüm köşe noktaları
                // Referans: MarkerManager.cs WorldToScreen kullanımı
                // ✅ YENİ: snapEndPoint kontrolü ile aç/kapa
                // ═══════════════════════════════════════════════════════

                // ✅ ENDPOINT SNAP - Eğer aktifse
                if (snapEndPoint)
                {
                    foreach (Point3D vertex in mesh.Vertices)
                    {
                        Point3D screenPt = viewport.WorldToScreen(vertex);

                        // ✅ DÜ ZELTME: Eyeshot "zero on bottom" kullanıyor!
                        // MouseEventArgs ise "zero on top" (Windows Forms standard)
                        // Y koordinatını flip etmeliyiz
                        double screenY = viewport.Size.Height - screenPt.Y;

                        double dx = screenPt.X - mouseLocation.X;
                        double dy = screenY - mouseLocation.Y;
                        double screenDist = Math.Sqrt(dx * dx + dy * dy);

                        // ✅ Snap toleransı kontrolü (dinamik!)
                        if (screenDist < snapTolerance && screenDist < minScreenDistance)
                        {
                            minScreenDistance = screenDist;
                            nearestPoint = vertex;
                            snapType = "EndPoint"; // ✅ SNAP TİPİ
                        }
                    }
                }

                // ═══════════════════════════════════════════════════════
                // MIDPOINT SNAP - SADECE BOUNDARY (SINIR) KENARLARI
                // ✅ YENİ: İç kenarları atla, sadece yüzey sınırlarını göster
                // ✅ Boundary kenar = Sadece 1 triangle tarafından kullanılan kenar
                // ═══════════════════════════════════════════════════════
                if (snapMidPoint && mesh.Triangles != null)
                {
                    // ✅ 1. ADIM: Tüm kenarları say (kaç triangle kullanıyor?)
                    Dictionary<string, int> edgeCount = new Dictionary<string, int>();
                    Dictionary<string, Point3D[]> edgeVertices = new Dictionary<string, Point3D[]>();

                    foreach (var tri in mesh.Triangles)
                    {
                        Point3D v1 = mesh.Vertices[tri.V1];
                        Point3D v2 = mesh.Vertices[tri.V2];
                        Point3D v3 = mesh.Vertices[tri.V3];

                        // 3 kenar - her kenarı string key ile sakla (küçük index önce)
                        var edges = new[]
                        {
                            new { A = Math.Min(tri.V1, tri.V2), B = Math.Max(tri.V1, tri.V2), V1 = v1, V2 = v2 },
                            new { A = Math.Min(tri.V2, tri.V3), B = Math.Max(tri.V2, tri.V3), V1 = v2, V2 = v3 },
                            new { A = Math.Min(tri.V3, tri.V1), B = Math.Max(tri.V3, tri.V1), V1 = v3, V2 = v1 }
                        };

                        foreach (var edge in edges)
                        {
                            string edgeKey = $"{edge.A}-{edge.B}";

                            if (!edgeCount.ContainsKey(edgeKey))
                            {
                                edgeCount[edgeKey] = 0;
                                edgeVertices[edgeKey] = new Point3D[] { edge.V1, edge.V2 };
                            }
                            edgeCount[edgeKey]++;
                        }
                    }

                    // ✅ 2. ADIM: Sadece boundary kenarların (count = 1) ortalarını hesapla
                    foreach (var kvp in edgeCount)
                    {
                        if (kvp.Value == 1) // Boundary kenar!
                        {
                            Point3D[] verts = edgeVertices[kvp.Key];
                            Point3D midPt = new Point3D(
                                (verts[0].X + verts[1].X) / 2,
                                (verts[0].Y + verts[1].Y) / 2,
                                (verts[0].Z + verts[1].Z) / 2
                            );

                            Point3D screenPt = viewport.WorldToScreen(midPt);

                            // ✅ Y koordinatını flip et (Eyeshot zero on bottom)
                            double screenY = viewport.Size.Height - screenPt.Y;

                            double dx = screenPt.X - mouseLocation.X;
                            double dy = screenY - mouseLocation.Y;
                            double screenDist = Math.Sqrt(dx * dx + dy * dy);

                            // ✅ Dinamik tolerans
                            if (screenDist < snapTolerance && screenDist < minScreenDistance)
                            {
                                minScreenDistance = screenDist;
                                nearestPoint = midPt;
                                snapType = "MidPoint (Boundary)"; // ✅ SNAP TİPİ
                            }
                        }
                    }
                }

                // ═══════════════════════════════════════════════════════
                // ✅ YENİ: EDGEPOINT SNAP - Kenar üzerinde en yakın nokta
                // Mouse'a en yakın kenarı bul, kenar üzerinde projeksiyon noktası al
                // ═══════════════════════════════════════════════════════
                if (snapEdgePoint && mesh.Triangles != null)
                {
                    foreach (var tri in mesh.Triangles)
                    {
                        Point3D v1 = mesh.Vertices[tri.V1];
                        Point3D v2 = mesh.Vertices[tri.V2];
                        Point3D v3 = mesh.Vertices[tri.V3];

                        // 3 kenar
                        Point3D[][] edges = new Point3D[][]
                        {
                            new Point3D[] { v1, v2 },
                            new Point3D[] { v2, v3 },
                            new Point3D[] { v3, v1 }
                        };

                        foreach (var edge in edges)
                        {
                            Point3D edgeStart = edge[0];
                            Point3D edgeEnd = edge[1];

                            // Kenar üzerinde 10 nokta sample al (daha hassas)
                            for (int i = 0; i <= 10; i++)
                            {
                                double t = i / 10.0;
                                Point3D edgePt = new Point3D(
                                    edgeStart.X + t * (edgeEnd.X - edgeStart.X),
                                    edgeStart.Y + t * (edgeEnd.Y - edgeStart.Y),
                                    edgeStart.Z + t * (edgeEnd.Z - edgeStart.Z)
                                );

                                Point3D screenPt = viewport.WorldToScreen(edgePt);

                                // ✅ Y koordinatını flip et (Eyeshot zero on bottom)
                                double screenY = viewport.Size.Height - screenPt.Y;

                                double dx = screenPt.X - mouseLocation.X;
                                double dy = screenY - mouseLocation.Y;
                                double screenDist = Math.Sqrt(dx * dx + dy * dy);

                                // ✅ Dinamik tolerans
                                if (screenDist < snapTolerance && screenDist < minScreenDistance)
                                {
                                    minScreenDistance = screenDist;
                                    nearestPoint = edgePt;
                                    snapType = "EdgePoint"; // ✅ SNAP TİPİ
                                }
                            }
                        }
                    }
                }

                // ═══════════════════════════════════════════════════════
                // ✅ YENİ: CENTER SNAP - Cylindrical/Spherical surface merkezi
                // Surface entity üzerinde merkez noktası bul
                // ═══════════════════════════════════════════════════════
                if (snapCenter && entity is Surface surfaceForCenter)
                {
                    try
                    {
                        Point3D centerPoint = null;

                        // Surface tipini kontrol et
                        string surfaceType = surfaceForCenter.GetType().Name;
                        System.Diagnostics.Debug.WriteLine($"🔍 Surface tipi: {surfaceType}");

                        // ✅ DÜZELTME: BoxMin ve BoxMax direkt Point3D döndürür
                        Point3D boxMin = surfaceForCenter.BoxMin;
                        Point3D boxMax = surfaceForCenter.BoxMax;

                        Point3D boxCenter = new Point3D(
                            (boxMin.X + boxMax.X) / 2.0,
                            (boxMin.Y + boxMax.Y) / 2.0,
                            (boxMin.Z + boxMax.Z) / 2.0
                        );

                        centerPoint = boxCenter;

                        if (centerPoint != null)
                        {
                            // Merkezi kontrol et
                            Point3D screenPt = viewport.WorldToScreen(centerPoint);
                            double screenY = viewport.Size.Height - screenPt.Y;
                            double dx = screenPt.X - mouseLocation.X;
                            double dy = screenY - mouseLocation.Y;
                            double screenDist = Math.Sqrt(dx * dx + dy * dy);

                            // Center için daha geniş tolerans (24px = 3x normal)
                            if (screenDist < snapTolerance * 3.0 && screenDist < minScreenDistance)
                            {
                                minScreenDistance = screenDist;
                                nearestPoint = centerPoint;
                                snapType = "Center";
                                System.Diagnostics.Debug.WriteLine($"✅ Center bulundu: {surfaceType}");
                            }
                        }
                    }
                    catch (Exception centerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Center snap hatası: {centerEx.Message}");
                    }
                }

                // ═══════════════════════════════════════════════════════
                // ✅ YENİ: QUADRANT SNAP - Circle üzerinde 4 nokta
                // Cylindrical surface üzerinde N/E/S/W noktaları
                // ═══════════════════════════════════════════════════════
                if (snapQuadrant && entity is Surface surfaceForQuad)
                {
                    try
                    {
                        // ✅ DÜZELTME: BoxMin ve BoxMax direkt Point3D döndürür
                        Point3D boxMin = surfaceForQuad.BoxMin;
                        Point3D boxMax = surfaceForQuad.BoxMax;

                        Point3D meshCenter = new Point3D(
                            (boxMin.X + boxMax.X) / 2.0,
                            (boxMin.Y + boxMax.Y) / 2.0,
                            (boxMin.Z + boxMax.Z) / 2.0
                        );

                        // Yaklaşık yarıçap (bounding box'tan)
                        double radiusX = (boxMax.X - boxMin.X) / 2.0;
                        double radiusY = (boxMax.Y - boxMin.Y) / 2.0;
                        double radius = Math.Max(radiusX, radiusY);

                        // 4 quadrant noktası
                        Point3D[] quadrants = new Point3D[]
                        {
                            new Point3D(meshCenter.X + radius, meshCenter.Y, meshCenter.Z), // East (0°)
                            new Point3D(meshCenter.X, meshCenter.Y + radius, meshCenter.Z), // North (90°)
                            new Point3D(meshCenter.X - radius, meshCenter.Y, meshCenter.Z), // West (180°)
                            new Point3D(meshCenter.X, meshCenter.Y - radius, meshCenter.Z)  // South (270°)
                        };

                        foreach (Point3D qPt in quadrants)
                        {
                            Point3D qScreen = viewport.WorldToScreen(qPt);
                            double qScreenY = viewport.Size.Height - qScreen.Y;
                            double qDx = qScreen.X - mouseLocation.X;
                            double qDy = qScreenY - mouseLocation.Y;
                            double qDist = Math.Sqrt(qDx * qDx + qDy * qDy);

                            if (qDist < snapTolerance * 2.0 && qDist < minScreenDistance)
                            {
                                minScreenDistance = qDist;
                                nearestPoint = qPt;
                                snapType = "Quadrant";
                            }
                        }
                    }
                    catch (Exception quadEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Quadrant snap hatası: {quadEx.Message}");
                    }
                }

                // ═══════════════════════════════════════════════════════
                // ✅ YENİ: TANGENT SNAP - Curve'e Teğet Nokta
                // Circular edge'lerde teğet noktaları yakala
                // ═══════════════════════════════════════════════════════
                if (snapTangent && mesh != null && mesh.Triangles != null)
                {
                    try
                    {
                        // Boundary edge'leri bul (daha önceki MidPoint kodundan)
                        Dictionary<string, Point3D[]> boundaryEdges = new Dictionary<string, Point3D[]>();
                        Dictionary<string, int> edgeCount = new Dictionary<string, int>();

                        foreach (var tri in mesh.Triangles)
                        {
                            Point3D v1 = mesh.Vertices[tri.V1];
                            Point3D v2 = mesh.Vertices[tri.V2];
                            Point3D v3 = mesh.Vertices[tri.V3];

                            var edges = new[]
                            {
                                new { A = Math.Min(tri.V1, tri.V2), B = Math.Max(tri.V1, tri.V2), V1 = v1, V2 = v2 },
                                new { A = Math.Min(tri.V2, tri.V3), B = Math.Max(tri.V2, tri.V3), V1 = v2, V2 = v3 },
                                new { A = Math.Min(tri.V3, tri.V1), B = Math.Max(tri.V3, tri.V1), V1 = v3, V2 = v1 }
                            };

                            foreach (var edge in edges)
                            {
                                string edgeKey = $"{edge.A}-{edge.B}";

                                if (!edgeCount.ContainsKey(edgeKey))
                                {
                                    edgeCount[edgeKey] = 0;
                                    boundaryEdges[edgeKey] = new Point3D[] { edge.V1, edge.V2 };
                                }
                                edgeCount[edgeKey]++;
                            }
                        }

                        // Sadece boundary edge'ler üzerinde tangent noktalarını bul
                        foreach (var kvp in edgeCount)
                        {
                            if (kvp.Value == 1) // Boundary edge
                            {
                                Point3D[] edgeVerts = boundaryEdges[kvp.Key];
                                Point3D edgeStart = edgeVerts[0];
                                Point3D edgeEnd = edgeVerts[1];

                                // Edge vektörü
                                Vector3D edgeVector = new Vector3D(
                                    edgeEnd.X - edgeStart.X,
                                    edgeEnd.Y - edgeStart.Y,
                                    edgeEnd.Z - edgeStart.Z
                                );
                                edgeVector.Normalize();

                                // Edge üzerinde teğet noktaları (başlangıç ve bitiş)
                                // Tangent = edge direction
                                Point3D[] tangentPoints = new Point3D[] { edgeStart, edgeEnd };

                                foreach (Point3D tanPt in tangentPoints)
                                {
                                    Point3D screenPt = viewport.WorldToScreen(tanPt);
                                    double screenY = viewport.Size.Height - screenPt.Y;

                                    double dx = screenPt.X - mouseLocation.X;
                                    double dy = screenY - mouseLocation.Y;
                                    double screenDist = Math.Sqrt(dx * dx + dy * dy);

                                    if (screenDist < snapTolerance && screenDist < minScreenDistance)
                                    {
                                        minScreenDistance = screenDist;
                                        nearestPoint = tanPt;
                                        snapType = "Tangent";
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception tanEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Tangent snap hatası: {tanEx.Message}");
                    }
                }

                // ═══════════════════════════════════════════════════════
                // ✅ YENİ: ORIGIN SNAP - World Origin (0,0,0)
                // Koordinat sisteminin başlangıç noktası
                // ═══════════════════════════════════════════════════════
                if (snapOrigin)
                {
                    try
                    {
                        Point3D originPoint = new Point3D(0, 0, 0);

                        Point3D screenPt = viewport.WorldToScreen(originPoint);
                        double screenY = viewport.Size.Height - screenPt.Y;

                        double dx = screenPt.X - mouseLocation.X;
                        double dy = screenY - mouseLocation.Y;
                        double screenDist = Math.Sqrt(dx * dx + dy * dy);

                        // Origin için geniş tolerans (3x)
                        if (screenDist < snapTolerance * 3.0 && screenDist < minScreenDistance)
                        {
                            minScreenDistance = screenDist;
                            nearestPoint = originPoint;
                            snapType = "Origin";
                            System.Diagnostics.Debug.WriteLine($"✅ Origin snap bulundu (0,0,0)");
                        }
                    }
                    catch (Exception originEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Origin snap hatası: {originEx.Message}");
                    }
                }

                if (nearestPoint != null)
                {
                    System.Diagnostics.Debug.WriteLine($"📍 Snap bulundu [{snapType}]: ({nearestPoint.X:F2}, {nearestPoint.Y:F2}, {nearestPoint.Z:F2}) - Ekran mesafe: {minScreenDistance:F1}px");
                }

                return nearestPoint;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ FindNearestSnapPoint hatası: {ex.Message}");
                return null;
            }
        }
    }
}
