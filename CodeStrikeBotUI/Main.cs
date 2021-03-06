﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Xml;
using System.ServiceModel;

namespace CodeStrikeBot
{
    public partial class Main : Form
    {
        Controller ctrl;

        System.Timers.Timer tmrTimeout, tmrSniffer;
        System.Diagnostics.Stopwatch tmrSupressAction, tmrAttackNotify, tmrHeartBeat, tmrLateSchedule;

        Bitmap bmpCheck;

        //delegate void bsPacketCallback(SharpPcap.RawCapture packet);
        List<Messages.Message> messages;

        bool IsViewPayload;
        Messages.Message currentMessage;

        public static Main CurrentForm { get; private set; }

        public Main()
        {
            InitializeComponent();

            typeDataGridViewComboBoxColumn.ValueType = typeof(ScheduleType);
            typeDataGridViewComboBoxColumn.DataSource = System.Enum.GetValues(typeof(ScheduleType));

            priorityGridViewComboBoxColumn.ValueType = typeof(AccountPriority);
            priorityGridViewComboBoxColumn.DataSource = System.Enum.GetValues(typeof(AccountPriority));

            CurrentForm = this;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //System.Threading.Thread.Sleep(20000);
            CheckForIllegalCrossThreadCalls = false;

            IsViewPayload = true;

            this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left + 5;
            this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - this.Height - 5;

            tmrTimeout = new System.Timers.Timer(3000);
            tmrTimeout.Enabled = true;
            tmrTimeout.Stop();

            tmrSniffer = new System.Timers.Timer(300000); //5min
            tmrSniffer.Enabled = true;
            tmrSniffer.Elapsed += new System.Timers.ElapsedEventHandler(ConnectionUnstable);

            tmrSupressAction = new System.Diagnostics.Stopwatch();
            tmrSupressAction.Start();

            tmrHeartBeat = new System.Diagnostics.Stopwatch();
            tmrHeartBeat.Start();

            tmrLateSchedule = new System.Diagnostics.Stopwatch();
            tmrLateSchedule.Start();

            ctrl = new Controller();

            cboEmulator1.BindingContext = new BindingContext();
            cboEmulator1.DataSource = ctrl.apps;
            cboEmulator1.DisplayMember = "ShortName";
            cboEmulator1.ValueMember = null;
            cboEmulator2.BindingContext = new BindingContext();
            cboEmulator2.DataSource = ctrl.apps;
            cboEmulator2.DisplayMember = "ShortName";
            cboEmulator2.ValueMember = null;
            cboEmulator3.BindingContext = new BindingContext();
            cboEmulator3.DataSource = ctrl.apps;
            cboEmulator3.DisplayMember = "ShortName";
            cboEmulator3.ValueMember = null;
            cboEmulator4.BindingContext = new BindingContext();
            cboEmulator4.DataSource = ctrl.apps;
            cboEmulator4.DisplayMember = "ShortName";
            cboEmulator4.ValueMember = null;
            cboAccountAppFilter.BindingContext = new BindingContext();
            cboAccountAppFilter.DataSource = ctrl.apps;
            cboAccountAppFilter.DisplayMember = "Name";
            cboAccountAppFilter.ValueMember = null;

            txtEmulator1.Text = ctrl.Database.Settings.Emulator1.ToString();
            txtEmulator2.Text = ctrl.Database.Settings.Emulator2.ToString();
            txtEmulator3.Text = ctrl.Database.Settings.Emulator3.ToString();
            txtEmulator4.Text = ctrl.Database.Settings.Emulator4.ToString();
            if (ctrl.sc[0] != null && ctrl.sc[0].Emulator != null)
            {
                cboEmulator1.SelectedItem = ctrl.sc[0].Emulator.App;
            }
            if (ctrl.sc[1] != null && ctrl.sc[1].Emulator != null)
            {
                cboEmulator2.SelectedItem = ctrl.sc[1].Emulator.App;
            }
            if (ctrl.sc[2] != null && ctrl.sc[2].Emulator != null)
            {
                cboEmulator3.SelectedItem = ctrl.sc[2].Emulator.App;
            }
            if (ctrl.sc[3] != null && ctrl.sc[3].Emulator != null)
            {
                cboEmulator4.SelectedItem = ctrl.sc[3].Emulator.App;
            }
            txtSlackURL.Text = ctrl.Database.Settings.SlackURL;
            txtPushoverAPI.Text = ctrl.Database.Settings.PushoverAPIKey;
            txtPushoverUser.Text = ctrl.Database.Settings.PushoverUserKey;

            //debug windowing info
            string infoText = String.Format("FrameBorder={0}", SystemInformation.FrameBorderSize);
            infoText += String.Format("\nResizeFrameVerticalBorderWidth={0}", System.Windows.SystemParameters.ResizeFrameVerticalBorderWidth);
            infoText += String.Format("\nResizeFrameHorizBorderHeight={0}", System.Windows.SystemParameters.ResizeFrameHorizontalBorderHeight);
            infoText += String.Format("\nFixedFrameVerticalBorderWidth={0}", System.Windows.SystemParameters.FixedFrameVerticalBorderWidth);
            infoText += String.Format("\nFixedFrameHorizBorderHeight={0}", System.Windows.SystemParameters.FixedFrameHorizontalBorderHeight);
            infoText += String.Format("\nWindowResizeBorderThickness={0}", System.Windows.SystemParameters.WindowResizeBorderThickness);
            infoText += String.Format("\nWindowCaptionHeight={0}", System.Windows.SystemParameters.WindowCaptionHeight);
            infoText += String.Format("\nBorderSize={0}", System.Windows.Forms.SystemInformation.BorderSize);
            infoText += String.Format("\nPrimaryScreen={0}", System.Windows.Forms.Screen.PrimaryScreen.Bounds);
            infoText += String.Format("\nWorkingArea={0}", System.Windows.Forms.Screen.PrimaryScreen.WorkingArea);
            infoText += "\nGetWindowWidth=";
            for (int s = 0; s < ctrl.sc.Length; s++)
            {
                if (ctrl.sc[s] != null && ctrl.sc[s].EmulatorProcess != null && !ctrl.sc[s].EmulatorProcess.HasExited)
                {
                    infoText += String.Format("{0}({1});", s, ctrl.GetWindowWidth(ctrl.sc[s].EmulatorProcess.MainWindowHandle));
                }
            }
            infoText += "\nWindowRect=";
            for (int s = 0; s < ctrl.sc.Length; s++)
            {
                if (ctrl.sc[s] != null && ctrl.sc[s].EmulatorProcess != null && !ctrl.sc[s].EmulatorProcess.HasExited)
                {
                    infoText += String.Format("{0}({1});", s, ctrl.sc[s].WindowRect);
                }
            }

            System.IO.Directory.CreateDirectory(String.Format("{0}\\debug", Controller.Instance.GetFullScreenshotDir().Replace("\\ss", "")));
            System.IO.File.WriteAllText(String.Format("{0}\\debug\\info.txt", Controller.Instance.GetFullScreenshotDir().Replace("\\ss", "")), infoText);

            using (Bitmap bmp = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds.X, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
                }

                bmp.Save(String.Format("{0}\\debug\\info.bmp", Controller.Instance.GetFullScreenshotDir().Replace("\\ss", "")), ImageFormat.Bmp);
            }

            bool restart = true;
            foreach (Screen s in ctrl.sc)
            {
                if (s != null && s.EmulatorProcess != null && !s.EmulatorProcess.HasExited)
                {
                    restart = false;
                    break;
                }
            }

            if (restart)
            {
                //System.Threading.Thread.Sleep(5000);
                //Program.RestartApp();
            }
            //Controller.Instance.SendNotification("Bot start", NotificationType.General);

            ReloadAccountList();

            if (ctrl.accounts.Count == 0)
            {
                ctrl.SendNotification("Bot is null", NotificationType.Offline);
            }

            foreach (Screen s in ctrl.sc)
            {
                if (s != null)
                {
                    if (s.Emulator != null && s.Emulator.Id == 0)
                    {
                        s.Emulator.Save();
                    }
                }
            }

            messages = new List<Messages.Message>();

            bmpCheck = new Bitmap(10, 10);
            picCheck.Image = bmpCheck;

            if (lstAccounts.Items.Count > 0)
            {
                lstAccounts.SelectedIndex = 0;
            }

            chkScheduler.Checked = true;
            chkTasks.Checked = true;

            bckScreenState.RunWorkerAsync();

            bckSniffer.RunWorkerAsync();

            bckKeepAlive.RunWorkerAsync();

            bckAutoActions.RunWorkerAsync();

            bckService.RunWorkerAsync();
        }

        private void ReloadAccountList()
        {
            if (ctrl.ActiveScreen != null && ctrl.ActiveScreen.Emulator != null)
            {
                cboAccountAppFilter.SelectedItem = ctrl.ActiveScreen.Emulator.App;
                lstAccounts.Items.Clear();
                bsAccount.Clear();

                foreach (DataObjects.Account a in ctrl.accounts)
                {
                    if (a.App.Id == ctrl.ActiveScreen.Emulator.App.Id)
                    {
                        lstAccounts.Items.Add(a);
                        bsAccount.Add(a);
                    }
                }
                bsAccount.Add(new DataObjects.Account(0, "", "", "", "", 0, AccountPriority.NoMonitor, 0, new DateTime(), new DateTime(), ctrl.ActiveScreen.Emulator.App));
            }
        }

        private void btnScreen_Click(object sender, EventArgs e)
        {
            if (ctrl.ActiveScreen != null && ctrl.ActiveScreen.EmulatorProcess != null)
            {
                System.Threading.Thread.Sleep(1000);
                ctrl.UpdateWindowInfo();

                ctrl.ActiveScreen.ScreenShot(txtScreen.Text);

                ctrl.ActiveScreen.CheckIfSSReady();
            }
        }

        private void btnSwitch_Click(object sender, EventArgs e)
        {
            if (ctrl.ActiveScreen != null && ctrl.ActiveScreen.EmulatorProcess != null)
            {
                ctrl.ActiveScreen.AbortRoutine(1000);
                ctrl.Logout();
                ctrl.StartApp();
                ctrl.Login((DataObjects.Account)lstAccounts.SelectedItem);
                ctrl.ActiveScreen.CreateRoutine("AutoActions");
                ctrl.ActiveScreen.Thread.Start();
            }
        }

