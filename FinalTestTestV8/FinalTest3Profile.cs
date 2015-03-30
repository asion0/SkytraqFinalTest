using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace FinalTestV8
{
    public class FinalTest3Profile
    {
        public FinalTest3Profile(int siteNo, string iniPath)
        {
            Load(siteNo, iniPath);
        }
        
        public const int TesterDut = 8;
        public int baudRateIndex;
        public int dlBaudSel;
        public int testMode;
        public string[] DutsPort = new string[TesterDut];
        //Param for V815 Test
        public int v815TestColdStart = 1;
        public int v815TestRtc = 1;
        public int v815TestRtcDelay = 5;
        public int v815SnrBoundU = 45;
        public int v815SnrBoundL = 40;
        public int v815TestDuration = 10;
        //Param for V816 Test
        public int v816SnrBoundU = 45;
        public int v816SnrBoundL = 40;
        public int v816TestDuration = 10;
        //Param for V828 Test
        public int v828SnrBoundU = 45;
        public int v828SnrBoundL = 40;
        public int v828TestDuration = 10;
        //Param for V838 Test
        public int v838SnrBoundU = 45;
        public int v838SnrBoundL = 40;
        public int v838TestDuration = 10;

        private enum ErrorCode
        {
            NoError,
            NoGpsModule,
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal, int size, string filePath);

        private ErrorCode Load(int siteNo, string path)
        {
            StringBuilder temp = new StringBuilder(255);
            int n = GetPrivateProfileString("Setting", "Baud", "0", temp, 255, path);
            baudRateIndex = Convert.ToInt32(temp.ToString());
            n = GetPrivateProfileString("Setting", "Mode", "0", temp, 255, path);
            testMode = Convert.ToInt32(temp.ToString());
            n = GetPrivateProfileString("Setting", "DownloadBaud", "5", temp, 255, path);
            dlBaudSel = Convert.ToInt32(temp.ToString());

            if (Program.module == "V815")
            {
                n = GetPrivateProfileString("V815", "TEST_COLD_START", "1", temp, 255, path);
                v815TestColdStart = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V815", "TEST_RTC", "1", temp, 255, path);
                v815TestRtc = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V815", "TEST_RTC_DELAY", "5", temp, 255, path);
                v815TestRtcDelay = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V815", "SNR_U_BOUND", "45", temp, 255, path);
                v815SnrBoundU = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V815", "SNR_L_BOUND", "40", temp, 255, path);
                v815SnrBoundL = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V815", "TEST_DURATION", "10", temp, 255, path);
                v815TestDuration = Convert.ToInt32(temp.ToString());
            }

            if (Program.module == "V816")
            {
                n = GetPrivateProfileString("V816", "SNR_U_BOUND", "45", temp, 255, path);
                v816SnrBoundU = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V816", "SNR_L_BOUND", "40", temp, 255, path);
                v816SnrBoundL = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V816", "TEST_DURATION", "10", temp, 255, path);
                v816TestDuration = Convert.ToInt32(temp.ToString());
            }

            if (Program.module == "V828")
            {
                n = GetPrivateProfileString("V828", "SNR_U_BOUND", "45", temp, 255, path);
                v828SnrBoundU = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V828", "SNR_L_BOUND", "40", temp, 255, path);
                v828SnrBoundL = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V828", "TEST_DURATION", "10", temp, 255, path);
                v828TestDuration = Convert.ToInt32(temp.ToString());
            }

            if (Program.module == "V838")
            {
                n = GetPrivateProfileString("V838", "SNR_U_BOUND", "45", temp, 255, path);
                v838SnrBoundU = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V838", "SNR_L_BOUND", "40", temp, 255, path);
                v838SnrBoundL = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V838", "TEST_DURATION", "10", temp, 255, path);
                v838TestDuration = Convert.ToInt32(temp.ToString());
            }
            string session = "Site" + siteNo.ToString("D2");
            for (int i = 0; i < TesterDut; ++i)
            {
                string key = "DUT" + (i + 1).ToString();
                n = GetPrivateProfileString(session, key, "", temp, 255, path);
                Console.Write(n.ToString());
                if (n > 0)
                {
                    DutsPort[i] = temp.ToString();
                }
            }
            return ErrorCode.NoError;
        }

    }
}
