using _014.Managers.Data;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace _014.Handlers.Selection
{
    /// <summary>
    /// Yüzey seçim işlemlerini yönetir
    /// ✅ UPDATED: YEŞİL (Z-) yüzey seçim engeli + Grid'e bildirim
    /// </summary>
    public class FaceSelectionHandler
    {
        private Design design;
        private DataManager dataManager;
        private UIManager uiManager; // ✅ YENİ: Grid'e bildirim için
        private bool autoShowInfo = false;
        private Entity lastSelectedEntity = null;

        public FaceSelectionHandler(Design design, DataManager dataManager = null, UIManager uiManager = null)
        {
            this.design = design;
            this.dataManager = dataManager;
            this.uiManager = uiManager;
            this.design.MouseClick += Design_MouseClick;
        }

        private void Design_MouseClick(object sender, MouseEventArgs e)
        {
            if (!autoShowInfo || design.SelectionFilterMode != selectionFilterType.Face)
                return;

            if (e.Button != MouseButtons.Left)
                return;

            foreach (var entity in design.Entities)
            {
                if (entity.Selected)
                {
                    // ✅ YEŞİL YÜZEY SEÇİLEMEZ KONTROLÜ
                    if (!IsSurfaceSelectable(entity))
                    {
                        entity.Selected = false;
                        design.Invalidate();

                        // ❌ MessageBox kaldırıldı - sadece log
                        System.Diagnostics.Debug.WriteLine("⛔ YEŞİL yüzey (Z-) seçimi engellendi!");
                        return;
                    }

                    if (entity == lastSelectedEntity)
                        continue;

                    lastSelectedEntity = entity;

                    // ✅ YENİ: Grid'de o yüzeyi seç
                    NotifyGridSelection(entity);

                    ShowFaceInfoImmediate(entity);
                    break;
                }
            }
        }

        /// <summary>
        /// ✅ YENİ: Seçilen yüzeyi grid'de göster
        /// </summary>
        private void NotifyGridSelection(Entity entity)
        {
            try
            {
                if (dataManager == null || uiManager == null)
                    return;

                // Entity'nin yüzey index'ini bul
                if (entity.EntityData is string tag)
                {
                    if (tag.StartsWith("SURFACE_LABEL_") || tag.StartsWith("FACE_NORMAL_"))
                    {
                        var surface = dataManager.GetSurfaceByTag(tag);
                        if (surface != null)
                        {
                            uiManager.SelectSurfaceInGrid(surface.Index);
                            System.Diagnostics.Debug.WriteLine($"🎯 Ekran → Grid: Surface_{surface.Index}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ NotifyGridSelection hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ Yüzeyin seçilebilir olup olmadığını kontrol et
        /// YEŞİL yüzeyler (Z-) seçilemez!
        /// </summary>
        private bool IsSurfaceSelectable(Entity entity)
        {
            try
            {
                if (dataManager == null)
                    return true;

                // Tag kontrolü
                if (entity.EntityData is string tag)
                {
                    if (tag.StartsWith("SURFACE_LABEL_") || tag.StartsWith("FACE_NORMAL_"))
                    {
                        var surface = dataManager.GetSurfaceByTag(tag);
                        if (surface != null)
                        {
                            // Alt Yüzey (BOTTOM Z-) = YEŞİL = SEÇİLEMEZ!
                            if (surface.Group == "Alt Yüzey")
                            {
                                System.Diagnostics.Debug.WriteLine($"⛔ {surface.Name} (YEŞİL - Z-) seçilemez!");
                                return false;
                            }
                            return surface.IsSelectable;
                        }
                    }
                }

                // Entity index kontrolü (fallback)
                int entityIndex = -1;
                for (int i = 0; i < design.Entities.Count; i++)
                {
                    if (design.Entities[i] == entity)
                    {
                        entityIndex = i;
                        break;
                    }
                }

                if (entityIndex >= 0)
                {
                    var surfaceList = dataManager.GetSurfaceDataList();
                    foreach (var surface in surfaceList)
                    {
                        if (surface.EntityIndex == entityIndex)
                        {
                            if (surface.Group == "Alt Yüzey")
                            {
                                System.Diagnostics.Debug.WriteLine($"⛔ Entity[{entityIndex}] (YEŞİL - Z-) seçilemez!");
                                return false;
                            }
                            return surface.IsSelectable;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ IsSurfaceSelectable hatası: {ex.Message}");
                return true;
            }
        }

        private void ShowFaceInfoImmediate(Entity entity)
        {
            try
            {
                StringBuilder info = new StringBuilder();

                info.AppendLine("═══════════════════════════════════");
                info.AppendLine("   🎯 YÜZEY BİLGİLERİ");
                info.AppendLine("═══════════════════════════════════");
                info.AppendLine();

                info.AppendLine($"📦 Tip: {entity.GetType().Name}");
                info.AppendLine();

                if (entity is Brep brep)
                {
                    info.AppendLine("📷 BREP BİLGİLERİ:");
                    info.AppendLine($"   • Yüzey Sayısı: {brep.Faces?.Length ?? 0}");
                    info.AppendLine($"   • Kenar Sayısı: {brep.Edges?.Length ?? 0}");
                    info.AppendLine($"   • Vertex Sayısı: {brep.Vertices?.Length ?? 0}");
                    info.AppendLine();

                    var bbox = brep.BoxSize;
                    info.AppendLine("📐 BOYUTLAR:");
                    info.AppendLine($"   • X: {bbox.X:F2} mm");
                    info.AppendLine($"   • Y: {bbox.Y:F2} mm");
                    info.AppendLine($"   • Z: {bbox.Z:F2} mm");
                    info.AppendLine();

                    var center = brep.BoxMin + bbox / 2;
                    info.AppendLine("📍 MERKEZ:");
                    info.AppendLine($"   • X: {center.X:F2}");
                    info.AppendLine($"   • Y: {center.Y:F2}");
                    info.AppendLine($"   • Z: {center.Z:F2}");
                    info.AppendLine();

                    if (brep.Faces != null && brep.Faces.Length > 0)
                    {
                        try
                        {
                            var face = brep.Faces[0];
                            var mesh = face.ConvertToMesh();

                            if (mesh != null && mesh.Triangles != null && mesh.Triangles.Length > 0)
                            {
                                var tri = mesh.Triangles[0];
                                Point3D v0 = mesh.Vertices[tri.V1];
                                Point3D v1 = mesh.Vertices[tri.V2];
                                Point3D v2 = mesh.Vertices[tri.V3];

                                Vector3D edge1 = new Vector3D(v1.X - v0.X, v1.Y - v0.Y, v1.Z - v0.Z);
                                Vector3D edge2 = new Vector3D(v2.X - v0.X, v2.Y - v0.Y, v2.Z - v0.Z);
                                Vector3D normal = Vector3D.Cross(edge1, edge2);
                                normal.Normalize();

                                info.AppendLine("➡️ İLK YÜZEY NORMAL:");
                                info.AppendLine($"   • X: {normal.X:F3}");
                                info.AppendLine($"   • Y: {normal.Y:F3}");
                                info.AppendLine($"   • Z: {normal.Z:F3}");
                                info.AppendLine();

                                string direction = DetermineSurfaceType(normal);
                                info.AppendLine($"🧭 Yön: {direction}");
                                info.AppendLine();
                            }
                        }
                        catch { }
                    }
                }
                else if (entity is Mesh mesh)
                {
                    info.AppendLine("📷 MESH BİLGİLERİ:");
                    info.AppendLine($"   • Vertex Sayısı: {mesh.Vertices?.Length ?? 0}");
                    info.AppendLine($"   • Üçgen Sayısı: {mesh.Triangles?.Length ?? 0}");
                    info.AppendLine();
                }

                info.AppendLine($"🎨 Renk: {entity.Color.Name}");
                info.AppendLine();

                if (!string.IsNullOrEmpty(entity.LayerName))
                {
                    info.AppendLine($"📂 Layer: {entity.LayerName}");
                    info.AppendLine();
                }

                info.AppendLine("═══════════════════════════════════");

                // ❌ MessageBox kaldırıldı - sadece log
                System.Diagnostics.Debug.WriteLine($"ℹ️ Yüzey Bilgisi:\n{info.ToString()}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Bilgi gösterme hatası: {ex.Message}");
            }
        }

        private string DetermineSurfaceType(Vector3D normal)
        {
            double threshold = 0.9;

            if (normal.Z > threshold)
                return "ÜST YÜZEY (Z+)";
            if (normal.Z < -threshold)
                return "ALT YÜZEY (Z-) - 🟢 YEŞİL - SEÇİLEMEZ";
            if (normal.X > threshold)
                return "SAĞ YAN (X+)";
            if (normal.X < -threshold)
                return "SOL YAN (X-)";
            if (normal.Y > threshold)
                return "ÖN YAN (Y+)";
            if (normal.Y < -threshold)
                return "ARKA YAN (Y-)";

            return "EĞİK YÜZEY";
        }

        public void EnableFaceSelection(bool enable)
        {
            if (enable)
            {
                design.ActionMode = actionType.SelectVisibleByPick;
                design.SelectionFilterMode = selectionFilterType.Face;
                design.Cursor = Cursors.Hand;

                autoShowInfo = true;
                lastSelectedEntity = null;

                // ❌ MessageBox kaldırıldı
            }
            else
            {
                design.ActionMode = actionType.None;
                design.Cursor = Cursors.Default;
                design.Entities.ClearSelection();
                design.Invalidate();

                autoShowInfo = false;
                lastSelectedEntity = null;
            }
        }

        public void EnableEntitySelection(bool enable)
        {
            if (enable)
            {
                design.ActionMode = actionType.SelectVisibleByPick;
                design.SelectionFilterMode = selectionFilterType.Entity;
                design.Cursor = Cursors.Hand;

                autoShowInfo = true;
                lastSelectedEntity = null;
            }
            else
            {
                design.ActionMode = actionType.None;
                design.Cursor = Cursors.Default;
                design.Entities.ClearSelection();
                design.Invalidate();

                autoShowInfo = false;
                lastSelectedEntity = null;
            }
        }

        public void ClearSelection()
        {
            design.Entities.ClearSelection();
            design.Invalidate();
            lastSelectedEntity = null;
        }

        public int GetSelectedFaceCount()
        {
            int count = 0;
            foreach (var item in design.Entities)
            {
                if (item.Selected)
                    count++;
            }
            return count;
        }

        public int GetSelectedEntityCount()
        {
            return GetSelectedFaceCount();
        }

        public void ShowSelectedFacesInfo()
        {
            int count = GetSelectedFaceCount();
            // ❌ MessageBox kaldırıldı - sadece log
            System.Diagnostics.Debug.WriteLine($"ℹ️ Seçili yüzey sayısı: {count}");
        }

        public void ChangeSelectedFacesColor(Color color)
        {
            foreach (var item in design.Entities)
            {
                if (item.Selected)
                {
                    item.Color = color;
                    item.ColorMethod = colorMethodType.byEntity;
                }
            }
            design.Invalidate();
        }
    }
}