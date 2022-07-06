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
 *  Purpose:  Replication Class to Replicate Data
 *
 */
using System;
using System.Data;
using System.Reflection;

using FirebirdSql.Data.FirebirdClient;

using Replication.Engine.Classes;

#pragma warning disable IDE1005
#pragma warning disable IDE1006
#pragma warning disable IDE0018

namespace Replication.Engine
{
    public class ReplicationEngine : IDisposable
    {
        #region Private / protected Members

        private const string ENCRYPTION_KEY = "pERFJFPDAS903#OR4SDF#;A;SODRFSDSKJF";

        /// <summary>
        /// max string size for utf8 divided by max key length (bigint + spacer char)
        /// </summary>
        private const int PAGE_SIZE = (16380 / (24 + 1)) -1;

        private FbConnection _LocalDB;
        private FbConnection _RemoteDB;

        private bool _canReplicate = false;
        //private bool _canConfirmReplication = false;

        private string _failures;
        private string _failedTables = "";
        private int _missingRecordCount = 0;

        private Int64 _siteID;
        //private Int64 _siteVersion;

        private AutoCorrectRules _autoCorrectRules;

        //private Random _random;

        private string _remoteConnection = String.Empty;
        private string _localConnection = String.Empty;
        private string _tableConfig = String.Empty;

        private string _connectionName;

        #endregion Private / protected Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public ReplicationEngine(string connectionName, bool canReplicate, string childDatabase, string masterDatabase)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-gb");
            _tableConfig = Shared.Utilities.CurrentPath(true) + "Config\\Tables\\" + connectionName + ".config";
            _connectionName = connectionName;
            ChildDatabase = childDatabase;
            MasterDatabase = masterDatabase;
            _canReplicate = canReplicate;

            Statuses = TableStatuses.Load(_tableConfig);

            //TablesConfirmedCorrect = String.Empty;
            VerifyTableCounts = 20;
            VerifyAllDataInterval = 180; //every 2 days if replication is every 2 minutes
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Child connection string
        /// </summary>
        public string ChildDatabase { get; private set; }

        /// <summary>
        /// Master connection string
        /// </summary>
        public string MasterDatabase { get; private set; }

        /// <summary>
        /// Forces replication engine to confirm record counts for each table
        /// </summary>
        public bool ConfirmTableCounts { get; set; }

        /// <summary>
        /// Replication will exit after confirming MissingRecordCount if this value is true
        /// </summary>
        public int ForceRestartErrorCount { get; set; }

        /// <summary>
        /// Number of missing records
        /// </summary>
        public int MissingRecordCount 
        {
            get
            {
                return (_missingRecordCount);
            }

            set
            {
                _missingRecordCount = value;
                RaiseProgressText(String.Format("#MISSING#{0}", value));
            }
        }

        /// <summary>
        /// The number of iterations until table numbers and missing records confirmed
        /// </summary>
        public int VerifyTableCounts { get; set; }

        /// <summary>
        /// The number of iterations until a complete trawel of all ID's is undertaken
        /// </summary>
        public int VerifyAllDataInterval { get; set; }

        /// <summary>
        /// Allows user to cancel replication part way through
        /// </summary>
        public bool CancelReplication { get; set; }

        /// <summary>
        /// Indicates the system cancelled the replication
        /// </summary>
        public bool CancelReplicationSystem { get; set; }

        /// <summary>
        /// Tables confirmed as being correct on initial scan with no missing records
        /// </summary>
        public TableStatuses Statuses { get; set; }

        /// <summary>
        /// Maximum number of downloads before the data is committed and reset
        /// </summary>
        public int MaximumDownloadCount { get; set; }

        /// <summary>
        /// Maximum number of uploads before the data is committed and reset
        /// </summary>
        public int MaximumUploadCount { get; set; }

        /// <summary>
        /// Validates databases looking for missing index's, columns etc etc
        /// </summary>
        public bool Validate { get; set; }

        /// <summary>
        /// Indicates wether the replication is running or not
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// Time/Date Replication Started
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Number of minutes activity before time out occurs
        /// </summary>
        public int TimeOutMinutes { get; set; }

        /// <summary>
        /// Determines wether the time out period has been exceeded
        /// </summary>
        public bool TimeOutExceeded
        {
            get
            {
                TimeSpan span = DateTime.Now - StartTime;
                return (span.TotalMinutes >= TimeOutMinutes);
            }
        }

        /// <summary>
        /// If true then unique access to master is required when force verification is set
        /// </summary>
        public bool RequireUniqueAccess { get; set; }

        /// <summary>
        /// Local Database Attachment ID
        /// </summary>
        public Int64 LocalDatabaseAttachmentID { get; private set; }

        /// <summary>
        /// Remote database Attachment ID
        /// </summary>
        public Int64 RemoteDatabaseAttachmentID { get; private set; }

        #endregion Properties

        #region Public Methods

        public void Dispose()
        {
#if DEBUG
            System.GC.SuppressFinalize(this);
#endif
            TableStatuses.Save(Statuses, _tableConfig);
        }

#if DEBUG
        ~ReplicationEngine()
        {
            Dispose();
        }
#endif

        /// <summary>
        /// Replicates changes between the databases
        /// </summary>
        /// <param name="SendEmailsOnly">if true only email's will be sent</param>
        /// <param name="ReplicationCount">The number of times that the replication process has completed</param>
        /// <param name="AllowConfirmCounts">Allow's the replication engine to confirm record counts</param>
        /// <param name="ForceHardConfirm">Forces the replication to </param>
        /// <returns></returns>
        public ReplicationResult Run(bool AllowConfirmCounts, bool ForceHardConfirm)
        {
            ReplicationResult Result = ReplicationResult.NotInitialised;

            Int64 maxLocal = 0;
            MissingRecordCount = 0;
            _failures = String.Empty;
            _failedTables = String.Empty;
            CancelReplication = false;
            CancelReplicationSystem = false;
            ConfirmTableCounts = false;
            Running = true;
            StartTime = DateTime.Now;

            try
            {
                RaiseBeginReplication();
                try
                {
                    if (ConnectToDatabases())
                    {
                        try
                        {
                            if (!_canReplicate)
                            {
                                return (Result);
                            }

                            _autoCorrectRules = LoadReplicationFailureOptions();

                            // ********************************************************************
                            //from this point on it either all works or all fails, if fails then all 
                            //is rolled back, if works then all is committed
                            FbTransaction fbLocalTransaction = _LocalDB.BeginTransaction(IsolationLevel.Snapshot, 
                                "REPLICATION_ENGINE");
                            try
                            {
                                FbTransaction fbRemoteTransaction = _RemoteDB.BeginTransaction(IsolationLevel.Snapshot, 
                                    "REPLICATION_ENGINE");
                                try
                                {
                                    SetRemoteContext(fbRemoteTransaction, _siteID);
                                    RaiseProgressText("Replicating Log Files");

                                    ReplicationResult replicationResult;

                                    RaiseProgressText("Uploading Changes");

                                    if (!ReplicateChangesFromChildToMaster(fbLocalTransaction, fbRemoteTransaction, 
                                        true, out replicationResult))
                                    {
                                        switch (replicationResult)
                                        {
                                            case ReplicationResult.TimeOutExceeded:
                                            case ReplicationResult.ThresholdExceeded:
                                            case ReplicationResult.Completed:
                                            case ReplicationResult.Cancelled:
                                                Result = replicationResult;
                                                break;

                                            case ReplicationResult.Error:
                                                throw new Exception("Failed to upload changes");

                                            default:
                                                throw new Exception("Unknown Result Type");
                                        }
                                    }
                                    else
                                    {
                                        Result = ReplicationResult.Completed;
                                    }


                                    // download all updates 
                                    //only if all went well with upload
                                    if (replicationResult == ReplicationResult.Completed)
                                    {
                                        RaiseProgressText("Downloading Updates");

                                        if (!ReplicateChangesFromMasterToChild(fbLocalTransaction, fbRemoteTransaction,
                                            out replicationResult, out maxLocal))
                                        {
                                            switch (replicationResult)
                                            {
                                                case ReplicationResult.TimeOutExceeded:
                                                case ReplicationResult.ThresholdExceeded:
                                                case ReplicationResult.Completed:
                                                    Result = replicationResult;
                                                    break;

                                                case ReplicationResult.Error:
                                                    ResetMaxLocal(maxLocal, fbLocalTransaction);
                                                    throw new Exception("Failed to download updates");

                                                case ReplicationResult.Cancelled:
                                                    ResetMaxLocal(maxLocal, fbLocalTransaction);
                                                    Result = ReplicationResult.Cancelled;
                                                    break;

                                                default:
                                                    ResetMaxLocal(maxLocal, fbLocalTransaction);
                                                    throw new Exception("Unknown Result Type");
                                            }

                                        }
                                        else
                                        {
                                            Result = replicationResult;
                                        }
                                    }

                                    // commit all transactions to this point as normal change logs have been procssed
                                    fbRemoteTransaction.CommitRetaining();
                                    fbLocalTransaction.CommitRetaining();

                                    if (maxLocal > 0)
                                    {
                                        try
                                        {
                                            string sqlUpdateRemoteID = String.Format(
                                                "SET GENERATOR REPLICATE$REMOTE_LOG_ID TO {0}", maxLocal);

                                            FbCommand cmdUpdateGen = new FbCommand(sqlUpdateRemoteID, 
                                                _LocalDB, fbLocalTransaction);
                                            try
                                            {
                                                cmdUpdateGen.ExecuteNonQuery();
                                            }
                                            finally
                                            {
                                                CloseAndDispose(ref cmdUpdateGen);
                                            }
                                        }
                                        finally
                                        {
                                            fbLocalTransaction.CommitRetaining();
                                        }
                                    }

                                    // set maxlocal value now

                                    // from here we can perform a scan of all tables
                                    if (!RaiseCheckCancel() && AllowConfirmCounts && Result == ReplicationResult.Completed)
                                    {
                                        //fbLocalTransaction = _LocalDB.BeginTransaction(IsolationLevel.Snapshot, "REPLICATION_ENGINE");
                                        //fbRemoteTransaction = _RemoteDB.BeginTransaction(IsolationLevel.Snapshot, "REPLICATION_ENGINE");
                                        try
                                        {
                                            if (!RaiseCheckCancel() && (ForceHardConfirm))
                                            {
                                                Result = ReplicationResult.DeepScanInitialised;

                                                int connectionCount = 0;

                                                //this can only take place if there are no other connections doing a full scan
                                                string sqlCheckConnections = "SELECT COUNT(a.MON$ATTACHMENT_ID) FROM MON$ATTACHMENTS a " +
                                                    "WHERE a.MON$ATTACHMENT_ID <> CURRENT_CONNECTION AND a.MON$USER = 'REPLICATION'";
                                                FbDataReader rdrCheckConnections = null;
                                                FbCommand cmdCheckConnections = new FbCommand(sqlCheckConnections, _RemoteDB, fbRemoteTransaction);
                                                try
                                                {
                                                    rdrCheckConnections = cmdCheckConnections.ExecuteReader();

                                                    if (rdrCheckConnections.Read())
                                                        connectionCount = rdrCheckConnections.GetInt32(0);
                                                }
                                                finally
                                                {
                                                    CloseAndDispose(ref cmdCheckConnections, ref rdrCheckConnections);
                                                }

                                                if (RequireUniqueAccess && connectionCount > 0)
                                                {
                                                    Result = ReplicationResult.UniqueAccessDenied;
                                                }
                                                else
                                                {
                                                    //verify all id's from both tables every Nth iteration
                                                    HardConfirmAllTables(fbLocalTransaction, fbRemoteTransaction);

                                                    if (TimeOutExceeded)
                                                        Result = ReplicationResult.TimeOutExceeded;
                                                    else if (CancelReplication)
                                                        Result = ReplicationResult.Cancelled;
                                                    else if (MissingRecordCount >= ForceRestartErrorCount)
                                                        Result = ReplicationResult.ThresholdExceeded;
                                                    else
                                                        Result = ReplicationResult.DeepScanCompleted;
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            Shared.EventLog.Add("Before Commit AllowConfirmCounts");
                                            SetRemoteContext(fbRemoteTransaction, -1);
                                            fbRemoteTransaction.Commit();
                                            fbRemoteTransaction.Dispose();
                                            fbRemoteTransaction = null;
                                            fbLocalTransaction.Commit();
                                            fbLocalTransaction.Dispose();
                                            fbLocalTransaction = null;
                                        }
                                    }
                                }
                                catch (Exception errLocalizedRemote)
                                {
                                    Shared.EventLog.Add(errLocalizedRemote);
                                    Shared.EventLog.Add("errLocalizedRemote");
                                    Shared.EventLog.Add(errLocalizedRemote.Message);
                                    Shared.EventLog.Add(errLocalizedRemote.StackTrace.ToString());

                                    if (fbRemoteTransaction.Connection != null)
                                        fbRemoteTransaction.Rollback();
                                    throw;
                                }
                            }
                            catch (Exception errLocalized)
                            {
                                Shared.EventLog.Add(errLocalized);

                                if (errLocalized.Message.Contains(Constants.ERROR_FB_READING_CONNECTION) |
                                    errLocalized.Message.Contains(Constants.ERROR_FB_CONNECTION_SHUTDOWN))
                                {
                                    //nothing
                                }
                                else
                                {
                                    Shared.EventLog.Add("errLocalized");
                                    Shared.EventLog.Add(errLocalized.Message);
                                    Shared.EventLog.Add(errLocalized.StackTrace.ToString());
                                }

                                if (fbLocalTransaction.Connection != null)
                                    fbLocalTransaction.Rollback();

                                throw;
                            }
                        }
                        finally
                        {
                            DisconnectFromDatabases();
                        }
                    }
                    else
                    {
                        RaiseProgressText("Unable to connect to databases!");
                    }
                }
                finally
                {
                    RaiseEndReplication();

                    if (!String.IsNullOrEmpty(_failures) && (ForceHardConfirm))
                        Shared.EventLog.LogError(MethodBase.GetCurrentMethod(), _failures, 
                            AllowConfirmCounts, ForceHardConfirm);
                }
            }
            catch (Exception e)
            {
                Shared.EventLog.Add(e);
                Shared.EventLog.Add(e.Message);

                if (!e.Message.Contains("Unable to complete network request to host"))
                {
                    RaiseReplicationError(e.Message);
                }

                RaiseReplicationError(e.Message);
            }
            finally
            {
                if (CancelReplication)
                    RaiseProgressText("User cancelled replication");

                LocalIDChanges.LocalChanges.Save();

                Running = false;
            }


            return (Result);
        }

        #endregion Public Methods

        #region Private Methods

        #region Failure Logging

        /// <summary>
        /// Creates a log of upload fail errors for sending to server
        /// </summary>
        /// <param name="method">Method which is adding the record failure</param>
        /// <param name="error">error that occured</param>
        /// <param name="localError">true if the failure was on the slave database, otherwise false</param>
        /// <param name="values">list of parameter values for the method which called this function</param>
        private void AddRecordFailure(MethodBase method, string error, bool localError, params object[] values)
        {
            ParameterInfo[] parms = method.GetParameters();
            string parameters = String.Empty;

            for (int i = 0; i < parms.Length; i++)
            {
                if (i >= values.Length)
                    parameters += String.Format("{0} = {1}\r\n", parms[i].Name, "missing parameter value???");
                else
                    parameters += String.Format("{0} = {1}\r\n", parms[i].Name, values[i] ?? "null");
            }

            _failures += String.Format("\r\n\r\nMissing from Child Table: {4}\r\n\r\n{2}\r\n\r\n{3}\r\n\r\n{0}\r\n{1}", 
                DateTime.Now.ToString("g"), error, method.Name, parameters, localError.ToString());
            Shared.EventLog.Add(new Exception(String.Format("\r\n\r\nMissing from Child Table: {4}\r\n\r\n{2}\r\n\r\n{3}\r\n\r\n{0}\r\n{1}", 
                DateTime.Now.ToString("g"), error, method.Name, parameters, localError.ToString())));
        }

        #endregion Failure Logging

        #region Confirmation

        /// <summary>
        /// Initiates a hard confirm of all tables
        /// </summary>
        /// <param name="localTran">existing transaction for slave database</param>
        /// <param name="remoteTran">existing transaction for master database</param>
        private void HardConfirmAllTables(FbTransaction localTran, FbTransaction remoteTran)
        {
            try
            {
                // show that we are replicating in this transaction
                FbCommand setReplicationStatus = new FbCommand("EXECUTE PROCEDURE REPLICATE$REMOTEUPDATES_2(1);", 
                    _LocalDB, localTran);
                try
                {
                    setReplicationStatus.ExecuteNonQuery();
                    try
                    {
                        try
                        {
                            RaiseProgressText("Confirming Replication (Force Verify)");

                            HardConfirmReplicationCountsAllTables(_LocalDB, _RemoteDB, localTran, remoteTran);

                            RaiseProgressText("#STATUS#");
                        }
                        catch (Exception errLocalizedRemote)
                        {
                            Shared.EventLog.Add(errLocalizedRemote.Message);
                            throw;
                        }
                    }
                    finally
                    {
                        //remove replication context
                        FbCommand cmd = new FbCommand("EXECUTE PROCEDURE REPLICATE$REMOTEUPDATES_2(0);", 
                            _LocalDB, localTran);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd);
                        }
                    }
                }
                finally
                {
                    CloseAndDispose(ref setReplicationStatus);
                }
            }
            catch (Exception errLocalized)
            {
                Shared.EventLog.Add(errLocalized.Message);
                throw;
            }
        }

