using System;
using System.Drawing;
using System.Windows.Forms;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 3/3: Marker seçme, silme, yeniden indeksleme
    /// </summary>
    public partial class MarkerManager
    {
        // ═══════════════════════════════════════════════════════════
        // DELETION
        // ═══════════════════════════════════════════════════════════
        
        public void DeleteSelectedMarker()
        {
            try
            {
                if (selectedMarker == null)
                    return;

                System.Diagnostics.Debug.WriteLine($"🗑️ Marker siliniyor: {selectedMarkerIndex}");

                // İlgili normal çizgisini bul ve sil
                string normalLineTag = $"SURFACE_NORMAL_LINE_{selectedMarkerIndex}";
                for (int i = design.Entities.Count - 1; i >= 0; i--)
                {
                    if (design.Entities[i].EntityData is string tag && tag == normalLineTag)
                    {
                        design.Entities.RemoveAt(i);
                        System.Diagnostics.Debug.WriteLine($"  ✅ Normal çizgisi silindi: {normalLineTag}");
                        break;
                    }
                }

                // Marker'ı sil
                design.Entities.Remove(selectedMarker);
                System.Diagnostics.Debug.WriteLine($"  ✅ Marker silindi");

                // DataTable'dan sil
                if (selectedMarkerIndex >= 0 && selectedMarkerIndex < pointsDataTable.Rows.Count)
                {
                    pointsDataTable.Rows.RemoveAt(selectedMarkerIndex);
                    System.Diagnostics.Debug.WriteLine($"  ✅ DataTable'dan silindi");
                }

                // Index'leri yeniden düzenle
                ReindexMarkers();

                selectedMarker = null;
                selectedMarkerIndex = -1;

                design.Invalidate();

                System.Diagnostics.Debug.WriteLine("✅ Marker silme tamamlandı!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteSelectedMarker error: {ex.Message}");
                MessageBox.Show($"Marker silme hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeleteMarkerByIndex(int index)
        {
            try
            {
                if (index < 0 || index >= pointsDataTable.Rows.Count)
                {
                    MessageBox.Show("Geçersiz marker index!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"🗑️ Index ile marker siliniyor: {index}");

                // Marker'ı ve ok'u bul ve sil
                for (int i = design.Entities.Count - 1; i >= 0; i--)
                {
                    if (design.Entities[i].EntityData is string tag &&
                        (tag == $"POINT_MARKER_{index}" || tag == $"NORMAL_ARROW_{index}"))
                    {
                        design.Entities.RemoveAt(i);
                    }
                }

                // DataGrid'den sil
                if (index >= 0 && index < pointsDataTable.Rows.Count)
                {
                    pointsDataTable.Rows.RemoveAt(index);
                }

                // Tüm marker index'lerini yeniden düzenle
                ReindexMarkers();

                selectedMarker = null;
                selectedMarkerIndex = -1;

                design.Invalidate();

                System.Diagnostics.Debug.WriteLine("✅ Marker ve ok silindi, index'ler güncellendi!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteMarkerByIndex error: {ex.Message}");
                MessageBox.Show($"Marker silme hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // SELECTION
        // ═══════════════════════════════════════════════════════════
        
        public void SelectMarker(Entity marker)
        {
            try
            {
                // Önceki seçimi temizle
                if (selectedMarker != null)
                {
                    selectedMarker.Color = Color.Red;
                    selectedMarker.ColorMethod = colorMethodType.byEntity;
                }

                // Yeni marker'ı seç
                selectedMarker = marker;
                if (selectedMarker != null)
                {
                    selectedMarker.Color = Color.Yellow; // SARI
                    selectedMarker.ColorMethod = colorMethodType.byEntity;

                    // Index'i bul
                    if (marker.EntityData is string tag && tag.StartsWith("POINT_MARKER_"))
                    {
                        string indexStr = tag.Replace("POINT_MARKER_", "");
                        if (int.TryParse(indexStr, out int index))
                        {
                            selectedMarkerIndex = index;
                        }
                    }
                }

                design.Invalidate();
                System.Diagnostics.Debug.WriteLine($"✅ Marker seçildi: {selectedMarkerIndex}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SelectMarker error: {ex.Message}");
            }
        }

        public Entity GetMarkerAtMousePosition(int mouseX, int mouseY)
        {
            try
            {
                var viewport = design.Viewports[0];
                double minDistance = double.MaxValue;
                Entity closestMarker = null;

                foreach (Entity ent in design.Entities)
                {
                    if (!ent.Visible)
                        continue;

                    if (ent.EntityData is string tag && tag.StartsWith("POINT_MARKER"))
                    {
                        // Marker'ın merkezini al
                        Point3D markerCenter = ent.BoxMin != null && ent.BoxMax != null
                            ? new Point3D(
                                (ent.BoxMin.X + ent.BoxMax.X) / 2.0,
                                (ent.BoxMin.Y + ent.BoxMax.Y) / 2.0,
                                (ent.BoxMin.Z + ent.BoxMax.Z) / 2.0)
                            : null;

                        if (markerCenter != null)
                        {
                            // WorldToScreen: Point3D return ediyor
                            Point3D screenPt = viewport.WorldToScreen(markerCenter);
                            
                            double dx = screenPt.X - mouseX;
                            double dy = screenPt.Y - mouseY;
                            double distance = Math.Sqrt(dx * dx + dy * dy);

                            if (distance < 15 && distance < minDistance) // 15 pixel threshold
                            {
                                minDistance = distance;
                                closestMarker = ent;
                            }
                        }
                    }
                }

                return closestMarker;
            }
            catch
            {
                return null;
            }
        }

        public void ShowMarkerContextMenu(int x, int y)
        {
            try
            {
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                ToolStripMenuItem deleteItem = new ToolStripMenuItem("🗑️ Sil (Delete)");
                deleteItem.Click += (s, e) => DeleteSelectedMarker();
                contextMenu.Items.Add(deleteItem);

                contextMenu.Show(design, new System.Drawing.Point(x, y));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowMarkerContextMenu error: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // REINDEXING
        // ═══════════════════════════════════════════════════════════
        
        public void ReindexMarkers()
        {
            try
            {
                // DataTable'daki satır numaralarını güncelle
                for (int i = 0; i < pointsDataTable.Rows.Count; i++)
                {
                    pointsDataTable.Rows[i][0] = i + 1;
                }

                // Marker ve Arrow entity data'larını güncelle
                int markerIndex = 0;
                for (int i = 0; i < design.Entities.Count; i++)
                {
                    if (design.Entities[i].EntityData is string tag)
                    {
                        if (tag.StartsWith("POINT_MARKER"))
                        {
                            design.Entities[i].EntityData = $"POINT_MARKER_{markerIndex}";
                            markerIndex++;
                        }
                    }
                }

                // Arrow'ları ayrı olarak reindex et
                int arrowIndex = 0;
                for (int i = 0; i < design.Entities.Count; i++)
                {
                    if (design.Entities[i].EntityData is string tag)
                    {
                        if (tag.StartsWith("NORMAL_ARROW"))
                        {
                            design.Entities[i].EntityData = $"NORMAL_ARROW_{arrowIndex}";
                            arrowIndex++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReindexMarkers error: {ex.Message}");
            }
        }
    }
}
