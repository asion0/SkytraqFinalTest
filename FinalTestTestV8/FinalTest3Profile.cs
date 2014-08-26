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
        public int v815SnrBoundU = 45;
        public int v815SnrBoundL = 45;
        public int v815TestDuration = 10;

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
                n = GetPrivateProfileString("V815", "SNR_U_BOUND", "45", temp, 255, path);
                v815SnrBoundU = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V815", "SNR_L_BOUND", "40", temp, 255, path);
                v815SnrBoundL = Convert.ToInt32(temp.ToString());
                n = GetPrivateProfileString("V815", "TEST_DURATION", "10", temp, 255, path);
                v815TestDuration = Convert.ToInt32(temp.ToString());
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
