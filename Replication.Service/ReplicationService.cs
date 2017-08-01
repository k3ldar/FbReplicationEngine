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
 *  Purpose:  Replication Service
 *
 */

 using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Configuration.Install;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

#if USE_ERROR_MANAGER
using ErrorManager.ErrorClient;
#endif

using Shared;
using Shared.Classes;

namespace Replication.Service
{
    public class UpdateBackupReplication : Shared.BaseService
    {
        #region Private / Protected Members

        internal static UpdateBackupReplication INSTANCE;

        private static Dictionary<string, ReplicationThread> _replicationThreads = new Dictionary<string, ReplicationThread>();

        private static object _lockObject = new object();

        private FileSystemWatcher _watcher = null;

        internal const string ServiceInstallName = "Firebird Replication Engine";
        internal const string ServiceInstallDescription = "Firebird Database Replication Engine.  " +
            "Replication, Remote Update and Backup Service from Simon Carter " +
            "Required to Replicate databases on different servers, automated backups and remote DML and DDL updates.";

        private ContextMenuStrip pumNotifyIcon;
        private ToolStripMenuItem menuRunAsAppConfigure;
        private ToolStripMenuItem menuRunAsAppConsole;
        private ToolStripSeparator menuRunAsAppSeperator;
        private ToolStripMenuItem menuRunAsAppClose;
        private NotifyIcon notifyRunAsApp;
        private SharedControls.Controls.TextBlock versionMaster401;
        private SharedControls.Controls.TextBlock versionMaster402;
        private SharedControls.Controls.TextBlock versionMaster404;
        private System.ComponentModel.IContainer components;

        #endregion Private / Protected Members

        #region Designer


        #endregion Designer

        #region Constructors

        public UpdateBackupReplication()
        {
            // This call is required by the Windows.Forms Component Designer.
            InitializeComponent();
        }

        #endregion Constructors

        #region Private Run As Application Methods

        private void RunAsApp()
        {
            InitializeTCPServer();
            try
            {
                Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

                CreateReplicationThread();
                ThreadManager.ThreadCpuChanged += ThreadManager_ThreadCpuChanged;
                Shared.EventLog.Initialise(7);
                notifyRunAsApp.Visible = true;
                this.notifyRunAsApp.ContextMenuStrip = this.pumNotifyIcon;
                
                CreateConfigWatch();
                try
                {
                    Application.Run();
                }
                finally
                {
                    notifyRunAsApp.Visible = false;
                    RemoveConfigWatch();
                }
            }
            finally
            {
                this.TCPServerStop();
            }
        }

        #endregion Private Run As Application Methods

        #region Static Methods

        // The main entry point for the process
        static void Main(string[] args)
        {
            try
            {
                Parameters.Initialise(args, new char[] { '-', '/' }, new char[] { ' ', ':' });

                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");

                if (System.Environment.UserInteractive)
                {    
                    if (Parameters.OptionExists("decrypt"))
                    {
                        string decryptFile = Parameters.GetOption("file", String.Empty);
                        string decryptedFile = Parameters.GetOption("decryptedFile", String.Empty);

                        if (File.Exists(decryptFile))
                        {
                            string contents = Utilities.FileEncryptedRead(decryptFile, Forms.Configuration.ENCRYPRION_KEY);
                            Utilities.FileWrite(decryptedFile, contents);
                        }

                        return;
                    }

                    if (Parameters.OptionExists("encrypt"))
                    {
                        string decryptedFile = Parameters.GetOption("file", String.Empty);
                        string encryptedFile = Parameters.GetOption("encryptedFile", String.Empty);

                        if (File.Exists(decryptedFile))
                        {
                            string decryptedContents = Utilities.FileRead(decryptedFile, true);
                            Utilities.FileEncryptedWrite(encryptedFile, decryptedContents, Forms.Configuration.ENCRYPRION_KEY);
                        }

                        return;
                    }

                    Shared.Classes.ThreadManager.Initialise();
                    try
                    {                            
                        INSTANCE = new UpdateBackupReplication();

                        Shared.EventLog.Add("Initializing UserInteractive");

                        if (Parameters.OptionExists("c"))
                        {
                            Forms.Configuration config = new Forms.Configuration();
                            try
                            {
                                config.ShowDialog();
                            }
                            finally
                            {
                                config.Dispose();
                                config = null;
                            }
                        }
                        else if (Parameters.OptionExists("i")) // install
                        {
                            try
                            {
                                Shared.EventLog.Add("Installing");
                                ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                            }
                            catch (Exception err)
                            {
                                if (!err.Message.Contains("The installation failed, and the rollback has been performed"))
                                    throw;
                            }
                        }
                        else if (Parameters.OptionExists("u")) // uninstall
                        {
                            Shared.EventLog.Add("Uninstalling");
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                        }
                        else 
                        {
                            Shared.EventLog.Add("Run as Application");
                            Shared.Classes.ThreadManager.ThreadForcedToClose += ThreadManager_ThreadForcedToClose;
                            INSTANCE.RunAsApp();
                        }
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(String.Format("{0}\r\n{1}", err.Message, err.StackTrace.ToString()));
                        Shared.EventLog.Add(err.Message);
                        Shared.EventLog.Add(err);
                    }
                    finally
                    {
                        Shared.Classes.ThreadManager.Finalise();
                    }

                }
                else
                {
                    System.ServiceProcess.ServiceBase[] ServicesToRun;
                    INSTANCE = new UpdateBackupReplication();
                    ServicesToRun = new System.ServiceProcess.ServiceBase[] { INSTANCE };
                    System.ServiceProcess.ServiceBase.Run(ServicesToRun);
                }
            }
            catch (Exception error)
            {
                Shared.EventLog.Add(error);
            }
        }

