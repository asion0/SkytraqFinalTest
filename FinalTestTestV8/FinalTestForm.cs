using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Runtime.InteropServices;   // required for Marshal
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Xml;
using System.IO;

namespace FinalTestV8
{
    public partial class FinalTestForm : Form
    {

        public FinalTestForm()
        {
            InitializeComponent();
            Global.Init();
        }

        string[] logPath = new string[4];
        private void FinalTestForm_Load(object sender, EventArgs e)
        {
            for(int i=0; i<4; i++)
            {
                logPath[i] = System.Environment.CurrentDirectory + "\\Site" + Program.siteNumber.ToString() +
                    "-" + (i + 1).ToString() + ".log";
            }

            profile = new FinalTest3Profile(Program.siteNumber, Program.profilePath);
            this.Icon = Properties.Resources.FinalTest3;
            DoLogin();
        }

        private FinalTest3Profile profile;

        public const int ModuleCount = 8;
        public const String DefaultProfileName = "SkytraqTest.dat";
        /*
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);        // call default p

            if (m.Msg == WM_DEVICECHANGE)
            {
                // WM_DEVICECHANGE can have several meanings depending on the WParam value...
                int msgType = m.WParam.ToInt32();
                if (msgType == DBT_DEVICEARRIVAL || msgType == DBT_DEVICEREMOVECOMPLETE)
                {
                    int devType = Marshal.ReadInt32(m.LParam, 4);
                    if (DBT_DEVTYP_PORT == devType)
                    {

                        DEV_BROADCAST_PORT vol;
                        vol = (DEV_BROADCAST_PORT)
                            Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_PORT));

                        int step = (vol.dbcp_name[1] == 0x00) ? 2 : 1;
                        StringBuilder sb = new StringBuilder(8);
                        for (int i = 0; i < vol.dbcp_name.Length; i += step)
                        {
                            if (vol.dbcp_name[i] == 0x00)
                            {
                                break;
                            }
                            sb.Append(vol.dbcp_name[i]);
                        }
                        if (TestRunning == TestStatus.Finished || TestRunning == TestStatus.Ready)
                        {
                            if (msgType == DBT_DEVICEARRIVAL)
                            {
                                AddMessage(0, sb.ToString() + " plugged-in.");
                            }
                            else
                            {
                                AddMessage(0, sb.ToString() + " removed.");
                            }
                            InitComSel();
                        }

                    }
                }
            }
        }
        */
        //private ModuleTestProfile profile;

        private System.Windows.Forms.CheckBox[] disableTable;
        private ComboBox[] comSelTable;
        private Panel[] panelTable;
        private System.Windows.Forms.Label[] resultTable;
        private System.Windows.Forms.ListBox[] messageTable;
        //private PictureBox[] snrChartTable;

        private int[] failCount;
        private int[] passCount;
        private System.Windows.Forms.Label[] failTable;
        private System.Windows.Forms.Label[] totalTable;
        private System.Windows.Forms.Label[] yieldTable;

        private SessionReport rp = new SessionReport();
        private XmlDocument doc = new XmlDocument();
        //private XmlElement root;
        //private XmlElement testSession;

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void InitComSel()
        {
            FinalTestV8.Properties.Settings o = FinalTestV8.Properties.Settings.Default;
            string[] comPortSetting = { o.gdComPort, o.a1ComPort, o.a2ComPort, o.a3ComPort,
                    o.a4ComPort, o.b1ComPort, o.b2ComPort, o.b3ComPort, o.b4ComPort };

            string[] ports = SerialPort.GetPortNames();
            String selItem = "";
            for(int i = 0; i< ModuleCount; i++)
            {
                ComboBox c = comSelTable[i];
                if (c == null)
                {
                    continue;
                }
                if (c.SelectedIndex >= 0)
                {
                    selItem = c.Text;
                }
                else
                {
                    selItem = comPortSetting[i];
                }

                c.Items.Clear();
                foreach (string port in ports)
                {
                    int n = c.Items.Add(port);
                    if (port == selItem)
                    {
                        c.SelectedIndex = n;
                    }
                }
            }
        }

