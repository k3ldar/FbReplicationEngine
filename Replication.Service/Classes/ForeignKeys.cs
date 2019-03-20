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
 *  Purpose:  Foreign Key References
 *
 */

namespace Replication.Service
{
    public class ForeignKeys
    {
        public ForeignKeys(string tableName, string tableColumn, string referencedTable, string referencedColumn)
        {
            TableName = tableName;
            TableColumn = tableColumn;
            ReferencedTable = referencedTable;
            ReferencedColumn = referencedColumn;
        }

        public string TableName { get; private set; }
        public string TableColumn { get; private set; }
        public string ReferencedTable { get; private set; }
        public string ReferencedColumn { get; private set; }
    }
}
