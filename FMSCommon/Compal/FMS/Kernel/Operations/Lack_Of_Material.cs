using Compal.FMS.Kernel.Beans;
using Compal.FMS.Kernel.Operations;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Forms ;
using System.IO;
using Compal.FMS.Connections.DBLoader;
using System.Collections; 
using System.Net.Mail;
using System.Net;
using System.Data;
using SJeMES_Framework_NETCore.DBHelper;
using System.Drawing;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
//using System.Windows.Documents;

namespace FMSCommon.Compal.FMS.Kernel.Operations
{
    class Lack_Of_Material
    {
        #region Lack_Of_Material
        public class Retdata {
            public string status { get; set;  }  
            public string message { get; set; }   
        } 

        public async Task<Retdata> GetLackofMaterialPos(SrvInfo vsrvinfo)   
        {
            Retdata retdata = new Retdata();
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
                        List<MaterialStatusClass> planningList = GetMaterialStatus(conoa);    
                        DataTable dt = ToDataTable(planningList);   
                        List<string> sendResult = SendWhatsappMessageAsImage(dt, conoa );   
                    } catch (Exception ex)
                    {

                    } finally
                    {
                        conoa.Close(); 
                    }

                }

                } catch (Exception ex)
            {

            } 
            finally { 
                retdata = null; 
            }
            return retdata; 
        }
        public static List<MaterialStatusClass> GetMaterialStatus(OracleConnection con)
        {
            List<MaterialStatusClass> list = new List<MaterialStatusClass>();
            List<PlanningRow> planningList = GetPlanningListData(con);
            try
            {
                foreach (var so in planningList)
                {
                    string status = CheckStatusForSingleSO(so.SalesOrder);

                    list.Add(new MaterialStatusClass
                    {
                        Week = so.Week,
                        SalesOrder = so.SalesOrder,
                        Cono = so.Cono,
                        ShoeName = so.ShoeName,
                        Crd = so.Crd,
                        Psdd = so.Psdd,
                        Qty = so.Qty,
                        Line = so.Line,
                        Plant = so.Plant,
                        material = string.IsNullOrEmpty(status) ? "No Status" : status
                    });
                }
            }
            catch (Exception ex)
            {
                // Log error
            }

            return list;
        }
        public static List<PlanningRow> GetPlanningListData(OracleConnection conoa)
        {
            List<PlanningRow> list = new List<PlanningRow>();

            string week = GetWeekRange(DateTime.Now);

            string sql = $@"
        SELECT 
            p.week, 
            p.sales_order, 
            p.cono, 
            p.shoe_name, 
            p.crd ,  
            p.psdd ,  
            p.qty ,  
            p.line ,  
            p.plant  
        FROM t_cmt_planning_schedule p 
        WHERE p.week = '{week}' 
          AND p.process = 'C'  
    ";

            OracleCommand cmd = new OracleCommand(sql, conoa);
            OracleDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                PlanningRow row = new PlanningRow
                {
                    Week = reader["week"]?.ToString(),
                    SalesOrder = reader["sales_order"]?.ToString(),
                    Cono = reader["cono"]?.ToString(),
                    ShoeName = reader["shoe_name"]?.ToString(),
                    Crd = FormatDate(reader["crd"]?.ToString()),
                    Psdd = FormatDate(reader["psdd"]?.ToString()),
                    Qty = reader["qty"]?.ToString(),
                    Line = reader["line"]?.ToString(),
                    Plant = reader["plant"]?.ToString()
                };

                list.Add(row);
            }

            return list;
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


        public static string CheckStatusForSingleSO(string seid )  
        {
            var materialStatuses = GetMaterialStatusForEveryMaterial(seid); 

            if (materialStatuses == null || materialStatuses.Count == 0)
                return "No Data";

            var statuses = materialStatuses.Select(s => s.Status).ToList();

            // Priority order
            if (statuses.Any(s => s.Equals("Shortage", StringComparison.OrdinalIgnoreCase)))
                return "Material Shortage";

            if (statuses.Any(s => s.Equals("Partially Issued", StringComparison.OrdinalIgnoreCase)))
                return "Partially Issued";

            if (statuses.All(s => s.Equals("Material Received - Not Issued", StringComparison.OrdinalIgnoreCase)))
                return "Waiting for Issue";

            if (statuses.All(s => s.Equals("Completely Issued", StringComparison.OrdinalIgnoreCase)))
                return "Completely Issued";

            if (statuses.Any(s => s.Equals("Partially Available", StringComparison.OrdinalIgnoreCase) ||
                                  s.Equals("Material Received - Not Issued", StringComparison.OrdinalIgnoreCase)))
                return "Partially Available";

            return "Unknown / Need Review";
        } 

        public static List<Materialcode> GetMaterialStatusForEveryMaterial(string seid )  
        {
            DataTable dt = GetWarehouseStockBySO(seid); 

            if (dt == null || dt.Rows.Count == 0)
                return new List<Materialcode> { new Materialcode { MaterialCode = "N/A", Status = "No Data" } }; 

            List<Materialcode> materialStatuses = new List<Materialcode>(); 

            foreach (DataRow row in dt.Rows)
            {
                float DQ = row["xql"] == DBNull.Value ? 0 : Convert.ToSingle(row["xql"]);
                float PQ = row["menge"] == DBNull.Value ? 0 : Convert.ToSingle(row["menge"]);
                float RQ = row["hl"] == DBNull.Value ? 0 : Convert.ToSingle(row["hl"]);
                float SQ = row["ql"] == DBNull.Value ? 0 : Convert.ToSingle(row["ql"]);
                float IQ = row["yfl"] == DBNull.Value ? 0 : Convert.ToSingle(row["yfl"]);
                float DAI = row["kky"] == DBNull.Value ? 0 : Convert.ToSingle(row["kky"]);
                float DPI = row["kyg"] == DBNull.Value ? 0 : Convert.ToSingle(row["kyg"]);

                string materialCode = row["idnrk"].ToString();

                string status = GetMaterialStatus(DQ, PQ, RQ, SQ, IQ, DAI, DPI);
                materialStatuses.Add(new Materialcode
                {
                    MaterialCode = materialCode,
                    Status = status
                });
 
            }

            return materialStatuses;
        }

        private static string GetMaterialStatus(float DQ, float PQ, float RQ, float SQ, float IQ, float DAI, float DPI)
        {
            float totalAvailable = RQ + DAI + DPI;
            // 1
            if (IQ >= DQ)
                return "Completely Issued";
            //2 
            else if (totalAvailable >= DQ && IQ > 0 && IQ < DQ)
                return "Partially Issued";
            // 3 
            else if (totalAvailable >= DQ && IQ == 0)
                return "Material Received - Not Issued";
            /*else if (totalAvailable >= DQ && SQ == 0)
                return "All Material OK";*/
            // 4 
            else if (totalAvailable > 0 && totalAvailable < DQ)
                return "Partially Available";
            // 5 
            else if (PQ == 0 || PQ < DQ)
                return "Purchase Pending";
            // 6 
            else if (SQ > 0)
                return "Shortage";
            else
                return "Unknown / Need Review";
        } 

        private static DataTable ConvertJsonToDataTable(string json)
        {
            try
            {
                if (json.TrimStart().StartsWith("["))
                {
                    return JsonConvert.DeserializeObject<DataTable>(json);
                }
                else
                {
                    JObject obj = JObject.Parse(json);
                    foreach (var property in obj.Properties())
                    {
                        JToken token = property.Value;
                        if (token is JArray arr)
                        {
                            return arr.ToObject<DataTable>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error converting JSON to DataTable: " + ex.Message);
            }

            return new DataTable();
        }

        private static decimal SafeDecimal(object value)
        {
            if (value == null) return 0;
            if (value == DBNull.Value) return 0;

            decimal result;
            return decimal.TryParse(value.ToString(), out result) ? result : 0;
        } 

        public static DataTable GetWarehouseStockBySO(string seid) 
        { 
            try
            {
                string apiUrl = "http://acqy-bwapp2.apachefootwear.com:8000/sap/zcl_sap_zmm074?sap-client=800";
                var body = new[]
                {
                new { S_VBELN_LOW = seid  }
                };
                string jsonBody = JsonConvert.SerializeObject(body);
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(2);

                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"API returned error: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                    string resultJson = response.Content.ReadAsStringAsync().Result;

                    DataTable dt = ConvertJsonToDataTable(resultJson);
                    var filteredRows = dt.AsEnumerable()
                       .Where(row => row.Field<string>("sortf") == "C");

                    dt = filteredRows.Any() ? filteredRows.CopyToDataTable() : dt.Clone();

                    string[] selectedColumns = { "vbeln", "maktx", "satnr" , "idnrk", "zptnm", "satnr2",
                                                 "zmakt1" ,"kwmeng" ,"werks", "zpno", "zlpd" ,"ktext" ,
                                                 "begda" , "edate" , "sortf" , "menge" , "hl" ,"ql" ,
                                                 "yfl" ,"xql" ,"kky" ,"wfl" , "kyg" };
                    DataTable result = new DataTable();
                    if (dt.Rows.Count <= 0)
                    {
                        return result;
                    }
                    dt = dt.DefaultView.ToTable(false, selectedColumns);
                    var GroupedData = from row in dt.AsEnumerable()
                                      group row by new
                                      {
                                          vbeln = row.Field<string>("vbeln"),
                                          maktx = row.Field<string>("maktx"),
                                          satnr = row.Field<string>("satnr"),
                                          idnrk = row.Field<string>("idnrk"),
                                          satnr2 = row.Field<string>("satnr2"),
                                          zmakt1 = row.Field<string>("zmakt1"),
                                          zpno = row.Field<string>("zpno"),
                                          zptnm = row.Field<string>("zptnm"),
                                          werks = row.Field<string>("werks"),
                                          sortf = row.Field<string>("sortf"),
                                          zlpd = row.Field<string>("zlpd"),
                                          begda = row.Field<string>("begda"),
                                          edate = row.Field<string>("edate"),
                                      } into g
                                      select new
                                      {
                                          idnrk = g.Key.idnrk,
                                          zpno = g.Key.zpno,
                                          zptnm = g.Key.zptnm,
                                          vbeln = g.Key.vbeln,
                                          maktx = g.Key.maktx,
                                          satnr = g.Key.satnr,
                                          satnr2 = g.Key.satnr2,
                                          zmakt1 = g.Key.zmakt1,
                                          werks = g.Key.werks,
                                          sortf = g.Key.sortf,
                                          zlpd = g.Key.zlpd,
                                          begda = g.Key.begda,
                                          edate = g.Key.edate,

                                          menge = g.Sum(x => SafeDecimal(x["menge"])),
                                          hl = g.Sum(x => SafeDecimal(x["hl"])),
                                          ql = g.Sum(x => SafeDecimal(x["ql"])),
                                          yfl = g.Sum(x => SafeDecimal(x["yfl"])),
                                          xql = g.Sum(x => SafeDecimal(x["xql"])),
                                          kky = g.Sum(x => SafeDecimal(x["kky"])),
                                          wfl = g.Sum(x => SafeDecimal(x["wfl"])),
                                          kyg = g.Sum(x => SafeDecimal(x["kyg"]))
                                      };

                    result.Columns.Add("idnrk");
                    result.Columns.Add("zpno");
                    result.Columns.Add("zptnm");
                    result.Columns.Add("vbeln");
                    result.Columns.Add("maktx");
                    result.Columns.Add("satnr");
                    result.Columns.Add("satnr2");
                    result.Columns.Add("zmakt1");
                    result.Columns.Add("werks");
                    result.Columns.Add("sortf");
                    result.Columns.Add("zlpd");
                    result.Columns.Add("begda");
                    result.Columns.Add("edate");
                    result.Columns.Add("menge", typeof(decimal));
                    result.Columns.Add("hl", typeof(decimal));
                    result.Columns.Add("ql", typeof(decimal));
                    result.Columns.Add("yfl", typeof(decimal));
                    result.Columns.Add("xql", typeof(decimal));
                    result.Columns.Add("kky", typeof(decimal));
                    result.Columns.Add("wfl", typeof(decimal));
                    result.Columns.Add("kyg", typeof(decimal));

                    foreach (var item in GroupedData)
                    {
                        result.Rows.Add(
                            item.idnrk,
                            item.zpno,
                            item.zptnm,
                            item.vbeln,
                            item.maktx,
                            item.zlpd,
                            item.satnr,
                            item.satnr2,
                            item.zmakt1,
                            item.werks,
                            item.sortf,
                            item.begda,
                            item.edate,
                            item.menge,
                            item.hl,
                            item.ql,
                            item.yfl,
                            item.xql,
                            item.kky,
                            item.wfl,
                            item.kyg
                        );
                    }

                    return result;
                }
            }

            catch (Exception ex)
            {
                throw new Exception("Error fetching warehouse stock from SAP API: " + ex.Message);
            }
        }
        public class Materialcode
        {
            public string MaterialCode { get; set; }
            public string Status { get; set; }
        }
        public class PlanningRow
        {
            public string Week { get; set; }
            public string SalesOrder { get; set; }
            public string Cono { get; set; }
            public string ShoeName { get; set; }
            public string Crd { get; set; }
            public string Psdd { get; set; }
            public string Qty { get; set; }
            public string Line { get; set; }
            public string Plant { get; set; }
        }

        public class MaterialStatusClass 
        {
            public string Week { get; set; }
            public string SalesOrder { get; set; }
            public string Cono { get; set; }
            public string ShoeName { get; set; }
            public string Crd { get; set; }
            public string Psdd { get; set; }
            public string Qty { get; set; }
            public string Line { get; set; }
            public string Plant { get; set; }
            public string material { get; set; }  
        }
        public class MailConfig
        {
            public string Subject { get; set; }
            public List<string> ToList { get; set; }
            public List<string> CcList { get; set; }
        }
        public static string GetWeekRange(DateTime today)
        {
            // If today is Saturday → next week's Sunday to Saturday
            if (today.DayOfWeek == DayOfWeek.Saturday)
            {
                DateTime nextSunday = today.AddDays(1);        // tomorrow
                DateTime nextSaturday = nextSunday.AddDays(6); // +6 days

                return $"{nextSunday:yyyy/MM/dd}-{nextSaturday:yyyy/MM/dd}";
            }

            // For Monday–Friday → previous Sunday to this Saturday
            int daysFromSunday = (int)today.DayOfWeek; // Sunday=0, Monday=1…Friday=5
            DateTime previousSunday = today.AddDays(-daysFromSunday);
            DateTime thisSaturday = previousSunday.AddDays(6); 
            return $"{previousSunday:yyyy/MM/dd}-{thisSaturday:yyyy/MM/dd}";
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            var dt = new DataTable(typeof(T).Name);
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                dt.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                dt.Rows.Add(values);
            }
            return dt;
        }

        public static List<string> SendWhatsappMessageAsImage(DataTable dt, OracleConnection conna)
        {
            string html = BuildHTML(dt);
            MailConfig cfg = GetMailConfig(conna); 
            string subject = cfg.Subject;
            var toList = cfg.ToList;
            var ccList = cfg.CcList;

            return SendAlert(html, subject, toList, ccList); 
        } 

        public static string BuildHTML(DataTable dt)
        {
            var sb = new StringBuilder(); 

            sb.Append("<html><head><style>");
            sb.Append("body{font-family:Arial;background:#f4f4f9;padding:20px;}");
            sb.Append("table{width:100%;border-collapse:collapse;margin-top:20px;}");
            sb.Append("th{background:#105359;color:white;padding:10px;font-size:14px;}");
            sb.Append("td{border:1px solid #ccc;padding:8px;text-align:center;font-size:13px;}");
            sb.Append("</style></head><body>");
            sb.Append("<h2>Sales Order Material Status</h2>");
            sb.Append("<h3>For Detail View Please check in our website <a href='http://10.3.0.24:8075' target='_blank'>10.3.0.24:8075</a></h3>");
            sb.Append("<table><tr>");

            // Add table headers
            foreach (DataColumn col in dt.Columns)
            {
                sb.Append($"<th>{col.ColumnName}</th>");
            }
            sb.Append("</tr>");

            // Add table rows
            foreach (DataRow row in dt.Rows)
            {
                sb.Append("<tr>");
                foreach (var item in row.ItemArray)
                {
                    sb.Append($"<td>{item}</td>");
                }
                sb.Append("</tr>");
            }

            sb.Append("</table></body></html>");

            return sb.ToString();
        } 

        public static MailConfig GetMailConfig(OracleConnection conna)
        {
            string sql = @"SELECT TO_LIST, CC_LIST, MAIL_SUBJECT 
                   FROM tbl_e2e_mail_config 
                   WHERE ROLECODE = 'CMT00'";

            OracleCommand cmd = new OracleCommand(sql, conna);
            OracleDataReader reader = cmd.ExecuteReader();

            // No record?
            if (!reader.Read())
            {
                return new MailConfig
                {
                    Subject = "",
                    ToList = new List<string>(),
                    CcList = new List<string>()
                };
            }

            string subject = reader["MAIL_SUBJECT"]?.ToString() ?? "";
            string toListRaw = reader["TO_LIST"]?.ToString() ?? "";
            string ccListRaw = reader["CC_LIST"]?.ToString() ?? "";

            var result = new MailConfig
            {
                Subject = subject,
                ToList = toListRaw
                            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .ToList(),
                CcList = ccListRaw
                            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .ToList()
            };

            return result;
        }

        public static List<string> SendAlert(string html,
                                       string subject,
                                       List<string> recipientEmails,
                                       List<string> ccEmails)
        {
            List<string> result = new List<string>();

            using (var client = new SmtpClient())
            {
                string userEmailAddress = "IT-announcement@in.apachefootwear.com";
                string userName = "Remainder Mail";
                string password = "it-123456";
                string host = "10.3.0.250";
                int port = 25;

                try
                {
                    MailMessage msg = new MailMessage();
                    msg.From = new MailAddress(userEmailAddress, userName);
                    msg.Subject = subject;
                    msg.Body = html;
                    msg.BodyEncoding = Encoding.UTF8;
                    msg.IsBodyHtml = true;
                    msg.Priority = MailPriority.High;

                    // TO Mail
                    foreach (string email in recipientEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                        msg.To.Add(email.Trim());

                    // CC Mail
                    foreach (string email in ccEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                        msg.CC.Add(email.Trim());

                    // SMTP Settings
                    client.Host = host;
                    client.Port = port;
                    client.UseDefaultCredentials = false;
                    client.EnableSsl = false;
                    client.Credentials = new NetworkCredential(userEmailAddress, password);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    client.Send(msg);

                    result.Add("SUCCESS");
                    result.Add("Mail sent successfully.");
                }
                catch (Exception ex)
                {
                    result.Add("ERROR");
                    result.Add(ex.Message);
                }
            }

            return result;
        }
        #endregion
    }
}