        //Properties載入後，更新UI使之同步
        private void UpdateUIBySetting()
        {
            FinalTestV8.Properties.Settings o = FinalTestV8.Properties.Settings.Default;
            bool[] disableSetting = { o.gdDisable, o.a1Disable, o.a2Disable, o.a3Disable,
                    o.a4Disable, o.b1Disable, o.b2Disable, o.b3Disable, o.b4Disable };

            for (int i = 0; i < ModuleCount; i++)
            {
                if ((disableTable[i] as CheckBox) != null)
                {
                    (disableTable[i] as CheckBox).Checked = disableSetting[i];
                }
            }
        }

        //UI變更後，寫入Properties。
        private void UpdateSettingByUI()
        {
            FinalTestV8.Properties.Settings o = FinalTestV8.Properties.Settings.Default;

            o.a1Disable = (disableTable[0] as CheckBox) != null ? (disableTable[0] as CheckBox).Checked : false;
            o.a2Disable = (disableTable[1] as CheckBox) != null ? (disableTable[1] as CheckBox).Checked : false;
            o.a3Disable = (disableTable[2] as CheckBox) != null ? (disableTable[2] as CheckBox).Checked : false;
            o.a4Disable = (disableTable[3] as CheckBox) != null ? (disableTable[3] as CheckBox).Checked : false;
            o.b1Disable = (disableTable[4] as CheckBox) != null ? (disableTable[4] as CheckBox).Checked : false;
            o.b2Disable = (disableTable[5] as CheckBox) != null ? (disableTable[5] as CheckBox).Checked : false;
            o.b3Disable = (disableTable[6] as CheckBox) != null ? (disableTable[6] as CheckBox).Checked : false;
            o.b4Disable = (disableTable[7] as CheckBox) != null ? (disableTable[7] as CheckBox).Checked : false;

            o.a1ComPort = (comSelTable[0] as ComboBox) != null ? (comSelTable[0] as ComboBox).Text : "";
            o.a2ComPort = (comSelTable[1] as ComboBox) != null ? (comSelTable[1] as ComboBox).Text : "";
            o.a3ComPort = (comSelTable[2] as ComboBox) != null ? (comSelTable[2] as ComboBox).Text : "";
            o.a4ComPort = (comSelTable[3] as ComboBox) != null ? (comSelTable[3] as ComboBox).Text : "";
            o.b1ComPort = (comSelTable[4] as ComboBox) != null ? (comSelTable[4] as ComboBox).Text : "";
            o.b2ComPort = (comSelTable[5] as ComboBox) != null ? (comSelTable[5] as ComboBox).Text : "";
            o.b3ComPort = (comSelTable[6] as ComboBox) != null ? (comSelTable[6] as ComboBox).Text : "";
            o.b4ComPort = (comSelTable[7] as ComboBox) != null ? (comSelTable[7] as ComboBox).Text : "";
        }
        
        enum ResultDisplayType
        {
            None,
            Testing,
            Downloading,
            Fail,
            Pass,
        }

        private void UpdateSlotStatus(int index)
        {
            failTable[index].Text = failCount[index].ToString();
            totalTable[index].Text = passCount[index].ToString();

            if ((failCount[index] + passCount[index]) == 0)
            {
                yieldTable[index].Text = "0.0%";
            }
            yieldTable[index].Text = ((double)passCount[index] / (failCount[index] + passCount[index]) * 100.0).ToString("F1") + "%";
        }

        private void SetResultDisplay(System.Windows.Forms.Label l, ResultDisplayType r)
        {
            switch (r)
            {
                case ResultDisplayType.None:
                    l.Text = "";
                    l.ForeColor = System.Drawing.Color.Black;
                    break;
                case ResultDisplayType.Testing:
                    l.Text = "Testing...";
                    l.ForeColor = System.Drawing.Color.Green;
                    break;
                case ResultDisplayType.Downloading:
                    l.Text = "Loading...";
                    l.ForeColor = System.Drawing.Color.DarkOrange;
                    break;
                case ResultDisplayType.Fail:
                    l.Text = "FAIL";
                    l.ForeColor = System.Drawing.Color.Red;
                    break;
                case ResultDisplayType.Pass:
                    l.Text = "PASS";
                    l.ForeColor = System.Drawing.Color.Blue;
                    break;
            }
        }

        System.Drawing.Font snrFont = new Font(new FontFamily(SystemFonts.DialogFont.Name), 7);
       
        StringFormat drawFormat = new StringFormat();
        
