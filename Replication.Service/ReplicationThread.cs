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
 *  Purpose:  Replication Thread
 *
 */
using System;

using Replication.Engine;

using Shared.Communication;

using FirebirdSql.Data.FirebirdClient;

namespace Replication.Service
{
    internal class ReplicationThread : Shared.Classes.ThreadManager
    {
        #region Private Members

        private ReplicationEngine _replicationEngine;

        private ReplicationResult _replicationError;


        private bool _IsRunning = false;
        private bool _allowConfirmCounts = true;
        private DateTime _lastRunReplication;
        private DateTime _lastCheckUpdates;
        private int _replicationCount = 1;
        private int _runInterval = 5;

        private DatabaseConnection _databaseConnection = null;
        private string _filePath;
        private DateTime _forceVerifyRecordsTime;
        #endregion Private Members

        #region Constructors

        internal ReplicationThread(DatabaseConnection databaseConnection, string filePath)
            : base(databaseConnection, new TimeSpan(0, 0, 1), null, 2000)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif

            databaseConnection.ChildDatabase = FixConnectionString(databaseConnection.ChildDatabase);
            databaseConnection.MasterDatabase = FixConnectionString(databaseConnection.MasterDatabase);

            _databaseConnection = databaseConnection;
            _filePath = filePath;
            ForceVerifyRecords = false;
            LastRunReplication = DateTime.Now.AddMinutes(-1);
            _lastCheckUpdates = DateTime.Now.AddMinutes(-500);
            _forceVerifyRecordsTime = DateTime.Now.AddDays(-100);

            CanReplicate = databaseConnection.ReplicateDatabase;

            _runInterval = (int)databaseConnection.ReplicateInterval;
            SharedControls.Classes.Backup.DatabaseBackupThread.OnStageChanged += DatabaseBackupThread_OnStageChanged;

            API api = new API();
            try
            {
                api.UpdateReplicationVersion(_databaseConnection, databaseConnection.ReplicationType == ReplicationType.Master);
                Version = api.GetCurrentDatabaseVersion(_databaseConnection);
            }
            finally
            {
                api = null;
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Current database connection
        /// </summary>
        internal DatabaseConnection DatabaseConnection
        {
            get
            {
                return (_databaseConnection);
            }
        }

        

        /// <summary>
        /// File name of config file
        /// </summary>
        internal string FileName { get { return (_filePath); } }

        /// <summary>
        /// Date / time last run
        /// </summary>
        internal DateTime LastRunReplication
        {
            get
            { 
                return _lastRunReplication;
            }

            set
            {
                _lastRunReplication = value;
            }
        }

        /// <summary>
        /// Indicates wether replication can confirm counts
        /// </summary>
        internal bool AllowConfirmCounts
        {
            get
            {
                return (_allowConfirmCounts);
            }
        }

        /// <summary>
        /// Determines wether the replication engine is running or not
        /// </summary>
        internal bool IsRunning
        {
            get
            {
                return (_IsRunning);
            }
        }

        /// <summary>
        /// Indicates wether the engine can replicate or not
        /// </summary>
        internal bool CanReplicate { get; set; }

        /// <summary>
        /// Latest result from replication
        /// </summary>
        internal ReplicationResult Result
        {
            get
            {
                return (_replicationError);
            }
        }

        /// <summary>
        /// Number of iterations of the replication engine
        /// </summary>
        internal int ReplicationCount { get { return (_replicationCount); } }

        /// <summary>
        /// Forces the engine to force Verify all the data
        /// </summary>
        internal bool ForceVerifyRecords { get; set; }

        /// <summary>
        /// Current Database Version
        /// </summary>
        internal int Version { get; set; }

        /// <summary>
        /// Version of Replication engine
        /// </summary>
        internal int DatabaseVersion { get; set; }

        /// <summary>
        /// Name of replication client
        /// </summary>
        internal string ConnectionName 
        {
            get
            {
                if (_databaseConnection == null)
                    return ("Unknown");
                else
                    return (_databaseConnection.Name);
            }
        }

        #endregion Properties

        #region Protected Methods

        public override void CancelThread(int timeout = 10000, bool isUnResponsive = false)
        {
            if (_replicationEngine != null)
            {
                _replicationEngine.CancelReplication = true;
            }

            base.CancelThread(timeout, isUnResponsive);
        }

        public override void Abort()
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            if (_replicationEngine != null)
            {
                AddToLogFile(String.Format("{0} Abort received", ConnectionName));
                //force the db server to disconnect the connection
                ForceDatabaseDisconnect(_replicationEngine.ChildDatabase, _replicationEngine.LocalDatabaseAttachmentID);
                ForceDatabaseDisconnect(_replicationEngine.MasterDatabase, _replicationEngine.RemoteDatabaseAttachmentID);
            }

            base.Abort();
        }

