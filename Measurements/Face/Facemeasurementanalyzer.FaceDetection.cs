using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Plane = devDept.Geometry.Plane;
using Point3D = devDept.Geometry.Point3D;
using Vector3D = devDept.Geometry.Vector3D;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 2/3: Face detection and extraction
    /// </summary>
    public partial class FaceMeasurementAnalyzer
    {
        private void Design_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isEnabled || e.Button != MouseButtons.Left) return;

            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🖱️ MOUSE CLICK - Face Measurement");

                Face face = null;

                // ✅ BEST PRACTICE: GetEntityUnderMouseCursor (NURBS'ten!)
                int entityIndex = design.GetEntityUnderMouseCursor(e.Location, true);

                if (entityIndex == -1)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Mouse altında entity yok!");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }

                Entity entity = design.Entities[entityIndex];
                System.Diagnostics.Debug.WriteLine($"✅ Entity: {entity.GetType().Name} (Index: {entityIndex})");

                // ═══════════════════════════════════════════════════════
                // BREP İŞLEME
                // ═══════════════════════════════════════════════════════
                if (entity is Brep brep)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"   📦 Brep: {brep.Faces.Length} faces");

                        Brep.Face selectedBrepFace = GetSelectedBrepFace(brep, e.Location);

                        if (selectedBrepFace == null)
                        {
                            System.Diagnostics.Debug.WriteLine("⚠️ Brep face bulunamadı!");
                            return;
                        }

                        int faceIndex = Array.IndexOf(brep.Faces, selectedBrepFace);
                        System.Diagnostics.Debug.WriteLine($"   ✅ Brep Face #{faceIndex}");

                        face = CreateFaceFromBrepFace(selectedBrepFace, brep);

                        if (face == null)
                        {
                            System.Diagnostics.Debug.WriteLine("❌ Face oluşturulamadı!");
                            return;
                        }

                        // ✅ Entity'yi kaydet
                        face.SourceEntity = entity;
                        face.OriginalColor = entity.Color;
                        face.OriginalColorMethod = entity.ColorMethod;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Brep hatası: {ex.Message}");
                        return;
                    }
                }
                // ═══════════════════════════════════════════════════════
                // SURFACE/MESH İŞLEME (IFace pattern - NURBS'ten!)
                // ═══════════════════════════════════════════════════════
                else if (entity is IFace faceEntity)
                {
                    System.Diagnostics.Debug.WriteLine("   📐 IFace entity (Surface/Mesh)");

                    try
                    {
                        // ✅ FindClosestTriangle - TAM NOKTA BULMA
                        Point3D clickedPoint;
                        int triangleIndex;

                        double distance = design.FindClosestTriangle(
                            faceEntity,
                            e.Location,
                            out clickedPoint,
                            out triangleIndex
                        );

                        System.Diagnostics.Debug.WriteLine($"   📍 Distance: {distance:F3}, Triangle: {triangleIndex}");

                        if (distance >= 0 && triangleIndex >= 0 && clickedPoint != null)
                        {
                            // Mesh al
                            Mesh mesh = null;

                            if (entity is Surface surface)
                            {
                                System.Diagnostics.Debug.WriteLine("   🔄 Surface → Mesh");
                                mesh = surface.ConvertToMesh();
                            }
                            else if (entity is Mesh m)
                            {
                                System.Diagnostics.Debug.WriteLine("   ✅ Mesh entity");
                                mesh = m;
                            }

                            if (mesh != null && triangleIndex < mesh.Triangles.Length)
                            {
                                face = ExtractFaceFromTriangle(mesh, triangleIndex);

                                if (face != null)
                                {
                                    System.Diagnostics.Debug.WriteLine("   ✅ Face oluşturuldu!");

                                    // ✅ Entity'yi kaydet
                                    face.SourceEntity = entity;
                                    face.OriginalColor = entity.Color;
                                    face.OriginalColorMethod = entity.ColorMethod;
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("   ⚠️ FindClosestTriangle başarısız!");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"   ❌ IFace hatası: {ex.Message}");
                        return;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Desteklenmeyen tip: {entity.GetType().Name}");
                    return;
                }

                if (face == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Face oluşturulamadı!");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }

                // ═══════════════════════════════════════════════════════
                // FACE SEÇİM MANTIĞI
                // ═══════════════════════════════════════════════════════
                if (selectedFace1 == null)
                {
                    selectedFace1 = face;
                    HighlightFace(face, face1Color);
                    System.Diagnostics.Debug.WriteLine($"✅ 1. Yüzey seçildi");
                    System.Diagnostics.Debug.WriteLine($"   Normal: ({face.Normal.X:F3}, {face.Normal.Y:F3}, {face.Normal.Z:F3})");
                    System.Diagnostics.Debug.WriteLine($"   Center: ({face.Center.X:F3}, {face.Center.Y:F3}, {face.Center.Z:F3})");
                    System.Diagnostics.Debug.WriteLine("📍 2. yüzeyi seçin");
                }
                else if (selectedFace2 == null)
                {
                    selectedFace2 = face;
                    HighlightFace(face, face2Color);
                    design.Invalidate();  // ✅ Mavi boyayı HEMEN göster!
                    design.Refresh();     // ✅ Ekranı zorla güncelle!
                    System.Diagnostics.Debug.WriteLine($"✅ 2. Yüzey seçildi");
                    System.Diagnostics.Debug.WriteLine($"   Normal: ({face.Normal.X:F3}, {face.Normal.Y:F3}, {face.Normal.Z:F3})");
                    PerformMeasurements();
                }
                else
                {
                    ClearVisuals();
                    selectedFace1 = face;
                    selectedFace2 = null;
                    HighlightFace(face, face1Color);
                    System.Diagnostics.Debug.WriteLine("🔄 YENİ ÖLÇÜM");
                    System.Diagnostics.Debug.WriteLine($"✅ 1. Yüzey seçildi");
                }

                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                design.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ HATA: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
            }
        }


        // ════════════════════════════════════════════════════════
        // GET SELECTED BREP FACE
        // ════════════════════════════════════════════════════════
        private Brep.Face GetSelectedBrepFace(Brep brep, System.Drawing.Point mouseLocation)
        {
            try
            {
                var viewport = design.Viewports[0];

                // Ray origin = Camera location
                Point3D rayOrigin = viewport.Camera.Location;

                // World point
                Point3D worldPoint = viewport.ScreenToWorld(mouseLocation);

                // Ray direction
                Vector3D rayDirection = new Vector3D(
                    worldPoint.X - rayOrigin.X,
                    worldPoint.Y - rayOrigin.Y,
                    worldPoint.Z - rayOrigin.Z
                );

                double minDistance = double.MaxValue;
                Brep.Face closestFace = null;

                foreach (var face in brep.Faces)
                {
                    Mesh faceMesh = face.ConvertToMesh();

                    if (faceMesh == null || faceMesh.Triangles == null)
                        continue;

                    for (int i = 0; i < faceMesh.Triangles.Length; i++)
                    {
                        IndexTriangle tri = faceMesh.Triangles[i];
                        Point3D v0 = faceMesh.Vertices[tri.V1];
                        Point3D v1 = faceMesh.Vertices[tri.V2];
                        Point3D v2 = faceMesh.Vertices[tri.V3];

                        Point3D intersection;
                        if (RayIntersectsTriangle(rayOrigin, rayDirection, v0, v1, v2, out intersection))
                        {
                            double distance = rayOrigin.DistanceTo(intersection);

                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestFace = face;
                            }
                        }
                    }
                }

                return closestFace;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetSelectedBrepFace: {ex.Message}");
                return null;
            }
        }

        // ════════════════════════════════════════════════════════
        // CREATE FACE FROM BREP.FACE
        // ════════════════════════════════════════════════════════
        private Face CreateFaceFromBrepFace(Brep.Face brepFace, Brep parentBrep)
        {
            try
            {
                Face face = new Face();
                face.ParentBrep = parentBrep;
                face.BrepFace = brepFace;

                var surface = brepFace.Surface;
                System.Diagnostics.Debug.WriteLine($"   Surface: {surface.GetType().Name}");

                // ✅ Mesh fallback (tüm tipler için)
                Mesh faceMesh = brepFace.ConvertToMesh();

                if (faceMesh == null || faceMesh.Vertices == null || faceMesh.Vertices.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("   ❌ Mesh oluşturulamadı!");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"   ✅ Mesh: {faceMesh.Triangles.Length} triangles");

                face.SourceMesh = faceMesh;

                for (int i = 0; i < faceMesh.Triangles.Length; i++)
                {
                    face.TriangleIndices.Add(i);
                }

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
                face.Center = center;

                if (faceMesh.Triangles.Length > 0)
                {
                    var tri = faceMesh.Triangles[0];
                    Point3D v0 = faceMesh.Vertices[tri.V1];
                    Point3D v1 = faceMesh.Vertices[tri.V2];
                    Point3D v2 = faceMesh.Vertices[tri.V3];

                    Vector3D edge1 = new Vector3D(v1.X - v0.X, v1.Y - v0.Y, v1.Z - v0.Z);
                    Vector3D edge2 = new Vector3D(v2.X - v0.X, v2.Y - v0.Y, v2.Z - v0.Z);
                    face.Normal = Vector3D.Cross(edge1, edge2);
                    face.Normal.Normalize();
                }
                else
                {
                    face.Normal = new Vector3D(0, 0, 1);
                }

                face.Plane = new Plane(center, face.Normal);
                face.Vertices = new List<Point3D>(faceMesh.Vertices);

                return face;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateFaceFromBrepFace: {ex.Message}");
                return null;
            }
        }

        // ════════════════════════════════════════════════════════
        // RAY CASTING
        // ════════════════════════════════════════════════════════
        private int GetTriangleAtMouseLocation(System.Drawing.Point location, Mesh mesh)
        {
            try
            {
                var viewport = design.Viewports[0];

                Point3D rayOrigin = viewport.Camera.Location;
                Point3D worldPoint = viewport.ScreenToWorld(location);

                Vector3D rayDirection = new Vector3D(
                    worldPoint.X - rayOrigin.X,
                    worldPoint.Y - rayOrigin.Y,
                    worldPoint.Z - rayOrigin.Z
                );

                double minDistance = double.MaxValue;
                int closestTriangle = -1;

                for (int i = 0; i < mesh.Triangles.Length; i++)
                {
                    IndexTriangle tri = mesh.Triangles[i];
                    Point3D v0 = mesh.Vertices[tri.V1];
                    Point3D v1 = mesh.Vertices[tri.V2];
                    Point3D v2 = mesh.Vertices[tri.V3];

                    Point3D intersection;
                    if (RayIntersectsTriangle(rayOrigin, rayDirection, v0, v1, v2, out intersection))
                    {
                        double distance = rayOrigin.DistanceTo(intersection);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestTriangle = i;
                        }
                    }
                }

                return closestTriangle;
            }
            catch
            {
                return -1;
            }
        }

        // ════════════════════════════════════════════════════════
        // RAY TRIANGLE INTERSECTION
        // ════════════════════════════════════════════════════════
        private bool RayIntersectsTriangle(Point3D rayOrigin, Vector3D rayDirection, Point3D v0, Point3D v1, Point3D v2, out Point3D intersection)
        {
            intersection = Point3D.Origin;

            const double EPSILON = 0.0000001;

            Vector3D edge1 = new Vector3D(v1.X - v0.X, v1.Y - v0.Y, v1.Z - v0.Z);
            Vector3D edge2 = new Vector3D(v2.X - v0.X, v2.Y - v0.Y, v2.Z - v0.Z);

            Vector3D h = Vector3D.Cross(rayDirection, edge2);
            double a = DotProduct(edge1, h);

            if (a > -EPSILON && a < EPSILON)
                return false;

            double f = 1.0 / a;
            Vector3D s = new Vector3D(
                rayOrigin.X - v0.X,
                rayOrigin.Y - v0.Y,
                rayOrigin.Z - v0.Z
            );

            double u = f * DotProduct(s, h);

            if (u < 0.0 || u > 1.0)
                return false;

            Vector3D q = Vector3D.Cross(s, edge1);
            double v = f * DotProduct(rayDirection, q);

            if (v < 0.0 || u + v > 1.0)
                return false;

            double t = f * DotProduct(edge2, q);

            if (t > EPSILON)
            {
                intersection = new Point3D(
                    rayOrigin.X + rayDirection.X * t,
                    rayOrigin.Y + rayDirection.Y * t,
                    rayOrigin.Z + rayDirection.Z * t
                );
                return true;
            }

            return false;
        }

        // ════════════════════════════════════════════════════════
        // COPLANAR GROUPING
        // ════════════════════════════════════════════════════════
        private Face ExtractFaceFromTriangle(Mesh mesh, int startTriangleIndex)
        {
            try
            {
                Face face = new Face();
                face.SourceMesh = mesh;

                Queue<int> queue = new Queue<int>();
                HashSet<int> visited = new HashSet<int>();

                queue.Enqueue(startTriangleIndex);
                visited.Add(startTriangleIndex);

                Vector3D baseNormal = CalculateTriangleNormal(mesh, startTriangleIndex);
                face.Normal = baseNormal;

                while (queue.Count > 0)
                {
                    int currentTri = queue.Dequeue();
                    face.TriangleIndices.Add(currentTri);

                    for (int i = 0; i < mesh.Triangles.Length; i++)
                    {
                        if (visited.Contains(i)) continue;

                        if (AreTrianglesAdjacent(mesh, currentTri, i))
                        {
                            Vector3D neighborNormal = CalculateTriangleNormal(mesh, i);

                            if (AreNormalsCoplanar(baseNormal, neighborNormal))
                            {
                                queue.Enqueue(i);
                                visited.Add(i);
                            }
                        }
                    }
                }

                CalculateFaceProperties(face);
                return face;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ExtractFaceFromTriangle: {ex.Message}");
                return null;
            }
        }

        private Vector3D CalculateTriangleNormal(Mesh mesh, int triangleIndex)
        {
            IndexTriangle tri = mesh.Triangles[triangleIndex];
            Point3D v0 = mesh.Vertices[tri.V1];
            Point3D v1 = mesh.Vertices[tri.V2];
            Point3D v2 = mesh.Vertices[tri.V3];

            Vector3D edge1 = new Vector3D(v1.X - v0.X, v1.Y - v0.Y, v1.Z - v0.Z);
            Vector3D edge2 = new Vector3D(v2.X - v0.X, v2.Y - v0.Y, v2.Z - v0.Z);

            Vector3D normal = Vector3D.Cross(edge1, edge2);
            normal.Normalize();
            return normal;
        }

        private bool AreTrianglesAdjacent(Mesh mesh, int tri1Index, int tri2Index)
        {
            IndexTriangle t1 = mesh.Triangles[tri1Index];
            IndexTriangle t2 = mesh.Triangles[tri2Index];

            HashSet<int> vertices1 = new HashSet<int> { t1.V1, t1.V2, t1.V3 };
            HashSet<int> vertices2 = new HashSet<int> { t2.V1, t2.V2, t2.V3 };

            vertices1.IntersectWith(vertices2);
            return vertices1.Count >= 2;
        }

        private bool AreNormalsCoplanar(Vector3D normal1, Vector3D normal2)
        {
            double dotProduct = DotProduct(normal1, normal2);
            double angleDegrees = Math.Acos(Math.Max(-1.0, Math.Min(1.0, dotProduct))) * (180.0 / Math.PI);
            return angleDegrees <= coplanarTolerance;
        }

        private void CalculateFaceProperties(Face face)
        {
            if (face.SourceMesh == null || face.TriangleIndices.Count == 0)
                return;

            HashSet<int> uniqueVertexIndices = new HashSet<int>();
            foreach (int triIndex in face.TriangleIndices)
            {
                IndexTriangle tri = face.SourceMesh.Triangles[triIndex];
                uniqueVertexIndices.Add(tri.V1);
                uniqueVertexIndices.Add(tri.V2);
                uniqueVertexIndices.Add(tri.V3);
            }

            face.Vertices.Clear();
            foreach (int vertexIndex in uniqueVertexIndices)
            {
                face.Vertices.Add(face.SourceMesh.Vertices[vertexIndex]);
            }

            Point3D center = new Point3D(0, 0, 0);
            foreach (Point3D vertex in face.Vertices)
            {
                center.X += vertex.X;
                center.Y += vertex.Y;
                center.Z += vertex.Z;
            }
            center.X /= face.Vertices.Count;
            center.Y /= face.Vertices.Count;
            center.Z /= face.Vertices.Count;
            face.Center = center;

            face.Plane = new Plane(face.Center, face.Normal);
        }

    }
}
