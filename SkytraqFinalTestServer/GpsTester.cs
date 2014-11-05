using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SocketDemo;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SkytraqFinalTestServer
{
    class GpsTester
    {
        private class SiteTask
        {
            public int siteNumber = -1;  // Site 0 ~ 15
            //TcpClient tcp = null;          // For feedback message
            //Status st = Status.Uninitialize;
            enum Status
            {
                Uninitialize,
                TesterReady,
                TesterBusy,
            }
        }

        static SiteTask[] siteTask = new SiteTask[ServerForm.MaxSiteCount];
        static Process[] workingProcess = new Process[ServerForm.MaxSiteCount];
        public static void QuitAllProcess()
        {
            foreach (Process p in workingProcess)
            {
                if (p != null && p.HasExited == false)
                {
                    p.Kill();
                }
            }
        }

        private bool InitSite(CmdData cd)
        {
            string siteModuleKey = "SiteModule";
            string siteAckKey = "SiteAck" + cd.Siteno.ToString();
            string siteCmdKey = "SiteCmd" + cd.Siteno.ToString();

            const string keyPath = "Software\\Skytraq\\FinalTestV8";
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
            //if (key != null)
            {   //Clear all fields
                key.SetValue(siteModuleKey, cd.module, Microsoft.Win32.RegistryValueKind.String);
                key.SetValue(siteAckKey, "", Microsoft.Win32.RegistryValueKind.String);
                key.SetValue(siteCmdKey, "", Microsoft.Win32.RegistryValueKind.String);
            }
            key.Close();

            siteTask[cd.Siteno] = new SiteTask();
            siteTask[cd.Siteno].siteNumber = cd.Siteno;
            string ft3Path = Environment.CurrentDirectory + "\\FinalTestV8.exe";
            string param = cd.module + " " + cd.Siteno.ToString() + " " + cd.duts +
                " \"" + Environment.CurrentDirectory  + "\\SiteProfile.ini\" ";
            if (workingProcess[cd.Siteno] != null && workingProcess[cd.Siteno].HasExited == false)
            {
                workingProcess[cd.Siteno].Kill();
            }

            workingProcess[cd.Siteno] = Process.Start(ft3Path, param);
            Stopwatch sw = new Stopwatch(); //Count launch timeout
            sw.Reset();
            sw.Start();

            while (true)
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath);

                string s = key.GetValue(siteAckKey).ToString();
                key.Close();
                if (s == "Ready")
                {
                    key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
                    if (key != null)
                    {
                        key.SetValue(siteAckKey, "", Microsoft.Win32.RegistryValueKind.String);
                    }
                    key.Close();
                    return false;
                }
                Thread.Sleep(50);
                if (sw.ElapsedMilliseconds > 10000)  //Wait 10 seconds
                {
                    return true;
                }
            }
        }

        private bool DoInitial(ref CmdData cd)
        {
            if (ServerForm.noTestValue)
            {
                return false;
            } 
            if (siteTask[cd.Siteno] != null)
            {
                return false;
            }
            if (cd.duts == "00000000")
            {
                return false;
            }
            return InitSite(cd);
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal, int size, string filePath);

        private bool DoCheckIniCrc(UInt32 crc)
        {
            const int MaxReadLength = 512;
            StringBuilder temp = new StringBuilder(MaxReadLength);

            String path = Environment.CurrentDirectory + "\\prom.ini";
            if (0 == GetPrivateProfileString("Firmware", "CRC", "", temp, MaxReadLength, path))
            {
                return false;
            }
            UInt32 iniCrc = Convert.ToUInt32(temp.ToString(), 16);

            return crc == iniCrc;
        }

        private bool DoCheckPromCrc(UInt32 crc)
        {
            const int MaxReadLength = 512;
            StringBuilder temp = new StringBuilder(MaxReadLength);

            String path = Environment.CurrentDirectory + "\\prom.ini";
            if (0 == GetPrivateProfileString("Firmware", "Prom", "", temp, MaxReadLength, path))
            {
                return false;
            }

            int promCrc = 0;
            try
            {
                string filename = Environment.CurrentDirectory + "\\" + temp.ToString(); //
                var fs = new FileStream(filename, FileMode.Open);
                var len = (int)fs.Length;

                for (int i = 0; i < 0x80000 * 2; ++i)
                {
                    if (i < len)
                        promCrc += fs.ReadByte();
                    else
                        promCrc += 0xff;
                    promCrc &= 0xffff;
                }
                fs.Close();
            }
            catch
            {
                return false;
            }
            return crc == promCrc;
        }

        private bool DoWork(ref CmdData cd)
        {
            if (ServerForm.noTestValue)
            {
                Thread.Sleep(60 * 1000);
                cd.duts = "00000000";
                return false;
            } 

            if (workingProcess[cd.Siteno] == null || workingProcess[cd.Siteno].HasExited == true)
            {
                if (InitSite(cd))
                {
                    return true;
                }
            }

            const string keyPath = "Software\\Skytraq\\FinalTestV8";
            string siteAckKey = "SiteAck" + cd.Siteno.ToString();
            string siteCmdKey = "SiteCmd" + cd.Siteno.ToString();

            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
            if (key != null)
            {
                string value = "T ";
                switch (cd.cmdType)
                {
                    case CmdData.CmdType.Set1_Start:
                        value = "R ";       //V816 Set1 Test
                        break;
                    case CmdData.CmdType.Set2_Start:
                        value = "S ";       //V816 Set2 Test
                        break;
                    case CmdData.CmdType.Test_Start:
                        value = "T ";       //Normal Test V815, V822 Test
                        break;
                    case CmdData.CmdType.Load_Start:
                        value = "L ";       //Download Firmware, V822
                        break;
                    default:
                        break;
                }
                key.SetValue(siteCmdKey, value + cd.duts, Microsoft.Win32.RegistryValueKind.String);
            }
            key.Close();

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();

            while (true)
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath);

                string s = key.GetValue(siteAckKey).ToString();
                key.Close();
                if (s != "")
                {
                    cd.duts = s.Substring(s.Length - 8, 8);
                    key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
                    if (key != null)
                    {
                        key.SetValue(siteAckKey, "", Microsoft.Win32.RegistryValueKind.String);
                    }
                    key.Close();
                    return false;
                }
                Thread.Sleep(50);
                if (sw.ElapsedMilliseconds > 110000)
                {   //Timeout 100 seconds
                    workingProcess[cd.Siteno].Kill();
                    cd.duts = "00000000";
                    return false;
                }
            }
        }

        private bool DoLoad(ref CmdData cd)
        {
            if (ServerForm.noTestValue)
            {
                Thread.Sleep(60 * 1000);
                cd.duts = "00000000";
                return false;
            }

            if (workingProcess[cd.Siteno] == null || workingProcess[cd.Siteno].HasExited == true)
            {
                if (InitSite(cd))
                {
                    return true;
                }
            }

            const string keyPath = "Software\\Skytraq\\FinalTestV8";
            string siteAckKey = "SiteAck" + cd.Siteno.ToString();
            string siteCmdKey = "SiteCmd" + cd.Siteno.ToString();

            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
            if (key != null)
            {
                string value = "L " + cd.duts;
                key.SetValue(siteCmdKey, value, Microsoft.Win32.RegistryValueKind.String);
            }
            key.Close();

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();

            while (true)
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath);

                string s = key.GetValue(siteAckKey).ToString();
                key.Close();
                if (s != "")
                {
                    cd.duts = s.Substring(s.Length - 8, 8);
                    key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
                    if (key != null)
                    {
                        key.SetValue(siteAckKey, "", Microsoft.Win32.RegistryValueKind.String);
                    }
                    key.Close();
                    return false;
                }
                Thread.Sleep(50);
                if (sw.ElapsedMilliseconds > 110000)
                {   //Timeout 100 seconds
                    workingProcess[cd.Siteno].Kill();
                    cd.duts = "00000000";
                    return false;
                }
            }
        }

        //private static UInt32 crc = 0;
        //private static bool setCrc = false;
        public string DoCommand(string cmd, object tcp)
        {
            AddMessage("Received command " + cmd);

            CmdData cd = ParsingCmd(cmd);
            string retCmd = "";
            WorkerReportParam r = new WorkerReportParam();

            switch (cd.cmdType)
            {
                case CmdData.CmdType.Initial:
                    if(DoInitial(ref cd))
                    {
                        cd.cmdType = CmdData.CmdType.Err01;
                        retCmd = GetReturnCommand(cd);
                    }
                    else
                    {
                        cd.cmdType = CmdData.CmdType.Ready;
                        retCmd = GetReturnCommand(cd);
                    }
                    break;
                case CmdData.CmdType.Set1_Start:
                    if (DoWork(ref cd))
                    {
                        cd.cmdType = CmdData.CmdType.Err2;
                        retCmd = GetReturnCommand(cd);
                    }
                    else
                    {
                        cd.cmdType = CmdData.CmdType.Set1_End;
                        retCmd = GetReturnCommand(cd);
                        if (ServerForm.noAckValue)
                        {
                            retCmd = "";
                        }
                    }
                    break;
                case CmdData.CmdType.Set2_Start:
                    if (DoWork(ref cd))
                    {
                        cd.cmdType = CmdData.CmdType.Err3;
                        retCmd = GetReturnCommand(cd);
                    }
                    else
                    {
                        cd.cmdType = CmdData.CmdType.Set2_End;
                        retCmd = GetReturnCommand(cd);
                        if (ServerForm.noAckValue)
                        {
                            retCmd = "";
                        }
                    }
                    break;
                case CmdData.CmdType.Test_Start:
                    if (DoWork(ref cd))
                    {
                        cd.cmdType = CmdData.CmdType.Err02;
                        retCmd = GetReturnCommand(cd);
                    }
                    else
                    {
                        cd.cmdType = CmdData.CmdType.Test_End;
                        retCmd = GetReturnCommand(cd);
                        if (ServerForm.noAckValue)
                        {
                            retCmd = "";
                        }
                    }
                    break;
                case CmdData.CmdType.Load_Start:
                    if (DoWork(ref cd))
                    {
                        cd.cmdType = CmdData.CmdType.Err03;
                        retCmd = GetReturnCommand(cd);
                    }
                    else
                    {
                        cd.cmdType = CmdData.CmdType.Load_End;
                        retCmd = GetReturnCommand(cd);
                        if (ServerForm.noAckValue)
                        {
                            retCmd = "";
                        }
                    }
                    break;
                case CmdData.CmdType.Testmode:
                    if (cd.setCrc)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowCrc;
                        r.output = cd.crc.ToString("X4");
                        ServerForm.wp.bw.ReportProgress(0, new WorkerReportParam(r));
                        if (DoCheckIniCrc(cd.crc))
                        {
                            r.reportType = WorkerReportParam.ReportType.DisplayMode;
                            r.output = "Testing mode ready";
                        }
                        else
                        {
                            cd.cmdType = CmdData.CmdType.Err01;
                            r.reportType = WorkerReportParam.ReportType.DisplayError;
                            r.output = "Testing mode ini CRC error!";
                        }
                    }
                    else
                    {   //Must send CRC_ before this command
                        cd.cmdType = CmdData.CmdType.Err01;
                        r.reportType = WorkerReportParam.ReportType.DisplayError;
                        r.output = "Testing mode no CRC information!";
                    }
                    ServerForm.wp.bw.ReportProgress(0, new WorkerReportParam(r));
                    retCmd = GetReturnCommand(cd);
                    break;
                case CmdData.CmdType.Download:
                    if (cd.setCrc)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowCrc;
                        r.output = cd.crc.ToString("X4");
                        ServerForm.wp.bw.ReportProgress(0, new WorkerReportParam(r));
                        if (!DoCheckIniCrc(cd.crc))
                        {
                            cd.cmdType = CmdData.CmdType.Err02;
                            r.reportType = WorkerReportParam.ReportType.DisplayError;
                            r.output = "Download mode ini CRC error!";
                        }
                        else if(!DoCheckPromCrc(cd.crc))
                        {
                            cd.cmdType = CmdData.CmdType.Err02;
                            r.reportType = WorkerReportParam.ReportType.DisplayError;
                            r.output = "Download mode bin CRC error!";
                        }
                        else
                        {
                            r.reportType = WorkerReportParam.ReportType.DisplayMode;
                            r.output = "Download mode ready";
                        }
                    }
                    else
                    {   //Must send CRC_ before this command
                        cd.cmdType = CmdData.CmdType.Err02;
                        r.reportType = WorkerReportParam.ReportType.DisplayError;
                        r.output = "Download mode no CRC information!";
                    }
                    ServerForm.wp.bw.ReportProgress(0, new WorkerReportParam(r));
                    retCmd = GetReturnCommand(cd);
                    break;
                case CmdData.CmdType.LoadTest:
                    if (cd.setCrc)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowCrc;
                        r.output = cd.crc.ToString("X4");
                        ServerForm.wp.bw.ReportProgress(0, new WorkerReportParam(r));
                         if (!DoCheckIniCrc(cd.crc))
                        {
                            cd.cmdType = CmdData.CmdType.Err03;
                            r.reportType = WorkerReportParam.ReportType.DisplayError;
                            r.output = "Download+Testing mode ini CRC error!";
                        }
                        else if(!DoCheckPromCrc(cd.crc))
                        {
                            cd.cmdType = CmdData.CmdType.Err03;
                            r.reportType = WorkerReportParam.ReportType.DisplayError;
                            r.output = "Download+Testing mode bin CRC error!";
                        }
                        else
                        {
                            r.reportType = WorkerReportParam.ReportType.DisplayMode;
                            r.output = "Download+Testing mode ready";
                        }                    
                    }
                    else 
                    {   //Must send CRC_ before this command
                        cd.cmdType = CmdData.CmdType.Err03;
                        r.reportType = WorkerReportParam.ReportType.DisplayError;
                        r.output = "Download+Testing mode no CRC information!";
                    }
                    ServerForm.wp.bw.ReportProgress(0, new WorkerReportParam(r));
                    retCmd = GetReturnCommand(cd);
                    break;              
                default:
                    cd.cmdType = CmdData.CmdType.Err01;
                    return GetReturnCommand(cd);
            }

            CommunicationBase.SendMsg(retCmd, tcp as TcpClient);
            return retCmd;
        }

        private String GetReturnCommand(CmdData cd)
        {
            String cmd;

            if (cd.cmdType == CmdData.CmdType.Ready ||
                cd.cmdType == CmdData.CmdType.Set1_End ||
                cd.cmdType == CmdData.CmdType.Set2_End ||
                cd.cmdType == CmdData.CmdType.Test_End ||
                cd.cmdType == CmdData.CmdType.Load_End)
            {
                string dutResult = "";
                dutResult = cd.duts;

                cmd = "@" + cd.module + " " + cd.cmdType.ToString() + " " +
                    cd.Siteno.ToString("D2") + " " + dutResult +
                    "+";
            }
            else if (cd.cmdType == CmdData.CmdType.Err01 || 
                    cd.cmdType == CmdData.CmdType.Err02 ||
                    cd.cmdType == CmdData.CmdType.Err03 ||
                    cd.cmdType == CmdData.CmdType.Err04 ||
                    cd.cmdType == CmdData.CmdType.Err1 ||
                    cd.cmdType == CmdData.CmdType.Err2 ||
                    cd.cmdType == CmdData.CmdType.Err3 ||
                    cd.cmdType == CmdData.CmdType.Err4)
            {
                cmd = "@" + cd.module + " " + cd.cmdType.ToString() + " " +
                    cd.Siteno.ToString("D2") + "+";
            }
            else if (cd.cmdType == CmdData.CmdType.CRC_)
            {
                cmd = "@" + cd.module + " " + cd.cmdType.ToString() + cd.crc.ToString("X4") + " " +
                    cd.Siteno.ToString("D2") + " " + cd.duts + "+";
            }
            else if (cd.cmdType == CmdData.CmdType.Testmode ||
                    cd.cmdType == CmdData.CmdType.Download ||
                    cd.cmdType == CmdData.CmdType.LoadTest)
            {
                cmd = "@" + cd.module + " " + cd.cmdType.ToString() + " " +
                    cd.Siteno.ToString("D2") + " " + cd.duts + "+";
            }
            else
            {
                cmd = "";
            }

            AddMessage("Return : " + cmd);
            return cmd;
        }

        private void AddMessage(string msg)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = msg;
            ServerForm.wp.bw.ReportProgress(0, new WorkerReportParam(r));
        }

        public class CmdData
        {
            public enum CmdType
            {
                Unknown,
                Initial,
                Test_Start,
                Test_End,
                Load_Start,
                Load_End,
                CRC_,
                Testmode,
                Download,
                LoadTest,
                Set1_Start,
                Set1_End,
                Set2_Start,
                Set2_End,
                Ready,
                Err01,
                Err02,
                Err03,
                Err04,
                Err1,
                Err2,
                Err3,
                Err4
            };

            public int Siteno = -1;
            public bool setCrc = false;
            public UInt32 crc = 0;
            public String duts;
            public String module;
            public CmdType cmdType = CmdType.Unknown;

        }

        private CmdData ParsingCmd(string cmd)
        {
            CmdData cd = new CmdData();
            if (cmd.Length < 5)
            {
                AddMessage("Error : Invalid command format, length < 5");
                cd.cmdType = CmdData.CmdType.Unknown;
                return cd;
            }
            if (cmd[0] != '@' && cmd[cmd.Length - 1] != '+')
            {
                AddMessage("Error : Invalid command format, not begin in @ and not end in +");
                cd.cmdType = CmdData.CmdType.Unknown;
                return cd;
            }

            String[] param = null;
            try
            {
                char[] delimiterChars = { ' ' };
                param = cmd.Substring(1, cmd.Length - 2).Split(delimiterChars);
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                //return false;
            }

            if (param == null || param.Length < 4)
            {
                AddMessage("Error : Invalid command format, parameter count < 4");
                cd.cmdType = CmdData.CmdType.Unknown;
                return cd;
            }
            cd.module = param[0];
            cd.cmdType = CmdData.CmdType.Unknown;
            if (param[1] == "Initial")
            {
                cd.cmdType = CmdData.CmdType.Initial;
            }
            else if (param[1].StartsWith("Set1_Start"))
            {
                cd.cmdType = CmdData.CmdType.Set1_Start;
            }
            else if (param[1].StartsWith("Set2_Start"))
            {
                cd.cmdType = CmdData.CmdType.Set2_Start;
            }
            else if (param[1] == "Test_Start")
            {
                cd.cmdType = CmdData.CmdType.Test_Start;
            }
            else if (param[1] == "Load_Start")
            {
                cd.cmdType = CmdData.CmdType.Load_Start;
            }
            else if (param[1] == "Test_End")
            {
                cd.cmdType = CmdData.CmdType.Test_End;
            }
            else if (param[1].StartsWith("CRC_"))
            {
                cd.cmdType = CmdData.CmdType.CRC_;
                try
                {
                    cd.crc = Convert.ToUInt32(param[1].Substring(param[1].Length - 4, 4), 16);
                    cd.setCrc = true;
                }
                catch
                {
                    cd.setCrc = false;
                }
            }
            else if (param[1].StartsWith("Testmode_CRC_"))
            {
                cd.cmdType = CmdData.CmdType.Testmode;
                try
                {
                    cd.crc = Convert.ToUInt32(param[1].Substring(param[1].Length - 4, 4), 16);
                    cd.setCrc = true;
                }
                catch
                {
                    cd.setCrc = false;
                }
            }
            else if (param[1].StartsWith("Download_CRC_"))
            {
                cd.cmdType = CmdData.CmdType.Download;
                try
                {
                    cd.crc = Convert.ToUInt32(param[1].Substring(param[1].Length - 4, 4), 16);
                    cd.setCrc = true;
                }
                catch
                {
                    cd.setCrc = false;
                }
            }
            else if (param[1].StartsWith("LoadTest_CRC_"))
            {
                cd.cmdType = CmdData.CmdType.LoadTest;
                try
                {
                    cd.crc = Convert.ToUInt32(param[1].Substring(param[1].Length - 4, 4), 16);
                    cd.setCrc = true;
                }
                catch
                {
                    cd.setCrc = false;
                }
            }


            if (cd.cmdType == CmdData.CmdType.Unknown)
            {
                AddMessage("Error : Unknown command");
                cd.cmdType = CmdData.CmdType.Unknown;
                return cd;
            }

            cd.Siteno = -1;
            try
            {
                cd.Siteno = Convert.ToInt32(param[2]);
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                //AddMessage(e.ToString());
                //return false;
            }

            if (cd.Siteno < 0 || cd.Siteno > 15)
            {
                AddMessage("Error : Invalid Siteno");
                cd.cmdType = CmdData.CmdType.Unknown;
                return cd;
            }

            if (param[3].Length != 8)
            {
                AddMessage("Error : Invalid duts");
                cd.cmdType = CmdData.CmdType.Unknown;
                return cd;
            }

            cd.duts = param[3];
            for (int i = 0; i < 8; ++i)
            {
                if (cd.duts[i] != '0' && cd.duts[i] != '1')
                {
                    AddMessage("Error : Invalid duts");
                    cd.cmdType = CmdData.CmdType.Unknown;
                    return cd;
                }
            }
            return cd;
        }
    }
}
