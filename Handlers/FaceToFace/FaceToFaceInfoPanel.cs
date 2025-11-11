using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace _014.Handlers.FaceToFace
{
    public class FaceToFaceInfoPanel : Form
    {
        private Label lblTitle = null!;
        private Label lblSurface1Header = null!;
        private Label lblSurface2Header = null!;
        private Label lblSurface1Data = null!;
        private Label lblSurface2Data = null!;
        private Label lblDistance = null!;
        private Label lblAngle = null!;

        private Panel separator1 = null!;
        private Panel separator2 = null!;
        private Panel separator3 = null!;

        private Form parentForm = null!;

        public FaceToFaceInfoPanel(Form parent)
        {
            parentForm = parent;
            InitializeForm();
            InitializeControls();
        }

        private void InitializeForm()
        {
            Text = "";
            Size = new Size(260, 280);  // ✅ 208 → 260 (geniş)
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            BackColor = Color.White;
            ForeColor = Color.Black;
            ShowInTaskbar = false;

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

            Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.DarkGray, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            };

            PositionPanel();

            if (parentForm != null)
            {
                parentForm.LocationChanged += (s, e) => PositionPanel();
                parentForm.SizeChanged += (s, e) => PositionPanel();
            }
        }

        private void PositionPanel()
        {
            if (parentForm != null)
            {
                int x = parentForm.Right - Width;
                int y = parentForm.Top + 60;
                Location = new Point(x, y);
            }
        }

        private void InitializeControls()
        {
            int padding = 5;
            int yPos = padding;

            lblTitle = new Label
            {
                Location = new Point(padding, yPos),
                Size = new Size(250, 18),  // ✅ 198 → 250
                Text = "Face Measurement",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None
            };
            Controls.Add(lblTitle);
            yPos += 19;

            separator1 = new Panel
            {
                Location = new Point(padding, yPos),
                Size = new Size(250, 1),  // ✅ 198 → 250
                BackColor = Color.DarkGray
            };
            Controls.Add(separator1);
            yPos += 2;

            lblSurface1Header = new Label
            {
                Location = new Point(padding, yPos),
                Size = new Size(120, 16),  // ✅ 94 → 120
                Text = "Surface 1",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None
            };
            Controls.Add(lblSurface1Header);

            lblSurface2Header = new Label
            {
                Location = new Point(130, yPos),  // ✅ 104 → 130
                Size = new Size(120, 16),  // ✅ 94 → 120
                Text = "Surface 2",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None
            };
            Controls.Add(lblSurface2Header);
            yPos += 17;

            separator2 = new Panel
            {
                Location = new Point(padding, yPos),
                Size = new Size(250, 1),  // ✅ 198 → 250
                BackColor = Color.DarkGray
            };
            Controls.Add(separator2);
            yPos += 2;

            // ✅ FONT: 8 → 9, GENİŞLİK: 94 → 120
            lblSurface1Data = new Label
            {
                Location = new Point(padding, yPos),
                Size = new Size(120, 180),  // ✅ 94 → 120
                Text = "",
                Font = new Font("Courier New", 9, FontStyle.Regular),  // ✅ 8 → 9
                TextAlign = ContentAlignment.TopLeft,
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(2)
            };
            Controls.Add(lblSurface1Data);

            // ✅ FONT: 8 → 9, GENİŞLİK: 94 → 120
            lblSurface2Data = new Label
            {
                Location = new Point(130, yPos),  // ✅ 104 → 130
                Size = new Size(120, 180),  // ✅ 94 → 120
                Text = "",
                Font = new Font("Courier New", 9, FontStyle.Regular),  // ✅ 8 → 9
                TextAlign = ContentAlignment.TopLeft,
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(2)
            };
            Controls.Add(lblSurface2Data);
            yPos += 182;

            separator3 = new Panel
            {
                Location = new Point(padding, yPos),
                Size = new Size(250, 1),  // ✅ 198 → 250
                BackColor = Color.DarkGray
            };
            Controls.Add(separator3);
            yPos += 2;

            // ✅ FONT: 8 → 9
            lblDistance = new Label
            {
                Location = new Point(padding, yPos),
                Size = new Size(250, 15),  // ✅ 198 → 250
                Text = "",
                Font = new Font("Courier New", 9, FontStyle.Regular),  // ✅ 8 → 9
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(2, 0, 0, 0)
            };
            Controls.Add(lblDistance);
            yPos += 16;

            // ✅ FONT: 8 → 9
            lblAngle = new Label
            {
                Location = new Point(padding, yPos),
                Size = new Size(250, 15),  // ✅ 198 → 250
                Text = "",
                Font = new Font("Courier New", 9, FontStyle.Regular),  // ✅ 8 → 9
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(2, 0, 0, 0)
            };
            Controls.Add(lblAngle);
        }

        private string FormatNumber(double value)
        {
            var culture = new CultureInfo("fr-FR");
            culture.NumberFormat.NumberDecimalSeparator = ".";
            return value.ToString("N3", culture);
        }

        private string FormatCylindricalData(double diameter,
            devDept.Geometry.Point3D topCenter,
            devDept.Geometry.Point3D bottomCenter)
        {
            return $"Cylindrical\n\n" +
                   $"D1 : {FormatNumber(diameter),8}\n" +
                   $"X1 : {FormatNumber(topCenter.X),8}\n" +
                   $"Y1 : {FormatNumber(topCenter.Y),8}\n" +
                   $"Z1 : {FormatNumber(topCenter.Z),8}\n\n" +
                   $"D2 : {FormatNumber(diameter),8}\n" +
                   $"X2 : {FormatNumber(bottomCenter.X),8}\n" +
                   $"Y2 : {FormatNumber(bottomCenter.Y),8}\n" +
                   $"Z2 : {FormatNumber(bottomCenter.Z),8}";
        }

        private string FormatPlanarData(double area)
        {
            return $"Planar\n\n" +
                   $"Area : {FormatNumber(area)}";
        }

        public void UpdateSurface1Cylinder(double diameter,
            devDept.Geometry.Point3D topCenter,
            devDept.Geometry.Point3D bottomCenter)
        {
            lblSurface1Data.Text = FormatCylindricalData(diameter, topCenter, bottomCenter);
            lblSurface2Data.Text = "Click second\nsurface...";
            lblDistance.Text = "";
            lblAngle.Text = "";
        }

        public void UpdateSurface1(double area)
        {
            lblSurface1Data.Text = FormatPlanarData(area);
            lblSurface2Data.Text = "Click second\nsurface...";
            lblDistance.Text = "";
            lblAngle.Text = "";
        }

        public void UpdateSurface2Cylinder(
            double diameter1, devDept.Geometry.Point3D topCenter1, devDept.Geometry.Point3D bottomCenter1,
            double diameter2, devDept.Geometry.Point3D topCenter2, devDept.Geometry.Point3D bottomCenter2,
            double distance, double angle)
        {
            lblSurface1Data.Text = FormatCylindricalData(diameter1, topCenter1, bottomCenter1);
            lblSurface2Data.Text = FormatCylindricalData(diameter2, topCenter2, bottomCenter2);
            lblDistance.Text = $"Min Distance : {FormatNumber(distance),8}";
            lblAngle.Text = $"Angle        : {angle,8:F2}°";
        }

        public void UpdateDistance(double area1, double area2, double distance, double angle)
        {
            lblSurface1Data.Text = FormatPlanarData(area1);
            lblSurface2Data.Text = FormatPlanarData(area2);
            lblDistance.Text = $"Min Distance : {FormatNumber(distance),8}";
            lblAngle.Text = $"Angle        : {angle,8:F2}°";
        }

        public void UpdateMixedSurfaces(
            bool isCylindrical1, double diameter1,
            devDept.Geometry.Point3D topCenter1, devDept.Geometry.Point3D bottomCenter1,
            bool isCylindrical2, double area2,
            double distance, double angle)
        {
            lblSurface1Data.Text = FormatCylindricalData(diameter1, topCenter1, bottomCenter1);
            lblSurface2Data.Text = FormatPlanarData(area2);
            lblDistance.Text = $"Min Distance : {FormatNumber(distance),8}";
            lblAngle.Text = $"Angle        : {angle,8:F2}°";
        }

        public void UpdateMixedSurfaces(
            bool isCylindrical1, double area1,
            bool isCylindrical2, double diameter2,
            devDept.Geometry.Point3D topCenter2, devDept.Geometry.Point3D bottomCenter2,
            double distance, double angle)
        {
            lblSurface1Data.Text = FormatPlanarData(area1);
            lblSurface2Data.Text = FormatCylindricalData(diameter2, topCenter2, bottomCenter2);
            lblDistance.Text = $"Min Distance : {FormatNumber(distance),8}";
            lblAngle.Text = $"Angle        : {angle,8:F2}°";
        }

        public void ShowWaitingMessage()
        {
            lblSurface1Data.Text = "Click first\nsurface...";
            lblSurface2Data.Text = "";
            lblDistance.Text = "";
            lblAngle.Text = "";
        }

        public new void Show()
        {
            PositionPanel();
            base.Show();
        }
    }
}