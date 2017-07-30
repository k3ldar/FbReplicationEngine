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
 *  Purpose:  Master Database Preperation
 *
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Data;

using FirebirdSql.Data.FirebirdClient;

namespace Replication.Engine
{
    public class ReplicationPrepareMasterDatabase
    {
        #region Private Members

        private List<string> _xmlHashUpdates = new List<string>();

        #endregion Private Members

        #region Public Methods

        public string GenerateTriggerRemoveScript(string connectionString, bool generateOnly, DatabaseRemoteUpdate remoteUpdate)
        {
            // get temp file for triggers
            string Result = Path.GetTempFileName();
            try
            {
                _xmlHashUpdates.Clear();

                //connect to local DB
                FbConnection db = new FbConnection(connectionString);
                db.Open();
                try
                {
                    FbTransaction tran = db.BeginTransaction(IsolationLevel.ReadCommitted);
                    try
                    {
                        StreamWriter updateFile = new StreamWriter(Result, false);
                        try
                        {
                            // have any tables been removed from the list since the last time this was run?
                            string SQL = "SELECT TRIM(a.RDB$TRIGGER_NAME) " +
                                "FROM RDB$TRIGGERS a WHERE ((TRIM(a.RDB$TRIGGER_NAME) LIKE 'REPLICATE$%'));";
                            FbDataReader rdr = null;
                            FbCommand cmd = new FbCommand(SQL, db, tran);
                            try
                            {
                                rdr = cmd.ExecuteReader();

                                while (rdr.Read())
                                {
                                    updateFile.WriteLine(String.Format("DROP TRIGGER {0};", rdr.GetString(0).Trim()));

                                    string hashDatabase = "D" + Shared.Utilities.HashStringMD5(GetDatabaseName(db));
                                    string hashCode = "C";
                                    string triggerHash = "T" + Shared.Utilities.HashStringMD5(
                                        rdr.GetString(0).Trim().Replace("REPLICATE$", ""));

                                    _xmlHashUpdates.Add(String.Format("{0}${1}${2}", hashDatabase, triggerHash, hashCode));
                                }
                            }
                            finally
                            {
                                CloseAndDispose(ref cmd, ref rdr);
                            }

                            SQL = "SELECT TRIM(a.RDB$RELATION_NAME) FROM RDB$RELATION_FIELDS a " +
                                "WHERE a.RDB$FIELD_NAME = 'REPLICATE$HASH'";
                            cmd = new FbCommand(SQL, db, tran);
                            try
                            {
                                rdr = cmd.ExecuteReader();

                                while (rdr.Read())
                                {
                                    updateFile.WriteLine(String.Format("ALTER TABLE {0} DROP REPLICATE$HASH;", rdr.GetString(0)));
                                }
                            }
                            finally
                            {
                                CloseAndDispose(ref cmd, ref rdr);
                            }
                        }
                        finally
                        {
                            updateFile.Flush();
                            updateFile.Close();
                            updateFile = null;
                        }

                        if (generateOnly)
                            return (Result);

                        bool tableUpdated = false;

                        if (remoteUpdate.UpdateDatabase(connectionString, Result, -1, ref tableUpdated))
                        {
                            File.Delete(Result);

                            foreach (string update in _xmlHashUpdates)
                            {
                                string[] parts = update.Split('$');

                                Shared.XML.SetXMLValue(parts[0], parts[1], parts[2]);
                            }
                        }
                        else
                            throw new Exception("Error creating replication triggers");
                    }
                    finally
                    {
                        tran.Rollback();
                        tran.Dispose();
                    }
                }
                finally
                {
                    db.Close();
                    db.Dispose();
                    db = null;
                }
            }
            catch (Exception e)
            {
                Shared.EventLog.Add(e);
                throw;
            }

            return (Result);
        }

