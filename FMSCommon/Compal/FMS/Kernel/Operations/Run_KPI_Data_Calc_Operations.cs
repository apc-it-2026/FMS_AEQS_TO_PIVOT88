using AutoSendEmail;
using Compal.FMS.Connections.DBLoader;
using Compal.FMS.Kernel.Beans;
using Compal.FMS.Kernel.Operations;
using NewExportExcels;
using Newtonsoft.Json;
using OfficeOpenXml;
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
    class Run_KPI_Data_Calc_Operations
    {
        OracleConnection conoa = null;
        #region BGrade Data Download
        public async Task<Cls_Return> Get_BGrade_DataAsync(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            OracleTransaction transaction = null;

            try
            {
                string url = "http://acqy-bwapp2.apachefootwear.com:8000/sap/zcl_sap_zmm090?sap-client=800";
                string username = "APC_IT_TEAM";
                string password = "123456789";


                using (HttpClient client = new HttpClient())
                {

                    var byteArray = new System.Text.UTF8Encoding().GetBytes($"{username}:{password}");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    string constroa = null;
                    string filePath = Application.ExecutablePath;
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


                        try
                        {
                            string today = string.Empty;
                            transaction = conoa.BeginTransaction();


                            DateTime startOfMonth = DateTime.Today.AddDays(1 - DateTime.Today.Day);
                            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                            HashSet<DateTime> holidayDates = new HashSet<DateTime>();

                            using (OracleCommand cmd = new OracleCommand(@"
    SELECT CALENDAR
    FROM DA_CALENDAR_S@APCHRDB
    WHERE ORG_ID = 100
      AND TO_CHAR(CALENDAR, 'yyyy') = TO_CHAR(SYSDATE, 'yyyy')
      AND CALENDAR BETWEEN :startdate AND :enddate", conoa))
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
                            //DateTime Today = DateTime.Now.Date.AddDays(-1); 

                            if (!holidayDates.Contains(Today))
                            {

                                // for (DateTime currentDate = startdate; currentDate <= enddate; currentDate = currentDate.AddDays(1))
                                //  {
                                // DateTime Today = currentDate;
                                today = Today.ToString("yyyyMMdd");

                                string jsonPayload = $@"
            [
                {{
                    ""MATNR"": """",
                    ""MTART"": """",
                    ""EBELN"": """",
                    ""ERFME"": """",
                    ""WERKS"": ""9501,9511"",  
                    ""LGORT"": """",
                    ""CHARG"": """",
                    ""LIFNR"": """",
                    ""KUNNR"": """",
                    ""BWART"": """",
                    ""AUFNR"": """",
                    ""SOBKZ"": """",
                    ""KOSTL"": """",
                    ""SAKTO"": """",
                    ""FRBNR"": """",
                    ""MAT_KDAU"": """",
                    ""MAT_KDPO"": """",
                    ""ZKOSTL"": """",
                    ""ANLN1"": """",
                    ""BUDAT"": ""{today}""
                }}
            ]";   //Intially 9501 used as parameter for column WERKS

                                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                                HttpResponseMessage response = await client.PostAsync(url, content);
                                if (response.IsSuccessStatusCode)
                                {
                                    string responseData = await response.Content.ReadAsStringAsync();
                                    DataTable dt = SJeMES_Framework.Common.JsonHelper.GetDataTableByJson(responseData);
                                    foreach (DataRow row in dt.Rows)
                                    {

                                        if (Convert.ToDecimal(row["ERFMG"]) > 0 && !string.IsNullOrEmpty(row["WEMPF"].ToString()))
                                        {

                                            string Prod_date = row["BUDAT"].ToString();
                                            string Prod_line = row["WEMPF"].ToString().Replace("/", "");
                                            string Quantity = row["ERFMG"].ToString();
                                            string Size = row["MATNR"].ToString();
                                            string Factory = row["WERKS"].ToString();
                                            string SalesOrder = row["SGTXT1"].ToString();


                                            string sql = $@"INSERT INTO KPI_BGRADE_DATA
                                            (prod_date, prod_line, quantity,shoe_size,Factory,salesorder)
                                            VALUES
                                            (TO_DATE('{Prod_date}', 'YYYY-MM-DD'), '{Prod_line}', {Quantity},'{Size}',{Factory},'{SalesOrder}')";

                                            OracleCommand cmd = new OracleCommand(sql, conoa);
                                            cmd.CommandType = CommandType.Text;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Error {response.StatusCode}: {response.ReasonPhrase}");
                                    rt.TYPE = "E";
                                    rt.MESSAGE = "Error {response.StatusCode}: {response.ReasonPhrase";
                                    SendMessage(rt.MESSAGE);
                                    return rt;
                                }


                                transaction.Commit();
                                string msg = $@"BGrade Data  for  *{today}*  doownloaded from SAP successfully";
                                SendMessage(msg);
                            }
                        }



                        catch (Exception ex)
                        {
                            Console.WriteLine($"Request failed: {ex.Message}");
                            rt.TYPE = "E";
                            rt.MESSAGE = ex.Message;
                            SendMessage(rt.MESSAGE);
                            if (transaction != null)
                            {
                                transaction.Rollback();
                            }
                        }

                    }
                }
            }

            catch (Exception e)
            {
                rt.TYPE = "E";
                rt.MESSAGE = e.Message;
                return rt;
            }
            finally
            {
                if (conoa != null && conoa.State == System.Data.ConnectionState.Open)
                {
                    conoa.Close();
                }
                conoa?.Dispose();
                GC.Collect();
            }
            return rt;
        }
        #endregion

        #region KPI_Data_Lock
        public Cls_Return KPI_Data_Lock(SrvInfo vsrvinfo)
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

                        string sql_po_finish = $@"update kpi_po_finish
   set lock_status = 1
 where to_char(prod_date, 'yyyy/MM/dd') <=
       TO_CHAR(SYSDATE - 3, 'YYYY/MM/DD')";

                        string sql_size_label = $@"update kpi_size_label_data
   set lock_status = 1
 where to_char(prod_date, 'yyyy/MM/dd') <=
       TO_CHAR(SYSDATE - 3, 'YYYY/MM/DD')";

                        string sql_repairs = $@"update kpi_repairs_data
   set lock_status = 1
 where to_char(repair_date, 'yyyy/MM/dd') <=
       TO_CHAR(SYSDATE - 3, 'YYYY/MM/DD')";

                        string sql_repack = $@"update aql_repack_data
   set lock_status = 1
 where to_char(repack_date, 'yyyy/MM/dd') <=
       TO_CHAR(SYSDATE - 3, 'YYYY/MM/DD')";

                        string sql_haulting = $@"update kpi_haulting_data
   set lock_status = 1
 where to_char(haulting_date, 'yyyy/MM/dd') <=
       TO_CHAR(SYSDATE - 3, 'YYYY/MM/DD')";

                        string sql_replacement = $@"update kpi_replacement_interface_data
   set lock_status = 1
 where to_char(replacement_date, 'yyyy/MM/dd') <=
       TO_CHAR(SYSDATE - 3, 'YYYY/MM/DD')";

                        string sql_datalock_unlock = $@"update kpi_data_lock_unlock 
set MAXDATE=trunc(sysdate+1) , mindate=trunc(sysdate-3) , updatedby='system'";

                        OracleCommand cmd1 = new OracleCommand(sql_po_finish, conoa);
                        OracleCommand cmd2 = new OracleCommand(sql_size_label, conoa);
                        OracleCommand cmd3 = new OracleCommand(sql_repairs, conoa);
                        OracleCommand cmd4 = new OracleCommand(sql_repack, conoa);
                        OracleCommand cmd5 = new OracleCommand(sql_haulting, conoa);
                        OracleCommand cmd6 = new OracleCommand(sql_replacement, conoa);
                        OracleCommand cmd7 = new OracleCommand(sql_datalock_unlock, conoa);

                        cmd1.CommandType = CommandType.Text;
                        cmd2.CommandType = CommandType.Text;
                        cmd3.CommandType = CommandType.Text;
                        cmd4.CommandType = CommandType.Text;
                        cmd5.CommandType = CommandType.Text;
                        cmd6.CommandType = CommandType.Text;
                        cmd7.CommandType = CommandType.Text;

                        cmd1.ExecuteNonQuery();
                        cmd2.ExecuteNonQuery();
                        cmd3.ExecuteNonQuery();
                        cmd4.ExecuteNonQuery();
                        cmd5.ExecuteNonQuery();
                        cmd6.ExecuteNonQuery();
                        cmd7.ExecuteNonQuery();
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
        #endregion

        #region Daily_KPI_Calculation
        public async Task<Cls_Return> Daily_KPI_Calculation(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            OracleTransaction transaction = null;
            DateTime enddate = DateTime.Now.AddDays(-5).Date;
            DateTime enddate2 = DateTime.Now.AddDays(-3).Date;
            DateTime startdate = new DateTime(enddate.Year, enddate.Month, 1);
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


                        HashSet<DateTime> holidayDates = new HashSet<DateTime>();

                        using (OracleCommand cmd = new OracleCommand(@"
    SELECT CALENDAR
    FROM DA_CALENDAR_S@APCHRDB
    WHERE ORG_ID = 100
      AND TO_CHAR(CALENDAR, 'yyyy') = TO_CHAR(SYSDATE, 'yyyy')
      AND CALENDAR BETWEEN :startdate AND :enddate", conoa))
                        {
                            cmd.Parameters.Add("startdate", OracleDbType.Date).Value = startdate;
                            cmd.Parameters.Add("enddate", OracleDbType.Date).Value = enddate2;

                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (reader["CALENDAR"] != DBNull.Value)
                                        holidayDates.Add(Convert.ToDateTime(reader["CALENDAR"]).Date); // Ensure date-only comparison
                                }
                            }
                        }



                        if (!holidayDates.Contains(enddate2.Date))
                        {
                            SendMail_PC(enddate2, "KPI");
                        }

                        if (holidayDates.Contains(enddate.Date))
                        {
                            return rt;
                        }

                        transaction = conoa.BeginTransaction();

                        string sql = $@"select DEPARTMENT_CODE
  from base005m
 where UDF01 in ('C', 'S', 'L')
   and udf10 in ('Y')
   and udf05 not in ('MK1', 'APO', 'APEX', 'MK')
   and DEP_SAP not in ('SX') order by DEPARTMENT_CODE
";
                        OracleCommand cmd1 = new OracleCommand(sql, conoa);
                        OracleDataReader dr = cmd1.ExecuteReader();
                        while (dr.Read())
                        {
                            string line = dr["DEPARTMENT_CODE"].ToString();
                            for (DateTime currentDate = startdate; currentDate <= enddate; currentDate = currentDate.AddDays(1))
                            {
                                if (line.Contains("C"))
                                    using (OracleCommand cmd = new OracleCommand("KPI_NEW.KPI_CALC_CUTTING", conoa))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add("P_PROD_LINE", OracleDbType.Varchar2).Value = line;
                                        cmd.Parameters.Add("P_DATE_S", OracleDbType.Date).Value = startdate;
                                        cmd.Parameters.Add("P_DATE_E", OracleDbType.Date).Value = currentDate;
                                        cmd.ExecuteNonQuery();
                                    }
                                if (line.Contains("S"))
                                    using (OracleCommand cmd = new OracleCommand("KPI_NEW.KPI_CALC_STITCHING", conoa))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add("P_PROD_LINE", OracleDbType.Varchar2).Value = line;
                                        cmd.Parameters.Add("P_DATE_S", OracleDbType.Date).Value = startdate;
                                        cmd.Parameters.Add("P_DATE_E", OracleDbType.Date).Value = currentDate;
                                        cmd.ExecuteNonQuery();
                                    }
                                if (line.Contains("L"))
                                    using (OracleCommand cmd = new OracleCommand("KPI_NEW.KPI_CALC_ASSEMBLY", conoa))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add("P_PROD_LINE", OracleDbType.Varchar2).Value = line;
                                        cmd.Parameters.Add("P_DATE_S", OracleDbType.Date).Value = startdate;
                                        cmd.Parameters.Add("P_DATE_E", OracleDbType.Date).Value = currentDate;
                                        cmd.ExecuteNonQuery();
                                    }
                            }
                        }

                        transaction.Commit();
                        string date_s = startdate.ToString("yyyy/MM/dd");
                        string date_e = enddate.ToString("yyyy/MM/dd");
                        string msg = $@"Accumulated KPI from *{date_s}* to *{date_e}* calculated successfully";
                        SendMessage(msg);
                        SendMail_KPI(startdate, enddate);
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                            SendMessage(ex.ToString());
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

        public void SendMail_KPI(DateTime Start_date, DateTime End_date)
        {
            string[] listFomat = new string[0];
            string Error_msg = string.Empty;
            string Start_Date = Start_date.ToString("yyyy/MM/dd");
            string End_Date = End_date.ToString("yyyy/MM/dd");

            string msg = $@"<b>Accumulated KPI from {Start_Date} to {End_Date} calculated from MES Successfully. Please check and give your feedback.</b>";
            DataTable dt1 = new DataTable();
            string sql1 = $@"SELECT 
    book_date,
    prod_line,
    target,
    output,
    output_target_percent,
    output_target_score,
    target_po,
    finish_po,
    po_finish_percent,
    po_finish_score,
    repairs,
    b_grades,
    b_grade_percent,
    b_grade_score,
    qualified_qty,
    inspection_qty,
    rft,
    rft_score,
    repacking_qty,
    repacking_percent,
    repacking_score,
    size_label_count,
    size_label_score,
    replacement_amount,
    replacement_paircost,
    replacement_score,
    kaizen_percent,
    kaizen_score,
    haulting,
    bonding_percent,
    bonding_score,
    ie_percent,
    ie_score,
    total_score,
    updated_at,
    remarks
FROM 
    KPI_DAY_NEW
 WHERE TO_CHAR(BOOK_DATE, 'yyyy/MM/dd') = '{End_Date}'
   AND OUTPUT > 0";
            OracleCommand cmd1 = new OracleCommand(sql1, conoa);
            OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
            da1.Fill(dt1);
            if (dt1.Rows.Count > 0)
            {

                string _path = Path.Combine(Application.StartupPath, "Daily_KPI");

                if (!Directory.Exists(_path))
                {
                    Directory.CreateDirectory(_path);
                }

                string _fileName = End_date.ToString("yyyyMMdd") + ".xlsx";
                string _filePath = Path.Combine(_path, _fileName);


                DataTable dtCutting = dt1.AsEnumerable()
    .Where(row => row.Field<string>("prod_line").Contains("C"))
    .OrderBy(row => row.Field<string>("prod_line"))
    .CopyToDataTable();

                DataTable dtStitching = dt1.AsEnumerable()
    .Where(row => row.Field<string>("prod_line").Contains("S"))
    .OrderBy(row => row.Field<string>("prod_line"))
    .CopyToDataTable();

                DataTable dtAssembly = dt1.AsEnumerable()
                    .Where(row => row.Field<string>("prod_line").Contains("L"))
                    .OrderBy(row => row.Field<string>("prod_line"))
                    .CopyToDataTable();


                var sheetData = new Dictionary<string, DataTable>
{
    { "Cutting", dtCutting },
    { "Stitching", dtStitching },
    { "Assembly", dtAssembly }
};

                ExportMultipleSheets(sheetData, _filePath);

                string[] attachList = new string[] { _filePath };

                DataTable dt2 = new DataTable();
                string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='GMO'";
                //string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='TEST'";
                OracleCommand cmd2 = new OracleCommand(sql2, conoa);
                OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                da2.Fill(dt2);
                if (dt2.Rows.Count > 0)
                {
                    string To_List = dt2.Rows[0]["TO_LIST"].ToString();
                    string CC_List = dt2.Rows[0]["CC_LIST"].ToString();
                    string Error_List = dt2.Rows[0]["ERROR_LIST"].ToString();
                    string mailSubject = dt2.Rows[0]["MAIL_SUBJECT"].ToString();
                    List<string> listSend = To_List.Split(';').ToList();
                    List<string> listCopy = CC_List.Split(';').ToList();
                    List<string> listError = Error_List.Split(';').ToList();
                    if (MailUtil.SendMessage(listSend, listCopy, mailSubject, msg, attachList, out Error_msg))
                    {
                    }
                    else
                    {
                        MailUtil.SendMessage(listError, listError, mailSubject, Error_msg + "\n" + DateTime.Now, null, out Error_msg);
                    }


                }
                File.Delete(_filePath);
            }

        }

        public static void ExportMultipleSheets(Dictionary<string, DataTable> sheets, string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                foreach (var sheet in sheets)
                {
                    var worksheet = package.Workbook.Worksheets.Add(sheet.Key);
                    worksheet.Cells["A1"].LoadFromDataTable(sheet.Value, true);

                    // Highlight header
                    using (var headerRange = worksheet.Cells[1, 1, 1, sheet.Value.Columns.Count])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Format first column as Date (column A = 1)
                    int rowCount = sheet.Value.Rows.Count + 1; // +1 to include header row
                    var dateRange = worksheet.Cells[2, 1, rowCount, 1]; // Skip header row
                    dateRange.Style.Numberformat.Format = "yyyy/mm/dd"; // or "dd-MMM-yyyy" or any format you need

                    // Auto-fit all columns
                    if (worksheet.Dimension != null)
                    {
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    }
                }

                package.SaveAs(new FileInfo(filePath));
            }
        }
        #endregion

        #region Daily_IE_Calculation
        public async Task<Cls_Return> Daily_IE_Calculation(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
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
                        DateTime enddate = DateTime.Now.AddDays(-4).Date;
                       // DateTime startdate = DateTime.Now.AddDays(-6).Date;
                        DateTime startdate = new DateTime(enddate.Year, enddate.Month, 1);
                        DateTime enddate2 = DateTime.Now.AddDays(-2).Date;

                        HashSet<DateTime> holidayDates = new HashSet<DateTime>();

                        using (OracleCommand cmd = new OracleCommand(@"
    SELECT CALENDAR
    FROM DA_CALENDAR_S@APCHRDB
    WHERE ORG_ID = 100
      AND TO_CHAR(CALENDAR, 'yyyy') = TO_CHAR(SYSDATE, 'yyyy')
      AND CALENDAR BETWEEN :startdate AND :enddate", conoa))
                        {
                            cmd.Parameters.Add("startdate", OracleDbType.Date).Value = startdate;
                            cmd.Parameters.Add("enddate", OracleDbType.Date).Value = enddate2;

                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (reader["CALENDAR"] != DBNull.Value)
                                        holidayDates.Add(Convert.ToDateTime(reader["CALENDAR"]).Date); // Ensure date-only comparison
                                }
                            }
                        }


                        if (!holidayDates.Contains(enddate2.Date))
                        {
                            SendMail_PC(enddate2, "IE");
                        }

                        if (holidayDates.Contains(enddate.Date))
                        {
                            return rt;
                        }
                        transaction = conoa.BeginTransaction();


                        string sql = $@"select DEPARTMENT_CODE
  from base005m
 where UDF01 in ('C', 'S', 'L')
   and udf10 in ('Y')
   and udf05 not in ('MK1', 'APO', 'APEX', 'MK')
   and DEP_SAP not in ('SX') order by DEPARTMENT_CODE
";
                        OracleCommand cmd1 = new OracleCommand(sql, conoa);
                        OracleDataReader dr = cmd1.ExecuteReader();
                        while (dr.Read())
                        {

                            for (DateTime currentDate = startdate; currentDate <= enddate; currentDate = currentDate.AddDays(1))
                            {
                                if (holidayDates.Contains(currentDate.Date))
                                    continue; // Skip holidays
                                //if (currentDate.DayOfWeek == DayOfWeek.Sunday)
                                //    continue;

                                using (OracleCommand cmd = new OracleCommand("KPI_NEW.KPI_IE", conoa))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add("P_PROD_LINE", OracleDbType.Varchar2).Value = dr["DEPARTMENT_CODE"].ToString();
                                    cmd.Parameters.Add("P_DATE", OracleDbType.Date).Value = currentDate;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        string date = enddate.ToString("yyyy/MM/dd");
                        string msg = $@" daily IE  for  *{date}*  calculated successfully";
                        SendMessage(msg);
                        SendMail_IE(enddate, "IE");
                        SendMail_IE(enddate2, "TPPH");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                            SendMessage(ex.ToString());
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

        #region Send_KPI_DataEntry_Alerts
        public async Task<Cls_Return> Send_KPI_DataEntry_Alerts(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
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
                        DataTable dt = new DataTable();
                        using (OracleCommand cmd = new OracleCommand("KPI_ALERTS", conoa))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            OracleParameter outputCursorParam = new OracleParameter("result_cursor", OracleDbType.RefCursor);
                            outputCursorParam.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(outputCursorParam);
                            OracleDataAdapter dataAdapter = new OracleDataAdapter(cmd);
                            dataAdapter.Fill(dt);
                        }
                        transaction.Commit();
                        string msg = ConvertDataTableToHTML(dt);
                        //SendMessage(msg);
                        SendKPIAlerts(msg);
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                            SendMessage(ex.ToString());
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
        public void SendKPIAlerts(string msg)
        {
            string[] listFomat = new string[0];
            string[] attachList = new string[0];
            string Error_msg = string.Empty;
            string Msg = msg;
            DataTable dt2 = new DataTable();
            string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='KPI'";
           // string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='TEST'";
            OracleCommand cmd2 = new OracleCommand(sql2, conoa);
            OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
            da2.Fill(dt2);
            if (dt2.Rows.Count > 0)
            {
                string To_List = dt2.Rows[0]["TO_LIST"].ToString();
                string CC_List = dt2.Rows[0]["CC_LIST"].ToString();
                string Error_List = dt2.Rows[0]["ERROR_LIST"].ToString();
                string mailSubject = dt2.Rows[0]["MAIL_SUBJECT"].ToString();
                List<string> listSend = To_List.Split(';').ToList();
                List<string> listCopy = CC_List.Split(';').ToList();
                List<string> listError = Error_List.Split(';').ToList();
                if (MailUtil.SendMessage(listSend, listCopy, mailSubject, msg, attachList, out Error_msg))
                {
                }
                else
                {
                    MailUtil.SendMessage(listError, listError, mailSubject, Error_msg + "\n" + DateTime.Now, null, out Error_msg);
                }


            }

        }
        public string ConvertDataTableToHTML(DataTable dt)
        {
            StringBuilder html = new StringBuilder();

            // Start the HTML document
            html.Append("<html><body>");

            // Add greeting and dynamic message
            string currentMonth = DateTime.Now.ToString("MMMM yyyy");
            html.Append($"<p style='font-family: Times New Roman; font-size: 16px;'>Dear All,</p>");
            html.Append($"<p style='font-family: Times New Roman; font-size: 16px;'>Please check the KPI Input data status of the Month <strong>{currentMonth}</strong> in the table below:</p>");

            // Add heading
            html.Append("<h2 style='font-family: Times New Roman;'>KPI Input Data Details</h2>");

            // Start the table
            html.Append("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse; width:100%; font-family: Times New Roman;'>");

            // Add the header row
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
                            html.Append($"<td style='text-align:center; background-color:#d4edda; color:#155724;'>{number} Entries</td>");
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

            // Close the table and HTML tags
            html.Append("</table>");
            html.Append("</body></html>");

            return html.ToString();

        }
        #endregion

        #region C2B_C2S_IE_Calculation
        public async Task<Cls_Return> C2B_C2S_IE_Calculation(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
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
                        DateTime enddate = DateTime.Now.AddDays(-5).Date;
                        DateTime startdate = new DateTime(enddate.Year, enddate.Month, 1);

                        HashSet<DateTime> holidayDates = new HashSet<DateTime>();

                        using (OracleCommand cmd = new OracleCommand(@"
    SELECT CALENDAR
    FROM DA_CALENDAR_S@APCHRDB
    WHERE ORG_ID = 100
      AND TO_CHAR(CALENDAR, 'yyyy') = TO_CHAR(SYSDATE, 'yyyy')
      AND CALENDAR BETWEEN :startdate AND :enddate", conoa))
                        {
                            cmd.Parameters.Add("startdate", OracleDbType.Date).Value = startdate;
                            cmd.Parameters.Add("enddate", OracleDbType.Date).Value = enddate;

                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (reader["CALENDAR"] != DBNull.Value)
                                        holidayDates.Add(Convert.ToDateTime(reader["CALENDAR"]).Date); // Ensure date-only comparison
                                }
                            }
                        }

                        if (holidayDates.Contains(enddate.Date))
                        {
                            return rt;
                        }

                        transaction = conoa.BeginTransaction();

                        string sql = $@"select LINE_TYPE, CUTTING_LINE, STITCHING_LINE, ASSEMBLY_LINE from t_ie_lines";
                        OracleCommand cmd1 = new OracleCommand(sql, conoa);
                        OracleDataReader dr = cmd1.ExecuteReader();
                        while (dr.Read())
                        {

                            for (DateTime currentDate = startdate; currentDate <= enddate; currentDate = currentDate.AddDays(1))
                            {
                                if (holidayDates.Contains(currentDate.Date))
                                    continue; // Skip holidays
                                //if (currentDate.DayOfWeek == DayOfWeek.Sunday)

                                //    continue;

                                using (OracleCommand cmd = new OracleCommand("KPI_NEW.KPI_C2S_C2B_IE", conoa))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add("P_TYPE", OracleDbType.Varchar2).Value = dr["LINE_TYPE"].ToString();
                                    cmd.Parameters.Add("P_DATE_S", OracleDbType.Date).Value = startdate;
                                    cmd.Parameters.Add("P_DATE_E", OracleDbType.Date).Value = currentDate;
                                    cmd.Parameters.Add("P_CUT_LINE", OracleDbType.Varchar2).Value = dr["CUTTING_LINE"].ToString();
                                    cmd.Parameters.Add("P_STITCH_LINE", OracleDbType.Varchar2).Value = dr["STITCHING_LINE"].ToString();
                                    cmd.Parameters.Add("P_ASS_LINE", OracleDbType.Varchar2).Value = dr["ASSEMBLY_LINE"].ToString();
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        string date = enddate.ToString("yyyy/MM/dd");
                        string msg = $@"C2B_C2S_IE  upto  *{date}*  calculated successfully";
                        SendMessage(msg);
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                            SendMessage(ex.ToString());
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

        #region Common Methods
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
                        // Log the failure if needed
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions (logging, etc.) but do not return
                }
            }
        }

        public void SendMail_IE(DateTime End_date, string Type)
        {
            string[] listFomat = new string[0];
            string Error_msg = string.Empty;
            //DateTime startdate = new DateTime(End_date.Year, End_date.Month, 1);
            DateTime startdate = End_date.AddDays(-30);
            string Start_Date = startdate.ToString("yyyy/MM/dd");
            string End_Date = End_date.ToString("yyyy/MM/dd");

            if (Type == "IE")
            {
                string msg = $@"<b>Daily IE for {End_Date} calculated from MES Successfully. Please check and give your feedback.</b>";
                DataTable dt1 = new DataTable();
                string sql1 = $@"SELECT prod_date,
       prod_line,
       type,
       model_name,
       eolr,
       tpph,
       output,
       manpower,
       working_hours,
       downtime,
       cot,
       working_hours_f,
       ie,
       standard_manhours,
       actual_manhours,
       article,
       created_at
  FROM T_LINEWISE_DAILY_IE a
 where to_char(a.prod_date, 'yyyy/MM/dd') ='{End_Date}'
   and a.output > 0";
                OracleCommand cmd1 = new OracleCommand(sql1, conoa);
                OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
                da1.Fill(dt1);

                if (dt1.Rows.Count > 0)
                {
                    string _path = System.Windows.Forms.Application.StartupPath + "\\Daily_IE\\";

                    if (!Directory.Exists(_path))
                    {
                        Directory.CreateDirectory(_path);
                    }

                    string _fileName = End_date.ToString("yyyyMMdd");
                    string _filePath = _path + _fileName + ".xlsx";
                    ExportExcels.ExportFomat(dt1, _filePath, "sheet1", listFomat);
                    string[] attachList = new string[] { _filePath };
                    DataTable dt2 = new DataTable();
                    string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='IE' AND ROLECODE='IE001'";
                    OracleCommand cmd2 = new OracleCommand(sql2, conoa);
                    OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                    da2.Fill(dt2);
                    if (dt2.Rows.Count > 0)
                    {
                        string To_List = dt2.Rows[0]["TO_LIST"].ToString();
                        string CC_List = dt2.Rows[0]["CC_LIST"].ToString();
                        string Error_List = dt2.Rows[0]["ERROR_LIST"].ToString();
                        string mailSubject = dt2.Rows[0]["MAIL_SUBJECT"].ToString();
                        List<string> listSend = To_List.Split(';').ToList();
                        List<string> listCopy = CC_List.Split(';').ToList();
                        List<string> listError = Error_List.Split(';').ToList();
                        if (MailUtil.SendMessage(listSend, listCopy, mailSubject, msg, attachList, out Error_msg))
                        {
                        }
                        else
                        {
                            MailUtil.SendMessage(listError, listError, mailSubject, Error_msg + "\n" + DateTime.Now, null, out Error_msg);
                        }


                    }
                    File.Delete(_filePath);
                }

            }
            else if (Type == "TPPH")
            {
                string msg = $@"<b>Please Upload the TPPH values for Missing Models Mentioned in above Attachment. Thanks</b>";
                DataTable dt1 = new DataTable();
                string sql1 = $@" SELECT PROD_DATE,ARTICLE,MODEL_NAME,
       LISTAGG(DISTINCT PROD_LINE, ', ') WITHIN GROUP(ORDER BY PROD_LINE) AS PROD_LINES
  FROM T_LINEWISE_DAILY_IE A
 WHERE TO_CHAR(A.PROD_DATE, 'yyyy/MM/dd') BETWEEN '{Start_Date}' AND
       '{End_Date}'
   AND A.OUTPUT > 0
   AND TPPH = 0 AND PROD_LINE NOT LIKE '%API%'  --added this condition to hide API data
 GROUP BY PROD_DATE,ARTICLE,MODEL_NAME";
                OracleCommand cmd1 = new OracleCommand(sql1, conoa);
                OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
                da1.Fill(dt1);

                if (dt1.Rows.Count > 0)
                {
                    string _path = System.Windows.Forms.Application.StartupPath + "\\TPPH_Models\\";

                    if (!Directory.Exists(_path))
                    {
                        Directory.CreateDirectory(_path);
                    }

                    string _fileName = End_date.ToString("yyyyMMdd");
                    string _filePath = _path + _fileName + ".xlsx";
                    ExportExcels.ExportFomat(dt1, _filePath, "sheet1", listFomat);
                    string[] attachList = new string[] { _filePath };

                    DataTable dt2 = new DataTable();
                    string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='IE' AND ROLECODE='TPPH001'";
                    OracleCommand cmd2 = new OracleCommand(sql2, conoa);
                    OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                    da2.Fill(dt2);
                    if (dt2.Rows.Count > 0)
                    {
                        string To_List = dt2.Rows[0]["TO_LIST"].ToString();
                        string CC_List = dt2.Rows[0]["CC_LIST"].ToString();
                        string Error_List = dt2.Rows[0]["ERROR_LIST"].ToString();
                        string mailSubject = dt2.Rows[0]["MAIL_SUBJECT"].ToString();
                        List<string> listSend = To_List.Split(';').ToList();
                        List<string> listCopy = CC_List.Split(';').ToList();
                        List<string> listError = Error_List.Split(';').ToList();
                        if (MailUtil.SendMessage(listSend, listCopy, mailSubject, msg, attachList, out Error_msg))
                        {
                        }
                        else
                        {
                            MailUtil.SendMessage(listError, listError, mailSubject, Error_msg + "\n" + DateTime.Now, null, out Error_msg);
                        }


                    }
                    File.Delete(_filePath);
                }
            }
        }

        public void SendMail_PC(DateTime End_date, string Type)
        {
            string[] attachList = new string[0];
            string Error_msg = string.Empty;
            string End_Date = End_date.ToString("yyyy/MM/dd");
            DataTable dt = new DataTable();
            DataTable dt1 = new DataTable();
            if (Type == "IE")
            {
                string sql1 = $@"select * from ie_adjustmanpower a where a.output_date='{End_Date}'";
                OracleCommand cmd1 = new OracleCommand(sql1, conoa);
                OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
                da1.Fill(dt1);

                string sql = $@"select * from ie_adjustworkhours b where b.output_date='{End_Date}'";
                OracleCommand cmd = new OracleCommand(sql, conoa);
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                da.Fill(dt);
                if (dt1.Rows.Count > 0 && dt.Rows.Count > 0)
                {
                    return;
                }

                if (dt1.Rows.Count == 0)
                {
                    Error_msg += $@"<b>ManPower Data of {End_Date}<b>,";
                }
                if (dt.Rows.Count == 0)
                {
                    Error_msg += $@"<b>Working Hours Data of {End_Date}<b>,";
                }
                Error_msg = Error_msg.Trim(',');
                Error_msg += $@" <b>Not Uploaded Yet<b>. Please Upload by Today Workoff.";
                DataTable dt2 = new DataTable();
                string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='PC' and ROLECODE='IE'";
                OracleCommand cmd2 = new OracleCommand(sql2, conoa);
                OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                da2.Fill(dt2);
                if (dt2.Rows.Count > 0)
                {
                    string To_List = dt2.Rows[0]["TO_LIST"].ToString();
                    string CC_List = dt2.Rows[0]["CC_LIST"].ToString();
                    string Error_List = dt2.Rows[0]["ERROR_LIST"].ToString();
                    string mailSubject = dt2.Rows[0]["MAIL_SUBJECT"].ToString();
                    List<string> listSend = To_List.Split(';').ToList();
                    List<string> listCopy = CC_List.Split(';').ToList();
                    List<string> listError = Error_List.Split(';').ToList();
                    if (MailUtil.SendMessage(listSend, listCopy, mailSubject, Error_msg, attachList, out Error_msg))
                    {
                    }
                    else
                    {
                        MailUtil.SendMessage(listError, listError, mailSubject, Error_msg + "\n" + DateTime.Now, null, out Error_msg);
                    }


                }

            }
            else if (Type == "KPI")
            {
                string sql = $@"select * from kpi_po_finish where to_char(PROD_DATE,'yyyy/mm/dd')='{End_Date}'";
                OracleCommand cmd = new OracleCommand(sql, conoa);
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    return;
                }
                if (dt.Rows.Count == 0)
                {
                    Error_msg += $@"<b>PO Finish Data of {End_Date}<b>,";
                }
                Error_msg = Error_msg.Trim(',');
                Error_msg += $@" <b>Not Uploaded Yet<b>. Please Upload by Today Workoff.";
                DataTable dt2 = new DataTable();
                string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='PC' and ROLECODE='KPI'";
                OracleCommand cmd2 = new OracleCommand(sql2, conoa);
                OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                da2.Fill(dt2);
                if (dt2.Rows.Count > 0)
                {
                    string To_List = dt2.Rows[0]["TO_LIST"].ToString();
                    string CC_List = dt2.Rows[0]["CC_LIST"].ToString();
                    string Error_List = dt2.Rows[0]["ERROR_LIST"].ToString();
                    string mailSubject = dt2.Rows[0]["MAIL_SUBJECT"].ToString();
                    List<string> listSend = To_List.Split(';').ToList();
                    List<string> listCopy = CC_List.Split(';').ToList();
                    List<string> listError = Error_List.Split(';').ToList();
                    if (MailUtil.SendMessage(listSend, listCopy, mailSubject, Error_msg, attachList, out Error_msg))
                    {
                    }
                    else
                    {
                        MailUtil.SendMessage(listError, listError, mailSubject, Error_msg + "\n" + DateTime.Now, null, out Error_msg);
                    }


                }
            }

        }

        #endregion
    }
}
