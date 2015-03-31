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
        //private static byte boostBaudrateIndex = 5;      //115200
        //const int DefaultCmdTimeout = 1000;
        public static GpsMsgParser.ParsingStatus[] dvResult;
        private static int lastDeviceBaudIdx = -1;

        public enum V816Set
        {
            Set1,
            Set2
        };
        //public static UInt32 gdClockOffset = 0;
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

        private int ScanBaudRate(WorkerParam p, WorkerReportParam r, int first)
        {
            GPS_RESPONSE rep;
            int TestDeviceTimeout = 500;
            int[] testingOrder = { 5, 1, 0, 3, 2, 4, 6, 7, 8 };

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

                //TestDeviceTimeout = (first < 2) ? 1500 : 1000;
                TestDeviceTimeout = (first < 2) ? 500 : 500;
                rep = p.gps.TestDevice(TestDeviceTimeout, 1);
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

                TestDeviceTimeout = (i < 2) ? 500 : 500;
                rep = p.gps.TestDevice(TestDeviceTimeout, 1);
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

        private bool OpenDevice(WorkerParam p, WorkerReportParam r, int baudIdx)
        {
            GPS_RESPONSE rep = p.gps.Open(p.comPort, baudIdx);

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
            return true;
        }

        private void ReopenDevice(WorkerParam p, WorkerReportParam r, int baudIdx, int delay)
        {
            p.gps.Close();
            Thread.Sleep(delay);
            p.gps.Open(p.comPort, baudIdx);
        }

        private bool DoColdStart(WorkerParam p, WorkerReportParam r, int retry)
        {
            GPS_RESPONSE rep = p.gps.SendColdStart(retry, 2000);
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
            return true;
        }

        private bool DoNmeaPrepareCommand(WorkerParam p, WorkerReportParam r)
        {
            GPS_RESPONSE rep = p.gps.ConfigMessageOutput(0x01);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.ConfigMessageOutputNack : WorkerParam.ErrorType.ConfigMessageOutputTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Config Message Output success";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            rep = p.gps.ConfigNmeaOutput(1, 1, 1, 0, 1, 1, 0, 0);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.ConfigNmeaOutputNack : WorkerParam.ErrorType.ConfigNmeaOutputTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Config NMEA Interval success";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            return true;
        }

        private void ShowDelay(WorkerParam p, WorkerReportParam r, int delay, string prompt)
        {
            for (int i = 0; i < delay; ++i)
            {
                Thread.Sleep(1000);  //Delay
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = prompt + (i + 1).ToString();
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
        }

        private bool TestRtc(WorkerParam p, WorkerReportParam r)
        {
            // Test RTC
            UInt32 rtc1 = 0, rtc2 = 0;
            GPS_RESPONSE rep = p.gps.QueryRtc(ref rtc1);
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
                if ((rtc2 - rtc1) > 3 || (rtc2 - rtc1) < 1)
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
            return true;
        }

        private bool TestSnr(WorkerParam p, WorkerReportParam r, ref Stopwatch sw, int lowerBound, int upperBound, int duration)
        {
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

                            if (s.snr >= lowerBound && s.snr <= upperBound)
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

                    if (sw.ElapsedMilliseconds > duration)
                    {
                        //testPass = true;
                        break;
                    }
                }
            } while (!p.bw.CancellationPending);

            if (!testPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.SnrError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            return testPass;
        }
        
        private bool BoostBaudRate(WorkerParam p, WorkerReportParam r, int baudIdx)
        {
            GPS_RESPONSE rep = p.gps.ChangeBaudrate((byte)baudIdx, 2);
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
                r.output = "Change baud rate to " + baudIdx.ToString() + " success";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            return true;
        }

        private bool DoIoSrecTest(WorkerParam p, WorkerReportParam r, string testSrec, string srecCmd, int timeout)
        {
            String dbgOutput = "";
            GPS_RESPONSE rep = p.gps.SendLoaderDownload(ref dbgOutput);
            if (dbgOutput != "")
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = dbgOutput;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
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
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Loader Download success";
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                rep = p.gps.UploadLoader(testSrec);
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
            //TEST01 = Flag IoCount io1 io2 io3 io4 ......
            //Flag - bit wise for test function : 0 - IO Test, 1 - GSN MAG Test, 2 - Rtc Test
            //IoCount - Test IO pair count
            //io1, io2... - High byte - gpio pin from, Low byte gpio pin to.
            if (srecCmd.Length > 0)
            {
                rep = p.gps.SendTestSrecCmd(srecCmd, 1000);
            }
            System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
            w.Reset();
            w.Start();

            bool ioTestPass = true;
            bool ioTestFinished = false;
            while (w.ElapsedMilliseconds < timeout)
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

            return true;
        }

        private bool DoFactoryReset(WorkerParam p, WorkerReportParam r)
        {
            GPS_RESPONSE rep = p.gps.FactoryReset();
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

            return true;
        }

        public bool DoV815Test(WorkerParam p)
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

            if (p.profile.v815TestColdStart == 1 && !DoColdStart(p, r, 3))
            {
                return false;
                /*
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
                */
            }

            if (p.profile.v815TestRtcDelay != 0)
            {
                ShowDelay(p, r, p.profile.v815TestRtcDelay, "Test RTC Delay ");
            }

            if (p.profile.v815TestRtc == 1 && !TestRtc(p, r))
            {
                return false;
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

            //20150204 - Angus changed to test in ROM mode which is in 115200 bps.
            int baudIdx = 5;
            if (!OpenDevice(p, r, baudIdx))
            {
                return false;
            }

            if (!DoColdStart(p, r, 3))
            {
                EndProcess(p);
                return false;
            }

            //if (p.profile.dlBaudSel > baudIdx && !BoostBaudRate(p, r, p.profile.dlBaudSel))
            if (baudIdx < 5 && !BoostBaudRate(p, r, ioTestBaud))
            {
                EndProcess(p);
                return false;
            }

            if (!DoIoSrecTest(p, r, Properties.Resources.V822TesterSrec, "", 5000))
            {
                EndProcess(p);
                return false;
            }

            ReopenDevice(p, r, baudIdx, 1000);

            if (!DoFactoryReset(p, r))
            {
                EndProcess(p);
                return false;
            }
            
            r.reportType = WorkerReportParam.ReportType.ShowFinished;
            p.error = WorkerParam.ErrorType.NoError;
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            EndProcess(p);
            return true;
        }

        public bool DoV816Test(WorkerParam p, V816Set s)
        {
            GPS_RESPONSE rep;
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
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

            if (!DoColdStart(p, r, 3))
            {
                EndProcess(p);
                return false;
            } 

            if (s == V816Set.Set1 && !TestRtc(p, r))
            {
                EndProcess(p);
                return false;
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

            if (!p.bw.CancellationPending && !DoFactoryReset(p, r))
            {
                EndProcess(p);
                return false;
            }

            if (!testPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (fixPass) ? WorkerParam.ErrorType.NmeaError : WorkerParam.ErrorType.SnrError; ;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
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

        //V828Dowmload also use this function.
        public bool DoV822Download(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            GPS_RESPONSE rep = GPS_RESPONSE.UART_OK;
            
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            if (p.fwProfile == null || p.fwProfile.promRaw == null)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.NoDownloadBinFile;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                //EndProcess(p);
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
                baudIdx = ScanBaudRate(p, r, lastDeviceBaudIdx);
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

            //20150225 Always use external loader in download.
            //20150330 Do change baud rate in loader.
            /*
            if (p.profile.dlBaudSel > baudIdx)
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
            */
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
                //Thread.Sleep(1000);
            }

            p.gps.Close();
            Thread.Sleep(100);
            rep = p.gps.Open(p.comPort, p.profile.dlBaudSel);

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
                r.output = "Re-pen " + p.comPort + " in " +
                    GpsBaudRateConverter.Index2BaudRate(p.profile.dlBaudSel).ToString() + " success.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            Thread.Sleep(1000);


            if ((p.fwProfile.tagAddress == 0 && p.fwProfile.tagContent == 0) ||
                (p.fwProfile.tagAddress == 0xAAAAAAAA && p.fwProfile.tagContent == 0x55555555))
            {
                if (p.fwProfile.promRaw.Length < 0xfcff0)
                {
                    rep = p.gps.SendTagBinSize(p.fwProfile.promRaw.Length, p.fwProfile.CalcPromRawCheckSum(),
                        p.profile.dlBaudSel, 0xfcffc, 0xffff);
                }
                else
                {
                    rep = p.gps.SendRomBinSize(p.fwProfile.promRaw.Length,
                        p.fwProfile.CalcPromRawCheckSum());
                }
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

        private const int ioTestBaud = 5;
        public bool DoV828Test(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            Stopwatch sw = new Stopwatch(); //Count test fail timeout
            sw.Reset();
            sw.Start();
            int baudIdx = GpsBaudRateConverter.BaudRate2Index(p.fwProfile.dvBaudRate);

            if (!OpenDevice(p, r, baudIdx))
            {
                return false;
            }

            if (!DoColdStart(p, r, 3))
            {
                EndProcess(p);
                return false;
            }

            if (!TestRtc(p, r))
            {
                EndProcess(p);
                return false;
            }

            if (!DoNmeaPrepareCommand(p, r))
            {
                EndProcess(p);
                return false;
            }            

            bool testPass = TestSnr(p, r, ref sw, p.profile.v828SnrBoundL, p.profile.v828SnrBoundU, p.profile.v828TestDuration * 1000);
            if (!testPass)
            {
                EndProcess(p);
                return false;
            }
            //if (p.profile.dlBaudSel > baudIdx && !BoostBaudRate(p, r, p.profile.dlBaudSel))
            if (baudIdx < 5 && !BoostBaudRate(p, r, ioTestBaud))
            {
                EndProcess(p);
                return false;
            }

            if (!DoIoSrecTest(p, r, Properties.Resources.IoTesterSrec, "TEST01 = 0003 0006 001C 001D 031E 031F 1C00 1D1C ", 5000))
            {
                EndProcess(p);
                return false;
            }

            ReopenDevice(p, r, baudIdx, 1000);
            if (!DoFactoryReset(p, r))
            {
                EndProcess(p);
                return false;
            }

            if (testPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowFinished;
                p.error = WorkerParam.ErrorType.NoError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            EndProcess(p);
            return true;
        }
        
        public bool DoV838Test(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            Stopwatch sw = new Stopwatch(); //Count test fail timeout
            sw.Reset();
            sw.Start();
            int baudIdx = GpsBaudRateConverter.BaudRate2Index(p.fwProfile.dvBaudRate);

            if (!OpenDevice(p, r, baudIdx))
            {
                return false;
            }

            if (!DoColdStart(p, r, 3))
            {
                EndProcess(p);
                return false;
            }

            if (!TestRtc(p, r))
            {
                EndProcess(p);
                return false;
            }

            if (!DoNmeaPrepareCommand(p, r))
            {
                EndProcess(p);
                return false;
            }

            bool testPass = TestSnr(p, r, ref sw, p.profile.v838SnrBoundL, p.profile.v838SnrBoundU, p.profile.v838TestDuration * 1000);
            if (!testPass)
            {
                EndProcess(p);
                return false;
            }

            //if (p.profile.dlBaudSel > baudIdx && !BoostBaudRate(p, r, p.profile.dlBaudSel))
            if (baudIdx < 5 && !BoostBaudRate(p, r, ioTestBaud))
            {
                EndProcess(p);
                return false;
            }

            if (!DoIoSrecTest(p, r, Properties.Resources.IoTesterSrec, "TEST01 = 0001 000F 0119 1C1D 0C0D 0E16 0809 151B 100F 1406 0002 181A 1807 0B0A 0B17 031E 031F ", 5000))
            {
                EndProcess(p);
                return false;
            }

            ReopenDevice(p, r, baudIdx, 1000);

            if (!DoFactoryReset(p, r))
            {
                EndProcess(p);
                return false;
            }

            if (testPass)
            {
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
