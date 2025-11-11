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
        private void InitializeLayer()
        {
            try
            {
                // Layer zaten varsa çık
                bool layerExists = false;
                foreach (Layer layer in design.Layers)
                {
                    if (layer.Name == MARKER_LAYER_NAME)
                    {
                        layerExists = true;
                        break;
                    }
                }

                if (!layerExists)
                {
                    Layer probeLayer = new Layer(MARKER_LAYER_NAME);
                    probeLayer.Color = Color.Red;
                    probeLayer.Visible = true;
                    probeLayer.LineWeight = 2.0f;
                    
                    design.Layers.Add(probeLayer);
                    System.Diagnostics.Debug.WriteLine($"✅ Layer oluşturuldu: {MARKER_LAYER_NAME}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Layer oluşturma hatası: {ex.Message}");
            }
        }
    }
}
