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
 *  Purpose:  Database Connection
 *
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using Replication.Engine;
using Replication.Engine.Classes;

using Shared;

namespace Replication.Service
{
    [Serializable]
    public sealed class 
    DatabaseConnection
    {
        #region Private Members

        /// <summary>
        /// Lock OBject
        /// </summary>
        private static object _lockObject = new object();

        private List<string> _childTables;

        private Dictionary<string, List<string>> _childPrimaryKeys = new Dictionary<string,List<string>>();

        private List<string> _childReplicatedTables;

        private List<string> _childGenerators;

        private List<string> _masterTables;

        private List<string> _masterReplicatedTables;

        private List<string> _masterGenerators;

        private List<ForeignKeys> _foreignKeys;

        private List<PrimaryKeys> _primaryKeys;

        private AutoCorrectRules _autoCorrectRules;

        #endregion Private Members

        #region Constructors

        public DatabaseConnection()
        {
            // default values
            Name = "Database Connection";
            ChildDatabase = String.Empty;
            Enabled = false;

            // Replicaton
            ReplicationType = Engine.ReplicationType.NotSet;
            SiteID = -1;
            ReplicateDatabase = false;
            MasterDatabase = String.Empty;
            ReplicateInterval = 20;
            TimeOut = 30;
            MaximumUploadCount = 400;
            MaximumDownloadCount = 400;
            VerifyData = false;
            VerifyStart = 13;
            VerifyEnd = 14;
            VerifyDataInterval = 180;
            VerifyErrorReset = 200;
            ReplicateUpdateTriggers = false;
            InitialScriptExecuted = false;
            RequireUniqueAccess = true;

            // backup
            BackupDatabase = false;
            BackupCompress = true;
            BackupFTPPort = 21;
            BackupFTPHost = "";
            BackupFTPPassword = "";
            BackupFTPUsername = "";
            BackupDeleteOldFiles = false;
            BackupMaximumAge = 7;
            BackupAfterTimeEnabled = false;
            BackupAfterTime = new DateTime(2017, 1, 1, 21, 0, 0);

            // remote update
            RemoteUpdate = false;
            RemoteUpdateLocation = String.Empty;
            RemoteUpdateXML = String.Empty;
        }

        #endregion Constructors

        #region Properties

        #region General Settings

        /// <summary>
        /// Name of replication object, user defined
        /// </summary>
        public string Name { get; set; }


        public bool Enabled { get; set; }

        /// <summary>
        /// The current child database
        /// </summary>
        public string ChildDatabase { get; set; }

        /// <summary>
        /// Unique site id for this site, default to zero if not replicating or master database
        /// </summary>
        public int SiteID { get; set; }

        /// <summary>
        /// Indicates the child database is loaded
        /// </summary>
        [XmlIgnore]
        public bool ChildDatabaseLoaded { get; private set; }

        /// <summary>
        /// Indicates the master database is loaded
        /// </summary>
        [XmlIgnore]
        public bool MasterDatabaseLoaded { get; private set; }

        #endregion General Settings

        #region Replication Settings

        public ReplicationType ReplicationType { get; set; }

        /// <summary>
        /// Is the database being replicated?
        /// </summary>
        public bool ReplicateDatabase { get; set; }

        /// <summary>
        /// Master Database
        /// </summary>
        public string MasterDatabase { get; set; }

        /// <summary>
        /// Maximum updates to download in one go
        /// </summary>
        public uint MaximumDownloadCount { get; set; }

        /// <summary>
        /// Maximum updates to upload in one go
        /// </summary>
        public uint MaximumUploadCount { get; set; }

        /// <summary>
        /// Number of minutes that the operation should timeout
        /// </summary>
        public uint TimeOut { get; set; }

        /// <summary>
        /// Number of minutes between each replication
        /// </summary>
        public uint ReplicateInterval { get; set; }

        /// <summary>
        /// Force the verfification of data every n Iterations
        /// </summary>
        public uint VerifyDataInterval { get; set; }

        /// <summary>
        /// Forces a reset of replication after n Errors
        /// </summary>
        public uint VerifyErrorReset { get; set; }

        /// <summary>
        /// Allows the verification of data between certain hours
        /// </summary>
        public bool VerifyData { get; set; }


        public bool VerifyDataAfterHour { get; set; }

        /// <summary>
        /// Start hour for verifying data
        /// </summary>
        public Int64 VerifyStart { get; set; }

        /// <summary>
        /// End hour for verifying data
        /// </summary>
        public uint VerifyEnd { get; set; }

        /// <summary>
        /// Automatically update replication triggers after database update
        /// </summary>
        public bool ReplicateUpdateTriggers { get; set; }

        /// <summary>
        /// If true, the initial script to add replication has been executed
        /// </summary>
        public bool InitialScriptExecuted { get; set; }

        /// <summary>
        /// Unique access is required for verifying data
        /// </summary>
        public bool RequireUniqueAccess { get; set; }

        #endregion Replication Settings

        #region Backup Settings

        /// <summary>
        /// Indicates the database should be backed up
        /// </summary>
        public bool BackupDatabase { get; set; }

        [XmlIgnore]
        public DateTime LastBackupTime
        {
            get
            {
                return (DateTime.FromFileTime(LastBackup));
            }
        }

        /// <summary>
        /// Date/Time last backed up
        /// </summary>
        public long LastBackup { get; set; }

        [XmlIgnore]
        public DateTime LastBackupAttemptTime
        {
            get
            {
                return (DateTime.FromFileTime(LastBackupAttempt));
            }
        }

        /// <summary>
        /// Date/Time last backup attempt
        /// </summary>
        public long LastBackupAttempt { get; set; }

        /// <summary>
        /// Path where backups will be kept
        /// </summary>
        public string BackupPath { get; set; }

        /// <summary>
        /// If true, the site id is part of the backup file name
        /// </summary>
        public bool BackupUseSiteID { get; set; }

        /// <summary>
        /// Backup Name if site id not used
        /// </summary>
        public string BackupName { get; set; }

        /// <summary>
        /// If true backup is compressed
        /// </summary>
        public bool BackupCompress { get; set; }

        /// <summary>
        /// Copy Remotely via FTP
        /// </summary>
        public bool BackupCopyRemote { get; set; }

        /// <summary>
        /// FTP FTP Username
        /// </summary>
        public string BackupFTPUsername { get; set; }

        /// <summary>
        /// Backup FTP Password
        /// </summary>
        public string BackupFTPPassword { get; set; }

        /// <summary>
        /// Backup FTP Host
        /// </summary>
        public string BackupFTPHost { get; set; }

        /// <summary>
        /// Backup FTP Port
        /// </summary>
        public int BackupFTPPort { get; set; }


        public bool BackupDeleteOldFiles { get; set; }


        public int BackupMaximumAge { get; set; }


        public bool BackupAfterTimeEnabled { get; set; }


        public DateTime BackupAfterTime { get; set; }

        #endregion Backup Settings

        #region Remote Update

        public bool RemoteUpdate { get; set; }

        public string RemoteUpdateXML { get; set; }

        public string RemoteUpdateLocation { get; set; }

        #endregion Remote Update

        #region Other Properties

        [XmlIgnore]
        public AutoCorrectRules Rules
        {
            get
            {
                return (_autoCorrectRules);
            }
        }

        [XmlIgnore]
        public List<PrimaryKeys> PrimaryKeys
        {
            get
            {
                return (_primaryKeys);
            }
        }

        [XmlIgnore]
        public List<ForeignKeys> ForeignKeys 
        { 
            get
            {
                return (_foreignKeys);
            }
        }

        #endregion Other Properties

        #endregion Properties

        #region Methods

        public string PrimaryKeyType(string tableName)
        {
            string Result = "Unknown";

            foreach (PrimaryKeys key in _primaryKeys)
            {
                if (key.TableName == tableName)
                {
                    switch (Result)
                    {
                        case "Unknown":
                            Result = key.ColumnType;
                            break;
                        case "OTHER":
                            return ("OTHER");
                        default:
                            if (Result == "INT" && key.ColumnType == "BIGINT")
                                Result = "INT";
                            else
                                Result = "BIGINT";
                            break;
                    }
                }
            }

            return (Result);
        }

        public bool ColumnIsPrimaryKey(string tableName, string columnName, out int keyCount)
        {
            bool Result = false;
            keyCount = 0;

            foreach (PrimaryKeys key in _primaryKeys)
            {
                if (key.TableName == tableName)
                {
                    keyCount++;

                    if (key.ColumnName == columnName)
                        Result = true;
                }
            }

            return (Result);
        }

        public bool ColumnIsForeignKey(string tableName, string columnName)
        {
            foreach (ForeignKeys key in _foreignKeys)
            {
                if (key.TableName == tableName && key.TableColumn == columnName)
                {
                    return (true);
                }
            }

            return (false);
        }

        public int ChildTableSortOrder(string tableName)
        {
            API api = new API();
            try
            {
                List<ReplicatedTable> tables = api.GetChildTableReplicatedTable(this, tableName);
                try
                {
                    if (tables.Count > 0)
                    {
                        return (tables[0].SortOrder);
                    }
                }
                finally
                {
                    tables = null;
                }
            }
            finally
            {
                api = null;
            }

            return (0);
        }

        public bool ChildTableIsReplicated(string tableName)
        {
            foreach (string table in _childReplicatedTables)
            {
                if (tableName == table)
                {
                    return (true);
                }
            }

            return (false);
        }

        public bool MasterTableIsReplicated(string tableName)
        {
            foreach (string table in _masterReplicatedTables)
            {
                if (tableName == table)
                {
                    return (true);
                }
            }

            return (false);
        }

        public List<string> ChildTables()
        {
            return (_childTables);
        }

        public Dictionary<string, List<string>> ChildPrimaryKeys()
        {
            return (_childPrimaryKeys);
        }

        public List<string> MasterTables()
        {
            return (_masterTables);
        }

        /// <summary>
        /// Based on foreign keys, looks at all tables to determine a sort order
        /// 
        /// If a table relies on another table, the sort order should be greater than 
        /// the relied upon table
        /// </summary>
        /// <param name="tableName">Name of table to check</param>
        /// <returns></returns>
        public int SuggestedSortOrder(string tableName, bool autoUpdate, ref int suggested)
        {
            int Result = -1;
            API api = new API();
            try
            {
                List<ReplicatedTable> currentTable = api.GetChildTableReplicatedTable(this, tableName);

                if (currentTable.Count > 0)
                    suggested = currentTable[0].SortOrder;
                else
                    suggested = 50;

                foreach (ForeignKeys key in _foreignKeys)
                {
                    if (key.TableName == tableName)
                    {
                        // found a match
                        List<ReplicatedTable> replicatedTables = api.GetChildTableReplicatedTable(this, key.ReferencedTable);

                        if (replicatedTables.Count > 0)
                        {
                            int parentSortOrder = replicatedTables[0].SortOrder;

                            SuggestedSortOrder(key.ReferencedTable, false, ref parentSortOrder);

                            if (parentSortOrder >= Result)
                                suggested = parentSortOrder + 100;
                        }
                    }
                }
                                    
                if (autoUpdate)
                {
                    api.SetChildSortOrder(this, tableName, suggested);
                }

                return (Result);
            }
            finally
            {
                api = null;
            }
        }

        public void LoadAllTables()
        {
            API api = new API();
            try
            {
                try
                {
                    _childTables = api.GetChildTables(this);
                    _childReplicatedTables = api.GetChildReplicatedTables(this);
                    _childGenerators = api.GetChildGenerators(this);
                    _childPrimaryKeys = api.GetChildPrimaryKeys(this);
                    _autoCorrectRules = api.LoadAutoCorrectRules(this);
                    ChildDatabaseLoaded = true;

                    _foreignKeys = api.GetChildForeignKeys(this);
                    _primaryKeys = api.GetChildPrimaryKeys2(this);
                }
                catch //(Exception errChild)
                {
                    _childTables = new List<string>();
                    _childReplicatedTables = new List<string>();
                    _childGenerators = new List<string>();
                    ChildDatabaseLoaded = false;
                }

                try
                {
                    _masterTables = api.GetMasterTables(this);
                    _masterReplicatedTables = api.GetMasterReplicatedTables(this);
                    _masterGenerators = api.GetMasterGenerators(this);
                    MasterDatabaseLoaded = true;
                }
                catch //(Exception errMaster)
                {
                    _masterTables = new List<string>();
                    _masterReplicatedTables = new List<string>();
                    _masterGenerators = new List<string>();
                    MasterDatabaseLoaded = false;
                }
            }
            finally
            {
                api = null;
            }
        }

        #endregion Methods

        #region Static Methods

        public static DatabaseConnection Load(string configurationFile, string encryptionKey)
        {
            using (Shared.Classes.TimedLock.Lock(_lockObject))
            {
                string tempFile = Path.GetTempFileName();
                try
                {
                    Utilities.FileWrite(tempFile, Utilities.FileEncryptedRead(configurationFile, encryptionKey));
                    using (Stream stream = File.Open(tempFile, FileMode.Open))
                    {
                        try
                        {
                            XmlSerializer binaryFormatter = new XmlSerializer(typeof(DatabaseConnection));
                            return ((DatabaseConnection)binaryFormatter.Deserialize(stream));
                        }
                        catch
                        {
                            // delete the file, it can't be read
                            File.Delete(configurationFile);
                            return (null);
                        }
                    }
                }
                finally
                {
                    File.Delete(tempFile);
                }
            }
        }

        public static bool Save(DatabaseConnection connection, string configurationFile, string encryptionKey)
        {
            using (Shared.Classes.TimedLock.Lock(_lockObject))
            {
                string tempFile = Path.GetTempFileName();
                try
                {
                    using (Stream stream = File.Open(tempFile, FileMode.Create))
                    {
                        XmlSerializer binaryFormatter = new XmlSerializer(typeof(DatabaseConnection));
                        binaryFormatter.Serialize(stream, connection);
                    }
                        
                    Utilities.FileEncryptedWrite(configurationFile, Utilities.FileRead(tempFile, true), encryptionKey);
                }
                finally
                {
                    File.Delete(tempFile);
                }
            }

            return (true);
        }

        #endregion Static Methods
    }
}
