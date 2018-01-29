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
 *  Purpose:  Replication API
 *
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.IO;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;

using Replication.Engine;
using Replication.Engine.Classes;

#pragma warning disable IDE1005
#pragma warning disable IDE1006
#pragma warning disable IDE0018

namespace Replication.Service
{

    /// <summary>
    /// Replication Service API Class
    /// </summary>
    public sealed class API
    {
        #region Constants

        internal const int MAX_THREADS = 500;

        #endregion Constants

        #region Constructors

        public API()
        {

        }

        public API(string path, string encryptionKey)
            : this()
        {
            Path = path;
            EncryptionKey = encryptionKey;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Configuration File Path
        /// </summary>
        private string Path { get; set; }

        /// <summary>
        /// Encryption Key used to encrypt files
        /// </summary>
        private string EncryptionKey { get; set; }

        #endregion Properties

        #region Public Methods

        public string[] GetConfigFiles()
        {
            return (Directory.GetFiles(Path, "*.frc"));
        }

        public List<ConfigFileNode> GetConfigurationSettings()
        {
            List<ConfigFileNode> Result = new List<ConfigFileNode>();

            string[] configFiles = GetConfigFiles();

            foreach (string file in configFiles)
            {
                ConfigFileNode node = new ConfigFileNode
                {
                    Connection = DatabaseConnection.Load(file, EncryptionKey),
                    FileName = file
                };

                if (node != null)
                {
                    Result.Add(node);
                }
            }

            return (Result);
        }

        public bool ChildIsConfiguredForReplication(DatabaseConnection connection)
        {
            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = "SELECT a.RDB$TRIGGER_NAME FROM RDB$TRIGGERS a WHERE a.RDB$TRIGGER_NAME LIKE 'REPLICATE$%' " +
                    "UNION SELECT a.RDB$FIELD_NAME FROM RDB$RELATION_FIELDS a WHERE a.RDB$FIELD_NAME = 'REPLICATE$HASH'";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                        return (true);
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (false);
        }


        public Int64 GetSiteID(DatabaseConnection connection)
        {
            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, false);
            try
            {
                string SQL = String.Format("SELECT SITE_ID FROM REPLICATE$OPTIONS;", connection.SiteID);
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                        return (rdr.GetInt64(0));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (-1);
        }

        public void SetSiteID(DatabaseConnection connection)
        {
            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = String.Format("UPDATE REPLICATE$OPTIONS SET SITE_ID = {0};", connection.SiteID);
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    CloseAndDispose(ref cmd);
                    tran.Commit();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }
        }

        public int GetCurrentDatabaseVersion(DatabaseConnection connection)
        {
            int Result = 0;

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, false);
            try
            {
                string SQL = "SELECT a.DATABASE_VERSION FROM REPLICATE$OPTIONS a";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                    {
                        Result = rdr.GetInt32(0);
                    }
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public bool UpdateCurrentDatabaseVersion(DatabaseConnection connection, int version)
        {
            bool Result = false;

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, false);
            try
            {
                string SQL = String.Format("UPDATE REPLICATE$OPTIONS a SET a.DATABASE_VERSION = {0}", version);
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    cmd.ExecuteNonQuery();
                    Result = true;
                }
                finally
                {
                    CloseAndDispose(ref cmd);
                    tran.Commit();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public int GetTotalUsage()
        {
            return (MAX_THREADS);
        }

        public string GetConfigurationFileName(DatabaseConnection connection)
        {
            for (int i = 1; i <= MAX_THREADS; i++)
            {
                string newFile = Path + String.Format("File{0}.frc", i);

                if (!File.Exists(newFile))
                    return (newFile);
            }

            throw new Exception("Maximum of 10 connections used");
        }

        #region Child Database

        public void RemoveReplicatedTable(DatabaseConnection connection, 
            string tableName, bool removeFromMaster, bool insert, bool update,
            bool delete)
        {
            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                try
                {
                    string SQL = "DELETE FROM REPLICATE$TABLES a WHERE a.TABLE_NAME = '{0}' AND a.OPERATION = '{1}';";
                    FbCommand cmd = null;
                    
                    if (insert)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "INSERT"), db, tran);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd);
                        }
                    }
                    