        const int barWidth = 16;
        const int txtXo = 7;
        const int MaxSnrLine = 16;
        private int DrawSnr(int startPos, GpsMsgParser.ParsingStatus o, Graphics g, GpsMsgParser.SateType t)
        {
            for (int i = 0; i < GpsMsgParser.ParsingStatus.MaxSattellite; i++)
            {
                GpsMsgParser.ParsingStatus.sateInfo s = null;
                Brush inUseBarBrush = null;
                Pen noUseBarPen = null;
                Brush inUseIcoBrush = null;
                Brush noUseIcoBrush = null;
                Brush inUseBarTxtBrush = null;
                Brush inUseIcoTxtBrush = null;
                Brush noUseBarTxtBrush = null;
                Brush noUseIcoTxtBrush = null;

                if (GpsMsgParser.SateType.Gps == t)
                {
                    inUseBarBrush = Brushes.Blue;
                    noUseBarPen = Pens.Blue;
                    inUseIcoBrush = Brushes.Blue;
                    noUseIcoBrush = Brushes.Blue;
                    inUseBarTxtBrush = Brushes.White;
                    inUseIcoTxtBrush = Brushes.White;
                    noUseBarTxtBrush = Brushes.Blue;
                    noUseIcoTxtBrush = Brushes.White;
                    s = o.GetGpsSate(i);
                }
                else if(GpsMsgParser.SateType.Glonass == t)
                {
                    inUseBarBrush = Brushes.DarkOrchid;
                    noUseBarPen = Pens.DarkOrchid;
                    inUseIcoBrush = Brushes.DarkOrchid;
                    noUseIcoBrush = Brushes.DarkOrchid;
                    inUseBarTxtBrush = Brushes.White;
                    inUseIcoTxtBrush = Brushes.White;
                    noUseBarTxtBrush = Brushes.DarkOrchid;
                    noUseIcoTxtBrush = Brushes.White;
                    s = o.GetGlonassSate(i);
                }
                else if(GpsMsgParser.SateType.Beidou == t)
                {
                    inUseBarBrush = Brushes.Orange;
                    noUseBarPen = Pens.Orange;
                    inUseIcoBrush = Brushes.Orange;
                    noUseIcoBrush = Brushes.Orange;
                    inUseBarTxtBrush = Brushes.White;
                    inUseIcoTxtBrush = Brushes.White;
                    noUseBarTxtBrush = Brushes.Orange;
                    noUseIcoTxtBrush = Brushes.White;
                    s = o.GetBeidouSate(i);
                }                
                else
                {
                    inUseBarBrush = Brushes.Blue;
                    noUseBarPen = Pens.Blue;
                    inUseIcoBrush = Brushes.Blue;
                    noUseIcoBrush = Brushes.Blue;
                    inUseBarTxtBrush = Brushes.White;
                    inUseIcoTxtBrush = Brushes.White;
                    noUseBarTxtBrush = Brushes.Blue;
                    noUseIcoTxtBrush = Brushes.White;
                    s = o.GetGpsSate(i);
                }                

                if (s.prn == GpsMsgParser.ParsingStatus.NullValue)
                {
                    break;
                }
                if(s.snr == 0 || s.snr == GpsMsgParser.ParsingStatus.NullValue)
                {
                    continue;
                } 
                int barHeight = (s.snr > 45) ? 45 : s.snr;
                if (s.inUse)
                {
                    g.FillEllipse(inUseIcoBrush, barWidth * startPos - 1, 46, barWidth, barWidth);
                    g.DrawString(s.prn.ToString(), snrFont, inUseIcoTxtBrush, barWidth * startPos + txtXo, 49F, drawFormat);
                    if (s.snr != GpsMsgParser.ParsingStatus.NullValue)
                    {
                        g.FillRectangle(inUseBarBrush, barWidth * startPos, 45 - s.snr, barWidth - 1, s.snr);
                        g.DrawString(s.snr.ToString(), snrFont, inUseBarTxtBrush, barWidth * startPos + txtXo, 33F, drawFormat);
                    }
                }
                else 
                {
                    g.FillEllipse(noUseIcoBrush, barWidth * startPos - 1, 46, barWidth, barWidth);
                    g.DrawString(s.prn.ToString(), snrFont, noUseIcoTxtBrush, barWidth * startPos + txtXo, 49F, drawFormat);
                    if (s.snr != GpsMsgParser.ParsingStatus.NullValue)
                    {
                        g.DrawRectangle(noUseBarPen, barWidth * startPos, 45 - s.snr, barWidth - 2, s.snr);
                        g.DrawString(s.snr.ToString(), snrFont, noUseBarTxtBrush, barWidth * startPos + txtXo, 33F, drawFormat);
                    }
                }
                if (++startPos >= MaxSnrLine)
                {
                    break;
                }
            }
            return startPos;
        }