        static void ThreadManager_ThreadForcedToClose(object sender, Shared.ThreadManagerEventArgs e)
        {
            Shared.EventLog.Add(String.Format("Thread forced to close: {0}, Unresponsive: {1}, Marked For Removal: {2}", 
                e.Thread.Name, e.Thread.UnResponsive.ToString(), e.Thread.MarkedForRemoval.ToString()));
            Shared.EventLog.Add(String.Format("Start Time: {0}", e.Thread.TimeStart.ToString("g")));
            Shared.EventLog.Add(String.Format("End Time: {0}", e.Thread.TimeFinish.ToString("g")));
        }

        
        void ThreadManager_ThreadCpuChanged(object sender, EventArgs e)
        {
            MessageSend(new Shared.Communication.Message("THREAD_CPU_CHANGED",
                ThreadManager.CpuUsage.ToString(), 
                Shared.Communication.MessageType.Broadcast), true);

            string Result = String.Empty;

            for (int i = 0; i < ThreadManager.ThreadCount; i++)
            {
                ThreadManager thread = ThreadManager.Get(i);

                Result += String.Format("{0}\r\n", thread.ToString());
            }

            MessageSend(new Shared.Communication.Message("THREAD_USAGE", Result, 
                Shared.Communication.MessageType.Broadcast), true);
        }

#if USE_ERROR_MANAGER
        private static void GetErrorClient_AdditionalInformation(object sender, AdditionalInformationEventArgs e)
        {
            try
            {
                //extra information sent to the error server
                if (String.IsNullOrEmpty(e.Information))
                {
                    e.Information = String.Format("Version: {0}\r\n", Application.ProductVersion.ToString());
                    e.Information += String.Format("Current Time Stamp: {0}\r\n", DateTime.Now.ToString("g"));

                    using (TimedLock.Lock(_lockObject))
                    {
                        foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                        {
                            e.Information += String.Format("Connection Thread {0}\r\n", kvp.Key);
                            e.Information += String.Format("Last Run: {0}\r\n", kvp.Value.LastRunReplication.ToString("g"));
                            e.Information += String.Format("Replication Error: {0}\r\n", kvp.Value.Result.ToString());
                            e.Information += String.Format("Can Replicate: {0}\r\n", kvp.Value.CanReplicate.ToString());
                            e.Information += String.Format("IsRunning: {0}\r\n", kvp.Value.IsRunning.ToString());
                            e.Information += String.Format("Allow Confirm Counts: {0}\r\n", kvp.Value.AllowConfirmCounts.ToString());
                        }
                    }

                    e.Information += String.Format("\r\n\r\nConfig File:\r\n\r\n{0}",
                        Shared.Utilities.FileRead(Shared.Utilities.CurrentPath(true) + "HSCConfig.xml", false));
                }
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
            }

        }
#endif

        #endregion Static Methods

        #region System Events

