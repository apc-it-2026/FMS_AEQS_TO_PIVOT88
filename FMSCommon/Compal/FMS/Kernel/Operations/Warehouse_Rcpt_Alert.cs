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
using OfficeOpenXml.Style;


namespace FMSCommon.Compal.FMS.Kernel.Operations   
{ 
    class Warehouse_Rcpt_Alert 
    { 
        public async Task<Cls_Return> WarehouseRepotRcptData(SrvInfo vsrvinfo)  
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

                        string plsql = $@"select ROW_NUMBER() Over(Order by a.rcpt_date) As S_NO,  
       to_char(a.rcpt_date, 'dd/MM/yyyy') as rcpt_date,
       a.org_id as Rcpt_Org,
       a.stoc_no as WH,
       a.vend_no,
       a.transport_way,
       a.deliver_no,
       LISTAGG(distinct a.chk_no, ',') as Rcpt_No,
       a.source_no,
       b.item_no,
       REPLACE(i.name_t, '\""', '') AS name_t,
       sum(b.rcpt_qty) as Rcpt_Qty,
       sum(c.ord_qty) as Purchase_Qty,
       sum(c.rcpt_qty - c.return_qty) as Finish_Qty,
       sum(c.ord_qty - (c.rcpt_qty - c.return_qty)) as Balance,
       (case a.Status
         when '1' then
          'New'
         when '2' then
          'Check'
         when '7' then
          'Confirmed'
         else
          a.status
       end) Status,
       b.batch_no,
       a.insert_user
  from WMS_RCPT_M a 
  left join WMS_RCPT_D b 
    on a.chk_no = b.chk_no
  left join bdm_purchase_order_item c
    on a.source_no = c.order_no
   and b.source_seq = c.order_seq
  left join bdm_rd_item i
    on i.item_no = b.item_no
 where rcpt_by in ('01') 
   and RCPT_WAY is null
   and a.org_id = '5001'
   and a.stoc_no IN ( '1000' , '1007' ) 
   and a.status = 2
   and a.rcpt_date <= TRUNC(SYSDATE) - 3 
 group by a.chk_no,
          a.vend_no,
          a.deliver_no,
          a.rcpt_date,
          a.stoc_no,
          a.source_no,
          REPLACE(i.name_t, '\""', ''),
          a.status,
          a.insert_user,
          a.transport_way,
          b.batch_no,
          b.item_no,
          a.org_id
 order by a.rcpt_date, a.chk_no";

                        DataTable dt = new DataTable();
                        using (OracleConnection connn = new OracleConnection(constroa))
                        {
                            connn.Open();
                            using (OracleCommand cmddd = new OracleCommand(plsql, connn))
                            {
                                using (OracleDataAdapter adapterr = new OracleDataAdapter(cmddd))
                                {
                                    adapterr.Fill(dt);
                                }
                            }
                        }

                        if (dt.Rows.Count <= 0)
                        {
                            return null;
                        }


                        OracleCommand plcmdd = new OracleCommand(plsql, conoa);
                        OracleDataReader rdrr = plcmdd.ExecuteReader();
                        List<RCPTDATA> maresult = new List<RCPTDATA>();

                        while (rdrr.Read())
                        {
                            maresult.Add(new RCPTDATA
                            {
                                RCPT_DATE = rdrr["RCPT_DATE"]?.ToString(),
                                RCPT_ORG = rdrr["RCPT_ORG"]?.ToString(),
                                WH = rdrr["WH"]?.ToString(),
                                VEND_NO = rdrr["VEND_NO"]?.ToString(),
                                TRANSPORT_WAY = rdrr["TRANSPORT_WAY"]?.ToString(),
                                DELIVER_NO = rdrr["DELIVER_NO"]?.ToString(),
                                RCPT_NO = rdrr["RCPT_NO"]?.ToString(),
                                SOURCE_NO = rdrr["SOURCE_NO"]?.ToString(),
                                ITEM_NO = rdrr["ITEM_NO"]?.ToString(),
                                NAME_T = rdrr["NAME_T"]?.ToString(),
                                RCPT_QTY = rdrr["RCPT_QTY"]?.ToString(),
                                PURCHASE_QTY = rdrr["PURCHASE_QTY"]?.ToString(),
                                FINISH_QTY = rdrr["FINISH_QTY"]?.ToString(),
                                BALANCE = rdrr["BALANCE"]?.ToString(),
                                STATUS = rdrr["STATUS"]?.ToString(),
                                BATCH_NO = rdrr["BATCH_NO"]?.ToString(),
                                INSERT_USER = rdrr["INSERT_USER"]?.ToString()
                            });
                        }


                        string messageforlackofmaterial = ForWarehouseRepotRcptData(dt);