        /// <summary>
        /// Confirms each record within the table exists in the slave database, if it doesn't
        /// then copies the creates the record on the slave database
        /// </summary>
        /// <param name="fbLocalTransaction"></param>
        /// <param name="fbRemoteTransaction"></param>
        /// <param name="tableName"></param>
        /// <param name="missingRecordCount"></param>
        /// <param name="ignoreCount"></param>
        /// <returns>long, number of missing records</returns>
        private long ConfirmReplicationByTableMasterToChild(FbTransaction fbLocalTransaction, FbTransaction fbRemoteTransaction,
            string tableName, long missingRecordCount, TableStatus tableStatus, TableOptions options, bool ignoreCount = false)
        {
            long Result = 0;
            FbDataReader rdrReplicatedTables = null;
            FbCommand cmdLocalReplicatedTables = new FbCommand(String.Format("SELECT a.TABLE_NAME, a.LOCAL_ID_COLUMN " +
                "FROM REPLICATE$TABLES a WHERE a.LOCAL_ID_COLUMN <> '' " +
                "AND a.OPERATION = 'UPDATE' AND a.TABLE_NAME = '{0}' ORDER BY a.SORT_ORDER", tableName),
                _LocalDB, fbLocalTransaction);
            try
            {
                rdrReplicatedTables = cmdLocalReplicatedTables.ExecuteReader();

                while (rdrReplicatedTables.Read())
                {
                    string columnName = rdrReplicatedTables.GetString(1).Trim();
                    RaiseProgressText(String.Format("Verifying Child Table {0}", rdrReplicatedTables.GetString(0).Trim()));
                    Int64 lastID = 0;
                    string SQLRemote = String.Format("SELECT {0} FROM {1} WHERE {0} ", columnName,
                        rdrReplicatedTables.GetString(0).Trim());

                    if (options.HasFlag(TableOptions.Ascending))
                    {
                        SQLRemote += String.Format(" >= {0} ORDER BY {1};", tableStatus.ChildRecord, columnName);
                    }
                    else
                    {
                        SQLRemote += String.Format(" <= {0} ORDER BY {1} DESC;", tableStatus.ChildRecord, columnName);
                    }

                    FbDataReader rdrRemoteTable = null;
                    FbCommand cmdRemoteTable = new FbCommand(SQLRemote, _RemoteDB, fbRemoteTransaction);
                    try
                    {
                        rdrRemoteTable = cmdRemoteTable.ExecuteReader();

                        int count = 0;
                        string arrayList = "";
                        string SQLLocal = "";
                        string missingID = "";
                        string table = "";

                        while (rdrRemoteTable.Read())
                        {
                            count++;
                            lastID = rdrRemoteTable.GetInt64(0);
                            arrayList += String.Format("{0},", lastID);

                            #region if count
                            if (count % PAGE_SIZE == 0)
                            {
                                RaiseProgressText(String.Format("#STATUS#Verifying Child Table: {0}; {1} Records Verified", 
                                    tableName, count));
                                SQLLocal = String.Format("SELECT IPNOT_FOUND FROM REPLICATE$FIND_MISSING_RECORDS('{0}', '{1}', '{2}');",
                                    rdrReplicatedTables.GetString(0), rdrReplicatedTables.GetString(1), arrayList);
                                FbDataReader rdrLocalTable = null;
                                FbCommand cmdLocalTable = new FbCommand(SQLLocal, _LocalDB, fbLocalTransaction);
                                try
                                {
                                    rdrLocalTable = cmdLocalTable.ExecuteReader();

                                    // has the user cancelled the replication
                                    if (RaiseCheckCancel() | TimeOutExceeded)
                                        return (Result);

                                    while (rdrLocalTable.Read())
                                    {
                                        Result++;

                                        //each item that comes back is missing from main table
                                        missingID = rdrLocalTable.GetString(0);
                                        table = rdrReplicatedTables.GetString(0).Trim();
                                        RaiseProgressText(String.Format("Record {0} is missing from child table {1}", 
                                            missingID, table));

                                        MissingRecordCount++;

                                        //user can set a maximum record failure count, if
                                        // the number is met then exit
                                        if (MissingRecordCount >= ForceRestartErrorCount)
                                            return (Result);


                                        //copy record from remote to local
                                        ReplicateChangesFromMasterToChild(fbLocalTransaction, fbRemoteTransaction,
                                                tableName, rdrReplicatedTables.GetString(1).Trim(), missingID);

                                        // has the user cancelled the replication
                                        if (RaiseCheckCancel() | TimeOutExceeded)
                                            return (Result);
                                    }

                                    arrayList = "";
                                }
                                finally
                                {
                                    CloseAndDispose(ref cmdLocalTable, ref rdrLocalTable);
                                }
                            }
                            #endregion
                            // has the user cancelled the replication or time exceeded
                            if (RaiseCheckCancel() | TimeOutExceeded)
                                return (Result);
                        }

                        if (!String.IsNullOrEmpty(arrayList))
                        {
                            SQLLocal = String.Format("SELECT IPNOT_FOUND FROM REPLICATE$FIND_MISSING_RECORDS('{0}', '{1}', '{2}');",
                                rdrReplicatedTables.GetString(0), rdrReplicatedTables.GetString(1), arrayList);
                            FbDataReader rdrLocalTable = null;
                            FbCommand cmdLocalTable = new FbCommand(SQLLocal, _LocalDB, fbLocalTransaction);
                            try
                            {
                                rdrLocalTable = cmdLocalTable.ExecuteReader();

                                while (rdrLocalTable.Read())
                                {
                                    Result++;

                                    //each item that comes back is missing from main table
                                    missingID = rdrLocalTable.GetString(0);
                                    table = rdrReplicatedTables.GetString(0).Trim();

                                    MissingRecordCount++;

                                    if (MissingRecordCount >= ForceRestartErrorCount)
                                        return (Result);


                                    RaiseProgressText(String.Format("Record {0} is missing from child table {1}", 
                                        missingID, table));

                                    //copy record from remote to local
                                    ReplicateChangesFromMasterToChild(fbLocalTransaction, fbRemoteTransaction,
                                            tableName, rdrReplicatedTables.GetString(1).Trim(), missingID);

                                    // has the user cancelled the replication
                                    if (RaiseCheckCancel() | TimeOutExceeded)
                                        return (Result);
                                }
                            }
                            finally
                            {
                                CloseAndDispose(ref cmdLocalTable, ref rdrLocalTable);
                            }
                        }

                        RaiseProgressText(String.Format("{1} records checked in table {0}", 
                            rdrReplicatedTables.GetString(0), count));

                        if (!ignoreCount && Result == missingRecordCount)
                            break;

                        RaiseProgressText(String.Format("Table Local {0} Verified {1} records corrected",
                            rdrReplicatedTables.GetString(0).Trim(), Result));

                        // has the user cancelled the replication
                        if (RaiseCheckCancel() | TimeOutExceeded)
                            return (Result);
                    }
                    finally
                    {
                        if (lastID != 0)
                            tableStatus.ChildRecord = lastID;

                        CloseAndDispose(ref cmdRemoteTable, ref rdrRemoteTable);
                    }
                }
            }
            finally
            {
                CloseAndDispose(ref cmdLocalReplicatedTables, ref rdrReplicatedTables);
            }

            // return number of missing records
            return (Result);
#if OLD_MASTER_METHOD
            long Result = 0;

            string sql = String.Format("SELECT a.TABLE_NAME, a.LOCAL_ID_COLUMN FROM REPLICATE$TABLES a " +
                "WHERE a.LOCAL_ID_COLUMN <> '' " +
                "AND a.OPERATION = 'UPDATE' AND a.TABLE_NAME = '{0}' ORDER BY a.SORT_ORDER", tableName);
            FbDataReader rdrReplicatedTables = null;
            FbCommand cmdLocalReplicatedTables = new FbCommand(sql, _LocalDB, fbLocalTransaction);
            Int64 lastID = 0;
            try
            {
                rdrReplicatedTables = cmdLocalReplicatedTables.ExecuteReader();

                while (rdrReplicatedTables.Read())
                {
                    //calculate pages
                    long pageCount = Convert.ToInt64(Math.Ceiling(((double)missingRecordCount / PAGE_SIZE)));


                    for (int i = 0; i < pageCount; i++)
                    {
                        string identifiers = String.Empty;

                        //get Nth page
                        string SQLRemote = String.Format("SELECT opFIRSTVALUE, opLASTVALUE, opARRAY_VALUES " +
                            "FROM REPLICATE$VERIFY_RECORDS2('{0}', '{1}', {2}, {3}, {4}, {5});",
                            rdrReplicatedTables.GetString(0).Trim(),
                            rdrReplicatedTables.GetString(1).Trim(),
                            i + 1, PAGE_SIZE,
                            options.HasFlag(TableOptions.Ascending) ? 0 : 1,
                            tableStatus.ChildRecord);
                        FbDataReader rdrRemoteTable = null;
                        FbCommand cmdRemoteTable = new FbCommand(SQLRemote, _RemoteDB, fbRemoteTransaction);
                        try
                        {
                            rdrRemoteTable = cmdRemoteTable.ExecuteReader();
                            bool recordsFound = false;

                            while (rdrRemoteTable.Read())
                            {
                                recordsFound = true;

                                if (!rdrRemoteTable.IsDBNull(1))
                                    lastID = rdrRemoteTable.GetInt64(1);
                                else if (!rdrRemoteTable.IsDBNull(0))
                                    lastID = rdrRemoteTable.GetInt64(0);

                                identifiers = rdrRemoteTable.GetString(2);

                                //retrieve missing records by calling sp which will return any missing ID's
                                string sqlLocalVerify = String.Format("SELECT p.IPNOT_FOUND FROM " +
                                    "REPLICATE$FIND_MISSING_RECORDS('{0}', '{1}', '{2}') p",
                                    tableName, rdrReplicatedTables.GetString(1), identifiers);
                                FbDataReader rdrLocalVerify = null;
                                FbCommand cmdReplicteVerify = new FbCommand(sqlLocalVerify, _LocalDB, fbLocalTransaction);
                                try
                                {
                                    rdrLocalVerify = cmdReplicteVerify.ExecuteReader();

                                    // has the user cancelled replication
                                    if (RaiseCheckCancel() | TimeOutExceeded)
                                        return (Result);

                                    //will only return id of records not found
                                    while (rdrLocalVerify.Read())
                                    {
                                        Result++;
                                        string missingID = rdrLocalVerify.GetString(0);

                                        MissingRecordCount++;

                                        // if user set limit of missing records has been
                                        // met then exit the routine
                                        if (MissingRecordCount >= ForceRestartErrorCount)
                                            return (Result);


                                        RaiseProgressText(String.Format("Record: {1} is missing from child, table {0}",
                                            tableName, missingID));
                                        ReplicateChangesFromMasterToChild(fbLocalTransaction, fbRemoteTransaction,
                                             tableName, rdrReplicatedTables.GetString(1).Trim(), missingID);

                                        // has the user cancelled replication
                                        if (RaiseCheckCancel() | TimeOutExceeded)
                                            return (Result);

                                        if (!ignoreCount && Result == missingRecordCount)
                                            break;
                                    }

                                    // has the user cancelled replication
                                    if (RaiseCheckCancel() | TimeOutExceeded)
                                        return (Result);
                                }
                                finally
                                {
                                    CloseAndDispose(ref cmdReplicteVerify, ref rdrLocalVerify);
                                }
                            }

                            if (!recordsFound)
                            {
                                break;
                            }
                        }
                        finally
                        {
                            CloseAndDispose(ref cmdRemoteTable, ref rdrRemoteTable);
                        }

                        //notify x records checked
                        RaiseProgressText(String.Format("#STATUS#Verifying Child Table: {0}; {1} Records Verified",
                            tableName, i > 1 ? i * PAGE_SIZE : missingRecordCount));
                    }

                    RaiseProgressText(String.Format("Child Table {0} Verified {1} records fixed",
                        rdrReplicatedTables.GetString(0).Trim(), Result));

                    // has the user cancelled replication
                    if (RaiseCheckCancel() | TimeOutExceeded)
                        return (Result);
                }
            }
            finally
            {
                if (lastID != 0 && tableStatus.ChildRecord  != lastID)
                    tableStatus.ChildRecord = lastID;

                CloseAndDispose(ref cmdLocalReplicatedTables, ref rdrReplicatedTables);
            }

            // return number of records that have been updated
            return (Result);
#endif
        }

