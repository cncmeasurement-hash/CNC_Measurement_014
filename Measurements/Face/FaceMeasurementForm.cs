using System;
using System.Drawing;
using System.Windows.Forms;

namespace _014.Measurements.Face
{
    /// <summary>
    /// 📏 FACE MEASUREMENT FORMU
    /// ✅ DiameterMeasurementForm ile aynı stil
    /// ✅ Ekranın sağ üst köşesinde sabit kalır
    /// ✅ Fare ile taşınabilir
    /// ✅ İÇERİK: Minimum, Açı, 1. Yüzey Alanı, 2. Yüzey Alanı
    /// </summary>
    public class FaceMeasurementForm : Form
    {
        // UI Controls
        private Label lblTitle;
        private Label lblMinDistance;
        private Label lblAngle;
        private Label lblArea1;
        private Label lblArea2;

        // Value Labels (güncellenecek)
        private Label valueMinDistance;
        private Label valueAngle;
        private Label valueArea1;
        private Label valueArea2;

        public FaceMeasurementForm()
        {
            InitializeForm();
            InitializeControls();
            ResetValues();
        }

        /// <summary>
        /// Form ayarları (DiameterMeasurementForm ile aynı!)
        /// </summary>
        private void InitializeForm()
        {
            Text = "";  // ✅ Başlık YOK
            Size = new Size(230, 220);  // ✅ Biraz büyütüldü (4 satır için)
            FormBorderStyle = FormBorderStyle.None;  // ✅ Kenarlık yok
            StartPosition = FormStartPosition.Manual;
            TopMost = true;  // Her zaman üstte
            BackColor = Color.White;  // ✅ BEYAZ arka plan
            ForeColor = Color.Black;  // ✅ Siyah metin

            // ✅ Kenarlık çiz (gri çizgi) - Diameter ile aynı!
            Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.DarkGray, 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            };

            // ✅ FARE İLE TAŞIMA (Diameter ile aynı!)
            bool isDragging = false;
            Point dragStartPoint = Point.Empty;

            MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isDragging = true;
                    dragStartPoint = e.Location;
                    Cursor = Cursors.SizeAll;  // El imleci
                }
            };

            MouseMove += (s, e) =>
            {
                if (isDragging)
                {
                    Point newLocation = Location;
                    newLocation.X += e.X - dragStartPoint.X;
                    newLocation.Y += e.Y - dragStartPoint.Y;
                    Location = newLocation;
                }
            };

            MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isDragging = false;
                    Cursor = Cursors.Default;
                }
            };

            // Sağ üst köşeye yerleştir (Diameter ile aynı!)
            Load += (s, e) =>
            {
                var screen = Screen.FromControl(this);
                Location = new Point(
                    screen.WorkingArea.Right - Width - 20,
                    screen.WorkingArea.Top + 20
                );
            };

            // Form kapatma engelle (sadece gizle)
            FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    Hide();
                }
            };
        }

        /// <summary>
        /// Kontrolleri oluştur
        /// </summary>
        private void InitializeControls()
        {
            int yPos = 15;  // Başlangıç
            int spacing = 26;  // Satır arası

            // ═══════════════════════════════════════
            // BAŞLIK
            // ═══════════════════════════════════════
            lblTitle = CreateLabel("🎯 YÜZEY ÖLÇÜMLERİ", yPos, true, 12);
            lblTitle.Font = new Font(lblTitle.Font.FontFamily, 8, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 200, 255);  // Mavi
            yPos += spacing + 5;

            // Ayırıcı
            CreateSeparator(yPos);
            yPos += 12;

            // ═══════════════════════════════════════
            // MİNİMUM MESAFE
            // ═══════════════════════════════════════
            lblMinDistance = CreateLabel("📏 Minimum:", yPos);
            valueMinDistance = CreateValueLabel("--- mm", yPos);
            yPos += spacing + 3;

            // Ayırıcı
            CreateSeparator(yPos);
            yPos += 12;

            // ═══════════════════════════════════════
            // AÇI
            // ═══════════════════════════════════════
            lblAngle = CreateLabel("📐 Açı:", yPos);
            valueAngle = CreateValueLabel("---°", yPos);
            yPos += spacing + 3;

            // Ayırıcı
            CreateSeparator(yPos);
            yPos += 12;

            // ═══════════════════════════════════════
            // 1. YÜZEY ALANI
            // ═══════════════════════════════════════
            lblArea1 = CreateLabel("📊 1. Yüzey Alanı:", yPos);
            valueArea1 = CreateValueLabel("--- mm²", yPos);
            yPos += spacing + 3;

            // Ayırıcı
            CreateSeparator(yPos);
            yPos += 12;

            // ═══════════════════════════════════════
            // 2. YÜZEY ALANI
            // ═══════════════════════════════════════
            lblArea2 = CreateLabel("📊 2. Yüzey Alanı:", yPos);
            valueArea2 = CreateValueLabel("--- mm²", yPos);
            yPos += spacing + 8;
        }

        /// <summary>
        /// Label oluştur (Diameter ile aynı!)
        /// </summary>
        private Label CreateLabel(string text, int yPos, bool center = false, int fontSize = 9)
        {
            var label = new Label
            {
                Text = text,
                AutoSize = false,
                Width = center ? 210 : 120,  // ✅ Biraz genişletildi
                Height = 20,
                Location = new Point(center ? 10 : 5, yPos),
                Font = new Font("Segoe UI", fontSize),
                ForeColor = Color.Black,
                TextAlign = center ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft
            };
            Controls.Add(label);
            return label;
        }

        /// <summary>
        /// Değer label'ı oluştur (Diameter ile aynı!)
        /// </summary>
        private Label CreateValueLabel(string text, int yPos)
        {
            var label = new Label
            {
                Text = text,
                AutoSize = false,
                Width = 100,  // ✅ Biraz genişletildi (90 → 100)
                Height = 20,
                Location = new Point(125, yPos),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),  // Mavi
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(label);
            return label;
        }

        /// <summary>
        /// Ayırıcı çizgi (Diameter ile aynı!)
        /// </summary>
        private void CreateSeparator(int yPos)
        {
            var separator = new Label
            {
                AutoSize = false,
                Width = 220,  // ✅ Form genişliği
                Height = 1,
                Location = new Point(5, yPos),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray
            };
            Controls.Add(separator);
        }

        /// <summary>
        /// Değerleri sıfırla
        /// </summary>
        public void ResetValues()
        {
            valueMinDistance.Text = "--- mm";
            valueMinDistance.ForeColor = Color.Gray;

            valueAngle.Text = "---°";
            valueAngle.ForeColor = Color.Gray;

            valueArea1.Text = "--- mm²";
            valueArea1.ForeColor = Color.Gray;

            valueArea2.Text = "--- mm²";
            valueArea2.ForeColor = Color.Gray;
        }

        /// <summary>
        /// Ölçüm değerlerini güncelle
        /// </summary>
        public void UpdateMeasurements(double minDist, double angle, double area1, double area2)
        {
            // Minimum Mesafe
            valueMinDistance.Text = $"{minDist:F3} mm";
            valueMinDistance.ForeColor = Color.FromArgb(0, 120, 215);  // Mavi

            // Açı
            valueAngle.Text = $"{angle:F2}°";
            valueAngle.ForeColor = Color.FromArgb(0, 120, 215);  // Mavi

            // 1. Yüzey Alanı
            valueArea1.Text = $"{area1:F2} mm²";
            valueArea1.ForeColor = Color.FromArgb(0, 120, 215);  // Mavi

            // 2. Yüzey Alanı
            valueArea2.Text = $"{area2:F2} mm²";
            valueArea2.ForeColor = Color.FromArgb(0, 120, 215);  // Mavi

            // Form'u göster (gizliyse)
            if (!Visible)
            {
                Show();
            }
        }

        /// <summary>
        /// Form'u göster
        /// </summary>
        public new void Show()
        {
            base.Show();
            BringToFront();
        }

        /// <summary>
        /// Form'u gizle ve sıfırla
        /// </summary>
        public new void Hide()
        {
            base.Hide();
            ResetValues();
        }
    }
}
