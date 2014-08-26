﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Security.Permissions;
using System.Security;

//For Excel Object
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing;

using System.Security.Principal;
using System.Security.AccessControl;

//using Spire.Xls;

namespace FinalTestV8
{
/*
    class ExcelWriter
    {
        const int slotCount = FinalTestV8.FinalTestForm.ModuleCount;
        const int baseX = 2;
        const int baseY = 3;
        public ExcelWriter()
        {
        
        }
        private static bool HasWritePermission(string filePath)
        {
            try
            {
                FileSystemSecurity security;
                if (File.Exists(filePath))
                {
                    security = File.GetAccessControl(filePath);
                }
                else
                {
                    security = Directory.GetAccessControl(Path.GetDirectoryName(filePath));
                }
                var rules = security.GetAccessRules(true, true, typeof(NTAccount));

                var currentuser = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                bool result = false;
                foreach (FileSystemAccessRule rule in rules)
                {
                    if (0 == (rule.FileSystemRights &
                        (FileSystemRights.WriteData | FileSystemRights.Write)))
                    {
                        continue;
                    }

                    if (rule.IdentityReference.Value.StartsWith("S-1-"))
                    {
                        var sid = new SecurityIdentifier(rule.IdentityReference.Value);
                        if (!currentuser.IsInRole(sid))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!currentuser.IsInRole(rule.IdentityReference.Value))
                        {
                            continue;
                        }
                    }

                    if (rule.AccessControlType == AccessControlType.Deny)
                        return false;
                    if (rule.AccessControlType == AccessControlType.Allow)
                        result = true;
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        public class ExcelDocument
        {
            private Application xlApp = new Application();
            private Workbook wb;
            private Worksheet ws;
            private String savePath;

            public ExcelDocument(String path)
            {
                savePath = path;
                //InitDocument();
                wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
                ws = (Worksheet)wb.Worksheets[1];
            }

            public bool MargeCells(String lt, String br, Color c)
            {
                Range rg = ws.get_Range(lt, br);
                rg.MergeCells = true;
                rg.Interior.Color = ColorTranslator.ToOle(c);
                return true;
            }

            public bool MargeCells(int lt_c, int lt_r, int br_c, int br_r, Color c)
            {
                Range rg = ws.get_Range(ws.Cells[baseY + lt_r, baseX + lt_c], ws.Cells[baseY + br_r, baseX + br_c]);
                rg.MergeCells = true;
                rg.Interior.Color = ColorTranslator.ToOle(c);
                return true;
            }

            public bool FillCells(String lt, String br, Color c)
            {
                Range rg = ws.get_Range(lt, br);
                rg.Interior.Color = ColorTranslator.ToOle(c);
                return true;
            }

            public bool FillCells(int lt_c, int lt_r, int br_c, int br_r, Color c)
            {
                Range rg = ws.get_Range(ws.Cells[baseY + lt_r, baseX + lt_c], ws.Cells[baseY + br_r, baseX + br_c]);
                rg.Interior.Color = ColorTranslator.ToOle(c);
                return true;
            }

            public bool TextColorCells(String lt, String br, Color c)
            {
                Range rg = ws.get_Range(lt, br);
                rg.Font.Color = ColorTranslator.ToOle(c);
                return true;
            }

            public bool TextColorCells(int lt_c, int lt_r, int br_c, int br_r, Color c)
            {
                Range rg = ws.get_Range(ws.Cells[baseY + lt_r, baseX + lt_c], ws.Cells[baseY + br_r, baseX + br_c]);
                rg.Font.Color = ColorTranslator.ToOle(c);
                return true;
            }

            public bool AlignCells(String lt, String br, XlHAlign a)
            {
                Range rg = ws.get_Range(lt, br);
                rg.HorizontalAlignment = a;   
                return true;
            }

            public bool AlignCells(int lt_c, int lt_r, int br_c, int br_r, XlHAlign a)
            {
                Range rg = ws.get_Range(ws.Cells[baseY + lt_r, baseX + lt_c], ws.Cells[baseY + br_r, baseX + br_c]);
                rg.HorizontalAlignment = a;
                return true;
            }

            public bool LineAllCells(String lt, String br)
            {
                Range rg = ws.get_Range(lt, br);
                Borders borders = rg.Borders;
                borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlDiagonalUp].LineStyle = XlLineStyle.xlLineStyleNone;
                borders[XlBordersIndex.xlDiagonalDown].LineStyle = XlLineStyle.xlLineStyleNone;
                borders = null;
                return true;
            }

            public bool LineAllCells(int lt_c, int lt_r, int br_c, int br_r)
            {
                Range rg = ws.get_Range(ws.Cells[baseY + lt_r, baseX + lt_c], ws.Cells[baseY + br_r, baseX + br_c]);
                Borders borders = rg.Borders;
                borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlDiagonalUp].LineStyle = XlLineStyle.xlLineStyleNone;
                borders[XlBordersIndex.xlDiagonalDown].LineStyle = XlLineStyle.xlLineStyleNone;
                borders = null;
                return true;
            }

            public bool OutlineCells(String lt, String br)
            {
                Range rg = ws.get_Range(lt, br);
                Borders borders = rg.Borders;
                borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                borders = null;
                return true;
            }

            public bool OutlineCells(int lt_c, int lt_r, int br_c, int br_r)
            {
                Range rg = ws.get_Range(ws.Cells[baseY + lt_r, baseX + lt_c], ws.Cells[baseY + br_r, baseX + br_c]);
                Borders borders = rg.Borders;
                borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                borders = null;
                return true;
            }

            public bool WriteCell(String cell, String content)
            {
                Range rg = ws.get_Range(cell, cell);
                rg.Value2 = content;
                return true;
            }

            public bool WriteCell(int lt_c, int lt_r, String content)
            {
                Range rg = ws.get_Range(ws.Cells[baseY + lt_r, baseX + lt_c], ws.Cells[baseY + lt_r, baseX + lt_c]);
                rg.Value2 = content;
                return true;
            }

            private bool InitLoginArea()
            {
                if (ws == null)
                {
                    return false;
                }
                
                Color titleColor = Color.FromArgb(255, 174, 201);
                Color contentColor = Color.FromArgb(255, 255, 255);

                //Login Table
                MargeCells(0, 0, 1, 0, titleColor);
                MargeCells(0, 1, 1, 1, titleColor);
                MargeCells(0, 2, 1, 2, titleColor);
                MargeCells(0, 3, 1, 3, titleColor);
                MargeCells(0, 4, 1, 4, titleColor);

                MargeCells(2, 0, 4, 0, contentColor);
                MargeCells(2, 1, 4, 1, contentColor);
                MargeCells(2, 2, 4, 2, contentColor);
                MargeCells(2, 3, 4, 3, contentColor);
                MargeCells(2, 4, 4, 4, contentColor);

                AlignCells(2, 0, 4, 4, XlHAlign.xlHAlignCenter);
                
                WriteCell(0, 0, "測試人員編號");
                WriteCell(0, 1, "第一次測試");
                WriteCell(0, 2, "測試治具編號");
                WriteCell(0, 3, "工單號碼");
                WriteCell(0, 4, "登入時間");

                LineAllCells(0, 0, 4, 4);
                return true;
            }

            private bool InitTestProfileArea()
            {
                if (ws == null)
                {
                    return false;
                }

                Color titleColor = Color.FromArgb(255, 192, 0);
                Color contentColor = Color.FromArgb(255, 255, 255);
                Color lineColor = Color.FromArgb(10, 10, 10);

                //Login Table
                MargeCells(6, 0, 7, 0, titleColor);
                MargeCells(6, 1, 7, 1, titleColor);
                MargeCells(6, 3, 7, 3, titleColor);
                MargeCells(6, 6, 7, 6, titleColor);
                MargeCells(6, 7, 7, 7, titleColor);
                MargeCells(6, 8, 7, 8, titleColor);
                MargeCells(6, 9, 7, 9, titleColor);
                MargeCells(6, 11, 7, 11, titleColor);
                MargeCells(6, 14, 7, 14, titleColor);
                MargeCells(6, 15, 7, 15, titleColor);
                MargeCells(6, 16, 7, 16, titleColor);
                MargeCells(6, 22, 7, 22, titleColor);
                MargeCells(6, 23, 7, 23, titleColor);
                MargeCells(6, 24, 7, 24, titleColor);

                MargeCells("J8", "J8", titleColor);
                MargeCells("K8", "K8", titleColor);
                MargeCells("L8", "L8", titleColor);
                MargeCells("M8", "M8", titleColor);
                MargeCells("J16", "J16", titleColor);
                FillCells("K16", "L16", titleColor);
                FillCells("J21", "J21", titleColor);
                MargeCells("K21", "L21", titleColor);
                FillCells("H22", "I22", titleColor);
                MargeCells("J24", "K24", titleColor);
                MargeCells("L24", "M24", titleColor);

                FillCells("J3", "K3", contentColor);
                MargeCells("J4", "K4", contentColor);
                FillCells("J6", "K6", contentColor);
                FillCells("K17", "L17", contentColor);
                FillCells("K22", "L22", contentColor);
                MargeCells("J25", "K25", contentColor);
                MargeCells("J26", "K26", contentColor);
                MargeCells("L25", "M25", contentColor);
                MargeCells("L26", "M26", contentColor);
                MargeCells("J27", "K27", contentColor);

                LineAllCells("H3", "I4");
                OutlineCells("J3", "K3");
                LineAllCells("J4", "K4");
                LineAllCells("H6", "I6");
                OutlineCells("J6", "K6");
                LineAllCells("H9", "I12");
                LineAllCells("J8", "M12");
                LineAllCells("H14", "J14");
                LineAllCells("J16", "J16");
                LineAllCells("K16", "L16");
                LineAllCells("H17", "J19");
                LineAllCells("K17", "L17");

                LineAllCells("K21", "L21");
                OutlineCells("K22", "L22");
                LineAllCells("J21", "J22");
                LineAllCells("H22", "I22");
                LineAllCells("J24", "M26");
                LineAllCells("H25", "K27");

                AlignCells("J4", "K4", XlHAlign.xlHAlignCenter);
                AlignCells("J9", "M12", XlHAlign.xlHAlignCenter);
                AlignCells("J17", "J19", XlHAlign.xlHAlignCenter);
                AlignCells("J22", "J22", XlHAlign.xlHAlignCenter);
                AlignCells("J25", "M26", XlHAlign.xlHAlignCenter);
                AlignCells("J27", "K27", XlHAlign.xlHAlignCenter);

                WriteCell(6, 0, "Golden Baud Rate");
                WriteCell(9, 0, "bps");
                WriteCell(6, 1, "Test Module");
                WriteCell(6, 3, "SNR Test Period");
                WriteCell(9, 3, "second(s)");
                WriteCell(8, 5, "Enable");
                WriteCell(9, 5, "Lower");
                WriteCell(10, 5, "Upper");
                WriteCell(11, 5, "SNR Limit");
                WriteCell(6, 6, "Test GPS SNR");
                WriteCell(6, 7, "Test GLONASS SNR");
                WriteCell(6, 8, "Test Beidou SNR");
                WriteCell(6, 9, "Test Galileo SNR");

                WriteCell(6, 11, "Check prom crc");
                WriteCell(8, 13, "Enable");
                WriteCell(9, 13, "Threshold");
                WriteCell(10, 13, "Write Back");

                WriteCell(6, 14, "Test clock offset");
                WriteCell(6, 15, "Test e-compass");
                WriteCell(6, 16, "Check miniHommer");

                WriteCell(8, 18, "Enable");
                WriteCell(9, 18, "Duration");
                WriteCell(6, 19, "Test DR Cyro");
                WriteCell(10, 19, "second(s)");
                WriteCell(8, 21, "Clockwise");
                WriteCell(10, 21, "Anticlockwise");
                WriteCell(6, 22, "USL");
                WriteCell(6, 23, "SL");
                WriteCell(6, 24, "Threshold of COG");
                return true;
            }

            private bool InitFirmwareInfoArea()
            {
                if (ws == null)
                {
                    return false;
                }

                Color titleColor = Color.FromArgb(146, 205, 220);
                Color contentColor = Color.FromArgb(255, 255, 255);
                Color lineColor = Color.FromArgb(10, 10, 10);

                MargeCells("B10", "C10", titleColor);
                MargeCells("B11", "C11", titleColor);
                MargeCells("B12", "C12", titleColor);
                MargeCells("B13", "C13", titleColor);
                MargeCells("B14", "C14", titleColor);
                MargeCells("B15", "C15", titleColor);
                MargeCells("B16", "C16", titleColor);
                MargeCells("B17", "C17", titleColor);
                MargeCells("D10", "F10", contentColor);
                MargeCells("D11", "F11", contentColor);
                MargeCells("D12", "F12", contentColor);
                MargeCells("D13", "F13", contentColor);
                MargeCells("D14", "F14", contentColor);
                MargeCells("D15", "F15", contentColor);
                MargeCells("D16", "F16", contentColor);
                MargeCells("D17", "F17", contentColor);
                LineAllCells("B10", "F17");
                AlignCells("B10", "F17", XlHAlign.xlHAlignCenter);

                WriteCell("B10", "Prom File");
                WriteCell("B11", "Kernel Version");
                WriteCell("B12", "Software Version");
                WriteCell("B13", "Revision");
                WriteCell("B14", "CRC");
                WriteCell("B15", "Baud Rate");
                WriteCell("B16", "Tag Address");
                WriteCell("B17", "Tag Content");

                return true;
            }

            private bool InitSessionReportArea()
            {
                if (ws == null)
                {
                    return false;
                }
                Color textColor0 = Color.FromArgb(255, 255, 255);
                Color textColor1 = Color.FromArgb(0, 0, 0);

                Color titleColor0 = Color.FromArgb(166, 166, 166);
                Color titleColor1 = Color.FromArgb(105, 176, 46);
                Color titleColor2 = Color.FromArgb(73, 152, 43);
                Color titleColor3 = Color.FromArgb(52, 139, 97);
                Color titleColor4 = Color.FromArgb(49, 131, 168);
                Color titleColorA = Color.FromArgb(89, 89, 89);
                Color titleColorB = Color.FromArgb(49, 74, 72);
                Color titleColorC = Color.FromArgb(46, 71, 76);

                MargeCells("B29", "C29", titleColorA);
                MargeCells("D29", "G29", titleColorB);
                MargeCells("H29", "K29", titleColorC);
                MargeCells("L29", "O29", titleColorB);
                MargeCells("P29", "S29", titleColorC);
                TextColorCells("B29", "S29", textColor0);
                MargeCells("B30", "C30", titleColor0);
                FillCells("D30", "D30", titleColor1);
                FillCells("E30", "E30", titleColor2);
                FillCells("F30", "F30", titleColor3);
                FillCells("G30", "G30", titleColor4);

                FillCells("H30", "H30", titleColor1);
                FillCells("I30", "I30", titleColor2);
                FillCells("J30", "J30", titleColor3);
                FillCells("K30", "K30", titleColor4);

                FillCells("L30", "L30", titleColor1);
                FillCells("M30", "M30", titleColor2);
                FillCells("N30", "N30", titleColor3);
                FillCells("O30", "O30", titleColor4);

                FillCells("P30", "P30", titleColor1);
                FillCells("Q30", "Q30", titleColor2);
                FillCells("R30", "R30", titleColor3);
                FillCells("S30", "S30", titleColor4);

                WriteCell("B29", "Session");
                WriteCell("D29", "A1");
                WriteCell("H29", "A2");
                WriteCell("L29", "A3");
                WriteCell("P29", "A4");
                WriteCell("B30", "Start Time");

                WriteCell("D30", "COM");
                WriteCell("E30", "SNR");
                WriteCell("F30", "Error");
                WriteCell("G30", "Duration");

                WriteCell("H30", "COM");
                WriteCell("I30", "SNR");
                WriteCell("J30", "Error");
                WriteCell("K30", "Duration");

                WriteCell("L30", "COM");
                WriteCell("M30", "SNR");
                WriteCell("N30", "Error");
                WriteCell("O30", "Duration");

                WriteCell("P30", "COM");
                WriteCell("Q30", "SNR");
                WriteCell("R30", "Error");
                WriteCell("S30", "Duration");
                LineAllCells("B29", "S30");
                AlignCells("B29", "S30", XlHAlign.xlHAlignCenter);

                return true;
            }

            public void InitDocument()
            {
                InitLoginArea();
                InitTestProfileArea();
                InitFirmwareInfoArea();
                InitSessionReportArea();
            }

            public void Save()
            {
                //最後，呼叫SaveAs function儲存這個Excel物件到硬碟。
                object defOpt = System.Reflection.Missing.Value;
                wb.SaveAs(savePath, System.Type.Missing,
                    defOpt, defOpt, defOpt, defOpt,
                    XlSaveAsAccessMode.xlNoChange,
                    defOpt, defOpt, defOpt, defOpt, defOpt);

                Console.WriteLine("save");
                wb.Close(false, defOpt, defOpt);
                xlApp.Workbooks.Close();
                xlApp.Quit();


                //刪除 Windows工作管理員中的Excel.exe 進程，  
                Marshal.ReleaseComObject(xlApp);
                Marshal.ReleaseComObject(wb);
                Marshal.ReleaseComObject(ws);
                //Marshal.ReleaseComObject(aRange3);
                xlApp = null;
                wb = null;
                ws = null;
                //aRange3 = null;
                //呼叫垃圾回收  
                GC.Collect();
            }
        }
        
        public void Test()
        {
            const String LogFolderName = "Log";
            //const String LogFileName = "result.xml";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", FinalTestV8.FinalTestForm.LogFolderName);
            if (!Directory.Exists(sb.ToString()))
            {
                return;
            }
            //Scan the Log folder
            foreach (string d1 in Directory.GetDirectories(sb.ToString()))
            {
                foreach (string d2 in Directory.GetDirectories(d1))
                {
                    String excelFile = sb.ToString() + "\\" + Path.GetFileName(d1) + "_" + Path.GetFileName(d2) + ".xlsx";
                    if (HasWritePermission(excelFile))
                    {

                        ExcelDocument ed = new ExcelDocument(excelFile);
                        ed.InitDocument();
                        ParsingResultXml(d2 + "\\" + FinalTestV8.FinalTestForm.LogFileName, ed);
                        ed.Save();
                    }
                    return;
                }
            }
        }
        

        private bool Validate(XmlNode itemNode)
        {
            Crc32 crc32 = new Crc32();
            UInt64 c2 = Convert.ToUInt64(itemNode.SelectSingleNode("ItemKey").Attributes["Key"].Value);
            UInt64 c1 = crc32.ComputeChecksum(itemNode.SelectSingleNode("ItemData").OuterXml);
            return (c2 == c1);
        }

        private bool ValidateSession(XmlNode itemNode)
        {
            Crc32 crc32 = new Crc32();
            UInt64 c1 = crc32.ComputeChecksum(itemNode.SelectSingleNode("UISetting").OuterXml) ^
                crc32.ComputeChecksum(itemNode.SelectSingleNode("SnrOffset").OuterXml) ^
                crc32.ComputeChecksum(itemNode.SelectSingleNode("Tester").OuterXml);
            UInt64 c2 = Convert.ToUInt64(itemNode.SelectSingleNode("ItemKey").Attributes["Key"].Value);
            return (c2 == c1);
        }        
        
        private bool ParsingLoginData(XmlNode itemNode, ExcelDocument ed)
        {
            if (!Validate(itemNode))
            {
                return false;
            }

            XmlElement e = (XmlElement)itemNode.SelectSingleNode("ItemData");
            DateTime t = new DateTime();
            DateTime.TryParse(e.GetAttribute("LT"), out t);
            t = t.Subtract(new TimeSpan(8, 0, 0));
            ed.WriteCell("D3", e.GetAttribute("TN"));
            ed.WriteCell("D4", e.GetAttribute("FT"));
            ed.WriteCell("D5", e.GetAttribute("FN"));
            ed.WriteCell("D6", e.GetAttribute("WN"));
            ed.WriteCell("D7", t.ToString("yy/MM/dd HH:mm:ss"));
            return true;
        }

        private bool ParsingTestProfileData(XmlNode itemNode, ExcelDocument ed)
        {
            if (!Validate(itemNode))
            {
                return false;
            }

            XmlElement e = (XmlElement)itemNode.SelectSingleNode("ItemData");

            ed.WriteCell("J3", GpsBaudRateConverter.Index2BaudRate(Convert.ToInt32(e.GetAttribute("GB"))).ToString());
            ed.WriteCell("J4", e.GetAttribute("MN"));

            ed.WriteCell("J6", e.GetAttribute("STP"));
            ed.WriteCell("J9", e.GetAttribute("TGP"));
            ed.WriteCell("J10", e.GetAttribute("TGL"));
            ed.WriteCell("J11", e.GetAttribute("TBD"));
            ed.WriteCell("J12", e.GetAttribute("TGA"));
            ed.WriteCell("K9", e.GetAttribute("GPL"));
            ed.WriteCell("K10", e.GetAttribute("GLL"));
            ed.WriteCell("K11", e.GetAttribute("BDL"));
            ed.WriteCell("K12", e.GetAttribute("GAL"));
            ed.WriteCell("L9", e.GetAttribute("GPU"));
            ed.WriteCell("L10", e.GetAttribute("GLU"));
            ed.WriteCell("L11", e.GetAttribute("BDU"));
            ed.WriteCell("L12", e.GetAttribute("GAU"));
            ed.WriteCell("M9", e.GetAttribute("GPS"));
            ed.WriteCell("M10", e.GetAttribute("GLS"));
            ed.WriteCell("M11", e.GetAttribute("BDS"));
            ed.WriteCell("M12", e.GetAttribute("GAS"));

            ed.WriteCell("J14", e.GetAttribute("CPC"));
            ed.WriteCell("J17", e.GetAttribute("TCO"));
            ed.WriteCell("K17", e.GetAttribute("COT"));
            ed.WriteCell("L17", e.GetAttribute("WCO"));
            ed.WriteCell("J18", e.GetAttribute("TEC"));
            ed.WriteCell("J19", e.GetAttribute("TMH"));

            ed.WriteCell("J22", e.GetAttribute("TDC"));
            ed.WriteCell("K22", e.GetAttribute("TDD"));

            ed.WriteCell("J25", e.GetAttribute("USC"));
            ed.WriteCell("J26", e.GetAttribute("LSC"));
            ed.WriteCell("L25", e.GetAttribute("USA"));
            ed.WriteCell("L26", e.GetAttribute("LSA"));
            ed.WriteCell("J27", e.GetAttribute("TOC"));
            return true;
        }

        private bool ParsingFirmwareInfoData(XmlNode itemNode, ExcelDocument ed)
        {
            if (!Validate(itemNode))
            {
                return false;
            }
            
            XmlElement e = (XmlElement)itemNode.SelectSingleNode("ItemData");

            ed.WriteCell("D10", e.GetAttribute("PF"));
            ed.WriteCell("D11", e.GetAttribute("KV"));
            ed.WriteCell("D12", e.GetAttribute("SV"));
            ed.WriteCell("D13", e.GetAttribute("RV"));
            ed.WriteCell("D14", Convert.ToInt32(e.GetAttribute("CR")).ToString("X4"));
            ed.WriteCell("D15", e.GetAttribute("BR"));
            ed.WriteCell("D16", e.GetAttribute("TA"));
            ed.WriteCell("D17", e.GetAttribute("TC"));

            return true;
        }
        
        public class SessionItem
        {
            public bool[] disableSel = new bool[slotCount];
            public String[] comList = new String[slotCount];
            public int[] gpSnr = new int[slotCount];
            public int[] glSnr = new int[slotCount];
            public int[] bdSnr = new int[slotCount];
            //public int[] gaSnr = new int[slotCount];
            public DateTime startTime;
            public int duration = 0;
            public String[] rt = null;
            public String[] detail = null;
        }

        private int testItemColIndex = 31;
        private bool ParsingTestSessionData(XmlNode itemNode, ExcelDocument ed)
        {
            if (!ValidateSession(itemNode))
            {
                return false;
            }

            Color stColor1 = Color.FromArgb(230, 230, 230);
            Color stColor2 = Color.FromArgb(255, 255, 255);


            XmlElement e1 = (XmlElement)itemNode.SelectSingleNode("UISetting");
            XmlElement e2 = (XmlElement)itemNode.SelectSingleNode("SnrOffset");
            XmlElement e3 = (XmlElement)itemNode.SelectSingleNode("Tester");

            string[] dsKey = { "GDDS", "A1DS", "A2DS", "A3DS", "A4DS", "B1DS", "B2DS", "B3DS", "B4DS" };
            string[] cmKey = { "GDCM", "A1CM", "A2CM", "A3CM", "A4CM", "B1CM", "B2CM", "B3CM", "B4CM" };
            string[] gpSnrKey = { "", "A1GP", "A2GP", "A3GP", "A4GP", "B1GP", "B2GP", "B3GP", "B4GP" };
            string[] glSnrKey = { "", "A1GL", "A2GL", "A3GL", "A4GL", "B1GL", "B2GL", "B3GL", "B4GL" };
            string[] bdnrKey = { "", "A1BD", "A2BD", "A3BD", "A4BD", "B1BD", "B2BD", "B3BD", "B4BD" };
            string[] testerKey = {"", "a1", "a2", "a3", "a4", "b1", "b2", "b3", "b4" };

            SessionItem sItem = new SessionItem();
            DateTime.TryParse(e1.GetAttribute("ST"), out sItem.startTime);
            sItem.startTime = sItem.startTime.Subtract(new TimeSpan(8, 0, 0));
            ed.MargeCells("B" + testItemColIndex.ToString(), "C" + testItemColIndex.ToString(),
                (testItemColIndex % 2 == 0) ? stColor1 : stColor2);
            ed.WriteCell("B" + testItemColIndex.ToString(), sItem.startTime.ToString("yy/MM/dd HH:mm:ss"));

            for (int i = 1; i < slotCount; ++i)
            {


            }
            //ed.WriteCell("D10", e.GetAttribute("PF"));
            //ed.WriteCell("D11", e.GetAttribute("KV"));
            //ed.WriteCell("D12", e.GetAttribute("SV"));
            //ed.WriteCell("D13", e.GetAttribute("RV"));
            //ed.WriteCell("D14", Convert.ToInt32(e.GetAttribute("CR")).ToString("X4"));
            //ed.WriteCell("D15", e.GetAttribute("BR"));
            //ed.WriteCell("D16", e.GetAttribute("TA"));
            //ed.WriteCell("D17", e.GetAttribute("TC"));
            testItemColIndex++;
            return true;
        }

        public bool ParsingResultXml(String f, ExcelDocument ed)
        {
            //Test Codes
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(f);

            XmlNode itemNode = xmlDoc.SelectSingleNode("Root/LoginData/Item");
            ParsingLoginData(itemNode, ed);

            itemNode = xmlDoc.SelectSingleNode("Root/TestProfile/Item");
            ParsingTestProfileData(itemNode, ed);

            itemNode = xmlDoc.SelectSingleNode("Root/FirmwareInfo/Item");
            ParsingFirmwareInfoData(itemNode, ed);

            XmlNodeList nodeList = xmlDoc.SelectNodes("Root/TestSession/Item");

            foreach(XmlNode n in nodeList)
            {
                ParsingTestSessionData(n, ed);
            }
            return true;
        }
    }
 */
}