        /// <summary>
        /// Confirms each record within the table exists in the master database, if it doesn't
        /// then copies the creates the record on the master database
        /// </summary>
        /// <param name="fbLocalTransaction"></param>
        /// <param name="fbRemoteTransaction"></param>
        /// <param name="tableName"></param>
        /// <param name="missingRecordCount"></param>
        /// <param name="ignoreCount"></param>
        /// <returns>long, number of missing records</returns>
        private long ConfirmReplicationByTableChildToMaster(FbTransaction fbLocalTransaction, FbTransaction fbRemoteTransaction,
            string tableName, long missingRecordCount, TableStatus tableStatus, TableOptions options, bool ignoreCount = false)
        {
            long Result = 0;
            FbDataReader rdrReplicatedTables = null;
            FbCommand cmdLocalReplicatedTables = new FbCommand(String.Format("SELECT a.TABLE_NAME, a.LOCAL_ID_COLUMN " +
                "FROM REPLICATE$TABLES a WHERE a.LOCAL_ID_COLUMN <> '' " +
                "AND a.OPERATION = 'UPDATE' AND a.TABLE_NAME = '{0}' ORDER BY a.SORT_ORDER", tableName),
                _LocalDB, fbLocalTransaction);
            try
            {
                rdrReplicatedTables = cmdLocalReplicatedTables.ExecuteReader();

                while (rdrReplicatedTables.Read())
                {
                    string columnName = rdrReplicatedTables.GetString(1).Trim();
                    RaiseProgressText(String.Format("Verifying Master {0}", rdrReplicatedTables.GetString(0).Trim()));
                    Int64 lastID = 0;

                    string SQLLocal = String.Format("SELECT {0} FROM {1} WHERE {0} ", columnName,
                        rdrReplicatedTables.GetString(0).Trim());

                    if (options.HasFlag(TableOptions.Ascending))
                    {
                        SQLLocal += String.Format(" >= {0} ORDER BY {1};", tableStatus.MasterRecord, columnName);
                    }
                    else
                    {
                        SQLLocal += String.Format(" <= {0} ORDER BY {1} DESC;", tableStatus.MasterRecord, columnName);
                    }

                    FbDataReader rdrLocalTable = null;
                    FbCommand cmdLocalTable = new FbCommand(SQLLocal, _LocalDB, fbLocalTransaction);
                    try
                    {
                        rdrLocalTable = cmdLocalTable.ExecuteReader();

                        int count = 0;
                        string arrayList = "";
                        string SQLRemote = "";
                        string missingID = "";
                        string table = "";

                        while (rdrLocalTable.Read())
                        {
                            count++;
                            lastID = rdrLocalTable.GetInt64(0);
                            arrayList += String.Format("{0},", lastID);

                            #region if count
                            if (count % PAGE_SIZE == 0)
                            {
                                RaiseProgressText(String.Format("#STATUS#Verifying Master Table: {0}; {1} Records Verified", 
                                    tableName, count));
                                SQLRemote = String.Format("SELECT OPNOT_FOUND, OPISDELETED FROM REPLICATE$FIND_MISSING_RECORDS('{0}', '{1}', '{2}');",
                                    rdrReplicatedTables.GetString(0), rdrReplicatedTables.GetString(1), arrayList);
                                FbDataReader rdrRemoteTable = null;
                                FbCommand cmdRemoteTable = new FbCommand(SQLRemote, _RemoteDB, fbRemoteTransaction);
                                try
                                {
                                    rdrRemoteTable = cmdRemoteTable.ExecuteReader();

                                    // has the user cancelled the replication
                                    if (RaiseCheckCancel() | TimeOutExceeded)
                                        return (Result);

                                    while (rdrRemoteTable.Read())
                                    {
                                        Result++;

                                        //each item that comes back is missing from main table
                                        missingID = rdrRemoteTable.GetString(0);
                                        table = rdrReplicatedTables.GetString(0).Trim();
                                        RaiseProgressText(String.Format("Record {0} is missing from master table {1}", 
                                            missingID, table));

                                        bool isDeleted = rdrRemoteTable.GetString(1) == "Y";

                                        if (isDeleted)
                                        {
                                            // delete from local table too
                                            string sqlDeleteLocal = String.Format("DELETE FROM {0} WHERE {1} = {2};", 
                                                tableName, columnName, missingID);
                                            FbCommand lclDeleteTable = new FbCommand(sqlDeleteLocal, 
                                                _LocalDB, fbLocalTransaction);
                                            try
                                            {
                                                lclDeleteTable.ExecuteNonQuery();
                                            }
                                            finally
                                            {
                                                CloseAndDispose(ref lclDeleteTable);
                                            }
                                        }
                                        else
                                        {
                                            MissingRecordCount++;

                                            //user can set a maximum record failure count, if
                                            // the number is met then exit
                                            if (MissingRecordCount >= ForceRestartErrorCount)
                                                return (Result);


                                            //copy record from local to remote
                                            ReplicateChangesFromChildToMaster(fbLocalTransaction, 
                                                fbRemoteTransaction, table, columnName, missingID, false);
                                        }
                                        // has the user cancelled the replication
                                        if (RaiseCheckCancel() | TimeOutExceeded)
                                            return (Result);
                                    }

                                    arrayList = "";
                                }
                                finally
                                {
                                    CloseAndDispose(ref cmdRemoteTable, ref rdrRemoteTable);
                                }
                            }
                            #endregion
                            // has the user cancelled the replication or time exceeded
                            if (RaiseCheckCancel() | TimeOutExceeded)
                                return (Result);
                        }

                        if (!String.IsNullOrEmpty(arrayList))
                        {
                            SQLRemote = String.Format("SELECT OPNOT_FOUND, OPISDELETED FROM REPLICATE$FIND_MISSING_RECORDS('{0}', '{1}', '{2}');",
                                rdrReplicatedTables.GetString(0), rdrReplicatedTables.GetString(1), arrayList);
                            FbDataReader rdrRemoteTable = null;
                            FbCommand cmdRemoteTable = new FbCommand(SQLRemote, _RemoteDB, fbRemoteTransaction);
                            try
                            {
                                rdrRemoteTable = cmdRemoteTable.ExecuteReader();

                                while (rdrRemoteTable.Read())
                                {
                                    Result++;

                                    //each item that comes back is missing from main table
                                    missingID = rdrRemoteTable.GetString(0);
                                    table = rdrReplicatedTables.GetString(0).Trim();

                                    bool isDeleted = rdrRemoteTable.GetString(1) == "Y";

                                    if (isDeleted)
                                    {
                                        // delete from local table too
                                        string sqlDeleteLocal = String.Format("DELETE FROM {0} WHERE {1} = {2};", 
                                            tableName, columnName, missingID);
                                        FbCommand lclDeleteTable = new FbCommand(sqlDeleteLocal, _LocalDB, fbLocalTransaction);
                                        try
                                        {
                                            lclDeleteTable.ExecuteNonQuery();
                                        }
                                        finally
                                        {
                                            CloseAndDispose(ref lclDeleteTable);
                                        }
                                    }
                                    else
                                    {
                                        MissingRecordCount++;

                                        if (MissingRecordCount >= ForceRestartErrorCount)
                                            return (Result);


                                        RaiseProgressText(String.Format("Record {0} is missing from master table {1}", 
                                            missingID, table));

                                        //copy record from local to remote
                                        ReplicateChangesFromChildToMaster(fbLocalTransaction, fbRemoteTransaction, 
                                            table, columnName, missingID, false);
                                    }

                                    // has the user cancelled the replication
                                    if (RaiseCheckCancel() | TimeOutExceeded)
                                        return (Result);
                                }
                            }
                            finally
                            {
                                CloseAndDispose(ref cmdRemoteTable, ref rdrRemoteTable);
                            }
                        }

                        RaiseProgressText(String.Format("{1} records checked in table {0}", 
                            rdrReplicatedTables.GetString(0), count));

                        if (!ignoreCount && Result == missingRecordCount)
                            break;

                        RaiseProgressText(String.Format("Table Remote {0} Verified {1} records corrected",
                            rdrReplicatedTables.GetString(0).Trim(), Result));

                        // has the user cancelled the replication
                        if (RaiseCheckCancel() | TimeOutExceeded)
                            return (Result);
                    }
                    finally
                    {
                        if (lastID != 0)
                            tableStatus.MasterRecord = lastID;

                        CloseAndDispose(ref cmdLocalTable, ref rdrLocalTable);
                    }
                }
            }
            finally
            {
                CloseAndDispose(ref cmdLocalReplicatedTables, ref rdrReplicatedTables);
            }
            // return number of missing records
            return (Result);
        }

        #endregion Confirmation

        #region Events

        public event ReplicationEventHandler BeginReplication;
        public event ReplicationEventHandler EndReplication;
        public event ReplicationPercentEventArgs OnProgress;
        public event ReplicationProgress OnReplicationTextChanged;
        public event ReplicationError OnReplicationError;
        public event ReplicationIDChangedEventHandler OnIDChanged;
        public event ReplicationCancelHandler OnCheckCancel;

        private bool RaiseCheckCancel()
        {
            if (CancelReplication)
                return (true);

            ReplicationCancelEventArgs args = new ReplicationCancelEventArgs();

            if (!CancelReplicationSystem)
            {
                // if not user cancelled
                if (OnCheckCancel != null)
                    OnCheckCancel(this, args);

                CancelReplicationSystem = args.CancelReplication;
            }

            return (args.CancelReplication);
        }

        private void RaiseIDChanged(Int64 oldID, Int64 newID, string location)
        {
            if (OnIDChanged != null)
                OnIDChanged(this, new IDUpdatedEventArgs(oldID, newID, location));
        }

        private void RaiseProgressEvent(int Complete, string Message)
        {
            if (OnProgress != null)
                OnProgress(this, new PercentEventArgs(Complete, Message));
        }

        private void RaiseProgressText(string Text)
        {
            if (OnReplicationTextChanged != null)
                OnReplicationTextChanged(this, new SynchTextEventArgs(Text));
        }

        private void RaiseReplicationError(string Text)
        {
            if (OnReplicationError != null)
                OnReplicationError(this, new SynchTextEventArgs(Text));
        }

        private void RaiseBeginReplication()
        {
            if (BeginReplication != null)
                BeginReplication(this);
        }

        private void RaiseEndReplication()
        {
            if (EndReplication != null)
                EndReplication(this);
        }

        #endregion Events

        #region Replicated Data

        private void ResetMaxLocal(Int64 maxLocal, FbTransaction transaction)
        {
            //FbConnection con = new FbConnection(FixConnectionString(ConnectionString))
            string sqlUpdateRemoteID = String.Format("SET GENERATOR REPLICATE$REMOTE_LOG_ID TO {0}", maxLocal);

            FbCommand cmdUpdateGen = new FbCommand(sqlUpdateRemoteID, _LocalDB, transaction);
            try
            {
                cmdUpdateGen.ExecuteNonQuery();
            }
            finally
            {
                CloseAndDispose(ref cmdUpdateGen);
            }
        }

        /// <summary>
        /// Replaces the ID's based on the rule passed in
        /// </summary>
        /// <param name="Rule">Rule to be applied</param>
        /// <param name="SQL">SQL where ID resides</param>
        /// <returns>SQL with replaced ID's</returns>
        private string ReplaceOldID(string Rule, string SQL, FbTransaction tranLocal)
        {
            string Result = SQL;

            string[] rules = Rule.Split('#');

            string currTable = "";
            string currColumn = "";
            string linkedTable = "";
            string linkedColumn = "";
            string oldID = "";
            UpdateLocalID newIDRecord = null;
            string newID;

            currTable = rules[0];
            currColumn = rules[1];

            if (rules.Length == 3)
            {
                oldID = rules[2];
                newIDRecord = LocalIDChanges.LocalChanges.Find(currTable, currColumn, oldID);
            }
            else
            {
                linkedTable = rules[2];
                linkedColumn = rules[3];
                oldID = rules[4];
                newIDRecord = LocalIDChanges.LocalChanges.Find(linkedTable, linkedColumn, oldID);
            }


            //does the newIDRecord found above actually exist?
            if (newIDRecord != null)
            {
                string sqlLocal = String.Format("SELECT COUNT ({0}) FROM {1} WHERE {0} = {2};", 
                    newIDRecord.PKColumn, newIDRecord.TableName, newIDRecord.NewID);
                FbDataReader rdrLocal = null;
                FbCommand cmdLocal = new FbCommand(sqlLocal, _LocalDB, tranLocal);
                try
                {
                    rdrLocal = cmdLocal.ExecuteReader();

                    int count = 0;

                    if (rdrLocal.Read())
                        count = rdrLocal.GetInt32(0);

                    if (count == 0)
                        newIDRecord = null;
                }
                finally
                {
                    CloseAndDispose(ref cmdLocal, ref rdrLocal);
                }
            }

            //is there a new id for this record?

            if (newIDRecord != null)
            {
                //found a linked record
                newID = newIDRecord.NewID;
            }
            else
            {
                newID = GetReplicationID(tranLocal, linkedTable, linkedColumn, oldID);
            }

            SQL = SQL.Replace(String.Format("@{0}@", currColumn), newID.ToString());

            return (SQL);
        }

