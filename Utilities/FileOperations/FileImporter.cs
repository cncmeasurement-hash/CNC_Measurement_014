using _014.Analyzers.Data;
using _014.Managers.Data;
using _014.Utilities.Collision;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Translators;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _014.Utilities.FileOperations
{
    /// <summary>
    /// CAD dosyalarını (STEP, IGES) içe aktarmak için yardımcı sınıf.
    /// ✅ V3: Refactored - Sadece Brep → Surface dönüşümü otomatik
    /// ❌ Eksen analizi MANUEL (Measurement menüsünden)
    /// </summary>
    public class FileImporter
    {
        private Design design;
        private Form parentForm;
        private SurfaceProcessor surfaceProcessor;
        private DataManager dataManager;
        private ImportToMeshForCollision importToMeshForCollision;  // ✅ YENİ: Collision için mesh cache
        private RidgeWidthHandler ridgeWidthHandler;  // ✅ YENİ: Sayaçları sıfırlamak için

        public FileImporter(Design designControl, Form parent, ImportToMeshForCollision meshConverter, DataManager dataManager = null)
        {
            design = designControl;
            parentForm = parent;
            this.dataManager = dataManager ?? new DataManager();
            surfaceProcessor = new SurfaceProcessor(design, this.dataManager);
            importToMeshForCollision = meshConverter;  // ✅ YENİ: Mesh cache manager
        }

        /// <summary>
        /// ✅ YENİ: RidgeWidthHandler referansını set et
        /// </summary>
        public void SetRidgeWidthHandler(RidgeWidthHandler handler)
        {
            ridgeWidthHandler = handler;
            System.Diagnostics.Debug.WriteLine("✅ FileImporter: RidgeWidthHandler set edildi");
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ YENİ: CLEARANCE PLANE OTOMATİK HESAPLAMA
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Import edilen modelin en üst noktasını (Z max) bulur ve 
        /// Clearance Plane değerine atar (Z max + 50mm güvenlik mesafesi)
        /// </summary>
        private double CalculateAndSetClearancePlane()
        {
            try
            {
                // parentForm'u CNC_Measurement'a cast et
                if (!(parentForm is CNC_Measurement mainForm))
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ parentForm CNC_Measurement tipinde değil!");
                    return 0;
                }

                double zMax = double.MinValue;
                int entityCount = 0;

                // Tüm entity'leri tara
                foreach (var entity in design.Entities)
                {
                    if (entity == null) continue;

                    try
                    {
                        // Entity'nin bounding box'ını al
                        var bbox = entity.BoxMax;
                        
                        if (bbox != null && bbox.Z > zMax)
                        {
                            zMax = bbox.Z;
                        }

                        entityCount++;
                    }
                    catch
                    {
                        // Bazı entity'lerde bbox hesaplanamayabilir, devam et
                        continue;
                    }
                }

                // Z max bulunamazsa varsayılan değer
                if (zMax == double.MinValue || zMax < 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Z max bulunamadı, Clearance Plane güncellenmedi");
                    return 0;
                }

                // Z max + 50mm güvenlik mesafesi
                double clearancePlane = zMax + 50;

                // Ana forma yaz
                mainForm.txt_form1_Clerance.Text = Math.Round(clearancePlane, 2).ToString();

                // ✅ YENİ: Minimum değeri de ayarla (kullanıcı bundan aşağı yazamasın)
                mainForm.SetMinimumClearancePlane(clearancePlane);

                System.Diagnostics.Debug.WriteLine($"✅ Clearance Plane otomatik hesaplandı:");
                System.Diagnostics.Debug.WriteLine($"   - Entity sayısı: {entityCount}");
                System.Diagnostics.Debug.WriteLine($"   - Z max: {zMax:F2} mm");
                System.Diagnostics.Debug.WriteLine($"   - Clearance Plane: {clearancePlane:F2} mm (Z max + 50)");
                
                return clearancePlane;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Clearance Plane hesaplama hatası: {ex.Message}");
                return 0;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // STEP IMPORT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// STEP dosyası içe aktarır (Senkron).
        /// ✅ Otomatik: Model yükleme + Brep → Surface dönüşümü
        /// ❌ Manuel: Eksen analizi (Measurement menüsünden)
        /// </summary>
        public void ImportSTEP(string fileName)
        {
            try
            {
                // ✅ YENİ: Her STEP dosyası için ayrı JSON oluştur
                MeasurementDataManager.Instance.SetCurrentStepFile(fileName);
                
                var startTime = DateTime.Now;

                ReadSTEP reader = new ReadSTEP(fileName);
                reader.DoWork();
                reader.AddTo(design);

                design.ZoomFit();
                design.Invalidate();

                var duration = (DateTime.Now - startTime).TotalSeconds;

                // ✅ Surface Processing (Sadece Brep → Surface)
                // NOT: Eksen analizi artık MANUEL (Measurement menüsünden)
                surfaceProcessor.ProcessImportedModel(fileName);

                // ✅ YENİ: Surface'leri Mesh'e çevir ve cache'le (Collision için)
                importToMeshForCollision.ProcessImportedEntities();

                // ✅ YENİ: Clearance Plane otomatik hesapla
                double clearancePlane = CalculateAndSetClearancePlane();


                // ✅ YENİ: Otomatik Yüzey Analizi + JSON Kayıt
                PerformAutomaticSurfaceAnalysis(fileName, clearancePlane);
                
                // ✅ YENİ: Ridge Width sayaçlarını sıfırla
                ridgeWidthHandler?.ResetAllAxisCounters();
                
                // ✅ YENİ: MeasurementDataManager'a dosya adını bildir (Her STEP için ayrı JSON)
                MeasurementDataManager.Instance.SetCurrentStepFile(fileName);
                
                System.Diagnostics.Debug.WriteLine($"✅ STEP yükleme tamamlandı: {duration:F2}s");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"STEP yükleme hatası!\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// STEP dosyası içe aktarır (Async - UI donmaz).
        /// ✅ Otomatik: Model yükleme + Brep → Surface dönüşümü
        /// ❌ Manuel: Eksen analizi (Measurement menüsünden)
        /// </summary>
        public void ImportSTEPAsync(string fileName)
        {
            ImportAsync(fileName, "STEP", () =>
            {
                ReadSTEP reader = new ReadSTEP(fileName);
                reader.DoWork();
                return reader;
            });
        }

        // ═══════════════════════════════════════════════════════════
        // IGES IMPORT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// IGES dosyası içe aktarır (Senkron).
        /// ✅ Otomatik: Model yükleme + Brep → Surface dönüşümü
        /// ❌ Manuel: Eksen analizi (Measurement menüsünden)
        /// </summary>
        public void ImportIGES(string fileName)
        {
            try
            {
                // ✅ YENİ: Her IGES dosyası için ayrı JSON oluştur
                MeasurementDataManager.Instance.SetCurrentStepFile(fileName);
                
                var startTime = DateTime.Now;

                ReadIGES reader = new ReadIGES(fileName);
                reader.DoWork();
                reader.AddTo(design);

                design.ZoomFit();
                design.Invalidate();

                var duration = (DateTime.Now - startTime).TotalSeconds;

                // ✅ Surface Processing (Sadece Brep → Surface)
                // NOT: Eksen analizi artık MANUEL (Measurement menüsünden)
                surfaceProcessor.ProcessImportedModel(fileName);

                // ✅ YENİ: Surface'leri Mesh'e çevir ve cache'le (Collision için)
                importToMeshForCollision.ProcessImportedEntities();

                // ✅ YENİ: Clearance Plane otomatik hesapla
                double clearancePlane = CalculateAndSetClearancePlane();

                // ✅ YENİ: Otomatik Yüzey Analizi + JSON Kayıt
                PerformAutomaticSurfaceAnalysis(fileName, clearancePlane);
                
                // ✅ YENİ: Ridge Width sayaçlarını sıfırla
                ridgeWidthHandler?.ResetAllAxisCounters();
                
                // ✅ YENİ: MeasurementDataManager'a dosya adını bildir (Her IGES için ayrı JSON)
                MeasurementDataManager.Instance.SetCurrentStepFile(fileName);
                
                System.Diagnostics.Debug.WriteLine($"✅ IGES yükleme tamamlandı: {duration:F2}s");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"IGES yükleme hatası!\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// IGES dosyası içe aktarır (Async - UI donmaz).
        /// ✅ Otomatik: Model yükleme + Brep → Surface dönüşümü
        /// ❌ Manuel: Eksen analizi (Measurement menüsünden)
        /// </summary>
        public void ImportIGESAsync(string fileName)
        {
            ImportAsync(fileName, "IGES", () =>
            {
                ReadIGES reader = new ReadIGES(fileName);
                reader.DoWork();
                return reader;
            });
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPER METHOD
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Progress penceresi gösterir ve dosyayı async olarak yükler.
        /// ✅ Otomatik: Model yükleme + Brep → Surface dönüşümü
        /// ❌ Manuel: Eksen analizi (Measurement menüsünden)
        /// </summary>
        private void ImportAsync(string fileName, string fileType, Func<dynamic> readerFunc)
        {
            // Dosya boyutunu kontrol et
            FileInfo fileInfo = new FileInfo(fileName);
            double fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);

            // Büyük dosya uyarısı
            if (fileSizeMB > 50)
            {
                var result = MessageBox.Show(
                    $"UYARI: Dosya çok büyük ({fileSizeMB:F1} MB)!\n\n" +
                    $"Yükleme uzun sürebilir.\n" +
                    $"Devam etmek istiyor musunuz?",
                    "Büyük Dosya Uyarısı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result != DialogResult.Yes)
                    return;
            }

            // Progress form oluştur
            var progressForm = new Form();
            progressForm.Text = $"{fileType} Yükleniyor...";
            progressForm.Size = new Size(400, 150);
            progressForm.StartPosition = FormStartPosition.CenterScreen;
            progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            progressForm.ControlBox = false;

            var label = new Label();
            label.Text = $"{fileType} dosyası yükleniyor...\nLütfen bekleyin...";
            label.AutoSize = false;
            label.Size = new Size(360, 60);
            label.Location = new Point(20, 20);
            label.TextAlign = ContentAlignment.MiddleCenter;

            var cancelButton = new Button();
            cancelButton.Text = "İptal";
            cancelButton.Size = new Size(100, 30);
            cancelButton.Location = new Point(150, 80);

            bool cancelled = false;
            cancelButton.Click += (s, ev) =>
            {
                cancelled = true;
                progressForm.Close();
            };

            progressForm.Controls.Add(label);
            progressForm.Controls.Add(cancelButton);

            // Background thread'de yükle
            Task.Run(() =>
            {
                try
                {
                    if (cancelled) return;

                    var startTime = DateTime.Now;

                    // Dosyayı oku
                    dynamic reader = readerFunc();

                    if (cancelled) return;

                    var duration = (DateTime.Now - startTime).TotalSeconds;

                    // UI thread'de sahneye ekle
                    parentForm.Invoke(new Action(() =>
                    {
                        try
                        {
                            reader.AddTo(design);
                            design.ZoomFit();
                            design.Invalidate();

                            progressForm.Close();

                            // ✅ YENİ: Her dosya için ayrı JSON oluştur
                            MeasurementDataManager.Instance.SetCurrentStepFile(fileName);

                            // ✅ Surface Processing (Sadece Brep → Surface)
                            // NOT: Eksen analizi artık MANUEL (Measurement menüsünden)
                            surfaceProcessor.ProcessImportedModel(fileName);

                            // ✅ YENİ: Surface'leri Mesh'e çevir ve cache'le (Collision için)
                            importToMeshForCollision.ProcessImportedEntities();

                            // ✅ YENİ: Clearance Plane otomatik hesapla
                            double clearancePlane = CalculateAndSetClearancePlane();

                            
                            // ✅ YENİ: Ridge Width sayaçlarını sıfırla
                            ridgeWidthHandler?.ResetAllAxisCounters();
                            
                            // ✅ YENİ: MeasurementDataManager'a dosya adını bildir (Her dosya için ayrı JSON)
                            MeasurementDataManager.Instance.SetCurrentStepFile(fileName);
                            
                            System.Diagnostics.Debug.WriteLine($"✅ {fileType} async yükleme tamamlandı: {duration:F2}s");
                        }
                        catch (Exception ex)
                        {
                            progressForm.Close();
                            MessageBox.Show($"Sahneye eklenirken hata: {ex.Message}",
                                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));
                }
                catch (Exception ex)
                {
                    if (!cancelled)
                    {
                        parentForm.Invoke(new Action(() =>
                        {
                            progressForm.Close();
                            MessageBox.Show($"{fileType} yükleme hatası: {ex.Message}",
                                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                }
            });

            // Progress formu göster
            progressForm.Show();
        }

        /// <summary>
        /// Import sonrası otomatik yüzey analizi ve JSON kayıt
        /// </summary>
        private void PerformAutomaticSurfaceAnalysis(string fileName, double clearancePlane)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🔄 OTOMATİK YÜZEY ANALİZİ BAŞLIYOR...");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                
                // SurfaceAnalyzer oluştur ve analiz yap
                // STEP dosya adını al (uzantısız)
                string stepFileName = Path.GetFileNameWithoutExtension(fileName);

                SurfaceAnalyzer analyzer = new SurfaceAnalyzer(design, dataManager);
                analyzer.AnalyzePlanarSurfaces(stepFileName, clearancePlane);
                
                System.Diagnostics.Debug.WriteLine("✅ Otomatik yüzey analizi tamamlandı!");
                System.Diagnostics.Debug.WriteLine("💾 JSON Desktop'a kaydedildi!");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Otomatik analiz hatası: {ex.Message}");
            }
        }
    }
}
