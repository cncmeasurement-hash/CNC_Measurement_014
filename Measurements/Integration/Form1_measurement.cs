using System;
using System.Windows.Forms;

namespace _014
{
    /// <summary>
    /// MEASUREMENT MENÜ METODLARI
    /// ✅ Partial class - Form1.cs'nin devamı
    /// ✅ Konik, Delik, Çıkıntı ve NURBS Normal eksen analizleri
    /// ✅ Manuel kullanıcı kontrolü

    /// </summary>
    public partial class CNC_Measurement
    {
        // ═══════════════════════════════════════════════════════════


        /// <summary>
        /// Length modunu kapat (başka mod açıldığında)
        /// </summary>
        private void DisableLengthMode()
        {
            if (isLengthModeActive && lengthAnalyzer != null)
            {
                isLengthModeActive = false;
                lengthAnalyzer.Enable(false);
                System.Diagnostics.Debug.WriteLine("📏 Length modu otomatik kapatıldı (başka mod açıldı)");
            }
        }

        /// <summary>
        /// ✅ Direction Probe modunu kapat (ESC veya başka mod açıldığında)
        /// </summary>
        private void DisableDirectionProbeMode()
        {
            if (selectionManager != null && selectionManager.IsNurbsNormalModeActive())
            {
                selectionManager.DisableNurbsNormalMode();
                System.Diagnostics.Debug.WriteLine("🟡 Direction Probe modu otomatik kapatıldı");
            }
        }

        /// <summary>
        /// Face to Face modunu kapat (başka mod açıldığında)
        /// ✅ Cleanup ve reset
        /// </summary>
        /// <summary>
        /// Face to Face modunu kapat (başka mod açıldığında)
        /// ✅ FaceToFaceManager kullanıyor
        /// </summary>
        private void DisableFaceMode()
        {
            if (faceToFaceManager != null && faceToFaceManager.IsActive)
            {
                faceToFaceManager.Disable();
                faceToFaceToolStripMenuItem.Checked = false;
                System.Diagnostics.Debug.WriteLine("✅ Face to Face kapatıldı (başka mod açıldı)");
            }
        }

