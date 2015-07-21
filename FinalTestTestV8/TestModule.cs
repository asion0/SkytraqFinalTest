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

        private bool OpenDevice(WorkerParam p, WorkerReportParam r, int baudIdx, bool testMode)
        {
            GPS_RESPONSE rep = p.gps.Open(p.comPort, baudIdx);

            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (testMode) ? WorkerParam.ErrorType.TestOpenPortFail : WorkerParam.ErrorType.DownloadOpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                long timeout = 5000;
                if (!WaitOneNmea(p, ref timeout))
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (testMode) ? WorkerParam.ErrorType.TestBootNoNmea : WorkerParam.ErrorType.DownloadBootNoNmea;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Open " + p.comPort + " in " +
                p.gps.GetBaudRate().ToString() + " success.";
            p.bw.ReportProgress(0, new WorkerReportParam(r)); 
            return true;
        }

        private void ReopenDevice(WorkerParam p, WorkerReportParam r, int baudIdx, int delay)
        {
            p.gps.Close();
            Thread.Sleep(delay);
            p.gps.Open(p.comPort, baudIdx);
        }

        private bool DoColdStart(WorkerParam p, WorkerReportParam r, int retry, bool testMode)
        {
            GPS_RESPONSE rep = p.gps.SendColdStart(retry, 2000);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (testMode) ? WorkerParam.ErrorType.TestColdStartTimeOut : WorkerParam.ErrorType.DownloadColdStartTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
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
                p.error = WorkerParam.ErrorType.TestConfigMessageOutputTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
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
                p.error = WorkerParam.ErrorType.TestConfigNmeaOutputTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
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
        
        private bool TestCrc(WorkerParam p, WorkerReportParam r, uint srcCrc)
        {
            // Test RTC
            uint crc = 0;
            GPS_RESPONSE rep = p.gps.QueryCrc(1000, ref crc);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestCrcTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "FW CRC(" + crc.ToString("X") + "), ini CRC(" + srcCrc.ToString("X") + ")";
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (srcCrc != crc)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestCheckCrcError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Check crc pass";
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
                p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.TestQueryRtcNack : WorkerParam.ErrorType.TestQueryRtcTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Get RTC1 " + rtc1.ToString();
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                Thread.Sleep(1050);
                rep = p.gps.QueryRtc(ref rtc2);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.TestQueryRtcNack : WorkerParam.ErrorType.TestQueryRtcTimeOut;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Get RTC2 " + rtc2.ToString();
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                if ((rtc2 - rtc1) > 3 || (rtc2 - rtc1) < 1)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.TestCheckRtcError;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
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
            GpsMsgParser.ParsingStatus.sateInfo lastSateInfo = new GpsMsgParser.ParsingStatus.sateInfo();
            lastSateInfo.snr = 0;
            lastSateInfo.prn = 0;
            p.parser.parsingStat.ClearAllSate();
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
                            else
                            {
                                if(s.snr > 0 && s.snr > lastSateInfo.snr)
                                {
                                    lastSateInfo.snr = s.snr;
                                    lastSateInfo.prn = s.prn;
                                    lastSateInfo.inUse = s.inUse;
                                }
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
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Prn:" + lastSateInfo.prn.ToString() + " Max SNR:" + lastSateInfo.snr.ToString() + " test fail";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            if (!testPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestSnrError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            return testPass;
        }

        private bool DoClockOffsetTest(WorkerParam p, WorkerReportParam r)
        {
            UInt32 prn = 0;
            UInt32 freq = 0;
            Int32 clkData = 0;
            Int64 clkAll = 0;
            Int64 count = 0;
            int tryCount = 3;
            do
            {
                for (byte i = 0; i < 1; ++i)
                {
                    GPS_RESPONSE rep = p.gps.QueryChannelDoppler(i, ref prn, ref freq);
                    if (GPS_RESPONSE.ACK != rep)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.TestQueryChannelDopplerTimeout;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }

                    if (prn == 0xFFFF || freq == 0xFFFF)
                    {
                        continue;
                    }

                    rep = p.gps.QueryChannelClockOffset(0, prn, 0, ref clkData);
                    if (GPS_RESPONSE.ACK != rep)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.TestQueryChannelClockOffsetTimeout;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }

                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "No:" + i.ToString() + " Clk:" + clkData.ToString() + " Prn:" + prn.ToString();
                    p.bw.ReportProgress(0, new WorkerReportParam(r));

                    clkAll += clkData;
                    ++count;
                }

                if (count != 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            } while (--tryCount > 0);

            if (count == 0)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "No active channel to test clock offset";
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestCheckClockOffsetFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            double clkPpm = clkAll / count / (96.25 * 16.367667);
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Average Clock Offset:" + (clkAll / count).ToString() + "(" + clkPpm.ToString("F2") + " ppm)";
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (clkPpm > 2.5 || clkPpm < -2.5)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestCheckClockOffsetFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
     
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Check clock offset pass";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
    
            return true;
        }

        private bool WaitOneNmea(WorkerParam p, ref long timeout)
        {
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            while(sw.ElapsedMilliseconds < timeout)
            {
                byte[] buff = new byte[256];
                int l = p.gps.ReadLineNoWait(buff, 256, 1000);
                string line = Encoding.UTF8.GetString(buff, 0, l);
                if (GpsMsgParser.CheckNmea(line))
                {   
                    timeout = sw.ElapsedMilliseconds;
                    sw.Stop();
                    return true;
                }
            }
            timeout = sw.ElapsedMilliseconds;
            sw.Stop();
            return false;
        }

        private bool BoostBaudRate(WorkerParam p, WorkerReportParam r, int baudIdx)
        {
            GPS_RESPONSE rep = p.gps.ChangeBaudrate((byte)baudIdx, 2, true);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestChangeBaudRateFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                long timeut = 6000;
                bool b = WaitOneNmea(p, ref timeut);
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Waiting NMEA " + timeut.ToString() + " ms";
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                if (b)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Change baud rate to " + GpsBaudRateConverter.Index2BaudRate(baudIdx).ToString() + " success";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.TestChangeBaudRateFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
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
                p.error = WorkerParam.ErrorType.TestLoaderDownloadFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
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
                    p.error = WorkerParam.ErrorType.TestUploadLoaderFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
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
                p.error = WorkerParam.ErrorType.TestIoTestFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
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
                p.error = WorkerParam.ErrorType.TestFactoryResetTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
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
                p.error = WorkerParam.ErrorType.TestOpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
//                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Open " + p.comPort + " in " +
                    p.gps.GetBaudRate().ToString() + " success.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            if (p.profile.v815TestColdStart == 1 && !DoColdStart(p, r, 3, true))
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
                p.error = WorkerParam.ErrorType.TestSnrError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
//                EndProcess(p);
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
                    p.error = WorkerParam.ErrorType.TestFactoryResetTimeOut;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
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
//            EndProcess(p);
            return true;
        }

        public bool DoV822Test(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;

            //20150204 - Angus changed to test in ROM mode which is in 115200 bps.
            //int baudIdx = 5;
            //20150615 - Angus want to test flash crc version
            int baudIdx = GpsBaudRateConverter.BaudRate2Index(p.fwProfile.dvBaudRate);

            if (!OpenDevice(p, r, baudIdx, true))
            {
                return false;
            }

            if (!DoColdStart(p, r, 3, true))
            {
                return false;
            }

            if (!TestCrc(p, r, p.fwProfile.crc))
            {
                return false;
            }

            if (baudIdx < 5 && !BoostBaudRate(p, r, ioTestBaud))
            {
                return false;
            }

            if (!DoIoSrecTest(p, r, Properties.Resources.V822TesterSrec, "", 5000))
            {
                return false;
            }

            ReopenDevice(p, r, baudIdx, 1000);
            if (!DoFactoryReset(p, r))
            {
                return false;
            }
            
            r.reportType = WorkerReportParam.ReportType.ShowFinished;
            p.error = WorkerParam.ErrorType.NoError;
            p.bw.ReportProgress(0, new WorkerReportParam(r));
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
                p.error = WorkerParam.ErrorType.TestOpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
//              EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Open " + p.comPort + " in " +
                    p.gps.GetBaudRate().ToString() + " success.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            if (!DoColdStart(p, r, 3, true))
            {
                return false;
            } 

            if (s == V816Set.Set1 && !TestRtc(p, r))
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
//              EndProcess(p);
                return false;
            }

            if (!testPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestSnrError; ;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowFinished;
                p.error = WorkerParam.ErrorType.NoError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
//          EndProcess(p);
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
                p.error = WorkerParam.ErrorType.DownloadNoDownloadBinFile;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            lastDeviceBaudIdx = 5;      //V822 boot in ROM mode 115200 bps.
            if (!OpenDevice(p, r, lastDeviceBaudIdx, false))
            {
                return false;
            }

            if (!DoColdStart(p, r, 3, false))
            {
                return false;
            }

            String dbgOutput = "";
            rep = p.gps.SendLoaderDownload(ref dbgOutput);
            if (GPS_RESPONSE.OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.DownloadLoaderDownloadFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
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
                    p.error = WorkerParam.ErrorType.DownloadUploadLoaderFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }  

                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Upload Loader success";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            p.gps.Close();
            Thread.Sleep(100);
            rep = p.gps.Open(p.comPort, p.profile.dlBaudSel);

            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.DownloadOpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
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
                p.error = WorkerParam.ErrorType.DownloadBinsizeCmdTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
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
            }

            rep = p.gps.WaitStringAck(10000, "END\0");
            if (GPS_RESPONSE.OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.DownloadEndTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
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

            if (!OpenDevice(p, r, baudIdx, true))
            {
                return false;
            }

            if (!DoColdStart(p, r, 3, true))
            {
                return false;
            }

            if (baudIdx < 5 && !BoostBaudRate(p, r, ioTestBaud))
            {
                return false;
            }

            if (!TestRtc(p, r))
            {
                return false;
            }

            if (!DoNmeaPrepareCommand(p, r))
            {
                return false;
            }

            bool testPass = TestSnr(p, r, ref sw, p.profile.v828SnrBoundL, p.profile.v828SnrBoundU, p.profile.v828TestDuration * 1000);
            if (!testPass)
            {
                return false;
            }

            if (p.profile.v828TestClockOffset == 1 && !DoClockOffsetTest(p, r))
            {
                return false;
            }

            if (!DoIoSrecTest(p, r, Properties.Resources.IoTesterSrec, "TEST01 = 0003 0006 001C 001D 031E 031F 1C00 1D1C ", 5000))
            {
                return false;
            }

            ReopenDevice(p, r, baudIdx, 1000);
            if (!DoFactoryReset(p, r))
            {
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowFinished;
            p.error = WorkerParam.ErrorType.NoError;
            p.bw.ReportProgress(0, new WorkerReportParam(r));

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

            if (!OpenDevice(p, r, baudIdx, true))
            {
                return false;
            }

            if (!DoColdStart(p, r, 3, true))
            {
                return false;
            }

            if (!TestRtc(p, r))
            {
                return false;
            }

            if (!DoNmeaPrepareCommand(p, r))
            {
                return false;
            }

            bool testPass = TestSnr(p, r, ref sw, p.profile.v838SnrBoundL, p.profile.v838SnrBoundU, p.profile.v838TestDuration * 1000);
            if (!testPass)
            {
                return false;
            }

            if (baudIdx < 5 && !BoostBaudRate(p, r, ioTestBaud))
            {
                return false;
            }

            if (!DoIoSrecTest(p, r, Properties.Resources.IoTesterSrec, "TEST01 = 0001 000F 0119 1C1D 0C0D 0E16 0809 151B 100F 1406 0002 181A 1807 0B0A 0B17 031E 031F ", 5000))
            {
                return false;
            }

            ReopenDevice(p, r, baudIdx, 1000);

            if (!DoFactoryReset(p, r))
            {
                return false;
            }

            if (testPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowFinished;
                p.error = WorkerParam.ErrorType.NoError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            return true;
        }

        public void EndProcess(WorkerParam p)
        {
            p.gps.Close();
        }    
    }
}
