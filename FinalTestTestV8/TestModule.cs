using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

namespace FinalTestV8
{
    class TestModule
    {
       // private static byte boostBaudrateIndex = 5;      //115200
        const int DefaultCmdTimeout = 1000;
        public static GpsMsgParser.ParsingStatus[] dvResult;
        public static UInt32 gdClockOffset = 0;
        public static void ClearResult()
        {
            if (dvResult == null)
            {
                return;
            }

            for (int i = 0; i < FinalTestForm.ModuleCount; i++)
            {
                dvResult[i].ClearAllSate();
                dvResult[i].positionFixResult = 0;
            }        
        }

        public TestModule()
        {
            if(dvResult == null)
            {
                dvResult = new GpsMsgParser.ParsingStatus[FinalTestForm.ModuleCount];
                for (int i = 0; i < FinalTestForm.ModuleCount; i++)
                {
                    dvResult[i] = new GpsMsgParser.ParsingStatus();
                }
            }
        }
        public enum V816Set
        {
            Set1,
            Set2
        };

        public bool DoTest(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            GPS_RESPONSE rep;
            Stopwatch sw = new Stopwatch(); //Count test fail timeout
            sw.Reset();
            sw.Start();

            rep = p.gps.Open(p.comPort, p.profile.baudRateIndex);

            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Open " + p.comPort + " in " +
                    p.gps.GetBaudRate().ToString() + " success.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            rep = p.gps.SendColdStart(3);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.ColdStartNack : WorkerParam.ErrorType.ColdStartTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Cold start success";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                Thread.Sleep(500);  //For venus 6 testing.
            }

            UInt32 rtc1 = 0, rtc2 = 0;
            rep = p.gps.QueryRtc(ref rtc1);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.QueryRtcNack : WorkerParam.ErrorType.QueryRtcTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Get RTC1 " + rtc1.ToString();
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                Thread.Sleep(1000);
                rep = p.gps.QueryRtc(ref rtc2);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.QueryRtcNack : WorkerParam.ErrorType.QueryRtcTimeOut;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Get RTC2 " + rtc2.ToString();
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                if ((rtc2 - rtc1) > 2 || (rtc2 - rtc1) < 1)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.CheckRtcError;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Check rtc pass";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }

            bool testPass = false;
            bool fixPass = false;
            do
            {
                byte[] buff = new byte[256];
                int l = p.gps.ReadLineNoWait(buff, 256, 2000);
                string line = Encoding.UTF8.GetString(buff, 0, l);
                if (GpsMsgParser.CheckNmea(line))
                {
                    //if (sw2.ElapsedMilliseconds > 2000)
                    //{   //if no NMEA input within 2 seconds, test fail.
                    //    break;
                    //}
                    //else
                    //{
                    //    sw2.Reset();
                    //    sw2.Start();
                    //}

                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = line;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));

                    GpsMsgParser.ParsingResult ps = p.parser.ParsingNmea(line);
                    if (ps == GpsMsgParser.ParsingResult.UpdateSate)
                    {   //Now is in position fixed status.
                        //if (p.parser.parsingStat.positionFixResult < 2)
                        //{   //Lost position fixed will test fail.
                        //    fixPass = false;
                        //    break;
                        //}
                        for (int i = 0; i < GpsMsgParser.ParsingStatus.MaxSattellite; i++)
                        {
                            GpsMsgParser.ParsingStatus.sateInfo s = p.parser.parsingStat.GetGpsSate(i);
                            if (s.prn == GpsMsgParser.ParsingStatus.NullValue)
                            {
                                break;
                            }
                            if (s.snr == 0 || s.snr == GpsMsgParser.ParsingStatus.NullValue)
                            {
                                continue;
                            }

                            if (s.snr >= p.profile.v815SnrBoundL && s.snr <= p.profile.v815SnrBoundU)
                            {
                                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                                r.output = "Prn:" + s.prn.ToString() + " SNR:" + s.snr.ToString() + " test pass";
                                p.bw.ReportProgress(0, new WorkerReportParam(r));

                                testPass = true;
                                break;
                            }
                        }
                        if (testPass)
                        {
                            break;
                        }
                    }
                    else if (!fixPass && ps == GpsMsgParser.ParsingResult.UpdateFixPosition)
                    {   //Now is not in position fixed status. it'll become fixed.
                        if (p.parser.parsingStat.positionFixResult >= 2)
                        {
                            fixPass = true;
                        }
                    }


