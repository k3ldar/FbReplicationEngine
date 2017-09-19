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
 *  Purpose:  Replicated Table Class
 *
 */
using System;

namespace Replication.Engine
{
    public sealed class ReplicatedTable
    {
        public ReplicatedTable(Int64 id, string name, Operation operation, string triggerName,
            int sortOrder, string excludeFields, string localGenerator, string remoteGenerator,
            string idColumn, int indiceType, TableOptions options)
        {
            ID = id;
            Name = name;
            Operation = operation;
            TriggerName = triggerName;
            SortOrder = sortOrder;
            ExcludeFields = excludeFields;
            LocalGenerator = localGenerator;
            RemoteGenerator = remoteGenerator;
            IDColumn = idColumn;
            IndiceType = indiceType;
            Options = options;
        }

        public Int64 ID { get; set; }
        public string Name { get; set; }
        public Operation Operation { get; set; }
        public string TriggerName { get; set; }
        public int SortOrder { get; set; }
        public string ExcludeFields { get; set; }
        public string LocalGenerator { get; set; }
        public string RemoteGenerator { get; set; }
        public string IDColumn { get; set; }
        public int IndiceType { get; set; }
        public TableOptions Options { get; set; }
    }
}
