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
 *  Copyright (c) 2011 - 2019 Simon Carter.  All Rights Reserved
 *
 *  Purpose:  
 *
 */

 namespace Replication.Service.Console
{
    partial class ReplicationClient
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
            this.components = new System.ComponentModel.Container();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnBackupDatabase = new System.Windows.Forms.Button();
            this.cmbClient = new System.Windows.Forms.ComboBox();
            this.btnCancelReplication = new System.Windows.Forms.Button();
            this.btnHardConfirm = new System.Windows.Forms.Button();
            this.cbAutoScroll = new System.Windows.Forms.CheckBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnPreventReplication = new System.Windows.Forms.Button();
            this.btnForceReplication = new System.Windows.Forms.Button();
            this.lstReplicationMessages = new System.Windows.Forms.ListBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.tabPageThreads = new System.Windows.Forms.TabPage();
            this.lvThreads = new SharedControls.Classes.ListViewEx();
            this.colThreadsName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colThreadsProcess = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colThreadsSystem = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colThreadsID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colThreadsCancelled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colThreadsUnresponsive = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colThreadsRemoving = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuThreads = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuThreadsRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsLabelTimeTillRun = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsCPU = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsRunTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsMissingCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tmrRuntime = new System.Windows.Forms.Timer(this.components);
            this.tabMain.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPageThreads.SuspendLayout();
            this.contextMenuThreads.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabMain.Controls.Add(this.tabPage1);
            this.tabMain.Controls.Add(this.tabPageThreads);
            this.tabMain.Location = new System.Drawing.Point(12, 12);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(760, 312);
            this.tabMain.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnBackupDatabase);
            this.tabPage1.Controls.Add(this.cmbClient);
            this.tabPage1.Controls.Add(this.btnCancelReplication);
            this.tabPage1.Controls.Add(this.btnHardConfirm);
            this.tabPage1.Controls.Add(this.cbAutoScroll);
            this.tabPage1.Controls.Add(this.btnClear);
            this.tabPage1.Controls.Add(this.btnPreventReplication);
            this.tabPage1.Controls.Add(this.btnForceReplication);
            this.tabPage1.Controls.Add(this.lstReplicationMessages);
            this.tabPage1.Controls.Add(this.btnStop);
            this.tabPage1.Controls.Add(this.btnStart);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(752, 286);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Replication";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnBackupDatabase
            // 
            this.btnBackupDatabase.Location = new System.Drawing.Point(453, 17);
            this.btnBackupDatabase.Name = "btnBackupDatabase";
            this.btnBackupDatabase.Size = new System.Drawing.Size(116, 23);
            this.btnBackupDatabase.TabIndex = 8;
            this.btnBackupDatabase.Text = "Backup Database";
            this.btnBackupDatabase.UseVisualStyleBackColor = true;
            this.btnBackupDatabase.Click += new System.EventHandler(this.btnBackupDatabase_Click);
            // 
            // cmbClient
            // 
            this.cmbClient.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbClient.FormattingEnabled = true;
            this.cmbClient.Location = new System.Drawing.Point(14, 47);
            this.cmbClient.Name = "cmbClient";
            this.cmbClient.Size = new System.Drawing.Size(156, 21);
            this.cmbClient.TabIndex = 2;
            this.cmbClient.SelectedIndexChanged += new System.EventHandler(this.cmbClient_SelectedIndexChanged);
            // 
            // btnCancelReplication
            // 
            this.btnCancelReplication.Location = new System.Drawing.Point(453, 46);
            this.btnCancelReplication.Name = "btnCancelReplication";
            this.btnCancelReplication.Size = new System.Drawing.Size(116, 23);
            this.btnCancelReplication.TabIndex = 5;
            this.btnCancelReplication.Text = "Cancel Replication";
            this.btnCancelReplication.UseVisualStyleBackColor = true;
            this.btnCancelReplication.Click += new System.EventHandler(this.btnCancelReplication_Click);
            // 
            // btnHardConfirm
            // 
            this.btnHardConfirm.Location = new System.Drawing.Point(209, 17);
            this.btnHardConfirm.Name = "btnHardConfirm";
            this.btnHardConfirm.Size = new System.Drawing.Size(77, 23);
            this.btnHardConfirm.TabIndex = 6;
            this.btnHardConfirm.Text = "Force Verify";
            this.btnHardConfirm.UseVisualStyleBackColor = true;
            this.btnHardConfirm.Click += new System.EventHandler(this.btnHardConfirm_Click);
            // 
            // cbAutoScroll
            // 
            this.cbAutoScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAutoScroll.AutoSize = true;
            this.cbAutoScroll.Checked = true;
            this.cbAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAutoScroll.Location = new System.Drawing.Point(661, 21);
            this.cbAutoScroll.Name = "cbAutoScroll";
            this.cbAutoScroll.Size = new System.Drawing.Size(77, 17);
            this.cbAutoScroll.TabIndex = 9;
            this.cbAutoScroll.Text = "Auto Scroll";
            this.cbAutoScroll.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(292, 17);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 7;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnPreventReplication
            // 
            this.btnPreventReplication.Location = new System.Drawing.Point(331, 46);
            this.btnPreventReplication.Name = "btnPreventReplication";
            this.btnPreventReplication.Size = new System.Drawing.Size(116, 23);
            this.btnPreventReplication.TabIndex = 4;
            this.btnPreventReplication.Text = "Prevent Replication";
            this.btnPreventReplication.UseVisualStyleBackColor = true;
            this.btnPreventReplication.Click += new System.EventHandler(this.btnPreventReplication_Click);
            // 
            // btnForceReplication
            // 
            this.btnForceReplication.Location = new System.Drawing.Point(209, 46);
            this.btnForceReplication.Name = "btnForceReplication";
            this.btnForceReplication.Size = new System.Drawing.Size(116, 23);
            this.btnForceReplication.TabIndex = 3;
            this.btnForceReplication.Text = "Force Replication";
            this.btnForceReplication.UseVisualStyleBackColor = true;
            this.btnForceReplication.Click += new System.EventHandler(this.btnForceReplication_Click);
            // 
            // lstReplicationMessages
            // 
            this.lstReplicationMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstReplicationMessages.FormattingEnabled = true;
            this.lstReplicationMessages.HorizontalScrollbar = true;
            this.lstReplicationMessages.Location = new System.Drawing.Point(13, 78);
            this.lstReplicationMessages.Name = "lstReplicationMessages";
            this.lstReplicationMessages.ScrollAlwaysVisible = true;
            this.lstReplicationMessages.Size = new System.Drawing.Size(725, 199);
            this.lstReplicationMessages.TabIndex = 10;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(95, 17);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Disconnect";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(14, 17);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Connect";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabPageThreads
            // 
            this.tabPageThreads.Controls.Add(this.lvThreads);
            this.tabPageThreads.Location = new System.Drawing.Point(4, 22);
            this.tabPageThreads.Name = "tabPageThreads";
            this.tabPageThreads.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageThreads.Size = new System.Drawing.Size(752, 286);
            this.tabPageThreads.TabIndex = 4;
            this.tabPageThreads.Text = "Threads";
            this.tabPageThreads.UseVisualStyleBackColor = true;
            // 
            // lvThreads
            // 
            this.lvThreads.AllowColumnReorder = true;
            this.lvThreads.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvThreads.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colThreadsName,
            this.colThreadsProcess,
            this.colThreadsSystem,
            this.colThreadsID,
            this.colThreadsCancelled,
            this.colThreadsUnresponsive,
            this.colThreadsRemoving});
            this.lvThreads.ContextMenuStrip = this.contextMenuThreads;
            this.lvThreads.FullRowSelect = true;
            this.lvThreads.Location = new System.Drawing.Point(6, 6);
            this.lvThreads.MinimumSize = new System.Drawing.Size(740, 274);
            this.lvThreads.Name = "lvThreads";
            this.lvThreads.OwnerDraw = true;
            this.lvThreads.SaveName = "WDThreads";
            this.lvThreads.ShowToolTip = false;
            this.lvThreads.Size = new System.Drawing.Size(740, 274);
            this.lvThreads.TabIndex = 1;
            this.lvThreads.UseCompatibleStateImageBehavior = false;
            this.lvThreads.View = System.Windows.Forms.View.Details;
            // 
            // colThreadsName
            // 
            this.colThreadsName.Text = "Name";
            this.colThreadsName.Width = 230;
            // 
            // colThreadsProcess
            // 
            this.colThreadsProcess.Text = "Process CPU";
            this.colThreadsProcess.Width = 90;
            // 
            // colThreadsSystem
            // 
            this.colThreadsSystem.Text = "System CPU";
            this.colThreadsSystem.Width = 90;
            // 
            // colThreadsID
            // 
            this.colThreadsID.Text = "ID";
            this.colThreadsID.Width = 50;
            // 
            // colThreadsCancelled
            // 
            this.colThreadsCancelled.Text = "Cancelled";
            this.colThreadsCancelled.Width = 80;
            // 
            // colThreadsUnresponsive
            // 
            this.colThreadsUnresponsive.Text = "Unresponsive";
            this.colThreadsUnresponsive.Width = 80;
            // 
            // colThreadsRemoving
            // 
            this.colThreadsRemoving.Text = "Marked for Removal";
            this.colThreadsRemoving.Width = 120;
            // 
            // contextMenuThreads
            // 
            this.contextMenuThreads.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenuThreadsRefresh});
            this.contextMenuThreads.Name = "contextMenuStrip1";
            this.contextMenuThreads.Size = new System.Drawing.Size(114, 26);
            // 
            // contextMenuThreadsRefresh
            // 
            this.contextMenuThreadsRefresh.Name = "contextMenuThreadsRefresh";
            this.contextMenuThreadsRefresh.Size = new System.Drawing.Size(113, 22);
            this.contextMenuThreadsRefresh.Text = "Refresh";
            this.contextMenuThreadsRefresh.Click += new System.EventHandler(this.contextMenuThreadsRefresh_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsLabelTimeTillRun,
            this.tsCPU,
            this.tsRunTime,
            this.tsMissingCount,
            this.tsStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 325);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(784, 24);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip";
            // 
            // tsLabelTimeTillRun
            // 
            this.tsLabelTimeTillRun.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tsLabelTimeTillRun.Name = "tsLabelTimeTillRun";
            this.tsLabelTimeTillRun.Size = new System.Drawing.Size(92, 19);
            this.tsLabelTimeTillRun.Text = "Not Connected";
            // 
            // tsCPU
            // 
            this.tsCPU.AutoSize = false;
            this.tsCPU.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tsCPU.Name = "tsCPU";
            this.tsCPU.Size = new System.Drawing.Size(85, 19);
            this.tsCPU.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tsRunTime
            // 
            this.tsRunTime.Name = "tsRunTime";
            this.tsRunTime.Size = new System.Drawing.Size(0, 19);
            this.tsRunTime.TextChanged += new System.EventHandler(this.tsMissingCount_TextChanged);
            // 
            // tsMissingCount
            // 
            this.tsMissingCount.Name = "tsMissingCount";
            this.tsMissingCount.Size = new System.Drawing.Size(0, 19);
            this.tsMissingCount.TextChanged += new System.EventHandler(this.tsMissingCount_TextChanged);
            // 
            // tsStatus
            // 
            this.tsStatus.Name = "tsStatus";
            this.tsStatus.Size = new System.Drawing.Size(0, 19);
            this.tsStatus.TextChanged += new System.EventHandler(this.tsMissingCount_TextChanged);
            // 
            // tmrRuntime
            // 
            this.tmrRuntime.Enabled = true;
            this.tmrRuntime.Interval = 500;
            this.tmrRuntime.Tick += new System.EventHandler(this.tmrRuntime_Tick);
            // 
            // ReplicationClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 349);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabMain);
            this.Icon = global::Replication.Service.Console.Properties.Resources.Server_5720;
            this.MinimumSize = new System.Drawing.Size(800, 388);
            this.Name = "ReplicationClient";
            this.Text = "Replication";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SynchClient_FormClosing);
            this.tabMain.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPageThreads.ResumeLayout(false);
            this.contextMenuThreads.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.CheckBox cbAutoScroll;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnPreventReplication;
        private System.Windows.Forms.Button btnForceReplication;
        private System.Windows.Forms.ListBox lstReplicationMessages;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnHardConfirm;
        private System.Windows.Forms.Button btnCancelReplication;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsLabelTimeTillRun;
        private System.Windows.Forms.ToolStripStatusLabel tsStatus;
        private System.Windows.Forms.ToolStripStatusLabel tsMissingCount;
        private System.Windows.Forms.Timer tmrRuntime;
        private System.Windows.Forms.ToolStripStatusLabel tsRunTime;
        private System.Windows.Forms.TabPage tabPageThreads;
        private System.Windows.Forms.ComboBox cmbClient;
        private System.Windows.Forms.ToolStripStatusLabel tsCPU;
        private SharedControls.Classes.ListViewEx lvThreads;
        private System.Windows.Forms.ColumnHeader colThreadsName;
        private System.Windows.Forms.ColumnHeader colThreadsProcess;
        private System.Windows.Forms.ColumnHeader colThreadsSystem;
        private System.Windows.Forms.ColumnHeader colThreadsID;
        private System.Windows.Forms.ColumnHeader colThreadsCancelled;
        private System.Windows.Forms.ColumnHeader colThreadsUnresponsive;
        private System.Windows.Forms.ColumnHeader colThreadsRemoving;
        private System.Windows.Forms.ContextMenuStrip contextMenuThreads;
        private System.Windows.Forms.ToolStripMenuItem contextMenuThreadsRefresh;
        private System.Windows.Forms.Button btnBackupDatabase;

    }
}