                    //if (!fixPass && sw.ElapsedMilliseconds > 20 * 1000)
                    //{   //No position fixed more than 20 seconds, test fail!
                    //    testPass = false;
                    //    break;
                    //}
                    if (sw.ElapsedMilliseconds > p.profile.v815TestDuration * 1000)
                    {
                        //testPass = true;
                        break;
                    }
                }
                //if (sw2.ElapsedMilliseconds > 2000)
                //{
                //    testPass = false;
                //    break;
                //}
            } while (!p.bw.CancellationPending);

            if (!p.bw.CancellationPending)
            {
                rep = p.gps.FactoryReset();
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.FactoryResetNack : WorkerParam.ErrorType.FactoryResetTimeOut;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Factory Reset success";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }

            if (!testPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (fixPass) ? WorkerParam.ErrorType.NmeaError : WorkerParam.ErrorType.SnrError; ;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowFinished;
                p.error = WorkerParam.ErrorType.NoError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            EndProcess(p);
            return true;
        }
        
        public bool DoTest815(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            GPS_RESPONSE rep;
            Stopwatch sw = new Stopwatch(); //Count test fail timeout
            sw.Reset();
            sw.Start();

            rep = p.gps.Open(p.comPort, p.profile.baudRateIndex);

            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Open " + p.comPort + " in " +
                    p.gps.GetBaudRate().ToString() + " success.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            if (p.profile.v815TestColdStart == 1)
            {
                rep = p.gps.SendColdStart(3);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.ColdStartNack : WorkerParam.ErrorType.ColdStartTimeOut;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Cold start success";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    Thread.Sleep(500);  //For venus 6 testing.
                }
            }

            if (p.profile.v815TestRtcDelay != 0)
            {
                for (int i = 0; i < p.profile.v815TestRtcDelay; ++i)
                {
                    Thread.Sleep(1000);  //Delay
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Test RTC Delay " + (i + 1).ToString();
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }

            if (p.profile.v815TestRtc == 1)
            {
                // Test RTC
                UInt32 rtc1 = 0, rtc2 = 0;
                rep = p.gps.QueryRtc(ref rtc1);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.QueryRtcNack : WorkerParam.ErrorType.QueryRtcTimeOut;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Get RTC1 " + rtc1.ToString();
                    p.bw.ReportProgress(0, new WorkerReportParam(r));

                    Thread.Sleep(1010);
                    rep = p.gps.QueryRtc(ref rtc2);
                    if (GPS_RESPONSE.ACK != rep)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.QueryRtcNack : WorkerParam.ErrorType.QueryRtcTimeOut;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        EndProcess(p);
                        return false;
                    }
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Get RTC2 " + rtc2.ToString();
                    p.bw.ReportProgress(0, new WorkerReportParam(r));

                    if ((rtc2 - rtc1) > 2 || (rtc2 - rtc1) < 1)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.CheckRtcError;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        EndProcess(p);
                        return false;
                    }
                    else
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "Check rtc pass";
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }
                }
            }
            bool testPass = false;
            bool fixPass = false;
            do
            {
                byte[] buff = new byte[256];
                int l = p.gps.ReadLineNoWait(buff, 256, 2000);
                string line = Encoding.UTF8.GetString(buff, 0, l);
                if (GpsMsgParser.CheckNmea(line))
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = line;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));

                    GpsMsgParser.ParsingResult ps = p.parser.ParsingNmea(line);
                    if (ps == GpsMsgParser.ParsingResult.UpdateSate)
                    {   //Now is in position fixed status.
                        for (int i = 0; i < GpsMsgParser.ParsingStatus.MaxSattellite; i++)
                        {
                            GpsMsgParser.ParsingStatus.sateInfo s = p.parser.parsingStat.GetGpsSate(i);
                            if (s.prn == GpsMsgParser.ParsingStatus.NullValue)
                            {
                                break;
                            }
                            if (s.snr == 0 || s.snr == GpsMsgParser.ParsingStatus.NullValue)
                            {
                                continue;
                            }

                            if (s.snr >= p.profile.v815SnrBoundL && s.snr <= p.profile.v815SnrBoundU)
                            {
                                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                                r.output = "Prn:" + s.prn.ToString() + " SNR:" + s.snr.ToString() + " test pass";
                                p.bw.ReportProgress(0, new WorkerReportParam(r));

                                testPass = true;
                                break;
                            }
                        }
                        if (testPass)
                        {
                            break;
                        }
                    }
                    else if (!fixPass && ps == GpsMsgParser.ParsingResult.UpdateFixPosition)
                    {   //Now is not in position fixed status. it'll become fixed.
                        if (p.parser.parsingStat.positionFixResult >= 2)
                        {
                            fixPass = true;
                        }
                    }

                    if (sw.ElapsedMilliseconds > p.profile.v815TestDuration * 1000)
                    {
                        break;
                    }
                }
            } while (!p.bw.CancellationPending);

            if (!testPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (fixPass) ? WorkerParam.ErrorType.NmeaError : WorkerParam.ErrorType.SnrError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
            }  
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Check SNR pass";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            if (!p.bw.CancellationPending)
            {
                rep = p.gps.FactoryReset();
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.FactoryResetNack : WorkerParam.ErrorType.FactoryResetTimeOut;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Factory Reset success";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }

            //End Test
            r.reportType = WorkerReportParam.ReportType.ShowFinished;
            p.error = WorkerParam.ErrorType.NoError;
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            EndProcess(p);
            return true;
        }

        public bool DoV822Test(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            GPS_RESPONSE rep;
            Stopwatch sw = new Stopwatch(); //Count test fail timeout
            sw.Reset();
            sw.Start();

            //20150204 - Angus change to do ROM test in baud rate 115200.
            //rep = p.gps.Open(p.comPort, GpsBaudRateConverter.BaudRate2Index(p.fwProfile.dvBaudRate));
            int baudIdx = GpsBaudRateConverter.BaudRate2Index(115200);
            rep = p.gps.Open(p.comPort, baudIdx);

            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Open " + p.comPort + " in " + 
                    p.gps.GetBaudRate().ToString() + " success.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            rep = p.gps.SendColdStart(3);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.ColdStartNack : WorkerParam.ErrorType.ColdStartTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Cold start success";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                Thread.Sleep(500);  //For venus 6 testing.
            }

            if (p.profile.dlBaudSel != baudIdx)
            {
                rep = p.gps.ChangeBaudrate((byte)5, 2);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.ChangeBaudRateFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Change baud rate success";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }

            String dbgOutput = "";
            rep = p.gps.SendLoaderDownload(ref dbgOutput);
            if (GPS_RESPONSE.OK != rep)
            {
                if (dbgOutput != "")
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = dbgOutput;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }

                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.LoaderDownloadFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                if (dbgOutput != "")
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = dbgOutput;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }

                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Loader Download success";
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                rep = p.gps.UploadLoader(Properties.Resources.V822TesterSrec);
                if (GPS_RESPONSE.OK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.UploadLoaderFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }

                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Upload Loader success";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                Thread.Sleep(1000);
            }

            System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
            w.Reset();
            w.Start();

            bool ioTestPass = true;
            bool ioTestFinished = false;
            while (w.ElapsedMilliseconds < 5000)
            {
                byte[] buff = new byte[256];
                int l = p.gps.ReadLineNoWait(buff, 256, 2000);
                string line = Encoding.UTF8.GetString(buff, 0, l);

                if (line.Length > 0)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = line;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
                if (line.Contains("FINISH"))
                {
                    ioTestFinished = true;
                    break;
                }
                if (line.Contains("FAIL"))
                {
                    ioTestPass = false;
                    break;
                }

            };
            if (!ioTestFinished || !ioTestPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.IoTestFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "IO Test pass";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            Thread.Sleep(500);
            p.gps.Close();
            //rep = p.gps.Open(p.comPort, GpsBaudRateConverter.BaudRate2Index(p.fwProfile.dvBaudRate));
            rep = p.gps.Open(p.comPort, GpsBaudRateConverter.BaudRate2Index(115200));

            if (!p.bw.CancellationPending)
            {
                rep = p.gps.FactoryReset();
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.FactoryResetNack : WorkerParam.ErrorType.FactoryResetTimeOut;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Factory Reset success";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }
            
            r.reportType = WorkerReportParam.ReportType.ShowFinished;
            p.error = WorkerParam.ErrorType.NoError;
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            EndProcess(p);
            return true;
        }

        public bool DoV816Test(WorkerParam p, V816Set s)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            GPS_RESPONSE rep;
            Stopwatch sw = new Stopwatch(); //Count test fail timeout
            sw.Reset();
            sw.Start();

            rep = p.gps.Open(p.comPort, p.profile.baudRateIndex);

            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Open " + p.comPort + " in " +
                    p.gps.GetBaudRate().ToString() + " success.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            rep = p.gps.SendColdStart(3);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.ColdStartNack : WorkerParam.ErrorType.ColdStartTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Cold start success";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                Thread.Sleep(500);  //For venus 6 testing.
            }

            if (s == V816Set.Set1)
            {
                UInt32 rtc1 = 0, rtc2 = 0;
                rep = p.gps.QueryRtc(ref rtc1);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.QueryRtcNack : WorkerParam.ErrorType.QueryRtcTimeOut;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Get RTC1 " + rtc1.ToString();
                    p.bw.ReportProgress(0, new WorkerReportParam(r));

                    Thread.Sleep(1000);
                    rep = p.gps.QueryRtc(ref rtc2);
                    if (GPS_RESPONSE.ACK != rep)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.QueryRtcNack : WorkerParam.ErrorType.QueryRtcTimeOut;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        EndProcess(p);
                        return false;
                    }
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Get RTC2 " + rtc2.ToString();
                    p.bw.ReportProgress(0, new WorkerReportParam(r));

                    if ((rtc2 - rtc1) > 2 || (rtc2 - rtc1) < 1)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.CheckRtcError;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        EndProcess(p);
                        return false;
                    }
                    else
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "Check rtc pass";
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }
                }
            }

            bool testPass = false;
            bool fixPass = false;
            do
            {
                byte[] buff = new byte[256];
                int l = p.gps.ReadLineNoWait(buff, 256, 2000);
                string line = Encoding.UTF8.GetString(buff, 0, l);
                if (GpsMsgParser.CheckNmea(line))
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = line;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));

                    GpsMsgParser.ParsingResult ps = p.parser.ParsingNmea(line);
                    if (ps == GpsMsgParser.ParsingResult.UpdateSate)
                    {   //Now is in position fixed status.
                        for (int i = 0; i < GpsMsgParser.ParsingStatus.MaxSattellite; i++)
                        {
                            GpsMsgParser.ParsingStatus.sateInfo si = p.parser.parsingStat.GetGpsSate(i);
                            if (si.prn == GpsMsgParser.ParsingStatus.NullValue)
                            {
                                break;
                            }
                            if (si.snr == 0 || si.snr == GpsMsgParser.ParsingStatus.NullValue)
                            {
                                continue;
                            }

                            if (si.snr >= p.profile.v816SnrBoundL && si.snr <= p.profile.v816SnrBoundU)
                            {
                                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                                r.output = "Prn:" + si.prn.ToString() + " SNR:" + si.snr.ToString() + " test pass";
                                p.bw.ReportProgress(0, new WorkerReportParam(r));

                                testPass = true;
                                break;
                            }
                        }
                        if (testPass)
                        {
                            break;
                        }
                    }
                    else if (!fixPass && ps == GpsMsgParser.ParsingResult.UpdateFixPosition)
                    {   //Now is not in position fixed status. it'll become fixed.
                        if (p.parser.parsingStat.positionFixResult >= 2)
                        {
                            fixPass = true;
                        }
                    }
                    if (sw.ElapsedMilliseconds > p.profile.v816TestDuration * 1000)
                    {
                        //testPass = true;
                        break;
                    }
                }
            } while (!p.bw.CancellationPending);

            if (!p.bw.CancellationPending)
            {
                rep = p.gps.FactoryReset();
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.FactoryResetNack : WorkerParam.ErrorType.FactoryResetTimeOut;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Factory Reset success";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }

            if (!testPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (fixPass) ? WorkerParam.ErrorType.NmeaError : WorkerParam.ErrorType.SnrError; ;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowFinished;
                p.error = WorkerParam.ErrorType.NoError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            EndProcess(p);
            return true;
        }

        private byte CalcCheckSum16(byte[] data, int start, int len)
        {
            UInt16 checkSum = 0;

            for (int i = 0; i < len; i += sizeof(UInt16))
            {
                UInt16 word = Convert.ToUInt16(data[start + i + 1] | data[start + i] << 8);
	            checkSum += word;
            }
            return Convert.ToByte(((checkSum >> 8) + (checkSum & 0xFF)) & 0xFF);
        }

        private int ScanBaudRate(WorkerParam p, int first)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;

            GPS_RESPONSE rep;
            int TestDeviceTimeout = 500;
            int[] testingOrder = { 5, 1, 0, 3, 2, 4 };

            if (first != -1)
            {
                rep = p.gps.Open(p.comPort, first);
                if (GPS_RESPONSE.UART_OK != rep)
                {   //This com port can't open.
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Open " + p.comPort + " fail!";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));

                    return -1;
                }

                TestDeviceTimeout = (first < 2) ? 1500 : 1000;
                rep = p.gps.TestDevice(TestDeviceTimeout, 2);
                if (GPS_RESPONSE.NACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Baud rate " + GpsBaudRateConverter.Index2BaudRate(first).ToString() + " invalid.";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    p.gps.Close();
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Found working baud rate " + GpsBaudRateConverter.Index2BaudRate(first).ToString() + ".";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return first;
                }
            }

            foreach (int i in testingOrder)
            {
                if (i == first)
                {
                    continue;
                }
                if (p.bw.CancellationPending)
                {
                    return -1;
                }

                rep = p.gps.Open(p.comPort, i);
                if (GPS_RESPONSE.UART_OK != rep)
                {   //This com port can't open.
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Open " + p.comPort + " fail!";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return -1;
                }

                TestDeviceTimeout = (i < 2) ? 1500 : 1000;
                rep = p.gps.TestDevice(TestDeviceTimeout, 3);
                if (GPS_RESPONSE.NACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Baud rate " + GpsBaudRateConverter.Index2BaudRate(i).ToString() + " invalid!";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    p.gps.Close();
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Found working baud rate " + GpsBaudRateConverter.Index2BaudRate(i).ToString() + ".";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return i;
                }
            }
            return -1;
        }

        private static int lastDeviceBaudIdx = -1;
        //private static int lastRomBaudIdx = 1;
        public bool DoV822Download(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            GPS_RESPONSE rep = GPS_RESPONSE.UART_OK;
            
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            if (p.fwProfile.promRaw == null)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.NoDownloadBinFile;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Scanning " + p.comPort + " baud rate...";
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            // Retry three times for disable auto uart firmware, it'll 
            // change uart output baud rate after 5 seconds.
            int baudIdx = -1;
            lastDeviceBaudIdx = 5;      //V822 boot in ROM mode 115200 bps.
            for (int i = 0; i < 3; ++i)
            {
                baudIdx = ScanBaudRate(p, lastDeviceBaudIdx);
                if (-1 != baudIdx)
                {
                    lastDeviceBaudIdx = baudIdx;
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Open " + p.comPort + " in " +
                        GpsBaudRateConverter.Index2BaudRate(baudIdx).ToString() +
                        " success.";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    break;
                }
                Thread.Sleep(50);
            }

            if (-1 == baudIdx)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }

            //String kVer = "";
            //String sVer = "";
            //String rev = "";
            //rep = p.gps.QueryVersion(DefaultCmdTimeout, ref kVer, ref sVer, ref rev);
            //if (GPS_RESPONSE.ACK != rep)
            //{
            //    r.reportType = WorkerReportParam.ReportType.ShowError;
            //    p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.QueryVersionNack : WorkerParam.ErrorType.QueryVersionTimeOut;
            //    p.bw.ReportProgress(0, new WorkerReportParam(r));
            //    EndProcess(p);
            //    return false;
            //}
            //else if (rev != "20130221")
            //{
            //    //Reboot to ROM Code
            //    rep = p.gps.SetRegister(2000, 0x2000F050, 0x00000000);
            //    if (GPS_RESPONSE.ACK != rep)
            //    {
            //        r.reportType = WorkerReportParam.ReportType.ShowError;
            //        p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.ColdStartNack : WorkerParam.ErrorType.ColdStartTimeOut;
            //        p.bw.ReportProgress(0, new WorkerReportParam(r));
            //        EndProcess(p);
            //        return false;
            //    }
            //    else
            //    {
            //        r.reportType = WorkerReportParam.ReportType.ShowProgress;
            //        r.output = "Reboot from ROM success";
            //        p.bw.ReportProgress(0, new WorkerReportParam(r));
            //        p.gps.Close();
            //        Thread.Sleep(3000);  //Waiting for reboot
            //    }

            //    // Retry three times for disable auto uart firmware, it'll 
            //    // change uart output baud rate after 5 seconds.
            //    baudIdx = -1;
            //    for (int i = 0; i < 3; ++i)
            //    {
            //        baudIdx = ScanBaudRate(p, lastRomBaudIdx);
            //        if (-1 != baudIdx)
            //        {
            //            lastRomBaudIdx = baudIdx;
            //            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            //            r.output = "Open " + p.comPort + " in " +
            //                GpsBaudRateConverter.Index2BaudRate(baudIdx).ToString() +
            //                " success.";
            //            p.bw.ReportProgress(0, new WorkerReportParam(r));
            //            break;
            //        }
            //        Thread.Sleep(50);
            //    }

            //    if (-1 == baudIdx)
            //    {
            //        r.reportType = WorkerReportParam.ReportType.ShowError;
            //        p.error = WorkerParam.ErrorType.OpenPortFail;
            //        p.bw.ReportProgress(0, new WorkerReportParam(r));
            //        EndProcess(p);
            //        return false;
            //    }
            //}

            //if ((p.fwProfile.tagAddress == 0 && p.fwProfile.tagContent == 0) ||
            //    (p.fwProfile.tagAddress == 0xAAAAAAAA && p.fwProfile.tagContent == 0x55555555))
            //{   //No tag, using rom loader command
            //    rep = p.gps.StartDownload((byte)p.profile.dlBaudSel);
            //    if (GPS_RESPONSE.ACK != rep)
            //    {
            //        r.reportType = WorkerReportParam.ReportType.ShowError;
            //        p.error = (rep == GPS_RESPONSE.NACK)
            //            ? WorkerParam.ErrorType.DownloadCmdNack
            //            : WorkerParam.ErrorType.DownloadCmdTimeOut;
            //        p.bw.ReportProgress(0, new WorkerReportParam(r));
            //        EndProcess(p);
            //        return false;
            //    }
            //    else
            //    {
            //        p.gps.Close();
            //        rep = p.gps.Open(p.comPort, p.profile.dlBaudSel);
            //        if (GPS_RESPONSE.UART_FAIL == rep)
            //        {
            //            r.reportType = WorkerReportParam.ReportType.ShowError;
            //            p.error = (rep == GPS_RESPONSE.NACK)
            //                ? WorkerParam.ErrorType.DownloadCmdNack
            //                : WorkerParam.ErrorType.DownloadCmdTimeOut;
            //            p.bw.ReportProgress(0, new WorkerReportParam(r));
            //            //EndProcess(p);
            //            return false;
            //        }
            //        r.reportType = WorkerReportParam.ReportType.ShowProgress;
            //        r.output = "Download command success";
            //        p.bw.ReportProgress(0, new WorkerReportParam(r));

            //        Thread.Sleep(1000);
            //    }
            //}
            //else
            {   //20150225 Always use external loader in download.
                if (p.profile.dlBaudSel != baudIdx)
                {
                    rep = p.gps.ChangeBaudrate((byte)p.profile.dlBaudSel, 2);
                    if (GPS_RESPONSE.ACK != rep)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.ChangeBaudRateFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        EndProcess(p);
                        return false;
                    }
                    else
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "Change baud rate success";
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }
                }

                String dbgOutput = "";
                rep = p.gps.SendLoaderDownload(ref dbgOutput);
                if (GPS_RESPONSE.OK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.LoaderDownloadFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    if (dbgOutput != "")
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = dbgOutput;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }

                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Loader Download success";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    
                    rep = p.gps.UploadLoader(LoaderData.v8TagLoader);
                    if (GPS_RESPONSE.OK != rep)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.UploadLoaderFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        EndProcess(p);
                        return false;
                    }  

                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Upload Loader success";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    Thread.Sleep(1000);
                }
            }

            if ((p.fwProfile.tagAddress == 0 && p.fwProfile.tagContent == 0) ||
                (p.fwProfile.tagAddress == 0xAAAAAAAA && p.fwProfile.tagContent == 0x55555555))
            {
                rep = p.gps.SendRomBinSize(p.fwProfile.promRaw.Length,
                    p.fwProfile.CalcPromRawCheckSum());
            }
            else
            {
                rep = p.gps.SendTagBinSize(p.fwProfile.promRaw.Length, p.fwProfile.CalcPromRawCheckSum(),
                    p.profile.dlBaudSel, p.fwProfile.tagAddress, p.fwProfile.tagContent);
            }
            if (GPS_RESPONSE.OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.BinsizeCmdTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Start update firmware";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            const int nFlashBytes = 8 * 1024;
            const int headerSize = 3;

            byte[] header = new byte[headerSize];
            int lSize = p.fwProfile.promRaw.Length;
            int sentBytes = 0;
            int totalByte = 0;
            UInt16 sequence = 0;
            int rawItr = 0;

            int failCount = 0;
            while (lSize > 0)
            {
                sentBytes = (lSize >= nFlashBytes) ? nFlashBytes : lSize;
                totalByte += sentBytes;

                header[0] = (byte)(sequence >> 24 & 0xFF);
                header[1] = (byte)(sequence & 0xff);
                header[2] = CalcCheckSum16(p.fwProfile.promRaw, rawItr, sentBytes);

                //p.gps.SendDataNoWait(header, headerSize);
                rep = p.gps.SendDataWaitStringAck(p.fwProfile.promRaw, rawItr, sentBytes, 10000, "OK\0");

                if (rep == GPS_RESPONSE.OK)
                {
                    sequence++;
                    lSize -= sentBytes;
                    rawItr += nFlashBytes;
                }
                else
                {
                    if (++failCount > 0)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.DownloadWriteFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        EndProcess(p);
                        return false;
                    }
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Write block respone " + rep.ToString() + ", retry " + failCount.ToString();
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    continue;
                }
                failCount = 0;
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "left " + lSize.ToString() + " bytes";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                //Thread.Sleep(100);
            }

            rep = p.gps.WaitStringAck(10000, "END\0");
            if (GPS_RESPONSE.OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.DownloadEndTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Total time : " + (sw.ElapsedMilliseconds / 1000).ToString() + " seconds";
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                r.reportType = WorkerReportParam.ReportType.ShowFinished;
                p.error = WorkerParam.ErrorType.NoError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            EndProcess(p);
            return true;
        }

        private void EndProcess(WorkerParam p)
        {
            p.gps.Close();
        }    
    }
}
