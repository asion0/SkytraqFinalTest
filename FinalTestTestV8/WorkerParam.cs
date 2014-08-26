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
            OpenPortFail = (ulong)1 << 1,
            ChangeBaudRateFail = (ulong)1 << 2,
            ColdStartNack = (ulong)1 << 3,
            ColdStartTimeOut = (ulong)1 << 4,
            UploadEphemerisNack = (ulong)1 << 5,
            UploadEphemerisTimeout = (ulong)1 << 6,
            ConfigMessageOutputNack = (ulong)1 << 7,
            ConfigMessageOutputTimeOut = (ulong)1 << 8,
            ConfigNmeaOutputNack = (ulong)1 << 9,
            ConfigNmeaOutputTimeOut = (ulong)1 << 10,
            FactoryResetNack = (ulong)1 << 11,
            FactoryResetTimeOut = (ulong)1 << 12,
            NmeaError = (ulong)1 << 13,
            SnrError = (ulong)1 << 14,
            QueryRtcTimeOut = 1UL << 15,
            QueryRtcNack = 1UL << 16,
            CheckRtcError = 1UL << 17,
            LoaderDownloadFail = 1UL << 18,
            UploadLoaderFail = 1UL << 19,
            IoTestFail = 1UL << 20,
            DownloadCmdNack = 1UL << 21,
            DownloadCmdTimeOut = 1UL << 22,
            BinsizeCmdTimeOut = 1UL << 23,
            QueryVersionNack = 1UL << 24,
            QueryVersionTimeOut = 1UL << 25,
            DownloadWriteFail = 1UL << 26,
            DownloadEndTimeOut = 1UL << 27,
            NoDownloadBinFile = 1UL << 28,

            TestNotComplete = (ulong)1 << 29,
        }
        public const int ErrorCount = 30;

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