        protected override bool Run(object parameters)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            DatabaseConnection connection = (DatabaseConnection)parameters;
            bool tableUpdated = false;

            TimeSpan ts = new TimeSpan(0, 2, 0);

            if (!_IsRunning && (DateTime.Now - _lastCheckUpdates) >= ts)
            {
                try
                {
                    _lastCheckUpdates = DateTime.Now;

                    // are we looking for remote updates to the database
                    if (connection.RemoteUpdate)
                    {
                        AddToLogFile(String.Format("{0} Checking Version", ConnectionName));

                        API api = new API();
                        try
                        {
                            DatabaseRemoteUpdate remoteUpdate = new DatabaseRemoteUpdate();
                            try
                            {
                                remoteUpdate.OnNewMessage += DatabaseRemoteUpdate_OnNewMessage;
                                int version = Version;

                                if (remoteUpdate.CheckForDatabaseUpdates(connection.Name,
                                    connection.ChildDatabase,
                                    connection.RemoteUpdateXML, connection.RemoteUpdateLocation,
                                    ref version, ref tableUpdated))
                                {
                                    Version = version;

                                    if (api.UpdateCurrentDatabaseVersion(connection, version))
                                    {
                                        string fileName = String.Empty;

                                        if (connection.ReplicateDatabase && connection.ReplicateUpdateTriggers && tableUpdated)
                                        {
                                            if (connection.ReplicationType == ReplicationType.Child)
                                            {
                                                //create replication triggers if needed and new database users
                                                ReplicationPrepareChildDatabase repEng = new ReplicationPrepareChildDatabase();
                                                try
                                                {
                                                    if (tableUpdated)
                                                        AddToLogFile(String.Format("{0} Rebuilding Child Replication Triggers", ConnectionName));

                                                    if (repEng.PrepareDatabaseForReplication(_databaseConnection.ChildDatabase, 
                                                        tableUpdated, false, ref fileName, remoteUpdate))
                                                        AddToLogFile(String.Format("{0} Replication Child Triggers Rebuilt", ConnectionName));
                                                }
                                                finally
                                                {
                                                    repEng = null;
                                                }
                                            }
                                            else if (connection.ReplicationType == ReplicationType.Master)
                                            {
                                                //create replication triggers if needed and new database users
                                                ReplicationPrepareMasterDatabase repEng = new ReplicationPrepareMasterDatabase();
                                                try
                                                {
                                                    if (tableUpdated)
                                                        AddToLogFile(String.Format("{0} Rebuilding Master Replication Triggers", ConnectionName));

                                                    if (repEng.PrepareDatabaseForReplication(_databaseConnection.ChildDatabase,
                                                        tableUpdated, false, ref fileName, remoteUpdate))
                                                    {
                                                        AddToLogFile(String.Format("{0} Replication Master Triggers Rebuilt", ConnectionName));
                                                    }
                                                }
                                                finally
                                                {
                                                    repEng = null;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                remoteUpdate.OnNewMessage -= DatabaseRemoteUpdate_OnNewMessage;
                                remoteUpdate = null;
                            }
                        }
                        finally
                        {
                            api = null;
                        }
                    }

                    // are we backing up the database
                    if (connection.BackupDatabase)
                    {
                        DateTime compare = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                            connection.BackupAfterTime.Hour, connection.BackupAfterTime.Minute, 0);
                        TimeSpan spanLastBackup = DateTime.Now - connection.LastBackupTime;

                        if (
                            (
                                (!connection.BackupAfterTimeEnabled || 
                                    (connection.BackupAfterTimeEnabled && DateTime.Now.Subtract(compare).TotalMinutes >= 0)
                            ) ||
                                (spanLastBackup.TotalDays >= 1.0)
                            ))
                        {
                            AddToLogFile(String.Format("{0} Checking Backup", ConnectionName));
                            CheckLatestDBBackup();
                        }
                        else
                        {
                        }
                    }
                }
                catch (Exception errUB)
                {
                    Shared.EventLog.Add(errUB);
                }
            }

            int missingRecordCount = 0;

            try
            {
                if (CanReplicate && connection.ReplicationType == ReplicationType.Child)
                {
                    ts = new TimeSpan(0, _runInterval, 0); // xx minute increments

                    // has it been xx minutes since last run?
                    if (!_IsRunning && (DateTime.Now - LastRunReplication) >= ts)
                    {
                        AddToLogFile(String.Format("Run Replication {0}", ConnectionName));

                        _IsRunning = true;

                        //properties
                        _replicationEngine = new ReplicationEngine(
                            ConnectionName,
                            _databaseConnection.ReplicateDatabase,
                            _databaseConnection.ChildDatabase,
                            _databaseConnection.MasterDatabase);
                        try
                        {
                            _replicationEngine.Validate = true;

                            // settings
                            _replicationEngine.VerifyAllDataInterval = (int)_databaseConnection.VerifyDataInterval;
                            _replicationEngine.VerifyTableCounts = 20;
#if ERROR_LIMIT_30000
                            _replicationEngine.ForceRestartErrorCount = 30000;
#else
                            _replicationEngine.ForceRestartErrorCount = (int)_databaseConnection.VerifyErrorReset;
#endif
                            _replicationEngine.MaximumDownloadCount = (int)_databaseConnection.MaximumDownloadCount;
                            _replicationEngine.MaximumUploadCount = (int)_databaseConnection.MaximumUploadCount;
                            _replicationEngine.TimeOutMinutes = (int)_databaseConnection.TimeOut;
                            _replicationEngine.RequireUniqueAccess = _databaseConnection.RequireUniqueAccess;

                            // event hookups
                            _replicationEngine.OnProgress += new ReplicationPercentEventArgs(rep_OnProgress);
                            _replicationEngine.OnReplicationTextChanged += new ReplicationProgress(rep_OnReplicationTextChanged);
                            _replicationEngine.BeginReplication += new ReplicationEventHandler(rep_BeginReplication);
                            _replicationEngine.EndReplication += new ReplicationEventHandler(rep_EndReplication);
                            _replicationEngine.OnReplicationError += new ReplicationError(rep_OnReplicationError);
                            _replicationEngine.OnIDChanged += rep_OnIDChanged;
                            _replicationEngine.OnCheckCancel += _replicationEngine_OnCheckCancel;

                            //are we forcing hard confirm between certain hours?
                            if (!ForceVerifyRecords)
                            {
                                ForceVerifyRecords = ForceConfirmBasedOnHoursOrIterations();
                            }

                            _replicationError = _replicationEngine.Run(_allowConfirmCounts, ForceVerifyRecords);

                            missingRecordCount = _replicationEngine.MissingRecordCount;

                            switch (_replicationError)
                            {
                                case ReplicationResult.ThresholdExceeded:
                                    _runInterval = 0;
                                    break;

                                case ReplicationResult.UniqueAccessDenied:
                                    // we do not reset force hard confirm here as it was set before the run
                                    //_forceHardConfirm = _forceHardConfirm;
                                    _runInterval = (int)_databaseConnection.ReplicateInterval;
                                    break;
                            }

                            if (ForceVerifyRecords && (missingRecordCount >= _databaseConnection.VerifyErrorReset))
                            {
                                AddToLogFile(String.Format("{0} Force Verify Records Missing Records Exceeded", ConnectionName));
                                ForceVerifyRecords = true;
                                _runInterval = 0;

                                // get list of confirmed tables so we don't scan them next time
                                //_confirmedTables = _replicationEngine.TablesConfirmedCorrect;
                            }
                            else
                            {
                                switch (_replicationError)
                                {
                                    case ReplicationResult.TimeOutExceeded:
                                        //_confirmedTables = _replicationEngine.TablesConfirmedCorrect;
                                        _runInterval = (int)_databaseConnection.ReplicateInterval;
                                        AddToLogFile(String.Format("{0} Time out exceeded, restarting", ConnectionName));

                                        break;
                                    case ReplicationResult.ThresholdExceeded:
                                        //_confirmedTables = _replicationEngine.TablesConfirmedCorrect;

                                        // we do not reset force hard confirm here as it was set before the run
                                        //_forceHardConfirm = _forceHardConfirm;
                                        _runInterval = 0;
                                        break;
                                    case ReplicationResult.UniqueAccessDenied:
                                        //_confirmedTables = _replicationEngine.TablesConfirmedCorrect;
                                        _runInterval = (int)_databaseConnection.ReplicateInterval;
                                        AddToLogFile(String.Format("{0} Unique access for deep scan not allowed, retry next time...", ConnectionName));
                                        break;
                                    case ReplicationResult.Error:
                                    case ReplicationResult.DeepScanInitialised:
                                        //_confirmedTables = _replicationEngine.TablesConfirmedCorrect;
                                        _runInterval = 0;
                                        break;

                                    case ReplicationResult.NotInitialised:
                                    case ReplicationResult.Cancelled:
                                    case ReplicationResult.Completed:
                                    case ReplicationResult.DeepScanCompleted:
                                        ForceVerifyRecords = false;
                                        //_confirmedTables = String.Empty;
                                        _replicationEngine.Statuses.Clear();
                                        _runInterval = (int)_databaseConnection.ReplicateInterval;
                                        break;
                                }
                            }
                        }
                        finally
                        {
                            _replicationEngine.Dispose();
                            _replicationEngine = null;
                        }

                        _IsRunning = false;
                        LastRunReplication = DateTime.Now;
                        _replicationCount++;

                        //log management
                        Shared.EventLog.ArchiveOldLogFiles();
                    }
                }

                TimeSpan t = LastRunReplication.AddMinutes(_runInterval) - DateTime.Now;
                SendToTCPClients(String.Format("Sleeping, time until next run {0}", t.ToString().Substring(0, 8)));
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
                _IsRunning = false;
                AddToLogFile(String.Format("{0} {1}", ConnectionName, err.Message));
                AddToLogFile(err.StackTrace.ToString());
                LastRunReplication = DateTime.Now;
            }
            finally
            {
                IndicateNotHanging();
            }

            return (true);
        }

        private void DatabaseRemoteUpdate_OnNewMessage(object sender, Shared.AddToLogFileArgs e)
        {
            AddToLogFile(e.Message);
        }

        #endregion Protected Methods

        #region Internal Methods

        internal void CancelReplication()
        {
            if (_replicationEngine != null)
            {
                _replicationEngine.CancelReplication = true;
            }
        }

        internal void InitialiseReplication()
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            AddToLogFile(String.Format("{0} Initialise replication", ConnectionName));
        }

        internal void FinaliseReplication()
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            AddToLogFile(String.Format("{0} Finalise Replication", ConnectionName));
        }

        #endregion Internal Methods

        #region Private Methods

        #region Private Replication Events

        private void _replicationEngine_OnCheckCancel(object sender, ReplicationCancelEventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            e.CancelReplication = HasCancelled();

            if (e.CancelReplication)
                AddToLogFile(String.Format("{3} Replication Cancelled: Unresposive: {0}; Marked For Removal: {1}; Hang Timeout Minutes: {2}",
                    UnResponsive.ToString(), MarkedForRemoval.ToString(), Shared.Classes.ThreadManager.ThreadHangTimeout, ConnectionName));
        }

        private void rep_OnIDChanged(object sender, IDUpdatedEventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            SendToTCPClients(e.OldID, e.NewID, e.Location);
        }

        private void rep_OnProgress(object sender, PercentEventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            if (e.PercentComplete == -279)
            {
                SendToTCPClients(e.ProgressMessage);
                return;
            }

            if (!e.ProgressMessage.Contains(" of "))
                AddToLogFile(e.ProgressMessage);

            SendToTCPClients(e.ProgressMessage);

            HasCancelled();
            //AddToLogFile(e.PercentComplete.ToString());
        }

        private void rep_OnReplicationTextChanged(object sender, SynchTextEventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            AddToLogFile(e.Text);
        }

        private void rep_OnReplicationError(object sender, SynchTextEventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            SendToTCPClients(e.Text);
            AddToLogFile(String.Format("{2} {1} Replication Error\r\r{0}\r\r", e.Text, DateTime.Now.ToString("dd/MM/yyyy"), ConnectionName));
        }

        private void rep_EndReplication(object sender)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            AddToLogFile(String.Format("{0} Replication End", ConnectionName));
        }

