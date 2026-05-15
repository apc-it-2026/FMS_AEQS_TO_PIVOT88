using AutoSendEmail;
using Compal.FMS.Connections.DBLoader;
using Compal.FMS.Kernel.Beans;
using Compal.FMS.Kernel.Operations;
using FMSCommon.Compal.FMS.Kernel.Utils;
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
    class Run_AEQS_Operations
    {

        OracleConnection conoa = null;

        // DIRECT ORACLE CONNECTION STRING (your requirement)
        private readonly string directConStr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.3.0.227)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=APCMES)));User Id=mes00;Password=dbmes00;";
        //"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.3.0.240)(PORT=1521)))" +
        //"(CONNECT_DATA=(SERVICE_NAME=MESTEST01)));User Id=mes00;Password=dbmes00;";

        #region Insert  AQL Inspection PO List
        public Cls_Return Insert_Walmart_POS_Data(SrvInfo vsrvinfo)
        {
            Cls_Return ret = new Cls_Return();
            try
            {
                conoa = new OracleConnection(directConStr);
                conoa.Open();

                // ========== SELECT WALMART POS DATA ==========
                string sqlSelect = @"
               SELECT a.se_id,
       a.mer_po,
       b.prod_no,
       c.name_s,
       b.se_qty,
       d.c_name,
       b.cr_reqdate
  FROM bdm_se_order_master a
 INNER JOIN bdm_se_order_item b
    ON a.se_id = b.se_id
 INNER JOIN bdm_rd_prod c
    ON b.prod_no = c.prod_no
 INNER JOIN bdm_country d
    ON a.descountry_code = d.c_no
   AND d.l_no = 'EN'
 WHERE b.cr_reqdate IN
       (SELECT DISTINCT a.cr_reqdate
          FROM bdm_se_order_item a
         WHERE TO_CHAR(a.cr_reqdate, 'yyyy/MM/dd') BETWEEN
               TO_CHAR(SYSDATE, 'yyyy/MM/dd') AND
               TO_CHAR(SYSDATE + 7, 'yyyy/MM/dd'))
      --AND a.descountry_code IN ('CN','IN','JP')
   AND a.status NOT IN ('99')
   AND a.se_id NOT IN (SELECT se_id FROM bmd_se_shipment_m)
                ";

                DataTable dt = new DataTable();
                using (OracleDataAdapter da = new OracleDataAdapter(sqlSelect, conoa))
                {
                    da.Fill(dt);
                }

                if (dt.Rows.Count == 0)
                {
                    ret.TYPE = "S";
                    ret.MESSAGE = "No Walmart POS data found.";
                    return ret;
                }

                // ========== LOOP AND INSERT ==========
                foreach (DataRow row in dt.Rows)
                {
                    string so = row["se_id"].ToString();
                    DateTime crd = Convert.ToDateTime(row["cr_reqdate"]);

                    // ---------- Check Duplicate ----------
                    string sqlCheck = @"
                        SELECT COUNT(*)
                          FROM AQL_INSPECTION_PO_LIST
                         WHERE SO = :SO
                           AND CRD = :CRD";

                    using (OracleCommand cmdCheck = new OracleCommand(sqlCheck, conoa))
                    {
                        cmdCheck.Parameters.Add(":SO", so);
                        cmdCheck.Parameters.Add(":CRD", crd);

                        int exists = Convert.ToInt32(cmdCheck.ExecuteScalar());

                        if (exists > 0)
                        {
                            // Send exact error → controller → frontend popup
                            //throw new Exception($"SO '{so}' with CRD '{crd:yyyy/MM/dd}' already exists!");

                            continue;
                        }
                    }

                    // ---------- Insert ----------
                    string sqlInsert = @"
    INSERT INTO AQL_INSPECTION_PO_LIST
    (SO, PO, ARTICLE, MODEL, QTY, DESTINATION, CRD)
    VALUES (:SO, :PO, :ARTICLE, :MODEL, :QTY, :DEST, :CRD)
";


                    using (OracleCommand cmdInsert = new OracleCommand(sqlInsert, conoa))
                    {
                        cmdInsert.Parameters.Add(":SO", row["se_id"].ToString());
                        cmdInsert.Parameters.Add(":PO", row["mer_po"].ToString());
                        cmdInsert.Parameters.Add(":ARTICLE", row["prod_no"].ToString());
                        cmdInsert.Parameters.Add(":MODEL", row["name_s"].ToString());
                        cmdInsert.Parameters.Add(":QTY", row["se_qty"].ToString());
                        cmdInsert.Parameters.Add(":DEST", row["c_name"].ToString());
                        cmdInsert.Parameters.Add(":CRD", Convert.ToDateTime(row["cr_reqdate"]));

                        cmdInsert.ExecuteNonQuery();
                    }

                }

                ret.TYPE = "S";
                ret.MESSAGE = "Walmart POS data inserted successfully.";
                return ret;
            }
            catch (Exception ex)
            {
                ret.TYPE = "E";
                ret.MESSAGE = ex.Message;
                return ret;
            }
            finally
            {
                if (conoa != null)
                {
                    conoa.Close();
                    conoa.Dispose();
                }
                GC.Collect();
            }
        }
        #endregion

        #region  AQL_Inspection_Report
        public Cls_Return Send_AQL_Inspection_Alert(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            OracleConnection con = null;

            try
            {
                con = new OracleConnection(directConStr);
                con.Open();
                string sql = @"select *
  from (WITH base AS (SELECT b.udf05 plant,
                             S.SE_ID,
                             SS.MER_PO,
                             SSS.PROD_NO ARTICLE,
                             D.NAME_T ART_NAME,
                             SSS.SE_QTY ORDER_QTY,
                             SUM(S.QTY) IN_QTY,
                             SS.DESCOUNTRY_CODE,
                             X.C_NAME DESCOUNTRY_NAME,
                             TO_CHAR(SSS.CR_REQDATE, 'YYYY/MM/DD') CR_REQDATE
                        FROM MMS_FINISHEDTRACKIN_LIST S
                        JOIN BDM_SE_ORDER_MASTER SS
                          ON S.SE_ID = SS.SE_ID
                        JOIN BDM_SE_ORDER_ITEM SSS
                          ON SS.SE_ID = SSS.SE_ID
                        JOIN BDM_RD_PROD D
                          ON SSS.PROD_NO = D.PROD_NO
                        JOIN BDM_COUNTRY X
                          ON SS.DESCOUNTRY_CODE = X.C_NO
                         AND X.L_NO = 'EN'
                        JOIN base005m b
                          ON s.from_line = b.dep_sap
                         AND b.udf05 != 'API'
                       WHERE SS.SE_ID IN
                             (SELECT DISTINCT SE_ID
                                FROM MMS_FINISHEDTRACKIN_LIST
                               WHERE INSERT_TIME > SYSDATE - 2)
                       GROUP BY b.udf05,
                                S.SE_ID,
                                SS.MER_PO,
                                SSS.PROD_NO,
                                D.NAME_T,
                                SSS.SE_QTY,
                                SS.DESCOUNTRY_CODE,
                                X.C_NAME,
                                SSS.CR_REQDATE), progress AS (SELECT s.se_id,
                                                                     s.insert_time,
                                                                     s.qty,
                                                                     SUM(s.qty) OVER(PARTITION BY s.se_id ORDER BY s.insert_time, s.rowid) cumulative_qty
                                                                FROM MMS_FINISHEDTRACKIN_LIST s
                                                              
                                                               where s.se_id in
                                                                     (SELECT DISTINCT SE_ID
                                                                        FROM MMS_FINISHEDTRACKIN_LIST
                                                                       WHERE INSERT_TIME >
                                                                             SYSDATE - 2)), numbers AS (SELECT LEVEL lvl
                                                                                                          FROM dual
                                                                                                        CONNECT BY LEVEL <= 100),
       
       split AS (SELECT b.*,
                        n.lvl,
                        
                        CASE
                          WHEN n.lvl * 3200 <= b.IN_QTY THEN
                           3200
                          ELSE
                           b.IN_QTY - ((n.lvl - 1) * 3200)
                        END SPLIT_IN_QTY,
                        
                        CASE
                          WHEN b.ORDER_QTY < 3200 THEN
                           (SELECT MAX(p.insert_time)
                              FROM progress p
                             WHERE p.se_id = b.se_id)
                        
                          WHEN n.lvl * 3200 <= b.ORDER_QTY THEN
                           (SELECT MIN(p.insert_time)
                              FROM progress p
                             WHERE p.se_id = b.se_id
                               AND p.cumulative_qty >= n.lvl * 3200)
                        
                          ELSE
                           (SELECT MIN(p.insert_time)
                              FROM progress p
                             WHERE p.se_id = b.se_id
                               AND p.cumulative_qty >= b.ORDER_QTY)
                        END RECEIVED_TIME,
                        
                        CASE
                          WHEN ((CASE
                                 WHEN n.lvl * 3200 <= b.IN_QTY THEN
                                  3200
                                 ELSE
                                  b.IN_QTY - ((n.lvl - 1) * 3200)
                               END) + ((n.lvl - 1) * 3200)) = b.ORDER_QTY THEN
                           1
                          ELSE
                           0
                        END IS_FINAL_MATCH
                 
                   FROM base b
                   JOIN numbers n
                     ON n.lvl <= CEIL(b.IN_QTY / 3200)),
       
       bad_qty AS (SELECT task_no,
                          is_inspection,
                          SUM(CASE
                                WHEN problem_level = 0 THEN
                                 bad_qty
                                ELSE
                                 0
                              END) bad_qty_lvl0,
                          SUM(CASE
                                WHEN problem_level = 1 THEN
                                 bad_qty
                                ELSE
                                 0
                              END) bad_qty_lvl1
                     FROM (SELECT a.task_no,
                                  MAX(a.bad_qty) bad_qty,
                                  b.is_inspection,
                                  a.problem_level
                             FROM aql_cma_task_list_m_aql_e_br a
                             JOIN aql_cma_task_list_m b
                               ON a.task_no = b.task_no
                            GROUP BY a.task_no,
                                     bad_classify_code,
                                     bad_item_code,
                                     b.is_inspection,
                                     a.problem_level) --where task_no like '%0902064864%'
                    GROUP BY task_no, is_inspection)
       
         SELECT s.plant,
                s.SE_ID,
                s.MER_PO,
                s.MER_PO || '-' || s.lvl TASK_NO,
                s.ARTICLE,
                s.ART_NAME MODEL_NAME,
                s.ORDER_QTY,
                s.SPLIT_IN_QTY BATCH_QTY,
                s.RECEIVED_TIME,
                s.DESCOUNTRY_NAME,
                s.CR_REQDATE CRD,
                CASE
                  WHEN b.is_inspection IS NULL OR b.is_inspection = 0 THEN
                   'NOT INSPECTED'
                
                  WHEN NVL(b.bad_qty_lvl0, 0) > NVL(m.AC12, 0) OR
                       NVL(b.bad_qty_lvl1, 0) > NVL(m.AC13, 0) THEN
                   'REJECTED'
                
                  ELSE
                   'ACCEPTED'
                END AQL_RESULT
         
           FROM split s
         
           LEFT JOIN bad_qty b
             ON b.task_no = (s.MER_PO || '-' || s.lvl)
         
           LEFT JOIN BDM_AQL_M m
             ON m.HORI_TYPE = '2'
            AND m.LEVEL_TYPE = '2'
            AND TO_NUMBER(m.START_QTY) <= s.SPLIT_IN_QTY
            AND TO_NUMBER(m.END_QTY) >= s.SPLIT_IN_QTY
         
          WHERE (s.IS_FINAL_MATCH = 1 OR (s.lvl * 3200 <= s.IN_QTY))
            and s.RECEIVED_TIME is not null
            and to_char(s.RECEIVED_TIME, 'yyyy/MM/dd') > '2026/03/01' --Here date is static because Project went live from this date
          ORDER BY s.RECEIVED_TIME)
          where AQL_RESULT = 'NOT INSPECTED'";
                OracleDataAdapter da = new OracleDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                string fileName = $"AQL_Inspection_Alert_{DateTime.Now:yyyyMMdd_HHmmss}";
                string Msg = $"Please Check the AQL Inspection Result of PO's received Upto *{DateTime.Now:yyyy/MM/dd}* in above image.Please finish the pending Inspection ASAP.";
                string heading = $"AQL Inspection Status Upto {DateTime.Now:yyyy/MM/dd HH:mm:ss}";

                if (dt.Rows.Count > 0)
                {
                    string html = ConvertAQLInspectionToHTML(dt, heading);

                    // WhatsApp API send
                    SendWhatsappMessageAsimage(fileName, Msg, html);
                }

                return rt;
            }
            catch (Exception ex)
            {
                rt.TYPE = "E";
                rt.MESSAGE = ex.Message;
                return rt;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }


        public static string ConvertAQLInspectionToHTML(DataTable dt, string heading)
        {
            if (dt == null || dt.Rows.Count == 0)
                return "<div><h3>No AQL inspection data available</h3></div>";

            var sb = new StringBuilder();

            sb.Append("<html><body style='font-family:Segoe UI, Arial; background:#f2f6fb;'>");

            // =================== Heading Banner ===================
            sb.Append($@"
        <div style='background:#1F4E79;
                    color:#ffffff;
                    text-align:center;
                    padding:14px;
                    margin-bottom:15px;
                    border-radius:6px;
                    font-size:18px;
                    font-weight:bold;'>
            {heading}
        </div>");

            // =================== Table Wrapper ===================
        //    sb.Append(@"
        //<div style='display:flex;justify-content:center;'>
        //<table style='border-collapse:collapse;
        //              min-width:92%;
        //              max-width:96%;
        //              background:#ffffff;
        //              font-size:12.5px;
        //              border-radius:6px;
        //              overflow:hidden;
        //              box-shadow:0 3px 10px rgba(0,0,0,0.18);'>");
            sb.Append(@"
        <div style='display:flex;justify-content:center;'>
        <table style='border-collapse:collapse;
              width:auto;
              margin:auto;
              background:#ffffff;
              font-size:12.5px;
              border-radius:6px;
              overflow:hidden;
              box-shadow:0 3px 10px rgba(0,0,0,0.18);
              table-layout:auto;'>");

            // =================== Table Header ===================
            sb.Append("<thead><tr style='background:#4F81BD;color:#ffffff;'>");

            foreach (DataColumn c in dt.Columns)
            {
                sb.Append($@"
            <th style='padding:9px;
                       border:1px solid #9CC2E5;
                       text-align:center;
                       font-weight:bold;
                       white-space:nowrap;'>
                {System.Net.WebUtility.HtmlEncode(c.ColumnName)}
            </th>");
            }

            sb.Append("</tr></thead><tbody>");

            // =================== Table Body ===================
            int idx = 0;

            foreach (DataRow r in dt.Rows)
            {
                string rowBg = (idx++ % 2 == 0) ? "#FFFFFF" : "#E9F1FB";

                sb.Append($"<tr style='background:{rowBg};'>");

                foreach (DataColumn c in dt.Columns)
                {
                    string val = r[c] == DBNull.Value ? "" : r[c].ToString();
                    //string cellStyle =
                    //    "padding:8px;border:1px solid #9CC2E5;text-align:center;";
                    string cellStyle =
                    "padding:8px;border:1px solid #9CC2E5;text-align:center;white-space:nowrap;";



                    // ===== AQL_RESULT Color Highlight =====
                    if (c.ColumnName.Equals("AQL_RESULT", StringComparison.OrdinalIgnoreCase))
                    {
                        string status = val.ToUpper();

                        if (status == "NOT INSPECTED")
                            cellStyle += "background:#FFF2CC;color:#7F6000;font-weight:bold;";
                        else if (status == "REJECTED")
                            cellStyle += "background:#F8CBAD;color:#9C0006;font-weight:bold;";
                        else if (status == "ACCEPTED")
                            cellStyle += "background:#C6E0B4;color:#006100;font-weight:bold;";
                        else
                            cellStyle += "background:#E7E6E6;font-weight:bold;";
                    }
                    else
                    {
                        cellStyle += $"background:{rowBg};";
                    }

                    sb.Append($"<td style='{cellStyle}'>" +
                              $"{System.Net.WebUtility.HtmlEncode(val)}</td>");
                }

                sb.Append("</tr>");
            }

            sb.Append("</tbody></table></div>");

            // =================== Footer ===================
            sb.Append(@"
        <div style='text-align:center;margin-top:15px;color:#333;font-size:12px;'>
            <b>Note:</b> This is an automated AQL Inspection Alert report.
        </div>");

            sb.Append("</body></html>");

            return sb.ToString();
        }



        public List<string> SendWhatsappMessageAsimage(string fileName, string msg, string htmldata)
        {
            List<string> responseMessages = new List<string>();

            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";
            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "120363423613523406@g.us" },//120363423613523406@g.us(AQL Inspection Result)//120363347683285873@g.us(test)
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
                    string jsonPayload = JsonConvert.SerializeObject(payload);
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = response.Content.ReadAsStringAsync().Result;
                        responseMessages.Add(responseData);
                        FMSLOG.Platform(responseData, "AQLInspection_Report");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "AQLInspection_Report");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "AQLInspection_Report");
            }

            return responseMessages;
        }

        #endregion

        #region PO wise or batch wise Completion Report
        public Cls_Return Send_AQL_PO_Receive_Alert(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            OracleConnection con = null;

            try
            {
                con = new OracleConnection(directConStr);
                con.Open();
                //                string sql = @"WITH base AS
                // (SELECT S.SE_ID,
                //         MAX(SSS.SE_QTY) ORDER_QTY,
                //         SUM(S.QTY) IN_QTY,
                //         MAX(S.INSERT_TIME) AS RECEIVED_TIME,
                //         x.po,
                //         x.article,
                //         b.udf05,
                //         to_char(x.crd, 'yyyy/MM/dd') crd
                //    FROM MMS_FINISHEDTRACKIN_LIST S
                //   INNER JOIN BDM_SE_ORDER_ITEM SSS
                //      ON S.SE_ID = SSS.SE_ID
                //   INNER JOIN Aql_Inspection_Po_List x
                //      ON S.SE_ID = x.SO
                //   INNER JOIN BASE005M b
                //      on s.from_line = b.dep_sap
                //     and b.udf06 = 'Y'
                //   WHERE x.crd BETWEEN TRUNC(SYSDATE) AND TRUNC(SYSDATE + 7)
                //   GROUP BY S.SE_ID, x.po, x.article, b.udf05, to_char(x.crd, 'yyyy/MM/dd'))
                //SELECT b.SE_ID,
                //       b.po,
                //       b.article,
                //       b.ORDER_QTY,
                //       b.IN_QTY,
                //       b.received_time,
                //       b.udf05 as Plant,
                //       FLOOR(b.IN_QTY / 3200) milestone,
                //       b.crd
                //  FROM base b
                //  LEFT JOIN AQL_PO_ALERT_TRACK t
                //    ON t.SE_ID = b.SE_ID
                // WHERE FLOOR(b.IN_QTY / 3200) > NVL(t.LAST_MILESTONE, 0)
                //    OR (b.IN_QTY >= b.ORDER_QTY AND NVL(t.FINAL_SENT, 0) = 0)";

                string sql = @"WITH base AS
 (SELECT S.SE_ID,
         SSS.PROD_NO article,
         to_char(SSS.CR_REQDATE, 'yyyy/MM/dd') crd,
         s.po,
         MAX(SSS.SE_QTY) ORDER_QTY,
         SUM(S.QTY) IN_QTY,
         MAX(S.INSERT_TIME) AS RECEIVED_TIME,
         b.udf05
    FROM MMS_FINISHEDTRACKIN_LIST S
   INNER JOIN BDM_SE_ORDER_ITEM SSS
      ON S.SE_ID = SSS.SE_ID
   INNER JOIN (SELECT DISTINCT SE_ID
                FROM MMS_FINISHEDTRACKIN_LIST
               WHERE INSERT_TIME > SYSDATE - 1) X
      ON S.SE_ID = x.SE_ID
   INNER JOIN BASE005M b
      on s.from_line = b.dep_sap
     and b.udf06 = 'Y'  AND S.ORG_ID=B.FACTORY_SAP
   GROUP BY S.SE_ID,  SSS.PROD_NO,
         SSS.CR_REQDATE, s.po, b.udf05)
SELECT b.SE_ID,
       b.po,
       b.article,
       b.ORDER_QTY,
       b.IN_QTY,
       b.received_time,
       b.udf05 as Plant,
       FLOOR(b.IN_QTY / 3200) milestone ,
       b.crd
  FROM base b
  LEFT JOIN AQL_PO_ALERT_TRACK t
    ON t.SE_ID = b.SE_ID
 WHERE FLOOR(b.IN_QTY / 3200) > NVL(t.LAST_MILESTONE, 0)
    OR (b.IN_QTY >= b.ORDER_QTY AND NVL(t.FINAL_SENT, 0) = 0)";
                OracleDataAdapter da = new OracleDataAdapter(sql, con);
                DataTable dt = new DataTable();  
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string So = dr["SE_ID"].ToString();
                        int OrderQty = Convert.ToInt32(dr["ORDER_QTY"]);
                        int InQty = Convert.ToInt32(dr["IN_QTY"]);
                        string Msg = ConvertCompletedPOToWhatsApp(dr);
                        SendCompletedPODetails(Msg);
                        Update_AQL_PO_Alert_Track(con,So, OrderQty, InQty);
                    }
                    
                }

                return rt;
            }
            catch (Exception ex)
            {
                rt.TYPE = "E";
                rt.MESSAGE = ex.Message;
                return rt;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }


        public void Update_AQL_PO_Alert_Track(OracleConnection Con,string So,int OrderQty, int InQty)
        {
            string sql = $@"MERGE INTO AQL_PO_ALERT_TRACK t
USING (SELECT '{So}' id,
              FLOOR({InQty} / 3200) milestone,
              CASE
                WHEN {InQty} >= {OrderQty} THEN
                 1
                ELSE
                 0
              END final_flag
         FROM dual) s
ON (t.SE_ID = s.id)

WHEN MATCHED THEN
  UPDATE
     SET t.LAST_MILESTONE  = s.milestone,
         t.FINAL_SENT      = s.final_flag,
         t.LAST_ALERT_TIME = SYSDATE
WHEN NOT MATCHED THEN
  INSERT
    (SE_ID, LAST_MILESTONE, FINAL_SENT, LAST_ALERT_TIME)
  VALUES
    (s.id, s.milestone, s.final_flag, SYSDATE)";
            OracleCommand cmd = new OracleCommand(sql, Con);
            cmd.ExecuteNonQuery();
            
        }

        public static string ConvertCompletedPOToWhatsApp(DataRow dr)
        {
            if (dr == null)
                return "No PO data available.";

            string seId = dr["SE_ID"]?.ToString();
            string po = dr["PO"]?.ToString();
            string article = dr["ARTICLE"]?.ToString();
            string orderQty = dr["ORDER_QTY"]?.ToString();
            string inQty = dr["IN_QTY"]?.ToString();
            string crd = dr["CRD"]?.ToString();
            string Plant = dr["PLANT"]?.ToString();
            string Received_Time = dr["RECEIVED_TIME"]?.ToString();

            var sb = new StringBuilder();

            sb.AppendLine("📦 *PO FG Receive Alert*");
            sb.AppendLine($"Dear AQL Inspector, below PO from Plant *{Plant}* received to FG WareHouse");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"🆔Sales Order : *{seId}*");
            sb.AppendLine($"📄 PO Number : {po}");
            sb.AppendLine($"👟 Article : {article}");
            sb.AppendLine($"📅 CRD : {crd}");
            sb.AppendLine("");
            sb.AppendLine($"📊 PO Total Qty : *{orderQty}*");
            sb.AppendLine($"✅ Received Qty : *{inQty}*");
            sb.AppendLine($"📅 Received_Time : {Received_Time}");
            // ===== Status Logic =====
            int produced = Convert.ToInt32(inQty);
            int order = Convert.ToInt32(orderQty);

            if (produced >= order)
                sb.AppendLine("\n🎉 *PO Status : PO COMPLETED*");
            else if (produced % 3200 == 0)
                sb.AppendLine("\n📦 *PO Status : Batch Completed (3200)*");

            sb.AppendLine("--------------------------------");
            sb.AppendLine("🤖 Please Finish the PO Inspection and Submit AQL Test result in AEQS.");

            return sb.ToString();
        }

        public async Task SendCompletedPODetails(string msg)
        {
            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendMessage";

            var payload = new
            {
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "120363423613523406@g.us" },//120363423613523406@g.us(AQL Inspection Result)//120363347683285873@g.us(test)
                textMsg = msg,
                mediaurl = "",
                filename = ""
            };

            //var payload = new
            //{
            //    numbers = new[] { "9640416084" }, // Use the fetched phone number
            //    groups = new List<string>(),
            //    textMsg = msg,
            //    mediaurl = "",
            //    filename = ""
            //};



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

        #endregion

        #region  MES_To_AEQS_CompareData
        public Cls_Return MES_To_AEQS_CompareData(SrvInfo vsrvinfo)
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
                        string[] listFomat = new string[0];
                        conoa = new OracleConnection(constroa);
                        conoa.Open();
                        string sql = $@"SELECT DEPARTMENT_CODE FROM BASE005M A WHERE A.UDF01 in ('T','L') AND A.FACTORY_SAP='5001'";
                        DataTable dt = GetDataFromDatabase(constroa, sql);
                        DataTable dt1 = new DataTable(); 
                        transaction = conoa.BeginTransaction();
                        foreach (DataRow dr in dt.Rows)
                        {
                            
                            using (OracleCommand cmd = new OracleCommand("SP_MES_AEQS_COMPARE", conoa))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("P_PRODLINE", OracleDbType.Varchar2).Value = dr["DEPARTMENT_CODE"].ToString();
                                OracleParameter refCursorParam = new OracleParameter("V_CURSOR", OracleDbType.RefCursor);
                                refCursorParam.Direction = ParameterDirection.Output;
                                cmd.Parameters.Add(refCursorParam);
                                using (OracleDataReader reader = cmd.ExecuteReader())
                                {

                                    if (reader.HasRows)
                                    {
                                        dt1.Load(reader);
                                    }
                                    else
                                    {
                                        Console.WriteLine("No rows returned from the cursor.");
                                    }

                                }
                            }
                        }
                        transaction.Commit();
                        if (dt1.Rows.Count > 0)
                        {
                            string _path = System.Windows.Forms.Application.StartupPath + "\\MES_AEQS_COMPARE\\";

                            if (!Directory.Exists(_path))
                            {
                                Directory.CreateDirectory(_path);
                            }

                            string _fileName = DateTime.Now.ToString("yyyyMMdd");
                            string Date = DateTime.Now.ToString("yyyy/MM/dd");
                           // string _fileName = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                            string _filePath = _path + _fileName + ".xlsx";
                            if (File.Exists(_filePath))
                            {
                                File.Delete(_filePath);
                            }
                            ExportExcels.ExportFomat(dt1, _filePath, "sheet1", listFomat);
                            string[] attachList = new string[] { _filePath };

                            DataTable dt2 = new DataTable();
                            string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='QIP' and rolecode='MES_To_AEQS'";
                            //string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='TEST'";
                            OracleCommand cmd2 = new OracleCommand(sql2, conoa);
                            OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                            da2.Fill(dt2);
                            if (dt2.Rows.Count > 0)
                            {
                                string Error_msg = string.Empty;
                                string msg = $@"
        <html>
        <body style='font-family: Times New Roman, sans-serif; padding: 20px; color: #333;'>
            <p>Dear QIP Team,</p>

            <p>Please check the MES to AEQS Compare data of <strong>{Date}</strong> in above excel. Make sure that AEQS Output should match with MES </p>
           

           <div style='text-align: center; font-size: 0.9em; color: #777; margin-top: 20px;'>
                <p>For inquiries, please contact <a href='mailto:it-software05@in.apachefootwear.com'>it-software05@in.apachefootwear.com</a></p>
            </div>

        <p><strong>Thanks & Regards<strong></p>
        <p><strong>APC-IT Team<strong></p>
            
        </body>
        </html>";

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

        #region Quality_Bonus
        public Cls_Return Calculate_Quality_Bonus(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            // OracleConnection conoa = null;
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
                        string[] listFomat = new string[0];
                        string[] attachList = new string[0];
                        conoa = new OracleConnection(constroa);
                        conoa.Open();
                        DateTime enddate = DateTime.Now.AddDays(-3).Date;
                        if (enddate.DayOfWeek == DayOfWeek.Sunday)
                        {
                            return rt;
                        }
                        DateTime startdate = new DateTime(enddate.Year, enddate.Month, 1);
                        string Date = enddate.ToString("yyyyMM");
                        //string sql = $@"SELECT STITCHING_LINE,CUTTING_LINE,ASSEMBLY_LINE FROM T_QUALITY_BONUS_LINES WHERE MONTH = '{Date}'";
                        string sql = $@"SELECT ASSEMBLY_LINE,
       LISTAGG(DISTINCT CUTTING_LINE, ',') WITHIN GROUP(ORDER BY CUTTING_LINE) AS CUTTING_LINE,
       LISTAGG(DISTINCT STITCHING_LINE, ',') WITHIN GROUP(ORDER BY STITCHING_LINE) AS STITCHING_LINE
  FROM T_QUALITY_BONUS_LINES
 WHERE MONTH = '{Date}'
   AND CUTTING_LINE LIKE '%5001%'
 GROUP BY ASSEMBLY_LINE";
                        DataTable dt = GetDataFromDatabase(constroa, sql);
                        if (dt.Rows.Count > 0)
                        {
                            transaction = conoa.BeginTransaction();

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





                            for (DateTime currentDate = startdate; currentDate <= enddate; currentDate = currentDate.AddDays(1))
                            {
                                //if (currentDate.DayOfWeek == DayOfWeek.Sunday)
                                //    continue;
                                if (holidayDates.Contains(currentDate.Date))
                                    continue; // Skip holidays
                               
                                    using (OracleCommand cmd = new OracleCommand("QUALITY_BONUS", conoa))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add("P_DATE", OracleDbType.Date).Value = currentDate;
                                        cmd.ExecuteNonQuery();
                                    }
                                


                            }

                            transaction.Commit();
                            string End_Date = enddate.ToString("yyyyMMdd");
                            DataTable dt1 = new DataTable();
                            string sql2 = $@"SELECT prod_date,
       cut_line,
       stitch_line,
       assembly_line,
       output,
       cut_repack_qty,
       stitch_repack_qty,
       assembly_repack_qty,
       total_repack,
       repack_percent,
       cutting_rft,
       stitching_rft,
       assembly_rft,
       total_rft,
       cut_bgrades,
       stitch_bgrades,
       assembly_bgrades,
       total_bgrades,
       cut_repairs,
       stitch_repairs,
       assembly_repairs,
       total_repairs,
       bgrades_percent,
       operator_bonus,
       asst_bonus,
       sup_bonus,
       sh_bonus,
       updated_at
  FROM T_QUALITY_BONUS@Apchrdb a where to_char(a.prod_date,'yyyymmdd')='{End_Date}'";
                            OracleCommand cmd2 = new OracleCommand(sql2, conoa);
                            cmd2.CommandType = CommandType.Text;
                            OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                            da2.Fill(dt1);


                            string _path = System.Windows.Forms.Application.StartupPath + "\\Quality_Bonus\\";

                            if (!Directory.Exists(_path))
                            {
                                Directory.CreateDirectory(_path);
                            }

                            string _fileName = End_Date;
                            string _filePath = _path + _fileName + ".xlsx";
                            ExportExcels.ExportFomat(dt1, _filePath, "sheet1", listFomat);
                            attachList = new string[] { _filePath };

                            DataTable dt2 = new DataTable();
                            string sql3 = $@"SELECT TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT FROM TBL_E2E_MAIL_CONFIG WHERE DEPT='QIP' AND ROLECODE='QBonus01'";
                            //string sql3 = $@"SELECT TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT FROM TBL_E2E_MAIL_CONFIG WHERE DEPT='TEST'";
                            OracleCommand cmd3 = new OracleCommand(sql3, conoa);
                            OracleDataAdapter da3 = new OracleDataAdapter(cmd3);
                            da3.Fill(dt2);
                            if (dt2.Rows.Count > 0)
                            {
                                string Error_msg = string.Empty;
                                //string msg = $@"<b>Please check Qulaity Bonus of {End_Date} calculated from MES in above excel. Make sure that Bonus calculated is correct or wrong </b>";
                                string msg = $@"
        <html>
        <body style='font-family: Times New Roman, sans-serif; padding: 20px; color: #333;'>
            <p>Dear QIP Team,</p>
            
            <p><b>Quality Bonus for {End_Date} calculated from MES successfully. Please check the attached Excel for your reference. Make sure that the Bonus calculated is correct or incorrect.</b></p>
            
            <p>Thank you for your cooperation!</p>

            <div style='text-align: center; font-size: 0.9em; color: #777; margin-top: 20px;'>
                <p>For inquiries, please contact <a href='mailto:it-software05@in.apachefootwear.com'>it-software05@in.apachefootwear.com</a></p>
            </div>

        <p><strong>Thanks & Regards<strong></p>
        <p><strong>APC-IT Team<strong></p>

        </body>
        </html>";
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
                        else
                        {
                            Send_LinePlan_Not_Upload_alert(enddate);
                        }


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

        public void Send_LinePlan_Not_Upload_alert(DateTime enddate)
        {
            string[] attachList = new string[0];
            string Date = enddate.ToString("yyyyMM");
            DataTable dt2 = new DataTable();
            string sql3 = $@"SELECT TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT FROM TBL_E2E_MAIL_CONFIG WHERE DEPT='QIP' AND ROLECODE='QBonus02'";
           // string sql3 = $@"SELECT TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT FROM TBL_E2E_MAIL_CONFIG WHERE DEPT='TEST'";
            OracleCommand cmd3 = new OracleCommand(sql3, conoa);
            OracleDataAdapter da3 = new OracleDataAdapter(cmd3);
            da3.Fill(dt2);
            if (dt2.Rows.Count > 0)
            {
                string Error_msg = string.Empty;
                string msg = $@"
        <html>
        <body style='font-family: Times New Roman, sans-serif; padding: 20px; color: #333;'>
            <p>Dear Planning Team,</p>

            <p>To calculate the Quality Bonus, we need the line planning data for all APC and APEX lines. Please upload the data for the month of <strong>{Date}</strong> as soon as possible.</p>

            <p>Thank you for your cooperation!</p>

           <div style='text-align: center; font-size: 0.9em; color: #777; margin-top: 20px;'>
                <p>For inquiries, please contact <a href='mailto:it-software05@in.apachefootwear.com'>it-software05@in.apachefootwear.com</a></p>
            </div>

        <p><strong>Thanks & Regards<strong></p>
        <p><strong>APC-IT Team<strong></p>
            
        </body>
        </html>";
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

        static DataTable GetDataFromDatabase(string connectionString, string query)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                OracleDataAdapter dataAdapter = new OracleDataAdapter(query, conn);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                return dataTable;
            }
        }
        #endregion

        #region Send_Line_RFT_old
        //public async Task<Cls_Return> Send_Line_RFT(SrvInfo vsrvinfo)
        //{

        //    Cls_Return rt = new Cls_Return();
        //    OracleTransaction transaction = null;
        //    try
        //    {
        //        string constroa = null;
        //        string filePath;
        //        filePath = Application.ExecutablePath;
        //        filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
        //        string clientEnvConfigFileName = filePath + "database.config";
        //        XmlDocument clientEnvConfigDoc = new XmlDocument();
        //        if (File.Exists(clientEnvConfigFileName))
        //        {
        //            FileLoader obj = new FileLoader(clientEnvConfigFileName);
        //            Hashtable htdblinks = obj.GetDBLinks();
        //            if (htdblinks.ContainsKey(vsrvinfo.SDB))
        //                constroa = htdblinks[vsrvinfo.SDB].ToString();
        //            try
        //            {
        //                conoa = new OracleConnection(constroa);
        //                conoa.Open();
        //                transaction = conoa.BeginTransaction();
        //                DataTable dt = new DataTable();
        //                using (OracleCommand cmd = new OracleCommand("SP_GET_LINE_RFT_DATA", conoa))
        //                {
        //                    cmd.CommandType = CommandType.StoredProcedure;
        //                    OracleParameter outputCursorParam = new OracleParameter("result_cursor", OracleDbType.RefCursor);
        //                    outputCursorParam.Direction = ParameterDirection.Output;
        //                    cmd.Parameters.Add(outputCursorParam);
        //                    OracleDataAdapter dataAdapter = new OracleDataAdapter(cmd);
        //                    dataAdapter.Fill(dt);
        //                }
        //                transaction.Commit();
        //                if(dt.Rows.Count>0)
        //                {
        //                    string msg = ConvertDataTableToHTML(dt);
        //                    SendRFTAlerts(msg);
        //                }
        //                else
        //                {
        //                    Send_LinePlan_Not_Upload_alert(DateTime.Now.AddDays(-1));
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                if (transaction != null)
        //                {
        //                    transaction.Rollback();
        //                }
        //            }
        //            finally
        //            {
        //                if (conoa != null && conoa.State == System.Data.ConnectionState.Open)
        //                {
        //                    conoa.Close();
        //                }
        //            }
        //        }

        //        return rt;
        //    }
        //    catch (Exception e)
        //    {
        //        rt.TYPE = "E";
        //        rt.MESSAGE = e.Message;
        //        return rt;
        //    }
        //    finally
        //    {
        //        conoa.Close();
        //        conoa.Dispose();
        //        GC.Collect();
        //    }

        //}

        //public string ConvertDataTableToHTML(DataTable dt)
        //{
        //    StringBuilder html = new StringBuilder();
        //    string currentDate = DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd");

        //    html.Append("<html><body>");
        //    html.Append("<p style='font-family: Times New Roman; font-size: 16px;'>Dear All,</p>");
        //    html.Append($"<p style='font-family: Times New Roman; font-size: 16px;'>Please check the RFT data of each Production Line for <strong>{currentDate}</strong> in the table below:</p>");
        //    html.Append("<h2 style='font-family: Times New Roman;'>Line Wise RFT Data</h2>");

        //    html.Append("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse; width:100%; font-family: Times New Roman;'>");

        //    // Header
        //    html.Append("<tr>");
        //    foreach (DataColumn column in dt.Columns)
        //    {
        //        html.Append($"<th style='background-color:#f2f2f2; text-align:center;'>{System.Net.WebUtility.HtmlEncode(column.ColumnName)}</th>");
        //    }
        //    html.Append("</tr>");

        //    // Data Rows
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        html.Append("<tr>");
        //        foreach (DataColumn col in dt.Columns)
        //        {
        //            string colName = col.ColumnName;
        //            string value = row[col]?.ToString().Trim() ?? "";
        //            string encodedValue = System.Net.WebUtility.HtmlEncode(value);
        //            string style = "text-align:center;";

        //            // Determine section prefix: ASS, CUT, STITCH
        //            string[] prefixes = { "ASS", "CUT", "STITCH" };
        //            string matchedPrefix = prefixes.FirstOrDefault(prefix => colName.StartsWith(prefix));

        //            decimal rftValue = -1;

        //            // Find corresponding RFT column and value
        //            if (matchedPrefix != null)
        //            {
        //                string rftColName = $"{matchedPrefix}_RFT";
        //                if (dt.Columns.Contains(rftColName))
        //                {
        //                    decimal.TryParse(row[rftColName]?.ToString(), out rftValue);
        //                }

        //                // Apply coloring to RFT and its related QTY columns
        //                if (colName == rftColName || colName.Contains("INSPECTION_QTY") || colName.Contains("QUALIFIED_QTY"))
        //                {
        //                    if (rftValue == 0)
        //                        style += "background-color:#f8d7da; color:#721c24;";
        //                    else if (rftValue == 100)
        //                        style += "background-color:#d4edda; color:#155724;";
        //                    else if (rftValue > 0 && rftValue < 100)
        //                        style += "background-color:#fff3cd; color:#856404;";
        //                }
        //            }

        //            html.Append($"<td style='{style}'>{encodedValue}</td>");
        //        }
        //        html.Append("</tr>");
        //    }

        //    html.Append("</table>");
        //    html.Append("</body></html>");

        //    return html.ToString();
        //}



        //public void SendRFTAlerts(string msg)
        //{
        //   // OracleConnection conoa = null;
        //    string[] listFomat = new string[0];
        //    string[] attachList = new string[0];
        //    string Error_msg = string.Empty;
        //    string Msg = msg;
        //    DataTable dt2 = new DataTable();
        //    string sql2 = $@"SELECT TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT FROM TBL_E2E_MAIL_CONFIG WHERE DEPT='QIP' AND ROLECODE='QBonus03'";
        //    //string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='TEST'";
        //    OracleCommand cmd2 = new OracleCommand(sql2, conoa);
        //    OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
        //    da2.Fill(dt2);
        //    if (dt2.Rows.Count > 0)
        //    {
        //        string To_List = dt2.Rows[0]["TO_LIST"].ToString();
        //        string CC_List = dt2.Rows[0]["CC_LIST"].ToString();
        //        string Error_List = dt2.Rows[0]["ERROR_LIST"].ToString();
        //        string mailSubject = dt2.Rows[0]["MAIL_SUBJECT"].ToString();
        //        List<string> listSend = To_List.Split(';').ToList();
        //        List<string> listCopy = CC_List.Split(';').ToList();
        //        List<string> listError = Error_List.Split(';').ToList();
        //        if (MailUtil.SendMessage(listSend, listCopy, mailSubject, msg, attachList, out Error_msg))
        //        {
        //        }
        //        else
        //        {
        //            MailUtil.SendMessage(listError, listError, mailSubject, Error_msg + "\n" + DateTime.Now, null, out Error_msg);
        //        }


        //    }

        //}

        #endregion

        #region Send_Line_RFT_New
        public async Task<Cls_Return> Send_Line_RFT(SrvInfo vsrvinfo)
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
                        DataTable dt1 = new DataTable();
                        DataTable dt2 = new DataTable();
                        DataTable dt3 = new DataTable();
                        DataTable dt = new DataTable();


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
                        OracleCommand cmd = new OracleCommand(SQL_date, conoa);
                        OracleDataAdapter da = new OracleDataAdapter(cmd);
                        da.Fill(dt);

                        DateTime lastWorkingDate = Convert.ToDateTime(dt.Rows[0]["DT"]);
                        string format1 = lastWorkingDate.ToString("yyyy/MM/dd");
                        string format2 = lastWorkingDate.ToString("yyyy-MM-dd");

                        string _path = Path.Combine(Application.StartupPath, "Daily_RFT");

                        if (!Directory.Exists(_path))
                        {
                            Directory.CreateDirectory(_path);
                        }
                        string _fileName = lastWorkingDate.ToString("yyyyMMdd") + ".xlsx";
                        string _filePath = Path.Combine(_path, _fileName);

                        string sql1 = $@"SELECT TO_CHAR(PROD_DATE, 'YYYY/MM/DD') PROD_DATE,
       PROD_LINE,
       INSPECTION_QTY,
       QUALIFIED_QTY,
       ROUND((QUALIFIED_QTY / INSPECTION_QTY), 4) RFT
  FROM (SELECT B.PRODUCTION_LINE_CODE PROD_LINE,
               TO_DATE(B.CREATEDATE, 'YYYY-MM-DD') PROD_DATE,
               SUM(CASE
                     WHEN A.COMMIT_TYPE IN (0, 2) THEN
                      1
                     ELSE
                      0
                   END) AS QUALIFIED_QTY,
               COUNT(A.COMMIT_TYPE) AS INSPECTION_QTY
          FROM RQC_TASK_DETAIL_T A
          JOIN RQC_TASK_M B
            ON A.TASK_NO = B.TASK_NO
         WHERE B.CREATEDATE = '{format2}'
           AND B.WORKSHOP_SECTION_NO IN ('C')
         GROUP BY B.CREATEDATE, B.PRODUCTION_LINE_CODE
         ORDER BY B.PRODUCTION_LINE_CODE)";

                        string sql2 = $@"SELECT TO_CHAR(PROD_DATE, 'yyyy/MM/dd') PROD_DATE,
       PROD_LINE,
       INSPECTION_QTY,
       TOTAL_PASS_QTY QUALIFIED_QTY,
       RFT/100 as RFT
  FROM TQC_MANUAL_RFT
 WHERE TO_CHAR(PROD_DATE, 'yyyy/MM/dd') =
      '{format1}'
 ORDER BY PROD_LINE";
                        string sql3 = $@"SELECT TO_CHAR(PROD_DATE, 'YYYY/MM/DD') PROD_DATE,
       PROD_LINE,
       INSPECTION_QTY,
       QUALIFIED_QTY,
       ROUND((QUALIFIED_QTY / INSPECTION_QTY), 4) RFT
  FROM (SELECT B.PRODUCTION_LINE_CODE PROD_LINE,
               TO_DATE(B.CREATEDATE, 'YYYY-MM-DD') PROD_DATE,
               SUM(CASE
                     WHEN A.COMMIT_TYPE IN (0, 2) THEN
                      1
                     ELSE
                      0
                   END) AS QUALIFIED_QTY,
               COUNT(A.COMMIT_TYPE) AS INSPECTION_QTY
          FROM TQC_TASK_COMMIT_M A
          JOIN TQC_TASK_M B
            ON A.TASK_NO = B.TASK_NO
         WHERE B.CREATEDATE = '{format2}'
           AND B.WORKSHOP_SECTION_NO IN ('L')
         GROUP BY B.CREATEDATE, B.PRODUCTION_LINE_CODE
         ORDER BY B.PRODUCTION_LINE_CODE)";
                        OracleCommand cmd1 = new OracleCommand(sql1, conoa);
                        OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
                        da1.Fill(dt1);
                        OracleCommand cmd2 = new OracleCommand(sql2, conoa);
                        OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                        da2.Fill(dt2);
                        OracleCommand cmd3 = new OracleCommand(sql3, conoa);
                        OracleDataAdapter da3 = new OracleDataAdapter(cmd3);
                        da3.Fill(dt3);
                        
                        transaction.Commit();
                        if (dt1.Rows.Count > 0 || dt2.Rows.Count > 0|| dt3.Rows.Count > 0)
                        {

                            var sheetData = new Dictionary<string, DataTable>
{
    { "Cutting", dt1 },
    { "Stitching", dt2 },
    { "Assembly", dt3 }
};

                            ExportMultipleSheets(sheetData, _filePath);

                            string[] attachList = new string[] { _filePath };

                            SendRFTAlerts(attachList, format1);
                        }
                        

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


        public void SendRFTAlerts(string[] AttachList,string date)
        {
            // OracleConnection conoa = null;
            string[] listFomat = new string[0];
            string[] attachList = AttachList;
            string Error_msg = string.Empty;
            //string Date = DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd");
            string msg = $@"
        <html>
        <body style='font-family: Times New Roman, sans-serif; padding: 20px; color: #333;'>
            <p>Dear QIP Team,</p>

            <p>Please check the above attachment for Line-Wise RFT Details of <strong>{date}</strong></p>
           

           <div style='text-align: center; font-size: 0.9em; color: #777; margin-top: 20px;'>
                <p>For inquiries, please contact <a href='mailto:it-software05@in.apachefootwear.com'>it-software05@in.apachefootwear.com</a></p>
            </div>

        <p><strong>Thanks & Regards<strong></p>
        <p><strong>APC-IT Team<strong></p>
            
        </body>
        </html>";

            DataTable dt2 = new DataTable();
            string sql2 = $@"SELECT TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT FROM TBL_E2E_MAIL_CONFIG WHERE DEPT='QIP' AND ROLECODE='QBonus03'";
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

        }

        #endregion

        #region FGT Digitalization Report
        public async Task<Cls_Return> FGT_Digitalization_Report(SrvInfo vsrvinfo)
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
                      

                        string sql = $@"WITH tmp2 AS
 (SELECT art_no, test_result
    FROM qcm_ex_task_list_m
   WHERE test_type = '0'
     AND createdate >= TO_CHAR(SYSDATE - 90, 'YYYY-MM-DD')),
tmp1 AS
 (SELECT d.mer_po,
         b.prod_no,
         b.cr_reqdate,
         c.work_center,
         MIN(a.date_start_plan) date_start_plan
    FROM mes010m a
    JOIN bdm_se_order_item b
      ON a.sales_order = b.se_id
    JOIN mes010a1 c
      ON a.production_order = c.production_order
    JOIN bdm_se_order_master d
      on b.se_id = d.se_id
     AND c.procedure_no = 'L'
   WHERE a.order_type = 'ZP01'
     AND b.cr_reqdate between trunc(sysdate) and trunc(sysdate+15)
   GROUP BY d.mer_po, b.prod_no, b.cr_reqdate, c.work_center),
final_data AS
 (SELECT t1.prod_no,
         t1.mer_po PO_Number,
         t1.work_center,
         t1.date_start_plan,
         t1.cr_reqdate,
         t2.test_result,
         ROW_NUMBER() OVER(PARTITION BY t1.prod_no ORDER BY t1.cr_reqdate, t1.date_start_plan) rn
    FROM tmp1 t1
    LEFT JOIN tmp2 t2
      ON t1.prod_no = t2.art_no)

SELECT f.prod_no,
       f.PO_Number,
       f.work_center,
       -- f.date_start_plan,
       TO_CHAR(f.cr_reqdate, 'yyyy/MM/dd') cr_reqdate,
       -- d.test_type,
       d.shoe_size,
       d.quantity,
       -- f.test_result,
       d.lab_requested_date,
       d.prod_send_date,
       d.lab_confirmed_date
  FROM final_data f
  LEFT JOIN T_FGT_DIGITALIZATION d
    ON f.prod_no = d.prod_no
   AND f.cr_reqdate = d.cr_reqdate
 WHERE f.rn = 1
   and f.test_result is null
 ORDER BY f.cr_reqdate, f.date_start_plan";

                       
                        OracleCommand cmd = new OracleCommand(sql, conoa);
                        OracleDataAdapter da = new OracleDataAdapter(cmd);
                        da.Fill(dt);
                        transaction.Commit();
                        string fileName = $"FGT_Digitalization_Report_{DateTime.Now:yyyyMMdd_HHmmss}";
                        string Msg = $"Please Check the FGT Digitalization Report of Articles of Month  *{DateTime.Now:yyyy/MM}* in above image.";
                        string heading = $"FGT Digitalization Report Upto {DateTime.Now:yyyy/MM/dd}";

                        if (dt.Rows.Count > 0)
                        {
                            string WhasApp = FGT_Status_WhatsApp(dt, heading);
                            SendFGTReportAsimage(fileName, Msg, WhasApp);

                            string Mail = FGT_Status_Mail(dt, heading);
                            SendFGTReportAsMail(Mail);
                        }


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

        public static string FGT_Status_WhatsApp(DataTable dt, string heading)
        {
            if (dt == null || dt.Rows.Count == 0)
                return "<div><h3>No FGT_Digitalization_Report available</h3></div>";

            var sb = new StringBuilder();

            sb.Append("<html><body style='font-family:Segoe UI, Arial; background:#f2f6fb; margin:0; padding:10px;'>");

            // =================== WRAPPER ===================
            sb.Append(@"
    <div style='width:100%;'>");

            // =================== TABLE ===================
            sb.Append(@"
        <table id='mainTable' style='border-collapse:collapse;
                      width:100%;
                      background:#ffffff;
                      font-size:12.5px;
                      border-radius:6px;
                      overflow:hidden;
                      box-shadow:0 3px 10px rgba(0,0,0,0.18);'>");

            // =================== THEAD ===================
            sb.Append("<thead>");

            // ===== TOP HEADING ROW (MERGED) =====
            sb.Append($@"
    <tr>
        <th colspan='{dt.Columns.Count}'
            style='background:#1F4E79;
                   color:#ffffff;
                   text-align:center;
                   padding:14px;
                   font-size:16px;
                   font-weight:bold;
                   border:1px solid #1F4E79;'>
            {heading}
        </th>
    </tr>");

            // ===== COLUMN HEADER ROW =====
            sb.Append("<tr style='background:#4F81BD;color:#ffffff;'>");

            foreach (DataColumn c in dt.Columns)
            {
                sb.Append($@"
        <th style='padding:9px;
                   border:1px solid #9CC2E5;
                   text-align:center;
                   font-weight:bold;
                   white-space:nowrap;'>
            {System.Net.WebUtility.HtmlEncode(c.ColumnName)}
        </th>");
            }

            sb.Append("</tr></thead><tbody>");

            // =================== BODY ===================
            int idx = 0;

            foreach (DataRow r in dt.Rows)
            {
                string rowBg = (idx++ % 2 == 0) ? "#FFFFFF" : "#E9F1FB";
                sb.Append($"<tr style='background:{rowBg};'>");

                DateTime? crReqDate = null;
                DateTime? labRequestedDate = null;
                DateTime? prodSendDate = null;
                DateTime? labConfirmedDate = null;

                if (DateTime.TryParse(r["CR_REQDATE"]?.ToString(), out DateTime cr))
                    crReqDate = cr;

                if (DateTime.TryParse(r["LAB_REQUESTED_DATE"]?.ToString(), out DateTime lr))
                    labRequestedDate = lr;

                if (DateTime.TryParse(r["PROD_SEND_DATE"]?.ToString(), out DateTime ps))
                    prodSendDate = ps;

                if (DateTime.TryParse(r["LAB_CONFIRMED_DATE"]?.ToString(), out DateTime lc))
                    labConfirmedDate = lc;

                bool isDelay = false;

                if (crReqDate.HasValue)
                {
                    Func<DateTime?, bool> checkDelay = (date) =>
                    {
                        if (!date.HasValue) return true;
                        return (crReqDate.Value - date.Value).TotalDays > 9;
                    };

                    if (checkDelay(labRequestedDate) ||
                        checkDelay(prodSendDate) ||
                        checkDelay(labConfirmedDate))
                    {
                        isDelay = true;
                    }
                }

                foreach (DataColumn c in dt.Columns)
                {
                    string val = r[c] == DBNull.Value ? "" : r[c].ToString();

                    string style = "padding:8px;border:1px solid #9CC2E5;text-align:center;white-space:nowrap;";

                    if (c.ColumnName.Equals("LAB_REQUESTED_DATE", StringComparison.OrdinalIgnoreCase) ||
                        c.ColumnName.Equals("PROD_SEND_DATE", StringComparison.OrdinalIgnoreCase) ||
                        c.ColumnName.Equals("LAB_CONFIRMED_DATE", StringComparison.OrdinalIgnoreCase))
                    {
                        if (isDelay || string.IsNullOrWhiteSpace(val))
                            style += "color:#C00000;font-weight:bold;";
                        else
                            style += "color:#006100;font-weight:bold;";

                        style += $"background:{rowBg};";
                    }

                    sb.Append($"<td style='{style}'>{System.Net.WebUtility.HtmlEncode(val)}</td>");
                }

                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");

            // =================== PSDD STATUS ===================
            // Uses JavaScript to match exact table width after render
            sb.Append(@"
    <div id='psddBlock' style='margin-top:20px; width:100%;'>

        <div style='font-size:13px;
                    margin-bottom:8px;
                    font-weight:bold;'>
           Colour Code
        </div>

        <div style='display:flex; width:100%; gap:15px;'>

            <div style='flex:1;padding:10px;text-align:center;
                        background:#C6EFCE;color:#006100;
                        font-size:12px;border:1px solid #ddd;border-radius:4px;'>
                On Time (9 Days Before CRD)
            </div>

            <div style='flex:1;padding:10px;text-align:center;
                        background:#FFC7CE;color:#C00000;
                        font-size:12px;border:1px solid #ddd;border-radius:4px;'>
                Delayed (&lt; 9 Days Before CRD)
            </div>

        </div>
    </div>

    <script>
        (function() {
            var table = document.getElementById('mainTable');
            var psdd  = document.getElementById('psddBlock');
            if (table && psdd) {
                psdd.style.width = table.offsetWidth + 'px';
            }
        })();
    </script>");

            sb.Append("</div>");
            sb.Append("</body></html>");

            return sb.ToString();
        }

        public List<string> SendFGTReportAsimage(string FileName, string msg, string htmldata)
        {
            List<string> responseMessages = new List<string>();

            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";

            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "120363407101751518@g.us" },//120363407101751518@g.us(FGT Digitalization)//120363347683285873@g.us(test)
                textMsg = msg,
                htmL_Code = htmldata,
                fileName = FileName
            };

            //var payload = new
            //{
            //    tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
            //    numbers = new[] { "9640416084" }, // Use the fetched phone number
            //    groups = new List<string>(),
            //    textMsg = msg,
            //    htmL_Code = htmldata,
            //    fileName = FileName
            //};

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jsonPayload = JsonConvert.SerializeObject(payload);
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = response.Content.ReadAsStringAsync().Result;
                        responseMessages.Add(responseData);
                        FMSLOG.Platform(responseData, "AQLInspection_Report");
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        FMSLOG.Platform(response.Content.ReadAsStringAsync().Result, "AQLInspection_Report");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, "AQLInspection_Report");
            }

            return responseMessages;
        }

        public static string FGT_Status_Mail(DataTable dt, string heading)
        {
            if (dt == null || dt.Rows.Count == 0)
                return "<div><h3>No FGT_Digitalization_Report available</h3></div>";

            var sb = new StringBuilder();

            // ===== Times New Roman font, no background outside table =====
            sb.Append("<html><body style='font-family:Times New Roman, Times, serif; background:none; margin:0; padding:0;'>");

            // =================== TOP MESSAGE ===================
            sb.Append(@"
<div style='
    margin:20px;
    padding:15px 20px;
    background:none;
    font-size:18px;
    color:#000;
    line-height:1.6;
    font-family:Times New Roman, Times, serif;
'>
    <div style='margin-bottom:8px;'>
        Dear All,
    </div>

    <div>
       Please review the FGT status for the current month and ensure the Production team submits the required FGT shoes to the Lab on time.
       The Lab team is requested to confirm receipt upon submission and proceed with necessary actions accordingly.
    </div>
</div>");

            // =================== WRAPPER ===================
            sb.Append(@"
<div style='margin:0 20px;'>
    <table id='mainTable' style='border-collapse:collapse;
                  width:100%;
                  background:#ffffff;
                  font-size:12.5px;
                  font-family:Times New Roman, Times, serif;
                  border-radius:6px;
                  overflow:hidden;
                  box-shadow:0 3px 10px rgba(0,0,0,0.18);'>");

            // =================== THEAD ===================
            sb.Append("<thead>");

            // ===== TOP HEADING ROW =====
            sb.Append($@"
<tr>
    <th colspan='{dt.Columns.Count}'
        style='background:#1F4E79;
               color:#ffffff;
               text-align:center;
               padding:14px;
               font-size:16px;
               font-weight:bold;
               font-family:Times New Roman, Times, serif;
               border:1px solid #1F4E79;'>
        {heading}
    </th>
</tr>");

            // ===== COLUMN HEADER =====
            sb.Append("<tr style='background:#4F81BD;color:#ffffff;'>");

            foreach (DataColumn c in dt.Columns)
            {
                sb.Append($@"
    <th style='padding:9px;
               border:1px solid #9CC2E5;
               text-align:center;
               font-weight:bold;
               font-family:Times New Roman, Times, serif;
               white-space:nowrap;'>
        {System.Net.WebUtility.HtmlEncode(c.ColumnName)}
    </th>");
            }

            sb.Append("</tr></thead><tbody>");

            // =================== BODY ===================
            int idx = 0;

            foreach (DataRow r in dt.Rows)
            {
                string rowBg = (idx++ % 2 == 0) ? "#FFFFFF" : "#E9F1FB";
                sb.Append($"<tr style='background:{rowBg};'>");

                DateTime? crReqDate = null;
                DateTime? labRequestedDate = null;
                DateTime? prodSendDate = null;
                DateTime? labConfirmedDate = null;

                if (DateTime.TryParse(r["CR_REQDATE"]?.ToString(), out DateTime cr)) crReqDate = cr;
                if (DateTime.TryParse(r["LAB_REQUESTED_DATE"]?.ToString(), out DateTime lr)) labRequestedDate = lr;
                if (DateTime.TryParse(r["PROD_SEND_DATE"]?.ToString(), out DateTime ps)) prodSendDate = ps;
                if (DateTime.TryParse(r["LAB_CONFIRMED_DATE"]?.ToString(), out DateTime lc)) labConfirmedDate = lc;

                bool isDelay = false;

                if (crReqDate.HasValue)
                {
                    Func<DateTime?, bool> checkDelay = (date) =>
                    {
                        if (!date.HasValue) return true;
                        return (crReqDate.Value - date.Value).TotalDays > 9;
                    };

                    if (checkDelay(labRequestedDate) ||
                        checkDelay(prodSendDate) ||
                        checkDelay(labConfirmedDate))
                    {
                        isDelay = true;
                    }
                }

                foreach (DataColumn c in dt.Columns)
                {
                    string val = r[c] == DBNull.Value ? "" : r[c].ToString();
                    string style = "padding:8px;border:1px solid #9CC2E5;text-align:center;white-space:nowrap;font-family:Times New Roman, Times, serif;";

                    if (c.ColumnName.Equals("LAB_REQUESTED_DATE", StringComparison.OrdinalIgnoreCase) ||
                        c.ColumnName.Equals("PROD_SEND_DATE", StringComparison.OrdinalIgnoreCase) ||
                        c.ColumnName.Equals("LAB_CONFIRMED_DATE", StringComparison.OrdinalIgnoreCase))
                    {
                        style += (isDelay || string.IsNullOrWhiteSpace(val))
                            ? "color:#C00000;font-weight:bold;"
                            : "color:#006100;font-weight:bold;";

                        style += $"background:{rowBg};";
                    }

                    sb.Append($"<td style='{style}'>{System.Net.WebUtility.HtmlEncode(val)}</td>");
                }

                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");

            // =================== PSDD STATUS ===================
            sb.Append(@"
<div id='psddBlock' style='margin-top:20px; width:100%;'>

    <div style='font-size:20px;
                margin-bottom:8px;
                font-weight:bold;
                font-family:Times New Roman, Times, serif;'>
        Colour Code
    </div>

    <table style='width:100%; border-collapse:separate; border-spacing:15px 0;'>
        <tr>
            <td style='width:50%;
                       padding:10px;
                       text-align:center;
                       background:#C6EFCE;
                       color:#006100;
                       font-size:12px;
                       border:1px solid #ddd;
                       border-radius:4px;
                       font-family:Times New Roman, Times, serif;'>
                On Time (9 Days Before CRD)
            </td>
            <td style='width:50%;
                       padding:10px;
                       text-align:center;
                       background:#FFC7CE;
                       color:#C00000;
                       font-size:12px;
                       border:1px solid #ddd;
                       border-radius:4px;
                       font-family:Times New Roman, Times, serif;'>
                Delayed (&lt; 9 Days Before CRD)
            </td>
        </tr>
    </table>

</div>

<script>
    (function() {
        var table = document.getElementById('mainTable');
        var psdd  = document.getElementById('psddBlock');
        if (table && psdd) {
            psdd.style.width = table.offsetWidth + 'px';
        }
    })();
</script>");

            sb.Append("</div>"); // close margin wrapper

            // =================== FOOTER MESSAGE ===================
            sb.Append(@"
<div style='padding:20px;
            margin-top:30px;
            font-size:20px;
            font-family:Times New Roman, Times, serif;
            background:none;'>
    <b>Regards,<br>
    APC-IT Team</b>
</div>");

            sb.Append("</body></html>");

            return sb.ToString();
        }

        public void SendFGTReportAsMail(string Msg)
        {
            string[] listFomat = new string[0];
            string[] attachList = new string[0];
            string Error_msg = string.Empty;
            string MailBody = Msg.ToString();

            DataTable dt2 = new DataTable();
            //string sql2 = $@"select TO_LIST,CC_LIST,ERROR_LIST,MAIL_SUBJECT from tbl_e2e_mail_config where DEPT='TEST'";
            string sql2 = $@"select TO_LIST, CC_LIST, ERROR_LIST, MAIL_SUBJECT from tbl_e2e_mail_config where DEPT = 'QIP' and ROLECODE = 'FGT'";
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
                if (MailUtil.SendMessage(listSend, listCopy, mailSubject, MailBody, attachList, out Error_msg))
                {
                }
                else
                {
                    MailUtil.SendMessage(listError, listError, mailSubject, Error_msg + "\n" + DateTime.Now, null, out Error_msg);
                }


            }

        }

        #endregion

    }
}