        /// <summary>
        /// ✅ Surface to Surface modunu kapat (başka mod açıldığında)
        /// </summary>
        private void DisableSurfaceToSurfaceMode()
        {
            if (isSurfaceToSurfaceActive && surfaceToSurfaceMeasurement != null)
            {
                isSurfaceToSurfaceActive = false;
                surfaceToSurfaceMeasurement.Disable();
                surfaceToSurfaceToolStripMenuItem.Checked = false;
                System.Diagnostics.Debug.WriteLine("📏 Surface to Surface modu otomatik kapatıldı (başka mod açıldı)");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MEASUREMENT MENÜ - EKSEN ANALİZLERİ
        // ═══════════════════════════════════════════════════════════


        /// <summary>
        /// Sadece HOLE (delik) eksenlerini göster
        /// </summary>

        // ═══════════════════════════════════════════════════════════
        // ✅ YENİ: NURBS NORMAL ANALİZİ
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// ✅ YENİ: Normal Nurbs - İnteraktif mod aktif/pasif
        /// Measurement → Normal Nurbs
        /// Kullanıcı NURBS yüzeylere tıklayarak normal vektörleri görebilir
        /// </summary>
        private void normalNurbsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DisableLengthMode();
                DisableFaceMode();
                DisableSurfaceToSurfaceMode();

                // SelectionManager üzerinden toggle yap
                bool isActive = selectionManager.ToggleNurbsNormalMode();

                if (isActive)
                {
                    // ✅ MOD AKTİF
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    System.Diagnostics.Debug.WriteLine("🟡 NURBS NORMAL MODU AKTİF");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                    //                     MessageBox.Show(
                    //                         "🟡 NURBS Normal Modu Aktif!\n\n" +
                    //                         "✅ NURBS yüzeylere tıklayın\n" +
                    //                         "✅ Normal vektörleri gösterilecek\n\n" +
                    //                         "🟣 Mor marker = Tıklanan nokta\n" +
                    //                         "🟡 Sarı çizgi = Normal vektör\n\n" +
                    //                         "Kapatmak için tekrar tıklayın.",
                    //                         "NURBS Normal Modu",
                    //                         MessageBoxButtons.OK,
                    //                         MessageBoxIcon.Information
                    //                     );
                }
                else
                {
                    // ⛔ MOD PASİF
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    System.Diagnostics.Debug.WriteLine("⛔ NURBS NORMAL MODU KAPALI");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                    //                     MessageBox.Show(
                    //                         "⛔ NURBS Normal Modu Kapatıldı!\n\n" +
                    //                         "Mod devre dışı bırakıldı.",
                    //                         "NURBS Normal Modu",
                    //                         MessageBoxButtons.OK,
                    //                         MessageBoxIcon.Information
                    //                     );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Normal Nurbs hatası: {ex.Message}");

                MessageBox.Show(
                    $"NURBS Normal modu sırasında hata oluştu!\n\n" +
                    $"Hata: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // ═══════════════════════════════════════════════════════════
        // ✅ LENGTH - UZUNLUK ÖLÇME MODU
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// ✅ Length - İki nokta arası uzunluk ölçümü
        /// Measurement → Length
        /// Kullanıcı iki nokta seçerek aralarındaki mesafeyi ölçer
        /// </summary>
        private void lengthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DisableLengthMode();
                DisableFaceMode();
                DisableSurfaceToSurfaceMode();
                DisableDirectionProbeMode();


                // İlk kullanımda analyzer'ı oluştur
                if (lengthAnalyzer == null)
                {
                    lengthAnalyzer = new LengthMeasurementAnalyzer(design1);

                    // ESC tuşu ile kapatıldığında callback
                    lengthAnalyzer.OnDisabled = () =>
                DisableDirectionProbeMode();
                    {
                        isLengthModeActive = false;
                        System.Diagnostics.Debug.WriteLine("📏 isLengthModeActive = false (ESC callback)");
                    }
                    ;

                    System.Diagnostics.Debug.WriteLine("✅ LengthMeasurementAnalyzer oluşturuldu!");
                }

                // Toggle - aç/kapat
                if (isLengthModeActive)
                {
                    // ⛔ MOD ZATEN AKTİF → KAPAT
                    isLengthModeActive = false;

                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    System.Diagnostics.Debug.WriteLine("⛔ LENGTH MODU KAPATILDI");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                    lengthAnalyzer.Enable(false);
                }
                else
                {
                    // ✅ MOD PASİF → AKTİF ET
                    isLengthModeActive = true;

                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                    System.Diagnostics.Debug.WriteLine("📏 LENGTH MODU AKTİF");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                    lengthAnalyzer.Enable(true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Length modu hatası: {ex.Message}");

                MessageBox.Show(
                    $"Length ölçüm modu sırasında hata oluştu!\n\n" +
                    $"Hata: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ═══════════════════════════════════════════════════════════
        // DİĞER MENÜ METODLARI (ŞİMDİLİK BOŞ - GELECEKTE EKLENEBİLİR)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Normal Faces - Şimdilik boş (gelecekte eklenebilir)
        /// </summary>
        private void normalFacesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Gelecekte eklenecek
            MessageBox.Show(
                "Bu özellik henüz eklenmedi.\n\n" +
                "Normal Faces analizi için gelecek güncellemeleri bekleyin.",
                "Bilgi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════
// ÖZET - MEASUREMENT MENÜ YAPISI
// ═══════════════════════════════════════════════════════════════════════

/*

MENÜ YAPISI:

Measurement
├── Normal Faces       → normalFacesToolStripMenuItem_Click (Boş - TODO)
├── Normal Nurbs       → normalNurbsToolStripMenuItem_Click ✅ İNTERAKTİF MOD
├── ──────────────────
├── ──────────────────

*/