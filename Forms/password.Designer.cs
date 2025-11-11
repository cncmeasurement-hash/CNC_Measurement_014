namespace _014
{
    partial class password
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

        private void InitializeComponent()
        {
            this.label_Title = new System.Windows.Forms.Label();
            this.label_WebText = new System.Windows.Forms.Label();
            this.textBox_WebText = new System.Windows.Forms.TextBox();
            this.button_SaveSettings = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_Title
            // 
            this.label_Title.AutoSize = true;
            this.label_Title.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label_Title.Location = new System.Drawing.Point(30, 30);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(200, 32);
            this.label_Title.TabIndex = 0;
            this.label_Title.Text = "Probe Text Ayarları";
            // 
            // label_WebText
            // 
            this.label_WebText.AutoSize = true;
            this.label_WebText.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label_WebText.Location = new System.Drawing.Point(30, 90);
            this.label_WebText.Name = "label_WebText";
            this.label_WebText.Size = new System.Drawing.Size(140, 23);
            this.label_WebText.TabIndex = 1;
            this.label_WebText.Text = "WEB SİTE METNİ";
            // 
            // textBox_WebText
            // 
            this.textBox_WebText.Location = new System.Drawing.Point(30, 125);
            this.textBox_WebText.Name = "textBox_WebText";
            this.textBox_WebText.Size = new System.Drawing.Size(400, 27);
            this.textBox_WebText.TabIndex = 2;
            this.textBox_WebText.Text = "yalcin cinar";
            // 
            // button_SaveSettings
            // 
            this.button_SaveSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.button_SaveSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_SaveSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.button_SaveSettings.ForeColor = System.Drawing.Color.White;
            this.button_SaveSettings.Location = new System.Drawing.Point(30, 180);
            this.button_SaveSettings.Name = "button_SaveSettings";
            this.button_SaveSettings.Size = new System.Drawing.Size(180, 45);
            this.button_SaveSettings.TabIndex = 3;
            this.button_SaveSettings.Text = "✓ KAYDET";
            this.button_SaveSettings.UseVisualStyleBackColor = false;
            this.button_SaveSettings.Click += new System.EventHandler(this.button_SaveSettings_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.BackColor = System.Drawing.Color.Gray;
            this.button_Cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Cancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.button_Cancel.ForeColor = System.Drawing.Color.White;
            this.button_Cancel.Location = new System.Drawing.Point(220, 180);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(180, 45);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "✕ İPTAL";
            this.button_Cancel.UseVisualStyleBackColor = false;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // password
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 260);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_SaveSettings);
            this.Controls.Add(this.textBox_WebText);
            this.Controls.Add(this.label_WebText);
            this.Controls.Add(this.label_Title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "password";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Probe Text Ayarları";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label label_Title;
        private System.Windows.Forms.Label label_WebText;
        private System.Windows.Forms.TextBox textBox_WebText;
        private System.Windows.Forms.Button button_SaveSettings;
        private System.Windows.Forms.Button button_Cancel;
    }
}