        private void rdoWindow1_CheckedChanged(object sender, EventArgs e)
        {
            ChangeActiveWindow(0);
        }

        private void rdoWindow2_CheckedChanged(object sender, EventArgs e)
        {
            ChangeActiveWindow(1);
        }

        private void rdoWindow3_CheckedChanged(object sender, EventArgs e)
        {
            ChangeActiveWindow(2);
        }

        private void rdoWindow4_CheckedChanged(object sender, EventArgs e)
        {
            ChangeActiveWindow(3);
        }

        private void ChangeActiveWindow(int idx)
        {
            DataObjects.App app = ctrl.ActiveScreen.Emulator.App;

            ctrl.ActiveWindow = idx;

            if (app != ctrl.ActiveScreen.Emulator.App)
            {
                ReloadAccountList();
            }
        }

        private void tmrTimeout_Tick(object sender, EventArgs e)
        {
            //throw new TimeoutException();
            //Application.Exit();
        }

        private void btnResize_Click(object sender, EventArgs e)
        {
            ctrl.RefreshWindows();
            ctrl.UpdateWindowInfo();
        }

        private void SetScreenStateText(string text)
        {
            stsState.Text = text;
        }

        private Point GetCustomColorPoint()
        {
            return new Point(Int32.Parse(txtCustomX.Text), Int32.Parse(txtCustomY.Text));
        }

        private void bckHeartBeat_DoWork(object sender, DoWorkEventArgs e)
        {
            if (System.Threading.Thread.CurrentThread.Name == null)
            {
                System.Threading.Thread.CurrentThread.Name = "EnemySearchHeartBeat";
            }

            while (true)
            {
                if (ctrl.ActiveScreen != null && ctrl.ActiveScreen.EmulatorProcess != null)
                {
                    System.Threading.Thread.Sleep(5000);

                    //while (ctrl.IdleLevel > 0)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }

                    //Screen sc2 = new Screen("BlueStacks");
                    /*Screen sc2 = new Screen("Droid4X");
                    sc2.ActiveWindow = sc.ActiveWindow;

                    sc2.SendClick(375, 20, 300); //click exit fullscreen
                    sc2.SendClick(150, 610, 500); //click chat bar
                    sc2.SendClick(305, 20, 300); //click custom room

                    sc2.SendClick(133, 682, 800); //click chat text
                    sc2.SendKey("Just a moment");
                    System.Threading.Thread.Sleep(300);
                    sc2.SendClick(350, 682, 300); //click send

                    sc2.SendClick(133, 682, 800); //click chat text
                    sc2.SendKey("Just a moment");
                    System.Threading.Thread.Sleep(300);
                    sc2.SendClick(350, 682, 300); //click send

                    sc2.SendClick(375, 20, 300); //click exit chat
                    sc2.SendClick(375, 20); //click fullscreen

                    sc.Halt = false;*/

                    System.Threading.Thread.Sleep(60000 * 30);
                }
            }
        }

        private void ConnectionUnstable(object sender, System.Timers.ElapsedEventArgs e)
        {
            /*if (sc.ActiveProcess() != null)
            {
                sc.UpdateWindowInfo();
                sc.RefreshWindows();

                sc.GoToBaseOrMap();
                //sc.Logout();
                //sc.Login(
            }*/
            //TODO: Vacation mode
            ctrl.SendNotification("Bot offline", NotificationType.Offline);		
		
            foreach (Screen s in ctrl.sc)		
            {		
                ctrl.KillEmulator(s);		
            }		
		
            Program.RestartApp();
        }

        #region ScreenState
        private void bckScreenState_DoWork(object sender, DoWorkEventArgs e)
        {
            if (System.Threading.Thread.CurrentThread.Name == null)
            {
                System.Threading.Thread.CurrentThread.Name = "ScreenState";
            }

            while (!bckScreenState.CancellationPending)
            {
                ctrl.StartScreenState = DateTime.Now;
                ctrl.UpdateWindowInfo();

                string screens = "", screenLocal;
                for (int s = 0; s < ctrl.sc.Length; s++)
                {
                    screenLocal = String.Format("{0}: ", s);
                    if (ctrl.sc[s] != null && ctrl.sc[s].EmulatorProcess != null)
                    {
                        screenLocal += ctrl.sc[s];
                    }
                    screens += screenLocal + "\r\n";
                }
                txtScreenOrder.Text = screens;

                foreach (Screen s in ctrl.sc)
                {
                    if (s != null)
                    {
                        Controller.CaptureApplication(s);
                        ushort chksum = s.SuperBitmap.Checksum();
                        if (chksum != s.LastChecksum)
                        {
                            s.LastChecksum = chksum;
                            s.TimeSinceChecksumChanged = DateTime.Now;
                        }
                    }
                }

                bckScreenState.ReportProgress(0, ctrl.ActiveScreen);

                System.Threading.Thread.Sleep(200);
            }
        }

        private void bckScreenState_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                Screen s = (Screen)e.UserState;
                if (s.ScreenState != null)
                {
                    SetScreenStateText(String.Format("{0} {1}", s.TimeoutFactor.ToString(), s.ScreenState.ToString()));
                }

                Point p = Cursor.Position;
                Rect r = ctrl.ActiveScreen.WindowRect;

                p.X -= r.left;
                p.X -= s.WindowMarginL;
                p.Y -= r.top;
                p.Y -= s.WindowTitlebarH;

                int value;
                Int32.TryParse(txtBmpSize.Text, out value);

