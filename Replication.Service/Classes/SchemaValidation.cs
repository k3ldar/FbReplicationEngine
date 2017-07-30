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
 *  Purpose:  Schema Validation Thread
 *
 */

 using System;

using Shared;

namespace Replication.Service
{
    public sealed class SchemaValidation : Shared.Classes.ThreadManager
    {
        #region Constructors

        public SchemaValidation(DatabaseConnection connection)
            : base (connection, new TimeSpan())
        {

        }

        #endregion Constructors

        #region Overridden Methods

        public override void CancelThread(int timeout = 10000, bool isUnResponsive = false)
        {
            DatabaseValidation.Cancel();
            base.CancelThread(timeout, isUnResponsive);
        }

        protected override bool Run(object parameters)
        {
            try
            {
                DatabaseConnection connection = (DatabaseConnection)parameters;

                DatabaseObjects dbObjects = DatabaseValidation.Validate(connection.MasterDatabase, connection.ChildDatabase, false, true);
                
                foreach (DatabaseObject dbObject in dbObjects)
                {
                    if (ValidationError != null)
                    {
                        if (dbObject.ObjectParameter1 == "REPLICATE$HASH")
                            continue;

                        ValidationError(this, new SchemaValidationArgs(dbObject.ObjectType.ToString(),
                            dbObject.ObjectName, dbObject.ObjectParameter1, dbObject.Status.ToString(),
                            dbObject.SQL, dbObject.ExistsWithDifferentName));
                    }
                }
            }
            catch
            {
                return (false);
            }
            finally
            {
                if (ValidationComplete != null)
                    ValidationComplete(this, EventArgs.Empty);
            }

            return (false);
        }

        #endregion Overridden Methods

        #region Events

        public event SchemaValidationHandler ValidationError;

        public event EventHandler ValidationComplete;

        #endregion Events
    }
}
