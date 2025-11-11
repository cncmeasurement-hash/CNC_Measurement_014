using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _014.Probe.Configuration;
using _014.Probe.Core;
using _014.Managers.Data;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 6/6: ProbeManagement - Probe point management, groups, selection
    /// </summary>
    public partial class TreeViewManager
    {
        public void AddProbingPoint()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎯 Point Probing seçildi!");
                
                if (selectionManager == null)
                {
                    MessageBox.Show("SelectionManager bulunamadı!", "Hata", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // ✅ TÜM BOŞ GRUPLARI KONTROL ET VE SİL
                var groupsToRemove = new List<TreeNode>();
                foreach (TreeNode node in methodNode.Nodes)
                {
                    if (node.Text.StartsWith("Probing - Point"))
                    {
                        int pointCount = 0;
                        foreach (TreeNode child in node.Nodes)
                        {
                            if (child.Tag is Point3D) pointCount++;
                        }
                        
                        if (pointCount == 0)
                        {
                            groupsToRemove.Add(node);
                        }
                    }
                }

                // Boş grupları sil
                foreach (var group in groupsToRemove)
                {
                    methodNode.Nodes.Remove(group);
                    probePointsGroups.Remove(group);
                    System.Diagnostics.Debug.WriteLine($"🗑️ Boş grup silindi: {group.Text}");
                }

                // activeProbeGroup'u güncelle
                if (activeProbeGroup != null && groupsToRemove.Contains(activeProbeGroup))
                {
                    activeProbeGroup = null;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Toplam {groupsToRemove.Count} boş grup silindi");
                
                // ✅ YENİ GRUP OLUŞTUR
                int groupId = selectionManager.CreateNewProbingGroup();
                if (groupId <= 0)
                {
                    MessageBox.Show("Grup oluşturulamadı!", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // TreeView'de yeni grup node'u oluştur
                TreeNode groupNode = CreateNewProbeGroup();
                if (groupNode == null) return;
                
                // Grup ID'sini TreeNode'a kaydet
                groupNode.Tag = $"PROBE_GROUP_{groupId}";
                
                // Handler'a TreeViewManager'ı bağla
                var handler = selectionManager.GetPointProbingHandler();
                if (handler != null)
                {
                    handler.SetTreeViewManager(this);
                    
                    // Callback bağla - her nokta eklendiğinde TreeView'i güncelle
                    handler.OnPointAdded = (point) =>
                    {
                        AddPointToTreeView(point, groupId);
                    };
                    
                    // ✅ Oluşturulan grubun numarasını al
                    string groupNumber = groupNode.Text.Replace("Probing - Point ", "");
                    
                    // Debug log (MessageBox kaldırıldı)
                    System.Diagnostics.Debug.WriteLine($"✅ Probing - Point {groupNumber} aktif!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AddProbingPoint hatası: {ex.Message}");
                MessageBox.Show($"Hata: {ex.Message}", "Point Probing Hatası", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// TreeView'e nokta ekle
        /// </summary>
        private void AddPointToTreeView(Point3D point, int groupId)
        {
            try
            {
                // Grup node'unu bul
                TreeNode groupNode = probePointsGroups.Find(n => n.Tag?.ToString() == $"PROBE_GROUP_{groupId}");
                if (groupNode == null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Grup {groupId} bulunamadı");
                    return;
                }
                
                // Gruptaki nokta sayısını hesapla (Clear All ve Select All hariç)
                int pointCount = 0;
                foreach (TreeNode node in groupNode.Nodes)
                {
                    if (node.Tag is Point3D)
                    {
                        pointCount++;
                    }
                }
                
                int pointIndex = pointCount + 1;
                string pointText = $"Point {pointIndex}: X={point.X,8:F3} Y={point.Y,8:F3} Z={point.Z,8:F3}                                        ";
                
                TreeNode pointNode = new TreeNode(pointText);
                pointNode.Tag = point;  // Noktayı tag'de sakla
                pointNode.ForeColor = Color.Black;  // ✅ Seçili → Siyah
                
                groupNode.Nodes.Add(pointNode);
                groupNode.Expand();
                
                treeView.Invoke((MethodInvoker)(() =>
                {
                    treeView.Refresh();
                }));
                
                System.Diagnostics.Debug.WriteLine($"✅ TreeView'e eklendi: {pointText}");
                
                // ✅ Otomatik JSON kaydet (her nokta eklendiğinde)
                try
                {
                    _014.Managers.Data.MeasurementDataManager.Instance.SaveToJson();
                }
                catch (Exception saveEx)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Otomatik kayıt hatası: {saveEx.Message}");
                }

                // ✅ DÜZELTİLDİ: DataManager'a nokta ekleme KALDIRILDI
                // Nokta zaten PointProbingHandler.AddProbePoint() içinde MeasurementDataManager.Instance.AddPoint() ile ekleniyor
                // Burada tekrar eklemek çift kayıt oluşturuyordu!

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AddPointToTreeView hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// 2. Hole Probing - Delik ölçümü
        /// </summary>

        /// <summary>
        /// ✅ Yeni probe grubu oluştur
        /// </summary>
        public TreeNode CreateNewProbeGroup()
        {
            try
            {
                // ✅ TreeView'deki en büyük grup numarasını bul
                int maxGroupNumber = 0;
                foreach (TreeNode node in methodNode.Nodes)
                {
                    if (node.Text.StartsWith("Probing - Point "))
                    {
                        string numStr = node.Text.Replace("Probing - Point ", "");
                        if (int.TryParse(numStr, out int num))
                        {
                            if (num > maxGroupNumber)
                                maxGroupNumber = num;
                        }
                    }
                }
                
                // ✅ Yeni grup numarası = en büyük + 1
                int newGroupNumber = maxGroupNumber + 1;
                
                // Yeni grup node'u oluştur
                TreeNode groupNode = new TreeNode($"Probing - Point {newGroupNumber}")
                {
                    Tag = "PROBE_GROUP",
                    ForeColor = Color.Black,
                    NodeFont = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                
                // methodNode altına ekle
                methodNode.Nodes.Add(groupNode);
                probePointsGroups.Add(groupNode);
                activeProbeGroup = groupNode;
                
                groupNode.Expand();
                methodNode.Expand();
                treeView.SelectedNode = groupNode;
                
                System.Diagnostics.Debug.WriteLine($"✅ Yeni probe grubu oluşturuldu: Probing - Point {newGroupNumber}");
                
                // ⭐ YENİ: DataManager'a kaydet
                var measurementGroup = new MeasurementGroup
                {
                    GroupId = newGroupNumber,
                    GroupName = $"Probing - Point {newGroupNumber}",
                    MeasurementMode = "PointProbing",
                    ProbeName = SelectedProbe,
                    ProbeDiameter = GetSelectedProbeDiameter(),
                    RetractDistance = RetractDistance,
                    ZSafety = ZSafetyDistance
                };
                
                if (_dataManager != null)
                {
                    _dataManager.AddGroup(measurementGroup);
                    System.Diagnostics.Debug.WriteLine($"✅ DataManager'a grup eklendi: ID={newGroupNumber}, Mode=PointProbing");
                }
                
                return groupNode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateNewProbeGroup hatası: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// ✅ Yeni Ridge Width grubu oluştur
        /// "Ridge Width 1", "Ridge Width 2", ... formatında
        /// </summary>
        public TreeNode CreateNewRidgeWidthGroup()
        {
            try
            {
                // ✅ TreeView'deki en büyük grup numarasını bul (görünüm için)
                int maxGroupNumber = 0;
                foreach (TreeNode node in methodNode.Nodes)
                {
                    if (node.Text.StartsWith("Ridge Width "))
                    {
                        string numStr = node.Text.Replace("Ridge Width ", "");
                        if (int.TryParse(numStr, out int num))
                        {
                            if (num > maxGroupNumber)
                                maxGroupNumber = num;
                        }
                    }
                }
                
                // ✅ Yeni grup numarası = en büyük + 1 (TreeView'de görünecek)
                int newGroupNumber = maxGroupNumber + 1;
                
                // ⭐ DataManager grup ID'si (static counter kullan - Duplicate ID önlemek için)
                ridgeWidthIdCounter++;
                int dataManagerGroupId = ridgeWidthIdCounter;
                
                // Yeni grup node'u oluştur
                TreeNode groupNode = new TreeNode($"Ridge Width {newGroupNumber}")
                {
                    Tag = $"RIDGE_WIDTH_{dataManagerGroupId}",  // ✅ YENİ: Grup ID'si ile tag
                    ForeColor = Color.DarkBlue,
                    NodeFont = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                
                // methodNode altına ekle
                methodNode.Nodes.Add(groupNode);
                
                groupNode.Expand();
                methodNode.Expand();
                treeView.SelectedNode = groupNode;
                
                System.Diagnostics.Debug.WriteLine($"✅ Yeni Ridge Width grubu oluşturuldu: Ridge Width {newGroupNumber}, ID={dataManagerGroupId}");
                
                // ⭐ DataManager'a kaydet
                
                var measurementGroup = new MeasurementGroup
                {
                    GroupId = dataManagerGroupId,
                    GroupName = $"Ridge Width {newGroupNumber}",
                    MeasurementMode = "RidgeWidth",
                    ProbeName = SelectedProbe,
                    ProbeDiameter = GetSelectedProbeDiameter(),
                    RetractDistance = RetractDistance,
                    ZSafety = ZSafetyDistance
                };
                
                if (_dataManager != null)
                {
                    _dataManager.AddGroup(measurementGroup);
                    System.Diagnostics.Debug.WriteLine($"✅ DataManager'a grup eklendi: ID={dataManagerGroupId}, Mode=RidgeWidth");
                }
                
                return groupNode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateNewRidgeWidthGroup hatası: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Ridge Width grubuna nokta ekler
        /// </summary>
        /// <param name="groupNode">Ridge Width grup node'u</param>
        /// <param name="point">Nokta koordinatları</param>
        /// <param name="pointNumber">Nokta numarası</param>
        public void AddPointToRidgeWidthGroup(TreeNode groupNode, Point3D point, int pointNumber)
        {
            try
            {
                if (groupNode == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ groupNode null!");
                    return;
                }
                
                // Nokta node'u oluştur
                string pointText = $"Point {pointNumber}: ({point.X:F2}, {point.Y:F2}, {point.Z:F2})";
                TreeNode pointNode = new TreeNode(pointText)
                {
                    Tag = "RIDGE_WIDTH_POINT",
                    ForeColor = Color.DarkRed,
                    NodeFont = new Font("Segoe UI", 8.5F, FontStyle.Regular)
                };
                
                // Grup node'u altına ekle
                groupNode.Nodes.Add(pointNode);
                groupNode.Expand();
                
                System.Diagnostics.Debug.WriteLine($"✅ TreeView'a nokta eklendi: {pointText}");
                
                // ⭐ YENİ: DataManager'a nokta kaydet
                if (_dataManager != null)
                {
                    try
                    {
                        // Grup ID'sini groupNode.Text'ten çıkar (örn: "Ridge Width 1" -> 1)
                        string groupText = groupNode.Text;
                        int treeViewGroupNumber = int.Parse(groupText.Replace("Ridge Width ", ""));
                        
                        // DataManager'da Ridge Width grupları 2000+ aralığında
                        int dataManagerGroupId = 2000 + treeViewGroupNumber;
                        
                        var group = _dataManager.GetGroup(dataManagerGroupId);
                        if (group != null)
                        {
                            var measurementPoint = new MeasurementPoint
                            {
                                Position = point,
                                MeasurementMode = group.MeasurementMode,
                                ProbeName = group.ProbeName,
                                ProbeDiameter = group.ProbeDiameter,
                                RetractDistance = group.RetractDistance,
                                ZSafety = group.ZSafety
                            };
                            
                            group.AddPoint(measurementPoint);
                            System.Diagnostics.Debug.WriteLine($"✅ DataManager'a nokta eklendi (Ridge Width): Group={dataManagerGroupId}, Point #{group.Points.Count}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ DataManager'da grup bulunamadı (Ridge Width): ID={dataManagerGroupId}");
                        }
                    }
                    catch (Exception parseEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Ridge Width grup ID parse hatası: {parseEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AddPointToRidgeWidthGroup hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Ridge Width grubuna ölçüm sonucu ekler
        /// </summary>
        public void AddResultToRidgeWidthGroup(TreeNode groupNode, double result)
        {
            try
            {
                if (groupNode == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ groupNode null!");
                    return;
                }
                
                // Ölçüm sonucu node'u oluştur
                string resultText = $"Ölçüm Sonucu: {result:F2} mm";
                TreeNode resultNode = new TreeNode(resultText)
                {
                    Tag = "RIDGE_WIDTH_RESULT",
                    ForeColor = Color.DarkBlue,
                    NodeFont = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                
                // Grup node'u altına ekle
                groupNode.Nodes.Add(resultNode);
                groupNode.Expand();
                treeView.Refresh();  // ✅ TreeView'i güncelle
                
                System.Diagnostics.Debug.WriteLine($"✅ TreeView'a ölçüm sonucu eklendi: {resultText}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AddResultToRidgeWidthGroup hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ✅ Grup için Clear All Points
        /// </summary>
        private void ClearGroupPoints(TreeNode groupNode)
        {
            try
            {
                if (groupNode == null || !groupNode.Tag?.ToString().StartsWith("PROBE_GROUP_") == true)
                {
                    return;
                }
                
                // Grup ID'sini al
                string groupTag = groupNode.Tag.ToString();
                int groupId = int.Parse(groupTag.Replace("PROBE_GROUP_", ""));
                
                // Handler'ı al ve temizle
                var handler = selectionManager.GetPointProbingHandler(groupId);
                if (handler != null)
                {
                    handler.ClearAllPoints();
                }
                
                // TreeView'den nokta node'larını sil (Clear All ve Select All hariç)
                List<TreeNode> toRemove = new List<TreeNode>();
                foreach (TreeNode node in groupNode.Nodes)
                {
                    if (node.Tag is Point3D)
                    {
                        toRemove.Add(node);
                    }
                }
                
                foreach (var node in toRemove)
                {
                    groupNode.Nodes.Remove(node);
                }
                
                treeView.Refresh();
                
                System.Diagnostics.Debug.WriteLine($"✅ Grup {groupId} noktaları temizlendi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ClearGroupPoints hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ✅ TÜM grupların point node'larını TreeView'den sil
        /// </summary>
        public void ClearAllGroupsPoints()
        {
            try
            {
                // Tüm grupları iterate et
                foreach (TreeNode groupNode in probePointsGroups)
                {
                    if (groupNode != null)
                    {
                        ClearGroupPoints(groupNode);  // Her grup için point'leri temizle
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("✅ Tüm grupların TreeView point'leri temizlendi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ClearAllGroupsPoints hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Tüm grupları siler (Point Probing, Ridge Width, Angle) - TreeView + DataManager
        /// Form1'den probe değiştiğinde çağrılır
        /// </summary>
        public void ClearAllGroups()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🗑️ TÜM GRUPLAR SİLİNİYOR (TreeView + DataManager)");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                
                int pointGroupCount = 0;
                int ridgeGroupCount = 0;
                int angleGroupCount = 0;
                
                // 1. Point Probing gruplarını topla ve sil
                var pointGroups = new List<TreeNode>();
                foreach (TreeNode node in methodNode.Nodes)
                {
                    if (node.Text.StartsWith("Probing - Point "))
                    {
                        pointGroups.Add(node);
                    }
                }
                
                foreach (var group in pointGroups)
                {
                    methodNode.Nodes.Remove(group);
                    probePointsGroups.Remove(group);
                    pointGroupCount++;
                }
                
                // 2. Ridge Width gruplarını topla ve sil
                var ridgeGroups = new List<TreeNode>();
                foreach (TreeNode node in methodNode.Nodes)
                {
                    if (node.Text.StartsWith("Ridge Width "))
                    {
                        ridgeGroups.Add(node);
                    }
                }
                
                foreach (var group in ridgeGroups)
                {
                    methodNode.Nodes.Remove(group);
                    ridgeGroupCount++;
                }
                
                // 3. Angle gruplarını topla ve sil
                var angleGroups = new List<TreeNode>();
                foreach (TreeNode node in methodNode.Nodes)
                {
                    if (node.Text.StartsWith("Angle "))
                    {
                        angleGroups.Add(node);
                    }
                }
                
                foreach (var group in angleGroups)
                {
                    methodNode.Nodes.Remove(group);
                    angleGroupCount++;
                }
                
                // 4. Active grup referansını temizle
                activeProbeGroup = null;
                
                // 5. DataManager'ı temizle
                if (_dataManager != null)
                {
                    _dataManager.ClearAllData();
                    System.Diagnostics.Debug.WriteLine("✅ DataManager temizlendi");
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ {pointGroupCount} Point Probing grubu silindi");
                System.Diagnostics.Debug.WriteLine($"✅ {ridgeGroupCount} Ridge Width grubu silindi");
                System.Diagnostics.Debug.WriteLine($"✅ {angleGroupCount} Angle grubu silindi");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("✅ TÜM GRUPLAR SİLİNDİ (TreeView + DataManager)");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ClearAllGroups hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Grup ID'sine göre grubu siler (TreeView + DataManager)
        /// Point Probing, Ridge Width ve Angle grupları için çalışır
        /// </summary>
        public void RemoveGroup(int groupId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🗑️ Grup siliniyor: ID={groupId}");
                
                // Grup tipine göre Tag formatını belirle
                string pointTag = $"PROBE_GROUP_{groupId}";
                string ridgeTag = $"RIDGE_WIDTH_{groupId}";
                string angleTag = $"ANGLE_{groupId}";
                
                TreeNode groupToRemove = null;
                
                // methodNode altındaki tüm grupları kontrol et
                foreach (TreeNode node in methodNode.Nodes)
                {
                    string nodeTag = node.Tag?.ToString() ?? "";
                    
                    if (nodeTag == pointTag || nodeTag == ridgeTag || nodeTag == angleTag)
                    {
                        groupToRemove = node;
                        break;
                    }
                }
                
                // Grubu TreeView'den sil
                if (groupToRemove != null)
                {
                    methodNode.Nodes.Remove(groupToRemove);
                    probePointsGroups.Remove(groupToRemove);
                    
                    // activeProbeGroup'u güncelle
                    if (activeProbeGroup == groupToRemove)
                    {
                        activeProbeGroup = null;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Grup TreeView'den silindi: {groupToRemove.Text}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Grup TreeView'de bulunamadı: ID={groupId}");
                }
                
                // DataManager'dan sil
                if (_dataManager != null)
                {
                    _dataManager.RemoveGroup(groupId);
                    System.Diagnostics.Debug.WriteLine($"✅ Grup DataManager'dan silindi: ID={groupId}");
                }
                
                treeView.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RemoveGroup hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ✅ Grup için Select All toggle
        /// </summary>
        private void ToggleGroupSelectAll(TreeNode groupNode)
        {
            try
            {
                if (groupNode == null || !groupNode.Tag?.ToString().StartsWith("PROBE_GROUP_") == true)
                {
                    return;
                }
                
                // Seçili nokta sayısını say
                int selectedCount = 0;
                int totalCount = 0;
                
                foreach (TreeNode node in groupNode.Nodes)
                {
                    if (node.Tag is Point3D)
                    {
                        totalCount++;
                        if (node.ForeColor == Color.Black)
                        {
                            selectedCount++;
                        }
                    }
                }
                
                if (totalCount == 0) return;
                
                // Hepsi seçili değilse → Hepsini seç
                // Hepsi seçiliyse → Hepsini kaldır
                bool selectAll = selectedCount < totalCount;
                
                foreach (TreeNode node in groupNode.Nodes)
                {
                    if (node.Tag is Point3D)
                    {
                        if (selectAll)
                        {
                            node.ForeColor = Color.Black;  // Seçili
                        }
                        else
                        {
                            node.ForeColor = Color.Gray;  // Devre dışı
                        }
                    }
                }
                
                treeView.Refresh();
                
                if (selectAll)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Grup noktaları SEÇİLDİ ({totalCount} nokta)");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"☐ Grup noktaları DEVRE DIŞI ({totalCount} nokta)");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ToggleGroupSelectAll hatası: {ex.Message}");
            }
        }
        private void AddProbingHole()
        {
            System.Diagnostics.Debug.WriteLine("🎯 2️⃣ Hole Probing seçildi!");
            MessageBox.Show("Hole Probing seçildi!\n\nDelik çapı ölçümü yapılacak.", 
                "Probing - Hole", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 3. Boss Probing - Çıkıntı ölçümü
        /// </summary>
        private void AddProbingBoss()
        {
            System.Diagnostics.Debug.WriteLine("🎯 3️⃣ Boss Probing seçildi!");
            MessageBox.Show("Boss Probing seçildi!\n\nÇıkıntı çapı ölçümü yapılacak.", 
                "Probing - Boss", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 4. Slot Width Probing - Yuva genişliği ölçümü
        /// </summary>
        private void AddProbingSlotWidth()
        {
            System.Diagnostics.Debug.WriteLine("🎯 4️⃣ Slot Width Probing seçildi!");
            MessageBox.Show("Slot Width Probing seçildi!\n\nYuva genişliği ölçümü yapılacak.", 
                "Probing - Slot Width", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 5. Ridge Width Probing - Çıkıntı genişliği ölçümü
        /// </summary>
        private void AddProbingRidgeWidth()
        {
            System.Diagnostics.Debug.WriteLine("🎯 5️⃣ Ridge Width Probing seçildi!");
            MessageBox.Show("Ridge Width Probing seçildi!\n\nÇıkıntı genişliği ölçümü yapılacak.", 
                "Probing - Ridge Width", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 6. Rectangle In Probing - İç dikdörtgen ölçümü
        /// </summary>
        private void AddProbingRectangleIn()
        {
            System.Diagnostics.Debug.WriteLine("🎯 6️⃣ Rectangle In Probing seçildi!");
            MessageBox.Show("Rectangle In Probing seçildi!\n\nİç dikdörtgen ölçümü yapılacak.", 
                "Probing - Rectangle In", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 7. Rectangle Out Probing - Dış dikdörtgen ölçümü
        /// </summary>
        private void AddProbingRectangleOut()
        {
            System.Diagnostics.Debug.WriteLine("🎯 7️⃣ Rectangle Out Probing seçildi!");
            MessageBox.Show("Rectangle Out Probing seçildi!\n\nDış dikdörtgen ölçümü yapılacak.", 
                "Probing - Rectangle Out", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 8. Two Holes Probing - İki delik arası mesafe ölçümü
        /// </summary>
        private void AddProbingTwoHoles()
        {
            System.Diagnostics.Debug.WriteLine("🎯 8️⃣ Two Holes Probing seçildi!");
            MessageBox.Show("Two Holes Probing seçildi!\n\nİki delik arası mesafe ölçümü yapılacak.", 
                "Probing - Two Holes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 9. Four Holes Probing - Dört delik ölçümü
        /// </summary>
        private void AddProbingFourHoles()
        {
            System.Diagnostics.Debug.WriteLine("🎯 9️⃣ Four Holes Probing seçildi!");
            MessageBox.Show("Four Holes Probing seçildi!\n\nDört delik ölçümü yapılacak.", 
                "Probing - Four Holes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 10. Angle Probing - Açı ölçümü
        /// </summary>
        private void AddProbingAngle()
        {
            System.Diagnostics.Debug.WriteLine("🎯 🔟 Angle Probing seçildi!");
            MessageBox.Show("Angle Probing seçildi!\n\nAçı ölçümü yapılacak.", 
                "Probing - Angle", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 11. Plane Probing - Düzlem ölçümü
        /// </summary>
        private void AddProbingPlane()
        {
            System.Diagnostics.Debug.WriteLine("🎯 1️⃣1️⃣ Plane Probing seçildi!");
            MessageBox.Show("Plane Probing seçildi!\n\nDüzlem ölçümü yapılacak.", 
                "Probing - Plane", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// ✅ Checkbox değiştiğinde renk güncelle
        /// </summary>

        /// <summary>
        /// ✅ Checkbox değiştirmeden önce kontrol et
        /// Sadece Probe Points node'larına izin ver
        /// </summary>

        /// <summary>
        /// ✅ Sadece seçili (checked) probe noktalarını al
        /// G-code üretimi için kullanılır
        /// </summary>
        public List<Point3D> GetSelectedProbePoints()
        {
            List<Point3D> selectedPoints = new List<Point3D>();
            
            try
            {
                if (probePointsGroups == null || probePointsGroups.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Hiç probe grubu yok!");
                    return selectedPoints;
                }
                
                int totalCount = 0;
                int selectedCount = 0;
                
                // Tüm grupları gez
                foreach (TreeNode groupNode in probePointsGroups)
                {
                    foreach (TreeNode node in groupNode.Nodes)
                    {
                        // Clear All ve Select All hariç, sadece Point3D olan node'lar
                        if (node.Tag is Point3D point)
                        {
                            totalCount++;
                            
                            // ✅ Renk kontrolü: Gri değil = Seçili, Gri = Devre dışı
                            if (node.ForeColor != Color.Gray)
                            {
                                selectedPoints.Add(point);
                                selectedCount++;
                                System.Diagnostics.Debug.WriteLine($"✅ Seçili: {node.Text}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"☐ Atlandı: {node.Text}");
                            }
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"📊 Toplam: {totalCount}, Seçili: {selectedCount}, G-code'a dahil: {selectedCount}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetSelectedProbePoints hatası: {ex.Message}");
            }
            
            return selectedPoints;
        }

        /// <summary>
        /// ✅ Probe Point seçimini toggle et (Color + Icon)
        /// </summary>
        private void ToggleProbePointSelection(TreeNode node)
        {
            try
            {
                // Mevcut durumu kontrol et (siyah = seçili, gri = devre dışı)
                bool isCurrentlySelected = node.ForeColor != Color.Gray;
                
                if (isCurrentlySelected)
                {
                    // Devre dışı yap: Gri
                    node.ForeColor = Color.Gray;
                    System.Diagnostics.Debug.WriteLine($"☐ {node.Text} → Devre dışı (Gri)");
                }
                else
                {
                    // Seçili yap: Siyah
                    node.ForeColor = Color.Black;
                    System.Diagnostics.Debug.WriteLine($"✅ {node.Text} → Seçili (Siyah)");
                }
                
                treeView.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ToggleProbePointSelection hatası: {ex.Message}");
            }
        }


        /// <summary>
        /// ✅ Seçili probe'un D (çap) değerini al
        /// Marker boyutu için kullanılır
        /// </summary>

        /// <summary>
        /// ✅ TreeView'den probe point'i kaldır
        /// Marker silindiğinde çağrılır
        /// </summary>
        public void RemoveProbePointFromTree(Point3D point)
        {
            try
            {
                if (probePointsGroups == null || probePointsGroups.Count == 0)
                {
                    return;
                }
                
                // Tüm grupları gez ve point'i bul
                foreach (TreeNode groupNode in probePointsGroups)
                {
                    TreeNode nodeToRemove = null;
                    
                    foreach (TreeNode node in groupNode.Nodes)
                    {
                        // Point3D tag'ine sahip node'ları kontrol et
                        if (node.Tag is Point3D nodePoint)
                        {
                            // Koordinatları karşılaştır
                            if (Math.Abs(nodePoint.X - point.X) < 0.01 &&
                                Math.Abs(nodePoint.Y - point.Y) < 0.01 &&
                                Math.Abs(nodePoint.Z - point.Z) < 0.01)
                            {
                                nodeToRemove = node;
                                break;
                            }
                        }
                    }
                    
                    // Bulduysak sil
                    if (nodeToRemove != null)
                    {
                        groupNode.Nodes.Remove(nodeToRemove);
                        
                        // Kalan noktaları yeniden numaralandır
                        RenumberGroupPoints(groupNode);
                        
                        treeView.Refresh();
                        System.Diagnostics.Debug.WriteLine($"✅ TreeView'den nokta silindi: X={point.X:F2}, Y={point.Y:F2}, Z={point.Z:F2}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RemoveProbePointFromTree hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ✅ Grup içindeki noktaları yeniden numaralandır
        /// Bir nokta silindiğinde kullanılır
        /// </summary>
        private void RenumberGroupPoints(TreeNode groupNode)
        {
            try
            {
                int pointIndex = 1;
                
                foreach (TreeNode node in groupNode.Nodes)
                {
                    if (node.Tag is Point3D point)
                    {
                        // Yeni numarayla güncelle
                        string oldText = node.Text;
                        string checkMark = oldText.StartsWith("✓") ? "✓" : "☐";
                        node.Text = $"{checkMark} Point {pointIndex}: X={point.X:F2}, Y={point.Y:F2}, Z={point.Z:F2}";
                        pointIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RenumberGroupPoints hatası: {ex.Message}");
            }
        }
        public double GetSelectedProbeDiameter()
        {
            try
            {
                var probeDataList = ProbeStorage.LoadFromJson();
                
                if (probeDataList != null && probeDataList.Count > 0)
                {
                    // Seçili probe'u bul
                    var selectedProbeData = probeDataList.FirstOrDefault(p => p.Name == SelectedProbe);
                    
                    if (selectedProbeData != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Probe çapı alındı: {SelectedProbe} → D={selectedProbeData.D} mm");
                        return (double)selectedProbeData.D;  // ✅ Cast: decimal → double
                    }
                }
                
                // Default: 6mm (renishaw varsayılan)
                System.Diagnostics.Debug.WriteLine($"⚠️ Probe çapı bulunamadı, default kullanılıyor: 6mm");
                return 6.0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetSelectedProbeDiameter hatası: {ex.Message}");
                return 6.0; // Default
            }
        }

        /// <summary>
        /// ✅ TreeView'dan seçili probe'un tüm verisini döndür
        /// CollisionDetector için kullanılır
        /// </summary>
        public ProbeData GetSelectedProbeData()
        {
            try
            {
                var probeDataList = ProbeStorage.LoadFromJson();
                
                if (probeDataList != null && probeDataList.Count > 0)
                {
                    // Seçili probe'u bul
                    var selectedProbeData = probeDataList.FirstOrDefault(p => p.Name == SelectedProbe);
                    
                    if (selectedProbeData != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Seçili probe verisi alındı: {SelectedProbe} (D={selectedProbeData.D}, d1={selectedProbeData.d1}, d2={selectedProbeData.d2})");
                        return selectedProbeData;
                    }
                }
                
                // Default: Renishaw TP20 (6mm)
                System.Diagnostics.Debug.WriteLine($"⚠️ Probe verisi bulunamadı, default kullanılıyor: Renishaw TP20");
                return new ProbeData
                {
                    Name = "Renishaw TP20",
                    D = 6,
                    d1 = 4,
                    d2 = 20,
                    L1 = 20,
                    L2 = 30,
                    L3 = 4
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetSelectedProbeData hatası: {ex.Message}");
                // Default döndür
                return new ProbeData
                {
                    Name = "Renishaw TP20",
                    D = 6,
                    d1 = 4,
                    d2 = 20,
                    L1 = 20,
                    L2 = 30,
                    L3 = 4
                };
            }
        }
        
        /// <summary>
        /// Toolpath oluştur (Marker noktalarından)
        /// </summary>
        private void GenerateToolpath()
        {
            System.Diagnostics.Debug.WriteLine("🔧 Toolpath oluşturuluyor...");
            
            // ToolpathManager'a gönder
            toolpathManager?.GenerateToolpath();
        }
        
        
        /// <summary>
        /// Toolpath node'unu döndür
        /// </summary>
        public TreeNode GetToolpathNode()
        {
            return toolpathNode;
        }
        
        /// <summary>
        /// ✅ YENİ: Tüm Ridge Width gruplarını al (RegenerateAllToolpaths için)
        /// </summary>
        public List<TreeNode> GetAllRidgeWidthGroups()
        {
            var ridgeWidthGroups = new List<TreeNode>();
            
            try
            {
                if (methodNode == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ methodNode null!");
                    return ridgeWidthGroups;
                }
                
                // methodNode altındaki tüm Ridge Width gruplarını bul
                foreach (TreeNode node in methodNode.Nodes)
                {
                    if (node.Tag?.ToString() == "RIDGE_WIDTH_GROUP")
                    {
                        ridgeWidthGroups.Add(node);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ {ridgeWidthGroups.Count} Ridge Width grubu bulundu");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetAllRidgeWidthGroups hatası: {ex.Message}");
            }
            
            return ridgeWidthGroups;
        }

        // ═══════════════════════════════════════════════════════════
        // ANGLE MEASUREMENT METODLARI (RidgeWidth'den uyarlandı)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Yeni Angle Measurement grubu oluşturur (RidgeWidth'den uyarlandı)
        /// </summary>
        public TreeNode CreateNewAngleMeasurementGroup()
        {
            try
            {
                // ✅ TreeView'deki en büyük grup numarasını bul (görünüm için)
                int maxGroupNumber = 0;
                foreach (TreeNode node in methodNode.Nodes)
                {
                    if (node.Text.StartsWith("Angle "))
                    {
                        string numStr = node.Text.Replace("Angle ", "");
                        if (int.TryParse(numStr, out int num))
                        {
                            if (num > maxGroupNumber)
                                maxGroupNumber = num;
                        }
                    }
                }
                
                // ✅ Yeni grup numarası = en büyük + 1 (TreeView'de görünecek)
                int newGroupNumber = maxGroupNumber + 1;
                
                // ⭐ DataManager grup ID'si (static counter kullan - Duplicate ID önlemek için)
                angleMeasurementIdCounter++;
                int dataManagerGroupId = angleMeasurementIdCounter;
                
                // Yeni grup node'u oluştur
                TreeNode groupNode = new TreeNode($"Angle {newGroupNumber}")
                {
                    Tag = $"ANGLE_{dataManagerGroupId}",  // ✅ YENİ: Grup ID'si ile tag
                    ForeColor = Color.DarkGreen,
                    NodeFont = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                
                // methodNode altına ekle
                methodNode.Nodes.Add(groupNode);
                
                groupNode.Expand();
                methodNode.Expand();
                treeView.SelectedNode = groupNode;
                
                System.Diagnostics.Debug.WriteLine($"✅ Yeni Angle Measurement grubu oluşturuldu: Angle {newGroupNumber}, ID={dataManagerGroupId}");
                
                // ⭐ DataManager'a kaydet
                
                var measurementGroup = new MeasurementGroup
                {
                    GroupId = dataManagerGroupId,
                    GroupName = $"Angle {newGroupNumber}",
                    MeasurementMode = "Angle",
                    ProbeName = SelectedProbe,
                    ProbeDiameter = GetSelectedProbeDiameter(),
                    RetractDistance = RetractDistance,
                    ZSafety = ZSafetyDistance
                };
                
                if (_dataManager != null)
                {
                    _dataManager.AddGroup(measurementGroup);
                    System.Diagnostics.Debug.WriteLine($"✅ DataManager'a grup eklendi: ID={dataManagerGroupId}, Mode=Angle");
                }
                
                return groupNode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateNewAngleMeasurementGroup hatası: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Angle Measurement grubuna nokta ekler
        /// </summary>
        /// <param name="groupNode">Angle grup node'u</param>
        /// <param name="point">Nokta koordinatları</param>
        /// <param name="pointNumber">Nokta numarası (1 veya 2)</param>
        public void AddPointToAngleMeasurementGroup(TreeNode groupNode, Point3D point, int pointNumber)
        {
            try
            {
                if (groupNode == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ groupNode null!");
                    return;
                }
                
                // Nokta node'u oluştur
                string pointText = $"Point {pointNumber}: ({point.X:F2}, {point.Y:F2}, {point.Z:F2})";
                TreeNode pointNode = new TreeNode(pointText)
                {
                    Tag = "ANGLE_POINT",
                    ForeColor = pointNumber == 1 ? Color.Red : Color.Blue,  // 1. nokta kırmızı, 2. mavi
                    NodeFont = new Font("Segoe UI", 8.5F, FontStyle.Regular)
                };
                
                // Grup node'u altına ekle
                groupNode.Nodes.Add(pointNode);
                groupNode.Expand();
                
                System.Diagnostics.Debug.WriteLine($"✅ TreeView'a nokta eklendi: {pointText}");
                
                // ⭐ YENİ: DataManager'a nokta kaydet
                if (_dataManager != null)
                {
                    try
                    {
                        // Grup ID'sini groupNode.Text'ten çıkar (örn: "Angle 1" -> 1)
                        string groupText = groupNode.Text;
                        int treeViewGroupNumber = int.Parse(groupText.Replace("Angle ", ""));
                        
                        // DataManager'da Angle grupları 1000+ aralığında
                        int dataManagerGroupId = 1000 + treeViewGroupNumber;
                        
                        var group = _dataManager.GetGroup(dataManagerGroupId);
                        if (group != null)
                        {
                            var measurementPoint = new MeasurementPoint
                            {
                                Position = point,
                                MeasurementMode = group.MeasurementMode,
                                ProbeName = group.ProbeName,
                                ProbeDiameter = group.ProbeDiameter,
                                RetractDistance = group.RetractDistance,
                                ZSafety = group.ZSafety
                            };
                            
                            group.AddPoint(measurementPoint);
                            System.Diagnostics.Debug.WriteLine($"✅ DataManager'a nokta eklendi (Angle): Group={dataManagerGroupId}, Point #{group.Points.Count}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ DataManager'da grup bulunamadı (Angle): ID={dataManagerGroupId}");
                        }
                    }
                    catch (Exception parseEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Angle grup ID parse hatası: {parseEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AddPointToAngleMeasurementGroup hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Angle Measurement grubuna ölçüm sonucu ekler
        /// </summary>
        public void AddResultToAngleMeasurementGroup(TreeNode groupNode, double angle, string referenceAxis)
        {
            try
            {
                if (groupNode == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ groupNode null!");
                    return;
                }
                
                // Ölçüm sonucu node'u oluştur
                string resultText = $"Açı: {angle:F2}° (Referans: {referenceAxis})";
                TreeNode resultNode = new TreeNode(resultText)
                {
                    Tag = "ANGLE_RESULT",
                    ForeColor = Color.DarkGreen,
                    NodeFont = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                
                // Grup node'u altına ekle
                groupNode.Nodes.Add(resultNode);
                groupNode.Expand();
                treeView.Refresh();  // ✅ TreeView'i güncelle
                
                System.Diagnostics.Debug.WriteLine($"✅ TreeView'a açı sonucu eklendi: {resultText}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AddResultToAngleMeasurementGroup hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Tüm Angle Measurement gruplarını döndürür
        /// </summary>
        public List<TreeNode> GetAllAngleMeasurementGroups()
        {
            List<TreeNode> angleGroups = new List<TreeNode>();
            
            try
            {
                foreach (TreeNode node in methodNode.Nodes)
                {
                    if (node.Tag?.ToString() == "ANGLE_MEASUREMENT_GROUP")
                    {
                        angleGroups.Add(node);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ {angleGroups.Count} Angle Measurement grubu bulundu");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetAllAngleMeasurementGroups hatası: {ex.Message}");
            }
            
            return angleGroups;
        }
    }
}
    