        void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            Shared.EventLog.Add("PowerModeChanged");
            switch (e.Mode)
            {
                case Microsoft.Win32.PowerModes.Suspend:
                    Shared.EventLog.Add("System Suspend, closing replication Threads");
                    
                    API api = new API(Utilities.AddTrailingBackSlash(Utilities.CurrentPath(true) + "Config"),
                        Forms.Configuration.ENCRYPRION_KEY);

                    foreach (ConfigFileNode config in api.GetConfigurationSettings())
                    {
                        ThreadManager.Cancel(String.Format("Replication Thread {0}", config.Connection.Name));
                    }

                    break;

                case Microsoft.Win32.PowerModes.Resume:
                    Shared.EventLog.Add("System Resumed, creating replication Threads");
                    CreateReplicationThread();
                    break;
            }
        }

        #endregion System Events

        #region Private Designer Methods

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateBackupReplication));
            this.pumNotifyIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuRunAsAppConfigure = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRunAsAppConsole = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRunAsAppSeperator = new System.Windows.Forms.ToolStripSeparator();
            this.menuRunAsAppClose = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyRunAsApp = new System.Windows.Forms.NotifyIcon(this.components);
            this.versionMaster401 = new SharedControls.Controls.TextBlock();
            this.versionMaster402 = new SharedControls.Controls.TextBlock();
            this.versionMaster404 = new SharedControls.Controls.TextBlock();
            this.pumNotifyIcon.SuspendLayout();
            // 
            // pumNotifyIcon
            // 
            this.pumNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuRunAsAppConfigure,
            this.menuRunAsAppConsole,
            this.menuRunAsAppSeperator,
            this.menuRunAsAppClose});
            this.pumNotifyIcon.Name = "pumNotifyIcon";
            this.pumNotifyIcon.Size = new System.Drawing.Size(158, 76);
            // 
            // menuRunAsAppConfigure
            // 
            this.menuRunAsAppConfigure.Name = "menuRunAsAppConfigure";
            this.menuRunAsAppConfigure.Size = new System.Drawing.Size(127, 22);
            this.menuRunAsAppConfigure.Text = "Configure";
            this.menuRunAsAppConfigure.Click += new System.EventHandler(this.menuRunAsAppConfigure_Click);
            // 
            // menuRunAsAppConsole
            // 
            this.menuRunAsAppConsole.Name = "menuRunAsAppConsole";
            this.menuRunAsAppConsole.Size = new System.Drawing.Size(157, 22);
            this.menuRunAsAppConsole.Text = "Service Console";
            this.menuRunAsAppConsole.Click += new System.EventHandler(this.menuRunAsAppConsole_Click);
            // 
            // menuRunAsAppSeperator
            // 
            this.menuRunAsAppSeperator.Name = "menuRunAsAppSeperator";
            this.menuRunAsAppSeperator.Size = new System.Drawing.Size(127, 22);
            // 
            // menuRunAsAppClose
            // 
            this.menuRunAsAppClose.Name = "menuRunAsAppClose";
            this.menuRunAsAppClose.Size = new System.Drawing.Size(127, 22);
            this.menuRunAsAppClose.Text = "Close";
            this.menuRunAsAppClose.Click += new System.EventHandler(this.menuRunAsAppClose_Click);
            // 
            // notifyRunAsApp
            // 
            this.notifyRunAsApp.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyRunAsApp.Icon")));
            this.notifyRunAsApp.Text = "Firebird Replication Engine";
            this.notifyRunAsApp.DoubleClick += new System.EventHandler(this.notifyRunAsApp_DoubleClick);
            // 
            // versionMaster401
            // 
            this.versionMaster401.StringBlock = resources.GetString("versionMaster401.StringBlock");
            // 
            // versionMaster402
            // 
            this.versionMaster402.StringBlock = resources.GetString("versionMaster402.StringBlock");
            // 
            // versionMaster404
            // 
            this.versionMaster404.StringBlock = resources.GetString("versionMaster404.StringBlock");
            // 
            // UpdateBackupReplication
            // 
            this.AutoLog = false;
            this.ServiceName = "Firebird Replication Engine";
            this.pumNotifyIcon.ResumeLayout(false);

        }

        #endregion Private Designer Methods

        #region Protected Overridden Service Methods

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
#if DEBUG
            System.GC.SuppressFinalize(this);
