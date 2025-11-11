using System;
using System.Collections.Generic;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 3/6: Configuration - Ayar güncelleme işlemleri
    /// </summary>
    public partial class TreeViewManager
    {
        /// <summary>
        /// ✅ YENİ: Sol panelden CNC Machine seçimini güncelle
        /// </summary>
        public void SetSelectedMachine(string machineName)
        {
            if (string.IsNullOrEmpty(machineName))
                return;

            if (machines.Contains(machineName))
            {
                SelectedMachine = machineName;
                
                // ✅ KALDIRILDI - Machine Name artık TreeView'de gösterilmiyor
                /*
                // TreeView'deki Machine node'unu güncelle
                if (machineNode != null)
                {
                    string machineLabel = "Machine Name".PadRight(15);
                    machineNode.Text = $"{machineLabel}: {SelectedMachine} ▼";
                }
                */

                System.Diagnostics.Debug.WriteLine($"✅ TreeView: Machine seçimi güncellendi: {machineName}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ TreeView: '{machineName}' makine listesinde bulunamadı");
            }
        }

        /// <summary>
        /// ✅ YENİ: Sol panelden Probe seçimini güncelle
        /// </summary>
        public void SetSelectedProbe(string probeName)
        {
            if (string.IsNullOrEmpty(probeName))
                return;

            if (probes.Contains(probeName))
            {
                SelectedProbe = probeName;
                
                // ✅ KALDIRILDI - Probe Name artık TreeView'de gösterilmiyor
                /*
                // TreeView'deki Probe node'unu güncelle
                if (probeNode != null)
                {
                    string probeLabel = "Probe Name".PadRight(15);
                    probeNode.Text = $"{probeLabel}: {SelectedProbe} ▼";
                }
                */

                System.Diagnostics.Debug.WriteLine($"✅ TreeView: Probe seçimi güncellendi: {probeName}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ TreeView: '{probeName}' probe listesinde bulunamadı");
            }
        }

        /// <summary>
        /// ✅ YENİ: Clearance Plane değerinden Z Safety'yi güncelle
        /// </summary>
        public void UpdateZSafetyFromClearancePlane(double clearancePlaneValue)
        {
            ZSafetyDistance = clearancePlaneValue;

            // ✅ KALDIRILDI - Z Safety artık TreeView'de gösterilmiyor
            /*
            // TreeView'deki Z Safety node'unu güncelle
            if (zSafetyNode != null)
            {
                string zSafetyLabel = "Z Safety".PadRight(15);
                zSafetyNode.Text = $"{zSafetyLabel}: {ZSafetyDistance} mm";
            }
            */

            // ✅ 1. EKRANDAKI TÜM GRUPLARIN MARKER'LARINI SİL
            foreach (TreeNode group in probePointsGroups)
            {
                if (group.Tag?.ToString().StartsWith("PROBE_GROUP_") == true)
                {
                    string groupTag = group.Tag.ToString();
                    int groupId = int.Parse(groupTag.Replace("PROBE_GROUP_", ""));
                    
                    var handler = selectionManager.GetPointProbingHandler(groupId);
                    if (handler != null)
                    {
                        handler.ClearAllPoints();
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("🗑️ Tüm grupların marker'ları ekrandan silindi (Clearance değişti)");

            // ✅ TÜM TOOLPATH'LERİ SİL
            toolpathManager?.ClearToolpath();

            // ✅ 2. TREEVIEW'DEKİ TÜM GRUPLARIN TÜM MARKER SATIRLARINI SİL
            int totalPointsRemoved = 0;
            foreach (TreeNode group in methodNode.Nodes)
            {
                if (group.Text.StartsWith("Probing - Point"))
                {
                    var pointNodesToRemove = new List<TreeNode>();
                    foreach (TreeNode node in group.Nodes)
                    {
                        if (node.Tag is Point3D)
                        {
                            pointNodesToRemove.Add(node);
                        }
                    }
                    
                    foreach (var node in pointNodesToRemove)
                    {
                        group.Nodes.Remove(node);
                    }
                    
                    totalPointsRemoved += pointNodesToRemove.Count;
                }
            }

            System.Diagnostics.Debug.WriteLine($"🗑️ {totalPointsRemoved} marker numarası TreeView'den silindi (Clearance değişti)");

            // ✅ 3. BOŞ GRUPLARI KONTROL ET VE SİL (TÜM GRUPLAR)
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
                System.Diagnostics.Debug.WriteLine($"🗑️ Boş grup silindi (Clearance değişti): {group.Text}");
            }

            // activeProbeGroup'u güncelle
            if (activeProbeGroup != null && groupsToRemove.Contains(activeProbeGroup))
            {
                activeProbeGroup = null;
            }

            System.Diagnostics.Debug.WriteLine($"✅ Toplam {groupsToRemove.Count} boş grup silindi");

            // ✅ 4. POINT EKLE MODUNU KAPAT
            selectionManager?.DisablePointProbing();

            // Event fırlat (ClearancePlaneManager için)
            OnZSafetyChanged?.Invoke(this, ZSafetyDistance);

            System.Diagnostics.Debug.WriteLine($"✅ TreeView: Z Safety Clearance Plane'den güncellendi: {ZSafetyDistance:F2} mm");
        }

        /// <summary>
        /// ✅ YENİ: Üst paneldeki Retract TextBox'ından TreeView Retract'i güncelle
        /// </summary>
        public void UpdateRetractFromTextBox(int retractValue)
        {
            // Retract değeri 1-20 mm arasında olmalı
            if (retractValue < 1 || retractValue > 20)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Geçersiz Retract değeri: {retractValue} (1-20 mm arası olmalı)");
                return;
            }

            RetractDistance = retractValue;

            // ✅ KALDIRILDI - Retract artık TreeView'de gösterilmiyor
            /*
            // TreeView'deki Retract node'unu güncelle
            if (retractNode != null)
            {
                string retractLabel = "Retract".PadRight(15);
                retractNode.Text = $"{retractLabel}: {RetractDistance} mm";
            }
            */

            // ✅ 1. POINT MODUNDAN ÇIK
            selectionManager?.DisablePointProbing();

            // ✅ 2. TÜM MARKER'LARI EKRANDAN SİL
            var handler = selectionManager?.GetPointProbingHandler();
            if (handler != null)
            {
                handler.ClearAllPoints();
                System.Diagnostics.Debug.WriteLine("🗑️ Tüm marker'lar ekrandan silindi (Retract değişti)");
            }

            // ✅ TÜM TOOLPATH'LERİ SİL
            toolpathManager?.ClearToolpath();

            // ✅ 3. TÜM MARKER NUMARALARINI TREEVIEW'DEN SİL
            if (activeProbeGroup != null)
            {
                var pointNodesToRemove = new List<TreeNode>();
                foreach (TreeNode node in activeProbeGroup.Nodes)
                {
                    if (node.Tag is Point3D)
                    {
                        pointNodesToRemove.Add(node);
                    }
                }
                
                foreach (var node in pointNodesToRemove)
                {
                    activeProbeGroup.Nodes.Remove(node);
                }
                
                System.Diagnostics.Debug.WriteLine($"🗑️ {pointNodesToRemove.Count} marker numarası TreeView'den silindi");
            }

            // ✅ 4. BOŞ GRUPLARI KONTROL ET VE SİL (TÜM GRUPLAR)
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
                System.Diagnostics.Debug.WriteLine($"🗑️ Boş grup silindi (Retract değişti): {group.Text}");
            }

            // activeProbeGroup'u güncelle
            if (activeProbeGroup != null && groupsToRemove.Contains(activeProbeGroup))
            {
                activeProbeGroup = null;
            }

            System.Diagnostics.Debug.WriteLine($"✅ Toplam {groupsToRemove.Count} boş grup silindi");

            System.Diagnostics.Debug.WriteLine($"✅ TreeView: Retract TextBox'tan güncellendi: {RetractDistance} mm");
            
            // ✅ YENİ: Ridge Width modundan çık
            OnRetractChanged?.Invoke(this, EventArgs.Empty);
            System.Diagnostics.Debug.WriteLine("✅ Retract değişti - OnRetractChanged event tetiklendi");
        }
    }
}
