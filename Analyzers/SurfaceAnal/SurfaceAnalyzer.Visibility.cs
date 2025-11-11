using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Drawing;
using System.Linq;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 2/2: Surface visibility and label control
    /// </summary>
    public partial class SurfaceAnalyzer
    {
        private void AddSurfaceLabel(Point3D center, Vector3D normal, int surfaceIndex, string surfaceType, string group)
        {
            try
            {
                string labelText = $"Surface_{surfaceIndex}\n{surfaceType}";
                double textHeight = 5.0;
                double offset = 15.0;
                
                Point3D labelPos = new Point3D(
                    center.X + normal.X * offset,
                    center.Y + normal.Y * offset,
                    center.Z + normal.Z * offset
                );

                devDept.Eyeshot.Entities.Text textEntity = new devDept.Eyeshot.Entities.Text(
                    labelPos,
                    labelText,
                    textHeight
                );

                textEntity.Alignment = devDept.Eyeshot.Entities.Text.alignmentType.MiddleCenter;
                
                // Z- için KIRMIZI yazı
                if (group == "Alt Yüzey")
                {
                    textEntity.Color = Color.Red;
                }
                else
                {
                    textEntity.Color = Color.White;
                }

                textEntity.ColorMethod = colorMethodType.byEntity;
                textEntity.EntityData = $"SURFACE_LABEL_{surfaceIndex}";

                design.Entities.Add(textEntity);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"  ❌ Etiket hatası: {ex.Message}");
            }
        }

        public void ToggleLabelVisibility(int surfaceIndex, bool visible)
        {
            try
            {
                string labelTag = $"SURFACE_LABEL_{surfaceIndex}";
                
                foreach (Entity ent in design.Entities)
                {
                    if (ent.EntityData is string tag && tag == labelTag)
                    {
                        ent.Visible = visible;
                        break;
                    }
                }

                design.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ToggleLabelVisibility hatası: {ex.Message}");
            }
        }

        public void ToggleArrowVisibility(int surfaceIndex, bool visible)
        {
            try
            {
                string arrowTag = $"FACE_NORMAL_{surfaceIndex}";
                
                foreach (Entity ent in design.Entities)
                {
                    if (ent.EntityData is string tag && tag == arrowTag)
                    {
                        ent.Visible = visible;
                        break;
                    }
                }

                design.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ToggleArrowVisibility hatası: {ex.Message}");
            }
        }

        public void HighlightSurface(int surfaceIndex, bool highlight)
        {
            try
            {
                var surface = dataManager.GetSurfaceByIndex(surfaceIndex);
                if (surface == null) return;

                // Yeşil mesh'i bul ve renklendir
                string greenTag = $"GREEN_FACE_{surface.EntityIndex}_{surface.FaceIndex}";
                
                foreach (Entity ent in design.Entities)
                {
                    if (ent.EntityData is string tag && tag == greenTag && ent is Mesh mesh)
                    {
                        mesh.Color = highlight ? Color.Yellow : Color.Lime;
                        mesh.ColorMethod = colorMethodType.byEntity;
                        break;
                    }
                }

                design.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ HighlightSurface hatası: {ex.Message}");
            }
        }

        public void ClearSurfaceLabels()
        {
            try
            {
                int labelCount = 0;
                int arrowCount = 0;
                int greenCount = 0;

                for (int i = design.Entities.Count - 1; i >= 0; i--)
                {
                    var entity = design.Entities[i];
                    if (entity.EntityData is string tag)
                    {
                        if (tag.StartsWith("SURFACE_LABEL_"))
                        {
                            design.Entities.RemoveAt(i);
                            labelCount++;
                        }
                        else if (tag.StartsWith("FACE_NORMAL_"))
                        {
                            design.Entities.RemoveAt(i);
                            arrowCount++;
                        }
                        else if (tag.StartsWith("GREEN_FACE_"))
                        {
                            design.Entities.RemoveAt(i);
                            greenCount++;
                        }
                    }
                }

                design.Invalidate();
                
                System.Diagnostics.Debug.WriteLine($"✅ Temizlendi: {labelCount} etiket, {arrowCount} ok, {greenCount} yeşil yüzey");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Temizleme hatası: {ex.Message}");
            }
        }
    }
}
