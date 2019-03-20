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
 *  Purpose:  Configure Child Database Form
 *
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Replication.Engine;
using Replication.Engine.Classes;

using Shared;

namespace Replication.Service.Forms
{
    public partial class ConfigureChild : SharedControls.Forms.BaseForm
    {
        #region Private Members

        private DatabaseConnection _connection;

        private List<Generators> _generatorValues;

        private bool _isValidating = false;

        #endregion Private Members

        #region Constructors

        public ConfigureChild()
        {
            InitializeComponent();

            tabMain.TabPages.Remove(tabPageRemoteUpdateSettings);
            tabMain.TabPages.Remove(tabPageBackupSettings);
            tabMain.TabPages.Remove(tabPageRemoteDatabase);
            tabMain.TabPages.Remove(tabPageReplicationSettings);
            tabMain.TabPages.Remove(tabPageTables);
            tabMain.TabPages.Remove(tabPageAutoCorrect);
            tabMain.TabPages.Remove(tabPageChildReplicationSchema);
            tabMain.TabPages.Remove(tabPageChildDropScript);
            tabMain.TabPages.Remove(tabPageMasterCreateScript);
            tabMain.TabPages.Remove(tabPageMasterDropScript);
            tabMain.TabPages.Remove(tabPageGenerators);

            btnChildCreateScript.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnChildExecuteRemoveScript.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnChildScriptExecute.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnGenerateChildDropScript.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnMasterCreate.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnMasterCreateExecute.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnMasterDrop.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnMasterExecuteDropScript.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnUpdateGenerators.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        }

        #endregion Constructors

        #region Static Methods

        public static bool ShowChildSettings(DatabaseConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("Invalid Connection Data");

            ConfigureChild child = new ConfigureChild();
            try
            {
                child.Connection = connection;
                return (child.ShowDialog() == DialogResult.OK);
            }
            finally
            {
                child.Dispose();
                child = null;
            }
        }

        #endregion Static Methods

        #region Properties

        public DatabaseConnection Connection
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Connection is invalid");

                _connection = value;
                txtName.Text = _connection.Name;
                cbEnabled.Checked = _connection.Enabled;
                dbConnChild.ConnectionString = _connection.ChildDatabase;

                // replication
                dbConnMaster.ConnectionString = _connection.MasterDatabase;
                udReplicateMinutes.Value = _connection.ReplicateInterval;
                udTimeoutMinutes.Value = _connection.TimeOut;
                udUploadCount.Value = _connection.MaximumUploadCount;
                udDownloadCount.Value = _connection.MaximumDownloadCount;
                cbForceVerification.Checked = _connection.VerifyData;
                cbForceHours.Checked = _connection.VerifyDataAfterHour;

                DateTime startDate = DateTime.FromFileTime(_connection.VerifyStart);

                if (startDate.Year < 2000)
                    startDate = new DateTime(DateTime.Now.Year, startDate.Month, startDate.Day, startDate.Hour, startDate.Minute, 0);

                dtpVerifyAfter.Value = startDate;
                udVerifyAll.Value = _connection.VerifyDataInterval;
                udResetErrorCount.Value = _connection.VerifyErrorReset;
                cbRequireUniqueAcess.Checked = _connection.RequireUniqueAccess;
                cbReplicationAutoUpdateTriggers.Checked = _connection.ReplicateDatabase && _connection.ReplicateUpdateTriggers;

                switch (_connection.ReplicationType)
                {
                    case ReplicationType.NotSet:
                        rbReplicateChild.Enabled = _connection.ReplicateDatabase;
                        rbReplicateMaster.Enabled = _connection.ReplicateDatabase;
                        break;
                    case ReplicationType.Child:
                        rbReplicateChild.Checked = true;
                        txtSiteID.Text = _connection.SiteID.ToString();
                        rbReplicateChild.Enabled = false;
                        rbReplicateMaster.Enabled = false;
                        break;
                    case ReplicationType.Master:
                        rbReplicateMaster.Checked = true;
                        rbReplicateChild.Enabled = false;
                        rbReplicateMaster.Enabled = false;
                        break;
                }

                txtSiteID.Enabled = _connection.ReplicateDatabase && (_connection.ReplicationType == ReplicationType.Child || rbReplicateChild.Checked) && _connection.SiteID < 1;
                lblSiteID.Enabled = _connection.ReplicateDatabase && (_connection.ReplicationType == ReplicationType.Child || rbReplicateChild.Checked) && _connection.SiteID < 1;

                btnInstallReplicationScript.Enabled = !_connection.InitialScriptExecuted;
                btnUninstallReplicationScript.Enabled = _connection.InitialScriptExecuted;

                LoadData();


                cbOptionBackupDatabase.Enabled = _connection.InitialScriptExecuted && _connection.ChildDatabaseLoaded;
                cbOptionRemoteUpdate.Enabled = _connection.InitialScriptExecuted && _connection.ChildDatabaseLoaded;
                cbOptionReplicate.Enabled = _connection.ChildDatabaseLoaded;

                cbReplicationAutoUpdateTriggers.Enabled = _connection.ReplicateDatabase;

                if (cbOptionBackupDatabase.Enabled)
                    cbOptionBackupDatabase.Checked = _connection.BackupDatabase;

                if (cbOptionRemoteUpdate.Enabled)
                    cbOptionRemoteUpdate.Checked = _connection.RemoteUpdate;

                if (cbOptionReplicate.Enabled)
                    cbOptionReplicate.Checked = _connection.ReplicateDatabase;

                // backup
                txtBackupPath.Text = _connection.BackupPath;
                cbBackupFileCompress.Checked = _connection.BackupCompress;
                cbBackupCopyToRemote.Checked = _connection.BackupCopyRemote;
                cbBackupsDeleteOldFiles.Checked = _connection.BackupDeleteOldFiles;
                udBackupMaxAge.Value =_connection.BackupMaximumAge;
                cbBackupAfter.Checked = _connection.BackupAfterTimeEnabled;
                dtpBackupTime.Value = _connection.BackupAfterTime;

                dtpBackupTime.Enabled = cbBackupAfter.Checked;


                if (_connection.ReplicateDatabase)
                {
                    cbBackupUseSiteID.Checked = _connection.BackupUseSiteID;
                }
                else
                {
                    cbBackupUseSiteID.Checked = false;
                    cbBackupUseSiteID.Enabled = false;
                }

                txtBackupName.Text = _connection.BackupName;

