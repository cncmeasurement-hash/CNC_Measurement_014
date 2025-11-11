namespace _014
{
    partial class Form_CNC_Machines
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_CNC_Machines));
            tableLayoutPanel1 = new TableLayoutPanel();
            btn_new_machine_save = new Button();
            lbl_Control_system = new Label();
            textBox1 = new TextBox();
            lbl_Machine_name_001 = new Label();
            cmb_cnc_control_system = new ComboBox();
            dataGridView1 = new DataGridView();

            // ✅ SADECE 2 KOLON TANIMLA (Boş kolon yok!)
            col_RowNumber = new DataGridViewTextBoxColumn();        // Sıra numarası
            col_MachineName = new DataGridViewTextBoxColumn();      // Machine Name
            col_ControlSystem = new DataGridViewTextBoxColumn();    // Control System

            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(btn_new_machine_save, 1, 2);
            tableLayoutPanel1.Controls.Add(lbl_Control_system, 0, 1);
            tableLayoutPanel1.Controls.Add(textBox1, 1, 0);
            tableLayoutPanel1.Controls.Add(lbl_Machine_name_001, 0, 0);
            tableLayoutPanel1.Controls.Add(cmb_cnc_control_system, 1, 1);
            tableLayoutPanel1.Location = new Point(36, 57);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(200, 171);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // btn_new_machine_save
            // 
            btn_new_machine_save.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btn_new_machine_save.Image = (Image)resources.GetObject("btn_new_machine_save.Image");
            btn_new_machine_save.Location = new Point(157, 64);
            btn_new_machine_save.Name = "btn_new_machine_save";
            btn_new_machine_save.Size = new Size(40, 36);
            btn_new_machine_save.TabIndex = 12;
            btn_new_machine_save.UseVisualStyleBackColor = true;
            btn_new_machine_save.Click += btn_new_machine_save_Click;
            // 
            // lbl_Control_system
            // 
            lbl_Control_system.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lbl_Control_system.AutoSize = true;
            lbl_Control_system.Location = new Point(3, 30);
            lbl_Control_system.Name = "lbl_Control_system";
            lbl_Control_system.Size = new Size(94, 30);
            lbl_Control_system.TabIndex = 1;
            lbl_Control_system.Text = "Control System";
            lbl_Control_system.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Left;
            textBox1.Location = new Point(103, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(94, 23);
            textBox1.TabIndex = 1;
            textBox1.TextAlign = HorizontalAlignment.Right;
            // 
            // lbl_Machine_name_001
            // 
            lbl_Machine_name_001.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lbl_Machine_name_001.AutoSize = true;
            lbl_Machine_name_001.Location = new Point(3, 0);
            lbl_Machine_name_001.Name = "lbl_Machine_name_001";
            lbl_Machine_name_001.Size = new Size(94, 30);
            lbl_Machine_name_001.TabIndex = 0;
            lbl_Machine_name_001.Text = "Machine Name";
            lbl_Machine_name_001.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cmb_cnc_control_system
            // 
            cmb_cnc_control_system.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cmb_cnc_control_system.FormattingEnabled = true;
            cmb_cnc_control_system.Location = new Point(103, 33);
            cmb_cnc_control_system.Name = "cmb_cnc_control_system";
            cmb_cnc_control_system.Size = new Size(94, 23);
            cmb_cnc_control_system.TabIndex = 1;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            // ✅ 3 KOLON EKLE (Boş kolon yok!)
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] {
                col_RowNumber,      // #
                col_MachineName,    // Name
                col_ControlSystem   // Control System
            });

            dataGridView1.Location = new Point(259, 57);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(350, 171);
            dataGridView1.TabIndex = 1;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            // 
            // col_RowNumber
            // 
            col_RowNumber.HeaderText = "#";
            col_RowNumber.Name = "col_RowNumber";
            col_RowNumber.Width = 40;
            col_RowNumber.ReadOnly = true;
            // 
            // col_MachineName
            // 
            col_MachineName.HeaderText = "Name";
            col_MachineName.Name = "col_MachineName";
            col_MachineName.Width = 150;
            col_MachineName.ReadOnly = true;
            // 
            // col_ControlSystem
            // 
            col_ControlSystem.HeaderText = "Control System";
            col_ControlSystem.Name = "col_ControlSystem";
            col_ControlSystem.Width = 130;
            col_ControlSystem.ReadOnly = true;
            // 
            // Form_CNC_Machines
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(630, 279);
            Controls.Add(dataGridView1);
            Controls.Add(tableLayoutPanel1);
            Name = "Form_CNC_Machines";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CNC Machines";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label lbl_Machine_name_001;
        private Label lbl_Control_system;
        private TextBox textBox1;
        private ComboBox cmb_cnc_control_system;
        private Button btn_new_machine_save;
        private DataGridView dataGridView1;

        // ✅ 3 KOLON TANIMLA
        private DataGridViewTextBoxColumn col_RowNumber;
        private DataGridViewTextBoxColumn col_MachineName;
        private DataGridViewTextBoxColumn col_ControlSystem;
    }
}