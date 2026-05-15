using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using AutoSendEmail;
using Compal.FMS.Connections.DBLoader;
using Compal.FMS.Kernel.Beans;
using Compal.FMS.Kernel.Operations;
using FMSCommon.Compal.FMS.Kernel.Utils;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;

namespace FMSCommon.Compal.FMS.Kernel.Operations
{
    class Run_TempAndHumid_Data_Transfer
    {
        Cls_Return rt = new Cls_Return();
        SqlConnection con = new SqlConnection("Data Source=10.3.0.29;Initial Catalog=EasyMonitor;User ID=sa;Password=apc-1234;");
        SqlCommand cmd;
        SqlDataAdapter adapt;
        OracleConnection conoa = null;
        #region TempAndHumid_Data_Transfer
        public Cls_Return TempAndHumid_Data_Transfer(SrvInfo vsrvinfo)
        {
            con.Open();
            DataTable dt = new DataTable();
            adapt = new SqlDataAdapter("SELECT * FROM tab_historydata a where a.SaveTime >= DATEADD(minute, -30, GETDATE())", con);
            adapt.Fill(dt); 
            con.Close();
            
              
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

                        conoa = new OracleConnection(constroa);
                    conoa.Open();
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                        string sql = $@"insert into t_MSD_historydata(
               DEVKEY,
               DEVNAME,
               DEVADDRESS,
               DEVTYPE,
               DEVLOCATION,
               TRANSPORTNUMBER,
               LONGITUDE,
               LATITUDE,
               SPEED,
               TEMPVALUE,
               HUMIVALUE,
               DEWVALUE,
               TEMPALARMRANGE,
               HUMIALARMRANGE,
               SAVETIME,
               TEMPUNIT
               )values(
               '{dr["DEVKEY"]}',
               '{dr["DEVNAME"]}',
               '{dr["DEVADDRESS"]}',
               'Network Equipment',
               '{dr["DEVLOCATION"]}',
               '{dr["TRANSPORTNUMBER"]}',
               '{dr["LONGITUDE"]}',
               '{dr["LATITUDE"]}',
               '{dr["SPEED"]}',
               '{dr["TEMPVALUE"]}',
               '{dr["HUMIVALUE"]}',
               '{dr["DEWVALUE"]}',
               '{dr["TEMPALARMRANGE"]}',
               '{dr["HUMIALARMRANGE"]}',
               '{dr["SAVETIME"]}',
               '{dr["TEMPUNIT"]}')";

                       
                        OracleCommand cmd = new OracleCommand(sql, conoa);
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        DateTime today = DateTime.Now;
                        if (today.DayOfWeek != DayOfWeek.Sunday)
                        {
                            if (today.TimeOfDay >= TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)) && today.TimeOfDay <= TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(15)))
                            {
                                //SendMail();
                                string msg = $@"Temparature and Humidity devices are not connected to network. Please check";
                                SendMessage(msg);

                            }

                        }
                    }

                    conoa.Close();
                    
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
                    conoa.Close();
                    conoa.Dispose();

                    GC.Collect();
                }


            }
        public async Task SendMessage(string msg)
        {
            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendMessage";
            var payload = new
            {
                numbers = new[] { "9640416084" }, // Use the fetched phone number
                groups = new List<string>(),
                textMsg = msg,
                mediaurl = "",
                filename = ""
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);

            using (var httpClient = new HttpClient())
            {
                try
                {
                    // Set the content type to application/json
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send the POST request
                    var response = await httpClient.PostAsync(apiUrl, content); // Ensure url is defined

                    // Optionally log the response or handle errors here, but do not return
                    if (!response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "Temperature_And_Humidity");
                    }
                }
                catch (Exception ex)
                {
                    FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "Temperature_And_Humidity");
                }
            }
        }
        #endregion

        #region TempAndHumid_RangeExceedAlert
        public Cls_Return TempAndHumid_RangeExceedAlert(SrvInfo vsrvinfo)
        {
            try
            {
                string constroa = null;
                string filePath;

                filePath = Application.ExecutablePath;
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                string clientEnvConfigFileName = filePath + "database.config";
                XmlDocument clientEnvConfigDoc = new XmlDocument();
                DataTable dt = new DataTable();
                string fileName = $@"Temparature and Humidity Range Exceed Locations List{DateTime.Now:yyyyMMdd_HHmmss}";

                string msg = "*Please check the Temperature and Humidity data of all locations in above Image*";


                if (File.Exists(clientEnvConfigFileName))
                {
                    FileLoader obj = new FileLoader(clientEnvConfigFileName);
                    Hashtable htdblinks = obj.GetDBLinks();
                    if (htdblinks.ContainsKey(vsrvinfo.SDB))
                        constroa = htdblinks[vsrvinfo.SDB].ToString();

                    conoa = new OracleConnection(constroa);
                    conoa.Open();


                            string sql = $@"with tmp as
 (select m.devlocation,
         m.devname,
         case
           when maxtempalarmrange is null then
            'Above ' || mintempalarmrange
           else
            mintempalarmrange || '~' || maxtempalarmrange
         end as standard_temparature,
         m.tempvalue as current_temparature,
         case
           when maxtempalarmrange is not null then
            case
              when to_number(m.tempvalue) between to_number(mintempalarmrange) and
                   to_number(maxtempalarmrange) then
               '0'
              else
               '1'
            end
           else
            case
              when to_number(m.tempvalue) > to_number(mintempalarmrange) then
               '0'
              else
               '1'
            end
         end as tempstatus,
         case
           when minhumialarmrange is null then
            'Below ' || maxhumialarmrange
           else
            minhumialarmrange || '~' || maxhumialarmrange
         end as standard_humidity,
         m.humivalue as current_humidity,
         case
           when minhumialarmrange is not null then
            case
              when to_number(m.humivalue) between to_number(minhumialarmrange) and
                   to_number(maxhumialarmrange) then
               '0'
              else
               '1'
            end
           else
            case
              when to_number(m.humivalue) < to_number(maxhumialarmrange) then
               '0'
              else
               '1'
            end
         end as humstatus
    from t_msd_realtimedata m
    left join t_msd_devlist a
      on m.devname = a.devname
   where 1 = 1
     and to_char(savetime, 'yyyy-MM-dd') = to_char(sysdate, 'yyyy-mm-dd'))
select *
  from tmp t
 --where t.tempstatus = 1
   -- or t.humstatus = '1'
";

                            OracleCommand cmd = new OracleCommand(sql, conoa);
                            cmd.CommandType = CommandType.Text;
                           OracleDataAdapter da = new OracleDataAdapter(cmd);
                           da.Fill(dt);
                           conoa.Close();
                    if (dt.Rows.Count > 0)
                    {
                        string HTMLData = ConvertDataTableToHTML(dt);
                        SendWhatsappMessageAsimage(fileName, msg, HTMLData);
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
                conoa.Close();
                conoa.Dispose();

                GC.Collect();
            }


        }

        public List<string> SendWhatsappMessageAsimage(string fileName, string msg, string htmldata)
        {

            List<string> responseMessages = new List<string>();


            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";
            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "120363122655008537@g.us" },//120363122655008537@g.us(AEQS_Working_Condition)//120363347683285873@g.us(test)
                textMsg = msg,
                htmL_Code = htmldata,
                fileName = fileName
            };

            //var payload = new
            //{
            //    tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
            //    numbers = new[] { "9640416084" }, // Use the fetched phone number
            //    groups = new List<string>(),
            //    textMsg = msg,
            //    htmL_Code = htmldata,
            //    fileName = fileName
            //};


            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Serialize payload to JSON
                    string jsonPayload = JsonConvert.SerializeObject(payload);

                    // Create HttpContent for the JSON payload
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send POST request
                    HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = response.Content.ReadAsStringAsync().Result;
                        responseMessages.Add(responseData); // Add response to the list
                        FMSLOG.Platform(responseData, "Temperature_And_Humidity");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "Temperature_And_Humidity");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "Temperature_And_Humidity");
            }

            return responseMessages;

        }

        public string ConvertDataTableToHTML(DataTable dt)
        {
            StringBuilder html = new StringBuilder();

            // Start the HTML document
            html.Append("<html><body>");

            // Greeting and message
            string currentMonth = DateTime.Now.ToString("MMMM yyyy");
            html.Append("<p style='font-family: Times New Roman; font-size: 16px;'>Dear All,</p>");
            html.Append("<p style='font-family: Times New Roman; font-size: 16px;'>Please check the Temparature and Humidity data of all locations in the below image.</p>");

            // Heading
            html.Append("<h2 style='font-family: Times New Roman;'>Locations List</h2>");

            // Start table
            html.Append("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse; width:100%; font-family: Times New Roman;'>");

            // Add header row (skip TempStatus and HumStatus)
            html.Append("<tr>");
            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName.Equals("TempStatus", StringComparison.OrdinalIgnoreCase) ||
                    column.ColumnName.Equals("HumStatus", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // skip these
                }

                html.Append("<th style='background-color:#f2f2f2; text-align:center;'>" + column.ColumnName + "</th>");
            }
            html.Append("</tr>");

            // Add data rows
            foreach (DataRow row in dt.Rows)
            {
                html.Append("<tr>");

                // Read hidden values for conditional formatting
                int tempStatus = Convert.ToInt32(row["TempStatus"]);
                int humStatus = Convert.ToInt32(row["HumStatus"]);

                foreach (DataColumn col in dt.Columns)
                {
                    string colName = col.ColumnName;

                    // Skip hidden status columns
                    if (colName.Equals("TempStatus", StringComparison.OrdinalIgnoreCase) ||
                        colName.Equals("HumStatus", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string cellValue = row[col].ToString();
                    string style = "text-align:center;"; // default

                    // Apply conditional coloring
                    if (colName.Equals("standard_temparature", StringComparison.OrdinalIgnoreCase) ||
                        colName.Equals("current_temparature", StringComparison.OrdinalIgnoreCase))
                    {
                        style += tempStatus == 1
                                 ? "background-color:#f8d7da; color:#721c24;"  // red
                                 : "background-color:#d4edda; color:#155724;"; // green
                    }
                    else if (colName.Equals("standard_humidity", StringComparison.OrdinalIgnoreCase) ||
                             colName.Equals("current_humidity", StringComparison.OrdinalIgnoreCase))
                    {
                        style += humStatus == 1
                                 ? "background-color:#f8d7da; color:#721c24;"  // red
                                 : "background-color:#d4edda; color:#155724;"; // green
                    }

                    html.Append($"<td style='{style}'>" + System.Net.WebUtility.HtmlEncode(cellValue) + "</td>");
                }

                html.Append("</tr>");
            }

            // Close table
            html.Append("</table>");
            html.Append("</body></html>");

            return html.ToString();
        }
        #endregion

    }
}