                        string plexcelFilePath2 = Path.Combine(Path.GetTempPath(), "WarehouseRepotRcptData.xlsx");

                        GenerateExcelFileForWarehouseRepotRcptData(maresult, plexcelFilePath2);


                        string planningsql = $@"SELECT E.TO_LIST , E.CC_LIST , E.ERROR_LIST , E.MAIL_SUBJECT 
                                                 FROM TBL_E2E_MAIL_CONFIG E WHERE E.ROLECODE = 'WRM3' ";
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
                            // string host = "apcmx1.apachefootwear.com";
                            string host = "10.3.0.250";
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
                                attachment.Name = "WarehouseRepotRcptData.xlsx";
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

        public static string FormatDate(string inputDate)
        {
            DateTime parsedDate;

            if (DateTime.TryParse(inputDate, out parsedDate))
            {
                return parsedDate.ToString("yyyy/MM/dd");
            }
            else
            {
                return "Invalid Date";
            }
        }

        private void GenerateExcelFileForWarehouseRepotRcptData(List<RCPTDATA> results, string filePath)
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
                }

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("RCPT_DATA");

                    // ✅ Headers
                    string[] headers = {
                "RCPT_DATE","RCPT_ORG","WH","VEND_NO","TRANSPORT_WAY",
                "DELIVER_NO","RCPT_NO","SOURCE_NO","ITEM_NO","NAME_T",
                "RCPT_QTY","PURCHASE_QTY","FINISH_QTY","BALANCE",
                "STATUS","BATCH_NO","INSERT_USER"
            };

                    for (int col = 0; col < headers.Length; col++)
                    {
                        worksheet.Cells[1, col + 1].Value = headers[col];
                    }

                    // ✅ Header Styling
                    using (var header = worksheet.Cells[1, 1, 1, headers.Length])
                    {
                        header.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        header.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
                        header.Style.Font.Bold = true;
                        header.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        header.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    worksheet.Row(1).Height = 25;

                    // ✅ Data Mapping
                    int row = 2;
                    foreach (var item in results)
                    {
                        worksheet.Cells[row, 1].Value = item.RCPT_DATE;
                        worksheet.Cells[row, 2].Value = item.RCPT_ORG;
                        worksheet.Cells[row, 3].Value = item.WH;
                        worksheet.Cells[row, 4].Value = item.VEND_NO;
                        worksheet.Cells[row, 5].Value = item.TRANSPORT_WAY;
                        worksheet.Cells[row, 6].Value = item.DELIVER_NO;
                        worksheet.Cells[row, 7].Value = item.RCPT_NO;
                        worksheet.Cells[row, 8].Value = item.SOURCE_NO;
                        worksheet.Cells[row, 9].Value = item.ITEM_NO;
                        worksheet.Cells[row, 10].Value = item.NAME_T;
                        worksheet.Cells[row, 11].Value = item.RCPT_QTY;
                        worksheet.Cells[row, 12].Value = item.PURCHASE_QTY;
                        worksheet.Cells[row, 13].Value = item.FINISH_QTY;
                        worksheet.Cells[row, 14].Value = item.BALANCE;
                        worksheet.Cells[row, 15].Value = item.STATUS;
                        worksheet.Cells[row, 16].Value = item.BATCH_NO;
                        worksheet.Cells[row, 17].Value = item.INSERT_USER;

                        row++;
                    }

                    // ✅ Auto fit columns (better than fixed width)
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

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

        public string ForWarehouseRepotRcptData(DataTable dt)
        {
            StringBuilder html = new StringBuilder();

            html.Append("<html><body>");

            string currentMonth = DateTime.Now.ToString("MMMM yyyy");
            html.Append($"<p style='font-family: Times New Roman; font-size: 16px;'>Dear All,</p>");
            html.Append($"<p style='font-family: Times New Roman; font-size: 16px;'> " +
                $"We would like to inform you that these materials from the 1000 & 1007 warehouse have not been confirmed." + 
                $"Please check and confirm receive material in check state once." +
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
        public class RCPTDATA
        {
            public string RCPT_DATE { get; set; }
            public string RCPT_ORG { get; set; }
            public string WH { get; set; }
            public string VEND_NO { get; set; }
            public string TRANSPORT_WAY { get; set; }
            public string DELIVER_NO { get; set; }
            public string RCPT_NO { get; set; }
            public string SOURCE_NO { get; set; }
            public string ITEM_NO { get; set; }
            public string NAME_T { get; set; }
            public string RCPT_QTY { get; set; }
            public string PURCHASE_QTY { get; set; }
            public string FINISH_QTY { get; set; }
            public string BALANCE { get; set; }
            public string STATUS { get; set; }
            public string BATCH_NO { get; set; }
            public string INSERT_USER { get; set; }
        }


    }
}
