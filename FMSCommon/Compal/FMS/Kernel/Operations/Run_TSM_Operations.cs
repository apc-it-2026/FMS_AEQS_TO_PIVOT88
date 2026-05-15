using Compal.FMS.Connections.DBLoader;
using Compal.FMS.Kernel.Beans;
using Compal.FMS.Kernel.Operations;
using FMSCommon.Compal.FMS.Kernel.Utils;
using Newtonsoft.Json;
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
    class Run_TSM_Operations
    {
        #region TSM Registration Status Update
        public Cls_Return Update_TSM_Registration_Status(SrvInfo vsrvinfo)
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

                        string sql_tsm_registration_status = $@"update t_tsm_emp_registration
   set status = 'Completed'
 where training_e_date <= to_char(sysdate, 'yyyy/MM/dd')
   and status is null";



                        OracleCommand cmd1 = new OracleCommand(sql_tsm_registration_status, conoa);

                        cmd1.CommandType = CommandType.Text;

                        cmd1.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "TSM_Registration");
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
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
                conoa.Close();
                conoa.Dispose();
                GC.Collect();
            }
        }
        #endregion

        #region Advance Absent Report
        public Cls_Return Send_Employee_Absent_Report(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            OracleConnection conoa = null;
            OracleTransaction transaction = null;
            string where1 = string.Empty;
            string where2 = string.Empty;
            string where3 = string.Empty;
            string where4 = string.Empty;
            string where5 = string.Empty;
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
                        string str = "Cutting,Stitching,Assembly";
                        string Process_type=string.Empty;
                        DataTable dt = new DataTable();
                        DataTable dt2 = new DataTable();
                        foreach (string value in str.Split(','))
                        {
                            Process_type = value.Trim();
                            string get_nxt_work_day = $@"SELECT MIN(next_day) AS next_working_day
  FROM (SELECT TRUNC(SYSDATE) + LEVEL AS next_day
          FROM dual
        CONNECT BY LEVEL <= 366)
 WHERE next_day NOT IN
       (SELECT calendar
          FROM DA_CALENDAR_S@APCHRDB
         WHERE org_id = '100'
           AND TO_CHAR(calendar, 'YYYY') = TO_CHAR(SYSDATE, 'YYYY'))";
                            OracleCommand cmd = new OracleCommand(get_nxt_work_day, conoa);
                            object result = cmd.ExecuteScalar();
                            string nextWorkingDay = result == DBNull.Value ? "" : Convert.ToDateTime(result).ToString("yyyy/MM/dd");
                            string fileName = $@"{Process_type}_Absent_Report{DateTime.Now:yyyyMMdd_HHmmss}";
                            string msg = $@"Please Check the {Process_type}_Absent_Report_of_{nextWorkingDay} in above Image";

                            string sql_get_process = $@"select distinct WORKING_SKILL
  from t_tsm_prod_adjustment a
 inner join t_tsm_processlist b
    on a.working_skill = b.name
 where to_char(a.prod_date,'yyyy/MM/dd') = '{nextWorkingDay}'
   and b.process_type = '{Process_type}'";
                            OracleCommand cmd1 = new OracleCommand(sql_get_process, conoa);
                            OracleDataAdapter da = new OracleDataAdapter(cmd1);
                            da.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow item in dt.Rows)
                                {
                                    where1 += "'" + item["WORKING_SKILL"].ToString().Replace(" ", "") + "'" + "AS " + item["WORKING_SKILL"].ToString().Replace(" ", "") + ",";
                                }
                                where1 = where1.TrimEnd(',');
                                foreach (DataRow item in dt.Rows)
                                {
                                    where2 += item["WORKING_SKILL"].ToString().Replace(" ", "") + ",";
                                }
                                foreach (DataRow item in dt.Rows)
                                {
                                    where3 += item["WORKING_SKILL"].ToString().Replace(" ", "") + "+";
                                }
                                where3 = where3.TrimEnd('+');
                                foreach (DataRow item in dt.Rows)
                                {
                                    where4 += "SUM(" + item["WORKING_SKILL"].ToString().Replace(" ", "") + ")" + ",";
                                }
                                foreach (DataRow item in dt.Rows)
                                {
                                    where5 += item["WORKING_SKILL"].ToString().Replace(" ", "") + "+";
                                }
                                where5 = where5.TrimEnd('+');
                                where5 = "SUM(" + where5 + ")";

                                string sql_get_report = $@"WITH pivot_data AS
 (SELECT *
    FROM (SELECT PLANT, WORKING_SKILL, 1 AS CNT
            FROM t_tsm_prod_adjustment a
 inner join t_tsm_processlist b
    on a.working_skill = b.name
           WHERE to_char(prod_date,'yyyy/MM/dd') = '{nextWorkingDay}' and b.process_type = '{Process_type}')
  PIVOT(COUNT(CNT)
     FOR WORKING_SKILL IN({where1}))),

row_data AS
 (SELECT plant,
         {where2}
         ({where3}) AS Plant_Total
    FROM pivot_data
  
  UNION ALL
  
  SELECT 'TOTAL',
         {where4}
         {where5}
    FROM pivot_data)

SELECT *
  FROM row_data
 ORDER BY CASE
            WHEN plant = 'TOTAL' THEN
             1
            ELSE
             0
          END,
          plant";
                                OracleCommand cmd2 = new OracleCommand(sql_get_report, conoa);
                                OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                                da2.Fill(dt2);
                                string htmlString = CreateSupportHtml(dt2, Process_type, nextWorkingDay);
                                SendAbsentReportAsimage(fileName, msg, htmlString);
                                dt.Clear();
                                dt2.Clear();
                                dt2.Columns.Clear();
                                where1 = string.Empty;
                                where2 = string.Empty;
                                where3 = string.Empty;
                                where4 = string.Empty;
                                where5 = string.Empty;
                                htmlString = string.Empty;
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
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
                conoa.Close();
                conoa.Dispose();
                GC.Collect();
            }
        }

        public static string CreateSupportHtml(DataTable dt, string processType,string nextWorkingDay)
        {
            //string currentDate = DateTime.Now.ToString("dd/MM/yyyy");

            string html = $@"
<style>
    table {{
        border-collapse: collapse;
        width: 100%;
        font-family: Arial, sans-serif;
        font-size: 15px;
        text-align: center;
    }}

    th, td {{
        border: 1px solid #000;
        padding: 8px;
    }}

    .title {{
        background-color: #FFE600;   /* Yellow */
        font-weight: bold;
        font-size: 20px;
        text-align: center;
        padding: 10px;
        border: 1px solid #000;
    }}

    .header-row th {{
        background-color: #00AEEF;   /* Blue */
        color: #fff;
        font-weight: bold;
    }}

    .total-row td {{
        background-color: #FFE600;    /* Yellow */
        font-weight: bold;
    }}
</style>

<div class='title'>
    {nextWorkingDay} MPAC {processType} Required Data
</div>

<table>
    <tr class='header-row'>";

            // Add table headers
            foreach (DataColumn col in dt.Columns)
                html += $"<th>{col.ColumnName.Replace("_", " ")}</th>";

            html += "</tr>";

            // Add rows
            foreach (DataRow row in dt.Rows)
            {
                bool isTotal = row[0].ToString().Trim().ToUpper() == "TOTAL";

                html += isTotal ? "<tr class='total-row'>" : "<tr>";

                foreach (var item in row.ItemArray)
                    html += $"<td>{item}</td>";

                html += "</tr>";
            }

            html += "</table>";

            return html;
        }



        public List<string> SendAbsentReportAsimage(string fileName, string msg, string htmldata)
        {

            List<string> responseMessages = new List<string>();


            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";
            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "120363425336019363@g.us" },//120363425336019363@g.us(MPAC WhatsApp Alerts)//120363347683285873@g.us(test)
                textMsg = msg,
                htmL_Code = htmldata,
                fileName = fileName
            };

            //var payload = new
            //{
            //    tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
            //    numbers = new[] { "9640416084","8886321672" }, // Use the fetched phone number
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
                        FMSLOG.Platform(responseData, "Advance_Absent_Report");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "Advance_Absent_Report");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "Advance_Absent_Report");
            }

            return responseMessages;

        }
        #endregion

        #region Excess Employee Report
        public Cls_Return Send_Employee_Excess_Report(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            OracleConnection conoa = null;
            OracleTransaction transaction = null;
            string where1 = string.Empty;
            string where2 = string.Empty;
            string where3 = string.Empty;
            string where4 = string.Empty;
            string where5 = string.Empty;
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
                        string str = "Cutting,Stitching,Assembly";
                        string Process_type = string.Empty;
                        DataTable dt = new DataTable();
                        DataTable dt2 = new DataTable();
                        foreach (string value in str.Split(','))
                        {
                            Process_type = value.Trim();
                            string WorkingDay = DateTime.Now.ToString("yyyy/MM/dd");
                            string fileName = $@"{Process_type}_Excess_Report{DateTime.Now:yyyyMMdd_HHmmss}";
                            string msg = $@"Please Check the {Process_type}_Excess_Report_of_{WorkingDay} in above Image";

                            string sql_get_process = $@"select distinct WORKING_SKILL
  from t_tsm_excess_employee a
 inner join t_tsm_processlist b
    on a.working_skill = b.name
 where to_char(a.prod_date,'yyyy/MM/dd') = '{WorkingDay}'
   and b.process_type = '{Process_type}'";
                            OracleCommand cmd1 = new OracleCommand(sql_get_process, conoa);
                            OracleDataAdapter da = new OracleDataAdapter(cmd1);
                            da.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow item in dt.Rows)
                                {
                                    where1 += "'" + item["WORKING_SKILL"].ToString().Replace(" ", "") + "'" + "AS " + item["WORKING_SKILL"].ToString().Replace(" ", "") + ",";
                                }
                                where1 = where1.TrimEnd(',');
                                foreach (DataRow item in dt.Rows)
                                {
                                    where2 += item["WORKING_SKILL"].ToString().Replace(" ", "") + ",";
                                }
                                foreach (DataRow item in dt.Rows)
                                {
                                    where3 += item["WORKING_SKILL"].ToString().Replace(" ", "") + "+";
                                }
                                where3 = where3.TrimEnd('+');
                                foreach (DataRow item in dt.Rows)
                                {
                                    where4 += "SUM(" + item["WORKING_SKILL"].ToString().Replace(" ", "") + ")" + ",";
                                }
                                foreach (DataRow item in dt.Rows)
                                {
                                    where5 += item["WORKING_SKILL"].ToString().Replace(" ", "") + "+";
                                }
                                where5 = where5.TrimEnd('+');
                                where5 = "SUM(" + where5 + ")";

                                string sql_get_report = $@"WITH pivot_data AS
 (SELECT *
    FROM (SELECT PLANT, WORKING_SKILL, 1 AS CNT
            FROM t_tsm_excess_employee a
 inner join t_tsm_processlist b
    on a.working_skill = b.name
           WHERE to_char(prod_date,'yyyy/MM/dd') = '{WorkingDay}' and b.process_type = '{Process_type}')
  PIVOT(COUNT(CNT)
     FOR WORKING_SKILL IN({where1}))),

row_data AS
 (SELECT plant,
         {where2}
         ({where3}) AS Plant_Total
    FROM pivot_data
  
  UNION ALL
  
  SELECT 'TOTAL',
         {where4}
         {where5}
    FROM pivot_data)

SELECT *
  FROM row_data
 ORDER BY CASE
            WHEN plant = 'TOTAL' THEN
             1
            ELSE
             0
          END,
          plant";
                                OracleCommand cmd2 = new OracleCommand(sql_get_report, conoa);
                                OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                                da2.Fill(dt2);
                                string htmlString = CreateExcessReportHtml(dt2, Process_type);
                                SendExcessReportAsimage(fileName, msg, htmlString);
                                dt.Clear();
                                dt2.Clear();
                                dt2.Columns.Clear();
                                where1 = string.Empty;
                                where2 = string.Empty;
                                where3 = string.Empty;
                                where4 = string.Empty;
                                where5 = string.Empty;
                                htmlString = string.Empty;
                            }
                            
                        }


                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
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
                conoa.Close();
                conoa.Dispose();
                GC.Collect();
            }
        }
        public static string CreateExcessReportHtml(DataTable dt, string processType)
        {
            string currentDate = DateTime.Now.ToString("dd/MM/yyyy");

            string html = $@"
<style>
    table {{
        border-collapse: collapse;
        width: 100%;
        font-family: Arial, sans-serif;
        font-size: 15px;
        text-align: center;
    }}

    th, td {{
        border: 1px solid #000;
        padding: 8px;
    }}

    .title {{
        background-color: #FFE600;   /* Yellow */
        font-weight: bold;
        font-size: 20px;
        text-align: center;
        padding: 10px;
        border: 1px solid #000;
    }}

    .header-row th {{
        background-color: #00AEEF;   /* Blue */
        color: #fff;
        font-weight: bold;
    }}

    .total-row td {{
        background-color: #FFE600;    /* Yellow */
        font-weight: bold;
    }}
</style>

<div class='title'>
    {currentDate} {processType} Excess Employee Data
</div>

<table>
    <tr class='header-row'>";

            // Add table headers
            foreach (DataColumn col in dt.Columns)
                html += $"<th>{col.ColumnName.Replace("_", " ")}</th>";

            html += "</tr>";

            // Add rows
            foreach (DataRow row in dt.Rows)
            {
                bool isTotal = row[0].ToString().Trim().ToUpper() == "TOTAL";

                html += isTotal ? "<tr class='total-row'>" : "<tr>";

                foreach (var item in row.ItemArray)
                    html += $"<td>{item}</td>";

                html += "</tr>";
            }

            html += "</table>";

            return html;
        }



        public List<string> SendExcessReportAsimage(string fileName, string msg, string htmldata)
        {

            List<string> responseMessages = new List<string>();


            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";
            //var payload = new
            //{
            //    tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
            //    numbers = new List<string>(), // Use the fetched phone number
            //    groups = new[] { "919989502631-1597025029@g.us" },//120363122655008537@g.us(AEQS_Working_Condition)//120363347683285873@g.us(test)//919989502631-1597025029@g.us(Apache India Team)
            //    textMsg = msg,
            //    htmL_Code = htmldata,
            //    fileName = fileName
            //};

            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new[] { "9640416084" }, // Use the fetched phone number , "8886321672"
                groups = new List<string>(),
                textMsg = msg,
                htmL_Code = htmldata,
                fileName = fileName
            };


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
                        FMSLOG.Platform(responseData, "Excess_Report");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "Excess_Report");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "Excess_Report");
            }

            return responseMessages;

        }
        #endregion

        #region Daily Attendance Report
        public Cls_Return Send_Daily_Attendance_Report(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            OracleConnection conoa = null;
            OracleTransaction transaction = null;
            try
            {
                string constroa = null;
                string filePath;
                string fileName = $@"Daily_Attendance_Report_of_{DateTime.Now:yyyyMMdd_HHmmss}";
                string msg = $@"Dear All, Please check the Daily_Attendance_Report of {DateTime.Now:yyyyMMdd} in above Image";
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
                        DataTable dt = new DataTable();


                        #region Query Based on Priority Process
//                        string sql_get_report = $@"WITH attd AS
// (SELECT q1.UDF05 AS PLANT,
//         COUNT(e.EMP_NO) AS EMP_COUNT,
//         COUNT(ci.EMP_NO) AS ATTD_COUNT,
//         COUNT(ci.EMP_NO) - COUNT(e.EMP_NO) AS ABSENT_COUNT,
//         ROUND(COUNT(ci.EMP_NO) / COUNT(e.EMP_NO) * 100, 2) ATTD_PERCENT
//    FROM (SELECT CHILD_DEPT HR_DEPT, PARENT_DEPT UDF05
//            FROM t_daily_attandance_dept) q1
//    JOIN EP_MAIN@Apchrdb e
//      ON e.DEPT_NO = q1.HR_DEPT
//     AND e.WORK_NO in ('G01', 'OP1')
//     AND e.STATUS = 1
//     AND e.ORG_ID = 100
//    LEFT JOIN (SELECT DISTINCT EMP_NO
//                FROM CA_ICDATA@APCHRDB
//               WHERE CARD_DATE = TRUNC(SYSDATE)) ci
//      ON ci.EMP_NO = e.EMP_NO
//   GROUP BY q1.UDF05),

//tmp AS
// (SELECT a.emp_no, a.dept_no
//    FROM t_oa_empmain a
//   WHERE a.dept_no IN (SELECT CHILD_DEPT FROM t_daily_attandance_dept)
//     AND a.emp_no NOT IN (SELECT DISTINCT emp_no
//                            FROM CA_ICDATA@APCHRDB
//                           WHERE card_date = TRUNC(SYSDATE))
//     AND a.work_no = 'G01'),

//tmp2 AS
// (SELECT a.emp_no, a.dept_no, c.process_type, d.parent_dept
//    FROM tmp a
//    JOIN t_tsm_process_priority b
//      ON a.emp_no = b.emp_no
//     AND b.priority = '1'
//    JOIN t_tsm_processlist c
//      ON b.working_skill = c.name
//    JOIN t_daily_attandance_dept d
//      ON a.dept_no = d.child_dept
//   WHERE c.skill_type = 'Key_Skill'),

//skill_absent AS
// (SELECT parent_dept,
//         SUM(CASE
//               WHEN process_type = 'Cutting' THEN
//                1
//               ELSE
//                0
//             END) AS Cutting,
//         SUM(CASE
//               WHEN process_type = 'Stitching' THEN
//                1
//               ELSE
//                0
//             END) AS Stitching,
//         SUM(CASE
//               WHEN process_type = 'Assembly' THEN
//                1
//               ELSE
//                0
//             END) AS Assembly
//    FROM tmp2
//   GROUP BY parent_dept),

//final_data AS
// (SELECT a.PLANT,
//         a.EMP_COUNT,
//         a.ATTD_COUNT,
//         a.ABSENT_COUNT,
//         a.ATTD_PERCENT,
//         NVL(s.Cutting, 0) Cutting,
//         NVL(s.Stitching, 0) Stitching,
//         NVL(s.Assembly, 0) Assembly,
//         NVL(s.Cutting, 0) + NVL(s.Stitching, 0) + NVL(s.Assembly, 0) Total
//    FROM attd a
//    LEFT JOIN skill_absent s
//      ON a.PLANT = s.parent_dept order by plant)
      
//      SELECT *
//FROM
//(
//    SELECT *
//    FROM final_data

//    UNION ALL

//    SELECT 'TOTAL',
//           SUM(EMP_COUNT),
//           SUM(ATTD_COUNT),
//           SUM(ABSENT_COUNT),
//           ROUND(SUM(ATTD_COUNT)/SUM(EMP_COUNT)*100,2),
//           SUM(Cutting),
//           SUM(Stitching),
//           SUM(Assembly),
//           SUM(Total)
//    FROM final_data
//)
// ORDER BY CASE
//            WHEN PLANT = 'TOTAL' THEN
//             1
//            ELSE
//             0
//          END,
//          CASE
//            WHEN REGEXP_LIKE(PLANT, '^AP[0-9]+$') THEN
//             TO_NUMBER(REGEXP_SUBSTR(PLANT, '[0-9]+'))
//            ELSE
//             999
//          END,
//          PLANT";

                        #endregion

                        #region Query Based on Working Process
                        string sql_get_report = $@"WITH attd AS
 (SELECT q1.UDF05 AS PLANT,
         COUNT(e.EMP_NO) AS EMP_COUNT,
         COUNT(ci.EMP_NO) AS ATTD_COUNT,
         COUNT(ci.EMP_NO) - COUNT(e.EMP_NO) AS ABSENT_COUNT,
         ROUND(COUNT(ci.EMP_NO) / COUNT(e.EMP_NO) * 100, 2) ATTD_PERCENT
    FROM (SELECT CHILD_DEPT HR_DEPT, PARENT_DEPT UDF05
            FROM t_daily_attandance_dept) q1
    JOIN EP_MAIN@Apchrdb e
      ON e.DEPT_NO = q1.HR_DEPT
     AND e.WORK_NO in ('G01', 'OP1')
     AND e.STATUS = 1
     AND e.ORG_ID = 100
    LEFT JOIN (SELECT DISTINCT EMP_NO
                FROM CA_ICDATA@APCHRDB
               WHERE CARD_DATE = TRUNC(SYSDATE)) ci
      ON ci.EMP_NO = e.EMP_NO
   GROUP BY q1.UDF05),

tmp AS
 (SELECT a.emp_no, a.dept_no
    FROM t_oa_empmain a
   WHERE a.dept_no IN (SELECT CHILD_DEPT FROM t_daily_attandance_dept)
     AND a.emp_no NOT IN (SELECT DISTINCT emp_no
                            FROM CA_ICDATA@APCHRDB
                           WHERE card_date = TRUNC(SYSDATE))
     AND a.work_no = 'G01'),

tmp2 AS
 (SELECT a.emp_no, a.dept_no, c.process_type, d.parent_dept
    FROM tmp a
    JOIN t_tsm_emp_working_skill b
      ON a.emp_no = b.emp_no
    JOIN t_tsm_processlist c
      ON b.working_skill = c.name
    JOIN t_daily_attandance_dept d
      ON a.dept_no = d.child_dept
   WHERE c.skill_type = 'Key_Skill'),

skill_absent AS
 (SELECT parent_dept,
         SUM(CASE
               WHEN process_type = 'Cutting' THEN
                1
               ELSE
                0
             END) AS Cutting,
         SUM(CASE
               WHEN process_type = 'Stitching' THEN
                1
               ELSE
                0
             END) AS Stitching,
         SUM(CASE
               WHEN process_type = 'Assembly' THEN
                1
               ELSE
                0
             END) AS Assembly
    FROM tmp2
   GROUP BY parent_dept),

final_data AS
 (SELECT a.PLANT,
         a.EMP_COUNT,
         a.ATTD_COUNT,
         a.ABSENT_COUNT,
         a.ATTD_PERCENT,
         NVL(s.Cutting, 0) Cutting,
         NVL(s.Stitching, 0) Stitching,
         NVL(s.Assembly, 0) Assembly,
         NVL(s.Cutting, 0) + NVL(s.Stitching, 0) + NVL(s.Assembly, 0) Total
    FROM attd a
    LEFT JOIN skill_absent s
      ON a.PLANT = s.parent_dept
   order by plant)

SELECT *
  FROM (SELECT *
          FROM final_data
        
        UNION ALL
        
        SELECT 'TOTAL',
               SUM(EMP_COUNT),
               SUM(ATTD_COUNT),
               SUM(ABSENT_COUNT),
               ROUND(SUM(ATTD_COUNT) / SUM(EMP_COUNT) * 100, 2),
               SUM(Cutting),
               SUM(Stitching),
               SUM(Assembly),
               SUM(Total)
          FROM final_data)
 ORDER BY CASE
            WHEN PLANT = 'TOTAL' THEN
             1
            ELSE
             0
          END,
          CASE
            WHEN REGEXP_LIKE(PLANT, '^AP[0-9]+$') THEN
             TO_NUMBER(REGEXP_SUBSTR(PLANT, '[0-9]+'))
            ELSE
             999
          END,
          PLANT";

                        #endregion
                        OracleCommand cmd = new OracleCommand(sql_get_report, conoa);
                                OracleDataAdapter da = new OracleDataAdapter(cmd);
                                da.Fill(dt);
                        if(dt.Rows.Count>0)
                        {
                            string htmlString = CreateDaily_AttendanceHtml(dt);
                            SendDaily_AttendanceReportAsimage(fileName, msg, htmlString);
                        }
                                

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
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
                conoa.Close();
                conoa.Dispose();
                GC.Collect();
            }
        }


        //        public static string CreateDaily_AttendanceHtml(DataTable dt)
        //        {
        //            string html = $@"
        //<style>
        //    table {{
        //        border-collapse: collapse;
        //        width: 100%;
        //        font-family: Arial, sans-serif;
        //        font-size: 14px;
        //        text-align: center;
        //    }}
        //    th, td {{
        //        border: 1px solid #000;
        //        padding: 6px;
        //        font-weight: bold;
        //    }}
        //    .title {{
        //        background-color: #F6D7C3;
        //        font-size: 20px;
        //        font-weight: bold;
        //    }}
        //    .header th {{
        //        background-color: #DDE8C3;
        //        font-weight: bold;
        //    }}
        //    .percent {{
        //        color: red;
        //        font-weight: bold;
        //    }}
        //    .ab {{
        //        color: red;
        //        font-weight: bold;
        //    }}
        //    .sub {{
        //        font-size: 11px;
        //    }}
        //</style>

        //<table>
        //<tr>
        //    <th colspan='9' class='title'>
        //        APC員工出勤記錄 (Daily Attendance Report) &nbsp;&nbsp; Date: {DateTime.Now.ToString("yyyy/MM/dd")}
        //    </th>
        //</tr>

        //<tr class='header'>
        //    <th>Plant</th>
        //    <th>Workers Eligible<br/><span class='sub'>應到人數</span></th>
        //    <th>Actual Attendance<br/><span class='sub'>實到人數</span></th>
        //    <th>Absent of the day<br/><span class='sub'>缺勤</span></th>
        //    <th>Actual %</th>
        //    <th>Cutting Skill Emp<br/>AB report</th>
        //    <th>Stitching Skill Emp<br/>AB report</th>
        //    <th>Assembly Skill Emp<br/>AB report</th>
        //    <th>Total Skill Employees<br/>AB Report</th>
        //</tr>";
        //            foreach (DataRow row in dt.Rows)
        //            {
        //                int emp = Convert.ToInt32(row["EMP_COUNT"]);
        //                int attd = Convert.ToInt32(row["ATTD_COUNT"]);
        //                int absent = attd - emp;
        //                decimal percent = emp == 0 ? 0 : Math.Round((decimal)attd * 100 / emp, 1);

        //                int cutting = Convert.ToInt32(row["CUTTING"]);
        //                int stitching = Convert.ToInt32(row["STITCHING"]);
        //                int assembly = Convert.ToInt32(row["ASSEMBLY"]);
        //                int totalSkill = Convert.ToInt32(row["TOTAL"]);

        //                html += $@"
        //<tr>
        //    <td><b>{row["PLANT"]}</b></td>
        //    <td>{emp}</td>
        //    <td>{attd}</td>
        //    <td>{absent}</td>
        //    <td class='percent'>{percent}</td>
        //    <td class='ab'>{(cutting == 0 ? "" : cutting.ToString())}</td>
        //    <td class='ab'>{(stitching == 0 ? "" : stitching.ToString())}</td>
        //    <td class='ab'>{(assembly == 0 ? "" : assembly.ToString())}</td>
        //    <td class='ab'>{(totalSkill == 0 ? "" : totalSkill.ToString())}</td>
        //</tr>";
        //            }

        //            html += "</table>";
        //            return html;
        //        }


        public static string CreateDaily_AttendanceHtml(DataTable dt)
        {
            string html = $@"
<style>
table {{
    border-collapse: collapse;
    width: 100%;
    font-family: Arial;
    font-size: 13px;
    table-layout: fixed; 
}}

th, td {{
    border: 1px solid black;
    padding: 5px;
    font-weight: bold;
}}

.title {{
    background-color:#F6D7C3;
    font-size:20px;
    font-weight:bold;
    text-align:center;
}}

.header th {{
    background-color:#DDE8C3;
    text-align:center;
}}

.plant {{
    text-align:left;
    font-weight:bold;
}}

.num {{
    text-align:center;
}}

.percent {{
    color:red;
    font-weight:bold;
    text-align:center;
}}

.ab {{
    color:red;
    font-weight:bold;
    text-align:center;
}}

.totalrow {{
    background-color:yellow;
    font-weight:bold;
}}

.colPlant {{width:160px}}
.col1 {{width:120px}}
.col2 {{width:120px}}
.col3 {{width:120px}}
.col4 {{width:90px}}
.col5 {{width:120px}}
.col6 {{width:120px}}
.col7 {{width:120px}}
.col8 {{width:140px}}

.sub {{
    font-size:11px;
}}
</style>

<table>

<tr>
<th colspan='9' class='title'>
APC員工出勤記錄 (Daily Attendance Report) &nbsp;&nbsp; Date: {DateTime.Now:dd-MM-yyyy}
</th>
</tr>

<tr class='header'>
<th class='colPlant'>Plant</th>
<th class='col1'>Workers Eligible<br>Employees</th>
<th class='col2'>Actual Attendance</th>
<th class='col3'>Absent of the day</th>
<th class='col4'>Actual %</th>
<th class='col5'>Cutting Skill Emp<br>AB report</th>
<th class='col6'>Stitching Skill Emp<br>AB report</th>
<th class='col7'>Assembly Skill Emp<br>AB report</th>
<th class='col8'>Total Skill Employees<br>AB Report</th>
</tr>
";

            foreach (DataRow row in dt.Rows)
            {
                int emp = Convert.ToInt32(row["EMP_COUNT"]);
                int attd = Convert.ToInt32(row["ATTD_COUNT"]);
                int absent = attd - emp;
                decimal percent = emp == 0 ? 0 : Math.Round((decimal)attd * 100 / emp, 1);

                int cutting = Convert.ToInt32(row["CUTTING"]);
                int stitching = Convert.ToInt32(row["STITCHING"]);
                int assembly = Convert.ToInt32(row["ASSEMBLY"]);
                int totalSkill = Convert.ToInt32(row["TOTAL"]);

                string plant = row["PLANT"].ToString();

                string rowClass = plant.Contains("Total") || plant.Contains("TOTAL") ? "totalrow" : "";

                html += $@"
<tr class='{rowClass}'>
<td class='plant'>{plant}</td>
<td class='num'>{emp}</td>
<td class='num'>{attd}</td>
<td class='num'>{absent}</td>
<td class='percent'>{percent}</td>
<td class='ab'>{(cutting == 0 ? "" : cutting.ToString())}</td>
<td class='ab'>{(stitching == 0 ? "" : stitching.ToString())}</td>
<td class='ab'>{(assembly == 0 ? "" : assembly.ToString())}</td>
<td class='ab'>{(totalSkill == 0 ? "" : totalSkill.ToString())}</td>
</tr>";
            }

            html += "</table>";
            return html;
        }

        public List<string> SendDaily_AttendanceReportAsimage(string fileName, string msg, string htmldata)
        {

            List<string> responseMessages = new List<string>();


            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";

            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "120363425336019363@g.us" },//120363122655008537@g.us(AEQS_Working_Condition)//120363347683285873@g.us(test)//919989502631-1597025029@g.us(Apache India Team)
                textMsg = msg,
                htmL_Code = htmldata,
                fileName = fileName
            };

            //var payload = new
            //{
            //    tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
            //    numbers = new[] { "9640416084" }, // Use the fetched phone number  ,"8074509268", "8074509268", "8886321672"
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
                        FMSLOG.Platform(responseData, "Advance_Absent_Report");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "Advance_Absent_Report");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "Advance_Absent_Report");
            }

            return responseMessages;

        }

        #endregion
    }
}
