using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _014.Analyzers.Data;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// RidgeWidthHandler - Mouse Interaction
    /// Fare tıklama ve klavye event'leri
    /// </summary>
    public partial class RidgeWidthHandler
    {
        private void Design_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isPointSelectionActive) return;
            
            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("🖱️ RIDGE WIDTH: Mouse tıklandı");
                
                // 1. Hangi entity tıklandı?
                int entityIndex = design.GetEntityUnderMouseCursor(e.Location, true);
                
                if (entityIndex == -1)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Hiçbir entity tıklanmadı");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }
                
                // 2. Entity'yi al
                Entity entity = design.Entities[entityIndex];
                System.Diagnostics.Debug.WriteLine($"📦 Entity bulundu: {entity.GetType().Name} (Index: {entityIndex})");
                
                // 3. IFace mi kontrol et (Surface, Brep, Mesh)
                if (!(entity is IFace faceEntity))
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Entity IFace değil (Marker veya başka bir şey)");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }
                
                // 4. Tıklanan noktayı bul
                Point3D clickedPoint;
                int triangleIndex;
                double distance = design.FindClosestTriangle(
                    faceEntity,
                    e.Location,
                    out clickedPoint,
                    out triangleIndex
                );
                
                if (distance == double.MaxValue || triangleIndex == -1)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Tıklanan nokta bulunamadı");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ Nokta bulundu: ({clickedPoint.X:F3}, {clickedPoint.Y:F3}, {clickedPoint.Z:F3})");
                
                // 5. DataManager'dan yüzey bilgisi al
                SurfaceData surfaceData = dataManager.GetSurfaceByEntityIndex(entityIndex);
                
                if (surfaceData == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Bu yüzey analiz edilmemiş veya bulunamadı");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    return;
                }
                
                // 3. SADECE DİK yüzeyler kabul edilir (X+, X-, Y+, Y- → Z YÖNLERİ HARİÇ!)
                if (surfaceData.Group != "Dik")
                {
                    System.Diagnostics.Debug.WriteLine("⛔ SADECE DİK YÜZEYLER SEÇİLEBİLİR!");
                    System.Diagnostics.Debug.WriteLine($"   Bu yüzey: {surfaceData.SurfaceType} ({surfaceData.Group})");
                    System.Diagnostics.Debug.WriteLine("   Lütfen sarı renkli yüzeylerden birini seçin!");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    
                    // ✅ UYARI MESAJI KALDIRILDI - Sessizce görmezden gel
                    
                    System.Diagnostics.Debug.WriteLine("❌ Z YÖNÜNDEKİ YÜZEYLER SEÇİLEMEZ!");
                    System.Diagnostics.Debug.WriteLine($"   Bu yüzey: {surfaceData.SurfaceType}");
                    System.Diagnostics.Debug.WriteLine("   Ridge Width için sadece X+, X-, Y+, Y- yönleri seçilebilir!");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    
                    // ✅ Sessizce görmezden gel (MessageBox kaldırıldı)
                    return;
                }
                
                // ✅ YENİ KONTROL: YÜZEY TAM DİK OLMALI (Eğik yüzeyler seçilemez!)
                Vector3D normal = surfaceData.Normal;
                
                // X ekseni için tam dik kontrol: X dominant, Y ve Z sıfıra yakın
                bool isVerticalX = Math.Abs(normal.X) > 0.95 && 
                                   Math.Abs(normal.Y) < 0.15 && 
                                   Math.Abs(normal.Z) < 0.15;
                
                // Y ekseni için tam dik kontrol: Y dominant, X ve Z sıfıra yakın
                bool isVerticalY = Math.Abs(normal.Y) > 0.95 && 
                                   Math.Abs(normal.X) < 0.15 && 
                                   Math.Abs(normal.Z) < 0.15;
                
                if (!isVerticalX && !isVerticalY)
                {
                    System.Diagnostics.Debug.WriteLine("❌ BU YÜZEY TAM DİK DEĞİL!");
                    System.Diagnostics.Debug.WriteLine($"   Normal vektör: ({normal.X:F3}, {normal.Y:F3}, {normal.Z:F3})");
                    System.Diagnostics.Debug.WriteLine($"   |X|={Math.Abs(normal.X):F3}, |Y|={Math.Abs(normal.Y):F3}, |Z|={Math.Abs(normal.Z):F3}");
                    System.Diagnostics.Debug.WriteLine("   Ridge Width için yüzey TAM DİK olmalıdır!");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    
                    // ✅ UYARI MESAJI KALDIRILDI - Sessizce görmezden gel
                    
                    
                    return;
                }
                
                // ✅ Dik yüzey seçildi!
                System.Diagnostics.Debug.WriteLine($"✅ DİK YÜZEY SEÇİLDİ: {surfaceData.SurfaceType}");
                System.Diagnostics.Debug.WriteLine($"   Normal: ({surfaceData.Normal.X:F3}, {surfaceData.Normal.Y:F3}, {surfaceData.Normal.Z:F3})");
                System.Diagnostics.Debug.WriteLine($"   Center: ({surfaceData.Center.X:F3}, {surfaceData.Center.Y:F3}, {surfaceData.Center.Z:F3})");
                
                // ✅ İKİNCİ NOKTA ve SONRASI KONTROLÜ: Sadece karşı yüzey seçilebilir
                if (selectedPointCount >= 1)  // İkinci tıklama ve sonrası
                {
                    // Null kontrolü
                    if (firstSelectedNormal == null)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ HATA: firstSelectedNormal null!");
                        return;
                    }
                    
                    // Karşı normal hesapla
                    Vector3D oppositeNormal = new Vector3D(
                        -firstSelectedNormal.X,
                        -firstSelectedNormal.Y,
                        -firstSelectedNormal.Z
                    );
                    
                    // Seçilen yüzeyin normal'i
                    Vector3D currentNormal = new Vector3D(
                        surfaceData.Normal.X,
                        surfaceData.Normal.Y,
                        surfaceData.Normal.Z
                    );
                    
                    // Karşı yüzey mi kontrol et
                    const double TOLERANCE = 0.01;
                    bool isOpposite = Math.Abs(currentNormal.X - oppositeNormal.X) < TOLERANCE &&
                                      Math.Abs(currentNormal.Y - oppositeNormal.Y) < TOLERANCE &&
                                      Math.Abs(currentNormal.Z - oppositeNormal.Z) < TOLERANCE;
                    
                    if (!isOpposite)
                    {
                        // Karşı yüzey değil → HATA
                        System.Diagnostics.Debug.WriteLine("⛔ SADECE KARŞI YÜZEYDEN NOKTA SEÇİLEBİLİR!");
                        System.Diagnostics.Debug.WriteLine($"   İlk seçilen: {GetSurfaceTypeName(firstSelectedNormal)}");
                        System.Diagnostics.Debug.WriteLine($"   Karşı yüzey olmalı: {GetSurfaceTypeName(oppositeNormal)}");
                        System.Diagnostics.Debug.WriteLine($"   Tıkladığınız: {surfaceData.SurfaceType}");
                        System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                        
                        // ✅ UYARI MESAJI KALDIRILDI - Sessizce görmezden gel
                        
                        
                        return;
                    }
                    
                    // Karşı yüzey → Devam et
                    System.Diagnostics.Debug.WriteLine("✅ KARŞI YÜZEY SEÇİLDİ - Nokta geçerli!");
                }
                
                selectedPointCount++;
                System.Diagnostics.Debug.WriteLine($"📊 Seçilen nokta sayısı: {selectedPointCount}/2");
                
                // ✅ Marker ekle
                Vector3D normalVector = new Vector3D(surfaceData.Normal.X, surfaceData.Normal.Y, surfaceData.Normal.Z);
                AddRidgeWidthPoint(clickedPoint, normalVector);
                
                // ✅ İlk nokta seçildiyse → Karşı olmayan yüzeyleri orijinal renge döndür
                if (selectedPointCount == 1)
                {
                    firstSelectedNormal = normalVector;
                    firstSelectedEntityIndex = entityIndex;  // ← YENİ: EntityIndex kaydet
                    RestoreNonOppositeVerticalSurfaces(normalVector);
                    System.Diagnostics.Debug.WriteLine("✅ İlk nokta seçildi - Karşı olmayan yüzeyler orijinal renge döndürüldü");
                }
                
                // ✅ İkinci nokta seçildiyse → Mesafeyi hesapla ve göster
                if (selectedPointCount == 2)
                {
                    secondSelectedNormal = normalVector;  // ✅ İkinci noktanın normal'ini kaydet
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    System.Diagnostics.Debug.WriteLine("📏 RIDGE WIDTH MESAFE HESAPLAMA");
                    
                    Point3D p1 = selectedPoints[0];
                    Point3D p2 = selectedPoints[1];
                    
                    System.Diagnostics.Debug.WriteLine($"   İlk nokta: ({p1.X:F3}, {p1.Y:F3}, {p1.Z:F3})");
                    System.Diagnostics.Debug.WriteLine($"   İkinci nokta: ({p2.X:F3}, {p2.Y:F3}, {p2.Z:F3})");
                    System.Diagnostics.Debug.WriteLine($"   Normal yönü: ({firstSelectedNormal.X:F3}, {firstSelectedNormal.Y:F3}, {firstSelectedNormal.Z:F3})");
                    
                    // Normal yönündeki mesafe (projeksiyon)
                    Vector3D diff = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
                    double ridgeWidth = Math.Abs(diff.X * firstSelectedNormal.X + 
                                                   diff.Y * firstSelectedNormal.Y + 
                                                   diff.Z * firstSelectedNormal.Z);
                    
                    System.Diagnostics.Debug.WriteLine($"   Hesaplanan mesafe: {ridgeWidth:F3} mm");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    
                    MessageBox.Show(
                        $"📏 RIDGE WIDTH ÖLÇÜMÜ\n\n" +
                        $"Normal Yönü: {GetSurfaceTypeName(firstSelectedNormal)}\n" +
                        $"Mesafe: {ridgeWidth:F3} mm",
                        "Ridge Width - Ölçüm Sonucu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    
                    // ✅ ADIM 1: TreeView'e ölçüm sonucunu ekle
                    if (currentGroupNode != null)
                    {
                        treeViewManager.AddResultToRidgeWidthGroup(currentGroupNode, ridgeWidth);
                        System.Diagnostics.Debug.WriteLine($"✅ TreeView'e ölçüm sonucu eklendi: {ridgeWidth:F3} mm");
                    }
                    
                    // ✅ YENİ: ADIM 1.5: Ekrana 3D ölçü çizgileri ekle (NORMAL'E DİK EKSENDE)
                    try
                    {
                        // Z ortası hesapla: (Z1 + Z2) / 2
                        double zMid = (p1.Z + p2.Z) / 2;
                        
                        // ✅ ADIM 1: p1'den ve p2'den dikey Z yönünde (Z1+Z2)/2 seviyesine
                        Point3D p1Mid = new Point3D(p1.X, p1.Y, zMid);
                        Point3D p2Mid = new Point3D(p2.X, p2.Y, zMid);
                        
                        // ✅ ADIM 2: Normal'e DİK ekseni bul (cross product)
                        Vector3D perpAxis;
                        
                        // Normal vektörü Z ekseni ile cross product yap
                        // Eğer normal X yönündeyse → perpAxis Y olur
                        // Eğer normal Y yönündeyse → perpAxis X olur
                        perpAxis = Vector3D.Cross(firstSelectedNormal, Vector3D.AxisZ);
                        
                        // Eğer normal zaten Z yönündeyse, X eksenini kullan
                        if (perpAxis.Length < 0.01)
                        {
                            perpAxis = Vector3D.AxisX;
                        }
                        else
                        {
                            perpAxis.Normalize();
                        }
                        
                        // ✅ ADIM 3: Parçanın maksimum boyutunu bul
                        double maxSize = 0;
                        foreach (Entity ent in design.Entities)
                        {
                            if (ent.Visible && ent.LayerName != "RidgeWidthMeasurements" && 
                                ent.LayerName != "RidgeWidthPoints" && ent.LayerName != "RidgeWidthProbe")
                            {
                                double entitySize = Math.Max(Math.Max(ent.BoxSize.X, ent.BoxSize.Y), ent.BoxSize.Z);
                                if (entitySize > maxSize)
                                {
                                    maxSize = entitySize;
                                }
                            }
                        }
                        
                        // ✅ ADIM 4: EKSEN BAZLI SAYAÇ SİSTEMİ
                        // Her eksen kendi sayacını tutar - böylece farklı eksenlerde boşluk olmaz
                        // Önce hangi eksende olduğumuzu belirleyelim, sonra o eksenin sayacını artıralım
                        
                        double targetCoordinate;
                        string activeAxis;
                        int axisCount;
                        
                        // perpAxis hangi eksende dominant?
                        if (Math.Abs(perpAxis.X) > 0.9) // X ekseni
                        {
                            xAxisCounter++;
                            axisCount = xAxisCounter;
                            activeAxis = "X";
                            targetCoordinate = (maxSize / 2.0) + (xAxisCounter * 50);
                            System.Diagnostics.Debug.WriteLine($"   📐 X ekseni: sayaç={xAxisCounter}, targetCoordinate = {maxSize/2.0:F3} + ({xAxisCounter}×50) = {targetCoordinate:F3} mm");
                        }
                        else if (Math.Abs(perpAxis.Y) > 0.9) // Y ekseni
                        {
                            yAxisCounter++;
                            axisCount = yAxisCounter;
                            activeAxis = "Y";
                            targetCoordinate = (maxSize / 2.0) + (yAxisCounter * 50);
                            System.Diagnostics.Debug.WriteLine($"   📐 Y ekseni: sayaç={yAxisCounter}, targetCoordinate = {maxSize/2.0:F3} + ({yAxisCounter}×50) = {targetCoordinate:F3} mm");
                        }
                        else if (Math.Abs(perpAxis.Z) > 0.9) // Z ekseni
                        {
                            zAxisCounter++;
                            axisCount = zAxisCounter;
                            activeAxis = "Z";
                            targetCoordinate = (maxSize / 2.0) + (zAxisCounter * 50);
                            System.Diagnostics.Debug.WriteLine($"   📐 Z ekseni: sayaç={zAxisCounter}, targetCoordinate = {maxSize/2.0:F3} + ({zAxisCounter}×50) = {targetCoordinate:F3} mm");
                        }
                        else
                        {
                            // Diagonal - en yakın ekseni seç
                            double absX = Math.Abs(perpAxis.X);
                            double absY = Math.Abs(perpAxis.Y);
                            double absZ = Math.Abs(perpAxis.Z);
                            
                            if (absX >= absY && absX >= absZ)
                            {
                                xAxisCounter++;
                                axisCount = xAxisCounter;
                                activeAxis = "X";
                                targetCoordinate = (maxSize / 2.0) + (xAxisCounter * 50);
                                System.Diagnostics.Debug.WriteLine($"   📐 X ekseni (diagonal): sayaç={xAxisCounter}, targetCoordinate = {targetCoordinate:F3} mm");
                            }
                            else if (absY >= absX && absY >= absZ)
                            {
                                yAxisCounter++;
                                axisCount = yAxisCounter;
                                activeAxis = "Y";
                                targetCoordinate = (maxSize / 2.0) + (yAxisCounter * 50);
                                System.Diagnostics.Debug.WriteLine($"   📐 Y ekseni (diagonal): sayaç={yAxisCounter}, targetCoordinate = {targetCoordinate:F3} mm");
                            }
                            else
                            {
                                zAxisCounter++;
                                axisCount = zAxisCounter;
                                activeAxis = "Z";
                                targetCoordinate = (maxSize / 2.0) + (zAxisCounter * 50);
                                System.Diagnostics.Debug.WriteLine($"   📐 Z ekseni (diagonal): sayaç={zAxisCounter}, targetCoordinate = {targetCoordinate:F3} mm");
                            }
                        }
                        
                        // ✅ ADIM 5: Yeni noktaları hesapla - MUTLAK KOORDİNAT (İki çizgi aynı seviyede bitsin)
                        Point3D new1, new2;
                        
                        if (Math.Abs(perpAxis.X) > 0.9) // X ekseni dominant
                        {
                            new1 = new Point3D(targetCoordinate, p1Mid.Y, p1Mid.Z);
                            new2 = new Point3D(targetCoordinate, p2Mid.Y, p2Mid.Z);
                            System.Diagnostics.Debug.WriteLine($"   📍 X ekseni: new1=({targetCoordinate:F3}, {p1Mid.Y:F3}, {p1Mid.Z:F3}), new2=({targetCoordinate:F3}, {p2Mid.Y:F3}, {p2Mid.Z:F3})");
                        }
                        else if (Math.Abs(perpAxis.Y) > 0.9) // Y ekseni dominant
                        {
                            new1 = new Point3D(p1Mid.X, targetCoordinate, p1Mid.Z);
                            new2 = new Point3D(p2Mid.X, targetCoordinate, p2Mid.Z);
                            System.Diagnostics.Debug.WriteLine($"   📍 Y ekseni: new1=({p1Mid.X:F3}, {targetCoordinate:F3}, {p1Mid.Z:F3}), new2=({p2Mid.X:F3}, {targetCoordinate:F3}, {p2Mid.Z:F3})");
                        }
                        else if (Math.Abs(perpAxis.Z) > 0.9) // Z ekseni dominant
                        {
                            new1 = new Point3D(p1Mid.X, p1Mid.Y, targetCoordinate);
                            new2 = new Point3D(p2Mid.X, p2Mid.Y, targetCoordinate);
                            System.Diagnostics.Debug.WriteLine($"   📍 Z ekseni: new1=({p1Mid.X:F3}, {p1Mid.Y:F3}, {targetCoordinate:F3}), new2=({p2Mid.X:F3}, {p2Mid.Y:F3}, {targetCoordinate:F3})");
                        }
                        else
                        {
                            // Diagonal ise, en yakın ekseni seç
                            double absX = Math.Abs(perpAxis.X);
                            double absY = Math.Abs(perpAxis.Y);
                            double absZ = Math.Abs(perpAxis.Z);
                            
                            if (absX >= absY && absX >= absZ)
                            {
                                new1 = new Point3D(targetCoordinate, p1Mid.Y, p1Mid.Z);
                                new2 = new Point3D(targetCoordinate, p2Mid.Y, p2Mid.Z);
                                System.Diagnostics.Debug.WriteLine($"   📍 X ekseni seçildi (diagonal): new1=({targetCoordinate:F3}, {p1Mid.Y:F3}, {p1Mid.Z:F3}), new2=({targetCoordinate:F3}, {p2Mid.Y:F3}, {p2Mid.Z:F3})");
                            }
                            else if (absY >= absX && absY >= absZ)
                            {
                                new1 = new Point3D(p1Mid.X, targetCoordinate, p1Mid.Z);
                                new2 = new Point3D(p2Mid.X, targetCoordinate, p2Mid.Z);
                                System.Diagnostics.Debug.WriteLine($"   📍 Y ekseni seçildi (diagonal): new1=({p1Mid.X:F3}, {targetCoordinate:F3}, {p1Mid.Z:F3}), new2=({p2Mid.X:F3}, {targetCoordinate:F3}, {p2Mid.Z:F3})");
                            }
                            else
                            {
                                new1 = new Point3D(p1Mid.X, p1Mid.Y, targetCoordinate);
                                new2 = new Point3D(p2Mid.X, p2Mid.Y, targetCoordinate);
                                System.Diagnostics.Debug.WriteLine($"   📍 Z ekseni seçildi (diagonal): new1=({p1Mid.X:F3}, {p1Mid.Y:F3}, {targetCoordinate:F3}), new2=({p2Mid.X:F3}, {p2Mid.Y:F3}, {targetCoordinate:F3})");
                            }
                        }
                        
                        // ✅ ÇİZGİ 1: p1'den p1Mid'e (DİKEY - Z yönünde)
                        devDept.Eyeshot.Entities.Line vertLine1 = new devDept.Eyeshot.Entities.Line(p1, p1Mid);
                        vertLine1.Color = System.Drawing.Color.White;  // ✅ BEYAZ
                        vertLine1.ColorMethod = colorMethodType.byEntity;
                        vertLine1.LayerName = "RidgeWidthMeasurements";
                        if (currentGroupNumber > 0)
                        {
                            vertLine1.EntityData = $"RidgeWidth_{currentGroupNumber}_Line";
                        }
                        design.Entities.Add(vertLine1, "RidgeWidthMeasurements");
                        
                        // ✅ ÇİZGİ 2: p1Mid'den new1'e (Normal'e DİK eksende)
                        devDept.Eyeshot.Entities.Line extLine1 = new devDept.Eyeshot.Entities.Line(p1Mid, new1);
                        extLine1.Color = System.Drawing.Color.White;  // ✅ BEYAZ
                        extLine1.ColorMethod = colorMethodType.byEntity;
                        extLine1.LayerName = "RidgeWidthMeasurements";
                        if (currentGroupNumber > 0)
                        {
                            extLine1.EntityData = $"RidgeWidth_{currentGroupNumber}_Line";
                        }
                        design.Entities.Add(extLine1, "RidgeWidthMeasurements");
                        
                        // ✅ ÇİZGİ 3: p2'den p2Mid'e (DİKEY - Z yönünde)
                        devDept.Eyeshot.Entities.Line vertLine2 = new devDept.Eyeshot.Entities.Line(p2, p2Mid);
                        vertLine2.Color = System.Drawing.Color.White;  // ✅ BEYAZ
                        vertLine2.ColorMethod = colorMethodType.byEntity;
                        vertLine2.LayerName = "RidgeWidthMeasurements";
                        if (currentGroupNumber > 0)
                        {
                            vertLine2.EntityData = $"RidgeWidth_{currentGroupNumber}_Line";
                        }
                        design.Entities.Add(vertLine2, "RidgeWidthMeasurements");
                        
                        // ✅ ÇİZGİ 4: p2Mid'den new2'ye (Normal'e DİK eksende)
                        devDept.Eyeshot.Entities.Line extLine2 = new devDept.Eyeshot.Entities.Line(p2Mid, new2);
                        extLine2.Color = System.Drawing.Color.White;  // ✅ BEYAZ
                        extLine2.ColorMethod = colorMethodType.byEntity;
                        extLine2.LayerName = "RidgeWidthMeasurements";
                        if (currentGroupNumber > 0)
                        {
                            extLine2.EntityData = $"RidgeWidth_{currentGroupNumber}_Line";
                        }
                        design.Entities.Add(extLine2, "RidgeWidthMeasurements");
                        
                        // ✅ ÇİZGİ 5: Dimension Line (new1 ile new2 arasında)
                        devDept.Eyeshot.Entities.Line dimensionLine = new devDept.Eyeshot.Entities.Line(new1, new2);
                        dimensionLine.Color = System.Drawing.Color.White;  // ✅ BEYAZ
                        dimensionLine.ColorMethod = colorMethodType.byEntity;
                        dimensionLine.LayerName = "RidgeWidthMeasurements";
                        if (currentGroupNumber > 0)
                        {
                            dimensionLine.EntityData = $"RidgeWidth_{currentGroupNumber}_Line";
                        }
                        design.Entities.Add(dimensionLine, "RidgeWidthMeasurements");
                        
                        // ✅ Text (Dimension line'ın ortasında)
                        Point3D textPosition = new Point3D(
                            (new1.X + new2.X) / 2,
                            (new1.Y + new2.Y) / 2,
                            (new1.Z + new2.Z) / 2
                        );
                        
                        devDept.Eyeshot.Entities.Text measurementText = new devDept.Eyeshot.Entities.Text(
                            textPosition,
                            $"{ridgeWidth:F3}",  // ✅ Sadece sayı, "mm" YOK!
                            10  // Font yüksekliği (mm)
                        );
                        measurementText.Color = System.Drawing.Color.White;  // ✅ BEYAZ
                        measurementText.ColorMethod = colorMethodType.byEntity;
                        measurementText.LayerName = "RidgeWidthMeasurements";
                        if (currentGroupNumber > 0)
                        {
                            measurementText.EntityData = $"RidgeWidth_{currentGroupNumber}_Text";
                        }
                        
                        // ✅ Eksen bazlı rotasyon: Y ekseninde -90° (sağa doğru), diğerlerinde +90°
                        if (activeAxis == "Y")
                        {
                           // measurementText.Rotate(-Math.PI / 2, Vector3D.AxisZ, textPosition);  // -90° (sağa doğru yatay)
                        }
                        else
                        {
                            measurementText.Rotate(Math.PI / 2, Vector3D.AxisZ, textPosition);   // +90°
                        }
                        
                        design.Entities.Add(measurementText, "RidgeWidthMeasurements");
                        
                        // ✅ Debug log: Grup tag'leri eklendi
                        if (currentGroupNumber > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"   📝 6 çizgi entity'sine grup tag'i eklendi: RidgeWidth_{currentGroupNumber}_Line");
                        }
                        
                        design.Invalidate();
                        
                        System.Diagnostics.Debug.WriteLine($"✅ 3D ölçü çizgisi eklendi: {ridgeWidth:F3} mm");
                        System.Diagnostics.Debug.WriteLine($"   Z ortası: {zMid:F3} mm");
                        System.Diagnostics.Debug.WriteLine($"   Normal: ({firstSelectedNormal.X:F3}, {firstSelectedNormal.Y:F3}, {firstSelectedNormal.Z:F3})");
                        System.Diagnostics.Debug.WriteLine($"   Dik eksen: ({perpAxis.X:F3}, {perpAxis.Y:F3}, {perpAxis.Z:F3})");
                        System.Diagnostics.Debug.WriteLine($"   MaxSize: {maxSize:F3} mm, Eksen: {activeAxis}, Sayaç: {axisCount}, Koordinat: {targetCoordinate:F3} mm");
                    }
                    catch (Exception textEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ 3D ölçü çizgisi ekleme hatası: {textEx.Message}");
                    }
                    
                    // ═══════════════════════════════════════════════════════════
                    // ═══════════════════════════════════════════════════════════
                    // ⚠️ KALDIRILD: Otomatik Toolpath Oluşturma
                    // ═══════════════════════════════════════════════════════════
                    // Artık toolpath sadece CREATE TOOLPATH butonuna basıldığında
                    // ToolpathManager tarafından oluşturulur.
                    // Ridge Width marker'ları otomatik olarak dahil edilir.
                    // ═══════════════════════════════════════════════════════════
                    
                    // ✅ ADIM 2: Ridge Width modundan çık
                    DisablePointSelection();
                    System.Diagnostics.Debug.WriteLine("✅ Ridge Width modu kapatıldı");
                    
                    // ✅ ADIM 3: Marker'ları temizle
                    foreach (var marker in pointMarkers)
                    {
                        design.Entities.Remove(marker);
                    }
                    pointMarkers.Clear();
                    selectedPoints.Clear();
                    design.Invalidate();
                    System.Diagnostics.Debug.WriteLine("✅ Marker'lar temizlendi");
                    
                    // ✅ ADIM 4: Sarı yüzeyleri orijinal renge döndür
                    RestoreAllVerticalSurfaces();
                    System.Diagnostics.Debug.WriteLine("✅ Yüzey renkleri orijinal haline döndürüldü");
                }
                
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Design_MouseDown hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            }
        }

        private void Design_KeyDown(object sender, KeyEventArgs e)
        {
            // ESC tuşu basıldı mı?
            if (e.KeyCode == Keys.Escape)
            {
                // Ridge Width modu aktifse kapat
                if (isPointSelectionActive)
                {
                    DisablePointSelection();
                    System.Diagnostics.Debug.WriteLine("⛔ ESC tuşu: Ridge Width modu kapatıldı");
                }
                return;
            }
        }
    }
}
