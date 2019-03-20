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
 *  Purpose:  Console Program File
 *
 */

 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Replication.Service.Console
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Shared.Classes.Parameters.Initialise(args, new char[] { '-', '/' }, new char[] { ' ', ':' });

                Shared.Classes.ThreadManager.ThreadAbortForced += ThreadManager_ThreadAbortForced;
                Shared.Classes.ThreadManager.ThreadCancellAll += ThreadManager_ThreadCancellAll;
                Shared.Classes.ThreadManager.ThreadCpuChanged += ThreadManager_ThreadCpuChanged;
                Shared.Classes.ThreadManager.ThreadExceptionRaised += ThreadManager_ThreadExceptionRaised;
                Shared.Classes.ThreadManager.ThreadForcedToClose += ThreadManager_ThreadForcedToClose;
                Shared.Classes.ThreadManager.ThreadStarted += ThreadManager_ThreadStarted;
                Shared.Classes.ThreadManager.ThreadStopped += ThreadManager_ThreadStopped;
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
            }

            Shared.Classes.ThreadManager.Initialise();
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ReplicationClient());
            }
            finally
            {
                Shared.Classes.ThreadManager.Finalise();
            }
        }

        static void ThreadManager_ThreadStopped(object sender, Shared.ThreadManagerEventArgs e)
        {
            Shared.EventLog.Add(String.Format("Thread Stopped - {0}", e.Thread.ToString()));
        }

        static void ThreadManager_ThreadStarted(object sender, Shared.ThreadManagerEventArgs e)
        {
            Shared.EventLog.Add(String.Format("Thread Start - {0}", e.Thread.ToString()));
        }

        static void ThreadManager_ThreadForcedToClose(object sender, Shared.ThreadManagerEventArgs e)
        {
            Shared.EventLog.Add(String.Format("Thread Forced To Close - {0}", e.Thread.ToString()));
        }

        static void ThreadManager_ThreadExceptionRaised(object sender, Shared.ThreadManagerExceptionEventArgs e)
        {
            Shared.EventLog.Add(String.Format("Thread Error - {0}", e.Thread.ToString()));
            Shared.EventLog.Add(e.Error);
        }

        static void ThreadManager_ThreadCpuChanged(object sender, EventArgs e)
        {
            //Shared.EventLog.Add(String.Format("Thread CPU Changed - {0}", e.Thread.ToString()));
        }

        static void ThreadManager_ThreadCancellAll(object sender, EventArgs e)
        {
            Shared.EventLog.Add("Thread Cancell All");
        }

        static void ThreadManager_ThreadAbortForced(object sender, Shared.ThreadManagerEventArgs e)
        {
            Shared.EventLog.Add(String.Format("Thread Abort Forced - {0}", e.Thread.ToString()));
        }
    }
}
