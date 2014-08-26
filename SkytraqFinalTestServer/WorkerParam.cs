    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.ComponentModel;

namespace SkytraqFinalTestServer
{
    public class WorkerParam
    {
        [Flags]
        public enum ErrorType : ulong
        {
            NoError = 0,
            WaitingPositionFixTimeout = (ulong)1 << 1,
            TestErr10 = (ulong)1 << 44,
        }
        public const int ErrorCount = 1;

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

        public BackgroundWorker bw;
        public ErrorType error;
        public long duration;
    }

    public class WorkerReportParam
    {

        public enum ReportType
        {
            ShowProgress,
            ShowIP,
            AllTaskFinished,
            GotMessage,

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
        public ReportType reportType { get; set; }
    }

    
}
