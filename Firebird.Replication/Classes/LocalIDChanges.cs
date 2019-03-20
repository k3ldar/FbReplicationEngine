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
 *  Purpose:  Collection of local ID Changes
 *
 */
using System;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

namespace Replication.Engine.Classes
{
    [Serializable]
    public class LocalIDChanges : CollectionBase
    {
        #region Static

        private static LocalIDChanges _localChanges;

        /// <summary>
        /// Returns a collection of LocalChanges
        /// </summary>
        public static LocalIDChanges LocalChanges
        {
            get
            {
                if (_localChanges == null)
                    _localChanges = LoadChanges();

                return (_localChanges);
            }
        }

        public static LocalIDChanges LoadChanges()
        {
            LocalIDChanges Result = null;

            String path = "ReplicationChanges.xml";

            if (File.Exists(path))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(LocalIDChanges));

                    StreamReader reader = new StreamReader(path);
                    Result = (LocalIDChanges)serializer.Deserialize(reader);
                    reader.Close();
                }
                catch
                {
                    Result = new LocalIDChanges();
                }
            }
            else
            {
                Result = new LocalIDChanges();
            }

            RemoveOldEntries(Result, 100);

            return (Result);
        }

        /// <summary>
        /// Saves user settings
        /// </summary>
        /// <param name="changes">changes to be saved</param>
        public static void Save(LocalIDChanges changes)
        {
            String path = "ReplicationChanges.xml";

            //if (!Directory.Exists(Path.GetDirectoryName(path)))
            //    Directory.CreateDirectory(Path.GetDirectoryName(path));

            var serializer = new XmlSerializer(typeof(LocalIDChanges));

            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, changes);
            }
        }

        /// <summary>
        /// Removes old entries from the list of changes
        /// </summary>
        /// <param name="changes">List of changes</param>
        /// <param name="AgeDays">Age in days of entries to remove</param>
        private static void RemoveOldEntries(LocalIDChanges changes, int AgeDays)
        {
            //remove any entries that are 1 week old
            for (int i = changes.Count -1; i >= 0; i--)
            {
                UpdateLocalID update = changes[i];

                TimeSpan span = DateTime.Now - update.CreateDate;

                if (span.Days > AgeDays)
                    changes.Remove(update);
            }

        }

        #endregion Static

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public LocalIDChanges()
        {
            LastReplication = DateTime.Now;
        }

        #endregion Constructors

        #region Public Properties

        public DateTime LastReplication { get; set; }

        public UpdateLocalID this[int Index]
        {
            get
            {
                return ((UpdateLocalID)List[Index]);
            }

            set
            {
                List[Index] = value;
            }
        }


        #endregion Public Properties

        #region Public Methods

        public UpdateLocalID Find(string tableName, string pkColumn, string oldID)
        {
            UpdateLocalID Result = null;

            foreach (UpdateLocalID item in this)
            {
                if (item.TableName == tableName && item.PKColumn == pkColumn && item.OldID == oldID)
                {
                    Result = item;
                    //break;
                }
            }

            return (Result);
        }

        public void Save()
        {
            LastReplication = DateTime.Now;
            LocalIDChanges.Save(this);
        }

        #endregion Public Methods

        #region Generic CollectionBase Code

        #region Public Methods

        /// <summary>
        /// Adds an item to the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Add(UpdateLocalID value)
        {
            return (List.Add(value));
        }

        /// <summary>
        /// Returns the index of an item within the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(UpdateLocalID value)
        {
            return (List.IndexOf(value));
        }

        /// <summary>
        /// Inserts an item into the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Insert(int index, UpdateLocalID value)
        {
            List.Insert(index, value);
        }


        /// <summary>
        /// Removes an item from the collection
        /// </summary>
        /// <param name="value"></param>
        public void Remove(UpdateLocalID value)
        {
            List.Remove(value);
        }


        /// <summary>
        /// Indicates the existence of an item within the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(UpdateLocalID value)
        {
            // If value is not of type OBJECT_TYPE, this will return false.
            return (List.Contains(value));
        }

        #endregion Public Methods

        #region Private Members

        private const string OBJECT_TYPE = "Replication.Engine.Classes.UpdateLocalID";
        private const string OBJECT_TYPE_ERROR = "Must be of type UpdateLocalID";


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

    [Serializable]
    public class UpdateLocalID
    {
        #region Constructors

        public UpdateLocalID()
        {
        }

        public UpdateLocalID(string table, string pk, string oldID, string newID)
        {
            TableName = table;
            PKColumn = pk;
            OldID = oldID;
            NewID = newID;
            CreateDate = DateTime.Now;
        }

        #endregion Constructors

        /// <summary>
        /// Date/Time entry created
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Name of table
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Primary Key Column
        /// </summary>
        public string PKColumn { get; set; }

        /// <summary>
        /// Old ID
        /// </summary>
        public string OldID { get; set; }

        /// <summary>
        /// New ID
        /// </summary>
        public string NewID { get; set; }
    }


}