        void MySnrChartPaint(object sender, PaintEventArgs pea)
        {
            int idx = (int)((sender as PictureBox).Tag);
            if (disableTable[idx].Checked)
            {
                return;
            }

            if (TestModule.dvResult == null)
            {
                return;
            } 
                        
            GpsMsgParser.ParsingStatus o = TestModule.dvResult[idx];
            int lastChannel = 0;
            lastChannel = DrawSnr(0, o, pea.Graphics, GpsMsgParser.SateType.Gps);
            lastChannel = DrawSnr(lastChannel, o, pea.Graphics, GpsMsgParser.SateType.Glonass);
            lastChannel = DrawSnr(lastChannel, o, pea.Graphics, GpsMsgParser.SateType.Beidou);
        }

        private bool isPromIniLoaded = false;
        private FirmwareProfile fwProfile;
        private const int MaxReadLength = 512;
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal, int size, string filePath);

        public bool ReadePromIniFile()
        {
            String path = Environment.CurrentDirectory + "\\prom.ini";
            FirmwareProfile tmpFwProfile = new FirmwareProfile();
            fwProfile = null;

            StringBuilder temp = new StringBuilder(MaxReadLength);
            if (0 == GetPrivateProfileString("Firmware", "Prom", "", temp, MaxReadLength, path))
            {
                //return false;
            }
            tmpFwProfile.promFile = temp.ToString();

            if (0 == GetPrivateProfileString("Firmware", "K_Version", "", temp, MaxReadLength, path))
            {
                return false;
            }
            tmpFwProfile.kVersion = temp.ToString();

            if (0 == GetPrivateProfileString("Firmware", "S_Version", "", temp, MaxReadLength, path))
            {
                return false;
            }
            tmpFwProfile.sVersion = temp.ToString();

            if (0 == GetPrivateProfileString("Firmware", "Rev", "", temp, MaxReadLength, path))
            {
                return false;
            }
            tmpFwProfile.rVersion = temp.ToString();

            if (0 == GetPrivateProfileString("Firmware", "CRC", "", temp, MaxReadLength, path))
            {
                return false;
            }
            tmpFwProfile.crc = Convert.ToUInt32(temp.ToString(), 16);
            tmpFwProfile.crcTxt = temp.ToString();

            if (0 == GetPrivateProfileString("Firmware", "Baudrate", "", temp, MaxReadLength, path))
            {
                //return false;
            }
            tmpFwProfile.dvBaudRate = Convert.ToInt32(temp.ToString());

            if (0 == GetPrivateProfileString("Firmware", "Address", "", temp, MaxReadLength, path))
            {
                //return false;
            }
            tmpFwProfile.tagAddress = Convert.ToUInt32(temp.ToString(), 16);

            if (0 == GetPrivateProfileString("Firmware", "Value", "", temp, MaxReadLength, path))
            {
                //return false;
            }
            tmpFwProfile.tagContent = Convert.ToUInt32(temp.ToString(), 16);
            fwProfile = tmpFwProfile;
            return true;
        }

        private void DoLogin()
        {
            InitMainForm();
            this.Text = "Skytraq FT Tester " +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() +
                " - " + Program.module + " Site " +
                Program.siteNumber.ToString() + " Duts " + Program.duts;
            TestRunning = TestStatus.Ready;
            if (Program.module == "V822")
            {
                isPromIniLoaded = ReadePromIniFile();
                if(isPromIniLoaded)
                {
                    fwProfile.ReadePromRawData(Environment.CurrentDirectory + "\\" + fwProfile.promFile);
                    promFile.Text = fwProfile.promFile;
                }
            }
            moduleName.Text = Program.module;

        }