        private void rep_BeginReplication(object sender)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            AddToLogFile(String.Format("{0} Replication Begin", ConnectionName));
        }

        #endregion Private Replication Events

        #region Logging

        private void SendToTCPClients(Int64 oldID, Int64 newID, string location)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
                Shared.Communication.Message msg = new Shared.Communication.Message("IDCHANGED", String.Format("{0}${1}${2}", oldID, newID, location), MessageType.Command);
                UpdateBackupReplication.INSTANCE.SendMessage(msg);
        }

        private void SendToTCPClients(string Text)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shared.EventLog.Debug("RepThread " + Text);
#endif
            try
            {
                Shared.Communication.Message msg = new Shared.Communication.Message(
                    String.Format("Replication Engine@{0}", _databaseConnection.Name), Text, MessageType.Info);
                UpdateBackupReplication.INSTANCE.SendMessage(msg);
            }
            catch (Exception err)
            {

                Shared.EventLog.Add(err);
            }
        }

        public void AddToLogFile(string Text)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            SendToTCPClients(Text);

#if DEBUG
            Shared.EventLog.Debug("RepThread " +Text);
#endif
        }

        #endregion Logging

        #region Database

        #region Force Close

        private void ForceDatabaseDisconnect(string connectionString, Int64 attachmentID)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            AddToLogFile(String.Format("{2} Force Database Disconnect : {1} - {0}", "", attachmentID, ConnectionName));
            FbConnection database = new FbConnection(connectionString);
            database.Open();
            try
            {
                FbTransaction tran = database.BeginTransaction();
                try
                {
                    string sql = String.Format("DELETE FROM MON$ATTACHMENTS a WHERE a.MON$ATTACHMENT_ID = {0};", attachmentID);
                    FbCommand cmd = new FbCommand(sql, database, tran);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        cmd.Dispose();
                    }
                }
                finally
                {
                    tran.Commit();
                    tran.Dispose();
                }
            }
            catch (Exception err)
            {
                AddToLogFile(String.Format("{0} {1}", ConnectionName, err.Message));
            }
            finally
            {
                database.Close();
                database.Dispose();
            }
        }

        #endregion Force Close

        #endregion Database

        #region Replication Thread

        /// <summary>
        /// Checks config file, if force confirm between certain hours then set's the value
        /// </summary>
        /// <param name="localConfigFile">Configuratio File</param>
        /// <returns>true if we should force confirm between certain hours, otherwise false</returns>
        private bool ForceConfirmBasedOnHoursOrIterations()
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            if (!_databaseConnection.VerifyData)
                return (false);

            TimeSpan span = DateTime.Now - _forceVerifyRecordsTime;

            // maximum of force verify once per day
            if (span.Days < 1.0)
                return (false);

            // if not forced verification within 7 days, do it anyway
            if (span.Days > 7.0)
            {
                AddToLogFile(String.Format("{0} Force Verify Records Age", ConnectionName));
                _forceVerifyRecordsTime = DateTime.Now;
                return (true);
            }

            if (_databaseConnection.VerifyDataAfterHour)
            {
                DateTime vStart = DateTime.FromFileTime(_databaseConnection.VerifyStart);

                if (DateTime.Now.Hour >= vStart.Hour && DateTime.Now.Minute >= vStart.Minute)
                {
                    if (span.TotalDays >= 1.0)
                    {
                        _forceVerifyRecordsTime = DateTime.Now;
                        AddToLogFile(String.Format("{0} Force Verify Records Time", ConnectionName));
                        return (true);
                    }
                }
            }

            if (_databaseConnection.VerifyDataInterval == 0)
                return (false);

            if ((ReplicationCount % _databaseConnection.VerifyDataInterval) == 0)
            {
                AddToLogFile(String.Format("{0} Force Verify Records Iterations", ConnectionName));
                _forceVerifyRecordsTime = DateTime.Now;

                return (true);
            }

            return (false);
        }

        #endregion Replication Thread

        #region Database Backup

        /// <summary>
        /// Checks when the database was last backed up, if more than 24 hours performs a backup
        /// and uploads to the server
        /// </summary>
        private void CheckLatestDBBackup()
        {
            string name = "Database Backup Thread";

            if (Shared.Classes.ThreadManager.Exists(name))
                return;

            TimeSpan span = DateTime.Now - _databaseConnection.LastBackupTime;

            if (span.TotalDays < 1.0)
                return;

            span = DateTime.Now - _databaseConnection.LastBackupAttemptTime;

            // try every six hours
            if (span.TotalHours < 6.0)
                return;

            _databaseConnection.LastBackupAttempt = DateTime.Now.ToFileTime();
            DatabaseConnection.Save(_databaseConnection, _filePath, Forms.Configuration.ENCRYPRION_KEY);

            if (_databaseConnection.BackupDeleteOldFiles)
                Shared.Utilities.FileDeleteOlder(
                    _databaseConnection.BackupPath,
                    String.Format("Backup*.{0}", _databaseConnection.BackupCompress ? "zip" : "fbk"),
                    _databaseConnection.BackupMaximumAge);

            SharedControls.Classes.Backup.DatabaseBackupThread.BackupDatabase(
                _databaseConnection.BackupPath, _databaseConnection.BackupCopyRemote,
                _databaseConnection.BackupUseSiteID, _databaseConnection.SiteID,
                _databaseConnection.BackupName, name,
                _databaseConnection.ChildDatabase, _databaseConnection.BackupFTPHost,
                _databaseConnection.BackupFTPUsername, _databaseConnection.BackupFTPPassword,
                _databaseConnection.BackupFTPPort);
        }

        private void DatabaseBackupThread_OnStageChanged(SharedControls.Classes.Backup.DatabaseBackupStage e)
        {
            SendToTCPClients(String.Format("{0} Backup Status: {1}", _databaseConnection.Name, e.ToString()));

            if (e == SharedControls.Classes.Backup.DatabaseBackupStage.BackupComplete)
            {
                _databaseConnection.LastBackup = DateTime.Now.AddDays(1).ToFileTime();
                DatabaseConnection.Save(_databaseConnection, _filePath, Forms.Configuration.ENCRYPRION_KEY);
            }
        }

        #endregion Database Backup

        #region Connection String

        private string FixConnectionString(string connectionString)
        {
            FbConnectionStringBuilder connString = new FbConnectionStringBuilder(connectionString);
            try
            {
                connString.MaxPoolSize = 1;
                connString.NoGarbageCollect = true;
                connString.Pooling = false;
                return (connString.ToString());
            }
            finally
            {
                connString = null;
            }

        }

        #endregion Connection String

        #endregion Private Methods
    }
}