        private string GetReplicationID(FbTransaction tranLocal, string linkedTable, string linkedColumn, string oldID)
        {
            string Result = oldID;

            //try to find it manually
            string sql = String.Format("SELECT a.NEW_PK_VALUE FROM REPLICATE$LOCALPKCHANGES a " +
                "WHERE a.TABLE_NAME = '{0}' AND a.PK_COLUMN = '{1}' AND a.OLD_PK_VALUE = '{2}';", 
                linkedTable, linkedColumn, oldID);
            FbDataReader rdr = null;
            FbCommand cmd = new FbCommand(sql, _LocalDB, tranLocal);
            try
            {
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    Result = rdr.GetString(0);
                }
                else
                {
                    //try and find the id in the table for changes
                    sql = String.Format("SELECT a.OLD_VALUE, a.NEW_VALUE FROM REPLICATE$COLUMNLOG a WHERE a.OLD_VALUE = '{2}' " +
                        "AND a.COLUMN_NAME = '{1}' AND a.NEW_VALUE IS NOT NULL AND a.OPERATIONLOG_ID IN (SELECT ol.ID " +
                        "FROM REPLICATE$OPERATIONLOG ol " +
                        "WHERE ol.TABLE_NAME = '{0}' AND ol.PKEY1_VALUE = a.OLD_VALUE )", linkedTable, linkedColumn, oldID);
                    FbDataReader rdrLocal = null;
                    FbCommand cmdLocal = new FbCommand(sql, _LocalDB, tranLocal);
                    try
                    {
                        rdrLocal = cmdLocal.ExecuteReader();

                        if (rdr.Read())
                        {
                            Result = rdrLocal.GetString(1);
                        }
                        else
                        {
                            //not found link, use existing ID
                            Result = oldID;
                        }
                    }
                    finally
                    {
                        CloseAndDispose(ref cmdLocal, ref rdrLocal);
                    }
                }
            }
            finally
            {
                CloseAndDispose(ref cmd, ref rdr);
            }

            return (Result);
        }

        /// <summary>
        /// Replicates changes from the local computer to the remote computer 
        /// </summary>
        /// <param name="tranLocal"></param>
        /// <param name="tranRemote"></param>
        /// <returns></returns>
        private bool ReplicateChangesFromChildToMaster(FbTransaction tranLocal, FbTransaction tranRemote,
            bool showCounts, out ReplicationResult downloadResult)
        {
            downloadResult = ReplicationResult.Completed;
            bool Result = false;
            string SQLRemote = "";
            string fKey1 = "";
            string fKey2 = "";
            string fKey3 = "";

            try
            {
                Double Total = 0;
                string SQLLocal = "SELECT COUNT (*) FROM REPLICATE$OPERATIONLOG r WHERE r.REPLICATED = 0;";
                FbDataReader rdrLocal = null;
                FbCommand cmdLocal = new FbCommand(SQLLocal, _LocalDB, tranLocal);
                try
                {
                    rdrLocal = cmdLocal.ExecuteReader();

                    if (rdrLocal.Read())
                        Total = rdrLocal.GetInt32(0);
                }
                finally
                {
                    CloseAndDispose(ref cmdLocal, ref rdrLocal);
                }

                SQLLocal = "SELECT p.OPOPERATION_ID, p.OPSQL, p.OPTABLE_NAME, p.OPOLD_ID, p.OPPRIMARY_KEY_COLUMN, " +
                    "p.OPREMOTE_GENERATOR, p.OPFKEY1, p.OPFKEY2, p.OPFKEY3, p.opOPTIONS " +
                    "FROM REPLICATE$REPLICATECHANGES p";
                cmdLocal = new FbCommand(SQLLocal, _LocalDB, tranLocal);
                try
                { 
                    rdrLocal = cmdLocal.ExecuteReader();
                    int I = 0;

                    while (rdrLocal.Read())
                    {
                        Int64 operationID = rdrLocal.GetInt64(0);
                        SQLRemote = rdrLocal.GetString(1);
                        string tableName = rdrLocal.GetString(2);
                        string oldID = rdrLocal.IsDBNull(3) ? "" : rdrLocal.GetString(3);
                        string primaryKey = rdrLocal.IsDBNull(4) ? "" : rdrLocal.GetString(4);
                        string generatorName = rdrLocal.IsDBNull(5) ? "" : rdrLocal.GetString(5);
                        fKey1 = rdrLocal.IsDBNull(6) ? "" : rdrLocal.GetString(6);
                        fKey2 = rdrLocal.IsDBNull(7) ? "" : rdrLocal.GetString(7);
                        fKey3 = rdrLocal.IsDBNull(8) ? "" : rdrLocal.GetString(8);
                        ReplicationErrors errors = (ReplicationErrors)rdrLocal.GetInt64(9);
                        
                        ReplicateDataToMaster(tranLocal, tranRemote, operationID, SQLRemote, tableName, 
                            oldID, primaryKey, generatorName, fKey1, fKey2, fKey3, errors);

                        I++;

                        if (showCounts)
                            RaiseProgressEvent(Convert.ToInt32((100 / Total) * I), String.Format("{0} of {1}", I, (int)Total));

                        // has the user cancelled the replication
                        if (RaiseCheckCancel())
                        {
                            RaiseProgressText("User Cancelled Replication");
                            downloadResult = ReplicationResult.Cancelled;
                            return (false);
                        }

                        if (TimeOutExceeded)
                        {
                            RaiseProgressText("Timeout Exceeded");
                            downloadResult = ReplicationResult.TimeOutExceeded;
                            return (false);
                        }

                        if (I >= MaximumUploadCount)
                        {
                            RaiseProgressText("Maximum Download Threshold reached");
                            downloadResult = ReplicationResult.ThresholdExceeded;
                            return (false);
                        }
                    }

                    Result = true;
                }
                catch (Exception errInner)
                {
                    Shared.EventLog.Add(errInner);
                    Result = false;
                    downloadResult = ReplicationResult.Error;
                    RaiseReplicationError(SQLRemote);
                    RaiseReplicationError(errInner.Message);
                }
                finally
                {
                    CloseAndDispose(ref cmdLocal, ref rdrLocal);
                }
            }
            catch (Exception errOuter)
            {
                Shared.EventLog.Add(errOuter);
                Result = false;
                downloadResult = ReplicationResult.Error;
                RaiseReplicationError(errOuter.Message);
                throw;
            }

            return (Result);
        }

