using System;
using System.Drawing;
using System.Windows.Forms;

namespace _014.Handlers.EdgeToEdge
{
    /// <summary>
    /// Edge to Edge measurement info panel
    /// İki edge arasındaki mesafe, açı ve edge bilgilerini gösterir
    /// </summary>
    public class EdgeToEdgeInfoPanel : Form
    {
        // ═══════════════════════════════════════════════════════════
        // UI CONTROLS
        // ═══════════════════════════════════════════════════════════

        private Label lblTitle;
        private Label lblEdge1Info;
        private Label lblEdge2Info;
        private Label lblDistanceInfo;
        private Label lblAngleInfo;
        private Label lblWaiting;
        private Panel separatorPanel1;
        private Panel separatorPanel2;

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        public EdgeToEdgeInfoPanel(Form parentForm)
        {
            InitializeComponents();
            SetupForm(parentForm);
        }

        // ═══════════════════════════════════════════════════════════
        // INITIALIZATION
        // ═══════════════════════════════════════════════════════════

        private void InitializeComponents()
        {
            // Form özellikleri
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.FromArgb(45, 45, 48);
            Size = new Size(350, 280);
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            ShowInTaskbar = false;

            // Title Label
            lblTitle = new Label
            {
                Text = "📏 EDGE TO EDGE ÖLÇÜM",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 255),
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(330, 30),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblTitle);

            // Separator 1
            separatorPanel1 = new Panel
            {
                BackColor = Color.FromArgb(80, 80, 80),
                Size = new Size(330, 1),
                Location = new Point(10, 45)
            };
            Controls.Add(separatorPanel1);

            // Edge 1 Info
            lblEdge1Info = new Label
            {
                Text = "📍 Edge 1: -",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Yellow,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(330, 30),
                Location = new Point(10, 55),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblEdge1Info);

            // Edge 2 Info
            lblEdge2Info = new Label
            {
                Text = "📍 Edge 2: -",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Cyan,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(330, 30),
                Location = new Point(10, 90),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblEdge2Info);

            // Separator 2
            separatorPanel2 = new Panel
            {
                BackColor = Color.FromArgb(80, 80, 80),
                Size = new Size(330, 1),
                Location = new Point(10, 130)
            };
            Controls.Add(separatorPanel2);

            // Distance Info
            lblDistanceInfo = new Label
            {
                Text = "📏 Mesafe: -",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 100, 100),
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(330, 35),
                Location = new Point(10, 140),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblDistanceInfo);

            // Angle Info
            lblAngleInfo = new Label
            {
                Text = "📐 Açı: -",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 255, 150),
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(330, 35),
                Location = new Point(10, 180),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblAngleInfo);

            // Waiting Message
            lblWaiting = new Label
            {
                Text = "⏳ Edge seçimi bekleniyor...\n\n" +
                       "1️⃣ İlk edge'i seçin (SARI)\n" +
                       "2️⃣ İkinci edge'i seçin (CYAN)",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(330, 100),
                Location = new Point(10, 55),
                TextAlign = ContentAlignment.TopCenter,
                Visible = true
            };
            Controls.Add(lblWaiting);

            // Paint event (border)
            Paint += OnPaint;
        }

        private void SetupForm(Form parentForm)
        {
            if (parentForm == null) return;

            // Sağ üst köşede konumlandır
            int padding = 10;
            Location = new Point(
                parentForm.Right - Width - padding,
                parentForm.Top + padding
            );

            // Parent form hareket edince beraber hareket et
            parentForm.LocationChanged += (s, e) =>
            {
                if (Visible && !parentForm.IsDisposed)
                {
                    Location = new Point(
                        parentForm.Right - Width - padding,
                        parentForm.Top + padding
                    );
                }
            };
        }

        // ═══════════════════════════════════════════════════════════
        // PUBLIC METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Waiting mesajını göster
        /// </summary>
        public void ShowWaitingMessage()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ShowWaitingMessage));
                return;
            }

            lblWaiting.Visible = true;
            lblEdge1Info.Text = "📍 Edge 1: -";
            lblEdge2Info.Text = "📍 Edge 2: -";
            lblDistanceInfo.Text = "📏 Mesafe: -";
            lblAngleInfo.Text = "📐 Açı: -";

            // Separator'ları gizle
            separatorPanel1.Visible = false;
            separatorPanel2.Visible = false;
            lblDistanceInfo.Visible = false;
            lblAngleInfo.Visible = false;
        }

        /// <summary>
        /// İlk edge bilgisini güncelle
        /// </summary>
        public void UpdateFirstEdgeInfo(double edge1Length)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<double>(UpdateFirstEdgeInfo), edge1Length);
                return;
            }

            lblWaiting.Visible = false;
            separatorPanel1.Visible = true;

            lblEdge1Info.Text = $"📍 Edge 1: {edge1Length:F2} mm (SARI)";
            lblEdge2Info.Text = "📍 Edge 2: Bekleniyor...";

            // Distance ve angle henüz yok
            lblDistanceInfo.Visible = false;
            lblAngleInfo.Visible = false;
            separatorPanel2.Visible = false;
        }

        /// <summary>
        /// Ölçüm bilgilerini güncelle
        /// </summary>
        public void UpdateMeasurementInfo(
            double edge1Length,
            double edge2Length,
            double distance,
            double angle,
            string angleStatus)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<double, double, double, double, string>(
                    UpdateMeasurementInfo),
                    edge1Length, edge2Length, distance, angle, angleStatus);
                return;
            }

            lblWaiting.Visible = false;
            separatorPanel1.Visible = true;
            separatorPanel2.Visible = true;
            lblDistanceInfo.Visible = true;
            lblAngleInfo.Visible = true;

            // Edge bilgileri
            lblEdge1Info.Text = $"📍 Edge 1: {edge1Length:F2} mm (SARI)";
            lblEdge2Info.Text = $"📍 Edge 2: {edge2Length:F2} mm (CYAN)";

            // Mesafe bilgisi
            lblDistanceInfo.Text = $"📏 Mesafe: {distance:F2} mm";

            // Açı bilgisi
            lblAngleInfo.Text = $"📐 Açı: {angle:F2}°{angleStatus}";

            // Açı durumuna göre renk
            if (angleStatus.Contains("Paralel"))
            {
                lblAngleInfo.ForeColor = Color.FromArgb(100, 200, 255); // Mavi
            }
            else if (angleStatus.Contains("Dik"))
            {
                lblAngleInfo.ForeColor = Color.FromArgb(255, 150, 50); // Turuncu
            }
            else
            {
                lblAngleInfo.ForeColor = Color.FromArgb(150, 255, 150); // Yeşil
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PAINT
        // ═══════════════════════════════════════════════════════════

        private void OnPaint(object sender, PaintEventArgs e)
        {
            // Cyan border
            using (Pen borderPen = new Pen(Color.FromArgb(0, 200, 255), 2))
            {
                e.Graphics.DrawRectangle(
                    borderPen,
                    0, 0,
                    Width - 1,
                    Height - 1
                );
            }
        }

        // ═══════════════════════════════════════════════════════════
        // OVERRIDE
        // ═══════════════════════════════════════════════════════════

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                return cp;
            }
        }
    }
}
