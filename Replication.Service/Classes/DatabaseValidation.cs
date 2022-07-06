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
 *  Purpose:  Database Validation
 *
 */
using System;
using System.Collections;

using FirebirdSql.Data.FirebirdClient;

using Shared;

namespace Replication.Service
{
    public static class DatabaseValidation
    {
        #region Private Static Members

        private static int TOTAL_STEPS = 39;

        private static int _currentStage = 1;

        private static DatabaseObjects _databaseObjectsMaster = new DatabaseObjects();
        private static DatabaseObjects _databaseObjectsChild = new DatabaseObjects();

        private static DatabaseObjects _dependenciesMaster = new DatabaseObjects();

        private static FbConnection _childDB = new FbConnection();
        private static FbConnection _masterDB = new FbConnection();

        #endregion Private Static Members

        #region Public Static Methods

        /// <summary>
        /// Compares two database looking for differences
        /// </summary>
        /// <param name="masterDatabase"></param>
        /// <param name="childDatabase"></param>
        public static DatabaseObjects Validate(
            string masterDatabaseConnection, 
            string childDatabaseConnection,
            bool systemObjects,
            bool tablesOnly)
        {
            LoadSystemObjects = systemObjects;
            CancelAnalysis = false;
            _currentStage = 1;

            _childDB.ConnectionString = childDatabaseConnection;
            _masterDB.ConnectionString = masterDatabaseConnection;

            Initialise();
            try
            {
                if (!tablesOnly)
                {
                    LoadDomains(_databaseObjectsChild, "Loading child domains", GetCurrentStage());
                    LoadDomains(_databaseObjectsMaster, "Loading master domains", GetCurrentStage());

                    LoadRoles(_databaseObjectsChild, "Loading child roles", GetCurrentStage());
                    LoadRoles(_databaseObjectsMaster, "Loading master roles", GetCurrentStage());

                    LoadExceptions(_databaseObjectsChild, "Loading child exceptions", GetCurrentStage());
                    LoadExceptions(_databaseObjectsMaster, "Loading master exceptions", GetCurrentStage());

                    LoadFunctions(_databaseObjectsChild, "Loading child functions", GetCurrentStage());
                    LoadFunctions(_databaseObjectsMaster, "Loading master functions", GetCurrentStage());

                    LoadGenerators(_databaseObjectsChild, "Loading child generators", GetCurrentStage());
                    LoadGenerators(_databaseObjectsMaster, "Loading master generators", GetCurrentStage());
                }

                LoadTables(_databaseObjectsChild, "Loading child tables", GetCurrentStage());
                LoadTables(_databaseObjectsMaster, "Loading master tables", GetCurrentStage());

                if (!tablesOnly)
                {
                    LoadViews(_databaseObjectsChild, "Loading child views", GetCurrentStage());
                    LoadViews(_databaseObjectsMaster, "Loading master views", GetCurrentStage());

                    LoadViewRelations(_databaseObjectsChild, "Loading child view relations", GetCurrentStage());
                    LoadViewRelations(_databaseObjectsMaster, "Loading master view relations", GetCurrentStage());
                }

                LoadColumns(_databaseObjectsChild, "Loading child columns", GetCurrentStage());
                LoadColumns(_databaseObjectsMaster, "Loading master columns", GetCurrentStage());

                LoadPrimaryKeys(_databaseObjectsChild, "Loading child primary keys", GetCurrentStage());
                LoadPrimaryKeys(_databaseObjectsMaster, "Loading master primary keys", GetCurrentStage());

                LoadIndices(_databaseObjectsChild, "Loading child indices", GetCurrentStage());
                LoadIndices(_databaseObjectsMaster, "Loading master indices", GetCurrentStage());

                LoadForeignKeys(_databaseObjectsChild, "Loading child foreign keys", GetCurrentStage());
                LoadForeignKeys(_databaseObjectsMaster, "Loading master foreign keys", GetCurrentStage());

                if (!tablesOnly)
                {
                    LoadProcedures(_databaseObjectsChild, "Loading child procedures", GetCurrentStage());
                    LoadProcedures(_databaseObjectsMaster, "Loading master procedures", GetCurrentStage());

                    LoadParameters(_databaseObjectsChild, "Loading child parameters", GetCurrentStage());
                    LoadParameters(_databaseObjectsMaster, "Loading master parameters", GetCurrentStage());

                    LoadTriggers(_databaseObjectsChild, "Loading child triggers", GetCurrentStage());
                    LoadTriggers(_databaseObjectsMaster, "Loading master triggers", GetCurrentStage());
                }

                LoadDependencies(_dependenciesMaster, "Loading master dependencies", GetCurrentStage());

                return (CompareChildToMaster());
            }
            finally
            {
                Finalise();
            }
        }

        #endregion Public Static Methods

        #region Internal Properties

        /// <summary>
        /// Indicates wether to load system objects as well as user defined
        /// </summary>
        internal static bool LoadSystemObjects { get; set; }

        #endregion Internal Properties

        #region Public Properties

        /// <summary>
        /// Indicates wether the operation should cancel or not
        /// </summary>
        public static bool CancelAnalysis { get; internal set; }

        /// <summary>
        /// Returns a dependencies collection for the master database
        /// </summary>
        public static DatabaseObjects Dependencies
        {
            get
            {
                return (_dependenciesMaster);
            }
        }

        /// <summary>
        /// Returns a collection of objects from the child database
        /// </summary>
        public static DatabaseObjects ChildObjects
        {
            get
            {
                return (_databaseObjectsChild);
            }
        }

        #endregion Public Properties

        #region Private Static Methods

        private static DatabaseObjects CompareChildToMaster()
        {
            DatabaseObjects Result = new DatabaseObjects();

            PreProcess1(_databaseObjectsChild);
            PreProcess2(_databaseObjectsMaster, _databaseObjectsChild);

            int countMissing = 0;
            int i = 0;

            foreach (DatabaseObject objMaster in _databaseObjectsMaster)
            {
                if (CancelAnalysis)
                    return (Result);

                RaiseProcessStatusChanged("Analysing database objects", Utilities.Percentage(_databaseObjectsMaster.Count, i), GetCurrentStage(false));
                objMaster.Status = ObjectStatus.Processing;

                DatabaseObject objChild = null;

                switch (objMaster.ObjectType)
                {
                    case DatabaseObjectType.Table:
                    case DatabaseObjectType.Procedure:
                        objChild = FindObject(objMaster.ObjectName, _databaseObjectsChild, objMaster.ObjectType);

                        break;

                    default:
                        objChild = FindObject(_databaseObjectsChild, objMaster);

                        break;
                }

                if (objChild != null)
                {
                    if (objChild.FurtherChecks)
                    {
                        bool found;
                        
                        switch (objChild.ObjectType)
                        {
                            case DatabaseObjectType.Procedure:
                            case DatabaseObjectType.TableColumn:
                            case DatabaseObjectType.ViewColumn:
                            case DatabaseObjectType.Parameter:
                            case DatabaseObjectType.PrimaryKey:
                            case DatabaseObjectType.Index:
                            case DatabaseObjectType.ForeignKey:
                            case DatabaseObjectType.Trigger:
                            case DatabaseObjectType.Domain:
                                found = objMaster.ObjectType == objChild.ObjectType &&
                                    objMaster.ObjectName == objChild.ObjectName &&
                                    objMaster.ObjectParameter1 == objChild.ObjectParameter1 &&
                                    objMaster.System == objChild.System &&
                                    objMaster.Hash == objChild.Hash;

                                if (found)
                                {
                                    if (objChild.NotNull != objMaster.NotNull ||
                                        objChild.DefaultValue != objMaster.DefaultValue ||
                                        objChild.Size != objMaster.Size ||
                                        (objChild.ObjectParameter2 == "BLOB" && objChild.SubType != objMaster.SubType) ||
                                        objChild.ObjectParameter2 != objMaster.ObjectParameter2 ||
                                        objChild.ObjectParameter3 != objMaster.ObjectParameter3 ||
                                        objChild.ObjectParameter4 != objMaster.ObjectParameter4 ||
                                        objChild.ObjectParameter5 != objMaster.ObjectParameter5)
                                    {
                                        objChild.SetInformation(objMaster);
                                        objChild.Status = ObjectStatus.DifferentSettings;

                                        countMissing++;
                                        Result.Add(objChild);
                                    }
                                }
                                else
                                {
                                    // try finding based on less settings
                                    found = objMaster.ObjectType == objChild.ObjectType &&
                                        objMaster.ObjectName == objChild.ObjectName &&
                                        objMaster.ObjectParameter1 == objChild.ObjectParameter1;

                                    if (found)
                                    {
                                        if (objChild.NotNull != objMaster.NotNull ||
                                            objChild.DefaultValue != objMaster.DefaultValue ||
                                            objChild.Size != objMaster.Size ||
                                            (objChild.ObjectParameter2 == "BLOB" && objChild.SubType != objMaster.SubType) ||
                                            objChild.ObjectParameter2 != objMaster.ObjectParameter2 ||
                                            objChild.ObjectParameter3 != objMaster.ObjectParameter3 ||
                                            objChild.ObjectParameter4 != objMaster.ObjectParameter4 ||
                                            objChild.ObjectParameter5 != objMaster.ObjectParameter5)
                                        {
                                            objChild.SetInformation(objMaster);
                                            objChild.Status = ObjectStatus.DifferentSettings;

                                            countMissing++;
                                            Result.Add(objChild);
                                        }
                                    }
                                }

                                break;

                            default:
                                found = objMaster.ObjectType == objChild.ObjectType &&
                                    objMaster.ObjectName == objChild.ObjectName &&
                                    objMaster.ObjectParameter1 == objChild.ObjectParameter1 &&
                                    objMaster.ObjectParameter2 == objChild.ObjectParameter2 &&
                                    objMaster.ObjectParameter3 == objChild.ObjectParameter3 &&
                                    objMaster.ObjectParameter4 == objChild.ObjectParameter4 &&
                                    objMaster.ObjectParameter5 == objChild.ObjectParameter5 &&
                                    objMaster.System == objChild.System &&
                                    objMaster.Hash == objChild.Hash;

                                if (!found)
                                {
                                    countMissing++;
                                    //Result.Add()
                                }

                                break;
                        }

                        if (!found)
                        {
                            string s = objMaster.ObjectParameter1;
                            s += " " + objMaster.ObjectName;
                        }
                        else
                        {
                            objMaster.Status = ObjectStatus.Found;
                        }
                    }
                    else
                    {
                        int j = 0;
                        j++;
                    }
                }
                else
                {
                    if (objMaster.FurtherChecks)
                    {
                        objMaster.Status = ObjectStatus.MissingFromChild;
                        objMaster.Information = "Missing from Child Database\r\n";
                        objMaster.FurtherChecks = false;
                        objMaster.SetInformation(objMaster);

                        //missing
                        countMissing++;
                        Result.Add(objMaster);
                    }
                }

                i++;
            }

            PostProcess1(Result, _databaseObjectsChild);
            PostProcess2(Result, _databaseObjectsChild);
            Result = PostProcess3(Result, _databaseObjectsMaster, _databaseObjectsChild);

            PostProcess5(Result);

            PostProcess4(Result, _databaseObjectsChild);

            return (Result);
        }

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

        /// <summary>
        /// Ensures any related objects are not checked if the parent object is not found
        /// </summary>
        /// <param name="objectsPrimary">Master Database Objects</param>
        /// <param name="objectsSecondary">Child Database Objects</param>
        private static void PreProcess2(DatabaseObjects objectsPrimary, DatabaseObjects objectsSecondary)
        {
            int i = 0;

            foreach (DatabaseObject dbObject in objectsPrimary)
            {
                if (CancelAnalysis)
                    return;

                RaiseProcessStatusChanged("Validating Parent Objects", Shared.Utilities.Percentage(objectsPrimary.Count, i), GetCurrentStage(false));

                switch (dbObject.ObjectType)
                {
                    case DatabaseObjectType.PrimaryKey:
                    case DatabaseObjectType.ForeignKey:
                    case DatabaseObjectType.Index:
                    case DatabaseObjectType.TableColumn:

                        dbObject.FurtherChecks = FindObject(dbObject.ObjectName, objectsSecondary, DatabaseObjectType.Table) != null;

                        break;

                    case DatabaseObjectType.ViewColumn:

                        dbObject.FurtherChecks = FindObject(dbObject.ObjectName, objectsSecondary, DatabaseObjectType.View) != null;

                        break;

                    case DatabaseObjectType.Parameter:

                        dbObject.FurtherChecks = FindObject(dbObject.ObjectName, objectsSecondary, DatabaseObjectType.Procedure) != null;

                        break;
                }

                i++;
            }

        }

        #endregion Pre Process

        #region Post Process

