namespace Compal.FMS.UI
{
    partial class fmDLConfig
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tvSrvList = new System.Windows.Forms.TreeView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cmbSyncType = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnManual = new System.Windows.Forms.Button();
            this.labMS = new System.Windows.Forms.Label();
            this.txbInterval = new System.Windows.Forms.TextBox();
            this.labInterval = new System.Windows.Forms.Label();
            this.txtStartDate = new System.Windows.Forms.TextBox();
            this.cmbOperations = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbIDB = new System.Windows.Forms.ComboBox();
            this.cmbMDB = new System.Windows.Forms.ComboBox();
            this.cmbServiceCategory = new System.Windows.Forms.ComboBox();
            this.labServiceCategory = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.cmbSDB = new System.Windows.Forms.ComboBox();
            this.labDatabase = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Size = new System.Drawing.Size(680, 425);
            this.splitContainer1.SplitterDistance = 218;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tvSrvList);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(218, 425);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ServerList";
            // 
            // tvSrvList
            // 
            this.tvSrvList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvSrvList.FullRowSelect = true;
            this.tvSrvList.Location = new System.Drawing.Point(3, 16);
            this.tvSrvList.Name = "tvSrvList";
            this.tvSrvList.Size = new System.Drawing.Size(212, 406);
            this.tvSrvList.TabIndex = 4;
            this.tvSrvList.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvSrvList_NodeMouseDoubleClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cmbSyncType);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.btnManual);
            this.groupBox2.Controls.Add(this.labMS);
            this.groupBox2.Controls.Add(this.txbInterval);
            this.groupBox2.Controls.Add(this.labInterval);
            this.groupBox2.Controls.Add(this.txtStartDate);
            this.groupBox2.Controls.Add(this.cmbOperations);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.cmbIDB);
            this.groupBox2.Controls.Add(this.cmbMDB);
            this.groupBox2.Controls.Add(this.cmbServiceCategory);
            this.groupBox2.Controls.Add(this.labServiceCategory);
            this.groupBox2.Controls.Add(this.btnExit);
            this.groupBox2.Controls.Add(this.btnSave);
            this.groupBox2.Controls.Add(this.cmbSDB);
            this.groupBox2.Controls.Add(this.labDatabase);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(458, 423);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Base Info";
            // 
            // cmbSyncType
            // 
            this.cmbSyncType.FormattingEnabled = true;
            this.cmbSyncType.Items.AddRange(new object[] {
            "Daily",
            "Interval",
            "Monthly"});
            this.cmbSyncType.Location = new System.Drawing.Point(341, 182);
            this.cmbSyncType.Name = "cmbSyncType";
            this.cmbSyncType.Size = new System.Drawing.Size(99, 21);
            this.cmbSyncType.TabIndex = 65;
            this.cmbSyncType.SelectedIndexChanged += new System.EventHandler(this.cmbSyncType_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(249, 190);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(61, 13);
            this.label6.TabIndex = 64;
            this.label6.Text = "Sync Type:";
            // 
            // btnManual
            // 
            this.btnManual.Location = new System.Drawing.Point(10, 293);
            this.btnManual.Name = "btnManual";
            this.btnManual.Size = new System.Drawing.Size(141, 20);
            this.btnManual.TabIndex = 58;
            this.btnManual.Text = "Post SAP Manual";
            this.btnManual.UseVisualStyleBackColor = true;
            this.btnManual.Click += new System.EventHandler(this.BtnManual_Click);
            // 
            // labMS
            // 
            this.labMS.AutoSize = true;
            this.labMS.Location = new System.Drawing.Point(399, 224);
            this.labMS.Name = "labMS";
            this.labMS.Size = new System.Drawing.Size(48, 13);
            this.labMS.TabIndex = 61;
            this.labMS.Text = "(HH:mm)";
            // 
            // txbInterval
            // 
            this.txbInterval.Location = new System.Drawing.Point(341, 217);
            this.txbInterval.Name = "txbInterval";
            this.txbInterval.Size = new System.Drawing.Size(52, 20);
            this.txbInterval.TabIndex = 59;
            // 
            // labInterval
            // 
            this.labInterval.AutoSize = true;
            this.labInterval.Location = new System.Drawing.Point(260, 224);
            this.labInterval.Name = "labInterval";
            this.labInterval.Size = new System.Drawing.Size(75, 13);
            this.labInterval.TabIndex = 60;
            this.labInterval.Text = "TIme Interval :";
            // 
            // txtStartDate
            // 
            this.txtStartDate.Location = new System.Drawing.Point(10, 310);
            this.txtStartDate.Multiline = true;
            this.txtStartDate.Name = "txtStartDate";
            this.txtStartDate.Size = new System.Drawing.Size(436, 103);
            this.txtStartDate.TabIndex = 57;
            // 
            // cmbOperations
            // 
            this.cmbOperations.FormattingEnabled = true;
            this.cmbOperations.Location = new System.Drawing.Point(197, 145);
            this.cmbOperations.Name = "cmbOperations";
            this.cmbOperations.Size = new System.Drawing.Size(152, 21);
            this.cmbOperations.TabIndex = 56;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(82, 153);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 54;
            this.label3.Text = "Operation Table";
            // 
            // cmbIDB
            // 
            this.cmbIDB.FormattingEnabled = true;
            this.cmbIDB.Items.AddRange(new object[] {
            "SMT",
            "A31",
            "A32",
            "A51",
            "ABO",
            "A58",
            "T36",
            "C38",
            "S38",
            "A77",
            "T88"});
            this.cmbIDB.Location = new System.Drawing.Point(10, 199);
            this.cmbIDB.Name = "cmbIDB";
            this.cmbIDB.Size = new System.Drawing.Size(28, 21);
            this.cmbIDB.TabIndex = 52;
            this.cmbIDB.Visible = false;
            // 
            // cmbMDB
            // 
            this.cmbMDB.FormattingEnabled = true;
            this.cmbMDB.Items.AddRange(new object[] {
            "SMT",
            "A31",
            "A32",
            "A51",
            "ABO",
            "A58",
            "T36",
            "C38",
            "S38",
            "A77",
            "T88"});
            this.cmbMDB.Location = new System.Drawing.Point(44, 202);
            this.cmbMDB.Name = "cmbMDB";
            this.cmbMDB.Size = new System.Drawing.Size(42, 21);
            this.cmbMDB.TabIndex = 50;
            this.cmbMDB.Visible = false;
            // 
            // cmbServiceCategory
            // 
            this.cmbServiceCategory.FormattingEnabled = true;
            this.cmbServiceCategory.Items.AddRange(new object[] {
            "AEQS to Pivot88",
            "AEQS to Middle"});
            this.cmbServiceCategory.Location = new System.Drawing.Point(197, 46);
            this.cmbServiceCategory.Name = "cmbServiceCategory";
            this.cmbServiceCategory.Size = new System.Drawing.Size(152, 21);
            this.cmbServiceCategory.TabIndex = 48;
            this.cmbServiceCategory.SelectedIndexChanged += new System.EventHandler(this.cmbServiceCategory_SelectedIndexChanged);
            // 
            // labServiceCategory
            // 
            this.labServiceCategory.AutoSize = true;
            this.labServiceCategory.Location = new System.Drawing.Point(107, 49);
            this.labServiceCategory.Name = "labServiceCategory";
            this.labServiceCategory.Size = new System.Drawing.Size(55, 13);
            this.labServiceCategory.TabIndex = 47;
            this.labServiceCategory.Text = "Category :";
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(316, 264);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 25);
            this.btnExit.TabIndex = 17;
            this.btnExit.Text = "&Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(235, 264);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 25);
            this.btnSave.TabIndex = 16;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cmbSDB
            // 
            this.cmbSDB.FormattingEnabled = true;
            this.cmbSDB.Items.AddRange(new object[] {
            "SMT",
            "A31",
            "A32",
            "A51",
            "ABO",
            "A58",
            "T36",
            "C38",
            "S38",
            "A77",
            "T88"});
            this.cmbSDB.Location = new System.Drawing.Point(197, 98);
            this.cmbSDB.Name = "cmbSDB";
            this.cmbSDB.Size = new System.Drawing.Size(152, 21);
            this.cmbSDB.TabIndex = 1;
            // 
            // labDatabase
            // 
            this.labDatabase.AutoSize = true;
            this.labDatabase.Location = new System.Drawing.Point(75, 105);
            this.labDatabase.Name = "labDatabase";
            this.labDatabase.Size = new System.Drawing.Size(96, 13);
            this.labDatabase.TabIndex = 0;
            this.labDatabase.Text = "Source Database :";
            // 
            // fmDLConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 425);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fmDLConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Service Configuration";
            this.Load += new System.EventHandler(this.fmDLConfig_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TreeView tvSrvList;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label labDatabase;
        private System.Windows.Forms.ComboBox cmbSDB;
        private System.Windows.Forms.ComboBox cmbServiceCategory;
        private System.Windows.Forms.Label labServiceCategory;
        private System.Windows.Forms.ComboBox cmbMDB;
        private System.Windows.Forms.ComboBox cmbIDB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnManual;
        private System.Windows.Forms.TextBox txtStartDate;
        private System.Windows.Forms.Label labMS;
        private System.Windows.Forms.TextBox txbInterval;
        private System.Windows.Forms.Label labInterval;
        private System.Windows.Forms.ComboBox cmbSyncType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbOperations;
    }
}