        /// <summary>
        /// Creates new replication triggers based on rules in REPLICATE$TABLES
        /// </summary>
        public bool PrepareDatabaseForReplication(string connectionString, 
            bool dbUpdated, bool generateOnly, ref string fileName, DatabaseRemoteUpdate remoteUpdate)
        {
            bool Result = false;
            try
            {
                _xmlHashUpdates.Clear();

                FbConnection localDB = new FbConnection(connectionString);
                try
                {
                    localDB.Open();

                    if (dbUpdated)
                    {
                        Shared.EventLog.Add("Rebuilding Replication Triggers");
                        fileName = RebuildReplicationTriggers(connectionString, generateOnly);
                        bool tableUpdated = false;

                        if (generateOnly)
                            return (true);

                        if (remoteUpdate.UpdateDatabase(connectionString, fileName, -1, ref tableUpdated))
                        {
                            File.Delete(fileName);

                            foreach (string update in _xmlHashUpdates)
                            {
                                string[] parts = update.Split('$');

                                Shared.XML.SetXMLValue(parts[0], parts[1], parts[2]);
                            }
                        }
                        else
                            throw new Exception("Error creating replication triggers");

                        Result = true;
                    }
                }
                finally
                {
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

        #endregion Public Methods

        #region Trigger Replication Methods

        /// <summary>
        /// Rebuilds all replication triggers based on rules within REPLICATE$TABLES
        /// </summary>
        /// <returns>Physical SQL File on disk containing all the updates</returns>
        private string RebuildReplicationTriggers(string connectionString, bool generateOnly)
        {
            // get current assembly path
            string Result = Path.GetTempFileName(); 
            try
            {
                //connect to local DB
                FbConnection db = new FbConnection(connectionString);
                db.Open();
                try
                {
                    FbTransaction tran = db.BeginTransaction(IsolationLevel.ReadCommitted);
                    try
                    {
                        StreamWriter updateFile = new StreamWriter(Result, false);
                        try
                        {
                            string tableNames = String.Empty;
                            string SQL = "SELECT r.TABLE_NAME, r.OPERATION, r.TRIGGER_NAME, r.EXCLUDE_FIELDS, r.LOCAL_ID_COLUMN \n" +
                                "FROM REPLICATE$TABLES r \nORDER BY r.TABLE_NAME, r.OPERATION ";
                            FbDataReader rdr = null;
                            FbCommand cmd = new FbCommand(SQL, db, tran);
                            try
                            {
                                rdr = cmd.ExecuteReader();

                                while (rdr.Read())
                                {
                                    if (!tableNames.Contains(rdr.GetString(0).Trim()))
                                    {
                                        if (tableNames.Length > 0)
                                            tableNames += String.Format(",'{0}'\n", rdr.GetString(0).Trim());
                                        else
                                            tableNames += String.Format("'{0}'\n", rdr.GetString(0).Trim());
                                    }
                                }
                            }
                            finally
                            {
                                CloseAndDispose(ref cmd, ref rdr);
                            }

                            // have any tables been removed from the list since the last time this was run?
                            SQL = String.Format("SELECT TRIM(a.RDB$TRIGGER_NAME) FROM RDB$TRIGGERS a " +
                                "WHERE ((TRIM(a.RDB$TRIGGER_NAME) LIKE 'REPLICATE$%_ID')) " +
                                "OR ((TRIM(a.RDB$TRIGGER_NAME) <> 'REPLICATE$PK_CHANGES') AND " +
                                "TRIM(a.RDB$TRIGGER_NAME) LIKE 'REPLICATE$%'  " +
                                "AND a.RDB$RELATION_NAME NOT IN (  \n" +
                                tableNames + "\n)) OR a.RDB$TRIGGER_NAME LIKE 'REPLICATE$%_ID';");
                            cmd = new FbCommand(SQL, db, tran);
                            try
                            {
                                rdr = cmd.ExecuteReader();

                                while (rdr.Read())
                                {
                                    updateFile.WriteLine(String.Format("DROP TRIGGER {0};", rdr.GetString(0).Trim()));
                                }
                            }
                            finally
                            {
                                CloseAndDispose(ref cmd, ref rdr);
                            }

                            SQL = "SELECT DISTINCT r.TABLE_NAME \n" +
                                "FROM REPLICATE$TABLES r ";
                            cmd = new FbCommand(SQL, db, tran);
                            try
                            {
                                tableNames = String.Empty;
                                rdr = cmd.ExecuteReader();
                                string hashUpdateTables = String.Empty;

                                while (rdr.Read())
                                {
                                    hashUpdateTables += ReplicateTableHasReplicateFields(db, tran, 
                                        updateFile, rdr.GetString(0).Trim());
                                }
                            }
                            finally
                            {
                                CloseAndDispose(ref cmd, ref rdr);
                            }

                            SQL = "SELECT r.TABLE_NAME, r.OPERATION, r.TRIGGER_NAME, r.EXCLUDE_FIELDS, r.LOCAL_ID_COLUMN \n" +
                                "FROM REPLICATE$TABLES r \nORDER BY r.TABLE_NAME, r.OPERATION ";
                            cmd = new FbCommand(SQL, db, tran);
                            try
                            {
                                tableNames = String.Empty;
                                rdr = cmd.ExecuteReader();

                                while (rdr.Read())
                                {
                                    string triggerCode = String.Empty;

                                    switch (rdr.GetString(1))
                                    {
                                        case "INSERT":
                                            triggerCode = ReplicateCreateTriggerInsert(db, tran, generateOnly,
                                                rdr.GetString(0).Trim(), rdr.GetString(2).Trim(),
                                                rdr.GetString(3));
                                            break;
                                        case "UPDATE":
                                            triggerCode = ReplicateCreateTriggerUpdate(db, tran, generateOnly,
                                                rdr.GetString(0).Trim(), rdr.GetString(2).Trim(),
                                                rdr.GetString(3), rdr.GetString(4));
                                            break;
                                        case "DELETE":
                                            triggerCode = ReplicateCreateTriggerDelete(db, tran, generateOnly,
                                                rdr.GetString(0).Trim(), rdr.GetString(2).Trim(),
                                                rdr.GetString(3));
                                            break;
                                    }

                                    if (!String.IsNullOrEmpty(triggerCode))
                                        updateFile.Write(triggerCode);
                                }
                            }
                            finally
                            {
                                CloseAndDispose(ref cmd, ref rdr);
                            }
                        }
                        finally
                        {
                            updateFile.Flush();
                            updateFile.Close();
                            updateFile = null;
                        }
                    }
                    finally
                    {
                        tran.Rollback();
                        tran.Dispose();
                    }
                }
                finally
                {
                    db.Close();
                    db.Dispose();
                    db = null;
                }
            }
            catch (Exception e)
            {
                Shared.EventLog.Add(e);
                throw;
            }

            return (Result);
        }


        /// <summary>
        /// Ensures replicated tables have the right columns setup
        /// </summary>
        /// <param name="_LocalDB"></param>
        /// <param name="tran"></param>
        /// <param name="updateFile"></param>
        /// <param name="tableName"></param>
        private string ReplicateTableHasReplicateFields(FbConnection conn, FbTransaction tran,
            StreamWriter updateFile, string tableName)
        {
            string Result = String.Empty;

            string SQL = String.Format("SELECT COUNT(a.RDB$FIELD_NAME) FROM RDB$RELATION_FIELDS a " +
                "WHERE a.RDB$FIELD_NAME = 'REPLICATE$HASH' AND a.RDB$RELATION_NAME = '{0}'", tableName);
            FbDataReader rdr = null;
            FbCommand cmd = new FbCommand(SQL, conn, tran);
            try
            {
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    string activateTriggers = String.Empty;
                    string deactivateTriggers = String.Empty;

                    if (rdr.GetInt32(0) == 0)
                    {
                        string sqlTriggers = String.Format("SELECT TRIM(a.RDB$TRIGGER_NAME) " +
                            "FROM RDB$TRIGGERS a WHERE a.RDB$SYSTEM_FLAG = 0 " +
                            " AND a.RDB$TRIGGER_INACTIVE = 0 AND TRIM(UPPER(a.RDB$RELATION_NAME)) = '{0}'", 
                            tableName.Trim().ToUpper());
                        FbDataReader rdrTriggers = null;
                        FbCommand cmdTriggers = new FbCommand(sqlTriggers, conn, tran);
                        try
                        {
                            rdrTriggers = cmdTriggers.ExecuteReader();

                            while (rdrTriggers.Read())
                            {
                                deactivateTriggers += String.Format("ALTER TRIGGER {0} INACTIVE;\r\n", rdrTriggers.GetString(0));
                                activateTriggers += String.Format("ALTER TRIGGER {0} ACTIVE;\r\n", rdrTriggers.GetString(0));
                            }
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd, ref rdrTriggers);
                        }

                        updateFile.Write(deactivateTriggers);
                        updateFile.Write(String.Format("ALTER TABLE {0} ADD REPLICATE$HASH BIGINT;\r\n", tableName));

                        string hashTriggerSQL = String.Format("SELECT r.TABLE_NAME, r.OPERATION, r.TRIGGER_NAME, " +
                            "r.EXCLUDE_FIELDS, r.LOCAL_ID_COLUMN \n" +
                            "FROM REPLICATE$TABLES r WHERE r.OPERATION = 'UPDATE' AND r.TABLE_NAME = '{0}' " +
                            "\nORDER BY r.TABLE_NAME, r.OPERATION ", tableName);
                        FbDataReader rdrHashTrigger = null;
                        FbCommand cmdHashTrigger = new FbCommand(hashTriggerSQL, conn, tran);
                        try
                        {
                            rdrHashTrigger = cmdHashTrigger.ExecuteReader();

                            if (rdrHashTrigger.Read())
                            {
                                updateFile.Write(ReplicateCreateTriggerUpdateHash(conn, tran,
                                    rdrHashTrigger.GetString(0).Trim(), rdrHashTrigger.GetString(2).Trim(),
                                    rdrHashTrigger.GetString(3), rdrHashTrigger.GetString(4)));
                            }
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd, ref rdrHashTrigger);
                        }

                        updateFile.Write(String.Format("UPDATE {0} SET REPLICATE$HASH = NULL;\r\n", tableName));
                        updateFile.Write(activateTriggers);
                        updateFile.WriteLine(String.Empty);

                        Result += String.Format("{0}:", tableName);
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
        /// Create Insert Replication Triggers based on rules engine
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="file"></param>
        /// <param name="TableName"></param>
        /// <param name="TriggerName"></param>
        /// <param name="ExcludeFields"></param>
        private string ReplicateCreateTriggerInsert(FbConnection conn, FbTransaction tran, bool generateOnly,
            string tableName, string triggerName, string excludeFields)
        {
            string Result = String.Empty;
            excludeFields = ":" + excludeFields;

            if (!excludeFields.EndsWith(":"))
                excludeFields += ":";

            int i = 0;
            string Indexes = "";
            string SQL = String.Format("select rc.RDB$RELATION_NAME, ris.rdb$field_name from rdb$relation_constraints rc " +
                "join rdb$index_segments ris on ris.rdb$index_name = rc.rdb$index_name where rc.rdb$relation_name = '{0}' " +
                "and rc.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY'", tableName);
            FbDataReader rdr = null;
            FbCommand cmd = new FbCommand(SQL, conn, tran);
            try
            {
                rdr = cmd.ExecuteReader();
                bool First = true;

                while (rdr.Read())
                {
                    if (First)
                    {
                        Indexes += String.Format("'{0}', NEW.{0}", rdr.GetString(1).Trim());
                        First = false;
                    }
                    else
                        Indexes += String.Format(", '{0}', NEW.{0}", rdr.GetString(1).Trim());

                    i++;
                }
            }
            finally
            {
                CloseAndDispose(ref cmd, ref rdr);
            }

            if (String.IsNullOrEmpty(Indexes))
                return (String.Empty);


            excludeFields += ":REPLICATE$HASH:";

            Result += "SET TERM ^ ;\r\n";
            Result += String.Format("CREATE OR ALTER TRIGGER REPLICATE${0}_I FOR {1} ACTIVE\r\n", triggerName, tableName);
            Result += "BEFORE INSERT POSITION 32767\r\n";
            Result += "AS\r\n";
            Result += "  DECLARE VARIABLE vOperationLogID BIGINT;\r\n";
            Result += "  DECLARE VARIABLE vHASH BIGINT;\r\n";
            Result += "BEGIN\r\n";
            //Result += ReplicateCreateAnyRecordChangedTest(conn, tran, tableName, excludeFields, 1));

            while (i < 3)
            {
                Indexes += ", NULL, NULL";
                i++;
            }

            Result += String.Format("    EXECUTE PROCEDURE REPLICATE$OPERATIONLOG_INSERT ('{0}', 'INSERT', {1}) " +
                "RETURNING_VALUES :vOperationLogID;\r\n", tableName, Indexes);
            Result += "\r\n";

            string updateHash = "NEW.REPLICATE$HASH = HASH(";
            SQL = String.Format("select f.rdb$field_name, CASE flds.RDB$FIELD_TYPE WHEN 261 THEN 50000 ELSE " +
                "flds.RDB$CHARACTER_LENGTH END from rdb$relation_fields f join rdb$relations r on " +
                "f.rdb$relation_name = r.rdb$relation_name " +
                "and r.rdb$view_blr is null and (r.rdb$system_flag is null or r.rdb$system_flag = 0) join " +
                "rdb$fields flds on flds.RDB$FIELD_NAME = f.RDB$FIELD_SOURCE WHERE f.RDB$RELATION_NAME = '{0}' " +
                "order by 1, f.rdb$field_position;", tableName);
            cmd = new FbCommand(SQL, conn, tran);
            try
            {
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    if (!excludeFields.Contains(String.Format(":{0}:", rdr.GetString(0).Trim())))
                    {
                        int Length = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);

                        if (updateHash.Length < 30)
                            updateHash += String.Format("COALESCE(NEW.{0}, '')", rdr.GetString(0).Trim());
                        else
                            updateHash += String.Format(" || COALESCE(NEW.{0}, '')", rdr.GetString(0).Trim());


                        Result += String.Format("    IF (NEW.{0} IS NOT NULL) THEN\r\n", rdr.GetString(0).Trim());
                        Result += "        INSERT INTO REPLICATE$COLUMNLOG (ID, OPERATIONLOG_ID, COLUMN_NAME, " +
                            "OLD_VALUE, NEW_VALUE, OLD_VALUE_BLOB, NEW_VALUE_BLOB)\r\n";

                        if (Length < 301)
                            Result += String.Format("      VALUES (GEN_ID(REPLICATE$COLUMNLOG_ID, 1), :vOperationLogID, " +
                                "'{0}', NULL, NEW.{0}, NULL, NULL);\r\n", rdr.GetString(0).Trim());
                        else
                            Result += String.Format("      VALUES (GEN_ID(REPLICATE$COLUMNLOG_ID, 1), :vOperationLogID, " +
                                "'{0}', NULL, NULL, NULL, NEW.{0});\r\n", rdr.GetString(0).Trim());

                        Result += "\r\n";
                    }
                }
            }
            finally
            {
                CloseAndDispose(ref cmd, ref rdr);
            }

            Result += "\r\n";
            Result += String.Format("    {0});\r\n", updateHash);
            //Result += "   END\r\n";
            Result += "END^\r\n";
            Result += "SET TERM ; ^\r\n";
            Result += "\r\n";
            Result += "\r\n";

            if (IncludeTrigger(conn, generateOnly, String.Format("{0}_I", triggerName), Result))
                return (Result);
            else
                return (String.Empty);
        }

        /// <summary>
        /// Creates Delete Replication Triggers based on rules engine
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="file"></param>
        /// <param name="TableName"></param>
        /// <param name="TriggerName"></param>
        /// <param name="ExcludeFields"></param>
        private string ReplicateCreateTriggerDelete(FbConnection conn, FbTransaction tran, bool generateOnly,
            string tableName, string triggerName, string excludeFields)
        {
            string Result = String.Empty;

            if (!excludeFields.EndsWith(":"))
                excludeFields += ":";

            int i = 0;
            string Indexes = "";
            string SQL = String.Format("select rc.RDB$RELATION_NAME, ris.rdb$field_name from rdb$relation_constraints rc " +
                "join rdb$index_segments ris on ris.rdb$index_name = rc.rdb$index_name where rc.rdb$relation_name = " +
                "'{0}' and rc.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY'", tableName);
            FbDataReader rdr = null;
            FbCommand cmd = new FbCommand(SQL, conn, tran);
            try
            {
                rdr = cmd.ExecuteReader();
                bool First = true;

                while (rdr.Read())
                {
                    if (First)
                    {
                        Indexes += String.Format("'{0}', OLD.{0}", rdr.GetString(1).Trim());
                        First = false;
                    }
                    else
                        Indexes += String.Format(", '{0}', OLD.{0}", rdr.GetString(1).Trim());

                    i++;
                }
            }
            finally
            {
                CloseAndDispose(ref cmd, ref rdr);
            }

            if (String.IsNullOrEmpty(Indexes))
                return (String.Empty);

            excludeFields += ":REPLICATE$HASH:";

            Result += "SET TERM ^ ;\r\n";
            Result += String.Format("CREATE OR ALTER TRIGGER REPLICATE${0}_D FOR {1} ACTIVE\r\n", triggerName, tableName);
            Result += "AFTER DELETE POSITION 32767\r\n";
            Result += "AS\r\n";
            Result += "  DECLARE VARIABLE vOperationLogID BIGINT;\r\n";
            Result += "BEGIN\r\n";
            //Result += ReplicateCreateAnyRecordChangedTest(conn, tran, tableName, excludeFields, 2));


            while (i < 3)
            {
                Indexes += ", NULL, NULL";
                i++;
            }

            Result += String.Format("    EXECUTE PROCEDURE REPLICATE$OPERATIONLOG_INSERT ('{0}', 'DELETE', {1}) " +
                "RETURNING_VALUES :vOperationLogID;\r\n", tableName, Indexes);
            Result += "\r\n";

            SQL = String.Format("select f.rdb$field_name, CASE flds.RDB$FIELD_TYPE WHEN 261 THEN 50000 ELSE " +
                "flds.RDB$CHARACTER_LENGTH END from rdb$relation_fields f join rdb$relations r on f.rdb$relation_name = " +
                "r.rdb$relation_name and r.rdb$view_blr is null and (r.rdb$system_flag is null or r.rdb$system_flag = 0) " +
                "join rdb$fields flds on flds.RDB$FIELD_NAME = f.RDB$FIELD_SOURCE WHERE f.RDB$RELATION_NAME = '{0}' " +
                "order by 1, f.rdb$field_position;", tableName);
            cmd = new FbCommand(SQL, conn, tran);
            try
            {
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    int Length = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);

                    if (!excludeFields.Contains(String.Format("{0}:", rdr.GetString(0).Trim())))
                    {
                        //Result += String.Format("    IF ((OLD.{0} IS DISTINCT FROM NEW.{0})) THEN", rdr.GetString(0).Trim()));
                        Result += String.Format("    IF (OLD.{0} IS NOT NULL) THEN \r\n", rdr.GetString(0).Trim());
                        Result += "      INSERT INTO REPLICATE$COLUMNLOG (ID, OPERATIONLOG_ID, COLUMN_NAME, OLD_VALUE, " +
                            "NEW_VALUE, OLD_VALUE_BLOB, NEW_VALUE_BLOB)\r\n";

                        if (Length < 301)
                            Result += String.Format("      VALUES (GEN_ID(REPLICATE$COLUMNLOG_ID, 1), :vOperationLogID, " +
                                "'{0}', OLD.{0}, NULL, NULL, NULL);\r\n", rdr.GetString(0).Trim());
                        else
                            Result += String.Format("      VALUES (GEN_ID(REPLICATE$COLUMNLOG_ID, 1), :vOperationLogID, " +
                                "'{0}', NULL, NULL, OLD.{0}, NULL);\r\n", rdr.GetString(0).Trim());

                        Result += "\r\n";
                    }
                }
            }
            finally
            {
                CloseAndDispose(ref cmd, ref rdr);
            }

            //Result += "   END");
            Result += "END^\r\n";
            Result += "SET TERM ; ^\r\n";
            Result += "\r\n";
            Result += "\r\n";

            if (IncludeTrigger(conn, generateOnly, String.Format("{0}_D", triggerName), Result))
                return (Result);
            else
                return (String.Empty);
        }

