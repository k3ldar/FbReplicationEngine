using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;

using Shared;
using Shared.Classes;

namespace Replication.Service
{
    public static class DatabaseCompareRows
    {
        #region Private Static Members

        private static int TOTAL_STEPS = 5;

        private static int _currentStage = 1;

        private static DatabaseObjects _databaseObjects = new DatabaseObjects();

        private static FbConnection _connectionDB = new FbConnection();

        private static FbTransaction _connectionTransaction;

        private static List<string> _triggersToDisable;

        private static int Delay70Capacity = 40;

        private static int Delay90Capacity = 150;

        #endregion Private Static Members

        #region Public Static Methods

        /// <summary>
        /// Compares two database looking for differences
        /// </summary>
        /// <param name="masterDatabase"></param>
        /// <param name="childDatabase"></param>
        public static void LoadObjects(string dbConnection)
        {
            CancelAnalysis = false;
            _currentStage = 1;

            _connectionDB.ConnectionString = dbConnection;

            if (_connectionDB.State == System.Data.ConnectionState.Closed)
                _connectionDB.Open();

            _databaseObjects.Clear();

            LoadTables(_databaseObjects, "Loading tables", GetCurrentStage());

            LoadPrimaryKeys(_databaseObjects, "Loading primary keys", GetCurrentStage());
        }

        /// <summary>
        /// Generates hash data for listed tables
        /// </summary>
        /// <param name="tables"></param>
        public static void GenerateHashData(string connectionString, string tables)
        {
            _connectionDB = new FbConnection(connectionString);

            string[] splitTables = tables.Split(';');

            foreach (string table in splitTables)
            {
                GenerateHashDataThread dbPrepare = new GenerateHashDataThread(_connectionDB, table, _databaseObjects);
                dbPrepare.ThreadFinishing += generateHash_ThreadFinishing;
                dbPrepare.ExceptionRaised += generateHash_ExceptionRaised;
                dbPrepare.OnHashGenerationUpdate += dbPrepare_OnHashGenerationUpdate;
                dbPrepare.HangTimeout = 60;
                dbPrepare.ContinueIfGlobalException = true;
                Shared.Classes.ThreadManager.ThreadStart(dbPrepare, String.Format("Generate Database Hash Values {0}", table), System.Threading.ThreadPriority.Normal);
            }
        }

        static void dbPrepare_OnHashGenerationUpdate(object sender, ValidationArgs e)
        {
            if (GenerateHashUpdate != null)
                GenerateHashUpdate(null, e);
        }

        static void childPrepare_OnUpdate(object sender, ValidationArgs e)
        {
            if (GenerateHashUpdate != null)
                GenerateHashUpdate(null, e);
        }

        static void generateHash_ExceptionRaised(object sender, ThreadManagerExceptionEventArgs e)
        {
            GenerateHashDataThread thread = (GenerateHashDataThread)e.Thread;

            //if (PrepareFinished != null)
            //    PrepareFinished(null, new DatabasePrepareFinished(thread.MasterDatabase, true));
        }

        static void generateHash_ThreadFinishing(object sender, ThreadManagerEventArgs e)
        {
            GenerateHashDataThread thread = (GenerateHashDataThread)e.Thread;

            //GenerateHashDataThread thread = (GenerateHashDataThread)e.Thread;

            if (TableHashFinished != null && !String.IsNullOrEmpty(thread.TableName))
                TableHashFinished(null, new TableHashGenerationFinished(thread.TableName));
        }

        /// <summary>
        /// Prepares Master and Child databases for Row Comparison
        /// </summary>
        /// <returns></returns>
        public static void DatabasePrepare(string tables)
        {
            PrepareDatabaseThread dbPrepareThread = new PrepareDatabaseThread(_connectionDB, tables, _databaseObjects);
            dbPrepareThread.ThreadFinishing += childPrepare_ThreadFinishing;
            dbPrepareThread.ExceptionRaised += childPrepare_ExceptionRaised;
            dbPrepareThread.ContinueIfGlobalException = true;
            Shared.Classes.ThreadManager.ThreadStart(dbPrepareThread, "Prepare Database", System.Threading.ThreadPriority.Normal);
        }

        private static void childPrepare_ExceptionRaised(object sender, ThreadManagerExceptionEventArgs e)
        {
            PrepareDatabaseThread thread = (PrepareDatabaseThread)e.Thread;

            if (PrepareFinished != null)
                PrepareFinished(null, new DatabasePrepareFinished(true));
        }

        private static void childPrepare_ThreadFinishing(object sender, ThreadManagerEventArgs e)
        {
            PrepareDatabaseThread thread = (PrepareDatabaseThread)e.Thread;

            if (PrepareFinished != null)
                PrepareFinished(null, new DatabasePrepareFinished(false));
        }

        /// <summary>
        /// Determines wether the database is prepared or not
        /// </summary>
        /// <param name="masterDatabase"></param>
        /// <returns></returns>
        public static bool DatabasePrepared()
        {
            bool Result = false;

            foreach (DatabaseObject dbObject in _databaseObjects)
            {
                if (dbObject.ObjectType == DatabaseObjectType.Table && dbObject.ObjectName == "SD$COMPARE_RESULTS")
                {
                    Result = true;
                    break;
                }
            }

            return (Result);
        }


        public static string PreparedTables(string databaseConnection)
        {
            string Result = String.Empty;

            FbConnection conn = null;
            try
            {
                conn = new FbConnection(databaseConnection);
                conn.Open();

                FbTransaction tran = conn.BeginTransaction();
                try
                {
                    string sql = "SELECT TRIM(a.SD$NAME) FROM SD$COMPARE_SETTINGS a WHERE a.SD$TYPE = 1;";
                    FbCommand cmd = new FbCommand(sql, conn, tran);
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        while (rdr.Read())
                            Result += String.Format("{0}#", rdr.GetString(0));
                    }
                    finally
                    {
                        rdr.Close();
                    }
                }
                finally
                {
                    tran.Commit();
                }
            }
            catch (Exception err)
            {
                if (err.Message == "blah")
                {

                }
            }
            finally
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return (Result);
        }

        /// <summary>
        /// Initiates a compare of all data
        /// </summary>
        /// <param name="databaseMaster">Connection String for Master Database</param>
        /// <param name="databaseChild">Connection String for child Database</param>
        public static void CompareRowLevelData(string databaseMaster, string databaseChild, string table)
        {
            CompareAllRowData allRowDataComparison = new CompareAllRowData(databaseMaster, databaseChild, table, Delay70Capacity, Delay90Capacity);
            allRowDataComparison.ContinueIfGlobalException = false;
            Shared.Classes.ThreadManager.ThreadStart(allRowDataComparison, "Compare All Records Thread", System.Threading.ThreadPriority.BelowNormal);
        }