        private void InitMainForm()
        {
            drawFormat.Alignment = StringAlignment.Center;

            //Establish UI controls table
            disableTable = new CheckBox[ModuleCount] { a1Disable, a2Disable, a3Disable, a4Disable, 
                b1Disable, b2Disable, b3Disable, b4Disable };
            comSelTable = new ComboBox[ModuleCount] { a1ComSel, a2ComSel, a3ComSel, a4ComSel, 
                b1ComSel, b2ComSel, b3ComSel, b4ComSel };
            panelTable = new Panel[ModuleCount] { a1Panel, a2Panel, a3Panel, a4Panel, 
                b1Panel, b2Panel, b3Panel, b4Panel };
            resultTable = new Label[ModuleCount] { a1Result, a2Result, a3Result, a4Result, 
                b1Result, b2Result, b3Result, b4Result };
            messageTable = new ListBox[ModuleCount] { a1Message, a2Message, a3Message, a4Message, 
                b1Message, b2Message, b3Message, b4Message };

            failCount = new int[ModuleCount];
            passCount = new int[ModuleCount];

            failTable = new Label[ModuleCount] { a1FailCount, a2FailCount, a3FailCount, a4FailCount, 
                b1FailCount, b2FailCount, b3FailCount, b4FailCount };

            totalTable = new Label[ModuleCount] { a1TotalCount, a2TotalCount, a3TotalCount, a4TotalCount, 
                b1TotalCount, b2TotalCount, b3TotalCount, b4TotalCount };

            yieldTable = new Label[ModuleCount] { a1Yield, a2Yield, a3Yield, a4Yield, 
                            b1Yield, b2Yield, b3Yield, b4Yield };

            for (int i = 0; i < 8; ++i)
            {
                if (Program.duts[7 - i] == '1')
                {
                    try
                    {
                        comSelTable[i].Items.Add(profile.DutsPort[i]);
                        comSelTable[i].SelectedIndex = 0;
                        comSelTable[i].Enabled = false;
                    }
                    catch
                    {
                    }
                    disableTable[i].Enabled = false;
                    disableTable[i].Checked = false;
                }
            }

            initBackgroundWorker();
            testTimer.Tick += new EventHandler(TimerEventProcessor);
            cmdTimer.Tick += new EventHandler(TimerEventProcessor);
            cmdTimer.Interval = 100;
            cmdTimer.Start();

            const string keyPath = "Software\\Skytraq\\FinalTestV8";
            string siteAckKey = "SiteAck" + Program.siteNumber.ToString();
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
            if (key != null)
            {
                key.SetValue(siteAckKey, "Ready", Microsoft.Win32.RegistryValueKind.String);
            }
            key.Close();
        }

        private void SendResult()
        {
            const string keyPath = "Software\\Skytraq\\FinalTestV8";
            string siteAckKey = "SiteAck" + Program.siteNumber.ToString();
            string ack = "T ";
            for (int i = 0; i < ModuleCount; ++i)
            {
                ack += (testingResult[ModuleCount - i - 1] == 0) ? '0' : '1';
            }

            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
            if (key != null)
            {
                key.SetValue(siteAckKey, ack, Microsoft.Win32.RegistryValueKind.String);
            }
            key.Close();
        }

        private void EnableButton(bool e)
        {

        }

        private int FindIndex(object sender)
        {
            if (sender is CheckBox)
            {
                for (int i = 0; i < ModuleCount; i++)
                {
                    if (sender == disableTable[i])
                        return i;
                }
                return -1;
            }
            if (sender is ComboBox)
            {
                for (int i = 0; i < ModuleCount; i++)
                {
                    if (sender == comSelTable[i])
                        return i;
                }
                return -1;
            }
            return -1;
        }

        private void StopTesting()
        {
            testTimer.Stop();
            StopAllWorker();
        }

        // This is the method to run when the timer is raised.
        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            if (myObject == cmdTimer)
            {
                const string keyPath = "Software\\Skytraq\\FinalTestV8";
                string siteCmdKey = "SiteCmd" + Program.siteNumber.ToString();
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath);
                string s = key.GetValue(siteCmdKey).ToString();
                key.Close();
                if (s != "")
                {
                    key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
                    if (key != null)
                    {
                        key.SetValue(siteCmdKey, "", Microsoft.Win32.RegistryValueKind.String);
                    }
                    key.Close();
                    StartTesting(s);
                }