        /// <summary>
        /// Checks to see if an object is not found, if it isn't found checks to see if it has a different name than the master record with the same type
        /// 
        /// Only looks at Primary Keys, Foreign Keys and Index's
        /// </summary>
        private static void PostProcess1(DatabaseObjects objectsPrimary, DatabaseObjects objectsSecondary)
        {
            int i = 0;

            foreach (DatabaseObject dbObject in objectsPrimary)
            {
                if (CancelAnalysis)
                    return;

                RaiseProcessStatusChanged("Checking Name Changes", Shared.Utilities.Percentage(objectsPrimary.Count, i), GetCurrentStage(false));

                switch (dbObject.ObjectType)
                {
                    case DatabaseObjectType.PrimaryKey:
                    case DatabaseObjectType.ForeignKey:
                    case DatabaseObjectType.Index:

                        DatabaseObject dbObjectFound = FindObject(objectsSecondary, dbObject, false);

                        if (dbObjectFound != null)
                        {
                            dbObject.ExistsWithDifferentName = true;
                            dbObject.DifferentName = dbObjectFound.ObjectParameter1;

                            dbObject.Information += String.Format("\r\nAnother {0} ({1}) was found with similar properties in the child database",
                                dbObjectFound.ObjectType.ToString(), dbObjectFound.ObjectParameter1);
                        }

                        break;
                }

                i++;
            }
        }

        /// <summary>
        /// Scans the list of missing objects, checks to see if the parent object exists or not
        /// 
        /// Only looks at parameters and columns
        /// </summary>
        /// <param name="objectsPrimary">List of missing objects</param>
        /// <param name="objectsSecondary">List of objects from child database</param>
        private static void PostProcess2(DatabaseObjects objectsPrimary, DatabaseObjects objectsSecondary)
        {
            int i = 0;

            foreach (DatabaseObject dbObject in objectsPrimary)
            {
                if (CancelAnalysis)
                    return;

                RaiseProcessStatusChanged("Validating Results", Shared.Utilities.Percentage(objectsPrimary.Count, i), GetCurrentStage(false));

                DatabaseObject dbObjectFound = null;

                switch (dbObject.ObjectType)
                {
                    case DatabaseObjectType.Parameter:

                        dbObjectFound = FindObject(dbObject.ObjectName, objectsSecondary, DatabaseObjectType.Procedure);

                        if (dbObjectFound == null)
                            dbObject.Status = ObjectStatus.ParentObjectDoesNotExist;

                        break;

                    case DatabaseObjectType.ViewColumn:

                        dbObjectFound = FindObject(dbObject.ObjectName, objectsSecondary, DatabaseObjectType.View);

                        if (dbObjectFound == null)
                            dbObject.Status = ObjectStatus.ParentObjectDoesNotExist;

                        break;

                    case DatabaseObjectType.TableColumn:

                        dbObjectFound = FindObject(dbObject.ObjectName, objectsSecondary, DatabaseObjectType.Table);

                        if (dbObjectFound == null)
                            dbObject.Status = ObjectStatus.ParentObjectDoesNotExist;

                        break;

                }

                i++;
            }
        }

        /// <summary>
        /// Scans the list of missing objects and removes objects that do not belong in there
        /// </summary>
        /// <param name="objectsProblemList"></param>
        /// <param name="objectsPrimary"></param>
        /// <param name="objectsSecondary"></param>
        /// <returns></returns>
        private static DatabaseObjects PostProcess3(DatabaseObjects objectsProblemList, DatabaseObjects objectsPrimary, DatabaseObjects objectsSecondary)
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

        /// <summary>
        /// Generate SQL for objects
        /// </summary>
        /// <param name="objectsPrimary"></param>
        /// <param name="objectsSecondary"></param>
        private static void PostProcess4(DatabaseObjects objectsPrimary, DatabaseObjects objectsSecondary)
        {
            int i = 0;

            foreach (DatabaseObject dbObject in objectsPrimary)
            {
                if (CancelAnalysis)
                    return;

                RaiseProcessStatusChanged("Generating SQL", Shared.Utilities.Percentage(objectsPrimary.Count, i), GetCurrentStage(false));

                if (dbObject.DependencyMissing)
                {
                    dbObject.Information += "\r\n\r\nDependency is missing from child database, SQL not generated\r\n";
                }
                else
                {
                    GenerateMissingSQL(dbObject, objectsSecondary);
                }

                i++;
            }
        }

        /// <summary>
        /// Determine if sql would fail due to missing dependencies
        /// </summary>
        /// <param name="objects"></param>
        private static void PostProcess5(DatabaseObjects objectsPrimary)
        {
            int i = 0;

            foreach (DatabaseObject dbObject in objectsPrimary)
            {
                if (CancelAnalysis)
                    return;

                RaiseProcessStatusChanged("Validating Dependencies", Shared.Utilities.Percentage(objectsPrimary.Count, i), GetCurrentStage(false));

                dbObject.DependencyMissing = FindDependency(dbObject.ObjectName, objectsPrimary, dbObject.ObjectType);

                i++;
            }
        }

        #endregion Post Process

        #region Find Objects

        /// <summary>
        /// Finds an object of the same type
        /// </summary>
        /// <param name="objectList"></param>
        /// <param name="objectMaster"></param>
        /// <returns></returns>
        private static DatabaseObject FindObject(DatabaseObjects objectList, DatabaseObject objectMaster, bool matchName = true)
        {
            DatabaseObject Result = null;

            foreach (DatabaseObject dbObject in objectList)
            {
                bool found;
                
                if (matchName)
                {
                    found = objectMaster.ObjectParameter1 == dbObject.ObjectParameter1 &&
                        objectMaster.ObjectType == dbObject.ObjectType &&
                        objectMaster.ObjectName == dbObject.ObjectName &&
                        objectMaster.ObjectParameter2 == dbObject.ObjectParameter2 &&
                        objectMaster.ObjectParameter3 == dbObject.ObjectParameter3 &&
                        objectMaster.ObjectParameter4 == dbObject.ObjectParameter4 &&
                        objectMaster.ObjectParameter5 == dbObject.ObjectParameter5 &&
                        objectMaster.System == dbObject.System &&
                        objectMaster.Hash == dbObject.Hash;
                }
                else
                {
                    found = objectMaster.ObjectType == dbObject.ObjectType &&
                        objectMaster.ObjectName == dbObject.ObjectName &&
                        objectMaster.ObjectParameter2 == dbObject.ObjectParameter2 &&
                        objectMaster.ObjectParameter3 == dbObject.ObjectParameter3 &&
                        objectMaster.ObjectParameter4 == dbObject.ObjectParameter4 &&
                        objectMaster.ObjectParameter5 == dbObject.ObjectParameter5 &&
                        objectMaster.System == dbObject.System;
                }

                // one last try based on name, type and param1
                if (!found)
                {
                    found = objectMaster.ObjectParameter1 == dbObject.ObjectParameter1 &&
                        objectMaster.ObjectType == dbObject.ObjectType &&
                        objectMaster.ObjectName == dbObject.ObjectName;
                }

                if (found)
                {
                    Result = dbObject;
                    break;
                }
            }

            return (Result);
        }

        private static DatabaseObject FindObject(string objectName, DatabaseObjects objectList, DatabaseObjectType objectType)
        {
            DatabaseObject Result = null;

            foreach (DatabaseObject dbObject in objectList)
            {
                if (dbObject.ObjectType == objectType &&
                    dbObject.ObjectName == objectName)
                {
                    Result = dbObject;
                    break;
                }
            }

            return (Result);
        }

        private static DatabaseObject FindObject(string objectName, string objectParam1, DatabaseObjects objectList, 
            DatabaseObjectType objectType)
        {
            DatabaseObject Result = null;

            foreach (DatabaseObject dbObject in objectList)
            {
                if (dbObject.ObjectType == objectType &&
                    dbObject.ObjectName == objectName &&
                    dbObject.ObjectParameter1 == objectParam1)
                {
                    Result = dbObject;
                    break;
                }
            }

            return (Result);
        }

        private static bool FindDependency(string name, DatabaseObjects objects, DatabaseObjectType objectType)
        {
            bool Result = false;

            DatabaseObjects masterDependencies = GetDependencies(name);
            DatabaseObjectType type;

            // check each dependency to ensure it exists within the child database, or will be created during this process
            foreach(DatabaseObject dbObject in masterDependencies)
            {
                type = (DatabaseObjectType)Enum.Parse(typeof(DatabaseObjectType), dbObject.ObjectParameter4);
                
                if (FindObject(dbObject.ObjectParameter1, _databaseObjectsChild, type) == null)
                {
                    //dependency not found in child, is it being created now?
                    if (FindObject(dbObject.ObjectParameter1, objects, type) == null)
                    {
                        Result = true;
                        break;
                    }
                }
            }

            return (Result);
        }

        /// <summary>
        /// Gets all dependencies for the object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static DatabaseObjects GetDependencies(string name)
        {
            DatabaseObjects Result = new DatabaseObjects();

            foreach (DatabaseObject dbObject in _dependenciesMaster)
            {
                switch (dbObject.ObjectName.Substring(0, 4))
                { 
                    case "RDB$":
                    case "MON$":
                    case "SEC$":
                        break;

                    default:
                        if (dbObject.ObjectName == name)
                            Result.Add(dbObject);

                        break;
                }
            }

            return (Result);
        }

        #endregion Find Objects

        #region Load System Objects

