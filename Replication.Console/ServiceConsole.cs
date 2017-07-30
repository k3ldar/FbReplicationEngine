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
 *  Purpose:  Console Form
 *
 */

using System;
using System.Windows.Forms;

using Shared.Communication;

namespace Replication.Service.Console
{
    public partial class ReplicationClient : Form
    {
        private MessageClient _client;

        private bool _replicationRunning = false;
        private bool _replicationEnabled = true;

        private DateTime _replicationStartTime;

        public ReplicationClient()
        {
            InitializeComponent();

#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");


            _replicationStartTime = DateTime.Now.AddYears(-10);

            tsStatus.Text = "";
            tsMissingCount.Text = "";
            tabMain.TabPages.Remove(tabPageThreads);

            int port = Replication.Engine.Constants.DEFAULT_PORT;
            string server = Replication.Engine.Constants.DEFAULT_SERVER;

            if (Shared.Classes.Parameters.OptionExists("port"))
                port = Shared.Classes.Parameters.GetOption("port", port);

            if (Shared.Classes.Parameters.OptionExists("server"))
                server = Shared.Classes.Parameters.GetOption("server", server);

            _client = new MessageClient(server, port);
            _client.ClientIDChanged += _client_ClientIDChanged;
            _client.MessageReceived += _client_MessageReceived;
            _client.Connected += _client_Connected;
            _client.Disconnected += _client_Disconnected;
            _client.OnError += _client_OnError;

            if (!Shared.Classes.Parameters.OptionExists("noautostart"))
                button1_Click(this, EventArgs.Empty);
        }       

        void _client_OnError(object sender, Shared.Communication.ErrorEventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shared.EventLog.Debug(e.Error.Message);
#endif
            Shared.EventLog.Add(e.Error, "Console");

            e.Continue = true;
            try
            {
                if (this.Disposing)
                    return;

                if (this.InvokeRequired)
                {
                    Shared.Communication.ErrorEventHandler mreh = new Shared.Communication.ErrorEventHandler(_client_OnError);
                    this.Invoke(mreh, new object[] { sender, e });
                }
                else
                {
                    lstReplicationMessages.Items.Add(e.Error.Message);
                    int idx = lstReplicationMessages.Items.Add(e.Error.StackTrace.ToString());

                    if (cbAutoScroll.Checked && lstReplicationMessages.Items != null)
                        lstReplicationMessages.SelectedIndex = idx;
                }
            }
            catch (Exception err)
            {
                Shared.EventLog.Add(err);
            }
        }

        void _client_ClientIDChanged(object sender, EventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler(_client_ClientIDChanged));
            }
            else
            {
                this.Text = String.Format("Replication: Client ID {0}", _client.ClientID);

                _client.SendMessage(new Shared.Communication.Message("REPLICATION_CLIENTS", String.Empty, MessageType.Command));
                tabMain.TabPages.Add(tabPageThreads);
            }
        }

        void _client_MessageReceived(object sender, Shared.Communication.Message message)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);

            string msgAsString = new string(Shared.Communication.Message.MessageToStringArray(message));
            Shared.EventLog.Debug("Sync " + msgAsString);
