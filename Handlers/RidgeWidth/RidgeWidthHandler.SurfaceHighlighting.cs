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
    /// RidgeWidthHandler - Surface Highlighting
    /// Yüzey vurgulama, renk değiştirme, restore işlemleri
    /// </summary>
    public partial class RidgeWidthHandler
    {
        private void HighlightVerticalSurfaces()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🎨 Ridge Width: Dikey yüzeyler sarıya çevriliyor...");

                // Dikey yüzeyleri al (X+, X-, Y+, Y-)
                var allSurfaces = dataManager.GetSurfaceDataList();
                
                if (allSurfaces == null || allSurfaces.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ RAM'de yüzey verisi yok! JSON'dan yükleyin.");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }

                var verticalSurfaces = allSurfaces
                    .Where(s => 
                    {
                        // İlk filtre: Sadece X ve Y yönleri (Z yönü hariç!)
                        if (!(s.SurfaceType == "RIGHT (X+)" ||
                              s.SurfaceType == "LEFT (X-)" ||
                              s.SurfaceType == "FRONT (Y+)" ||
                              s.SurfaceType == "BACK (Y-)"))
                            return false;
                        
                        // ✅ İkinci filtre: TAM DİK KONTROLÜ (Eğik yüzeyler hariç!)
                        double absX = Math.Abs(s.Normal.X);
                        double absY = Math.Abs(s.Normal.Y);
                        double absZ = Math.Abs(s.Normal.Z);
                        
                        // X ekseni tam dik mi?
                        bool isVerticalX = absX > 0.95 && absY < 0.15 && absZ < 0.15;
                        
                        // Y ekseni tam dik mi?
                        bool isVerticalY = absY > 0.95 && absX < 0.15 && absZ < 0.15;
                        
                        // TAM DİK ise dahil et
                        return isVerticalX || isVerticalY;
                    })
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"📊 Toplam yüzey sayısı: {allSurfaces.Count}");
                
                // Eğik yüzey sayısını hesapla
                int totalVerticalByType = allSurfaces.Count(s => 
                    s.SurfaceType == "RIGHT (X+)" ||
                    s.SurfaceType == "LEFT (X-)" ||
                    s.SurfaceType == "FRONT (Y+)" ||
                    s.SurfaceType == "BACK (Y-)");
                
                int filteredCount = totalVerticalByType - verticalSurfaces.Count;
                
                System.Diagnostics.Debug.WriteLine($"📊 X/Y yönündeki yüzey sayısı: {totalVerticalByType}");
                System.Diagnostics.Debug.WriteLine($"📊 TAM DİK yüzey sayısı: {verticalSurfaces.Count}");
                if (filteredCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Eğik yüzey sayısı (filtrelendi): {filteredCount}");
                }

                // Her entity'yi sarıya çevir (ColorMethod ile)
                int selectedCount = 0;
                foreach (var surface in verticalSurfaces)
                {
                    if (surface.EntityIndex >= 0 && surface.EntityIndex < design.Entities.Count)
                    {
                        var entity = design.Entities[surface.EntityIndex];
                        
                        // ✅ Orijinal rengi kaydet
                        if (!originalColors.ContainsKey(surface.EntityIndex))
                        {
                            originalColors[surface.EntityIndex] = entity.Color;
                        }
                        
                        // ✅ ColorMethod ile sarıya çevir (Selected kullanma!)
                        entity.ColorMethod = colorMethodType.byEntity;
                        entity.Color = Color.Yellow;
                        
                        selectedCount++;
                        System.Diagnostics.Debug.WriteLine($"  ✅ {surface.Name} ({surface.SurfaceType}) sarıya çevrildi - Entity[{surface.EntityIndex}]");
                    }
                }

                design.Invalidate();

                System.Diagnostics.Debug.WriteLine($"✅ {selectedCount} dikey yüzey sarıya çevrildi!");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ HighlightVerticalSurfaces hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
        }

        private void RestoreNonOppositeVerticalSurfaces(Vector3D selectedNormal)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🔄 Karşı olmayan yüzeyler orijinal renge döndürülüyor...");
                System.Diagnostics.Debug.WriteLine($"   İlk seçilen normal: ({selectedNormal.X:F3}, {selectedNormal.Y:F3}, {selectedNormal.Z:F3})");
                
                // 1. Ters normal hesapla
                Vector3D oppositeNormal = new Vector3D(
                    -selectedNormal.X,
                    -selectedNormal.Y,
                    -selectedNormal.Z
                );
                
                System.Diagnostics.Debug.WriteLine($"   Ters normal: ({oppositeNormal.X:F3}, {oppositeNormal.Y:F3}, {oppositeNormal.Z:F3})");
                
                // 2. Dikey yüzeyleri al
                var allSurfaces = dataManager.GetSurfaceDataList();
                var verticalSurfaces = allSurfaces
                    .Where(s => s.Group == "Dik")
                    .ToList();
                
                System.Diagnostics.Debug.WriteLine($"   Toplam dikey yüzey: {verticalSurfaces.Count}");
                
                const double TOLERANCE = 0.01;
                int restoredCount = 0;
                int oppositeCount = 0;
                int firstSelectedCount = 0;
                
                // 3. Her yüzeyi kontrol et
                foreach (var surface in verticalSurfaces)
                {
                    if (surface.EntityIndex < 0 || surface.EntityIndex >= design.Entities.Count)
                        continue;
                    
                    Entity entity = design.Entities[surface.EntityIndex];
                    
                    // Normal vektörü
                    Vector3D surfaceNormal = new Vector3D(
                        surface.Normal.X,
                        surface.Normal.Y,
                        surface.Normal.Z
                    );
                    
                    // Karşı yüzey mi? (tolerance: 0.01)
                    bool isOpposite = Math.Abs(surfaceNormal.X - oppositeNormal.X) < TOLERANCE &&
                                      Math.Abs(surfaceNormal.Y - oppositeNormal.Y) < TOLERANCE &&
                                      Math.Abs(surfaceNormal.Z - oppositeNormal.Z) < TOLERANCE;
                    
                    // İlk seçilen yüzey mi? → EntityIndex ile karşılaştır (Normal vektör DEĞİL!)
                    bool isFirstSelected = (surface.EntityIndex == firstSelectedEntityIndex);
                    
                    if (isOpposite)
                    {
                        // Karşı yüzey → Sarı kalır
                        oppositeCount++;
                        System.Diagnostics.Debug.WriteLine($"   🟡 {surface.SurfaceType} → KARŞI YÜZEY (Sarı kalır)");
                    }
                    else if (isFirstSelected)
                    {
                        // İlk seçilen → Sarı kalır
                        firstSelectedCount++;
                        System.Diagnostics.Debug.WriteLine($"   🟡 {surface.SurfaceType} → İLK SEÇİLEN (Sarı kalır)");
                    }
                    else
                    {
                        // Diğerleri → Orijinal renge döndür
                        if (originalColors.ContainsKey(surface.EntityIndex))
                        {
                            entity.Color = originalColors[surface.EntityIndex];
                            entity.ColorMethod = colorMethodType.byEntity;
                            restoredCount++;
                            System.Diagnostics.Debug.WriteLine($"   ⚪ {surface.SurfaceType} → ORİJİNAL RENGE DÖNDÜ");
                        }
                    }
                }
                
                design.Invalidate();
                
                System.Diagnostics.Debug.WriteLine($"📊 ÖZET:");
                System.Diagnostics.Debug.WriteLine($"   - İlk seçilen: {firstSelectedCount} (Sarı)");
                System.Diagnostics.Debug.WriteLine($"   - Karşı yüzey: {oppositeCount} (Sarı)");
                System.Diagnostics.Debug.WriteLine($"   - Orijinal renge dönen: {restoredCount}");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RestoreNonOppositeVerticalSurfaces hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
        }

        private void RestoreAllVerticalSurfaces()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🔄 Tüm dikey yüzeyler orijinal renge döndürülüyor...");
                
                int restoredCount = 0;
                
                foreach (var kvp in originalColors)
                {
                    int entityIndex = kvp.Key;
                    Color originalColor = kvp.Value;
                    
                    if (entityIndex >= 0 && entityIndex < design.Entities.Count)
                    {
                        Entity entity = design.Entities[entityIndex];
                        entity.Color = originalColor;
                        entity.ColorMethod = colorMethodType.byEntity;
                        restoredCount++;
                    }
                }
                
                design.Invalidate();
                
                System.Diagnostics.Debug.WriteLine($"✅ {restoredCount} yüzey orijinal renge döndürüldü");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RestoreAllVerticalSurfaces hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
        }

        private string GetSurfaceTypeName(Vector3D normal)
        {
            const double TOLERANCE = 0.01;
            
            if (Math.Abs(normal.X - 1.0) < TOLERANCE && Math.Abs(normal.Y) < TOLERANCE && Math.Abs(normal.Z) < TOLERANCE)
                return "RIGHT (X+)";
            
            if (Math.Abs(normal.X + 1.0) < TOLERANCE && Math.Abs(normal.Y) < TOLERANCE && Math.Abs(normal.Z) < TOLERANCE)
                return "LEFT (X-)";
            
            if (Math.Abs(normal.Y - 1.0) < TOLERANCE && Math.Abs(normal.X) < TOLERANCE && Math.Abs(normal.Z) < TOLERANCE)
                return "FRONT (Y+)";
            
            if (Math.Abs(normal.Y + 1.0) < TOLERANCE && Math.Abs(normal.X) < TOLERANCE && Math.Abs(normal.Z) < TOLERANCE)
                return "BACK (Y-)";
            
            return "Unknown";
        }
    }
}
