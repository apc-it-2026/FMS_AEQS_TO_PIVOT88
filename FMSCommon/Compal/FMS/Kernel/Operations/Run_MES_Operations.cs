using Compal.FMS.Connections.DBLoader;
using Compal.FMS.Kernel.Beans;
using Compal.FMS.Kernel.Operations;
using FMSCommon.Compal.FMS.Kernel.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace FMSCommon.Compal.FMS.Kernel.Operations
{

    class Run_MES_Operations
    {
        public readonly string mes_con = "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST =10.3.0.227)(PORT = 1521)))(CONNECT_DATA =(SERVER = DEDICATED) (SID = APCMES)(SERVICE_NAME = APCMES))); User Id =mes00; Password =dbmes00;";

        OracleConnection conoa = null;

        #region BGrade Report
        public Cls_Return Send_BGrade_Report(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            try
            {
                string constroa = null;
                string filePath;
                filePath = Application.ExecutablePath;
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                string clientEnvConfigFileName = filePath + "database.config";
                XmlDocument clientEnvConfigDoc = new XmlDocument();
                DataTable dt1 = new DataTable();
                DataTable dt2 = new DataTable();
                DataTable dt3 = new DataTable();
                string fileName = $@"BGrade_Report_Upto_{DateTime.Now:yyyyMMdd_HHmmss}";
                string msg = $@"Dear All, Please check the BGrade_Summary_Report_Upto_{DateTime.Now.AddDays(-1):yyyyMMdd} in above Image";
                string H1 = $@"Month Wise Comparision Report";
                string H2 = $@"Last Two Days Coparision Report";
                string H3 = $@"Previous Day Detailed Report";

                if (File.Exists(clientEnvConfigFileName))
                {
                    FileLoader obj = new FileLoader(clientEnvConfigFileName);
                    Hashtable htdblinks = obj.GetDBLinks();
                    if (htdblinks.ContainsKey(vsrvinfo.SDB))
                        constroa = htdblinks[vsrvinfo.SDB].ToString();

                    conoa = new OracleConnection(constroa);
                    conoa.Open();
                    #region Before Excess BGrade
                    //                   string sql1 = $@"SELECT MAX(CASE
                    //            WHEN rn = 2 THEN
                    //             month_label
                    //          END) AS Previous_Month,
                    //      MAX(CASE
                    //            WHEN rn = 2 THEN
                    //             quantity
                    //          END) AS BGrade_Qty,
                    //      MAX(CASE
                    //            WHEN rn = 1 THEN
                    //             month_label
                    //          END) AS Current_Month,
                    //      MAX(CASE
                    //            WHEN rn = 1 THEN
                    //             quantity
                    //          END) AS BGrade_Qty
                    // FROM (SELECT TO_CHAR(PROD_DATE, 'YYYYMM') AS ym,
                    //              TO_CHAR(PROD_DATE, 'YYYY') || ' ' ||
                    //              TRIM(TO_CHAR(PROD_DATE, 'Month')) AS month_label,
                    //              SUM(QUANTITY) AS quantity,
                    //              ROW_NUMBER() OVER(ORDER BY TO_CHAR(PROD_DATE, 'YYYYMM') DESC) AS rn
                    //         FROM kpi_bgrade_data
                    //        WHERE TRUNC(PROD_DATE, 'MM') >=
                    //              ADD_MONTHS(TRUNC(SYSDATE, 'MM'), -1)
                    //        GROUP BY TO_CHAR(PROD_DATE, 'YYYYMM'),
                    //                 TO_CHAR(PROD_DATE, 'YYYY') || ' ' ||
                    //                 TRIM(TO_CHAR(PROD_DATE, 'Month')))
                    //WHERE rn <= 2";

                    //                    string sql2 = $@"WITH last_two_days AS
                    // (SELECT dt, ROW_NUMBER() OVER(ORDER BY dt) rn
                    //    FROM (SELECT dt
                    //            FROM (SELECT TRUNC(SYSDATE) - LEVEL AS dt
                    //                    FROM dual
                    //                  CONNECT BY LEVEL <= 10)
                    //           WHERE TO_CHAR(dt, 'DY', 'NLS_DATE_LANGUAGE=ENGLISH') <> 'SUN'
                    //             AND dt NOT IN
                    //                 (SELECT calendar
                    //                    FROM DA_CALENDAR_S@APCHRDB
                    //                   WHERE org_id = 100
                    //                     AND TO_CHAR(calendar, 'YYYY') = TO_CHAR(SYSDATE, 'YYYY'))
                    //           ORDER BY dt DESC)
                    //   WHERE ROWNUM <= 2),
                    //data_summary AS
                    // (SELECT l.dt AS prod_date, NVL(SUM(k.quantity), 0) AS total_qty, l.rn
                    //    FROM last_two_days l
                    //    LEFT JOIN kpi_bgrade_data k
                    //      ON k.prod_date = l.dt
                    //   GROUP BY l.dt, l.rn)
                    //SELECT MAX(CASE
                    //             WHEN rn = 1 THEN
                    //              TO_CHAR(prod_date, 'YYYY-MM-DD')
                    //           END) AS PREVIOUS_WORKING_DAY,
                    //       MAX(CASE
                    //             WHEN rn = 1 THEN
                    //              total_qty
                    //           END) AS PREVIOUS_BGRADES,
                    //       MAX(CASE
                    //             WHEN rn = 2 THEN
                    //              TO_CHAR(prod_date, 'YYYY-MM-DD')
                    //           END) AS LATEST_WORKING_DAY,
                    //       MAX(CASE
                    //             WHEN rn = 2 THEN
                    //              total_qty
                    //           END) AS LATEST_BGRADES
                    //  FROM data_summary";

                    //                    string sql3 = $@"select PROD_DATE, PLANT, RESPONSIBLE_DEPT, MODEL_NAME, QUANTITY
                    //  from (SELECT TO_CHAR(a.prod_date, 'yyyy/MM/dd') AS prod_date,
                    //               CASE
                    //                 WHEN b.udf05 IS NULL THEN
                    //                  'MK1'
                    //                 ELSE
                    //                  b.udf05
                    //               END AS plant,
                    //               a.prod_line AS responsible_dept,
                    //               d.name_s AS model_name,
                    //               SUM(a.quantity) AS quantity,
                    //               1 AS sort_order
                    //          FROM kpi_bgrade_data a
                    //          LEFT JOIN base005m b
                    //            ON a.prod_line = b.department_code
                    //         INNER JOIN bdm_rd_item c
                    //            ON a.shoe_size = c.item_no
                    //         INNER JOIN bdm_rd_prod d
                    //            ON c.parent_item_no = d.prod_no
                    //         WHERE a.prod_date IN
                    //               (SELECT dt
                    //                  FROM (SELECT dt
                    //                          FROM (SELECT TRUNC(SYSDATE) - LEVEL AS dt
                    //                                  FROM dual
                    //                                CONNECT BY LEVEL <= 5)
                    //                         WHERE TO_CHAR(dt, 'DY', 'NLS_DATE_LANGUAGE=ENGLISH') <>
                    //                               'SUN'
                    //                           AND dt NOT IN
                    //                               (SELECT calendar
                    //                                  FROM DA_CALENDAR_S@APCHRDB
                    //                                 WHERE org_id = 100
                    //                                   AND TO_CHAR(calendar, 'yyyy') =
                    //                                       TO_CHAR(SYSDATE, 'yyyy'))
                    //                         ORDER BY dt DESC)
                    //                 WHERE ROWNUM = 1)
                    //         GROUP BY a.prod_date, a.prod_line, b.udf05, d.name_s
                    //        UNION ALL
                    //        SELECT prod_date,
                    //               plant_group AS plant,
                    //               NULL AS responsible_dept,
                    //               '' AS model_name,
                    //               SUM(quantity) AS quantity,
                    //               2 AS sort_order
                    //          FROM (SELECT TO_CHAR(a.prod_date, 'yyyy/MM/dd') AS prod_date,
                    //                       CASE
                    //                         WHEN NVL(b.udf05, 'MK1') = 'MK1' THEN
                    //                          'MK_TOTAL'
                    //                         WHEN NVL(b.udf05, 'MK1') = 'APEX' THEN
                    //                          'APEX_TOTAL'
                    //                         ELSE
                    //                          'APC_TOTAL'
                    //                       END AS plant_group,
                    //                       a.quantity
                    //                  FROM kpi_bgrade_data a
                    //                  LEFT JOIN base005m b
                    //                    ON a.prod_line = b.department_code
                    //                 INNER JOIN bdm_rd_item c
                    //                    ON a.shoe_size = c.item_no
                    //                 INNER JOIN bdm_rd_prod d
                    //                    ON c.parent_item_no = d.prod_no
                    //                 WHERE a.prod_date IN
                    //                       (SELECT dt
                    //                          FROM (SELECT dt
                    //                                  FROM (SELECT TRUNC(SYSDATE) - LEVEL AS dt
                    //                                          FROM dual
                    //                                        CONNECT BY LEVEL <= 5)
                    //                                 WHERE TO_CHAR(dt,
                    //                                               'DY',
                    //                                               'NLS_DATE_LANGUAGE=ENGLISH') <>
                    //                                       'SUN'
                    //                                   AND dt NOT IN
                    //                                       (SELECT calendar
                    //                                          FROM DA_CALENDAR_S@APCHRDB
                    //                                         WHERE org_id = 100
                    //                                           AND TO_CHAR(calendar, 'yyyy') =
                    //                                               TO_CHAR(SYSDATE, 'yyyy'))
                    //                                 ORDER BY dt DESC)
                    //                         WHERE ROWNUM = 1))
                    //         GROUP BY prod_date, plant_group

                    //         ORDER BY sort_order, prod_date, plant)
                    // order by plant";

                    #endregion

                    string SQL_date = $@"SELECT dt
  FROM (SELECT dt
          FROM (SELECT TRUNC(SYSDATE) - LEVEL AS dt
                  FROM dual
                CONNECT BY LEVEL <= 10)
         WHERE TO_CHAR(dt, 'DY', 'NLS_DATE_LANGUAGE=ENGLISH') <> 'SUN'
           AND dt NOT IN
               (SELECT calendar
                  FROM DA_CALENDAR_S@APCHRDB
                 WHERE org_id = 100
                   AND TO_CHAR(calendar, 'YYYY') = TO_CHAR(SYSDATE, 'YYYY'))
         ORDER BY dt DESC)
 WHERE ROWNUM <= 1";

                    #region after Excess BGrades
                    string sql1 = $@"SELECT MAX(CASE
             WHEN rn = 2 THEN
              month_label
           END) AS Previous_Month,
       MAX(CASE
             WHEN rn = 2 THEN
              quantity
           END) AS Previous_BGrade_Qty,
       MAX(CASE
             WHEN rn = 1 THEN
              month_label
           END) AS Current_Month,
       MAX(CASE
             WHEN rn = 1 THEN
              quantity
           END) AS Current_BGrade_Qty
  FROM (SELECT TO_CHAR(PROD_DATE, 'YYYYMM') AS ym,
               TO_CHAR(PROD_DATE, 'YYYY') || ' ' ||
               TRIM(TO_CHAR(PROD_DATE, 'Month')) AS month_label,
               SUM(QUANTITY) AS quantity,
               ROW_NUMBER() OVER(ORDER BY TO_CHAR(PROD_DATE, 'YYYYMM') DESC) AS rn
          FROM kpi_bgrade_data k
         WHERE TRUNC(PROD_DATE, 'MM') >=
               ADD_MONTHS(TRUNC(SYSDATE, 'MM'), -1)
           AND NOT EXISTS
         (SELECT 1 FROM BGRADE_SO b WHERE b.SALESORDER = k.SALESORDER)
         GROUP BY TO_CHAR(PROD_DATE, 'YYYYMM'),
                  TO_CHAR(PROD_DATE, 'YYYY') || ' ' ||
                  TRIM(TO_CHAR(PROD_DATE, 'Month')))
 WHERE rn <= 2";


                    string sql2 = $@"WITH last_two_days AS
 (SELECT dt, ROW_NUMBER() OVER(ORDER BY dt) rn
    FROM (SELECT dt
            FROM (SELECT TRUNC(SYSDATE) - LEVEL AS dt
                    FROM dual
                  CONNECT BY LEVEL <= 10)
           WHERE TO_CHAR(dt, 'DY', 'NLS_DATE_LANGUAGE=ENGLISH') <> 'SUN'
             AND dt NOT IN
                 (SELECT calendar
                    FROM DA_CALENDAR_S@APCHRDB
                   WHERE org_id = 100
                     AND TO_CHAR(calendar, 'YYYY') = TO_CHAR(SYSDATE, 'YYYY'))
           ORDER BY dt DESC)
   WHERE ROWNUM <= 2),
data_summary AS
 (SELECT l.dt AS prod_date, NVL(SUM(k.quantity), 0) AS total_qty, l.rn
    FROM last_two_days l
    LEFT JOIN kpi_bgrade_data k
      ON k.prod_date = l.dt
     AND NOT EXISTS
   (SELECT 1 FROM BGRADE_SO b WHERE b.SALESORDER = k.SALESORDER)
   GROUP BY l.dt, l.rn)
SELECT MAX(CASE
             WHEN rn = 1 THEN
              TO_CHAR(prod_date, 'YYYY-MM-DD')
           END) AS PREVIOUS_WORKING_DAY,
       MAX(CASE
             WHEN rn = 1 THEN
              total_qty
           END) AS PREVIOUS_BGRADES,
       MAX(CASE
             WHEN rn = 2 THEN
              TO_CHAR(prod_date, 'YYYY-MM-DD')
           END) AS LATEST_WORKING_DAY,
       MAX(CASE
             WHEN rn = 2 THEN
              total_qty
           END) AS LATEST_BGRADES
  FROM data_summary";

                    string sql3 = $@"WITH target_date AS
 (SELECT dt
    FROM (SELECT dt
            FROM (SELECT TRUNC(SYSDATE) - LEVEL dt
                    FROM dual
                  CONNECT BY LEVEL <= 5)
           WHERE TO_CHAR(dt, 'DY', 'NLS_DATE_LANGUAGE=ENGLISH') <> 'SUN'
             AND dt NOT IN
                 (SELECT calendar
                    FROM DA_CALENDAR_S@APCHRDB
                   WHERE org_id = 100
                     AND TO_CHAR(calendar, 'yyyy') = TO_CHAR(SYSDATE, 'yyyy'))
           ORDER BY dt DESC)
   WHERE ROWNUM = 1),

base_data AS
 (SELECT TO_CHAR(a.prod_date, 'yyyy/MM/dd') prod_date,
         NVL(b.udf05, 'MK1') plant,
         a.prod_line responsible_dept,
         d.name_s model_name,
         a.quantity
    FROM kpi_bgrade_data a
    LEFT JOIN base005m b
      ON a.prod_line = b.department_code
    JOIN bdm_rd_item c
      ON a.shoe_size = c.item_no
    JOIN bdm_rd_prod d
      ON c.parent_item_no = d.prod_no
   WHERE a.prod_date = (SELECT dt FROM target_date)
     AND NOT EXISTS
   (SELECT 1 FROM BGRADE_SO bs WHERE bs.SALESORDER = a.SALESORDER))

SELECT prod_date,
       plant,
       responsible_dept,
       model_name,
       SUM(quantity) quantity
  FROM (
        /* Detail rows → keep original plant */
        SELECT prod_date,
                plant,
                responsible_dept,
                model_name,
                quantity,
                1 sort_order
          FROM base_data
        
        UNION ALL
        
        /* Total rows → group others under APC_TOTAL */
        SELECT prod_date,
                CASE
                  WHEN plant = 'MK1' THEN
                   'MK_TOTAL'
                  WHEN plant = 'APEX' THEN
                   'APEX_TOTAL'
                  ELSE
                   'APC_TOTAL'
                END AS plant,
                NULL AS responsible_dept,
                '' AS model_name,
                quantity,
                2 sort_order
          FROM base_data)
 GROUP BY prod_date, plant, responsible_dept, model_name, sort_order
 ORDER BY CASE
            WHEN plant NOT IN
                 ('MK1', 'APEX', 'MK_TOTAL', 'APEX_TOTAL', 'APC_TOTAL') THEN
            
             1
            WHEN plant = 'APC_TOTAL' THEN
            
             2
            WHEN plant = 'APEX' THEN
             3
            WHEN plant = 'APEX_TOTAL' THEN
             4
            WHEN plant = 'MK1' THEN
             5
            WHEN plant = 'MK_TOTAL' THEN
             6
            ELSE
             7
          END,
          plant,
          responsible_dept,
          model_name";
                    #endregion
                    OracleCommand cmd1 = new OracleCommand(sql1, conoa);
                    cmd1.CommandType = CommandType.Text;
                    OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
                    da1.Fill(dt1);
                    OracleCommand cmd2 = new OracleCommand(sql2, conoa);
                    cmd2.CommandType = CommandType.Text;
                    OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                    da2.Fill(dt2);
                    OracleCommand cmd3 = new OracleCommand(sql3, conoa);
                    cmd3.CommandType = CommandType.Text;
                    OracleDataAdapter da3 = new OracleDataAdapter(cmd3);
                    da3.Fill(dt3);
                    conoa.Close();
                    if (dt1.Rows.Count > 0 || dt2.Rows.Count > 0 || dt3.Rows.Count > 0)
                    {
                        string HTMLData = ConvertBGradeDataToHTML(dt1, H1, dt2, H2, dt3, H3);
                        SendBGradeDataAsimage(fileName, msg, HTMLData);
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

        public static string ConvertBGradeDataToHTML(DataTable dt1, string h1, DataTable dt2, string h2, DataTable dt3, string h3)
        {
            StringBuilder finalHtml = new StringBuilder();

            // ====== Common Page Wrapper ======
            finalHtml.Append(@"
    <div style='
        background-color:#faf9f6;
        padding:20px;
        font-family:Segoe UI, Arial;
        color:#333;
    '>");

            // ====== Helper Function for Reuse ======
            string BuildTableHTML(DataTable dt, string heading, bool isThirdTable = false)
            {
                if (dt == null || dt.Rows.Count == 0)
                    return $"<p style='text-align:center; color:#999;'>No data available for {heading}</p>";

                StringBuilder html = new StringBuilder();

                // ======= Heading =======
                html.Append($@"
        <div style='text-align:center; margin:30px 0 15px 0;'>
            <h2 style='
                display:inline-block;
                background: linear-gradient(90deg, #ffb347, #ffcc33);
                color:#4a2c00;
                padding:10px 25px;
                border-radius:8px;
                font-family:Segoe UI, Arial;
                box-shadow:0 4px 10px rgba(0,0,0,0.15);
            '>
                {heading.Replace("_", " ")}
            </h2>
        </div>");

                // ======= Table Container =======
                html.Append(@"
        <div style='display:flex; justify-content:center; margin-bottom:30px;'>
        <table style='
            border-collapse:collapse;
            min-width:85%;
            font-family:Segoe UI, Arial;
            font-size:14px;
            border-radius:10px;
            overflow:hidden;
            box-shadow:0 4px 15px rgba(0,0,0,0.08);
        '>
        ");

                // ======= Table Header =======
                html.Append(@"
        <thead style='background:linear-gradient(90deg, #ffb347, #ffcc33); color:#4a2c00;'>
            <tr>");
                foreach (DataColumn col in dt.Columns)
                {
                    html.Append($"<th style='padding:10px 12px; text-align:center; font-weight:600; letter-spacing:0.5px;'>{col.ColumnName.Replace("_", " ")}</th>");
                }
                html.Append("</tr></thead>");

                // ======= Table Body =======
                html.Append("<tbody>");
                int rowIndex = 0;

                foreach (DataRow row in dt.Rows)
                {
                    // Alternate background shades
                    string bgColor;
                    if (isThirdTable)
                    {
                        bgColor = (rowIndex++ % 2 == 0) ? "#fff7e6" : "#fff0cc"; // warm orange shades for 3rd table
                    }
                    else
                    {
                        bgColor = (rowIndex++ % 2 == 0) ? "#fefefe" : "#f9f9f9"; // neutral for other tables
                    }

                    html.Append($"<tr style='background-color:{bgColor}; transition:background 0.3s;'>");

                    foreach (DataColumn col in dt.Columns)
                    {
                        string value = row[col]?.ToString() ?? string.Empty;

                        string cellStyle = @"
                    padding:10px; 
                    text-align:center; 
                    border-bottom:1px solid #ddd; 
                    color:#333;
                    ";

                        // ======= Highlight numeric columns slightly =======
                        if (double.TryParse(value, out _))
                        {
                            cellStyle += "color:#003366; font-weight:500;";
                        }

                        // ======= Special Formatting for Percent Change =======
                        if (col.ColumnName.Equals("PERCENT_CHANGE", StringComparison.OrdinalIgnoreCase))
                        {
                            string arrowHtml = "";
                            if (double.TryParse(value, out double percent))
                            {
                                if (percent < 0)
                                    arrowHtml = " <span style='color:green; font-size:16px;'>▼</span>";
                                else if (percent > 0)
                                    arrowHtml = " <span style='color:red; font-size:16px;'>▲</span>";
                                else
                                    arrowHtml = " <span style='color:gray;'>●</span>";
                            }
                            value = $"{System.Net.WebUtility.HtmlEncode(value)}{arrowHtml}";
                        }

                        html.Append($"<td style='{cellStyle}'>{value}</td>");
                    }

                    html.Append("</tr>");
                }

                html.Append("</tbody></table></div>");
                return html.ToString();
            }

            // ====== Append Each Table ======
            finalHtml.Append(BuildTableHTML(dt1, h1));
            finalHtml.Append(BuildTableHTML(dt2, h2));
            finalHtml.Append(BuildTableHTML(dt3, h3, isThirdTable: true)); // third table with custom alternating colors

            // ====== End Wrapper ======
            finalHtml.Append("</div>");

            return finalHtml.ToString();
        }

        public List<string> SendBGradeDataAsimage(string fileName, string msg, string htmldata)
        {

            List<string> responseMessages = new List<string>();


            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";
            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "919989502631-1597025029@g.us" },//120363122655008537@g.us(AEQS_Working_Condition)//120363347683285873@g.us(test)//919989502631-1597025029@g.us(Apache India Team)
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
                        FMSLOG.Platform(responseData, "BGrade_Data");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "BGrade_Data");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "BGrade_Data");
            }

            return responseMessages;

        }
        #endregion

        #region Auto_schedule_data
        public Cls_Return Send_Auto_Schedule_Insert_Report(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            try
            {
                string constroa = null;
                string filePath;
                filePath = Application.ExecutablePath;
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                string clientEnvConfigFileName = filePath + "database.config";
                XmlDocument clientEnvConfigDoc = new XmlDocument();
                DataTable dt = new DataTable();
                string fileName = $@"Auto_Schedule_Insert_Report{DateTime.Now:yyyyMMdd_HHmmss}";
                string msg = $@"Auto_Schedule_Insert_Report of {DateTime.Now.ToString("yyyy/MM/dd")}";

                if (File.Exists(clientEnvConfigFileName))
                {
                    FileLoader obj = new FileLoader(clientEnvConfigFileName);
                    Hashtable htdblinks = obj.GetDBLinks();
                    if (htdblinks.ContainsKey(vsrvinfo.SDB))
                        constroa = htdblinks[vsrvinfo.SDB].ToString();

                    conoa = new OracleConnection(constroa);
                    conoa.Open();


                    string sql = $@"SELECT to_char(K.DATE_,'yyyy/MM/dd') Prod_Date,
       RECORDS_NEED_TO_BE_INSERT Carry_Forward_Records,
       INSERTED_RECORDS Rolled_Out_Records,
       NVL(RECORDS_NEED_TO_BE_INSERT, 0) - NVL(INSERTED_RECORDS, 0) Pending_Records
  FROM (SELECT TRUNC(SYSDATE) DATE_,
               (SELECT COUNT(*)
                  FROM (SELECT S.SE_ID,
                               S.MAIN_PROD_ORDER,
                               S.PRODUCTION_ORDER,
                               S.D_DEPT,
                               SUM(S.work_qty) work_qty
                          FROM (SELECT b.UDF03 AS po,
                                       b.UDF10 AS art_no,
                                       b.ORDER_DATE AS se_day,
                                       a.org_id,
                                       a.se_id,
                                       a.size_no,
                                       NVL(a.size_seq, 0) size_seq,
                                       NVL(a.work_qty, 0) -
                                       NVL(a.finish_qty, 0) AS work_qty,
                                       0 AS supplement_qty,
                                       a.d_dept,
                                       DECODE(NVL(a.work_qty, 0) -
                                              NVL(a.finish_qty, 0),
                                              0,
                                              'N',
                                              'Y') AS column2,
                                       TO_CHAR((SYSDATE), 'yyyy/MM/dd') AS work_day,
                                       (SELECT rout_no
                                          FROM sjqdms_work_day
                                         WHERE org_id = a.org_id
                                           AND d_dept = a.d_dept
                                           AND se_id = a.se_id
                                           AND main_prod_order =
                                               a.main_prod_order
                                           AND work_day = a.work_day
                                           AND inout_pz = a.inout_pz
                                         GROUP BY rout_no) AS work_pz,
                                       a.inout_pz,
                                       a.production_order,
                                       a.main_prod_order
                                  FROM (SELECT org_id,
                                               se_id,
                                               se_seq,
                                               size_no,
                                               size_seq,
                                               d_dept,
                                               production_order,
                                               main_prod_order,
                                               work_day,
                                               inout_pz,
                                               SUM(NVL(work_qty, 0) +
                                                   NVL(supplement_qty, 0)) work_qty,
                                               SUM(NVL(finish_qty, 0)) finish_qty
                                          FROM sjqdms_work_day_size
                                         WHERE status = '7'
                                           AND work_day = TRUNC(SYSDATE - 1)
                                         GROUP BY org_id,
                                                  se_id,
                                                  se_seq,
                                                  size_no,
                                                  size_seq,
                                                  d_dept,
                                                  production_order,
                                                  main_prod_order,
                                                  work_day,
                                                  inout_pz) a,
                                       mes010m b
                                 WHERE a.org_id = b.ORG
                                   AND a.se_id = b.SALES_ORDER
                                   AND a.production_order = b.production_order
                                   AND NVL(a.work_qty, 0) -
                                       NVL(a.finish_qty, 0) > 0) S
                         GROUP BY S.SE_ID,
                                  S.MAIN_PROD_ORDER,
                                  S.PRODUCTION_ORDER,
                                  S.D_DEPT)) AS RECORDS_NEED_TO_BE_INSERT,
               (SELECT COUNT(*)
                  FROM SJQDMS_WORK_DAY_SIZE S
                 WHERE S.COLUMN1 = 'S'
                   AND S.WORK_DAY = TRUNC(SYSDATE)) AS INSERTED_RECORDS
          FROM DUAL) K";

                    OracleCommand cmd = new OracleCommand(sql, conoa);
                    cmd.CommandType = CommandType.Text;
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    da.Fill(dt);
                    conoa.Close();
                    if (dt.Rows.Count > 0)
                    {
                        string HTMLData = ConvertAutoScheduleDataToHTML(dt, msg);
                        SendAutoScheduleDataAsimage(fileName, msg, HTMLData);
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


        public static string ConvertAutoScheduleDataToHTML(DataTable dt, string heading)
        {
            if (dt == null || dt.Rows.Count == 0)
                return "<p>No records found.</p>";

            StringBuilder html = new StringBuilder();

            html.Append("<html><body style='font-family:Calibri, sans-serif; font-size:14px;'>");

            // ====== Heading ======
            html.AppendFormat("<h2 style='text-align:left; color:#2f5496; margin-bottom:10px;'>{0}</h2>", heading);

            // ====== Table ======
            html.Append("<table border='1' cellspacing='0' cellpadding='6' " +
                        "style='border-collapse:collapse; text-align:center; margin-left:0;'>");

            // Header row
            html.Append("<tr style='background-color:#b7dee8; font-weight:bold;'>");
            foreach (DataColumn column in dt.Columns)
            {
                html.AppendFormat("<td>{0}</td>", column.ColumnName);
            }
            html.Append("</tr>");

            // Data rows
            foreach (DataRow row in dt.Rows)
            {
                html.Append("<tr style='background-color:#fff2cc;'>");
                foreach (var item in row.ItemArray)
                {
                    html.AppendFormat("<td>{0}</td>", item);
                }
                html.Append("</tr>");
            }

            // End table
            html.Append("</table>");

            // ====== Special Note ======
            html.Append("<p style='font-weight:bold; margin-top:15px;'>Special Note:</p>");
            html.Append("<p style='font-weight:bold;'>");
            html.Append("Please Check and Insert Records in SJQDMS_WORK_DAY_SIZE and SJQDMS_WORK_DAY Table, If balance records Exists");
            html.Append("</p>");

            html.Append("</body></html>");

            return html.ToString();
        }



        public List<string> SendAutoScheduleDataAsimage(string fileName, string msg, string htmldata)
        {

            List<string> responseMessages = new List<string>();


            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";
            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "120363402385372191@g.us" },//120363122655008537@g.us(AEQS_Working_Condition)//120363347683285873@g.us(test)//120363402385372191@g.us(APC MES Team)
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
                        FMSLOG.Platform(responseData, "Auto_Schedule");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "Auto_Schedule");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "Auto_Schedule");
            }

            return responseMessages;

        }


        #endregion

        #region PO Count Report(Stitching)
        public List<string> Plant_PO_Count_Stitching(SrvInfo vsrvinfo)
        {
            var response = new List<string>();
            DateTime today = DateTime.Now;

            string startDateStr = today.ToString("yyyy/MM/dd");
            string endDateStr = today.ToString("yyyy/MM/dd");

            string fileName = $@"PlantWisePOCOUNT_{DateTime.Now:yyyyMMdd_HHmmss}";
            string message = $@"Dear All,
Kindly find attached the Stitching PO Count for {startDateStr}.";

            string body = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <title>Plant Wise PO Count</title>

    <style>
       .container {
    width: max-content;     /* Table width = content width */
    margin: 0 auto;         /* Center the whole block */
}

.headerRow {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 100%;            /* Match table width */
}

.headerTitle {
    font-size: 22px;
    font-weight: bold;
    padding: 10px 0;
}

.headerDate {
    font-size: 16px;
    font-weight: bold;
    padding: 10px 0;
}

th {
    background-color: #65b2ff;
    color: black;
    text-align: center;
    padding: 10px;
}

td {
    text-align: center;
    padding: 8px;
    border: 1px solid #ddd;
}

.grayRow td {
    background-color: #dbd3d3 !important;
    color: black !important;
    font-weight: bold;
}

    </style>
</head>
<body>";

            // GET DATA
            var employees = Get_PO_Count_Stitching(startDateStr, endDateStr);

            if (employees == null || !employees.Any())
            {
                response.Add($"No data available for the specified date of {startDateStr}");
                return response;
            }

            // HEADER LIKE SECOND IMAGE
            body += $@"
<div class=container>
<div class='headerRow'>
<div class='headerTitle'>Stitching NO. of running PO's list of APC</div>
<div class='headerDate'>Date: {DateTime.Now:dd-MM-yyyy}</div>
</div>
<br/>";

            // TABLE START
            body += "<table border='1'>";

            // HEADERS
            body += "<tr><th>Line</th><th>AP1</th><th>AP2</th><th>AP3</th><th>AP6</th><th>AP7</th><th>AP8</th><th>AP9</th><th>AP10</th><th>AP12</th><th>Total</th></tr>";

            // NORMAL LINES (L01–L11)
            foreach (var emp in employees.Where(e =>
                    e.Line != "Total_Running_PO_Count" &&
                    e.Line != "Running_Lines_Count" &&
                    e.Line != "Standard_PO_Count"))
            {
                body += "<tr>";
                body += $"<td>{emp.Line}</td>";

                foreach (var plant in new[] { emp.AP1, emp.AP2, emp.AP3,/* emp.AP5,*/ emp.AP6, emp.AP7, emp.AP8, emp.AP9, emp.AP10,/* emp.AP11,*/ emp.AP12 })
                {
                    int count = int.TryParse(plant, out var result) ? result : 0;

                    string color = count > 8 ? "red" :
                                   count >= 1 && count <= 8 ? "green" : "white";

                    string textColor = color == "white" ? "black" : "white";

                    body += $"<td style='background-color:{color}; color:{textColor};'>{count}</td>";
                }

                body += "</tr>";
            }

            // SUMMARY ROW OBJECTS
            var totalRunning = employees.First(x => x.Line == "Total_Running_PO_Count");
            var runningLines = employees.First(x => x.Line == "Running_Lines_Count");
            var standardPO = employees.First(x => x.Line == "Standard_PO_Count");

            // ---------------- COLOR LOGIC ROW: Total Running PO Count ----------------
            body += "<tr><td style='font-weight:bold;'>Total_Running_PO_Count</td>";

            var runningArr = new[] { totalRunning.AP1, totalRunning.AP2, totalRunning.AP3, /*totalRunning.AP5,*/ totalRunning.AP6, totalRunning.AP7, totalRunning.AP8, totalRunning.AP9, totalRunning.AP10, /*totalRunning.AP11,*/ totalRunning.AP12, totalRunning.Total };
            var standardArr = new[] { standardPO.AP1, standardPO.AP2, standardPO.AP3, /*standardPO.AP5, */standardPO.AP6, standardPO.AP7, standardPO.AP8, standardPO.AP9, standardPO.AP10,/* standardPO.AP11,*/ standardPO.AP12, standardPO.Total };

            for (int i = 0; i < runningArr.Length; i++)
            {
                int r = int.TryParse(runningArr[i], out var rr) ? rr : 0;
                int s = int.TryParse(standardArr[i], out var ss) ? ss : 0;

                string bg = r > s ? "red" : "green";
                string textColor = "white";

                body += $"<td style='background-color:{bg}; color:{textColor}; font-weight:bold;'>{r}</td>";
            }

            body += "</tr>";
            // ---------------- GRAY ROW: Standard PO Count ----------------
            body += "<tr class='grayRow'><td>Standard_PO_Count</td>";
            foreach (var val in new[] { standardPO.AP1, standardPO.AP2, standardPO.AP3, /*standardPO.AP5,*/ standardPO.AP6, standardPO.AP7, standardPO.AP8, standardPO.AP9, standardPO.AP10, /*standardPO.AP11,*/ standardPO.AP12, standardPO.Total })
            {
                body += $"<td>{val}</td>";
            }
            body += "</tr>";
            // ---------------- GRAY ROW: Running Lines Count ----------------
            body += "<tr class='grayRow'><td>Running_Lines_Count</td>";
            foreach (var val in new[] { runningLines.AP1, runningLines.AP2, runningLines.AP3, /*runningLines.AP5,*/ runningLines.AP6, runningLines.AP7, runningLines.AP8, runningLines.AP9, runningLines.AP10, /*runningLines.AP11,*/ runningLines.AP12, runningLines.Total })
            {
                body += $"<td>{val}</td>";
            }
            body += "</tr>";
            body += "</table></div></body></html>";

            response = SendStitchingPOReportAsimage(fileName, message, body);
            return response;
        }

        public List<string> SendStitchingPOReportAsimage(string fileName, string msg, string htmldata)
        {

            List<string> responseMessages = new List<string>();


            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";


            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "919989502631-1597025029@g.us" },//919490996631-1606387197@g.us//120363347683285873@g.us//919989502631-1597025029@g.us(Apache India Team)
                textMsg = msg,
                htmL_Code = htmldata,
                fileName = fileName
            };

            //var payload = new
            //{
            //    tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
            //    numbers = new[] { "9640416084","7095216564" }, // Use the fetched phone number
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
                        FMSLOG.Platform(responseData, "PO_Count");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "PO_Count");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "PO_Count");
            }

            return responseMessages;

        }

        public List<Stitching_plants> Get_PO_Count_Stitching(string fromdate, string todate)
        {

            List<Stitching_plants> employees = new List<Stitching_plants>();
            OracleConnection con = new OracleConnection(mes_con);
            try
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("GET_PLANT_PO_COUNT_BY_DATE_UPDATED_STIT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("p_start_date", OracleDbType.Varchar2).Value = fromdate;
                cmd.Parameters.Add("p_end_date", OracleDbType.Varchar2).Value = todate;
                cmd.Parameters.Add("cur_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;


                OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    employees.Add(new Stitching_plants
                    {
                        Line = reader["LINE"].ToString(),
                        AP1 = reader["AP1"].ToString(),
                        AP2 = reader["AP2"].ToString(),
                        AP3 = reader["AP3"].ToString(),
                        //  AP5 = reader["AP5"].ToString(),
                        AP6 = reader["AP6"].ToString(),
                        AP7 = reader["AP7"].ToString(),
                        AP8 = reader["AP8"].ToString(),
                        AP9 = reader["AP9"].ToString(),
                        AP10 = reader["AP10"].ToString(),
                        // AP11 = reader["AP11"].ToString(),
                        AP12 = reader["AP12"].ToString(),
                        Total = reader["Total"].ToString()
                        //MSP = reader["MSP"].ToString()
                    });
                }

                reader.Close();
            }
            catch (Exception Ex)
            {

                Ex.Message.ToList();
            }
            finally
            {
                con.Close();
            }
            return employees;
        }


        public class Stitching_plants
        {
            public string Line { get; set; }
            public string AP1 { get; set; }
            public string AP2 { get; set; }
            public string AP3 { get; set; }
            // public string AP5 { get; set; }
            public string AP6 { get; set; }
            public string AP7 { get; set; }
            public string AP8 { get; set; }
            public string AP9 { get; set; }
            public string AP10 { get; set; }
            // public string AP11 { get; set; }
            public string AP12 { get; set; }
            public string Total { get; set; }
            // public string MSP { get; set; }
        }
        #endregion

        #region PO Count Report(Assembly)
        public List<string> Plant_PO_Count_Assembly(SrvInfo vsrvinfo)
        {
            var response = new List<string>();
            DateTime today = DateTime.Now;

            string startDateStr = today.ToString("yyyy/MM/dd");
            string endDateStr = today.ToString("yyyy/MM/dd");

            string fileName = $@"PlantWiseAssemblyPOCOUNT_{DateTime.Now:yyyyMMdd_HHmmss}";
            string message = $@"Dear All,
Kindly find attached the Assembly PO Count for {startDateStr}.";

            string body = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <title>Plant Wise PO Count</title>

    <style>
       .container {
    width: max-content;     /* Table width = content width */
    margin: 0 auto;         /* Center the whole block */
}

.headerRow {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 100%;            /* Match table width */
}

.headerTitle {
    font-size: 22px;
    font-weight: bold;
    padding: 10px 0;
}

.headerDate {
    font-size: 16px;
    font-weight: bold;
    padding: 10px 0;
}

th {
    background-color: #65b2ff;
    color: black;
    text-align: center;
    padding: 10px;
}

td {
    text-align: center;
    padding: 8px;
    border: 1px solid #ddd;
}

.grayRow td {
    background-color: #dbd3d3 !important;
    color: black !important;
    font-weight: bold;
}

    </style>
</head>
<body>";

            // GET DATA
            var employees = Get_PO_Count_Assembly(startDateStr, endDateStr);

            if (employees == null || !employees.Any())
            {
                response.Add($"No data available for the specified date of {startDateStr}");
                return response;
            }

            // HEADER LIKE SECOND IMAGE
            body += $@"
<div class=container>
<div class='headerRow'>
<div class='headerTitle'>Assembly NO. of running PO's list of APC</div>
<div class='headerDate'>Date: {DateTime.Now:dd-MM-yyyy}</div>
</div>
<br/>";

            // TABLE START
            body += "<table border='1'>";

            // HEADERS
            body += "<tr><th>Line</th><th>AP1</th><th>AP2</th><th>AP3</th><th>AP5</th><th>AP6</th><th>AP7</th><th>AP8</th><th>AP9</th><th>AP11</th><th>Total</th></tr>";

            // NORMAL LINES (L01–L11)
            foreach (var emp in employees.Where(e =>
                    e.Line != "Total_Running_PO_Count" &&
                    e.Line != "Running_Lines_Count" &&
                    e.Line != "Standard_PO_Count"))
            {
                body += "<tr>";
                body += $"<td>{emp.Line}</td>";

                #region Old logic before Spl logic for AP1
                //foreach (var plant in new[] { emp.AP1, emp.AP2, emp.AP3, emp.AP5, emp.AP6, emp.AP7, emp.AP8, emp.AP9, /*emp.AP10,*/ emp.AP11, /*emp.AP12*/ })
                //{
                //    int count = int.TryParse(plant, out var result) ? result : 0;

                //    string color = count > 5 ? "red" :
                //                   count >= 1 && count <= 5 ? "green" : "white";

                //    string textColor = color == "white" ? "black" : "white";

                //    body += $"<td style='background-color:{color}; color:{textColor};'>{count}</td>";
                //}
                #endregion

                var plants = new[]
 {
    new { Value = emp.AP1, Name = "AP1" },
    new { Value = emp.AP2, Name = "AP2" },
    new { Value = emp.AP3, Name = "AP3" },
    new { Value = emp.AP5, Name = "AP5" },
    new { Value = emp.AP6, Name = "AP6" },
    new { Value = emp.AP7, Name = "AP7" },
    new { Value = emp.AP8, Name = "AP8" },
    new { Value = emp.AP9, Name = "AP9" },
    new { Value = emp.AP11, Name = "AP11" }
};

                foreach (var plant in plants)
                {
                    int count = int.TryParse(plant.Value, out var result) ? result : 0;

                    string color;


                    if (plant.Name == "AP1" &&
                        (emp.Line == "AL01" || emp.Line == "AL03" || emp.Line == "AL05" || emp.Line == "AL06"))
                    {
                        // Special rule
                        color = count > 10 ? "red" :
                                count >= 1 && count <= 10 ? "green" : "white";
                    }
                    else
                    {
                        // Default rule
                        color = count > 5 ? "red" :
                                count >= 1 && count <= 5 ? "green" : "white";
                    }

                    string textColor = color == "white" ? "black" : "white";

                    body += $"<td style='background-color:{color}; color:{textColor};'>{count}</td>";
                }
                body += "</tr>";
            }

            // SUMMARY ROW OBJECTS
            var totalRunning = employees.First(x => x.Line == "Total_Running_PO_Count");
            var runningLines = employees.First(x => x.Line == "Running_Lines_Count");
            var standardPO = employees.First(x => x.Line == "Standard_PO_Count");

            // ---------------- COLOR LOGIC ROW: Total Running PO Count ----------------
            body += "<tr><td style='font-weight:bold;'>Total_Running_PO_Count</td>";

            var runningArr = new[] { totalRunning.AP1, totalRunning.AP2, totalRunning.AP3, totalRunning.AP5, totalRunning.AP6, totalRunning.AP7, totalRunning.AP8, totalRunning.AP9, /*totalRunning.AP10,*/ totalRunning.AP11, /*totalRunning.AP12,*/ totalRunning.Total };
            var standardArr = new[] { standardPO.AP1, standardPO.AP2, standardPO.AP3, standardPO.AP5, standardPO.AP6, standardPO.AP7, standardPO.AP8, standardPO.AP9, /*standardPO.AP10,*/ standardPO.AP11, /*standardPO.AP12,*/ standardPO.Total };

            for (int i = 0; i < runningArr.Length; i++)
            {
                int r = int.TryParse(runningArr[i], out var rr) ? rr : 0;
                int s = int.TryParse(standardArr[i], out var ss) ? ss : 0;

                string bg = r > s ? "red" : "green";
                string textColor = "white";

                body += $"<td style='background-color:{bg}; color:{textColor}; font-weight:bold;'>{r}</td>";
            }

            body += "</tr>";
            // ---------------- GRAY ROW: Standard PO Count ----------------
            body += "<tr class='grayRow'><td>Standard_PO_Count</td>";
            foreach (var val in new[] { standardPO.AP1, standardPO.AP2, standardPO.AP3, standardPO.AP5, standardPO.AP6, standardPO.AP7, standardPO.AP8, standardPO.AP9, /*standardPO.AP10,*/ standardPO.AP11, /*standardPO.AP12,*/ standardPO.Total })
            {
                body += $"<td>{val}</td>";
            }
            body += "</tr>";
            // ---------------- GRAY ROW: Running Lines Count ----------------
            body += "<tr class='grayRow'><td>Running_Lines_Count</td>";
            foreach (var val in new[] { runningLines.AP1, runningLines.AP2, runningLines.AP3, runningLines.AP5, runningLines.AP6, runningLines.AP7, runningLines.AP8, runningLines.AP9,/* runningLines.AP10,*/ runningLines.AP11, /*runningLines.AP12,*/ runningLines.Total })
            {
                body += $"<td>{val}</td>";
            }
            body += "</tr>";
            body += "</table></div></body></html>";

            response = SendAssemblyPOReportAsimage(fileName, message, body);
            return response;
        }

        public List<string> SendAssemblyPOReportAsimage(string fileName, string msg, string htmldata)
        {

            List<string> responseMessages = new List<string>();


            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";


            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "919989502631-1597025029@g.us" },//919490996631-1606387197@g.us//120363347683285873@g.us//919989502631-1597025029@g.us(Apache India Team)
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
                        FMSLOG.Platform(responseData, "PO_Count");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "PO_Count");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "PO_Count");
            }

            return responseMessages;

        }


        public class Assembly_plants
        {
            public string Line { get; set; }
            public string AP1 { get; set; }
            public string AP2 { get; set; }
            public string AP3 { get; set; }

            public string AP5 { get; set; }
            public string AP6 { get; set; }
            public string AP7 { get; set; }
            public string AP8 { get; set; }
            public string AP9 { get; set; }
            // public string AP10 { get; set; }
            public string AP11 { get; set; }
            // public string AP12 { get; set; }
            public string Total { get; set; }
            // public string MSP { get; set; }
        }

        public List<Assembly_plants> Get_PO_Count_Assembly(string fromdate, string todate)
        {

            List<Assembly_plants> employees = new List<Assembly_plants>();
            OracleConnection con = new OracleConnection(mes_con);
            try
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("GET_PLANT_PO_COUNT_BY_DATE_UPDATED", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("p_start_date", OracleDbType.Varchar2).Value = fromdate;
                cmd.Parameters.Add("p_end_date", OracleDbType.Varchar2).Value = todate;
                cmd.Parameters.Add("cur_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;


                OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    employees.Add(new Assembly_plants
                    {
                        Line = reader["LINE"].ToString(),
                        AP1 = reader["AP1"].ToString(),
                        AP2 = reader["AP2"].ToString(),
                        AP3 = reader["AP3"].ToString(),
                        AP5 = reader["AP5"].ToString(),
                        AP6 = reader["AP6"].ToString(),
                        AP7 = reader["AP7"].ToString(),
                        AP8 = reader["AP8"].ToString(),
                        AP9 = reader["AP9"].ToString(),
                        // AP10 = reader["AP10"].ToString(),
                        AP11 = reader["AP11"].ToString(),
                        //  AP12 = reader["AP12"].ToString(),
                        Total = reader["Total"].ToString()
                        //MSP = reader["MSP"].ToString()
                    });
                }

                reader.Close();
            }
            catch (Exception Ex)
            {

                Ex.Message.ToList();
            }
            finally
            {
                con.Close();
            }
            return employees;
        }
        #endregion

        #region Supplementary Report
        public async Task<Cls_Return> Get_SupplementaryData_From_ClientAPIAsync(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            OracleTransaction transaction = null;

            try
            {
                OracleConnection con = new OracleConnection(mes_con);
                con.Open();
                DateTime startOfMonth = DateTime.Today.AddDays(1 - DateTime.Today.Day);
                DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                HashSet<DateTime> holidayDates = new HashSet<DateTime>();

                using (OracleCommand cmd = new OracleCommand(@"
    SELECT CALENDAR
    FROM DA_CALENDAR_S@APCHRDB
    WHERE ORG_ID = 100
      AND TO_CHAR(CALENDAR, 'yyyy') = TO_CHAR(SYSDATE, 'yyyy')
      AND CALENDAR BETWEEN :startdate AND :enddate", con))
                {
                    cmd.Parameters.Add("startdate", OracleDbType.Date).Value = startOfMonth;
                    cmd.Parameters.Add("enddate", OracleDbType.Date).Value = endOfMonth;

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["CALENDAR"] != DBNull.Value)
                                holidayDates.Add(Convert.ToDateTime(reader["CALENDAR"]).Date); // Ensure date-only comparison
                        }
                    }
                }



                DateTime Today = DateTime.Now.Date;

                if (!holidayDates.Contains(Today))
                {

                    string datesql = $@"SELECT Last_Working_Date, ROW_NUMBER() OVER(ORDER BY Last_Working_Date) rn
  FROM (SELECT to_char(dt, 'yyyy-MM-dd') Last_Working_Date
          FROM (SELECT TRUNC(SYSDATE) - LEVEL AS dt
                  FROM dual
                CONNECT BY LEVEL <= 10)
         WHERE TO_CHAR(dt, 'DY', 'NLS_DATE_LANGUAGE=ENGLISH') <> 'SUN'
           AND dt NOT IN
               (SELECT calendar
                  FROM DA_CALENDAR_S@APCHRDB
                 WHERE org_id = 100
                   AND TO_CHAR(calendar, 'YYYY') = TO_CHAR(SYSDATE, 'YYYY'))
         ORDER BY dt DESC)
 WHERE ROWNUM = 1";

                    OracleDataAdapter da = new OracleDataAdapter(datesql, con);
                    DataTable dt1 = new DataTable();
                    da.Fill(dt1);
                    con.Close();
                    string apiDate = dt1.Rows[0]["Last_Working_Date"].ToString();
                    //DateTime selectedDate = DateTime.Now.AddDays(-5);
                    //apiDate = selectedDate.ToString("yyyy-MM-dd");

                    string fileName = $@"Supplementary_Report_Upto_{DateTime.Now:yyyyMMdd_HHmmss}";
                    string msg = $@"Dear All, Please check the Supplementary Report Upto {apiDate} in above Image";



                    #region Test URL
                    //string url = $"http://10.2.171.134:1080/webroot/decision/sp/client/api/data/api?Date={apiDate}";

                    //string clientId = "3855ef7509aa460b9afa4089c4a2a7b3";
                    //string clientSecret = "17ea74b138d84169b684ea973c71e22e";
                    #endregion

                    #region Production URL
                    string url = $"http://10.2.171.132:8080/webroot/decision/sp/client/api/data/api?Date={apiDate}";

                    string clientId = "b0ca0fdd7f784b818568e2a23423acf4";
                    string clientSecret = "1fe9df8a1f3b4e688744105714246dbc";
                    #endregion
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("client_id", clientId);
                        client.DefaultRequestHeaders.Add("secret", clientSecret);

                        client.DefaultRequestHeaders.Accept.Add(
                            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response = await client.GetAsync(url);

                        if (!response.IsSuccessStatusCode)
                        {
                            rt.TYPE = "E";
                            rt.MESSAGE = $"API Error : {response.StatusCode} - {response.ReasonPhrase}";
                            return rt;
                        }

                        string responseData = await response.Content.ReadAsStringAsync();


                        JObject jsonObject = JObject.Parse(responseData);


                        JArray dataArray = (JArray)jsonObject["data"];


                        DataTable dt = JsonConvert.DeserializeObject<DataTable>(dataArray.ToString());

                        var sorted = dt.AsEnumerable()
      .OrderBy(r => r["Company"].ToString())
      .ThenBy(r =>
      {
          string team = r["Team"].ToString();

          // 1️⃣ AP Teams first (AP-1, AP-2...)
          if (team.StartsWith("AP-"))
              return 1;

          // 2️⃣ MK Plant next
          if (team.Contains("MK-Plant"))
              return 2;

          // 3️⃣ APEX production next
          if (team.Contains("APEX"))
              return 3;

          // 4️⃣ Others
          return 4;
      })
      .ThenBy(r =>
      {
          string team = r["Team"].ToString();

          // Extract number only for AP teams
          var match = System.Text.RegularExpressions.Regex.Match(team, @"\d+");
          return match.Success ? int.Parse(match.Value) : int.MaxValue;
      })
      .CopyToDataTable();


                        if (sorted.Rows.Count > 0)
                        {
                            string htmlTable = ConvertDataTableToHtml(sorted, apiDate);
                            SendSupplementaryReportAsImage(fileName, msg, htmlTable);
                        }


                        rt.TYPE = "S";
                        rt.MESSAGE = $"Data fetched and stored successfully for {apiDate}";
                    }
                }
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                rt.TYPE = "E";
                rt.MESSAGE = ex.Message;
            }
            finally
            {
                if (conoa != null && conoa.State == ConnectionState.Open)
                    conoa.Close();

                conoa?.Dispose();
            }

            return rt;
        }

        //        public static string ConvertDataTableToHtml(DataTable dt,string apiDate)
        //        {
        //            if (dt == null || dt.Rows.Count == 0)
        //                return "<p>No data available.</p>";

        //            StringBuilder html = new StringBuilder();

        //            html.Append(@"
        //<table border='1' cellpadding='5' cellspacing='0'
        //       style='border-collapse:collapse;
        //              font-family:Arial;
        //              font-size:12px;
        //              width:100%;
        //              text-align:center;'>

        //<tr style='background-color:#ffff00;font-weight:bold;'>
        //    <td colspan='10' style='padding:6px;'>
        //        <table width='100%' style='border-collapse:collapse;'>
        //            <tr>
        //                <td style='text-align:center;font-size:16px;font-weight:bold;'>
        //                    APC Material Replacement Tracking Report<br/>
        //                    <span style='font-size:12px;'>APC 物料更換追蹤報告</span>
        //                </td>
        //                <td style='text-align:right;font-size:14px;white-space:nowrap;font-weight:bold;'>
        //                    Date 日期 : " + apiDate + @"
        //                </td>
        //            </tr>
        //        </table>
        //    </td>
        //</tr>

        //<!-- Header Row 1 -->
        //<tr style='background-color:#9dc3e6;font-weight:bold;'>
        //    <th rowspan='2'>Sl No<br/><span style='font-size:10px;'>序號</span></th>
        //    <th rowspan='2'>Company<br/><span style='font-size:10px;'>公司</span></th>
        //    <th rowspan='2'>Team<br/><span style='font-size:10px;'>團隊</span></th>
        //    <th colspan='3'>Yesterday Data " + apiDate + @"<br/><span style='font-size:10px;'>昨日數據</span></th>
        //    <th colspan='3'>Accumulate Data<br/><span style='font-size:10px;'>收集數據</span></th>
        //    <th rowspan='2' style='background-color:#00b050;color:white;'>
        //        Standard Target Rs<br/>
        //        <span style='font-size:10px;'>標準目標價（盧比）</span>
        //    </th>
        //</tr>

        //<!-- Header Row 2 -->
        //<tr style='background-color:#bdd7ee;font-weight:bold;'>
        //    <th>Money<br/><span style='font-size:10px;'>錢</span></th>
        //    <th>Out Put<br/><span style='font-size:10px;'>輸出</span></th>
        //    <th>Pair Cost RS<br/><span style='font-size:10px;'>配對成本</span></th>
        //    <th>Accumulate Money<br/><span style='font-size:10px;'>累積資金</span></th>
        //    <th>Accumulate Out Put<br/><span style='font-size:10px;'>累積輸出</span></th>
        //    <th>Accumulate Pair Cost RS<br/><span style='font-size:10px;'>累積配對成本</span></th>
        //</tr>
        //");


        //            int siNo = 1;

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                decimal money = row["Money"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Money"]);
        //                decimal output = row["Out_put"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Out_put"]);
        //                decimal pairCost = output == 0 ? 0 : Math.Round(money / output, 2);

        //                html.Append("<tr>");

        //                html.AppendFormat("<td>{0}</td>", siNo++);
        //                html.AppendFormat("<td>{0}</td>", row["Company"]);
        //                html.AppendFormat("<td>{0}</td>", row["Team"]);

        //                html.AppendFormat("<td>{0:N2}</td>", money);
        //                html.AppendFormat("<td>{0}</td>", output);
        //                html.AppendFormat("<td>{0:N2}</td>", pairCost);

        //                html.AppendFormat("<td>{0:N2}</td>", row["Accumulate_Money"]);
        //                html.AppendFormat("<td>{0}</td>", row["Accumulate_Out_put"]);
        //                html.AppendFormat("<td>{0:N2}</td>", row["Accumulate_pair_cost_RS"]);

        //                html.AppendFormat("<td>{0:N2}</td>", row["Standard_Target_Rs"]);

        //                html.Append("</tr>");
        //            }

        //            html.Append("</table>");

        //            return html.ToString();
        //        }

        public static string ConvertDataTableToHtml(DataTable dt, string apiDate)
        {
            if (dt == null || dt.Rows.Count == 0)
                return "<p>No data available.</p>";


            StringBuilder html = new StringBuilder();

            html.Append(@"
<table border='1' cellpadding='5' cellspacing='0'
       style='border-collapse:collapse;
              font-family:Arial;
              font-size:12px;
              width:100%;
              text-align:center;'>

<tr style='background-color:#ffff00;font-weight:bold;'>
    <td colspan='11' style='padding:6px;'>
        <table width='100%' style='border-collapse:collapse;'>
            <tr>
                <td style='text-align:center;font-size:16px;font-weight:bold;'>
                    APC Material Replacement Tracking Report<br/>
                    <span style='font-size:12px;'>APC 物料更換追蹤報告</span>
                </td>
                <td style='text-align:right;font-size:14px;white-space:nowrap;font-weight:bold;'>
                    Date 日期 : " + apiDate + @"
                </td>
            </tr>
        </table>
    </td>
</tr>

<!-- Header Row 1 -->

<tr style='background-color:#9dc3e6;font-weight:bold;'>
    <th rowspan='2'>Sl No<br/><span style='font-size:10px;'>序號</span></th>
    <th rowspan='2'>Company<br/><span style='font-size:10px;'>公司</span></th>
    <th rowspan='2'>Team<br/><span style='font-size:10px;'>團隊</span></th>
    <th colspan='3'>Yesterday Data " + apiDate + @"<br/><span style='font-size:10px;'>昨日數據</span></th>
    <th colspan='3'>Accumulate Data<br/><span style='font-size:10px;'>收集數據</span></th>


<th rowspan='2' style='background-color:#00b050;color:white;'>
    Standard Target Rs<br/>
    <span style='font-size:10px;'>標準目標價（盧比）</span>
</th>

<th rowspan='2' style='background-color:#00b050;color:white;'>
    Now R/F approval standard targets<br/>
    <span style='font-size:10px;'>目前批准標準目標</span>
</th>


</tr>

<!-- Header Row 2 -->

<tr style='background-color:#bdd7ee;font-weight:bold;'>
    <th>Money<br/><span style='font-size:10px;'>錢</span></th>
    <th>Out Put<br/><span style='font-size:10px;'>輸出</span></th>
    <th>Pair Cost RS<br/><span style='font-size:10px;'>配對成本</span></th>
    <th>Accumulate Money<br/><span style='font-size:10px;'>累積資金</span></th>
    <th>Accumulate Out Put<br/><span style='font-size:10px;'>累積輸出</span></th>
    <th>Accumulate Pair Cost RS<br/><span style='font-size:10px;'>累積配對成本</span></th>
</tr>
");


            int siNo = 1;

            foreach (DataRow row in dt.Rows)
            {
                decimal money = row["Money"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Money"]);
                decimal output = row["Out_put"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Out_put"]);
                decimal pairCost = output == 0 ? 0 : Math.Round(money / output, 2);

                html.Append("<tr>");

                html.AppendFormat("<td>{0}</td>", siNo++);
                html.AppendFormat("<td>{0}</td>", row["Company"]);
                html.AppendFormat("<td>{0}</td>", row["Team"]);

                html.AppendFormat("<td>{0:N2}</td>", money);
                html.AppendFormat("<td>{0}</td>", output);
                html.AppendFormat("<td>{0:N2}</td>", pairCost);

                html.AppendFormat("<td>{0:N2}</td>", row["Accumulate_Money"]);
                html.AppendFormat("<td>{0}</td>", row["Accumulate_Out_put"]);
                html.AppendFormat("<td>{0:N2}</td>", row["Accumulate_pair_cost_RS"]);

                html.AppendFormat("<td>{0:N2}</td>", row["Standard_Target_Rs"]);

                // New Column (from DataTable)
                html.AppendFormat("<td>{0:N2}</td>", row["Now_R/F_approval_standard_targets"]);

                html.Append("</tr>");
            }

            html.Append("</table>");

            return html.ToString();

        }


        public List<string> SendSupplementaryReportAsImage(string fileName, string msg, string htmldata)
        {

            List<string> responseMessages = new List<string>();


            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";


            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "919989502631-1597025029@g.us" },//919490996631-1606387197@g.us//120363347683285873@g.us//919989502631-1597025029@g.us(Apache India Team)
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
                        FMSLOG.Platform(responseData, "Supplement_Data");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "Supplement_Data");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "Supplement_Data");
            }

            return responseMessages;

        }
        #endregion

        #region Unfinished POs List
        public Cls_Return Send_Unfinished_POs_List(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            try
            {
                string constroa = null;
                string filePath;
                filePath = Application.ExecutablePath;
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                string clientEnvConfigFileName = filePath + "database.config";
                XmlDocument clientEnvConfigDoc = new XmlDocument();
                DataTable dt1 = new DataTable();
                DataTable dt2 = new DataTable();
                DataTable dt3 = new DataTable();
                string fileName = $@"Unfinished_POs_List_Upto_{DateTime.Now:yyyyMMdd_HHmmss}";
                string msg = $@"Dear All, Please check Unfinished POs as of Now for PSDD in above Image";
                string h1 = $@"Unfinished POs Upto {DateTime.Now:yyyy/MM/dd} for PSDD";

                if (File.Exists(clientEnvConfigFileName))
                {
                    FileLoader obj = new FileLoader(clientEnvConfigFileName);
                    Hashtable htdblinks = obj.GetDBLinks();
                    if (htdblinks.ContainsKey(vsrvinfo.SDB))
                        constroa = htdblinks[vsrvinfo.SDB].ToString();

                    conoa = new OracleConnection(constroa);
                    conoa.Open();
                  
                    string sql1 = $@"WITH base_data AS
 (SELECT CASE
           WHEN INSTR(t.scan_detpt, '5001API') > 0 THEN
            'Plant-A'
           WHEN INSTR(t.scan_detpt, '5001APO') > 0 THEN
            'APO'
           WHEN INSTR(t.scan_detpt, '5001AP10') > 0 THEN
            'AP10'
           WHEN INSTR(t.scan_detpt, '5001AP11') > 0 THEN
            'AP11'
           WHEN INSTR(t.scan_detpt, '5001AP12') > 0 THEN
            'AP12'
           WHEN INSTR(t.scan_detpt, '5001AP1') > 0 THEN
            'AP1'
           WHEN INSTR(t.scan_detpt, '5001AP2') > 0 THEN
            'AP2'
           WHEN INSTR(t.scan_detpt, '5001AP3') > 0 THEN
            'AP3'
           WHEN INSTR(t.scan_detpt, '5001AP5') > 0 THEN
            'AP5'
           WHEN INSTR(t.scan_detpt, '5001AP6') > 0 THEN
            'AP6'
           WHEN INSTR(t.scan_detpt, '5001AP7') > 0 THEN
            'AP7'
           WHEN INSTR(t.scan_detpt, '5001AP8') > 0 THEN
            'AP8'
           WHEN INSTR(t.scan_detpt, '5001AP9') > 0 THEN
            'AP9'
           WHEN INSTR(t.scan_detpt, '5011') > 0 THEN
            'MAX KING'
           WHEN INSTR(t.scan_detpt, '5021') > 0 THEN
            'Camphor'
           WHEN INSTR(t.scan_detpt, '5013') > 0 THEN
            'MAX KING Domestic'
           WHEN INSTR(t.scan_detpt, '5041') > 0 THEN
            'APEX'
           WHEN INSTR(t.scan_detpt, '5002') > 0 THEN
            'APC outsole'
           ELSE
            'Other'
         END AS plant_name,
         
         z.*,
         ct.DESCOUNTRY_NAME_EN,
         assm.assembly_finished_date
  
    FROM (SELECT m.se_id,
                 m.mer_po,
                 m.DESCOUNTRY_NAME AS DESCOUNTRY_NAME_CN,
                 r.name_e AS model_name,
                 i.cr_reqdate,
                 i.lpd,
                 i.nst AS psdd,
                 i.nlt AS podd,
                 s.size_no,
                 s.se_qty,
                 NVL(pk.packingQty, 0) AS packingQty,
                 s.se_qty - NVL(pk.packingQty, 0) AS packingQty_balance,
                 NVL(o.inStock_qty, 0) AS inStock_qty,
                 NVL(p.shipping_qty, 0) AS shipping_qty
            FROM bdm_se_order_master m
            JOIN bdm_se_order_item i
              ON m.se_id = i.se_id
            JOIN bdm_se_order_size s
              ON m.se_id = s.se_id
            JOIN bdm_rd_prod r
              ON i.prod_no = r.prod_no
          
            LEFT JOIN (SELECT se_id, size_no, SUM(label_qty) packingQty
                        FROM sfc_trackout_list
                       WHERE process_no = 'A'
                       GROUP BY se_id, size_no) pk
              ON pk.se_id = m.se_id
             AND pk.size_no = s.size_no
          
            LEFT JOIN (SELECT se_id, size_no, SUM(qty) inStock_qty
                        FROM wms_finishedtrackin_orderlist
                       GROUP BY se_id, size_no) o
              ON o.se_id = m.se_id
             AND o.size_no = s.size_no
          
            LEFT JOIN (SELECT a.se_id,
                             c.size_no,
                             SUM(shipping_qty) shipping_qty
                        FROM bmd_se_shipment_m a
                        JOIN bmd_se_shipment_d b
                          ON a.delivery_no = b.delivery_no
                        JOIN bdm_se_order_size c
                          ON a.se_id = c.se_id
                         AND b.item_no = c.item_no
                       WHERE a.status = '7'
                       GROUP BY a.se_id, c.size_no) p
              ON p.se_id = m.se_id
             AND p.size_no = s.size_no
          
           WHERE s.se_qty > 0) z
  
    LEFT JOIN (SELECT se_id, size_no, MIN(scan_detpt) AS scan_detpt
                FROM sfc_trackin_list
               WHERE process_no = 'L'
                 AND scan_detpt LIKE '5001AP%'
               GROUP BY se_id, size_no) t
      ON t.se_id = z.se_id
     AND t.size_no = z.size_no
  
    LEFT JOIN COUNTRY_TRANSLATION ct
      ON z.DESCOUNTRY_NAME_CN = ct.DESCOUNTRY_NAME_CN
  
    LEFT JOIN (SELECT se_id,
                     size_no,
                     TRUNC(MAX(scan_date)) AS assembly_finished_date
                FROM sfc_trackout_list
               WHERE process_no = 'A'
               GROUP BY se_id, size_no) assm
      ON assm.se_id = z.se_id
     AND assm.size_no = z.size_no
  
   WHERE t.scan_detpt IS NOT NULL),

agg_data AS
 (SELECT plant_name,
         se_id,
         mer_po,
         model_name,
         cr_reqdate,
         lpd,
         psdd,
         podd,
         TRUNC(SYSDATE) - TRUNC(psdd) AS aging_days,
         SUM(se_qty) AS se_qty,
         SUM(packingQty) AS packingQty,
         SUM(inStock_qty) AS inStock_qty,
         SUM(shipping_qty) AS shipping_qty,
         MAX(assembly_finished_date) AS assembly_finished_date,
         DESCOUNTRY_NAME_EN
    FROM base_data
   GROUP BY plant_name,
            se_id,
            mer_po,
            model_name,
            cr_reqdate,
            lpd,
            psdd,
            podd,
            DESCOUNTRY_NAME_EN)

SELECT plant_name,
       se_id      AS SO,
       mer_po     AS PO,
       model_name,
       
       TO_CHAR(cr_reqdate, 'yyyy/MM/dd') AS CRD,
       TO_CHAR(psdd, 'yyyy/MM/dd') AS psdd,
       
       se_qty       AS SO_QTY,
       packingQty,
       inStock_qty  AS FG_QTY,
       shipping_qty,
       
       ROUND((packingQty / NULLIF(se_qty, 0)) * 100, 2) AS po_finish_prct,
       
       to_char(assembly_finished_date,'yyyy/MM/dd') AS packing_finished_date,
       DESCOUNTRY_NAME_EN     AS Destination_Country,
       
       CASE
         WHEN aging_days <= 7 THEN
          'Grace period'
         WHEN aging_days BETWEEN 8 AND 15 THEN
          '5%'
         WHEN aging_days BETWEEN 16 AND 30 THEN
          '20%'
         WHEN aging_days BETWEEN 31 AND 60 THEN
          '40%'
         ELSE
          'Min 60% - Max 100%'
       END AS FOB_DISCOUNT_PERCENTAGE

  FROM agg_data

 WHERE (packingQty / NULLIF(se_qty, 0)) * 100 < 100
   AND psdd < SYSDATE
   AND TRUNC(psdd, 'YYYY') = TRUNC(SYSDATE, 'YYYY')

-- ✅ FINAL SORTING (Discount first, then plant)
 ORDER BY CASE
            WHEN aging_days <= 7 THEN
             1
            WHEN aging_days BETWEEN 8 AND 15 THEN
             2
            WHEN aging_days BETWEEN 16 AND 30 THEN
             3
            WHEN aging_days BETWEEN 31 AND 60 THEN
             4
            ELSE
             5
          END,
          plant_name";
                   
                    OracleCommand cmd1 = new OracleCommand(sql1, conoa);
                    cmd1.CommandType = CommandType.Text;
                    OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
                    da1.Fill(dt1);
                    conoa.Close();
                    if (dt1.Rows.Count > 0)
                    {
                        string HTMLData = ConvertPOCompletionToHTML(dt1,h1);
                        Send_PO_Completion_ReportAsimage(fileName, msg, HTMLData);
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

        public static string ConvertPOCompletionToHTML(DataTable dt1, string title)
        {
            StringBuilder html = new StringBuilder();

            html.Append(@"
    <div style='
        background-color:#f4f6f8;
        padding:20px;
        font-family:Segoe UI, Arial;
    '>");

            // ===== Title Bar =====
            html.Append($@"
    <div style='
        background-color:#2f5d8a;
        color:white;
        padding:12px;
        text-align:center;
        font-size:16px;
        font-weight:600;
        border-radius:6px;
        margin-bottom:15px;
    '>
        {title}
    </div>");

            if (dt1 == null || dt1.Rows.Count == 0)
            {
                html.Append("<p style='text-align:center;color:#999;'>No Data Available</p>");
                html.Append("</div>");
                return html.ToString();
            }

            // ===== Table Start =====
            html.Append(@"
    <table style='
        width:100%;
        border-collapse:collapse;
        font-size:13px;
        background:white;
        border:1px solid #d0d7de;
    '>");

            // ===== Header =====
            html.Append(@"
    <thead>
        <tr style='background-color:#3c6e9e; color:white;'>");

            foreach (DataColumn col in dt1.Columns)
            {
                html.Append($@"
        <th style='
            padding:8px;
            border:1px solid #d0d7de;
            text-align:center;
            font-weight:600;
        '>
            {col.ColumnName.Replace("_", " ")}
        </th>");
            }

            html.Append("</tr></thead>");

            // ===== Body =====
            html.Append("<tbody>");

            int rowIndex = 0;

            foreach (DataRow row in dt1.Rows)
            {
                string bgColor = (rowIndex++ % 2 == 0) ? "#ffffff" : "#f2f5f9";

                html.Append($"<tr style='background-color:{bgColor};'>");

                foreach (DataColumn col in dt1.Columns)
                {
                    string value = row[col]?.ToString() ?? "";

                    string textColor = "#333";
                    string cellBgColor = "";

                    if (col.ColumnName.Equals("FOB_DISCOUNT_PERCENTAGE", StringComparison.OrdinalIgnoreCase))
                    {
                        string val = value.Trim().ToUpper();

                        if (val.Contains("GRACE"))
                            cellBgColor = "#d4edda"; // light green
                        else if (val.Contains("5"))
                            cellBgColor = "#fff3cd"; // pale yellow
                        else if (val.Contains("20"))
                            cellBgColor = "#ffeb3b"; // bright yellow
                        else if (val.Contains("40"))
                            cellBgColor = "#ffa500"; // orange
                        else if (val.Contains("60"))
                            cellBgColor = "#f8d7da"; // pale red
                    }

                    // Highlight AQL_RESULT column like image
                    if (col.ColumnName.Equals("AQL_RESULT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (value.ToUpper().Contains("NOT"))
                        {
                            textColor = "#c58a00"; // orange highlight
                            value = $"<b>{value}</b>";
                        }
                    }

                    html.Append($@"
<td style='
    padding:7px;
    border:1px solid #e0e0e0;
    text-align:center;
    color:{textColor};
    background-color:{(string.IsNullOrEmpty(cellBgColor) ? "transparent" : cellBgColor)};
'>
    {value}
</td>");

                }

                html.Append("</tr>");
            }

            html.Append("</tbody></table>");

            #region Colour Code
            html.Append("<div style='margin-top:15px; font-weight:600;'>PSDD Delay Days</div>");

            html.Append(@"
<table style='
    width:100%;
    table-layout:fixed;
    border-collapse:collapse;
    margin-top:5px;
    font-size:13px;
    text-align:center;
'>
    <tr>

        <td style='width:20%; padding:10px; background-color:#d4edda; border:1px solid #ccc; font-weight:600;'>
            Grace Period (0-7 Days)
        </td>

        <td style='width:20%; padding:10px; background-color:#fff3cd; border:1px solid #ccc; font-weight:600;'>
            5% (8-15 Days)
        </td>

        <td style='width:20%; padding:10px; background-color:#ffeb3b; border:1px solid #ccc; font-weight:600;'>
            20%(16-30 Days)
        </td>

        <td style='width:20%; padding:10px; background-color:#ffa500; border:1px solid #ccc; font-weight:600;'>
           40% (31-60 Days), incl. potential cancellation without cost
        </td>

        <td style='width:20%; padding:10px; background-color:#f8d7da; border:1px solid #ccc; font-weight:600;'>
            Min 60% - Max 100% (Over 60 Days)(to be decided by adidas), incl. potential cancellation without cost
        </td>

    </tr>
</table>");

            html.Append("</tr></table>");

            #endregion
            html.Append("</div>");

            return html.ToString();
        }

        public List<string> Send_PO_Completion_ReportAsimage(string fileName, string msg, string htmldata)
        {

            List<string> responseMessages = new List<string>();


            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";
            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "919989502631-1597025029@g.us" },//120363122655008537@g.us(AEQS_Working_Condition)//120363347683285873@g.us(test)//919989502631-1597025029@g.us(Apache India Team)
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
                        FMSLOG.Platform(responseData, "BGrade_Data");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "BGrade_Data");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "BGrade_Data");
            }

            return responseMessages;

        }
        #endregion
    }
}
