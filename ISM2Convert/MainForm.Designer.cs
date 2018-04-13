namespace ISM2Convert
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSelectInput = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblInputInfo = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkSplitMeshes = new System.Windows.Forms.CheckBox();
            this.chkMergeVertices = new System.Windows.Forms.CheckBox();
            this.btnCv = new System.Windows.Forms.Button();
            this.tbOutputlog = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cbOutputFormat = new System.Windows.Forms.ComboBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.dlgOpenISM = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSelectInput
            // 
            this.btnSelectInput.Location = new System.Drawing.Point(6, 19);
            this.btnSelectInput.Name = "btnSelectInput";
            this.btnSelectInput.Size = new System.Drawing.Size(438, 23);
            this.btnSelectInput.TabIndex = 0;
            this.btnSelectInput.Text = "Select input files";
            this.btnSelectInput.UseVisualStyleBackColor = true;
            this.btnSelectInput.Click += new System.EventHandler(this.btnSelectInput_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblInputInfo);
            this.groupBox1.Controls.Add(this.btnSelectInput);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(450, 75);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Input files";
            // 
            // lblInputInfo
            // 
            this.lblInputInfo.Location = new System.Drawing.Point(6, 45);
            this.lblInputInfo.Name = "lblInputInfo";
            this.lblInputInfo.Size = new System.Drawing.Size(438, 26);
            this.lblInputInfo.TabIndex = 1;
            this.lblInputInfo.Text = " - no files selected -";
            this.lblInputInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkSplitMeshes);
            this.groupBox2.Controls.Add(this.chkMergeVertices);
            this.groupBox2.Location = new System.Drawing.Point(12, 93);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(450, 74);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // chkSplitMeshes
            // 
            this.chkSplitMeshes.AutoSize = true;
            this.chkSplitMeshes.Location = new System.Drawing.Point(9, 42);
            this.chkSplitMeshes.Name = "chkSplitMeshes";
            this.chkSplitMeshes.Size = new System.Drawing.Size(341, 17);
            this.chkSplitMeshes.TabIndex = 1;
            this.chkSplitMeshes.Text = "Try to split model parts into meshes (not recommendable for stages)";
            this.chkSplitMeshes.UseVisualStyleBackColor = true;
            // 
            // chkMergeVertices
            // 
            this.chkMergeVertices.AutoSize = true;
            this.chkMergeVertices.Location = new System.Drawing.Point(9, 19);
            this.chkMergeVertices.Name = "chkMergeVertices";
            this.chkMergeVertices.Size = new System.Drawing.Size(337, 17);
            this.chkMergeVertices.TabIndex = 0;
            this.chkMergeVertices.Text = "Try to merge vertices (Recommended for Megadimension meshes)";
            this.chkMergeVertices.UseVisualStyleBackColor = true;
            // 
            // btnCv
            // 
            this.btnCv.Enabled = false;
            this.btnCv.Location = new System.Drawing.Point(12, 224);
            this.btnCv.Name = "btnCv";
            this.btnCv.Size = new System.Drawing.Size(450, 23);
            this.btnCv.TabIndex = 3;
            this.btnCv.Text = "Convert";
            this.btnCv.UseVisualStyleBackColor = true;
            this.btnCv.Click += new System.EventHandler(this.btnCv_Click);
            // 
            // tbOutputlog
            // 
            this.tbOutputlog.BackColor = System.Drawing.Color.Black;
            this.tbOutputlog.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.tbOutputlog.ForeColor = System.Drawing.Color.Silver;
            this.tbOutputlog.Location = new System.Drawing.Point(6, 19);
            this.tbOutputlog.Multiline = true;
            this.tbOutputlog.Name = "tbOutputlog";
            this.tbOutputlog.ReadOnly = true;
            this.tbOutputlog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbOutputlog.Size = new System.Drawing.Size(438, 183);
            this.tbOutputlog.TabIndex = 4;
            this.tbOutputlog.Text = "- output log -";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tbOutputlog);
            this.groupBox3.Location = new System.Drawing.Point(12, 253);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(450, 208);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Output log";
            // 
            // cbOutputFormat
            // 
            this.cbOutputFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOutputFormat.FormattingEnabled = true;
            this.cbOutputFormat.Items.AddRange(new object[] {
            "PMX",
            "PMD"});
            this.cbOutputFormat.Location = new System.Drawing.Point(9, 19);
            this.cbOutputFormat.Name = "cbOutputFormat";
            this.cbOutputFormat.Size = new System.Drawing.Size(435, 21);
            this.cbOutputFormat.TabIndex = 2;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cbOutputFormat);
            this.groupBox4.Location = new System.Drawing.Point(12, 173);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(450, 45);
            this.groupBox4.TabIndex = 6;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Output format";
            // 
            // dlgOpenISM
            // 
            this.dlgOpenISM.DefaultExt = "ism2";
            this.dlgOpenISM.FileName = ".ism2";
            this.dlgOpenISM.Filter = "ISM2 files (*.ism2)|*.ism2|All files (*.*)|*.*";
            this.dlgOpenISM.Multiselect = true;
            this.dlgOpenISM.ReadOnlyChecked = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(474, 473);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnCv);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(490, 512);
            this.MinimumSize = new System.Drawing.Size(490, 512);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Converter";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSelectInput;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblInputInfo;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkSplitMeshes;
        private System.Windows.Forms.CheckBox chkMergeVertices;
        private System.Windows.Forms.Button btnCv;
        private System.Windows.Forms.TextBox tbOutputlog;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cbOutputFormat;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.OpenFileDialog dlgOpenISM;
    }
}

