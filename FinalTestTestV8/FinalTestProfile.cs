using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;

namespace FinalTestV8
{
    public class Crc32
    {
        private static uint[] table;

        public uint ComputeChecksum(byte[] bytes)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }

        public uint ComputeChecksum(String s)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(s);
            return ComputeChecksum(bytes);
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            return BitConverter.GetBytes(ComputeChecksum(bytes));
        }

        private void CreateCrc32Table()
        {
            uint poly = 0xedb88320;
            table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ poly);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                table[i] = temp;
            }
        }

        public Crc32()
        {
            if(table==null)
            {
                CreateCrc32Table();
            }
        }
    }

    public class ModuleTestProfile
    {
        public int gdBaudSel { get; set; }
        //Module
        public int moduleType { get; set; }
        public int gpModuleSel { get; set; }
        public int glModuleSel { get; set; }
        public int bdModuleSel { get; set; }
        public int gaModuleSel { get; set; }
        public String moduleName { get; set; }
        //Device
        public String iniFileName { get; set; }
        public int snrTestPeriod { get; set; }
        public bool testGpSnr { get; set; }
        public bool testGlSnr { get; set; }
        public bool testBdSnr { get; set; }
        public bool testGaSnr { get; set; }
        //public int gpPassSel { get; set; }
        //public int glPassSel { get; set; }
        //public int bdPassSel { get; set; }
        //public int gaPassSel { get; set; }

        public int gpSnrUpper { get; set; }
        public int gpSnrLower { get; set; }
        public int glSnrUpper { get; set; }
        public int glSnrLower { get; set; }
        public int bdSnrUpper { get; set; }
        public int bdSnrLower { get; set; }
        public int gaSnrUpper { get; set; }
        public int gaSnrLower { get; set; }

        public int gpSnrLimit { get; set; }
        public int glSnrLimit { get; set; }
        public int bdSnrLimit { get; set; }
        public int gaSnrLimit { get; set; }
        //Testing
        public bool writeTag { get; set; }
        public bool enableDownload { get; set; }
        public int  dlBaudSel { get; set; }

        //public bool testBootStatus { get; set; }
        public bool checkPromCrc { get; set; }
        public bool testClockOffset { get; set; }
        public double clockOffsetThreshold { get; set; }
        public bool writeClockOffset { get; set; }
        public bool testEcompass { get; set; }
        public bool testMiniHommer { get; set; }
        public bool testDrCyro { get; set; }
        public int testDrDuration { get; set; }
        public double uslClockWise { get; set; }
        public double uslAnticlockWise { get; set; }
        public double lslClockWise { get; set; }
        public double lslAnticlockWise { get; set; }
        public double thresholdCog { get; set; }

        public class FirmwareProfile
        {
            public String promFile { get; set; }
            public String kVersion { get; set; }
            public String sVersion { get; set; }
            public String rVersion { get; set; }
            public UInt32 crc { get; set; }
            public String crcTxt { get; set; }
            public int dvBaudRate { get; set; }
            public UInt32 tagAddress { get; set; }
            public UInt32 tagContent { get; set; }
            public byte[] promRaw = null;

            public byte CalcPromRawCheckSum()
            {
                byte c = 0;
                foreach (byte b in promRaw)
                {
                    c += b;
                }
                return c;
            }

            public bool ReadePromRawData(String path)
            {
                //promRaw
                if (!File.Exists(path))
                {
                    return false;
                }
                try
                {
                    promRaw = File.ReadAllBytes(path);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            public bool GenerateXml(ref XmlElement item, XmlDocument doc)
            {
                XmlElement itemData = doc.CreateElement("ItemData");
                itemData.SetAttribute("PF", promFile.ToString());
                itemData.SetAttribute("KV", kVersion.ToString());
                itemData.SetAttribute("SV", sVersion.ToString());
                itemData.SetAttribute("RV", rVersion.ToString());
                itemData.SetAttribute("CR", crc.ToString());
                itemData.SetAttribute("BR", dvBaudRate.ToString());
                itemData.SetAttribute("TA", tagAddress.ToString());
                itemData.SetAttribute("TC", tagContent.ToString());
                itemData.SetAttribute("PS", (promRaw==null) ? "0" : promRaw.Length.ToString());
                item.AppendChild(itemData);

                Crc32 crc32 = new Crc32();
                XmlElement itemKey = doc.CreateElement("ItemKey");
                itemKey.SetAttribute("Key", crc32.ComputeChecksum(itemData.OuterXml).ToString());
                item.AppendChild(itemKey);

                return true;
            }

        }

        public static String[] CriteriaStrings = new String[] {
                "0", "+1 ~ -1", "+2 ~ -2", "-2",
                "+3 ~ -3", "-3", "+5 ~ -5", "-5" };

        public static String GpsCriteriaStrings(int lower, int upper)
        {
            return lower.ToString() + " ~ " + upper.ToString();
        }

        public static int GetPassSelUpperBound(int sel)
        {
            switch (sel)
            {
                case -1 :
                    return InitSnrUpper;
                case 0:
                    return 0;
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return MaxSnrValue;
                case 4:
                    return 3;
                case 5:
                    return MaxSnrValue;
                case 6:
                    return 5;
                case 7:
                    return MaxSnrValue;
                default:
                    return 0;
            }
        }
        public static int GetPassSelLowerBound(int sel)
        {
            switch (sel)
            {
                case -1:
                    return InitSnrLower;
                case 0:
                    return 0;
                case 1:
                    return -1;
                case 2:
                    return -2;
                case 3:
                    return -2;
                case 4:
                    return -3;
                case 5:
                    return -3;
                case 6:
                    return -5;
                case 7:
                    return -5;
                default:
                    return 0;
            }
        }
        public FirmwareProfile fwProfile;
        public bool ReadePromIniFile()
        {
            //String path = loginInfo.currentPath + "\\" + iniFileName;
            String path = iniFileName;
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

        public enum ErrorCode
        {
            NoError,
            InvalidateFormat,
        }
        public ErrorCode error { get; set; }
        public const int MaxSnrValue = 999;
        public const int MinSnrValue = -999;

        private const int MaxReadLength = 512;
        private const int InitGoldenBaudrate = 1;
        private const int InitPassCriteria = -1;
        private const int InitSnrUpper = 5;
        private const int InitSnrLower = -2;

        private const int InitSnrLimit = 48;
        private const int InitSnrTestPeriod = 10;
        private const int InitDownloadBaudrate = 7;

        private const String InitIniFileName = "prom.ini";
        private const double InitClockOffsetThreshold = 2.5;
        private const int InitDrDuration = 10;

        public ModuleTestProfile()
        {
            error = ErrorCode.NoError;
            gdBaudSel = InitGoldenBaudrate;
            //dvBaudRateIdx = 1;
            //gpPassSel = InitPassCriteria;
            //glPassSel = InitPassCriteria;
            //bdPassSel = InitPassCriteria;
            //gaPassSel = InitPassCriteria;
            gpSnrUpper = InitSnrUpper;
            glSnrUpper = InitSnrUpper;
            bdSnrUpper = InitSnrUpper;
            gaSnrUpper = InitSnrUpper;
            gpSnrLower = InitSnrLower;
            glSnrLower = InitSnrLower;
            bdSnrLower = InitSnrLower;
            gaSnrLower = InitSnrLower;

            snrTestPeriod = InitSnrTestPeriod;
            gpSnrLimit = InitSnrLimit;
            glSnrLimit = InitSnrLimit;
            bdSnrLimit = InitSnrLimit;
            gaSnrLimit = InitSnrLimit;
            testGpSnr = true;
            //testBootStatus = true;
            iniFileName = InitIniFileName;
            clockOffsetThreshold = InitClockOffsetThreshold;
            testDrDuration = InitDrDuration;
            dlBaudSel = InitDownloadBaudrate;
        }

        public ModuleTestProfile(ModuleTestProfile r)
        {
            gdBaudSel = r.gdBaudSel;
            moduleType = r.moduleType;
            gpModuleSel = r.gpModuleSel;
            glModuleSel = r.glModuleSel;
            bdModuleSel = r.bdModuleSel;
            gaModuleSel = r.gaModuleSel;
            moduleName = r.moduleName;
            //dvBaudRateIdx = r.dvBaudRateIdx;
            testGpSnr = r.testGpSnr;
            testGlSnr = r.testGlSnr;
            testBdSnr = r.testBdSnr;
            testGaSnr = r.testGaSnr;
            //gpPassSel = r.gpPassSel;
            //glPassSel = r.glPassSel;
            //bdPassSel = r.bdPassSel;
            //gaPassSel = r.gaPassSel;
            gpSnrUpper = r.gpSnrUpper;
            glSnrUpper = r.glSnrUpper;
            bdSnrUpper = r.bdSnrUpper;
            gaSnrUpper = r.gaSnrUpper;
            gpSnrLower = r.gpSnrLower;
            glSnrLower = r.glSnrLower;
            bdSnrLower = r.bdSnrLower;
            gaSnrLower = r.gaSnrLower;

            snrTestPeriod = r.snrTestPeriod;
            gpSnrLimit = r.gpSnrLimit;
            glSnrLimit = r.glSnrLimit;
            bdSnrLimit = r.bdSnrLimit;
            gaSnrLimit = r.gaSnrLimit;
            writeTag = r.writeTag;
            iniFileName = r.iniFileName;
            enableDownload = r.enableDownload;
            dlBaudSel = r.dlBaudSel;
            //promFileName = r.promFileName;
            //testBootStatus = r.testBootStatus;
            //testPromCrc = r.testPromCrc;
            //promCrc = r.promCrc;
            checkPromCrc = r.checkPromCrc;
            testClockOffset = r.testClockOffset;
            clockOffsetThreshold = r.clockOffsetThreshold;
            writeClockOffset = r.writeClockOffset;
            testEcompass = r.testEcompass;
            testMiniHommer = r.testMiniHommer;
            testDrCyro = r.testDrCyro;
            testDrDuration = r.testDrDuration;
            uslClockWise = r.uslClockWise;
            uslAnticlockWise = r.uslAnticlockWise;
            lslClockWise = r.lslClockWise;
            lslAnticlockWise = r.lslAnticlockWise;
            thresholdCog = r.thresholdCog;
            error = ErrorCode.NoError;

        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal, int size, string filePath);

        private String GetVerification(String path, bool isNewFile)
        {
            StreamReader sr = new StreamReader(path);
            UInt32[] hashTable = { 0xE3D930C4, 0x8CE0F048, 0x21697CF8, 0x36F78E93, 
                                     0x47DE9E6E, 0x58C8631E, 0x6683FFC4, 0x70BBDEFC };
            int tailPos = 0, lineCount = 0;
            bool inTheTail = false;

            Crc32 crc32 = new Crc32();

            while (!sr.EndOfStream)
            {   // Read one line until end of file.
                string line = sr.ReadLine();
                if (0 == line.CompareTo("[Verification]"))
                {
                    inTheTail = true;
                    tailPos = lineCount;
                }

                if (!inTheTail)
                {
                    uint ucrc = crc32.ComputeChecksum(line);
                    for(int i=0; i<hashTable.Length; i++)
                    {
                        //hashTable[i] = hashTable[i] ^ (UInt32)line.GetHashCode();
                        hashTable[i] = hashTable[i] ^ ucrc;
                    }
                }
                lineCount++;
            }
            sr.Close();
            if (lineCount != (tailPos + 2) && !isNewFile)
            {
                return "";
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (UInt32 a in hashTable)
            {
                sb.Append(a.ToString("X"));
            }
            return sb.ToString();
        }

        public bool LoadFromIniFile(String path)
        {
            String vKey = GetVerification(path, false);
            if (vKey.Length == 0)
            {
                error = ErrorCode.InvalidateFormat;
                return false;
            }

            StringBuilder temp = new StringBuilder(MaxReadLength);
            GetPrivateProfileString("Verification", "Key", "", temp, MaxReadLength, path);
            if (vKey.CompareTo(temp.ToString()) != 0)
            {
                error = ErrorCode.InvalidateFormat;
                return false;
            }

            GetPrivateProfileString("Golden", "Golden_Baud_Rate", InitGoldenBaudrate.ToString(), temp, MaxReadLength, path);
            gdBaudSel = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Module", "Module_Type", "0", temp, MaxReadLength, path);
            moduleType = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Module", "GPS_Module_Select", "0", temp, MaxReadLength, path);
            gpModuleSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Module", "Glonass_Module_Select", "0", temp, MaxReadLength, path);
            glModuleSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Module", "Beidou_Module_Select", "0", temp, MaxReadLength, path);
            bdModuleSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Module", "Galileo_Module_Select", "0", temp, MaxReadLength, path);
            gaModuleSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Module", "Module_Name", "", temp, MaxReadLength, path);
            moduleName = temp.ToString();
            
            GetPrivateProfileString("Device", "Ini_File", InitIniFileName, temp, MaxReadLength, path);
            iniFileName = temp.ToString();
            GetPrivateProfileString("Device", "SNR_Test_Period", InitSnrTestPeriod.ToString(), temp, MaxReadLength, path);
            snrTestPeriod = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Device", "Test_GPS_SNR", "True", temp, MaxReadLength, path);
            testGpSnr = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Device", "Test_Glonass_SNR", "False", temp, MaxReadLength, path);
            testGlSnr = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Device", "Test_Beidou_SNR", "False", temp, MaxReadLength, path);
            testBdSnr = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Device", "Test_Galileo_SNR", "False", temp, MaxReadLength, path);
            testGaSnr = Convert.ToBoolean(temp.ToString());

            GetPrivateProfileString("Device", "GPS_SNR_Criteria", "-1", temp, MaxReadLength, path);
            int gpPassSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Glonass_SNR_Criteria", "-1", temp, MaxReadLength, path);
            int glPassSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Beidou_SNR_Criteria", "-1", temp, MaxReadLength, path);
            int bdPassSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Galileo_SNR_Criteria", "-1", temp, MaxReadLength, path);
            int gaPassSel = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Device", "GPS_SNR_Upper_Bound", GetPassSelUpperBound(gpPassSel).ToString(), temp, MaxReadLength, path);
            gpSnrUpper = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "GPS_SNR_Lower_Bound", GetPassSelLowerBound(gpPassSel).ToString(), temp, MaxReadLength, path);
            gpSnrLower = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Glonass_SNR_Upper_Bound", GetPassSelUpperBound(glPassSel).ToString(), temp, MaxReadLength, path);
            glSnrUpper = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Glonass_SNR_Lower_Bound", GetPassSelLowerBound(glPassSel).ToString(), temp, MaxReadLength, path);
            glSnrLower = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Beidou_SNR_Upper_Bound", GetPassSelUpperBound(bdPassSel).ToString(), temp, MaxReadLength, path);
            bdSnrUpper = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Beidou_SNR_Lower_Bound", GetPassSelLowerBound(bdPassSel).ToString(), temp, MaxReadLength, path);
            bdSnrLower = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Galileo_SNR_Upper_Bound", GetPassSelUpperBound(gaPassSel).ToString(), temp, MaxReadLength, path);
            gaSnrUpper = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Galileo_SNR_Lower_Bound", GetPassSelLowerBound(gaPassSel).ToString(), temp, MaxReadLength, path);
            gaSnrLower = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Device", "GPS_SNR_Limit", InitSnrLimit.ToString(), temp, MaxReadLength, path);
            gpSnrLimit = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Glonass_SNR_Limit", InitSnrLimit.ToString(), temp, MaxReadLength, path);
            glSnrLimit = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Beidou_SNR_Limit", InitSnrLimit.ToString(), temp, MaxReadLength, path);
            bdSnrLimit = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Galileo_SNR_Limit", InitSnrLimit.ToString(), temp, MaxReadLength, path);
            gaSnrLimit = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Testing", "Write_Tag", "False", temp, MaxReadLength, path);
            writeTag = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Enable_Download", "False", temp, MaxReadLength, path);
            enableDownload = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Download_Baud_Rate", InitDownloadBaudrate.ToString(), temp, MaxReadLength, path);
            dlBaudSel = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Testing", "Check_Prom_Crc", "False", temp, MaxReadLength, path);
            checkPromCrc = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_Colok_Offset", "False", temp, MaxReadLength, path);
            testClockOffset = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Clock_Offset_Threshold", InitClockOffsetThreshold.ToString(), temp, MaxReadLength, path);
            clockOffsetThreshold = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "Write_Clock_Offset", "False", temp, MaxReadLength, path);
            writeClockOffset = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_E_Compass", "False", temp, MaxReadLength, path);
            testEcompass = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_miniHommer", "False", temp, MaxReadLength, path);
            testMiniHommer = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_DR_Cyro", "False", temp, MaxReadLength, path);
            testDrCyro = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_DR_Duration", InitDrDuration.ToString(), temp, MaxReadLength, path);
            testDrDuration = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Testing", "USL_Clockwise", "0", temp, MaxReadLength, path);
            uslClockWise = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "USL_Anticlockwise", "0", temp, MaxReadLength, path);
            uslAnticlockWise = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "LSL_Clockwise", "0", temp, MaxReadLength, path);
            lslClockWise = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "LSL_Anticlockwise", "0", temp, MaxReadLength, path);
            lslAnticlockWise = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "Threshold_Of_Cog", "0", temp, MaxReadLength, path);
            thresholdCog = Convert.ToDouble(temp.ToString());

            return true;
        }
        public bool SaveToIniFile(String path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            WritePrivateProfileString("Golden", "Golden_Baud_Rate", gdBaudSel.ToString(), path);

            WritePrivateProfileString("Module", "Module_Type", moduleType.ToString(), path);
            WritePrivateProfileString("Module", "GPS_Module_Select", gpModuleSel.ToString(), path);
            WritePrivateProfileString("Module", "Glonass_Module_Select", glModuleSel.ToString(), path);
            WritePrivateProfileString("Module", "Beidou_Module_Select", bdModuleSel.ToString(), path);
            WritePrivateProfileString("Module", "Galileo_Module_Select", gaModuleSel.ToString(), path);
            WritePrivateProfileString("Module", "Module_Name", moduleName, path);

            WritePrivateProfileString("Device", "Ini_File", iniFileName, path);
            WritePrivateProfileString("Device", "SNR_Test_Period", snrTestPeriod.ToString(), path);

            WritePrivateProfileString("Device", "Test_GPS_SNR", testGpSnr.ToString(), path);
            WritePrivateProfileString("Device", "Test_Glonass_SNR", testGlSnr.ToString(), path);
            WritePrivateProfileString("Device", "Test_Beidou_SNR", testBdSnr.ToString(), path);
            WritePrivateProfileString("Device", "Test_Galileo_SNR", testGaSnr.ToString(), path);

            //WritePrivateProfileString("Device", "GPS_SNR_Criteria", gpPassSel.ToString(), path);
            //WritePrivateProfileString("Device", "Glonass_SNR_Criteria", glPassSel.ToString(), path);
            //WritePrivateProfileString("Device", "Beidou_SNR_Criteria", bdPassSel.ToString(), path);
            //WritePrivateProfileString("Device", "Galileo_SNR_Criteria", gaPassSel.ToString(), path);
            WritePrivateProfileString("Device", "GPS_SNR_Upper_Bound", gpSnrUpper.ToString(), path);
            WritePrivateProfileString("Device", "GPS_SNR_Lower_Bound", gpSnrLower.ToString(), path);
            WritePrivateProfileString("Device", "Glonass_SNR_Upper_Bound", glSnrUpper.ToString(), path);
            WritePrivateProfileString("Device", "Glonass_SNR_Lower_Bound", glSnrLower.ToString(), path);
            WritePrivateProfileString("Device", "Beidou_SNR_Upper_Bound", bdSnrUpper.ToString(), path);
            WritePrivateProfileString("Device", "Beidou_SNR_Lower_Bound", bdSnrLower.ToString(), path);
            WritePrivateProfileString("Device", "Galileo_SNR_Upper_Bound", gaSnrUpper.ToString(), path);
            WritePrivateProfileString("Device", "Galileo_SNR_Lower_Bound", gaSnrLower.ToString(), path);

            WritePrivateProfileString("Device", "GPS_SNR_Limit", gpSnrLimit.ToString(), path);
            WritePrivateProfileString("Device", "Glonass_SNR_Limit", glSnrLimit.ToString(), path);
            WritePrivateProfileString("Device", "Beidou_SNR_Limit", bdSnrLimit.ToString(), path);
            WritePrivateProfileString("Device", "Galileo_SNR_Limit", gaSnrLimit.ToString(), path);

            WritePrivateProfileString("Testing", "Write_Tag", writeTag.ToString(), path);
            WritePrivateProfileString("Testing", "Enable_Download", enableDownload.ToString(), path);
            WritePrivateProfileString("Testing", "Download_Baud_Rate", dlBaudSel.ToString(), path);
            //WritePrivateProfileString("Testing", "Test_Flash_Boot", testBootStatus.ToString(), path);
            WritePrivateProfileString("Testing", "Test_Colok_Offset", testClockOffset.ToString(), path);
            WritePrivateProfileString("Testing", "Clock_Offset_Threshold", clockOffsetThreshold.ToString(), path);
            WritePrivateProfileString("Testing", "Write_Clock_Offset", writeClockOffset.ToString(), path);
            WritePrivateProfileString("Testing", "Test_E_Compass", testEcompass.ToString(), path);
            WritePrivateProfileString("Testing", "Test_miniHommer", testMiniHommer.ToString(), path);
            WritePrivateProfileString("Testing", "Test_DR_Cyro", testDrCyro.ToString(), path);
            WritePrivateProfileString("Testing", "Test_DR_Duration", testDrDuration.ToString(), path);
            WritePrivateProfileString("Testing", "USL_Clockwise", uslClockWise.ToString(), path);
            WritePrivateProfileString("Testing", "USL_Anticlockwise", uslAnticlockWise.ToString(), path);
            WritePrivateProfileString("Testing", "LSL_Clockwise", lslClockWise.ToString(), path);
            WritePrivateProfileString("Testing", "LSL_Anticlockwise", lslAnticlockWise.ToString(), path);
            WritePrivateProfileString("Testing", "Threshold_Of_Cog", thresholdCog.ToString(), path);

            String vKey = GetVerification(path, true);
            WritePrivateProfileString("Verification", "Key", vKey, path);
            return true;
        }
        public bool GenerateXml(ref XmlElement item, XmlDocument doc)
        {
            XmlElement itemData = doc.CreateElement("ItemData");
            itemData.SetAttribute("GB", gdBaudSel.ToString());
            itemData.SetAttribute("MT", moduleType.ToString());
            itemData.SetAttribute("MN", moduleName);
            itemData.SetAttribute("GPM", gpModuleSel.ToString());
            itemData.SetAttribute("GLM", glModuleSel.ToString());
            itemData.SetAttribute("BDM", bdModuleSel.ToString());
            itemData.SetAttribute("GAM", gaModuleSel.ToString());
            itemData.SetAttribute("STP", snrTestPeriod.ToString());
            itemData.SetAttribute("TGP", testGpSnr.ToString());
            itemData.SetAttribute("TGL", testGlSnr.ToString());
            itemData.SetAttribute("TBD", testBdSnr.ToString());
            itemData.SetAttribute("TGA", testGaSnr.ToString());
            itemData.SetAttribute("GPU", gpSnrUpper.ToString());
            itemData.SetAttribute("GPL", gpSnrLower.ToString());
            itemData.SetAttribute("GLU", glSnrUpper.ToString());
            itemData.SetAttribute("GLL", glSnrLower.ToString());
            itemData.SetAttribute("BDU", bdSnrUpper.ToString());
            itemData.SetAttribute("BDL", bdSnrLower.ToString());
            itemData.SetAttribute("GAU", gaSnrUpper.ToString());
            itemData.SetAttribute("GAL", gaSnrLower.ToString());
            itemData.SetAttribute("GPS", gpSnrLimit.ToString());
            itemData.SetAttribute("GLS", glSnrLimit.ToString());
            itemData.SetAttribute("BDS", bdSnrLimit.ToString());
            itemData.SetAttribute("GAS", gaSnrLimit.ToString());
            itemData.SetAttribute("WT", writeTag.ToString());
            itemData.SetAttribute("ED", enableDownload.ToString());
            itemData.SetAttribute("DB", dlBaudSel.ToString());
            itemData.SetAttribute("CPC", checkPromCrc.ToString());
            itemData.SetAttribute("TCO", testClockOffset.ToString());
            itemData.SetAttribute("COT", clockOffsetThreshold.ToString());
            itemData.SetAttribute("WCO", writeClockOffset.ToString());
            itemData.SetAttribute("TEC", testEcompass.ToString());
            itemData.SetAttribute("TMH", testMiniHommer.ToString());
            itemData.SetAttribute("TDC", testDrCyro.ToString());
            itemData.SetAttribute("TDD", testDrDuration.ToString());
            itemData.SetAttribute("USC", uslClockWise.ToString());
            itemData.SetAttribute("USA", uslAnticlockWise.ToString());
            itemData.SetAttribute("LSC", lslClockWise.ToString());
            itemData.SetAttribute("LSA", lslAnticlockWise.ToString());
            itemData.SetAttribute("TOC", thresholdCog.ToString());
            item.AppendChild(itemData);

            Crc32 crc32 = new Crc32();
            XmlElement itemKey = doc.CreateElement("ItemKey");
            itemKey.SetAttribute("Key", crc32.ComputeChecksum(itemData.OuterXml).ToString());
            item.AppendChild(itemKey);

            return true;
        }
    }
}