        private static void LoadDependencies(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$DEPENDENCIES", "", false);
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT a.RDB$DEPENDENT_NAME, a.RDB$DEPENDED_ON_NAME, COALESCE(a.RDB$FIELD_NAME, '') AS FIELD_NAME, " +
                "CASE WHEN a.RDB$DEPENDENT_TYPE = 0 THEN 'Table'  WHEN a.RDB$DEPENDENT_TYPE = 1 THEN 'View' " +
                "WHEN a.RDB$DEPENDENT_TYPE = 2 THEN 'Trigger' WHEN a.RDB$DEPENDENT_TYPE = 3 THEN 'Computed' " +
                "WHEN a.RDB$DEPENDENT_TYPE = 4 THEN 'Validation' WHEN a.RDB$DEPENDENT_TYPE = 5 THEN 'Procedure' " +
                "WHEN a.RDB$DEPENDENT_TYPE = 6 THEN 'expression_r_INDEX' WHEN a.RDB$DEPENDENT_TYPE = 7 THEN 'Exception' " +
                "WHEN a.RDB$DEPENDENT_TYPE = 8 THEN 'User' WHEN a.RDB$DEPENDENT_TYPE = 9 THEN 'TableColumn' " +
                "WHEN a.RDB$DEPENDENT_TYPE = 10 THEN 'Index' WHEN a.RDB$DEPENDENT_TYPE = 14 THEN 'Generator' " +
                "ELSE 'UNKNOWN' END AS DEPENDENT_TYPE , CASE WHEN a.RDB$DEPENDED_ON_TYPE = 0 THEN 'Table' " +
                "WHEN a.RDB$DEPENDED_ON_TYPE = 1 THEN 'View' WHEN a.RDB$DEPENDED_ON_TYPE = 2 THEN 'Trigger' " +
                "WHEN a.RDB$DEPENDED_ON_TYPE = 3 THEN 'Computed' WHEN a.RDB$DEPENDED_ON_TYPE = 4 THEN 'Validation' " +
                "WHEN a.RDB$DEPENDED_ON_TYPE = 5 THEN 'Procedure' WHEN a.RDB$DEPENDED_ON_TYPE = 6 THEN 'expression_r_INDEX' " +
                "WHEN a.RDB$DEPENDED_ON_TYPE = 7 THEN 'Exception' WHEN a.RDB$DEPENDED_ON_TYPE = 8 THEN 'User' " +
                "WHEN a.RDB$DEPENDED_ON_TYPE = 9 THEN CASE WHEN a.RDB$DEPENDENT_TYPE = 5 THEN 'Domain' ELSE 'TableColumn' END WHEN a.RDB$DEPENDED_ON_TYPE = 10 THEN 'Index' " +
                "WHEN a.RDB$DEPENDED_ON_TYPE = 14 THEN 'Generator' ELSE 'UNKNOWN' END AS DEPEND_ON_TYPE " +
                "FROM RDB$DEPENDENCIES a where a.RDB$DEPENDED_ON_TYPE IN (0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 14) " +
                "AND (a.RDB$DEPENDED_ON_NAME NOT LIKE 'MON$%') " +
                "ORDER BY  a.RDB$DEPENDED_ON_NAME, a.RDB$FIELD_NAME ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        int i = 0;

                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            objects.Add(new DatabaseObject(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2),
                                rdr.GetString(3), rdr.GetString(4),
                                String.Empty, DatabaseObjectType.Dependency, false, true, 0));

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadTables(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$RELATION_FIELDS", " WHERE RDB$VIEW_CONTEXT IS NULL ");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT DISTINCT TRIM(RDB$RELATION_NAME), HASH(TRIM(RDB$RELATION_NAME)), COALESCE(RDB$SYSTEM_FLAG, 0) FROM RDB$RELATION_FIELDS ";

            if (!LoadSystemObjects)
                SQL += "WHERE COALESCE(RDB$SYSTEM_FLAG, 0) = 0 AND RDB$VIEW_CONTEXT IS NULL;";
            else
                SQL += "WHERE RDB$VIEW_CONTEXT IS NULL;";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
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
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadColumns(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$RELATION_FIELDS");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT r.RDB$RELATION_NAME, r.RDB$FIELD_NAME, REPLACE(COALESCE(r.RDB$DEFAULT_SOURCE, ''), 'DEFAULT ', ''), " +
                "COALESCE(r.RDB$NULL_FLAG, 1), f.RDB$FIELD_LENGTH, " +
                "CASE f.RDB$FIELD_TYPE WHEN 261 THEN 'BLOB' WHEN 23 THEN 'BOOLEAN' WHEN 14 THEN 'CHAR' WHEN 40 THEN 'CSTRING' " +
                "WHEN 11 THEN 'D_FLOAT' WHEN 27 THEN 'DOUBLE' WHEN 10 THEN 'FLOAT' WHEN 16 THEN 'BIGINT' " +
                "WHEN 8 THEN 'INTEGER' WHEN 9 THEN 'QUAD' WHEN 7 THEN 'SMALLINT' WHEN 12 THEN 'DATE' " +
                "WHEN 13 THEN 'TIME' WHEN 35 THEN 'TIMESTAMP' WHEN 37 THEN 'VARCHAR' ELSE 'UNKNOWN' END, " +
                "COALESCE(f.RDB$FIELD_SUB_TYPE, 0), COALESCE(cset.RDB$CHARACTER_SET_NAME, 'NONE'), COALESCE(r.RDB$SYSTEM_FLAG, 0), " +
                "HASH(r.RDB$FIELD_NAME || r.RDB$FIELD_NAME || f.RDB$FIELD_TYPE || COALESCE(cset.RDB$CHARACTER_SET_NAME, 'NONE')), " +
                "IIF(r.RDB$VIEW_CONTEXT IS NULL, 'Table', 'View'), COALESCE(cset.RDB$BYTES_PER_CHARACTER, 1), r.RDB$FIELD_SOURCE, " +
                "COALESCE(r.RDB$BASE_FIELD, ''), r.RDB$VIEW_CONTEXT  " +
                "FROM RDB$RELATION_FIELDS r LEFT JOIN RDB$FIELDS f ON r.RDB$FIELD_SOURCE = f.RDB$FIELD_NAME " +
                "LEFT JOIN RDB$CHARACTER_SETS cset ON cset.RDB$CHARACTER_SET_ID = f.RDB$CHARACTER_SET_ID ";

            if (!LoadSystemObjects)
                SQL += "WHERE COALESCE(r.RDB$SYSTEM_FLAG, 0) = 0 ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        int i = 0;

                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);


                            objects.Add(new DatabaseObject(rdr.GetString(0), rdr.GetString(1), rdr.GetString(5), rdr.GetString(7),
                                rdr.GetString(13), rdr.GetString(12).StartsWith("RDB$") ? String.Empty : rdr.GetString(12),
                                rdr.GetString(10) == "Table" ? DatabaseObjectType.TableColumn : DatabaseObjectType.ViewColumn,
                                rdr.GetInt32(8) == 1, true, rdr.GetInt64(9), rdr.GetInt32(4), rdr.IsDBNull(2) ? "" : rdr.GetString(2), rdr.GetInt32(3) == 1,
                               rdr.GetInt32(6), rdr.GetInt32(11), rdr.IsDBNull(14) ? 0 : rdr.GetInt32(14)));

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadViews(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$RELATION_FIELDS", " WHERE RDB$VIEW_CONTEXT IS NOT NULL ");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT DISTINCT TRIM(rf.RDB$RELATION_NAME), HASH(TRIM(rf.RDB$RELATION_NAME)), " +
                "COALESCE(rf.RDB$SYSTEM_FLAG, 0), a.RDB$VIEW_SOURCE FROM RDB$RELATION_FIELDS rf " +
                "LEFT JOIN RDB$RELATIONS a ON (a.RDB$RELATION_NAME = rf.RDB$RELATION_NAME)";

            if (!LoadSystemObjects)
                SQL += "WHERE COALESCE(rf.RDB$SYSTEM_FLAG, 0) = 0 AND rf.RDB$VIEW_CONTEXT IS NOT NULL;";
            else
                SQL += "WHERE rf.RDB$VIEW_CONTEXT IS NOT NULL;";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        int i = 0;

                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            objects.Add(new DatabaseObject(rdr.GetString(0), String.Empty, String.Empty, String.Empty,
                                String.Empty, rdr.GetString(3).Trim(), DatabaseObjectType.View, rdr.GetInt32(2) != 0, true,
                                rdr.GetInt64(1)));

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadViewRelations(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$RELATION_FIELDS", " WHERE RDB$VIEW_CONTEXT IS NOT NULL ");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT a.RDB$VIEW_NAME, a.RDB$RELATION_NAME, a.RDB$VIEW_CONTEXT, a.RDB$CONTEXT_NAME " +
                "FROM RDB$VIEW_RELATIONS a ORDER BY a.RDB$VIEW_NAME, a.RDB$VIEW_CONTEXT ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        int i = 0;

                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            objects.Add(new DatabaseObject(rdr.GetString(0), rdr.GetString(1), rdr.GetString(3), String.Empty,
                                String.Empty, String.Empty, DatabaseObjectType.ViewRelation, false, true,
                                0, 0, "", false, 0, 1, rdr.GetInt32(2)));

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadForeignKeys(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$INDICES");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT TRIM(i.RDB$INDEX_NAME), TRIM(s.RDB$FIELD_NAME), HASH(TRIM(i.RDB$INDEX_NAME) || TRIM(s.RDB$FIELD_NAME)), " +
                "COALESCE(i.RDB$SYSTEM_FLAG, 0), COALESCE(i.RDB$SYSTEM_FLAG, 0), TRIM(rc.RDB$RELATION_NAME), " +
                "COALESCE(refc.RDB$UPDATE_RULE, 'UPDATE'), COALESCE(refc.RDB$DELETE_RULE, 'UPDATE'), rc1.RDB$RELATION_NAME, " +
                "s1.RDB$FIELD_NAME " +
                "FROM RDB$INDEX_SEGMENTS s LEFT JOIN RDB$INDICES i ON i.RDB$INDEX_NAME = s.RDB$INDEX_NAME " +
                "LEFT JOIN RDB$RELATION_CONSTRAINTS rc ON rc.RDB$INDEX_NAME = i.RDB$INDEX_NAME " +
                "JOIN RDB$REF_CONSTRAINTS refc ON refc.RDB$CONSTRAINT_NAME = i.RDB$INDEX_NAME " +
                "LEFT JOIN RDB$RELATION_CONSTRAINTS rc1 ON rc1.RDB$CONSTRAINT_NAME = refc.RDB$CONST_NAME_UQ " +
                "JOIN RDB$INDEX_SEGMENTS s1 ON s1.RDB$INDEX_NAME = rc1.RDB$INDEX_NAME " +
                "WHERE rc.RDB$CONSTRAINT_TYPE = 'FOREIGN KEY' ";

            if (!LoadSystemObjects)
                SQL += " AND COALESCE(i.RDB$SYSTEM_FLAG, 0) = 0 ";

            SQL += " ORDER BY i.RDB$RELATION_NAME, i.RDB$INDEX_NAME, s.RDB$FIELD_POSITION ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        int i = 0;

                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            objects.Add(new DatabaseObject(rdr.GetString(5), rdr.GetString(0), rdr.GetString(1), rdr.GetString(6),
                                rdr.GetString(7), rdr.GetString(8),
                                DatabaseObjectType.ForeignKey, rdr.GetInt32(3) != 0, rdr.GetInt32(4) == 1, rdr.GetInt64(2),
                                0, rdr.GetString(9), true, 0, 1));

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
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

            if (!LoadSystemObjects)
                SQL += " AND COALESCE(i.RDB$SYSTEM_FLAG, 0) = 0 ";

            SQL += " ORDER BY i.RDB$RELATION_NAME, i.RDB$INDEX_NAME, s.RDB$FIELD_POSITION ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
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
                            DatabaseObject existingObject = FindObject(newObj.ObjectName, newObj.ObjectParameter1, objects, DatabaseObjectType.PrimaryKey);

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
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadIndices(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$INDICES", "", false);
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT TRIM(i.RDB$INDEX_NAME), TRIM(s.RDB$FIELD_NAME), HASH(TRIM(i.RDB$INDEX_NAME) || TRIM(s.RDB$FIELD_NAME)), " +
                "COALESCE(i.RDB$SYSTEM_FLAG, 0), COALESCE(i.RDB$UNIQUE_FLAG, 0), TRIM(i.RDB$RELATION_NAME), COALESCE(i.RDB$INDEX_TYPE, 0) " +
                "FROM RDB$INDEX_SEGMENTS s LEFT JOIN RDB$INDICES i ON i.RDB$INDEX_NAME = s.RDB$INDEX_NAME " +
                "LEFT JOIN RDB$RELATION_CONSTRAINTS rc ON rc.RDB$INDEX_NAME = i.RDB$INDEX_NAME " +
                "WHERE rc.RDB$CONSTRAINT_TYPE IS NULL ";

            if (!LoadSystemObjects)
                SQL += " AND COALESCE(i.RDB$SYSTEM_FLAG, 0) = 0 ";

            SQL += " ORDER BY i.RDB$RELATION_NAME, i.RDB$INDEX_NAME, s.RDB$FIELD_POSITION ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        int i = 0;

                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            DatabaseObject newObj = new DatabaseObject(rdr.GetString(5), rdr.GetString(0), rdr.GetString(1),
                                rdr.GetInt32(6) == 0 ? "" : "DESCENDING", String.Empty, String.Empty,
                                DatabaseObjectType.Index, rdr.GetInt32(3) != 0, rdr.GetInt32(4) == 1, rdr.GetInt64(2));

                            //does one already exists with same name (multiple parameters)?
                            DatabaseObject existingObject = FindObject(newObj.ObjectName, newObj.ObjectParameter1, objects, DatabaseObjectType.Index);

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
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadProcedures(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$PROCEDURES");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT a.RDB$PROCEDURE_NAME, COALESCE(a.RDB$PROCEDURE_INPUTS, 0) || '#' ||  " +
                "COALESCE(a.RDB$PROCEDURE_OUTPUTS, 0) AS IN_OUT_PARAMS, COALESCE(a.RDB$SYSTEM_FLAG, 0), " +
                "CASE a.RDB$PROCEDURE_TYPE WHEN 1 THEN 'OUTPUT' WHEN 2 THEN 'INPUT' ELSE 'UNKNOWN' END AS PROCEDURE_TYPE, " +
                "a.RDB$PROCEDURE_SOURCE FROM RDB$PROCEDURES a ";

            if (!LoadSystemObjects)
                SQL += " WHERE COALESCE(a.RDB$SYSTEM_FLAG, 0) = 0 ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        int i = 0;

                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            objects.Add(new DatabaseObject(rdr.GetString(0), rdr.GetString(3), rdr.GetString(1),
                                String.Empty, String.Empty, rdr.GetString(4).Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n"), DatabaseObjectType.Procedure, rdr.GetInt32(2) != 0, true, 0));

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadParameters(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$PROCEDURE_PARAMETERS");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT p.RDB$PROCEDURE_NAME, p.RDB$PARAMETER_NAME, p.RDB$PARAMETER_NUMBER, " +
                "p.RDB$PARAMETER_TYPE, COALESCE(p.RDB$SYSTEM_FLAG, 0), COALESCE(p.RDB$NULL_FLAG, 0), COALESCE(cset.RDB$CHARACTER_SET_NAME, ''), " +
                "CASE f.RDB$FIELD_TYPE WHEN 261 THEN 'BLOB' WHEN 23 THEN 'BOOLEAN' WHEN 14 THEN 'CHAR' " +
                "WHEN 40 THEN 'CSTRING' WHEN 11 THEN 'D_FLOAT' WHEN 27 THEN 'DOUBLE' WHEN 10 THEN 'FLOAT' " +
                "WHEN 16 THEN 'BIGINT' WHEN 8 THEN 'INTEGER' WHEN 9 THEN 'QUAD' WHEN 7 THEN 'SMALLINT' " +
                "WHEN 12 THEN 'DATE' WHEN 13 THEN 'TIME' WHEN 35 THEN 'TIMESTAMP' WHEN 37 THEN 'VARCHAR' " +
                "ELSE 'UNKNOWN' END, f.RDB$FIELD_LENGTH, REPLACE(COALESCE(f.RDB$DEFAULT_SOURCE, ''), 'DEFAULT ', ''), COALESCE(f.RDB$FIELD_SUB_TYPE, 0), " +
                "COALESCE(cset.RDB$BYTES_PER_CHARACTER, 1) " +
                "FROM RDB$PROCEDURE_PARAMETERS p " +
                "LEFT JOIN RDB$FIELDS f ON (f.RDB$FIELD_NAME = p.RDB$FIELD_SOURCE) " +
                "LEFT JOIN RDB$CHARACTER_SETS cset ON f.RDB$CHARACTER_SET_ID = cset.RDB$CHARACTER_SET_ID ";


            if (!LoadSystemObjects)
                SQL += "WHERE COALESCE(p.RDB$SYSTEM_FLAG, 0) = 0 ";

            SQL += "ORDER BY p.RDB$PROCEDURE_NAME, p.RDB$PARAMETER_TYPE, p.RDB$PARAMETER_NUMBER ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    int i = 0;
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            DatabaseObject dbObjectProcedure = FindObject(rdr.GetString(0).Trim(), objects, DatabaseObjectType.Procedure);

                            DatabaseObject param = new DatabaseObject(rdr.GetString(0), rdr.GetString(1), rdr.GetString(7), rdr.GetString(6),
                                rdr.GetInt32(2).ToString(), rdr.GetInt32(3).ToString(), DatabaseObjectType.Parameter,
                                rdr.GetInt32(4) == 1, true, 0, rdr.GetInt32(8), rdr.GetString(9), rdr.GetInt32(5) == 1,
                                rdr.GetInt32(10), rdr.GetInt32(11));

                            string dataType = SQLGenerator.CreateColumnType(param.ObjectParameter2, param.Size / param.BytesPerCharacter,
                                param.ObjectParameter3, param.SubType);

                            if (param.ObjectParameter5 == "0")
                            {
                                //in param
                                if (!String.IsNullOrEmpty(dbObjectProcedure.ObjectParameter3))
                                    dbObjectProcedure.ObjectParameter3 += ", \r\n  ";

                                dbObjectProcedure.ObjectParameter3 += String.Format("{0} {1}", param.ObjectParameter1, dataType);
                            }
                            else
                            {
                                //out param
                                if (!String.IsNullOrEmpty(dbObjectProcedure.ObjectParameter4))
                                    dbObjectProcedure.ObjectParameter4 += ", \r\n  ";

                                dbObjectProcedure.ObjectParameter4 += String.Format("{0} {1}", param.ObjectParameter1, dataType);
                            }

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadGenerators(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$GENERATORS");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT a.RDB$GENERATOR_NAME, COALESCE(a.RDB$SYSTEM_FLAG, 0), a.RDB$DESCRIPTION " +
                "FROM RDB$GENERATORS a ";


            if (!LoadSystemObjects)
                SQL += "WHERE COALESCE(a.RDB$SYSTEM_FLAG, 0) = 0 ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    int i = 0;
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            objects.Add(new DatabaseObject(rdr.GetString(0), String.Empty, String.Empty, String.Empty,
                                String.Empty, rdr.GetString(2), DatabaseObjectType.Generator,
                                rdr.GetInt32(1) == 1, true, 0, 0, String.Empty, false, 0));

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadRoles(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$ROLES");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT a.RDB$ROLE_NAME, COALESCE(a.RDB$SYSTEM_FLAG, 0) FROM RDB$ROLES a ";


            if (!LoadSystemObjects)
                SQL += "WHERE COALESCE(a.RDB$SYSTEM_FLAG, 0) = 0 ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    int i = 0;
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            objects.Add(new DatabaseObject(rdr.GetString(0), String.Empty, String.Empty, String.Empty,
                                String.Empty, String.Empty, DatabaseObjectType.Role,
                                rdr.GetInt32(1) != 0, true, 0, 0, String.Empty, false, 0));

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadTriggers(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$TRIGGERS");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT a.RDB$TRIGGER_NAME, a.RDB$RELATION_NAME, a.RDB$TRIGGER_SEQUENCE, a.RDB$TRIGGER_TYPE, " +
                "COALESCE(a.RDB$TRIGGER_SOURCE, '') AS SOURCE, COALESCE(a.RDB$TRIGGER_INACTIVE, 0) AS IS_ACTIVE, " +
                "COALESCE(a.RDB$SYSTEM_FLAG, 0), a.RDB$FLAGS FROM RDB$TRIGGERS a ";


            if (!LoadSystemObjects)
                SQL += "WHERE COALESCE(a.RDB$SYSTEM_FLAG, 0) = 0 ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                { 
                    int i = 0;
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step); 
                        
                            objects.Add(new DatabaseObject(rdr.GetString(0), rdr.GetString(1), String.Empty, String.Empty,
                                rdr.GetInt32(7).ToString(), rdr.GetString(4).Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n"), 
                                DatabaseObjectType.Trigger,
                                rdr.GetInt32(6) != 0, true, rdr.GetInt32(3), rdr.GetInt32(2), String.Empty, false, rdr.GetInt32(5)));
                        
                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadFunctions(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$FUNCTIONS");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT a.RDB$FUNCTION_NAME, a.RDB$MODULE_NAME, a.RDB$ENTRYPOINT, " +
                "a.RDB$RETURN_ARGUMENT, COALESCE(a.RDB$SYSTEM_FLAG, 0) FROM RDB$FUNCTIONS a ";


            if (!LoadSystemObjects)
                SQL += "WHERE COALESCE(a.RDB$SYSTEM_FLAG, 0) = 0 ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    int i = 0;
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            DatabaseObject dbObjectFunction = new DatabaseObject(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2),
                                String.Empty, String.Empty, String.Empty, DatabaseObjectType.Function,
                                rdr.GetInt32(4) != 0, true, 0, 0, String.Empty, false, rdr.GetInt32(3));

                            objects.Add(dbObjectFunction);

                            string SQLParams = String.Format("SELECT a.RDB$FUNCTION_NAME, a.RDB$ARGUMENT_POSITION, " +
                                "CASE a.RDB$FIELD_TYPE WHEN 261 THEN 'BLOB' WHEN 23 THEN 'BOOLEAN' WHEN 14 THEN 'CHAR' WHEN 40 THEN 'CSTRING' " +
                                "WHEN 11 THEN 'D_FLOAT' WHEN 27 THEN 'DOUBLE' WHEN 10 THEN 'FLOAT' WHEN 16 THEN 'BIGINT' " +
                                "WHEN 8 THEN 'INTEGER' WHEN 9 THEN 'QUAD' WHEN 7 THEN 'SMALLINT' WHEN 12 THEN 'DATE' " +
                                "WHEN 13 THEN 'TIME' WHEN 35 THEN 'TIMESTAMP' WHEN 37 THEN 'VARCHAR' ELSE 'UNKNOWN' END || " +
                                "CASE a.RDB$FIELD_TYPE WHEN 261 THEN 'SUB_TYPE ' || a.RDB$FIELD_SUB_TYPE WHEN 14 THEN '(' || a.RDB$FIELD_LENGTH || ')' " +
                                "WHEN 40 THEN '(' || CAST(a.RDB$FIELD_LENGTH / cset.RDB$BYTES_PER_CHARACTER AS VARCHAR(12)) || ')' " +
                                "WHEN 11 THEN 'D_FLOAT' WHEN 27 THEN '(' || a.RDB$FIELD_PRECISION || ', ' || a.RDB$FIELD_SCALE || ')' " +
                                "WHEN 10 THEN 'FLOAT' WHEN 37 THEN '(' || a.RDB$FIELD_LENGTH || ')' ELSE '' END || " +
                                "' ' || COALESCE(TRIM('CHARACTER SET ' || cset.RDB$CHARACTER_SET_NAME), ''), " +
                                "CASE a.RDB$MECHANISM WHEN 1 THEN 'BY REFERENCE' WHEN 0 THEN 'BY VALUE' WHEN -1 THEN 'BY REFERENCE FREE_IT' ELSE 'UNKNOWN' END " +
                                "FROM RDB$FUNCTION_ARGUMENTS a " +
                                "LEFT JOIN RDB$CHARACTER_SETS cset ON cset.RDB$CHARACTER_SET_ID = a.RDB$CHARACTER_SET_ID " +
                                "WHERE a.RDB$FUNCTION_NAME = '{0}' " +
                                "ORDER BY a.RDB$FUNCTION_NAME, a.RDB$ARGUMENT_POSITION ", rdr.GetString(0));

                            FbCommand cmdParams = new FbCommand(SQLParams, objects.Connection, transaction);
                            try
                            {
                                FbDataReader rdrParams = cmdParams.ExecuteReader();
                                try
                                {
                                    int iParams = 0;

                                    while (rdrParams.Read())
                                    {
                                        if (rdrParams.GetInt32(1) == 0)
                                        {
                                            dbObjectFunction.ObjectParameter3 = rdrParams.GetString(2).Trim();
                                        }
                                        else
                                        {
                                            if (iParams == 0)
                                                dbObjectFunction.ObjectParameter4 = rdrParams.GetString(2).Trim();
                                            else
                                                dbObjectFunction.ObjectParameter4 += String.Format(", {0}", rdrParams.GetString(2).Trim());

                                            iParams++;
                                        }
                                    }
                                }
                                finally
                                {
                                    rdrParams.Close();
                                    rdrParams.Dispose();
                                }
                            }
                            finally
                            {
                                cmdParams.Dispose();
                            }

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadExceptions(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$EXCEPTIONS");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT a.RDB$EXCEPTION_NAME, a.RDB$MESSAGE, COALESCE(a.RDB$SYSTEM_FLAG, 0) " +
                "FROM RDB$EXCEPTIONS a ";


            if (!LoadSystemObjects)
                SQL += "WHERE COALESCE(a.RDB$SYSTEM_FLAG, 0) = 0 ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    int i = 0;
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            objects.Add(new DatabaseObject(rdr.GetString(0), rdr.GetString(1), String.Empty, String.Empty,
                                String.Empty, String.Empty, DatabaseObjectType.Exception,
                                rdr.GetInt32(2) != 0, true, 0, 0, String.Empty, false, 0));

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
            }
        }

        private static void LoadDomains(DatabaseObjects objects, string processStatus, int step)
        {
            if (CancelAnalysis)
                return;

            int total = GetCount(objects, "RDB$FIELDS");
            RaiseProcessStatusChanged(processStatus, step);

            string SQL = "SELECT f.RDB$FIELD_NAME, COALESCE(f.RDB$VALIDATION_SOURCE, '') AS VALIDATION_SOURCE, " +
                "CAST(COALESCE(f.RDB$COMPUTED_SOURCE, '') AS VARCHAR(8000)) AS COMPUTED_SOURCE,  " +
                "REPLACE(COALESCE(f.RDB$DEFAULT_SOURCE, ''), 'DEFAULT ', '') AS DEFAULT_SOURCE, f.RDB$FIELD_LENGTH, f.RDB$FIELD_SCALE,  " +
                "COALESCE(f.RDB$FIELD_SUB_TYPE, 0) AS SUB_TYPE,  COALESCE(f.RDB$SYSTEM_FLAG, 0) as SYS_FLAG,   " +
                "COALESCE(f.RDB$NULL_FLAG, 0) as NULL_FLAG,  " +
                "COALESCE(f.RDB$FIELD_PRECISION, 0), " +
                "CASE f.RDB$FIELD_TYPE WHEN 261 THEN 'BLOB' WHEN 23 THEN 'BOOLEAN' WHEN 14 THEN 'CHAR' WHEN 40 THEN 'CSTRING'  " +
                "WHEN 11 THEN 'D_FLOAT' WHEN 27 THEN 'DOUBLE' WHEN 10 THEN 'FLOAT' WHEN 16 THEN 'BIGINT'  " +
                "WHEN 8 THEN 'INTEGER' WHEN 9 THEN 'QUAD' WHEN 7 THEN 'SMALLINT' WHEN 12 THEN 'DATE'  " +
                "WHEN 13 THEN 'TIME' WHEN 35 THEN 'TIMESTAMP' WHEN 37 THEN 'VARCHAR' ELSE 'UNKNOWN' END AS FIELD_TYPE, " +
                "COALESCE(cset.RDB$CHARACTER_SET_NAME, 'NONE') AS CHAR_sET, COALESCE(cset.RDB$BYTES_PER_CHARACTER, 1) " +
                "FROM RDB$FIELDS f /* INNER JOIN RDB$RELATION_FIELDS rf on (rf.RDB$FIELD_SOURCE = f.RDB$FIELD_NAME) */ " +
                "LEFT OUTER JOIN RDB$CHARACTER_SETS cset ON f.RDB$CHARACTER_SET_ID = cset.RDB$CHARACTER_SET_ID ";


            if (!LoadSystemObjects)
                SQL += "WHERE COALESCE(f.RDB$SYSTEM_FLAG, 0) = 0 ";

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    int i = 0;
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        while (rdr.Read())
                        {
                            if (CancelAnalysis)
                                return;

                            RaiseProcessStatusChanged(processStatus, Utilities.Percentage(total, i), step);

                            DatabaseObject newObject = new DatabaseObject(rdr.GetString(0), rdr.GetString(10), rdr.GetString(11), rdr.GetString(1),
                                rdr.GetInt32(9).ToString(), String.Empty, DatabaseObjectType.Domain,
                                rdr.GetInt32(7) != 0, true, rdr.GetInt32(6), rdr.GetInt32(4), rdr.GetString(3),
                                rdr.GetInt32(8) == 1, 0, rdr.GetInt32(12));

                            if (objects.Find(newObject.ObjectName, newObject.ObjectType) == null)
                                objects.Add(newObject);

                            i++;
                        }
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
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
                transaction.Dispose();
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

                if (!LoadSystemObjects && allowSystemCheck)
                    SQL += " AND COALESCE(RDB$SYSTEM_FLAG, 0) = 0";
            }
            else
            {
                if (!LoadSystemObjects && allowSystemCheck)
                    SQL += "WHERE COALESCE(RDB$SYSTEM_FLAG, 0) = 0";
            }

            FbTransaction transaction = objects.Connection.BeginTransaction();
            try
            {
                FbCommand cmd = new FbCommand(SQL, objects.Connection, transaction);
                try
                {
                    FbDataReader rdr = cmd.ExecuteReader();
                    try
                    {
                        if (rdr.Read())
                            Result = rdr.GetInt32(0);
                    }
                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                    }
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            finally
            {
                transaction.Rollback();
                transaction.Dispose();
            }

            return (Result);
        }

        #endregion Load System Objects

        #region Initialisation / Finalisation

        private static void Initialise()
        {
            _databaseObjectsMaster.Clear();
            _databaseObjectsMaster.Connection = _masterDB;

            _databaseObjectsChild.Clear();
            _databaseObjectsChild.Connection = _childDB;

            _dependenciesMaster.Clear();
            _dependenciesMaster.Connection = _masterDB;

            _childDB.Open();
            _masterDB.Open();
        }

        private static void Finalise()
        {
            _childDB.Close();
            _childDB.Dispose();
            _childDB = null;

            _masterDB.Close();
            _masterDB.Dispose();
            _masterDB = null;

            _databaseObjectsMaster.Connection = null;

            _databaseObjectsChild.Connection = null;

            _dependenciesMaster.Connection = null;
        }

        #endregion Initialisation / Finalisation

        #region SQL Generation

        /// <summary>
        /// Generates SQL code for objects
        /// </summary>
        /// <param name="dbObject">object whose code needs generating</param>
        /// <param name="primaryObjectList">List of primary objects</param>
        private static void GenerateMissingSQL(DatabaseObject dbObject, DatabaseObjects primaryObjectList)
        {
            SQLGenerator generator = new SQLGenerator();
            dbObject.SQL = generator.GenerateSQLScript(_databaseObjectsMaster, _databaseObjectsChild, dbObject, null);

        }

        #endregion SQL Generation

        #region Others

        private static void CloseAndDispose(ref FbCommand command, ref FbDataReader reader)
        {
            if (command != null)
            {
                command.Dispose();
                command = null;
            }

            if (reader == null)
                return;

            reader.Close();
            reader.Dispose();
            reader = null;
        }

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

        #endregion Events
    }

    /// <summary>
    /// Status of Database Object Compared to Master Table
    /// </summary>
    public enum ObjectStatus 
    { 
        NotProcessed, 
        
        Processing, 
        
        Found, 
        
        MissingFromMaster, 
        
        MissingFromChild, 
        
        DifferentSettings, 
        
        ParentObjectDoesNotExist 
    }

    /// <summary>
    /// Type of database object
    /// </summary>
    public enum DatabaseObjectType 
    { 
        Table, 
        
        TableColumn, 

        ViewColumn,

        Index, 
        
        PrimaryKey, 
        
        ForeignKey, 
        
        Procedure, 
        
        Parameter, 
        
        Generator, 
        
        Trigger, 
        
        View, 

        ViewRelation,
        
        Role,

        Exception,

        Function,

        Domain,

        Dependency
    }

    /// <summary>
    /// Database Object
    /// </summary>
    public class DatabaseObject
    {
        #region Constructors

        public DatabaseObject(string objectName, string objectParameter1, string objectParameter2, string objectParameter3,
            string objectParameter4, string objectParameter5, DatabaseObjectType objectType,
            bool sysObject, bool unique, Int64 hash, int size = 0, string defaultValue = "", bool notNull = true, 
            int subType = 0, int bytesPerCharacter = 1, int viewContext = 0)
        {
            ObjectName = objectName.Trim();
            ObjectParameter1 = objectParameter1.Trim();
            ObjectParameter2 = objectParameter2.Trim();
            ObjectParameter3 = objectParameter3.Trim();
            ObjectParameter4 = objectParameter4.Trim();
            ObjectParameter5 = objectParameter5.Trim();
            ObjectType = objectType;
            System = sysObject;
            Hash = hash;
            Unique = unique;
            FurtherChecks = true;

            DifferentName = String.Empty;
            ExistsWithDifferentName = false;
            DependencyMissing = false;

            Status = ObjectStatus.NotProcessed;

            // following have default values
            Size = size;
            DefaultValue = defaultValue.Trim();
            NotNull = notNull;
            SubType = subType;
            BytesPerCharacter = bytesPerCharacter;
            ViewContext = viewContext;
        }

        #endregion Constructors

        #region Methods

        internal void SetInformation(DatabaseObject masterObject)
        {
            MasterObject = masterObject;

            string temp = String.Empty;

            switch (this.ObjectType)
            {
                case DatabaseObjectType.TableColumn:
                case DatabaseObjectType.ViewColumn:

                    if (this.NotNull != masterObject.NotNull)
                    {
                        if (this.NotNull)
                            temp += "Child allows NULL value's, Master does not\r\n";
                        else
                            temp += "Master allows NULL value's Child does not\r\n";
                    }

                    if (this.DefaultValue != masterObject.DefaultValue)
                        temp += String.Format("Default value for column is different\r\n\r\nMaster Table Default is: {0}\r\nChild Default is: {1}\r\n",
                            String.IsNullOrEmpty(masterObject.DefaultValue) ? "No Default Value" : masterObject.DefaultValue, 
                            String.IsNullOrEmpty(this.DefaultValue) ? "No Default Value" : this.DefaultValue);

                    if (this.Size != masterObject.Size)
                    {
                        temp += "Size of column is different\r\n";

                        if (this.Size < masterObject.Size)
                            temp += String.Format("Column size {0} is smaller than Master Table {1}\r\n", this.Size / this.BytesPerCharacter, masterObject.Size / masterObject.BytesPerCharacter);
                        else
                            temp += String.Format("Column size {0} is larger than Master Table {1}\r\n", this.Size / this.BytesPerCharacter, masterObject.Size / masterObject.BytesPerCharacter);
                    }

                    if (this.ObjectParameter2 != masterObject.ObjectParameter2)
                        temp += String.Format("Data type for column is different\r\nChild Data Type {0}; Master Data Type {1}\r\n", this.ObjectParameter2, masterObject.ObjectParameter2);

                    if (this.ObjectParameter3 != masterObject.ObjectParameter3)
                        temp += String.Format("Character set for column is different\r\nChild Character Set {0}; Master Character Set {1}\r\n", this.ObjectParameter3, masterObject.ObjectParameter3);

                    break;

                case DatabaseObjectType.Parameter:

                    if (this.NotNull != masterObject.NotNull)
                        temp += "Allow null values for parameter is different\r\n";

                    if (this.DefaultValue != masterObject.DefaultValue)
                        temp += "Default value for parameter is different\r\n\r\n";

                    if (this.Size != masterObject.Size)
                        temp += "Size of parameter is different\r\n";

                    if (this.ObjectParameter2 != masterObject.ObjectParameter2)
                        temp += "Data type for parameter is different\r\n";

                    if (this.ObjectParameter3 != masterObject.ObjectParameter3)
                        temp += "Character set for parameter is different\r\n";

                    if (this.ObjectParameter4 != masterObject.ObjectParameter4)
                        temp += "Parameter Number for parameter is different\r\n";

                    if (this.ObjectParameter5 != masterObject.ObjectParameter5)
                        temp += "Parameter type for parameter is different\r\n";

                    break;

                case DatabaseObjectType.ForeignKey:

                    if (this.ObjectParameter2 != masterObject.ObjectParameter2)
                        temp += String.Format("Column for referenced table is different\r\nChild Column {0}; Master Column {1}\r\n", this.ObjectParameter2, masterObject.ObjectParameter2);

                    if (this.ObjectParameter3 != masterObject.ObjectParameter3)
                        temp += String.Format("Update rule for referenced table is different\r\nChild Rule {0}; Master Rule {1}\r\n", this.ObjectParameter3, masterObject.ObjectParameter3);

                    if (this.ObjectParameter4 != masterObject.ObjectParameter4)
                        temp += String.Format("Delete rule for referenced table is different\r\nChild Rule {0}; Master Rule {1}\r\n", this.ObjectParameter4, masterObject.ObjectParameter4);

                    if (this.ObjectParameter5 != masterObject.ObjectParameter5)
                        temp += String.Format("Referenced table is different\r\nChild Table {0}; Master Table {1}\r\n", this.ObjectParameter5, masterObject.ObjectParameter5);

                    break;

                case DatabaseObjectType.Procedure:

                    if (this.ObjectParameter3 != masterObject.ObjectParameter3)
                        temp += "Input parameters do not match\r\n";

                    if (this.ObjectParameter4 != masterObject.ObjectParameter4)
                        temp += "Output parameters do not match\r\n";

                    if (this.ObjectParameter5 != masterObject.ObjectParameter5)
                        temp += "The procedure contains different code\r\n";

                    break;

                case DatabaseObjectType.Trigger:
                    if (this.ObjectParameter1 != masterObject.ObjectParameter1)
                        temp += String.Format("Trigger assigned to different table, Master: {0}; Child {1}\r\n", masterObject.ObjectParameter1, this.ObjectParameter1);

                    if (this.ObjectParameter4 != masterObject.ObjectParameter4)
                        temp += String.Format("Trigger has different position, Master: {0}; Child {1}\r\n", masterObject.ObjectParameter4, this.ObjectParameter4);

                    if (this.ObjectParameter5 != masterObject.ObjectParameter5)
                        temp += "The trigger contains different code\r\n";

                    break;
                default:
                    temp += "";
                    break;
            }


            //this.NotNull != masterObject.NotNull ||
            //this.DefaultValue != masterObject.DefaultValue ||
            //this.Size != masterObject.Size ||
            //this.SubType != masterObject.SubType ||
            //this.ObjectParameter2 != masterObject.ObjectParameter2 ||
            //this.ObjectParameter3 != masterObject.ObjectParameter3 ||
            //this.ObjectParameter4 != masterObject.ObjectParameter4 ||
            //this.ObjectParameter5 != masterObject.ObjectParameter5        

            Information = temp;
        }

        internal void DoFurtherChecks()
        {

        }

        #endregion Methods

        #region Overridden Methods

        public override string ToString()
        {
            string Result = String.Format("{0}; {1} {2}", Status.ToString(), ObjectType.ToString(), ObjectName);

            switch (ObjectType)
            {
                case DatabaseObjectType.Table:
                    Result = String.Format("{0}; Table: {1}", Status.ToString(), ObjectName);

                    break;

                case DatabaseObjectType.TableColumn:
                    Result = String.Format("{0}; Column: {2}; Table: {1}; Type: {3}; Size: {4}",
                        Status.ToString(), ObjectName, ObjectParameter1, ObjectParameter2, Size / BytesPerCharacter);

                    break;

                case DatabaseObjectType.ViewColumn:
                    Result = String.Format("{0}; Column: {2}; View: {1}; Type: {3}; Size: {4}",
                        Status.ToString(), ObjectName, ObjectParameter1, ObjectParameter2, Size / BytesPerCharacter);

                    break;

                case DatabaseObjectType.ForeignKey:
                    Result = String.Format("{0}; Foreign Key: {1}; Table: {2}", 
                        Status.ToString(), ObjectParameter1, ObjectName);
                    break;

                case DatabaseObjectType.Index:
                    Result = String.Format("{0}; Index: {1}; Table: {2}",
                        Status.ToString(), ObjectParameter1, ObjectName);

                    break;

                case DatabaseObjectType.PrimaryKey:
                    Result = String.Format("{0}; Primary Key: {1}; Table: {2}",
                        Status.ToString(), ObjectParameter1, ObjectName);

                    break;

                case DatabaseObjectType.Generator:
                    Result = String.Format("{0} Generator: {1}",
                        Status.ToString(), ObjectName);

                    break;

                case DatabaseObjectType.Procedure:
                    Result = String.Format("{0} Stored Procedure: {1}",
                        Status.ToString(), ObjectName);

                    break;

                case DatabaseObjectType.Parameter:
                    Result = String.Format("{0} Stored Procedure: {1}; Parameter: {2}", 
                        Status.ToString(), ObjectName, ObjectParameter1);

                    break;

                case DatabaseObjectType.Trigger:
                    Result = String.Format("{0} Trigger: {1}",
                        Status.ToString(), ObjectName);

                    break;

                case DatabaseObjectType.View:
                    Result = String.Format("{0} View: {1}",
                        Status.ToString(), ObjectName);

                    break;

                case DatabaseObjectType.Role:
                    Result = String.Format("{0} Role: {1}",
                        Status.ToString(), ObjectName);

                    break;

                case DatabaseObjectType.Domain:
                    Result = String.Format("{0} Domain: {1}",
                        Status.ToString(), ObjectName);

                    break;

                case DatabaseObjectType.Exception:
                    Result = String.Format("{0} Exception: {1}",
                        Status.ToString(), ObjectName);

                    break;

                case DatabaseObjectType.Function:
                    Result = String.Format("{0} Function: {1}",
                        Status.ToString(), ObjectName);

                    break;

                default:

                    throw new Exception("Unknown ObjectType");
            }

            return (Result);
        }

        #endregion Overridden Methods

        #region Properties

        /// <summary>
        /// Name of database object if relevant
        /// </summary>
        public string ObjectName { get; private set; }

        /// <summary>
        /// Name of object 1
        /// </summary>
        public string ObjectParameter1 { get; internal set; }

        /// <summary>
        /// Name of object 2
        /// </summary>
        public string ObjectParameter2 { get; internal set; }

        /// <summary>
        /// Name of object 3
        /// </summary>
        public string ObjectParameter3 { get; internal set; }

        /// <summary>
        /// Name of object 4
        /// </summary>
        public string ObjectParameter4 { get; internal set; }

        /// <summary>
        /// Name of object 5
        /// </summary>
        public string ObjectParameter5 { get; internal set; }
        
        /// <summary>
        /// Type of object
        /// </summary>
        public DatabaseObjectType ObjectType { get; private set; }

        /// <summary>
        /// Status of object
        /// </summary>
        public ObjectStatus Status { get; internal set; }

        /// <summary>
        /// Hash of data, if relevant
        /// </summary>
        public Int64 Hash { get; private set; }

        /// <summary>
        /// If true it's a system object
        /// </summary>
        public bool System { get; private set; }

        /// <summary>
        /// Indicates wether the object is unique or not
        /// </summary>
        public bool Unique { get; private set; }

        /// <summary>
        /// Indicates wether or not to do further checks
        /// </summary>
        public bool FurtherChecks { get; internal set; }

        /// <summary>
        /// If true indicates that their is an object with same properties but different name
        /// </summary>
        public bool ExistsWithDifferentName { get; internal set; }

        /// <summary>
        /// Name of object with different name
        /// </summary>
        public string DifferentName { get; internal set; }

        /// <summary>
        /// Size of object
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Default value for object
        /// </summary>
        public string DefaultValue { get; private set; }

        /// <summary>
        /// Object allows null values
        /// </summary>
        public bool NotNull { get; private set; }

        /// <summary>
        /// SubType of object
        /// </summary>
        public int SubType { get; private set; }

        /// <summary>
        /// Extra information about the testing of the objects
        /// </summary>
        public string Information { get; internal set; }

        /// <summary>
        /// SQL Required to fix problem
        /// </summary>
        public string SQL { get; internal set; }

        /// <summary>
        /// Number of bytes for each character
        /// 
        /// Depends on character set used
        /// </summary>
        public int BytesPerCharacter { get; internal set; }

        /// <summary>
        /// Master Object, if applicable
        /// </summary>
        public DatabaseObject MasterObject { get; private set; }

        /// <summary>
        /// If true, one of the dependencies for the object is missing, if false dependency exists
        /// </summary>
        public bool DependencyMissing { get; internal set; }

        /// <summary>
        /// View Context for Object
        /// </summary>
        public int ViewContext { get; internal set; }

        #endregion Properties
    }

    /// <summary>
    /// Database Object Collection
    /// </summary>
    public class DatabaseObjects : CollectionBase
    {
        #region Properties

        /// <summary>
        /// Database connection object associated with the object
        /// </summary>
        internal FbConnection Connection { get; set; }

        #endregion Properties

        #region Public Methods

        public DatabaseObject Find(string objectName, DatabaseObjectType objectType)
        {
            DatabaseObject Result = null;

            foreach (DatabaseObject dbObject in this)
            {
                if (dbObject.ObjectType == objectType &&
                    dbObject.ObjectName == objectName)
                {
                    Result = dbObject;
                    break;
                }
            }

            return (Result);
        }


        public bool Find(string objectName, DatabaseObjects objectList, DatabaseObjectType objectType)
        {
            bool Result = false;

            foreach (DatabaseObject dbObject in objectList)
            {
                if (dbObject.ObjectType == objectType &&
                    dbObject.ObjectName == objectName)
                {
                    Result = true;
                    break;
                }
            }

            return (Result);
        }

        public DatabaseObject Find(string objectName, string objectParam1, DatabaseObjects objectList,
            DatabaseObjectType objectType)
        {
            DatabaseObject Result = null;

            foreach (DatabaseObject dbObject in objectList)
            {
                if (dbObject.ObjectType == objectType &&
                    dbObject.ObjectName == objectName &&
                    dbObject.ObjectParameter1 == objectParam1)
                {
                    Result = dbObject;
                    break;
                }
            }

            return (Result);
        }

        #endregion Public Methods

        #region Generic CollectionBase Code

        #region Internal Methods

        /// <summary>
        /// Adds an item to the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal int Add(DatabaseObject value)
        {
            return (List.Add(value));
        }

        /// <summary>
        /// Returns the index of an item within the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal int IndexOf(DatabaseObject value)
        {
            return (List.IndexOf(value));
        }

        /// <summary>
        /// Inserts an item into the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        internal void Insert(int index, DatabaseObject value)
        {
            List.Insert(index, value);
        }


        /// <summary>
        /// Removes an item from the collection
        /// </summary>
        /// <param name="value"></param>
        internal void Remove(DatabaseObject value)
        {
            List.Remove(value);
        }


        /// <summary>
        /// Indicates the existence of an item within the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal bool Contains(DatabaseObject value)
        {
            // If value is not of type OBJECT_TYPE, this will return false.
            return (List.Contains(value));
        }

        #endregion Internal Methods

        #region Private Members

        private const string OBJECT_TYPE = "WebDefender.Classes.DatabaseObject";
        private const string OBJECT_TYPE_ERROR = "Must be of type DatabaseObject";


        #endregion Private Members

        #region Overridden Methods

        /// <summary>
        /// When Inserting an Item
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        protected override void OnInsert(int index, Object value)
        {
            if (value.GetType() != Type.GetType(OBJECT_TYPE))
                throw new ArgumentException(OBJECT_TYPE_ERROR, "value");
        }


        /// <summary>
        /// When removing an item
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        protected override void OnRemove(int index, Object value)
        {
            if (value.GetType() != Type.GetType(OBJECT_TYPE))
                throw new ArgumentException(OBJECT_TYPE_ERROR, "value");
        }


        /// <summary>
        /// When Setting an Item
        /// </summary>
        /// <param name="index"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected override void OnSet(int index, Object oldValue, Object newValue)
        {
            if (newValue.GetType() != Type.GetType(OBJECT_TYPE))
                throw new ArgumentException(OBJECT_TYPE_ERROR, "newValue");
        }


        /// <summary>
        /// Validates an object
        /// </summary>
        /// <param name="value"></param>
        protected override void OnValidate(Object value)
        {
            if (value.GetType() != Type.GetType(OBJECT_TYPE))
                throw new ArgumentException(OBJECT_TYPE_ERROR);
        }


        #endregion Overridden Methods

        #endregion Generic CollectionBase Code
    }

    public class ValidationArgs
    {
        #region Constructor

        public ValidationArgs(string processStatus, int totalSteps, int currentStep)
        {
            ProcessStatus = processStatus;
            IsPercentage = false;
            Percent = 0;
            TotalSteps = totalSteps;
            CurrentStep = currentStep;
        }

        public ValidationArgs(string processStatus, int percentage, int totalSteps, int currentStep)
        {
            ProcessStatus = processStatus;
            Percent = percentage;
            IsPercentage = true;
            TotalSteps = totalSteps;
            CurrentStep = currentStep;
        }

        #endregion Constructor

        #region Internal Methods

        internal void Update (int currentStep)
        {
            CurrentStep = currentStep;
            Percent = Shared.Utilities.Percentage(TotalSteps, CurrentStep);
        }

        #endregion Internal Methods

        #region Properties

        /// <summary>
        /// Process Status
        /// </summary>
        public string ProcessStatus { get; private set; }

        /// <summary>
        /// Percentage of operation
        /// </summary>
        public int Percent { get; private set; }

        /// <summary>
        /// Indicates the value can be used as a percentage
        /// </summary>
        public bool IsPercentage { get; private set; }

        /// <summary>
        /// Total number of steps
        /// </summary>
        public int TotalSteps { get; private set; }

        /// <summary>
        /// Current Step for overall progress
        /// </summary>
        public int CurrentStep { get; private set; }

        #endregion Properties
    }

    public delegate void ValidationArgsDelegate (object sender, ValidationArgs e);


    internal class SQLGenerator
    {
        #region Internal Methods

        /// <summary>
        /// Generates SQL DDL for a given object
        /// </summary>
        /// <param name="masterObjects">List of all objects</param>
        /// <param name="newObject">Create DDL for this object</param>
        /// <param name="existingObject">Existing object or null, if exists then create ddl to drop object</param>
        /// <returns></returns>
        internal string GenerateSQLScript(DatabaseObjects masterObjects, DatabaseObjects childObjects, 
            DatabaseObject newObject, DatabaseObject existingObject)
        {
            MasterObjects = masterObjects;
            ChildObjects = childObjects;
            NewObject = newObject;
            ExistingObject = existingObject;

            string Result = String.Empty;

            switch (newObject.ObjectType)
            {
                case DatabaseObjectType.Table:
                    Result = CreateTable();
                    break;

                case DatabaseObjectType.ViewColumn:
                    Result = CreateView();
                    break;

                case DatabaseObjectType.Procedure:
                    Result = CreateProcedure();
                    break;

                case DatabaseObjectType.Parameter:
                    break;

                case DatabaseObjectType.View:
                    Result = CreateView();
                    break;

                case DatabaseObjectType.TableColumn:
                    Result = CreateColumn();
                    break;

                case DatabaseObjectType.Index:
                    Result = CreateIndex();
                    break;

                case DatabaseObjectType.Function:
                    Result = CreateFunction();
                    break;

                case DatabaseObjectType.ForeignKey:
                    Result = CreateForeignKey();
                    break;

                case DatabaseObjectType.PrimaryKey:
                    Result = CreatePrimaryKey();
                    break;

                case DatabaseObjectType.Trigger:
                    Result = CreateTrigger();
                    break;

                case DatabaseObjectType.Role:
                    Result = CreateRole();
                    break;

                case DatabaseObjectType.Generator:
                    Result = CreateGenerator();
                    break;

                case DatabaseObjectType.Exception:
                    Result = CreateException();
                    break;

                case DatabaseObjectType.Domain:
                    Result = CreateDomain();
                    break;
            }

            return (Result);
        }

        #endregion Internal Methods

        #region Private Methods

        #region Helper Methods

        private DatabaseObject FindPrimaryKey(string objectName, string objectParam1, DatabaseObjects childObjects)
        {
            DatabaseObject Result = null;

            foreach (DatabaseObject dbObject in childObjects)
            {
                if (dbObject.ObjectType == DatabaseObjectType.PrimaryKey &&
                    dbObject.ObjectName == objectName &&
                    dbObject.ObjectParameter2 == objectParam1)
                {
                    Result = dbObject;
                    break;
                }
            }

            return (Result);
        }

        private DatabaseObject FindView(string objectName, DatabaseObjects childObjects)
        {
            DatabaseObject Result = null;

            foreach (DatabaseObject dbObject in childObjects)
            {
                if (dbObject.ObjectType == DatabaseObjectType.View &&
                    dbObject.ObjectName == objectName)
                {
                    Result = dbObject;
                    break;
                }
            }

            return (Result);
        }

        private DatabaseObject FindViewRelation(string objectName, int viewContext, DatabaseObjects childObjects)
        {
            DatabaseObject Result = null;

            foreach (DatabaseObject dbObject in childObjects)
            {
                if (dbObject.ObjectType == DatabaseObjectType.ViewRelation &&
                    dbObject.ObjectName == objectName && dbObject.ViewContext == viewContext)
                {
                    Result = dbObject;
                    break;
                }
            }

            return (Result);
        }

        private string GetDefaultValue(DatabaseObject dbObject)
        {
            string Result = String.Empty;

            if (!String.IsNullOrEmpty(dbObject.DefaultValue) || (dbObject.MasterObject != null && !String.IsNullOrEmpty(dbObject.MasterObject.DefaultValue)))
            {
                Result = String.IsNullOrEmpty(dbObject.DefaultValue) ? dbObject.MasterObject.DefaultValue : dbObject.DefaultValue;

                Result = Result.Replace("DEFAULT", "").Trim();
            }
            else
            {
                //determine default from data type
                switch (dbObject.ObjectParameter2)
                {
                    case "CHAR":
                    case "VARCHAR":
                    case "CSTRING":
                    case "BLOB":
                        Result = "''";
                        break;

                    case "INTEGER":
                    case "BIGINT":
                    case "INT64":
                    case "SMALLINT":
                        Result = "0";
                        break;

                    case "DATE":
                        Result = "CURRENT_DATE";
                        break;

                    case "TIME":
                        Result = "CURRENT_TIME";
                        break;

                    case "TIMESTAMP":
                        Result = "CURRENT-tiME";
                        break;

                    case "D_FLOAT":
                    case "DOUBLE":
                    case "FLOAT":
                        Result = "0.00";
                        break;
                }
            }

            return (Result);
        }

        internal static string CreateColumnType(string type, int size, string charSet, int subType)
        {
            string Result = type;

            switch (type)
            {
                case "VARCHAR":
                case "CHAR":
                    Result += String.Format("({0}) CHARACTER SET {1}", size, charSet);
                    break;

                case "DOUBLE":
                    Result += " PRECISION ";
                    break;

                case "BLOB":
                    Result += String.Format(" SUB_TYPE {0} ", subType);
                    break;
            }

            return (Result);
        }

        #endregion Helper Methods

        #region DB Object Creation Methods

        private string CreateTable()
        {
            string Result = String.Format("CREATE TABLE {0}\r\n(\r\n", NewObject.ObjectName);
            string append = String.Empty;

            /*
             * 
             CREATE TABLE table_name
            (
                column_name {< datatype> | COMPUTED BY (< expr>) | domain}
                    [DEFAULT { literal | NULL | USER}] [NOT NULL]
                ...
                CONSTRAINT constraint_name
                    PRIMARY KEY (column_list),
                    UNIQUE      (column_list),
                    FOREIGN KEY (column_list) REFERENCES other_table (column_list),
                    CHECK       (condition),
                ...
            );
             * 
             */

            bool itemAdded = false;

            foreach (DatabaseObject dbObject in MasterObjects)
            {
                if (dbObject.ObjectName == NewObject.ObjectName)
                {
                    switch (dbObject.ObjectType)
                    {
                        case DatabaseObjectType.TableColumn:
                            if (itemAdded)
                            {
                                Result += ",\r\n";
                            }
                            else
                            {
                                Result += "\r\n";
                                itemAdded = true;
                            }

                            if (String.IsNullOrEmpty(dbObject.ObjectParameter5))
                            {
                                Result += String.Format("    {0} {1} {2} {3}",
                                    dbObject.ObjectParameter1,
                                    CreateColumnType(dbObject.ObjectParameter2, dbObject.Size / dbObject.BytesPerCharacter, dbObject.ObjectParameter3, dbObject.SubType),
                                    String.IsNullOrEmpty(dbObject.DefaultValue) ? "" : String.Format("DEFAULT {0}", dbObject.DefaultValue),
                                    dbObject.NotNull ? " NOT NULL" : "");
                            }
                            else
                            {
                                Result += String.Format("    {0} {1}",
                                    dbObject.ObjectParameter1, dbObject.ObjectParameter5);
                            }
                            break;

                        case DatabaseObjectType.PrimaryKey:
                            if (itemAdded)
                            {
                                Result += ",\r\n\r\n";
                            }
                            else
                            {
                                Result += "\r\n";
                                itemAdded = true;
                            }

                            Result += String.Format("    CONSTRAINT {0} PRIMARY KEY ({1})", dbObject.ObjectParameter1, dbObject.ObjectParameter2);

                            break;
                        case DatabaseObjectType.Index:

                            append += String.Format("CREATE {0} {4} INDEX {1} ON {2} ({3});\r\n",
                                dbObject.Unique ? "UNIQUE " : "", dbObject.ObjectParameter1, dbObject.ObjectName,
                                dbObject.ObjectParameter2, dbObject.ObjectParameter3);

                            break;
                        case DatabaseObjectType.ForeignKey:
                            if (itemAdded)
                            {
                                Result += ",\r\n";
                            }
                            else
                            {
                                Result += "\r\n";
                                itemAdded = true;
                            }

                            Result += String.Format("    CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3}) ON UPDATE {4} ON DELETE {5}",
                                dbObject.ObjectParameter1, dbObject.ObjectParameter2, dbObject.ObjectParameter5, dbObject.DefaultValue,
                                dbObject.ObjectParameter3, dbObject.ObjectParameter4);

                            Result = Result.Replace("ON UPDATE RESTRICT", "").Replace("ON DELETE RESTRICT", "");

                            break;
                    }
                }
            }


            Result = String.Format("{0}\r\n);\r\n\r\n{1}\r\n", Result, append);

            return (Result);
        }

        private string CreateProcedure()
        {
            string Result = String.Format("SET TERM ^ ; \r\n\r\nCREATE OR ALTER PROCEDURE {0}\r\n", NewObject.ObjectName);

            if (!String.IsNullOrEmpty(NewObject.MasterObject.ObjectParameter3))
                Result += String.Format(" ({0})\r\n", NewObject.MasterObject.ObjectParameter3);

            if (!String.IsNullOrEmpty(NewObject.MasterObject.ObjectParameter4))
                Result += String.Format("RETURNS\r\n ({0})\r\n", NewObject.MasterObject.ObjectParameter4);

            Result += "AS\r\n";

            Result += NewObject.MasterObject.ObjectParameter5;

            Result += "^\r\n\r\nSET TERM ; ^ \r\n\r\n";

            return (Result);
        }

        private string CreateColumn()
        {
            string Result = String.Empty;

            if (NewObject.Status == ObjectStatus.DifferentSettings)
            {
                if (NewObject.DefaultValue != NewObject.MasterObject.DefaultValue)
                {
                    if (String.IsNullOrEmpty(NewObject.MasterObject.DefaultValue))
                        Result = String.Format("ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT;\r\n", NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter1);
                    else
                        Result = String.Format("ALTER TABLE {0} ALTER COLUMN {1} SET DEFAULT {2};\r\n", NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.DefaultValue);
                }

                if (NewObject.NotNull != NewObject.MasterObject.NotNull)
                {
                    if (NewObject.NotNull)
                    {
                        Result += String.Format("UPDATE RDB$RELATION_FIELDS SET RDB$NULL_FLAG = NULL WHERE RDB$FIELD_NAME = '{0}' AND RDB$RELATION_NAME = '{1}';\r\n",
                            NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.ObjectName);
                    }
                    else
                    {
                        Result += "--Default Value may require changing...\r\n\r\n";

                        Result += String.Format("UPDATE {0}\r\nSET {1} = {2}\r\nWHERE {1} IS NULL;\r\n\r\n",
                            NewObject.ObjectName, NewObject.MasterObject.ObjectParameter1, GetDefaultValue(NewObject.MasterObject));

                        Result += String.Format("UPDATE RDB$RELATION_FIELDS SET RDB$NULL_FLAG = 1 WHERE RDB$FIELD_NAME = '{0}' AND RDB$RELATION_NAME = '{1}';\r\n",
                            NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.ObjectName);
                    }
                }

                if (NewObject.Size != NewObject.MasterObject.Size && 
                    (NewObject.ObjectParameter2 == NewObject.MasterObject.ObjectParameter2) || 
                    (NewObject.ObjectParameter2 == "INTEGER" && NewObject.MasterObject.ObjectParameter2 == "BIGINT"))
                {
                    //ALTER TABLE WS_ANNOUNCEMENTS ALTER COLUMN SUBJECT TYPE Varchar(120);
                    if (NewObject.Size < NewObject.MasterObject.Size)
                    {
                        // does the object being changed also have a primary key??
                        DatabaseObject columnPrimaryKey = FindPrimaryKey(NewObject.ObjectName, NewObject.ObjectParameter1, ChildObjects);

                        if (columnPrimaryKey != null)
                        {
                            // drop the index
                            Result += "\r\n\r\n-- can not alter table with primary key so drop primary key\r\n\r\n";
                            Result += String.Format("ALTER TABLE {0} DROP CONSTRAINT {1};\r\n\r\n",
                                columnPrimaryKey.MasterObject.ObjectName, columnPrimaryKey.MasterObject.ObjectParameter1);
                        }

                        Result += String.Format("ALTER TABLE {0} ALTER COLUMN {1} TYPE {2};\r\n",
                            NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter1,
                            CreateColumnType(NewObject.MasterObject.ObjectParameter2, NewObject.MasterObject.Size / NewObject.MasterObject.BytesPerCharacter, NewObject.MasterObject.ObjectParameter3, NewObject.MasterObject.SubType));

                        if (columnPrimaryKey != null)
                        {
                            // recreate the index
                            Result += "\r\n\r\n-- recreate the primary key now column type has changed\r\n\r\n";

                            Result += String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2});\r\n",
                                columnPrimaryKey.ObjectName, columnPrimaryKey.MasterObject.ObjectParameter1, columnPrimaryKey.MasterObject.ObjectParameter2);
                        }
                    }
                    else
                        Result += "\r\n-- can not alter child column to make it smaller\r\n";
                }

                if (NewObject.ObjectParameter3 != NewObject.MasterObject.ObjectParameter3)
                {
                    Result += String.Format("ALTER TABLE {0} ALTER COLUMN {1} TYPE {2};\r\n",
                        NewObject.ObjectName, NewObject.ObjectParameter1,
                        CreateColumnType(NewObject.MasterObject.ObjectParameter2, NewObject.MasterObject.Size / NewObject.MasterObject.BytesPerCharacter, NewObject.MasterObject.ObjectParameter3, NewObject.MasterObject.SubType));
                }
            }
            else
            {
                if (String.IsNullOrEmpty(NewObject.ObjectParameter5))
                {
                    Result = String.Format("ALTER TABLE {0} ADD {1} {2} {3} {4};\r\n",
                        NewObject.ObjectName, NewObject.MasterObject.ObjectParameter1,
                        CreateColumnType(NewObject.MasterObject.ObjectParameter2, NewObject.MasterObject.Size / NewObject.MasterObject.BytesPerCharacter, NewObject.MasterObject.ObjectParameter3, NewObject.MasterObject.SubType),
                        String.IsNullOrEmpty(NewObject.MasterObject.DefaultValue) ? "" : String.Format("DEFAULT {0}", NewObject.MasterObject.DefaultValue),
                        NewObject.NotNull ? " NOT NULL" : "");
                }
                else
                {
                    Result = String.Format("ALTER TABLE {0} ADD {1} {2} {3};\r\n",
                        NewObject.ObjectName, NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.ObjectParameter5,
                        String.IsNullOrEmpty(NewObject.MasterObject.DefaultValue) ? "" : String.Format("DEFAULT {0}", NewObject.MasterObject.DefaultValue));
                }
            }

            return (Result);
        }

        private string CreateIndex()
        {
            string Result = String.Empty;

            /*
             * 
             * CREATE [UNIQUE] [ASC[ENDING] | [DESC[ENDING]] INDEX indexname
                   ON tablename
                   { (<col> [, <col> ...]) | COMPUTED BY (expression) }
             * 
             */
            if (NewObject.ExistsWithDifferentName)
            {
                Result = String.Format("DROP INDEX ;\r\n", NewObject.ObjectParameter1);

                Result += String.Format("CREATE {0}{4} INDEX {1} ON {2} ({3});\r\n",
                    NewObject.MasterObject.Unique ? "UNIQUE " : "", NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.ObjectName,
                    NewObject.MasterObject.ObjectParameter2, NewObject.MasterObject.ObjectParameter3);
            }
            else
            {
                Result += String.Format("CREATE {0}{4}{5}INDEX {1} ON {2} ({3});\r\n",
                    NewObject.MasterObject.Unique ? "UNIQUE" : "", 
                    NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.ObjectName,
                    NewObject.MasterObject.ObjectParameter2, String.IsNullOrEmpty(NewObject.MasterObject.ObjectParameter3) ?
                    "" : NewObject.MasterObject.ObjectParameter3 + " ",
                    NewObject.MasterObject.Unique && String.IsNullOrEmpty(NewObject.MasterObject.ObjectParameter3) ? " " : "");
            }

            return (Result);
        }

        private string CreateFunction()
        {
            string Result = String.Empty;

            /*
             * DECLARE EXTERNAL FUNCTION name [datatype | CSTRING (int) [, datatype | CSTRING (int) ...]]
                RETURNS {datatype [BY VALUE] | CSTRING (int)} [FREE_IT]
                ENTRY_POINT 'entryname'
                MODULE_NAME 'modulename';

             * 
             */

            Result = String.Format("DECLARE EXTERNAL FUNCTION {0}\r\n{1}\r\nRETURNS {2}\r\nENTRY_POINT '{3}'\r\nMODULE_NAME '{4}';",
                NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter4, NewObject.MasterObject.ObjectParameter3,
                NewObject.MasterObject.ObjectParameter2, NewObject.MasterObject.ObjectParameter1);

            return (Result);
        }

        private string CreateForeignKey()
        {
            string Result = String.Empty;
            /*
             * ALTER TABLE WS_INVOICE ADD CONSTRAINT FK_WS_INVOICE_USER_ID
                    FOREIGN KEY (USERID) REFERENCES WS_MEMBERS (ID) ON UPDATE CASCADE ON DELETE CASCADE;
             * 
             */
            if (NewObject.Status == ObjectStatus.DifferentSettings)
            {
                Result = String.Format("ALTER TABLE {0} DROP CONSTRAINT {1};\r\n\r\n",
                    NewObject.ObjectName, NewObject.MasterObject.ObjectParameter1);

                Result += String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2})\r\n  REFERENCES {3} ({4}) ON UPDATE {5} ON DELETE {6};",
                    NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.ObjectParameter2,
                    NewObject.MasterObject.ObjectParameter5, NewObject.MasterObject.DefaultValue, NewObject.MasterObject.ObjectParameter3,
                    NewObject.MasterObject.ObjectParameter4);

                Result = Result.Replace("ON UPDATE RESTRICT", "").Replace("ON DELETE RESTRICT", "");
            }
            else
            {
                if (NewObject.ExistsWithDifferentName)
                {
                    Result = String.Format("ALTER TABLE {0} DROP CONSTRAINT {1};\r\n\r\n",
                        NewObject.ObjectName, NewObject.DifferentName);

                    Result += String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2})\r\n  REFERENCES {3} ({4}) ON UPDATE {5} ON DELETE {6};",
                            NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.ObjectParameter2,
                            NewObject.MasterObject.ObjectParameter5, NewObject.MasterObject.DefaultValue, NewObject.MasterObject.ObjectParameter3,
                            NewObject.MasterObject.ObjectParameter4);
                }
                else
                    Result += String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2})\r\n  REFERENCES {3} ({4}) ON UPDATE {5} ON DELETE {6};",
                            NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.ObjectParameter2, NewObject.MasterObject.ObjectParameter5,
                            NewObject.MasterObject.DefaultValue, NewObject.MasterObject.ObjectParameter3, NewObject.MasterObject.ObjectParameter4);

