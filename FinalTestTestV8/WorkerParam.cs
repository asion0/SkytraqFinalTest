using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace FinalTestV8
{
    public class WorkerParam
    {
        [Flags] public enum ErrorType : ulong
        {
            NoError = 0,
            //Download Error Code
            DownloadOpenPortFail = 1UL << 1,
            DownloadNoDownloadBinFile = 1UL << 2,
            DownloadBootNoNmea = 1UL << 3,
            DownloadColdStartTimeOut = 1UL << 4,
            DownloadLoaderDownloadFail = 1UL << 5,
            DownloadUploadLoaderFail = 1UL << 6,
            DownloadBinsizeCmdTimeOut = 1UL << 7,
            DownloadWriteFail = 1UL << 8,
            DownloadEndTimeOut = 1UL << 9,

            //Test Error Code
            TestOpenPortFail = 1UL << 33,
            TestBootNoNmea = 1UL << 34,
            TestColdStartTimeOut = 1UL << 35,
            TestLoaderDownloadFail = 1UL << 36,
            TestUploadLoaderFail = 1UL << 37,
            TestBinsizeCmdTimeOut = 1UL << 38,
            TestChangeBaudRateFail = 1UL << 39,
            TestQueryRtcTimeOut = 1UL << 40,
            TestQueryRtcNack = 1UL << 41,
            TestCheckRtcError = 1UL << 42,
            TestConfigMessageOutputTimeOut = 1UL << 43,
            TestConfigNmeaOutputTimeOut = 1UL << 44,
            TestSnrError = 1UL << 45,
            TestQueryChannelDopplerTimeout = 1UL << 46,
            TestQueryChannelClockOffsetTimeout = 1UL << 47,
            TestCheckClockOffsetFail = 1UL << 48,
            TestIoTestFail = 1UL << 49,
            TestFactoryResetTimeOut = 1UL << 50,
            TestCrcTimeOut = 1UL << 51,
            TestCheckCrcError = 1UL << 52,
            /*
            UploadEphemerisNack = (ulong)1 << 5,
            UploadEphemerisTimeout = (ulong)1 << 6,
            DownloadCmdNack = 1UL << 21,
            DownloadCmdTimeOut = 1UL << 22,
            QueryVersionNack = 1UL << 24,
            QueryVersionTimeOut = 1UL << 25,
            CommandNoneAck = 1UL << 29,
            CommandTimeout = 1UL << 30,
            */

            TestNotComplete = (ulong)1 << 63,
            //Test Error Code

        }
        public const int ErrorCount = 64;

        public static String GetErrorString(ErrorType er)
        {
            if (ErrorType.NoError == er)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            UInt64 nErr = (UInt64)er;
            bool first = true;
            for (byte i = 0; i < 64; i++)
            {
                UInt64 tt = nErr & ((UInt64)1 << i);

                if ((nErr & ((UInt64)1 << i)) != 0)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(i.ToString());
                    first = false;
                }
            }
            return sb.ToString();
        }

        public int index;
        public String comPort;
        public BackgroundWorker bw;
        public SkytraqGps gps;
        public GpsMsgParser parser;
        public FinalTest3Profile profile;
        public double gpSnrOffset;
        public double glSnrOffset;
        public double bdSnrOffset;
        public ErrorType error;
        public char cmd;
        //for Report
        //public DateTime startTime;
        public long duration;
        public StringBuilder log;
        public FirmwareProfile fwProfile;
    }

    public class WorkerReportParam
    {

        public enum ReportType
        {
            ShowProgress,
            GoldenSampleReady,
            UpdateSnrChart,
            ShowError,
            ShowFinished,
            ShowWaitingGoldenSample,
            HideWaitingGoldenSample,
            AllTaskFinished,
        }

        public WorkerReportParam()
        {

        }

        public WorkerReportParam(WorkerReportParam r)
        {
            index = r.index;
            output = r.output;
            reportType = r.reportType;
        }
        public int index { get; set; }
        public String output { get; set; }
        //public ErrorType error { get; set; }
        public ReportType reportType { get; set; }
    }
}
