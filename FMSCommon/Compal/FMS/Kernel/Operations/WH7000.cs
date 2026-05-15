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
    class WH7000
    {
        public async Task<Cls_Return> GetWH7000Materials(SrvInfo vsrvinfo)
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
                        string plsql = $@" SELECT DISTINCT 
    d.item_no,
    d.unit,
    to_char(r.trans_date, 'yyyy/mm/dd' ) AS in_date,   
    a.note , a.name_t as material_name
FROM wms_trans_m r
JOIN wms_trans_d d
  ON r.trans_no = d.trans_no
  left join 
  ( 
     select d.item_no,m.note , i.name_t from wms_move_m m 
  inner join  wms_move_d d on m.move_no = d.move_no 
  inner join bdm_rd_item i on i.item_no = d.item_no
  where m.in_stocno = '7000'
  ) a 
  on a.item_no = d.item_no
WHERE r.stoc_no = '7000'
  AND r.inout_pz = 'IN'
    AND r.trans_date <= SYSDATE - 30  
  AND NOT EXISTS (
      SELECT 1
      FROM wms_trans_m r2
      JOIN wms_trans_d d2
        ON r2.trans_no = d2.trans_no
      WHERE r2.stoc_no = '7000'
        AND d2.item_no = d.item_no
        AND r2.inout_pz = 'OUT'
        AND r2.trans_date > r.trans_date
  )  
  AND R.TRANS_DATE >= TO_DATE ( '2025/12/01' , 'YYYY/MM/DD' ) ";

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
                        List<wh7000materials> maresult = new List<wh7000materials>();

                        while (rdrr.Read())
                        {
                            maresult.Add(new wh7000materials
                            {
                                ITEM_NO = rdrr["ITEM_NO"].ToString(),
                                UNIT = rdrr["UNIT"].ToString(),
                                IN_DATE = FormatDate(rdrr["IN_DATE"].ToString()),
                                material_name = rdrr["material_name"].ToString(),
                                note = rdrr["note"].ToString()
                            }
                            );
                        }

                        string messageforlackofmaterial = ForWH7000Material(dt);

                        string plexcelFilePath2 = Path.Combine(Path.GetTempPath(), "WH7000.xlsx");

                        GenerateExcelFileForWH7000(maresult, plexcelFilePath2);


                        string planningsql = $@"SELECT E.TO_LIST , E.CC_LIST , E.ERROR_LIST , E.MAIL_SUBJECT 
                                                 FROM TBL_E2E_MAIL_CONFIG E WHERE E.ROLECODE = 'WH7000' ";
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
                                attachment.Name = "WH7000.xlsx";
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

        /*public static string FormatDate(string inputDate)
        {
            DateTime parsedDate;

            if (DateTime.TryParseExact(
                    inputDate,
                    "yyyy/MM/dd HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out parsedDate))
            {
                return parsedDate.ToString("yyyy/MM/dd");
            }
            else
            {
                return "Invalid Date";
            }
        }*/

        /*   public static string FormatDate(string inputDate)
           {
               return DateTime.Parse(inputDate).ToString("yyyy/MM/dd");
           }*/


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

        private void GenerateExcelFileForWH7000(List<wh7000materials> results, string filePath)
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

                    worksheet.Cells[1, 1].Value = "ITEM_NO";
                    worksheet.Cells[1, 2].Value = "UNIT";
                    worksheet.Cells[1, 3].Value = "IN_DATE";
                    worksheet.Cells[1, 4].Value = "material_name";
                    worksheet.Cells[1, 5].Value = "note";
                    using (var header = worksheet.Cells[1, 1, 1, 5])
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
                    worksheet.Column(4).Width = 60;
                    worksheet.Column(5).Width = 60;
                    worksheet.Row(1).Height = 25;
                    int row = 2;
                    foreach (var item in results)
                    {
                        worksheet.Cells[row, 1].Value = item.ITEM_NO;
                        worksheet.Cells[row, 2].Value = item.UNIT;
                        worksheet.Cells[row, 3].Value = item.IN_DATE;
                        worksheet.Cells[row, 4].Value = item.material_name;
                        worksheet.Cells[row, 5].Value = item.note;
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

        public string ForWH7000Material(DataTable dt)
        {
            StringBuilder html = new StringBuilder();

            html.Append("<html><body>");

            string currentMonth = DateTime.Now.ToString("MMMM yyyy");
            html.Append($"<p style='font-family: Times New Roman; font-size: 16px;'>Dear All,</p>");
            html.Append($"<p style='font-family: Times New Roman; font-size: 16px;'> " +
                $"We would like to inform you that these materials from the 7000 warehouse have not been issued." +
                $"Please check and transfer the materials within 30 days from the in-date. " +
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
        public class wh7000materials
        {
            public string ITEM_NO { get; set; }
            public string UNIT { get; set; }
            public string IN_DATE { get; set; }
            public string material_name { get; set; }
            public string note { get; set; }

        }
    }
}
