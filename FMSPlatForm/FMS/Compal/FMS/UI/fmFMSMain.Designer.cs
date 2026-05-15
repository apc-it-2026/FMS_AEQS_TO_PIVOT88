#region Copyright & License
/******************************************************************************
* This document is the property of LCFC Electronics Inc, (LCFC).
* No exploitation or transfer of any information contained herein is permitted 
* in the absence of an agreement with LCFC, 
* and neither the document nor any such information
* may be released without the written consent of LCFC
*  
* All right reserved by LCFC Electronics Inc.  
*******************************************************************************
* Owner: Jason   
* Version: 1.2.0.4
* FMS.Component: MES File Monitor
* Function Description:*
* Revision / History
*------------------------------------------------------------------------------
* Flag     Date     Who             Changes Description
* -------- -------- --------------- -------------------------------------------
*          20100730 Jason           File created for new simple FMS
* JC01     20101004 Jason           Modify modify servic info design        
*------------------------------------------------------------------------------
*/
#endregion
using System.Windows.Forms;
using Compal.FMS.Kernel.Utils;
namespace Compal.FMS.UI
{
    partial class fmFMSMain
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
            if (!FMSConfig.CheckServiceRunning())//@JC01M
            {
                DialogResult dlgResult = MessageBox.Show("Do you want to close FMS ?", "Close Warning",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);//@JC01M
                if (dlgResult == DialogResult.No)
                {
                    return;
                }
                else
                {
                    if (disposing && (components != null))
                    {
                        components.Dispose();
                    }
                    base.Dispose(disposing);
                    if (fmsLog.IsInfoEnabled)
                        fmsLog.Info("FMS closed successfully.");//@JC01M
                }
            }
            else
            {
                MessageBox.Show("FMS can't be closed in this time. pls STOP all service first.", "Closed Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);//@JC01M
            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fmFMSMain));
            this.msTopMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiModify = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.databaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsActions = new System.Windows.Forms.ToolStrip();
            this.tsbtnNew = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbtnStartOne = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbtnStartAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbtnPauseOne = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbtnPauseAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbtnDeleteOne = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbtnDeleteAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbtnRefresh = new System.Windows.Forms.ToolStripButton();
            this.stspStauts = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslbAPServerIP = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssAPSrvConnectSts = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslProcessID = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel6 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslStartTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel7 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel8 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tcpSrvList = new System.Windows.Forms.TabPage();
            this.dgwServer = new System.Windows.Forms.DataGridView();
            this.tbcMainControls = new System.Windows.Forms.TabControl();
            this.ServiceCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SrvStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SDB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Operation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SyncType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Interval = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IDB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MDB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.msTopMain.SuspendLayout();
            this.tsActions.SuspendLayout();
            this.stspStauts.SuspendLayout();
            this.tcpSrvList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwServer)).BeginInit();
            this.tbcMainControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // msTopMain
            // 
            this.msTopMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.exitToolStripMenuItem,
            this.databaseToolStripMenuItem});
            this.msTopMain.Location = new System.Drawing.Point(0, 0);
            this.msTopMain.Name = "msTopMain";
            this.msTopMain.Size = new System.Drawing.Size(913, 24);
            this.msTopMain.TabIndex = 0;
            this.msTopMain.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem1});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.newToolStripMenuItem.Text = "&New Download Config";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(230, 6);
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Q)));
            this.exitToolStripMenuItem1.Size = new System.Drawing.Size(233, 22);
            this.exitToolStripMenuItem1.Text = "&Quit";
            this.exitToolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiModify,
            this.toolStripSeparator3,
            this.tsmiRemove,
            this.tsmiRemoveAll,
            this.toolStripSeparator2});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // tsmiModify
            // 
            this.tsmiModify.Name = "tsmiModify";
            this.tsmiModify.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.M)));
            this.tsmiModify.Size = new System.Drawing.Size(185, 22);
            this.tsmiModify.Text = "&Modify";
            this.tsmiModify.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(182, 6);
            // 
            // tsmiRemove
            // 
            this.tsmiRemove.Name = "tsmiRemove";
            this.tsmiRemove.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.tsmiRemove.Size = new System.Drawing.Size(185, 22);
            this.tsmiRemove.Text = "&Removed";
            this.tsmiRemove.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // tsmiRemoveAll
            // 
            this.tsmiRemoveAll.Name = "tsmiRemoveAll";
            this.tsmiRemoveAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.tsmiRemoveAll.Size = new System.Drawing.Size(185, 22);
            this.tsmiRemoveAll.Text = "&Remove All";
            this.tsmiRemoveAll.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(182, 6);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Q)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // databaseToolStripMenuItem
            // 
            this.databaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectionConfigToolStripMenuItem});
            this.databaseToolStripMenuItem.Name = "databaseToolStripMenuItem";
            this.databaseToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.databaseToolStripMenuItem.Text = "Database";
            // 
            // connectionConfigToolStripMenuItem
            // 
            this.connectionConfigToolStripMenuItem.Name = "connectionConfigToolStripMenuItem";
            this.connectionConfigToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.connectionConfigToolStripMenuItem.Text = "Config Connection";
            this.connectionConfigToolStripMenuItem.Click += new System.EventHandler(this.connectionConfigToolStripMenuItem_Click);
            // 
            // tsActions
            // 
            this.tsActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbtnNew,
            this.toolStripSeparator4,
            this.tsbtnStartOne,
            this.toolStripSeparator5,
            this.tsbtnStartAll,
            this.toolStripSeparator6,
            this.tsbtnPauseOne,
            this.toolStripSeparator7,
            this.tsbtnPauseAll,
            this.toolStripSeparator8,
            this.tsbtnDeleteOne,
            this.toolStripSeparator9,
            this.tsbtnDeleteAll,
            this.toolStripSeparator10,
            this.tsbtnRefresh});
            this.tsActions.Location = new System.Drawing.Point(0, 24);
            this.tsActions.Name = "tsActions";
            this.tsActions.Size = new System.Drawing.Size(913, 25);
            this.tsActions.TabIndex = 1;
            this.tsActions.Text = "toolStrip1";
            // 
            // tsbtnNew
            // 
            this.tsbtnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnNew.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnNew.Image")));
            this.tsbtnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnNew.Name = "tsbtnNew";
            this.tsbtnNew.Size = new System.Drawing.Size(23, 22);
            this.tsbtnNew.Text = "Service Configuration";
            this.tsbtnNew.ToolTipText = "Service Configuration";
            this.tsbtnNew.Click += new System.EventHandler(this.tsbtnNew_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbtnStartOne
            // 
            this.tsbtnStartOne.Enabled = false;
            this.tsbtnStartOne.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnStartOne.Image")));
            this.tsbtnStartOne.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnStartOne.Name = "tsbtnStartOne";
            this.tsbtnStartOne.Size = new System.Drawing.Size(51, 22);
            this.tsbtnStartOne.Text = "Start";
            this.tsbtnStartOne.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.tsbtnStartOne.ToolTipText = "Start One (Selected Server)";
            this.tsbtnStartOne.Click += new System.EventHandler(this.tsbtnStartOne_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbtnStartAll
            // 
            this.tsbtnStartAll.Enabled = false;
            this.tsbtnStartAll.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnStartAll.Image")));
            this.tsbtnStartAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnStartAll.Name = "tsbtnStartAll";
            this.tsbtnStartAll.Size = new System.Drawing.Size(68, 22);
            this.tsbtnStartAll.Text = "Start All";
            this.tsbtnStartAll.ToolTipText = "Start All Server (All Server Info in list)";
            this.tsbtnStartAll.Click += new System.EventHandler(this.tsbtnStartAll_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbtnPauseOne
            // 
            this.tsbtnPauseOne.Enabled = false;
            this.tsbtnPauseOne.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnPauseOne.Image")));
            this.tsbtnPauseOne.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnPauseOne.Name = "tsbtnPauseOne";
            this.tsbtnPauseOne.Size = new System.Drawing.Size(58, 22);
            this.tsbtnPauseOne.Text = "Pause";
            this.tsbtnPauseOne.ToolTipText = "Stop One (Seleceted Server)";
            this.tsbtnPauseOne.Click += new System.EventHandler(this.tsbtnPauseOne_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbtnPauseAll
            // 
            this.tsbtnPauseAll.Enabled = false;
            this.tsbtnPauseAll.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnPauseAll.Image")));
            this.tsbtnPauseAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnPauseAll.Name = "tsbtnPauseAll";
            this.tsbtnPauseAll.Size = new System.Drawing.Size(75, 22);
            this.tsbtnPauseAll.Text = "Pause All";
            this.tsbtnPauseAll.ToolTipText = "Stop All (All Server In List)";
            this.tsbtnPauseAll.Click += new System.EventHandler(this.tsbtnPauseAll_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbtnDeleteOne
            // 
            this.tsbtnDeleteOne.Enabled = false;
            this.tsbtnDeleteOne.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnDeleteOne.Image")));
            this.tsbtnDeleteOne.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnDeleteOne.Name = "tsbtnDeleteOne";
            this.tsbtnDeleteOne.Size = new System.Drawing.Size(60, 22);
            this.tsbtnDeleteOne.Text = "Delete";
            this.tsbtnDeleteOne.ToolTipText = "Delete One (Selected Server)";
            this.tsbtnDeleteOne.Click += new System.EventHandler(this.tsbtnDeleteOne_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbtnDeleteAll
            // 
            this.tsbtnDeleteAll.Enabled = false;
            this.tsbtnDeleteAll.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnDeleteAll.Image")));
            this.tsbtnDeleteAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnDeleteAll.Name = "tsbtnDeleteAll";
            this.tsbtnDeleteAll.Size = new System.Drawing.Size(77, 22);
            this.tsbtnDeleteAll.Text = "Delete All";
            this.tsbtnDeleteAll.ToolTipText = "Delete All(All Server In List)";
            this.tsbtnDeleteAll.Click += new System.EventHandler(this.tsbtnDeleteAll_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbtnRefresh
            // 
            this.tsbtnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnRefresh.Image")));
            this.tsbtnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnRefresh.Name = "tsbtnRefresh";
            this.tsbtnRefresh.Size = new System.Drawing.Size(66, 22);
            this.tsbtnRefresh.Text = "Refresh";
            this.tsbtnRefresh.ToolTipText = "Refresh(Reload All new server config)";
            this.tsbtnRefresh.Click += new System.EventHandler(this.tsbtnRefresh_Click);
            // 
            // stspStauts
            // 
            this.stspStauts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel4,
            this.tsslbAPServerIP,
            this.toolStripStatusLabel2,
            this.tssAPSrvConnectSts,
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel3,
            this.tsslProcessID,
            this.toolStripStatusLabel5,
            this.toolStripStatusLabel6,
            this.tsslStartTime,
            this.toolStripStatusLabel7,
            this.toolStripStatusLabel8});
            this.stspStauts.Location = new System.Drawing.Point(0, 450);
            this.stspStauts.Name = "stspStauts";
            this.stspStauts.Size = new System.Drawing.Size(913, 22);
            this.stspStauts.TabIndex = 2;
            this.stspStauts.Text = "statusStrip1";
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(49, 17);
            this.toolStripStatusLabel4.Text = "Run At :";
            // 
            // tsslbAPServerIP
            // 
            this.tsslbAPServerIP.Name = "tsslbAPServerIP";
            this.tsslbAPServerIP.Size = new System.Drawing.Size(52, 17);
            this.tsslbAPServerIP.Text = "127.0.0.1";
            this.tsslbAPServerIP.ToolTipText = "AP Server IP";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 17);
            // 
            // tssAPSrvConnectSts
            // 
            this.tssAPSrvConnectSts.Name = "tssAPSrvConnectSts";
            this.tssAPSrvConnectSts.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(22, 17);
            this.toolStripStatusLabel1.Text = "  |  ";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(28, 17);
            this.toolStripStatusLabel3.Text = "PID:";
            // 
            // tsslProcessID
            // 
            this.tsslProcessID.Name = "tsslProcessID";
            this.tsslProcessID.Size = new System.Drawing.Size(31, 17);
            this.tsslProcessID.Text = "1605";
            // 
            // toolStripStatusLabel5
            // 
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(10, 17);
            this.toolStripStatusLabel5.Text = "|";
            // 
            // toolStripStatusLabel6
            // 
            this.toolStripStatusLabel6.Name = "toolStripStatusLabel6";
            this.toolStripStatusLabel6.Size = new System.Drawing.Size(49, 17);
            this.toolStripStatusLabel6.Text = "Start At:";
            // 
            // tsslStartTime
            // 
            this.tsslStartTime.Name = "tsslStartTime";
            this.tsslStartTime.Size = new System.Drawing.Size(89, 17);
            this.tsslStartTime.Text = "2014/3/19 09:36";
            // 
            // toolStripStatusLabel7
            // 
            this.toolStripStatusLabel7.Name = "toolStripStatusLabel7";
            this.toolStripStatusLabel7.Size = new System.Drawing.Size(10, 17);
            this.toolStripStatusLabel7.Text = "|";
            // 
            // toolStripStatusLabel8
            // 
            this.toolStripStatusLabel8.Name = "toolStripStatusLabel8";
            this.toolStripStatusLabel8.Size = new System.Drawing.Size(242, 17);
            this.toolStripStatusLabel8.Text = "Copyright@ApacheFootwear MES 2019-2022";
            // 
            // tcpSrvList
            // 
            this.tcpSrvList.Controls.Add(this.dgwServer);
            this.tcpSrvList.Location = new System.Drawing.Point(4, 24);
            this.tcpSrvList.Name = "tcpSrvList";
            this.tcpSrvList.Padding = new System.Windows.Forms.Padding(3);
            this.tcpSrvList.Size = new System.Drawing.Size(905, 373);
            this.tcpSrvList.TabIndex = 0;
            this.tcpSrvList.Text = "Server List";
            this.tcpSrvList.UseVisualStyleBackColor = true;
            // 
            // dgwServer
            // 
            this.dgwServer.AllowUserToAddRows = false;
            this.dgwServer.AllowUserToDeleteRows = false;
            this.dgwServer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwServer.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ServiceCategory,
            this.SrvStatus,
            this.SDB,
            this.Operation,
            this.SyncType,
            this.Interval,
            this.IDB,
            this.MDB});
            this.dgwServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgwServer.Location = new System.Drawing.Point(3, 3);
            this.dgwServer.Name = "dgwServer";
            this.dgwServer.ReadOnly = true;
            this.dgwServer.RowTemplate.Height = 24;
            this.dgwServer.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgwServer.Size = new System.Drawing.Size(899, 367);
            this.dgwServer.TabIndex = 36;
            this.dgwServer.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwServer_CellContentDoubleClick);
            // 
            // tbcMainControls
            // 
            this.tbcMainControls.Controls.Add(this.tcpSrvList);
            this.tbcMainControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbcMainControls.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbcMainControls.Location = new System.Drawing.Point(0, 49);
            this.tbcMainControls.Name = "tbcMainControls";
            this.tbcMainControls.SelectedIndex = 0;
            this.tbcMainControls.Size = new System.Drawing.Size(913, 401);
            this.tbcMainControls.TabIndex = 3;
            // 
            // ServiceCategory
            // 
            this.ServiceCategory.HeaderText = "Type";
            this.ServiceCategory.Name = "ServiceCategory";
            this.ServiceCategory.ReadOnly = true;
            this.ServiceCategory.Width = 125;
            // 
            // SrvStatus
            // 
            this.SrvStatus.HeaderText = "Status";
            this.SrvStatus.Name = "SrvStatus";
            this.SrvStatus.ReadOnly = true;
            // 
            // SDB
            // 
            this.SDB.HeaderText = "Source DB";
            this.SDB.Name = "SDB";
            this.SDB.ReadOnly = true;
            this.SDB.Width = 150;
            // 
            // Operation
            // 
            this.Operation.HeaderText = "Operation";
            this.Operation.Name = "Operation";
            this.Operation.ReadOnly = true;
            this.Operation.Width = 180;
            // 
            // SyncType
            // 
            this.SyncType.HeaderText = "SyncType";
            this.SyncType.Name = "SyncType";
            this.SyncType.ReadOnly = true;
            this.SyncType.Width = 125;
            // 
            // Interval
            // 
            this.Interval.HeaderText = "Interval";
            this.Interval.Name = "Interval";
            this.Interval.ReadOnly = true;
            // 
            // IDB
            // 
            this.IDB.HeaderText = "Int DB";
            this.IDB.Name = "IDB";
            this.IDB.ReadOnly = true;
            this.IDB.Visible = false;
            this.IDB.Width = 150;
            // 
            // MDB
            // 
            this.MDB.HeaderText = "MES DB";
            this.MDB.Name = "MDB";
            this.MDB.ReadOnly = true;
            this.MDB.Visible = false;
            this.MDB.Width = 150;
            // 
            // fmFMSMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 472);
            this.Controls.Add(this.tbcMainControls);
            this.Controls.Add(this.stspStauts);
            this.Controls.Add(this.tsActions);
            this.Controls.Add(this.msTopMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.msTopMain;
            this.Name = "fmFMSMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "fmFMSMain";
            this.msTopMain.ResumeLayout(false);
            this.msTopMain.PerformLayout();
            this.tsActions.ResumeLayout(false);
            this.tsActions.PerformLayout();
            this.stspStauts.ResumeLayout(false);
            this.stspStauts.PerformLayout();
            this.tcpSrvList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgwServer)).EndInit();
            this.tbcMainControls.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip msTopMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemoveAll;
        private System.Windows.Forms.ToolStrip tsActions;
        private System.Windows.Forms.StatusStrip stspStauts;
        private System.Windows.Forms.ToolStripMenuItem tsmiModify;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsbtnNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel tsslbAPServerIP;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel tssAPSrvConnectSts;
        private System.Windows.Forms.ToolStripButton tsbtnStartOne;
        private System.Windows.Forms.ToolStripButton tsbtnStartAll;
        private System.Windows.Forms.ToolStripButton tsbtnPauseOne;
        private System.Windows.Forms.ToolStripButton tsbtnPauseAll;
        private System.Windows.Forms.ToolStripButton tsbtnDeleteOne;
        private System.Windows.Forms.ToolStripButton tsbtnDeleteAll;
        private System.Windows.Forms.ToolStripButton tsbtnRefresh;
        private TabPage tcpSrvList;
        private DataGridView dgwServer;
        private TabControl tbcMainControls;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripMenuItem databaseToolStripMenuItem;
        private ToolStripMenuItem connectionConfigToolStripMenuItem;
        private ToolStripStatusLabel toolStripStatusLabel3;
        private ToolStripStatusLabel tsslProcessID;
        private ToolStripStatusLabel toolStripStatusLabel5;
        private ToolStripStatusLabel toolStripStatusLabel6;
        private ToolStripStatusLabel tsslStartTime;
        private ToolStripStatusLabel toolStripStatusLabel7;
        private ToolStripStatusLabel toolStripStatusLabel8;
        private DataGridViewTextBoxColumn ServiceCategory;
        private DataGridViewTextBoxColumn SrvStatus;
        private DataGridViewTextBoxColumn SDB;
        private DataGridViewTextBoxColumn Operation;
        private DataGridViewTextBoxColumn SyncType;
        private DataGridViewTextBoxColumn Interval;
        private DataGridViewTextBoxColumn IDB;
        private DataGridViewTextBoxColumn MDB;
    }
}