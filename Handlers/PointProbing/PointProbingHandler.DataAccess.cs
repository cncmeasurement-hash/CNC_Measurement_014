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
        public List<Point3D> GetPoints()
        {
            return new List<Point3D>(selectedPoints);
        }

        public List<Point3D> GetMarkerPositions()
        {
            var markerPositions = new List<Point3D>();
            
            try
            {
                // Probe diameter'ı al
                double probeDiameter = 6.0; // Default
                if (treeViewManager != null)
                {
                    probeDiameter = treeViewManager.GetSelectedProbeDiameter();
                }
                
                double offset = probeDiameter / 2.0;
                
                // Her nokta için marker pozisyonunu hesapla
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    Point3D contactPoint = selectedPoints[i];
                    Vector3D normal = pointNormals[i];
                    
                    Point3D markerPosition = new Point3D(
                        contactPoint.X + normal.X * offset,
                        contactPoint.Y + normal.Y * offset,
                        contactPoint.Z + normal.Z * offset
                    );
                    
                    markerPositions.Add(markerPosition);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetMarkerPositions hatası: {ex.Message}");
            }
            
            return markerPositions;
        }

        public List<Vector3D> GetNormals()
        {
            return new List<Vector3D>(pointNormals);
        }

        /// <summary>
        /// Nokta sayısı
        /// </summary>
        public int PointCount => selectedPoints.Count;
    }
}