                    if (update)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "UPDATE"), db, tran);
                        try
                        { 
                            cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd);
                        }
                    }
                    
                    if (delete)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "DELETE"), db, tran);
                        try
                        { 
                            cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd);
                        }
                    }

                    if (removeFromMaster)
                        RemoveMasterReplicatedTable(connection, tableName, insert, update, delete);
                }
                finally
                {
                    tran.Commit();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }
        }

        public List<string> GetChildReplicatedTables(DatabaseConnection connection)
        {
            List<string> Result = new List<string>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = "SELECT DISTINCT TRIM(a.TABLE_NAME) FROM REPLICATE$TABLES a ORDER BY a.TABLE_NAME";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(rdr.GetString(0));
                }
                catch (Exception err)
                {
                    if (!err.Message.Contains("Table unknown"))
                        throw;
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public void SetChildSortOrder(DatabaseConnection connection, string tableName, int suggested)
        {
            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = String.Format("UPDATE REPLICATE$TABLES SET SORT_ORDER = {0} WHERE TABLE_NAME = '{1}';",
                    suggested, tableName);
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    tran.Commit();
                    CloseAndDispose(ref cmd);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }
        }

        public void SetChildGeneratorValues(DatabaseConnection connection, List<Generators> generators)
        {
            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                try
                {
                    string SQL = "SET GENERATOR {0} TO {1};";

                   foreach (Generators gen in generators)
                   {
                       FbCommand cmd = new FbCommand(String.Format(SQL, gen.Name, gen.NewValue), db, tran);
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
                    tran.Commit();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }
        }

        public List<Generators> GetChildGeneratorValues(DatabaseConnection connection)
        {
            List<Generators> Result = new List<Generators>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                try
                {
                    string SQL = "EXECUTE BLOCK RETURNS (opNAME VARCHAR(310), opVALUE BIGINT, opSetREMOTE CHAR(1), opTYPE VARCHAR(15)) \n" +
                        "AS  \n" +
                        "   DECLARE VARIABLE vRemoteGen VARCHAR(31); \n" +
                        "   DECLARE VARIABLE vTable VARCHAR(31); \n" +
                        "   DECLARE VARIABLE vColumn VARCHAR(31); \n" +
                        "   DECLARE VARIABLE vColType1 INTEGER; \n" +
                        "   DECLARE VARIABLE vColType2 INTEGER;  \n" +
                        "BEGIN  \n" +
                        "    FOR  \n" +
                        "         SELECT TRIM(a.RDB$GENERATOR_NAME)  \n" +
                        "         FROM RDB$GENERATORS a  \n" +
                        "         WHERE (a.RDB$SYSTEM_FLAG IS NULL OR a.RDB$SYSTEM_FLAG = 0)  \n" +
                        "            AND (a.RDB$GENERATOR_NAME NOT LIKE 'REPLICATE$%')  \n" +
                        "         ORDER BY a.RDB$GENERATOR_NAME  \n" +
                        "         INTO :opNAME  \n" +
                        "    DO  \n" +
                        "    BEGIN  \n" +
                        "        opSetREMOTE = 'N';  \n" +
                        "        IF (EXISTS(SELECT rt.ID FROM REPLICATE$TABLES rt WHERE rt.REMOTE_GENERATOR = :opNAME)) THEN  \n" +
                        "          opSetREMOTE = 'Y';  \n" +
                        "        vColType2 = 100000; \n" +
                        "        FOR \n" +
                        "            SELECT COALESCE(UPPER(rt.REMOTE_GENERATOR), ''), rt.TABLE_NAME, rt.LOCAL_ID_COLUMN       \n" +   
                        "            FROM REPLICATE$TABLES rt          \n" +
                        "            WHERE rt.LOCAL_GENERATOR = :opNAME          \n" +
                        "            INTO :vRemoteGen, :vTable, :vColumn \n" +
                        "        DO \n" +
                        "        BEGIN \n" +
                        "            FOR \n" +
                        "              SELECT f.RDB$FIELD_TYPE \n" +
                        "               FROM RDB$RELATION_FIELDS r \n" +
                        "               LEFT JOIN RDB$FIELDS f ON r.RDB$FIELD_SOURCE = f.RDB$FIELD_NAME \n" +
                        "              WHERE r.RDB$RELATION_NAME = :vTable \n" +
                        "                AND r.RDB$FIELD_NAME = :vColumn   \n" +
                        "              INTO :vColType1 \n" +
                        "            DO \n" +
                        "            BEGIN \n" +
                        "                IF (vColType1 < vColType2) THEN \n" +
                        "                    vColType2 = vColType1; \n" +
                        "            END \n" +
                        "        END \n" +
                        "        IF (vColType2 = 16) THEN \n" +
                        "            opTYPE = 'BIGINT'; \n" +
                        "        ELSE  \n" +
                        "            opTYPE = 'INT'; \n" +
                        "        EXECUTE STATEMENT 'SELECT GEN_ID(' || opNAME || ', 0) FROM RDB$DATABASE' INTO :opVALUE; \n" + 
                        "        SUSPEND;  \n" +
                        "    END  \n" +
                        "END ";
                    FbDataReader rdr = null;
                    FbCommand cmd = new FbCommand(SQL, db, tran);
                    try
                    {
                        rdr = cmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            Result.Add(new Generators(connection, rdr.GetString(0), rdr.GetInt64(1),
                                rdr.GetString(2) == "Y", rdr.GetString(3) == "BIGINT"));
                        }
                    }
                    finally
                    {
                        CloseAndDispose(ref cmd, ref rdr);
                    }
                }
                catch (Exception err)
                {
                    if (!err.Message.Contains("Table unknown"))
                        throw;
                }
                finally
                {
                    tran.Rollback();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public List<string> GetChildTriggerNames(DatabaseConnection connection)
        {
            List<string> Result = new List<string>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                try
                {
                    string SQL = "SELECT DISTINCT TRIM(a.TRIGGER_NAME) FROM REPLICATE$TABLES a order by a.ID DESC";
                    FbDataReader rdr = null;
                    FbCommand cmd = new FbCommand(SQL, db, tran);
                    try
                    {
                        rdr = cmd.ExecuteReader();

                        while (rdr.Read())
                            Result.Add(rdr.GetString(0));
                    }
                    finally
                    {
                        CloseAndDispose(ref cmd, ref rdr);
                    }
                }
                finally
                {
                    tran.Rollback();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public List<string> GetChildTables(DatabaseConnection connection)
        {
            List<string> Result = new List<string>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = "select TRIM(rdb$relation_name)\nfrom rdb$relations\nwhere rdb$view_blr is null\n" +
                    "and (rdb$system_flag is null or rdb$system_flag = 0)\n and (rdb$relation_name NOT LIKE 'REPLICATE$%')\n" +
                    "ORDER BY rdb$relation_name";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(rdr.GetString(0));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public List<ForeignKeys> GetChildForeignKeys(DatabaseConnection connection)
        {
            List<ForeignKeys> Result = new List<ForeignKeys>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = "SELECT TRIM(drc.RDB$RELATION_NAME), TRIM(dis.RDB$FIELD_NAME), TRIM(mrc.RDB$RELATION_NAME), TRIM(mis.RDB$FIELD_NAME) " +
                    "FROM RDB$RELATION_CONSTRAINTS drc " +
                    "    JOIN RDB$INDEX_SEGMENTS dis ON (dis.RDB$INDEX_NAME = drc.RDB$INDEX_NAME) " +
                    "    JOIN RDB$REF_CONSTRAINTS rc ON (rc.RDB$CONSTRAINT_NAME = drc.RDB$CONSTRAINT_NAME) " +
                    "    JOIN RDB$RELATION_CONSTRAINTS mrc ON (mrc.RDB$CONSTRAINT_NAME = rc.RDB$CONST_NAME_UQ) " +
                    "    JOIN RDB$INDEX_SEGMENTS mis ON (mis.RDB$INDEX_NAME = mrc.RDB$INDEX_NAME) " +
                    "WHERE drc.RDB$CONSTRAINT_TYPE = 'FOREIGN KEY' " +
                    "ORDER BY drc.RDB$RELATION_NAME";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(new ForeignKeys(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2), rdr.GetString(3)));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public List<PrimaryKeys> GetChildPrimaryKeys2(DatabaseConnection connection)
        {
            List<PrimaryKeys> Result = new List<PrimaryKeys>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = "SELECT TRIM(rc.RDB$RELATION_NAME), TRIM(ix.RDB$INDEX_NAME), TRIM(sg.RDB$FIELD_NAME), \n" +
                    "CASE f.RDB$FIELD_TYPE WHEN 16 THEN 'BIGINT' WHEN 8 THEN 'INT' ELSE 'OTHER' END \n" +
                    "FROM RDB$INDICES ix \n" +
                    "     LEFT JOIN RDB$INDEX_SEGMENTS sg ON (ix.RDB$INDEX_NAME = sg.RDB$INDEX_NAME) \n" +
                    "     LEFT JOIN RDB$RELATION_CONSTRAINTS rc ON (rc.RDB$INDEX_NAME = ix.RDB$INDEX_NAME) \n" + 
                    "     LEFT JOIN RDB$RELATION_FIELDS rf ON (rf.RDB$FIELD_NAME = sg.RDB$FIELD_NAME AND rf.RDB$RELATION_NAME = rc.RDB$RELATION_NAME) \n" +
                    "     LEFT JOIN RDB$FIELDS f ON (f.RDB$FIELD_NAME = rf.RDB$FIELD_SOURCE) \n" +
                    "WHERE rc.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY'  \n" +
                    "ORDER BY rc.RDB$RELATION_NAME";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(new PrimaryKeys(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2), rdr.GetString(3).Trim()));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public Dictionary<string, List<string>> GetChildPrimaryKeys(DatabaseConnection connection)
        {
            Dictionary<string, List<string>> Result = new Dictionary<string, List<string>>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                 string SQL = "select TRIM(rc.rdb$relation_name), TRIM(sg.rdb$field_name) " +
                    "from rdb$indices ix left join rdb$index_segments sg on ix.rdb$index_name = sg.rdb$index_name " +
                    "left join rdb$relation_constraints rc on rc.rdb$index_name = ix.rdb$index_name " +
                    "where rc.rdb$constraint_type = 'PRIMARY KEY' ORDER BY rc.rdb$relation_name, sg.RDB$FIELD_NAME;";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();
                    string lastTable = string.Empty;

                    while (rdr.Read())
                    {
                        if (lastTable != rdr.GetString(0).Trim())
                        {
                            lastTable = rdr.GetString(0);
                            Result.Add(lastTable, new List<string>());
                        }

                        Result[lastTable].Add(rdr.GetString(1));
                    }
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public void ChildAddTable(DatabaseConnection connection, string tableName, string triggerName)
        {
            string SQL = "INSERT INTO REPLICATE$TABLES (TABLE_NAME, OPERATION, TRIGGER_NAME, SORT_ORDER, " +
                "EXCLUDE_FIELDS, LOCAL_GENERATOR, REMOTE_GENERATOR, LOCAL_ID_COLUMN, INDICE_TYPE, OPTIONS, ID) " +
                " VALUES ('{0}', '{1}', '{2}', 0, '', '', '', '', 0, 0, GEN_ID(REPLICATE$REPLICATETABLES_ID, 1))";

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                
                try
                {
                    FbCommand cmd = new FbCommand(String.Format(SQL, tableName, "INSERT", triggerName), db, tran);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        CloseAndDispose(ref cmd);
                    }

                    cmd = new FbCommand(String.Format(SQL, tableName, "DELETE", triggerName), db, tran);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        CloseAndDispose(ref cmd);
                    }

                    cmd = new FbCommand(String.Format(SQL, tableName, "UPDATE", triggerName), db, tran);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        CloseAndDispose(ref cmd);
                    }
                }
                finally
                {
                    tran.Commit();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }
        }
        
        public List<string> GetChildGenerators(DatabaseConnection connection)
        {
            List<string> Result = new List<string>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = "SELECT TRIM(a.RDB$GENERATOR_NAME) FROM RDB$GENERATORS a " +
                    "WHERE a.RDB$SYSTEM_FLAG IS NULL or a.RDB$SYSTEM_FLAG = 0 " +
                    "ORDER BY a.RDB$GENERATOR_NAME";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(rdr.GetString(0));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public List<string> GetChildTableColumns(DatabaseConnection connection, string tableName)
        {
            List<string> Result = new List<string>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = String.Format("SELECT TRIM(a.RDB$FIELD_NAME) FROM RDB$RELATION_FIELDS a " +
                    "WHERE a.RDB$RELATION_NAME = '{0}' AND a.RDB$FIELD_NAME <> 'REPLICATE$HASH' " +
                    "ORDER BY a.RDB$FIELD_POSITION", tableName);
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(rdr.GetString(0));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public List<ReplicatedTable> GetChildTableReplicatedTable(DatabaseConnection connection, string tableName)
        {
            List<ReplicatedTable> Result = new List<ReplicatedTable>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = String.Format("SELECT a.ID, a.TABLE_NAME, a.OPERATION, a.TRIGGER_NAME, " +
                    "a.SORT_ORDER, a.EXCLUDE_FIELDS, a.LOCAL_GENERATOR, a.REMOTE_GENERATOR, " +
                    "a.LOCAL_ID_COLUMN, a.INDICE_TYPE, a.OPTIONS FROM REPLICATE$TABLES a " +
                    "WHERE a.TABLE_NAME = '{0}'", tableName);
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(new ReplicatedTable(rdr.GetInt64(0), rdr.GetString(1),
                            (Operation)Enum.Parse(typeof(Operation), rdr.GetString(2), true), 
                            rdr.GetString(3), rdr.GetInt32(4), rdr.GetString(5), rdr.GetString(6), rdr.GetString(7),
                            rdr.GetString(8), rdr.GetInt32(9), (TableOptions)rdr.GetInt64(10)));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public void ChildUpdateReplicatedTables(DatabaseConnection connection, string tableName, string localID, 
            bool insert, bool update, bool delete, string triggerName, string excludeFields, int sortOrder,
            int indiceType, string localGenerator, string remoteGenerator, bool updateMaster, TableOptions options)
        {
            RemoveReplicatedTable(connection, tableName, updateMaster, true, true, true);

            string SQL = "INSERT INTO REPLICATE$TABLES (TABLE_NAME, OPERATION, TRIGGER_NAME, SORT_ORDER, " +
                "EXCLUDE_FIELDS, LOCAL_GENERATOR, REMOTE_GENERATOR, LOCAL_ID_COLUMN, INDICE_TYPE, OPTIONS, ID) " +
                " VALUES ('{0}', '{1}', '{2}', {3}, '{4}', '{5}', '{6}', '{7}', {8}, {9}, " +
                "GEN_ID(REPLICATE$REPLICATETABLES_ID, 1))";

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                try
                {
                    FbCommand cmd;

                    if (insert)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "INSERT",
                            triggerName, sortOrder, excludeFields, localGenerator, remoteGenerator,
                            localID, indiceType, (Int64)options), db, tran);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd);
                        }
                    }

                    if (delete)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "DELETE",
                            triggerName, sortOrder, excludeFields, String.Empty, String.Empty,
                            localID, indiceType, (Int64)options), db, tran);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd);
                        }
                    }

                    if (update)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "UPDATE",
                            triggerName, sortOrder, excludeFields, localGenerator, remoteGenerator,
                            localID, indiceType, (Int64)options), db, tran);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd);
                        }
                    }

                    if (updateMaster)
                        MasterUpdateReplicatedTables(connection, tableName, localID, insert, update,
                            delete, triggerName, excludeFields, sortOrder, indiceType, localGenerator,
                            remoteGenerator, options);
                }
                finally
                {
                    tran.Commit();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

        }

        public List<string> ChildKeys(DatabaseConnection connection, string tableName)
        {
            List<string> Result = new List<string>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = String.Format("SELECT TRIM(a.RDB$INDEX_NAME) FROM RDB$INDICES a " +
                    "WHERE a.RDB$RELATION_NAME = '{0}'", tableName);
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(rdr.GetString(0));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public void DeleteRule(DatabaseConnection connection, AutoCorrectRule rule)
        {
            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = String.Format("DELETE FROM REPLICATE$AUTOCORRECTRULES a WHERE a.OPTIONS = {0} " +
                    "AND a.TABLE_NAME = '{1}' and a.KEY_NAME = '{2}';", (int)rule.Options, rule.TableName, rule.KeyName);
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    tran.Commit();
                    CloseAndDispose(ref cmd);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }
        }

        public void AutoCorrectRuleAdd(DatabaseConnection connection, AutoCorrectRule rule)
        {
            DeleteRule(connection, rule);
            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, true);
            try
            {
                string SQL = "INSERT INTO REPLICATE$AUTOCORRECTRULES (TABLE_NAME, KEY_NAME, TARGET_TABLE, " +
                    "TARGET_COLUMN, REPLICATE_COLUMN_NAME, OPTIONS, DEPENDENCIES, SQL_RULE, SQL_RULE_REMOTE) " +
                    "VALUES (@TABLE_NAME, @KEY_NAME, @TARGET_TABLE, @TARGET_COLUMN, @REPLICATE_COLUMN_NAME, " +
                    "@OPTIONS, @DEPENDENCIES, @SQL_RULE, @SQL_RULE_REMOTE);";
                    
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    AddParam(cmd, "@TABLE_NAME", FbDbType.VarChar, rule.TableName);
                    AddParam(cmd, "@KEY_NAME", FbDbType.VarChar, rule.KeyName);
                    AddParam(cmd, "@TARGET_TABLE", FbDbType.VarChar, rule.TargetTable);
                    AddParam(cmd, "@TARGET_COLUMN", FbDbType.VarChar, rule.TargetColumn);
                    AddParam(cmd, "@REPLICATE_COLUMN_NAME", FbDbType.VarChar, rule.ReplicateName);
                    AddParam(cmd, "@OPTIONS", FbDbType.Integer, (int)rule.Options);
                    AddParam(cmd, "@DEPENDENCIES", FbDbType.VarChar, rule.Dependencies);
                    AddParam(cmd, "@SQL_RULE", FbDbType.VarChar, rule.SQLRuleLocal);
                    AddParam(cmd, "@SQL_RULE_REMOTE", FbDbType.VarChar, rule.SQLRuleRemote);
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    tran.Commit();
                    CloseAndDispose(ref cmd);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }
        }

        /// <summary>
        /// Loads a list of rules which attempt to automatically fix data issues
        /// </summary>
        /// <returns></returns>
        public AutoCorrectRules LoadAutoCorrectRules(DatabaseConnection connection)
        {
            AutoCorrectRules Result = new AutoCorrectRules();
            try
            {
                FbTransaction localTran = null;
                FbConnection localDB = new FbConnection(connection.ChildDatabase);
                try
                {
                    localDB.Open();

                    localTran = localDB.BeginTransaction(IsolationLevel.ReadCommitted);
                    FbDataReader rdr = null;
                    FbCommand cmd = new FbCommand("SELECT a.TABLE_NAME, a.KEY_NAME, a.TARGET_TABLE, " +
                        "a.TARGET_COLUMN, a.REPLICATE_COLUMN_NAME, a.OPTIONS, a.SQL_RULE, A.SQL_RULE_REMOTE, a.DEPENDENCIES " +
                        "FROM REPLICATE$AUTOCORRECTRULES a", localDB, localTran);
                    try
                    {
                        rdr = cmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            Result.Add(new AutoCorrectRule(rdr.GetString(0), rdr.GetString(1),
                                rdr.GetString(2), rdr.GetString(3), rdr.GetString(4), rdr.GetString(6),
                                rdr.GetString(7), rdr.GetString(8), (AutoFixOptions)rdr.GetInt32(5)));
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
                    CloseAndDispose(ref localDB, ref localTran);
                }
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
            }

            return (Result);
        }

        public void UpdateReplicationVersion(DatabaseConnection connection, bool master)
        {
            int replicationVersion = 0;

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.ChildDatabase, ref tran, false);
            try
            {
                try
                {
                    string SQL = "SELECT a.REPLICATION_VERSION FROM REPLICATE$OPTIONS a";
                    FbDataReader rdr = null;
                    FbCommand cmd = new FbCommand(SQL, db, tran);
                    try
                    {
                        rdr = cmd.ExecuteReader();

                        if (rdr.Read())
                        {
                            replicationVersion = rdr.GetInt32(0);

                            if (replicationVersion == 4)
                                replicationVersion = 400;
                        }
                    }
                    finally
                    {
                        CloseAndDispose(ref cmd, ref rdr);
                    }

                    if (replicationVersion <= 0)
                        return;

                    string repVersion = Assembly.GetCallingAssembly().GetName().Version.ToString();

                    // loca
                    repVersion = repVersion.Substring(0, repVersion.LastIndexOf("."));
                    int internalVersion = Shared.Utilities.StrToInt(repVersion.Replace(".", ""), 0);

                    while (internalVersion > replicationVersion)
                    {
                        string contents = UpdateBackupReplication.GetInternalVersion(replicationVersion, master);
                        string localTempFile = String.Empty;

                        if (String.IsNullOrEmpty(contents))
                        {
                            replicationVersion++;
                        }
                        else
                        {
                            localTempFile = System.IO.Path.GetTempFileName();
                            try
                            {
                                Shared.Utilities.FileWrite(localTempFile, contents);
                            }
                            catch (Exception err)
                            {
                                Shared.EventLog.Add(err, localTempFile);
                            }

                            #region Update Database

                            FbScript script = new FbScript(Shared.Utilities.FileRead(localTempFile, false));
                            script.Parse();

                            FbConnection updateDB = new FbConnection(db.ConnectionString);
                            try
                            {
                                foreach (FbStatement statement in script.Results)
                                {
                                    try
                                    {
                                        if (statement.Text == "COMMIT" || statement.Text == "ROLLBACK")
                                            continue;

                                        FbBatchExecution fbe = new FbBatchExecution(updateDB);
                                        try
                                        {
                                            fbe.Statements.Add(statement);
                                            fbe.Execute();
                                            fbe.Statements.Clear();
                                        }
                                        finally
                                        {
                                            fbe = null;
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        if ((!err.Message.Contains("unsuccessful metadata update") &&
                                            !err.Message.Contains("does not exist")) &&
                                            !err.Message.ToUpper().Contains("ATTEMPT TO STORE DUPLICATE VALUE") &&
                                            !err.Message.ToUpper().Contains("ALREADY EXISTS") &&
                                            !err.Message.Contains("violation of PRIMARY or UNIQUE") &&
                                            !err.Message.Contains("violation of FOREIGN KEY constraint") &&
                                            !err.Message.Contains("GRANT USAGE ON "))
                                        {
                                            Shared.EventLog.Add(err, statement.Text);
                                            throw;
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                updateDB.Close();
                                updateDB.Dispose();
                                updateDB = null;
                            }

                            replicationVersion++;

                            #endregion Update Database
                        }
                    }

                    SQL = String.Format("UPDATE REPLICATE$OPTIONS a SET a.REPLICATION_VERSION = {0};", replicationVersion);
                    cmd = new FbCommand(SQL, db, tran);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        CloseAndDispose(ref cmd);
                    }
                }
                finally
                {
                    tran.Commit();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }
        }

        #endregion Child Database

        #region Master Database

        public void RemoveMasterReplicatedTable(DatabaseConnection connection,
            string tableName, bool insert, bool update, bool delete)
        {
            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.MasterDatabase, ref tran, true);
            try
            {
                try
                {
                    string SQL = "DELETE FROM REPLICATE$TABLES a WHERE a.TABLE_NAME = '{0}' AND a.OPERATION = '{1}';";
                    FbCommand cmd;

                    if (insert)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "INSERT"), db, tran);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd);
                        }
                    }

                    if (update)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "UPDATE"), db, tran);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd);
                        }
                    }

                    if (delete)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "DELETE"), db, tran);
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
                    tran.Commit();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }
        }

        public List<string> GetMasterReplicatedTables(DatabaseConnection connection)
        {
            List<string> Result = new List<string>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.MasterDatabase, ref tran, true);
            try
            {
                string SQL = "SELECT DISTINCT TRIM(a.TABLE_NAME) FROM REPLICATE$TABLES a ORDER BY a.TABLE_NAME";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(rdr.GetString(0));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public List<string> GetMasterTables(DatabaseConnection connection)
        {
            List<string> Result = new List<string>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.MasterDatabase, ref tran, true);
            try
            {
                string SQL = "select TRIM(rdb$relation_name)\nfrom rdb$relations\nwhere rdb$view_blr is null\n" +
                    "and (rdb$system_flag is null or rdb$system_flag = 0)\n and (rdb$relation_name NOT LIKE 'REPLICATE$%')\n" +
                    "ORDER BY rdb$relation_name";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(rdr.GetString(0));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public void MasterAddTable(DatabaseConnection connection, string tableName)
        {
            string SQL = "INSERT INTO REPLICATE$TABLES (TABLE_NAME, OPERATION, TRIGGER_NAME, SORT_ORDER, " +
                "EXCLUDE_FIELDS, LOCAL_GENERATOR, REMOTE_GENERATOR, LOCAL_ID_COLUMN, INDICE_TYPE, OPTIONS, ID) " +
                " VALUES ('{0}', '{1}', 'TRIGGER_NAME', 0, '', '', '', '', 0, 0, GEN_ID(REPLICATE$REPLICATETABLES_ID, 1))";

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.MasterDatabase, ref tran, true);
            try
            {
                try
                {
                    FbCommand cmd = new FbCommand(String.Format(SQL, tableName, "INSERT"), db, tran);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        CloseAndDispose(ref cmd);
                    }

                    cmd = new FbCommand(String.Format(SQL, tableName, "DELETE"), db, tran);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        CloseAndDispose(ref cmd);
                    }

                    cmd = new FbCommand(String.Format(SQL, tableName, "UPDATE"), db, tran);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        CloseAndDispose(ref cmd);
                    }
                }
                finally
                {
                    tran.Commit();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }
        }

        public List<string> GetMasterGenerators(DatabaseConnection connection)
        {
            List<string> Result = new List<string>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.MasterDatabase, ref tran, true);
            try
            {
                string SQL = "SELECT TRIM(a.RDB$GENERATOR_NAME) FROM RDB$GENERATORS a " +
                    "WHERE a.RDB$SYSTEM_FLAG IS NULL or a.RDB$SYSTEM_FLAG = 0 " +
                    "ORDER BY a.RDB$GENERATOR_NAME";
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(rdr.GetString(0));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public List<string> GetMasterTableColumns(DatabaseConnection connection, string tableName)
        {
            List<string> Result = new List<string>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.MasterDatabase, ref tran, true);
            try
            {
                string SQL = String.Format("SELECT TRIM(a.RDB$FIELD_NAME) FROM RDB$RELATION_FIELDS a " +
                    "WHERE a.RDB$RELATION_NAME = '{0}' AND a.RDB$FIELD_NAME <> 'REPLICATE$HASH' " +
                    "ORDER BY a.RDB$FIELD_POSITION", tableName);
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Result.Add(rdr.GetString(0));
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public List<ReplicatedTable> GetMasterTableReplicatedTable(DatabaseConnection connection, string tableName)
        {
            List<ReplicatedTable> Result = new List<ReplicatedTable>();

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.MasterDatabase, ref tran, true);
            try
            {
                string SQL = String.Format("SELECT a.ID, a.TABLE_NAME, a.OPERATION, a.TRIGGER_NAME, " +
                    "a.SORT_ORDER, a.EXCLUDE_FIELDS, a.LOCAL_GENERATOR, a.REMOTE_GENERATOR, " +
                    "a.LOCAL_ID_COLUMN, a.INDICE_TYPE, a.OPTIONS FROM REPLICATE$TABLES a " +
                    "WHERE a.TABLE_NAME = '{0}'", tableName);
                FbDataReader rdr = null;
                FbCommand cmd = new FbCommand(SQL, db, tran);
                try
                {
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        Result.Add(new ReplicatedTable(rdr.GetInt64(0), rdr.GetString(1), (Operation)rdr.GetInt32(2),
                            rdr.GetString(3), rdr.GetInt32(4), rdr.GetString(5), rdr.GetString(6), rdr.GetString(7),
                            rdr.GetString(8), rdr.GetInt32(9), (TableOptions)rdr.GetInt64(10)));
                    }
                }
                finally
                {
                    tran.Rollback();
                    CloseAndDispose(ref cmd, ref rdr);
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }

            return (Result);
        }

        public void MasterUpdateReplicatedTables(DatabaseConnection connection, string tableName, string localID,
            bool insert, bool update, bool delete, string triggerName, string excludeFields, int sortOrder,
            int indiceType, string localGenerator, string remoteGenerator, TableOptions options)
        {
            RemoveMasterReplicatedTable(connection, tableName, true, true, true);

            string SQL = "INSERT INTO REPLICATE$TABLES (TABLE_NAME, OPERATION, TRIGGER_NAME, SORT_ORDER, " +
                "EXCLUDE_FIELDS, LOCAL_GENERATOR, REMOTE_GENERATOR, LOCAL_ID_COLUMN, INDICE_TYPE, OPTIONS, ID) " +
                " VALUES ('{0}', '{1}', '{2}', {3}, '{4}', '{5}', '{6}', '{7}', {8}, {9}, " +
                "GEN_ID(REPLICATE$REPLICATETABLES_ID, 1))";

            FbTransaction tran = null;
            FbConnection db = ConnectToDatabase(connection.MasterDatabase, ref tran, true);
            try
            {
                try
                {
                    FbCommand cmd;

                    if (insert)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "INSERT",
                            triggerName, sortOrder, excludeFields, String.Empty, String.Empty,
                            localID, indiceType, (Int64)options), db, tran);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd);
                        }
                    }

                    if (delete)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "DELETE",
                            triggerName, sortOrder, excludeFields, String.Empty, String.Empty,
                            localID, indiceType, (Int64)options), db, tran);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        finally
                        {
                            CloseAndDispose(ref cmd);
                        }
                    }

                    if (update)
                    {
                        cmd = new FbCommand(String.Format(SQL, tableName, "UPDATE",
                            triggerName, sortOrder, excludeFields, String.Empty, String.Empty,
                            localID, indiceType, (Int64)options), db, tran);
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
                    tran.Commit();
                }
            }
            finally
            {
                CloseAndDispose(ref db, ref tran);
            }
        }

        #endregion Master Database

        #endregion Public Methods

        #region Global Procs

        private void AddParam(FbCommand cmd, string parameterName, FbDbType parameterType,
            object parameterValue)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");

            if (String.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException("parameterName");

            if (!cmd.Parameters.Contains(parameterName))
                cmd.Parameters.Add(parameterName, parameterType);

            cmd.Parameters[parameterName].Direction = ParameterDirection.Input;
            cmd.Parameters[parameterName].Value = parameterValue;
        }

        private void CloseAndDispose(ref FbCommand cmd)
        {
            if (cmd == null)
                return;

            cmd.Dispose();
            cmd = null;
        }

        private void CloseAndDispose(ref FbCommand cmd, ref FbDataReader rdr)
        {
            if (rdr != null)
            {
                rdr.Close();
                rdr.Dispose();
                rdr = null;
            }

            if (cmd == null)
                return;

            cmd.Dispose();
            cmd = null;
        }

        private void CloseAndDispose(ref FbConnection db, ref FbTransaction tran)
        {
            if (tran != null)
            {
                tran.Dispose();
                tran = null;
            }

            if (db != null)
            {
                db.Close();

                FbConnectionStringBuilder csb = new FbConnectionStringBuilder(db.ConnectionString);

                db.Dispose();
                db = null;
            }
        }

        private const int MaxReconnectAttempts = 10;

        private FbConnection ConnectToDatabase(string connectionString, ref FbTransaction tran,
            bool allowPool, System.Data.IsolationLevel isolationLevel = IsolationLevel.Snapshot, 
            int attempt = 0)
        {
            FbConnection Result = null;

            try
            {
                FbConnectionStringBuilder connString = new FbConnectionStringBuilder(connectionString);
                try
                {
                    connString.NoGarbageCollect = true;
                    connString.Pooling = allowPool;
                    connString.MaxPoolSize = allowPool ? 10 : 1;
                    connString.ConnectionLifeTime = 60;

                    Result = new FbConnection(connString.ToString());

                    Result.Open();

                    tran = Result.BeginTransaction(isolationLevel);

                    return (Result);
                }
                finally
                {
                    connString = null;
                }
            }
            catch (Exception err)
            {
                if (err.Message.Contains("Error reading data from the connection") ||
                    err.Message.Contains("connection shutdown") ||
                    err.Message.Contains("Unable to complete network request"))
                {
                    FbConnection.ClearPool(Result);

                    if (attempt < MaxReconnectAttempts)
                    {
                        return (ConnectToDatabase(connectionString, ref tran, allowPool, isolationLevel, ++attempt));
                    }
                    else
                        throw;
                }
                else
                    throw;
            }

        }

        #endregion Global Procs
    }
}
