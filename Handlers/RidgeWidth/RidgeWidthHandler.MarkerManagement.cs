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
    /// RidgeWidthHandler - Marker Management
    /// Marker layer yönetimi
    /// </summary>
    public partial class RidgeWidthHandler
    {
        private void CreateMarkerLayer()
        {
            try
            {
                // Layer zaten var mı kontrol et
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
                    Layer markerLayer = new Layer(MARKER_LAYER_NAME);
                    markerLayer.Color = Color.Red;
                    markerLayer.Visible = true;
                    markerLayer.LineWeight = 2.0f;
                    
                    design.Layers.Add(markerLayer);
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
