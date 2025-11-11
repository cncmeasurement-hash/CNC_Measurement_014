using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _014.Probe.Core;
using _014.Utilities.UI;
using _014.Managers.Data;  // ✅ YENİ: MeasurementDataManager için
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace _014
{
    /// <summary>
    /// RidgeWidthHandler - Point Selection
    /// Nokta seçimi, collision detection, probe visualization
    /// </summary>
    public partial class RidgeWidthHandler
    {
        public void EnablePointSelection()
        {
            if (!isPointSelectionActive)
            {
                // Layer'ı oluştur
                CreateMarkerLayer();

                isPointSelectionActive = true;
                selectedPointCount = 0;
                design.MouseDown += Design_MouseDown;

                // Design control'e focus ver (ESC tuşu hemen çalışsın)
                design.Focus();

                System.Diagnostics.Debug.WriteLine("✅ Ridge Width nokta seçimi AKTİF");
            }
        }

        private void AddRidgeWidthPoint(Point3D point, Vector3D normal)
        {
            try
            {
                // 1. Noktayı listeye ekle
                selectedPoints.Add(point);

                // ✅ YENİ: Grup bazlı saklama (Point Probing pattern'i)
                if (currentGroupNumber > 0)
                {
                    // Dictionary'leri initialize et
                    if (!groupPoints.ContainsKey(currentGroupNumber))
                    {
                        groupPoints[currentGroupNumber] = new List<Point3D>();
                        groupNormals[currentGroupNumber] = new List<Vector3D>();
                        System.Diagnostics.Debug.WriteLine($"✅ Grup {currentGroupNumber} için List'ler oluşturuldu");
                    }

                    // Point ve Normal'i grup bazlı kaydet
                    groupPoints[currentGroupNumber].Add(point);
                    groupNormals[currentGroupNumber].Add(normal);
                    System.Diagnostics.Debug.WriteLine($"✅ Grup {currentGroupNumber}: Point ve Normal kaydedildi (Toplam: {groupPoints[currentGroupNumber].Count})");
                }

                System.Diagnostics.Debug.WriteLine($"🔴 MARKER EKLENİYOR...");
                System.Diagnostics.Debug.WriteLine($"📍 Nokta: ({point.X:F3}, {point.Y:F3}, {point.Z:F3})");

                // 2. Seçili probe'un D değerini al
                ProbeData selectedProbe = treeViewManager.GetSelectedProbeData();
                if (selectedProbe == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Seçili probe bulunamadı!");
                    return;
                }

                // ✅ YENİ: Probe mesh oluştur (PointProbingHandler gibi)
                Mesh probeMesh = ProbeBuilder.CreateProbeMesh(selectedProbe);
                if (probeMesh == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ ProbeBuilder.CreateProbeMesh() null döndü!");
                    return;
                }
                System.Diagnostics.Debug.WriteLine($"✅ Probe mesh oluşturuldu (Vertex: {probeMesh.Vertices.Length})");

                // ✅ YENİ: İLK NOKTA için probe'u tıklanan noktaya ekle (BEYAZ)
                if (selectedPoints.Count == 1)  // İlk nokta
                {
                    Mesh displayProbe = (Mesh)probeMesh.Clone();
                    displayProbe.Translate(point.X, point.Y, point.Z);  // Tıklanan nokta

                    // Z- yönde D/2 kadar kaydir
                    double probeRadius = (double)selectedProbe.D / 2.0;
                    displayProbe.Translate(0, 0, -probeRadius);  // Z ekseninde -D/2

                    // Normal yönünde D*0.6 kadar kaydir
                    double offset = (double)selectedProbe.D * 0.6;
                    displayProbe.Translate(normal.X * offset, normal.Y * offset, normal.Z * offset);

                    // ✅ ÖNCE DESIGN'A EKLE (Geçici olarak - CollisionDetection için gerekli)
                    displayProbe.Visible = false;  // ✅ PROBE GÖRÜNMEZ!
                    displayProbe.Color = Color.White;
                    displayProbe.ColorMethod = colorMethodType.byEntity;
                    displayProbe.LayerName = PROBE_LAYER_NAME;
                    design.Entities.Add(displayProbe);
                    design.Invalidate();

                    // ═══════════════════════════════════════════════════════════
                    // ✅ YENİ: ÇARPIŞMA KONTROLÜ (Mevcut pozisyonda)
                    // ═══════════════════════════════════════════════════════════
                    System.Diagnostics.Debug.WriteLine("🔍 ÇARPIŞMA KONTROLÜ BAŞLADI...");

                    List<Mesh> partMeshes = meshConverter.GetMeshesForCollision();
                    System.Diagnostics.Debug.WriteLine($"📦 Kontrol edilecek mesh sayısı: {partMeshes.Count}");

                    bool hasCollision = false;
                    foreach (Mesh partMesh in partMeshes)
                    {
                        // Mesh validasyonu
                        if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                            continue;

                        try
                        {
                            // Eyeshot CollisionDetection
                            CollisionDetection cd = new CollisionDetection(
                                new Entity[] { displayProbe },  // Yerleştirilmiş probe
                                new Entity[] { partMesh },      // Parça mesh
                                null
                            );

                            cd.CheckMethod = collisionCheckType.SubdivisionTree;
                            cd.DoWork();

                            if (cd.Result != null && cd.Result.Length > 0)
                            {
                                hasCollision = true;
                                System.Diagnostics.Debug.WriteLine("💥 ÇARPIŞMA TESPİT EDİLDİ!");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Çarpışma kontrolü hatası: {ex.Message}");
                        }
                    }

                    if (hasCollision)
                    {
                        System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                        System.Diagnostics.Debug.WriteLine("⛔ ÇARPIŞMA - İŞLEM İPTAL EDİLİYOR");
                        System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                        // ✅ ÇARPIŞMA VAR - Probe'u MAVİ YAP!
                        displayProbe.Visible = true;  // ✅ ÇARPIŞMA - PROBE GÖRÜNÜR YAP!
                        displayProbe.Color = Color.Blue;
                        design.Invalidate();

                        MessageBox.Show(
                            "⚠️ ÇARPIŞMA TESPİT EDİLDİ!\n\n" +
                            "Probe parça ile çarpışıyor.\n" +
                            "Lütfen farklı bir nokta seçin.",
                            "Ridge Width - Çarpışma Uyarısı",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        // ✅ MessageBox kapandıktan SONRA - Probe'u sil
                        design.Entities.Remove(displayProbe);
                        design.Invalidate();

                        // Seçilen noktayı geri al
                        selectedPoints.RemoveAt(selectedPoints.Count - 1);
                        selectedPointCount--;

                        return;  // ❌ Marker ekleme, iptal et!
                    }

                    // ═══════════════════════════════════════════════════════════
                    // ✅ YENİ: İLK KONUMDA ÇARPIŞMA YOK - NORMAL YÖNÜNDE İLERLE
                    // ═══════════════════════════════════════════════════════════
                    System.Diagnostics.Debug.WriteLine("✅ İlk kontrol başarılı - Normal yönünde ilerleme başlıyor");

                    double retractDistance = treeViewManager.RetractDistance;
                    int stepCount = (int)retractDistance;
                    System.Diagnostics.Debug.WriteLine($"📏 Retract mesafesi: {retractDistance}mm ({stepCount} adım)");

                    for (int i = 0; i < stepCount; i++)
                    {
                        // Probe'u normal yönünde 1mm kaydir
                        displayProbe.Translate(normal.X * 1.0, normal.Y * 1.0, normal.Z * 1.0);
                        design.Invalidate();

                        System.Diagnostics.Debug.WriteLine($"🔍 Adım {i + 1}/{stepCount}: Probe 1mm kaydırıldı (Normal yönü)");

                        // ÇARPIŞMA KONTROLÜ (Adım konumunda)
                        foreach (Mesh partMesh in partMeshes)
                        {
                            // Mesh validasyonu
                            if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                continue;

                            try
                            {
                                // Eyeshot CollisionDetection
                                CollisionDetection cd = new CollisionDetection(
                                    new Entity[] { displayProbe },  // Kaydırılmış probe
                                    new Entity[] { partMesh },      // Parça mesh
                                    null
                                );

                                cd.CheckMethod = collisionCheckType.SubdivisionTree;
                                cd.DoWork();

                                if (cd.Result != null && cd.Result.Length > 0)
                                {
                                    hasCollision = true;
                                    System.Diagnostics.Debug.WriteLine($"💥 ÇARPIŞMA TESPİT EDİLDİ! (Adım {i + 1})");
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ Çarpışma kontrolü hatası (Adım {i + 1}): {ex.Message}");
                            }
                        }

                        // İç döngüde çarpışma tespit edildiyse, dış döngüden çık
                        if (hasCollision)
                            break;
                    }

                    // ═══════════════════════════════════════════════════════════
                    // Z+ YÖNÜNDEKİ ÇARPIŞMA KONTROLÜ (50-350mm)
                    // ═══════════════════════════════════════════════════════════
                    if (!hasCollision)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Retract kontrolü başarılı - Z+ yönünde kontrol başlıyor");

                        for (int zStep = 50; zStep <= 350; zStep += 50)
                        {
                            // 50mm Z+ yönünde hareket
                            displayProbe.Translate(0, 0, 50.0);
                            design.Invalidate();

                            System.Diagnostics.Debug.WriteLine($"🔍 Z+ Adım: {zStep}mm yukarı çıkıldı");

                            // ÇARPIŞMA KONTROLÜ
                            foreach (Mesh partMesh in partMeshes)
                            {
                                if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                    continue;

                                try
                                {
                                    CollisionDetection cd = new CollisionDetection(
                                        new Entity[] { displayProbe },
                                        new Entity[] { partMesh },
                                        null
                                    );

                                    cd.CheckMethod = collisionCheckType.SubdivisionTree;
                                    cd.DoWork();

                                    if (cd.Result != null && cd.Result.Length > 0)
                                    {
                                        hasCollision = true;
                                        System.Diagnostics.Debug.WriteLine($"💥 ÇARPIŞMA TESPİT EDİLDİ! (Z+ {zStep}mm)");
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"❌ Çarpışma kontrolü hatası (Z+ {zStep}mm): {ex.Message}");
                                }
                            }

                            if (hasCollision)
                                break;
                        }

                        if (!hasCollision)
                        {
                            System.Diagnostics.Debug.WriteLine("✅ Z+ kontrolü tamamlandı - Çarpışma yok");
                        }
                    }

                    // ═══════════════════════════════════════════════════════════
                    // DÖNGÜ SONRASI KONTROL
                    // ═══════════════════════════════════════════════════════════
                    if (hasCollision)
                    {
                        System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                        System.Diagnostics.Debug.WriteLine("⛔ ÇARPIŞMA - İŞLEM İPTAL EDİLİYOR (Döngü sırasında)");
                        System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                        // ✅ ÇARPIŞMA VAR - Probe'u MAVİ YAP!
                        displayProbe.Visible = true;  // ✅ ÇARPIŞMA - PROBE GÖRÜNÜR YAP!
                        displayProbe.Color = Color.Blue;
                        design.Invalidate();

                        MessageBox.Show(
                            "⚠️ ÇARPIŞMA TESPİT EDİLDİ!\n\n" +
                            "Probe parça ile çarpışıyor.\n" +
                            "Lütfen farklı bir nokta seçin.",
                            "Ridge Width - Çarpışma Uyarısı",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        // ✅ MessageBox kapandıktan SONRA - Probe'u sil
                        design.Entities.Remove(displayProbe);
                        design.Invalidate();

                        // Seçilen noktayı geri al
                        selectedPoints.RemoveAt(selectedPoints.Count - 1);
                        selectedPointCount--;

                        return;  // ❌ Marker ekleme, iptal et!
                    }

                    System.Diagnostics.Debug.WriteLine("✅ TÜM KONTROLLER TAMAM - Çarpışma yok");
                    // ═══════════════════════════════════════════════════════════

                    System.Diagnostics.Debug.WriteLine("✅ ÇARPIŞMA YOK - Probe zaten eklendi");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    // ═══════════════════════════════════════════════════════════
                    // ÇARPIŞMA KONTROLÜ BİTTİ
                    // ═══════════════════════════════════════════════════════════

                    System.Diagnostics.Debug.WriteLine($"✅ BEYAZ PROBE eklendi: Z- D/2 + Normal D*0.6 kaydırıldı");
                }

                // ═══════════════════════════════════════════════════════════
                // ✅ YENİ: İKİNCİ NOKTA için probe'u ekle ve çarpışma kontrolü yap
                // ═══════════════════════════════════════════════════════════
                else if (selectedPoints.Count == 2)  // İkinci nokta
                {
                    Mesh displayProbe = (Mesh)probeMesh.Clone();
                    displayProbe.Translate(point.X, point.Y, point.Z);  // Tıklanan nokta

                    // Z- yönde D/2 kadar kaydir
                    double probeRadius = (double)selectedProbe.D / 2.0;
                    displayProbe.Translate(0, 0, -probeRadius);  // Z ekseninde -D/2

                    // Normal yönünde D*0.6 kadar kaydir
                    double offset = (double)selectedProbe.D * 0.6;
                    displayProbe.Translate(normal.X * offset, normal.Y * offset, normal.Z * offset);

                    // ✅ ÖNCE DESIGN'A EKLE (Geçici olarak - CollisionDetection için gerekli)
                    displayProbe.Visible = false;  // ✅ PROBE GÖRÜNMEZ!
                    displayProbe.Color = Color.White;
                    displayProbe.ColorMethod = colorMethodType.byEntity;
                    displayProbe.LayerName = PROBE_LAYER_NAME;
                    design.Entities.Add(displayProbe);
                    design.Invalidate();

                    // ═══════════════════════════════════════════════════════════
                    // ✅ YENİ: ÇARPIŞMA KONTROLÜ (Mevcut pozisyonda)
                    // ═══════════════════════════════════════════════════════════
                    System.Diagnostics.Debug.WriteLine("🔍 ÇARPIŞMA KONTROLÜ BAŞLADI... (İKİNCİ NOKTA)");

                    List<Mesh> partMeshes = meshConverter.GetMeshesForCollision();
                    System.Diagnostics.Debug.WriteLine($"📦 Kontrol edilecek mesh sayısı: {partMeshes.Count}");

                    bool hasCollision = false;
                    foreach (Mesh partMesh in partMeshes)
                    {
                        // Mesh validasyonu
                        if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                            continue;

                        try
                        {
                            // Eyeshot CollisionDetection
                            CollisionDetection cd = new CollisionDetection(
                                new Entity[] { displayProbe },  // Yerleştirilmiş probe
                                new Entity[] { partMesh },      // Parça mesh
                                null
                            );

                            cd.CheckMethod = collisionCheckType.SubdivisionTree;
                            cd.DoWork();

                            if (cd.Result != null && cd.Result.Length > 0)
                            {
                                hasCollision = true;
                                System.Diagnostics.Debug.WriteLine("💥 ÇARPIŞMA TESPİT EDİLDİ!");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Çarpışma kontrolü hatası: {ex.Message}");
                        }
                    }

                    if (hasCollision)
                    {
                        System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                        System.Diagnostics.Debug.WriteLine("⛔ ÇARPIŞMA - İŞLEM İPTAL EDİLİYOR");
                        System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                        // ✅ ÇARPIŞMA VAR - Probe'u MAVİ YAP!
                        displayProbe.Visible = true;  // ✅ ÇARPIŞMA - PROBE GÖRÜNÜR YAP!
                        displayProbe.Color = Color.Blue;
                        design.Invalidate();

                        MessageBox.Show(
                            "⚠️ ÇARPIŞMA TESPİT EDİLDİ!\n\n" +
                            "Probe parça ile çarpışıyor.\n" +
                            "Lütfen farklı bir nokta seçin.",
                            "Ridge Width - Çarpışma Uyarısı",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        // ✅ MessageBox kapandıktan SONRA - Probe'u sil
                        design.Entities.Remove(displayProbe);
                        design.Invalidate();

                        // ✅ ÇARPIŞMA - MODDAN ÇIK
                        DisablePointSelection();

                        return;
                    }

                    // ═══════════════════════════════════════════════════════════
                    // ✅ YENİ: İLK KONUMDA ÇARPIŞMA YOK - NORMAL YÖNÜNDE İLERLE
                    // ═══════════════════════════════════════════════════════════
                    System.Diagnostics.Debug.WriteLine("✅ İlk kontrol başarılı - Normal yönünde ilerleme başlıyor (İKİNCİ NOKTA)");

                    double retractDistance = treeViewManager.RetractDistance;
                    int stepCount = (int)retractDistance;
                    System.Diagnostics.Debug.WriteLine($"📏 Retract mesafesi: {retractDistance}mm ({stepCount} adım)");

                    for (int i = 0; i < stepCount; i++)
                    {
                        // Probe'u normal yönünde 1mm kaydir
                        displayProbe.Translate(normal.X * 1.0, normal.Y * 1.0, normal.Z * 1.0);
                        design.Invalidate();

                        System.Diagnostics.Debug.WriteLine($"🔍 Adım {i + 1}/{stepCount}: Probe 1mm kaydırıldı (Normal yönü)");

                        // ÇARPIŞMA KONTROLÜ (Adım konumunda)
                        foreach (Mesh partMesh in partMeshes)
                        {
                            // Mesh validasyonu
                            if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                continue;

                            try
                            {
                                // Eyeshot CollisionDetection
                                CollisionDetection cd = new CollisionDetection(
                                    new Entity[] { displayProbe },  // Kaydırılmış probe
                                    new Entity[] { partMesh },      // Parça mesh
                                    null
                                );

                                cd.CheckMethod = collisionCheckType.SubdivisionTree;
                                cd.DoWork();

                                if (cd.Result != null && cd.Result.Length > 0)
                                {
                                    hasCollision = true;
                                    System.Diagnostics.Debug.WriteLine($"💥 ÇARPIŞMA TESPİT EDİLDİ! (Adım {i + 1})");
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ Çarpışma kontrolü hatası (Adım {i + 1}): {ex.Message}");
                            }
                        }

                        // İç döngüde çarpışma tespit edildiyse, dış döngüden çık
                        if (hasCollision)
                            break;
                    }

                    // ═══════════════════════════════════════════════════════════
                    // Z+ YÖNÜNDEKİ ÇARPIŞMA KONTROLÜ (50-350mm) - İKİNCİ NOKTA
                    // ═══════════════════════════════════════════════════════════
                    if (!hasCollision)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Retract kontrolü başarılı - Z+ yönünde kontrol başlıyor (İKİNCİ NOKTA)");

                        for (int zStep = 50; zStep <= 350; zStep += 50)
                        {
                            // 50mm Z+ yönünde hareket
                            displayProbe.Translate(0, 0, 50.0);
                            design.Invalidate();

                            System.Diagnostics.Debug.WriteLine($"🔍 Z+ Adım: {zStep}mm yukarı çıkıldı (İKİNCİ NOKTA)");

                            // ÇARPIŞMA KONTROLÜ
                            foreach (Mesh partMesh in partMeshes)
                            {
                                if (partMesh == null || partMesh.Vertices == null || partMesh.Vertices.Length == 0)
                                    continue;

                                try
                                {
                                    CollisionDetection cd = new CollisionDetection(
                                        new Entity[] { displayProbe },
                                        new Entity[] { partMesh },
                                        null
                                    );

                                    cd.CheckMethod = collisionCheckType.SubdivisionTree;
                                    cd.DoWork();

                                    if (cd.Result != null && cd.Result.Length > 0)
                                    {
                                        hasCollision = true;
                                        System.Diagnostics.Debug.WriteLine($"💥 ÇARPIŞMA TESPİT EDİLDİ! (Z+ {zStep}mm) (İKİNCİ NOKTA)");
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"❌ Çarpışma kontrolü hatası (Z+ {zStep}mm): {ex.Message}");
                                }
                            }

                            if (hasCollision)
                                break;
                        }

                        if (!hasCollision)
                        {
                            System.Diagnostics.Debug.WriteLine("✅ Z+ kontrolü tamamlandı - Çarpışma yok (İKİNCİ NOKTA)");
                        }
                    }

                    // ═══════════════════════════════════════════════════════════
                    // DÖNGÜ SONRASI KONTROL
                    // ═══════════════════════════════════════════════════════════
                    if (hasCollision)
                    {
                        System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                        System.Diagnostics.Debug.WriteLine("⛔ ÇARPIŞMA - İŞLEM İPTAL EDİLİYOR (Döngü sırasında)");
                        System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                        // ✅ ÇARPIŞMA VAR - Probe'u MAVİ YAP!
                        displayProbe.Visible = true;  // ✅ ÇARPIŞMA - PROBE GÖRÜNÜR YAP!
                        displayProbe.Color = Color.Blue;
                        design.Invalidate();

                        MessageBox.Show(
                            "⚠️ ÇARPIŞMA TESPİT EDİLDİ!\n\n" +
                            "Probe parça ile çarpışıyor.\n" +
                            "Lütfen farklı bir nokta seçin.",
                            "Ridge Width - Çarpışma Uyarısı",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        // ✅ MessageBox kapandıktan SONRA - Probe'u sil
                        design.Entities.Remove(displayProbe);
                        design.Invalidate();

                        // Seçilen noktayı geri al
                        selectedPoints.RemoveAt(selectedPoints.Count - 1);
                        selectedPointCount--;

                        return;  // ❌ Marker ekleme, iptal et!
                    }

                    System.Diagnostics.Debug.WriteLine("✅ TÜM KONTROLLER TAMAM - Çarpışma yok (İKİNCİ NOKTA)");
                    // ═══════════════════════════════════════════════════════════

                    System.Diagnostics.Debug.WriteLine("✅ ÇARPIŞMA YOK - Probe zaten eklendi (İKİNCİ NOKTA)");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    // ═══════════════════════════════════════════════════════════
                    // ÇARPIŞMA KONTROLÜ BİTTİ
                    // ═══════════════════════════════════════════════════════════

                    System.Diagnostics.Debug.WriteLine($"✅ BEYAZ PROBE eklendi: Z- D/2 + Normal D*0.6 kaydırıldı (İKİNCİ NOKTA)");
                }

                double D = (double)selectedProbe.D;
                double radius = D / 2.0;

                System.Diagnostics.Debug.WriteLine($"🔵 Probe D: {D:F3}mm, Radius: {radius:F3}mm");

                // 3. KIRMIZI KÜRE oluştur
                Mesh sphere = Mesh.CreateSphere(radius, 20, 20);

                // ✅ YENİ: Marker'ı normal yönünde D/2 kadar kaydır (yüzeyden dışarı)
                Point3D offsetPoint = new Point3D(
                    point.X + normal.X * radius,
                    point.Y + normal.Y * radius,
                    point.Z + normal.Z * radius
                );

                System.Diagnostics.Debug.WriteLine($"   📍 Orijinal nokta: ({point.X:F3}, {point.Y:F3}, {point.Z:F3})");
                System.Diagnostics.Debug.WriteLine($"   📐 Normal yönü: ({normal.X:F3}, {normal.Y:F3}, {normal.Z:F3})");
                System.Diagnostics.Debug.WriteLine($"   ➡️ Kaydırma: {radius:F3}mm (D/2)");
                System.Diagnostics.Debug.WriteLine($"   📍 Kaydırılmış nokta: ({offsetPoint.X:F3}, {offsetPoint.Y:F3}, {offsetPoint.Z:F3})");

                sphere.Translate(offsetPoint.X, offsetPoint.Y, offsetPoint.Z);
                sphere.Color = Color.Red;
                sphere.ColorMethod = colorMethodType.byEntity;
                sphere.LayerName = MARKER_LAYER_NAME;

                // ✅ YENİ: Grup numarasını EntityData'ya yaz
                if (currentGroupNumber > 0)
                {
                    sphere.EntityData = $"RidgeWidth_{currentGroupNumber}_Marker";
                    System.Diagnostics.Debug.WriteLine($"  ✅ Marker'a grup tag'i eklendi: RidgeWidth_{currentGroupNumber}_Marker");
                }

                // 4. Design'a ekle
                design.Entities.Add(sphere);
                pointMarkers.Add(sphere);

                // 5. Refresh
                design.Invalidate();

                System.Diagnostics.Debug.WriteLine($"✅ Kırmızı küre eklendi - D: {D:F3}mm, Toplam: {pointMarkers.Count}");

                // 6. TreeView'a nokta ekle
                if (currentGroupNode != null)
                {
                    treeViewManager.AddPointToRidgeWidthGroup(currentGroupNode, point, pointMarkers.Count);
                    System.Diagnostics.Debug.WriteLine($"✅ TreeView'a nokta eklendi: Point {pointMarkers.Count}: ({point.X:F2}, {point.Y:F2}, {point.Z:F2})");
                    
                    // ═══════════════════════════════════════════════════════════
                    // ✅ YENİ: MeasurementDataManager'a ekle
                    // ═══════════════════════════════════════════════════════════
                    
                    // Grup bilgilerini al
                    string groupText = currentGroupNode.Text;  // "Ridge Width 1" gibi
                    int currentGroupId = 0;
                    
                    // ✅ DÜZELTME: TreeView'dan grup ID'yi al (TAG STRING formatında)
                    if (currentGroupNode.Tag != null)
                    {
                        string tagStr = currentGroupNode.Tag.ToString();
                        
                        // "RIDGE_WIDTH_2001" -> 2001
                        if (tagStr.StartsWith("RIDGE_WIDTH_"))
                        {
                            string idStr = tagStr.Replace("RIDGE_WIDTH_", "");
                            if (int.TryParse(idStr, out int parsedId))
                            {
                                currentGroupId = parsedId;
                                System.Diagnostics.Debug.WriteLine($"✅ Grup ID parse edildi: {currentGroupId}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ Grup ID parse edilemedi: {tagStr}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Tag formatı yanlış: {tagStr}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ currentGroupNode.Tag NULL!");
                    }
                    
                    if (currentGroupId > 0)
                    {
                        // Değişkenleri al
                        ProbeData selectedProbeData = treeViewManager.GetSelectedProbeData();
                        double retractDistance = treeViewManager.RetractDistance;
                        double zSafetyDistance = treeViewManager.ZSafetyDistance;
                        
                        // MeasurementPoint oluştur
                        var measurementPoint = new MeasurementPoint
                        {
                            MeasurementMode = "RidgeWidth",
                            GroupId = currentGroupId,
                            PointIndex = pointMarkers.Count - 1,  // 0-based index
                            Position = point,
                            MarkerPosition = offsetPoint,  // Kaydırılmış pozisyon (offsetPoint zaten var - satır 589)
                            SurfaceNormal = normal,
                            ProbeName = selectedProbeData?.Name ?? "Unknown",
                            ProbeDiameter = D,
                            RetractDistance = retractDistance,
                            ZSafety = zSafetyDistance,
                            ApproachPoint = new Point3D(
                                offsetPoint.X + normal.X * retractDistance,
                                offsetPoint.Y + normal.Y * retractDistance,
                                offsetPoint.Z + normal.Z * retractDistance
                            ),
                            TouchPoint = point,
                            CreatedAt = DateTime.Now,
                            IsActive = true,
                            Notes = ""
                        };
                        
                        // MeasurementDataManager'a ekle
                        bool success = MeasurementDataManager.Instance.AddPoint(currentGroupId, measurementPoint);
                        
                        if (success)
                        {
                            System.Diagnostics.Debug.WriteLine($"✅ DataManager'a nokta eklendi (Ridge Width): Group={currentGroupId}, Point #{pointMarkers.Count}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ DataManager'a nokta eklenemedi!");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Group ID alınamadı! currentGroupNode.Tag={currentGroupNode.Tag}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ currentGroupNode null - TreeView'a eklenemedi");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AddRidgeWidthPoint hatası: {ex.Message}");
            }
        }

        public bool IsPointSelectionActive()
        {
            return isPointSelectionActive;
        }

        public void DisablePointSelection()
        {
            if (isPointSelectionActive)
            {
                isPointSelectionActive = false;
                design.MouseDown -= Design_MouseDown;

                bool isCompleted = false;

                // ✅ SADECE YARIM KALAN GRUBU SİL (tamamlanmış grupları KORUMA!)
                if (currentGroupNode != null)
                {
                    // Grup tamamlanmış mı kontrol et (3 child node varsa: Point 1, Point 2, Ölçüm Sonucu)
                    isCompleted = currentGroupNode.Nodes.Count >= 3;

                    if (isCompleted)
                    {
                        // Tamamlanmış grup - SADECE currentGroupNode'u null yap, silme!
                        System.Diagnostics.Debug.WriteLine($"✅ Tamamlanmış grup korundu: {currentGroupNode.Text}");
                        currentGroupNode = null;
                    }
                    else
                    {
                        // Yarım kalan grup - SİL!
                        System.Diagnostics.Debug.WriteLine($"🗑️ Yarım kalan grup silindi: {currentGroupNode.Text} (Nodes: {currentGroupNode.Nodes.Count})");
                        currentGroupNode.Remove();
                        currentGroupNode = null;
                    }
                }

                // ✅ Marker'ları temizle - SADECE YARIM KALAN GRUPLARDA!
                if (!isCompleted && pointMarkers.Count > 0)
                {
                    foreach (var marker in pointMarkers)
                    {
                        design.Entities.Remove(marker);
                    }
                    pointMarkers.Clear();
                    selectedPoints.Clear();
                    design.Invalidate();
                    System.Diagnostics.Debug.WriteLine("✅ Marker'lar temizlendi (yarım grup)");
                }
                else if (isCompleted && pointMarkers.Count > 0)
                {
                    // Tamamlanmış grup - Marker'ları KORUMA, sadece listeden temizle
                    pointMarkers.Clear();
                    selectedPoints.Clear();
                    System.Diagnostics.Debug.WriteLine("✅ Marker'lar ekranda bırakıldı (tamamlanmış ölçüm)");
                }

                // ✅ Beyaz probe'u temizle (PROBE_LAYER_NAME layer'ındaki tüm entity'ler)
                var probeEntitiesToRemove = new List<Entity>();
                foreach (Entity entity in design.Entities)
                {
                    if (entity.LayerName == PROBE_LAYER_NAME)
                    {
                        probeEntitiesToRemove.Add(entity);
                    }
                }

                foreach (var entity in probeEntitiesToRemove)
                {
                    design.Entities.Remove(entity);
                }

                if (probeEntitiesToRemove.Count > 0)
                {
                    design.Invalidate();
                    System.Diagnostics.Debug.WriteLine($"✅ {probeEntitiesToRemove.Count} beyaz probe temizlendi");
                }

                // ✅ Yüzeyleri orijinal renge döndür
                RestoreAllVerticalSurfaces();
                System.Diagnostics.Debug.WriteLine("✅ Yüzey renkleri orijinal haline döndürüldü");

                // ✅ YENİ: Aktif grup numarasını temizle
                ClearActiveGroup();

                System.Diagnostics.Debug.WriteLine("⛔ Ridge Width nokta seçimi PASİF");

                // ✅ YENİ: InstructionPanel'i Main Menu'ye döndür
                if (instructionPanel != null && !instructionPanel.IsDisposed)
                {
                    instructionPanel.UpdatePanel(
                        InstructionTexts.TITLE_MAIN_MENU,
                        InstructionTexts.WELCOME
                    );
                    System.Diagnostics.Debug.WriteLine("📋 InstructionPanel Main Menu'ye döndürüldü");
                }
            }
        }
    }
}