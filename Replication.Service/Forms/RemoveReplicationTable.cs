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
 *  Purpose:  Stop Replicating Tables
 *
 */

using System.Windows.Forms;

namespace Replication.Service.Forms
{
    public partial class RemoveReplicationTable : Form
    {
        #region Constructors

        public RemoveReplicationTable()
        {
            InitializeComponent();
        }

        public RemoveReplicationTable(bool masterEnabled)
            : this()
        {
            cbRemoveMaster.Enabled = masterEnabled;
            cbRemoveMaster.Checked = masterEnabled;
        }

        #endregion Constructors

        #region Properties

        public bool MasterChecked
        {
            get
            {
                return (cbRemoveMaster.Checked);
            }
        }

        #endregion Properties

        #region Static Methods

        public static bool Show(ref bool removeMaster)
        {
            RemoveReplicationTable frm = new RemoveReplicationTable(removeMaster);
            try
            {
                bool Result = frm.ShowDialog() == DialogResult.Yes;

                if (Result)
                    removeMaster = frm.MasterChecked;

                return (Result);
            }
            finally
            {
                frm.Close();
                frm.Dispose();
                frm = null;
            }
        }

        #endregion Static Methods
    }
}
