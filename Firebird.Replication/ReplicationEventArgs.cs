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
 *  Purpose:  Event Arguments
 *
 */
using System;

namespace Replication.Engine
{
    /// <summary>
    /// Event args for changing ID's
    /// </summary>
    public class IDUpdatedEventArgs
    {
        public IDUpdatedEventArgs(Int64 oldID, Int64 newID, string location)
        {
            OldID = oldID;
            NewID = newID;
            Location = location;
        }

        public Int64 OldID;
        public Int64 NewID;
        public String Location;
    }

    public delegate void ReplicationIDChangedEventHandler(object sender, IDUpdatedEventArgs e);

    public class PercentEventArgs
    {
        public PercentEventArgs(int Complete, string Message) { PercentComplete = Complete; ProgressMessage = Message; }
        public int PercentComplete { get; private set; } 
        public string ProgressMessage { get; private set; }
    }

    public delegate void ReplicationPercentEventArgs(object sender, PercentEventArgs e);

    public class SynchTextEventArgs
    {
        public SynchTextEventArgs(string ProgressText) { Text = ProgressText; }
        public string Text { get; private set; }
    }

    public delegate void ReplicationEventHandler(object sender);
    public delegate void ReplicationProgress(object sender, SynchTextEventArgs e);
    public delegate void ReplicationError(object sender, SynchTextEventArgs e);

    public class ReplicationCancelEventArgs
    {
        public ReplicationCancelEventArgs()
        {
            CancelReplication = false;
        }

        public bool CancelReplication { get; set; }
    }

    public delegate void ReplicationCancelHandler(object sender, ReplicationCancelEventArgs e);
}
