/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * 
 * The contents of this file are subject to the GNU General Public License
 * v3.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy * of the License at 
 * https://github.com/k3ldar/FbReplicationEngine/blob/master/LICENSE
 *
 * Software distributed under the License is distributed on an
 * "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
 * or implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 *  The Original Code was created by Simon Carter (s1cart3r@gmail.com)
 *
 *  Copyright (c) 2011 - 2022 Simon Carter.  All Rights Reserved
 *
 *  Purpose:  
 *
 */
namespace Replication.Service.Forms
{
    partial class ConfigureChild
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
#if DEBUG
            System.GC.SuppressFinalize(this);
#endif
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigureChild));
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabPageOptions = new System.Windows.Forms.TabPage();
            this.btnUninstallReplicationScript = new System.Windows.Forms.Button();
            this.btnInstallReplicationScript = new System.Windows.Forms.Button();
            this.cbEnabled = new System.Windows.Forms.CheckBox();
            this.txtSiteID = new SharedControls.TextBoxEx();
            this.lblSiteID = new System.Windows.Forms.Label();
            this.rbReplicateChild = new System.Windows.Forms.RadioButton();
            this.rbReplicateMaster = new System.Windows.Forms.RadioButton();
            this.cbOptionReplicate = new System.Windows.Forms.CheckBox();
            this.cbOptionRemoteUpdate = new System.Windows.Forms.CheckBox();
            this.cbOptionBackupDatabase = new System.Windows.Forms.CheckBox();
            this.txtName = new SharedControls.TextBoxEx();
            this.label11 = new System.Windows.Forms.Label();
            this.tabPageLocalDatabase = new System.Windows.Forms.TabPage();
            this.dbConnChild = new SharedControls.DatabaseConnection();
            this.tabPageRemoteDatabase = new System.Windows.Forms.TabPage();
            this.dbConnMaster = new SharedControls.DatabaseConnection();
            this.tabPageBackupSettings = new System.Windows.Forms.TabPage();
            this.dtpBackupTime = new System.Windows.Forms.DateTimePicker();
            this.cbBackupAfter = new System.Windows.Forms.CheckBox();
            this.lblBackupMaxAgeDays = new System.Windows.Forms.Label();
            this.udBackupMaxAge = new System.Windows.Forms.NumericUpDown();
            this.lblMaxAge = new System.Windows.Forms.Label();
            this.cbBackupsDeleteOldFiles = new System.Windows.Forms.CheckBox();
            this.txtBackupName = new System.Windows.Forms.TextBox();
            this.lblBackupName = new System.Windows.Forms.Label();
            this.gbBackupFTPDetails = new System.Windows.Forms.GroupBox();
            this.udBackupFTPPort = new System.Windows.Forms.NumericUpDown();
            this.btnBackupFTPTest = new System.Windows.Forms.Button();
            this.txtBackupFTPPassword = new System.Windows.Forms.TextBox();
            this.txtBackupFTPUsername = new System.Windows.Forms.TextBox();
            this.txtBackupFTPHost = new System.Windows.Forms.TextBox();
            this.lblBackupFTPPort = new System.Windows.Forms.Label();
            this.lblBackupFTPPassword = new System.Windows.Forms.Label();
            this.lblBackupFTPUsername = new System.Windows.Forms.Label();
            this.lblBackupFTPHost = new System.Windows.Forms.Label();
            this.cbBackupCopyToRemote = new System.Windows.Forms.CheckBox();
            this.cbBackupFileCompress = new System.Windows.Forms.CheckBox();
            this.cbBackupUseSiteID = new System.Windows.Forms.CheckBox();
            this.txtBackupPath = new System.Windows.Forms.TextBox();
            this.lblBackupPath = new System.Windows.Forms.Label();
            this.tabPageRemoteUpdateSettings = new System.Windows.Forms.TabPage();
            this.txtRemoteUpdateLocation = new System.Windows.Forms.TextBox();
            this.lblRemoteUpdateLocation = new System.Windows.Forms.Label();
            this.txtRemoteUpdateXMLFile = new System.Windows.Forms.TextBox();
            this.lblRemoteUpdateVersion = new System.Windows.Forms.Label();
            this.tabPageReplicationSettings = new System.Windows.Forms.TabPage();
            this.cbForceVerification = new System.Windows.Forms.CheckBox();
            this.dtpVerifyAfter = new System.Windows.Forms.DateTimePicker();
            this.cbRequireUniqueAcess = new System.Windows.Forms.CheckBox();
            this.cbReplicationAutoUpdateTriggers = new System.Windows.Forms.CheckBox();
            this.lblVerifyAfterHours = new System.Windows.Forms.Label();
            this.cbForceHours = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.udTimeoutMinutes = new System.Windows.Forms.NumericUpDown();
            this.label13 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.udUploadCount = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.udDownloadCount = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.udResetErrorCount = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.udReplicateMinutes = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.lblVerifyAfterIterationsIt = new System.Windows.Forms.Label();
            this.udVerifyAll = new System.Windows.Forms.NumericUpDown();
            this.lblVerifyAfterIterations = new System.Windows.Forms.Label();
            this.tabPageTables = new System.Windows.Forms.TabPage();
            this.lvTables = new SharedControls.Classes.ListViewEx();
            this.colTablesName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTableChildReplicated = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTableMasterReplicating = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTableSortOrder = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pumTables = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.pumTablesAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.pumTablesRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.pumTablesConfigure = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.pumTablesValidateAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.pumTablesUpdateSortOrders = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.pumTablesRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPageAutoCorrect = new System.Windows.Forms.TabPage();
            this.lvAutoCorrect = new SharedControls.Classes.ListViewEx();
            this.colAutoCorrectTable = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAutoCorrectKeyName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAutoCorrectOptions = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pumAutoCorrect = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.pumAutoCorrectAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.pumAutoCorrectDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.pumAutoCorrectEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.pumAutoCorrectRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPageChildReplicationSchema = new System.Windows.Forms.TabPage();
            this.btnChildScriptExecute = new System.Windows.Forms.Button();
            this.btnChildCreateScript = new System.Windows.Forms.Button();
            this.txtChildCreateScript = new System.Windows.Forms.TextBox();
            this.tabPageChildDropScript = new System.Windows.Forms.TabPage();
            this.btnChildExecuteRemoveScript = new System.Windows.Forms.Button();
            this.btnGenerateChildDropScript = new System.Windows.Forms.Button();
            this.txtChildDropScript = new System.Windows.Forms.TextBox();
            this.tabPageMasterCreateScript = new System.Windows.Forms.TabPage();
            this.btnMasterCreate = new System.Windows.Forms.Button();
            this.btnMasterCreateExecute = new System.Windows.Forms.Button();
            this.txtMasterCreate = new System.Windows.Forms.TextBox();
            this.tabPageMasterDropScript = new System.Windows.Forms.TabPage();
            this.btnMasterExecuteDropScript = new System.Windows.Forms.Button();
            this.btnMasterDrop = new System.Windows.Forms.Button();
            this.txtMasterDrop = new System.Windows.Forms.TextBox();
            this.tabPageGenerators = new System.Windows.Forms.TabPage();
            this.btnUpdateGenerators = new System.Windows.Forms.Button();
            this.gridGenerators = new System.Windows.Forms.DataGridView();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.textBlockServerSQL = new SharedControls.Controls.TextBlock();
            this.textBlockClientSQL = new SharedControls.Controls.TextBlock();
            this.textBlockClientRemove = new SharedControls.Controls.TextBlock();
            this.textBlockServerRemove = new SharedControls.Controls.TextBlock();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.tabMain.SuspendLayout();
            this.tabPageOptions.SuspendLayout();
            this.tabPageLocalDatabase.SuspendLayout();
            this.tabPageRemoteDatabase.SuspendLayout();
            this.tabPageBackupSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udBackupMaxAge)).BeginInit();
            this.gbBackupFTPDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udBackupFTPPort)).BeginInit();
            this.tabPageRemoteUpdateSettings.SuspendLayout();
            this.tabPageReplicationSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udTimeoutMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udUploadCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udDownloadCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udResetErrorCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udReplicateMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udVerifyAll)).BeginInit();
            this.tabPageTables.SuspendLayout();
            this.pumTables.SuspendLayout();
            this.tabPageAutoCorrect.SuspendLayout();
            this.pumAutoCorrect.SuspendLayout();
            this.tabPageChildReplicationSchema.SuspendLayout();
            this.tabPageChildDropScript.SuspendLayout();
            this.tabPageMasterCreateScript.SuspendLayout();
            this.tabPageMasterDropScript.SuspendLayout();
            this.tabPageGenerators.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridGenerators)).BeginInit();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabMain.Controls.Add(this.tabPageOptions);
            this.tabMain.Controls.Add(this.tabPageLocalDatabase);
            this.tabMain.Controls.Add(this.tabPageRemoteDatabase);
            this.tabMain.Controls.Add(this.tabPageBackupSettings);
            this.tabMain.Controls.Add(this.tabPageRemoteUpdateSettings);
            this.tabMain.Controls.Add(this.tabPageReplicationSettings);
            this.tabMain.Controls.Add(this.tabPageTables);
            this.tabMain.Controls.Add(this.tabPageAutoCorrect);
            this.tabMain.Controls.Add(this.tabPageChildReplicationSchema);
            this.tabMain.Controls.Add(this.tabPageChildDropScript);
            this.tabMain.Controls.Add(this.tabPageMasterCreateScript);
            this.tabMain.Controls.Add(this.tabPageMasterDropScript);
            this.tabMain.Controls.Add(this.tabPageGenerators);
            this.tabMain.Location = new System.Drawing.Point(12, 12);
            this.tabMain.Multiline = true;
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(678, 346);
            this.tabMain.TabIndex = 0;
            // 
            // tabPageOptions
            // 
            this.tabPageOptions.Controls.Add(this.btnUninstallReplicationScript);
            this.tabPageOptions.Controls.Add(this.btnInstallReplicationScript);
            this.tabPageOptions.Controls.Add(this.cbEnabled);
            this.tabPageOptions.Controls.Add(this.txtSiteID);
            this.tabPageOptions.Controls.Add(this.lblSiteID);
            this.tabPageOptions.Controls.Add(this.rbReplicateChild);
            this.tabPageOptions.Controls.Add(this.rbReplicateMaster);
            this.tabPageOptions.Controls.Add(this.cbOptionReplicate);
            this.tabPageOptions.Controls.Add(this.cbOptionRemoteUpdate);
            this.tabPageOptions.Controls.Add(this.cbOptionBackupDatabase);
            this.tabPageOptions.Controls.Add(this.txtName);
            this.tabPageOptions.Controls.Add(this.label11);
            this.tabPageOptions.Location = new System.Drawing.Point(4, 40);
            this.tabPageOptions.Name = "tabPageOptions";
            this.tabPageOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOptions.Size = new System.Drawing.Size(670, 302);
            this.tabPageOptions.TabIndex = 7;
            this.tabPageOptions.Text = "Options";
            this.tabPageOptions.UseVisualStyleBackColor = true;
            // 
            // btnUninstallReplicationScript
            // 
            this.btnUninstallReplicationScript.Location = new System.Drawing.Point(90, 229);
            this.btnUninstallReplicationScript.Name = "btnUninstallReplicationScript";
            this.btnUninstallReplicationScript.Size = new System.Drawing.Size(75, 23);
            this.btnUninstallReplicationScript.TabIndex = 12;
            this.btnUninstallReplicationScript.Text = "Uninstall";
            this.btnUninstallReplicationScript.UseVisualStyleBackColor = true;
            this.btnUninstallReplicationScript.Click += new System.EventHandler(this.btnUninstallReplicationScript_Click);
            // 
            // btnInstallReplicationScript
            // 
            this.btnInstallReplicationScript.Location = new System.Drawing.Point(9, 229);
            this.btnInstallReplicationScript.Name = "btnInstallReplicationScript";
            this.btnInstallReplicationScript.Size = new System.Drawing.Size(75, 23);
            this.btnInstallReplicationScript.TabIndex = 11;
            this.btnInstallReplicationScript.Text = "Install";
            this.btnInstallReplicationScript.UseVisualStyleBackColor = true;
            this.btnInstallReplicationScript.Click += new System.EventHandler(this.btnInstallReplicationScript_Click);
            // 
            // cbEnabled
            // 
            this.cbEnabled.AutoSize = true;
            this.cbEnabled.Location = new System.Drawing.Point(9, 48);
            this.cbEnabled.Name = "cbEnabled";
            this.cbEnabled.Size = new System.Drawing.Size(65, 17);
            this.cbEnabled.TabIndex = 2;
            this.cbEnabled.Text = "Enabled";
            this.cbEnabled.UseVisualStyleBackColor = true;
            // 
            // txtSiteID
            // 
            this.txtSiteID.AllowBackSpace = true;
            this.txtSiteID.AllowCopy = true;
            this.txtSiteID.AllowCut = true;
            this.txtSiteID.AllowedCharacters = "0123456789";
            this.txtSiteID.AllowPaste = true;
            this.txtSiteID.Location = new System.Drawing.Point(72, 191);
            this.txtSiteID.MaxLength = 5;
            this.txtSiteID.Name = "txtSiteID";
            this.txtSiteID.Size = new System.Drawing.Size(100, 20);
            this.txtSiteID.TabIndex = 9;
            // 
            // lblSiteID
            // 
            this.lblSiteID.AutoSize = true;
            this.lblSiteID.Location = new System.Drawing.Point(27, 194);
            this.lblSiteID.Name = "lblSiteID";
            this.lblSiteID.Size = new System.Drawing.Size(39, 13);
            this.lblSiteID.TabIndex = 8;
            this.lblSiteID.Text = "Site ID";
            // 
            // rbReplicateChild
            // 
            this.rbReplicateChild.AutoSize = true;
            this.rbReplicateChild.Location = new System.Drawing.Point(30, 167);
            this.rbReplicateChild.Name = "rbReplicateChild";
            this.rbReplicateChild.Size = new System.Drawing.Size(97, 17);
            this.rbReplicateChild.TabIndex = 7;
            this.rbReplicateChild.Text = "Child Database";
            this.rbReplicateChild.UseVisualStyleBackColor = true;
            this.rbReplicateChild.CheckedChanged += new System.EventHandler(this.rbReplicateMaster_CheckedChanged);
            // 
            // rbReplicateMaster
            // 
            this.rbReplicateMaster.AutoSize = true;
            this.rbReplicateMaster.Location = new System.Drawing.Point(30, 143);
            this.rbReplicateMaster.Name = "rbReplicateMaster";
            this.rbReplicateMaster.Size = new System.Drawing.Size(106, 17);
            this.rbReplicateMaster.TabIndex = 6;
            this.rbReplicateMaster.Text = "Master Database";
            this.rbReplicateMaster.UseVisualStyleBackColor = true;
            this.rbReplicateMaster.CheckedChanged += new System.EventHandler(this.rbReplicateMaster_CheckedChanged);
            // 
            // cbOptionReplicate
            // 
            this.cbOptionReplicate.AutoSize = true;
            this.cbOptionReplicate.Location = new System.Drawing.Point(9, 119);
            this.cbOptionReplicate.Name = "cbOptionReplicate";
            this.cbOptionReplicate.Size = new System.Drawing.Size(71, 17);
            this.cbOptionReplicate.TabIndex = 5;
            this.cbOptionReplicate.Text = "Replicate";
            this.cbOptionReplicate.UseVisualStyleBackColor = true;
            this.cbOptionReplicate.CheckedChanged += new System.EventHandler(this.cbOptionReplicate_CheckedChanged);
            // 
            // cbOptionRemoteUpdate
            // 
            this.cbOptionRemoteUpdate.AutoSize = true;
            this.cbOptionRemoteUpdate.Location = new System.Drawing.Point(9, 95);
            this.cbOptionRemoteUpdate.Name = "cbOptionRemoteUpdate";
            this.cbOptionRemoteUpdate.Size = new System.Drawing.Size(150, 17);
            this.cbOptionRemoteUpdate.TabIndex = 4;
            this.cbOptionRemoteUpdate.Text = "Remote Database Update";
            this.cbOptionRemoteUpdate.UseVisualStyleBackColor = true;
            this.cbOptionRemoteUpdate.CheckedChanged += new System.EventHandler(this.cbOptionRemoteUpdate_CheckedChanged);
            // 
            // cbOptionBackupDatabase
            // 
            this.cbOptionBackupDatabase.AutoSize = true;
            this.cbOptionBackupDatabase.Location = new System.Drawing.Point(9, 71);
            this.cbOptionBackupDatabase.Name = "cbOptionBackupDatabase";
            this.cbOptionBackupDatabase.Size = new System.Drawing.Size(112, 17);
            this.cbOptionBackupDatabase.TabIndex = 3;
            this.cbOptionBackupDatabase.Text = "Backup Database";
            this.cbOptionBackupDatabase.UseVisualStyleBackColor = true;
            this.cbOptionBackupDatabase.CheckedChanged += new System.EventHandler(this.cbOptionBackupDatabase_CheckedChanged);
            // 
            // txtName
            // 
            this.txtName.AllowBackSpace = true;
            this.txtName.AllowCopy = true;
            this.txtName.AllowCut = true;
            this.txtName.AllowedCharacters = "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            this.txtName.AllowPaste = true;
            this.txtName.Location = new System.Drawing.Point(134, 11);
            this.txtName.MaxLength = 40;
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(362, 20);
            this.txtName.TabIndex = 1;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 14);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Name";
            // 
            // tabPageLocalDatabase
            // 
            this.tabPageLocalDatabase.Controls.Add(this.dbConnChild);
            this.tabPageLocalDatabase.Location = new System.Drawing.Point(4, 40);
            this.tabPageLocalDatabase.Name = "tabPageLocalDatabase";
            this.tabPageLocalDatabase.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLocalDatabase.Size = new System.Drawing.Size(670, 302);
            this.tabPageLocalDatabase.TabIndex = 2;
            this.tabPageLocalDatabase.Text = "Current Database";
            this.tabPageLocalDatabase.UseVisualStyleBackColor = true;
            // 
            // dbConnChild
            // 
            this.dbConnChild.AllowSaveSettings = true;
            this.dbConnChild.Connection = null;
            this.dbConnChild.ConnectionString = "User=;Password=;Database=;DataSource=127.0.0.1;Role=;Port=3050;Dialect=3;Pooling=" +
    "true;MinPoolSize=1;MaxPoolSize=80;Connection Lifetime=600;Packet Size=2048;CharS" +
    "et=";
            this.dbConnChild.ConnectionType = Shared.DatabaseConnectionType.Firebird;
            this.dbConnChild.HintControl = null;
            this.dbConnChild.LocalHostOnly = true;
            this.dbConnChild.Location = new System.Drawing.Point(7, 7);
            this.dbConnChild.Name = "dbConnChild";
            this.dbConnChild.Size = new System.Drawing.Size(279, 267);
            this.dbConnChild.TabIndex = 0;
            this.dbConnChild.TestConnectionSuccess += new System.EventHandler(this.dbConnChild_TestConnectionSuccess);
            // 
            // tabPageRemoteDatabase
            // 
            this.tabPageRemoteDatabase.Controls.Add(this.dbConnMaster);
            this.tabPageRemoteDatabase.Location = new System.Drawing.Point(4, 40);
            this.tabPageRemoteDatabase.Name = "tabPageRemoteDatabase";
            this.tabPageRemoteDatabase.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRemoteDatabase.Size = new System.Drawing.Size(670, 302);
            this.tabPageRemoteDatabase.TabIndex = 3;
            this.tabPageRemoteDatabase.Text = "Master Database";
            this.tabPageRemoteDatabase.UseVisualStyleBackColor = true;
            // 
            // dbConnMaster
            // 
            this.dbConnMaster.AllowSaveSettings = false;
            this.dbConnMaster.Connection = null;
            this.dbConnMaster.ConnectionString = "User=;Password=;Database=;DataSource=;Role=;Port=3050;Dialect=3;Pooling=true;MinP" +
    "oolSize=1;MaxPoolSize=80;Connection Lifetime=600;Packet Size=2048;CharSet=";
            this.dbConnMaster.ConnectionType = Shared.DatabaseConnectionType.Firebird;
            this.dbConnMaster.HintControl = null;
            this.dbConnMaster.LocalHostOnly = false;
            this.dbConnMaster.Location = new System.Drawing.Point(7, 7);
            this.dbConnMaster.Name = "dbConnMaster";
            this.dbConnMaster.Size = new System.Drawing.Size(279, 267);
            this.dbConnMaster.TabIndex = 0;
            this.dbConnMaster.TestConnectionSuccess += new System.EventHandler(this.dbConnChild_TestConnectionSuccess);
            // 
            // tabPageBackupSettings
            // 
            this.tabPageBackupSettings.Controls.Add(this.dtpBackupTime);
            this.tabPageBackupSettings.Controls.Add(this.cbBackupAfter);
            this.tabPageBackupSettings.Controls.Add(this.lblBackupMaxAgeDays);
            this.tabPageBackupSettings.Controls.Add(this.udBackupMaxAge);
            this.tabPageBackupSettings.Controls.Add(this.lblMaxAge);
            this.tabPageBackupSettings.Controls.Add(this.cbBackupsDeleteOldFiles);
            this.tabPageBackupSettings.Controls.Add(this.txtBackupName);
            this.tabPageBackupSettings.Controls.Add(this.lblBackupName);
            this.tabPageBackupSettings.Controls.Add(this.gbBackupFTPDetails);
            this.tabPageBackupSettings.Controls.Add(this.cbBackupCopyToRemote);
            this.tabPageBackupSettings.Controls.Add(this.cbBackupFileCompress);
            this.tabPageBackupSettings.Controls.Add(this.cbBackupUseSiteID);
            this.tabPageBackupSettings.Controls.Add(this.txtBackupPath);
            this.tabPageBackupSettings.Controls.Add(this.lblBackupPath);
            this.tabPageBackupSettings.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPageBackupSettings.Location = new System.Drawing.Point(4, 40);
            this.tabPageBackupSettings.Name = "tabPageBackupSettings";
            this.tabPageBackupSettings.Size = new System.Drawing.Size(670, 302);
            this.tabPageBackupSettings.TabIndex = 8;
            this.tabPageBackupSettings.Text = "Backup Settings";
            this.tabPageBackupSettings.UseVisualStyleBackColor = true;
            // 
            // dtpBackupTime
            // 
            this.dtpBackupTime.CustomFormat = "HH:mm";
            this.dtpBackupTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpBackupTime.Location = new System.Drawing.Point(33, 238);
            this.dtpBackupTime.Name = "dtpBackupTime";
            this.dtpBackupTime.ShowUpDown = true;
            this.dtpBackupTime.Size = new System.Drawing.Size(72, 20);
            this.dtpBackupTime.TabIndex = 11;
            this.dtpBackupTime.Value = new System.DateTime(2017, 2, 16, 21, 0, 0, 0);
            // 
            // cbBackupAfter
            // 
            this.cbBackupAfter.AutoSize = true;
            this.cbBackupAfter.Location = new System.Drawing.Point(12, 214);
            this.cbBackupAfter.Name = "cbBackupAfter";
            this.cbBackupAfter.Size = new System.Drawing.Size(156, 17);
            this.cbBackupAfter.TabIndex = 10;
            this.cbBackupAfter.Text = "Only complete backup after";
            this.cbBackupAfter.UseVisualStyleBackColor = true;
            this.cbBackupAfter.CheckedChanged += new System.EventHandler(this.cbBackupAfter_CheckedChanged);
            // 
            // lblBackupMaxAgeDays
            // 
            this.lblBackupMaxAgeDays.AutoSize = true;
            this.lblBackupMaxAgeDays.Location = new System.Drawing.Point(149, 179);
            this.lblBackupMaxAgeDays.Name = "lblBackupMaxAgeDays";
            this.lblBackupMaxAgeDays.Size = new System.Drawing.Size(31, 13);
            this.lblBackupMaxAgeDays.TabIndex = 9;
            this.lblBackupMaxAgeDays.Text = "Days";
            // 
            // udBackupMaxAge
            // 
            this.udBackupMaxAge.Location = new System.Drawing.Point(88, 177);
            this.udBackupMaxAge.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udBackupMaxAge.Name = "udBackupMaxAge";
            this.udBackupMaxAge.Size = new System.Drawing.Size(55, 20);
            this.udBackupMaxAge.TabIndex = 8;
            this.udBackupMaxAge.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblMaxAge
            // 
            this.lblMaxAge.AutoSize = true;
            this.lblMaxAge.Location = new System.Drawing.Point(33, 179);
            this.lblMaxAge.Name = "lblMaxAge";
            this.lblMaxAge.Size = new System.Drawing.Size(49, 13);
            this.lblMaxAge.TabIndex = 7;
            this.lblMaxAge.Text = "Max Age";
            // 
            // cbBackupsDeleteOldFiles
            // 
            this.cbBackupsDeleteOldFiles.AutoSize = true;
            this.cbBackupsDeleteOldFiles.Location = new System.Drawing.Point(12, 151);
            this.cbBackupsDeleteOldFiles.Name = "cbBackupsDeleteOldFiles";
            this.cbBackupsDeleteOldFiles.Size = new System.Drawing.Size(121, 17);
            this.cbBackupsDeleteOldFiles.TabIndex = 6;
            this.cbBackupsDeleteOldFiles.Text = "Delete Old Backups";
            this.cbBackupsDeleteOldFiles.UseVisualStyleBackColor = true;
            // 
            // txtBackupName
            // 
            this.txtBackupName.Location = new System.Drawing.Point(111, 71);
            this.txtBackupName.MaxLength = 50;
            this.txtBackupName.Name = "txtBackupName";
            this.txtBackupName.Size = new System.Drawing.Size(159, 20);
            this.txtBackupName.TabIndex = 4;
            // 
            // lblBackupName
            // 
            this.lblBackupName.AutoSize = true;
            this.lblBackupName.Location = new System.Drawing.Point(30, 74);
            this.lblBackupName.Name = "lblBackupName";
            this.lblBackupName.Size = new System.Drawing.Size(75, 13);
            this.lblBackupName.TabIndex = 3;
            this.lblBackupName.Text = "Backup Name";
            // 
            // gbBackupFTPDetails
            // 
            this.gbBackupFTPDetails.Controls.Add(this.udBackupFTPPort);
            this.gbBackupFTPDetails.Controls.Add(this.btnBackupFTPTest);
            this.gbBackupFTPDetails.Controls.Add(this.txtBackupFTPPassword);
            this.gbBackupFTPDetails.Controls.Add(this.txtBackupFTPUsername);
            this.gbBackupFTPDetails.Controls.Add(this.txtBackupFTPHost);
            this.gbBackupFTPDetails.Controls.Add(this.lblBackupFTPPort);
            this.gbBackupFTPDetails.Controls.Add(this.lblBackupFTPPassword);
            this.gbBackupFTPDetails.Controls.Add(this.lblBackupFTPUsername);
            this.gbBackupFTPDetails.Controls.Add(this.lblBackupFTPHost);
            this.gbBackupFTPDetails.Enabled = false;
            this.gbBackupFTPDetails.Location = new System.Drawing.Point(323, 71);
            this.gbBackupFTPDetails.Name = "gbBackupFTPDetails";
            this.gbBackupFTPDetails.Size = new System.Drawing.Size(266, 149);
            this.gbBackupFTPDetails.TabIndex = 13;
            this.gbBackupFTPDetails.TabStop = false;
            this.gbBackupFTPDetails.Text = "FTP Details";
            // 
            // udBackupFTPPort
            // 
            this.udBackupFTPPort.Location = new System.Drawing.Point(83, 114);
            this.udBackupFTPPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.udBackupFTPPort.Name = "udBackupFTPPort";
            this.udBackupFTPPort.Size = new System.Drawing.Size(96, 20);
            this.udBackupFTPPort.TabIndex = 7;
            this.udBackupFTPPort.Value = new decimal(new int[] {
            21,
            0,
            0,
            0});
            // 
            // btnBackupFTPTest
            // 
            this.btnBackupFTPTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBackupFTPTest.Location = new System.Drawing.Point(185, 112);
            this.btnBackupFTPTest.Name = "btnBackupFTPTest";
            this.btnBackupFTPTest.Size = new System.Drawing.Size(75, 23);
            this.btnBackupFTPTest.TabIndex = 8;
            this.btnBackupFTPTest.Text = "Test";
            this.btnBackupFTPTest.UseVisualStyleBackColor = true;
            this.btnBackupFTPTest.Click += new System.EventHandler(this.btnBackupFTPTest_Click);
            // 
            // txtBackupFTPPassword
            // 
            this.txtBackupFTPPassword.Location = new System.Drawing.Point(83, 82);
            this.txtBackupFTPPassword.MaxLength = 100;
            this.txtBackupFTPPassword.Name = "txtBackupFTPPassword";
            this.txtBackupFTPPassword.PasswordChar = '*';
            this.txtBackupFTPPassword.Size = new System.Drawing.Size(175, 20);
            this.txtBackupFTPPassword.TabIndex = 5;
            // 
            // txtBackupFTPUsername
            // 
            this.txtBackupFTPUsername.Location = new System.Drawing.Point(83, 51);
            this.txtBackupFTPUsername.MaxLength = 100;
            this.txtBackupFTPUsername.Name = "txtBackupFTPUsername";
            this.txtBackupFTPUsername.Size = new System.Drawing.Size(175, 20);
            this.txtBackupFTPUsername.TabIndex = 3;
            // 
            // txtBackupFTPHost
            // 
            this.txtBackupFTPHost.Location = new System.Drawing.Point(83, 21);
            this.txtBackupFTPHost.MaxLength = 100;
            this.txtBackupFTPHost.Name = "txtBackupFTPHost";
            this.txtBackupFTPHost.Size = new System.Drawing.Size(175, 20);
            this.txtBackupFTPHost.TabIndex = 1;
            // 
            // lblBackupFTPPort
            // 
            this.lblBackupFTPPort.AutoSize = true;
            this.lblBackupFTPPort.Location = new System.Drawing.Point(10, 116);
            this.lblBackupFTPPort.Name = "lblBackupFTPPort";
            this.lblBackupFTPPort.Size = new System.Drawing.Size(26, 13);
            this.lblBackupFTPPort.TabIndex = 6;
            this.lblBackupFTPPort.Text = "Port";
            // 
            // lblBackupFTPPassword
            // 
            this.lblBackupFTPPassword.AutoSize = true;
            this.lblBackupFTPPassword.Location = new System.Drawing.Point(10, 85);
            this.lblBackupFTPPassword.Name = "lblBackupFTPPassword";
            this.lblBackupFTPPassword.Size = new System.Drawing.Size(53, 13);
            this.lblBackupFTPPassword.TabIndex = 4;
            this.lblBackupFTPPassword.Text = "Password";
            // 
            // lblBackupFTPUsername
            // 
            this.lblBackupFTPUsername.AutoSize = true;
            this.lblBackupFTPUsername.Location = new System.Drawing.Point(10, 54);
            this.lblBackupFTPUsername.Name = "lblBackupFTPUsername";
            this.lblBackupFTPUsername.Size = new System.Drawing.Size(55, 13);
            this.lblBackupFTPUsername.TabIndex = 2;
            this.lblBackupFTPUsername.Text = "Username";
            // 
            // lblBackupFTPHost
            // 
            this.lblBackupFTPHost.AutoSize = true;
            this.lblBackupFTPHost.Location = new System.Drawing.Point(10, 24);
            this.lblBackupFTPHost.Name = "lblBackupFTPHost";
            this.lblBackupFTPHost.Size = new System.Drawing.Size(29, 13);
            this.lblBackupFTPHost.TabIndex = 0;
            this.lblBackupFTPHost.Text = "Host";
            // 
            // cbBackupCopyToRemote
            // 
            this.cbBackupCopyToRemote.AutoSize = true;
            this.cbBackupCopyToRemote.Location = new System.Drawing.Point(309, 48);
            this.cbBackupCopyToRemote.Name = "cbBackupCopyToRemote";
            this.cbBackupCopyToRemote.Size = new System.Drawing.Size(146, 17);
            this.cbBackupCopyToRemote.TabIndex = 12;
            this.cbBackupCopyToRemote.Text = "Copy to Remote Location";
            this.cbBackupCopyToRemote.UseVisualStyleBackColor = true;
            this.cbBackupCopyToRemote.CheckedChanged += new System.EventHandler(this.cbBackupCopyToRemote_CheckedChanged);
            // 
            // cbBackupFileCompress
            // 
            this.cbBackupFileCompress.AutoSize = true;
            this.cbBackupFileCompress.Location = new System.Drawing.Point(12, 112);
            this.cbBackupFileCompress.Name = "cbBackupFileCompress";
            this.cbBackupFileCompress.Size = new System.Drawing.Size(131, 17);
            this.cbBackupFileCompress.TabIndex = 5;
            this.cbBackupFileCompress.Text = "Compress Backup File";
            this.cbBackupFileCompress.UseVisualStyleBackColor = true;
            // 
            // cbBackupUseSiteID
            // 
            this.cbBackupUseSiteID.AutoSize = true;
            this.cbBackupUseSiteID.Location = new System.Drawing.Point(12, 48);
            this.cbBackupUseSiteID.Name = "cbBackupUseSiteID";
            this.cbBackupUseSiteID.Size = new System.Drawing.Size(80, 17);
            this.cbBackupUseSiteID.TabIndex = 2;
            this.cbBackupUseSiteID.Text = "Use Site ID";
            this.cbBackupUseSiteID.UseVisualStyleBackColor = true;
            this.cbBackupUseSiteID.CheckedChanged += new System.EventHandler(this.cbBackupUseSiteID_CheckedChanged);
            // 
            // txtBackupPath
            // 
            this.txtBackupPath.Location = new System.Drawing.Point(84, 10);
            this.txtBackupPath.Name = "txtBackupPath";
            this.txtBackupPath.Size = new System.Drawing.Size(467, 20);
            this.txtBackupPath.TabIndex = 1;
            // 
            // lblBackupPath
            // 
            this.lblBackupPath.AutoSize = true;
            this.lblBackupPath.Location = new System.Drawing.Point(9, 13);
            this.lblBackupPath.Name = "lblBackupPath";
            this.lblBackupPath.Size = new System.Drawing.Size(69, 13);
            this.lblBackupPath.TabIndex = 0;
            this.lblBackupPath.Text = "Backup Path";
            // 
            // tabPageRemoteUpdateSettings
            // 
            this.tabPageRemoteUpdateSettings.Controls.Add(this.txtRemoteUpdateLocation);
            this.tabPageRemoteUpdateSettings.Controls.Add(this.lblRemoteUpdateLocation);
            this.tabPageRemoteUpdateSettings.Controls.Add(this.txtRemoteUpdateXMLFile);
            this.tabPageRemoteUpdateSettings.Controls.Add(this.lblRemoteUpdateVersion);
            this.tabPageRemoteUpdateSettings.Location = new System.Drawing.Point(4, 40);
            this.tabPageRemoteUpdateSettings.Name = "tabPageRemoteUpdateSettings";
            this.tabPageRemoteUpdateSettings.Size = new System.Drawing.Size(670, 302);
            this.tabPageRemoteUpdateSettings.TabIndex = 9;
            this.tabPageRemoteUpdateSettings.Text = "Remote Update Settings";
            this.tabPageRemoteUpdateSettings.UseVisualStyleBackColor = true;
            // 
            // txtRemoteUpdateLocation
            // 
            this.txtRemoteUpdateLocation.Location = new System.Drawing.Point(146, 41);
            this.txtRemoteUpdateLocation.MaxLength = 300;
            this.txtRemoteUpdateLocation.Name = "txtRemoteUpdateLocation";
            this.txtRemoteUpdateLocation.Size = new System.Drawing.Size(510, 20);
            this.txtRemoteUpdateLocation.TabIndex = 3;
            // 
            // lblRemoteUpdateLocation
            // 
            this.lblRemoteUpdateLocation.AutoSize = true;
            this.lblRemoteUpdateLocation.Location = new System.Drawing.Point(14, 44);
            this.lblRemoteUpdateLocation.Name = "lblRemoteUpdateLocation";
            this.lblRemoteUpdateLocation.Size = new System.Drawing.Size(126, 13);
            this.lblRemoteUpdateLocation.TabIndex = 2;
            this.lblRemoteUpdateLocation.Text = "Remote Update Location";
            // 
            // txtRemoteUpdateXMLFile
            // 
            this.txtRemoteUpdateXMLFile.Location = new System.Drawing.Point(146, 12);
            this.txtRemoteUpdateXMLFile.MaxLength = 300;
            this.txtRemoteUpdateXMLFile.Name = "txtRemoteUpdateXMLFile";
            this.txtRemoteUpdateXMLFile.Size = new System.Drawing.Size(510, 20);
            this.txtRemoteUpdateXMLFile.TabIndex = 1;
            // 
            // lblRemoteUpdateVersion
            // 
            this.lblRemoteUpdateVersion.AutoSize = true;
            this.lblRemoteUpdateVersion.Location = new System.Drawing.Point(14, 15);
            this.lblRemoteUpdateVersion.Name = "lblRemoteUpdateVersion";
            this.lblRemoteUpdateVersion.Size = new System.Drawing.Size(126, 13);
            this.lblRemoteUpdateVersion.TabIndex = 0;
            this.lblRemoteUpdateVersion.Text = "Remote Update XML File";
            // 
            // tabPageReplicationSettings
            // 
            this.tabPageReplicationSettings.Controls.Add(this.cbForceVerification);
            this.tabPageReplicationSettings.Controls.Add(this.dtpVerifyAfter);
            this.tabPageReplicationSettings.Controls.Add(this.cbRequireUniqueAcess);
            this.tabPageReplicationSettings.Controls.Add(this.cbReplicationAutoUpdateTriggers);
            this.tabPageReplicationSettings.Controls.Add(this.lblVerifyAfterHours);
            this.tabPageReplicationSettings.Controls.Add(this.cbForceHours);
            this.tabPageReplicationSettings.Controls.Add(this.label14);
            this.tabPageReplicationSettings.Controls.Add(this.udTimeoutMinutes);
            this.tabPageReplicationSettings.Controls.Add(this.label13);
            this.tabPageReplicationSettings.Controls.Add(this.label10);
            this.tabPageReplicationSettings.Controls.Add(this.udUploadCount);
            this.tabPageReplicationSettings.Controls.Add(this.label9);
            this.tabPageReplicationSettings.Controls.Add(this.label8);
            this.tabPageReplicationSettings.Controls.Add(this.udDownloadCount);
            this.tabPageReplicationSettings.Controls.Add(this.label7);
            this.tabPageReplicationSettings.Controls.Add(this.label6);
            this.tabPageReplicationSettings.Controls.Add(this.udResetErrorCount);
            this.tabPageReplicationSettings.Controls.Add(this.label5);
            this.tabPageReplicationSettings.Controls.Add(this.label4);
            this.tabPageReplicationSettings.Controls.Add(this.udReplicateMinutes);
            this.tabPageReplicationSettings.Controls.Add(this.label3);
            this.tabPageReplicationSettings.Controls.Add(this.lblVerifyAfterIterationsIt);
            this.tabPageReplicationSettings.Controls.Add(this.udVerifyAll);
            this.tabPageReplicationSettings.Controls.Add(this.lblVerifyAfterIterations);
            this.tabPageReplicationSettings.Location = new System.Drawing.Point(4, 40);
            this.tabPageReplicationSettings.Name = "tabPageReplicationSettings";
            this.tabPageReplicationSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageReplicationSettings.Size = new System.Drawing.Size(670, 302);
            this.tabPageReplicationSettings.TabIndex = 1;
            this.tabPageReplicationSettings.Text = "Replication Settings";
            this.tabPageReplicationSettings.UseVisualStyleBackColor = true;
            // 
            // cbForceVerification
            // 
            this.cbForceVerification.AutoSize = true;
            this.cbForceVerification.Location = new System.Drawing.Point(301, 8);
            this.cbForceVerification.Name = "cbForceVerification";
            this.cbForceVerification.Size = new System.Drawing.Size(108, 17);
            this.cbForceVerification.TabIndex = 13;
            this.cbForceVerification.Text = "Force Verification";
            this.cbForceVerification.UseVisualStyleBackColor = true;
            this.cbForceVerification.CheckedChanged += new System.EventHandler(this.cbForceVerification_CheckedChanged);
            // 
            // dtpVerifyAfter
            // 
            this.dtpVerifyAfter.CustomFormat = "HH:mm";
            this.dtpVerifyAfter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpVerifyAfter.Location = new System.Drawing.Point(330, 63);
            this.dtpVerifyAfter.Name = "dtpVerifyAfter";
            this.dtpVerifyAfter.ShowUpDown = true;
            this.dtpVerifyAfter.Size = new System.Drawing.Size(72, 20);
            this.dtpVerifyAfter.TabIndex = 15;
            this.dtpVerifyAfter.Value = new System.DateTime(2017, 2, 16, 21, 0, 0, 0);
            // 
            // cbRequireUniqueAcess
            // 
            this.cbRequireUniqueAcess.AutoSize = true;
            this.cbRequireUniqueAcess.Location = new System.Drawing.Point(330, 126);
            this.cbRequireUniqueAcess.Name = "cbRequireUniqueAcess";
            this.cbRequireUniqueAcess.Size = new System.Drawing.Size(224, 17);
            this.cbRequireUniqueAcess.TabIndex = 20;
            this.cbRequireUniqueAcess.Text = "Require uniqe access when verifying data";
            this.cbRequireUniqueAcess.UseVisualStyleBackColor = true;
            // 
            // cbReplicationAutoUpdateTriggers
            // 
            this.cbReplicationAutoUpdateTriggers.AutoSize = true;
            this.cbReplicationAutoUpdateTriggers.Location = new System.Drawing.Point(9, 139);
            this.cbReplicationAutoUpdateTriggers.Name = "cbReplicationAutoUpdateTriggers";
            this.cbReplicationAutoUpdateTriggers.Size = new System.Drawing.Size(223, 17);
            this.cbReplicationAutoUpdateTriggers.TabIndex = 12;
            this.cbReplicationAutoUpdateTriggers.Text = "Automatically Update Replication Triggers";
            this.cbReplicationAutoUpdateTriggers.UseVisualStyleBackColor = true;
            // 
            // lblVerifyAfterHours
            // 
            this.lblVerifyAfterHours.AutoSize = true;
            this.lblVerifyAfterHours.Location = new System.Drawing.Point(408, 67);
            this.lblVerifyAfterHours.Name = "lblVerifyAfterHours";
            this.lblVerifyAfterHours.Size = new System.Drawing.Size(33, 13);
            this.lblVerifyAfterHours.TabIndex = 16;
            this.lblVerifyAfterHours.Text = "hours";
            // 
            // cbForceHours
            // 
            this.cbForceHours.AutoSize = true;
            this.cbForceHours.Location = new System.Drawing.Point(330, 38);
            this.cbForceHours.Name = "cbForceHours";
            this.cbForceHours.Size = new System.Drawing.Size(149, 17);
            this.cbForceHours.TabIndex = 14;
            this.cbForceHours.Text = "Force verify all tables after";
            this.cbForceHours.UseVisualStyleBackColor = true;
            this.cbForceHours.CheckedChanged += new System.EventHandler(this.cbForceHours_CheckedChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(210, 39);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(44, 13);
            this.label14.TabIndex = 5;
            this.label14.Text = "Minutes";
            // 
            // udTimeoutMinutes
            // 
            this.udTimeoutMinutes.Location = new System.Drawing.Point(134, 37);
            this.udTimeoutMinutes.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.udTimeoutMinutes.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.udTimeoutMinutes.Name = "udTimeoutMinutes";
            this.udTimeoutMinutes.Size = new System.Drawing.Size(70, 20);
            this.udTimeoutMinutes.TabIndex = 4;
            this.udTimeoutMinutes.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 39);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(125, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Timeout and restart after ";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(210, 104);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "Records";
            // 
            // udUploadCount
            // 
            this.udUploadCount.Location = new System.Drawing.Point(134, 102);
            this.udUploadCount.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.udUploadCount.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udUploadCount.Name = "udUploadCount";
            this.udUploadCount.Size = new System.Drawing.Size(70, 20);
            this.udUploadCount.TabIndex = 10;
            this.udUploadCount.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 104);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(111, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "Upload a maximum of ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(210, 69);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Records";
            // 
            // udDownloadCount
            // 
            this.udDownloadCount.Location = new System.Drawing.Point(134, 67);
            this.udDownloadCount.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.udDownloadCount.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udDownloadCount.Name = "udDownloadCount";
            this.udDownloadCount.Size = new System.Drawing.Size(70, 20);
            this.udDownloadCount.TabIndex = 7;
            this.udDownloadCount.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 69);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(125, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "Download a maximum of ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(480, 168);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 23;
            this.label6.Text = "Errors";
            // 
            // udResetErrorCount
            // 
            this.udResetErrorCount.Location = new System.Drawing.Point(426, 166);
            this.udResetErrorCount.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.udResetErrorCount.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.udResetErrorCount.Name = "udResetErrorCount";
            this.udResetErrorCount.Size = new System.Drawing.Size(48, 20);
            this.udResetErrorCount.TabIndex = 22;
            this.udResetErrorCount.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(298, 168);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Force reset after ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(210, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Minutes";
            // 
            // udReplicateMinutes
            // 
            this.udReplicateMinutes.Location = new System.Drawing.Point(134, 6);
            this.udReplicateMinutes.Maximum = new decimal(new int[] {
            520,
            0,
            0,
            0});
            this.udReplicateMinutes.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udReplicateMinutes.Name = "udReplicateMinutes";
            this.udReplicateMinutes.Size = new System.Drawing.Size(70, 20);
            this.udReplicateMinutes.TabIndex = 1;
            this.udReplicateMinutes.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Replicate data every ";
            // 
            // lblVerifyAfterIterationsIt
            // 
            this.lblVerifyAfterIterationsIt.AutoSize = true;
            this.lblVerifyAfterIterationsIt.Location = new System.Drawing.Point(518, 102);
            this.lblVerifyAfterIterationsIt.Name = "lblVerifyAfterIterationsIt";
            this.lblVerifyAfterIterationsIt.Size = new System.Drawing.Size(50, 13);
            this.lblVerifyAfterIterationsIt.TabIndex = 19;
            this.lblVerifyAfterIterationsIt.Text = "Iterations";
            // 
            // udVerifyAll
            // 
            this.udVerifyAll.Location = new System.Drawing.Point(442, 100);
            this.udVerifyAll.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udVerifyAll.Name = "udVerifyAll";
            this.udVerifyAll.Size = new System.Drawing.Size(70, 20);
            this.udVerifyAll.TabIndex = 18;
            // 
            // lblVerifyAfterIterations
            // 
            this.lblVerifyAfterIterations.AutoSize = true;
            this.lblVerifyAfterIterations.Location = new System.Drawing.Point(327, 102);
            this.lblVerifyAfterIterations.Name = "lblVerifyAfterIterations";
            this.lblVerifyAfterIterations.Size = new System.Drawing.Size(109, 13);
            this.lblVerifyAfterIterations.TabIndex = 17;
            this.lblVerifyAfterIterations.Text = "Verify all tables every ";
            // 
            // tabPageTables
            // 
            this.tabPageTables.Controls.Add(this.lvTables);
            this.tabPageTables.Location = new System.Drawing.Point(4, 40);
            this.tabPageTables.Name = "tabPageTables";
            this.tabPageTables.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTables.Size = new System.Drawing.Size(670, 302);
            this.tabPageTables.TabIndex = 4;
            this.tabPageTables.Text = "Tables To Replicate";
            this.tabPageTables.UseVisualStyleBackColor = true;
            // 
            // lvTables
            // 
            this.lvTables.AllowColumnReorder = true;
            this.lvTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTablesName,
            this.colTableChildReplicated,
            this.colTableMasterReplicating,
            this.colTableSortOrder});
            this.lvTables.ContextMenuStrip = this.pumTables;
            this.lvTables.FullRowSelect = true;
            this.lvTables.Location = new System.Drawing.Point(6, 6);
            this.lvTables.MinimumSize = new System.Drawing.Size(658, 290);
            this.lvTables.Name = "lvTables";
            this.lvTables.OwnerDraw = true;
            this.lvTables.SaveName = "";
            this.lvTables.ShowToolTip = true;
            this.lvTables.Size = new System.Drawing.Size(658, 290);
            this.lvTables.TabIndex = 0;
            this.lvTables.UseCompatibleStateImageBehavior = false;
            this.lvTables.View = System.Windows.Forms.View.Details;
            this.lvTables.ToolTipShow += new Shared.ToolTipEventHandler(this.lvTables_ToolTipShow);
            this.lvTables.DoubleClick += new System.EventHandler(this.lvTables_DoubleClick);
            // 
            // colTablesName
            // 
            this.colTablesName.Text = "Table Name";
            this.colTablesName.Width = 255;
            // 
            // colTableChildReplicated
            // 
            this.colTableChildReplicated.Text = "Child Replicating";
            this.colTableChildReplicated.Width = 100;
            // 
            // colTableMasterReplicating
            // 
            this.colTableMasterReplicating.Text = "Master Replicating";
            this.colTableMasterReplicating.Width = 100;
            // 
            // colTableSortOrder
            // 
            this.colTableSortOrder.Text = "Sort Order";
            this.colTableSortOrder.Width = 90;
            // 
            // pumTables
            // 
            this.pumTables.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pumTablesAdd,
            this.pumTablesRemove,
            this.toolStripMenuItem1,
            this.pumTablesConfigure,
            this.toolStripMenuItem2,
            this.pumTablesValidateAll,
            this.toolStripMenuItem3,
            this.pumTablesUpdateSortOrders,
            this.toolStripMenuItem4,
            this.pumTablesRefresh});
            this.pumTables.Name = "pumTables";
            this.pumTables.Size = new System.Drawing.Size(175, 160);
            this.pumTables.Opening += new System.ComponentModel.CancelEventHandler(this.pumTables_Opening);
            // 
            // pumTablesAdd
            // 
            this.pumTablesAdd.Name = "pumTablesAdd";
            this.pumTablesAdd.Size = new System.Drawing.Size(174, 22);
            this.pumTablesAdd.Text = "Add";
            this.pumTablesAdd.Click += new System.EventHandler(this.pumTablesAdd_Click);
            // 
            // pumTablesRemove
            // 
            this.pumTablesRemove.Name = "pumTablesRemove";
            this.pumTablesRemove.Size = new System.Drawing.Size(174, 22);
            this.pumTablesRemove.Text = "Remove";
            this.pumTablesRemove.Click += new System.EventHandler(this.pumTablesRemove_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(171, 6);
            // 
            // pumTablesConfigure
            // 
            this.pumTablesConfigure.Name = "pumTablesConfigure";
            this.pumTablesConfigure.Size = new System.Drawing.Size(174, 22);
            this.pumTablesConfigure.Text = "Configure";
            this.pumTablesConfigure.Click += new System.EventHandler(this.pumTablesConfigure_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(171, 6);
            // 
            // pumTablesValidateAll
            // 
            this.pumTablesValidateAll.Name = "pumTablesValidateAll";
            this.pumTablesValidateAll.Size = new System.Drawing.Size(174, 22);
            this.pumTablesValidateAll.Text = "Validate All Tables";
            this.pumTablesValidateAll.Click += new System.EventHandler(this.pumTablesValidateAll_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(171, 6);
            // 
            // pumTablesUpdateSortOrders
            // 
            this.pumTablesUpdateSortOrders.Name = "pumTablesUpdateSortOrders";
            this.pumTablesUpdateSortOrders.Size = new System.Drawing.Size(174, 22);
            this.pumTablesUpdateSortOrders.Text = "Update Sort Orders";
            this.pumTablesUpdateSortOrders.Click += new System.EventHandler(this.pumTablesUpdateSortOrders_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(171, 6);
            // 
            // pumTablesRefresh
            // 
            this.pumTablesRefresh.Name = "pumTablesRefresh";
            this.pumTablesRefresh.Size = new System.Drawing.Size(174, 22);
            this.pumTablesRefresh.Text = "Refresh";
            this.pumTablesRefresh.Click += new System.EventHandler(this.pumTablesRefresh_Click);
            // 
            // tabPageAutoCorrect
            // 
            this.tabPageAutoCorrect.Controls.Add(this.lvAutoCorrect);
            this.tabPageAutoCorrect.Location = new System.Drawing.Point(4, 40);
            this.tabPageAutoCorrect.Name = "tabPageAutoCorrect";
            this.tabPageAutoCorrect.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAutoCorrect.Size = new System.Drawing.Size(670, 302);
            this.tabPageAutoCorrect.TabIndex = 5;
            this.tabPageAutoCorrect.Text = "Auto Correct Rules";
            this.tabPageAutoCorrect.UseVisualStyleBackColor = true;
            // 
            // lvAutoCorrect
            // 
            this.lvAutoCorrect.AllowColumnReorder = true;
            this.lvAutoCorrect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvAutoCorrect.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colAutoCorrectTable,
            this.colAutoCorrectKeyName,
            this.colAutoCorrectOptions});
            this.lvAutoCorrect.ContextMenuStrip = this.pumAutoCorrect;
            this.lvAutoCorrect.FullRowSelect = true;
            this.lvAutoCorrect.HideSelection = false;
            this.lvAutoCorrect.Location = new System.Drawing.Point(7, 7);
            this.lvAutoCorrect.MinimumSize = new System.Drawing.Size(657, 289);
            this.lvAutoCorrect.MultiSelect = false;
            this.lvAutoCorrect.Name = "lvAutoCorrect";
            this.lvAutoCorrect.OwnerDraw = true;
            this.lvAutoCorrect.SaveName = "colAutoCorrect";
            this.lvAutoCorrect.ShowToolTip = false;
            this.lvAutoCorrect.Size = new System.Drawing.Size(657, 289);
            this.lvAutoCorrect.TabIndex = 0;
            this.lvAutoCorrect.UseCompatibleStateImageBehavior = false;
            this.lvAutoCorrect.View = System.Windows.Forms.View.Details;
            this.lvAutoCorrect.DoubleClick += new System.EventHandler(this.lvAutoCorrect_DoubleClick);
            // 
            // colAutoCorrectTable
            // 
            this.colAutoCorrectTable.Text = "Table Name";
            this.colAutoCorrectTable.Width = 150;
            // 
            // colAutoCorrectKeyName
            // 
            this.colAutoCorrectKeyName.Text = "Key Name";
            this.colAutoCorrectKeyName.Width = 150;
            // 
            // colAutoCorrectOptions
            // 
            this.colAutoCorrectOptions.Text = "Options";
            this.colAutoCorrectOptions.Width = 300;
            // 
            // pumAutoCorrect
            // 
            this.pumAutoCorrect.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pumAutoCorrectAdd,
            this.pumAutoCorrectDelete,
            this.pumAutoCorrectEdit,
            this.toolStripMenuItem5,
            this.pumAutoCorrectRefresh});
            this.pumAutoCorrect.Name = "pumAutoCorrect";
            this.pumAutoCorrect.Size = new System.Drawing.Size(114, 98);
            this.pumAutoCorrect.Opening += new System.ComponentModel.CancelEventHandler(this.pumAutoCorrect_Opening);
            // 
            // pumAutoCorrectAdd
            // 
            this.pumAutoCorrectAdd.Name = "pumAutoCorrectAdd";
            this.pumAutoCorrectAdd.Size = new System.Drawing.Size(113, 22);
            this.pumAutoCorrectAdd.Text = "Add";
            this.pumAutoCorrectAdd.Click += new System.EventHandler(this.pumAutoCorrectAdd_Click);
            // 
            // pumAutoCorrectDelete
            // 
            this.pumAutoCorrectDelete.Name = "pumAutoCorrectDelete";
            this.pumAutoCorrectDelete.Size = new System.Drawing.Size(113, 22);
            this.pumAutoCorrectDelete.Text = "Delete";
            this.pumAutoCorrectDelete.Click += new System.EventHandler(this.pumAutoCorrectDelete_Click);
            // 
            // pumAutoCorrectEdit
            // 
            this.pumAutoCorrectEdit.Name = "pumAutoCorrectEdit";
            this.pumAutoCorrectEdit.Size = new System.Drawing.Size(113, 22);
            this.pumAutoCorrectEdit.Text = "Edit";
            this.pumAutoCorrectEdit.Click += new System.EventHandler(this.lvAutoCorrect_DoubleClick);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(110, 6);
            // 
            // pumAutoCorrectRefresh
            // 
            this.pumAutoCorrectRefresh.Name = "pumAutoCorrectRefresh";
            this.pumAutoCorrectRefresh.Size = new System.Drawing.Size(113, 22);
            this.pumAutoCorrectRefresh.Text = "Refresh";
            this.pumAutoCorrectRefresh.Click += new System.EventHandler(this.pumAutoCorrectRefresh_Click);
            // 
            // tabPageChildReplicationSchema
            // 
            this.tabPageChildReplicationSchema.Controls.Add(this.btnChildScriptExecute);
            this.tabPageChildReplicationSchema.Controls.Add(this.btnChildCreateScript);
            this.tabPageChildReplicationSchema.Controls.Add(this.txtChildCreateScript);
            this.tabPageChildReplicationSchema.Location = new System.Drawing.Point(4, 40);
            this.tabPageChildReplicationSchema.Name = "tabPageChildReplicationSchema";
            this.tabPageChildReplicationSchema.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageChildReplicationSchema.Size = new System.Drawing.Size(670, 302);
            this.tabPageChildReplicationSchema.TabIndex = 6;
            this.tabPageChildReplicationSchema.Text = "Replication Add Schema";
            this.tabPageChildReplicationSchema.UseVisualStyleBackColor = true;
            // 
            // btnChildScriptExecute
            // 
            this.btnChildScriptExecute.Location = new System.Drawing.Point(572, 273);
            this.btnChildScriptExecute.Name = "btnChildScriptExecute";
            this.btnChildScriptExecute.Size = new System.Drawing.Size(92, 23);
            this.btnChildScriptExecute.TabIndex = 2;
            this.btnChildScriptExecute.Text = "Execute Script";
            this.btnChildScriptExecute.UseVisualStyleBackColor = true;
            this.btnChildScriptExecute.Click += new System.EventHandler(this.btnChildScriptExecute_Click);
            // 
            // btnChildCreateScript
            // 
            this.btnChildCreateScript.Location = new System.Drawing.Point(490, 273);
            this.btnChildCreateScript.Name = "btnChildCreateScript";
            this.btnChildCreateScript.Size = new System.Drawing.Size(75, 23);
            this.btnChildCreateScript.TabIndex = 1;
            this.btnChildCreateScript.Text = "Generate";
            this.btnChildCreateScript.UseVisualStyleBackColor = true;
            this.btnChildCreateScript.Click += new System.EventHandler(this.tabPageChildCreateScript_Click);
            // 
            // txtChildCreateScript
            // 
            this.txtChildCreateScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChildCreateScript.HideSelection = false;
            this.txtChildCreateScript.Location = new System.Drawing.Point(7, 7);
            this.txtChildCreateScript.MaxLength = 50032767;
            this.txtChildCreateScript.MinimumSize = new System.Drawing.Size(657, 260);
            this.txtChildCreateScript.Multiline = true;
            this.txtChildCreateScript.Name = "txtChildCreateScript";
            this.txtChildCreateScript.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtChildCreateScript.Size = new System.Drawing.Size(657, 260);
            this.txtChildCreateScript.TabIndex = 0;
            // 
            // tabPageChildDropScript
            // 
            this.tabPageChildDropScript.Controls.Add(this.btnChildExecuteRemoveScript);
            this.tabPageChildDropScript.Controls.Add(this.btnGenerateChildDropScript);
            this.tabPageChildDropScript.Controls.Add(this.txtChildDropScript);
            this.tabPageChildDropScript.Location = new System.Drawing.Point(4, 40);
            this.tabPageChildDropScript.Name = "tabPageChildDropScript";
            this.tabPageChildDropScript.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageChildDropScript.Size = new System.Drawing.Size(670, 302);
            this.tabPageChildDropScript.TabIndex = 10;
            this.tabPageChildDropScript.Text = "Replication Remove Schema";
            this.tabPageChildDropScript.UseVisualStyleBackColor = true;
            // 
            // btnChildExecuteRemoveScript
            // 
            this.btnChildExecuteRemoveScript.Location = new System.Drawing.Point(572, 273);
            this.btnChildExecuteRemoveScript.Name = "btnChildExecuteRemoveScript";
            this.btnChildExecuteRemoveScript.Size = new System.Drawing.Size(92, 23);
            this.btnChildExecuteRemoveScript.TabIndex = 3;
            this.btnChildExecuteRemoveScript.Text = "Execute Script";
            this.btnChildExecuteRemoveScript.UseVisualStyleBackColor = true;
            this.btnChildExecuteRemoveScript.Click += new System.EventHandler(this.btnChildExecuteRemoveScript_Click);
            // 
            // btnGenerateChildDropScript
            // 
            this.btnGenerateChildDropScript.Location = new System.Drawing.Point(490, 273);
            this.btnGenerateChildDropScript.Name = "btnGenerateChildDropScript";
            this.btnGenerateChildDropScript.Size = new System.Drawing.Size(75, 23);
            this.btnGenerateChildDropScript.TabIndex = 1;
            this.btnGenerateChildDropScript.Text = "Generate";
            this.btnGenerateChildDropScript.UseVisualStyleBackColor = true;
            this.btnGenerateChildDropScript.Click += new System.EventHandler(this.btnGenerateDropScript_Click);
            // 
            // txtChildDropScript
            // 
            this.txtChildDropScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChildDropScript.Location = new System.Drawing.Point(7, 7);
            this.txtChildDropScript.MaxLength = 999932767;
            this.txtChildDropScript.MinimumSize = new System.Drawing.Size(657, 260);
            this.txtChildDropScript.Multiline = true;
            this.txtChildDropScript.Name = "txtChildDropScript";
            this.txtChildDropScript.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtChildDropScript.Size = new System.Drawing.Size(657, 260);
            this.txtChildDropScript.TabIndex = 0;
            // 
            // tabPageMasterCreateScript
            // 
            this.tabPageMasterCreateScript.Controls.Add(this.btnMasterCreate);
            this.tabPageMasterCreateScript.Controls.Add(this.btnMasterCreateExecute);
            this.tabPageMasterCreateScript.Controls.Add(this.txtMasterCreate);
            this.tabPageMasterCreateScript.Location = new System.Drawing.Point(4, 40);
            this.tabPageMasterCreateScript.Name = "tabPageMasterCreateScript";
            this.tabPageMasterCreateScript.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMasterCreateScript.Size = new System.Drawing.Size(670, 302);
            this.tabPageMasterCreateScript.TabIndex = 11;
            this.tabPageMasterCreateScript.Text = "Master Create Script";
            this.tabPageMasterCreateScript.UseVisualStyleBackColor = true;
            // 
            // btnMasterCreate
            // 
            this.btnMasterCreate.Location = new System.Drawing.Point(490, 273);
            this.btnMasterCreate.Name = "btnMasterCreate";
            this.btnMasterCreate.Size = new System.Drawing.Size(75, 23);
            this.btnMasterCreate.TabIndex = 5;
            this.btnMasterCreate.Text = "Generate";
            this.btnMasterCreate.UseVisualStyleBackColor = true;
            this.btnMasterCreate.Click += new System.EventHandler(this.btnMasterCreate_Click);
            // 
            // btnMasterCreateExecute
            // 
            this.btnMasterCreateExecute.Location = new System.Drawing.Point(572, 273);
            this.btnMasterCreateExecute.Name = "btnMasterCreateExecute";
            this.btnMasterCreateExecute.Size = new System.Drawing.Size(92, 23);
            this.btnMasterCreateExecute.TabIndex = 4;
            this.btnMasterCreateExecute.Text = "Execute Script";
            this.btnMasterCreateExecute.UseVisualStyleBackColor = true;
            this.btnMasterCreateExecute.Click += new System.EventHandler(this.btnMasterCreateExecute_Click);
            // 
            // txtMasterCreate
            // 
            this.txtMasterCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMasterCreate.Location = new System.Drawing.Point(7, 7);
            this.txtMasterCreate.MaxLength = 999932767;
            this.txtMasterCreate.MinimumSize = new System.Drawing.Size(657, 260);
            this.txtMasterCreate.Multiline = true;
            this.txtMasterCreate.Name = "txtMasterCreate";
            this.txtMasterCreate.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMasterCreate.Size = new System.Drawing.Size(657, 260);
            this.txtMasterCreate.TabIndex = 2;
            // 
            // tabPageMasterDropScript
            // 
            this.tabPageMasterDropScript.Controls.Add(this.btnMasterExecuteDropScript);
            this.tabPageMasterDropScript.Controls.Add(this.btnMasterDrop);
            this.tabPageMasterDropScript.Controls.Add(this.txtMasterDrop);
            this.tabPageMasterDropScript.Location = new System.Drawing.Point(4, 40);
            this.tabPageMasterDropScript.Name = "tabPageMasterDropScript";
            this.tabPageMasterDropScript.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMasterDropScript.Size = new System.Drawing.Size(670, 302);
            this.tabPageMasterDropScript.TabIndex = 12;
            this.tabPageMasterDropScript.Text = "Master Drop Script";
            this.tabPageMasterDropScript.UseVisualStyleBackColor = true;
            // 
            // btnMasterExecuteDropScript
            // 
            this.btnMasterExecuteDropScript.Location = new System.Drawing.Point(572, 273);
            this.btnMasterExecuteDropScript.Name = "btnMasterExecuteDropScript";
            this.btnMasterExecuteDropScript.Size = new System.Drawing.Size(92, 23);
            this.btnMasterExecuteDropScript.TabIndex = 5;
            this.btnMasterExecuteDropScript.Text = "Execute Script";
            this.btnMasterExecuteDropScript.UseVisualStyleBackColor = true;
            this.btnMasterExecuteDropScript.Click += new System.EventHandler(this.btnMasterExecuteDropScript_Click);
            // 
            // btnMasterDrop
            // 
            this.btnMasterDrop.Location = new System.Drawing.Point(490, 273);
            this.btnMasterDrop.Name = "btnMasterDrop";
            this.btnMasterDrop.Size = new System.Drawing.Size(75, 23);
            this.btnMasterDrop.TabIndex = 4;
            this.btnMasterDrop.Text = "Generate";
            this.btnMasterDrop.UseVisualStyleBackColor = true;
            this.btnMasterDrop.Click += new System.EventHandler(this.btnMasterDrop_Click);
            // 
            // txtMasterDrop
            // 
            this.txtMasterDrop.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMasterDrop.Location = new System.Drawing.Point(7, 7);
            this.txtMasterDrop.MaxLength = 999932767;
            this.txtMasterDrop.MinimumSize = new System.Drawing.Size(657, 260);
            this.txtMasterDrop.Multiline = true;
            this.txtMasterDrop.Name = "txtMasterDrop";
            this.txtMasterDrop.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMasterDrop.Size = new System.Drawing.Size(657, 260);
            this.txtMasterDrop.TabIndex = 2;
            // 
            // tabPageGenerators
            // 
            this.tabPageGenerators.Controls.Add(this.btnUpdateGenerators);
            this.tabPageGenerators.Controls.Add(this.gridGenerators);
            this.tabPageGenerators.Location = new System.Drawing.Point(4, 40);
            this.tabPageGenerators.Name = "tabPageGenerators";
            this.tabPageGenerators.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGenerators.Size = new System.Drawing.Size(670, 302);
            this.tabPageGenerators.TabIndex = 13;
            this.tabPageGenerators.Text = "Generators";
            this.tabPageGenerators.UseVisualStyleBackColor = true;
            this.tabPageGenerators.Enter += new System.EventHandler(this.tabPageGenerators_Enter);
            // 
            // btnUpdateGenerators
            // 
            this.btnUpdateGenerators.Location = new System.Drawing.Point(589, 264);
            this.btnUpdateGenerators.Name = "btnUpdateGenerators";
            this.btnUpdateGenerators.Size = new System.Drawing.Size(75, 23);
            this.btnUpdateGenerators.TabIndex = 1;
            this.btnUpdateGenerators.Text = "Update";
            this.btnUpdateGenerators.UseVisualStyleBackColor = true;
            this.btnUpdateGenerators.Click += new System.EventHandler(this.btnUpdateGenerators_Click);
            // 
            // gridGenerators
            // 
            this.gridGenerators.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridGenerators.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridGenerators.Location = new System.Drawing.Point(6, 6);
            this.gridGenerators.MinimumSize = new System.Drawing.Size(658, 252);
            this.gridGenerators.Name = "gridGenerators";
            this.gridGenerators.Size = new System.Drawing.Size(658, 252);
            this.gridGenerators.TabIndex = 0;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(611, 366);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(530, 366);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // textBlockServerSQL
            // 
            this.textBlockServerSQL.StringBlock = resources.GetString("textBlockServerSQL.StringBlock");
            // 
            // textBlockClientSQL
            // 
            this.textBlockClientSQL.StringBlock = resources.GetString("textBlockClientSQL.StringBlock");
            // 
            // textBlockClientRemove
            // 
            this.textBlockClientRemove.StringBlock = resources.GetString("textBlockClientRemove.StringBlock");
            // 
            // textBlockServerRemove
            // 
            this.textBlockServerRemove.StringBlock = resources.GetString("textBlockServerRemove.StringBlock");
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.ApplicationData;
            // 
            // ConfigureChild
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 401);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(718, 440);
            this.Name = "ConfigureChild";
            this.SaveState = true;
            this.Text = "Configure Database";
            this.tabMain.ResumeLayout(false);
            this.tabPageOptions.ResumeLayout(false);
            this.tabPageOptions.PerformLayout();
            this.tabPageLocalDatabase.ResumeLayout(false);
            this.tabPageRemoteDatabase.ResumeLayout(false);
            this.tabPageBackupSettings.ResumeLayout(false);
            this.tabPageBackupSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udBackupMaxAge)).EndInit();
            this.gbBackupFTPDetails.ResumeLayout(false);
            this.gbBackupFTPDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udBackupFTPPort)).EndInit();
            this.tabPageRemoteUpdateSettings.ResumeLayout(false);
            this.tabPageRemoteUpdateSettings.PerformLayout();
            this.tabPageReplicationSettings.ResumeLayout(false);
            this.tabPageReplicationSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udTimeoutMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udUploadCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udDownloadCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udResetErrorCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udReplicateMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udVerifyAll)).EndInit();
            this.tabPageTables.ResumeLayout(false);
            this.pumTables.ResumeLayout(false);
            this.tabPageAutoCorrect.ResumeLayout(false);
            this.pumAutoCorrect.ResumeLayout(false);
            this.tabPageChildReplicationSchema.ResumeLayout(false);
            this.tabPageChildReplicationSchema.PerformLayout();
            this.tabPageChildDropScript.ResumeLayout(false);
            this.tabPageChildDropScript.PerformLayout();
            this.tabPageMasterCreateScript.ResumeLayout(false);
            this.tabPageMasterCreateScript.PerformLayout();
            this.tabPageMasterDropScript.ResumeLayout(false);
            this.tabPageMasterDropScript.PerformLayout();
            this.tabPageGenerators.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridGenerators)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabPageLocalDatabase;
        private SharedControls.DatabaseConnection dbConnChild;
        private System.Windows.Forms.TabPage tabPageRemoteDatabase;
        private SharedControls.DatabaseConnection dbConnMaster;
        private System.Windows.Forms.TabPage tabPageReplicationSettings;
        private System.Windows.Forms.CheckBox cbForceHours;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown udTimeoutMinutes;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown udUploadCount;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown udDownloadCount;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown udResetErrorCount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown udReplicateMinutes;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblVerifyAfterIterationsIt;
        private System.Windows.Forms.NumericUpDown udVerifyAll;
        private System.Windows.Forms.Label lblVerifyAfterIterations;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabPage tabPageTables;
        private System.Windows.Forms.TabPage tabPageAutoCorrect;
        private SharedControls.Classes.ListViewEx lvTables;
        private System.Windows.Forms.ColumnHeader colTablesName;
        private System.Windows.Forms.ColumnHeader colTableMasterReplicating;
        private System.Windows.Forms.ColumnHeader colTableChildReplicated;
        private System.Windows.Forms.ContextMenuStrip pumTables;
        private System.Windows.Forms.ToolStripMenuItem pumTablesAdd;
        private System.Windows.Forms.ToolStripMenuItem pumTablesRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem pumTablesConfigure;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem pumTablesValidateAll;
        private System.Windows.Forms.TabPage tabPageChildReplicationSchema;
        private System.Windows.Forms.TextBox txtChildCreateScript;
        private System.Windows.Forms.TabPage tabPageOptions;
        private System.Windows.Forms.TabPage tabPageBackupSettings;
        private System.Windows.Forms.TabPage tabPageRemoteUpdateSettings;
        private SharedControls.TextBoxEx txtName;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox cbOptionReplicate;
        private System.Windows.Forms.CheckBox cbOptionRemoteUpdate;
        private System.Windows.Forms.CheckBox cbOptionBackupDatabase;
        private System.Windows.Forms.TextBox txtBackupPath;
        private System.Windows.Forms.Label lblBackupPath;
        private System.Windows.Forms.CheckBox cbBackupUseSiteID;
        private System.Windows.Forms.CheckBox cbBackupCopyToRemote;
        private System.Windows.Forms.CheckBox cbBackupFileCompress;
        private System.Windows.Forms.GroupBox gbBackupFTPDetails;
        private System.Windows.Forms.NumericUpDown udBackupFTPPort;
        private System.Windows.Forms.Button btnBackupFTPTest;
        private System.Windows.Forms.TextBox txtBackupFTPPassword;
        private System.Windows.Forms.TextBox txtBackupFTPUsername;
        private System.Windows.Forms.TextBox txtBackupFTPHost;
        private System.Windows.Forms.Label lblBackupFTPPort;
        private System.Windows.Forms.Label lblBackupFTPPassword;
        private System.Windows.Forms.Label lblBackupFTPUsername;
        private System.Windows.Forms.Label lblBackupFTPHost;
        private System.Windows.Forms.TextBox txtBackupName;
        private System.Windows.Forms.Label lblBackupName;
        private System.Windows.Forms.TextBox txtRemoteUpdateLocation;
        private System.Windows.Forms.Label lblRemoteUpdateLocation;
        private System.Windows.Forms.TextBox txtRemoteUpdateXMLFile;
        private System.Windows.Forms.Label lblRemoteUpdateVersion;
        private System.Windows.Forms.Label lblBackupMaxAgeDays;
        private System.Windows.Forms.NumericUpDown udBackupMaxAge;
        private System.Windows.Forms.Label lblMaxAge;
        private System.Windows.Forms.CheckBox cbBackupsDeleteOldFiles;
        private System.Windows.Forms.CheckBox cbReplicationAutoUpdateTriggers;
        private SharedControls.TextBoxEx txtSiteID;
        private System.Windows.Forms.Label lblSiteID;
        private System.Windows.Forms.RadioButton rbReplicateChild;
        private System.Windows.Forms.RadioButton rbReplicateMaster;
        private System.Windows.Forms.TabPage tabPageChildDropScript;
        private System.Windows.Forms.Button btnGenerateChildDropScript;
        private System.Windows.Forms.TextBox txtChildDropScript;
        private System.Windows.Forms.Button btnChildCreateScript;
        private System.Windows.Forms.CheckBox cbEnabled;
        private System.Windows.Forms.TabPage tabPageMasterCreateScript;
        private System.Windows.Forms.TabPage tabPageMasterDropScript;
        private System.Windows.Forms.TextBox txtMasterCreate;
        private System.Windows.Forms.TextBox txtMasterDrop;
        private System.Windows.Forms.Button btnMasterCreateExecute;
        private System.Windows.Forms.Button btnMasterCreate;
        private System.Windows.Forms.Button btnMasterDrop;
        private System.Windows.Forms.Button btnChildScriptExecute;
        private System.Windows.Forms.Button btnChildExecuteRemoveScript;
        private System.Windows.Forms.Button btnMasterExecuteDropScript;
        private System.Windows.Forms.TabPage tabPageGenerators;
        private System.Windows.Forms.DataGridView gridGenerators;
        private System.Windows.Forms.Button btnUpdateGenerators;
        private System.Windows.Forms.Button btnInstallReplicationScript;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem pumTablesRefresh;
        private SharedControls.Controls.TextBlock textBlockServerSQL;
        private SharedControls.Controls.TextBlock textBlockClientSQL;
        private System.Windows.Forms.ColumnHeader colTableSortOrder;
        private System.Windows.Forms.ToolStripMenuItem pumTablesUpdateSortOrders;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private SharedControls.Classes.ListViewEx lvAutoCorrect;
        private System.Windows.Forms.ColumnHeader colAutoCorrectTable;
        private System.Windows.Forms.ColumnHeader colAutoCorrectKeyName;
        private System.Windows.Forms.ColumnHeader colAutoCorrectOptions;
        private System.Windows.Forms.ContextMenuStrip pumAutoCorrect;
        private System.Windows.Forms.ToolStripMenuItem pumAutoCorrectAdd;
        private System.Windows.Forms.ToolStripMenuItem pumAutoCorrectDelete;
        private System.Windows.Forms.ToolStripMenuItem pumAutoCorrectEdit;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem pumAutoCorrectRefresh;
        private System.Windows.Forms.Button btnUninstallReplicationScript;
        private SharedControls.Controls.TextBlock textBlockClientRemove;
        private SharedControls.Controls.TextBlock textBlockServerRemove;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox cbRequireUniqueAcess;
        private System.Windows.Forms.DateTimePicker dtpBackupTime;
        private System.Windows.Forms.CheckBox cbBackupAfter;
        private System.Windows.Forms.Label lblVerifyAfterHours;
        private System.Windows.Forms.DateTimePicker dtpVerifyAfter;
        private System.Windows.Forms.CheckBox cbForceVerification;
    }
}