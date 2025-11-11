using devDept.Eyeshot.Control;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using _014.Managers.Data;
using _014.Utilities.Collision;
using _014.Analyzers.SurfaceAnal;
using _014.Handlers.Selection;

namespace _014.Managers.Selection
{
    /// <summary>
    /// Ana coordinator - Tüm alt sistemleri yönetir
    /// ✅ V2: NURBS Normal modu eklendi
    /// </summary>
    public class SelectionManager
    {
        public enum SelectionMode
        {
            None = 0,
            Face = 1,
            Entity = 2,
            Point = 3,
            NurbsNormal = 4,  // ✅ YENİ: NURBS normal modu
            PointProbing = 5  // ✅ YENİ: Point Probing modu
        }

        private Design design;
        private Form parentForm;
        private bool isSelectionEnabled = false;
        private SelectionMode currentMode = SelectionMode.None;

        // Alt sistemler
        private UIManager uiManager;
        private MarkerManager markerManager;
        private DataManager dataManager;
        private SurfaceAnalyzer surfaceAnalyzer;
        private PointSelectionHandler pointHandler;
        private FaceSelectionHandler faceHandler;
        private NurbsNormalHandler nurbsHandler;  // ✅ YENİ
        private ImportToMeshForCollision meshConverter;  // ✅ ADIM 1: Cache erişimi için
        // ✅ GRUP SİSTEMİ: Her grup için ayrı handler
        private Dictionary<int, PointProbingHandler> probingHandlers = new Dictionary<int, PointProbingHandler>();
        private int activeGroupId = -1;
        private int groupCounter = 0;
        
        // ✅ YENİ: Ridge Width Handler referansı (Toolpath için)
        private RidgeWidthHandler ridgeWidthHandler;

        /// <summary>
        /// Constructor
        /// ✅ GÜNCELLENDI: Shared DataManager parametresi eklendi + NURBS handler
        /// </summary>
        public SelectionManager(Design designControl, Form parent, DataManager sharedDataManager = null, ImportToMeshForCollision meshConv = null)
        {
            design = designControl;
            parentForm = parent;

            // ✅ SHARED DataManager kullan (FileImporter ile aynı)
            dataManager = sharedDataManager ?? new DataManager();
            
            // ✅ ADIM 1: ImportToMeshForCollision referansını sakla
            meshConverter = meshConv;

            // Alt sistemleri başlat
            uiManager = new UIManager(design, parent, dataManager);
            markerManager = new MarkerManager(design, uiManager.PointsDataTable, dataManager, uiManager.PointsGridView);
            uiManager.SetMarkerManager(markerManager);

            surfaceAnalyzer = new SurfaceAnalyzer(design, dataManager);
            uiManager.SetSurfaceAnalyzer(surfaceAnalyzer);

            pointHandler = new PointSelectionHandler(design, dataManager, markerManager, uiManager);
            faceHandler = new FaceSelectionHandler(design, dataManager, uiManager);

            // ✅ YENİ: NURBS normal handler
            nurbsHandler = new NurbsNormalHandler(design);
            
        }

        public void EnablePointSelection(bool enable)
        {
            if (enable)
            {
                currentMode = SelectionMode.Point;
                isSelectionEnabled = true;
                pointHandler.Enable(true);
                uiManager.PointsGridView.Visible = true;

                if (dataManager.GetSurfaceDataList().Count > 0)
                {
                    uiManager.SurfacesGridView.Visible = true;
                }
            }
            else
            {
                currentMode = SelectionMode.None;
                isSelectionEnabled = false;
                pointHandler.Enable(false);
                uiManager.PointsGridView.Visible = false;
                uiManager.SurfacesGridView.Visible = false;
            }
        }

        public void EnableFaceSelection(bool enable)
        {
            faceHandler.EnableFaceSelection(enable);
            currentMode = enable ? SelectionMode.Face : SelectionMode.None;
            isSelectionEnabled = enable;
        }