        /// <summary>
        /// Creates Update Replication Triggers based on rules table
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="file"></param>
        /// <param name="TableName"></param>
        /// <param name="TriggerName"></param>
        /// <param name="ExcludeFields"></param>
        private string ReplicateCreateTriggerUpdate(FbConnection conn, FbTransaction tran, bool generateOnly,
            string tableName, string triggerName, string excludeFields, string localIDColumn)
        {
            string Result = String.Empty;

            if (!excludeFields.EndsWith(":"))
                excludeFields += ":";

            int i = 0;
            string Indexes = "";
            string updateHash = "NEW.REPLICATE$HASH = HASH(";
            string SQL = String.Format("select rc.RDB$RELATION_NAME, ris.rdb$field_name from rdb$relation_constraints rc " +
                "join rdb$index_segments ris on ris.rdb$index_name = rc.rdb$index_name where rc.rdb$relation_name = '{0}' " +
                "and rc.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY'", tableName);
            FbDataReader rdr = null;
            FbCommand cmd = new FbCommand(SQL, conn, tran);
            try
            {
                rdr = cmd.ExecuteReader();
                bool First = true;

                while (rdr.Read())
                {
                    if (First)
                    {
                        Indexes += String.Format("'{0}', OLD.{0}", rdr.GetString(1).Trim());
                        First = false;
                    }
                    else
                        Indexes += String.Format(", '{0}', OLD.{0}", rdr.GetString(1).Trim());

                    i++;
                }
            }
            finally
            {
                CloseAndDispose(ref cmd, ref rdr);
            }

            if (String.IsNullOrEmpty(Indexes))
                return (String.Empty);

            excludeFields += ":REPLICATE$HASH:";

            Result += "SET TERM ^ ;\r\n";
            Result += String.Format("CREATE OR ALTER TRIGGER REPLICATE${0}_U FOR {1} ACTIVE\r\n", triggerName, tableName);
            Result += "BEFORE UPDATE POSITION 32767\r\n";
            Result += "AS\r\n";
            Result += "  DECLARE VARIABLE vOperationLogID BIGINT;\r\n";
            Result += "  DECLARE VARIABLE vHASH BIGINT;\r\n";
            Result += "BEGIN\r\n";
            //Result += ReplicateCreateAnyRecordChangedTest(conn, tran, tableName, excludeFields, 3));

            while (i < 3)
            {
                Indexes += ", NULL, NULL";
                i++;
            }

            Result += String.Format("    EXECUTE PROCEDURE REPLICATE$OPERATIONLOG_INSERT ('{0}', 'UPDATE', {1}) " +
                "RETURNING_VALUES :vOperationLogID;\r\n", tableName, Indexes);
            Result += "\r\n";

            SQL = String.Format("select f.rdb$field_name, CASE flds.RDB$FIELD_TYPE WHEN 261 THEN 50000 ELSE " +
                "flds.RDB$CHARACTER_LENGTH END from rdb$relation_fields f join rdb$relations r on f.rdb$relation_name = " +
                "r.rdb$relation_name and r.rdb$view_blr is null and (r.rdb$system_flag is null or r.rdb$system_flag = 0) " +
                "join rdb$fields flds on flds.RDB$FIELD_NAME = f.RDB$FIELD_SOURCE WHERE f.RDB$RELATION_NAME = '{0}' " +
                "order by 1, f.rdb$field_position;", tableName);
            cmd = new FbCommand(SQL, conn, tran);
            try
            {
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    int Length = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);

                    if (!excludeFields.Contains(String.Format("{0}:", rdr.GetString(0).Trim())))
                    {
                        if (updateHash.Length < 30)
                            updateHash += String.Format("COALESCE(NEW.{0}, '')", rdr.GetString(0).Trim());
                        else
                            updateHash += String.Format(" || COALESCE(NEW.{0}, '')", rdr.GetString(0).Trim());

                        Result += String.Format("    IF ((OLD.{0} IS DISTINCT FROM NEW.{0})) THEN\r\n", rdr.GetString(0).Trim());
                        Result += "    INSERT INTO REPLICATE$COLUMNLOG (ID, OPERATIONLOG_ID, COLUMN_NAME, OLD_VALUE, " +
                            "NEW_VALUE, OLD_VALUE_BLOB, NEW_VALUE_BLOB)\r\n";

                        if (Length < 301)
                            Result += String.Format("    VALUES (GEN_ID(REPLICATE$COLUMNLOG_ID, 1), :vOperationLogID, " +
                                "'{0}', OLD.{0}, NEW.{0}, NULL, NULL);\r\n", rdr.GetString(0).Trim());
                        else
                            Result += String.Format("    VALUES (GEN_ID(REPLICATE$COLUMNLOG_ID, 1), :vOperationLogID, " +
                                "'{0}', NULL, NULL, OLD.{0}, NEW.{0});\r\n", rdr.GetString(0).Trim());

                        Result += "\r\n";
                    }
                }
            }
            finally
            {
                CloseAndDispose(ref cmd, ref rdr);
            }

