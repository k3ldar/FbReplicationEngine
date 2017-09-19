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
 *  Copyright (c) 2011 - 2017 Simon Carter.  All Rights Reserved
 *
 *  Purpose:  Replication Table Configuration
 *
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Replication.Engine;

namespace Replication.Service.Forms
{
    public partial class ConfigureReplicatedTable : SharedControls.Forms.BaseForm
    {
        #region Private Members

        private API _api;
        private DatabaseConnection _connection;
        private List<ReplicatedTable> _replicatedTables;

        #endregion Private Members

        #region Constructors

        public ConfigureReplicatedTable()
        {
            InitializeComponent();

#if LogRowData
            cbLogRowData.Visible = true;
#else
            cbLogRowData.Visible = false;
#endif
        }

        public ConfigureReplicatedTable(API api, DatabaseConnection connection, string tableName)
            : this()
        {
            txtTableName.Text = tableName;
            _api = api;
            _connection = connection;
            _replicatedTables = api.GetChildTableReplicatedTable(connection, tableName);
            cbUpdateMaster.Checked = false;

            LoadLocalGenerators();

            switch (connection.ReplicationType)
            {
                case ReplicationType.Master:
                    cmbRemoteGenerator.Enabled = false;
                    label1.Enabled = false;
                    lblGenerator.Enabled = false;
                    cmbLocalGenerator.Enabled = false;
                    label2.Enabled = false;
                    udSortOrder.Enabled = false;
                    udSortOrder.Value = 0;
                    break;
                case ReplicationType.Child:
                    LoadRemoteGenerators();
                    break;
            }

            cbUpdateMaster.Checked = false;
            cbUpdateMaster.Enabled = false;

            LoadOtherData();

            LoadColumnNames();

            if (_replicatedTables.Count > 0)
            {
#if LogRowData
                cbLogRowData.Checked = _replicatedTables[0].Options.HasFlag(TableOptions.LogRowData);
#endif
                rbAscending.Checked = _replicatedTables[0].Options.HasFlag(TableOptions.Ascending);
                rbDescending.Checked = !rbAscending.Checked;
                cbVerify.Checked = !_replicatedTables[0].Options.HasFlag(TableOptions.DoNotVerify);
                cbVerifyChild.Checked = !_replicatedTables[0].Options.HasFlag(TableOptions.DoNotVerifyChild);
                cbVerifyMaster.Checked = !_replicatedTables[0].Options.HasFlag(TableOptions.DoNotVerifyMaster);
            }
            else
            {
                cbLogRowData.Checked = false;
                rbDescending.Checked = true;
                cbVerify.Checked = true;
                cbVerifyChild.Checked = true;
                cbVerifyMaster.Checked = true;
            }
        }

#endregion Constructors

#region Static Methods

        public static bool Show(API api, DatabaseConnection connection, string tableName)
        {
            ConfigureReplicatedTable frm = new ConfigureReplicatedTable(api, connection, tableName);
            try
            {
                return (frm.ShowDialog() == DialogResult.OK);
            }
            finally
            {
                frm.Close();
                frm.Dispose();
                frm = null;
            }
        }

#endregion Static Methods

#region Private Methods

        private void LoadOtherData()
        {
            bool found = false;

            foreach (ReplicatedTable table in _replicatedTables)
            {
                if (!found)
                    found = true;

                if (table.Operation == Operation.Insert)
                    cbInsert.Checked = true;
                else if (table.Operation == Operation.Delete)
                    cbDelete.Checked = true;
                else if (table.Operation == Operation.Update)
                    cbUpdate.Checked = true;
            }

            if (found)
            {
                txtTriggerName.Text = _replicatedTables[0].TriggerName;

                if (_connection.ReplicationType == ReplicationType.Child)
                    udSortOrder.Value = _replicatedTables[0].SortOrder;

                switch (_replicatedTables[0].IndiceType)
                {
                    case 1:
                        rbForeignKey.Checked = true;
                        break;
                    case 2:
                        rbMultipleColumns.Checked = true;
                        break;
                    default:
                        rbSingleColumn.Checked = true;
                        break;
                }
            }
            else
            {
                txtTriggerName.Text = txtTableName.Text;
            }
        }

