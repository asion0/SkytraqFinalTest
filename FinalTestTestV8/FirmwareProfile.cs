using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FinalTestV8
{
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
    }

}