                return;
            }
            if (!CheckTestBusy(false))
            {
                CancelTest(false);
                return;
            } 
            StopTesting();
        }

        private void disable_CheckedChanged(object sender, EventArgs e)
        {
            /*
            int index = FindIndex(sender);
            (panelTable[index] as Panel).Enabled = !(sender as CheckBox).Checked;
            UpdateSettingByUI();
            FinalTestV8.Properties.Settings.Default.Save();
             * */
        }

        private void comSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSettingByUI();
            //FinalTestV8.Properties.Settings.Default.Save();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //FinalTestV8.Properties.Settings.Default.Save();
        }

        private BackgroundWorker[] bkWorker = new BackgroundWorker[ModuleCount];

        private void initBackgroundWorker()
        {
            for (int i = 0; i < ModuleCount; i++)
            {
                bkWorker[i] = new BackgroundWorker();
                bkWorker[i].WorkerReportsProgress = true;
                bkWorker[i].WorkerSupportsCancellation = true;
                bkWorker[i].DoWork += new DoWorkEventHandler(bw_DoWork);
                bkWorker[i].ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                bkWorker[i].RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

                testParam[i] = new WorkerParam();
                testParam[i].index = i;
                testParam[i].bw = bkWorker[i];
                testParam[i].gps = new SkytraqGps();
                testParam[i].parser = new GpsMsgParser();
                testParam[i].log = new StringBuilder();
            }
        }
        private WorkerParam[] testParam = new WorkerParam[ModuleCount];
        private static System.Windows.Forms.Timer testTimer = new System.Windows.Forms.Timer();
        private static System.Windows.Forms.Timer cmdTimer = new System.Windows.Forms.Timer();
        private enum TestStatus
        {
            NotReady,
            Ready,
            Waiting,
            GoldenLaunched,
            Testing,
            Downloading,
            Finished
        } 
        private TestStatus TestRunning { get; set; }
        //private static int[] baudRateTable = { 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
        private bool IsDeviceChecked(int index)
        {
            CheckBox c = disableTable[index] as CheckBox;
            if (c == null || c.Checked)
            {
                return false;
            }
            if ((comSelTable[index] as ComboBox).SelectedIndex < 0)
            {   // doesn't select a baud rate, disable it!
                c.Checked = true;
                return false;
            }
            return true;
        }

        private void StopAllWorker()
        {
            for (int i = 0; i < ModuleCount; i++)
            {
                if (!IsDeviceChecked(i))
                {
                    continue;
                }

                if (bkWorker[i].IsBusy)
                {
                    bkWorker[i].CancelAsync();
                }
             }
        }

        private void AddMessage(int i, String s)
        {
            ListBox b = messageTable[i] as ListBox;

            bool scroll = (b.TopIndex == b.Items.Count - (int)(b.Height / b.ItemHeight));
            b.Items.Add(s);
            if (scroll)
            {
                b.TopIndex = b.Items.Count - (int)(b.Height / b.ItemHeight);
            }
            testParam[i].log.AppendLine(s);

            StreamWriter w = File.AppendText(logPath[i]);
            if (s[s.Length - 1] == '\n')
            {
                w.Write("{0} {1} {2}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString(), s);
            }
            else
            {
                w.WriteLine("{0} {1} {2}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString(), s);
            } 
            w.Close();
        }

        private void LaunchTestDevice()
        {
            for (int i = 1; i < ModuleCount; i++)
            {
                if (!IsDeviceChecked(i))
                {
                    continue;
                }
                testParam[i].error = WorkerParam.ErrorType.TestNotComplete;
                bkWorker[i].RunWorkerAsync(testParam[i]);
                AddMessage(i, "-------------------- Begin testing --------------------");
                Thread.Sleep(20);
            }
        }
        private string testingDuts;
        private byte[] testingResult = new byte[ModuleCount];
        private bool StartTesting(string s)
        {
            rp.NewSession(SessionReport.SessionType.Testing);
            string duts = s.Substring(2, s.Length - 2);
            for (int i = 0; i < ModuleCount; i++)
            {
                if (duts[7 - i] != '1')
                {
                    disableTable[i].Checked = true;
                    continue;
                }
                disableTable[i].Checked = false;

                testParam[i].comPort = (comSelTable[i] as ComboBox).Text;
                testParam[i].profile = profile;
                testParam[i].log.Remove(0, testParam[i].log.Length);
                testParam[i].cmd = s[0];
                if (Program.module == "V822")
                {
                    testParam[i].fwProfile = fwProfile;
                }

                if (s[0] == 'L')
                {
                    SetResultDisplay(resultTable[i] as Label, ResultDisplayType.Downloading);
                }
                else
                {
                    SetResultDisplay(resultTable[i] as Label, ResultDisplayType.Testing);
                }
            }

            //TestModule.ClearResult();
            for (int i = 0; i < ModuleCount; i++)
            {
                if (duts[7 - i] != '1')
                {
                    continue;
                }
                bkWorker[i].RunWorkerAsync(testParam[i]);
            }
            TestRunning = TestStatus.Testing;
            testingDuts = duts;
            for(int i=0; i<ModuleCount; ++i)
            {
                testingResult[i] = 0;
            }
            return false;
        }

        //背景執行
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerParam p = e.Argument as WorkerParam;
            TestModule t = new TestModule();
            //p.startTime = DateTime.Now;
            Stopwatch w = new Stopwatch();
            w.Start();
            if (p.cmd == 'R' && Program.module == "V816")
            {
                e.Cancel = t.DoV816Test(p, TestModule.V816Set.Set1);
            }
            else if (p.cmd == 'S' && Program.module == "V816")
            {
                e.Cancel = t.DoV816Test(p, TestModule.V816Set.Set2);
            }
            else if (p.cmd == 'T' && Program.module == "V822")
            {
                e.Cancel = t.DoV822Test(p);
            }
            else if (p.cmd == 'L' && Program.module == "V822")
            {
                e.Cancel = t.DoV822Download(p);
            }
            else if(Program.module == "V815")
            {
                e.Cancel = t.DoTest815(p);
            }
            else 
            {
                e.Cancel = t.DoTest(p);
            }
            p.duration = w.ElapsedMilliseconds;
            w.Stop();
        }

        //處理進度
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //progressBar1.Value = e.ProgressPercentage;
            WorkerReportParam r = e.UserState as WorkerReportParam;
            if (r.reportType == WorkerReportParam.ReportType.ShowProgress)
            {
                AddMessage(r.index, r.output);
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowError)
            {
                WorkerParam.ErrorType er = testParam[r.index].error;
                {   //Test device error.
                    //
                    AddMessage(r.index, "Error : " + er.ToString());
                    AddMessage(r.index, "Error Code : " + WorkerParam.GetErrorString(er));
                    SetResultDisplay((resultTable[r.index] as Label), ResultDisplayType.Fail);
                    failCount[r.index]++;
                    UpdateSlotStatus(r.index);
                }
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowFinished)
            {

                AddMessage(r.index, "Test Completed.");
                SetResultDisplay((resultTable[r.index] as Label), ResultDisplayType.Pass);
                passCount[r.index]++;
                UpdateSlotStatus(r.index);
                testingResult[r.index] = 1;
            }
        }

        //執行完成
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker b = (sender as BackgroundWorker);

            int busyCount = GetBusyCount();
            if (0 == busyCount)
            {   //All task is done
                TestRunning = TestStatus.Finished;
                EnableButton(true);
                SendResult();
            }
        }

        private bool CheckTestBusy(bool includeGolden)
        {
            int start = 0;
            for (int i = start; i < ModuleCount; i++)
            {
                if (bkWorker[i].IsBusy)
                {
                    return true;
                }
            }
            return false;
        }

        private int GetBusyCount()
        {
            int count = 0;
            for (int i = 0; i < ModuleCount; i++)
            {
                if (bkWorker[i].IsBusy)
                {
                    count++;
                }
            }
            return count;
        }

        private void CancelTest(bool userCancel)
        {
            for (int i = 0; i < ModuleCount; i++)
            {
                if (bkWorker[i].IsBusy)
                {
                    bkWorker[i].CancelAsync();
                }
                if (i > 0 && userCancel)
                {
                    testParam[i].error |= WorkerParam.ErrorType.TestNotComplete;
                }
            }
            TestRunning = TestStatus.Finished;
            testTimer.Stop();
            //testCounter.Text = profile.snrTestPeriod.ToString();
            SetResultDisplay(resultTable[0] as Label, ResultDisplayType.None);
        }

        private void CancelDownload()
        {
            for (int i = 0; i < ModuleCount; i++)
            {
                if (bkWorker[i].IsBusy)
                {
                    bkWorker[i].CancelAsync();
                }
            }

            TestRunning = TestStatus.Finished;
            SetResultDisplay(resultTable[0] as Label, ResultDisplayType.None);
        }
        private void cancel_Click(object sender, EventArgs e)
        {
            CancelTest(true);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < ModuleCount; i++)
            {
                if (bkWorker[i] != null && bkWorker[i].IsBusy)
                {
                    MessageBox.Show("BackgroundWroker is still running!", "Title", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    break;
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }

        private const int DBT_DEVTYP_HANDLE = 6;
        private const int DBT_DEVTYP_PORT = 3;

        private const int BROADCAST_QUERY_DENY = 0x424D5144;
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000; // system detected a new device
        //private const int DBT_DEVICEQUERYREMOVE = 0x8001;   // Preparing to remove (any program can disable the removal)
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // removed 
        private const int DBT_DEVTYP_VOLUME = 0x00000002; // drive type is logical volume

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_PORT
        {
            public int dbcp_size;
            public int dbcp_devicetype;
            public int dbcp_reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public char[] dbcp_name;
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options optForm = new Options();
            optForm.StartPosition = FormStartPosition.CenterParent;
            if (DialogResult.OK == optForm.ShowDialog())
            {
                FinalTestV8.Properties.Settings o = FinalTestV8.Properties.Settings.Default;
                double[] gpSnrOffsetTable = {0, o.a1GpSnrOffset, o.a2GpSnrOffset, o.a3GpSnrOffset, o.a4GpSnrOffset, 
                                   o.b1GpSnrOffset, o.b2GpSnrOffset, o.b3GpSnrOffset, o.b4GpSnrOffset};
                double[] glSnrOffsetTable = {0, o.a1GlSnrOffset, o.a2GlSnrOffset, o.a3GlSnrOffset, o.a4GlSnrOffset, 
                                   o.b1GlSnrOffset, o.b2GlSnrOffset, o.b3GlSnrOffset, o.b4GlSnrOffset};
                double[] bdSnrOffsetTable = {0, o.a1BdSnrOffset, o.a2BdSnrOffset, o.a3BdSnrOffset, o.a4BdSnrOffset, 
                                   o.b1BdSnrOffset, o.b2BdSnrOffset, o.b3BdSnrOffset, o.b4BdSnrOffset};

                for (int i = 0; i < ModuleCount; i++)
                {
                    testParam[i].gpSnrOffset = gpSnrOffsetTable[i];
                    testParam[i].glSnrOffset = glSnrOffsetTable[i];
                    testParam[i].bdSnrOffset = bdSnrOffsetTable[i];
                }

            }
   
        }

        private void hiddenNotify_TextChanged(object sender, EventArgs e)
        {
            if ((sender as TextBox).Text == "WaitingCancel")
            {
                CancelTest(true);
            }
        }

        private void generateReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ExcelWriter ew = new ExcelWriter();
            //ew.Test();
        }

        private void errorMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String errorFile = null;
            if (Global.functionType == Global.FunctionType.FinalTest3)
            {
                errorFile = "Error.txt";
            }


            using (System.IO.StreamWriter file = new System.IO.StreamWriter(errorFile))
            {
                file.WriteLine("Error " + 0.ToString() + " : " + ((WorkerParam.ErrorType)((ulong)0)).ToString());

                for (int i = 1; i <= WorkerParam.ErrorCount; i++)
                {
                    file.WriteLine("Error " + i.ToString() + " : " + ((WorkerParam.ErrorType)((ulong)1 << i)).ToString());
                }

            }

            Process notePad = new Process();
            notePad.StartInfo.FileName = "notepad.exe";
            notePad.StartInfo.Arguments = errorFile;
            notePad.Start();
        }

    }

    public class SessionReport
    {
        public enum SessionType { Testing, Download }

        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public SessionType sessionType { get; set; }

        public void NewSession(SessionType s)
        {
            sessionType = s;
            startTime = DateTime.Now;
        }
        public void EndSession()
        {
            endTime = DateTime.Now;
        }

    }
}
