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
 *  Purpose:  About Form
 *
 */

using System;
using System.Windows.Forms;

namespace Replication.Service.Forms
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            lblVersion.Text = String.Format("{0}: {1}",
                 Application.ProductName, Application.ProductVersion);
        }
    }
}
