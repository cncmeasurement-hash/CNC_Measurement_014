using System;
using System.Drawing;
using System.Windows.Forms;
using devDept.Geometry;  // ✅ Eyeshot'un Point3D'si

namespace _014.Measurements.Length
{
    /// <summary>
    /// 📏 LENGTH ÖLÇÜM FORMU
    /// ✅ Ekranın sağ üst köşesinde sabit kalır
    /// ✅ Her tıklamada değerler canlı güncellenir
    /// ✅ MessageBox yerine sürekli açık tablo
    /// </summary>
    public class LengthMeasurementForm : Form
    {
        // UI Controls - Labels
        private Label lblDistance = null!;
        private Label lblDeltaX = null!;
        private Label lblDeltaY = null!;
        private Label lblDeltaZ = null!;
        private Label lblPoint1 = null!;
        private Label lblPoint2 = null!;

        // Value Labels (güncellenecek)
        private Label valueDistance = null!;
        private Label valueDeltaX = null!;
        private Label valueDeltaY = null!;
        private Label valueDeltaZ = null!;
        private Label valuePoint1 = null!;
        private Label valuePoint2 = null!;

        public LengthMeasurementForm()
        {
            InitializeForm();
            InitializeControls();
            ResetValues();
        }

        /// <summary>
        /// Form ayarları
        /// </summary>
        private void InitializeForm()
        {
            Text = "";  // Başlık YOK
            Size = new Size(220, 280);  // ✅ Diameter ile aynı boyut
            FormBorderStyle = FormBorderStyle.None;  // Kenarlık yok
            StartPosition = FormStartPosition.Manual;
            TopMost = true;  // Her zaman üstte
            BackColor = Color.White;  // Beyaz arka plan
            ForeColor = Color.Black;  // Siyah metin

            // Kenarlık çiz (gri çizgi)
            Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.DarkGray, 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            };

            // FARE İLE TAŞIMA (Sürükle-Bırak)
            bool isDragging = false;
            Point dragStartPoint = Point.Empty;

            MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isDragging = true;
                    dragStartPoint = e.Location;
                    Cursor = Cursors.SizeAll;
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

            // Sağ üst köşeye yerleştir
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
            int yPos = 15;
            int leftLabelX = 15;
            int valueX = 95;  // ✅ 140 → 95 (daha sola)
            int lineHeight = 32;

            // Font ayarları
            Font labelFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            Font valueFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            Font titleFont = new Font("Segoe UI", 10F, FontStyle.Bold);

            // Başlık
            var lblTitle = new Label
            {
                Text = "📏 MESAFE ÖLÇÜMÜ",
                Location = new Point(15, yPos),
                Size = new Size(190, 25),  // ✅ 250 → 190
                Font = titleFont,
                ForeColor = Color.FromArgb(0, 102, 204)  // Mavi
            };
            Controls.Add(lblTitle);
            yPos += 30;

            // Separator
            var separator1 = new Panel
            {
                Location = new Point(15, yPos),
                Size = new Size(190, 1),  // ✅ 250 → 190
                BackColor = Color.LightGray
            };
            Controls.Add(separator1);
            yPos += 10;

            // Distance
            lblDistance = new Label
            {
                Text = "Mesafe:",
                Location = new Point(leftLabelX, yPos),
                Size = new Size(75, 20),  // ✅ 120 → 75
                Font = labelFont
            };
            Controls.Add(lblDistance);

            valueDistance = new Label
            {
                Text = "-",
                Location = new Point(valueX, yPos),
                Size = new Size(110, 20),  // ✅ 120 → 110
                Font = valueFont,
                ForeColor = Color.FromArgb(0, 128, 0)  // Yeşil
            };
            Controls.Add(valueDistance);
            yPos += lineHeight;

            // ΔX
            lblDeltaX = new Label
            {
                Text = "ΔX:",
                Location = new Point(leftLabelX, yPos),
                Size = new Size(75, 20),  // ✅ 120 → 75
                Font = labelFont
            };
            Controls.Add(lblDeltaX);

