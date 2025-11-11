namespace _014
{
    partial class Form_New_Prob
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_New_Prob));
            devDept.Eyeshot.Control.CancelToolBarButton cancelToolBarButton1 = new devDept.Eyeshot.Control.CancelToolBarButton("Cancel", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.ProgressBar progressBar1 = new devDept.Eyeshot.Control.ProgressBar(devDept.Eyeshot.Control.ProgressBar.styleType.Speedometer, 0, "Idle", Color.Black, Color.Transparent, Color.Green, 1D, true, cancelToolBarButton1, false, 0.1D, 0.333D, true);
            devDept.Eyeshot.Control.BackgroundSettings backgroundSettings1 = new devDept.Eyeshot.Control.BackgroundSettings(devDept.Graphics.backgroundStyleType.LinearGradient, Color.FromArgb(245, 245, 245), Color.DodgerBlue, Color.FromArgb(102, 163, 210), 0.75D, null, devDept.Eyeshot.colorThemeType.Auto, 0.33D);
            devDept.Eyeshot.Camera camera1 = new devDept.Eyeshot.Camera(new devDept.Geometry.Point3D(0D, 0D, 45D), 380D, new devDept.Geometry.Quaternion(0.018434349666532526D, 0.039532590434972079D, 0.42221602280006187D, 0.90544518284475428D), devDept.Eyeshot.projectionType.Orthographic, 40D, 6.2900017056700159D, false, 0.001D);
            devDept.Eyeshot.Control.HomeToolBarButton homeToolBarButton1 = new devDept.Eyeshot.Control.HomeToolBarButton("Home", devDept.Eyeshot.Control.ToolBarButton.styleType.PushButton, true, true);
            devDept.Eyeshot.Control.MagnifyingGlassToolBarButton magnifyingGlassToolBarButton1 = new devDept.Eyeshot.Control.MagnifyingGlassToolBarButton("Magnifying Glass", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.ZoomWindowToolBarButton zoomWindowToolBarButton1 = new devDept.Eyeshot.Control.ZoomWindowToolBarButton("Zoom Window", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.ZoomToolBarButton zoomToolBarButton1 = new devDept.Eyeshot.Control.ZoomToolBarButton("Zoom", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.PanToolBarButton panToolBarButton1 = new devDept.Eyeshot.Control.PanToolBarButton("Pan", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.RotateToolBarButton rotateToolBarButton1 = new devDept.Eyeshot.Control.RotateToolBarButton("Rotate", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.ZoomFitToolBarButton zoomFitToolBarButton1 = new devDept.Eyeshot.Control.ZoomFitToolBarButton("Zoom Fit", devDept.Eyeshot.Control.ToolBarButton.styleType.PushButton, true, true);
            devDept.Eyeshot.Control.ToolBar toolBar1 = new devDept.Eyeshot.Control.ToolBar(devDept.Eyeshot.Control.ToolBar.positionType.HorizontalTopCenter, true, new devDept.Eyeshot.Control.ToolBarButton[] { homeToolBarButton1, magnifyingGlassToolBarButton1, zoomWindowToolBarButton1, zoomToolBarButton1, panToolBarButton1, rotateToolBarButton1, zoomFitToolBarButton1 }, 3, 0, Color.FromArgb(0, 0, 0, 0), 0D, Color.FromArgb(0, 0, 0, 0), 0D);
            devDept.Eyeshot.Control.LegendItem legendItem1 = new devDept.Eyeshot.Control.LegendItem(10, 30, Color.FromArgb(0, 0, 255));
            devDept.Eyeshot.Control.LegendItem legendItem2 = new devDept.Eyeshot.Control.LegendItem(10, 30, Color.FromArgb(0, 127, 255));
            devDept.Eyeshot.Control.LegendItem legendItem3 = new devDept.Eyeshot.Control.LegendItem(10, 30, Color.FromArgb(0, 255, 255));
            devDept.Eyeshot.Control.LegendItem legendItem4 = new devDept.Eyeshot.Control.LegendItem(10, 30, Color.FromArgb(0, 255, 127));
            devDept.Eyeshot.Control.LegendItem legendItem5 = new devDept.Eyeshot.Control.LegendItem(10, 30, Color.FromArgb(0, 255, 0));
            devDept.Eyeshot.Control.LegendItem legendItem6 = new devDept.Eyeshot.Control.LegendItem(10, 30, Color.FromArgb(127, 255, 0));
            devDept.Eyeshot.Control.LegendItem legendItem7 = new devDept.Eyeshot.Control.LegendItem(10, 30, Color.FromArgb(255, 255, 0));
            devDept.Eyeshot.Control.LegendItem legendItem8 = new devDept.Eyeshot.Control.LegendItem(10, 30, Color.FromArgb(255, 127, 0));
            devDept.Eyeshot.Control.LegendItem legendItem9 = new devDept.Eyeshot.Control.LegendItem(10, 30, Color.FromArgb(255, 0, 0));
            devDept.Eyeshot.Control.Legend legend1 = new devDept.Eyeshot.Control.Legend(0D, 100D, "Title", "Subtitle", new Point(24, 24), true, false, false, "{0:+0.###;-0.###;0}", Color.Transparent, Color.Black, Color.Black, null, null, new devDept.Eyeshot.Control.LegendItem[] { legendItem1, legendItem2, legendItem3, legendItem4, legendItem5, legendItem6, legendItem7, legendItem8, legendItem9 }, true, false, false, 0);
            devDept.Eyeshot.Control.Histogram histogram1 = new devDept.Eyeshot.Control.Histogram(30, 80, "Title", Color.Blue, Color.Gray, Color.Black, Color.Red, Color.LightYellow, false, true, false, "{0:+0.###;-0.###;0}");
            devDept.Eyeshot.Control.Grid grid1 = new devDept.Eyeshot.Control.Grid(new devDept.Geometry.Point2D(-100D, -100D), new devDept.Geometry.Point2D(100D, 100D), 30D, new devDept.Geometry.Plane(new devDept.Geometry.Point3D(0D, 0D, 0D), new devDept.Geometry.Vector3D(1D, 0D, 0D), new devDept.Geometry.Vector3D(0D, 1D, 0D)), Color.FromArgb(63, 128, 128, 128), Color.FromArgb(127, 255, 0, 0), Color.FromArgb(127, 0, 128, 0), false, true, false, false, 30, 100, 10, Color.FromArgb(127, 90, 90, 90), Color.Transparent, false, Color.FromArgb(12, 0, 0, 255));
            devDept.Eyeshot.Control.OriginSymbol originSymbol1 = new devDept.Eyeshot.Control.OriginSymbol(10, devDept.Eyeshot.Control.originSymbolStyleType.Ball, new Font("Segoe UI", 9F), Color.Black, Color.Black, Color.Black, Color.Black, Color.Red, Color.Green, Color.Blue, "Origin", "X", "Y", "Z", true, null, false);
            devDept.Eyeshot.Control.RotateSettings rotateSettings1 = new devDept.Eyeshot.Control.RotateSettings(new devDept.Eyeshot.Control.MouseButton(devDept.Eyeshot.Control.mouseButtonsZPR.Middle, devDept.Eyeshot.Control.modifierKeys.None), 10D, true, 1D, devDept.Eyeshot.rotationType.Trackball, devDept.Eyeshot.rotationCenterType.CursorLocation, new devDept.Geometry.Point3D(0D, 0D, 0D), false);
            devDept.Eyeshot.Control.ZoomSettings zoomSettings1 = new devDept.Eyeshot.Control.ZoomSettings(new devDept.Eyeshot.Control.MouseButton(devDept.Eyeshot.Control.mouseButtonsZPR.Middle, devDept.Eyeshot.Control.modifierKeys.Shift), 25, true, devDept.Eyeshot.zoomStyleType.AtCursorLocation, false, 1D, Color.Empty, devDept.Eyeshot.Camera.perspectiveFitType.Accurate, false, 10, true);
            devDept.Eyeshot.Control.PanSettings panSettings1 = new devDept.Eyeshot.Control.PanSettings(new devDept.Eyeshot.Control.MouseButton(devDept.Eyeshot.Control.mouseButtonsZPR.Middle, devDept.Eyeshot.Control.modifierKeys.Ctrl), 25, true);
            devDept.Eyeshot.Control.NavigationSettings navigationSettings1 = new devDept.Eyeshot.Control.NavigationSettings(devDept.Eyeshot.Camera.navigationType.Examine, new devDept.Eyeshot.Control.MouseButton(devDept.Eyeshot.Control.mouseButtonsZPR.Left, devDept.Eyeshot.Control.modifierKeys.None), new devDept.Geometry.Point3D(-1000D, -1000D, -1000D), new devDept.Geometry.Point3D(1000D, 1000D, 1000D), 8D, 50D, 50D);
            devDept.Eyeshot.Control.CoordinateSystemIcon coordinateSystemIcon1 = new devDept.Eyeshot.Control.CoordinateSystemIcon(new Font("Segoe UI", 9F), Color.Black, Color.Black, Color.Black, Color.Black, Color.FromArgb(80, 80, 80), Color.FromArgb(80, 80, 80), Color.OrangeRed, "Origin", "X", "Y", "Z", true, devDept.Eyeshot.Control.coordinateSystemPositionType.BottomLeft, 37, null, false);
            devDept.Eyeshot.Control.ViewCubeIcon viewCubeIcon1 = new devDept.Eyeshot.Control.ViewCubeIcon(devDept.Eyeshot.Control.coordinateSystemPositionType.TopRight, true, Color.FromArgb(220, 20, 60), true, "FRONT", "BACK", "LEFT", "RIGHT", "TOP", "BOTTOM", Color.FromArgb(240, 77, 77, 77), Color.FromArgb(240, 77, 77, 77), Color.FromArgb(240, 77, 77, 77), Color.FromArgb(240, 77, 77, 77), Color.FromArgb(240, 77, 77, 77), Color.FromArgb(240, 77, 77, 77), 'S', 'N', 'W', 'E', true, null, Color.White, Color.Black, 120, true, true, null, null, null, null, null, null, false, new devDept.Geometry.Quaternion(0D, 0D, 0D, 1D), true);
            devDept.Eyeshot.Control.ScaleBar scaleBar1 = new devDept.Eyeshot.Control.ScaleBar(false, Color.Black, Color.White, Color.Black, "{0:G6}", false, devDept.Eyeshot.Control.ScaleBar.styleType.Alternate, 4, 15, 0.4D, devDept.Eyeshot.Control.ScaleBar.positionType.BottomCenter, new Font("Segoe UI", 9F), devDept.Geometry.linearUnitsType.Meters);
            devDept.Eyeshot.Control.Viewport viewport1 = new devDept.Eyeshot.Control.Viewport(new Point(0, 0), new Size(456, 629), backgroundSettings1, camera1, new devDept.Eyeshot.Control.ToolBar[] { toolBar1 }, new devDept.Eyeshot.Control.Legend[] { legend1 }, histogram1, devDept.Eyeshot.displayType.Rendered, true, false, false, new devDept.Eyeshot.Control.Grid[] { grid1 }, new devDept.Eyeshot.Control.OriginSymbol[] { originSymbol1 }, false, rotateSettings1, zoomSettings1, panSettings1, navigationSettings1, coordinateSystemIcon1, viewCubeIcon1, scaleBar1);
            table_New_prob = new TableLayoutPanel();
            label4 = new Label();
            numeric_new_probe_L3 = new NumericUpDown();
            numeric_new_probe_L2 = new NumericUpDown();
            label5 = new Label();
            numeric_new_probe_L1 = new NumericUpDown();
            numeric_new_probe_d2 = new NumericUpDown();
            numeric_new_probe_d1 = new NumericUpDown();
            numeric_new_probe_D = new NumericUpDown();
            label1 = new Label();
            lbl_new_probe_name = new Label();
            txt_new_probe_name = new TextBox();
            lbl_new_probe_D = new Label();
            label2 = new Label();
            label3 = new Label();
            btn_new_probe_save = new Button();
            numericUpDown1 = new NumericUpDown();
            label6 = new Label();
            pic_new_probe = new PictureBox();
            design_new_probe = new devDept.Eyeshot.Control.Design();
            dataGrid_new_prob = new DataGridView();
            Columd_new_probe_name = new DataGridViewTextBoxColumn();
            Columd_new_probe_D = new DataGridViewTextBoxColumn();
            Columd_new_probe_d1 = new DataGridViewTextBoxColumn();
            Columd_new_probe_d2 = new DataGridViewTextBoxColumn();
            Columd_new_probe_L1 = new DataGridViewTextBoxColumn();
            Columd_new_probe_L2 = new DataGridViewTextBoxColumn();
            Columd_new_probe_L3 = new DataGridViewTextBoxColumn();
            panel1 = new Panel();
            panel2 = new Panel();
            panel3 = new Panel();
            panel4 = new Panel();
            checkBox_lamp_prob_2 = new CheckBox();
            trackBar_lamp_probe = new TrackBar();
            panel5 = new Panel();
            panel7 = new Panel();
            panel6 = new Panel();
            table_New_prob.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_L3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_L2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_L1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_d2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_d1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_D).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pic_new_probe).BeginInit();
            ((System.ComponentModel.ISupportInitialize)design_new_probe).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGrid_new_prob).BeginInit();
            panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar_lamp_probe).BeginInit();
            panel5.SuspendLayout();
            panel6.SuspendLayout();
            SuspendLayout();
            // 
            // table_New_prob
            // 
            table_New_prob.ColumnCount = 3;
            table_New_prob.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            table_New_prob.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 106F));
            table_New_prob.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 15F));
            table_New_prob.Controls.Add(label4, 0, 5);
            table_New_prob.Controls.Add(numeric_new_probe_L3, 1, 6);
            table_New_prob.Controls.Add(numeric_new_probe_L2, 1, 5);
            table_New_prob.Controls.Add(label5, 0, 6);
            table_New_prob.Controls.Add(numeric_new_probe_L1, 1, 4);
            table_New_prob.Controls.Add(numeric_new_probe_d2, 1, 3);
            table_New_prob.Controls.Add(numeric_new_probe_d1, 1, 2);
            table_New_prob.Controls.Add(numeric_new_probe_D, 1, 1);
            table_New_prob.Controls.Add(label1, 0, 2);
            table_New_prob.Controls.Add(lbl_new_probe_name, 0, 0);
            table_New_prob.Controls.Add(txt_new_probe_name, 1, 0);
            table_New_prob.Controls.Add(lbl_new_probe_D, 0, 1);
            table_New_prob.Controls.Add(label2, 0, 3);
            table_New_prob.Controls.Add(label3, 0, 4);
            table_New_prob.Controls.Add(btn_new_probe_save, 1, 7);
            table_New_prob.Location = new Point(12, 23);
            table_New_prob.Name = "table_New_prob";
            table_New_prob.RowCount = 8;
            table_New_prob.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            table_New_prob.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            table_New_prob.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            table_New_prob.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            table_New_prob.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            table_New_prob.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            table_New_prob.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            table_New_prob.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            table_New_prob.Size = new Size(219, 252);
            table_New_prob.TabIndex = 0;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(3, 150);
            label4.Name = "label4";
            label4.Size = new Size(94, 30);
            label4.TabIndex = 9;
            label4.Text = "L2";
            label4.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numeric_new_probe_L3
            // 
            numeric_new_probe_L3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            numeric_new_probe_L3.Location = new Point(103, 183);
            numeric_new_probe_L3.Maximum = new decimal(new int[] { 15, 0, 0, 0 });
            numeric_new_probe_L3.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            numeric_new_probe_L3.MinimumSize = new Size(10, 0);
            numeric_new_probe_L3.Name = "numeric_new_probe_L3";
            numeric_new_probe_L3.Size = new Size(100, 23);
            numeric_new_probe_L3.TabIndex = 3;
            numeric_new_probe_L3.TextAlign = HorizontalAlignment.Right;
            numeric_new_probe_L3.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // numeric_new_probe_L2
            // 
            numeric_new_probe_L2.Dock = DockStyle.Fill;
            numeric_new_probe_L2.Location = new Point(103, 153);
            numeric_new_probe_L2.Minimum = new decimal(new int[] { 30, 0, 0, 0 });
            numeric_new_probe_L2.MinimumSize = new Size(10, 0);
            numeric_new_probe_L2.Name = "numeric_new_probe_L2";
            numeric_new_probe_L2.Size = new Size(100, 23);
            numeric_new_probe_L2.TabIndex = 4;
            numeric_new_probe_L2.TextAlign = HorizontalAlignment.Right;
            numeric_new_probe_L2.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Dock = DockStyle.Fill;
            label5.Location = new Point(3, 180);
            label5.Name = "label5";
            label5.Size = new Size(94, 30);
            label5.TabIndex = 8;
            label5.Text = "L3";
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numeric_new_probe_L1
            // 
            numeric_new_probe_L1.Dock = DockStyle.Fill;
            numeric_new_probe_L1.Location = new Point(103, 123);
            numeric_new_probe_L1.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            numeric_new_probe_L1.MinimumSize = new Size(10, 0);
            numeric_new_probe_L1.Name = "numeric_new_probe_L1";
            numeric_new_probe_L1.Size = new Size(100, 23);
            numeric_new_probe_L1.TabIndex = 5;
            numeric_new_probe_L1.TextAlign = HorizontalAlignment.Right;
            numeric_new_probe_L1.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // numeric_new_probe_d2
            // 
            numeric_new_probe_d2.Dock = DockStyle.Fill;
            numeric_new_probe_d2.Location = new Point(103, 93);
            numeric_new_probe_d2.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numeric_new_probe_d2.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            numeric_new_probe_d2.MinimumSize = new Size(10, 0);
            numeric_new_probe_d2.Name = "numeric_new_probe_d2";
            numeric_new_probe_d2.Size = new Size(100, 23);
            numeric_new_probe_d2.TabIndex = 3;
            numeric_new_probe_d2.TextAlign = HorizontalAlignment.Right;
            numeric_new_probe_d2.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // numeric_new_probe_d1
            // 
            numeric_new_probe_d1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            numeric_new_probe_d1.Location = new Point(103, 63);
            numeric_new_probe_d1.Maximum = new decimal(new int[] { 15, 0, 0, 0 });
            numeric_new_probe_d1.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numeric_new_probe_d1.MinimumSize = new Size(10, 0);
            numeric_new_probe_d1.Name = "numeric_new_probe_d1";
            numeric_new_probe_d1.Size = new Size(100, 23);
            numeric_new_probe_d1.TabIndex = 3;
            numeric_new_probe_d1.TextAlign = HorizontalAlignment.Right;
            numeric_new_probe_d1.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // numeric_new_probe_D
            // 
            numeric_new_probe_D.Dock = DockStyle.Fill;
            numeric_new_probe_D.Location = new Point(103, 33);
            numeric_new_probe_D.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numeric_new_probe_D.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            numeric_new_probe_D.MinimumSize = new Size(10, 0);
            numeric_new_probe_D.Name = "numeric_new_probe_D";
            numeric_new_probe_D.Size = new Size(100, 23);
            numeric_new_probe_D.TabIndex = 1;
            numeric_new_probe_D.TextAlign = HorizontalAlignment.Right;
            numeric_new_probe_D.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.ImageAlign = ContentAlignment.MiddleLeft;
            label1.Location = new Point(3, 60);
            label1.Name = "label1";
            label1.Size = new Size(94, 30);
            label1.TabIndex = 3;
            label1.Text = "d1";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lbl_new_probe_name
            // 
            lbl_new_probe_name.AutoSize = true;
            lbl_new_probe_name.Dock = DockStyle.Fill;
            lbl_new_probe_name.Location = new Point(3, 0);
            lbl_new_probe_name.Name = "lbl_new_probe_name";
            lbl_new_probe_name.Size = new Size(94, 30);
            lbl_new_probe_name.TabIndex = 0;
            lbl_new_probe_name.Text = "Name";
            lbl_new_probe_name.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txt_new_probe_name
            // 
            txt_new_probe_name.Dock = DockStyle.Fill;
            txt_new_probe_name.Location = new Point(103, 3);
            txt_new_probe_name.Name = "txt_new_probe_name";
            txt_new_probe_name.Size = new Size(100, 23);
            txt_new_probe_name.TabIndex = 1;
            txt_new_probe_name.TextAlign = HorizontalAlignment.Right;
            // 
            // lbl_new_probe_D
            // 
            lbl_new_probe_D.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lbl_new_probe_D.AutoSize = true;
            lbl_new_probe_D.ImageAlign = ContentAlignment.MiddleLeft;
            lbl_new_probe_D.Location = new Point(3, 30);
            lbl_new_probe_D.Name = "lbl_new_probe_D";
            lbl_new_probe_D.Size = new Size(94, 30);
            lbl_new_probe_D.TabIndex = 2;
            lbl_new_probe_D.Text = "D";
            lbl_new_probe_D.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(3, 90);
            label2.Name = "label2";
            label2.Size = new Size(94, 30);
            label2.TabIndex = 5;
            label2.Text = "d2";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Location = new Point(3, 120);
            label3.Name = "label3";
            label3.Size = new Size(94, 30);
            label3.TabIndex = 6;
            label3.Text = "L1";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btn_new_probe_save
            // 
            btn_new_probe_save.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btn_new_probe_save.Image = (Image)resources.GetObject("btn_new_probe_save.Image");
            btn_new_probe_save.Location = new Point(163, 213);
            btn_new_probe_save.Name = "btn_new_probe_save";
            btn_new_probe_save.Size = new Size(40, 36);
            btn_new_probe_save.TabIndex = 11;
            btn_new_probe_save.UseVisualStyleBackColor = true;
            btn_new_probe_save.Click += btn_new_probe_save_Click;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            numericUpDown1.Location = new Point(-259, 12);
            numericUpDown1.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            numericUpDown1.MinimumSize = new Size(10, 0);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(100, 23);
            numericUpDown1.TabIndex = 2;
            numericUpDown1.TextAlign = HorizontalAlignment.Right;
            numericUpDown1.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(184, 20);
            label6.Name = "label6";
            label6.Size = new Size(19, 15);
            label6.TabIndex = 9;
            label6.Text = "L1";
            label6.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pic_new_probe
            // 
            pic_new_probe.Image = (Image)resources.GetObject("pic_new_probe.Image");
            pic_new_probe.InitialImage = (Image)resources.GetObject("pic_new_probe.InitialImage");
            pic_new_probe.Location = new Point(12, 326);
            pic_new_probe.Name = "pic_new_probe";
            pic_new_probe.Size = new Size(219, 302);
            pic_new_probe.SizeMode = PictureBoxSizeMode.StretchImage;
            pic_new_probe.TabIndex = 10;
            pic_new_probe.TabStop = false;
            // 
            // design_new_probe
            // 
            design_new_probe.Dock = DockStyle.Fill;
            design_new_probe.Font = new Font("Segoe UI", 9F);
            design_new_probe.Location = new Point(0, 0);
            design_new_probe.Name = "design_new_probe";
            design_new_probe.ProgressBar = progressBar1;
            design_new_probe.Size = new Size(456, 629);
            design_new_probe.TabIndex = 11;
            design_new_probe.Text = "design1";
            design_new_probe.Viewports.Add(viewport1);
            // 
            // dataGrid_new_prob
            // 
            dataGrid_new_prob.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGrid_new_prob.Columns.AddRange(new DataGridViewColumn[] { Columd_new_probe_name, Columd_new_probe_D, Columd_new_probe_d1, Columd_new_probe_d2, Columd_new_probe_L1, Columd_new_probe_L2, Columd_new_probe_L3 });
            dataGrid_new_prob.Dock = DockStyle.Fill;
            dataGrid_new_prob.Location = new Point(0, 0);
            dataGrid_new_prob.Name = "dataGrid_new_prob";
            dataGrid_new_prob.Size = new Size(456, 143);
            dataGrid_new_prob.TabIndex = 12;
            // 
            // Columd_new_probe_name
            // 
            Columd_new_probe_name.HeaderText = "Name";
            Columd_new_probe_name.Name = "Columd_new_probe_name";
            Columd_new_probe_name.Width = 150;
            // 
            // Columd_new_probe_D
            // 
            Columd_new_probe_D.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            Columd_new_probe_D.HeaderText = "D";
            Columd_new_probe_D.Name = "Columd_new_probe_D";
            Columd_new_probe_D.Width = 40;
            // 
            // Columd_new_probe_d1
            // 
            Columd_new_probe_d1.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            Columd_new_probe_d1.HeaderText = "d1";
            Columd_new_probe_d1.Name = "Columd_new_probe_d1";
            Columd_new_probe_d1.Width = 45;
            // 
            // Columd_new_probe_d2
            // 
            Columd_new_probe_d2.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            Columd_new_probe_d2.HeaderText = "d2";
            Columd_new_probe_d2.Name = "Columd_new_probe_d2";
            Columd_new_probe_d2.Width = 45;
            // 
            // Columd_new_probe_L1
            // 
            Columd_new_probe_L1.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            Columd_new_probe_L1.HeaderText = "L1";
            Columd_new_probe_L1.Name = "Columd_new_probe_L1";
            Columd_new_probe_L1.Width = 44;
            // 
            // Columd_new_probe_L2
            // 
            Columd_new_probe_L2.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            Columd_new_probe_L2.HeaderText = "L2";
            Columd_new_probe_L2.Name = "Columd_new_probe_L2";
            Columd_new_probe_L2.Width = 44;
            // 
            // Columd_new_probe_L3
            // 
            Columd_new_probe_L3.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            Columd_new_probe_L3.HeaderText = "L3";
            Columd_new_probe_L3.Name = "Columd_new_probe_L3";
            Columd_new_probe_L3.Width = 44;
            // 
            // panel1
            // 
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 799);
            panel1.Name = "panel1";
            panel1.Size = new Size(730, 20);
            panel1.TabIndex = 13;
            // 
            // panel2
            // 
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(730, 20);
            panel2.TabIndex = 14;
            // 
            // panel3
            // 
            panel3.Dock = DockStyle.Right;
            panel3.Location = new Point(696, 20);
            panel3.Name = "panel3";
            panel3.Size = new Size(34, 779);
            panel3.TabIndex = 15;
            // 
            // panel4
            // 
            panel4.Controls.Add(checkBox_lamp_prob_2);
            panel4.Controls.Add(trackBar_lamp_probe);
            panel4.Controls.Add(table_New_prob);
            panel4.Controls.Add(pic_new_probe);
            panel4.Dock = DockStyle.Left;
            panel4.Location = new Point(0, 20);
            panel4.Name = "panel4";
            panel4.Size = new Size(240, 779);
            panel4.TabIndex = 14;
            // 
            // checkBox_lamp_prob_2
            // 
            checkBox_lamp_prob_2.AutoSize = true;
            checkBox_lamp_prob_2.Location = new Point(12, 281);
            checkBox_lamp_prob_2.Name = "checkBox_lamp_prob_2";
            checkBox_lamp_prob_2.Size = new Size(90, 19);
            checkBox_lamp_prob_2.TabIndex = 13;
            checkBox_lamp_prob_2.Text = "Probe Lamp";
            checkBox_lamp_prob_2.UseVisualStyleBackColor = true;
            checkBox_lamp_prob_2.CheckedChanged += checkBox_lamp_prob_2_CheckedChanged;
            // 
            // trackBar_lamp_probe
            // 
            trackBar_lamp_probe.Location = new Point(98, 277);
            trackBar_lamp_probe.Name = "trackBar_lamp_probe";
            trackBar_lamp_probe.Size = new Size(136, 45);
            trackBar_lamp_probe.TabIndex = 11;
            trackBar_lamp_probe.ValueChanged += trackBar_lamp_probe_ValueChanged;
            // 
            // panel5
            // 
            panel5.Controls.Add(design_new_probe);
            panel5.Controls.Add(panel7);
            panel5.Dock = DockStyle.Fill;
            panel5.Location = new Point(240, 20);
            panel5.Name = "panel5";
            panel5.Size = new Size(456, 779);
            panel5.TabIndex = 16;
            // 
            // panel7
            // 
            panel7.Dock = DockStyle.Bottom;
            panel7.Location = new Point(0, 629);
            panel7.Name = "panel7";
            panel7.Size = new Size(456, 150);
            panel7.TabIndex = 12;
            // 
            // panel6
            // 
            panel6.Controls.Add(dataGrid_new_prob);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(240, 656);
            panel6.Name = "panel6";
            panel6.Size = new Size(456, 143);
            panel6.TabIndex = 17;
            // 
            // Form_New_Prob
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(730, 819);
            Controls.Add(panel6);
            Controls.Add(panel5);
            Controls.Add(panel4);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(label6);
            Controls.Add(numericUpDown1);
            Name = "Form_New_Prob";
            Text = "Form_New_Prob";
            table_New_prob.ResumeLayout(false);
            table_New_prob.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_L3).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_L2).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_L1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_d2).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_d1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_new_probe_D).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pic_new_probe).EndInit();
            ((System.ComponentModel.ISupportInitialize)design_new_probe).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGrid_new_prob).EndInit();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar_lamp_probe).EndInit();
            panel5.ResumeLayout(false);
            panel6.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel table_New_prob;
        private Label lbl_new_probe_name;
        private TextBox txt_new_probe_name;
        private Label lbl_new_probe_D;
        private ComboBox cmb_new_probe_D;
        private Label label1;
        private TextBox txt_new_probe_d1;
        private Label label2;
        private TextBox txt_new_probe_d3;
        private NumericUpDown numeric_new_probe_D;
        private NumericUpDown numeric_new_probe_d1;
        private NumericUpDown numericUpDown1;
        private Label label4;
        private NumericUpDown numeric_new_probe_L1;
        private NumericUpDown numeric_new_probe_d2;
        private Label label3;
        private NumericUpDown numeric_new_probe_L3;
        private NumericUpDown numeric_new_probe_L2;
        private Label label5;
        private Label label6;
        public PictureBox pic_new_probe;
        private devDept.Eyeshot.Control.Design design_new_probe;
        private DataGridView dataGrid_new_prob;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private Panel panel4;
        private Panel panel5;
        private Panel panel6;
        private Button btn_new_probe_save;
        private Panel panel7;
        private DataGridViewTextBoxColumn Columd_new_probe_name;
        private DataGridViewTextBoxColumn Columd_new_probe_D;
        private DataGridViewTextBoxColumn Columd_new_probe_d1;
        private DataGridViewTextBoxColumn Columd_new_probe_d2;
        private DataGridViewTextBoxColumn Columd_new_probe_L1;
        private DataGridViewTextBoxColumn Columd_new_probe_L2;
        private DataGridViewTextBoxColumn Columd_new_probe_L3;
        private TrackBar trackBar_lamp_probe;
        private CheckBox checkBox_lamp_prob_2;
    }
}