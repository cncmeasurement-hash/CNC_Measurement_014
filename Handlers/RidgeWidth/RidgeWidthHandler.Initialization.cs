using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// RidgeWidthHandler - Initialization
    /// Layer başlatma işlemleri
    /// </summary>
    public partial class RidgeWidthHandler
    {
        private void InitializeProbeLayer()
        {
            try
            {
                // Layer zaten varsa çık
                bool layerExists = false;
                foreach (Layer layer in design.Layers)
                {
                    if (layer.Name == PROBE_LAYER_NAME)
                    {
                        layerExists = true;
                        break;
                    }
                }

                if (!layerExists)
                {
                    Layer probeLayer = new Layer(PROBE_LAYER_NAME);
                    probeLayer.Color = Color.White;
                    probeLayer.Visible = true;
                    probeLayer.LineWeight = 1.0f;
                    
                    design.Layers.Add(probeLayer);
                    System.Diagnostics.Debug.WriteLine($"✅ Layer oluşturuldu: {PROBE_LAYER_NAME}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Probe layer oluşturma hatası: {ex.Message}");
            }
        }

        private void EnsureMeasurementLayerExists()
        {
            try
            {
                const string MEASUREMENT_LAYER = "RidgeWidthMeasurements";
                
                // Layer zaten varsa çık
                bool layerExists = false;
                foreach (Layer layer in design.Layers)
                {
                    if (layer.Name == MEASUREMENT_LAYER)
                    {
                        layerExists = true;
                        break;
                    }
                }

                if (!layerExists)
                {
                    Layer measurementLayer = new Layer(MEASUREMENT_LAYER);
                    measurementLayer.Color = Color.Blue;  // Mavi renk
                    measurementLayer.Visible = true;
                    measurementLayer.LineWeight = 1.0f;
                    
                    design.Layers.Add(measurementLayer);
                    System.Diagnostics.Debug.WriteLine($"✅ Layer oluşturuldu: {MEASUREMENT_LAYER}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Measurement layer oluşturma hatası: {ex.Message}");
            }
        }
    }
}