            valueDeltaX = new Label
            {
                Text = "-",
                Location = new Point(valueX, yPos),
                Size = new Size(110, 20),  // ✅ 120 → 110
                Font = valueFont
            };
            Controls.Add(valueDeltaX);
            yPos += lineHeight;

            // ΔY
            lblDeltaY = new Label
            {
                Text = "ΔY:",
                Location = new Point(leftLabelX, yPos),
                Size = new Size(75, 20),  // ✅ 120 → 75
                Font = labelFont
            };
            Controls.Add(lblDeltaY);

            valueDeltaY = new Label
            {
                Text = "-",
                Location = new Point(valueX, yPos),
                Size = new Size(110, 20),  // ✅ 120 → 110
                Font = valueFont
            };
            Controls.Add(valueDeltaY);
            yPos += lineHeight;

            // ΔZ
            lblDeltaZ = new Label
            {
                Text = "ΔZ:",
                Location = new Point(leftLabelX, yPos),
                Size = new Size(75, 20),  // ✅ 120 → 75
                Font = labelFont
            };
            Controls.Add(lblDeltaZ);

            valueDeltaZ = new Label
            {
                Text = "-",
                Location = new Point(valueX, yPos),
                Size = new Size(110, 20),  // ✅ 120 → 110
                Font = valueFont
            };
            Controls.Add(valueDeltaZ);
            yPos += lineHeight;

            // Separator 2
            var separator2 = new Panel
            {
                Location = new Point(15, yPos),
                Size = new Size(190, 1),  // ✅ 250 → 190
                BackColor = Color.LightGray
            };
            Controls.Add(separator2);
            yPos += 10;

            // Point 1
            lblPoint1 = new Label
            {
                Text = "Nokta 1:",
                Location = new Point(leftLabelX, yPos),
                Size = new Size(55, 20),  // ✅ 70 → 55
                Font = new Font("Segoe UI", 8F, FontStyle.Regular)
            };
            Controls.Add(lblPoint1);

            valuePoint1 = new Label
            {
                Text = "-",
                Location = new Point(70, yPos),  // ✅ 85 → 70
                Size = new Size(135, 20),  // ✅ 180 → 135
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = Color.Gray
            };
            Controls.Add(valuePoint1);
            yPos += 24;

            // Point 2
            lblPoint2 = new Label
            {
                Text = "Nokta 2:",
                Location = new Point(leftLabelX, yPos),
                Size = new Size(55, 20),  // ✅ 70 → 55
                Font = new Font("Segoe UI", 8F, FontStyle.Regular)
            };
            Controls.Add(lblPoint2);

            valuePoint2 = new Label
            {
                Text = "-",
                Location = new Point(70, yPos),  // ✅ 85 → 70
                Size = new Size(135, 20),  // ✅ 180 → 135
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = Color.Gray
            };
            Controls.Add(valuePoint2);
        }

        /// <summary>
        /// Değerleri sıfırla
        /// </summary>
        public void ResetValues()
        {
            valueDistance.Text = "-";
            valueDeltaX.Text = "-";
            valueDeltaY.Text = "-";
            valueDeltaZ.Text = "-";
            valuePoint1.Text = "-";
            valuePoint2.Text = "-";
        }

        /// <summary>
        /// Ölçüm sonuçlarını güncelle
        /// </summary>
        public void UpdateMeasurement(
            double distance,
            double deltaX,
            double deltaY,
            double deltaZ,
            Point3D point1,
            Point3D point2)
        {
            // Distance
            valueDistance.Text = $"{distance:F3} mm";

            // Deltas
            valueDeltaX.Text = $"{deltaX:F3} mm";
            valueDeltaY.Text = $"{deltaY:F3} mm";
            valueDeltaZ.Text = $"{deltaZ:F3} mm";

            // Points
            valuePoint1.Text = $"({point1.X:F3}, {point1.Y:F3}, {point1.Z:F3})";
            valuePoint2.Text = $"({point2.X:F3}, {point2.Y:F3}, {point2.Z:F3})";

            // Formu göster (eğer gizliyse)
            if (!Visible)
            {
                Show();
            }
        }
    }
}
