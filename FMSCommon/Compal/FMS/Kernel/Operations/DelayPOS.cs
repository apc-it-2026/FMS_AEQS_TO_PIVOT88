using Compal.FMS.Kernel.Beans;
using Compal.FMS.Kernel.Operations;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using Compal.FMS.Connections.DBLoader;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OfficeOpenXml;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.Globalization;
using System.Linq;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Wordprocessing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
//using System.Windows.Controls.Primitives;
using OfficeOpenXml.Style;
using System.Drawing; 


namespace FMSCommon.Compal.FMS.Kernel.Operations  
{
    class DelayPOS
    {
        #region GetDelayPOS
        public async Task<Cls_Return> GetDelayPOSMethods(SrvInfo vsrvinfo)  
        {
            Cls_Return rt = new Cls_Return();
            OracleConnection conoa = null;
            OracleTransaction transaction = null;
            try
            {
                string constroa = null;
                string filePath;
                filePath = Application.ExecutablePath;
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);  
                string clientEnvConfigFileName = filePath + "database.config";  
                XmlDocument clientEnvConfigDoc = new XmlDocument(); 

                if (File.Exists(clientEnvConfigFileName)) 
                {
                    FileLoader obj = new FileLoader(clientEnvConfigFileName); 
                    Hashtable htdblinks = obj.GetDBLinks();
                    if (htdblinks.ContainsKey(vsrvinfo.SDB))
                        constroa = htdblinks[vsrvinfo.SDB].ToString();

                    try
                    {
                        conoa = new OracleConnection(constroa);
                        conoa.Open();
                        transaction = conoa.BeginTransaction();
                        string aftercrd = $@"SELECT DISTINCT B.CR_REQDATE
                                             FROM BDM_SE_ORDER_ITEM B
                                             WHERE B.CR_REQDATE > SYSDATE
                                             ORDER BY B.CR_REQDATE
                                             FETCH FIRST 1 ROWS ONLY";

                        OracleCommand crdcmd = new OracleCommand(aftercrd, conoa); 
                        OracleDataReader rdrrr = crdcmd.ExecuteReader();
                        rdrrr.Read();
                        DateTime acrd = rdrrr.GetDateTime(0);
                        string formattedAcrd = acrd.ToString("yyyy/MM/dd");
                          

                        string plsql = $@"  SELECT D.WEEK AS CUT_WEEK  , D.ARTICLE , D.SO ,  D.CUTTING_LINE  
FROM WMS_PLAN_SHEDULE_D D 
WHERE D.CRD = TO_DATE ( '{formattedAcrd}' , 'YYYY/MM/DD' )  
AND D.SO NOT IN  
( 
SELECT  
  W.DUMMY_PO 
FROM 
  WMS_MAT_UPLOAD_DATA W
WHERE 
  W.CRD = TO_DATE('{formattedAcrd}', 'YYYY/MM/DD')  
  AND W.INVOICE != 'SAP TECHICAL ISSUE DOUBLE TIME DOWNLOAD'
GROUP BY 
  W.DUMMY_PO
HAVING 
  COUNT(*) = SUM(
    CASE 
      WHEN W.INVOICE = 'STOCK' THEN 1
      WHEN W.INVOICE IS NOT NULL AND W.INVOICE NOT IN ('NO INVOICE') AND W.ETA IS NOT NULL THEN 1
      ELSE 0
    END
  )
 )    ";

                        DataTable dtforlackofmaterial = new DataTable(); 
                        using (OracleConnection connn = new OracleConnection(constroa))
                        {
                            connn.Open();
                            using (OracleCommand cmddd = new OracleCommand(plsql, connn))
                            {
                                using (OracleDataAdapter adapterr = new OracleDataAdapter(cmddd)) 
                                {
                                    adapterr.Fill(dtforlackofmaterial); 
                                }
                            }
                        }

                        if(dtforlackofmaterial.Rows.Count < 0)
                        {
                            return null;  
                        } 

                        
                        OracleCommand plcmdd = new OracleCommand(plsql, conoa); 
                        OracleDataReader rdrr = plcmdd.ExecuteReader();
                        List<PlanningPOs> poresult = new List<PlanningPOs>();

                        while (rdrr.Read())
                        {
                            poresult.Add(new PlanningPOs
                            {
                                CUT_WEEK = rdrr["CUT_WEEK"].ToString(),
                                ARTICLE = rdrr["ARTICLE"].ToString(),
                                SO = rdrr["SO"].ToString(),
                                CUTTING_LINE = rdrr["CUTTING_LINE"].ToString(),
                            }
                            );

                        } 

                        string messageforlackofmaterial = ForLackofMaterialConvert(dtforlackofmaterial);

                       

                        string plexcelFilePath2 = Path.Combine(Path.GetTempPath(), "Planning_shedule2.xlsx");

                        GenerateExcelFileForPlanningg(poresult, plexcelFilePath2);
                       

                        string planningsql = $@"SELECT E.TO_LIST , E.CC_LIST , E.ERROR_LIST , E.MAIL_SUBJECT 
                                                 FROM TBL_E2E_MAIL_CONFIG E WHERE E.ROLECODE = 'PL00' ";  
                        OracleCommand plcmd2 = new OracleCommand(planningsql, conoa);   
                        OracleDataReader planreader = plcmd2.ExecuteReader();   
                        List<string> plrecipientEmails2 = new List<string>(); 
                        List<string> plccEmails2 = new List<string>(); 
                        string plsubject2 = "";  

                        while (planreader.Read()) 
                        { 
                            plsubject2 = planreader["MAIL_SUBJECT"].ToString(); 

                            if (!string.IsNullOrEmpty(planreader["TO_LIST"].ToString()))
                                plrecipientEmails2.AddRange(planreader["TO_LIST"].ToString().Split(','));

                            if (!string.IsNullOrEmpty(planreader["CC_LIST"].ToString())) 
                                plccEmails2.AddRange(planreader["CC_LIST"].ToString().Split(',')); 
                        } 


                        using (var client = new SmtpClient())
                        {
                            string userEmailAddress = "IT-announcement@in.apachefootwear.com";
                            string userName = "Remainder Mail";
                            string password = "it-123456";
                            string host = "apcmx1.apachefootwear.com";
                            int port = 25;

                            MailMessage msg = new MailMessage();
                            msg.From = new MailAddress(userEmailAddress, userName); 
                            msg.Subject = plsubject2;
                            msg.Body = messageforlackofmaterial; 
                            msg.IsBodyHtml = true;

                             foreach (string email in plrecipientEmails2) 
                                msg.To.Add(email.Trim()); 
                            foreach (string email in plccEmails2) 
                                msg.CC.Add(email.Trim()); 

                            if (File.Exists(plexcelFilePath2)) 
                            {
                                Attachment attachment = new Attachment(plexcelFilePath2); 
                                attachment.Name = "Planning_shedule2.xlsx";   
                                msg.Attachments.Add(attachment); 
                            }


                            client.Host = host;
                            client.Port = port;
                            client.EnableSsl = false;  
                            client.UseDefaultCredentials = false;
                            client.Credentials = new NetworkCredential(userEmailAddress, password);

                            client.Send(msg);

                        }

                        DataTable dt34 = dtforlackofmaterial.Clone();

                        DateTime today = DateTime.Today;
                        DateTime tomorrow = today.AddDays(1);

                        DateTime dayAfterTomorrow = today.AddDays(3);


                        int currentYear = today.Year;

                        var filtered = dtforlackofmaterial.AsEnumerable()
                            .Where(row =>
                            {
                                string cutWeek = row.Field<string>("CUT_WEEK");

                                if (!string.IsNullOrWhiteSpace(cutWeek) && cutWeek.Contains("-"))
                                {
                                    string endPart = cutWeek.Split('-')[1];

                                    if (DateTime.TryParseExact(endPart, "MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                                        out DateTime endDateNoYear))
                                    {
                                        int targetYear = (endDateNoYear.Month < today.Month ||
                                                          (endDateNoYear.Month == today.Month && endDateNoYear.Day < today.Day))
                                                          ? currentYear + 1
                                                          : currentYear;

                                        DateTime fullEndDate = new DateTime(targetYear, endDateNoYear.Month, endDateNoYear.Day);
                                        return fullEndDate >= today.AddDays(1) && fullEndDate <= today.AddDays(3);

                                    }
                                }

                                return false;
                            });

                        if (filtered.Any())
                        {
                            dt34 = filtered.CopyToDataTable();
                        }



                        
                        if (dt34.Rows.Count <= 0)
                        {
                            return null;
                        }
                        List<StopPlanningPOs> scheduleList = dt34.AsEnumerable()
                                  .Select(row => new StopPlanningPOs
                                  {
                                      CUT_WEEK = row.Field<string>("CUT_WEEK"),
                                      ARTICLE = row.Field<string>("ARTICLE"),
                                      SO = row.Field<string>("SO"),
                                      CUTTING_LINE = row.Field<string>("CUTTING_LINE")
                                  })
                                  .ToList();

                        string message = ConvertDataTableToHTML(dt34);
                        string plexcelFilePath4 = Path.Combine(Path.GetTempPath(), "Planning_shedule4.xlsx");
                        GenerateExcelFileForStoppinglines(scheduleList, plexcelFilePath4);

                        using (var client = new SmtpClient())
                        {
                            string userEmailAddress = "IT-announcement@in.apachefootwear.com";
                            string userName = "Remainder Mail";
                            string password = "it-123456";
                            string host = "apcmx1.apachefootwear.com";
                            int port = 25;

                            MailMessage msg = new MailMessage();
                            msg.From = new MailAddress(userEmailAddress, userName);
                            msg.Subject = plsubject2;
                            msg.Body = message;
                            msg.IsBodyHtml = true;

                            foreach (string email in plrecipientEmails2)
                                msg.To.Add(email.Trim());
                            foreach (string email in plccEmails2)
                                msg.CC.Add(email.Trim());

                            if (File.Exists(plexcelFilePath2))
                            {
                                Attachment attachment = new Attachment(plexcelFilePath4);
                                attachment.Name = "Planning_shedule4.xlsx"; 
                                msg.Attachments.Add(attachment);
                            }


                            client.Host = host;
                            client.Port = port;
                            client.EnableSsl = false;
                            client.UseDefaultCredentials = false;
                            client.Credentials = new NetworkCredential(userEmailAddress, password);

                           client.Send(msg);

                        }



                    }  
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                        rt.TYPE = "E";
                        rt.MESSAGE = ex.Message;
                    }
                    finally
                    {
                        if (conoa != null && conoa.State == System.Data.ConnectionState.Open)
                        {
                            conoa.Close(); 
                        }
                    }
                }
                return rt;

            }
            catch (Exception e)
            {
                rt.TYPE = "E";
                rt.MESSAGE = e.Message;
                return rt;
            }
            finally
            {
                conoa?.Dispose();
                GC.Collect();
            }
        }

         
   
