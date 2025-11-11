using _014.Managers.Selection;
using _014.Managers.Toolpath;
using _014.Measurements.Surface;
using _014.Utilities.UI;
using _014.Handlers.AngleMeasurement;  // ✅ YENİ: AngleMeasurementManager için gerekli
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
        private void CNC_Measurement_Load(object sender, EventArgs e)
        {
            // Design control artık tamamen hazır
            // ✅ SelectionManager'a aynı dataManager'ı ver
            selectionManager = new SelectionManager(design1, this, dataManager, importToMeshForCollision);

            // ✅ TreeViewManager başlat (SelectionManager ile)
            treeViewManager = new TreeViewManager(treeView1, design1, this, selectionManager);
            
            // YENI: AngleMeasurementManager'i BURADA olustur (treeViewManager'dan SONRA!)
            angleMeasurementManager = new AngleMeasurementManager(design1, this, dataManager, treeViewManager, importToMeshForCollision);
            Debug.WriteLine("✅ AngleMeasurementManager oluşturuldu (TreeViewManager ile)");
            treeViewManager.OnGenerateGCodeClicked += TreeViewManager_OnGenerateGCodeClicked;
            treeViewManager.OnZSafetyChanged += (s, zValue) =>
            {
                System.Diagnostics.Debug.WriteLine("🔔 OnZSafetyChanged TETIKLENDI! ridgeWidthHandler null mu? " + (ridgeWidthHandler == null));
                
                clearancePlaneManager.UpdateZSafety(zValue);
                
                // ✅ YENİ: Ridge Width modundan çık ve marker'ları temizle
                if (ridgeWidthHandler != null)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Ridge Width handler bulundu, DisablePointSelection çağrılıyor...");
                    ridgeWidthHandler.DisablePointSelection();
                    System.Diagnostics.Debug.WriteLine("✅ Clearance Plane değişti - Ridge Width modu kapatıldı");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ ridgeWidthHandler NULL! Ridge Width modu kapatılamadı!");
                }
            };
            treeViewManager.OnProbeChanged += (s, e) =>
            {
                selectionManager.GetPointProbingHandler()?.ClearAllPoints();  // ✅ 3D view'den TÜM marker ve line'ları sil
                treeViewManager.ClearAllGroupsPoints();  // ✅ TreeView'den TÜM point node'larını sil
            };
            treeViewManager.OnRetractChanged += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("🔔 OnRetractChanged TETIKLENDI! ridgeWidthHandler null mu? " + (ridgeWidthHandler == null));
                
                selectionManager.GetPointProbingHandler()?.ClearAllPoints();  // ✅ 3D view'den aktif grubu temizle
                
                // ✅ YENİ: Ridge Width modundan çık ve marker'ları temizle
                if (ridgeWidthHandler != null)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Ridge Width handler bulundu, DisablePointSelection çağrılıyor...");
                    ridgeWidthHandler.DisablePointSelection();
                    System.Diagnostics.Debug.WriteLine("✅ Retract değişti - Ridge Width modu kapatıldı");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ ridgeWidthHandler NULL! Ridge Width modu kapatılamadı!");
                }
            };

            // ✅ RidgeWidthHandler başlat


            ridgeWidthHandler = new RidgeWidthHandler(treeViewManager,design1,dataManager,importToMeshForCollision,instructionPanel,selectionManager);



            System.Diagnostics.Debug.WriteLine("✅ RidgeWidthHandler başlatıldı (TreeViewManager + Design + DataManager)");
            
            // ✅ YENİ: FileImporter'a RidgeWidthHandler'ı set et
            fileImporter.SetRidgeWidthHandler(ridgeWidthHandler);
            System.Diagnostics.Debug.WriteLine("✅ FileImporter'a RidgeWidthHandler set edildi");
            
            // ✅ YENİ: SelectionManager'a RidgeWidthHandler'ı set et (Toolpath için)
            selectionManager.SetRidgeWidthHandler(ridgeWidthHandler);
            System.Diagnostics.Debug.WriteLine("✅ SelectionManager'a RidgeWidthHandler set edildi");

            // ✅ ToolpathManager başlat
            // TreeViewManager'dan toolpath node'unu al
            TreeNode toolpathNode = treeViewManager.GetToolpathNode();
            if (toolpathNode != null)
            {
                toolpathManager = new ToolpathManager(toolpathNode, selectionManager, treeViewManager);

                // Simülasyon event'lerini bağla
                treeViewManager.OnSimulateToolpathClicked += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("📢 Form1: OnSimulateToolpathClicked event tetiklendi!");
                    if (toolpathManager != null)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ toolpathManager mevcut, StartSimulation() çağrılıyor...");
                        toolpathManager.StartSimulation();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ toolpathManager NULL!");
                        MessageBox.Show("ToolpathManager başlatılmadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                treeViewManager.OnStopSimulationClicked += (s, e) =>
                {
                    if (toolpathManager != null)
                    {
                        toolpathManager.StopSimulation();
                    }
                };

                // Hız değişikliği event'ini bağla
                treeViewManager.OnSimulationSpeedChanged += (s, speed) =>
                {
                    if (toolpathManager != null)
                    {
                        toolpathManager.SetSimulationSpeed(speed);
                        System.Diagnostics.Debug.WriteLine($"📢 Form1: Simülasyon hızı değiştirildi: {speed}x");
                    }
                };
            }

            // ✅ PointProbingHandler'a TreeViewManager'ı bağla (probe diameter için)
            // ✅ Artık her grup oluşturulduğunda handler.SetTreeViewManager(this) çağrılıyor
            //             selectionManager.GetPointProbingHandler().SetTreeViewManager(treeViewManager);

            surfaceToSurfaceMeasurement = new SurfaceToSurfaceMeasurement(design1);

            // ✅ Face to Face Manager başlat
            faceToFaceManager = new FaceToFaceManager(design1, this, dataManager);

            // ✅ Edge to Edge Manager başlat
            edgeToEdgeManager = new EdgeToEdgeManager(design1, this, dataManager);



            // ✅ ViewManager menü öğelerini ayarla
            viewManager.SetMenuItems(
                wireFrameToolStripMenuItem,
                shadedToolStripMenuItem,
                renderedToolStripMenuItem,
                hiddenLineToolStripMenuItem
            );

            // ✅ CNC Makineleri yükle (Sol paneldeki dropdown için)
            LoadCNCMachines();

            // ✅ Probe'ları yükle (Sol paneldeki dropdown için)
            LoadProbes();

            // ✅ Clearance Plane varsayılan değeri
            txt_form1_Clerance.Text = "50";

            // ✅ Retract varsayılan değeri
            txt_Form1_Retract.Text = "3";

            Debug.WriteLine("✅ Form yüklendi");
            Debug.WriteLine("✅ SelectionManager başlatıldı (shared DataManager ile)");

            // ✅ YENİ: Cursor'ı normal hale getir
            Cursor.Current = Cursors.Default;
            Application.UseWaitCursor = false;

            // ✅ YENİ: Eyeshot cursor'ını sıfırla
            design1.Cursor = Cursors.Default;
            design1.ActionMode = actionType.None;
            design1.Invalidate();
            Debug.WriteLine("✅ Eyeshot cursor sıfırlandı");

            // ✅ YENİ: Menü bar'a focus ver (ilk tıklamada açılması için)
            this.ActiveControl = null;
            menuStrip1.Focus();
            Debug.WriteLine("✅ Menü bar focus ayarlandı");
        }

        private void CNC_Measurement_Shown(object sender, EventArgs e)
        {
            try
            {
                // Panel oluştur
                if (instructionPanel == null || instructionPanel.IsDisposed)
                {
                    instructionPanel = new InstructionPanel(design1);
                    
                    // ✅ YENİ: RidgeWidthHandler'a InstructionPanel'i set et
                    if (ridgeWidthHandler != null)
                    {
                        ridgeWidthHandler.SetInstructionPanel(instructionPanel);
                    }
                }

                // Panel'i göster
                instructionPanel.Show();
                instructionPanel.BringToFront();
                instructionPanel.UpdatePanel(InstructionTexts.TITLE_MAIN_MENU, InstructionTexts.WELCOME);
                instructionPanel.Owner = this;  // ← GARANTİLİ GÖSTER!

                Debug.WriteLine("✅ InstructionPanel garantili gösterildi (Shown event)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ InstructionPanel gösterim hatası: {ex.Message}");
            }
        }

        private void CNC_Measurement_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                bool anyModeWasActive = false;

                // 1. Face to Face Measurement
                if (faceToFaceManager != null && faceToFaceManager.IsActive)
                {
                    faceToFaceManager.Disable();
                    faceToFaceToolStripMenuItem.Checked = false;
                    anyModeWasActive = true;
                    Debug.WriteLine("✅ ESC → Face to Face kapatıldı");
                }

                // 1.5 Edge to Edge Measurement
                if (edgeToEdgeManager != null && edgeToEdgeManager.IsActive)
                {
                    edgeToEdgeManager.Disable();
                    edgeToEdgeToolStripMenuItem.Checked = false;
                    anyModeWasActive = true;
                    Debug.WriteLine("✅ ESC → Edge to Edge kapatıldı");
                }

                // 2. Length Measurement
                if (isLengthModeActive)
                {
                    if (lengthAnalyzer != null)
                    {
                        lengthAnalyzer.Enable(false);
                    }
                    isLengthModeActive = false;
                    lengthToolStripMenuItem.Checked = false;
                    Debug.WriteLine("🟡 Length Measurement ESC ile kapatıldı");
                    anyModeWasActive = true;
                }


                // 4. Surface to Surface Measurement
                if (isSurfaceToSurfaceActive)
                {
                    if (surfaceToSurfaceMeasurement != null)
                    {
                        surfaceToSurfaceMeasurement.Disable();
                    }
                    isSurfaceToSurfaceActive = false;
                    surfaceToSurfaceToolStripMenuItem.Checked = false;
                    Debug.WriteLine("🟡 Surface to Surface Measurement ESC ile kapatıldı");
                    anyModeWasActive = true;
                }

                // 5. Direction Probe (Nurbs Normal Mode)
                if (selectionManager != null && selectionManager.IsNurbsNormalModeActive())
                {
                    selectionManager.DisableNurbsNormalMode();
                    Debug.WriteLine("🟡 Direction Probe modu ESC ile kapatıldı");
                    anyModeWasActive = true;
                }

                // ✅ Her mod kendi welcome mesajını zaten gösteriyor
                // Bu yüzden burada tekrar yapmaya gerek yok

                if (anyModeWasActive)
                {
                    e.Handled = true; // ESC tuşunu işledik
                    Debug.WriteLine("✅ Tüm aktif modlar ESC ile kapatıldı");
                }
            }
        }
    }
}
