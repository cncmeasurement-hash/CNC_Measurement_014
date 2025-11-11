using System;
using System.Drawing;
using System.Windows.Forms;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;

namespace _014.Probe.Visualization
{
    /// <summary>
    /// Prob üzerindeki belirli bir bölgenin rengini animasyonlu olarak değiştiren sınıf.
    /// Yeşil bölgenin yanıp sönmesi efekti sağlar.
    /// </summary>
    public class ProbeColorAnimator
    {
        private Design design;
        private System.Windows.Forms.Timer animationTimer;
        private int currentPhase = 0;
        private DateTime phaseStartTime;

        // Renkler
        private readonly Color greenColor = Color.FromArgb(0, 255, 0);
        private readonly Color whiteColor = Color.FromArgb(240, 240, 240);

        // Süre ayarları (milisaniye) - ÇOK HIZLI ANIMASYON
        private const int WHITE_PHASE_1_MS = 500;   // 0.5 saniye beyaz
        private const int GREEN_PHASE_MS = 250;     // 0.25 saniye yeşil  
        private const int WHITE_PHASE_2_MS = 500;   // 0.5 saniye beyaz
        private const int TIMER_INTERVAL_MS = 50;   // 50ms güncelleme

        public bool IsAnimating { get; private set; }

        public ProbeColorAnimator(Design designControl)
        {
            design = designControl;
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = TIMER_INTERVAL_MS;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        public void StartAnimation()
        {
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine("🚀 StartAnimation() çağrıldı!");

            if (IsAnimating)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Zaten animasyon çalışıyor!");
                return;
            }

            IsAnimating = true;
            currentPhase = 0;
            phaseStartTime = DateTime.Now;

            System.Diagnostics.Debug.WriteLine("📊 İlk renk değişimi başlatılıyor (Beyaz)...");
            SetGreenRegionColor(whiteColor);

            animationTimer.Start();

            System.Diagnostics.Debug.WriteLine("✅ Animasyon başlatıldı! Timer çalışıyor.");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
        }

        public void StopAnimation()
        {
            if (!IsAnimating)
                return;

            animationTimer.Stop();
            IsAnimating = false;
            SetGreenRegionColor(greenColor);

            System.Diagnostics.Debug.WriteLine("⛔ Animasyon durduruldu!");
        }

        /// <summary>
        /// Animasyon hızını günceller
        /// </summary>
        /// <param name="intervalMs">Timer interval (ms) - Min: 50, Max: 500</param>
        public void UpdateSpeed(int intervalMs)
        {
            // Hız sınırlarını kontrol et
            if (intervalMs < 50) intervalMs = 50;     // Minimum 50ms
            if (intervalMs > 500) intervalMs = 500;   // Maximum 500ms

            animationTimer.Interval = intervalMs;

            System.Diagnostics.Debug.WriteLine($"🎨 Animasyon hızı güncellendi: {intervalMs}ms");
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - phaseStartTime;
            int elapsedMs = (int)elapsed.TotalMilliseconds;

            switch (currentPhase)
            {
                case 0:
                    if (elapsedMs >= WHITE_PHASE_1_MS)
                    {
                        currentPhase = 1;
                        phaseStartTime = DateTime.Now;
                        SetGreenRegionColor(greenColor);
                        System.Diagnostics.Debug.WriteLine("🟢 Faz 1: YEŞİL");
                    }
                    break;

                case 1:
                    if (elapsedMs >= GREEN_PHASE_MS)
                    {
                        currentPhase = 2;
                        phaseStartTime = DateTime.Now;
                        SetGreenRegionColor(whiteColor);
                        System.Diagnostics.Debug.WriteLine("⚪ Faz 2: BEYAZ");
                    }
                    break;

                case 2:
                    if (elapsedMs >= WHITE_PHASE_2_MS)
                    {
                        currentPhase = 0;
                        phaseStartTime = DateTime.Now;
                        SetGreenRegionColor(whiteColor);
                        System.Diagnostics.Debug.WriteLine("⚪ Faz 0: BEYAZ");
                    }
                    break;
            }
        }

        private void SetGreenRegionColor(Color color)
        {
            int colorChangedCount = 0;
            int totalEntitiesChecked = 0;
            int blocksFound = 0;
            int entitiesInBlocks = 0;

            System.Diagnostics.Debug.WriteLine($"🔍 Renk değiştirme başladı: Hedef renk = {color.Name} (R:{color.R}, G:{color.G}, B:{color.B})");

            foreach (Entity entity in design.Entities)
            {
                totalEntitiesChecked++;

                System.Diagnostics.Debug.WriteLine($"  📦 Entity {totalEntitiesChecked}: Type={entity.GetType().Name}, Color={entity.Color.Name} (R:{entity.Color.R}, G:{entity.Color.G}, B:{entity.Color.B})");

                if (IsGreenishColor(entity.Color))
                {
                    System.Diagnostics.Debug.WriteLine($"    ✅ YEŞİL BULUNDU! Renk değiştiriliyor...");
                    entity.Color = color;
                    entity.ColorMethod = colorMethodType.byEntity;
                    colorChangedCount++;
                }

                if (entity is BlockReference blockRef)
                {
                    blocksFound++;
                    var blockName = blockRef.BlockName;
                    var block = design.Blocks[blockName];

                    System.Diagnostics.Debug.WriteLine($"  🎁 Block bulundu: {blockName}");

                    if (block != null)
                    {
                        foreach (var ent in block.Entities)
                        {
                            entitiesInBlocks++;
                            System.Diagnostics.Debug.WriteLine($"    📦 Block içi entity: Type={ent.GetType().Name}, Color={ent.Color.Name} (R:{ent.Color.R}, G:{ent.Color.G}, B:{ent.Color.B})");

                            if (IsGreenishColor(ent.Color))
                            {
                                System.Diagnostics.Debug.WriteLine($"      ✅ YEŞİL BULUNDU! Renk değiştiriliyor...");
                                ent.Color = color;
                                ent.ColorMethod = colorMethodType.byEntity;
                                colorChangedCount++;
                            }
                        }
                    }
                }
            }

            design.Invalidate();
            System.Diagnostics.Debug.WriteLine($"🎨 ÖZET: {colorChangedCount} entity rengi değişti (Toplam kontrol edilen: {totalEntitiesChecked}, Block sayısı: {blocksFound}, Block içi entity: {entitiesInBlocks})");
        }

        private bool IsGreenishColor(Color c)
        {
            // Parlak yeşil: Lime (0, 255, 0) - Delik silindirinin başlangıç rengi
            bool isLimeGreen = c.R == 0 && c.G == 255 && c.B == 0;

            // Animasyon beyazı: (240, 240, 240) - Sadece animasyon sırasında kullanılan beyaz
            // DİKKAT: Konik geçiş ve sap Color.White (255,255,255) olduğu için onlara dokunmaz
            bool isAnimationWhite = c.R == 240 && c.G == 240 && c.B == 240;

            return isLimeGreen || isAnimationWhite;
        }

        public int CountGreenEntities()
        {
            int count = 0;

            foreach (Entity entity in design.Entities)
            {
                if (IsGreenishColor(entity.Color))
                    count++;

                if (entity is BlockReference blockRef)
                {
                    var block = design.Blocks[blockRef.BlockName];
                    if (block != null)
                    {
                        foreach (var ent in block.Entities)
                        {
                            if (IsGreenishColor(ent.Color))
                                count++;
                        }
                    }
                }
            }

            return count;
        }

        public void Dispose()
        {
            if (animationTimer != null)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
            }
        }
    }
}