        private void LoadLocalGenerators()
        {
            cmbLocalGenerator.Items.Clear();
            cmbLocalGenerator.Items.Add("Use Child Generated ID");
            string localGen = String.Empty;

            foreach (ReplicatedTable table in _replicatedTables)
            {
                if (String.IsNullOrEmpty(table.LocalGenerator))
                {
                    continue;
                }

                localGen = table.LocalGenerator;
                break;
            }

            List<string> generators = _api.GetChildGenerators(_connection);

            foreach (string generator in generators)
            {
                int idx = cmbLocalGenerator.Items.Add(generator);

                if (generator == localGen)
                    cmbLocalGenerator.SelectedIndex = idx;
            }

            if (cmbLocalGenerator.SelectedIndex == -1)
                cmbLocalGenerator.SelectedIndex = 0;
        }

        private void LoadRemoteGenerators()
        {
            cmbRemoteGenerator.Items.Clear();
            cmbRemoteGenerator.Items.Add("Use Child Generated ID");
            string remoteGen = String.Empty;

            foreach (ReplicatedTable table in _replicatedTables)
            {
                if (String.IsNullOrEmpty(table.RemoteGenerator))
                {
                    continue;
                }

                remoteGen = table.RemoteGenerator;
                break;
            }

            List<string> generators = _api.GetMasterGenerators(_connection);

            foreach (string generator in generators)
            {
                int idx = cmbRemoteGenerator.Items.Add(generator);

                if (generator == remoteGen)
                    cmbRemoteGenerator.SelectedIndex = idx;
            }

            if (cmbRemoteGenerator.SelectedIndex == -1)
                cmbRemoteGenerator.SelectedIndex = 0;
        }