        internal static void differences_ThreadFinishing(object sender, ThreadManagerEventArgs e)
        {
            FindRowDataDifferences thread = (FindRowDataDifferences)sender;

            if (DataRowDifference != null && thread.Differences.Differences.Count > 0)
                DataRowDifference(null, thread.Differences);
        }

        internal static void RaiseCompareProgress(int current, int total)
        {
            if (CompareProgress != null)
                CompareProgress(null, new ValidationArgs("Comparing", total, current));
        }


        public static void DatabaseUpdateStart(string connectionString, List<string> triggersToDisable)
        {
            _triggersToDisable = triggersToDisable;

            if (String.IsNullOrEmpty(connectionString))
                return;

            _connectionDB = new FbConnection(connectionString);
            _connectionDB.Open();

            _connectionTransaction = _connectionDB.BeginTransaction();
            TriggersDisable(_connectionDB);
        }

        public static void DatabaseUpdateRecord(string sql, bool restartTransaction, string connectionString = "")
        {
            if (!String.IsNullOrEmpty(connectionString) && ( _connectionDB == null || (_connectionDB.State == System.Data.ConnectionState.Open && _connectionDB.ConnectionString != connectionString)))
            {
                if (_connectionTransaction != null)
                {
                    _connectionTransaction.Commit();
                    _connectionTransaction.Dispose();
                }

                _connectionDB.Close();
                _connectionDB.Dispose();
                _connectionDB = new FbConnection(connectionString);
                _connectionDB.Open();
                _connectionTransaction = _connectionDB.BeginTransaction();
                TriggersDisable(_connectionDB);
            }

            if (_connectionDB == null || _connectionDB.State == System.Data.ConnectionState.Closed)
            {
                _connectionDB = new FbConnection(connectionString);
                _connectionDB.Open();
                _connectionTransaction = _connectionDB.BeginTransaction();
                TriggersDisable(_connectionDB);
            }

            FbCommand command = new FbCommand(sql, _connectionDB, _connectionTransaction);
            command.ExecuteNonQuery();

            if (restartTransaction)
            {
                _connectionTransaction.Commit();
                _connectionTransaction = _connectionDB.BeginTransaction();
            }
        }

        private static void TriggersDisable(FbConnection connection)
        {
            FbConnection conn = new FbConnection(connection.ConnectionString);
            try
            {
                conn.Open();
                FbTransaction tran = conn.BeginTransaction();
                try
                {
                    //ALTER TRIGGER REPLICATE$DOWNLOADTYPE_D INACTIVE;
                    //ALTER TRIGGER REPLICATE$DOWNLOADTYPE_D ACTIVE;
                    foreach (string triggerName in _triggersToDisable)
                    {
                        FbCommand cmd = new FbCommand(String.Format("ALTER TRIGGER {0} INACTIVE", triggerName), conn, tran);
                        cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    tran.Commit();
                }
            }
            finally
            {
                conn.Close();
            }
        }

        private static void TriggersEnable(FbConnection connection)
        {
            FbConnection conn = new FbConnection(connection.ConnectionString);
            try
            {
                conn.Open();
                FbTransaction tran = conn.BeginTransaction();
                try
                {
                    //ALTER TRIGGER REPLICATE$DOWNLOADTYPE_D INACTIVE;
                    //ALTER TRIGGER REPLICATE$DOWNLOADTYPE_D ACTIVE;
                    foreach (string triggerName in _triggersToDisable)
                    {
                        FbCommand cmd = new FbCommand(String.Format("ALTER TRIGGER {0} ACTIVE", triggerName), conn, tran);
                        cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    tran.Commit();
                }
            }
            finally
            {
                conn.Close();
            }
        }

        public static void DatabaseUpdateFinish()
        {
            if (_connectionTransaction != null && _connectionTransaction.Connection != null)
                _connectionTransaction.Commit();

            TriggersEnable(_connectionDB);

            if (_connectionDB.State == System.Data.ConnectionState.Open)
            {
                _connectionDB.Close();
                _connectionDB.Dispose();
            }
            
            _triggersToDisable = null;
        }

        /// <summary>
        /// Returns a list of triggers for a table
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static List<string> GetTableTriggers(string connectionString, string tableName)
        {
            List<string> Result = new List<string>();

            FbConnection conn = new FbConnection(connectionString);
            conn.Open();
            try
            {
                FbTransaction tran = conn.BeginTransaction();
                try
                {
                    string sql = String.Format("SELECT a.RDB$TRIGGER_NAME FROM RDB$TRIGGERS a " +
                        "WHERE TRIM(a.RDB$RELATION_NAME) = '{0}' AND COALESCE(a.RDB$SYSTEM_FLAG, 0) = 0;",
                        tableName);
                    FbCommand cmd = new FbCommand(sql, conn, tran);
                    FbDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(rdr.GetString(0));
                }
                finally
                {
                    tran.Rollback();
                }
            }
            finally
            {
                conn.Close();
            }

            return (Result);
        }

        #endregion Public Static Methods

        #region Internal Properties

        #endregion Internal Properties

        #region Public Properties

        /// <summary>
        /// Indicates wether the operation should cancel or not
        /// </summary>
        public static bool CancelAnalysis { get; internal set; }

        /// <summary>
        /// Returns a collection of objects from the child database
        /// </summary>
        public static DatabaseObjects DatabaseObjects
        {
            get
            {
                return (_databaseObjects);
            }
        }

        #endregion Public Properties

        #region Private Static Methods

        /// <summary>
        /// Cancels the operation
        /// </summary>
        public static void Cancel()
        {
            CancelAnalysis = true;
        }

        #region Event Wrappers

        private static void RaiseProcessStatusChanged(string processStage, int percent, int currentStep)
        {
            if (StatusChange != null)
                StatusChange(null, new ValidationArgs(processStage, percent, TOTAL_STEPS, currentStep));
        }

        private static void RaiseProcessStatusChanged(string processStage, int currentStep)
        {
            if (StatusChange != null)
                StatusChange(null, new ValidationArgs(processStage, TOTAL_STEPS, currentStep));
        }

        #endregion Event Wrappers

        #region Pre Process

        /// <summary>
        /// Ensures all FurtherChecks is true
        /// </summary>
        /// <param name="objects"></param>
        private static void PreProcess1(DatabaseObjects objects)
        {
            int i = 0;

            foreach (DatabaseObject dbObject in objects)
            {
                if (CancelAnalysis)
                    return;

                RaiseProcessStatusChanged("Updating Status", Shared.Utilities.Percentage(objects.Count, i), GetCurrentStage(false));
                dbObject.FurtherChecks = true;

                i++;
            }
        }

        #endregion Pre Process

        #region Post Process

        /// <summary>
        /// Scans the list of missing objects and removes objects that do not belong in there
        /// </summary>
        /// <param name="objectsProblemList"></param>
        /// <param name="objectsPrimary"></param>
        /// <param name="objectsSecondary"></param>
        /// <returns></returns>
        private static DatabaseObjects PostProcess3(DatabaseObjects objectsProblemList, DatabaseObjects objectsPrimary, 
            DatabaseObjects objectsSecondary)
        {
            int i = 0;

            DatabaseObjects Result = new DatabaseObjects();

            foreach (DatabaseObject dbObject in objectsProblemList)
            {
                if (CancelAnalysis)
                    return (Result);

                RaiseProcessStatusChanged("Collating Results", Shared.Utilities.Percentage(objectsPrimary.Count, i), GetCurrentStage(false));
                switch (dbObject.Status)
                {
                    case ObjectStatus.DifferentSettings:
                    case ObjectStatus.MissingFromChild:
                    case ObjectStatus.MissingFromMaster:
                    case ObjectStatus.ParentObjectDoesNotExist:

                        Result.Add(dbObject);

                        break;

                    case ObjectStatus.Found:

                        if (dbObject.ExistsWithDifferentName)
                            Result.Add(dbObject);

                        break;
                    default:

                        Result.Add(dbObject);

                        break;
                }

                i++;
            }

            return (Result);
        }

        #endregion Post Process

        #region Load System Objects

        private static void LoadTables(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$RELATION_FIELDS", " WHERE RDB$VIEW_CONTEXT IS NULL ");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT DISTINCT TRIM(RDB$RELATION_NAME), HASH(TRIM(RDB$RELATION_NAME)), COALESCE(RDB$SYSTEM_FLAG, 0) FROM RDB$RELATION_FIELDS ";

            SQL += "WHERE COALESCE(RDB$SYSTEM_FLAG, 0) = 0 AND RDB$VIEW_CONTEXT IS NULL;";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);

                FbDataReader rdr = cmd.ExecuteReader();
                try
                {
                    int i = 0;

                    while (rdr.Read())
                    {
                        if (CancelAnalysis)
                            return;

                        RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                        objects.Add(new DatabaseObject(rdr.GetString(0), String.Empty, String.Empty, String.Empty, String.Empty,
                            String.Empty, DatabaseObjectType.Table, rdr.GetInt32(2) != 0, true, rdr.GetInt64(1)));

                        i++;
                    }
                }
                finally
                {
                    rdr.Close();
                }
            }
#if DEBUG
            catch (Exception error)
            {
                EventLog.Add(error, SQL);
                throw;
            }
#endif
            finally
            {
                transaction.Rollback();
            }
        }

