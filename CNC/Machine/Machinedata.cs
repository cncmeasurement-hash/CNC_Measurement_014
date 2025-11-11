using System;

namespace _014.CNC.Machine
{
    /// <summary>
    /// Makine bilgilerini saklayan veri sınıfı
    /// </summary>
    [Serializable]
    public class MachineData
    {
        /// <summary>
        /// Makine adı
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Kontrol sistemi (Heidenhain, Siemens, Fanuc, vb.)
        /// </summary>
        public string ControlSystem { get; set; }

        /// <summary>
        /// Koordinat sistemi
        /// </summary>
        public string Coordinates { get; set; }

        /// <summary>
        /// Tool numarası
        /// </summary>
        public int ToolNumber { get; set; }

        public MachineData()
        {
            MachineName = string.Empty;
            ControlSystem = string.Empty;
            Coordinates = string.Empty;
            ToolNumber = 1;
        }

        public MachineData(string machineName, string controlSystem, string coordinates, int toolNumber)
        {
            MachineName = machineName;
            ControlSystem = controlSystem;
            Coordinates = coordinates;
            ToolNumber = toolNumber;
        }
    }
}