using _014;
using devDept;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
namespace _014
{
    public partial class CNC_Measurement : Form
    {
        private void surfaceToSurfaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isSurfaceToSurfaceActive)
            {
                surfaceToSurfaceMeasurement.Enable();
                isSurfaceToSurfaceActive = true;
                surfaceToSurfaceToolStripMenuItem.Checked = true;
            }
            else
            {
                surfaceToSurfaceMeasurement.Disable();
                isSurfaceToSurfaceActive = false;
                surfaceToSurfaceToolStripMenuItem.Checked = false;
            }
        }

        private void CreateMeasurementLayer()
        {
            // Layer zaten varsa çıkış
            if (design1.Layers.Contains(MEASUREMENT_LAYER_NAME))
                return;

            // ✅ Yeni layer oluştur
            var measurementLayer = new devDept.Eyeshot.Layer(
                MEASUREMENT_LAYER_NAME,           // Layer adı
                System.Drawing.Color.Red,         // Kırmızı renk
                true                               // Visible
            );

            // ✅ Layer özelliklerini ayarla
            measurementLayer.LineWeight = 2.0f;   // Çizgi kalınlığı = 2
            measurementLayer.Visible = true;       // Görünür
            measurementLayer.Locked = false;       // Kilitli değil

            // ✅ Layer'ı design'a ekle
            design1.Layers.Add(measurementLayer);

            Debug.WriteLine($"✅ '{MEASUREMENT_LAYER_NAME}' layer'ı oluşturuldu (LineWeight=2.0)");
        }

        private void faceToFaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (faceToFaceManager == null)
                return;

            if (!faceToFaceManager.IsActive)
            {
                // Modu aktif et
                faceToFaceManager.Enable(instructionPanel);
                faceToFaceToolStripMenuItem.Checked = true;
                Debug.WriteLine("✅ Face to Face AKTIF");
            }
            else
            {
                // Modu pasif et
                faceToFaceManager.Disable();
                faceToFaceToolStripMenuItem.Checked = false;
                Debug.WriteLine("❌ Face to Face PASİF");
            }
        }

        private void edgeToEdgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (edgeToEdgeManager == null)
                return;

            if (!edgeToEdgeManager.IsActive)
            {
                // Modu aktif et
                edgeToEdgeManager.Enable(instructionPanel);
                edgeToEdgeToolStripMenuItem.Checked = true;
                Debug.WriteLine("✅ Edge to Edge AKTIF");
            }
            else
            {
                // Modu pasif et
                edgeToEdgeManager.Disable();
                edgeToEdgeToolStripMenuItem.Checked = false;
                Debug.WriteLine("❌ Edge to Edge PASİF");
            }
        }
    }
}