        private bool ReplicateChangesFromChildToMaster(FbTransaction tranLocal, FbTransaction tranRemote, 
            string tableName, string columnName, string oldID, bool showCounts)
        {
            bool Result = false;
            string SQLRemote = "";
            string sqlRemoteInsert = "";
            string fKey1 = "";
            string fKey2 = "";
            string fKey3 = "";
            Int64 operationID = 0;

            try
            {
                Double Total = 0;
                string SQLLocal = "SELECT COUNT (*) FROM REPLICATE$OPERATIONLOG r WHERE r.REPLICATED = 0;";
                FbDataReader rdrLocal = null;
                FbCommand cmdLocal = new FbCommand(SQLLocal, _LocalDB, tranLocal);
                try
                {
                    rdrLocal = cmdLocal.ExecuteReader();

                    if (rdrLocal.Read())
                        Total = rdrLocal.GetInt32(0);

                    SQLLocal = String.Format("SELECT p.OPOPERATION_ID, p.OPSQL, p.OPTABLE_NAME, p.OPOLD_ID, " +
                        "p.OPPRIMARY_KEY_COLUMN, " +
                        "p.OPREMOTE_GENERATOR, p.OPFKEY1, p.OPFKEY2, p.OPFKEY3, p.opOPTIONS " +
                        "FROM REPLICATE$REPLICATECHANGES_I('{0}', '{1}', '{2}') p", tableName, columnName, oldID);

                    #region
                    int I = 0;
                    // unelegant method of getting count of rows
                    string localCountSQL = String.Format("SELECT COUNT(a.ID) FROM REPLICATE$OPERATIONLOG a WHERE a.TABLE_NAME = '{0}' " +
                        "AND a.OPERATION = 'INSERT' AND ((a.PKEY1 = '{1}' AND a.PKEY1_VALUE = '{2}') OR (a.PKEY2 = '{1}' AND a.PKEY2_VALUE = '{2}') " +
                        "OR (a.PKEY3 = '{1}' AND a.PKEY3_VALUE = '{2}'))", tableName, columnName, oldID);
                    FbCommand cmdLocalCount = new FbCommand(localCountSQL, _LocalDB, tranLocal);
                    FbDataReader rdrLocalCount = cmdLocalCount.ExecuteReader();
                    int localCount = -1;
                    
                    if (rdrLocalCount.Read())
                        localCount = rdrLocalCount.GetInt32(0);

                    if (localCount == 0)
                    {
                        // no record found in replication table so create it here to be processed in the while loop below
                        int Replicate = oldID.StartsWith("-") ? 0 : 1;
                        string sqlInsert = String.Format("SELECT opSQL FROM REPLICATE$GENERATEINSERT('{0}', '{1}', '{2}', {3});", 
                            tableName, columnName, oldID, Replicate);
                        FbDataReader rdrInsert = null;
                        FbCommand cmdInsert = new FbCommand(sqlInsert, _LocalDB, tranLocal);
                        try
                        {
                            rdrInsert = cmdInsert.ExecuteReader();

                            if (rdrInsert.Read())
                            {
                                if (Replicate == 1)
                                {
                                    sqlRemoteInsert = rdrInsert.GetString(0);
                                    FbCommand cmdInsertRemote = new FbCommand(sqlRemoteInsert, _RemoteDB, tranRemote);
                                    try
                                    {
                                        cmdInsertRemote.ExecuteNonQuery();
                                        RaiseProgressText(sqlRemoteInsert);
                                    }
                                    finally
                                    {
                                        CloseAndDispose(ref cmdInsertRemote);
                                    }
                                }
                            }
                            else
                            {
                                RaiseProgressText("Failed to upload row to server");
                            }
                        }
                        finally
                        {
                            CloseAndDispose(ref cmdInsert, ref rdrInsert);
                        }
                    }

                    cmdLocal = new FbCommand(SQLLocal, _LocalDB, tranLocal);
                    rdrLocal = cmdLocal.ExecuteReader();

                    while (rdrLocal.Read())
                    {
                        operationID = rdrLocal.GetInt64(0);
                        SQLRemote = rdrLocal.GetString(1);
                        string table = rdrLocal.GetString(2);
                        string oldid = rdrLocal.IsDBNull(3) ? "" : rdrLocal.GetString(3);
                        string primaryKey = rdrLocal.IsDBNull(4) ? "" : rdrLocal.GetString(4);
                        string generatorName = rdrLocal.IsDBNull(5) ? "" : rdrLocal.GetString(5);
                        fKey1 = rdrLocal.IsDBNull(6) ? "" : rdrLocal.GetString(6);
                        fKey2 = rdrLocal.IsDBNull(7) ? "" : rdrLocal.GetString(7);
                        fKey3 = rdrLocal.IsDBNull(8) ? "" : rdrLocal.GetString(8);
                        ReplicationErrors errors = (ReplicationErrors)rdrLocal.GetInt64(9);

                        ReplicateDataToMaster(tranLocal, tranRemote, operationID, SQLRemote, table, 
                            oldid, primaryKey, generatorName, fKey1, fKey2, fKey3, errors);

                        I++;

                        if (showCounts && Total > 0.00)
                            RaiseProgressEvent(Convert.ToInt32((100 / Total) * I), String.Format("{0} of {1}", I, (int)Total));

                        // has the user cancelled the replication
                        if (RaiseCheckCancel() | TimeOutExceeded)
                            return (false);
                    }

                    Result = true;
                    #endregion
                }
                catch (Exception errInner)
                {
                    Shared.EventLog.Add(errInner);
                    Result = false;
                    ProcessReplicationFailure(tranRemote, tranLocal, errInner.Message, SQLRemote, operationID, 
                        tableName, columnName, oldID, true);
                    AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}\r\n\r\n{2}", 
                        errInner.Message, SQLRemote, sqlRemoteInsert), false, tranLocal, tranRemote, 
                        tableName, columnName, oldID);
                    RaiseReplicationError(SQLRemote);
                    RaiseReplicationError(errInner.Message);
                }
                finally
                {
                    CloseAndDispose(ref cmdLocal, ref rdrLocal);
                }
            }
            catch (Exception errOuter)
            {
                Shared.EventLog.Add(errOuter);
                Result = false;
                RaiseReplicationError(errOuter.Message);
                throw;
            }

            return (Result);
        }

        /// <summary>
        /// Uploads an individual record to the live server
        /// </summary>
        private bool ReplicateDataToMaster(FbTransaction tranLocal, FbTransaction tranRemote, Int64 operationID,
            string SQLRemote, string tableName, string oldID, string primaryKey, string generatorName,
            string fKey1, string fKey2, string fKey3, ReplicationErrors errors)
        {
            bool Result = false;

            try
            {
                if (!String.IsNullOrEmpty(fKey1))
                    SQLRemote = ReplaceOldID(fKey1, SQLRemote, tranLocal);

                if (!String.IsNullOrEmpty(fKey2))
                    SQLRemote = ReplaceOldID(fKey2, SQLRemote, tranLocal);

                if (!String.IsNullOrEmpty(fKey3))
                    SQLRemote = ReplaceOldID(fKey3, SQLRemote, tranLocal);

                if (!String.IsNullOrEmpty(primaryKey))
                {
                    UpdateLocalID newIDRecord = LocalIDChanges.LocalChanges.Find(tableName, primaryKey, oldID);

                    if (newIDRecord != null)
                    {
                        if (SQLRemote.Contains(String.Format("@{0}@", primaryKey)))
                        {
                            SQLRemote = SQLRemote.Replace(String.Format("@{0}@", primaryKey), newIDRecord.NewID.ToString());
                        }
                    }
                    else
                    {
                        if (SQLRemote.StartsWith("UPDATE"))
                            SQLRemote = SQLRemote.Replace(String.Format("@{0}@", primaryKey), oldID);

                        if (SQLRemote.StartsWith("INSERT"))
                        {
                            if (String.IsNullOrEmpty(generatorName))
                            {
                                if (SQLRemote.Contains("@NEWGEN@"))
                                {
                                    SQLRemote = SQLRemote.Replace("@NEWGEN@", oldID.ToString());
                                }
                                else
                                {
                                    if (SQLRemote.Contains(String.Format("@{0}@", primaryKey)))
                                    {
                                        SQLRemote = SQLRemote.Replace(String.Format("@{0}@", primaryKey), oldID);
                                    }
                                }
                            }
                            else
                                SQLRemote = SQLRemote.Replace(String.Format("@{0}@", primaryKey), "@NEWGEN@");
                        }
                    }

                }

                RaiseProgressEvent(-279, SQLRemote);

                //RaiseProgressText(SQLRemote);
                string newID = String.Empty;

                FbCommand cmdRemote = new FbCommand("REPLICATE$RUNSQL", _RemoteDB, tranRemote);
                try
                {
                    PrepareCommand(cmdRemote);
                    AddParam(cmdRemote, "@ipSQL", FbDbType.Text, SQLRemote);
                    AddParam(cmdRemote, "@ipGENERATOR_NAME", FbDbType.VarChar, 31, generatorName);
                    AddParam(cmdRemote, "@opNEW_GENERATOR_VALUE", FbDbType.BigInt);

                    int updateCount = cmdRemote.ExecuteNonQuery();

                    if (updateCount != 1)
                    {
                        updateCount--;
                    }

                    newID = Convert.ToString((Int64)cmdRemote.Parameters["@opNEW_GENERATOR_VALUE"].Value);

                    UpdateReplicatedState(tranLocal, operationID, ReplicationStatus.Replicated);
                }
                finally
                {
                    CloseAndDispose(ref cmdRemote);
                }

                if (SQLRemote.StartsWith("INSERT") && !String.IsNullOrEmpty(generatorName) && newID != "-1" && 
                    !String.IsNullOrEmpty(primaryKey))
                {
                    RaiseIDChanged(Convert.ToInt64(oldID), Convert.ToInt64(newID), tableName);
                    LocalIDChanges.LocalChanges.Add(new UpdateLocalID(tableName, primaryKey, oldID, newID));

                    //update the local record ID with the change
                    string sqlLocalUpdateReplicated = String.Format("UPDATE {0} SET {1} = {2} WHERE {1} = {3};",
                        tableName, primaryKey, newID, oldID);
                    FbCommand cmdLocal1 = new FbCommand(sqlLocalUpdateReplicated, _LocalDB, tranLocal);
                    try
                    {
                        cmdLocal1.ExecuteNonQuery();
                    }
                    finally
                    {
                        CloseAndDispose(ref cmdLocal1);
                    }
                }

                Result = true;
            }
            catch (Exception err)
            {
                if (err.Message.Contains("violation of PRIMARY or UNIQUE KEY constraint"))
                {
                    UpdateReplicatedState(tranLocal, operationID, ReplicationStatus.ViolationOfUniqueKey);

                    if (!ProcessReplicationFailure(tranRemote, tranLocal, err.Message, SQLRemote, 
                        operationID, tableName, primaryKey, oldID, true))
                    {
                        AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}\r\n\r\n", 
                            err.Message, SQLRemote), false, tranLocal, tranRemote, operationID, SQLRemote, 
                            tableName, oldID, primaryKey, generatorName, fKey1, fKey2, fKey3, errors);
                    }
                }
                else if (err.Message.Contains("violation of FOREIGN KEY constraint"))
                {
                    string sqlLocalUpdateReplicated = "REPLICATE$AUTOFIXRECORD";
                    FbCommand cmdLocal1 = new FbCommand(sqlLocalUpdateReplicated, _LocalDB, tranLocal);
                    try
                    {
                        PrepareCommand(cmdLocal1);
                        AddParam(cmdLocal1, "@ipRECORDID", FbDbType.BigInt, operationID);
                        cmdLocal1.ExecuteNonQuery();
                    }
                    finally
                    {
                        CloseAndDispose(ref cmdLocal1);
                    }
                }
                else if (err.Message.Contains("attempt to store duplicate value (visible to active transactions) in unique index"))
                {
                    UpdateReplicatedState(tranLocal, operationID, ReplicationStatus.OperationIDIsDuplicated);

                    if (ProcessReplicationFailure(tranRemote, tranLocal, err.Message, SQLRemote, 
                        operationID, tableName, primaryKey, oldID, true))
                    {
                        AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}\r\n\r\n", 
                            err.Message, SQLRemote), false, tranLocal, tranRemote, operationID, SQLRemote, 
                            tableName, oldID, primaryKey, generatorName, fKey1, fKey2, fKey3, errors);
                    }
                }
                else if (err.Message.Contains("conversion error from string"))
                {
                    UpdateReplicatedState(tranLocal, operationID, ReplicationStatus.NotReplicated);

                    if (ProcessReplicationFailure(tranRemote, tranLocal, err.Message, SQLRemote, 
                        operationID, tableName, primaryKey, oldID, true))
                    {
                        AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}\r\n\r\n", 
                            err.Message, SQLRemote), false, tranLocal, tranRemote, operationID, SQLRemote, 
                            tableName, oldID, primaryKey, generatorName, fKey1, fKey2, fKey3, errors);
                    }
                }
                else if (err.Message.Contains("validation error for column"))
                {
                    UpdateReplicatedState(tranLocal, operationID, ReplicationStatus.NotReplicated);

                    if (ProcessReplicationFailure(tranRemote, tranLocal, err.Message, SQLRemote, 
                        operationID, tableName, primaryKey, oldID, true))
                    {
                        AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}\r\n\r\n", 
                            err.Message, SQLRemote), false, tranLocal, tranRemote, operationID, SQLRemote, 
                            tableName, oldID, primaryKey, generatorName, fKey1, fKey2, fKey3, errors);
                    }
                }
                else if (err.Message.ToLower().Contains("string was not in a correct format"))
                {
                    UpdateReplicatedState(tranLocal, operationID, ReplicationStatus.NotReplicated);

                    if (!ProcessReplicationFailure(tranRemote, tranLocal, err.Message, SQLRemote, 
                        operationID, tableName, primaryKey, oldID, true))
                    {
                        AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}\r\n\r\n", 
                            err.Message, SQLRemote), false, tranLocal, tranRemote, operationID, SQLRemote, 
                            tableName, oldID, primaryKey, generatorName, fKey1, fKey2, fKey3, errors);
                    }
                }
                else if (err.Message.Contains("arithmetic exception, numeric overflow, or string truncation"))
                {
                    if (!ProcessReplicationFailure(tranRemote, tranLocal, err.Message, SQLRemote, 
                        operationID, tableName, primaryKey, oldID, true))
                    {
                        AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}\r\n\r\n", 
                            err.Message, SQLRemote), false, tranLocal, tranRemote, operationID, SQLRemote, 
                            tableName, oldID, primaryKey, generatorName, fKey1, fKey2, fKey3, errors);
                    }
                }
                else if (err.Message.Contains("The command text for this Command has not been set"))
                {
                    if (!ProcessReplicationFailure(tranRemote, tranLocal, err.Message, SQLRemote, 
                        operationID, tableName, primaryKey, oldID, true))
                    {
                        AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}\r\n\r\n", 
                            err.Message, SQLRemote), false, tranLocal, tranRemote, operationID, SQLRemote, 
                            tableName, oldID, primaryKey, generatorName, fKey1, fKey2, fKey3, errors);
                    }
                }
                else
                {
                    if (ProcessReplicationFailure(tranRemote, tranLocal, err.Message, SQLRemote, operationID, 
                        tableName, primaryKey, oldID, true))
                    {
                        UpdateReplicatedState(tranLocal, operationID, ReplicationStatus.Replicated);
                    }
                    else
                    {
                        Shared.EventLog.Add(err);

                        if (err.StackTrace != null)
                            RaiseReplicationError(err.StackTrace.ToString());

                        throw;
                    }
                }
            }

            return (Result);
        }

        private void UpdateReplicatedState(FbTransaction tranLocal, Int64 operationID, ReplicationStatus replicatedStatus)
        {
            try
            {
                string sqlLocalUpdateReplicated = String.Format("UPDATE REPLICATE$OPERATIONLOG SET " +
                    "REPLICATED = {1} WHERE ID = {0}", operationID, (int)replicatedStatus);
                FbCommand cmdLocal1 = new FbCommand(sqlLocalUpdateReplicated, _LocalDB, tranLocal);
                try
                {
                    cmdLocal1.ExecuteNonQuery();
                }
                finally
                {
                    CloseAndDispose(ref cmdLocal1);
                }
            }
            catch(Exception err)
            {
                Shared.EventLog.LogError(MethodBase.GetCurrentMethod(), err, 
                    tranLocal, operationID, replicatedStatus);
            }
        }

        #endregion Replicated Data

        #region Master Replication

        /// <summary>
        /// Replicates a record from the master (remote) database to the local slave (local) database
        /// </summary>
        /// <param name="tranLocal">remote transaction</param>
        /// <param name="tranRemote">local transaction</param>
        /// <returns>true if record succesfully replicated</returns>
        private bool ReplicateChangesFromMasterToChild(FbTransaction tranLocal, FbTransaction tranRemote, 
            out ReplicationResult downloadResult, out Int64 maxLocal)
        {
            bool Result = false;
            downloadResult = ReplicationResult.Error;
            //  
            maxLocal = MaxOperationLogIDLocal(tranLocal);
            Int64 MaxRemote = MaxOperationLogIDRemote(tranRemote);

            Double Total = Convert.ToDouble(RemoteReplicationCount(tranRemote, maxLocal));

            string SQLLocal1 = String.Empty;
            string UpdateSQL = String.Empty;
            Int64 RemoteID = 0;
            string tableName = String.Empty;
            string primaryKey = String.Empty;
            string oldID = String.Empty;
            
            if (maxLocal >= MaxRemote)
            {
                maxLocal = MaxRemote;
                RaiseProgressText(String.Format("Replication upto date at: {0}", maxLocal));
                downloadResult = ReplicationResult.Completed;
                return (true);
            }

            try
            {
                try
                {
                    Int64 highestRemoteID = maxLocal + this.MaximumDownloadCount;

                    long maximumRecords = this.MaximumDownloadCount;

                    if (((maxLocal + Total) < MaxRemote) || (highestRemoteID > MaxRemote))
                    {
                        highestRemoteID = MaxRemote;
                        maximumRecords = (MaxRemote - maxLocal) + 10;
                    }

                    string SQLRemote = String.Format("SELECT FIRST {2} p.OPOPERATION_ID, p.OPTABLE, p.OPPRIMARY_KEY, " +
                        "p.OPPRIMARY_KEY_VALUE, p.OPSQL FROM REPLICATE$REPLICATECHANGES({0}, {1}) p", 
                        maxLocal, highestRemoteID, maximumRecords);
                    FbDataReader rdrRemote = null;
                    FbCommand cmdRemote = new FbCommand(SQLRemote, _RemoteDB, tranRemote);
                    try
                    {
                        cmdRemote.FetchSize = Shared.Utilities.CheckMinMax(this.MaximumDownloadCount, 200, 1000);
                        rdrRemote = cmdRemote.ExecuteReader();

                        int I = 0;

                        while (rdrRemote.Read())
                        {
                            try
                            {
                                SQLLocal1 = "REPLICATE$REMOTEUPDATES";
                                FbCommand cmdLocal1 = new FbCommand(SQLLocal1, _LocalDB, tranLocal);
                                try
                                {
                                    PrepareCommand(cmdLocal1);

                                    RemoteID = rdrRemote.GetInt64(0);
                                    tableName = rdrRemote.GetString(1);
                                    primaryKey = rdrRemote.GetString(2);
                                    oldID = rdrRemote.GetString(3);
                                    UpdateSQL = rdrRemote.GetString(4);

                                    RaiseProgressEvent(-279, UpdateSQL);

                                    AddParam(cmdLocal1, "@ipOPERATIONID", FbDbType.BigInt, RemoteID);
                                    AddParam(cmdLocal1, "@ipSQL", FbDbType.Text, UpdateSQL);

                                    cmdLocal1.ExecuteNonQuery();
                                }
                                finally
                                {
                                    CloseAndDispose(ref cmdLocal1);
                                }

                                Result = true;
                            }
                            catch (Exception err)
                            {
                                if ((err.Message.Contains("violation of FOREIGN KEY constraint") ||
                                    err.Message.Contains("Foreign key reference target does not exist") ||
                                    err.Message.Contains("violation of PRIMARY or UNIQUE KEY") ||
                                    err.Message.Contains("attempt to store duplicate value")))
                                {
                                    // the above errors will be ignored when downloading from the master database
                                    //if (!ProcessReplicationFailure(tranRemote, tranLocal, err.Message, UpdateSQL, RemoteID, tableName, primaryKey, oldID, false))
                                    //{
                                    //    AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}", err.Message, SQLRemote), true, tranLocal, tranRemote);
                                    //    Shared.EventLog.Add(err, String.Format("{0} {1}", RemoteID, UpdateSQL));
                                    //}
                                }
                                else
                                {
                                    if (!ProcessReplicationFailure(tranRemote, tranLocal, err.Message, UpdateSQL, 
                                        RemoteID, tableName, primaryKey, oldID, false))
                                    {
                                        AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}", 
                                            err.Message, SQLRemote), true, tranLocal, tranRemote);
                                        Shared.EventLog.Add(err, String.Format("{0} {1}", RemoteID, UpdateSQL));
                                        return (false);
                                    }
                                }
                            }

                            I++;

                            int Perc = Convert.ToInt32((100 / Total) * I);
                            RaiseProgressEvent(Perc > 100 ? 100 : Perc, String.Format("{0} of {1}", I, (int)Total));


                            // has the user cancelled the replication
                            if (RaiseCheckCancel())
                            {
                                RaiseProgressText("User Cancelled Replication");
                                downloadResult = ReplicationResult.Cancelled;
                                return (false);
                            }

                            if (TimeOutExceeded)
                            {
                                RaiseProgressText("Timeout Exceeded");
                                downloadResult = ReplicationResult.TimeOutExceeded;
                                return (false);
                            }

                            if (I >= MaximumDownloadCount)
                            {
                                RaiseProgressText("Maximum Download Threshold reached");
                                downloadResult = ReplicationResult.ThresholdExceeded;
                                break;
                            }

                        } // while

                        if (highestRemoteID < MaxRemote)
                        {
                            downloadResult = ReplicationResult.ThresholdExceeded;
                        }

                        Int64 newValue = maxLocal + this.MaximumDownloadCount;

                        if (newValue > MaxRemote)
                            newValue = MaxRemote;

                        if (RemoteID > 0 && newValue > RemoteID)
                        {
                            newValue = RemoteID;
                            downloadResult = ReplicationResult.Completed;
                        }

                        if (RemoteID > 0 && newValue < RemoteID)
                        {
                            downloadResult = ReplicationResult.ThresholdExceeded;
                        }

                        if (newValue > 0)
                        {
                            // if there is no downloads and the max local is less than MaxRemote > maxlocal by
                            // less than maximumRecords then return max remote (i.e. upto date) otherwise
                            // set maxLocal to the hightest Remote ID, or zero if none as the value will only
                            // be updated if its above zero
                            if (RemoteID == 0 && (maxLocal + (maximumRecords - 10)) == MaxRemote)
                            {
                                maxLocal = MaxRemote;
                            }
                            else
                            {
                                maxLocal = RemoteID;
                            }

                            if (newValue == MaxRemote)
                                downloadResult = ReplicationResult.Completed;
                        }
                    }
                    finally
                    {
                        CloseAndDispose(ref cmdRemote, ref rdrRemote);
                    }

                    Result = false;

                    if (downloadResult == ReplicationResult.ThresholdExceeded)
                        return (false);

                    Result = true;
                }
                catch (Exception errInner)
                {
                    Shared.EventLog.Add(errInner);

                    if (errInner.Message.Contains("update conflicts with concurrent update"))
                    {
                        //it happens, wait for next iteration
                    }
                    else
                    {
                        RaiseReplicationError(String.Format("Remote ID: {0}", RemoteID));
                        RaiseReplicationError(SQLLocal1);
                        RaiseReplicationError(errInner.Message);
                        throw;
                    }
                }
            }
            catch (Exception errOuter)
            {
                Shared.EventLog.Add(errOuter);

                if (errOuter.Message.Contains("update conflicts with concurrent update"))
                {
                    //it happens, wait for next iteration
                }
                else
                {
                    RaiseReplicationError(UpdateSQL);
                    RaiseReplicationError(errOuter.Message);
                    throw;
                }
            }

            return (Result);
        }

        private void ReplicateChangesFromMasterToChild(FbTransaction tranLocal, FbTransaction tranRemote, string tableName, 
            string idColumn, string idValue)
        {
            string SQLLocal1 = "";
            string UpdateSQL = "";

            try
            {
                string SQLRemote = String.Format("SELECT p.OPOPERATION_ID, p.OPSQL " +
                    "FROM REPLICATE$MISSINGRECORD('{0}', '{1}', '{2}') p", tableName.Trim(), idColumn, idValue);
                FbDataReader rdrRemote = null;
                FbCommand cmdRemote = new FbCommand(SQLRemote, _RemoteDB, tranRemote);
                try
                {
                    rdrRemote = cmdRemote.ExecuteReader();

                    while (rdrRemote.Read())
                    {
                        SQLLocal1 = "REPLICATE$REMOTEUPDATES";// rdrRemote.GetString(1);
                        FbCommand cmdLocal1 = new FbCommand(SQLLocal1, _LocalDB, tranLocal);
                        try
                        {
                            // the following is wrapped in a procedure to prevent the item being re-added to
                            // the operation log, the procedure must set the replicating context

                            PrepareCommand(cmdLocal1);
                            RaiseProgressEvent(-279, rdrRemote.GetString(1));

                            AddParam(cmdLocal1, "@ipOPERATIONID", FbDbType.BigInt, rdrRemote.GetInt64(0));
                            AddParam(cmdLocal1, "@ipSQL", FbDbType.Text, rdrRemote.GetString(1));

                            int updateCount = cmdLocal1.ExecuteNonQuery();

                            if (updateCount == 0)
                            {// can add breakpoint here

                            }
                        }
                        catch (Exception err)
                        {
                            if ((err.Message.Contains("violation of FOREIGN KEY constraint") || 
                                err.Message.Contains("Foreign key reference target does not exist")))
                            {
                                if (!ProcessReplicationFailure(tranRemote, tranLocal, err.Message, SQLRemote,
                                    0, tableName, idColumn, idValue, false))
                                {
                                    AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}\r\n\r\n", 
                                        err.Message, SQLRemote), true, tranLocal, tranRemote, tableName, idColumn, idValue);
                                }
                            }
                            else
                            {
                                if (err.Message.Contains("violation of PRIMARY or UNIQUE KEY") || 
                                    err.Message.Contains("attempt to store duplicate value"))
                                {
                                    if (!ProcessReplicationFailure(tranRemote, tranLocal, err.Message, SQLRemote, 0,
                                        tableName, idColumn, idValue, false))
                                    {
                                        AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}\r\n\r\n", 
                                            err.Message, SQLRemote), true, tranLocal, tranRemote, tableName, idColumn, idValue);
                                    }
                                }
                                else
                                {
                                    if (!ProcessReplicationFailure(tranRemote, tranLocal, err.Message, SQLRemote, 0, 
                                        tableName, idColumn, idValue, false))
                                    {
                                        AddRecordFailure(MethodBase.GetCurrentMethod(), String.Format("{0}\r\n\r\n{1}\r\n\r\n", 
                                            err.Message, SQLRemote), true, tranLocal, tranRemote, tableName, idColumn, idValue);
                                        RaiseReplicationError(UpdateSQL);
                                        RaiseReplicationError(String.Format("Table: {0}; Column: {1}; Value: {2}", 
                                            tableName, idColumn, idValue));
                                        throw;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            CloseAndDispose(ref cmdLocal1);
                        }

                        // has the user cancelled replication
                        if (RaiseCheckCancel() | TimeOutExceeded)
                            return;
                    } // while
                }
                catch (Exception errInner)
                {
                    Shared.EventLog.Add(errInner);
                    RaiseReplicationError(SQLLocal1);
                    RaiseReplicationError(errInner.Message);
                    throw;
                }
                finally
                {
                    CloseAndDispose(ref cmdRemote, ref rdrRemote);
                }
            }
            catch (Exception errOuter)
            {
                Shared.EventLog.Add(errOuter);
                RaiseReplicationError(UpdateSQL);
                RaiseReplicationError(errOuter.Message);
                throw;
            }
        }

        /// <summary>
        /// Initiates a deep scan of all tables confirming each record is present in both master and child database
        /// </summary>
        /// <param name="local">Connection to slave database</param>
        /// <param name="remote">Connection to master database</param>
        /// <param name="tranLocal">Existing transaction to slave database</param>
        /// <param name="tranRemote">Existing transaction to master database</param>
        private void HardConfirmReplicationCountsAllTables(FbConnection local, FbConnection remote, 
            FbTransaction tranLocal, FbTransaction tranRemote)
        {
            FbDataReader rdrReplicatedTables = null;
            string SQL = "SELECT DISTINCT(rt.TABLE_NAME), rt.OPTIONS FROM REPLICATE$TABLES rt WHERE rt.OPERATION IN " +
                "('DELETE', 'INSERT') AND rt.TABLE_NAME <> 'WS_EMAIL' " +

                " AND rt.TABLE_NAME IN (SELECT DISTINCT TRIM(a.RDB$RELATION_NAME) " +
                " FROM RDB$RELATION_FIELDS a " +
                " GROUP BY a.RDB$RELATION_NAME) " +


                "ORDER BY rt.SORT_ORDER, rt.TABLE_NAME;";
            FbCommand cmdLocalReplicatedTables = new FbCommand(SQL, local, tranLocal);
            try
            {
                rdrReplicatedTables = cmdLocalReplicatedTables.ExecuteReader();

                // loop through all tables that are being replicated
                while (rdrReplicatedTables.Read())
                {
                    //retrieve record counts for master and slave table
                    string tableName = rdrReplicatedTables.GetString(0).Trim();
                    TableOptions tableOptions = (TableOptions)rdrReplicatedTables.GetInt64(1);

                    TableStatus tableStatus = Statuses.Find(tableName, tableOptions.HasFlag(TableOptions.Ascending));

                    if (tableStatus.ConfirmedChild && tableStatus.ConfirmedMaster)
                    {
                        RaiseProgressText(String.Format("Table Confirmed: {0}", tableName));
                        continue;
                    }

                    Int64 localCount = 0;
                    Int64 remoteCount = 0;
                    bool tableMissing = false;

                    FbDataReader rdrLocalCount = null;
                    FbCommand cmdLocalCount = new FbCommand(String.Format("SELECT COUNT(*) FROM {0}", tableName), 
                        local, tranLocal);
                    try
                    {
                        rdrLocalCount = cmdLocalCount.ExecuteReader();

                        if (rdrLocalCount.Read())
                            localCount = rdrLocalCount.GetInt64(0);
                    }
                    finally
                    {
                        CloseAndDispose(ref cmdLocalCount, ref rdrLocalCount);
                    }

                    FbDataReader rdrRemoteCount = null;
                    FbCommand cmdRemoteCount = new FbCommand(String.Format("SELECT COUNT(*) FROM {0}", tableName), 
                        remote, tranRemote);
                    try
                    {
                        try
                        { 
                            rdrRemoteCount = cmdRemoteCount.ExecuteReader();

                            if (rdrRemoteCount.Read())
                                remoteCount = rdrRemoteCount.GetInt64(0);
                        }
                        catch (Exception err)
                        {
                            if (err.Message.Contains("Table unknown"))
                            {
                                continue;
                            }
                            else
                                throw;
                        }
                    }
                    finally
                    {
                        CloseAndDispose(ref cmdRemoteCount, ref rdrRemoteCount);
                    }

                    // has the user cancelled replication
                    if (RaiseCheckCancel() | TimeOutExceeded)
                        return;

                    // user can limit the number of record failures, to enhance speed
                    // if the limit has been reached then exit the routine
                    if (MissingRecordCount >= ForceRestartErrorCount)
                    {
                        RaiseProgressText(String.Format("Force Reset Limit Reached!"));
                        return;
                    }

                    // if a table fails and needs to be re-run because of rule changes then there is 
                    // no point continuing to validate dependant tables as they will possibly fail
                    // until master record has been fixed either automatically via rules engine or 
                    // manually
                    bool skipifSuccessful = true;

                    if (CanValidateTable(tableName, tableOptions, ref skipifSuccessful))
                    {
                        RaiseProgressText(String.Format("Verifying Table {0}; Local: {1}; Remote: {2};", 
                            tableName, localCount, remoteCount));

                        long missingRemote = 0;
                        long missingLocal = 0;

                        if (!tableStatus.ConfirmedChild && !tableOptions.HasFlag(TableOptions.DoNotVerifyChild))
                        {
                            missingLocal = ConfirmReplicationByTableMasterToChild(tranLocal, tranRemote,
                                 tableName, remoteCount, tableStatus, tableOptions, true);

                            // no records missing indicates child scan complete
                            if (RaiseCheckCancel() | TimeOutExceeded)
                                return;

                            if (missingLocal == 0)
                                tableStatus.ConfirmedChild = true;
                        }
                        else
                        {
                            if (tableOptions.HasFlag(TableOptions.DoNotVerifyChild))
                                RaiseProgressText("Child Table Skipped as not set to verify");
                            else
                                RaiseProgressText("Child Table Skipped as upto date");
                        }

                        if (MissingRecordCount < ForceRestartErrorCount && !tableStatus.ConfirmedMaster &&
                             !tableOptions.HasFlag(TableOptions.DoNotVerifyMaster))
                        {
                            missingRemote = ConfirmReplicationByTableChildToMaster(tranLocal, tranRemote,
                                tableName, localCount, tableStatus, tableOptions, true);

                            if (RaiseCheckCancel() | TimeOutExceeded)
                                return;

                            // no missing records indicates master scan complete
                            if (missingRemote == 0)
                                tableStatus.ConfirmedMaster = true;
                        }
                        else
                        {
                            if (tableOptions.HasFlag(TableOptions.DoNotVerifyMaster))
                                RaiseProgressText("Master Table Skipped as not set to verify");
                            else if (tableStatus.ConfirmedMaster)
                                RaiseProgressText("Master Table Skipped as upto date");
                        }
                    }
                    else
                    {
                        if (tableOptions.HasFlag(TableOptions.DoNotVerify))
                        {
                            RaiseProgressText(String.Format("Table {0} is not configured for verification", tableName));
                        }
                        else
                        {
                            if (skipifSuccessful)
                                RaiseProgressText(String.Format("Table {0} skipped as upto date", tableName));
                            else
                                RaiseProgressText(String.Format("Table {0} skipped due to failures on master table", tableName));
                        }
                    }
                } // while
            }
            finally
            {
                CloseAndDispose(ref cmdLocalReplicatedTables, ref rdrReplicatedTables);
            }
        }

        /// <summary>
        /// Adds a table to the failed table list
        /// </summary>
        /// <param name="tableName">table that failed replication and an automatic fix was applied</param>
        private void AddFailedTable(string tableName)
        {
            string table = String.Format(";{0}#", tableName.ToUpper());

            if (!_failedTables.Contains(table))
                _failedTables += table;
        }

        /// <summary>
        /// Determines wether a table is dependant on a table that has previously failed to validate
        /// due to rule changes if so no point validating dependant tables
        /// </summary>
        /// <param name="tableToConfirm">Name of table to validate</param>
        /// <param name="previouslySuccessful">true if table is being skipped as it is up to date</param>
        /// <returns>true if the table can be validated, otherwise false</returns>
        private bool CanValidateTable(string tableToConfirm, TableOptions options, ref bool previouslySuccessful)
        {
            bool Result = true;
            previouslySuccessful = true;

            if (options.HasFlag(TableOptions.DoNotVerify))
            {
                previouslySuccessful = false;
                return (false);
            }

            string table = String.Format(";{0}#", tableToConfirm.ToUpper());

            previouslySuccessful = false;

            //check rules engine to see if the table can be skipped also
            if (!String.IsNullOrEmpty(_failedTables))
            {
                AutoCorrectRules rules = _autoCorrectRules;

                foreach (AutoCorrectRule rule in rules)
                {
                    if (rule.TableName == tableToConfirm)
                    {
                        if (!String.IsNullOrEmpty(rule.Dependencies))
                        {
                            string[] dependencies = rule.Dependencies.Split(';');

                            foreach (string dependantTable in dependencies)
                            {
                                if (_failedTables.Contains(String.Format("{0}#", dependantTable)))
                                    return (false);
                            }
                        }
                    } // if table name
                } // foreach
            }

            return (Result);
        }

        private string QuoteString(string s)
        {
            string Result = "'" + PrepareString(s) + "'";

            return (Result);
        }

        private Int64 MaxOperationLogIDLocal(FbTransaction tranLocal)
        {
            Int64 Result = 0;
            string SQLLocal = "SELECT GEN_ID(REPLICATE$REMOTE_LOG_ID, 0) FROM RDB$DATABASE;";
            FbDataReader rdrLocal = null;
            FbCommand cmdLocal = new FbCommand(SQLLocal, _LocalDB, tranLocal);
            try
            {
                rdrLocal = cmdLocal.ExecuteReader();

                if (rdrLocal.Read())
                {
                    Result = rdrLocal.IsDBNull(0) ? 0 : rdrLocal.GetInt64(0);
                }
                else
                    throw new Exception("Unable to determine REPLICATE$REMOTE_LOG_ID Max ID");
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
                RaiseReplicationError(err.Message);
                throw;
            }
            finally
            {
                CloseAndDispose(ref cmdLocal, ref rdrLocal);
            }

            return (Result);
        }

        private Int64 MaxOperationLogIDRemote(FbTransaction tranRemote)
        {
            Int64 Result = 0;
            string SQLRemote = "EXECUTE BLOCK RETURNS (maxID BIGINT) " +
                "AS " +
                "  DECLARE VARIABLE vRepID BIGINT; " +
                "  DECLARE VARIABLE vNewMax BIGINT; " +
                "BEGIN " +
                "    SELECT a.SITE_ID " +
                "    FROM REPLICATE$OPTIONS a  " +
                "    WHERE a.SITE_ID = 0 WITH LOCK " +
                "    INTO :vRepID; " +
                "    maxID = GEN_ID(REPLICATE$OPERATIONLOG_ID, 0); " +
                "    SELECT GEN_ID(REPLICATE$MAXIMUM_ID, :maxID - GEN_ID(REPLICATE$MAXIMUM_ID, 0))  " +
                "    FROM RDB$DATABASE " +
                "    INTO :vNewMax; " +
                "    SUSPEND; " +
                "    WHEN ANY DO " +
                "    BEGIN " +
                "        maxID = GEN_ID(REPLICATE$MAXIMUM_ID, 0); " +
                "        SUSPEND; " +
                "    END " +
                "END  ";

            FbDataReader rdrRemote = null;
            FbCommand cmdRemote = new FbCommand(SQLRemote, _RemoteDB, tranRemote);
            try
            {
                rdrRemote = cmdRemote.ExecuteReader();

                if (rdrRemote.Read())
                {
                    Result = rdrRemote.IsDBNull(0) ? 0 : rdrRemote.GetInt64(0);
                }
                else
                    throw new Exception("Unable to determine REPLICATE$OPERATIONLOG_ID Max ID");
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
                RaiseReplicationError(err.Message);
                throw;
            }
            finally
            {
                CloseAndDispose(ref cmdRemote, ref rdrRemote);
            }

            return (Result);
        }

        /// <summary>
        /// Returns the total number of items which require replicating from live to local
        /// </summary>
        /// <param name="tranRemote"></param>
        /// <param name="maxLocal"></param>
        /// <returns></returns>
        private Int64 RemoteReplicationCount(FbTransaction tranRemote, Int64 maxLocal)
        {
            Int64 Result = 0;
            string SQLRemote = String.Format("SELECT p.OPUPDATES FROM REPLICATION$REMOTECOUNT({0}) p", maxLocal);
            FbDataReader rdrRemote = null;
            FbCommand cmdRemote = new FbCommand(SQLRemote, _RemoteDB, tranRemote);
            try
            {
                rdrRemote = cmdRemote.ExecuteReader();

                if (rdrRemote.Read())
                {
                    Result = rdrRemote.IsDBNull(0) ? 0 : rdrRemote.GetInt64(0);
                }
                else
                    throw new Exception("Unable to determine GEN_IBLM$OPERATIONLOG_ID Max ID");
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
                RaiseReplicationError(err.Message);
                throw;
            }
            finally
            {
                CloseAndDispose(ref cmdRemote, ref rdrRemote);
            }

            return (Result);
        }

        private int MaxClientID(FbTransaction tranRemote)
        {
            int Result = 0;
            string SQLRemote = "SELECT GEN_ID(GEN_WS_CLIENTS_ID, 0) FROM RDB$DATABASE;";
            FbDataReader rdrRemote = null;
            FbCommand cmdRemote = new FbCommand(SQLRemote, _RemoteDB, tranRemote);
            try
            {
                rdrRemote = cmdRemote.ExecuteReader();

                if (rdrRemote.Read())
                {
                    Result = rdrRemote.IsDBNull(0) ? 0 : rdrRemote.GetInt32(0);
                }
                else
                    throw new Exception("Unable to determine Client Max ID");
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
                throw;
            }
            finally
            {
                CloseAndDispose(ref cmdRemote, ref rdrRemote);
            }

            return (Result);
        }


        #endregion Master Replication

        #region Database

        #region Connection Methods

        /// <summary>
        /// Initialises and opens a connection to master and slave databases
        /// </summary>
        private bool ConnectToDatabases()
        {
            bool Result = false;
            try
            {
                if (_canReplicate)
                {
                    _LocalDB = new FbConnection(FixConnectionString(ChildDatabase));

                    _LocalDB.Open();
                    RaiseProgressText("Connected To Local Database");
                    LocalDatabaseAttachmentID = GetDatabaseCurrentConnection(_LocalDB);
                    _siteID = GetDatabaseCurrentSiteID(_LocalDB);

                    //connect to remote DB
                    _RemoteDB = new FbConnection(FixConnectionString(MasterDatabase));
                    _RemoteDB.Open();
                    RaiseProgressText("Connected To Remote Database");
                    RemoteDatabaseAttachmentID = GetDatabaseCurrentConnection(_RemoteDB);
                }

                Result = true;
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
                RaiseProgressText(err.Message);
            }

            return (Result);
        }

        /// <summary>
        /// Closes connections to master and slave databases
        /// </summary>
        private void DisconnectFromDatabases()
        {
            if (_LocalDB != null)
            {
                RaiseProgressText("Disconnect From Local Database");
                _LocalDB.Close();
                _LocalDB.Dispose();
                _LocalDB = null;
            }

            if (_RemoteDB != null)
            {
                RaiseProgressText("Disconnect From Remote Database");
                _RemoteDB.Close();
                _RemoteDB.Dispose();
                _RemoteDB = null;
            }
        }

        /// <summary>
        /// Retrieves the connection id for the current connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private Int64 GetDatabaseCurrentConnection(FbConnection connection)
        {
            Int64 Result = -10;

            FbTransaction tran = connection.BeginTransaction();
            try
            {
                string SQL = "SELECT CURRENT_CONNECTION FROM RDB$DATABASE;";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, connection, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                        Result = rdr.GetInt64(0);
                }
                finally
                {
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                tran.Rollback();
                tran.Dispose();
            }

            return (Result);
        }

        /// <summary>
        /// Retrieves the connection id for the current connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private Int64 GetDatabaseCurrentSiteID(FbConnection connection)
        {
            Int64 Result = -10;

            FbTransaction tran = connection.BeginTransaction();
            try
            {
                string SQL = "SELECT FIRST 1 a.SITE_ID FROM REPLICATE$OPTIONS a;";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, connection, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                        Result = rdr.GetInt64(0);
                }
                finally
                {
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                tran.Rollback();
                tran.Dispose();
            }

            return (Result);
        }

        /// <summary>
        /// Retrieves the connection id for the current connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private void SetRemoteContext(FbTransaction transaction, Int64 siteID)
        {
            string SQL = String.Format("SELECT RDB$SET_CONTEXT('USER_TRANSACTION', 'CLIENT_ID', {0}) FROM RDB$DATABASE;",
                siteID == -1 ? "NULL" : siteID.ToString());
            FbDataReader rdr = null;
            FbCommand cmd = new FbCommand(SQL, _RemoteDB, transaction);
            try
            {
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {

                }
            }
            finally
            {
                CloseAndDispose(ref cmd, ref rdr);
            }
        }

        private string FixConnectionString(string connectionString)
        {
            FbConnectionStringBuilder csb = new FbConnectionStringBuilder(connectionString)
            {
                MaxPoolSize = 1,
                Pooling = false
            };

            return (csb.ToString());
        }

        #endregion Connection Methods

        #region Helper Methods

        private string PrepareString(string s)
        {
            return (s.Replace("'", "''"));
        }

        #endregion Helper Methods

        #endregion Database

        #region Logging

        /// <summary>
        /// Logs an error
        /// </summary>
        /// <param name="method">Method where error occured</param>
        /// <param name="ex">Exception created</param>
        /// <param name="values">variable values</param>
        private static void LogError(MethodBase method, Exception ex, params object[] values)
        {
            ParameterInfo[] parms = method.GetParameters();
            object[] namevalues = new object[2 * parms.Length];

            string msg = "Error in " + method.Name + "(";

            for (int i = 0, j = 0; i < parms.Length; i++, j += 2)
            {
                msg += "{" + j + "}={" + (j + 1) + "}, ";
                namevalues[j] = parms[i].Name;

                if (i < values.Length)
                    namevalues[j + 1] = values[i] == null ? "null" : values[i].ToString();
            }

            msg += ") exception: \r\n" + ex.Message + "\r\n" + 
                ex.StackTrace == null ? "No Stack Trace" : ex.StackTrace.ToString();
            Shared.EventLog.Add(ex, String.Format(msg, namevalues));
        }

        #endregion Logging

        #region Global Procs

        private void UpdateVarIfNotEmpty(string Data, ref string Value)
        {
            if (Data != null && Data != "")
                Value = Data;
        }

        private void PrepareCommand(FbCommand cmd)
        {
            PrepareCommand(cmd, CommandType.StoredProcedure);
        }

        private void PrepareCommand(FbCommand cmd, CommandType CmdType)
        {
            cmd.CommandType = CmdType;
        }

        /// <summary>
        /// Output paramater
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ParamName"></param>
        /// <param name="ParamType"></param>
        private void AddParam(FbCommand cmd, string ParamName, FbDbType ParamType)
        {
            cmd.Parameters.Add(ParamName, ParamType);
            cmd.Parameters[ParamName].Direction = ParameterDirection.Output;
        }

        /// <summary>
        /// Parameter, user specified direction/type
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ParamName"></param>
        /// <param name="ParamType"></param>
        /// <param name="Direction"></param>
        /// <param name="ParamValue"></param>
        private void AddParam(FbCommand cmd, string ParamName, FbDbType ParamType,
            ParameterDirection Direction, object ParamValue)
        {
            cmd.Parameters.Add(ParamName, ParamType);
            cmd.Parameters[ParamName].Direction = Direction;
            cmd.Parameters[ParamName].Value = ParamValue;
        }

        /// <summary>
        /// Adds varchar output paramater
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ParamName"></param>
        /// <param name="ParamType"></param>
        /// <param name="Length"></param>
        private void AddParamOutput(FbCommand cmd, string ParamName, int Length)
        {
            cmd.Parameters.Add(ParamName, FbDbType.VarChar);
            cmd.Parameters[ParamName].Size = Length;
            cmd.Parameters[ParamName].Direction = ParameterDirection.Output;
        }

        /// <summary>
        /// Adds non varchar output param
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ParamName"></param>
        /// <param name="ParamType"></param>
        private void AddParamOutput(FbCommand cmd, string ParamName, FbDbType ParamType)
        {
            cmd.Parameters.Add(ParamName, ParamType);
            cmd.Parameters[ParamName].Direction = ParameterDirection.Output;
        }

        /// <summary>
        /// Input paramater, specify type
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ParamName"></param>
        /// <param name="ParamType"></param>
        /// <param name="ParamValue"></param>
        private void AddParam(FbCommand cmd, string ParamName, FbDbType ParamType,
            object ParamValue)
        {
            cmd.Parameters.Add(ParamName, ParamType);
            cmd.Parameters[ParamName].Direction = ParameterDirection.Input;
            cmd.Parameters[ParamName].Value = ParamValue;
        }

        /// <summary>
        /// Input paramater for type Char/Varchar
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ParamName"></param>
        /// <param name="ParamType"></param>
        /// <param name="TextLength"></param>
        /// <param name="ParamValue"></param>
        private void AddParam(FbCommand cmd, string ParamName, FbDbType ParamType,
            int TextLength, object ParamValue)
        {
            cmd.Parameters.Add(ParamName, ParamType);
            cmd.Parameters[ParamName].Size = TextLength;
            cmd.Parameters[ParamName].Direction = ParameterDirection.Input;
            cmd.Parameters[ParamName].Value = ParamValue;
        }

        #endregion Global Procs

        #region Fix Data
    
        /// <summary>
        /// When a record fails to replicate, this routine will check a rules engine to see if
        /// there is a rule that can automatically fixe data in the local table
        /// </summary>
        /// <param name="remoteTran">Currently active remote transaction</param>
        /// <param name="localTran">Currently active local transaction</param>
        /// <param name="errorMessage">Generated error Message</param>
        /// <param name="sqlStatement">SQL Statement</param>
        /// <param name="operationID">Operation ID of record</param>
        /// <param name="recordID">ID of record to be fixed</param>
        /// <param name="localFix">Is the fix being applied to local or remote database</param>
        /// <param name="tableName"></param>
        /// <param name="recordColumn"></param>
        /// <param name="pass">The number of times this method has been called</param>
        private bool ProcessReplicationFailure(FbTransaction remoteTran, FbTransaction localTran,
            string errorMessage, string sqlStatement, Int64 operationID, string tableName, string recordColumn,  
            string recordID,  bool localFix)
        {
            bool Result = false;

            AutoCorrectRules rules = _autoCorrectRules;
            
            foreach (AutoCorrectRule rule in rules)
            {
                if (errorMessage.Contains(rule.KeyName) && rule.TableName.ToUpper() == tableName)
                {
                    if (rule.Options.HasFlag(AutoFixOptions.AppendExtraChar))
                    {
                        string SQL = String.Format(rule.SQLRuleLocal, recordID);

                        if (localFix)
                        {
                            Result = FixRecordLocal(localTran, rule, rule.TargetColumn, tableName, recordColumn, recordID);
                        }
                        else
                        {
                            Result = FixRecordRemote(remoteTran, localTran, rule, rule.TargetColumn, tableName, 
                                recordColumn, recordID);
                        }

                        break;
                    }

                    if (rule.Options.HasFlag(AutoFixOptions.AttemptIDRemote) && localFix)
                    {
                        Result = FixRecordRemoteID(remoteTran, localTran, rule, recordID);

                        break;
                    }
                        
                    if ((rule.Options.HasFlag(AutoFixOptions.AttemptIDLocal) && localFix) || 
                        (rule.Options.HasFlag(AutoFixOptions.AttemptIDLocal) && !localFix) || 
                        (rule.Options.HasFlag(AutoFixOptions.AttemptIDRemote) && !localFix))
                    {
                        Result = FixRecordLocalID(remoteTran, localTran, rule, recordID, sqlStatement, 
                            operationID, tableName, recordColumn, recordID, localFix);

                        break;
                    }

                    if (rule.Options.HasFlag(AutoFixOptions.IgnoreRecord))
                    {
                        Result = true;

                        break;
                    }
                }
            }

            if (Result)
            {
                // an auto fix based on rule has been applied, add to list
                // of failed tables so no action is taken with dependant tables
                // on this run
                AddFailedTable(tableName);
            }
            else
            {
                Shared.EventLog.LogError(MethodBase.GetCurrentMethod(), "Missing Rule in Replication",
                    remoteTran, localTran, errorMessage, sqlStatement, operationID, tableName, recordColumn, recordID, localFix);
            }

            return (Result);
        }

        private bool FixRecordRemote(FbTransaction tranRemote, FbTransaction tranLocal, AutoCorrectRule rule, 
            string targetColumn, string tableName, string recordColumn, string recordID)
        {
            bool Result = false;

            //get the value of the clashing record
            string RemoteSQL = String.Format("SELECT {0} FROM {1} WHERE {2} = {3};",
                targetColumn, tableName, recordColumn, recordID);
            FbDataReader localRdr = null;
            FbCommand localCmd = new FbCommand(RemoteSQL, _LocalDB, tranLocal);
            try
            {
                localRdr = localCmd.ExecuteReader();

                if (localRdr.Read())
                {
                    string currentValue = localRdr.GetString(0);

                    //now update the local copy with the new 
                    string[] ruleOptions = rule.SQLRuleRemote.Split(';');
                    string localSQL = ruleOptions[0];
                    FbCommand localCmdUpdate = new FbCommand(localSQL, _LocalDB, tranLocal);
                    try
                    {
                        AddParam(localCmdUpdate, "PARAM0", FbDbType.VarChar, Convert.ToInt32(ruleOptions[1]), currentValue);
                        localCmdUpdate.ExecuteNonQuery();
                    }
                    finally
                    {
                        CloseAndDispose(ref localCmdUpdate);
                    }

                    //attempt to fix record made, add the table to the failed table
                    //list so that validation on dependant tables will not be validated
                    AddFailedTable(tableName);

                    Result = true;
                }
                else
                {
                    FbDataReader remoteRdr = null;
                    FbCommand remoteCmd = new FbCommand(RemoteSQL, _RemoteDB, tranRemote);
                    try
                    {
                        remoteRdr = remoteCmd.ExecuteReader();

                        if (remoteRdr.Read())
                        {
                            string currentValue = remoteRdr.GetString(0);

                            //now update the local copy with the new 
                            string[] ruleOptions = rule.SQLRuleRemote.Split(';');
                            string localSQL = ruleOptions[0];
                            FbCommand localCmdUpdate = new FbCommand(localSQL, _LocalDB, tranLocal);
                            try
                            {
                                AddParam(localCmdUpdate, "PARAM0", FbDbType.VarChar, Convert.ToInt32(ruleOptions[1]), currentValue);
                                localCmdUpdate.ExecuteNonQuery();
                            }
                            finally
                            {
                                CloseAndDispose(ref localCmd);
                            }

                            //attempt to fix record made, add the table to the failed table
                            //list so that validation on dependant tables will not be validated
                            AddFailedTable(tableName);

                            Result = true;
                        }
                    }
                    finally
                    {
                        CloseAndDispose(ref remoteCmd, ref remoteRdr);
                    }
                }
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
                Result = false;
            }
            finally
            {
                CloseAndDispose(ref localCmd, ref localRdr);
            }

            return (Result);
        }

        private bool FixRecordLocal(FbTransaction tranLocal, AutoCorrectRule rule, string targetColumn, string tableName,
            string recordColumn, string recordID)
        {
            bool Result = false;

            try
            {
                string sqlUpdateReplicationRecord = String.Format("UPDATE REPLICATE$COLUMNLOG a SET a.NEW_VALUE = a.NEW_VALUE || '~lf' WHERE (a.OPERATIONLOG_ID IN (SELECT ol.ID " +
                    "FROM REPLICATE$OPERATIONLOG ol WHERE ol.TABLE_NAME = '{1}' AND ol.PKEY1 = '{2}' AND ol.PKEY1_VALUE = '{3}' " +
                    "AND ol.OPERATION = 'INSERT')) AND a.COLUMN_NAME = '{4}';",
                    rule.SQLRuleLocal, tableName, recordColumn, recordID, targetColumn);
                FbCommand localCmd = new FbCommand(sqlUpdateReplicationRecord, _LocalDB, tranLocal);
                try
                {
                    localCmd.ExecuteNonQuery();
                }
                finally
                {
                    CloseAndDispose(ref localCmd);
                }

                string[] ruleOptions = rule.SQLRuleLocal.Split(';');
                string sqlUpdateRecord = ruleOptions[0];
                localCmd = new FbCommand(sqlUpdateRecord, _LocalDB, tranLocal);
                try
                { 
                    AddParam(localCmd, "@PARAM0", FbDbType.VarChar, Convert.ToInt32(ruleOptions[1]), recordID);
                    localCmd.ExecuteNonQuery();
                }
                finally
                {
                    CloseAndDispose(ref localCmd);
                }

                //attempt to fix record made, add the table to the failed table
                //list so that validation on dependant tables will not be validated
                AddFailedTable(tableName);

                Result = true;
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
                Result = false;
            }

            return (Result);
        }

        /// <summary>
        /// Sets the local id to that of the remote id based on the rules
        /// </summary>
        /// <param name="remoteTran">Remote Database Transaction</param>
        /// <param name="localTran">Local Database Transaction</param>
        /// <param name="rule">Rule being applied</param>
        /// <param name="oldID">The ID of the record in the local database</param>
        /// <returns></returns>
        private bool FixRecordRemoteID(FbTransaction remoteTran, FbTransaction localTran, 
            AutoCorrectRule rule, string oldID)
        {
            bool Result = false;
            try
            {
                string selectColumns = rule.TargetColumn + "," + rule.ReplicateName;
                string[] columns = selectColumns.Split(',');

                ///Retrieve the remote ID
                string localSQL = String.Format("SELECT {0} FROM {1} WHERE {2} = {3}",
                    selectColumns, rule.TableName, rule.TargetColumn, oldID);
                FbDataReader localRdr = null;
                FbCommand localCmd = new FbCommand(localSQL, _LocalDB, localTran);
                try
                {
                    localRdr = localCmd.ExecuteReader();

                    if (localRdr.Read())
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            columns[i] = localRdr.GetString(i);
                        }
                    }
                }
                finally
                {
                    CloseAndDispose(ref localCmd, ref localRdr);
                }

                string[] localSQLParts = rule.SQLRuleLocal.Split(';');
                //based on rule colums get the values of the columns to verify 
                FbDataReader remoteRdr = null;
                FbCommand remoteCmd = new FbCommand(localSQLParts[0], _RemoteDB, remoteTran);
                try
                {
                    for (int i = 0; i < columns.Length; i++)
                    {
                        AddParam(remoteCmd, String.Format("PARAM{0}", i), FbDbType.VarChar,
                            Shared.Utilities.StrToIntDef(localSQLParts[1], 200), columns[i]);
                    }

                    remoteRdr = remoteCmd.ExecuteReader();

                    if (remoteRdr.Read())
                    {
                        Int64 newID = remoteRdr.GetInt64(0);

                        localSQL = String.Format("UPDATE {0} SET {1} = {2} WHERE {1} = {3}",
                            rule.TableName, rule.TargetColumn, newID, oldID);
                        localCmd = new FbCommand(localSQL, _LocalDB, localTran);
                        try
                        {
                            localCmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref localCmd);
                        }

                        Result = true;
                    }
                }
                finally
                {
                    CloseAndDispose(ref remoteCmd, ref remoteRdr);
                }
            }
            catch (Exception error)
            {
                Shared.EventLog.Add(error);
            }

            return (Result);
        }

        /// <summary>
        /// Attempts to set the local ID to that of the remote ID, based on the rules
        /// </summary>
        /// <param name="remoteTran">Remote Database Transaction</param>
        /// <param name="localTran">Local Database Transaction</param>
        /// <param name="rule">Rule being applied</param>
        /// <param name="remoteID">The remote ID of the record</param>
        /// <returns></returns>
        private bool FixRecordLocalID(FbTransaction remoteTran, FbTransaction localTran,
            AutoCorrectRule rule, string remoteID, string sqlStatement, Int64 operationID, 
            string tableName, string recordColumn, string recordID,
            bool localFix)
        {
            bool Result = false;
            try
            {
                string selectColumns = rule.TargetColumn + "," + rule.ReplicateName;
                string[] columns = selectColumns.Split(',');

                ///Retrieve the remote ID
                string remoteSQL = String.Format("SELECT {0} FROM {1} WHERE {2} = {3}",
                    selectColumns, rule.TableName, rule.TargetColumn, remoteID);
                FbDataReader remoteRdr = null;
                FbCommand remoteCmd = new FbCommand(remoteSQL, _RemoteDB, remoteTran);
                try
                {
                    remoteRdr = remoteCmd.ExecuteReader();

                    if (remoteRdr.Read())
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            columns[i] = remoteRdr.GetString(i);
                        }
                    }
                    else
                    {
                        string localSQL1 = String.Format("SELECT {0} FROM {1} WHERE {2} = {3}",
                            selectColumns, rule.TableName, rule.TargetColumn, remoteID);
                        FbDataReader localRdr1 = null;
                        FbCommand localCmd1 = new FbCommand(localSQL1, _LocalDB, localTran);
                        try
                        {
                            localRdr1 = localCmd1.ExecuteReader();

                            if (localRdr1.Read())
                            {
                                for (int i = 0; i < columns.Length; i++)
                                {
                                    columns[i] = localRdr1.GetString(i);
                                }
                            }
                        }
                        finally
                        {
                            CloseAndDispose(ref localCmd1, ref localRdr1);
                        }
                    }
                }
                finally
                {
                    CloseAndDispose(ref remoteCmd, ref remoteRdr);
                }

                string[] remoteSQLParts = rule.SQLRuleRemote.Split(';');

                //based on rule colums get the values of the columns to verify 
                FbCommand localCmd = new FbCommand(remoteSQLParts[0], _LocalDB, localTran);
                try
                {
                    for (int i = 0; i < columns.Length; i++)
                    {
                        AddParam(localCmd, String.Format("PARAM{0}", i), FbDbType.VarChar,
                            Shared.Utilities.StrToIntDef(remoteSQLParts[1], 200),
                            columns[i]);
                    }

                    Result = localCmd.ExecuteNonQuery() > 0;
                }
                finally
                {
                    CloseAndDispose(ref localCmd);
                }
            }
            catch (Exception error)
            {
                Shared.EventLog.Add(error);
            }

            return (Result);
        }

        private void CloseAndDispose(ref FbCommand command, ref FbDataReader reader)
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
                reader = null;
            }

            if (command != null)
            {
                command.Dispose();
                command = null;
            }
        }

        private void CloseAndDispose(ref FbCommand command)
        {
            if (command != null)
            {
                command.Dispose();
                command = null;
            }
        }

        /// <summary>
        /// Loads a list of rules which attempt to automatically fix data issues
        /// </summary>
        /// <returns></returns>
        private AutoCorrectRules LoadReplicationFailureOptions()
        {
            AutoCorrectRules Result = new AutoCorrectRules();
            try
            {
                FbConnection localDB = new FbConnection(FixConnectionString(ChildDatabase));
                try
                {
                    localDB.Open();

                    FbTransaction localTran = localDB.BeginTransaction(IsolationLevel.ReadCommitted);
                    try
                    {
                        FbDataReader rdr = null;
                        FbCommand cmd = new FbCommand("SELECT a.TABLE_NAME, a.KEY_NAME, a.TARGET_TABLE, " +
                            "a.TARGET_COLUMN, a.REPLICATE_COLUMN_NAME, a.OPTIONS, a.SQL_RULE, A.SQL_RULE_REMOTE, " +
                            "a.DEPENDENCIES FROM REPLICATE$AUTOCORRECTRULES a", localDB, localTran);
                        try
                        {
                            rdr = cmd.ExecuteReader();

                            while (rdr.Read())
                            {
                                Result.Add(new AutoCorrectRule(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2), 
                                    rdr.GetString(3), rdr.GetString(4), rdr.GetString(6), rdr.GetString(7), rdr.GetString(8), 
                                    (AutoFixOptions)rdr.GetInt32(5)));
                            }
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd, ref rdr);
                        }
                    }
                    finally
                    {
                        localTran.Rollback();
                        localTran.Dispose();
                    }
                }
                finally
                {
                    localDB.Close();
                    localDB.Dispose();
                    localDB = null;
                }
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
            }

            return (Result);
        }

        #endregion Fix Data

        #endregion Private Methods
    }
}

