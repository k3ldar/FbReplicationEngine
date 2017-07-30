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
 *  Purpose:  Remote Database Updates
 *
 */
using System;
using System.IO;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;

using Shared;

namespace Replication.Engine
{
    public sealed class DatabaseRemoteUpdate
    {
        #region Properties

        private string ConnectionString { get; set; }

        private string RemoteUpdateFile { get; set; }

        private string RemoteUpdateLocation { get; set; }

        #endregion Properties

        #region Database Updates

        /// <summary>
        /// Executes a script on the database
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="script"></param>
        /// <returns></returns>
        public bool DatabaseRunScript(string connectionString, string script)
        {
            string fileName = Path.GetTempFileName();
            try
            {
                Shared.Utilities.FileWrite(fileName, script);
                bool tableUpdated = false;
                return (UpdateDatabase(connectionString, fileName, -1, ref tableUpdated));
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        /// <summary>
        /// Determines wether a new database version is available
        /// </summary>
        /// <param name="NewVersion">Latest available version</param>
        /// <returns>true if new version available, otherwise flse</returns>
        private bool NewDatabaseVersionAvailable(ref int NewVersion, int currentVersion)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            bool Result = false;

            try
            {
                int VerRemote = Shared.Utilities.StrToInt(Shared.XML.GetXMLValue(RemoteUpdateFile, "Database", "Version"), 0);

                if (VerRemote > currentVersion)
                    RaiseOnMessage(String.Format("Local Version: {0}", currentVersion));

                if (currentVersion < VerRemote)
                {
                    NewVersion = currentVersion + 1;

                    RaiseOnMessage(String.Format("Remote Version: {0}", NewVersion));

                    if (NewVersion > Convert.ToInt32(VerRemote))
                        return (false);
                    else
                        return (true);
                }

            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
            }

            NewVersion = 0;
            return (Result);
        }

        #endregion Database Updates

        /// <summary>
        /// Checks for database updates and updates the database if any new updates are found
        /// </summary>
        public bool CheckForDatabaseUpdates(string name, string connectionString, 
            string remoteUpdateFile, string remoteUpdateLocation, ref int currentVersion,
            ref bool tableUpdated)
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            if (String.IsNullOrEmpty(remoteUpdateFile) || String.IsNullOrEmpty(remoteUpdateLocation))
                throw new ArgumentNullException(String.Format("Database Remote Update is not configured correctly for {0}", name));

            ConnectionString = connectionString;
            RemoteUpdateFile = remoteUpdateFile;
            RemoteUpdateLocation = remoteUpdateLocation;

            int NewVersion = 0;
            bool Result = false;

            try
            {
                bool tableChanged = false;
                while (NewDatabaseVersionAvailable(ref NewVersion, currentVersion))
                {
                    RaiseOnMessage(String.Format("Database Version {0} is now available", NewVersion));
                    string newFile = Path.GetTempFileName();

                    string downloadFile = String.Format("{0}{1}.txt", remoteUpdateLocation, NewVersion);

                    Shared.FileDownload.Download(downloadFile, newFile);

                    if (File.Exists(newFile))
                    {
                        try
                        {
                            RaiseOnMessage(String.Format("Updating Database to version {0}", NewVersion));

                            Result = UpdateDatabase(ConnectionString, newFile, NewVersion, ref tableChanged);

                            if (tableChanged)
                                tableUpdated = true;

                            if (!Result)
                            {
                                //failed
                                throw new Exception("Database update failed");
                            }

                            currentVersion++;
                        }
                        finally
                        {
                            File.Delete(newFile);
                        }

                        Result = true;
                    }

                    if (Result)
                        RaiseOnMessage(String.Format("Database Update Complete, Version {0}", NewVersion));
                    else
                        RaiseOnMessage(String.Format("Failed to complete database update, Version {0}", NewVersion));
                }

                if (File.Exists(String.Format("{0}\\adhoc.sql", Shared.Utilities.CurrentPath())))
                {
                    RaiseOnMessage("Found ADHOC SQL");
                    UpdateDatabase("", String.Format("{0}\\adhoc.sql", Shared.Utilities.CurrentPath()), -1, ref tableUpdated);

                    File.Delete(String.Format("{0}\\adhoc.sql", Shared.Utilities.CurrentPath()));
                    Result = true;
                }
            }
            catch (Exception err)
            {
                RaiseOnMessage(err.Message);

                if (err.Message.Contains("Database update failed"))
                    throw;
            }

            return (Result);
        }

        #region Update Database

        /// <summary>
        /// Executes SQL Update Scripts
        /// </summary>
        /// <param name="SQLFile">Script File to process</param>
        /// <returns>true if succesful, otherwise false</returns>
        public bool UpdateDatabase(string connectionString, string sqlFile, int newVersion, ref bool tableUpdated)
        {
            bool Result = false;

            if (tableUpdated)
                tableUpdated = true;
            else
                tableUpdated = false;

            FbConnectionStringBuilder cb = new FbConnectionStringBuilder(connectionString);
            cb.Pooling = false;

            //connect to local DB
            FbConnection database = new FbConnection(cb.ToString());
            try
            {
                database.Open();
                FbScript script = new FbScript(Shared.Utilities.FileRead(sqlFile, false));
                script.Parse();
                int idx = 0;

                foreach (FbStatement cmd in script.Results)
                {
                    try
                    {
                        if (cmd.Text == "COMMIT" || cmd.Text == "ROLLBACK")
                            continue;

                        if (!tableUpdated && 
                            (
                                cmd.Text.ToUpper().Contains("ALTER TABLE") || // any table changes
                                cmd.Text.ToUpper().Contains("CREATE TABLE") || // any new tables
                                cmd.Text.ToUpper().Contains("REPLICATE$TABLES") // anything relating to replicate tables
                            )
                            )
                        {
                            tableUpdated = true;
                        }

                        if (OnUpdateDatabase != null)
                        {
                            OnUpdateDatabase(null, new FileProgressArgs(String.Empty, script.Results.Count, idx));
                            idx++;
                        }

                        FbBatchExecution fbe = new FbBatchExecution(database);
                        try
                        {
                            fbe.Statements.Add(cmd);
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
                            Shared.EventLog.Add(err, cmd.Text);
                            throw;
                        }
                    }
                }

                if (newVersion > 0)
                {
                    //update the version
                    string SQL = String.Format("UPDATE REPLICATE$OPTIONS SET DATABASE_VERSION = {0}", newVersion);
                    FbTransaction tran = database.BeginTransaction();
                    try
                    {
                        FbCommand cmdUpdate = new FbCommand(SQL, database, tran);
                        try
                        {
                            cmdUpdate.ExecuteNonQuery();
                        }
                        finally
                        {
                            cmdUpdate.Dispose();
                            cmdUpdate = null;
                        }
                    }
                    finally
                    {
                        tran.Commit();
                        tran.Dispose();
                    }
                }

                Result = true;
            }
            catch (Exception e)
            {
                Shared.EventLog.Add(e);
                Result = false;
            }
            finally
            {
                database.Close();
                database.Dispose();
                database = null;
            }

            return (Result);
        }

        #endregion Update Database

        #region Event Wrappers

        private void RaiseOnMessage(string message)
        {
            if (OnNewMessage != null)
            {
                OnNewMessage(this, new AddToLogFileArgs(message));
            }
            else
            {
                Shared.EventLog.Add(message);
            }
        }

        #endregion Event Wrappers

        #region Events

        public event AddToLogFileHandler OnNewMessage;

        public event FileProgressHandler OnUpdateDatabase;

        #endregion Events
    }
}
