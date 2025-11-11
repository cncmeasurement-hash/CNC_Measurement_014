using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace _014.Managers.Data
{
    /// <summary>
    /// Merkezi veri yönetimi sistemi
    /// Tüm measurement gruplarını ve noktalarını saklar
    /// Thread-safe Singleton pattern kullanır
    /// JSON kaydetme/yükleme desteği (AŞAMA 3'te eklenecek)
    /// </summary>
    public sealed class MeasurementDataManager
    {
        // ═══════════════════════════════════════════════════════════
        // SINGLETON PATTERN (THREAD-SAFE)
        // ═══════════════════════════════════════════════════════════

        private static readonly object _lock = new object();
        private static MeasurementDataManager _instance = null;

        /// <summary>
        /// Singleton instance
        /// Thread-safe lazy initialization
        /// </summary>
        public static MeasurementDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MeasurementDataManager();
                            Debug.WriteLine("✅ MeasurementDataManager instance oluşturuldu (Singleton)");
                        }
                    }
                }
                return _instance;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE FIELDS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Tüm measurement gruplarını saklar
        /// </summary>
        private List<MeasurementGroup> _groups;

        /// <summary>
        /// Thread-safe operations için lock
        /// </summary>
        private readonly object _dataLock = new object();

        /// <summary>
        /// Otomatik artan grup ID counter
        /// </summary>
        private int _nextGroupId = 1;

        /// <summary>
        /// ✅ YENİ: Şu anda açık olan STEP dosyasının adı (uzantısız)
        /// Örnek: "444", "bracket", "part_01"
        /// </summary>
        private string _currentStepFileName = "AutoSave";

        /// <summary>
        /// ✅ YENİ: JSON dosya yolu - Her STEP dosyası için ayrı JSON
        /// Örnek: 444.step → 444.cncproj
        ///        bracket.step → bracket.cncproj
        /// </summary>
        private string _jsonFilePath
        {
            get
            {
                string projectsFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "014",
                    "Projects"
                );
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(projectsFolder))
                {
                    Directory.CreateDirectory(projectsFolder);
                }
                
                return Path.Combine(projectsFolder, $"{_currentStepFileName}.cncproj");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // EVENTS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Yeni grup eklendiğinde tetiklenir
        /// </summary>
        public event EventHandler<MeasurementGroup> OnGroupAdded;

        /// <summary>
        /// Yeni nokta eklendiğinde tetiklenir
        /// </summary>
        public event EventHandler<MeasurementPoint> OnPointAdded;

        /// <summary>
        /// Grup silindiğinde tetiklenir
        /// </summary>
        public event EventHandler<int> OnGroupRemoved;

        /// <summary>
        /// Herhangi bir veri değiştiğinde tetiklenir
        /// </summary>
        public event EventHandler OnDataChanged;

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR (PRIVATE - SINGLETON)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Private constructor (Singleton pattern)
        /// </summary>
        private MeasurementDataManager()
        {
            _groups = new List<MeasurementGroup>();
            Debug.WriteLine("═══════════════════════════════════════");
            Debug.WriteLine("🎯 MeasurementDataManager BAŞLATILDI");
            Debug.WriteLine("═══════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════
        // PUBLIC PROPERTIES
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Toplam grup sayısı
        /// </summary>
        public int GroupCount
        {
            get
            {
                lock (_dataLock)
                {
                    return _groups.Count(g => g.IsActive);
                }
            }
        }

        /// <summary>
        /// Toplam aktif nokta sayısı (tüm gruplarda)
        /// </summary>
        public int TotalPointCount
        {
            get
            {
                lock (_dataLock)
                {
                    return _groups
                        .Where(g => g.IsActive)
                        .Sum(g => g.ActivePointCount);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // CRUD OPERATIONS - GROUP
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Yeni bir measurement grubu ekler
        /// </summary>
        /// <param name="group">Eklenecek grup</param>
        /// <returns>Eklenen grubun ID'si</returns>
        public int AddGroup(MeasurementGroup group)
        {
            if (group == null)
            {
                Debug.WriteLine("❌ AddGroup: Grup NULL!");
                return -1;
            }

            lock (_dataLock)
            {
                try
                {
                    // Grup ID'si yoksa otomatik ata
                    if (group.GroupId <= 0)
                    {
                        group.GroupId = _nextGroupId++;
                    }
                    else
                    {
                        // Manuel ID verilmişse, next ID'yi güncelle
                        if (group.GroupId >= _nextGroupId)
                        {
                            _nextGroupId = group.GroupId + 1;
                        }
                    }

                    // Duplicate ID kontrolü
                    if (_groups.Any(g => g.GroupId == group.GroupId && g.IsActive))
                    {
                        Debug.WriteLine($"❌ AddGroup: Duplicate GroupId: {group.GroupId}");
                        return -1;
                    }

                    // Grubu ekle
                    _groups.Add(group);

                    Debug.WriteLine($"✅ Grup eklendi: ID={group.GroupId}, Name={group.GroupName}, Mode={group.MeasurementMode}");

                    // Event'leri tetikle
                    OnGroupAdded?.Invoke(this, group);
                    OnDataChanged?.Invoke(this, EventArgs.Empty);

                    return group.GroupId;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ AddGroup hatası: {ex.Message}");
                    return -1;
                }
            }
        }

        /// <summary>
        /// Grup ID'sine göre grup getirir
        /// </summary>
        /// <param name="groupId">Grup ID</param>
        /// <returns>Grup veya null</returns>
        public MeasurementGroup GetGroup(int groupId)
        {
            lock (_dataLock)
            {
                try
                {
                    var group = _groups.FirstOrDefault(g => g.GroupId == groupId && g.IsActive);
                    
                    if (group == null)
                    {
                        Debug.WriteLine($"⚠️ GetGroup: Grup bulunamadı: ID={groupId}");
                    }

                    return group;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ GetGroup hatası: {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Tüm aktif grupları getirir
        /// </summary>
        /// <returns>Aktif grupların listesi</returns>
        public List<MeasurementGroup> GetAllGroups()
        {
            lock (_dataLock)
            {
                try
                {
                    // Yeni liste oluştur (referans koruması)
                    var activeGroups = _groups.Where(g => g.IsActive).ToList();
                    Debug.WriteLine($"📊 GetAllGroups: {activeGroups.Count} aktif grup döndürüldü");
                    return activeGroups;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ GetAllGroups hatası: {ex.Message}");
                    return new List<MeasurementGroup>();
                }
            }
        }

        /// <summary>
        /// Belirli bir mod için grupları getirir
        /// </summary>
        /// <param name="measurementMode">"PointProbing", "RidgeWidth", "Angle"</param>
        /// <returns>İlgili modun grupları</returns>
        public List<MeasurementGroup> GetGroupsByMode(string measurementMode)
        {
            if (string.IsNullOrEmpty(measurementMode))
            {
                Debug.WriteLine("❌ GetGroupsByMode: MeasurementMode boş!");
                return new List<MeasurementGroup>();
            }

            lock (_dataLock)
            {
                try
                {
                    var groups = _groups
                        .Where(g => g.IsActive && g.MeasurementMode == measurementMode)
                        .ToList();

                    Debug.WriteLine($"📊 GetGroupsByMode: {groups.Count} grup bulundu (Mode: {measurementMode})");
                    return groups;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ GetGroupsByMode hatası: {ex.Message}");
                    return new List<MeasurementGroup>();
                }
            }
        }

        /// <summary>
        /// Grubu siler (soft delete)
        /// </summary>
        /// <param name="groupId">Silinecek grup ID</param>
        /// <returns>Başarılı mı?</returns>
        public bool RemoveGroup(int groupId)
        {
            lock (_dataLock)
            {
                try
                {
                    var group = _groups.FirstOrDefault(g => g.GroupId == groupId);

                    if (group == null)
                    {
                        Debug.WriteLine($"⚠️ RemoveGroup: Grup bulunamadı: ID={groupId}");
                        return false;
                    }

                    // Soft delete
                    group.IsActive = false;
                    group.LastModified = DateTime.Now;

                    // Gruptaki tüm noktaları da soft delete
                    foreach (var point in group.Points)
                    {
                        point.IsActive = false;
                    }

                    Debug.WriteLine($"🗑️ Grup silindi: ID={groupId}, Name={group.GroupName}");

                    // Event'leri tetikle
                    OnGroupRemoved?.Invoke(this, groupId);
                    OnDataChanged?.Invoke(this, EventArgs.Empty);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ RemoveGroup hatası: {ex.Message}");
                    return false;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // CRUD OPERATIONS - POINT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Gruba yeni nokta ekler
        /// </summary>
        /// <param name="groupId">Grup ID</param>
        /// <param name="point">Eklenecek nokta</param>
        /// <returns>Başarılı mı?</returns>
        public bool AddPoint(int groupId, MeasurementPoint point)
        {
            if (point == null)
            {
                Debug.WriteLine("❌ AddPoint: Nokta NULL!");
                return false;
            }

            lock (_dataLock)
            {
                try
                {
                    var group = _groups.FirstOrDefault(g => g.GroupId == groupId && g.IsActive);

                    if (group == null)
                    {
                        Debug.WriteLine($"❌ AddPoint: Grup bulunamadı: ID={groupId}");
                        return false;
                    }

                    // Noktayı gruba ekle (MeasurementGroup.AddPoint metodu kullanılır)
                    group.AddPoint(point);

                    Debug.WriteLine($"✅ Nokta eklendi: GroupId={groupId}, PointIndex={point.PointIndex}");

                    // Event'leri tetikle
                    OnPointAdded?.Invoke(this, point);
                    OnDataChanged?.Invoke(this, EventArgs.Empty);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ AddPoint hatası: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Gruptan nokta siler (soft delete)
        /// </summary>
        /// <param name="groupId">Grup ID</param>
        /// <param name="pointIndex">Nokta index</param>
        /// <returns>Başarılı mı?</returns>
        public bool RemovePoint(int groupId, int pointIndex)
        {
            lock (_dataLock)
            {
                try
                {
                    var group = _groups.FirstOrDefault(g => g.GroupId == groupId && g.IsActive);

                    if (group == null)
                    {
                        Debug.WriteLine($"❌ RemovePoint: Grup bulunamadı: ID={groupId}");
                        return false;
                    }

                    // Noktayı sil (MeasurementGroup.RemovePoint metodu kullanılır)
                    group.RemovePoint(pointIndex);

                    Debug.WriteLine($"🗑️ Nokta silindi: GroupId={groupId}, PointIndex={pointIndex}");

                    // Event'leri tetikle
                    OnDataChanged?.Invoke(this, EventArgs.Empty);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ RemovePoint hatası: {ex.Message}");
                    return false;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // DATA MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// TÜM veriyi temizler (hard delete)
        /// </summary>
        public void ClearAllData()
        {
            lock (_dataLock)
            {
                try
                {
                    int groupCount = _groups.Count;
                    int pointCount = TotalPointCount;

                    _groups.Clear();
                    _nextGroupId = 1;

                    Debug.WriteLine("═══════════════════════════════════════");
                    Debug.WriteLine("🗑️ TÜM VERİ TEMİZLENDİ");
                    Debug.WriteLine($"   Silinen grup: {groupCount}");
                    Debug.WriteLine($"   Silinen nokta: {pointCount}");
                    Debug.WriteLine("═══════════════════════════════════════");

                    // Event'i tetikle
                    OnDataChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ ClearAllData hatası: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Belirli bir modu temizler
        /// </summary>
        /// <param name="measurementMode">"PointProbing", "RidgeWidth", "Angle"</param>
        public void ClearMode(string measurementMode)
        {
            if (string.IsNullOrEmpty(measurementMode))
            {
                Debug.WriteLine("❌ ClearMode: MeasurementMode boş!");
                return;
            }

            lock (_dataLock)
            {
                try
                {
                    var groupsToRemove = _groups
                        .Where(g => g.IsActive && g.MeasurementMode == measurementMode)
                        .ToList();

                    foreach (var group in groupsToRemove)
                    {
                        RemoveGroup(group.GroupId);
                    }

                    Debug.WriteLine($"🗑️ {measurementMode} modu temizlendi: {groupsToRemove.Count} grup silindi");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ ClearMode hatası: {ex.Message}");
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // STATISTICS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Veri istatistiklerini gösterir
        /// </summary>
        public void PrintStatistics()
        {
            lock (_dataLock)
            {
                try
                {
                    Debug.WriteLine("═══════════════════════════════════════");
                    Debug.WriteLine("📊 MEASUREMENT DATA MANAGER İSTATİSTİKLER");
                    Debug.WriteLine("═══════════════════════════════════════");
                    Debug.WriteLine($"Toplam grup: {GroupCount}");
                    Debug.WriteLine($"Toplam nokta: {TotalPointCount}");

                    var pointProbingCount = _groups.Count(g => g.IsActive && g.MeasurementMode == "PointProbing");
                    var ridgeWidthCount = _groups.Count(g => g.IsActive && g.MeasurementMode == "RidgeWidth");
                    var angleCount = _groups.Count(g => g.IsActive && g.MeasurementMode == "Angle");

                    Debug.WriteLine($"Point Probing: {pointProbingCount} grup");
                    Debug.WriteLine($"Ridge Width: {ridgeWidthCount} grup");
                    Debug.WriteLine($"Angle: {angleCount} grup");
                    Debug.WriteLine("═══════════════════════════════════════");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ PrintStatistics hatası: {ex.Message}");
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // JSON OPERATIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Veriyi JSON dosyasına kaydeder
        /// </summary>
        /// <param name="filePath">Dosya yolu (null ise default kullanılır)</param>
        /// <returns>Başarılı mı?</returns>
        public bool SaveToJson(string filePath = null)
        {
            lock (_dataLock)
            {
                try
                {
                    // Dosya yolunu belirle
                    string targetPath = string.IsNullOrEmpty(filePath) ? _jsonFilePath : filePath;

                    // ✅ YENİ: Eğer filePath verilmişse, _currentStepFileName'i de güncelle
                    // Böylece bundan sonra her auto-save bu dosyaya gider
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        string newFileName = Path.GetFileNameWithoutExtension(filePath);
                        _currentStepFileName = newFileName;
                        Debug.WriteLine($"✅ Aktif proje dosyası değişti: {newFileName}.cncproj");
                    }

                    Debug.WriteLine("═══════════════════════════════════════");
                    Debug.WriteLine("💾 JSON KAYDETME BAŞLIYOR...");
                    Debug.WriteLine($"   Dosya: {targetPath}");

                    // Backup oluştur (dosya mevcutsa)
                    if (File.Exists(targetPath))
                    {
                        string backupPath = targetPath + ".backup";
                        File.Copy(targetPath, backupPath, true);
                        Debug.WriteLine($"   ✅ Backup oluşturuldu: {backupPath}");
                    }

                    // JSON serialization options
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        DefaultIgnoreCondition = JsonIgnoreCondition.Never
                    };

                    // Custom converter'ları ekle
                    options.Converters.Add(new Point3DConverter());
                    options.Converters.Add(new Vector3DConverter());

                    // Serialize
                    string jsonString = JsonSerializer.Serialize(_groups, options);

                    // UTF-8 ile dosyaya yaz
                    File.WriteAllText(targetPath, jsonString, Encoding.UTF8);

                    Debug.WriteLine($"   ✅ {_groups.Count} grup kaydedildi");
                    Debug.WriteLine($"   ✅ {TotalPointCount} nokta kaydedildi");
                    Debug.WriteLine($"   ✅ Dosya boyutu: {new FileInfo(targetPath).Length / 1024.0:F2} KB");
                    Debug.WriteLine("═══════════════════════════════════════");
                    Debug.WriteLine("✅ JSON KAYDETME TAMAMLANDI!");
                    Debug.WriteLine("═══════════════════════════════════════");

                    return true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine($"❌ Dosya erişim hatası: {ex.Message}");
                    return false;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"❌ Dosya I/O hatası: {ex.Message}");
                    return false;
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"❌ JSON serialization hatası: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ SaveToJson hatası: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// JSON dosyasından veri yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu (null ise default kullanılır)</param>
        /// <returns>Başarılı mı?</returns>
        public bool LoadFromJson(string filePath = null)
        {
            lock (_dataLock)
            {
                try
                {
                    // Dosya yolunu belirle
                    string targetPath = string.IsNullOrEmpty(filePath) ? _jsonFilePath : filePath;

                    Debug.WriteLine("═══════════════════════════════════════");
                    Debug.WriteLine("📂 JSON YÜKLEME BAŞLIYOR...");
                    Debug.WriteLine($"   Dosya: {targetPath}");

                    // Dosya var mı kontrol et
                    if (!File.Exists(targetPath))
                    {
                        Debug.WriteLine($"⚠️ Dosya bulunamadı: {targetPath}");
                        Debug.WriteLine("═══════════════════════════════════════");
                        return false;
                    }

                    // Dosya boyutunu kontrol et
                    var fileInfo = new FileInfo(targetPath);
                    Debug.WriteLine($"   📊 Dosya boyutu: {fileInfo.Length / 1024.0:F2} KB");

                    // JSON'u oku
                    string jsonString = File.ReadAllText(targetPath, Encoding.UTF8);

                    // Boş dosya kontrolü
                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        Debug.WriteLine("⚠️ JSON dosyası boş!");
                        Debug.WriteLine("═══════════════════════════════════════");
                        return false;
                    }

                    // JSON deserialization options
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.Never
                    };

                    // Custom converter'ları ekle
                    options.Converters.Add(new Point3DConverter());
                    options.Converters.Add(new Vector3DConverter());

                    // Deserialize
                    var loadedGroups = JsonSerializer.Deserialize<List<MeasurementGroup>>(jsonString, options);

                    if (loadedGroups == null)
                    {
                        Debug.WriteLine("❌ JSON deserialize edilemedi (null result)");
                        Debug.WriteLine("═══════════════════════════════════════");
                        return false;
                    }

                    // Mevcut veriyi temizle
                    int oldGroupCount = _groups.Count;
                    _groups.Clear();

                    // Yeni veriyi yükle
                    _groups = loadedGroups;

                    // Next ID'yi güncelle
                    if (_groups.Count > 0)
                    {
                        _nextGroupId = _groups.Max(g => g.GroupId) + 1;
                    }
                    else
                    {
                        _nextGroupId = 1;
                    }

                    Debug.WriteLine($"   ✅ {_groups.Count} grup yüklendi");
                    Debug.WriteLine($"   ✅ {TotalPointCount} nokta yüklendi");
                    Debug.WriteLine($"   🗑️ {oldGroupCount} eski grup temizlendi");
                    Debug.WriteLine("═══════════════════════════════════════");
                    Debug.WriteLine("✅ JSON YÜKLEME TAMAMLANDI!");
                    Debug.WriteLine("═══════════════════════════════════════");

                    // Event tetikle
                    OnDataChanged?.Invoke(this, EventArgs.Empty);

                    return true;
                }
                catch (FileNotFoundException ex)
                {
                    Debug.WriteLine($"❌ Dosya bulunamadı: {ex.Message}");
                    return false;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine($"❌ Dosya erişim hatası: {ex.Message}");
                    return false;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"❌ Dosya I/O hatası: {ex.Message}");
                    return false;
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"❌ JSON deserialization hatası: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ LoadFromJson hatası: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Mevcut JSON dosya yolunu döndürür
        /// </summary>
        public string GetJsonFilePath()
        {
            lock (_dataLock)
            {
                return _jsonFilePath;
            }
        }

        /// <summary>
        /// ✅ YENİ: Açılan STEP dosyasına göre JSON dosya adını ayarlar
        /// Her STEP dosyası için ayrı JSON oluşturur
        /// </summary>
        /// <param name="stepFilePath">STEP dosyasının tam yolu</param>
        public void SetCurrentStepFile(string stepFilePath)
        {
            lock (_dataLock)
            {
                try
                {
                    // ✅ 1. ESKİ DOSYAYI KAYDET (varsa)
                    if (!string.IsNullOrEmpty(_currentStepFileName) && _groups.Count > 0)
                    {
                        Debug.WriteLine($"💾 Eski dosya kaydediliyor: {_currentStepFileName}.cncproj");
                        SaveToJson(); // Eski dosyaya kaydet
                    }

                    // ✅ 2. YENİ DOSYA ADINI AYARLA
                    if (string.IsNullOrEmpty(stepFilePath))
                    {
                        _currentStepFileName = "AutoSave";
                        Debug.WriteLine("⚠️ STEP dosya yolu boş, AutoSave kullanılıyor");
                    }
                    else
                    {
                        // Dosya adını al (uzantısız)
                        _currentStepFileName = Path.GetFileNameWithoutExtension(stepFilePath);
                        Debug.WriteLine($"✅ Yeni STEP dosyası: {_currentStepFileName}.step");
                    }

                    // ✅ 3. ESKİ VERİYİ TEMİZLE
                    _groups.Clear();
                    _nextGroupId = 1;
                    Debug.WriteLine("🗑️ Eski measurement verileri temizlendi");

                    // ✅ 4. ESKİ JSON DOSYASINI SİL (varsa)
                    string newJsonPath = _jsonFilePath;
                    if (File.Exists(newJsonPath))
                    {
                        File.Delete(newJsonPath);
                        Debug.WriteLine($"🗑️ Eski measurement dosyası silindi: {newJsonPath}");
                        Debug.WriteLine($"📝 Yeni JSON dosyası oluşturulacak: {newJsonPath}");
                    }
                    else
                    {
                        Debug.WriteLine($"📝 Yeni JSON dosyası oluşturulacak: {newJsonPath}");
                    }

                    Debug.WriteLine("═══════════════════════════════════════");
                    Debug.WriteLine($"✅ STEP DOSYASI DEĞİŞTİ");
                    Debug.WriteLine($"   STEP: {_currentStepFileName}.step");
                    Debug.WriteLine($"   JSON: {_currentStepFileName}.cncproj");
                    Debug.WriteLine("═══════════════════════════════════════");

                    // Event tetikle
                    OnDataChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ SetCurrentStepFile hatası: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// ✅ YENİ: Şu anda açık olan STEP dosya adını döndürür
        /// </summary>
        public string GetCurrentStepFileName()
        {
            lock (_dataLock)
            {
                return _currentStepFileName;
            }
        }
    }
}