        public void EnableEntitySelection(bool enable)
        {
            faceHandler.EnableEntitySelection(enable);
            currentMode = enable ? SelectionMode.Entity : SelectionMode.None;
            isSelectionEnabled = enable;
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ YENİ: NURBS NORMAL MODU
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// NURBS Normal modunu aç/kapat (toggle)
        /// </summary>
        /// <returns>Mod aktif mi?</returns>
        public bool ToggleNurbsNormalMode()
        {
            if (currentMode == SelectionMode.NurbsNormal)
            {
                // Kapatılıyor
                EnableNurbsNormalMode(false);
                return false;
            }
            else
            {
                // Açılıyor
                EnableNurbsNormalMode(true);
                return true;
            }
        }

        /// <summary>
        /// NURBS Normal modunu aktif/pasif et
        /// </summary>
        public void EnableNurbsNormalMode(bool enable)
        {
            if (enable)
            {
                // Diğer modları kapat
                DisableAllModes();

                currentMode = SelectionMode.NurbsNormal;
                isSelectionEnabled = true;

                nurbsHandler.Enable(true);

                System.Diagnostics.Debug.WriteLine("✅ NURBS Normal modu AKTİF");
            }
            else
            {
                currentMode = SelectionMode.None;
                isSelectionEnabled = false;

                nurbsHandler.Enable(false);

                System.Diagnostics.Debug.WriteLine("⛔ NURBS Normal modu PASİF");
            }
        }

        /// <summary>
        /// NURBS Normal modu aktif mi kontrol et
        /// </summary>
        public bool IsNurbsNormalModeActive()
        {
            return currentMode == SelectionMode.NurbsNormal && isSelectionEnabled;
        }

        /// <summary>
        /// ✅ Direction Probe (NURBS Normal) modunu kapat
        /// </summary>
        public void DisableNurbsNormalMode()
        {
            EnableNurbsNormalMode(false);
        }

        /// <summary>
        /// ✅ Point Probing modunu kapat
        /// </summary>
        public void DisablePointProbing()
        {
            // Tüm probing handler'ları kapat
            foreach (var handler in probingHandlers.Values)
            {
                handler.Enable(false);
            }
            
            activeGroupId = -1;  // Aktif grup yok
            currentMode = SelectionMode.None;
            isSelectionEnabled = false;
            
            System.Diagnostics.Debug.WriteLine("⛔ POINT PROBING MODU PASİF");
        }

        /// <summary>
        /// Tüm modları kapat (yeni mod açılırken)
        /// </summary>
        private void DisableAllModes()
        {
            pointHandler.Enable(false);
            faceHandler.EnableFaceSelection(false);
            faceHandler.EnableEntitySelection(false);
            nurbsHandler.Enable(false);
            // ✅ Tüm aktif probing handler'ları kapat
            foreach (var handler in probingHandlers.Values)
            {
                handler.Enable(false);
            }

            uiManager.PointsGridView.Visible = false;
            uiManager.SurfacesGridView.Visible = false;
        }

        // ═══════════════════════════════════════════════════════════
        // MEVCUT METODLAR
        // ═══════════════════════════════════════════════════════════

        public void ShowSelectedFaceNormals()
        {
            surfaceAnalyzer.ShowSelectedFaceNormals();
            uiManager.PopulateSurfacesGrid();
        }

        public void ClearSurfaceLabels()
        {
            surfaceAnalyzer.ClearSurfaceLabels();
        }

        public void ShowSelectedFacesInfo()
        {
            faceHandler.ShowSelectedFacesInfo();
        }

        public void ChangeSelectedFacesColor(Color color)
        {
            faceHandler.ChangeSelectedFacesColor(color);
        }

        public void ClearPointMarkers()
        {
            markerManager.ClearPointMarkers();
        }

        public void ClearSelection()
        {
            faceHandler.ClearSelection();
        }

        public SelectionMode GetCurrentMode()
        {
            return currentMode;
        }

        public bool IsEnabled()
        {
            return isSelectionEnabled;
        }

        /// <summary>
        /// ✅ DataManager'a erişim
        /// </summary>
        public DataManager GetDataManager()
        {
            return dataManager;
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ POINT PROBING MODE
        // ═══════════════════════════════════════════════════════════

        
        /// <summary>
        /// ✅ Yeni probing grubu oluştur
        /// </summary>
        public int CreateNewProbingGroup()
        {
            groupCounter++;
            
            // Yeni handler oluştur
            PointProbingHandler newHandler = new PointProbingHandler(design, meshConverter, null, this, groupCounter);
            probingHandlers[groupCounter] = newHandler;
            
            // Eski grubu pasif yap
            if (activeGroupId > 0 && probingHandlers.ContainsKey(activeGroupId))
            {
                probingHandlers[activeGroupId].Enable(false);
            }
            
            // Yeni grubu aktif yap
            activeGroupId = groupCounter;
            newHandler.Enable(true);
            
            System.Diagnostics.Debug.WriteLine($"✅ Yeni probing grubu oluşturuldu: ID={groupCounter}");
            
            return groupCounter;
        }
        
        /// <summary>
        /// ✅ Aktif grup handler'ını al
        /// </summary>
        public PointProbingHandler GetPointProbingHandler()
        {
            if (activeGroupId > 0 && probingHandlers.ContainsKey(activeGroupId))
            {
                return probingHandlers[activeGroupId];
            }
            return null;
        }
        
        /// <summary>
        /// ✅ Belirli grup handler'ını al
        /// </summary>
        public PointProbingHandler GetPointProbingHandler(int groupId)
        {
            if (probingHandlers.ContainsKey(groupId))
            {
                return probingHandlers[groupId];
            }
            return null;
        }
        
        /// <summary>
        /// ✅ YENİ: Aktif grubu temizle (sadece aktif grup)
        /// Retract değiştiğinde kullanılır
        /// </summary>
        public void ClearActiveGroupPoints()
        {
            if (activeGroupId > 0 && probingHandlers.ContainsKey(activeGroupId))
            {
                PointProbingHandler activeHandler = probingHandlers[activeGroupId];
                activeHandler.ClearAllPoints();
                System.Diagnostics.Debug.WriteLine($"✅ Aktif grup temizlendi: ID={activeGroupId}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Aktif grup yok veya bulunamadı");
            }
        }
        
        /// <summary>
        /// Design referansını döndür (ToolpathManager için)
        /// </summary>
        public Design GetDesign()
        {
            return design;
        }
        
        /// <summary>
        /// ✅ YENİ: Ridge Width Handler'ı set et (Toolpath için)
        /// </summary>
        public void SetRidgeWidthHandler(RidgeWidthHandler handler)
        {
            ridgeWidthHandler = handler;
            System.Diagnostics.Debug.WriteLine("✅ SelectionManager: RidgeWidthHandler set edildi");
        }
        
        /// <summary>
        /// Tüm probe noktalarını al (tüm gruplardan) - ToolpathManager için
        /// Marker pozisyonlarını döndürür (temas noktası değil!)
        /// </summary>
        public List<devDept.Geometry.Point3D> GetAllProbePoints()
        {
            var allPoints = new List<devDept.Geometry.Point3D>();
            
            try
            {
                // 1️⃣ Point Probing grupları
                foreach (var kvp in probingHandlers.OrderBy(x => x.Key))
                {
                    var handler = kvp.Value;
                    var markerPositions = handler.GetMarkerPositions();  // ✅ Marker pozisyonlarını al
                    
                    if (markerPositions != null && markerPositions.Count > 0)
                    {
                        allPoints.AddRange(markerPositions);
                        System.Diagnostics.Debug.WriteLine($"  📍 Point Probing Grup {kvp.Key}: {markerPositions.Count} marker pozisyonu");
                    }
                }
                
                // 2️⃣ ✅ YENİ: Ridge Width marker'ları
                if (ridgeWidthHandler != null)
                {
                    var ridgeWidthMarkers = ridgeWidthHandler.GetMarkerPositions();
                    if (ridgeWidthMarkers != null && ridgeWidthMarkers.Count > 0)
                    {
                        allPoints.AddRange(ridgeWidthMarkers);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"📊 Toplam {allPoints.Count} marker pozisyonu toplandı");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetAllProbePoints hatası: {ex.Message}");
            }
            
            return allPoints;
        }
        
        /// <summary>
        /// Tüm normal vektörlerini al (tüm gruplardan) - ToolpathManager için
        /// </summary>
        public List<devDept.Geometry.Vector3D> GetAllNormals()
        {
            var allNormals = new List<devDept.Geometry.Vector3D>();
            
            try
            {
                // 1️⃣ Point Probing grupları
                foreach (var kvp in probingHandlers.OrderBy(x => x.Key))
                {
                    var handler = kvp.Value;
                    var normals = handler.GetNormals();
                    
                    if (normals != null && normals.Count > 0)
                    {
                        allNormals.AddRange(normals);
                        System.Diagnostics.Debug.WriteLine($"  📐 Point Probing Grup {kvp.Key}: {normals.Count} normal vektör");
                    }
                }
                
                // 2️⃣ ✅ YENİ: Ridge Width normal'leri
                if (ridgeWidthHandler != null)
                {
                    var ridgeWidthNormals = ridgeWidthHandler.GetNormals();
                    if (ridgeWidthNormals != null && ridgeWidthNormals.Count > 0)
                    {
                        allNormals.AddRange(ridgeWidthNormals);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"📊 Toplam {allNormals.Count} normal vektör toplandı");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetAllNormals hatası: {ex.Message}");
            }
            
            return allNormals;
        }
        
    }
}
