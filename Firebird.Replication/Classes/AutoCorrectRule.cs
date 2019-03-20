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
 *  Purpose:  Automatic Correct Rule Item
 *
 */
using System;

namespace Replication.Engine.Classes
{
    public class AutoCorrectRule
    {
        #region Consructors

        public AutoCorrectRule()
        {
            TableName = String.Empty;
            KeyName = String.Empty;
            TargetTable = String.Empty;
            TargetColumn = String.Empty;
            ReplicateName = String.Empty;
            SQLRuleLocal = String.Empty;
            SQLRuleRemote = String.Empty;
            Dependencies = String.Empty;
        }

        public AutoCorrectRule(string tableName, string keyName, 
            string targetTable, string targetColumn, string replicateName, 
            string sqlRuleLocal, string sqlRuleRemote, string dependencies, AutoFixOptions options)
        {
            TableName = tableName;
            KeyName = keyName;
            TargetTable = targetTable;
            TargetColumn = targetColumn;
            ReplicateName = replicateName;
            SQLRuleLocal = sqlRuleLocal;
            SQLRuleRemote = sqlRuleRemote;
            Dependencies = dependencies; 
            Options = options;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Table where failure occured
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Key Name for failure
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// Target Replication Column
        /// </summary>
        public string TargetColumn { get; set; }

        /// <summary>
        /// Target Replication Table
        /// </summary>
        public string TargetTable { get; set; }

        /// <summary>
        /// Replication Column Names
        /// </summary>
        public string ReplicateName { get; set; }

        /// <summary>
        /// SQL used to try and fix data
        /// </summary>
        public string SQLRuleLocal { get; set; }

        /// <summary>
        /// SQL used to fix remote replication table
        /// </summary>
        public string SQLRuleRemote { get; set; }

        /// <summary>
        /// List of tables the rules depend on, seperated by a semi colon
        /// </summary>
        public string Dependencies { get; set; }

        /// <summary>
        /// Options for fixing data
        /// </summary>
        public AutoFixOptions Options { get; set; }


        #endregion Properties
    }
}