                UpdateBmpChk(s.SuperBitmap, p.X, p.Y, value);
            }
        }
        #endregion

        public void UpdateBmpChk(SuperBitmap bmp, int x, int y, int size)
        {
            if (x >= 0 && x < Controller.SCREEN_W && y >= 0 && y < Controller.SCREEN_H)
            {
                txtCustomX.Text = x.ToString();
                txtCustomY.Text = y.ToString();
            }
            else if (txtCustomX.Text == "" || txtCustomY.Text == "")
            {
                txtCustomX.Text = "0";
                txtCustomY.Text = "0";
            }

            if (size > 0)
            {
                bmpCheck.Dispose();

                bmpCheck = new Bitmap(size, size);
            }

            using (Graphics g = Graphics.FromImage(bmpCheck))
            {
                try
                {
                    g.DrawImage(bmp.Bitmap, 0, 0, new Rectangle(Int32.Parse(txtCustomX.Text), Int32.Parse(txtCustomY.Text), bmpCheck.Width, bmpCheck.Height), GraphicsUnit.Pixel);
                    if (picCheck.Image != null)
                    {
                        picCheck.Image.Dispose();
                    }
                    
                    picCheck.Image = bmpCheck;
                    //lblBmpChecksum.Text = bmpCheck.Checksum().ToString("X4");
                    lblBmpChecksum.Text = bmp.Checksum(Int32.Parse(txtCustomX.Text), Int32.Parse(txtCustomY.Text), bmpCheck.Width, bmpCheck.Height).ToString("X4");
                }
                catch (ArgumentException ex)
                {
                    bmpCheck = new Bitmap(1, 1);

                    //TODO:Log
                }
                catch (InvalidOperationException ex)
                {
                    bmpCheck = new Bitmap(1, 1);
                }
            }

            try
            {
                bsColorCustom.Clear();
                bsColorCustom.Add(bmp.GetPixel(Int32.Parse(txtCustomX.Text), Int32.Parse(txtCustomY.Text)));
            }
            catch (InvalidOperationException ex)
            {
                BotDatabase.InsertLog(0, String.Format("{0} {1}", ex.GetType(), ex.Message), ex.StackTrace, new byte[1] { 0x0 });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                BotDatabase.InsertLog(0, String.Format("{0} {1}", ex.GetType(), ex.Message), ex.StackTrace, new byte[1] { 0x0 });
            }
        }

        #region RegularTasks
        private void btnTasks_Click(object sender, EventArgs e)
        {
            ctrl.ActiveScreen.AbortRoutine(1000);
            ctrl.RegularTasks();
            ctrl.ActiveScreen.CreateRoutine("AutoActions");
            ctrl.ActiveScreen.Thread.Start();
        }

        private void bckRegularTasks_DoWork(object sender, DoWorkEventArgs e)
        {
            if (System.Threading.Thread.CurrentThread.Name == null)
            {
                System.Threading.Thread.CurrentThread.Name = "RegularTasks";

            }

            while (!bckRegularTasks.CancellationPending)
            {
                bckRegularTasks.ReportProgress(0);
                int i = 10;
                while (i > 0 && !bckRegularTasks.CancellationPending)
                {
                    System.Threading.Thread.Sleep(1000);
                    i--;
                }

                if (!bckKeepAlive.IsBusy)
                {
                    BotDatabase.InsertLog(0, "Keep Alive Fail:", "", new byte[1] { 0x0 });
                    bckKeepAlive.RunWorkerAsync();
                }

                if (!bckRegularTasks.CancellationPending)
                {
                    ctrl.StartTasks = DateTime.Now;
                    bckRegularTasks.ReportProgress(1);

                    foreach (Screen s in ctrl.sc)
                    {
                        s.AbortRoutine();

                        s.CreateRoutine("RegularTasks");
                        s.Thread.Start();
                    }

                    List<Screen> screens = new List<Screen>();

                    foreach (Screen s in ctrl.sc)
                    {
                        screens.Add(s);
                    }

                    while (screens.Count != 0)
                    {
                        foreach (Screen s in screens)
                        {
                            if (!s.Thread.IsAlive || DateTime.Now.Subtract(ctrl.StartTasks).TotalSeconds > 60)
                            {
                                screens.Remove(s);

                                if (s.Thread.Name == "RegularTasks")
                                {
                                    s.CreateRoutine("AutoActions");
                                    s.Thread.Start();
                                }
                                break;
                            }
                        }
                    }

                    foreach (Screen s in ctrl.sc)
                    {
                        if (s.ScreenState != null && s.ScreenState.CurrentArea == Area.Unknown)
                        {
                            System.Threading.Thread.Sleep((int)(1000 * s.TimeoutFactor));
                            Controller.CaptureApplication(s);

                            if (s.ScreenState != null && s.ScreenState.CurrentArea == Area.Unknown)
                            {
                                //TODO: Change to Kill and Reopen app instead of killing emulator
                                ctrl.RestartEmulator(s);
                            }
                        }
                    }
                }

                bckRegularTasks.ReportProgress(100);

                i = 60;
                while (i > 0 && !bckRegularTasks.CancellationPending)
                {
                    System.Threading.Thread.Sleep(1000);
                    i--;
                }
            }
        }

        private void bckRegularTasks_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100)
            {
                stsStatus.Text = "Idle";
                this.BackColor = Color.FromName("Control");
            }
            else if (e.ProgressPercentage == 1)
            {
                stsStatus.Text = "Tasks are executing";
                this.BackColor = Color.FromName("LightCoral");
            }
            else if (e.ProgressPercentage == 0)
            {
                stsStatus.Text = "Tasks will run shortly";
                this.BackColor = Color.FromName("Coral");
            }
        }

        private void chkTasks_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTasks.Checked)
            {
                if (bckRegularTasks.IsBusy && bckRegularTasks.CancellationPending)
                {
                    chkTasks.Checked = false;
                }
                else
                {
                    bckRegularTasks.RunWorkerAsync();
                }
            }
            else
            {
                bckRegularTasks.CancelAsync();
                stsStatus.Text = "";
                stsStatus.BackColor = Color.FromName("Control");
            }
        }
        #endregion

        #region Scheduler
        private void bckScheduler_DoWork(object sender, DoWorkEventArgs e)
        {
            if (System.Threading.Thread.CurrentThread.Name == null)
            {
                System.Threading.Thread.CurrentThread.Name = "Scheduler";
            }

            while (!bckScheduler.CancellationPending)
            {
                DataObjects.ScheduleTask task = ctrl.GetNextTask();
                
                int timeout = 5;

                if (task != null)
                {
                    bckScheduler.ReportProgress(0);

                    while (timeout > 0 && !bckScheduler.CancellationPending)
                    {
                        System.Threading.Thread.Sleep(1000);
                        timeout--;
                    }

                    if (!bckScheduler.CancellationPending)
                    {
                        bckScheduler.ReportProgress(1);

                        ctrl.StartScheduler = DateTime.Now;

                        Screen s = ctrl.GetNextWindow(task);

                        s.AbortRoutine();

                        s.CreateRoutine("ScheduledTask");
                        s.Thread.Start(task);

                        while (s.Thread.IsAlive && DateTime.Now.Subtract(ctrl.StartScheduler).TotalSeconds < 60) { }
                        
                        if (s.Thread.Name == "ScheduledTask")
                        {
                            s.CreateRoutine("AutoActions");
                            s.Thread.Start();
                        }
                    }

                    bckScheduler.ReportProgress(100);
                }

                timeout = 60;
                while (timeout > 0 && !bckScheduler.CancellationPending)
                {
                    System.Threading.Thread.Sleep(1000);
                    timeout--;
                }
            }
        }

        private void bckScheduler_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100)
            {
                stsStatus.Text = "Idle";
                this.BackColor = Color.FromName("Control");
            }
            else if (e.ProgressPercentage == 1)
            {
                stsStatus.Text = "Scheduler is executing";
                this.BackColor = Color.FromName("LightCoral");
            }
            else if (e.ProgressPercentage == 0)
            {
                stsStatus.Text = "Scheduler will run shortly";
                this.BackColor = Color.FromName("Coral");
            }
        }

        private void btnScheduler_Click(object sender, EventArgs e)
        {
            DataObjects.ScheduleTask task = ctrl.GetNextTask();
            if (task != null)
            {
                try
                {
                    ctrl.ExecuteTask(task);
                }
                catch (Exception ex) { }
            }
        }

        private void chkScheduler_CheckedChanged(object sender, EventArgs e)
        {
            if (chkScheduler.Checked)
            {
                if (bckScheduler.IsBusy && bckScheduler.CancellationPending)
                {
                    chkScheduler.Checked = false;
                }
                else
                {
                    tmrLateSchedule.Restart();
                    bckScheduler.RunWorkerAsync();
                }
            }
            else
            {
                bckScheduler.CancelAsync();
                stsStatus.Text = "";
                stsStatus.BackColor = Color.FromName("Control");
            }
        }

        private void btnScheduleRun_Click(object sender, EventArgs e)
        {
            DataObjects.ScheduleTask task = (DataObjects.ScheduleTask)bsScheduleTask.Current;

            if (task != null)
            {
                ctrl.ExecuteTask(task);
            }
        }

        private void gridSchedules_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            DataObjects.ScheduleTask task = (DataObjects.ScheduleTask)bsScheduleTask.Current;

            if (task.Interval > 0)
            {
                bool shouldSave = false;

                switch (task.Type)
                {
                    case ScheduleType.StoneTransfer:
                    case ScheduleType.OilTransfer:
                    case ScheduleType.IronTransfer:
                    case ScheduleType.FoodTransfer:
                    case ScheduleType.CoinTransfer:
                    case ScheduleType.FoodT2Transfer:
                    case ScheduleType.OilT2Transfer:
                    case ScheduleType.StoneT2Transfer:
                    case ScheduleType.IronT2Transfer:
                    case ScheduleType.CoinT2Transfer:
                        shouldSave = task.X > 0 && task.Y > 0 && task.Amount > 0 && task.Count > 0;
                        break;
                    case ScheduleType.Login:
                        shouldSave = true;
                        break;
                    case ScheduleType.Shield:
                    case ScheduleType.AntiScout:
                    case ScheduleType.ActivateVIP:
                        shouldSave = task.Amount > 0;
                        break;
                }

                if (shouldSave)
                {
                    task = task.Save();
                    tmrLateSchedule.Restart();
                }
            }
        }

        private void gridSchedules_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DialogResult response = MessageBox.Show("Are you sure you want to delete this schedule?", "Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if ((response == DialogResult.Yes))
            {
                DataObjects.ScheduleTask task = (DataObjects.ScheduleTask)bsScheduleTask.Current;
                if (task.Id > 0)
                {
                    task.Delete();
                }
            }
            else
            {
                e.Cancel = true;
            }
        }
        #endregion

        #region Service
        private void bckService_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                ServiceHost svch = new ServiceHost(typeof(Services.CodeBotService));
                svch.AddServiceEndpoint(typeof(Services.ICodeBotService), new NetTcpBinding(), "net.tcp://localhost:2633");
                //var svcep = new System.ServiceModel.Description.ServiceEndpoint(typeof(Services.ICodeBotService));//, new NetTcpBinding(), "net.tcp://localhost:2633");
                //svch.AddServiceEndpoint(svcep);
                //svch.OpenTimeout = new TimeSpan(0, 0, 3);
                svch.Open();

                while (!bckService.CancellationPending)
                {
                    System.Threading.Thread.Sleep(100);
                }

                svch.Close();
            }
        }
        #endregion

        #region Sniffer
        private void bckSniffer_DoWork(object sender, DoWorkEventArgs e)
        {
            if (System.Threading.Thread.CurrentThread.Name == null)
            {
                System.Threading.Thread.CurrentThread.Name = "PacketSniffer";
            }

            // Print SharpPcap version 
            //string ver = SharpPcap.Version.VersionString;
            //Console.WriteLine("SharpPcap {0}, Example1.IfList.cs", ver);

            // Retrieve the device list
            SharpPcap.CaptureDeviceList devices = SharpPcap.CaptureDeviceList.Instance;

            // If no devices were found print an error
            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine");
                return;
            }

            Console.WriteLine("\nThe following devices are available on this machine:");
            Console.WriteLine("----------------------------------------------------\n");

            SharpPcap.WinPcap.WinPcapDevice device = null;

            // Print out the available network devices
            foreach (SharpPcap.ICaptureDevice dev in devices)
            {
                if (dev is SharpPcap.WinPcap.WinPcapDevice)
                {
                    if (((SharpPcap.WinPcap.WinPcapDevice)dev).Interface.GatewayAddress != null)
                    {
                        device = (SharpPcap.WinPcap.WinPcapDevice)dev;
                        break;
                    }
                }
                //if (dev.int
                Console.WriteLine("{0}\n", dev.ToString());
            }

            if (device != null)
            {
                // Register our handler function to the
                // 'packet arrival' event
                //device.OnPacketArrival +=
                //new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);

                // Open the device for capturing
                int readTimeoutMilliseconds = 0;

                string filter = "";
                //filter = "host 104.254.132.31";

                //EpicWar Address
                foreach (System.Net.IPAddress address in System.Net.Dns.GetHostAddresses("live-api-wiso.epicwar-online.com"))
                {
                    if (filter == "")
                    {
                        filter = "host " + address.ToString();
                    }
                    else
                    {
                        filter += " or host " + address.ToString();
                    }
                }

                //DIFF EpicAction Address
                foreach (System.Net.IPAddress address in System.Net.Dns.GetHostAddresses("api-niso.epicaction-online.com"))
                {
                    if (filter == "")
                    {
                        filter = "host " + address.ToString();
                    }
                    else
                    {
                        filter += " or host " + address.ToString();
                    }
                }

                tmrSniffer.Start();

                while (!bckSniffer.CancellationPending)
                {
                    device.Open(SharpPcap.DeviceMode.Promiscuous, readTimeoutMilliseconds);
                    device.Filter = filter;

                    SharpPcap.RawCapture packet = null;

                    // Keep capture packets using GetNextPacket()
                    while (!bckSniffer.CancellationPending && (packet = device.GetNextPacket()) != null)
                    {
                        bckSniffer.ReportProgress(50, packet);
                        tmrSniffer.Stop();
                        tmrSniffer.Start();
                    }

                    device.Close();
                    //device.Capture();
                }
            }
        }

        private void bckSniffer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SharpPcap.RawCapture packet = (SharpPcap.RawCapture)e.UserState;

            this.ProcessPacket(packet);
        }

        private void ProcessPacket(SharpPcap.RawCapture packet)
        {
            // Prints the time and length of each received packet
            //DateTime time = packet.PcapHeader.Date;
            //int len = packet.PcapHeader.PacketLength;
            //Console.WriteLine("{0}:{1}:{2},{3} Len={4}",
            //time.Hour, time.Minute, time.Second,
            // time.Millisecond, len);

            PacketDotNet.TcpPacket tcpPacket = (PacketDotNet.TcpPacket)PacketDotNet.Packet.ParsePacket(packet.LinkLayerType, packet.Data).PayloadPacket.PayloadPacket;

            bool found = false;

            //TODO: Debug - remove
            if (tcpPacket.Fin)
            {
                found = found;
            }

            Messages.Message message = null;

            foreach (Messages.Message m in messages)
            {
                if (!m.Complete && m.Ack == tcpPacket.AcknowledgmentNumber)
                {
                    message = m;
                    found = true;
                    message.AddPacket(packet);
                    break;
                }
            }

            if (!found)
            {
                message = new Messages.Message(packet);

                if (!(message is Messages.EmptyMessage))
                {
                    messages.Add(message);
                }
            }

            if (message.Complete)
            {
                message = Messages.Message.Parse(message);

                if (!(message is Messages.EmptyMessage))
                {
                    found = false;

                    foreach (Messages.Message m in messages)
                    {
                        if (m.Complete && message.Id != null && message.Id != "" && m.Id == message.Id)
                        {
                            if (m.Length == message.Length)
                            {
                                found = true;
                                break;
                            }
                            else
                            {
                                found = found;
                                //BotDatabase.InsertLog(0, "Packet", "", message.PayloadData);
                            }
                        }
                    }

                    if (!found)
                    {
                        messages.Add(message);

                        if (message is Messages.Message)
                        {
                            bsPacket.Add(message);
                        }
                        else
                        {
                            BotDatabase.InsertLog(1, "Not a message", "", message.PayloadData);
                        }

                        if (message is Messages.WarRallyMessage)
                        {
                            Messages.WarRallyMessage warMessage = (Messages.WarRallyMessage)message;

                            Messages.Objects.Rally rally = ctrl.rallies.Where(r => r.RallyId == warMessage.war_id).FirstOrDefault();

                            if (rally == null)
                            {
                                rally = new Messages.Objects.Rally(warMessage);
                                ctrl.rallies.Add(rally);

                                if (warMessage.timer <= 900 && tmrSupressAction.ElapsedMilliseconds > 1000)
                                {
                                    if (warMessage.defender.AllianceTag == "(~InK)") //TODO: Move this hard-coded value to user-defined Settings
                                    {
                                        if (warMessage.timer == 60)
                                        {
                                            ctrl.SendNotification(String.Format("1min Rally on {0}{1} by {2}{3}", warMessage.defender.AllianceTag, warMessage.defender.UserName, warMessage.attacker.AllianceTag, warMessage.attacker.UserName), NotificationType.RallyDefense);
                                        }
                                        else
                                        {
                                            ctrl.SendNotification(String.Format("Rally on {0}{1} by {2}{3}", warMessage.defender.AllianceTag, warMessage.defender.UserName, warMessage.attacker.AllianceTag, warMessage.attacker.UserName), NotificationType.RallyDefense);
                                        }
                                    }
                                    else
                                    {
                                        //normal
                                        ctrl.SendNotification(String.Format("Rally call for {0}{1} by {2}{3}", warMessage.defender.AllianceTag, warMessage.defender.UserName, warMessage.attacker.AllianceTag, warMessage.attacker.UserName), NotificationType.RallyOffense);
                                    }

                                    tmrSupressAction.Restart();
                                }
                            }
                            else
                            {
                                rally.Update(warMessage);
                            }

                            ctrl.marches.Sort();
                            
                        }
                        else if (message is Messages.MarchMessage)
                        {
                            Messages.MarchMessage marchMessage = (Messages.MarchMessage)message;

                            Messages.Objects.March march = ctrl.marches.Where(m => m.MarchId == marchMessage.march_id).FirstOrDefault();

                            if (march == null)
                            {
                                march = new Messages.Objects.March(marchMessage);
                                ctrl.marches.Add(march);

                                //output info on KOS players
                                try
                                {
                                    if (march.State == Messages.Objects.March.MarchState.Advancing && march.FromName.Substring(march.FromName.IndexOf(") ") + 1).Trim() == "ShaeKitty")
                                    {
                                        TimeSpan duration = march.EndTime.Subtract(march.StartTime);
                                        double distance = Math.Sqrt(Utilities.DistanceSquared(march.FromCoordinate, march.DestCoordinate));

                                        double speed = (distance * 1000) / duration.TotalMilliseconds;

                                        System.IO.Directory.CreateDirectory(String.Format(".\\output\\debug\\kos"));
                                        System.IO.File.WriteAllText(String.Format(".\\output\\debug\\kos\\{0}.txt", marchMessage.Id), String.Format("Tile {0}:{1}\nSpeed = {2}\nDuration = {3}\nDistance = {4}\nStart: {5}\nEnd: {6}", march.DestCoordinate.X, march.DestCoordinate.Y, speed, duration.TotalSeconds, distance, march.StartTime.ToLongTimeString(), march.EndTime.ToLongTimeString()));
                                    }
                                }
                                catch (NullReferenceException ex)
                                {
                                    System.IO.Directory.CreateDirectory(String.Format(".\\output\\debug\\marches"));
                                    System.IO.File.WriteAllText(String.Format(".\\output\\debug\\marches\\NULL-{0}.txt", marchMessage.Id), marchMessage.RawJson);
                                }

                                //notify attacks on specific users
                                if (march.Type == Messages.Objects.March.MarchType.Attack && march.State != Messages.Objects.March.MarchState.Returning)
                                {
                                    string playerName = "";

                                    try
                                    {
                                        playerName = march.DestName.Substring(march.DestName.IndexOf(") ") + 1).Trim();
                                    }
                                    catch (NullReferenceException ex)
                                    {
                                        //ctrl.SendNotification(String.Format("Null Crash: {0}x{1}->{2}x{3}", march.FromCoordinate.X, march.FromCoordinate.Y, march.DestCoordinate.X, march.DestCoordinate.Y), NotificationType.Crash);
                                    }

                                    DataObjects.Account a;
                                    bool success;
                                    
                                    //TODO: Make this a checkbox parameter on Account record
                                    switch (playerName)
                                    {
                                        case "codemann8":
                                        case "1TroopWonder":
                                            ctrl.SendNotification(String.Format("Attack on {0}", march.DestName), NotificationType.IncomingAttack);

                                            /*if (march.Watchtower != null && march.Watchtower.ActualTotalUnits != 0 && march.Watchtower.ActualTotalUnits < 15000)
                                            {
                                                ctrl.SendNotification(String.Format("Attack on {0}", march.DestName), NotificationType.IncomingAttack);
                                            }
                                            else
                                            {
                                                a = ctrl.FindAccount(playerName);

                                                success = false;

                                                if (a != null)
                                                {
                                                    Screen s = ctrl.GetNextWindow(a);

                                                    if (s != null)
                                                    {
                                                        s.AbortRoutine();
                                                        while (s.Emulator.LastKnownAccount == null || s.Emulator.LastKnownAccount.Id != a.Id)
                                                        {
                                                            ctrl.Logout(s);
                                                            ctrl.StartApp(s);
                                                            ctrl.Login(s, a);
                                                        }
                                                        if (s.Emulator.LastKnownAccount != null && s.Emulator.LastKnownAccount.Id == a.Id)
                                                        {
                                                            //success = s.ActivateBoost(ScheduleType.Shield, 3); //DIFF MS
                                                            success = s.ActivateBoost(ScheduleType.Shield, 8);
                                                        }
                                                        s.CreateRoutine("AutoActions");
                                                        s.Thread.Start();
                                                    }
                                                }

                                                if (success)
                                                {
                                                    ctrl.SendNotification(String.Format("Attack on {0}", march.DestName), NotificationType.IncomingAttack);
                                                }
                                                else
                                                {
                                                    ctrl.SendNotification(String.Format("Failed to activate {0} on {1}", ScheduleType.Shield.ToString(), a.ToString()), NotificationType.BoostActivationFail);
                                                }
                                            }*/
                                            break;
                                        case "codelady8":
                                        case "codegirl8":
                                        case "codeboy8":
                                        case "codepuppy8":
                                            ctrl.SendNotification(String.Format("Attack on {0}", march.DestName), NotificationType.IncomingAttack);

                                            a = ctrl.FindAccount(playerName);

                                            success = false;

                                            if (a != null)
                                            {
                                                Screen s = ctrl.GetNextWindow(a);

                                                if (s != null)
                                                {
                                                    s.AbortRoutine();
                                                    while (s.Emulator.LastKnownAccount == null || s.Emulator.LastKnownAccount.Id != a.Id)
                                                    {
                                                        ctrl.Logout(s);
                                                        ctrl.StartApp(s);
                                                        ctrl.Login(s, a);
                                                    }
                                                    if (s.Emulator.LastKnownAccount != null && s.Emulator.LastKnownAccount.Id == a.Id)
                                                    {
                                                        s.AttackEnemy(163, 185, 220, 346, RallyDelay.None, true);
                                                    }
                                                    s.CreateRoutine("AutoActions");
                                                    s.Thread.Start();
                                                }
                                            }

                                            if (!success)
                                            {
                                                ctrl.SendNotification(String.Format("Auto march failed, attack on {0}", march.DestName), NotificationType.IncomingAttack);
                                            }
                                            break;
                                    }
                                }
                                else if (march.Type == Messages.Objects.March.MarchType.Scout && march.State != Messages.Objects.March.MarchState.Returning)
                                {
                                    string playerName = march.DestName.Substring(march.DestName.IndexOf(") ") + 1).Trim();

                                    DataObjects.Account a;
                                    bool success;

                                    //TODO: Make this a checkbox parameter on Account record
                                    switch (playerName)
                                    {
                                        case "codemann8":
                                        case "1TroopWonder":
                                            playerName = "codemann8";
                                            a = ctrl.FindAccount(playerName);

                                            success = false;

                                            if (a != null)
                                            {
                                                Screen s = ctrl.GetNextWindow(a);

                                                if (s != null)
                                                {
                                                    s.AbortRoutine();
                                                    /*while (s.Emulator.LastKnownAccount == null || s.Emulator.LastKnownAccount.Id != a.Id)
                                                    {
                                                        ctrl.Logout(s);
                                                        ctrl.StartApp(s);
                                                        ctrl.Login(s, a);
                                                    }*/
                                                    if (s.Emulator.LastKnownAccount != null && s.Emulator.LastKnownAccount.Id == a.Id)
                                                    {
                                                        //success = s.ActivateBoost(ScheduleType.Shield, 3); //DIFF MS
                                                        //success = s.ActivateBoost(ScheduleType.AntiScout, 24, false);
                                                    }
                                                    s.CreateRoutine("AutoActions");
                                                    s.Thread.Start();

                                                    if (!success)
                                                    {
                                                        ctrl.SendNotification(String.Format("Scout on {0}", march.DestName), NotificationType.IncomingAttack);
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                march.Update(marchMessage);
                            }

                            if (march.State == Messages.Objects.March.MarchState.Ended)
                            {
                                ctrl.marches.Remove(march);
                                ctrl.endedMarches.Add(march);
                            }

                            foreach (Messages.Objects.March m in ctrl.marches.Where(m => m.EndTime <= DateTime.Now.ToUniversalTime()).ToList())
                            {
                                ctrl.marches.Remove(m);
                                ctrl.endedMarches.Add(m);
                            }

                            ctrl.marches.Sort();

                            lstMarches.Items.Clear();
                            lstMarches.Items.AddRange(ctrl.marches.ToArray());
                        }
                        else if (message is Messages.TileUpdatedMessage)
                        {
                            Messages.TileUpdatedMessage tileMessage = (Messages.TileUpdatedMessage)message;

                            foreach (Messages.TileUpdatedMessage.Chunk c in tileMessage.chunks)
                            {
                                foreach (Messages.TileUpdatedMessage.Tile t in c.tiles)
                                {
                                    try
                                    {
                                        System.Web.UI.DataVisualization.Charting.Point3D coord = Utilities.ProvinceChunkTile2Point3D(c.p_id, c.c_id, t.tile_id);

                                        Messages.Objects.Tile tile = ctrl.tiles.Where(tl => tl.Coordinate.X == coord.X && tl.Coordinate.Y == coord.Y && tl.Coordinate.Z == coord.Z).FirstOrDefault();

                                        if (tile == null)
                                        {
                                            tile = new Messages.Objects.Tile(t, coord, tileMessage);
                                            ctrl.tiles.Add(tile);
                                        }
                                        else
                                        {
                                            tile.Update(tileMessage);
                                        }

                                        /*foreach (Messages.Objects.Tile t in ctrl.tiles.Where(tl => tl.RCustomExpireTime <= DateTime.Now.ToUniversalTime()).ToList())
                                        {
                                            ctrl.tiles.Remove(t);
                                        }*/
                                    }
                                    catch (NullReferenceException ex)
                                    {
                                        //error
                                    }
                                }
                            }

                            //ctrl.tiles.Sort();

                            lstTiles.Items.Clear();
                            lstTiles.Items.AddRange(ctrl.tiles.ToArray());
                        }
                        else if (message is Messages.SyncedDataMessage)
                        {
                            Messages.SyncedDataMessage syncedDataMessage = (Messages.SyncedDataMessage)message;

                            foreach (Messages.Objects.Watchtower w in syncedDataMessage.Watchtowers)
                            {
                                Messages.Objects.March march = ctrl.marches.Where(m => m.MarchId == w.march_id).FirstOrDefault();

                                if (march == null)
                                {
                                    //TODO: error, should not be a watchtower without a march
                                }
                                else
                                {
                                    march.Update(w);
                                }
                            }
                        }
                        else if (message is Messages.ChatMessage)
                        {
                            Messages.ChatMessage chatMessage = (Messages.ChatMessage)message;

                            if (chatMessage.Message.StartsWith("codebot") && tmrSupressAction.ElapsedMilliseconds > 5000)
                            {
                                tmrSupressAction.Restart();

                                string command = chatMessage.Message.Substring(7).Trim();

                                if (command.StartsWith("restart"))
                                {
                                    Program.RestartApp();
                                }
                                else if (command.StartsWith("status"))
                                {
                                    //while (ctrl.IdleLevel > 0)
                                    {
                                        //System.Threading.Thread.Sleep(1000);
                                    }

                                    command = command.Substring(6).Trim();

                                    if (command == "chat")
                                    {
                                        Screen s = ctrl.GetFirstAbleWindow();
                                        if (s != null)
                                        {
                                            s.AbortRoutine();
                                            s.SendChat(ctrl.GetStatusMessage(), 1);
                                            s.CreateRoutine("AutoActions");
                                            s.Thread.Start();
                                        }
                                    }
                                    else
                                    {
                                        ctrl.SendNotification(ctrl.GetStatusMessage(), NotificationType.Status);
                                    }
                                }
                                else if (command.StartsWith("rss"))
                                {
                                    //while (ctrl.IdleLevel > 0)
                                    {
                                        //System.Threading.Thread.Sleep(1000);
                                    }

                                    command = command.Substring(4);

                                    int window = Int32.Parse(command.Substring(0, 1)) - 1;

                                    command = command.Substring(2);

                                    int x = Int32.Parse(command.Substring(0, command.IndexOf(' ')));

                                    command = command.Substring(command.IndexOf(' ') + 1);

                                    int y = Int32.Parse(command.Substring(0, command.IndexOf(' ')));

                                    command = command.Substring(command.IndexOf(' ') + 1);

                                    int type = Int32.Parse(command.Substring(0, 1));

                                    command = command.Substring(2);

                                    int deployments = Int32.Parse(command);

                                    /*ctrl.semaphore.WaitOne();

                                    ctrl..ResourceTransferx, y, type, deployments);

                                    ctrl.semaphore.Release();*/
                                }
                                else if (command.StartsWith("login"))
                                {
                                    //while (ctrl.IdleLevel > 0)
                                    {
                                        //System.Threading.Thread.Sleep(1000);
                                    }

                                    command = command.Substring(6);
                                    command = command.Trim();

                                    DataObjects.Account acc = null;
                                    Screen s = null;

                                    if (command.IndexOf(' ') > -1)
                                    {
                                        acc = ctrl.FindAccount(command.Substring(0, command.IndexOf(' ')));
                                        command = command.Substring(command.IndexOf(' ') + 1).Trim();
                                        int screenNum = Int32.Parse(command) - 1;

                                        if (screenNum >= 0 && screenNum < ctrl.sc.Length)
                                        {
                                            s = ctrl.sc[screenNum];
                                        }
                                    }
                                    else
                                    {
                                        acc = ctrl.FindAccount(command);
                                        s = ctrl.GetNextWindow(acc);
                                    }

                                    if (acc != null && s != null && s.EmulatorProcess != null)
                                    {
                                        s.AbortRoutine();
                                        ctrl.Logout(s);
                                        ctrl.StartApp(s);
                                        ctrl.Login(s, acc);
                                        s.CreateRoutine("AutoActions");
                                        s.Thread.Start();
                                    }
                                }
                                else if (command.StartsWith("shield"))
                                {
                                    command = command.Substring(7).Trim();

                                    DataObjects.Account a = ctrl.FindAccount(command);

                                    bool success = false;

                                    if (a != null)
                                    {
                                        Screen s = ctrl.GetNextWindow(a);

                                        if (s != null)
                                        {
                                            s.AbortRoutine();
                                            while (s.Emulator.LastKnownAccount == null || s.Emulator.LastKnownAccount.Id != a.Id)
                                            {
                                                ctrl.Logout(s);
                                                ctrl.StartApp(s);
                                                ctrl.Login(s, a);
                                            }
                                            if (s.Emulator.LastKnownAccount != null && s.Emulator.LastKnownAccount.Id == a.Id)
                                            {
                                                //success = s.ActivateBoost(ScheduleType.Shield, 3); //DIFF MS
                                                success = s.ActivateBoost(ScheduleType.Shield, 8);
                                            }
                                            s.CreateRoutine("AutoActions");
                                            s.Thread.Start();
                                        }
                                    }

                                    if (!success)
                                    {
                                        ctrl.SendNotification(String.Format("Failed to activate {0} on {1}", ScheduleType.Shield.ToString(), a.ToString()), NotificationType.BoostActivationFail);
                                    }
                                }
                                else if (command.StartsWith("ss"))
                                {
                                    try
                                    {
                                        using (Bitmap bmpScreenCapture = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height))
                                        {
                                            using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                                            {
                                                g.CopyFromScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds.X, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y, 0, 0, bmpScreenCapture.Size, CopyPixelOperation.SourceCopy);
                                            }

                                            System.IO.Directory.CreateDirectory(String.Format("{0}\\auto", Controller.Instance.GetFullScreenshotDir()));
                                            bmpScreenCapture.Save(String.Format("{0}\\auto\\ss{1}.bmp", ctrl.GetFullScreenshotDir(), bmpScreenCapture.Checksum().ToString("X4")), ImageFormat.Bmp);
                                        }
                                    }
                                    catch (Exception ex) { }
                                }
                                else if (command.StartsWith("job"))
                                {
                                    command = command.Substring(3).Trim();

                                    string job = command.Substring(0, command.IndexOf(' '));

                                    command = command.Substring(command.IndexOf(' ') + 1).Trim();

                                    switch (job)
                                    {
                                        case "scheduler":
                                            chkScheduler.Checked = command == "on";
                                            break;
                                        case "tasks":
                                            chkTasks.Checked = command == "on";
                                            break;
                                    }
                                }
                                else if (command.StartsWith("kill"))
                                {
                                    command = command.Substring(4).Trim();

                                    int window = Int32.Parse(command);

                                    if (window > 0)
                                    {
                                        if (ctrl.sc[window - 1] != null)
                                        {
                                            ctrl.KillEmulator(ctrl.sc[window - 1]);
                                        }
                                    }
                                    else
                                    {
                                        foreach (Screen s in ctrl.sc)
                                        {
                                            if (s != null)
                                            {
                                                ctrl.KillEmulator(s);
                                            }
                                        }

                                        Program.RestartApp();
                                    }
                                }
                                else
                                {
                                    //while (ctrl.IdleLevel > 0)
                                    {
                                        System.Threading.Thread.Sleep(1000);
                                    }

                                    Screen s = ctrl.GetFirstAbleWindow();
                                    s.AbortRoutine();
                                    s.SendChat("Invalid command", 1);
                                    s.CreateRoutine("AutoActions");
                                    s.Thread.Start();
                                }

                                tmrSupressAction.Restart();
                            }
                            else if (chatMessage.Message.StartsWith("A new Alliance Gift Tile has appeared at") && tmrSupressAction.ElapsedMilliseconds > 5000)
                            {
                                tmrSupressAction.Restart();

                                ctrl.SendNotification("Alliance tile appeared", NotificationType.AllianceTile);
                            }
                        }
                        else if (message.GetType() == typeof(Messages.JsonMessage))
                        {
                            Messages.JsonMessage json = (Messages.JsonMessage)message;

                            if (json.RawJson.Contains("AdventurersDayGatheringEvent"))
                            {
                                Controller.Instance.SendNotification("Gather Event", NotificationType.Status);
                            }
                            else if (json.RawJson.Contains("AdventurersDayTrainingEvent"))
                            {
                                Controller.Instance.SendNotification("Training Event", NotificationType.Status);
                            }
                            else if (json.RawJson.Contains("AdventurersDayMonsterHuntEvent"))
                            {
                                Controller.Instance.SendNotification("Monster Event", NotificationType.Status);
                            }
                            else if (json.RawJson.Contains("AdventurersDayQuestCompleteEvent"))
                            {
                                Controller.Instance.SendNotification("Hero Quest Event", NotificationType.Status);
                            }
                            else if (json.RawJson.Contains("Empire41AscensionFlashCityGuardian"))
                            {
                                //Controller.Instance.SendNotification("Event", NotificationType.Status);
                            }
                        }
                    }
                }
            }
        }

        private static void device_OnPacketArrival(object sender, SharpPcap.CaptureEventArgs e)
        {
            DateTime time = e.Packet.Timeval.Date;
            int len = e.Packet.Data.Length;
            string res = BitConverter.ToString(e.Packet.Data);
            res = res.Replace("-", "");
        }

        private void bsPacket_CurrentChanged(object sender, EventArgs e)
        {
            currentMessage = (Messages.Message)bsPacket.Current;

            if (currentMessage != null)
            {
                if (IsViewPayload && currentMessage.PayloadData != null)
                {
                    hexData.SetBytes(currentMessage.PayloadData);
                }
                else if (!IsViewPayload && currentMessage.RawData != null)
                {
                    hexData.SetBytes(currentMessage.RawData);
                }

                treeXml.Nodes.Clear();

                if (currentMessage is Messages.XmlMessage)
                {
                    Messages.XmlMessage xmlMessage = (Messages.XmlMessage)currentMessage;

                    treeXml.Nodes.Clear();

                    try
                    {
                        treeXml.Nodes.Add(new TreeNode(xmlMessage.Document.DocumentElement.Name));
                        //AddNode(doc.DocumentElement, treeXml.Nodes[0]);
                        ConvertXmlNodeToTreeNode(xmlMessage.Document, treeXml.Nodes);
                        treeXml.ExpandAll();

                        /*System.Xml.XmlNodeList nodes = doc.DocumentElement.SelectNodes("/message/body");

                        foreach (System.Xml.XmlNode node in nodes)
                        {
                            if (node.InnerText != "")
                            {
                                //cut code below
                            }
                        }*/
                    }
                    catch (System.Xml.XmlException ex) { }
                    catch (System.NullReferenceException ex) { }
                }
                //System.Threading.Thread.Sleep(500);
                //gridPacket.Focus();
            }
        }
        #endregion

        #region PacketActivityDisplay
        private void ConvertXmlNodeToTreeNode(XmlNode xmlNode, TreeNodeCollection treeNodes)
        {
            TreeNode newTreeNode = treeNodes.Add(xmlNode.Name);

            switch (xmlNode.NodeType)
            {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.XmlDeclaration:
                    newTreeNode.Text = "<?" + xmlNode.Name + " " +
                      xmlNode.Value + "?>";
                    break;
                case XmlNodeType.Element:
                    newTreeNode.Text = "<" + xmlNode.Name + ">";
                    break;
                case XmlNodeType.Attribute:
                    newTreeNode.Text = "ATTRIBUTE: " + xmlNode.Name;
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    newTreeNode.Text = xmlNode.Value;
                    break;
                case XmlNodeType.Comment:
                    newTreeNode.Text = "<!--" + xmlNode.Value + "-->";
                    break;
            }

            if (xmlNode.Attributes != null)
            {
                foreach (XmlAttribute attribute in xmlNode.Attributes)
                {
                    ConvertXmlNodeToTreeNode(attribute, newTreeNode.Nodes);
                }
            }
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                ConvertXmlNodeToTreeNode(childNode, newTreeNode.Nodes);
            }
        }

        private void rdoPayload_CheckedChanged(object sender, EventArgs e)
        {
            IsViewPayload = true;

            if (currentMessage != null && currentMessage.PayloadData != null)
            {
                hexData.SetBytes(currentMessage.PayloadData);
            }
        }

        private void rdoRaw_CheckedChanged(object sender, EventArgs e)
        {
            IsViewPayload = false;

            if (currentMessage != null && currentMessage.RawData != null)
            {
                hexData.SetBytes(currentMessage.RawData);
            }
        }

        private void btnCopyText_Click(object sender, EventArgs e)
        {
            if (currentMessage != null && currentMessage.PayloadData != null)
            {
                Clipboard.SetText(System.Text.Encoding.Default.GetString(currentMessage.PayloadData));
            }
        }
        #endregion

        #region Single Tasks
        private void btnClearGifts_Click(object sender, EventArgs e)
        {
            if (ctrl.ActiveScreen != null && ctrl.ActiveScreen.EmulatorProcess != null)
            {
                ctrl.ActiveScreen.AbortRoutine(1000);
                ctrl.CollectGifts();
                ctrl.ActiveScreen.CreateRoutine("AutoActions");
                ctrl.ActiveScreen.Thread.Start();
            }
        }

        private void btnMissionXP_Click(object sender, EventArgs e)
        {
            if (ctrl.ActiveScreen != null && ctrl.ActiveScreen.EmulatorProcess != null)
            {
                ctrl.ActiveScreen.AbortRoutine(1000);
                ctrl.ActiveScreen.MissionXP();
                ctrl.ActiveScreen.CreateRoutine("AutoActions");
                ctrl.ActiveScreen.Thread.Start();
            }
        }

        private void btnMissions_Click(object sender, EventArgs e)
        {
            ctrl.ActiveScreen.AbortRoutine(1000);
            ctrl.CollectMissions();
            ctrl.ActiveScreen.CreateRoutine("AutoActions");
            ctrl.ActiveScreen.Thread.Start();
        }

        private void btnMap_Click(object sender, EventArgs e)
        {
            if (ctrl.ActiveScreen != null && ctrl.ActiveScreen.EmulatorProcess != null)
            {
                stsState.Text = System.DateTime.Now.ToLongTimeString();

                ctrl.ActiveScreen.AbortRoutine(2000);
                ctrl.ActiveScreen.Map(Int32.Parse(txtSliceStartX.Text), Int32.Parse(txtSliceStartY.Text), Int32.Parse(txtRowStart.Text), Int32.Parse(txtColStart.Text));
                ctrl.ActiveScreen.CreateRoutine("AutoActions");
                ctrl.ActiveScreen.Thread.Start(); ;

                stsState.Text = stsState.Text + " - " + System.DateTime.Now.ToLongTimeString();
            }
        }

        private void btnSearchEnemy_Click(object sender, EventArgs e)
        {
            if (ctrl.ActiveScreen != null && ctrl.ActiveScreen.EmulatorProcess != null)
            {
                stsState.Text = System.DateTime.Now.ToLongTimeString();

                ctrl.ActiveScreen.AbortRoutine(2000);
                bckHeartBeat.RunWorkerAsync();
                ctrl.ActiveScreen.SearchEnemies(Int32.Parse(txtSliceStartX.Text), Int32.Parse(txtSliceStartY.Text));
                bckHeartBeat.CancelAsync();
                ctrl.ActiveScreen.CreateRoutine("AutoActions");
                ctrl.ActiveScreen.Thread.Start();

                stsState.Text = stsState.Text + " - " + System.DateTime.Now.ToLongTimeString();
            }
        }

        private void btnGrowXP_Click(object sender, EventArgs e)
        {
            if (ctrl.ActiveScreen != null && ctrl.ActiveScreen.EmulatorProcess != null)
            {
                ctrl.ActiveScreen.AbortRoutine(1000);
                ctrl.ActiveScreen.GrowXP();
                ctrl.ActiveScreen.CreateRoutine("AutoActions");
                ctrl.ActiveScreen.Thread.Start();
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (ctrl.ActiveScreen != null && ctrl.ActiveScreen.EmulatorProcess != null)
            {
                ctrl.ActiveScreen.AttackEnemy(261, 445, 261, 445, RallyDelay.None, true);
                //System.Threading.Thread.Sleep(1000);
                //sc.ResizeWindow();
                //sc.SendClickDrag(30, 400, 30, 340, 100, true, 500);
                //sc.ResourceTransfer(64, 348, Int32.Parse(txtRowStart.Text), Int32.Parse(txtSliceStartX.Text), Int32.Parse(txtSliceStartY.Text));
                //Controller.CaptureApplicationNew(ctrl.ActiveScreen);

                //Controller.SendKeyTest(ctrl.sc[0], "ok");
                
                //ctrl.sc[0].KillApp();

                //ctrl.sc[0].GoToCoordinate(50, 78);

                //Bitmap bmp = Controller.CaptureApplication(ctrl.sc[0], 0, 32, 394, 648);
                //bmp.Save(String.Format("{0}\\pic1.jpg", Controller.Instance.GetFullScreenshotDir()), ImageFormat.Jpeg);

                //ctrl.ActiveScreen.ActivateBoost(ScheduleType.Shield, 8);

                //sc.SendClick(375, 20, 300); //click exit fullscreen
                //sc.SendClick(150, 610, 300); //click chat bar

                //sc.SendClick(305, 20, 300); //click custom room
                //sc.SendClick(133, 682, 300); //click chat text
                //sc.SendKey("This is a testgbfdhgfhgf");
                //sc.SendClick(-15, 545);
                //System.Threading.Thread.Sleep(5400);
                //sc.SendClick(350, 200, 300); //click out
                //sc.SendClick(350, 682, 100); //click chat bar
                //sc.SendKeyTest(30);
            }
        }
        #endregion

        public void SetLastMapParameters(int x, int y, int r, int c)
        {
            txtSliceStartX.Text = x.ToString();
            txtSliceStartY.Text = y.ToString();
            txtRowStart.Text = r.ToString();
            txtColStart.Text = c.ToString();
        }

        private void lstAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            bsScheduleTask.Clear();

            foreach (DataObjects.ScheduleTask task in BotDatabase.GetObjects<DataObjects.ScheduleTask>())
            {
                if (task.Account.Id == ((DataObjects.Account)lstAccounts.SelectedItem).Id)
                {
                    task.Account = (DataObjects.Account)lstAccounts.SelectedItem;
                    bsScheduleTask.Add(task);
                }
            }

            DataObjects.Account account = (DataObjects.Account)lstAccounts.SelectedItem;

            bsScheduleTask.Add(new DataObjects.ScheduleTask(0, account, 0, 0, 0, 0, 0, 0, 0, 0, new DateTime(), account.App));
        }

        private void bckKeepAlive_DoWork(object sender, DoWorkEventArgs e)
        {
            if (System.Threading.Thread.CurrentThread.Name == null)
            {
                System.Threading.Thread.CurrentThread.Name = "KeepAlive";
            }

            while (!bckKeepAlive.CancellationPending)
            {
                try
                {
                    if (chkScheduler.Checked)
                    {
                        foreach (DataObjects.ScheduleTask task in BotDatabase.GetObjects<DataObjects.ScheduleTask>())
                        {
                            if (task.App.Id == 2 && DateTime.Now.Subtract(task.NextAction).Minutes > 5)
                            {
                                if (tmrLateSchedule.ElapsedMilliseconds > 900000)
                                {
                                    ctrl.SendNotification("Scheduled tasks are past due", NotificationType.TasksPastDue);

                                    /*foreach (Screen s in ctrl.sc)
                                    {
                                        ctrl.KillEmulator(s, false);
                                    }*/

                                    Program.RestartApp();
                                }
                                else if (tmrLateSchedule.ElapsedMilliseconds > 660000)
                                {
                                    ctrl.KillEmulator(ctrl.GetNextWindow(task));
                                }
                                else if (tmrLateSchedule.ElapsedMilliseconds > 480000)
                                {
                                    ctrl.GetNextWindow(task).ClickHome();
                                }
                            }
                        }
                    }

                    if (tmrHeartBeat.ElapsedMilliseconds >= 3600000)
                    {
                        ctrl.SendNotification(String.Format("Heartbeat:\n{0}", ctrl.GetStatusMessage()), NotificationType.Status);
                        tmrHeartBeat.Restart();
                    }

                    System.Threading.Thread.Sleep(30000);

                    if ((!chkScheduler.Checked || (ctrl.StartScheduler > DateTime.Today && DateTime.Now.Subtract(ctrl.StartScheduler).Minutes >= 1)) && (!chkTasks.Checked || (ctrl.StartTasks > DateTime.Today && DateTime.Now.Subtract(ctrl.StartTasks).Minutes >= 3)))
                    {
                        //ctrl.EndTask();
                    }

                    if (chkScheduler.Checked)
                    {
                        try
                        {
                            bckScheduler.RunWorkerAsync();
                        }
                        catch (InvalidOperationException ex) { }
                    }

                    if (chkTasks.Checked)
                    {
                        try
                        {
                            bckRegularTasks.RunWorkerAsync();
                        }
                        catch (InvalidOperationException ex) { }
                    }

                    try
                    {
                        bckScreenState.RunWorkerAsync();
                    }
                    catch (InvalidOperationException ex) { }

                    try
                    {
                        bckAutoActions.RunWorkerAsync();
                    }
                    catch (InvalidOperationException ex) { }
                }
                catch (Exception ex)
                {
                    BotDatabase.InsertLog(0, String.Format("Keep Alive Fail: {0}", ex.Message), String.Format("{0}{1}", ex.GetType(), ex.StackTrace), new byte[1] { 0x0 });
                }
            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (bckKeepAlive.IsBusy)
            {
                bckKeepAlive.CancelAsync();
                bckKeepAlive.Dispose();
            }

            if (bckScreenState.IsBusy)
            {
                bckScreenState.CancelAsync();
                bckScreenState.Dispose();
            }

            if (bckRegularTasks.IsBusy)
            {
                bckRegularTasks.CancelAsync();
                bckRegularTasks.Dispose();
            }

            if (bckScheduler.IsBusy)
            {
                bckScheduler.CancelAsync();
                bckScheduler.Dispose();
            }

            if (bckSniffer.IsBusy)
            {
                bckSniffer.CancelAsync();
                bckSniffer.Dispose();
            }
        }

        private void bckAutoActions_DoWork(object sender, DoWorkEventArgs e)
        {
            if (System.Threading.Thread.CurrentThread.Name == null)
            {
                System.Threading.Thread.CurrentThread.Name = "WholeScreenActions";
            }

            System.Threading.Thread.Sleep(10000);

            tmrAttackNotify = new System.Diagnostics.Stopwatch();
            tmrAttackNotify.Start();

            while (true)
            {
                try
                {
                    ctrl.StartAutoActions = DateTime.Now;

                    //Whole Screen Actions
                    using (Bitmap bmpScreenCapture = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height))
                    {
                        using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                        {
                            g.CopyFromScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds.X, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y, 0, 0, bmpScreenCapture.Size, CopyPixelOperation.SourceCopy);
                        }

                        ushort chksum;

                        //check Droid4X is booting message
                        bool shouldContinue = true;

                        for (int y = 0; y < bmpScreenCapture.Height - 20 && shouldContinue; y += 20)
                        {
                            for (int x = 0; x < bmpScreenCapture.Width - 113 && shouldContinue; x += 113)
                            {
                                Color c1 = bmpScreenCapture.GetPixel(x, y), c2 = bmpScreenCapture.GetPixel(x + 113, y + 20);

                                if (c1.Equals(c2.R, c2.G, c2.B) && c1.Equals(221, 47, 48))
                                {
                                    while (!c1.Equals(255, 255, 255))
                                    {
                                        y++;
                                        c1 = bmpScreenCapture.GetPixel(x, y);
                                    }

                                    y += 236;

                                    for (x = 0; x < bmpScreenCapture.Width - 50 && shouldContinue; x++)
                                    {
                                        chksum = bmpScreenCapture.Checksum(x, y + 6, 20, 20);

                                        if (chksum == 0x94f9 || chksum == 0xa5de)
                                        {
                                            Controller.SendClick(Screen.WholeScreen, x + 5, y + 6, 1000);
                                            shouldContinue = false;
                                        }
                                    }
                                }
                            }
                        }

                        //check MEmu error
                        bool foundMemuError = false;
                        int taskbarLeftOffset = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left;
                        int taskbarTopOffset = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top;
                        Color errorFormColor = Color.FromArgb(27, 39, 41); //MEMU 2.9.6.1 //Color to find was 255,255,255, 3.0.5.2 is 17,34,34, 3.5.0.2-5.1.1.1 is 27,39,41

                        for (int y = taskbarTopOffset + 50; y < Controller.SCREEN_H; y += 10)
                        {
                            for (int s = 1; s < Controller.Instance.sc.Length; s++)
                            {
                                int x = Controller.FORM_X + taskbarLeftOffset + s * (2 + Controller.SCREEN_W + 38 + 14 + 5 + 3) - 11;

                                Color c = bmpScreenCapture.GetPixel(x, y);

                                if (c.Equals(errorFormColor.R, errorFormColor.G, errorFormColor.B))
                                {
                                    Controller.SendClick(Screen.WholeScreen, x + 1, y, 300);
                                    foundMemuError = true;
                                    break;
                                }
                            }
                        }

                        if (foundMemuError)
                        {
                            using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                            {
                                g.CopyFromScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds.X, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y, 0, 0, bmpScreenCapture.Size, CopyPixelOperation.SourceCopy);
                            }

                            //debug
                            //bmpScreenCapture.Save(String.Format("{0}\\memuerror.bmp", ctrl.GetFullScreenshotDir()), ImageFormat.Bmp);
                            //ctrl.SendNotification("MEmu error Unable to database", NotificationType.Offline);
                            //System.Threading.Thread.Sleep(10000);

                            for (int y = taskbarTopOffset + 50; y < Controller.SCREEN_H; y += 10)
                            {
                                for (int s = 1; s < Controller.Instance.sc.Length; s++)
                                {
                                    int x = Controller.FORM_X + taskbarLeftOffset + s * (2 + Controller.SCREEN_W + 38 + 14 + 5 + 3) - 11;

                                    Color c = bmpScreenCapture.GetPixel(x, y);

                                    if (c.Equals(errorFormColor.R, errorFormColor.G, errorFormColor.B))
                                    {
                                        do
                                        {
                                            y--;
                                            c = bmpScreenCapture.GetPixel(x, y);
                                        }
                                        while (c.Equals(errorFormColor.R, errorFormColor.G, errorFormColor.B));

                                        y++;

                                        do
                                        {
                                            x++;
                                            c = bmpScreenCapture.GetPixel(x, y);
                                        }
                                        while (c.Equals(errorFormColor.R, errorFormColor.G, errorFormColor.B));

                                        x--;

                                        do
                                        {
                                            y++;
                                            c = bmpScreenCapture.GetPixel(x, y);
                                        }
                                        while (c.Equals(errorFormColor.R, errorFormColor.G, errorFormColor.B));

                                        y--;

                                        c = bmpScreenCapture.GetPixel(x - 20, y - 30);

                                        if (c.Equals(errorFormColor.R, errorFormColor.G, errorFormColor.B)) //button not there, memu failed to start
                                        {
                                            int xLeft = x;

                                            do
                                            {
                                                xLeft--;
                                                c = bmpScreenCapture.GetPixel(xLeft, y);
                                            }
                                            while (c.Equals(errorFormColor.R, errorFormColor.G, errorFormColor.B));

                                            xLeft++;

                                            Controller.SendClick(Screen.WholeScreen, (x + xLeft) / 2, y - 30, 300);
                                        }
                                        else //"Unable to database" error
                                        {
                                            Controller.SendClick(Screen.WholeScreen, x - 20, y - 30, 300);
                                        }
                                        
                                        break;
                                    }
                                }
                            }
                        }

                        //check LINE update dialog
                        Color c3 = bmpScreenCapture.GetPixel(970, 555);
                        if (c3.Equals(11, 178, 3))
                        {
                            Controller.SendClick(Screen.WholeScreen, 965, 555, 10000);
                            Program.RestartApp();
                        }

                        //check Teamviewer session ended
                        chksum = bmpScreenCapture.Checksum(768, 505, 20, 20);
                        if (chksum == 0x9b7f)
                        {
                            Controller.SendClick(Screen.WholeScreen, 1176, 600, 1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    BotDatabase.InsertLog(0, String.Format("{0} {1}", ex.GetType(), ex.Message), ex.StackTrace, new byte[1] { 0x0 });
                    ctrl.SendNotification(String.Format("Auto Actions Crash {0} {1} {2}", ex.GetType(), ex.Message, ex.StackTrace), NotificationType.Crash);


                    System.Threading.Thread.Sleep(5000);
                    Program.RestartApp();
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private void gridAccounts_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            DataObjects.Account account = (DataObjects.Account)bsAccount.Current;

            if (account.Name != "" && account.UserName != "" && account.Email != "" && account.Password != "")
            {
                bool reloadAccountList = account.Id == 0;

                account.App = ctrl.ActiveScreen.Emulator.App;

                account = account.Save();

                if (reloadAccountList)
                {
                    ReloadAccountList();
                }
            }
        }

        private void gridAccounts_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DialogResult response = MessageBox.Show("Are you sure you want to delete this account?", "Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if ((response == DialogResult.Yes))
            {
                DataObjects.Account account = (DataObjects.Account)bsAccount.Current;
                if (account.Id > 0)
                {
                    account.Delete();
                    ReloadAccountList();
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void gridAccounts_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (gridAccounts.CurrentCell.ColumnIndex == 3)
            {
                TextBox textBox = e.Control as TextBox;
                if (textBox != null)
                {
                    textBox.UseSystemPasswordChar = true;
                }
            }
            else
            {
                TextBox textBox = e.Control as TextBox;
                if (textBox != null)
                {
                    textBox.UseSystemPasswordChar = false;
                }
            }
        }

        private void gridAccounts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 3 && e.Value != null)
            {
                gridAccounts.Rows[e.RowIndex].Tag = e.Value;
                e.Value = new String('\u25CF', e.Value.ToString().Length);
            }
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            Program.RestartApp();
        }

        private void rdoWindow1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ctrl.GetAndSetEmulatorProcess(0);
            }
        }

        private void rdoWindow2_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ctrl.GetAndSetEmulatorProcess(1);
            }
        }

        private void rdoWindow3_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ctrl.GetAndSetEmulatorProcess(2);
            }
        }

        private void rdoWindow4_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ctrl.GetAndSetEmulatorProcess(3);
            }
        }

        private void btnBoostModifier_Click(object sender, EventArgs e)
        {
            Controller.CaptureApplication(ctrl.ActiveScreen);
            uint chksum = ScreenState.GetScreenChecksum(ctrl.ActiveScreen.SuperBitmap, 110, 188, 20), chksum2;

            while (ctrl.ActiveScreen.ScreenState.CurrentArea == Area.Menus.BuildingBoost && !(chksum == 0xed29 || chksum == 0xf425))
            {
                Controller.SendClick(ctrl.ActiveScreen, 250, 566, 100);

                Application.DoEvents();

                Controller.CaptureApplication(ctrl.ActiveScreen);
                chksum = ScreenState.GetScreenChecksum(ctrl.ActiveScreen.SuperBitmap, 110, 188, 20);
            }

            //now on lv23 and lv24
            if (chksum == 0xed29 || chksum == 0xf425)
            {
                chksum = ScreenState.GetScreenChecksum(ctrl.ActiveScreen.SuperBitmap, 289, 311, 20);
                chksum2 = ScreenState.GetScreenChecksum(ctrl.ActiveScreen.SuperBitmap, 290, 249, 20);

                while (ctrl.ActiveScreen.ScreenState.CurrentArea == Area.Menus.BuildingBoost && !(chksum == 0xc0ed || (chksum == 0xcbcc && chksum2 == 0x933c)))
                {
                    Controller.SendClick(ctrl.ActiveScreen, 250, 566, 1000);

                    Application.DoEvents();

                    Controller.CaptureApplication(ctrl.ActiveScreen);
                    chksum = ScreenState.GetScreenChecksum(ctrl.ActiveScreen.SuperBitmap, 289, 311, 20);
                    chksum2 = ScreenState.GetScreenChecksum(ctrl.ActiveScreen.SuperBitmap, 290, 249, 20);
                    if (!System.IO.File.Exists(String.Format("{0}\\boost\\mult\\{1}.bmp", ctrl.GetFullScreenshotDir(), chksum.ToString("X4"))))
                    {
                        ctrl.ActiveScreen.SuperBitmap.Save(String.Format("{0}\\boost\\mult\\{1}.bmp", ctrl.GetFullScreenshotDir(), chksum.ToString("X4")));
                    }
                    if (!System.IO.File.Exists(String.Format("{0}\\boost\\percent\\{1}.bmp", ctrl.GetFullScreenshotDir(), chksum2.ToString("X4"))))
                    {
                        ctrl.ActiveScreen.SuperBitmap.Save(String.Format("{0}\\boost\\percent\\{1}.bmp", ctrl.GetFullScreenshotDir(), chksum2.ToString("X4")));
                    }

                    while (ctrl.ActiveScreen.ScreenState.CurrentArea == Area.Menus.BuildingBoost && !ctrl.ActiveScreen.SuperBitmap.GetPixel(244, 546).Equals(33, 142, 82))
                    {
                        System.Threading.Thread.Sleep(100);
                        Controller.CaptureApplication(ctrl.ActiveScreen);
                    }
                }
            }
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            ctrl.Database.Settings.Emulator1 = Int32.Parse(txtEmulator1.Text);
            ctrl.Database.Settings.Emulator2 = Int32.Parse(txtEmulator2.Text);
            ctrl.Database.Settings.Emulator3 = Int32.Parse(txtEmulator3.Text);
            ctrl.Database.Settings.Emulator4 = Int32.Parse(txtEmulator4.Text);
            ctrl.Database.Settings.SlackURL = txtSlackURL.Text;
            ctrl.Database.Settings.PushoverAPIKey = txtPushoverAPI.Text;
            ctrl.Database.Settings.PushoverUserKey = txtPushoverUser.Text;
            ctrl.Database.Settings.Save();

            ctrl.sc[0].Emulator.App = (DataObjects.App)cboEmulator1.SelectedItem;
            ctrl.sc[0].Emulator.Save();
            ctrl.sc[1].Emulator.App = (DataObjects.App)cboEmulator2.SelectedItem;
            ctrl.sc[1].Emulator.Save();
            ctrl.sc[2].Emulator.App = (DataObjects.App)cboEmulator3.SelectedItem;
            ctrl.sc[2].Emulator.Save();
            ctrl.sc[3].Emulator.App = (DataObjects.App)cboEmulator4.SelectedItem;
            ctrl.sc[3].Emulator.Save();
        }

        private void tabControlMain_Selected(object sender, TabControlEventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;

            FormContainer.Panel2Collapsed = current.Text == "Activity";
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (tabControlMonitor.SelectedTab == tabMarches)
            {
                Messages.Objects.March march = (Messages.Objects.March)lstMarches.SelectedItem;

                System.IO.Directory.CreateDirectory(String.Format("{0}\\output\\debug\\marches", System.Windows.Forms.Application.StartupPath));
                
                foreach (Messages.MarchMessage message in march.Messages)
                {
                    System.IO.File.WriteAllText(String.Format("{0}\\output\\debug\\marches\\{1}-{2}.txt", System.Windows.Forms.Application.StartupPath, march.Id, message.Id), Utilities.FormatJSON(message.RawJson));
                }
            }
            else if (tabControlMonitor.SelectedTab == tabTiles)
            {
                Messages.Objects.Tile tile = (Messages.Objects.Tile)lstTiles.SelectedItem;

                System.IO.Directory.CreateDirectory(String.Format("{0}\\output\\debug\\tiles", System.Windows.Forms.Application.StartupPath));

                foreach (Messages.TileUpdatedMessage message in tile.Messages)
                {
                    System.IO.File.WriteAllText(String.Format("{0}\\output\\debug\\tiles\\{1}-{2}.txt", System.Windows.Forms.Application.StartupPath, tile.ObjectId, message.Id), Utilities.FormatJSON(message.RawJson));
                }
            }
        }

        private void btnGoTo_Click(object sender, EventArgs e)
        {
            if (tabControlMonitor.SelectedTab == tabMarches)
            {
                Messages.Objects.March march = (Messages.Objects.March)lstMarches.SelectedItem;
                if (march != null)
                {
                    ctrl.ActiveScreen.GoToCoordinate(march.DestCoordinate);
                }
            }
            else if (tabControlMonitor.SelectedTab == tabTiles)
            {
                Messages.Objects.Tile tile = (Messages.Objects.Tile)lstTiles.SelectedItem;
                if (tile != null)
                {
                    ctrl.ActiveScreen.GoToCoordinate(tile.Coordinate);
                }
            }
        }
    }
}