#endif
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Start this service
        /// </summary>
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            Parameters.Initialise(args, new char[] { '-', '/' }, new char[] { ' ', ':' });
            Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            Shared.EventLog.Initialise(7);
            ThreadManager.ThreadForcedToClose += ThreadManager_ThreadForcedToClose;
            ThreadManager.ThreadCpuChanged += ThreadManager_ThreadCpuChanged;
            CreateConfigWatch();
            Shared.EventLog.Add("OnStart");
            InitializeTCPServer();
            CreateReplicationThread();
        }

        /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
        {
            Shared.EventLog.Add("OnStop");
            RemoveConfigWatch();

            using (TimedLock.Lock(_lockObject))
            {
                foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                    kvp.Value.FinaliseReplication();
            }

            base.OnStop();
            Shared.Classes.ThreadManager.ThreadForcedToClose -= ThreadManager_ThreadForcedToClose;
        }

        protected override void OnContinue()
        {
            Shared.EventLog.Add("Continue");
        }

        #endregion Protected Overridden Service Methods

        #region Watch Config Files

        private void CreateConfigWatch()
        {
            string configFolder = Utilities.AddTrailingBackSlash(Utilities.CurrentPath(true) + "Config");

            // delete any existing temp files
            string[] tempFiles = Directory.GetFiles(configFolder, "tmp*.tmp");

            foreach (string file in tempFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }

            _watcher = new System.IO.FileSystemWatcher(configFolder);
            _watcher.Changed += watcher_Changed;
            _watcher.Deleted += watcher_Deleted;
            _watcher.Renamed += watcher_Renamed;
            _watcher.Created += watcher_Created;
            _watcher.EnableRaisingEvents = true;
            _watcher.IncludeSubdirectories = false;
        }

        private void RemoveConfigWatch()
        {

        }

        #region Watch Events

        private void watcher_Created(object sender, FileSystemEventArgs e)
        {
            string fileName = e.FullPath.ToLower();
        }

        private void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            string fileName = e.FullPath.ToLower();

        }

        private void watcher_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            string fileName = e.FullPath.ToLower();

            if (!File.Exists(fileName))
                return;

            if (Path.GetExtension(fileName).ToLower() == ".tmp")
            {
                FileInfo info = new FileInfo(fileName);

                if (info.Length == 0)
                    return;

                string[] parts = Shared.Utilities.FileRead(fileName, false).Split('@');

                if (parts.Length == 2)
                {
                    switch (parts[0])
                    {
                        case "NEW":
                            CreateReplicationThread();
                            break;

                        case "DELETE":
                        case "CHANGED":
                            using (TimedLock.Lock(_lockObject))
                            {
                                string keyName = String.Empty;

                                foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                                {
                                    if (kvp.Value.FileName.ToLower() == parts[1].ToLower())
                                    {
                                        ThreadManager.Cancel(kvp.Value.Name);
                                        keyName = kvp.Key;
                                    }
                                }

                                if (!String.IsNullOrEmpty(keyName))
                                    _replicationThreads.Remove(keyName);
                            }

                            CreateReplicationThread();
                            

                            break;
                    }
                }

                MessageSend(new Shared.Communication.Message("CONFIGURATION_CHANGED", String.Empty, Shared.Communication.MessageType.Info), true);
                File.Delete(fileName);
            }
        }

        #endregion Watch Events

        #endregion Watch Config Files

        #region Thread Creation

        private void CreateReplicationThread()
        {
            Shared.EventLog.Add("Create Replication Threads");
            try
            {
                using (TimedLock.Lock(_lockObject))
                {
                    API api = new API(Utilities.AddTrailingBackSlash(Utilities.CurrentPath(true) + "Config"),
                        Forms.Configuration.ENCRYPRION_KEY);
                    try
                    {
                        foreach (ConfigFileNode config in api.GetConfigurationSettings())
                        {
                            if (!config.Connection.Enabled)
                                continue;

                            try
                            {
                                string name = String.Format("Replication Thread {0}", config.Connection.Name);

                                if (_replicationThreads.ContainsKey(name))
                                    continue;

                                ReplicationThread replicationThread = new ReplicationThread(config.Connection, config.FileName);

                                replicationThread.ExceptionRaised += _ReplicationThread_ExceptionRaised;
                                replicationThread.ThreadFinishing += _ReplicationThread_ThreadFinishing;
                                replicationThread.ContinueIfGlobalException = true;

                                replicationThread.InitialiseReplication();

                                Shared.Classes.ThreadManager.ThreadHangTimeout = 20;
                                replicationThread.HangTimeout = Shared.Classes.ThreadManager.ThreadHangTimeout;
                                Shared.Classes.ThreadManager.ThreadStart(replicationThread,
                                    name, ThreadPriority.BelowNormal);

                                _replicationThreads.Add(name, replicationThread);
                                ThreadManager_ThreadCpuChanged(this, EventArgs.Empty);
                            }
                            catch (Exception error)
                            {
                                Shared.EventLog.Add(error);
                            }
                        }
                    }
                    finally
                    {
                        api = null;
                    }
                }
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err.Message);
                Shared.EventLog.Add(err);
            }
        }

        private void _ReplicationThread_ThreadFinishing(object sender, Shared.ThreadManagerEventArgs e)
        {
            if (e.Thread.Name.StartsWith("Replication Thread"))
            {
                Shared.EventLog.Add(String.Format("{0} Finishing; Unresponsive: {1}", 
                    e.Thread.Name, e.Thread.UnResponsive.ToString()));

                lock (_lockObject)
                {
                    if (_replicationThreads.ContainsKey(e.Thread.Name))
                    {
                        if (e.Thread.UnResponsive)
                        {
                            ThreadManager thread = (ThreadManager)_replicationThreads[e.Thread.Name];
                            _replicationThreads.Remove(e.Thread.Name);
                            thread.CancelThread();
                            Thread.Sleep(2000);
                            CreateReplicationThread();
                        }
                    }
                }
            }
        }

        private void _ReplicationThread_ExceptionRaised(object sender, Shared.ThreadManagerExceptionEventArgs e)
        {
            Shared.EventLog.Add(String.Format("Thread: {1}; Message: {0}", e.Error.Message, e.Thread.Name));
        }

        #endregion Thread Creation

        #region Message Server

        internal void SendMessage(Shared.Communication.Message msg)
        {
            base.MessageSend(msg, true);
        }

        private void InitializeTCPServer()
        {
#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            try
            {
                int port = Replication.Engine.Constants.DEFAULT_PORT;

                if (Shared.Classes.Parameters.OptionExists("port"))
                    port = Shared.Classes.Parameters.GetOption("port", port);

                MessageServerPort = port;
                MessageServerActive = true;
                base.InitializeTCPServer(port);
            }
            catch (Exception err)
            {
                if (err.Message.Contains("Only one usage of each socket"))
                {
                    Shared.Classes.ThreadManager.Cancel("MessageServer Connection Thread");
                }
            }
        }

        protected override void MessageReceived(object sender, Shared.Communication.Message message)
        {
            base.MessageReceived(sender, message);

#if DEBUG
            Shared.EventLog.Debug("RepThread " +System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shared.EventLog.Debug("RepThread " + message.Title + " " + message.Contents);
#endif

            if (message.Type == Shared.Communication.MessageType.Command)
            {
                string[] client = message.Title.Split('@');

                switch (client[0])
                {
                    case "REPLICATION_CLIENTS":
                        message.Contents = String.Empty;

                        foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                            message.Contents += String.Format("{0}#", kvp.Value.DatabaseConnection.Name);

                        MessageSend(message, false);

                        break;

                    case "THREAD_USAGE":
                        string Result = String.Empty;

                        for (int i = 0; i < ThreadManager.ThreadCount; i++)
                        {
                            ThreadManager thread = ThreadManager.Get(i);

                            Result += String.Format("{0}\r\n", thread.ToString());
                        }

                        message.Contents = Result;
                            
                        MessageSend(message, false);

                        break;

                    case "REPLICATE":
                        foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                        {
                            if (client.Length > 1 && kvp.Value.DatabaseConnection.Name == client[1])
                            {
                                kvp.Value.LastRunReplication = DateTime.Now.AddDays(-500);
                            }
                        }

                        break;


                    case "FORCE_BACKUP":
                        foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                        {
                            if (client.Length > 1 && kvp.Value.DatabaseConnection.Name == client[1])
                            {
                                kvp.Value.DatabaseConnection.LastBackupAttempt = DateTime.Now.AddDays(-100).ToFileTimeUtc();
                                kvp.Value.DatabaseConnection.LastBackup = DateTime.Now.AddDays(-100).ToFileTimeUtc();
                                message.Contents = "Database Listed for Backup";
                                MessageSend(message, false);
                                break;
                            }
                        }

                        break;

                    case "PREVENT":
                        foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                        {
                            if (client.Length > 1 && kvp.Value.DatabaseConnection.Name == client[1])
                            {
                                kvp.Value.LastRunReplication = DateTime.Now.AddDays(500);
                                break;
                            }
                        }

                        break;

                    case "REPLICATION_ENABLED":
                        foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                        {
                            if (client.Length > 1 && kvp.Value.DatabaseConnection.Name == client[1])
                            {
                                bool replicationEnabled = kvp.Value.DatabaseConnection.ReplicationType == Engine.ReplicationType.Child &&
                                    kvp.Value.DatabaseConnection.Enabled;
                                message.Contents = replicationEnabled.ToString();
                                message.Type = Shared.Communication.MessageType.Acknowledge;
                                MessageSend(message, false);
                                break;
                            }
                        }

                        break;


                    case "ALLOWCONFIRMCOUNTS":
                        //foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                        //    kvp.Value._allowConfirmCounts = Convert.ToBoolean(message.Contents);

                        break;

                    case "ISREPLICATING":
                        foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                        {
                            if (client.Length > 1 && kvp.Value.DatabaseConnection.Name == client[1])
                            {
                                message.Contents = kvp.Value.IsRunning.ToString();
                                message.Type = Shared.Communication.MessageType.Acknowledge;
                                MessageSend(message, false);
                            }
                        }

                        break;

                    case "FORCEHARDCOUNT":
                        foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                        {
                            if (client.Length > 1 && kvp.Value.DatabaseConnection.Name == client[1])
                            {
                                kvp.Value.ForceVerifyRecords = true;
                                message.Contents = kvp.Value.ForceVerifyRecords.ToString();
                                message.Type = Shared.Communication.MessageType.Acknowledge;
                                MessageSend(message, false);
                            }
                        }

                        break;

                    case "CANCELREPLICATION":
                        foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                        {
                            if (client.Length > 1 && kvp.Value.DatabaseConnection.Name == client[1])
                            {
                                kvp.Value.CancelReplication();
                            }
                        }

                        break;

                    case "VALIDATEDATABASES":
                        //if (_replicationEngine != null)
                        //{
                        //    _replicationEngine.Validate = true;
                        //}

                        break;

                    case "REPLICATION_RUNNING":
                        foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                        {
                            if (client.Length > 1 && kvp.Value.DatabaseConnection.Name == client[1])
                            {
                                message.Contents = kvp.Value.IsRunning.ToString();
                                message.Type = Shared.Communication.MessageType.Acknowledge;
                                MessageSend(message, false);
                            }
                        }

                        break;

                    case "REPLICATION_RUNTIME":
                        DateTime StartTime = DateTime.Now;

                        foreach (KeyValuePair<string, ReplicationThread> kvp in _replicationThreads)
                        {
                            if (client.Length > 1 && kvp.Value.DatabaseConnection.Name == client[1])
                            {
                                StartTime = kvp.Value.TimeStart;

                                message.Contents = StartTime.ToFileTimeUtc().ToString();
                                message.Type = Shared.Communication.MessageType.Acknowledge;
                                MessageSend(message, false);
                            }
                        }

                        break;
                }
            }
        }

        #endregion Message Server

        #region Popup Menu

        private void menuRunAsAppConsole_Click(object sender, EventArgs e)
        {
            ProcessStartInfo si = new ProcessStartInfo();
            si.FileName = Utilities.CurrentPath(true) + "Replication.Service.Console.exe";
            System.Diagnostics.Process.Start(si);
        }

        private void menuRunAsAppClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuRunAsAppConfigure_Click(object sender, EventArgs e)
        {
            ProcessStartInfo si = new ProcessStartInfo();
            si.Arguments = "/c";
            si.FileName = Application.ExecutablePath;
            System.Diagnostics.Process.Start(si);
        }

        private void notifyRunAsApp_DoubleClick(object sender, EventArgs e)
        {
            menuRunAsAppConfigure_Click(sender, e);
        }

        #endregion Popup Menu

        #region Version Data

        //internal static int INTERNAL_VERSION = 404;


        internal static string GetInternalVersion(int version, bool master)
        {
            if (master)
            {
                switch (version)
                {
                    case 401:
                        return (INSTANCE.versionMaster401.StringBlock);
                    case 402:
                        return (INSTANCE.versionMaster402.StringBlock);
                    case 403:
                        return (String.Empty);
                    case 404:
                        return (INSTANCE.versionMaster404.StringBlock);
                    case 405:
                        return (String.Empty);
                    default:
                        throw new Exception("Invalid Version Data");
                }
            }
            else
            {
                switch (version)
                {
                    case 401:
                    case 402:
                    case 403:
                    case 404:
                    case 405:
                        return (String.Empty);
                    default:
                        throw new Exception("Invalid Version Data");
                }
            }
        }

        #endregion Version Data
    }
}
