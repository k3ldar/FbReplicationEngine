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
 *  Purpose:  Configuration Form
 *
 */

using System;
using System.IO;
using System.Windows.Forms;

using Shared;

namespace Replication.Service.Forms
{
    public partial class Configuration : SharedControls.Forms.BaseForm
    {
        #region Constants

        internal const string ENCRYPRION_KEY = "Pad'sfkqw;oe8u;ldkfn cma;lkdfjadjfjxm";

        #endregion Constants

        #region Constructors

        public Configuration()
        {
            InitializeComponent();

            LoadConfigurationFiles();
        }

        #endregion Constructors

        #region Private Methods

        private void LoadConfigurationFiles()
        {
            tvConfigurationFiles.Nodes.Clear();

            API api = new API(Utilities.AddTrailingBackSlash(Utilities.CurrentPath(true) + "Config"), ENCRYPRION_KEY);
            try
            {
                int maxAvailable = int.MaxValue;

                foreach (ConfigFileNode fileNode in api.GetConfigurationSettings())
                {
                    TreeNode node = new TreeNode(fileNode.Connection.Name);
                    node.Tag = fileNode;
                    tvConfigurationFiles.Nodes.Add(node);
                }
            }
            catch (Exception err)
            {
                ShowError("Error", String.Format("{0}\r\n\r\n{1}", err.Message, Path.GetTempPath()));
            }
            finally
            {
                api = null;
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            DatabaseConnection connection = new DatabaseConnection();

            if (ConfigureChild.ShowChildSettings(connection))
            {
                TreeNode node = new TreeNode(connection.Name);
                ConfigFileNode fileNode = new ConfigFileNode();
                fileNode.Connection = connection;

                API api = new API(Utilities.AddTrailingBackSlash(Utilities.CurrentPath(true) + "Config"), ENCRYPRION_KEY);
                try
                {
                    fileNode.FileName = api.GetConfigurationFileName(fileNode.Connection);
                    string newUpdateFile = System.IO.Path.GetTempFileName();
                    Shared.Utilities.FileWrite(newUpdateFile,
                        String.Format("ADD@{0}", fileNode.FileName));
                    File.Move(newUpdateFile, Utilities.AddTrailingBackSlash(Utilities.CurrentPath(true) + "Config\\") +
                        Path.GetFileName(newUpdateFile));
                }
                catch (Exception err)
                {
                    ShowError("Error", err.Message);
                    return;
                }
                finally
                {
                    api = null;
                }

                node.Tag = fileNode;
                DatabaseConnection.Save(connection, fileNode.FileName, ENCRYPRION_KEY);
                tvConfigurationFiles.Nodes.Add(node);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (tvConfigurationFiles.SelectedNode == null)
                return;

            ConfigFileNode fileNode = (ConfigFileNode)tvConfigurationFiles.SelectedNode.Tag;

            if (ShowQuestion("Remove Database", String.Format("Are you sure you want to remove \"{0}\"?", fileNode.Connection.Name)))
            {
                System.IO.File.Delete(fileNode.FileName);
                tvConfigurationFiles.Nodes.Remove(tvConfigurationFiles.SelectedNode);

                string newUpdateFile = System.IO.Path.GetTempFileName();
                Shared.Utilities.FileWrite(newUpdateFile,
                    String.Format("DELETE@{0}", fileNode.FileName));
                File.Move(newUpdateFile, Utilities.AddTrailingBackSlash(Utilities.CurrentPath(true) + "Config\\") +
                    Path.GetFileName(newUpdateFile));
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (tvConfigurationFiles.SelectedNode == null)
                return;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                ConfigFileNode fileNode = (ConfigFileNode)tvConfigurationFiles.SelectedNode.Tag;

                if (ConfigureChild.ShowChildSettings(fileNode.Connection))
                {
                    DatabaseConnection.Save(fileNode.Connection, fileNode.FileName, ENCRYPRION_KEY);
                    tvConfigurationFiles.SelectedNode.Text = fileNode.Connection.Name;

                    string newUpdateFile = System.IO.Path.GetTempFileName();
                    Shared.Utilities.FileWrite(newUpdateFile,
                        String.Format("CHANGED@{0}", fileNode.FileName));
                    File.Move(newUpdateFile, Utilities.AddTrailingBackSlash(Utilities.CurrentPath(true) + "Config\\") +
                        Path.GetFileName(newUpdateFile));

                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            AboutForm frm = new AboutForm();
            try
            {
                frm.ShowDialog(this);
            }
            finally
            {
                frm.Dispose();
                frm = null;
            }
        }

        #endregion Private Methods
    }
}