        private void LoadColumnNames()
        {
            cmbLocalIDColumn.Items.Clear();
            clbColumns.Items.Clear();
            string exclude = String.Empty;
            string localID = String.Empty;

            foreach (ReplicatedTable table in _replicatedTables)
            {
                if (String.IsNullOrEmpty(localID) && !String.IsNullOrEmpty(table.IDColumn))
                    localID = table.IDColumn;

                if (String.IsNullOrEmpty(table.ExcludeFields))
                    continue;

                string excluded = table.ExcludeFields;

                if (!excluded.EndsWith(":"))
                    excluded += ":";

                if (exclude != excluded)
                {
                    exclude += table.ExcludeFields;

                    if (!exclude.EndsWith(":"))
                        exclude += ":";
                }
            }

            List<string> columns = _api.GetChildTableColumns(_connection, txtTableName.Text);

            foreach (string col in columns)
            {
                int index = cmbLocalIDColumn.Items.Add(col);

                if (col == localID)
                    cmbLocalIDColumn.SelectedIndex = index;
            }

            foreach (string column in columns)
            {
                int idx = clbColumns.Items.Add(column);

                clbColumns.SetItemChecked(idx, !exclude.Contains(String.Format("{0}:", column)));
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(txtTriggerName.Text))
                    throw new Exception("Trigger Name can not be blank");

                if (!cbDelete.Checked && !cbInsert.Checked && !cbUpdate.Checked)
                    throw new Exception("At least one of the log changes must be checked");

                if (clbColumns.CheckedItems.Count == 0)
                    throw new Exception("You must specify at least 1 column that will be replicated");

                if (!rbForeignKey.Checked && !rbMultipleColumns.Checked && !rbSingleColumn.Checked)
                    throw new Exception("Please specify the index type");

                if (_connection.ReplicationType == ReplicationType.Child && cmbRemoteGenerator.SelectedIndex == -1 && !rbForeignKey.Checked)
                    throw new Exception("Please select the remote generator");

                if (cmbLocalIDColumn.SelectedIndex == -1 && !rbMultipleColumns.Checked)
                    throw new Exception("Local ID Column (Primary Key) must be selected");

                //if (cmbLocalGenerator.SelectedIndex == -1 && rbSingleColumn.Checked)
                //    throw new Exception("Local Generator must be selected");

                string excludeColumns = String.Empty;

                for (int i = 0; i < clbColumns.Items.Count; i++)
                {
                    if (!clbColumns.GetItemChecked(i))
                    {
                        excludeColumns += clbColumns.Items[i] + ":";
                    }
                }

                int indiceType = 0;

                if (rbForeignKey.Checked)
                    indiceType = 1;
                else if (rbMultipleColumns.Checked)
                    indiceType = 2;

                string remoteGen = String.Empty;

                if (_connection.ReplicationType == ReplicationType.Child)
                {
                    if (cmbRemoteGenerator.SelectedIndex > 0)
                        remoteGen = (string)cmbRemoteGenerator.Items[cmbRemoteGenerator.SelectedIndex];
                }

                string localGen = String.Empty;

                if (cmbLocalGenerator.SelectedIndex > 0)
                    localGen = (string)cmbLocalGenerator.Items[cmbLocalGenerator.SelectedIndex];

                string localID = String.Empty;

                if (cmbLocalIDColumn.SelectedIndex > -1)
                    localID = (string)cmbLocalIDColumn.Items[cmbLocalIDColumn.SelectedIndex];

                TableOptions options = TableOptions.None;

                if (!cbVerify.Checked)
                    options |= TableOptions.DoNotVerify;

                if (rbAscending.Checked)
                    options |= TableOptions.Ascending;

                if (!cbVerifyChild.Checked)
                    options |= TableOptions.DoNotVerifyChild;

                if (!cbVerifyMaster.Checked)
                    options |= TableOptions.DoNotVerifyMaster;

#if LogRowData
                if (cbLogRowData.Checked)
                    options |= TableOptions.LogRowData;
#endif


                _api.ChildUpdateReplicatedTables(_connection, txtTableName.Text.Trim(), localID, 
                    cbInsert.Checked, cbUpdate.Checked, cbDelete.Checked, txtTriggerName.Text.Trim(),
                    excludeColumns, (int)udSortOrder.Value, indiceType, localGen,
                    remoteGen,
                    cbUpdateMaster.Checked, options);

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception err)
            {
                if (err.Message.Contains("UNQ_REPLICATE$TABLES"))
                    ShowError("Trigger Name", "Trigger name is not unique, please change it to a unique value");
                else
                    ShowError("Error", err.Message);
            }
        }

        private void cmbLocalIDColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLocalIDColumn.SelectedIndex == -1)
                return;

            RadioButton selectNew = null;
            string column = (string)cmbLocalIDColumn.Items[cmbLocalIDColumn.SelectedIndex];

            if (_connection.ColumnIsForeignKey(txtTableName.Text, column))
            {
                selectNew = rbForeignKey;
            }
            else
            { 
                int count = 0;
                if (_connection.ColumnIsPrimaryKey(txtTableName.Text, column, out count))
                {
                    if (count > 1)
                        selectNew = rbMultipleColumns;
                    else
                        selectNew = rbSingleColumn;
                }
                else
                {
                    selectNew = rbSingleColumn;
                }
            }

            if (selectNew != null)
            {
                if (!selectNew.Checked && ShowQuestion("Change Primary Key Type", 
                    String.Format("The primary key type should be {0}\r\n\r\nWould you like it to be changed?", selectNew.Text)))
                {
                    selectNew.Checked = true;
                }
            }
        }

#endregion Private Methods

        private void ConfigureReplicatedTable_Shown(object sender, EventArgs e)
        {
            cmbLocalIDColumn.SelectedIndexChanged += new System.EventHandler(this.cmbLocalIDColumn_SelectedIndexChanged);
            cmbLocalIDColumn_SelectedIndexChanged(this, e);
        }
    }
}