                Result = Result.Replace("ON UPDATE RESTRICT", "").Replace("ON DELETE RESTRICT", "");
            }
            return (Result);
        }

        private string CreatePrimaryKey()
        {
            string Result = String.Empty;

            if (NewObject.Status == ObjectStatus.DifferentSettings)
            {
                Result = String.Format("ALTER TABLE {0} DROP CONSTRAINT {1};\r\n\r\n",
                    NewObject.ObjectName, NewObject.MasterObject.ObjectParameter1);

                Result += String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2});\r\n",
                    NewObject.ObjectName, NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.ObjectParameter2);
            }
            else
            {
                if (NewObject.ExistsWithDifferentName)
                {
                    Result = String.Format("ALTER TABLE {0} DROP CONSTRAINT {1};\r\n\r\n",
                        NewObject.ObjectName, NewObject.DifferentName);

                    Result += String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2});\r\n",
                        NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.ObjectParameter2);
                }
                else
                    Result += String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2});\r\n",
                        NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter1, NewObject.MasterObject.ObjectParameter2);
            }
            return (Result);
        }

        private string CreateView()
        {
            string Result = String.Format("CREATE OR ALTER VIEW {0}\r\n(\r\n", NewObject.ObjectName);
            string append = String.Empty;

            bool itemAdded = false;

            foreach (DatabaseObject dbObject in MasterObjects)
            {
                if (dbObject.ObjectName == NewObject.ObjectName)
                {
                    switch (dbObject.ObjectType)
                    {
                        case DatabaseObjectType.ViewColumn:
                            if (itemAdded)
                            {
                                Result += ",\r\n";
                            }
                            else
                            {
                                itemAdded = true;
                            }

                            Result += String.Format("    {0}", dbObject.ObjectParameter1);

                            break;

                    }
                }
            }

            if (NewObject.ObjectType == DatabaseObjectType.ViewColumn)
                Result = String.Format("{0}\r\n)\r\nAS\r\n{1}\r\n", Result, FindView(NewObject.ObjectName, MasterObjects).ObjectParameter5);
            else
                Result = String.Format("{0}\r\n)\r\nAS\r\n{1}\r\n", Result, NewObject.ObjectParameter5);

            return (Result);
        }

        private string CreateTrigger()
        {
            string Result = String.Empty;

            switch (NewObject.Hash)
            {
                case 8192://connect
                case 8193://disconnect
                case 8194://transaction start
                case 8195://transaction commit
                case 8196://transaction rollback
                    Result = String.Format("\r\nSET TERM ^ ;\r\n\r\nCREATE OR ALTER TRIGGER {0} ACTIVE ON {2} POSITION {3}\r\n {4}^\r\n\r\nSET TERM ; ^",
                        NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter1, GetTriggerType(NewObject.MasterObject.Hash), NewObject.MasterObject.Size, NewObject.MasterObject.ObjectParameter5);
                    break;

                default:
                    Result = String.Format("\r\nSET TERM ^ ;\r\n\r\nCREATE OR ALTER TRIGGER {0} FOR {1} ACTIVE {2} POSITION {3}\r\n {4}^\r\n\r\nSET TERM ; ^",
                        NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter1, GetTriggerType(NewObject.MasterObject.Hash), NewObject.MasterObject.Size, NewObject.MasterObject.ObjectParameter5);
                    break;
            }

            return (Result);
        }

        private string GetTriggerType(Int64 type)
        {
            string Result = String.Empty;

            switch (type)
            {
                case 1:
                    Result = "BEFORE INSERT";
                    break;
                case 3:
                    Result = "BEFORE UPDATE";
                    break;
                case 5:
                    Result  = "BEFORE DELETE";
                    break;
                case 17:
                    Result = "BEFORE INSERT OR UPDATE";
                    break;
                case 25:
                    Result = "BEFORE INSERT OR UPDATE";
                    break;
                case 27:
                    Result = "BEFORE UPDATE OR DELETE";
                    break;
                case 113:
                    Result = "BEFORE INSERT OR UPDATE OR DELETE";
                    break;
                case 2:
                    Result = "AFTER INSERT";
                    break;
                case 4:
                    Result = "AFTER UPDATE";
                    break;
                case 6:
                    Result = "AFTER DELETE";
                    break;
                case 18:
                    Result = "AFTER INSERT OR UPDATE";
                    break;
                case 26:
                    Result = "AFTER INSERT OR DELETE";
                    break;
                case 28:
                    Result = "AFTER UPDATE OR DELETE";
                    break;
                case 114:
                    Result = "AFTER INSERT OR UPDATE OR DELETE";
                    break;
                case 8192://connect
                    Result = "CONNECT";
                    break;
                case 8193://disconnect
                    Result = "DISCONNECT";
                    break;
                case 8194://transaction start
                    Result = "TRANSACTION START";
                    break;
                case 8195://transaction commit
                    Result = "TRASACTION COMMIT";
                    break;
                case 8196://transaction rollback
                    Result = "TRANSACTION ROLLBACK";
                    break;
                default:
                    throw new Exception("Unknown Trigger type");

                /*BI - 1 - 00 00 00 1
                * BU - 3 - 00 00 01 1
                * BD - 5 - 00 00 10 1
                * BIU - 17 - 00 10 00 1
                * BID - 25 - 00 11 00 1
                * BUD - 27 - 00 11 01 1
                * BIUD - 113 - 11 10 00 1
                * 
                * AI - 2 - 00 00 01 0
                * AU - 4 - 00 00 10 0
                * AD - 6 - 00 00 11 0
                * AIU - 18 - 00 10 01 0
                * AID - 26 - 00 11 01 0
                * AUD - 28 - 00 11 10 0
                * AIUD - 114 - 11 10 01 0                
                 */

            }

            return (Result);
        }

        private string CreateRole()
        {
            return (String.Format("CREATE ROLE {0};", NewObject.MasterObject.ObjectName));
        }

        private string CreateGenerator()
        {
            return (String.Format("\r\nCREATE GENERATOR {0};\r\n\r\n", NewObject.MasterObject.ObjectName));
        }

        private string CreateException()
        {
            return (String.Format("\r\nCREATE OR ALTER EXCEPTION {0} '{1}';\r\n\r\n", NewObject.MasterObject.ObjectName, NewObject.MasterObject.ObjectParameter1));
        }

        private string CreateDomain()
        {
            string Result = String.Empty;

            if (NewObject.Status == ObjectStatus.DifferentSettings)
            {
                Result += String.Format("ALTER DOMAIN {0} ", NewObject.ObjectName);

                if (NewObject.DefaultValue != NewObject.MasterObject.DefaultValue)
                {
                    if (String.IsNullOrEmpty(NewObject.MasterObject.DefaultValue))
                        Result += " DROP DEFAULT ";
                    else
                        Result += String.Format(" SET DEFAULT {0} ", NewObject.MasterObject.DefaultValue);
                }

                if (NewObject.ObjectParameter3 != NewObject.MasterObject.ObjectParameter3)
                {
                    if (String.IsNullOrEmpty(NewObject.MasterObject.ObjectParameter3))
                        Result += " DROP CONSTRAINT ";
                    else
                        Result += String.Format(" ADD CONSTRAINT {0} ", NewObject.MasterObject.ObjectParameter3);
                }

                if (NewObject.ObjectParameter1 != NewObject.MasterObject.ObjectParameter1)
                {
                    Result += " TYPE ";

                    switch (NewObject.MasterObject.ObjectParameter1)
                    {
                        case "VARCHAR":
                        case "CHAR":
                            Result += String.Format("({0}) CHARACTER SET {1}", NewObject.MasterObject.Size / NewObject.MasterObject.BytesPerCharacter, NewObject.MasterObject.ObjectParameter2);
                            break;

                        case "DOUBLE":
                            Result += " PRECISION ";
                            break;

                        case "BLOB":
                            Result += String.Format(" SUB_TYPE {0} ", NewObject.MasterObject.SubType);
                            break;
                    }
                }

                if (NewObject.NotNull != NewObject.MasterObject.NotNull)
                {
                    if (Result == String.Format("ALTER DOMAIN {0} ", NewObject.ObjectName))
                    {
                        Result = "\r\n--NOT NULL value is different, unable to alter domain!\r\n\r\n";
                    }
                }
            }
            else
            {
                Result += String.Format("CREATE DOMAIN {0} AS {1}", NewObject.ObjectName, NewObject.MasterObject.ObjectParameter1);

                switch (NewObject.MasterObject.ObjectParameter1)
                {
                    case "VARCHAR":
                    case "CHAR":
                        Result += String.Format("({0}) CHARACTER SET {1}", NewObject.MasterObject.Size / NewObject.MasterObject.BytesPerCharacter, NewObject.MasterObject.ObjectParameter2);
                        break;

                    case "DOUBLE":
                        Result += " PRECISION ";
                        break;

                    case "BLOB":
                        Result += String.Format(" SUB_TYPE {0} ", NewObject.MasterObject.SubType);
                        break;
                }

                if (!String.IsNullOrEmpty(NewObject.MasterObject.DefaultValue))
                    Result += String.Format("\r\n  DEFAULT {0} ", NewObject.MasterObject.DefaultValue);

                if (NewObject.MasterObject.NotNull)
                    Result += " NOT NULL ";

                if (!String.IsNullOrEmpty(NewObject.MasterObject.ObjectParameter3))
                    Result += String.Format(" {0} ", NewObject.MasterObject.ObjectParameter3);
            }

            Result += ";\r\n";

            return (Result);
        }

        #endregion DB Object Creation Methods

        #endregion Private Methods

        #region Properties

        /// <summary>
        /// Master list of objects to create ddl from
        /// </summary>
        internal DatabaseObjects MasterObjects { get; set; }

        /// <summary>
        /// Child list of objects from child database
        /// </summary>
        internal DatabaseObjects ChildObjects { get; set; }

        /// <summary>
        /// Object to create
        /// </summary>
        internal DatabaseObject NewObject { get; set; }
        
        /// <summary>
        /// Object to drop if exists
        /// </summary>
        internal DatabaseObject ExistingObject { get; set; }

        #endregion Properties
    }
}
