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
 *  Purpose:  Edit Auto Correct Settings
 *
 */

using System;
using System.Collections.Generic;

using Replication.Engine;
using Replication.Engine.Classes;

namespace Replication.Service.Forms
{
    public partial class AutoCorrectEdit : SharedControls.Forms.BaseForm
    {
        #region Private Members

        private DatabaseConnection _connection;
        private AutoCorrectRule _rule;

        #endregion Private Members

        #region Constructors

        public AutoCorrectEdit()
        {
            InitializeComponent();

            LoadOptions();
        }

        #endregion Constructors

        #region Properties

        public DatabaseConnection Connection 
        { 
            set
            {
                _connection = value;
                LoadTables();
            }
        }

        public AutoCorrectRule Rule
        {
            set
            {
                _rule = value;

                for (int i = 0; i < cmbTables.Items.Count; i++)
                {
                    string table = (string)cmbTables.Items[i];

                    if (table == _rule.TableName)
                    {
                        cmbTables.SelectedIndex = i;
                        break;
                    }
                }

                // set options
                for (int i = 0; i < cmbOptions.Items.Count; i++ )
                {
                    AutoFixOptions option = (AutoFixOptions)cmbOptions.Items[i];

                    if (option == _rule.Options)
                    {
                        cmbOptions.SelectedIndex = i;
                        break;
                    }
                }

                txtSelectSQL.Text = _rule.SQLRuleLocal;
                txtUpdateSQL.Text = _rule.SQLRuleRemote;
            }

            get
            {
                return (_rule);
            }
        }

        #endregion Properties

        #region Private Methods

        private void LoadTables()
        {
            cmbTables.Items.Clear();

            foreach (string table in _connection.ChildTables())
            {
                if (_connection.ChildTableIsReplicated(table))
                    cmbTables.Items.Add(table);
            }

            if (cmbTables.SelectedIndex == -1)
                cmbTables.SelectedIndex = 0;
        }

        private void cmbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbKeys.Items.Clear();
            cmbTargetColumn.Items.Clear();
            lstAvailable.Items.Clear();
            lstSelected.Items.Clear();

            API api = new API();
            try
            {
                List<string> columns = api.GetChildTableColumns(_connection, (string)cmbTables.SelectedItem);

                for (int i = 0; i < columns.Count; i++)
                {
                    cmbTargetColumn.Items.Add(columns[i].Trim());

                    if (_rule != null && _rule.TargetColumn == columns[i])
                    {
                        cmbTargetColumn.SelectedIndex = i;
                    }


                    if (_rule != null && !String.IsNullOrEmpty(_rule.Dependencies) && !_rule.Dependencies.Contains(columns[i]))
                    {
                        lstAvailable.Items.Add(columns[i].Trim());
                    }
                    else
                    {
                        lstAvailable.Items.Add(columns[i].Trim());
                    }
                }

                if (_rule != null && !String.IsNullOrEmpty(_rule.ReplicateName))
                {
                    lstSelected.Items.Clear();
                    string[] selected = _rule.ReplicateName.Split(',');

                    foreach (string s in selected)
                    {
                        lstSelected.Items.Add(s.Trim());
                        lstAvailable.Items.Remove(s.Trim());
                    }
                }

                List<string> childKeys = api.ChildKeys(_connection, (string)cmbTables.SelectedItem);

                for (int i = 0; i < childKeys.Count; i++)
                {
                    cmbKeys.Items.Add(childKeys[i]);

                    if (_rule != null && _rule.KeyName == childKeys[i])
                    {
                        cmbKeys.SelectedIndex = i;
                    }
                }

                int idx = cmbKeys.Items.Add("overflow, or string truncation");

                if (_rule != null && _rule.KeyName == (string)cmbKeys.Items[idx])
                {
                    cmbKeys.SelectedIndex = idx;
                }

                if (cmbKeys.SelectedIndex == -1)
                    cmbKeys.SelectedIndex = 0;
            }
            finally
            {
                api = null;
            }
        }

