using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SocketDemo;
using System.Net.Sockets;
using System.Diagnostics;

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
                    case CmdData.CmdType.Test_Start:
                        value = "T ";
                        break;
                    case CmdData.CmdType.Load_Start:
                        value = "L ";
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

        public string DoCommand(string cmd, object tcp)
        {
            AddMessage("Received command " + cmd);

            CmdData cd = ParsingCmd(cmd);
            string retCmd = "";
            switch (cd.cmdType)
            {
                case CmdData.CmdType.Initial:
                    if(DoInitial(ref cd))
                    {
                        cd.cmdType = CmdData.CmdType.Err1;
                        retCmd = GetReturnCommand(cd);
                    }
                    else
                    {
                        cd.cmdType = CmdData.CmdType.Ready;
                        retCmd = GetReturnCommand(cd);
                    }
                    break;
                case CmdData.CmdType.Test_Start:

                    if (DoWork(ref cd))
                    {
                        cd.cmdType = CmdData.CmdType.Err2;
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
                        cd.cmdType = CmdData.CmdType.Err3;
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
                default:
                    cd.cmdType = CmdData.CmdType.Err1;
                    return GetReturnCommand(cd);
            }

            CommunicationBase.SendMsg(retCmd, tcp as TcpClient);
            return retCmd;
        }

        private String GetReturnCommand(CmdData cd)
        {
            String cmd;

            if (cd.cmdType == CmdData.CmdType.Ready || 
                cd.cmdType == CmdData.CmdType.Test_End ||
                cd.cmdType == CmdData.CmdType.Load_End)
            {
                string dutResult = "";
                dutResult = cd.duts;

                cmd = "@" + cd.module + " " + cd.cmdType.ToString() + " " +
                    cd.Siteno.ToString("D2") + " " + dutResult +
                    "+";
            }
            else if (cd.cmdType == CmdData.CmdType.Err1 || 
                    cd.cmdType == CmdData.CmdType.Err2 ||
                    cd.cmdType == CmdData.CmdType.Err3)
            {
                cmd = "@" + cd.module + " " + cd.cmdType.ToString() + " " +
                    cd.Siteno.ToString("D2") + "+";
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
                Ready,
                Err1,
                Err2,
                Err3
            };

            public int Siteno = -1;
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

            if (param[0] == "V822")
            {
                if (!System.IO.File.Exists(Environment.CurrentDirectory + "\\prom.ini"))
                {
                    AddMessage("Error : Can't find file [prom.ini]");
                    cd.cmdType = CmdData.CmdType.Unknown;
                    return cd;
                }
            }
            return cd;
        }
    }
}