        private void GenerateExcelFileForPlanningg(List<PlanningPOs> results, string filePath) 
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try 
            {
                 

                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("Error: File path is null or empty.");     
                    return;
                }

                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Console.WriteLine($"Directory '{directoryPath}' was created.");  
                }  
                  

                 using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("POS_List");
                           
                     worksheet.Cells[1, 1].Value = "CUT_WEEK";
                    worksheet.Cells[1, 2].Value = "ARTICLE";
                    worksheet.Cells[1, 3].Value = "SO";
                    worksheet.Cells[1, 4].Value = "CUTTING_LINE";

                    using (var header = worksheet.Cells[1, 1, 1, 4])
                    {
                        header.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        header.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
                        header.Style.Font.Bold = true;
                        header.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        header.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    worksheet.Column(1).Width = 20;
                    worksheet.Column(2).Width = 40;
                    worksheet.Column(3).Width = 20;
                    worksheet.Column(4).Width = 40;
                    worksheet.Row(1).Height = 25;

                    int row = 2;
                    foreach (var item in results)
                    {
                        worksheet.Cells[row, 1].Value = item.CUT_WEEK;
                        worksheet.Cells[row, 2].Value = item.ARTICLE;
                        worksheet.Cells[row, 3].Value = item.SO;
                        worksheet.Cells[row, 4].Value = item.CUTTING_LINE; 
                        row++; 
                    } 

                     FileInfo fileInfo = new FileInfo(filePath);
                    package.SaveAs(fileInfo);
                    Console.WriteLine($"Excel file saved at: {fileInfo.FullName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while generating Excel file: {ex.Message}");
            }
        } 

        public string ConvertDataTableToHTML(DataTable dt)  
        { 
            StringBuilder html = new StringBuilder();

             html.Append("<html><body>");

             string currentMonth = DateTime.Now.ToString("MMMM yyyy");
            html.Append($"<p style='font-family: Times New Roman; font-size: 16px;'>Dear All,</p>");
            html.Append($"<p style='font-family: Times New Roman; font-size: 16px;'> We would like to inform you that these cutting " +
                $"lines will be stopped due to the " +
                $"lack of stock in the warehouse for these items, please contact the warehouse department..:</p>");

 
             html.Append("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse; width:100%; font-family: Times New Roman;'>");

             html.Append("<tr>");
            foreach (DataColumn column in dt.Columns)
            {
                html.Append("<th style='background-color:#f2f2f2; text-align:center;'>" + column.ColumnName + "</th>");
            }
            html.Append("</tr>");

            // Add the data rows
            foreach (DataRow row in dt.Rows)
            {
                html.Append("<tr>");
                foreach (var item in row.ItemArray)
                {
                    string cellValue = item.ToString().Trim();
                    int number;
                    if (int.TryParse(cellValue, out number))
                    {
                        if (number > 0)
                        {
                            html.Append($"<td style='text-align:center; background-color:#d4edda; color:#155724;'>{number} </td>");
                        }
                        else
                        {
                            html.Append("<td style='text-align:center; background-color:#f8d7da; color:#721c24;'>No Data</td>");
                        }
                    }
                    else
                    {
                        html.Append("<td style='text-align:center;'>" + System.Net.WebUtility.HtmlEncode(cellValue) + "</td>");
                    }
                }
                html.Append("</tr>");
            }

             html.Append("</table>");
            html.Append("</body></html>");

            return html.ToString();

        }

        public string ForLackofMaterialConvert(DataTable dt)
        {
            StringBuilder html = new StringBuilder(); 

            html.Append("<html><body>");

            string currentMonth = DateTime.Now.ToString("MMMM yyyy");
            html.Append($"<p style='font-family: Times New Roman; font-size: 16px;'>Dear All,</p>");
            html.Append($"<p style='font-family: Times New Roman; font-size: 16px;'> " +
                $"We are informing you that there is no stock in the warehouse for these PO'S but you have " +
                $"uploaded the schedule, please check with the warehouse department. .:" +
                $"</p>");


            html.Append("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse; width:100%; font-family: Times New Roman;'>");

            html.Append("<tr>");
            foreach (DataColumn column in dt.Columns)
            {
                html.Append("<th style='background-color:#f2f2f2; text-align:center;'>" + column.ColumnName + "</th>");
            }
            html.Append("</tr>");

            // Add the data rows
            foreach (DataRow row in dt.Rows)
            {
                html.Append("<tr>");
                foreach (var item in row.ItemArray)
                {
                    string cellValue = item.ToString().Trim();
                    int number;
                    if (int.TryParse(cellValue, out number))
                    {
                        if (number > 0)
                        {
                            html.Append($"<td style='text-align:center; background-color:#d4edda; color:#155724;'>{number} </td>");
                        }
                        else
                        {
                            html.Append("<td style='text-align:center; background-color:#f8d7da; color:#721c24;'>No Data</td>");
                        }
                    }
                    else
                    {
                        html.Append("<td style='text-align:center;'>" + System.Net.WebUtility.HtmlEncode(cellValue) + "</td>");
                    }
                }
                html.Append("</tr>");
            }

            html.Append("</table>");
            html.Append("</body></html>");

            return html.ToString();

        }



        private void GenerateExcelFileForStoppinglines(List<StopPlanningPOs> results, string filePath)  
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try
            {


                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("Error: File path is null or empty.");
                    return;
                }

                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Console.WriteLine($"Directory '{directoryPath}' was created.");
                }


                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("POS_List");

                    worksheet.Cells[1, 1].Value = "CUT_WEEK";
                    worksheet.Cells[1, 2].Value = "ARTICLE";
                    worksheet.Cells[1, 3].Value = "SO";
                    worksheet.Cells[1, 4].Value = "CUTTING_LINE";
                    using (var header = worksheet.Cells[1, 1, 1, 4])
                    {
                        header.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        header.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
                        header.Style.Font.Bold = true;
                        header.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        header.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    worksheet.Column(1).Width = 20;
                    worksheet.Column(2).Width = 40;
                    worksheet.Column(3).Width = 20;
                    worksheet.Column(4).Width = 40;
                    worksheet.Row(1).Height = 25; 

                    int row = 2;
                    foreach (var item in results)
                    {
                        worksheet.Cells[row, 1].Value = item.CUT_WEEK;
                        worksheet.Cells[row, 2].Value = item.ARTICLE;
                        worksheet.Cells[row, 3].Value = item.SO;
                        worksheet.Cells[row, 4].Value = item.CUTTING_LINE;
                        row++;
                    }

                    FileInfo fileInfo = new FileInfo(filePath);
                    package.SaveAs(fileInfo);
                    Console.WriteLine($"Excel file saved at: {fileInfo.FullName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while generating Excel file: {ex.Message}");
            }
        }


 
        public class PlanningPOs
        {
            public string CUT_WEEK { get; set; }
            public string ARTICLE { get; set; }
            public string SO { get; set; }
            public string CUTTING_LINE { get; set; }
            public string STITCHING_LINE { get; set; }
            public string ASSEMBLY_LINE { get; set; }
            public string ASS_WEEK { get; set; }

        }
        public class StopPlanningPOs
        {
            public string CUT_WEEK { get; set; }
            public string ARTICLE { get; set; }
            public string SO { get; set; }
            public string CUTTING_LINE { get; set; }
            public string STITCHING_LINE { get; set; }
            public string ASSEMBLY_LINE { get; set; }
            public string ASS_WEEK { get; set; }

        }
        #endregion

    }
}
