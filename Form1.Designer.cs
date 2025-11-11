namespace _014
{
    partial class CNC_Measurement
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            devDept.Eyeshot.Control.CancelToolBarButton cancelToolBarButton1 = new devDept.Eyeshot.Control.CancelToolBarButton("Cancel", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.ProgressBar progressBar1 = new devDept.Eyeshot.Control.ProgressBar(devDept.Eyeshot.Control.ProgressBar.styleType.Speedometer, 0, "Idle", Color.Black, Color.Transparent, Color.Green, 1D, true, cancelToolBarButton1, false, 0.1D, 0.333D, true);
            devDept.Eyeshot.Control.BackgroundSettings backgroundSettings1 = new devDept.Eyeshot.Control.BackgroundSettings(devDept.Graphics.backgroundStyleType.LinearGradient, Color.FromArgb(245, 245, 245), Color.DodgerBlue, Color.FromArgb(102, 163, 210), 0.75D, null, devDept.Eyeshot.colorThemeType.Auto, 0.33D);
            devDept.Eyeshot.Camera camera1 = new devDept.Eyeshot.Camera(new devDept.Geometry.Point3D(0D, 0D, 45D), 380D, new devDept.Geometry.Quaternion(0.018434349666532526D, 0.039532590434972079D, 0.42221602280006187D, 0.90544518284475428D), devDept.Eyeshot.projectionType.Orthographic, 40D, 6.042881712309689D, false, 0.001D);
            devDept.Eyeshot.Control.HomeToolBarButton homeToolBarButton1 = new devDept.Eyeshot.Control.HomeToolBarButton("Home", devDept.Eyeshot.Control.ToolBarButton.styleType.PushButton, true, true);
            devDept.Eyeshot.Control.MagnifyingGlassToolBarButton magnifyingGlassToolBarButton1 = new devDept.Eyeshot.Control.MagnifyingGlassToolBarButton("Magnifying Glass", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.ZoomWindowToolBarButton zoomWindowToolBarButton1 = new devDept.Eyeshot.Control.ZoomWindowToolBarButton("Zoom Window", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.ZoomToolBarButton zoomToolBarButton1 = new devDept.Eyeshot.Control.ZoomToolBarButton("Zoom", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.PanToolBarButton panToolBarButton1 = new devDept.Eyeshot.Control.PanToolBarButton("Pan", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.RotateToolBarButton rotateToolBarButton1 = new devDept.Eyeshot.Control.RotateToolBarButton("Rotate", devDept.Eyeshot.Control.ToolBarButton.styleType.ToggleButton, true, true);
            devDept.Eyeshot.Control.ZoomFitToolBarButton zoomFitToolBarButton1 = new devDept.Eyeshot.Control.ZoomFitToolBarButton("Zoom Fit", devDept.Eyeshot.Control.ToolBarButton.styleType.PushButton, true, true);
            devDept.Eyeshot.Control.ToolBar toolBar1 = new devDept.Eyeshot.Control.ToolBar(devDept.Eyeshot.Control.ToolBar.positionType.HorizontalTopCenter, true, new devDept.Eyeshot.Control.ToolBarButton[] { homeToolBarButton1, magnifyingGlassToolBarButton1, zoomWindowToolBarButton1, zoomToolBarButton1, panToolBarButton1, rotateToolBarButton1, zoomFitToolBarButton1 }, 5, 0, Color.FromArgb(0, 0, 0, 0), 0D, Color.FromArgb(0, 0, 0, 0), 0D);
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
            devDept.Eyeshot.Control.Grid grid1 = new devDept.Eyeshot.Control.Grid(new devDept.Geometry.Point2D(-100D, -100D), new devDept.Geometry.Point2D(100D, 100D), 30D, new devDept.Geometry.Plane(new devDept.Geometry.Point3D(0D, 0D, 0D), new devDept.Geometry.Vector3D(1D, 0D, 0D), new devDept.Geometry.Vector3D(0D, 1D, 0D)), Color.FromArgb(63, 128, 128, 128), Color.FromArgb(127, 255, 0, 0), Color.FromArgb(127, 0, 128, 0), false, true, false, false, 10, 100, 10, Color.FromArgb(127, 90, 90, 90), Color.Transparent, false, Color.FromArgb(12, 0, 0, 255));
            devDept.Eyeshot.Control.OriginSymbol originSymbol1 = new devDept.Eyeshot.Control.OriginSymbol(5, devDept.Eyeshot.Control.originSymbolStyleType.Ball, new Font("Segoe UI", 9F), Color.Black, Color.Black, Color.Black, Color.Black, Color.Red, Color.Green, Color.Blue, "Origin", "X", "Y", "Z", true, null, false);
            devDept.Eyeshot.Control.RotateSettings rotateSettings1 = new devDept.Eyeshot.Control.RotateSettings(new devDept.Eyeshot.Control.MouseButton(devDept.Eyeshot.Control.mouseButtonsZPR.Middle, devDept.Eyeshot.Control.modifierKeys.None), 10D, true, 1D, devDept.Eyeshot.rotationType.Trackball, devDept.Eyeshot.rotationCenterType.CursorLocation, new devDept.Geometry.Point3D(0D, 0D, 0D), false);
            devDept.Eyeshot.Control.ZoomSettings zoomSettings1 = new devDept.Eyeshot.Control.ZoomSettings(new devDept.Eyeshot.Control.MouseButton(devDept.Eyeshot.Control.mouseButtonsZPR.Middle, devDept.Eyeshot.Control.modifierKeys.Shift), 25, true, devDept.Eyeshot.zoomStyleType.AtCursorLocation, false, 1D, Color.Empty, devDept.Eyeshot.Camera.perspectiveFitType.Accurate, false, 10, true);
            devDept.Eyeshot.Control.PanSettings panSettings1 = new devDept.Eyeshot.Control.PanSettings(new devDept.Eyeshot.Control.MouseButton(devDept.Eyeshot.Control.mouseButtonsZPR.Middle, devDept.Eyeshot.Control.modifierKeys.Ctrl), 25, true);
            devDept.Eyeshot.Control.NavigationSettings navigationSettings1 = new devDept.Eyeshot.Control.NavigationSettings(devDept.Eyeshot.Camera.navigationType.Examine, new devDept.Eyeshot.Control.MouseButton(devDept.Eyeshot.Control.mouseButtonsZPR.Left, devDept.Eyeshot.Control.modifierKeys.None), new devDept.Geometry.Point3D(-1000D, -1000D, -1000D), new devDept.Geometry.Point3D(1000D, 1000D, 1000D), 8D, 50D, 50D);
            devDept.Eyeshot.Control.CoordinateSystemIcon coordinateSystemIcon1 = new devDept.Eyeshot.Control.CoordinateSystemIcon(new Font("Segoe UI", 9F), Color.Black, Color.Black, Color.Black, Color.Black, Color.FromArgb(80, 80, 80), Color.FromArgb(80, 80, 80), Color.OrangeRed, "Origin", "X", "Y", "Z", true, devDept.Eyeshot.Control.coordinateSystemPositionType.BottomLeft, 37, null, false);
            devDept.Eyeshot.Control.ViewCubeIcon viewCubeIcon1 = new devDept.Eyeshot.Control.ViewCubeIcon(devDept.Eyeshot.Control.coordinateSystemPositionType.TopRight, true, Color.FromArgb(220, 20, 60), true, "FRONT", "BACK", "LEFT", "RIGHT", "TOP", "BOTTOM", Color.FromArgb(240, 77, 77, 77), Color.FromArgb(240, 77, 77, 77), Color.FromArgb(240, 77, 77, 77), Color.FromArgb(240, 77, 77, 77), Color.FromArgb(240, 77, 77, 77), Color.FromArgb(240, 77, 77, 77), 'S', 'N', 'W', 'E', true, null, Color.White, Color.Black, 120, true, true, null, null, null, null, null, null, false, new devDept.Geometry.Quaternion(0D, 0D, 0D, 1D), true);
            devDept.Eyeshot.Control.ScaleBar scaleBar1 = new devDept.Eyeshot.Control.ScaleBar(false, Color.Black, Color.White, Color.Black, "{0:G6}", false, devDept.Eyeshot.Control.ScaleBar.styleType.Alternate, 4, 15, 0.4D, devDept.Eyeshot.Control.ScaleBar.positionType.BottomCenter, null, devDept.Geometry.linearUnitsType.Meters);
            devDept.Eyeshot.Control.Viewport viewport1 = new devDept.Eyeshot.Control.Viewport(new Point(0, 0), new Size(852, 518), backgroundSettings1, camera1, new devDept.Eyeshot.Control.ToolBar[] { toolBar1 }, new devDept.Eyeshot.Control.Legend[] { legend1 }, histogram1, devDept.Eyeshot.displayType.Rendered, true, false, false, new devDept.Eyeshot.Control.Grid[] { grid1 }, new devDept.Eyeshot.Control.OriginSymbol[] { originSymbol1 }, false, rotateSettings1, zoomSettings1, panSettings1, navigationSettings1, coordinateSystemIcon1, viewCubeIcon1, scaleBar1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CNC_Measurement));
            design1 = new devDept.Eyeshot.Control.Design();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            openInNewWindowToolStripMenuItem = new ToolStripMenuItem();
            closeToolStripMenuItem = new ToolStripMenuItem();
            closeAllToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            importSTEPToolStripMenuItem = new ToolStripMenuItem();
            importSTEPInNewWindowToolStripMenuItem = new ToolStripMenuItem();
            importAsyncSTEPToolStripMenuItem = new ToolStripMenuItem();
            importAsyncSTEPInNewWindowToolStripMenuItem = new ToolStripMenuItem();
            importIGESToolStripMenuItem = new ToolStripMenuItem();
            importIGESInNewWindowToolStripMenuItem = new ToolStripMenuItem();
            importAsyncIGESToolStripMenuItem = new ToolStripMenuItem();
            importAsyncIGESInNewWindowToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripSeparator3 = new ToolStripSeparator();
            backupToolStripMenuItem = new ToolStripMenuItem();
            restoreToolStripMenuItem = new ToolStripMenuItem();
            languageToolStripMenuItem = new ToolStripMenuItem();
            englishToolStripMenuItem = new ToolStripMenuItem();
            turkishToolStripMenuItem = new ToolStripMenuItem();
            russianToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            izometricToolStripMenuItem = new ToolStripMenuItem();
            frontToolStripMenuItem = new ToolStripMenuItem();
            upToolStripMenuItem = new ToolStripMenuItem();
            leftToolStripMenuItem = new ToolStripMenuItem();
            bottomToolStripMenuItem1 = new ToolStripMenuItem();
            backToolStripMenuItem = new ToolStripMenuItem();
            rigthToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            wireFrameToolStripMenuItem = new ToolStripMenuItem();
            shadedToolStripMenuItem = new ToolStripMenuItem();
            renderedToolStripMenuItem = new ToolStripMenuItem();
            hiddenLineToolStripMenuItem = new ToolStripMenuItem();
            instructionPanelToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            toSurfaceToolStripMenuItem = new ToolStripMenuItem();
            selectEntitiesToolStripMenuItem = new ToolStripMenuItem();
            selectPointToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            showSelectionInfoToolStripMenuItem = new ToolStripMenuItem();
            showFaceNormalsToolStripMenuItem = new ToolStripMenuItem();
            changeSelectionColorToolStripMenuItem = new ToolStripMenuItem();
            clearSelectionToolStripMenuItem = new ToolStripMenuItem();
            clearPointMarkersToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            endPointToolStripMenuItem = new ToolStripMenuItem();
            midPointToolStripMenuItem = new ToolStripMenuItem();
            pointOnCurveToolStripMenuItem = new ToolStripMenuItem();
            centerToolStripMenuItem = new ToolStripMenuItem();
            closeToFaceToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator11 = new ToolStripSeparator();
            facesToolStripMenuItem = new ToolStripMenuItem();
            edgesToolStripMenuItem = new ToolStripMenuItem();
            measurementToolStripMenuItem = new ToolStripMenuItem();
            normalFacesToolStripMenuItem = new ToolStripMenuItem();
            normalNurbsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            lengthToolStripMenuItem = new ToolStripMenuItem();
            faceToFaceToolStripMenuItem = new ToolStripMenuItem();
            surfaceToSurfaceToolStripMenuItem = new ToolStripMenuItem();
            edgeToEdgeToolStripMenuItem = new ToolStripMenuItem();
            probingToolStripMenuItem = new ToolStripMenuItem();
            passwordToolStripMenuItem = new ToolStripMenuItem();
            cMMProbePathTestToolStripMenuItem = new ToolStripMenuItem();
            windowToolStripMenuItem = new ToolStripMenuItem();
            cascadeToolStripMenuItem = new ToolStripMenuItem();
            tileHorizontalToolStripMenuItem = new ToolStripMenuItem();
            tileVerticalToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            closeAllWindowsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            helpToolStripMenuItem = new ToolStripMenuItem();
            panel1 = new Panel();
            panel3 = new Panel();
            panel5 = new Panel();
            panel2 = new Panel();
            panel8 = new Panel();
            treeView1 = new TreeView();
            panel7 = new Panel();
            tableLayoutPanel2 = new TableLayoutPanel();
            pictureBox_Creat_GCODE = new PictureBox();
            pictureBox22 = new PictureBox();
            pictureBoxplay = new PictureBox();
            pictureBox24 = new PictureBox();
            trackBar1 = new TrackBar();
            panel6 = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            pictureBox7 = new PictureBox();
            pictureBox_CMM_point = new PictureBox();
            pictureBox3 = new PictureBox();
            pictureBox2 = new PictureBox();
            pictureBox1 = new PictureBox();
            pictureBox11 = new PictureBox();
            pictureBox10 = new PictureBox();
            pictureBox9 = new PictureBox();
            groupBox1 = new GroupBox();
            checkBox1 = new CheckBox();
            label2 = new Label();
            txt_Form1_Retract = new TextBox();
            cmb_form1_probe = new ComboBox();
            cmb_form1_cnc_machine = new ComboBox();
            label1 = new Label();
            txt_form1_Clerance = new TextBox();
            lbl_form1_probe_name = new Label();
            lbl_Form1_CNC_machine = new Label();
            panel4 = new Panel();
            ((System.ComponentModel.ISupportInitialize)design1).BeginInit();
            menuStrip1.SuspendLayout();
            panel2.SuspendLayout();
            panel8.SuspendLayout();
            panel7.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_Creat_GCODE).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox22).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxplay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox24).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            panel6.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_CMM_point).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox11).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox10).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox9).BeginInit();
            groupBox1.SuspendLayout();
            panel4.SuspendLayout();
            SuspendLayout();
            // 
            // design1
            // 
            design1.Dock = DockStyle.Fill;
            design1.Font = new Font("Segoe UI", 9F);
            design1.Location = new Point(0, 0);
            design1.Name = "design1";
            design1.ProgressBar = progressBar1;
            design1.Size = new Size(852, 518);
            design1.TabIndex = 0;
            design1.Text = "design1";
            design1.Viewports.Add(viewport1);
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, languageToolStripMenuItem, viewToolStripMenuItem, editToolStripMenuItem, measurementToolStripMenuItem, probingToolStripMenuItem, windowToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1253, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, openInNewWindowToolStripMenuItem, closeToolStripMenuItem, closeAllToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator1, importSTEPToolStripMenuItem, importSTEPInNewWindowToolStripMenuItem, importAsyncSTEPToolStripMenuItem, importAsyncSTEPInNewWindowToolStripMenuItem, importIGESToolStripMenuItem, importIGESInNewWindowToolStripMenuItem, importAsyncIGESToolStripMenuItem, importAsyncIGESInNewWindowToolStripMenuItem, toolStripSeparator2, toolStripSeparator3, backupToolStripMenuItem, restoreToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(255, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // openInNewWindowToolStripMenuItem
            // 
            openInNewWindowToolStripMenuItem.Name = "openInNewWindowToolStripMenuItem";
            openInNewWindowToolStripMenuItem.Size = new Size(255, 22);
            openInNewWindowToolStripMenuItem.Text = "Open in New Window";
            openInNewWindowToolStripMenuItem.Click += openInNewWindowToolStripMenuItem_Click;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(255, 22);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
            // 
            // closeAllToolStripMenuItem
            // 
            closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            closeAllToolStripMenuItem.Size = new Size(255, 22);
            closeAllToolStripMenuItem.Text = "Close All";
            closeAllToolStripMenuItem.Click += closeAllToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(255, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(255, 22);
            saveAsToolStripMenuItem.Text = "Save As...";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(252, 6);
            // 
            // importSTEPToolStripMenuItem
            // 
            importSTEPToolStripMenuItem.Name = "importSTEPToolStripMenuItem";
            importSTEPToolStripMenuItem.Size = new Size(255, 22);
            importSTEPToolStripMenuItem.Text = "Import STEP";
            importSTEPToolStripMenuItem.Click += importSTEPToolStripMenuItem_Click;
            // 
            // importSTEPInNewWindowToolStripMenuItem
            // 
            importSTEPInNewWindowToolStripMenuItem.Name = "importSTEPInNewWindowToolStripMenuItem";
            importSTEPInNewWindowToolStripMenuItem.Size = new Size(255, 22);
            importSTEPInNewWindowToolStripMenuItem.Text = "Import STEP (New Window)";
            importSTEPInNewWindowToolStripMenuItem.Click += importSTEPInNewWindowToolStripMenuItem_Click;
            // 
            // importAsyncSTEPToolStripMenuItem
            // 
            importAsyncSTEPToolStripMenuItem.Name = "importAsyncSTEPToolStripMenuItem";
            importAsyncSTEPToolStripMenuItem.Size = new Size(255, 22);
            importAsyncSTEPToolStripMenuItem.Text = "Import Async STEP";
            importAsyncSTEPToolStripMenuItem.Click += importAsyncSTEPToolStripMenuItem_Click;
            // 
            // importAsyncSTEPInNewWindowToolStripMenuItem
            // 
            importAsyncSTEPInNewWindowToolStripMenuItem.Name = "importAsyncSTEPInNewWindowToolStripMenuItem";
            importAsyncSTEPInNewWindowToolStripMenuItem.Size = new Size(255, 22);
            importAsyncSTEPInNewWindowToolStripMenuItem.Text = "Import Async STEP (New Window)";
            importAsyncSTEPInNewWindowToolStripMenuItem.Click += importAsyncSTEPInNewWindowToolStripMenuItem_Click;
            // 
            // importIGESToolStripMenuItem
            // 
            importIGESToolStripMenuItem.Name = "importIGESToolStripMenuItem";
            importIGESToolStripMenuItem.Size = new Size(255, 22);
            importIGESToolStripMenuItem.Text = "Import IGES";
            importIGESToolStripMenuItem.Click += importIGESToolStripMenuItem_Click;
            // 
            // importIGESInNewWindowToolStripMenuItem
            // 
            importIGESInNewWindowToolStripMenuItem.Name = "importIGESInNewWindowToolStripMenuItem";
            importIGESInNewWindowToolStripMenuItem.Size = new Size(255, 22);
            importIGESInNewWindowToolStripMenuItem.Text = "Import IGES (New Window)";
            importIGESInNewWindowToolStripMenuItem.Click += importIGESInNewWindowToolStripMenuItem_Click;
            // 
            // importAsyncIGESToolStripMenuItem
            // 
            importAsyncIGESToolStripMenuItem.Name = "importAsyncIGESToolStripMenuItem";
            importAsyncIGESToolStripMenuItem.Size = new Size(255, 22);
            importAsyncIGESToolStripMenuItem.Text = "Import Async IGES";
            importAsyncIGESToolStripMenuItem.Click += importAsyncIGESToolStripMenuItem_Click;
            // 
            // importAsyncIGESInNewWindowToolStripMenuItem
            // 
            importAsyncIGESInNewWindowToolStripMenuItem.Name = "importAsyncIGESInNewWindowToolStripMenuItem";
            importAsyncIGESInNewWindowToolStripMenuItem.Size = new Size(255, 22);
            importAsyncIGESInNewWindowToolStripMenuItem.Text = "Import Async IGES (New Window)";
            importAsyncIGESInNewWindowToolStripMenuItem.Click += importAsyncIGESInNewWindowToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(252, 6);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(252, 6);
            // 
            // backupToolStripMenuItem
            // 
            backupToolStripMenuItem.Name = "backupToolStripMenuItem";
            backupToolStripMenuItem.Size = new Size(255, 22);
            backupToolStripMenuItem.Text = "Backup";
            // 
            // restoreToolStripMenuItem
            // 
            restoreToolStripMenuItem.Name = "restoreToolStripMenuItem";
            restoreToolStripMenuItem.Size = new Size(255, 22);
            restoreToolStripMenuItem.Text = "Restore";
            // 
            // languageToolStripMenuItem
            // 
            languageToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { englishToolStripMenuItem, turkishToolStripMenuItem, russianToolStripMenuItem });
            languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            languageToolStripMenuItem.Size = new Size(71, 20);
            languageToolStripMenuItem.Text = "Language";
            // 
            // englishToolStripMenuItem
            // 
            englishToolStripMenuItem.Name = "englishToolStripMenuItem";
            englishToolStripMenuItem.Size = new Size(114, 22);
            englishToolStripMenuItem.Text = "English";
            // 
            // turkishToolStripMenuItem
            // 
            turkishToolStripMenuItem.Name = "turkishToolStripMenuItem";
            turkishToolStripMenuItem.Size = new Size(114, 22);
            turkishToolStripMenuItem.Text = "Turkish";
            // 
            // russianToolStripMenuItem
            // 
            russianToolStripMenuItem.Name = "russianToolStripMenuItem";
            russianToolStripMenuItem.Size = new Size(114, 22);
            russianToolStripMenuItem.Text = "Russian";
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { izometricToolStripMenuItem, frontToolStripMenuItem, upToolStripMenuItem, leftToolStripMenuItem, bottomToolStripMenuItem1, backToolStripMenuItem, rigthToolStripMenuItem, toolStripSeparator8, wireFrameToolStripMenuItem, shadedToolStripMenuItem, renderedToolStripMenuItem, hiddenLineToolStripMenuItem, instructionPanelToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "View";
            // 
            // izometricToolStripMenuItem
            // 
            izometricToolStripMenuItem.Name = "izometricToolStripMenuItem";
            izometricToolStripMenuItem.Size = new Size(163, 22);
            izometricToolStripMenuItem.Text = "Isometric";
            izometricToolStripMenuItem.Click += izometricToolStripMenuItem_Click;
            // 
            // frontToolStripMenuItem
            // 
            frontToolStripMenuItem.Name = "frontToolStripMenuItem";
            frontToolStripMenuItem.Size = new Size(163, 22);
            frontToolStripMenuItem.Text = "Front";
            frontToolStripMenuItem.Click += bottomToolStripMenuItem_Click;
            // 
            // upToolStripMenuItem
            // 
            upToolStripMenuItem.Name = "upToolStripMenuItem";
            upToolStripMenuItem.Size = new Size(163, 22);
            upToolStripMenuItem.Text = "Top";
            upToolStripMenuItem.Click += upToolStripMenuItem_Click;
            // 
            // leftToolStripMenuItem
            // 
            leftToolStripMenuItem.Name = "leftToolStripMenuItem";
            leftToolStripMenuItem.Size = new Size(163, 22);
            leftToolStripMenuItem.Text = "Left";
            leftToolStripMenuItem.Click += leftToolStripMenuItem_Click;
            // 
            // bottomToolStripMenuItem1
            // 
            bottomToolStripMenuItem1.Name = "bottomToolStripMenuItem1";
            bottomToolStripMenuItem1.Size = new Size(163, 22);
            bottomToolStripMenuItem1.Text = "Bottom";
            bottomToolStripMenuItem1.Click += bottomToolStripMenuItem1_Click;
            // 
            // backToolStripMenuItem
            // 
            backToolStripMenuItem.Name = "backToolStripMenuItem";
            backToolStripMenuItem.Size = new Size(163, 22);
            backToolStripMenuItem.Text = "Back";
            backToolStripMenuItem.Click += backToolStripMenuItem_Click;
            // 
            // rigthToolStripMenuItem
            // 
            rigthToolStripMenuItem.Name = "rigthToolStripMenuItem";
            rigthToolStripMenuItem.Size = new Size(163, 22);
            rigthToolStripMenuItem.Text = "Right";
            rigthToolStripMenuItem.Click += rigthToolStripMenuItem_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(160, 6);
            // 
            // wireFrameToolStripMenuItem
            // 
            wireFrameToolStripMenuItem.Name = "wireFrameToolStripMenuItem";
            wireFrameToolStripMenuItem.Size = new Size(163, 22);
            wireFrameToolStripMenuItem.Text = "WireFrame";
            wireFrameToolStripMenuItem.Click += wireFrameToolStripMenuItem_Click;
            // 
            // shadedToolStripMenuItem
            // 
            shadedToolStripMenuItem.Name = "shadedToolStripMenuItem";
            shadedToolStripMenuItem.Size = new Size(163, 22);
            shadedToolStripMenuItem.Text = "Shaded";
            shadedToolStripMenuItem.Click += shadedToolStripMenuItem_Click;
            // 
            // renderedToolStripMenuItem
            // 
            renderedToolStripMenuItem.Name = "renderedToolStripMenuItem";
            renderedToolStripMenuItem.Size = new Size(163, 22);
            renderedToolStripMenuItem.Text = "Rendered";
            renderedToolStripMenuItem.Click += renderedToolStripMenuItem_Click;
            // 
            // hiddenLineToolStripMenuItem
            // 
            hiddenLineToolStripMenuItem.Name = "hiddenLineToolStripMenuItem";
            hiddenLineToolStripMenuItem.Size = new Size(163, 22);
            hiddenLineToolStripMenuItem.Text = "HiddenLine";
            hiddenLineToolStripMenuItem.Click += hiddenLineToolStripMenuItem_Click;
            // 
            // instructionPanelToolStripMenuItem
            // 
            instructionPanelToolStripMenuItem.Name = "instructionPanelToolStripMenuItem";
            instructionPanelToolStripMenuItem.Size = new Size(163, 22);
            instructionPanelToolStripMenuItem.Text = "Instruction Panel";
            instructionPanelToolStripMenuItem.Click += instructionPanelToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toSurfaceToolStripMenuItem, selectEntitiesToolStripMenuItem, selectPointToolStripMenuItem, toolStripSeparator7, showSelectionInfoToolStripMenuItem, showFaceNormalsToolStripMenuItem, changeSelectionColorToolStripMenuItem, clearSelectionToolStripMenuItem, clearPointMarkersToolStripMenuItem, toolStripSeparator10, endPointToolStripMenuItem, midPointToolStripMenuItem, pointOnCurveToolStripMenuItem, centerToolStripMenuItem, closeToFaceToolStripMenuItem, toolStripSeparator11, facesToolStripMenuItem, edgesToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(39, 20);
            editToolStripMenuItem.Text = "Edit";
            // 
            // toSurfaceToolStripMenuItem
            // 
            toSurfaceToolStripMenuItem.Enabled = false;
            toSurfaceToolStripMenuItem.Name = "toSurfaceToolStripMenuItem";
            toSurfaceToolStripMenuItem.Size = new Size(198, 22);
            toSurfaceToolStripMenuItem.Text = "Select Faces";
            toSurfaceToolStripMenuItem.Click += toSurfaceToolStripMenuItem_Click;
            // 
            // selectEntitiesToolStripMenuItem
            // 
            selectEntitiesToolStripMenuItem.Enabled = false;
            selectEntitiesToolStripMenuItem.Name = "selectEntitiesToolStripMenuItem";
            selectEntitiesToolStripMenuItem.Size = new Size(198, 22);
            selectEntitiesToolStripMenuItem.Text = "Select Entities";
            selectEntitiesToolStripMenuItem.Click += selectEntitiesToolStripMenuItem_Click_1;
            // 
            // selectPointToolStripMenuItem
            // 
            selectPointToolStripMenuItem.Enabled = false;
            selectPointToolStripMenuItem.Name = "selectPointToolStripMenuItem";
            selectPointToolStripMenuItem.Size = new Size(198, 22);
            selectPointToolStripMenuItem.Text = "📍 Select Point";
            selectPointToolStripMenuItem.Click += selectPointToolStripMenuItem_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(195, 6);
            // 
            // showSelectionInfoToolStripMenuItem
            // 
            showSelectionInfoToolStripMenuItem.Enabled = false;
            showSelectionInfoToolStripMenuItem.Name = "showSelectionInfoToolStripMenuItem";
            showSelectionInfoToolStripMenuItem.Size = new Size(198, 22);
            showSelectionInfoToolStripMenuItem.Text = "Show Selection Info";
            showSelectionInfoToolStripMenuItem.Click += showSelectionInfoToolStripMenuItem_Click_1;
            // 
            // showFaceNormalsToolStripMenuItem
            // 
            showFaceNormalsToolStripMenuItem.Enabled = false;
            showFaceNormalsToolStripMenuItem.Name = "showFaceNormalsToolStripMenuItem";
            showFaceNormalsToolStripMenuItem.Size = new Size(198, 22);
            showFaceNormalsToolStripMenuItem.Text = "Show Face Normals";
            showFaceNormalsToolStripMenuItem.Click += showFaceNormalsToolStripMenuItem_Click;
            // 
            // changeSelectionColorToolStripMenuItem
            // 
            changeSelectionColorToolStripMenuItem.Enabled = false;
            changeSelectionColorToolStripMenuItem.Name = "changeSelectionColorToolStripMenuItem";
            changeSelectionColorToolStripMenuItem.Size = new Size(198, 22);
            changeSelectionColorToolStripMenuItem.Text = "Change Selection Color";
            changeSelectionColorToolStripMenuItem.Click += changeSelectionColorToolStripMenuItem_Click_1;
            // 
            // clearSelectionToolStripMenuItem
            // 
            clearSelectionToolStripMenuItem.Enabled = false;
            clearSelectionToolStripMenuItem.Name = "clearSelectionToolStripMenuItem";
            clearSelectionToolStripMenuItem.Size = new Size(198, 22);
            clearSelectionToolStripMenuItem.Text = "Clear Selection";
            clearSelectionToolStripMenuItem.Click += clearSelectionToolStripMenuItem_Click_1;
            // 
            // clearPointMarkersToolStripMenuItem
            // 
            clearPointMarkersToolStripMenuItem.Enabled = false;
            clearPointMarkersToolStripMenuItem.Name = "clearPointMarkersToolStripMenuItem";
            clearPointMarkersToolStripMenuItem.Size = new Size(198, 22);
            clearPointMarkersToolStripMenuItem.Text = "Clear Point Markers";
            clearPointMarkersToolStripMenuItem.Click += clearPointMarkersToolStripMenuItem_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(195, 6);
            // 
            // endPointToolStripMenuItem
            // 
            endPointToolStripMenuItem.Enabled = false;
            endPointToolStripMenuItem.Name = "endPointToolStripMenuItem";
            endPointToolStripMenuItem.Size = new Size(198, 22);
            endPointToolStripMenuItem.Text = "End Point";
            // 
            // midPointToolStripMenuItem
            // 
            midPointToolStripMenuItem.Enabled = false;
            midPointToolStripMenuItem.Name = "midPointToolStripMenuItem";
            midPointToolStripMenuItem.Size = new Size(198, 22);
            midPointToolStripMenuItem.Text = "Mid Point";
            // 
            // pointOnCurveToolStripMenuItem
            // 
            pointOnCurveToolStripMenuItem.Enabled = false;
            pointOnCurveToolStripMenuItem.Name = "pointOnCurveToolStripMenuItem";
            pointOnCurveToolStripMenuItem.Size = new Size(198, 22);
            pointOnCurveToolStripMenuItem.Text = "Point to Curve";
            // 
            // centerToolStripMenuItem
            // 
            centerToolStripMenuItem.Enabled = false;
            centerToolStripMenuItem.Name = "centerToolStripMenuItem";
            centerToolStripMenuItem.Size = new Size(198, 22);
            centerToolStripMenuItem.Text = "Center";
            // 
            // closeToFaceToolStripMenuItem
            // 
            closeToFaceToolStripMenuItem.Enabled = false;
            closeToFaceToolStripMenuItem.Name = "closeToFaceToolStripMenuItem";
            closeToFaceToolStripMenuItem.Size = new Size(198, 22);
            closeToFaceToolStripMenuItem.Text = "Close to Face";
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new Size(195, 6);
            // 
            // facesToolStripMenuItem
            // 
            facesToolStripMenuItem.Enabled = false;
            facesToolStripMenuItem.Name = "facesToolStripMenuItem";
            facesToolStripMenuItem.Size = new Size(198, 22);
            facesToolStripMenuItem.Text = "Faces";
            // 
            // edgesToolStripMenuItem
            // 
            edgesToolStripMenuItem.Enabled = false;
            edgesToolStripMenuItem.Name = "edgesToolStripMenuItem";
            edgesToolStripMenuItem.Size = new Size(198, 22);
            edgesToolStripMenuItem.Text = "Edges";
            // 
            // measurementToolStripMenuItem
            // 
            measurementToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { normalFacesToolStripMenuItem, normalNurbsToolStripMenuItem, toolStripSeparator9, lengthToolStripMenuItem, faceToFaceToolStripMenuItem, surfaceToSurfaceToolStripMenuItem, edgeToEdgeToolStripMenuItem });
            measurementToolStripMenuItem.Name = "measurementToolStripMenuItem";
            measurementToolStripMenuItem.Size = new Size(92, 20);
            measurementToolStripMenuItem.Text = "Measurement";
            // 
            // normalFacesToolStripMenuItem
            // 
            normalFacesToolStripMenuItem.Enabled = false;
            normalFacesToolStripMenuItem.Name = "normalFacesToolStripMenuItem";
            normalFacesToolStripMenuItem.Size = new Size(169, 22);
            normalFacesToolStripMenuItem.Text = "Normal Faces";
            normalFacesToolStripMenuItem.Click += normalFacesToolStripMenuItem_Click;
            // 
            // normalNurbsToolStripMenuItem
            // 
            normalNurbsToolStripMenuItem.Name = "normalNurbsToolStripMenuItem";
            normalNurbsToolStripMenuItem.Size = new Size(169, 22);
            normalNurbsToolStripMenuItem.Text = "Direction Probe";
            normalNurbsToolStripMenuItem.Click += normalNurbsToolStripMenuItem_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(166, 6);
            // 
            // lengthToolStripMenuItem
            // 
            lengthToolStripMenuItem.Enabled = false;
            lengthToolStripMenuItem.Name = "lengthToolStripMenuItem";
            lengthToolStripMenuItem.Size = new Size(169, 22);
            lengthToolStripMenuItem.Text = "Point to Point";
            lengthToolStripMenuItem.Click += lengthToolStripMenuItem_Click;
            // 
            // faceToFaceToolStripMenuItem
            // 
            faceToFaceToolStripMenuItem.Name = "faceToFaceToolStripMenuItem";
            faceToFaceToolStripMenuItem.Size = new Size(169, 22);
            faceToFaceToolStripMenuItem.Text = "Face to Face";
            faceToFaceToolStripMenuItem.Click += faceToFaceToolStripMenuItem_Click;
            // 
            // surfaceToSurfaceToolStripMenuItem
            // 
            surfaceToSurfaceToolStripMenuItem.Enabled = false;
            surfaceToSurfaceToolStripMenuItem.Name = "surfaceToSurfaceToolStripMenuItem";
            surfaceToSurfaceToolStripMenuItem.Size = new Size(169, 22);
            surfaceToSurfaceToolStripMenuItem.Text = "Surface to Surface";
            surfaceToSurfaceToolStripMenuItem.Click += surfaceToSurfaceToolStripMenuItem_Click;
            // 
            // edgeToEdgeToolStripMenuItem
            // 
            edgeToEdgeToolStripMenuItem.Name = "edgeToEdgeToolStripMenuItem";
            edgeToEdgeToolStripMenuItem.Size = new Size(169, 22);
            edgeToEdgeToolStripMenuItem.Text = "Edge to Edge";
            edgeToEdgeToolStripMenuItem.Click += edgeToEdgeToolStripMenuItem_Click;
            // 
            // probingToolStripMenuItem
            // 
            probingToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { passwordToolStripMenuItem, cMMProbePathTestToolStripMenuItem });
            probingToolStripMenuItem.Name = "probingToolStripMenuItem";
            probingToolStripMenuItem.Size = new Size(61, 20);
            probingToolStripMenuItem.Text = "Probing";
            // 
            // passwordToolStripMenuItem
            // 
            passwordToolStripMenuItem.Enabled = false;
            passwordToolStripMenuItem.Name = "passwordToolStripMenuItem";
            passwordToolStripMenuItem.Size = new Size(188, 22);
            passwordToolStripMenuItem.Text = "Password";
            passwordToolStripMenuItem.Click += passwordToolStripMenuItem_Click;
            // 
            // cMMProbePathTestToolStripMenuItem
            // 
            cMMProbePathTestToolStripMenuItem.Name = "cMMProbePathTestToolStripMenuItem";
            cMMProbePathTestToolStripMenuItem.Size = new Size(188, 22);
            cMMProbePathTestToolStripMenuItem.Text = "CMM Probe Path Test";
            cMMProbePathTestToolStripMenuItem.Click += cMMProbePathTestToolStripMenuItem_Click;
            // 
            // windowToolStripMenuItem
            // 
            windowToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { cascadeToolStripMenuItem, tileHorizontalToolStripMenuItem, tileVerticalToolStripMenuItem, toolStripSeparator5, closeAllWindowsToolStripMenuItem, toolStripSeparator6 });
            windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            windowToolStripMenuItem.Size = new Size(63, 20);
            windowToolStripMenuItem.Text = "Window";
            windowToolStripMenuItem.DropDownOpening += windowToolStripMenuItem_DropDownOpening;
            // 
            // cascadeToolStripMenuItem
            // 
            cascadeToolStripMenuItem.Name = "cascadeToolStripMenuItem";
            cascadeToolStripMenuItem.Size = new Size(172, 22);
            cascadeToolStripMenuItem.Text = "Cascade";
            cascadeToolStripMenuItem.Click += cascadeToolStripMenuItem_Click;
            // 
            // tileHorizontalToolStripMenuItem
            // 
            tileHorizontalToolStripMenuItem.Name = "tileHorizontalToolStripMenuItem";
            tileHorizontalToolStripMenuItem.Size = new Size(172, 22);
            tileHorizontalToolStripMenuItem.Text = "Tile Horizontal";
            tileHorizontalToolStripMenuItem.Click += tileHorizontalToolStripMenuItem_Click;
            // 
            // tileVerticalToolStripMenuItem
            // 
            tileVerticalToolStripMenuItem.Name = "tileVerticalToolStripMenuItem";
            tileVerticalToolStripMenuItem.Size = new Size(172, 22);
            tileVerticalToolStripMenuItem.Text = "Tile Vertical";
            tileVerticalToolStripMenuItem.Click += tileVerticalToolStripMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(169, 6);
            // 
            // closeAllWindowsToolStripMenuItem
            // 
            closeAllWindowsToolStripMenuItem.Name = "closeAllWindowsToolStripMenuItem";
            closeAllWindowsToolStripMenuItem.Size = new Size(172, 22);
            closeAllWindowsToolStripMenuItem.Text = "Close All Windows";
            closeAllWindowsToolStripMenuItem.Click += closeAllWindowsToolStripMenuItem_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(169, 6);
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ActiveCaption;
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 24);
            panel1.Name = "panel1";
            panel1.Size = new Size(1253, 30);
            panel1.TabIndex = 3;
            // 
            // panel3
            // 
            panel3.BackColor = SystemColors.ActiveCaption;
            panel3.Dock = DockStyle.Right;
            panel3.Location = new Point(1103, 54);
            panel3.Name = "panel3";
            panel3.Size = new Size(150, 668);
            panel3.TabIndex = 4;
            // 
            // panel5
            // 
            panel5.BackColor = SystemColors.ActiveCaption;
            panel5.Dock = DockStyle.Bottom;
            panel5.Location = new Point(0, 572);
            panel5.Name = "panel5";
            panel5.Size = new Size(1103, 150);
            panel5.TabIndex = 5;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ActiveCaption;
            panel2.Controls.Add(panel8);
            panel2.Controls.Add(panel7);
            panel2.Controls.Add(panel6);
            panel2.Dock = DockStyle.Left;
            panel2.Location = new Point(0, 54);
            panel2.Name = "panel2";
            panel2.Size = new Size(251, 518);
            panel2.TabIndex = 6;
            // 
            // panel8
            // 
            panel8.Controls.Add(treeView1);
            panel8.Dock = DockStyle.Fill;
            panel8.Location = new Point(0, 256);
            panel8.Name = "panel8";
            panel8.Size = new Size(251, 219);
            panel8.TabIndex = 3;
            // 
            // treeView1
            // 
            treeView1.Dock = DockStyle.Fill;
            treeView1.Location = new Point(0, 0);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(251, 219);
            treeView1.TabIndex = 0;
            treeView1.AfterSelect += treeView1_AfterSelect;
            // 
            // panel7
            // 
            panel7.Controls.Add(tableLayoutPanel2);
            panel7.Dock = DockStyle.Bottom;
            panel7.Location = new Point(0, 475);
            panel7.Name = "panel7";
            panel7.Size = new Size(251, 43);
            panel7.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 5;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 89F));
            tableLayoutPanel2.Controls.Add(pictureBox_Creat_GCODE, 3, 0);
            tableLayoutPanel2.Controls.Add(pictureBox22, 2, 0);
            tableLayoutPanel2.Controls.Add(pictureBoxplay, 1, 0);
            tableLayoutPanel2.Controls.Add(pictureBox24, 0, 0);
            tableLayoutPanel2.Controls.Add(trackBar1, 4, 0);
            tableLayoutPanel2.Dock = DockStyle.Bottom;
            tableLayoutPanel2.Location = new Point(0, 2);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(251, 41);
            tableLayoutPanel2.TabIndex = 6;
            // 
            // pictureBox_Creat_GCODE
            // 
            pictureBox_Creat_GCODE.Dock = DockStyle.Fill;
            pictureBox_Creat_GCODE.Image = (Image)resources.GetObject("pictureBox_Creat_GCODE.Image");
            pictureBox_Creat_GCODE.InitialImage = null;
            pictureBox_Creat_GCODE.Location = new Point(123, 3);
            pictureBox_Creat_GCODE.Name = "pictureBox_Creat_GCODE";
            pictureBox_Creat_GCODE.Size = new Size(34, 35);
            pictureBox_Creat_GCODE.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_Creat_GCODE.TabIndex = 3;
            pictureBox_Creat_GCODE.TabStop = false;
            pictureBox_Creat_GCODE.Click += pictureBox_Creat_GCODE_Click;
            // 
            // pictureBox22
            // 
            pictureBox22.Dock = DockStyle.Fill;
            pictureBox22.Image = (Image)resources.GetObject("pictureBox22.Image");
            pictureBox22.Location = new Point(83, 3);
            pictureBox22.Name = "pictureBox22";
            pictureBox22.Size = new Size(34, 35);
            pictureBox22.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox22.TabIndex = 2;
            pictureBox22.TabStop = false;
            pictureBox22.Click += pictureBox22_Click;
            // 
            // pictureBoxplay
            // 
            pictureBoxplay.Dock = DockStyle.Fill;
            pictureBoxplay.Image = (Image)resources.GetObject("pictureBoxplay.Image");
            pictureBoxplay.Location = new Point(43, 3);
            pictureBoxplay.Name = "pictureBoxplay";
            pictureBoxplay.Size = new Size(34, 35);
            pictureBoxplay.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxplay.TabIndex = 1;
            pictureBoxplay.TabStop = false;
            pictureBoxplay.Click += pictureBoxplay_Click;
            // 
            // pictureBox24
            // 
            pictureBox24.Dock = DockStyle.Fill;
            pictureBox24.Image = (Image)resources.GetObject("pictureBox24.Image");
            pictureBox24.InitialImage = null;
            pictureBox24.Location = new Point(3, 3);
            pictureBox24.Name = "pictureBox24";
            pictureBox24.Size = new Size(34, 35);
            pictureBox24.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox24.TabIndex = 0;
            pictureBox24.TabStop = false;
            pictureBox24.Click += pictureBox24_Click;
            // 
            // trackBar1
            // 
            trackBar1.Dock = DockStyle.Bottom;
            trackBar1.Location = new Point(163, 3);
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(85, 35);
            trackBar1.TabIndex = 7;
            trackBar1.Scroll += trackBar1_Scroll;
            // 
            // panel6
            // 
            panel6.Controls.Add(tableLayoutPanel1);
            panel6.Controls.Add(groupBox1);
            panel6.Dock = DockStyle.Top;
            panel6.Location = new Point(0, 0);
            panel6.Name = "panel6";
            panel6.Size = new Size(251, 256);
            panel6.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.Controls.Add(pictureBox7, 2, 1);
            tableLayoutPanel1.Controls.Add(pictureBox_CMM_point, 0, 1);
            tableLayoutPanel1.Controls.Add(pictureBox3, 2, 0);
            tableLayoutPanel1.Controls.Add(pictureBox2, 1, 0);
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(pictureBox11, 3, 0);
            tableLayoutPanel1.Controls.Add(pictureBox10, 3, 1);
            tableLayoutPanel1.Controls.Add(pictureBox9, 1, 1);
            tableLayoutPanel1.Location = new Point(12, 135);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(231, 116);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // pictureBox7
            // 
            pictureBox7.BorderStyle = BorderStyle.FixedSingle;
            pictureBox7.Image = (Image)resources.GetObject("pictureBox7.Image");
            pictureBox7.InitialImage = null;
            pictureBox7.Location = new Point(118, 61);
            pictureBox7.Name = "pictureBox7";
            pictureBox7.Size = new Size(50, 49);
            pictureBox7.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox7.TabIndex = 6;
            pictureBox7.TabStop = false;
            // 
            // pictureBox_CMM_point
            // 
            pictureBox_CMM_point.BorderStyle = BorderStyle.FixedSingle;
            pictureBox_CMM_point.Image = (Image)resources.GetObject("pictureBox_CMM_point.Image");
            pictureBox_CMM_point.InitialImage = null;
            pictureBox_CMM_point.Location = new Point(4, 61);
            pictureBox_CMM_point.Name = "pictureBox_CMM_point";
            pictureBox_CMM_point.Size = new Size(50, 49);
            pictureBox_CMM_point.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_CMM_point.TabIndex = 4;
            pictureBox_CMM_point.TabStop = false;
            pictureBox_CMM_point.Click += pictureBox_CMM_point_Click;
            // 
            // pictureBox3
            // 
            pictureBox3.BorderStyle = BorderStyle.FixedSingle;
            pictureBox3.Image = (Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.Location = new Point(118, 4);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(50, 49);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 2;
            pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(61, 4);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(50, 49);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.InitialImage = null;
            pictureBox1.Location = new Point(4, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(50, 49);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // pictureBox11
            // 
            pictureBox11.BorderStyle = BorderStyle.FixedSingle;
            pictureBox11.Image = (Image)resources.GetObject("pictureBox11.Image");
            pictureBox11.InitialImage = null;
            pictureBox11.Location = new Point(175, 4);
            pictureBox11.Name = "pictureBox11";
            pictureBox11.Size = new Size(50, 50);
            pictureBox11.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox11.TabIndex = 11;
            pictureBox11.TabStop = false;
            // 
            // pictureBox10
            // 
            pictureBox10.BorderStyle = BorderStyle.FixedSingle;
            pictureBox10.Image = (Image)resources.GetObject("pictureBox10.Image");
            pictureBox10.InitialImage = null;
            pictureBox10.Location = new Point(175, 61);
            pictureBox10.Name = "pictureBox10";
            pictureBox10.Size = new Size(50, 50);
            pictureBox10.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox10.TabIndex = 10;
            pictureBox10.TabStop = false;
            // 
            // pictureBox9
            // 
            pictureBox9.BorderStyle = BorderStyle.FixedSingle;
            pictureBox9.Image = (Image)resources.GetObject("pictureBox9.Image");
            pictureBox9.InitialImage = null;
            pictureBox9.Location = new Point(61, 61);
            pictureBox9.Name = "pictureBox9";
            pictureBox9.Size = new Size(50, 50);
            pictureBox9.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox9.TabIndex = 8;
            pictureBox9.TabStop = false;
            pictureBox9.Click += pictureBox9_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(checkBox1);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(txt_Form1_Retract);
            groupBox1.Controls.Add(cmb_form1_probe);
            groupBox1.Controls.Add(cmb_form1_cnc_machine);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(txt_form1_Clerance);
            groupBox1.Controls.Add(lbl_form1_probe_name);
            groupBox1.Controls.Add(lbl_Form1_CNC_machine);
            groupBox1.Location = new Point(12, -5);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(231, 134);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Enter += groupBox1_Enter;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Checked = true;
            checkBox1.CheckState = CheckState.Checked;
            checkBox1.Location = new Point(101, 76);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(15, 14);
            checkBox1.TabIndex = 9;
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 102);
            label2.Name = "label2";
            label2.Size = new Size(44, 15);
            label2.TabIndex = 7;
            label2.Text = "Retract";
            // 
            // txt_Form1_Retract
            // 
            txt_Form1_Retract.Location = new Point(119, 99);
            txt_Form1_Retract.Name = "txt_Form1_Retract";
            txt_Form1_Retract.Size = new Size(106, 23);
            txt_Form1_Retract.TabIndex = 8;
            txt_Form1_Retract.TextAlign = HorizontalAlignment.Right;
            txt_Form1_Retract.TextChanged += textBox1_TextChanged;
            txt_Form1_Retract.KeyPress += txt_Form1_Retract_KeyPress;
            txt_Form1_Retract.Leave += txt_Form1_Retract_Leave;
            // 
            // cmb_form1_probe
            // 
            cmb_form1_probe.DrawMode = DrawMode.OwnerDrawFixed;
            cmb_form1_probe.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_form1_probe.FormattingEnabled = true;
            cmb_form1_probe.Location = new Point(119, 44);
            cmb_form1_probe.Name = "cmb_form1_probe";
            cmb_form1_probe.Size = new Size(106, 24);
            cmb_form1_probe.TabIndex = 6;
            cmb_form1_probe.DrawItem += cmb_form1_probe_DrawItem;
            cmb_form1_probe.SelectedIndexChanged += cmb_form1_probe_SelectedIndexChanged_1;
            // 
            // cmb_form1_cnc_machine
            // 
            cmb_form1_cnc_machine.DrawMode = DrawMode.OwnerDrawFixed;
            cmb_form1_cnc_machine.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_form1_cnc_machine.FormattingEnabled = true;
            cmb_form1_cnc_machine.Location = new Point(119, 18);
            cmb_form1_cnc_machine.Name = "cmb_form1_cnc_machine";
            cmb_form1_cnc_machine.Size = new Size(106, 24);
            cmb_form1_cnc_machine.TabIndex = 5;
            cmb_form1_cnc_machine.DrawItem += cmb_form1_cnc_machine_DrawItem;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 74);
            label1.Name = "label1";
            label1.Size = new Size(85, 15);
            label1.TabIndex = 4;
            label1.Text = "Clerance Plane";
            // 
            // txt_form1_Clerance
            // 
            txt_form1_Clerance.Location = new Point(119, 71);
            txt_form1_Clerance.Name = "txt_form1_Clerance";
            txt_form1_Clerance.Size = new Size(106, 23);
            txt_form1_Clerance.TabIndex = 5;
            txt_form1_Clerance.TextAlign = HorizontalAlignment.Right;
            // 
            // lbl_form1_probe_name
            // 
            lbl_form1_probe_name.AutoSize = true;
            lbl_form1_probe_name.Location = new Point(6, 47);
            lbl_form1_probe_name.Name = "lbl_form1_probe_name";
            lbl_form1_probe_name.Size = new Size(38, 15);
            lbl_form1_probe_name.TabIndex = 1;
            lbl_form1_probe_name.Text = "Probe";
            // 
            // lbl_Form1_CNC_machine
            // 
            lbl_Form1_CNC_machine.AutoSize = true;
            lbl_Form1_CNC_machine.Location = new Point(6, 21);
            lbl_Form1_CNC_machine.Name = "lbl_Form1_CNC_machine";
            lbl_Form1_CNC_machine.Size = new Size(81, 15);
            lbl_Form1_CNC_machine.TabIndex = 0;
            lbl_Form1_CNC_machine.Text = "CNC Machine";
            // 
            // panel4
            // 
            panel4.Controls.Add(design1);
            panel4.Dock = DockStyle.Fill;
            panel4.Location = new Point(251, 54);
            panel4.Name = "panel4";
            panel4.Size = new Size(852, 518);
            panel4.TabIndex = 7;
            // 
            // CNC_Measurement
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1253, 722);
            Controls.Add(panel4);
            Controls.Add(panel2);
            Controls.Add(panel5);
            Controls.Add(panel3);
            Controls.Add(panel1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "CNC_Measurement";
            Text = "CNC Measurement";
            WindowState = FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)design1).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panel2.ResumeLayout(false);
            panel8.ResumeLayout(false);
            panel7.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_Creat_GCODE).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox22).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxplay).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox24).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            panel6.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox7).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_CMM_point).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox11).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox10).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox9).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel4.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem importSTEPToolStripMenuItem;
        private ToolStripMenuItem importAsyncSTEPToolStripMenuItem;
        private ToolStripMenuItem importIGESToolStripMenuItem;
        private ToolStripMenuItem importAsyncIGESToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem backupToolStripMenuItem;
        private ToolStripMenuItem restoreToolStripMenuItem;
        private ToolStripMenuItem languageToolStripMenuItem;
        private ToolStripMenuItem englishToolStripMenuItem;
        private ToolStripMenuItem turkishToolStripMenuItem;
        private ToolStripMenuItem russianToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem izometricToolStripMenuItem;
        private ToolStripMenuItem frontToolStripMenuItem;
        private ToolStripMenuItem upToolStripMenuItem;
        private ToolStripMenuItem leftToolStripMenuItem;
        private ToolStripMenuItem bottomToolStripMenuItem1;
        private ToolStripMenuItem backToolStripMenuItem;
        private ToolStripMenuItem rigthToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem toSurfaceToolStripMenuItem;
        private devDept.Eyeshot.Control.Design design1;
        private ToolStripMenuItem measurementToolStripMenuItem;
        private ToolStripMenuItem probingToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        // private ToolStripMenuItem addNewProbToolStripMenuItem; // ✅ KALDIRILDI
        private Panel panel1;
        private Panel panel3;
        private Panel panel5;
        private Panel panel2;
        private Panel panel4;
        private ToolStripMenuItem selectEntitiesToolStripMenuItem;
        private ToolStripMenuItem showSelectionInfoToolStripMenuItem;
        private ToolStripMenuItem showFaceNormalsToolStripMenuItem; // ✅ YENİ
        private ToolStripMenuItem clearSurfaceLabelsToolStripMenuItem; // ✅ YENİ EKLENEN
        private ToolStripMenuItem changeSelectionColorToolStripMenuItem;
        private ToolStripMenuItem clearSelectionToolStripMenuItem;
        private ToolStripMenuItem passwordToolStripMenuItem;
        // private ToolStripMenuItem macineToolStripMenuItem; // ✅ KALDIRILDI
        // private ToolStripSeparator toolStripSeparator4; // ✅ KALDIRILDI
        private ToolStripMenuItem openInNewWindowToolStripMenuItem;
        private ToolStripMenuItem closeAllToolStripMenuItem;
        private ToolStripMenuItem importSTEPInNewWindowToolStripMenuItem;
        private ToolStripMenuItem importAsyncSTEPInNewWindowToolStripMenuItem;
        private ToolStripMenuItem importIGESInNewWindowToolStripMenuItem;
        private ToolStripMenuItem importAsyncIGESInNewWindowToolStripMenuItem;
        // ✅ YENİ WINDOW MENÜ ELEMANLARI
        private ToolStripMenuItem windowToolStripMenuItem;
        private ToolStripMenuItem cascadeToolStripMenuItem;
        private ToolStripMenuItem tileHorizontalToolStripMenuItem;
        private ToolStripMenuItem tileVerticalToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem closeAllWindowsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        // ✅ YENİ NOKTA SEÇİMİ MENÜ ELEMANLARI
        private ToolStripMenuItem selectPointToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem clearPointMarkersToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem wireFrameToolStripMenuItem;
        private ToolStripMenuItem shadedToolStripMenuItem;
        private ToolStripMenuItem renderedToolStripMenuItem;
        private ToolStripMenuItem hiddenLineToolStripMenuItem;
        private ToolStripMenuItem normalFacesToolStripMenuItem;
        private ToolStripMenuItem normalNurbsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripMenuItem instructionPanelToolStripMenuItem;
        private ToolStripMenuItem endPointToolStripMenuItem;
        private ToolStripMenuItem midPointToolStripMenuItem;
        private ToolStripMenuItem pointOnCurveToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem centerToolStripMenuItem;
        private ToolStripMenuItem closeToFaceToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripMenuItem facesToolStripMenuItem;
        private ToolStripMenuItem edgesToolStripMenuItem;
        private ToolStripMenuItem lengthToolStripMenuItem;
        private ToolStripMenuItem faceToFaceToolStripMenuItem;
        private ToolStripMenuItem surfaceToSurfaceToolStripMenuItem;
        private ToolStripMenuItem edgeToEdgeToolStripMenuItem;
        private ToolStripMenuItem cMMProbePathTestToolStripMenuItem;
        private TreeView treeView1;
        private Panel panel6;
        private GroupBox groupBox1;
        private Label lbl_form1_probe_name;
        private Label lbl_Form1_CNC_machine;
        private Label label1;
        public TextBox txt_form1_Clerance; // ✅ PUBLIC: FileImporter erişimi için
        private ComboBox cmb_form1_probe;
        private ComboBox cmb_form1_cnc_machine;
        private Label label2;
        private TextBox txt_Form1_Retract;
        private TableLayoutPanel tableLayoutPanel1;
        private PictureBox pictureBox1;
        private PictureBox pictureBox3;
        private PictureBox pictureBox2;
        private PictureBox pictureBox9;
        private PictureBox pictureBox7;
        private PictureBox pictureBox_CMM_point;
        private PictureBox pictureBox10;
        private PictureBox pictureBox11;
        private CheckBox checkBox1;
        private Panel panel7;
        private TableLayoutPanel tableLayoutPanel2;
        private PictureBox pictureBox_Creat_GCODE;
        private PictureBox pictureBox22;
        private PictureBox pictureBoxplay;
        private PictureBox pictureBox24;
        private TrackBar trackBar1;
        private Panel panel8;
    }
}