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
 *  Purpose:  Table Statuses Collection
 *
 */
using System;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

namespace Replication.Engine.Classes
{
    [Serializable]
    public class TableStatuses : CollectionBase
    {
        #region Static Methods

        public static TableStatuses Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TableStatuses));

                    StreamReader reader = new StreamReader(fileName);
                    try
                    {
                        return ((TableStatuses)serializer.Deserialize(reader));
                    }
                    finally
                    {
                        reader.Close();
                        reader.Dispose();
                        reader = null;
                    }
                }
                catch
                {
                    return (new TableStatuses());
                }
            }
            else
            {
                return (new TableStatuses());
            }
        }

        public static void Save(TableStatuses statuses, string fileName)
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            XmlSerializer serializer = new XmlSerializer(typeof(TableStatuses));

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, statuses);
            }
        }

        #endregion Static Methods

        #region Public Methods

        /// <summary>
        /// Resets the confirmed flag following successful confirmation
        /// </summary>
        public void Reset()
        {
            foreach (TableStatus status in this)
            {
                status.ConfirmedChild = false;
                status.ConfirmedMaster = false;
            }
        }

        /// <summary>
        /// Finds table data, if not found creates a new instance and adds it to the list
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public TableStatus Find(string tableName, bool ascending)
        {
            foreach (TableStatus status in this)
            {
                if (status.TableName == tableName)
                {
                    return (status);
                }
            }

            TableStatus Result = new TableStatus(tableName);
            this.Add(Result);
            Result.ChildRecord = ascending ? Int64.MinValue : Int64.MaxValue;
            Result.MasterRecord = Result.ChildRecord;

            return (Result);
        }

        #endregion Public Methods

        #region Generic CollectionBase Code

        #region Properties

        /// <summary>
        /// Indexer Property
        /// </summary>
        /// <param name="Index">Index of object to return</param>
        /// <returns>Video object</returns>
        public TableStatus this[int Index]
        {
            get
            {
                return ((TableStatus)this.InnerList[Index]);
            }

            set
            {
                this.InnerList[Index] = value;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Adds an item to the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Add(TableStatus value)
        {
            return (List.Add(value));
        }

        /// <summary>
        /// Returns the index of an item within the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(TableStatus value)
        {
            return (List.IndexOf(value));
        }

        /// <summary>
        /// Inserts an item into the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Insert(int index, TableStatus value)
        {
            List.Insert(index, value);
        }


        /// <summary>
        /// Removes an item from the collection
        /// </summary>
        /// <param name="value"></param>
        public void Remove(TableStatus value)
        {
            List.Remove(value);
        }


        /// <summary>
        /// Indicates the existence of an item within the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(TableStatus value)
        {
            // If value is not of type OBJECT_TYPE, this will return false.
            return (List.Contains(value));
        }

        #endregion Public Methods

        #region Private Members

        private const string OBJECT_TYPE = "Replication.Engine.Classes.TableStatus";
        private const string OBJECT_TYPE_ERROR = "Must be of type TableStatus";


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
}
