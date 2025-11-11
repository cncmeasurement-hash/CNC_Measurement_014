using System;
using System.Collections.Generic;
using System.Linq;
using _014.CNC.Machine;
using _014.Probe.Configuration;

namespace _014
{
    /// <summary>
    /// PARTIAL CLASS 2/6: DataLoading - JSON veri yükleme işlemleri
    /// </summary>
    public partial class TreeViewManager
    {
        /// <summary>
        /// JSON dosyasından makineleri yükle
        /// </summary>
        private void LoadMachinesFromJson()
        {
            try
            {
                var machineDataList = MachineStorage.LoadFromJson();
                string previousSelection = SelectedMachine; // Önceki seçimi sakla
                machines.Clear();
                
                if (machineDataList != null && machineDataList.Count > 0)
                {
                    // Sadece makine isimlerini al
                    machines.AddRange(machineDataList.Select(m => m.MachineName));
                    
                    // Önceki seçim hala varsa onu kullan, yoksa ilk makineyi seç
                    if (!string.IsNullOrEmpty(previousSelection) && machines.Contains(previousSelection))
                    {
                        SelectedMachine = previousSelection;
                    }
                    else if (machines.Count > 0)
                    {
                        SelectedMachine = machines[0];
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ TreeView: {machines.Count} makine yüklendi (Seçili: {SelectedMachine})");
                }
                else
                {
                    // JSON boşsa default makineler ekle
                    machines.Add("Hermle C30");
                    machines.Add("DMG Mori NTX");
                    machines.Add("Mazak Integrex");
                    SelectedMachine = "Hermle C30";
                    System.Diagnostics.Debug.WriteLine("ℹ️ TreeView: Default makineler kullanılıyor");
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda default makineler
                machines.Clear();
                machines.Add("Hermle C30");
                machines.Add("DMG Mori NTX");
                machines.Add("Mazak Integrex");
                SelectedMachine = "Hermle C30";
                System.Diagnostics.Debug.WriteLine($"⚠️ TreeView makine yükleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// JSON dosyasından probe'ları yükle
        /// </summary>
        private void LoadProbesFromJson()
        {
            try
            {
                var probeDataList = ProbeStorage.LoadFromJson();
                string previousSelection = SelectedProbe; // Önceki seçimi sakla
                probes.Clear();
                
                if (probeDataList != null && probeDataList.Count > 0)
                {
                    // Sadece probe isimlerini al
                    probes.AddRange(probeDataList.Select(p => p.Name));
                    
                    // Önceki seçim hala varsa onu kullan, yoksa ilk probe'u seç
                    if (!string.IsNullOrEmpty(previousSelection) && probes.Contains(previousSelection))
                    {
                        SelectedProbe = previousSelection;
                    }
                    else if (probes.Count > 0)
                    {
                        SelectedProbe = probes[0];
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ TreeView: {probes.Count} probe yüklendi (Seçili: {SelectedProbe})");
                }
                else
                {
                    // JSON boşsa default probe'lar ekle
                    probes.Add("Renishaw TP20");
                    probes.Add("Blum TC50");
                    probes.Add("Heidenhain TS");
                    SelectedProbe = "Renishaw TP20";
                    System.Diagnostics.Debug.WriteLine("ℹ️ TreeView: Default probe'lar kullanılıyor");
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda default probe'lar
                probes.Clear();
                probes.Add("Renishaw TP20");
                probes.Add("Blum TC50");
                probes.Add("Heidenhain TS");
                SelectedProbe = "Renishaw TP20";
                System.Diagnostics.Debug.WriteLine($"⚠️ TreeView probe yükleme hatası: {ex.Message}");
            }
        }
    }
}