        private static void LoadPrimaryKeys(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$INDEX_SEGMENTS", "", false);
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT TRIM(i.RDB$INDEX_NAME), TRIM(s.RDB$FIELD_NAME), HASH(TRIM(i.RDB$INDEX_NAME) || TRIM(s.RDB$FIELD_NAME)), " +
                "COALESCE(i.RDB$SYSTEM_FLAG, 0), COALESCE(i.RDB$SYSTEM_FLAG, 0), TRIM(rc.RDB$RELATION_NAME) " +
                "FROM RDB$INDEX_SEGMENTS s LEFT JOIN RDB$INDICES i ON i.RDB$INDEX_NAME = s.RDB$INDEX_NAME " +
                "LEFT JOIN RDB$RELATION_CONSTRAINTS rc ON rc.RDB$INDEX_NAME = i.RDB$INDEX_NAME " +
                "WHERE rc.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY' ";

            SQL += " AND COALESCE(i.RDB$SYSTEM_FLAG, 0) = 0 ";

            SQL += " ORDER BY i.RDB$RELATION_NAME, i.RDB$INDEX_NAME, s.RDB$FIELD_POSITION ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);

                FbDataReader rdr = cmd.ExecuteReader();
                try
                {
                    int i = 0;

                    while (rdr.Read())
                    {
                        if (CancelAnalysis)
                            return;

                        RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                        DatabaseObject newObj = new DatabaseObject(rdr.GetString(5), rdr.GetString(0), rdr.GetString(1), String.Empty,
                            String.Empty, String.Empty,
                            DatabaseObjectType.PrimaryKey, rdr.GetInt32(3) != 0, rdr.GetInt32(4) == 1, rdr.GetInt64(2));

                        //does one already exists with same name (multiple parameters)?
                        DatabaseObject existingObject = objects.Find(newObj.ObjectName, newObj.ObjectParameter1, objects, DatabaseObjectType.PrimaryKey);

                        if (existingObject == null)
                        {
                            objects.Add(newObj);
                        }
                        else
                        {
                            existingObject.ObjectParameter2 += String.Format(", {0}", newObj.ObjectParameter2);
                        }


                        i++;
                    }
                }
                finally
                {
                    rdr.Close();
                }
            }
#if DEBUG
            catch (Exception error)
            {
                EventLog.Add(error, SQL);
                throw;
            }
#endif
            finally
            {
                transaction.Rollback();
            }
        }

        private static int GetCount(DatabaseObjects objects, string tableName,
            string whereClause = "", bool allowSystemCheck = true)
        {
            int Result = 0;
            string SQL = String.Format("SELECT COUNT(*) FROM {0} ", tableName);

            if (!String.IsNullOrEmpty(whereClause))
            {
                SQL += whereClause;

                if (allowSystemCheck)
                    SQL += " AND COALESCE(RDB$SYSTEM_FLAG, 0) = 0";
            }
            else
            {
                if (allowSystemCheck)
                    SQL += "WHERE COALESCE(RDB$SYSTEM_FLAG, 0) = 0";
            }

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);

                FbDataReader rdr = cmd.ExecuteReader();
                try
                {
                    if (rdr.Read())
                        Result = rdr.GetInt32(0);
                }
                finally
                {
                    rdr.Close();
                }
            }
            finally
            {
                transaction.Rollback();
            }

            return (Result);
        }

        #endregion Load System Objects

        #region Initialisation / Finalisation

        public static void Initialise()
        {
            _databaseObjects.Clear();
            _databaseObjects.Connection = _connectionDB;
        }

        public static void Finalise()
        {
            _connectionDB.Close();
            _connectionDB.Dispose();

            _databaseObjects.Connection = null;
        }

        #endregion Initialisation / Finalisation

        #region SQL Generation


        #endregion SQL Generation

        #region Others

        private static int GetCurrentStage(bool increment = true)
        {
            if (increment)
                _currentStage++;

            return (_currentStage);
        }

        #endregion Others

        #endregion Private Static Methods

        #region Events

        /// <summary>
        /// Status changed event
        /// </summary>
        public static event ValidationArgsDelegate StatusChange;

        /// <summary>
        /// Database prepare finished
        /// </summary>
        public static event DatabasePrepareFinishedDelegate PrepareFinished;


        public static event TableHashGenerationFinishedDelegate TableHashFinished;

        /// <summary>
        /// Update event to give progress of updating hash table data
        /// </summary>
        public static event ValidationArgsDelegate GenerateHashUpdate;


        public static event DatabaseRowDifferenceDelegate DataRowDifference;


        public static event ValidationArgsDelegate CompareProgress;

        #endregion Events
    }

    public class DatabasePrepareFinished
    {
        public DatabasePrepareFinished (bool error)
        {
            ErrorOcurred = error;
        }

        public bool ErrorOcurred { get; private set; }
    }

    public delegate void DatabasePrepareFinishedDelegate (object sender, DatabasePrepareFinished e);





    public class TableHashGenerationFinished
    {
        public TableHashGenerationFinished(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; private set; }
    }

    public delegate void TableHashGenerationFinishedDelegate(object sender, TableHashGenerationFinished e);

    public class CompareAllRowData : ThreadManager
    {
        #region Private Members

        private int _threadPool70Capacity;
        private int _threadPool90Capacity;

        private string _childDatabase;
        private string _masterDatabase;

        private object _dictionaryLockObject = new object();
        private Dictionary<string, string> _primaryKeys = new Dictionary<string, string>();
        private Dictionary<string, Dictionary<string, string>> _masterTableColumns = new Dictionary<string,Dictionary<string,string>>();
        private Dictionary<string, Dictionary<string, string>> _childTableColumns = new Dictionary<string, Dictionary<string, string>>();

        #endregion Private Members

        #region Constructor

        public CompareAllRowData(string masterConnectionString, string childConnectionString, string tableName, 
            int capacityDelay70Percent, int capacityDelay90Percent)
            :base (tableName, new TimeSpan())
        {
            _childDatabase = childConnectionString;
            _masterDatabase = masterConnectionString;

            _threadPool70Capacity = capacityDelay70Percent;
            _threadPool90Capacity = capacityDelay90Percent;
        }

        #endregion Constructor

        #region Overridden Methods

        protected override bool Run(object parameters)
        {
            int totalRecordsChecked = 0;

            FbConnection dbChild = new FbConnection(_childDatabase);
            dbChild.Open();
            try
            {
                FbConnection dbMaster = new FbConnection(_masterDatabase);
                dbMaster.Open();
                try
                {
                    FbTransaction tranChild = dbChild.BeginTransaction();
                    try
                    {
                        FbTransaction tranMaster = dbMaster.BeginTransaction();
                        try
                        {
                            // select all data from the child database
                            string sql = "SELECT cr.SD$TABLE_NAME || '#' || cr.SD$TABLE_ID || '#' || cr.SD$RECORD_HASH, cr.SD$TABLE_NAME " +
                                "FROM SD$COMPARE_RESULTS cr ";

                            string tableNameToCompare = (string)parameters;

                            if (!String.IsNullOrEmpty(tableNameToCompare))
                                sql += String.Format("WHERE cr.SD$TABLE_NAME = '{0}' ", tableNameToCompare);

                            sql += "ORDER BY cr.SD$TABLE_NAME, cr.SD$TABLE_ID ";

                            FbCommand cmdChild = new FbCommand(sql, dbChild, tranChild);
                            FbDataReader rdrChild = cmdChild.ExecuteReader();
                            try
                            {
                                int iLoopCount = 1;
                                string childRowData = "";

                                while (rdrChild.Read())
                                {
                                    string tableName = rdrChild.GetString(1);
                                    using (TimedLock.Lock(_dictionaryLockObject))
                                    {
                                        if (!_primaryKeys.ContainsKey(tableName))
                                        {
                                            _primaryKeys.Add(tableName, GetPrimaryKey(dbMaster, tranMaster, tableName));
                                        }

                                        // make sure we have a copy of child/master column names
                                        if (!_masterTableColumns.ContainsKey(tableName))
                                        {
                                            _masterTableColumns.Add(tableName, new Dictionary<string, string>());
                                            GetFieldNames(dbMaster, tranMaster, tableName, _masterTableColumns[tableName]);
                                        }

                                        if (!_childTableColumns.ContainsKey(tableName))
                                        {
                                            _childTableColumns.Add(tableName, new Dictionary<string, string>());
                                            GetFieldNames(dbChild, tranChild, tableName, _childTableColumns[tableName]);
                                        }
                                    }

                                    totalRecordsChecked++;

                                    if (iLoopCount >= 1000)
                                    {
                                        CompareRecords(dbMaster, tranMaster, childRowData, dbChild.ConnectionString);
                                        childRowData = "";
                                        iLoopCount = 0;

                                        DatabaseCompareRows.RaiseCompareProgress(totalRecordsChecked, totalRecordsChecked);
                                    }

                                    childRowData += String.Format("{0};", rdrChild.GetString(0));
                                    iLoopCount++;

                                    if (HasCancelled())
                                        return (false);
                                }

                                CompareRecords(dbMaster, tranMaster, childRowData, dbChild.ConnectionString);
                                DatabaseCompareRows.RaiseCompareProgress(totalRecordsChecked, totalRecordsChecked);
                            }
                            finally
                            {
                                rdrChild.Close();
                            }
                        }
                        finally
                        {
                            tranMaster.Commit();
                        }
                    }
                    finally
                    {
                        tranChild.Commit();
                    }
                }
                finally
                {
                    dbMaster.Close();
                }
            }
            finally
            {
                dbChild.Close();
            }

            return (false);
        }

        #endregion Overridden Methods

        #region Private Methods

        private void CompareRecords(FbConnection database, FbTransaction transaction, string records, string childConnectionString)
        {
            // do we need to pause, are there too many threads waiting
            int runCount = Shared.Classes.ThreadManager.MaximumRunningThreads;

            string sqlMaster = "SD$COMPARE_RECORDS";
            FbCommand cmdMaster = new FbCommand(sqlMaster, database, transaction);
            cmdMaster.CommandType = System.Data.CommandType.StoredProcedure;

            cmdMaster.Parameters.Add("@IPARRAY_VALUES", FbDbType.Text);
            cmdMaster.Parameters[0].Value = records;

            FbDataReader rdr = cmdMaster.ExecuteReader();

            while (rdr.Read())
            {
                string tableName = rdr.GetString(0);
                Int64 recordID = rdr.GetInt64(1);

                Dictionary<string, string> masterColumns = new Dictionary<string, string>();
                Dictionary<string, string> childColumns = new Dictionary<string, string>();
                string primaryKey = String.Empty;

                // get the fields for the current row, these will be passed to the thread for comparison
                using (TimedLock.Lock(_dictionaryLockObject))
                {
                    foreach (KeyValuePair<string, string> kvp in _masterTableColumns[tableName])
                        masterColumns.Add(kvp.Key, kvp.Value);

                    foreach (KeyValuePair<string, string> kvp in _childTableColumns[tableName])
                        childColumns.Add(kvp.Key, kvp.Value);

                    if (_primaryKeys.ContainsKey(tableName))
                        primaryKey = _primaryKeys[tableName];
                }

                // start new thread with values to retrieve

                if (primaryKey == String.Empty)
                {

                }

                int poolCount = Shared.Classes.ThreadManager.ThreadPoolCount;

                
                if (Shared.Utilities.Percentage(Shared.Classes.ThreadManager.MaximumPoolSize, poolCount) > 90)
                    System.Threading.Thread.Sleep(_threadPool90Capacity);
                else if (Shared.Utilities.Percentage(Shared.Classes.ThreadManager.MaximumPoolSize, poolCount) > 70)
                    System.Threading.Thread.Sleep(_threadPool70Capacity);

                int i = 0;

                while (i < 10 && ((Shared.Utilities.Percentage(ThreadManager.MaximumPoolSize, ThreadManager.ThreadPoolCount) > 95)))
                {
                    Shared.Classes.ThreadManager.MaximumRunningThreads = 200;
                    System.Threading.Thread.Sleep(1000);
                    Shared.Classes.ThreadManager.MaximumRunningThreads = runCount;
                    i++;
                }

                FindRowDataDifferences differences = new FindRowDataDifferences(tableName, recordID, database.ConnectionString, childConnectionString, masterColumns, childColumns, primaryKey);
                differences.ThreadFinishing += DatabaseCompareRows.differences_ThreadFinishing;
                differences.ContinueIfGlobalException = true;
                Shared.Classes.ThreadManager.ThreadStart(differences, String.Format("Get Data {0} ID {1}", tableName, recordID), System.Threading.ThreadPriority.BelowNormal);
                System.Threading.Thread.Sleep(0);

                if (HasCancelled())
                    return;
            }

        }

        /// <summary>
        /// Gets the primary key for a table
        /// </summary>
        /// <param name="database"></param>
        /// <param name="transaction"></param>
        private string GetPrimaryKey(FbConnection database, FbTransaction transaction, string tableName)
        {
            string Result = String.Empty;

            string SQL = String.Format("SELECT TRIM(i.RDB$INDEX_NAME), TRIM(s.RDB$FIELD_NAME), HASH(TRIM(i.RDB$INDEX_NAME) || TRIM(s.RDB$FIELD_NAME)), " +
                "COALESCE(i.RDB$SYSTEM_FLAG, 0), COALESCE(i.RDB$SYSTEM_FLAG, 0), TRIM(rc.RDB$RELATION_NAME) " +
                "FROM RDB$INDEX_SEGMENTS s LEFT JOIN RDB$INDICES i ON i.RDB$INDEX_NAME = s.RDB$INDEX_NAME " +
                "LEFT JOIN RDB$RELATION_CONSTRAINTS rc ON rc.RDB$INDEX_NAME = i.RDB$INDEX_NAME " +
                "WHERE  rc.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY' AND COALESCE(i.RDB$SYSTEM_FLAG, 0) = 0 AND rc.RDB$RELATION_NAME = '{0}'", tableName);

            FbCommand command = new FbCommand(SQL, database, transaction);
            FbDataReader reader = command.ExecuteReader();
            try
            {
                if (reader.Read())
                    Result = reader.GetString(1);
            }
            finally
            {
                reader.Close();
            }

            return (Result);
        }

        /// <summary>
        /// Retrieves names of all fields for a table, stores them globally for other threads to use
        /// </summary>
        /// <param name="database"></param>
        /// <param name="transaction"></param>
        /// <param name="tableName"></param>
        /// <param name="storage"></param>
        private void GetFieldNames(FbConnection database, FbTransaction transaction, string tableName, Dictionary<string, string> storage)
        {
            storage.Clear();
            string sql = String.Format("SELECT TRIM(a.RDB$FIELD_NAME) FROM RDB$RELATION_FIELDS a " +
                "WHERE a.RDB$RELATION_NAME = '{0}' ORDER BY a.RDB$FIELD_NAME", tableName);
            FbCommand command = new FbCommand(sql, database, transaction);
            FbDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                    storage.Add(reader.GetString(0), String.Empty);
            }
            finally
            {
                reader.Close();
            }
        }

        #endregion Private Methods
    }

    public class FindRowDataDifferences : ThreadManager
    {
        #region Private Members

        string _tableName;
        Int64 _recordID;
        string _masterDatabase;
        string _childDatabase;
        string _primaryKey;

        private DatabaseDataRow _databaseRow;

        #endregion Private Members

        #region Constructors

        public FindRowDataDifferences(string tableName, Int64 recordID, string masterDatabase, string childDatabase, 
            Dictionary<string, string> masterColumns, Dictionary<string, string> childColumns, string primaryKey)
            :base(null, new TimeSpan())
        {
            _databaseRow = new DatabaseDataRow(tableName, recordID, masterColumns, childColumns);
            _tableName = tableName;
            _recordID = recordID;
            _masterDatabase = masterDatabase;
            _childDatabase = childDatabase;
            _primaryKey = primaryKey;
        }

        #endregion Constructors

        #region Properties

        public DatabaseDataRow Differences { get { return (_databaseRow); } }

        #endregion Properties

        #region Overridden Methods

        protected override bool Run(object parameters)
        {
            FbConnection childDB = new FbConnection(_childDatabase);
            FbConnection masterDB = new FbConnection(_masterDatabase);

            try
            {
                childDB.Open();
                masterDB.Open();
                FbTransaction tranChild = null;
                FbTransaction tranMaster = null;
                try
                {
                    tranChild = childDB.BeginTransaction();
                    tranMaster = masterDB.BeginTransaction();

                    _databaseRow.PrimaryKey = _primaryKey;


                    if (!GetFieldValues(childDB, tranChild, _databaseRow.ChildValues))
                    {
                        // if record not found, update hash value and try again
                        UpdateHashValue(childDB, tranChild, _tableName, _primaryKey, _recordID);
                        
                        if (!GetFieldValues(childDB, tranChild, _databaseRow.ChildValues))
                            return (false);
                    }

                    if (!GetFieldValues(masterDB, tranMaster, _databaseRow.MasterValues))
                    {
                        // if record not found, update hash value and try again
                        UpdateHashValue(masterDB, tranMaster, _tableName, _primaryKey, _recordID);

                        if (!GetFieldValues(masterDB, tranMaster, _databaseRow.MasterValues))
                            return (false);
                    }

                    _databaseRow.CompareValues();

                    if (_databaseRow.Differences.Count == 0)
                    {
                        UpdateHashValue(childDB, tranChild, _tableName, _primaryKey, _recordID);
                        UpdateHashValue(masterDB, tranMaster, _tableName, _primaryKey, _recordID);
                    }
                }
                catch (Exception err)
                {
                    string s = err.Message;
                    s = s + "";
                }
                finally
                {
                    if (tranChild != null)
                    {
                        tranChild.Commit();
                        tranChild.Dispose();
                    }

                    if (tranMaster != null)
                    {
                        tranMaster.Commit();
                        tranMaster.Dispose();
                    }
                }
            }
            finally
            {
                if (childDB.State == System.Data.ConnectionState.Open)
                {
                    childDB.Close();
                    childDB.Dispose();
                }

                if (masterDB.State == System.Data.ConnectionState.Open)
                {
                    masterDB.Close();
                    masterDB.Dispose();
                }
            }

            return (false);
        }

        #endregion Overridden Methods

        #region Private Methods

        /// <summary>
        /// Retrieves all values from the database
        /// </summary>
        /// <param name="database"></param>
        /// <param name="transaction"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        private bool GetFieldValues(FbConnection database, FbTransaction transaction, Dictionary<string, string> storage)
        {
            bool Result = true;

            string sql = "SELECT ";
            List<string> fields = new List<string>();

            foreach(KeyValuePair<string, string> kvp in storage)
            {
                fields.Add(kvp.Key);
                sql += String.Format("{0}, ", kvp.Key);
            }

            sql = sql.Substring(0, sql.Length - 2);

            sql += String.Format(" FROM {0} WHERE {1} = {2};", _tableName, _primaryKey, _recordID);

            FbCommand command = new FbCommand(sql, database, transaction);
            FbDataReader reader = command.ExecuteReader();
            try
            {
                if (reader.Read())
                {
                    for (int i = 0; i < fields.Count; i++)
                        storage[fields[i]] = reader.IsDBNull(i) ? "[NULL]" : String.IsNullOrEmpty(reader.GetString(i)) ? "[EMPTY]" : reader.GetString(i);
                }
                else
                    Result = false;
            }
            finally
            {
                reader.Close();
            }

            return (Result);
        }

        /// <summary>
        /// Updates the hash value for a record
        /// </summary>
        /// <param name="database"></param>
        /// <param name="transaction"></param>
        /// <param name="table"></param>
        /// <param name="recordID"></param>
        private void UpdateHashValue(FbConnection database, FbTransaction transaction, string table, string primaryKey, Int64 recordID)
        {
            //(IPTABLE_NAME, IPPRIMARY_KEY, IPRECORD_ID)
            FbCommand cmd = new FbCommand("SD$COMPARE_GENERATE_HASH", database, transaction);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            
            cmd.Parameters.Add("@IPTABLE_NAME", FbDbType.VarChar);
            cmd.Parameters.Add("@IPPRIMARY_KEY", FbDbType.VarChar);
            cmd.Parameters.Add("@IPRECORD_ID", FbDbType.BigInt);

            cmd.Parameters["@IPTABLE_NAME"].Value = table;
            cmd.Parameters["@IPPRIMARY_KEY"].Value = primaryKey;
            cmd.Parameters["@IPRECORD_ID"].Value = recordID;
            cmd.ExecuteNonQuery();
        }

        #endregion Private Methods
    }

    public delegate void DatabaseRowDifferenceDelegate(object sender, DatabaseDataRow e);

    public class DatabaseDataRow
    {
        #region Private Members

        private Dictionary<string, string> _childValues;
        private Dictionary<string, string> _masterValues;
        private List<string> _differences = new List<string>();

        #endregion Private Members

        #region Constructors

        public DatabaseDataRow(string tableName, Int64 recordID, Dictionary<string, string> masterColumns, Dictionary<string, string> childColumns)
        {
            TableName = tableName;
            RecordID = recordID;
            _childValues = childColumns;
            _masterValues = masterColumns;
        }

        #endregion Constructors

        #region Properties

        public string TableName { get; private set; }

        public Int64 RecordID { get; private set; }

        public string PrimaryKey { get; internal set; }

        public Dictionary<string, string> ChildValues { get { return (_childValues); } }

        public Dictionary<string, string> MasterValues { get { return (_masterValues); } }

        public List<string> Differences { get { return (_differences); } }

        #endregion Properties

        #region Public Methods

        public void CompareValues()
        {
            _differences.Clear();

            foreach (KeyValuePair<string, string> kvp in _childValues)
            {
                if (_masterValues[kvp.Key] != kvp.Value)
                    _differences.Add(kvp.Key);
            }

        }

        #endregion Public Methods
    }

    public class GenerateHashDataThread : ThreadManager
    {
        #region Private Members

        private string _table;
        DatabaseObjects _objects;
        private int _currentlyProcessed = 0;
        private int _recordCount = 0;

        #endregion Private Members

        #region Constructors

        public GenerateHashDataThread(FbConnection database, string table, DatabaseObjects objects)
            : base(database.ConnectionString, new TimeSpan(0, 0, 0))
        {
            _table = table;
            _objects = objects;
        }

        #endregion Constructors

        #region Overridden Methods

        protected override bool Run(object parameters)
        {
            int processed = 0;

            FbConnection connection = new FbConnection((string)parameters);
            connection.Open();
            try
            {
                FbTransaction transaction = connection.BeginTransaction();
                try
                {
                    DatabaseObject primKey = _objects.Find(_table, DatabaseObjectType.PrimaryKey);

                    if (primKey == null || primKey.ObjectParameter2.Contains(","))
                        return (false);

                    if (_recordCount == 0)
                        _recordCount = GetRecordCount(connection, transaction, _table, primKey.ObjectParameter2);

                    processed = ProcesssTable(connection, transaction, _table, primKey.ObjectParameter2, _recordCount, _currentlyProcessed);

                    _currentlyProcessed = _currentlyProcessed + processed;

                    // play nicely
                    if (HasCancelled())
                        return (false);

                }
                finally
                {
                    transaction.Commit();
                }
            }
            finally
            {
                connection.Close();
            }


            return (processed > 0);
        }

        #endregion Overridden Methods

        #region Private Methods

        private int ProcesssTable(FbConnection connection, FbTransaction transaction, string tableName, 
            string primaryKey, int recordCount, int currentlyProcessed)
        {
            int Result = 0;

            string sql = String.Format("SELECT FIRST 1000 {0} FROM {1} WHERE {0} NOT IN (SELECT a.SD$TABLE_ID " +
                "FROM SD$COMPARE_RESULTS a WHERE a.SD$TABLE_NAME = '{1}');", primaryKey, tableName);
            FbCommand command = new FbCommand(sql, connection, transaction);
            FbDataReader reader = command.ExecuteReader();

            ValidationArgs args = new ValidationArgs(String.Format("{0}", tableName), 0, recordCount, 0);

            if (currentlyProcessed == 0)
                if (OnHashGenerationUpdate != null)
                    OnHashGenerationUpdate(this, args);

            while (reader.Read())
            {
                Result++;
                try
                {
                    string recordID = reader.GetString(0);

                    string sqlHash = "SD$COMPARE_GENERATE_HASH";

                    FbCommand commandGenHash = new FbCommand(sqlHash, connection, transaction);
                    commandGenHash.CommandType = System.Data.CommandType.StoredProcedure;
                    commandGenHash.Parameters.Add("@ipTABLE_NAME", FbDbType.VarChar, 31);
                    commandGenHash.Parameters.Add("@ipPRIMARY_KEY", FbDbType.VarChar, 31);
                    commandGenHash.Parameters.Add("@ipRECORD_ID", FbDbType.BigInt);

                    commandGenHash.Parameters["@ipTABLE_NAME"].Value = tableName;
                    commandGenHash.Parameters["@ipPRIMARY_KEY"].Value = primaryKey;
                    commandGenHash.Parameters["@ipRECORD_ID"].Value = recordID;

                    commandGenHash.ExecuteNonQuery();

                        //tableName, primaryKey, recordID);
                    //play nicely
                    if (HasCancelled())
                        return (Result);

                    if (Result % 50 == 0)
                    {
                        if (OnHashGenerationUpdate != null)
                        {
                            args.Update(Result + currentlyProcessed);
                            OnHashGenerationUpdate(this, args);
                        }
                    }
                }
                catch (Exception err)
                {
                    if (err.Message.Contains("blah"))
                        throw;
                }
            }

            return (Result);
        }

        /// <summary>
        /// Gets count of records in a table
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private int GetRecordCount(FbConnection connection, FbTransaction transaction, string tableName, string primaryKey)
        {
            int Result = 0;

            string sql = String.Format("SELECT COUNT({0}) FROM {1} WHERE {0} NOT IN (SELECT SD$TABLE_ID FROM SD$COMPARE_RESULTS WHERE SD$TABLE_NAME = '{1}' );", primaryKey, tableName);

            FbCommand cmd = new FbCommand(sql, connection, transaction);

            FbDataReader rdr = cmd.ExecuteReader();
            try
            {
                if (rdr.Read())
                {
                    Result += rdr.GetInt32(0);
                }
            }
            finally
            {
                rdr.Close();
            }

            return (Result);
        }

        #endregion Private Methods

        public string TableName { get { return (_table); } }

        public event ValidationArgsDelegate OnHashGenerationUpdate;
    }

    public class PrepareDatabaseThread : ThreadManager
    {
        #region Private Members

        private string _tables;
        DatabaseObjects _objects;

        #endregion Private Members

        #region Constructors

        public PrepareDatabaseThread (FbConnection database, string tables, DatabaseObjects objects)
            : base(database.ConnectionString, new TimeSpan(0, 0, 0))
        {
            _tables = tables;
            _objects = objects;
        }

        #endregion Constructors

        #region Properties

        #endregion Properties

        #region Overridden Methods

        protected override bool Run(object parameters)
        {
            string scriptFile = String.Format("{0}updatescript.sql",
                Shared.Utilities.AddTrailingBackSlash(Shared.Utilities.CurrentPath()));

            string standardScript = Shared.Utilities.FileRead(String.Format("{0}CompareScriptCreate.sql",
                Shared.Utilities.AddTrailingBackSlash(Shared.Utilities.CurrentPath())), true);

            Shared.Utilities.FileWrite(scriptFile, standardScript);

            FbConnection connection = new FbConnection((string)parameters);
            connection.Open();
            try
            {
                GenerateSQLScript(scriptFile, _tables, connection, null);

                try
                {
                    FbScript script = new FbScript(Shared.Utilities.FileRead(scriptFile, true));
                    script.Parse();

                    foreach (FbStatement cmd in script.Results)
                    {
                        try
                        {
                            FbBatchExecution fbe = new FbBatchExecution(connection);
                            try
                            {
                                fbe.Statements.Add(cmd);
                                fbe.Execute();
                            }
                            finally
                            {
                                fbe.Statements.Clear();
                                fbe = null;
                            }

                            // play nicely
                            if (HasCancelled())
                                return (false);
                        }
                        catch (Exception err)
                        {
                            if ((!err.Message.Contains("unsuccessful metadata update") && !err.Message.Contains("does not exist")) &&
                                !err.Message.ToUpper().Contains("ATTEMPT TO STORE DUPLICATE VALUE") && !err.Message.ToUpper().Contains("ALREADY EXISTS") &&
                                !err.Message.Contains("violation of PRIMARY or UNIQUE") && !err.Message.Contains("violation of FOREIGN KEY constraint"))
                            {

                                throw;
                            }
                        }
                    }
                }
                finally
                {
                    File.Delete(scriptFile);
                }
            }
            finally
            {
                connection.Close();
            }


            return (false);
        }

        #endregion Overridden Methods

        #region Private Methods

        private void GenerateSQLScript(string fileName, string tables, FbConnection connection, FbTransaction transaction)
        {
            // current tables will hold a list of tables that are already in the database with a
            // trigger, in the form of  ID;TRIGGER_NAME, these will be compared to other ones
            // and if any are no longer selected then they will be removed from settinggs table
            string[] currentTables = GetCurrentTableList(connection, transaction).Split('#');

            string[] tableNames = tables.Split(';');

            StreamWriter file = new StreamWriter(fileName, true);
            try
            {
                foreach (string table in tableNames)
                {
                    if (String.IsNullOrEmpty(table))
                        continue;

                    for (int i = 0; i < currentTables.Length; i++)
                    {
                        if (currentTables[i].Contains(table))
                        {
                            currentTables[i] += ";KEEP";
                            break;
                        }
                    }

                    DatabaseObject objDB = _objects.Find(table, DatabaseObjectType.PrimaryKey);

                    if (objDB != null && !objDB.ObjectParameter2.Contains(","))
                        GenerateCreateAlterTrigger(connection, transaction, file, table, String.Format("TR_SD${0}", table), objDB.ObjectParameter2);
                }



                //remove triggers that were previously set but no longer selected
                for (int i = 0; i < currentTables.Length; i++)
                {
                    if (String.IsNullOrEmpty(currentTables[i]) || currentTables[i].Contains(";KEEP"))
                        continue;

                    string[] triggerName = currentTables[i].Split(';');

                    file.WriteLine(String.Format("DROP TRIGGER {0};", triggerName[2]));
                    file.WriteLine(String.Format("DELETE FROM SD$COMPARE_SETTINGS a WHERE a.SD$ID = {0};", triggerName[0]));
                    file.WriteLine("");
                }
            }
            finally
            {
                file.Flush();
                file.Close();
                file = null;
            }
        }

        /// <summary>
        /// Gets a list of current tables with a trigger for updating the hash data
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private string GetCurrentTableList(FbConnection connection, FbTransaction transaction)
        {
            string Result = "";

            string sql = "SELECT a.SD$ID || ';' || a.SD$VALUE FROM SD$COMPARE_SETTINGS a WHERE a.SD$TYPE = 1;";

            FbCommand cmd = new FbCommand(sql, connection, transaction);
            try
            {
                bool first = true;

                FbDataReader rdr = cmd.ExecuteReader();
                try
                {
                    while (rdr.Read())
                    {
                        Result += String.Format("{0}{1}", first ? "" : "#", rdr.GetString(0));

                        if (first)
                            first = false;
                    }
                }
                finally
                {
                    rdr.Close();
                }
            }
            finally
            {
                cmd = null;
            }

            return (Result);
        }

        private void GenerateCreateAlterTrigger(FbConnection conn, FbTransaction tran, StreamWriter file, 
            string tableName, string triggerName, string primaryKey)
        {
            string columnList = "";
            string sql = String.Format("SELECT TRIM(a.RDB$FIELD_NAME), " +
                "CASE f.RDB$FIELD_TYPE WHEN 35 THEN 'Y' ELSE 'N' END FROM RDB$RELATION_FIELDS a " +
                "LEFT JOIN RDB$FIELDS f ON a.RDB$FIELD_SOURCE = f.RDB$FIELD_NAME " +
                "WHERE a.RDB$RELATION_NAME = '{0}' ORDER BY a.RDB$FIELD_NAME;", tableName);

            FbCommand cmd = new FbCommand(sql, conn, tran);
            FbDataReader rdr = cmd.ExecuteReader();
            try
            {
                while (rdr.Read())
                {
                    if (rdr.GetString(1) == "Y")
                        columnList += String.Format("COALESCE(TRIM(SUBSTRING(CAST(NEW.{0} AS VARCHAR(30)) FROM 1 FOR 19)), '') || ", rdr.GetString(0));
                    else
                        columnList += String.Format("COALESCE(NEW.{0}, '') || ", rdr.GetString(0));
                }

                columnList += String.Format("HASH('{0}')", tableName);
            }
            finally
            {
                rdr.Close();
            }

            file.WriteLine("");
            file.WriteLine("SET TERM ^ ;");
            file.WriteLine(String.Format("CREATE OR ALTER TRIGGER {0} FOR {1} ACTIVE", triggerName, tableName));
            file.WriteLine("BEFORE INSERT OR UPDATE POSITION 32766");
            file.WriteLine("AS");
            file.WriteLine("  DECLARE VARIABLE vSelectStatement BLOB SUB_TYPE 1;");
            file.WriteLine("  DECLARE VARIABLE vIsTimeStamp VARCHAR(1);");
            file.WriteLine("  DECLARE VARIABLE vHash BIGINT;");
            file.WriteLine("BEGIN");
            file.WriteLine(String.Format("  IF ((UPDATING) AND (NEW.{0} <> OLD.{0})) THEN", primaryKey));
            file.WriteLine("  BEGIN");
            file.WriteLine("      UPDATE SD$COMPARE_RESULTS cr");
            file.WriteLine(String.Format("      SET cr.SD$TABLE_ID = NEW.{0}", primaryKey));
            file.WriteLine(String.Format("      WHERE cr.SD$TABLE_NAME = '{0}'", tableName));
            file.WriteLine(String.Format("        AND cr.SD$TABLE_ID = OLD.{0};", primaryKey));
            file.WriteLine("  END");
            file.WriteLine("");
            file.WriteLine(String.Format("  vHash = HASH({0});", columnList)); 
            file.WriteLine("");
            file.WriteLine("  UPDATE OR INSERT INTO SD$COMPARE_RESULTS (SD$ID, SD$TABLE_NAME, SD$TABLE_ID, SD$RECORD_HASH)");
            file.WriteLine(String.Format("  VALUES (GEN_ID(SD$GEN_COMPARE_ID, 1), '{0}', new.{1}, :vHash)", tableName, primaryKey));
            file.WriteLine("  MATCHING (SD$TABLE_NAME, SD$TABLE_ID);");
            file.WriteLine("");
            file.WriteLine("END^");
            file.WriteLine("SET TERM ; ^");
            file.WriteLine("");
            file.WriteLine("");
            file.WriteLine("INSERT INTO SD$COMPARE_SETTINGS (SD$ID, SD$NAME, SD$TYPE, SD$VALUE) ");
            file.WriteLine(String.Format("VALUES (NULL, '{0}', 1, '{1};{2}');", tableName, primaryKey, triggerName));
            file.WriteLine("");
            file.WriteLine("");
        }

        #endregion Private Methods
    }
}
