using System;
using System.Collections.Generic;
using System.Linq;

namespace _014.Managers.Data
{
    /// <summary>
    /// Bir measurement grubunun tüm bilgilerini içeren veri yapısı
    /// TreeView'deki bir grup node'una karşılık gelir
    /// JSON serialization için hazır
    /// </summary>
    public class MeasurementGroup
    {
        // ═══════════════════════════════════════════════════════════
        // TEMEL BİLGİLER
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Grup ID (unique identifier)
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Grup adı (örn: "Probing - Point 1", "Ridge Width 1")
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Measurement modu: "PointProbing", "RidgeWidth", "Angle"
        /// </summary>
        public string MeasurementMode { get; set; }

        /// <summary>
        /// Gruptaki tüm noktalar
        /// </summary>
        public List<MeasurementPoint> Points { get; set; }

        // ═══════════════════════════════════════════════════════════
        // PROBE BİLGİLERİ (Grup seviyesinde)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Bu grup için kullanılan probe adı
        /// </summary>
        public string ProbeName { get; set; }

        /// <summary>
        /// Bu grup için kullanılan probe çapı (mm)
        /// </summary>
        public double ProbeDiameter { get; set; }

        // ═══════════════════════════════════════════════════════════
        // CNC PARAMETRELERİ (Grup seviyesinde)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Retract mesafesi (mm)
        /// </summary>
        public double RetractDistance { get; set; }

        /// <summary>
        /// Z Safety / Clearance Plane (mm)
        /// </summary>
        public double ZSafety { get; set; }

        // ═══════════════════════════════════════════════════════════
        // RIDGE WIDTH ÖZEL BİLGİLER
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Hesaplanan Ridge Width değeri (mm)
        /// Sadece RidgeWidth modu için
        /// </summary>
        public double? CalculatedWidth { get; set; }

        // ═══════════════════════════════════════════════════════════
        // ANGLE MEASUREMENT ÖZEL BİLGİLER
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Hesaplanan açı değeri (derece)
        /// Sadece Angle modu için
        /// </summary>
        public double? CalculatedAngle { get; set; }

        // ═══════════════════════════════════════════════════════════
        // METADATA
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Grup ne zaman oluşturuldu
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Son güncelleme zamanı
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Grup aktif mi (silinmedi mi)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Ek notlar (kullanıcı girişi için)
        /// </summary>
        public string Notes { get; set; }

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Yeni bir MeasurementGroup oluşturur
        /// </summary>
        public MeasurementGroup()
        {
            Points = new List<MeasurementPoint>();
            CreatedAt = DateTime.Now;
            LastModified = DateTime.Now;
            IsActive = true;
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Gruptaki aktif nokta sayısı
        /// </summary>
        public int ActivePointCount => Points.Count(p => p.IsActive);

        /// <summary>
        /// Gruba yeni nokta ekle
        /// </summary>
        public void AddPoint(MeasurementPoint point)
        {
            if (point == null)
                return;

            // GroupId ve GroupName'i ayarla
            point.GroupId = GroupId;
            point.GroupName = GroupName;
            point.PointIndex = Points.Count;

            Points.Add(point);
            LastModified = DateTime.Now;

            System.Diagnostics.Debug.WriteLine($"✅ Gruba nokta eklendi: {GroupName} - Point {point.PointIndex}");
        }

        /// <summary>
        /// Gruptan nokta sil (soft delete)
        /// </summary>
        public void RemovePoint(int pointIndex)
        {
            if (pointIndex >= 0 && pointIndex < Points.Count)
            {
                Points[pointIndex].IsActive = false;
                LastModified = DateTime.Now;

                System.Diagnostics.Debug.WriteLine($"🗑️ Gruptan nokta silindi: {GroupName} - Point {pointIndex}");
            }
        }

        /// <summary>
        /// Tüm noktaları temizle
        /// </summary>
        public void ClearPoints()
        {
            foreach (var point in Points)
            {
                point.IsActive = false;
            }
            LastModified = DateTime.Now;

            System.Diagnostics.Debug.WriteLine($"🗑️ Gruptaki tüm noktalar temizlendi: {GroupName}");
        }

        /// <summary>
        /// Debug için string representation
        /// </summary>
        public override string ToString()
        {
            return $"{MeasurementMode} - {GroupName} - {ActivePointCount} points";
        }
    }
}