                if (_connection.BackupCopyRemote)
                {
                    txtBackupFTPHost.Text = _connection.BackupFTPHost;
                    txtBackupFTPUsername.Text = _connection.BackupFTPUsername;
                    txtBackupFTPPassword.Text = _connection.BackupFTPPassword;
                    udBackupFTPPort.Value = _connection.BackupFTPPort;
                }

                txtRemoteUpdateXMLFile.Text = _connection.RemoteUpdateXML;
                txtRemoteUpdateLocation.Text = _connection.RemoteUpdateLocation;
                SetEnabledState();

                cbForceHours_CheckedChanged(this, EventArgs.Empty);
                cbForceVerification_CheckedChanged(this, EventArgs.Empty);
            }
        }

        #endregion Properties

        #region Private Methods

        #region Popup Menu Tables

        private void pumTables_Opening(object sender, CancelEventArgs e)
        {
            if (lvTables.SelectedItems.Count == 0)
            {
                pumTablesRemove.Enabled = false;
                pumTablesConfigure.Enabled = false;
                pumTablesAdd.Enabled = false;

                return;
            }
            
            ListViewItem item = lvTables.SelectedItems[0];

            if (item == null)
            { 
                pumTablesRemove.Enabled = false;
                pumTablesConfigure.Enabled = false;
                pumTablesAdd.Enabled = false;
            }
            else
            {
                bool isConfigured = item.SubItems[1].Text == "Yes";
                pumTablesConfigure.Enabled = isConfigured;
                pumTablesRemove.Enabled = isConfigured;
                pumTablesAdd.Enabled = !item.Font.Strikeout && !isConfigured;
            }

            pumTablesValidateAll.Enabled = _connection.ReplicationType != ReplicationType.Master;
            pumTablesUpdateSortOrders.Enabled = _connection.ReplicationType != ReplicationType.Master;
        }

        private void pumTablesAdd_Click(object sender, EventArgs e)
        {
            if (lvTables.SelectedItems.Count == -1)
                return;

            ListViewItem item = lvTables.SelectedItems[0];

            if (item != null)
            {
                API api = new API();
                try
                {
                    string triggerName = GenerateTriggerName(item.Text);
                    api.ChildAddTable(_connection, item.Text, triggerName);

                    if (rbReplicateChild.Checked &&
                        !String.IsNullOrEmpty(Utilities.GetDatabasePart(_connection.MasterDatabase, "Database")))
                    {
                        api.MasterAddTable(_connection, item.Text);
                    }

                    item.SubItems[1].Text = "Yes";

                    if (rbReplicateChild.Checked) 
                        item.SubItems[2].Text = "Yes";

                    item.ForeColor = Color.Blue;
                    pumTablesConfigure_Click(sender, e);
                }
                finally
                {
                    api = null;
                }
            }
        }

        private bool ItemExists(List<string> list, string value)
        {
            foreach (string s in list)
                if (s == value)
                    return (true);

            return (false);
        }

        private string GenerateTriggerName(string tableName)
        {
            API api = new API();
            try
            {
                List<string> currentTriggers = api.GetChildTriggerNames(_connection);

                if (tableName.Length > 18)
                    tableName = tableName.Substring(0, 17);

                if (!ItemExists(currentTriggers, tableName))
                    return (tableName);

                tableName = tableName.Substring(0, tableName.Length > 15 ? 15 : tableName.Length);

                for (int i = 0; i < 999; i++)
                {
                    string temp = tableName + i.ToString();

                    if (!ItemExists(currentTriggers, temp))
                        return (temp);
                }

                throw new Exception("Could not generate unique trigger name");
            }
            finally
            {
                api = null;
            }
        }

        private void pumTablesRemove_Click(object sender, EventArgs e)
        {
            if (lvTables.SelectedItems.Count == -1)
                return;

            ListViewItem item = lvTables.SelectedItems[0];

            if (item != null)
            {      
                bool masterChecked = false;
                
                if (rbReplicateChild.Checked)
                    masterChecked = item.SubItems[2].Text == "Yes";
            
                if (Forms.RemoveReplicationTable.Show(ref masterChecked))
                {
                    API api = new API();
                    try
                    {
                        api.RemoveReplicatedTable(_connection, item.Text, masterChecked, true, true, true);
                        item.SubItems[1].Text = "No";
                        item.ForeColor = Color.Black;

                        if (masterChecked)
                            item.SubItems[2].Text = "No";
                    }
                    finally
                    {
                        api = null;
                    }
                }
            }
        }

        private void pumTablesConfigure_Click(object sender, EventArgs e)
        {
            if (lvTables.SelectedItems.Count == -1)
                return;

            ListViewItem item = lvTables.SelectedItems[0];

            if (item != null)
            {
                if (!_connection.ChildDatabaseLoaded)
                {
                    ShowError("Child Database", "The child database has not been setup correctly");
                    return;
                }

                if (_connection.ReplicationType != ReplicationType.Master && !_connection.MasterDatabaseLoaded)
                {
                    ShowError("Master Database", "The Master database has not been setup correctly");
                    return;
                }

                API api = new API();
                try
                {
                    if (ConfigureReplicatedTable.Show(api, _connection, item.Text))
                    {
                        ValidateSchema();
                    }
                }
                finally
                {
                    api = null;
                }
            }
        }

        private void pumTablesValidateAll_Click(object sender, EventArgs e)
        {
            ValidateSchema();
        }

        private void pumTablesRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void pumTablesUpdateSortOrders_Click(object sender, EventArgs e)
        {
            if (ShowQuestion("Update Sort Orders", "This will update the sort orders based on foreign keys within the database\r\n\r\nDo you want to continue?"))
            {
                this.Cursor = Cursors.WaitCursor;
                try
                {
                    foreach (ListViewItem item in lvTables.Items)
                    {
                        if (item.SubItems[1].Text == "Yes")
                        {
                            int sortOrder = 100;
                            int newValue = _connection.SuggestedSortOrder(item.Text, true, ref sortOrder);
                            item.SubItems[3].Text = sortOrder.ToString();
                        }
                    }
                }
                finally
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
        }

        #endregion Popup Menu Tables

        #region Schema Validation

        private void ValidateSchema()
        {
            this.Cursor = Cursors.AppStarting;
            _isValidating = true;

            foreach (ListViewItem item in lvTables.Items)
            {
                item.ForeColor = Color.Gray;
            }

            SchemaValidation validation = new SchemaValidation(_connection);
            validation.ValidationComplete += validation_ValidationComplete;
            validation.ValidationError += validation_ValidationError;
            Shared.Classes.ThreadManager.ThreadStart(validation, "Database Schema Validation", System.Threading.ThreadPriority.AboveNormal);
        }

        void validation_ValidationError(object sender, Shared.SchemaValidationArgs e)
        {
            if (this.InvokeRequired)
            {
                SchemaValidationHandler svh = new SchemaValidationHandler(validation_ValidationError);
                this.Invoke(svh, new object[] { sender, e });
            }
            else
            {
                foreach (ListViewItem item in lvTables.Items)
                {
                    if (item.Text == e.ObjectName1)
                    {
                        if (e.ExistDifferentName)
                            item.ForeColor = Color.DarkGreen;
                        else
                            item.ForeColor = Color.Red;

                        item.Tag = String.Format("Error: {0}\n\nTable: {1}\n\nObject: {2}\nObject Type: {3}\n\n" +
                            "Similar Object Found: {5}\n\nSQL:\n{4}", 
                            Shared.Utilities.SplitCamelCase(e.Message), 
                            e.ObjectName1, e.ObjectName2, 
                            Shared.Utilities.SplitCamelCase(e.ObjectType), e.SQL, 
                            e.ExistDifferentName ? "Yes" : "No");
                        break;
                    }
                }
            }
        }

        void validation_ValidationComplete(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                EventHandler eh = new EventHandler(validation_ValidationComplete);
                this.Invoke(eh, new object[] { sender, e });
            }
            else
            {
                API api = new API();
                try
                {
                    foreach (ListViewItem item in lvTables.Items)
                    {
                        if (item.Font.Strikeout)
                        {
                            item.ForeColor = Color.Red;
                        }
                        else
                        {
                            if (item.ForeColor == Color.Gray)
                            {
                                if (item.SubItems[1].Text == "Yes")
                                {
                                    List<ReplicatedTable> tableData = api.GetChildTableReplicatedTable(_connection, item.Text);

                                    if (!TableIsSetupCorrectly(tableData))
                                    {
                                        item.ForeColor = Color.DarkMagenta;
                                    }
                                    else
                                    {
                                        if ((_connection.ReplicationType == ReplicationType.Child && item.SubItems[1].Text == item.SubItems[2].Text) ||
                                                _connection.ReplicationType == ReplicationType.Master)
                                        {
                                            item.ForeColor = Color.Blue;
                                        }
                                        else
                                        {
                                            if (_connection.ReplicationType == ReplicationType.Child && item.SubItems[1].Text == "No" && item.SubItems[2].Text == "Yes")
                                                item.ForeColor = Color.DarkOrange;
                                            else
                                                item.ForeColor = Color.Cyan;
                                        }
                                    }
                                }
                                else
                                {
                                    if (_connection.ReplicationType == ReplicationType.Child && item.SubItems[1].Text == "No" && item.SubItems[2].Text == "Yes")
                                        item.ForeColor = Color.DarkOrange;
                                    else
                                        item.ForeColor = Color.Black;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    api = null;
                }

                this.Cursor = Cursors.Arrow;
                this.Invalidate(true);
                _isValidating = false;
            }
        }

        private bool TableIsSetupCorrectly(List<ReplicatedTable> tableData)
        {
            foreach (ReplicatedTable table in tableData)
            {
                if (table.IndiceType != 2 && String.IsNullOrEmpty(table.IDColumn))
                    return (false);

                //if (table.Operation == Operation.Update && table.IndiceType == 0 && String.IsNullOrEmpty(table.LocalGenerator))
                //    return (false);
            }

            return (true);
        }

        #endregion Schema Validation

        #region Options

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (SaveAllSettings())
                DialogResult = DialogResult.OK;
        }

        private bool SaveAllSettings()
        {
            // validate settings

            _connection.Name = txtName.Text;
            _connection.Enabled = cbEnabled.Checked;
            _connection.RemoteUpdate = cbOptionRemoteUpdate.Checked;
            _connection.BackupDatabase = cbOptionBackupDatabase.Checked;
            _connection.ReplicateDatabase = cbOptionReplicate.Checked;

            if (_connection.ReplicateDatabase)
            {
                if (_connection.ReplicationType == ReplicationType.NotSet)
                {
                    if (rbReplicateChild.Checked && (String.IsNullOrEmpty(txtSiteID.Text) ||
                        Shared.Utilities.StrToInt(txtSiteID.Text, -10) < 1))
                    {
                        ShowError("Site ID", "Please set the Site ID, this must be a number between 1 and 99999");
                        return (false);
                    }

                    _connection.ReplicationType = rbReplicateChild.Checked ? ReplicationType.Child : ReplicationType.Master;
                    _connection.SiteID = rbReplicateChild.Checked ? Shared.Utilities.StrToInt(txtSiteID.Text, 1) : 0;
                    
                }
            }

            _connection.ChildDatabase = dbConnChild.ConnectionString;

            if (_connection.ReplicateDatabase)
            {
                _connection.MasterDatabase = dbConnMaster.ConnectionString;
                _connection.ReplicateInterval = (uint)udReplicateMinutes.Value;
                _connection.TimeOut = (uint)udTimeoutMinutes.Value;
                _connection.MaximumUploadCount = (uint)udUploadCount.Value;
                _connection.MaximumDownloadCount = (uint)udDownloadCount.Value;
                _connection.VerifyData = cbForceVerification.Checked;
                _connection.VerifyDataAfterHour = cbForceHours.Checked;
                _connection.VerifyStart = dtpVerifyAfter.Value.ToFileTime();
                _connection.VerifyDataInterval = (uint)udVerifyAll.Value;
                _connection.VerifyErrorReset = (uint)udResetErrorCount.Value;
                _connection.RequireUniqueAccess = cbRequireUniqueAcess.Checked;
                _connection.ReplicateUpdateTriggers = (cbReplicationAutoUpdateTriggers.Enabled) && 
                    (cbReplicationAutoUpdateTriggers.Checked || _connection.SiteID == 0);
            }

            if (_connection.BackupDatabase)
            {

                if (String.IsNullOrEmpty(txtBackupPath.Text) || !System.IO.Directory.Exists(txtBackupPath.Text))
                {
                    ShowError("Backup Path", "The Backup Path does not exist, please select a valid path.");
                    return (false);
                }

                if (!cbBackupUseSiteID.Checked && String.IsNullOrEmpty(txtBackupName.Text))
                {
                    ShowError("Backup Name", "The backup Name can not be empty");
                    return (false);
                }

                _connection.BackupPath = txtBackupPath.Text;
                _connection.BackupCompress = cbBackupFileCompress.Checked;
                _connection.BackupCopyRemote = cbBackupCopyToRemote.Checked;
                _connection.BackupUseSiteID = cbBackupUseSiteID.Checked;
                _connection.BackupName = txtBackupName.Text;
                _connection.BackupDeleteOldFiles = cbBackupsDeleteOldFiles.Checked;
                _connection.BackupMaximumAge = (int)udBackupMaxAge.Value;
                _connection.BackupAfterTimeEnabled = cbBackupAfter.Checked;
                _connection.BackupAfterTime = dtpBackupTime.Value;

                if (_connection.BackupCopyRemote)
                {
                    if (String.IsNullOrEmpty(txtBackupFTPHost.Text) || 
                        String.IsNullOrEmpty(txtBackupFTPUsername.Text) ||
                        String.IsNullOrEmpty(txtBackupFTPPassword.Text))
                    {
                        ShowError("FTP Backup", "Please enter backup FTP details");
                        return (false);
                    }

                    _connection.BackupFTPHost = txtBackupFTPHost.Text;
                    _connection.BackupFTPUsername = txtBackupFTPUsername.Text;
                    _connection.BackupFTPPassword = txtBackupFTPPassword.Text;
                    _connection.BackupFTPPort = (int)udBackupFTPPort.Value;
                }
            }


            // remote update
            if (_connection.RemoteUpdate)
            {
                if (String.IsNullOrEmpty(txtRemoteUpdateLocation.Text) ||
                    String.IsNullOrEmpty(txtRemoteUpdateXMLFile.Text))
                {
                    ShowError("Remote Update", "Remote Update Settings have not been configured");
                    return (false);
                }

                _connection.RemoteUpdateXML = txtRemoteUpdateXMLFile.Text;
                _connection.RemoteUpdateLocation = txtRemoteUpdateLocation.Text;
            }

            return (true);
        }

        private void cbOptionBackupDatabase_CheckedChanged(object sender, EventArgs e)
        {
            if (cbOptionBackupDatabase.Checked)
            {
                if (cbOptionReplicate.Checked)
                    tabMain.TabPages.Insert(Shared.Utilities.CheckMinMax(2, 0, tabMain.TabPages.Count), tabPageBackupSettings);
                else
                    tabMain.TabPages.Add(tabPageBackupSettings);
            }
            else
                tabMain.TabPages.Remove(tabPageBackupSettings);

            SetEnabledState();
        }

        private void cbOptionRemoteUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (cbOptionRemoteUpdate.Checked)
            {
                if (cbOptionReplicate.Checked)
                    tabMain.TabPages.Insert(Shared.Utilities.CheckMinMax(3, 0, tabMain.TabPages.Count), tabPageRemoteUpdateSettings);
                else
                    tabMain.TabPages.Add(tabPageRemoteUpdateSettings);
            }
            else
                tabMain.TabPages.Remove(tabPageRemoteUpdateSettings);

            SetEnabledState();
        }

        private void cbOptionReplicate_CheckedChanged(object sender, EventArgs e)
        {
            cbBackupUseSiteID.Enabled = cbOptionReplicate.Checked;

            if (!cbBackupUseSiteID.Enabled)
                cbBackupUseSiteID.Checked = false;

            cbReplicationAutoUpdateTriggers.Enabled = cbOptionReplicate.Checked;

            if (cbOptionReplicate.Checked)
            {
                rbReplicateChild.Enabled = _connection.ReplicationType == ReplicationType.NotSet;
                rbReplicateMaster.Enabled = _connection.ReplicationType == ReplicationType.NotSet;

                txtSiteID.Enabled = (_connection.ReplicationType == ReplicationType.Child || rbReplicateChild.Checked) && _connection.SiteID < 1;
                lblSiteID.Enabled = (_connection.ReplicationType == ReplicationType.Child || rbReplicateChild.Checked) && _connection.SiteID < 1;
                
                btnInstallReplicationScript.Enabled = !_connection.InitialScriptExecuted;
                btnUninstallReplicationScript.Enabled = _connection.InitialScriptExecuted;

                if (_connection.ReplicationType == ReplicationType.Child)
                {
                    colTableChildReplicated.Text = "Child Replicating";
                    
                    if (!lvTables.Columns.Contains(colTableMasterReplicating))
                        lvTables.Columns.Add(colTableMasterReplicating);
                }
                else if (_connection.ReplicationType == ReplicationType.Master)
                {
                    colTableChildReplicated.Text = "Replicating";
                    lvTables.Columns.Remove(colTableMasterReplicating);
                }
            }
            else
            {
                rbReplicateChild.Enabled = false;
                rbReplicateMaster.Enabled = false;

                txtSiteID.Enabled = false;
                lblSiteID.Enabled = false;

                LoadTabs();
                
                btnInstallReplicationScript.Enabled = !_connection.InitialScriptExecuted;
                btnUninstallReplicationScript.Enabled = _connection.InitialScriptExecuted;
            }
                    
            LoadTabs();

            SetEnabledState();
        }

        private void SetEnabledState()
        {
            cbEnabled.Enabled = _connection.InitialScriptExecuted &&
                (cbOptionBackupDatabase.Checked |
                cbOptionRemoteUpdate.Checked |
                cbOptionReplicate.Checked);

            if (!cbEnabled.Enabled)
                cbEnabled.Checked = false;
        }

        private void btnInstallReplicationScript_Click(object sender, EventArgs e)
        {
            if (!rbReplicateChild.Checked && !rbReplicateMaster.Checked)
            {
                ShowError("Replication Type", "Please select either Master or Child before installing the script.");
                return;
            }

            if (SaveAllSettings())
            {
                if (rbReplicateMaster.Checked)
                {
                    if (ShowQuestion("Install", "Replication options will now be installed on the master database\n\nDo you want to continue?"))
                    {
                        RunScript(_connection.ChildDatabase, textBlockServerSQL.StringBlock);
                        _connection.InitialScriptExecuted = true;
                        btnInstallReplicationScript.Enabled = false;
                        btnUninstallReplicationScript.Enabled = true;
                        btnSave.Enabled = true;
                    }
                }
                else if (rbReplicateChild.Checked)
                {
                    if (ShowQuestion("Install", "Replication options will now be installed on the child database\n\nDo you want to continue?"))
                    {
                        RunScript(_connection.ChildDatabase, textBlockClientSQL.StringBlock);
                        _connection.InitialScriptExecuted = true;
                        btnInstallReplicationScript.Enabled = false;
                        btnUninstallReplicationScript.Enabled = true;
                        btnSave.Enabled = true;
                    }
                }

                API api = new API();
                try
                {
                    api.SetSiteID(_connection);
                }
                finally
                {
                    api = null;
                }

                cbOptionReplicate_CheckedChanged(sender, e);
                LoadData();

            }
        }

        private void btnUninstallReplicationScript_Click(object sender, EventArgs e)
        {
            API api = new API();
            try
            {
                if(api.ChildIsConfiguredForReplication(_connection))
                {
                    ShowInformation("Remove Replication", String.Format("The database is still configured for replication\r\n\r\n" +
                        "Please remove the configuration using {0} before continuing.",
                        rbReplicateMaster.Checked ? "Master Drop Script" : "Replication Remove Schema"));
                    return;
                }
            }
            finally
            {
                api = null;
            }

            if (rbReplicateMaster.Checked && !rbReplicateMaster.Enabled)
            {
                if (ShowQuestion("Install", "Replication options will now be removed from the master database\n\nDo you want to continue?"))
                {
                    RunScript(_connection.ChildDatabase, textBlockServerRemove.StringBlock);
                    _connection.InitialScriptExecuted = false;
                    btnInstallReplicationScript.Enabled = true;
                    btnUninstallReplicationScript.Enabled = false;
                }
            }
            else if (rbReplicateChild.Checked && !rbReplicateChild.Enabled)
            {
                if (ShowQuestion("Install", "Replication options will now be removed from the child database\n\nDo you want to continue?"))
                {
                    RunScript(_connection.ChildDatabase, textBlockClientRemove.StringBlock);
                    _connection.InitialScriptExecuted = false;
                    btnInstallReplicationScript.Enabled = true;
                    btnUninstallReplicationScript.Enabled = false;
                }
            }

            tabMain.TabPages.Remove(tabPageRemoteDatabase);
            tabMain.TabPages.Remove(tabPageReplicationSettings);
            tabMain.TabPages.Remove(tabPageTables);
            tabMain.TabPages.Remove(tabPageAutoCorrect);
            tabMain.TabPages.Remove(tabPageChildReplicationSchema);
            tabMain.TabPages.Remove(tabPageChildDropScript);
            tabMain.TabPages.Remove(tabPageGenerators);
        }

        private void rbReplicateMaster_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == rbReplicateMaster && rbReplicateMaster.Checked)
            {
                txtSiteID.Enabled = false;
                lblSiteID.Enabled = false;
            }
            else if (sender == rbReplicateChild && rbReplicateChild.Checked)
            {
                txtSiteID.Enabled = true;
                lblSiteID.Enabled = true;
            }

            LoadTabs();
        }

        private void LoadTabs()
        {
            tabMain.TabPages.Remove(tabPageMasterCreateScript);
            tabMain.TabPages.Remove(tabPageMasterDropScript);
            tabMain.TabPages.Remove(tabPageRemoteDatabase);
            tabMain.TabPages.Remove(tabPageReplicationSettings);
            tabMain.TabPages.Remove(tabPageTables);
            tabMain.TabPages.Remove(tabPageAutoCorrect);
            tabMain.TabPages.Remove(tabPageChildReplicationSchema);
            tabMain.TabPages.Remove(tabPageChildDropScript);
            tabMain.TabPages.Remove(tabPageGenerators);

            if (_connection.ReplicationType == ReplicationType.Child)
            {
                if (_connection.InitialScriptExecuted && cbOptionReplicate.Checked)
                {
                    tabMain.TabPages.AddRange(new TabPage[] { tabPageRemoteDatabase, tabPageReplicationSettings, 
                        tabPageTables, tabPageAutoCorrect, tabPageChildReplicationSchema, tabPageChildDropScript,
                        tabPageGenerators });
                }
            }
            else if (_connection.ReplicationType == ReplicationType.Master)
            {
                tabMain.TabPages.Add(tabPageTables);
                tabMain.TabPages.Add(tabPageMasterCreateScript);
                tabMain.TabPages.Add(tabPageMasterDropScript);
                tabMain.TabPages.Add(tabPageGenerators);
            }
        }

        #endregion Options

        #region Child Database

        private void dbConnChild_TestConnectionSuccess(object sender, EventArgs e)
        {
            _connection.ChildDatabase = dbConnChild.ConnectionString;
            _connection.MasterDatabase = dbConnMaster.ConnectionString;

            _connection.LoadAllTables();

            cbOptionBackupDatabase.Enabled = _connection.InitialScriptExecuted && _connection.ChildDatabaseLoaded;
            cbOptionRemoteUpdate.Enabled = _connection.InitialScriptExecuted && _connection.ChildDatabaseLoaded;
            cbOptionReplicate.Enabled = _connection.ChildDatabaseLoaded;
        }

        #endregion Child Database

        #region Backup Options

        private void cbBackupUseSiteID_CheckedChanged(object sender, EventArgs e)
        {
            lblBackupName.Enabled = !cbBackupUseSiteID.Checked;
            txtBackupName.Enabled = !cbBackupUseSiteID.Checked;
        }

        private void cbBackupCopyToRemote_CheckedChanged(object sender, EventArgs e)
        {
            gbBackupFTPDetails.Enabled = cbBackupCopyToRemote.Checked;
        }

        private void btnBackupFTPTest_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Shared.Classes.ftp ftp = new Shared.Classes.ftp(
                txtBackupFTPHost.Text, txtBackupFTPUsername.Text, txtBackupFTPPassword.Text,
                2048, true, true, true, (int)udBackupFTPPort.Value);
            try
            {
                ftp.DirectoryListSimple("/");
                ShowInformation("FTP", "Successfully connected to FTP Server.");
            }
            catch (Exception err)
            {
                ShowError("FTP Error", err.Message);
            }
            finally
            {
                ftp = null;
                this.Cursor = Cursors.Arrow;
            }
        }

        #endregion Backup Options

        #region Master Database

        private void btnMasterDrop_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                ReplicationPrepareMasterDatabase prep = new ReplicationPrepareMasterDatabase();
                try
                {
                    DatabaseRemoteUpdate remoteUpdate = new DatabaseRemoteUpdate();
                    try
                    {
                        string fileName = prep.GenerateTriggerRemoveScript(_connection.ChildDatabase, true, remoteUpdate);
                        txtMasterDrop.Text = Shared.Utilities.FileRead(fileName, false);
                        System.IO.File.Delete(fileName);
                    }
                    finally
                    {
                        remoteUpdate = null;
                    }
                }
                finally
                {
                    prep = null;
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnMasterCreate_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                ReplicationPrepareMasterDatabase prep = new ReplicationPrepareMasterDatabase();
                try
                {
                    string fileName = String.Empty;

                    DatabaseRemoteUpdate remoteUpdate = new DatabaseRemoteUpdate();
                    try
                    {
                        if (prep.PrepareDatabaseForReplication(_connection.ChildDatabase, true, true, 
                            ref fileName, remoteUpdate))
                        {
                            txtMasterCreate.Text = Shared.Utilities.FileRead(fileName, false);
                            System.IO.File.Delete(fileName);
                        }
                    }
                    finally
                    {
                        remoteUpdate = null;
                    }
                }
                finally
                {
                    prep = null;
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnMasterCreateExecute_Click(object sender, EventArgs e)
        {
            Forms.UpdatingDatabase.ShowUpdate();
            try
            {
                Forms.UpdatingDatabase.Update("Building Schema");
                this.Cursor = Cursors.WaitCursor;
                try
                {
                    ReplicationPrepareMasterDatabase prep = new ReplicationPrepareMasterDatabase();
                    try
                    {
                        string fileName = String.Empty;
                        DatabaseRemoteUpdate remoteUpdate = new DatabaseRemoteUpdate();
                        try
                        {
                            remoteUpdate.OnUpdateDatabase += DatabaseRemoteUpdate_OnUpdateDatabase;

                            if (prep.PrepareDatabaseForReplication(_connection.ChildDatabase, true, false, 
                                ref fileName, remoteUpdate))
                            {
                                txtMasterCreate.Text = Shared.Utilities.FileRead(fileName, false);
                                System.IO.File.Delete(fileName);
                            }
                        }
                        finally
                        {
                            remoteUpdate.OnUpdateDatabase -= DatabaseRemoteUpdate_OnUpdateDatabase;
                            remoteUpdate = null;
                        }
                    }
                    finally
                    {
                        prep = null;
                    }
                }
                finally
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
            finally
            {
                Forms.UpdatingDatabase.HideUpdate();
            }
        }

        #endregion Master Database

        #region Master Create / Drop Scripts


        #endregion Master Create / Drop Scripts

        #region Child Create / Drop Scripts


        #endregion Child Create / Drop Scripts

        private void btnGenerateDropScript_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                ReplicationPrepareChildDatabase prep = new ReplicationPrepareChildDatabase();
                try
                {
                        DatabaseRemoteUpdate remoteUpdate = new DatabaseRemoteUpdate();
                        try
                        {
                            remoteUpdate.OnUpdateDatabase += DatabaseRemoteUpdate_OnUpdateDatabase;

                            string fileName = prep.GenerateTriggerRemoveScript(_connection.ChildDatabase, true, remoteUpdate);
                            txtChildDropScript.Text = Shared.Utilities.FileRead(fileName, false);
                            System.IO.File.Delete(fileName);
                        }
                        finally
                        {
                            remoteUpdate.OnUpdateDatabase -= DatabaseRemoteUpdate_OnUpdateDatabase;
                        }
                }
                finally
                {
                    prep = null;
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void tabPageChildCreateScript_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                ReplicationPrepareChildDatabase prep = new ReplicationPrepareChildDatabase();
                try
                {
                    string fileName = String.Empty;

                        DatabaseRemoteUpdate remoteUpdate = new DatabaseRemoteUpdate();
                        try
                        {
                            remoteUpdate.OnUpdateDatabase += DatabaseRemoteUpdate_OnUpdateDatabase;

                            if (prep.PrepareDatabaseForReplication(_connection.ChildDatabase, 
                                true, true, ref fileName, remoteUpdate))
                            {
                                txtChildCreateScript.Text = Shared.Utilities.FileRead(fileName, false);
                                System.IO.File.Delete(fileName);
                            }
                            else
                            {
                                ShowError("Unable to Generate Schema", "Could not generate schema at this time, please try again later");
                            }
                        }
                        finally
                        {
                            remoteUpdate.OnUpdateDatabase -= DatabaseRemoteUpdate_OnUpdateDatabase;
                        }
                }
                finally
                {
                    prep = null;
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void DatabaseRemoteUpdate_OnUpdateDatabase(object sender, FileProgressArgs e)
        {
            Forms.UpdatingDatabase.Update(e.Total, e.Sent);
        }

        private void btnChildScriptExecute_Click(object sender, EventArgs e)
        {
            Forms.UpdatingDatabase.ShowUpdate();
            try
            {
                Forms.UpdatingDatabase.Update("Building Schema");

                this.Cursor = Cursors.WaitCursor;
                try
                {
                    ReplicationPrepareChildDatabase prep = new ReplicationPrepareChildDatabase();
                    try
                    {
                        string fileName = String.Empty;
                        DatabaseRemoteUpdate remoteUpdate = new DatabaseRemoteUpdate();
                        try
                        {
                            remoteUpdate.OnUpdateDatabase += DatabaseRemoteUpdate_OnUpdateDatabase;

                            if (prep.PrepareDatabaseForReplication(_connection.ChildDatabase, true, 
                                false, ref fileName, remoteUpdate))
                            {
                                txtMasterCreate.Text = Shared.Utilities.FileRead(fileName, false);
                                System.IO.File.Delete(fileName);
                            }
                            else
                            {
                                ShowError("Error Running Script", "An unexpected error occurred whilst executing the script");
                            }
                        }
                        finally
                        {
                            remoteUpdate.OnUpdateDatabase -= DatabaseRemoteUpdate_OnUpdateDatabase;
                        }
                    }
                    finally
                    {
                        prep = null;
                    }
                }
                finally
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
            finally
            {
                Forms.UpdatingDatabase.HideUpdate();
            }
        }

        private void btnChildExecuteRemoveScript_Click(object sender, EventArgs e)
        {
            Forms.UpdatingDatabase.ShowUpdate();
            try
            {
                Forms.UpdatingDatabase.Update("Building Schema");
                this.Cursor = Cursors.WaitCursor;
                try
                {
                    ReplicationPrepareChildDatabase prep = new ReplicationPrepareChildDatabase();
                    try
                    {
                        string fileName = String.Empty;
                        DatabaseRemoteUpdate remoteUpdate = new DatabaseRemoteUpdate();
                        try
                        {
                            remoteUpdate.OnUpdateDatabase += DatabaseRemoteUpdate_OnUpdateDatabase;
                            prep.GenerateTriggerRemoveScript(_connection.ChildDatabase, false, remoteUpdate);
                        }
                        finally
                        {
                            remoteUpdate.OnUpdateDatabase -= DatabaseRemoteUpdate_OnUpdateDatabase;
                            remoteUpdate = null;
                        }
                    }
                    finally
                    {
                        prep = null;
                    }
                }
                finally
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
            finally
            {
                Forms.UpdatingDatabase.HideUpdate();
            }
        }

        private void btnMasterExecuteDropScript_Click(object sender, EventArgs e)
        {
            Forms.UpdatingDatabase.ShowUpdate();
            try
            {
                Forms.UpdatingDatabase.Update("Building Schema");
                this.Cursor = Cursors.WaitCursor;
                try
                {
                    ReplicationPrepareMasterDatabase prep = new ReplicationPrepareMasterDatabase();
                    try
                    {
                        string fileName = String.Empty;
                        DatabaseRemoteUpdate remoteUpdate = new DatabaseRemoteUpdate();
                        try
                        {
                            remoteUpdate.OnUpdateDatabase += DatabaseRemoteUpdate_OnUpdateDatabase;
                            prep.GenerateTriggerRemoveScript(_connection.ChildDatabase, false, remoteUpdate);
                        }
                        finally
                        {
                            remoteUpdate.OnUpdateDatabase -= DatabaseRemoteUpdate_OnUpdateDatabase;
                            remoteUpdate = null;
                        }
                    }
                    finally
                    {
                        prep = null;
                    }
                }
                finally
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
            finally
            {
                Forms.UpdatingDatabase.HideUpdate();
            }
        }

        private void tabPageGenerators_Enter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                API api = new API();
                try
                {
                    _generatorValues = api.GetChildGeneratorValues(_connection);
                    gridGenerators.DataSource = _generatorValues;
                    gridGenerators.Columns[0].Width = 300;
                }
                finally
                {
                    api = null;
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnUpdateGenerators_Click(object sender, EventArgs e)
        {
            if (ShowQuestion("Update Generators", "This will update the selected generator values to the new values, do you wish to continue?"))
            {
                API api = new API();
                try
                {
                    api.SetChildGeneratorValues(_connection, _generatorValues);
                }
                finally
                {
                    api = null;
                }
            }
        }

        //private void btnReplicationSave_Click(object sender, EventArgs e)
        //{
        //    if (_connection.ReplicationType == ReplicationType.NotSet && 
        //        ((rbReplicateChild.Checked && !String.IsNullOrEmpty(txtSiteID.Text))) ||
        //        (rbReplicateMaster.Checked))
        //    {
        //        if (ShowQuestion("Site ID", "Do you want to set the replication type and site ID now?\n\nThis action can not be undone and is required to complete replication."))
        //        {
        //            if (rbReplicateMaster.Checked)
        //                _connection.SiteID = 0;

        //            SaveAllSettings();

        //            API api = new API();
        //            try
        //            {
        //                api.SetSiteID(_connection);
        //            }
        //            finally
        //            {
        //                api = null;
        //            }

        //            cbOptionReplicate_CheckedChanged(sender, e);
        //            LoadData();
        //        }
        //    }
        //}

        #region Replicated Tables

        private void lvTables_DoubleClick(object sender, EventArgs e)
        {
            if (lvTables.SelectedItems.Count > 0)
            {
                if (lvTables.SelectedItems[0].ForeColor == Color.Red)
                {
                    ShowError("Replicate Table", "This table can not be replicated!");
                    return;
                }

                if (lvTables.SelectedItems[0].SubItems[1].Text == "No")
                    pumTablesAdd_Click(sender, e);
                else
                    pumTablesConfigure_Click(sender, e);
            }
        }

        private void lvTables_ToolTipShow(object sender, ToolTipEventArgs e)
        {
            e.AllowShow = true;

            if (_isValidating)
            {
                e.Text = "Validating, please wait...";
            }
            else
            {
                if (e.ListViewItem != null)
                {
                    if (e.ListViewItem.ForeColor == Color.Red)
                    {
                        e.Title = "Schema Info";
                        e.Icon = ToolTipIcon.Error;
                        e.Text = e.ListViewItem.Text == null ? "Mismatch" : (string)e.ListViewItem.Tag;
                    }
                    else if (e.ListViewItem.ForeColor == Color.DarkGreen)
                    {
                        e.Title = "Schema Info";
                        e.Icon = ToolTipIcon.Warning;
                        e.Text = e.ListViewItem.Text == null ? "Mismatch" : (string)e.ListViewItem.Tag;
                    }
                    else if (e.ListViewItem.ForeColor == Color.DarkMagenta)
                    {
                        e.Title = "Schema Info";
                        e.Icon = ToolTipIcon.Warning;
                        e.Text = "Table not correctly setup";
                    }
                    else if (e.ListViewItem.ForeColor == Color.DarkOrange)
                    {
                        e.Title = "Schema Info";
                        e.Icon = ToolTipIcon.Warning;
                        e.Text = "Master is replicating, child is not replicating";
                    }
                    else if (e.ListViewItem.ForeColor == Color.Cyan)
                    {
                        e.Title = "Schema Info";
                        e.Icon = ToolTipIcon.Warning;
                        e.Text = "Child is replicating, Master is not replicating";
                    }
                }
            }
        }

        #endregion Replication Tables

        #region Generic Methods

        private void RunScript(string connectionString, string script)
        {
            Forms.UpdatingDatabase.ShowUpdate();
            try
            {
                Forms.UpdatingDatabase.Update("Running Script");
                this.Cursor = Cursors.WaitCursor;
                try
                {
                    ReplicationPrepareChildDatabase prep = new ReplicationPrepareChildDatabase();
                    try
                    {
                        DatabaseRemoteUpdate remoteUpdate = new DatabaseRemoteUpdate();
                        try
                        {
                            remoteUpdate.OnUpdateDatabase += DatabaseRemoteUpdate_OnUpdateDatabase;
                            remoteUpdate.DatabaseRunScript(connectionString, script);
                        }
                        finally
                        {
                            remoteUpdate.OnUpdateDatabase -= DatabaseRemoteUpdate_OnUpdateDatabase;
                            remoteUpdate = null;
                        }
                    }
                    finally
                    {
                        prep = null;
                    }
                }
                finally
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
            finally
            {
                Forms.UpdatingDatabase.HideUpdate();
            }
        }

        private void LoadData()
        {
            _connection.LoadAllTables();
            LoadTableData();
            LoadAutoCorrectRules();
        }

        private void LoadAutoCorrectRules()
        {
            if (_connection.Rules == null)
                return;

            this.Cursor = Cursors.WaitCursor;
            lvAutoCorrect.BeginUpdate();
            try
            {
                lvAutoCorrect.Items.Clear();

                foreach (AutoCorrectRule rule in _connection.Rules)
                {
                    ListViewItem item = new ListViewItem(rule.TableName);
                    item.Tag = rule;
                    item.SubItems.Add(rule.KeyName);
                    item.SubItems.Add(rule.Options.ToString());
                    lvAutoCorrect.Items.Add(item);
                }
            }
            finally
            {
                lvAutoCorrect.EndUpdate();
                this.Cursor = Cursors.Arrow;
            }
        }

        private void LoadTableData()
        {
            this.Cursor = Cursors.WaitCursor;
            API api = new API();
            lvTables.BeginUpdate();
            try
            {
                lvTables.Items.Clear();

                foreach (string table in _connection.ChildTables())
                {
                    ListViewItem item = new ListViewItem(table);
                    bool masterIsReplicating = _connection.MasterTableIsReplicated(table);

                    string pkType = _connection.PrimaryKeyType(table);

                    if ((pkType == "BIGINT" || pkType == "INT" || pkType == "Unknown") && 
                        (_connection.ChildPrimaryKeys().ContainsKey(table) && _connection.ChildPrimaryKeys()[table].Count > 0))
                    {
                        item.ForeColor = Color.Gray;
                        bool isReplicating = _connection.ChildTableIsReplicated(table);
                        
                        if (isReplicating)
                        {
                            item.SubItems.Add("Yes");
                        }
                        else
                        {
                            item.SubItems.Add("No");
                        }

                        if (_connection.ReplicationType == ReplicationType.Child)
                        {
                            item.SubItems.Add(masterIsReplicating ? "Yes" : "No");

                            if (isReplicating)
                                item.SubItems.Add(_connection.ChildTableSortOrder(table).ToString());
                            else
                                item.SubItems.Add("");
                        }
                    }
                    else
                    {
                        item.ForeColor = Color.Red;
                        item.Font = new Font(item.Font, FontStyle.Strikeout);
                        item.SubItems.Add("No");

                        if (_connection.ReplicationType == ReplicationType.Child)
                        {
                            item.SubItems.Add("No");
                            item.SubItems.Add("");
                        }

                        if (pkType == "OTHER")
                            item.Tag = "Table can not be included as primary key is not an integer or bigint";
                        else
                            item.Tag = "Table can not be included in replication as there is no Primary Key!";
                    }

                    lvTables.Items.Add(item);
                }
            }
            finally
            {
                api = null;
                lvTables.EndUpdate();
                this.Cursor = Cursors.Arrow;
            }

            ValidateSchema();
        }

        #endregion Generic Methods

        #region Popup Menu Auto Correct

        private void pumAutoCorrect_Opening(object sender, CancelEventArgs e)
        {
            if (lvAutoCorrect.SelectedItems.Count > 0)
            {
                pumAutoCorrectDelete.Enabled = true;
                pumAutoCorrectEdit.Enabled = true;
            }
            else
            {
                pumAutoCorrectDelete.Enabled = false;
                pumAutoCorrectEdit.Enabled = false;
            }
        }

        private void pumAutoCorrectAdd_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                AutoCorrectEdit frm = new AutoCorrectEdit();
                try
                {
                    frm.Connection = _connection;

                    if (frm.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        _connection.Rules.Add(frm.Rule);
                        LoadAutoCorrectRules();
                    }
                }
                finally
                {
                    frm.Close();
                    frm.Dispose();
                    frm = null;
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }

        }

        private void pumAutoCorrectDelete_Click(object sender, EventArgs e)
        {
            if (lvAutoCorrect.SelectedItems.Count == 0)
                return;

            if (ShowQuestion("Delete Rule", "Are you sure you want to delete the selected rule?"))
            {
                API api = new API();
                try
                {
                    api.DeleteRule(_connection, (AutoCorrectRule)lvAutoCorrect.SelectedItems[0].Tag);
                    _connection.Rules.Remove((AutoCorrectRule)lvAutoCorrect.SelectedItems[0].Tag);
                    lvAutoCorrect.Items.Remove(lvAutoCorrect.SelectedItems[0]);
                }
                finally
                {
                    api = null;
                }
            }
        }

        private void pumAutoCorrectRefresh_Click(object sender, EventArgs e)
        {
            LoadAutoCorrectRules();
        }

        #endregion Popup Menu Auto Correct

        #region Auto Correct

        private void lvAutoCorrect_DoubleClick(object sender, EventArgs e)
        {
            if (lvAutoCorrect.SelectedItems.Count == 0)
                return;

            this.Cursor = Cursors.WaitCursor;
            try
            {
                AutoCorrectEdit frm = new AutoCorrectEdit();
                try
                {
                    frm.Connection = _connection;
                    frm.Rule = (AutoCorrectRule)lvAutoCorrect.SelectedItems[0].Tag;

                    if (frm.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        LoadAutoCorrectRules();
                }
                finally
                {
                    frm.Close();
                    frm.Dispose();
                    frm = null;
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        #endregion Auto Correct

        private void cbBackupAfter_CheckedChanged(object sender, EventArgs e)
        {
            dtpBackupTime.Enabled = cbBackupAfter.Checked;
        }

        private void cbForceHours_CheckedChanged(object sender, EventArgs e)
        {
            dtpVerifyAfter.Enabled = cbForceHours.Checked;
            lblVerifyAfterHours.Enabled = cbForceHours.Checked;
        }

        private void cbForceVerification_CheckedChanged(object sender, EventArgs e)
        {
            cbRequireUniqueAcess.Enabled = cbForceVerification.Checked;
            dtpVerifyAfter.Enabled = cbForceVerification.Checked;
            cbForceHours.Enabled = cbForceVerification.Checked;
            dtpVerifyAfter.Enabled = cbForceVerification.Checked;
            udVerifyAll.Enabled = cbForceVerification.Checked;
            lblVerifyAfterIterations.Enabled = cbForceVerification.Checked;
            lblVerifyAfterIterationsIt.Enabled = cbForceVerification.Checked;
            lblVerifyAfterHours.Enabled = cbForceVerification.Checked;
        }

        #endregion Private Methods
    }
}