#endif
            try
            {
                if (this.InvokeRequired)
                {
                    MessageReceivedEventHandler mreh = new MessageReceivedEventHandler(_client_MessageReceived);
                    this.Invoke(mreh, new object[] { sender, message });
                }
                else
                {
                    if (message.Title == "REPLICATION_CLIENTS")
                    {
                        cmbClient.Items.Clear();
                        cmbClient.Items.Add("All");
                        string[] clients = message.Contents.Split('#');

                        foreach (string client in clients)
                        {
                            if (String.IsNullOrEmpty(client.Trim()))
                                continue;

                            cmbClient.Items.Add(client);
                        }

                        if (cmbClient.Items.Count > 0)
                            cmbClient.SelectedIndex = 0;

                        return;
                    }
                    else if (message.Title.StartsWith("THREAD_USAGE"))
                    {
                        UpdateThreadData(message.Contents);
                        return;
                    }
                    else if (message.Title == "THREAD_CPU_CHANGED")
                    {
                        decimal value = Shared.Utilities.StrToDecimal(message.Contents, 0);
                        tsCPU.Text = String.Format("CPU {0}%", Math.Round(value, 2));
                        return;
                    }
                    else if (message.Title == "CONFIGURATION_CHANGED")
                    {
                        SendMessage(new Shared.Communication.Message("REPLICATION_CLIENTS", String.Empty, MessageType.Command));
                        SendMessage(new Shared.Communication.Message("REPLICATION_RUNNING", String.Empty, MessageType.Command));
                        SendMessage(new Shared.Communication.Message("REPLICATION_RUNTIME", String.Empty, MessageType.Command));
                        SendMessage(new Shared.Communication.Message("REPLICATION_ENABLED", String.Empty, MessageType.Command));
                        SendMessage(new Shared.Communication.Message("THREAD_USAGE", String.Empty, MessageType.Command));
                        return;
                    }

                    bool addMessage = true;
                    string[] clientHeader = message.Title.Split('@');

                    if (cmbClient.SelectedIndex > 0 && clientHeader[1] != (string)cmbClient.SelectedItem)
                        return;

                    if (clientHeader[0] == "REPLICATION_RUNNING")
                    {
                        _replicationRunning = message.Contents.ToUpper() == "TRUE";

                        if (_replicationRunning)
                            tsLabelTimeTillRun.Text = "Replicating   ";

                        addMessage = false;
                    }
                    else if (clientHeader[0] == "REPLICATION_ENABLED")
                    {
                        _replicationEnabled = message.Contents.ToUpper() == "TRUE";
                        btnCancelReplication.Enabled = _replicationEnabled;
                        btnForceReplication.Enabled = _replicationEnabled;
                        btnPreventReplication.Enabled = _replicationEnabled;
                        btnHardConfirm.Enabled = _replicationEnabled;

                        if (!_replicationEnabled)
                            tsLabelTimeTillRun.Text = "Replication Disabled";

                        addMessage = false;
                    }
                    else if (clientHeader[0] == "FORCEHARDCOUNT")
                    {
                        addMessage = false;
                    }
                    else if (clientHeader[0] == "REPLICATION_RUNTIME")
                    {
                        Int64 time = Shared.Utilities.StrToInt64(message.Contents, 0);
                        _replicationStartTime = DateTime.FromFileTimeUtc(time);
                        addMessage = false;
                    }
                    if (message.Contents.StartsWith("#STATUS#"))
                    {
                        if (message.Contents.Contains(";"))
                            tsStatus.Text = message.Contents.Substring(message.Contents.IndexOf(";") + 2);
                        else
                            tsStatus.Text = message.Contents.Replace("#STATUS#", "");
                    }
                    else if (message.Contents.StartsWith("#MISSING#"))
                    {
                        tsMissingCount.Text = String.Format("Missing Records: {0}   ", message.Contents.Replace("#MISSING#", ""));
                    }
                    else if (message.Contents.StartsWith("Sleeping, time until next run"))
                    {
                        if (_replicationEnabled)
                            tsLabelTimeTillRun.Text = String.Format("Next Run: {0}", message.Contents.Substring(30));
                        else
                            tsLabelTimeTillRun.Text = "Replication Disabled";

                        tsLabelTimeTillRun.Invalidate();
                    }
                    else if (message.Contents.StartsWith("Run Replication"))
                    {
                        _replicationRunning = true;
                        _replicationStartTime = DateTime.Now;

                        tsLabelTimeTillRun.Text = "Replicating   ";

                        int idx = lstReplicationMessages.Items.Add(message.Contents);
                        //lstReplicationMessages.SelectedIndex = idx;

                        if (cbAutoScroll.Checked)
                            lstReplicationMessages.SelectedIndex = idx;
                    }
                    else if (message.Contents.Contains("Replication End"))
                    {
                        _replicationRunning = false;
                        tsLabelTimeTillRun.Text = "";
                        tsMissingCount.Text = "";
                        tsStatus.Text = "";

                        int idx = lstReplicationMessages.Items.Add(message.Contents);
                        //lstReplicationMessages.SelectedIndex = idx;

                        if (cbAutoScroll.Checked)
                            lstReplicationMessages.SelectedIndex = idx;
                    }
                    else
                    {
                        if (addMessage)
                        {
                            int idx = lstReplicationMessages.Items.Add(message.Contents);
                            //lstReplicationMessages.SelectedIndex = idx;

                            if (cbAutoScroll.Checked)
                                lstReplicationMessages.SelectedIndex = idx;

                            System.Threading.Thread.Sleep(1);
                            Application.DoEvents();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                int idx = lstReplicationMessages.Items.Add(err.Message);
                lstReplicationMessages.SelectedIndex = idx;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            try
            {
                tsLabelTimeTillRun.Text = "Connecting";
                _client.StartListening();

                if (_client.IsRunning)
                {
                    tsLabelTimeTillRun.Text = "Connected";
                }
            }
            catch (Exception err)
            {
                int idx = lstReplicationMessages.Items.Add(err.Message);

                if (cbAutoScroll.Checked)
                    lstReplicationMessages.SelectedIndex = idx;
            }

        }

        void _client_Disconnected(object sender, EventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new EventHandler(_client_Disconnected),new object[] {sender, EventArgs.Empty});
                }
                else
                {
                    btnStart.Enabled = true;
                    btnStop.Enabled = false;
                    btnForceReplication.Enabled = false;
                    btnPreventReplication.Enabled = false;
                    btnHardConfirm.Enabled = false;
                    tsCPU.Text = String.Empty;
                    tabMain.TabPages.Remove(tabPageThreads);

                    int idx = lstReplicationMessages.Items.Add("Disconnected");
                    
                    if (cbAutoScroll.Checked)
                        lstReplicationMessages.SelectedIndex = idx;
                }
            }
            catch (Exception err)
            {
                int idx = lstReplicationMessages.Items.Add(err.Message);
                lstReplicationMessages.SelectedIndex = idx;
            }
        }

        void _client_Connected(object sender, EventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler(_client_Connected), new object[] { sender, EventArgs.Empty });
            }
            else
            {
                btnStop.Enabled = true;
                btnStart.Enabled = false;
                btnForceReplication.Enabled = true;
                btnPreventReplication.Enabled = true;
                btnHardConfirm.Enabled = true;
                int idx = lstReplicationMessages.Items.Add("Connected");

                if (cbAutoScroll.Checked)
                    lstReplicationMessages.SelectedIndex = idx;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            _client.StopListening();
            tsMissingCount.Text = String.Empty;
            tsLabelTimeTillRun.Text = "Not Connected";
            tsStatus.Text = String.Empty;
            
            _replicationRunning = false;
        }

        private void SynchClient_FormClosing(object sender, FormClosingEventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            Shared.Communication.Message msg = new Shared.Communication.Message("ALLOWCONFIRMCOUNTS", Convert.ToString(true), MessageType.Command);
            SendMessage(msg);

            if (_client != null && _client.IsRunning)
                _client.StopListening();
        }

        private void btnForceReplication_Click(object sender, EventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            Shared.Communication.Message msg = new Shared.Communication.Message("REPLICATE", "", MessageType.Command);
            SendMessage(msg);
        }

        private void btnPreventReplication_Click(object sender, EventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            Shared.Communication.Message msg = new Shared.Communication.Message("PREVENT", "", MessageType.Command);
            SendMessage(msg);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            lstReplicationMessages.Items.Clear();
        }

        private void btnHardConfirm_Click(object sender, EventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            Shared.Communication.Message msg = new Shared.Communication.Message("FORCEHARDCOUNT", "true", MessageType.Command);
            SendMessage(msg);
        }

        private void btnCancelReplication_Click(object sender, EventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            Shared.Communication.Message msg = new Shared.Communication.Message("CANCELREPLICATION", "true", MessageType.Command);
            SendMessage(msg);
        }

        private void tmrRuntime_Tick(object sender, EventArgs e)
        {
            if (_replicationRunning && _replicationStartTime.Year == DateTime.Now.Year)
            {
                TimeSpan span = DateTime.Now - _replicationStartTime;

                tsRunTime.Text = String.Format("Elapsed Time: {0}   ", span.ToString().Substring(0, 8));
            }
            else
            {
                tsRunTime.Text = String.Empty;
            }
        }

        private void SendMessage(Shared.Communication.Message message)
        {
            if (cmbClient.SelectedIndex > -1)
                message.Title = String.Format("{0}@{1}", message.Title, (string)cmbClient.SelectedItem);

            _client.SendMessage(message);
        }

        private void cmbClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendMessage(new Shared.Communication.Message("REPLICATION_RUNNING", String.Empty, MessageType.Command));
            SendMessage(new Shared.Communication.Message("REPLICATION_RUNTIME", String.Empty, MessageType.Command));
            SendMessage(new Shared.Communication.Message("REPLICATION_ENABLED", String.Empty, MessageType.Command));

            btnCancelReplication.Enabled = cmbClient.SelectedIndex > 0;
            btnForceReplication.Enabled = cmbClient.SelectedIndex > 0;
            btnHardConfirm.Enabled = cmbClient.SelectedIndex > 0;
            btnPreventReplication.Enabled = cmbClient.SelectedIndex > 0;

            tsStatus.Text = String.Empty;
            tsMissingCount.Text = String.Empty;
        }

        private void UpdateThreadData(string rawData)
        {
            string[] threads = rawData.Split('\r');
            lvThreads.BeginUpdate();
            try
            {
                for (int i = 0; i < lvThreads.Items.Count; i++)
                {
                    lvThreads.Items[i].Tag = 1;
                }

                foreach (string thread in threads)
                {
                    if (String.IsNullOrEmpty(thread))
                        continue;

                    string[] threadParts = thread.Split(';');

                    ListViewItem threadItem = FindThreadListViewItem(SplitText(threadParts[1], ':'));
                    threadItem.Tag = 0;

                    string cpu = SplitText(threadParts[0], ':');
                    threadItem.SubItems[1].Text = cpu.Substring(0, cpu.IndexOf("/"));
                    threadItem.SubItems[2].Text = cpu.Substring(cpu.IndexOf("/") + 1);
                    threadItem.SubItems[3].Text = SplitText(threadParts[2], ':');
                    threadItem.SubItems[4].Text = SplitText(threadParts[3], ':');
                    threadItem.SubItems[5].Text = SplitText(threadParts[4], ':');
                    threadItem.SubItems[6].Text = SplitText(threadParts[5], ':');
                }

                for (int i = lvThreads.Items.Count - 1; i > 0; i--)
                {
                    if (lvThreads.Items[i].Tag == null || (int)lvThreads.Items[i].Tag == 1)
                    {
                        lvThreads.Items.RemoveAt(i);
                    }
                }
            }
            finally
            {
                lvThreads.EndUpdate();
            }

        }

        private ListViewItem FindThreadListViewItem(string name)
        {
            foreach (ListViewItem item in lvThreads.Items)
            {
                if (item.Text == name)
                    return (item);
            }

            ListViewItem Result = new ListViewItem(name);
            Result.SubItems.Add(String.Empty);
            Result.SubItems.Add(String.Empty);
            Result.SubItems.Add(String.Empty);
            Result.SubItems.Add(String.Empty);
            Result.SubItems.Add(String.Empty);
            Result.SubItems.Add(String.Empty);
            lvThreads.Items.Add(Result);

            return (Result);
        }

        private string SplitText(string text, char splitText)
        {
            if (text.Contains(splitText.ToString()))
            {
                string result = text.Substring(text.IndexOf(splitText) + 1);
                return (result.Trim());
            }
            else
                return (text);
        }

        private void tsMissingCount_TextChanged(object sender, EventArgs e)
        {
            ToolStripStatusLabel label = (ToolStripStatusLabel)sender;

            if (String.IsNullOrEmpty(label.Text))
                label.BorderSides = ToolStripStatusLabelBorderSides.None;
            else
                label.BorderSides = ToolStripStatusLabelBorderSides.Right;
        }

        private void contextMenuThreadsRefresh_Click(object sender, EventArgs e)
        {
#if DEBUG
            Shared.EventLog.Debug("Sync " +System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            Shared.Communication.Message msg = new Shared.Communication.Message("THREAD_USAGE", "", MessageType.Command);

            SendMessage(msg);
        }

        private void btnBackupDatabase_Click(object sender, EventArgs e)
        {
            Shared.Communication.Message msg = new Shared.Communication.Message("FORCE_BACKUP", "", MessageType.Command);

            SendMessage(msg);
        }
    }
}