            //Result += "   END");
            Result += "\r\n";
            Result += String.Format("   {0});\r\n", updateHash);
            Result += "END^\r\n";
            Result += "SET TERM ; ^\r\n";
            Result += "\r\n";
            Result += "\r\n";

            if (IncludeTrigger(conn, generateOnly, String.Format("{0}_U", triggerName), Result))
                return (Result);
            else
                return (String.Empty);
        }

        /// <summary>
        /// Creates Update Replication Triggers based on rules table
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="file"></param>
        /// <param name="TableName"></param>
        /// <param name="TriggerName"></param>
        /// <param name="ExcludeFields"></param>
        private string ReplicateCreateTriggerUpdateHash(FbConnection conn, FbTransaction tran, 
            string tableName, string triggerName, string excludeFields, string localIDColumn)
        {
            string Result = String.Empty;
            int i = 0;
            string updateHash = "NEW.REPLICATE$HASH = HASH(";
            string Indexes = "";
            string SQL = String.Format("select rc.RDB$RELATION_NAME, ris.rdb$field_name from rdb$relation_constraints rc " +
                "join rdb$index_segments ris on ris.rdb$index_name = rc.rdb$index_name where rc.rdb$relation_name = '{0}' " +
                "and rc.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY'", tableName);
            FbDataReader rdr = null;
            FbCommand cmd = new FbCommand(SQL, conn, tran);
            try
            {
                rdr = cmd.ExecuteReader();
                bool First = true;

                while (rdr.Read())
                {
                    if (First)
                    {
                        Indexes += String.Format("'{0}', OLD.{0}", rdr.GetString(1).Trim());
                        First = false;
                    }
                    else
                        Indexes += String.Format(", '{0}', OLD.{0}", rdr.GetString(1).Trim());

                    i++;
                }
            }
            finally
            {
                CloseAndDispose(ref cmd, ref rdr);
            }

            if (String.IsNullOrEmpty(Indexes))
                return (String.Empty);

            if (!excludeFields.EndsWith(":"))
                excludeFields += ":";

            excludeFields += ":REPLICATE$HASH:";

            Result += "\r\n\r\nSET TERM ^ ;\r\n";
            Result += String.Format("CREATE OR ALTER TRIGGER REPLICATE${0}_U FOR {1} ACTIVE\r\n", triggerName, tableName);
            Result += "BEFORE UPDATE POSITION 32767\r\n";
            Result += "AS\r\n";
            Result += "  DECLARE VARIABLE vHASH BIGINT;\r\n";
            Result += "BEGIN\r\n";

            SQL = String.Format("select f.rdb$field_name, CASE flds.RDB$FIELD_TYPE WHEN 261 THEN 50000 ELSE " +
                "flds.RDB$CHARACTER_LENGTH END from rdb$relation_fields f join rdb$relations r on " +
                "f.rdb$relation_name = r.rdb$relation_name " +
                "and r.rdb$view_blr is null and (r.rdb$system_flag is null or r.rdb$system_flag = 0) join rdb$fields " +
                "flds on flds.RDB$FIELD_NAME = f.RDB$FIELD_SOURCE WHERE f.RDB$RELATION_NAME = '{0}' " +
                "order by 1, f.rdb$field_position;", tableName);
            cmd = new FbCommand(SQL, conn, tran);
            try
            {
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    int Length = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);

                    if (!excludeFields.Contains(String.Format("{0}:", rdr.GetString(0).Trim())))
                    {
                        if (updateHash.Length < 30)
                            updateHash += String.Format("COALESCE(NEW.{0}, '')", rdr.GetString(0).Trim());
                        else
                            updateHash += String.Format(" || COALESCE(NEW.{0}, '')", rdr.GetString(0).Trim());
                    }
                }
            }
            finally
            {
                CloseAndDispose(ref cmd, ref rdr);
            }

            Result += String.Format(" {0});\r\n", updateHash);
            Result += "END^\r\n";
            Result += "SET TERM ; ^\r\n";
            Result += "\r\n";
            Result += "\r\n";

            return (Result);
        }

        private bool IncludeTrigger(FbConnection conn, bool generateOnly, string trigger, string code)
        {
            if (generateOnly)
                return (true);

            string hashDatabase = "D" + Shared.Utilities.HashStringMD5(GetDatabaseName(conn));
            string hashCode = "C";
            if (String.IsNullOrEmpty(code))
                hashCode += "0";
            else
                hashCode += Shared.Utilities.HashStringMD5(code);

            byte[] data = System.Text.Encoding.ASCII.GetBytes(trigger);
            string triggerHash = "T" + Shared.Utilities.HashStringMD5(trigger);

            string currentHash = Shared.XML.GetXMLValue(hashDatabase, triggerHash, String.Empty, String.Empty);

            if (currentHash == hashCode)
                return (false);

            _xmlHashUpdates.Add(String.Format("{0}${1}${2}", hashDatabase, triggerHash, hashCode));
            return (true);
        }

        private string GetDatabaseName(FbConnection conn)
        {
            FbConnectionStringBuilder cb = new FbConnectionStringBuilder(conn.ConnectionString);
            return (cb.Database);
        }

        private void CloseAndDispose(ref FbCommand cmd, ref FbDataReader rdr)
        {
            if (rdr != null)
            {
                rdr.Close();
                rdr.Dispose();
                rdr = null;
            }

            if (cmd != null)
            {
                cmd.Dispose();
                cmd = null;
            }
        }

        #endregion Trigger Replication Methods
    }
}
