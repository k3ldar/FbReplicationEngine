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
 *  Purpose:  Global Enums
 *
 */
using System;

namespace Replication.Engine
{
    [Flags]
    public enum TableOptions
    {
        /// <summary>
        /// No Options specified
        /// </summary>
        None = 0,

        /// <summary>
        /// Force verify is not enabled for table
        /// </summary>
        DoNotVerify = 1,

        /// <summary>
        /// Does not verify the child database
        /// </summary>
        DoNotVerifyChild = 2,

        /// <summary>
        /// Does not verify the master database
        /// </summary>
        DoNotVerifyMaster = 4,

        /// <summary>
        /// Full table scan is ascending, default is descending
        /// </summary>
        Ascending = 8,

#if LogRowData
        /// <summary>
        /// If set, then full table logging will be included, otherwise only overview changes
        /// will be logged in operation log table
        /// </summary>
        LogRowData = 16
#endif
    }

    public enum ReplicationStatus
    {
        NotReplicated = 0,

        Replicated = 1,

        NoChildColumns = 3,

        ViolationOfUniqueKey = 5,

        OperationIDIsDuplicated = 6,

        NoIDAvailable = 104
    }

    public enum ReplicationType
    {
        NotSet,

        Master,

        Child
    }

    /// <summary>
    /// Replication Results
    /// </summary>
    public enum ReplicationResult 
    { 
        /// <summary>
        /// Replication Complete
        /// </summary>
        Completed, 
        
        /// <summary>
        /// Deep scan completed
        /// </summary>
        DeepScanCompleted, 

        /// <summary>
        /// Deep scan is initialising
        /// </summary>
        DeepScanInitialised,
        
        /// <summary>
        /// An error occurred
        /// </summary>
        Error, 
        
        /// <summary>
        /// Replication Cancelled
        /// </summary>
        Cancelled, 
        
        /// <summary>
        /// Thresh hold exceeded
        /// </summary>
        ThresholdExceeded, 
        
        /// <summary>
        /// Time out exceeded
        /// </summary>
        TimeOutExceeded, 
        
        /// <summary>
        /// Unique access for deep scan denied
        /// </summary>
        UniqueAccessDenied,

        /// <summary>
        /// Initial value set prior to run
        /// </summary>
        NotInitialised
    }

    public enum AutoFixOptions
    {
        /// <summary>
        /// ignore the record by setting the operation id to 200
        /// </summary>
        IgnoreRecord = 2,

        /// <summary>
        /// Append the char 'F' to the end of the column that violated the rule
        /// </summary>
        AppendExtraChar = 4,

        /// <summary>
        /// Attempt to get the remote ID and update the local id with the value
        /// 
        /// Used if record is missing from remote table
        /// </summary>
        AttemptIDRemote = 64,

        /// <summary>
        /// Attempt to get the remote ID and update the local id with the value
        /// 
        /// used if record is missing from local table
        /// </summary>
        AttemptIDLocal = 128
    }

    [Flags]
    public enum ReplicationErrors
    {
        /// <summary>
        /// None, default value
        /// </summary>
        None = 0,

        /// <summary>
        /// Violation of PrimaryKey is allowed
        /// </summary>
        AllowViolationPrimaryKey = 1,

        /// <summary>
        /// Violation of foreign key is allowed
        /// </summary>
        AllowViolationForeignKey = 2,

        /// <summary>
        /// Attempt to store duplicate value is allowed
        /// </summary>
        AllowStoreDuplicateValue = 4

    }

    public enum Operation
    {
        Insert,

        Update,

        Delete
    }
}