        private void LoadOptions()
        {
            cmbOptions.Items.Clear();

            foreach (AutoFixOptions option in Enum.GetValues(typeof(AutoFixOptions)))
            {
                cmbOptions.Items.Add(option);
            }

            cmbOptions.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (lstAvailable.SelectedIndex == -1)
                return;

            string col = (string)lstAvailable.SelectedItem;

            lstSelected.Items.Add(col);
            lstAvailable.Items.Remove(col);
            RebuildSQL();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstSelected.SelectedIndex == -1)
                return;

            string col = (string)lstSelected.SelectedItem;

            lstAvailable.Items.Add(col);
            lstSelected.Items.Remove(col);
            RebuildSQL();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {

        }

        private void cmbOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            // update other options based on selection
            AutoFixOptions option = (AutoFixOptions)cmbOptions.SelectedItem;

            switch (option)
            {
                case AutoFixOptions.AttemptIDRemote:
                case AutoFixOptions.AttemptIDLocal:
                    lblSelectSQL.Visible = true;
                    lblUpdateSQL.Visible = true;
                    txtSelectSQL.Visible = true;
                    txtUpdateSQL.Visible = true;
                    lblAvailableColumns.Visible = true;
                    lblSelectedColumns.Visible = true;
                    lstAvailable.Visible = true;
                    lstSelected.Visible = true;
                    btnAdd.Visible = true;
                    btnRemove.Visible = true;
                    lblTargetColumn.Visible = true;
                    cmbTargetColumn.Visible = true;
                    this.Height = 500;

                    break;

                case AutoFixOptions.AppendExtraChar:
                    lblAvailableColumns.Visible = false;
                    lblSelectedColumns.Visible = false;
                    lstAvailable.Visible = false;
                    lstSelected.Visible = false;
                    btnAdd.Visible = false;
                    btnRemove.Visible = false;
                    lblSelectSQL.Visible = false;
                    lblUpdateSQL.Visible = true;
                    txtSelectSQL.Visible = false;
                    txtUpdateSQL.Visible = true;
                    lblTargetColumn.Visible = true;
                    cmbTargetColumn.Visible = true;
                    this.Height = 270;

                    break;

                case AutoFixOptions.IgnoreRecord:
                default:
                    lblAvailableColumns.Visible = false;
                    lblSelectedColumns.Visible = false;
                    lstAvailable.Visible = false;
                    lstSelected.Visible = false;
                    btnAdd.Visible = false;
                    btnRemove.Visible = false;
                    lblSelectSQL.Visible = false;
                    lblUpdateSQL.Visible = false;
                    txtSelectSQL.Visible = false;
                    txtUpdateSQL.Visible = false;
                    lblTargetColumn.Visible = false;
                    cmbTargetColumn.Visible = false;
                    this.Height = 200;

                    break;
            }
        }

        private void cmbTargetColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            RebuildSQL();
        }

        private void RebuildSQL()
        {
            // update other options based on selection
            AutoFixOptions option = (AutoFixOptions)cmbOptions.SelectedItem;

            switch (option)
            {
                case AutoFixOptions.AttemptIDRemote:
                case AutoFixOptions.AttemptIDLocal:
                    if (lstSelected.Items.Count < 1)
                    {
                        txtUpdateSQL.Text = "";
                        txtSelectSQL.Text = "";
                    }
                    else
                    {
                        txtSelectSQL.Text = "SELECT " + cmbTargetColumn.SelectedItem;
                        txtUpdateSQL.Text = "UPDATE " + cmbTables.SelectedItem + " SET " + cmbTargetColumn.SelectedItem + " = @PARAM0 WHERE ";
                        string selectSQL = "SELECT ";
                        bool first = true;

                        for (int i = 0; i < lstSelected.Items.Count; i++)
                        {
                            if (first)
                            {
                                selectSQL += lstSelected.Items[i];
                                first = false;
                            }
                            else
                            {
                                selectSQL += ", ";
                                selectSQL += lstSelected.Items[i];
                                txtUpdateSQL.Text += " AND ";
                            }

                            txtSelectSQL.Text += ", " + lstSelected.Items[i];
                            int paramCol = i + 1;
                            txtUpdateSQL.Text += lstSelected.Items[i] + " = @PARAM" + paramCol.ToString();
                        }

                        selectSQL += " FROM " + cmbTables.SelectedItem + " WHERE " + lstSelected.Items[0] + " = @PARAM0);";
                        txtSelectSQL.Text += " FROM " + cmbTables.SelectedItem + " WHERE " + lstSelected.Items[0] + " IN (" + selectSQL;
                        
                        if (!String.IsNullOrEmpty(txtUpdateSQL.Text))
                            txtUpdateSQL.Text += ";";
                    }

                    break;

                case AutoFixOptions.AppendExtraChar:
                    txtUpdateSQL.Text = String.Format("UPDATE {0} SET {1} = {1} || '~rf' WHERE {1} = @PARAM0",
                        cmbTables.SelectedItem, cmbTargetColumn.SelectedItem);
                    txtSelectSQL.Text = txtUpdateSQL.Text;
                    break;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbTables.SelectedIndex == -1)
            {
                ShowError("Error", "Please select a table.");
                cmbTables.DroppedDown = true;
                return;
            }

            if (cmbKeys.SelectedIndex == -1)
            {
                ShowError("Error", "Please select a key or error message.");
                cmbKeys.DroppedDown = true;
                return;
            }

            if (cmbOptions.SelectedIndex == -1)
            {
                ShowError("Error", "Please select an option.");
                cmbOptions.DroppedDown = true;
                return;
            }

            AutoFixOptions option = (AutoFixOptions)cmbOptions.SelectedItem;

            if (_rule == null)
            {
                _rule = new AutoCorrectRule();
                _rule.SQLRuleRemote = String.Empty;
                _rule.SQLRuleLocal = String.Empty;
                _rule.Dependencies = String.Empty;
                _rule.ReplicateName = String.Empty;
            }

            _rule.TableName = (string)cmbTables.SelectedItem;
            _rule.KeyName = (string)cmbKeys.SelectedItem;
            _rule.TargetTable = _rule.TableName;
            _rule.Options = option;

            API api = new API();
            try
            {
                if (option == AutoFixOptions.IgnoreRecord)
                {
                    api.AutoCorrectRuleAdd(_connection, _rule);
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    return;
                }

                if (cmbTargetColumn.SelectedIndex == -1)
                {
                    ShowError("Error", "Please select the target column.");
                    cmbTargetColumn.DroppedDown = true;
                    return;
                }

                _rule.TargetColumn = (string)cmbTargetColumn.SelectedItem;
                _rule.SQLRuleLocal = txtSelectSQL.Text;
                _rule.SQLRuleRemote = txtUpdateSQL.Text;

                if (String.IsNullOrEmpty(txtUpdateSQL.Text))
                {
                    ShowError("Error", "Please enter the update SQL for this rule.");
                    return;
                }

                if (option == AutoFixOptions.AppendExtraChar)
                {
                    api.AutoCorrectRuleAdd(_connection, _rule);
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    return;
                }

                if (lstSelected.Items.Count == 0)
                {
                    ShowError("Error", "Please select the columns which will be validated and updated.");
                    return;
                }

                if (String.IsNullOrEmpty(txtSelectSQL.Text))
                {
                    ShowError("Error", "Please enter the select SQL for this rule.");
                    return;
                }

                // fix id requires the columns to be set
                _rule.ReplicateName = String.Empty;
                
                foreach (string s in lstSelected.Items)
                {
                    if (!String.IsNullOrEmpty(_rule.ReplicateName))
                        _rule.ReplicateName += ",";

                    _rule.ReplicateName += s;
                }

                api.AutoCorrectRuleAdd(_connection, _rule);
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                return;
            }
            finally
            {
                api = null;
            }
        }

        #endregion Private Methods
    }
}
