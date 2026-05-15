using Compal.FMS.Connections.DBLoader;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Oracle.ManagedDataAccess.Client;
using Compal.FMS.Kernel.Beans;
using System.Reflection;
using FMSCommon.Compal.FMS.Kernel.Utils;
using System.Net;
using Newtonsoft.Json;
using Compal.FMS.Kernel.Operations;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Mail;
using ClosedXML.Excel;
using System.Linq;

namespace FMSCommon.Compal.FMS.Kernel.Operations
{
    public class Run_AEQS_Pivot88_Operations
    {
        OracleConnection conmes = null;
        string returnMsg = string.Empty;
        string Sync_StatusCode = string.Empty;
        string Sync_Message = string.Empty;

        string Json_CLOB = string.Empty;
        //string timezone = "+0800";//HongKong :GMT(+8)
        string timezone = "+0530";//India :GMT(+5:30)
        double requestTimeout = 300; //request timeout 300sec

        //正式环境
        #region AQL Outbound
        public async void PostRequestAsync_AQL(SrvInfo vsrvinfo)
        {
            try
            {
                string constrmes = null;
                string filePath;

                filePath = Application.ExecutablePath;
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                string clientEnvConfigFileName = filePath + "database.config";


                if (File.Exists(clientEnvConfigFileName))
                {
                    FileLoader obj = new FileLoader(clientEnvConfigFileName);
                    Hashtable htdblinks = obj.GetDBLinks();
                    if (htdblinks.ContainsKey(vsrvinfo.SDB))
                        constrmes = htdblinks[vsrvinfo.SDB].ToString();
                    conmes = new OracleConnection(constrmes);
                    int updatecount = 0;
                    int errorcount = 0;
                    FMSLOG.Platform("Data Post Started " + vsrvinfo.StartDate, vsrvinfo.Operation);
                    conmes.Open();

                    await PUTAsync_AQL(vsrvinfo);

                    FMSLOG.Platform("Data Send Status : " + returnMsg + "", vsrvinfo.Operation);
                }
                else
                {
                    FMSLOG.Platform("No Databases Exists..", vsrvinfo.Operation);
                }
            }
            catch (Exception ex)
            {
                FMSLOG.Platform(MethodBase.GetCurrentMethod().Name + " Exception : " + ex.Message, vsrvinfo.Operation);
            }
            finally
            {
                conmes.Close();
                conmes.Dispose();

                GC.Collect();
            }
        }

        public async Task PUTAsync_AQL(SrvInfo vsrvinfo)
        {
            string UniqueKey = string.Empty;
            var Error_Json = string.Empty;
            var PostJson = string.Empty;

            DataTable dtlist = null;
            //string timezone = "+0800";//HongKong :GMT(+8)
            string timezone = "+0530";//HongKong :GMT(+8)
            OracleCommand cmd = null;

            try
            {
                string sql = $@"select 
                                UNIQUE_KEY
                                ,l.STATUS
                                , l.DATE_STARTED
                                , l.DEFECTIVE_PARTS
                                , l.PASSFAILS_0_TITLE
                                , l.PASSFAILS_0_TYPE
                                , l.PASSFAILS_0_SUBSECTION
                                , l.PASSFAILS_0_LISTVALUES_VALUE from t_aeqs_to_p88_list l left join (select distinct union_id,sections_defects_critical_level from t_aeqs_to_p88_sections) s on l.unique_key = s.union_id 
                                where 
                                s.sections_defects_critical_level=0
                                and l.assignment_items_assignment_report_type_id=9
                                and ( IS_SYNC IS NULL 
                                or IS_SYNC='N' 
                                )
--and UNIQUE_KEY in ('apache1_2923') 
                                --and AEQS_INSERT_DATE < to_date('2024/04/26 23:59:59','yyyy/mm/dd hh24:mi:ss') 
                                --and to_char(l.aeqs_insert_date,'yyyy/mm/dd') < to_char(sysdate-1,'yyyy/mm/dd') 
                                ";



                cmd = new OracleCommand(sql, conmes);
                cmd.CommandType = CommandType.Text;
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                dtlist = new DataTable();
                da.Fill(dtlist);

            }
            catch (Exception ex)
            {
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, vsrvinfo.Operation);
                returnMsg = "Fail" + ex.ToString();
            }




            if (dtlist.Rows.Count > 0)
            {
                foreach (DataRow item in dtlist.Rows)
                {
                    try
                    {
                        UniqueKey = item["UNIQUE_KEY"].ToString();
                        DataTable dtsections_f = new DataTable();
                        string sql1 = $@"select ID
,UNION_ID
,SECTIONS_TYPE
,SECTIONS_TITLE
,SECTIONS_RESULT_ID
,SECTIONS_QTY_INSPECTED
,SECTIONS_SAMPLED_INSPECTED
,SECTIONS_DEFECTIVE_PARTS
,SECTIONS_INSPECTION_LEVEL
,SECTIONS_INSPECTION_METHOD
,SECTIONS_AQL_MINOR
,SECTIONS_AQL_MAJOR
,SECTIONS_AQL_CRITICAL
,SECTIONS_BARCODES_VALUE
,SECTIONS_QTY_TYPE
,SECTIONS_MAX_MINOR_DEFECTS
,SECTIONS_MAX_MAJOR_DEFECTS
,SECTIONS_MAX_MAJOR_A_DEFECTS
,SECTIONS_MAX_MAJOR_B_DEFECTS
,SECTIONS_MAX_CRITICAL_DEFECTS
,SECTIONS_DEFECTS_LABEL
,SECTIONS_DEFECTS_SUBSECTION
,SECTIONS_DEFECTS_CODE
,SECTIONS_DEFECTS_CRITICAL_LEVEL
,SECTIONS_DEFECTS_MAJOR_LEVEL
,SECTIONS_DEFECTS_MINOR_LEVEL
,SECTIONS_DEFECTS_COMMENTS 
from t_aeqs_to_p88_sections where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd1 = new OracleCommand(sql1, conmes);
                        cmd1.CommandType = CommandType.Text;
                        OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
                        DataTable dtsections = new DataTable();
                        da1.Fill(dtsections);

                        DataRow[] dr_pl = null;
                        DataRow[] dr_p = null;
                        dr_pl = dtsections.Select($"SECTIONS_TITLE='packing_packaging_labelling'");
                        dr_p = dtsections.Select($"SECTIONS_TITLE='product'");
                        List<string> defects_pl = new List<string>();
                        List<string> defects_p = new List<string>();





                        foreach (var pl in dr_pl)
                        {
                            List<string> defects_pl_pic = new List<string>();
                            DataTable dtsections_f_pl_pic = new DataTable();
                            string sqlpic = $@"select ID
,UNION_ID
,SECTIONS_DEFECTS_PICTURES_TITLE
,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
,SECTIONS_DEFECTS_PICTURES_NUMBER
,SECTIONS_DEFECTS_PICTURES_COMMENT
,SECTION_TYPE
,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{pl["ID"]}'";
                            OracleCommand cmdpic = new OracleCommand(sqlpic, conmes);
                            cmdpic.CommandType = CommandType.Text;
                            OracleDataAdapter dapic = new OracleDataAdapter(cmdpic);

                            dapic.Fill(dtsections_f_pl_pic);

                            foreach (DataRow itemsections_f_pl_pic in dtsections_f_pl_pic.Rows)
                            {
                                string full_filename = GetImageData(itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                                defects_pl_pic.Add("{\"title\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\": \"" + full_filename + "\",\"number\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");

                            }


                            defects_pl.Add("{\"label\":\"" + pl["SECTIONS_DEFECTS_LABEL"] + "\"," +
                            "\"subsection\":\"" + pl["SECTIONS_DEFECTS_SUBSECTION"] + "\",\"code\":\"" + pl["SECTIONS_DEFECTS_CODE"] + "\",\"critical_level\":\"" + pl["SECTIONS_DEFECTS_CRITICAL_LEVEL"] + "\",\"major_level\":\"" + pl["SECTIONS_DEFECTS_MAJOR_LEVEL"] + "\",\"minor_level\":\"" + pl["SECTIONS_DEFECTS_MINOR_LEVEL"] + "\"," +
                            "\"comments\":\"" + pl["SECTIONS_DEFECTS_COMMENTS"] + "\",\"pictures\":[" + string.Join(",", defects_pl_pic) + "]}");
                        }
                        foreach (var p in dr_p)
                        {
                            List<string> defects_p_pic = new List<string>();
                            DataTable dtsections_f_p_pic = new DataTable();
                            string sqlpic = $@"select UNION_ID
,SECTIONS_DEFECTS_PICTURES_TITLE
,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
,SECTIONS_DEFECTS_PICTURES_NUMBER
,SECTIONS_DEFECTS_PICTURES_COMMENT
,SECTION_TYPE
,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{p["ID"]}'";
                            OracleCommand cmdpic = new OracleCommand(sqlpic, conmes);
                            cmdpic.CommandType = CommandType.Text;
                            OracleDataAdapter dapic = new OracleDataAdapter(cmdpic);

                            dapic.Fill(dtsections_f_p_pic);

                            foreach (DataRow itemsections_f_pl_pic in dtsections_f_p_pic.Rows)
                            {
                                string full_filename = GetImageData(itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                                defects_p_pic.Add("{\"title\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\": \"" + full_filename + "\",\"number\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");

                            }


                            defects_p.Add("{\"label\":\"" + p["SECTIONS_DEFECTS_LABEL"] + "\"," +
                            "\"subsection\":\"" + p["SECTIONS_DEFECTS_SUBSECTION"] + "\",\"code\":\"" + p["SECTIONS_DEFECTS_CODE"] + "\",\"critical_level\":\"" + p["SECTIONS_DEFECTS_CRITICAL_LEVEL"] + "\",\"major_level\":\"" + p["SECTIONS_DEFECTS_MAJOR_LEVEL"] + "\",\"minor_level\":\"" + p["SECTIONS_DEFECTS_MINOR_LEVEL"] + "\"," +
                            "\"comments\":\"" + p["SECTIONS_DEFECTS_COMMENTS"] + "\",\"pictures\":[" + string.Join(",", defects_p_pic) + "]}");
                        }

                        List<string> lstsections = new List<string>();


                        lstsections.Add("{\"type\":\"" + dr_pl[0]["SECTIONS_TYPE"] + "\",\"title\":\"" + dr_pl[0]["SECTIONS_TITLE"] + "\",\"section_result_id\":\"" + dr_pl[0]["SECTIONS_RESULT_ID"] + "\",\"qty_inspected\":\"" + dr_pl[0]["SECTIONS_QTY_INSPECTED"] + "\"," +
                    "\"sampled_inspected\":\"" + dr_pl[0]["SECTIONS_SAMPLED_INSPECTED"] + "\",\"defective_parts\":\"" + dr_pl[0]["SECTIONS_DEFECTIVE_PARTS"] + "\",\"inspection_level\":\"" + dr_pl[0]["SECTIONS_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + dr_pl[0]["SECTIONS_INSPECTION_METHOD"] + "\",\"aql_minor\":\"" + dr_pl[0]["SECTIONS_AQL_MINOR"] + "\"," +
                    "\"aql_major\":\"" + dr_pl[0]["SECTIONS_AQL_MAJOR"] + "\",\"aql_critical\":\"" + dr_pl[0]["SECTIONS_AQL_CRITICAL"] + "\",\"barcodes\":[{\"value\":\"001\"}]," +
                    "\"qty_type\":\"" + dr_pl[0]["SECTIONS_QTY_TYPE"] + "\",\"max_minor_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MINOR_DEFECTS"] + "\",\"max_major_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_DEFECTS"] + "\",\"max_major_a_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_A_DEFECTS"] + "\",\"max_major_b_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_B_DEFECTS"] + "\",\"max_critical_defects\":\"" + dr_pl[0]["SECTIONS_MAX_CRITICAL_DEFECTS"] + "\"," +
                    "\"defects\":[" + string.Join(",", defects_pl) + "]}");

                        lstsections.Add("{\"type\":\"" + dr_p[0]["SECTIONS_TYPE"] + "\",\"title\":\"" + dr_p[0]["SECTIONS_TITLE"] + "\",\"section_result_id\":\"" + dr_p[0]["SECTIONS_RESULT_ID"] + "\",\"qty_inspected\":\"" + dr_p[0]["SECTIONS_QTY_INSPECTED"] + "\"," +
                    "\"sampled_inspected\":\"" + dr_p[0]["SECTIONS_SAMPLED_INSPECTED"] + "\",\"defective_parts\":\"" + dr_p[0]["SECTIONS_DEFECTIVE_PARTS"] + "\",\"inspection_level\":\"" + dr_p[0]["SECTIONS_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + dr_p[0]["SECTIONS_INSPECTION_METHOD"] + "\",\"aql_minor\":\"" + dr_p[0]["SECTIONS_AQL_MINOR"] + "\"," +
                    "\"aql_major\":\"" + dr_p[0]["SECTIONS_AQL_MAJOR"] + "\",\"aql_critical\":\"" + dr_p[0]["SECTIONS_AQL_CRITICAL"] + "\"," +
                    "\"qty_type\":\"" + dr_p[0]["SECTIONS_QTY_TYPE"] + "\",\"max_minor_defects\":\"" + dr_p[0]["SECTIONS_MAX_MINOR_DEFECTS"] + "\",\"max_major_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_DEFECTS"] + "\",\"max_major_a_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_A_DEFECTS"] + "\",\"max_major_b_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_B_DEFECTS"] + "\",\"max_critical_defects\":\"" + dr_p[0]["SECTIONS_MAX_CRITICAL_DEFECTS"] + "\"," +
                    "\"defects\":[" + string.Join(",", defects_p) + "]}");



                        string sql2 = $@"select UNION_ID
                        ,SECTIONS_DEFECTS_PICTURES_TITLE
                        ,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
                        ,SECTIONS_DEFECTS_PICTURES_NUMBER
                        ,SECTIONS_DEFECTS_PICTURES_COMMENT
                        ,SECTION_TYPE
                        ,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd2 = new OracleCommand(sql2, conmes);
                        cmd2.CommandType = CommandType.Text;
                        OracleDataAdapter da2 = new OracleDataAdapter(cmd2);

                        da2.Fill(dtsections_f);
                        List<string> lstsections_f = new List<string>();
                        foreach (DataRow itemsections_f in dtsections_f.Rows)
                        {
                            string full_filename = GetImageData(itemsections_f["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                            //lstsections_f.Add("{\"type\":\"" + itemsections_f["SECTION_TYPE"] + "\",\"title\":\"" + itemsections_f["SECTION_TITLE"] + "\",\"pictures\":[{\"title\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\":\"" + full_filename + "\",\"number\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}]}");
                            lstsections_f.Add("{\"title\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\":\"" + full_filename + "\",\"number\":\"\",\"comment\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");
                        }

                        string sql3 = $@"select UNION_ID
,PASSFAILS_TITLE
,PASSFAILS_VALUE
,PASSFAILS_TYPE
,PASSFAILS_SUBSECTION
,PASSFAILS_CHECKLISTSUBSECTION
,PASSFAILS_STATUS
,PASSFAILS_COMMENT
    from t_aeqs_to_p88_passfail where UNION_ID ='{item["UNIQUE_KEY"]}'
                        order by passfails_checklistsubsection";

                        OracleCommand cmd3 = new OracleCommand(sql3, conmes);
                        cmd3.CommandType = CommandType.Text;
                        OracleDataAdapter da3 = new OracleDataAdapter(cmd3);
                        DataTable dtpassfail = new DataTable();
                        da3.Fill(dtpassfail);
                        List<string> lstpassfail = new List<string>();
                        foreach (DataRow itempassfail in dtpassfail.Rows)
                        {
                            lstpassfail.Add("{\"title\":\"" + itempassfail["PASSFAILS_TITLE"] + "\",\"value\":\"" + itempassfail["PASSFAILS_VALUE"] + "\",\"type\":\"" + itempassfail["PASSFAILS_TYPE"] + "\"," +
                        "\"subsection\":\"" + itempassfail["PASSFAILS_SUBSECTION"] + "\",\"checkListSubsection\":\"" + itempassfail["PASSFAILS_CHECKLISTSUBSECTION"] + "\",\"status\":\"" + itempassfail["PASSFAILS_STATUS"] + "\",\"comment\":\"" + itempassfail["PASSFAILS_COMMENT"] + "\"}");
                        }


                        string sql4 = $@"select UNION_ID
                , ASSIGNMENT_ITEMS_SAMPLED_INSPECTED
                , ASSIGNMENT_ITEMS_INSPECTION_RESULT_ID
                , ASSIGNMENT_ITEMS_INSPECTION_STATUS_ID
                , ASSIGNMENT_ITEMS_QTY_INSPECTED
                , ASSIGNMENT_ITEMS_INSPECTION_COMPLETED_DATE
                , ASSIGNMENT_ITEMS_TOTAL_INSPECTION_MINUTES
                , ASSIGNMENT_ITEMS_SAMPLING_SIZE
                , ASSIGNMENT_ITEMS_QTY_TO_INSPECT
                , ASSIGNMENT_ITEMS_AQL_MINOR
                , ASSIGNMENT_ITEMS_AQL_MAJOR
                , ASSIGNMENT_ITEMS_AQL_MAJOR_A
                , ASSIGNMENT_ITEMS_AQL_MAJOR_B
                , ASSIGNMENT_ITEMS_AQL_CRITICAL
                , ASSIGNMENT_ITEMS_SUPPLIER_BOOKING_MSG
                , ASSIGNMENT_ITEMS_CONCLUSION_REMARKS
                , ASSIGNMENT_ITEMS_ASSIGNMENT_REPORT_TYPE_ID
                , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTOR_USERNAME
                , ASSIGNMENT_ITEMS_ASSIGNMENT_DATE_INSPECTION
                , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_LEVEL
                , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_METHOD
                , ASSIGNMENT_ITEMS_PO_LINE_QTY
                , ASSIGNMENT_ITEMS_PO_LINE_ETD
                , ASSIGNMENT_ITEMS_PO_LINE_ETA
                , ASSIGNMENT_ITEMS_PO_LINE_COLOR
                , ASSIGNMENT_ITEMS_PO_LINE_SIZE
                , ASSIGNMENT_ITEMS_PO_LINE_STYLE
                , ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ID
                , ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ERP_BUSINESS_ID
                , ASSIGNMENT_ITEMS_PO_LINE_PO_NUMBER
                , ASSIGNMENT_ITEMS_PO_LINE_CUSTOMER_PO
                , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ID
                , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ERP_BUSINESS_ID
                , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_PROJECT_ID
                , ASSIGNMENT_ITEMS_PO_LINE_SKU_SKU_NUMBER
                , ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_NAME
                , ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_DESCRIPTION
                , PO_LINE_PROJECT_CODE
                , REPORT_TYPE_NAME
    from t_aeqs_to_p88_assignment where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd4 = new OracleCommand(sql4, conmes);
                        cmd4.CommandType = CommandType.Text;
                        OracleDataAdapter da4 = new OracleDataAdapter(cmd4);
                        DataTable dtassignment = new DataTable();
                        da4.Fill(dtassignment);
                        List<string> lstassignment = new List<string>();

                        foreach (DataRow itemassignment in dtassignment.Rows)
                        {
                            //The old data remains unchanged, and the new PO after POChange is transferred to Project Code
                            string id = "\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_PROJECT_ID"] + "\"";
                            string reportTypeKey = "id";
                            string reportTypeValue = itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_REPORT_TYPE_ID"].ToString();
                            if (!string.IsNullOrEmpty(itemassignment["PO_LINE_PROJECT_CODE"].ToString()))
                            {
                                id = "\"project_code\":\"" + itemassignment["PO_LINE_PROJECT_CODE"] + "\"";
                                reportTypeKey = "name";
                                reportTypeValue = itemassignment["REPORT_TYPE_NAME"].ToString();
                            }

                             lstassignment.Add("{\"sampled_inspected\":\"" + itemassignment["ASSIGNMENT_ITEMS_SAMPLED_INSPECTED"] + "\",\"inspection_result_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_INSPECTION_RESULT_ID"] + "\",\"inspection_status_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_INSPECTION_STATUS_ID"] + "\",\"qty_inspected\":\"" + itemassignment["ASSIGNMENT_ITEMS_QTY_INSPECTED"] + "\"," +
                        "\"inspection_completed_date\":\"" + Convert.ToDateTime(itemassignment["ASSIGNMENT_ITEMS_INSPECTION_COMPLETED_DATE"]).ToString("yyyy-MM-ddTHH:mm:ss") + timezone + "\",\"total_inspection_minutes\":\"" + itemassignment["ASSIGNMENT_ITEMS_TOTAL_INSPECTION_MINUTES"] + "\",\"sampling_size\":\"" + itemassignment["ASSIGNMENT_ITEMS_SAMPLING_SIZE"] + "\",\"qty_to_inspect\":\"" + itemassignment["ASSIGNMENT_ITEMS_QTY_TO_INSPECT"] + "\"," +
                        "\"aql_minor\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MINOR"] + "\",\"aql_major\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR"] + "\",\"aql_major_a\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR_A"] + "\",\"aql_major_b\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR_B"] + "\",\"aql_critical\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_CRITICAL"] + "\",\"supplier_booking_msg\":\"" + itemassignment["ASSIGNMENT_ITEMS_SUPPLIER_BOOKING_MSG"] + "\"," +
                        "\"conclusion_remarks\":\"" + itemassignment["ASSIGNMENT_ITEMS_CONCLUSION_REMARKS"] + "\",\"assignment\":{\"report_type\":{\"" + reportTypeKey + "\":\"" + reportTypeValue + "\"},\"inspector\":{\"username\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTOR_USERNAME"] + "\"}," +
                        "\"date_inspection\":\"" + Convert.ToDateTime(itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_DATE_INSPECTION"]).ToString("yyyy-MM-ddTHH:mm:ss") + timezone + "\",\"inspection_level\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_METHOD"] + "\"},\"po_line\":{\"qty\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_QTY"] + "\"," +
                        "\"etd\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_ETD"] + "\",\"eta\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_ETA"] + "\",\"color\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_COLOR"] + "\",\"size\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SIZE"] + "\",\"style\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_STYLE"] + "\",\"po\":{\"exporter\":{\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ID"] + "\"," +
                        "\"erp_business_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ERP_BUSINESS_ID"] + "\"},\"po_number\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_NUMBER"] + "\",\"customer_po\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_CUSTOMER_PO"] + "\",\"importer\":{\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ID"] + "\",\"erp_business_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ERP_BUSINESS_ID"] + "\"}," +
                        "\"project\":{" + id + "}},\"sku\":{\"sku_number\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_SKU_NUMBER"] + "\",\"item_name\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_NAME"] + "\",\"item_description\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_DESCRIPTION"] + "\"}}}");
                        }

                        List<string> lstpassfail_val = new List<string>();
                        string[] arr = item["PASSFAILS_0_LISTVALUES_VALUE"].ToString().Split('/');
                        if (arr.Length > 0)
                        {
                            foreach (var item_val in arr)
                            {
                                lstpassfail_val.Add("{\"value\":\"" + item_val.Trim() + "\"}");
                            }

                        }




                        //string PostJson = "{\"status\":\"" + item["STATUS"] + "\",\"date_started\":\"" + Convert.ToDateTime(item["DATE_STARTED"]).ToString("yyyy-MM-ddTHH:mm:ss") +"\",\"defective_parts\":" + item["DEFECTIVE_PARTS"] + "," +

                        PostJson = "{\"status\":\"" + item["STATUS"] + "\",\"date_started\":\"" + Convert.ToDateTime(item["DATE_STARTED"]).ToString("yyyy-MM-ddTHH:mm:ss") + timezone + "\",\"defective_parts\":" + item["DEFECTIVE_PARTS"] + "," +

//sections 
//"\"sections\":[" + string.Join(",", lstsections) + "," + string.Join(",", lstsections_f) + "]," +
"\"sections\":[" + string.Join(",", lstsections) + ",{\"type\":\"" + dtsections_f.Rows[0]["SECTION_TYPE"] + "\",\"title\":\"" + dtsections_f.Rows[0]["SECTION_TITLE"] + "\",\"pictures\":[" + string.Join(",", lstsections_f) + "]}]," +
                        //"\"sections\":[" + string.Join(",", lstsections) + "]," +

                        //assignment_items
                        "\"assignment_items\":[" + string.Join(",", lstassignment) + "]," +

                        ////passFails


                        "\"passFails\":[{\"title\":\"" + item["PASSFAILS_0_TITLE"] + "\",\"type\":\"" + item["PASSFAILS_0_TYPE"] + "\",\"subsection\":\"" + item["PASSFAILS_0_SUBSECTION"] + "\",\"listValues\":[" + string.Join(",", lstpassfail_val) + "]}," + string.Join(",", lstpassfail) + "]}";




                        #region Retransmission
                        int maxRetries = 5;
                        int currentRetry = 0;

                        bool retryRequest = true;
                        HttpResponseMessage response = null;


                        try
                        {
                            do
                            {
                                var client = new HttpClient();
                                client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                                //Test
                                //var request = new HttpRequestMessage(HttpMethod.Put, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");    // P88 test
                                //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test

                                //offical
                                var request = new HttpRequestMessage(HttpMethod.Put, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");           // P88 Official
                                request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f");    //official

                                // Create and initialize request content
                                var content = new StringContent(PostJson, null, "application/json");
                                 
                                // Set the request content
                                request.Content = content;

                                // send request
                                response = await client.SendAsync(request);

                                // Check the status code, if it is not 500 or 405 or timeout, jump out of the retry loop
                                if (response != null && response.StatusCode != HttpStatusCode.InternalServerError && response.StatusCode != HttpStatusCode.MethodNotAllowed && response.StatusCode != HttpStatusCode.RequestTimeout)
                                {
                                    retryRequest = false;
                                }
                                else
                                {
                                    // If it is 500, wait for a while and try again (for example: linear backoff strategy, doubling the delay time each time)
                                    await Task.Delay(2500); //Assume that the interval between each retry is 2.5 seconds, and the specific delay strategy is adjusted according to the actual situation.
                                    if (currentRetry > 0) FMSLOG.Platform("Retransfer JSON : " + item["UNIQUE_KEY"] + $"...{currentRetry}", vsrvinfo.Operation);
                                    currentRetry++;
                                }

                            } while (retryRequest && currentRetry <= maxRetries);
                        }
                        catch (Exception ex)
                        {
                            FMSLOG.Platform("Error JSON : " + item["UNIQUE_KEY"] + $" Exection:{ex.Message}" + PostJson, vsrvinfo.Operation);
                        }

                        Error_Json = await response.Content.ReadAsStringAsync();
                        Sync_StatusCode = ((int)response.StatusCode).ToString();
                        Sync_Message = response.StatusCode.ToString();
                        #endregion

                        response.EnsureSuccessStatusCode();
                        if (response.IsSuccessStatusCode)
                        {
                            FMSLOG.Platform("Success JSON : " + item["UNIQUE_KEY"] + PostJson, vsrvinfo.Operation);
                            string sql_rtMsg = "UPDATE t_aeqs_to_p88_list SET IS_SYNC='S',SYNC_STATUS_CODE='" + Sync_StatusCode + "',SYNC_DATE=sysdate,SYNC_MESSAGE='" + Sync_Message + "'  WHERE UNIQUE_KEY ='" + UniqueKey + "' ";
                            cmd = new OracleCommand(sql_rtMsg, conmes);
                            cmd.CommandType = CommandType.Text;
                            int r = cmd.ExecuteNonQuery();
                            //await POSTAsync(dtsections_f);
                            await POSTAsync_AQL(UniqueKey, dtsections_f, vsrvinfo.Operation);
                            returnMsg = "Success";
                        }


                    }
                    catch (HttpRequestException ex)
                    {
                        FMSLOG.Platform("Error JSON Pivot88: " + Error_Json, vsrvinfo.Operation);
                        FMSLOG.Platform("Error JSON: " + item["UNIQUE_KEY"] + PostJson, vsrvinfo.Operation);
                        string sql_rtMsg = "UPDATE t_aeqs_to_p88_list SET IS_SYNC='N',SYNC_STATUS_CODE='" + Sync_StatusCode + "',SYNC_DATE=sysdate,SYNC_MESSAGE='" + Sync_Message + "' WHERE UNIQUE_KEY ='" + UniqueKey + "' ";
                        cmd = new OracleCommand(sql_rtMsg, conmes);
                        cmd.CommandType = CommandType.Text;
                        int r = cmd.ExecuteNonQuery();
                        returnMsg = "Fail" + ex.ToString();
                    }

                }
            }
            else
            {
                FMSLOG.Platform("No Data to Post SAP", vsrvinfo.Operation);
            }
        }

        //public async Task POSTAsync(DataTable dt_img)
        public async Task POSTAsync_AQL(string UniqueKey, DataTable dt_img1, string opeartion)
        {
            try
            {
                DataTable dt_img = new DataTable();
                string sql = $@"select UNION_ID
                        ,SECTIONS_DEFECTS_PICTURES_TITLE
                        ,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
                        ,SECTIONS_DEFECTS_PICTURES_NUMBER
                        ,SECTIONS_DEFECTS_PICTURES_COMMENT
                        ,SECTION_TYPE
                        ,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID in (select id from  t_aeqs_to_p88_sections where union_id ='" + UniqueKey + "') ";
                OracleCommand cmd = new OracleCommand(sql, conmes);
                cmd.CommandType = CommandType.Text;
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                da.Fill(dt_img);

                //Problem point pictures
                if (dt_img.Rows.Count > 0)
                {
                    foreach (DataRow item in dt_img.Rows)
                    {
                        string imgpath = string.Empty;
                        string imgfull_filename = string.Empty;
                        string img_SUFFIX = string.Empty;


                        #region Retransmission
                        int maxRetries = 2;//Retransmission times
                        int currentRetry = 0;

                        bool retryRequest = true;
                        HttpResponseMessage response = null;

                        try
                        {
                            do
                            {
                                var client = new HttpClient();
                                client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                                //var request = new HttpRequestMessage(HttpMethod.Post, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload"); // P88 test
                                var request = new HttpRequestMessage(HttpMethod.Post, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload");         // P88 official
                                //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test                                                                                                                                                                           //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test
                                request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f"); //official
                                var content = new MultipartFormDataContent();

                                imgfull_filename = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                                imgpath = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "B");
                                img_SUFFIX = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "C");


                                var webC = new System.Net.WebClient();
                                string url = imgpath.Replace("\\", "/");
                                Image image = new Bitmap(webC.OpenRead(url));
                                MemoryStream stream = new MemoryStream();
                                if (img_SUFFIX == "png") { image.Save(stream, ImageFormat.Png); } else if (img_SUFFIX == "jpg") { image.Save(stream, ImageFormat.Jpeg); }
                                image.Save(stream, ImageFormat.Png);
                                stream.Seek(0, SeekOrigin.Begin); //Need to reset position to 0

                                content.Add(new StreamContent(stream), "file", imgfull_filename);

                                // Set the request content
                                request.Content = content;

                                // send request
                                response = await client.SendAsync(request);

                                // Check the status code, if it is not 500 or 405 or timeout, jump out of the retry loop
                                if (response != null && response.StatusCode != HttpStatusCode.InternalServerError && response.StatusCode != HttpStatusCode.MethodNotAllowed && response.StatusCode != HttpStatusCode.RequestTimeout)
                                {
                                    retryRequest = false;
                                }
                                else
                                {
                                    // If it is 500, wait for a while and try again (for example: linear backoff strategy, doubling the delay time each time)
                                    await Task.Delay(2500); // Assume that the interval between each retry is 2.5 seconds, and the specific delay strategy is adjusted according to the actual situation.
                                    if (currentRetry > 0) FMSLOG.Platform("Retransfer IMG- : " + UniqueKey + "---" + imgfull_filename + $"...{currentRetry}", opeartion);
                                    currentRetry++;
                                }
                            } while (retryRequest && currentRetry <= maxRetries);
                        }
                        catch (Exception ex)
                        {
                            FMSLOG.Platform("Error Upload Img : " + UniqueKey + "---" + imgfull_filename + $" Exection:{ex.Message}", opeartion);
                        }

                        Sync_StatusCode = ((int)response.StatusCode).ToString();
                        Sync_Message = response.StatusCode.ToString();
                        #endregion

                        var j = await response.Content.ReadAsStringAsync();
                        response.EnsureSuccessStatusCode();

                    }
                }

                //Packaging, product images
                if (dt_img1.Rows.Count > 0)
                {
                    foreach (DataRow item in dt_img1.Rows)
                    {
                        string imgpath = string.Empty;
                        string imgfull_filename = string.Empty;
                        string img_SUFFIX = string.Empty;

                        #region Retransmission
                        int maxRetries = 2;//Retransmission times
                        int currentRetry = 0;

                        bool retryRequest = true;
                        HttpResponseMessage response = null;


                        try
                        {
                            do
                            {
                                var client = new HttpClient();
                                client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                                //var request = new HttpRequestMessage(HttpMethod.Post, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload"); // P88 test
                                var request = new HttpRequestMessage(HttpMethod.Post, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload");         // P88 official
                                //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test                                                                                                                                                                                //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test
                                request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f"); //official
                                var content = new MultipartFormDataContent();

                                imgfull_filename = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                                imgpath = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "B");
                                img_SUFFIX = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "C");

                                #region Commented by Ashok on 2026/04/01 to Compress the Photo size
                                //var webC = new System.Net.WebClient();
                                //string url = imgpath.Replace("\\", "/");
                                //Image image = new Bitmap(webC.OpenRead(url));
                                //MemoryStream stream = new MemoryStream();
                                //if (img_SUFFIX == "png") { image.Save(stream, ImageFormat.Png); } else if (img_SUFFIX == "jpg") { image.Save(stream, ImageFormat.Jpeg); }
                                //image.Save(stream, ImageFormat.Png);
                                //stream.Seek(0, SeekOrigin.Begin); //Need to reset position to 0

                                //content.Add(new StreamContent(stream), "file", imgfull_filename);

                                //// Set the request content
                                //request.Content = content;

                                #endregion

                                string url = imgpath.Replace("\\", "/");
                                var webC = new System.Net.WebClient();
                                using (var webStream = webC.OpenRead(url))
                                using (var image = Image.FromStream(webStream))
                                {
                                    MemoryStream stream = new MemoryStream();

                                    if (img_SUFFIX.ToLower() == "jpg" || img_SUFFIX.ToLower() == "jpeg")
                                    {
                                        // Apply JPEG compression
                                        var jpgEncoder = ImageCodecInfo.GetImageDecoders()
                                            .First(c => c.FormatID == ImageFormat.Jpeg.Guid);

                                        var encoder = System.Drawing.Imaging.Encoder.Quality;
                                        var encoderParams = new EncoderParameters(1);
                                        encoderParams.Param[0] = new EncoderParameter(encoder, 75L); // adjust quality

                                        image.Save(stream, jpgEncoder, encoderParams);
                                    }
                                    else if (img_SUFFIX.ToLower() == "png")
                                    {
                                        image.Save(stream, ImageFormat.Png);
                                    }

                                    stream.Position = 0;

                                    content.Add(new StreamContent(stream), "file", imgfull_filename);
                                }

                                // Set the request content
                                request.Content = content;

                                // send request
                                response = await client.SendAsync(request);

                                // Check the status code, if it is not 500 or 405 or timeout, jump out of the retry loop
                                if (response != null && response.StatusCode != HttpStatusCode.InternalServerError && response.StatusCode != HttpStatusCode.MethodNotAllowed && response.StatusCode != HttpStatusCode.RequestTimeout)
                                {
                                    retryRequest = false;
                                }
                                else
                                {
                                    // If it is 500, wait for a while and try again (for example: linear backoff strategy, doubling the delay time each time)
                                    await Task.Delay(2500); // Assume that the interval between each retry is 2.5 seconds, and the specific delay strategy is adjusted according to the actual situation.
                                    if (currentRetry > 0) FMSLOG.Platform("Retransfer IMG- : " + UniqueKey + "---" + imgfull_filename + $"...{currentRetry}", opeartion);
                                    currentRetry++;
                                }

                            } while (retryRequest && currentRetry <= maxRetries);
                        }
                        catch (Exception ex)
                        {
                            FMSLOG.Platform("Error Upload Img : " + UniqueKey + "---" + imgfull_filename + $" Exection:{ex.Message}", opeartion);
                        }

                        Sync_StatusCode = ((int)response.StatusCode).ToString();
                        Sync_Message = response.StatusCode.ToString();
                        #endregion

                        var j = await response.Content.ReadAsStringAsync();
                        response.EnsureSuccessStatusCode();

                    }
                }

            }
            catch (Exception ex)
            {
                //if (!response.IsSuccessStatusCode)
                //{
                //    var errorContent = await response.Content.ReadAsStringAsync();
                //    Console.WriteLine("Status Code: " + response.StatusCode);
                //    Console.WriteLine("Error: " + errorContent);
                //}
                //MessageBox.Show("Report Image Upload Exception："+ex.ToString());
                throw;
            }
        }

        #endregion


        #region Inline (RQC)
        //Cls_Return rt = new Cls_Return();
        public async void PostRequestAsync_Inline(SrvInfo vsrvinfo)
        {
            try
            {
                string constrmes = null;
                string filePath;

                filePath = Application.ExecutablePath;
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                string clientEnvConfigFileName = filePath + "database.config";


                if (File.Exists(clientEnvConfigFileName))
                {
                    FileLoader obj = new FileLoader(clientEnvConfigFileName);
                    Hashtable htdblinks = obj.GetDBLinks();
                    if (htdblinks.ContainsKey(vsrvinfo.SDB))
                        constrmes = htdblinks[vsrvinfo.SDB].ToString();

                    conmes = new OracleConnection(constrmes);
                    //OracleTransaction objTrans = null;

                    int updatecount = 0;
                    int errorcount = 0;
                    //await PushNotificationTestAsync();
                    //string errmsg = "";
                    //string sql = "select voucher_no,MAX(CREATE_DATE),SYNC_TIMES from wms_to_sap_list where (is_sync='N' or is_sync is null) and SYNC_TIMES < 3 and  INTERFACE_NO='" + interface_no + "' group by voucher_no,SYNC_TIMES";
                    //OracleCommand cmd = new OracleCommand(sql, conmes);
                    //cmd.CommandType = CommandType.Text;
                    //OracleDataAdapter da = new OracleDataAdapter(cmd);
                    //DataTable dtcheck = new DataTable();
                    //da.Fill(dtcheck);

                    //if (dtcheck.Rows.Count > 0)
                    //{
                    //FMSLOG.Platform("Data Retrived Successfull..(" + dtcheck.Rows.Count + ")Records on " + vsrvinfo.StartDate, vsrvinfo.Operation);
                    FMSLOG.Platform("Data Post Started " + vsrvinfo.StartDate, vsrvinfo.Operation);

                    conmes.Open();
                    //objTrans = conoa.BeginTransaction();

                    await PUTAsync_Inline(vsrvinfo);

                    //System.Threading.Thread.Sleep(30000);
                    //objTrans.Commit();

                    FMSLOG.Platform("Data Send Status : " + returnMsg + "", vsrvinfo.Operation);

                    //}
                    //else
                    //{
                    //    FMSLOG.Platform("No Data to Post SAP", vsrvinfo.Operation);
                    //}
                }
                else
                {
                    FMSLOG.Platform("No Databases Exists..", vsrvinfo.Operation);
                }
            }
            catch (Exception ex)
            {
                //string sql = "UPDATE  wms_to_sap_list SET SYNC_DATE=to_date('" + inp.synctime + "','yyyymmdd HH24:MI:SS'),SYNC_TIMES=nvl(sync_times,0)+1   WHERE UUID='" + inp.UUID + "' ";
                //OracleCommand cmd = new OracleCommand(sql, conmes);
                //cmd.CommandType = CommandType.Text;
                //int r = cmd.ExecuteNonQuery();
                FMSLOG.Platform(MethodBase.GetCurrentMethod().Name + " Exception : " + ex.Message, vsrvinfo.Operation);
            }
            finally
            {
                conmes.Close();
                conmes.Dispose();

                GC.Collect();
            }
        }

        public async Task PUTAsync_Inline(SrvInfo vsrvinfo)
        {
            string UniqueKey = string.Empty;
            var Error_Json = string.Empty;
            DataTable dtlist = null;
            //OracleCommand cmd = null;
            try
            {
                string sql = $@"select 
                        UNIQUE_KEY
                        ,l.STATUS
                        , l.DATE_STARTED
                        , l.DEFECTIVE_PARTS
                        , l.PASSFAILS_0_TITLE
                        , l.PASSFAILS_0_TYPE
                        , l.PASSFAILS_0_SUBSECTION
                        , l.PASSFAILS_0_LISTVALUES_VALUE
                        ,l.assignment_items_fields_string_12
                        from t_aeqs_to_p88_list l 
                        where 
                        --to_char(l.date_started,'yyyy/mm/dd')= to_char(sysdate,'yyyy/mm/dd') and 
                        l.assignment_items_assignment_report_type_id=27
--and  l.UNIQUE_KEY in ('apache1_19741')
                        --and AEQS_INSERT_DATE < to_date('2024/04/28 23:59:59','yyyy/mm/dd hh24:mi:ss') 

                        and (IS_SYNC IS NULL 
                        or IS_SYNC='N'
                        ) 
                        order by l.UNIQUE_KEY
                        ";



                OracleCommand cmd = new OracleCommand(sql, conmes);
                cmd.CommandType = CommandType.Text;
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                dtlist = new DataTable();
                da.Fill(dtlist);
            }
            catch (Exception ex)
            {
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, vsrvinfo.Operation);
                returnMsg = "Fail" + ex.ToString();
            }


            if (dtlist.Rows.Count > 0)
            {
                foreach (DataRow item in dtlist.Rows)
                {
                    try
                    {
                        UniqueKey = item["UNIQUE_KEY"].ToString();
                        DataTable dtsections_f = new DataTable();



                        string sql1 = $@"select ID
    ,UNION_ID
    ,SECTIONS_TYPE
    ,SECTIONS_TITLE
    ,SECTIONS_RESULT_ID
    ,SECTIONS_QTY_INSPECTED
    ,SECTIONS_SAMPLED_INSPECTED
    ,SECTIONS_DEFECTIVE_PARTS
    ,SECTIONS_INSPECTION_LEVEL
    ,SECTIONS_INSPECTION_METHOD
    ,SECTIONS_AQL_MINOR
    ,SECTIONS_AQL_MAJOR
    ,SECTIONS_AQL_CRITICAL
    ,SECTIONS_BARCODES_VALUE
    ,SECTIONS_QTY_TYPE
    ,SECTIONS_MAX_MINOR_DEFECTS
    ,SECTIONS_MAX_MAJOR_DEFECTS
    ,SECTIONS_MAX_MAJOR_A_DEFECTS
    ,SECTIONS_MAX_MAJOR_B_DEFECTS
    ,SECTIONS_MAX_CRITICAL_DEFECTS
    ,SECTIONS_DEFECTS_LABEL
    ,SECTIONS_DEFECTS_SUBSECTION
    ,SECTIONS_DEFECTS_CODE
    ,SECTIONS_DEFECTS_CRITICAL_LEVEL
    ,SECTIONS_DEFECTS_MAJOR_LEVEL
    ,SECTIONS_DEFECTS_MINOR_LEVEL
    ,SECTIONS_DEFECTS_COMMENTS from t_aeqs_to_p88_sections where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd1 = new OracleCommand(sql1, conmes);
                        cmd1.CommandType = CommandType.Text;
                        OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
                        DataTable dtsections = new DataTable();
                        da1.Fill(dtsections);

                        DataRow[] dr_pl = null;
                        DataRow[] dr_p = null;
                        dr_pl = dtsections.Select($"SECTIONS_TITLE='packing_packaging_labelling'");//包装
                        dr_p = dtsections.Select($"SECTIONS_TITLE='product'");//产品
                        List<string> defects_pl = new List<string>();
                        List<string> defects_p = new List<string>();





                        foreach (var pl in dr_pl)
                        {
                            List<string> defects_pl_pic = new List<string>();
                            DataTable dtsections_f_pl_pic = new DataTable();
                            string sqlpic = $@"select ID
    ,UNION_ID
    ,SECTIONS_DEFECTS_PICTURES_TITLE
    ,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
    ,SECTIONS_DEFECTS_PICTURES_NUMBER
    ,SECTIONS_DEFECTS_PICTURES_COMMENT
    ,SECTION_TYPE
    ,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{pl["ID"]}'";
                            OracleCommand cmdpic = new OracleCommand(sqlpic, conmes);
                            cmdpic.CommandType = CommandType.Text;
                            OracleDataAdapter dapic = new OracleDataAdapter(cmdpic);

                            dapic.Fill(dtsections_f_pl_pic);

                            foreach (DataRow itemsections_f_pl_pic in dtsections_f_pl_pic.Rows)
                            {
                                string full_filename = GetImageData(itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                                defects_pl_pic.Add("{\"title\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\": \"" + full_filename + "\",\"number\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");

                            }


                            defects_pl.Add("{\"label\":\"" + pl["SECTIONS_DEFECTS_LABEL"] + "\"," +
                            "\"subsection\":\"" + pl["SECTIONS_DEFECTS_SUBSECTION"] + "\",\"code\":\"" + pl["SECTIONS_DEFECTS_CODE"] + "\",\"critical_level\":\"" + pl["SECTIONS_DEFECTS_CRITICAL_LEVEL"] + "\",\"major_level\":\"" + pl["SECTIONS_DEFECTS_MAJOR_LEVEL"] + "\",\"minor_level\":\"" + pl["SECTIONS_DEFECTS_MINOR_LEVEL"] + "\"," +
                            "\"comments\":\"" + pl["SECTIONS_DEFECTS_COMMENTS"] + "\",\"pictures\":[" + string.Join(",", defects_pl_pic) + "]}");
                        }
                        foreach (var p in dr_p)
                        {
                            List<string> defects_p_pic = new List<string>();
                            DataTable dtsections_f_p_pic = new DataTable();
                            string sqlpic = $@"select UNION_ID
    ,SECTIONS_DEFECTS_PICTURES_TITLE
    ,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
    ,SECTIONS_DEFECTS_PICTURES_NUMBER
    ,SECTIONS_DEFECTS_PICTURES_COMMENT
    ,SECTION_TYPE
    ,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{p["ID"]}'";
                            OracleCommand cmdpic = new OracleCommand(sqlpic, conmes);
                            cmdpic.CommandType = CommandType.Text;
                            OracleDataAdapter dapic = new OracleDataAdapter(cmdpic);

                            dapic.Fill(dtsections_f_p_pic);

                            foreach (DataRow itemsections_f_pl_pic in dtsections_f_p_pic.Rows)
                            {
                                string full_filename = GetImageData(itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                                defects_p_pic.Add("{\"title\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\": \"" + full_filename + "\",\"number\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");

                            }


                            defects_p.Add("{\"label\":\"" + p["SECTIONS_DEFECTS_LABEL"] + "\"," +
                            "\"subsection\":\"" + p["SECTIONS_DEFECTS_SUBSECTION"] + "\",\"code\":\"" + p["SECTIONS_DEFECTS_CODE"] + "\",\"critical_level\":\"" + p["SECTIONS_DEFECTS_CRITICAL_LEVEL"] + "\",\"major_level\":\"" + p["SECTIONS_DEFECTS_MAJOR_LEVEL"] + "\",\"minor_level\":\"" + p["SECTIONS_DEFECTS_MINOR_LEVEL"] + "\"," +
                            "\"comments\":\"" + p["SECTIONS_DEFECTS_COMMENTS"] + "\",\"pictures\":[" + string.Join(",", defects_p_pic) + "]}");
                        }

                        List<string> lstsections = new List<string>();


                        lstsections.Add("{\"type\":\"" + dr_pl[0]["SECTIONS_TYPE"] + "\",\"title\":\"" + dr_pl[0]["SECTIONS_TITLE"] + "\",\"section_result_id\":\"" + dr_pl[0]["SECTIONS_RESULT_ID"] + "\",\"qty_inspected\":\"" + dr_pl[0]["SECTIONS_QTY_INSPECTED"] + "\"," +
                    "\"sampled_inspected\":\"" + dr_pl[0]["SECTIONS_SAMPLED_INSPECTED"] + "\",\"defective_parts\":\"" + dr_pl[0]["SECTIONS_DEFECTIVE_PARTS"] + "\",\"inspection_level\":\"" + dr_pl[0]["SECTIONS_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + dr_pl[0]["SECTIONS_INSPECTION_METHOD"] + "\",\"aql_minor\":\"" + dr_pl[0]["SECTIONS_AQL_MINOR"] + "\"," +
                    "\"aql_major\":\"" + dr_pl[0]["SECTIONS_AQL_MAJOR"] + "\",\"aql_critical\":\"" + dr_pl[0]["SECTIONS_AQL_CRITICAL"] + "\",\"barcodes\":[{\"value\":\"001\"}]," +
                    "\"qty_type\":\"" + dr_pl[0]["SECTIONS_QTY_TYPE"] + "\",\"max_minor_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MINOR_DEFECTS"] + "\",\"max_major_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_DEFECTS"] + "\",\"max_major_a_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_A_DEFECTS"] + "\",\"max_major_b_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_B_DEFECTS"] + "\",\"max_critical_defects\":\"" + dr_pl[0]["SECTIONS_MAX_CRITICAL_DEFECTS"] + "\"," +
                    "\"defects\":[" + string.Join(",", defects_pl) + "]}");

                        lstsections.Add("{\"type\":\"" + dr_p[0]["SECTIONS_TYPE"] + "\",\"title\":\"" + dr_p[0]["SECTIONS_TITLE"] + "\",\"section_result_id\":\"" + dr_p[0]["SECTIONS_RESULT_ID"] + "\",\"qty_inspected\":\"" + dr_p[0]["SECTIONS_QTY_INSPECTED"] + "\"," +
                    "\"sampled_inspected\":\"" + dr_p[0]["SECTIONS_SAMPLED_INSPECTED"] + "\",\"defective_parts\":\"" + dr_p[0]["SECTIONS_DEFECTIVE_PARTS"] + "\",\"inspection_level\":\"" + dr_p[0]["SECTIONS_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + dr_p[0]["SECTIONS_INSPECTION_METHOD"] + "\",\"aql_minor\":\"" + dr_p[0]["SECTIONS_AQL_MINOR"] + "\"," +
                    "\"aql_major\":\"" + dr_p[0]["SECTIONS_AQL_MAJOR"] + "\",\"aql_critical\":\"" + dr_p[0]["SECTIONS_AQL_CRITICAL"] + "\"," +
                    "\"qty_type\":\"" + dr_p[0]["SECTIONS_QTY_TYPE"] + "\",\"max_minor_defects\":\"" + dr_p[0]["SECTIONS_MAX_MINOR_DEFECTS"] + "\",\"max_major_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_DEFECTS"] + "\",\"max_major_a_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_A_DEFECTS"] + "\",\"max_major_b_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_B_DEFECTS"] + "\",\"max_critical_defects\":\"" + dr_p[0]["SECTIONS_MAX_CRITICAL_DEFECTS"] + "\"," +
                    "\"defects\":[" + string.Join(",", defects_p) + "]}");



                        string sql2 = $@"select UNION_ID
                        ,SECTIONS_DEFECTS_PICTURES_TITLE
                        ,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
                        ,SECTIONS_DEFECTS_PICTURES_NUMBER
                        ,SECTIONS_DEFECTS_PICTURES_COMMENT
                        ,SECTION_TYPE
                        ,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd2 = new OracleCommand(sql2, conmes);
                        cmd2.CommandType = CommandType.Text;
                        OracleDataAdapter da2 = new OracleDataAdapter(cmd2);

                        da2.Fill(dtsections_f);
                        List<string> lstsections_f = new List<string>();
                        //if (dtsections_f.Rows.Count > 0)
                        //{
                        foreach (DataRow itemsections_f in dtsections_f.Rows)
                        {
                            string full_filename = GetImageData(itemsections_f["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                            //lstsections_f.Add("{\"type\":\"" + itemsections_f["SECTION_TYPE"] + "\",\"title\":\"" + itemsections_f["SECTION_TITLE"] + "\",\"pictures\":[{\"title\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\":\"" + full_filename + "\",\"number\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}]}");
                            lstsections_f.Add("{\"title\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\":\"" + full_filename + "\",\"number\":\"\",\"comment\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");
                        }
                        //}
                        //else
                        //{
                        //    lstsections_f.Add("");
                        //}


                        string sql3 = $@"select UNION_ID
    ,PASSFAILS_TITLE
    ,PASSFAILS_VALUE
    ,PASSFAILS_TYPE
    ,PASSFAILS_SUBSECTION
    ,PASSFAILS_CHECKLISTSUBSECTION
    ,PASSFAILS_STATUS
    ,PASSFAILS_COMMENT
    from t_aeqs_to_p88_passfail where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd3 = new OracleCommand(sql3, conmes);
                        cmd3.CommandType = CommandType.Text;
                        OracleDataAdapter da3 = new OracleDataAdapter(cmd3);
                        DataTable dtpassfail = new DataTable();
                        da3.Fill(dtpassfail);
                        List<string> lstpassfail = new List<string>();
                        foreach (DataRow itempassfail in dtpassfail.Rows)
                        {
                            lstpassfail.Add("{\"title\":\"" + itempassfail["PASSFAILS_TITLE"] + "\",\"value\":\"" + itempassfail["PASSFAILS_VALUE"] + "\",\"type\":\"" + itempassfail["PASSFAILS_TYPE"] + "\"," +
                        "\"subsection\":\"" + itempassfail["PASSFAILS_SUBSECTION"] + "\",\"checkListSubsection\":\"" + itempassfail["PASSFAILS_CHECKLISTSUBSECTION"] + "\",\"status\":\"" + itempassfail["PASSFAILS_STATUS"] + "\",\"comment\":\"" + itempassfail["PASSFAILS_COMMENT"] + "\"}");
                        }


                        string sql4 = $@"select UNION_ID
                , ASSIGNMENT_ITEMS_SAMPLED_INSPECTED
                , ASSIGNMENT_ITEMS_INSPECTION_RESULT_ID
                , ASSIGNMENT_ITEMS_INSPECTION_STATUS_ID
                , ASSIGNMENT_ITEMS_QTY_INSPECTED
                , ASSIGNMENT_ITEMS_INSPECTION_COMPLETED_DATE
                , ASSIGNMENT_ITEMS_TOTAL_INSPECTION_MINUTES
                , ASSIGNMENT_ITEMS_SAMPLING_SIZE
                , ASSIGNMENT_ITEMS_QTY_TO_INSPECT
                , ASSIGNMENT_ITEMS_AQL_MINOR
                , ASSIGNMENT_ITEMS_AQL_MAJOR
                , ASSIGNMENT_ITEMS_AQL_MAJOR_A
                , ASSIGNMENT_ITEMS_AQL_MAJOR_B
                , ASSIGNMENT_ITEMS_AQL_CRITICAL
                , ASSIGNMENT_ITEMS_SUPPLIER_BOOKING_MSG
                , ASSIGNMENT_ITEMS_CONCLUSION_REMARKS
                , ASSIGNMENT_ITEMS_ASSIGNMENT_REPORT_TYPE_ID
                , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTOR_USERNAME
                , ASSIGNMENT_ITEMS_ASSIGNMENT_DATE_INSPECTION
                , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_LEVEL
                , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_METHOD
                , ASSIGNMENT_ITEMS_PO_LINE_QTY
                , ASSIGNMENT_ITEMS_PO_LINE_ETD
                , ASSIGNMENT_ITEMS_PO_LINE_ETA
                , ASSIGNMENT_ITEMS_PO_LINE_COLOR
                , ASSIGNMENT_ITEMS_PO_LINE_SIZE
                , ASSIGNMENT_ITEMS_PO_LINE_STYLE
                , ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ID
                , ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ERP_BUSINESS_ID
                , ASSIGNMENT_ITEMS_PO_LINE_PO_NUMBER
                , ASSIGNMENT_ITEMS_PO_LINE_CUSTOMER_PO
                , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ID
                , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ERP_BUSINESS_ID
                , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_PROJECT_ID
                , ASSIGNMENT_ITEMS_PO_LINE_SKU_SKU_NUMBER
                , ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_NAME
                , ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_DESCRIPTION
                , PO_LINE_PROJECT_CODE
                , REPORT_TYPE_NAME
    from t_aeqs_to_p88_assignment where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd4 = new OracleCommand(sql4, conmes);
                        cmd4.CommandType = CommandType.Text;
                        OracleDataAdapter da4 = new OracleDataAdapter(cmd4);
                        DataTable dtassignment = new DataTable();
                        da4.Fill(dtassignment);
                        List<string> lstassignment = new List<string>();

                        //Edit on 7/1(PO Change)
                        foreach (DataRow itemassignment in dtassignment.Rows)
                        {

                            //The old data remains unchanged, and the new PO after POChange is transferred to Project Code
                            string id = "\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_PROJECT_ID"] + "\"";
                            string reportTypeKey = "id";
                            string reportTypeValue = itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_REPORT_TYPE_ID"].ToString();
                            if (!string.IsNullOrEmpty(itemassignment["PO_LINE_PROJECT_CODE"].ToString()))
                            {
                                id = "\"project_code\":\"" + itemassignment["PO_LINE_PROJECT_CODE"] + "\"";
                                reportTypeKey = "name";
                                reportTypeValue = itemassignment["REPORT_TYPE_NAME"].ToString();
                            }


                            lstassignment.Add("{\"sampled_inspected\":\"" + itemassignment["ASSIGNMENT_ITEMS_SAMPLED_INSPECTED"] + "\",\"inspection_result_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_INSPECTION_RESULT_ID"] + "\",\"inspection_status_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_INSPECTION_STATUS_ID"] + "\",\"qty_inspected\":\"" + itemassignment["ASSIGNMENT_ITEMS_QTY_INSPECTED"] + "\"," +
                        "\"inspection_completed_date\":\"" + Convert.ToDateTime(itemassignment["ASSIGNMENT_ITEMS_INSPECTION_COMPLETED_DATE"]).ToString("yyyy-MM-ddTHH:mm:ss") + timezone + "\",\"total_inspection_minutes\":\"" + itemassignment["ASSIGNMENT_ITEMS_TOTAL_INSPECTION_MINUTES"] + "\",\"sampling_size\":\"" + itemassignment["ASSIGNMENT_ITEMS_SAMPLING_SIZE"] + "\",\"qty_to_inspect\":\"" + itemassignment["ASSIGNMENT_ITEMS_QTY_TO_INSPECT"] + "\"," +
                        "\"aql_minor\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MINOR"] + "\",\"aql_major\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR"] + "\",\"aql_major_a\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR_A"] + "\",\"aql_major_b\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR_B"] + "\",\"aql_critical\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_CRITICAL"] + "\",\"supplier_booking_msg\":\"" + itemassignment["ASSIGNMENT_ITEMS_SUPPLIER_BOOKING_MSG"] + "\"," +
                        "\"conclusion_remarks\":\"" + itemassignment["ASSIGNMENT_ITEMS_CONCLUSION_REMARKS"] + "\",\"assignment\":{\"report_type\":{\"" + reportTypeKey + "\":\"" + reportTypeValue + "\"},\"inspector\":{\"username\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTOR_USERNAME"] + "\"}," +
                        "\"date_inspection\":\"" + Convert.ToDateTime(itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_DATE_INSPECTION"]).ToString("yyyy-MM-ddTHH:mm:ss") + timezone + "\",\"inspection_level\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_METHOD"] + "\"},\"po_line\":{\"qty\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_QTY"] + "\"," +
                        "\"etd\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_ETD"] + "\",\"eta\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_ETA"] + "\",\"color\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_COLOR"] + "\",\"size\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SIZE"] + "\",\"style\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_STYLE"] + "\",\"po\":{\"exporter\":{\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ID"] + "\"," +
                        "\"erp_business_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ERP_BUSINESS_ID"] + "\"},\"po_number\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_NUMBER"] + "\",\"customer_po\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_CUSTOMER_PO"] + "\",\"importer\":{\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ID"] + "\",\"erp_business_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ERP_BUSINESS_ID"] + "\"}," +
                        "\"project\":{" + id + "}},\"sku\":{\"sku_number\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_SKU_NUMBER"] + "\",\"item_name\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_NAME"] + "\",\"item_description\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_DESCRIPTION"] + "\"}}, \"fields\": {\"string_12\":\"" + item["ASSIGNMENT_ITEMS_FIELDS_STRING_12"] + "\"}}");
                        }

                        List<string> lstpassfail_val = new List<string>();
                        string[] arr = item["PASSFAILS_0_LISTVALUES_VALUE"].ToString().Split('/');
                        //if (arr.Length > 0)
                        //{
                        //    foreach (var item_val in arr)
                        //    {
                        //        lstpassfail_val.Add("{\"value\":\"" + item_val.Trim() + "\"}");
                        //    }

                        //}
                        if (arr[0] != "")
                        {
                            foreach (var item_val in arr)
                            {
                                lstpassfail_val.Add("{\"value\":\"" + item_val.Trim() + "\"}");
                            }

                        }
                        else
                        {
                            lstpassfail_val.Add("{\"value\":\"N/A\"}");
                        }

                        string PostJson = "{\"status\":\"" + item["STATUS"] + "\",\"date_started\":\"" + Convert.ToDateTime(item["DATE_STARTED"]).ToString("yyyy-MM-ddTHH:mm:ss") + timezone + "\",\"defective_parts\":" + item["DEFECTIVE_PARTS"] + "," +

                        //sections 
                        //"\"sections\":[" + string.Join(",", lstsections) + "," + string.Join(",", lstsections_f) + "]," +
                        "\"sections\":[" + string.Join(",", lstsections) + ",{\"type\":\"pictures\",\"title\":\"photos\",\"pictures\":[" + string.Join(",", lstsections_f) + "]}]," +
                        //"\"sections\":[" + string.Join(",", lstsections) + "]," +

                        //assignment_items
                        "\"assignment_items\":[" + string.Join(",", lstassignment) + "]," +

                        ////passFails


                        "\"passFails\":[{\"title\":\"" + item["PASSFAILS_0_TITLE"] + "\",\"type\":\"" + item["PASSFAILS_0_TYPE"] + "\",\"subsection\":\"" + item["PASSFAILS_0_SUBSECTION"] + "\",\"listValues\":[" + string.Join(",", lstpassfail_val) + "]}," + string.Join(",", lstpassfail) + "]}";



                        #region Retransmission
                        int maxRetries = 5;
                        int currentRetry = 0;

                        bool retryRequest = true;
                        HttpResponseMessage response = null;

                        try
                        {
                            do
                            {
                                var client = new HttpClient();
                                client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                                //Test
                               //var request = new HttpRequestMessage(HttpMethod.Put, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");    // P88 test

                                //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test

                                //offical
                                var request = new HttpRequestMessage(HttpMethod.Put, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");           // P88 Official
                                //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58");  //test
                                 request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f");    //official

                                // Create and initialize request content
                                var content = new StringContent(PostJson, null, "application/json");

                                // Set the request content
                                request.Content = content;

                                // send request
                                response = await client.SendAsync(request);

                                // Check the status code, if it is not 500 or 405 or timeout, jump out of the retry loop
                                if (response != null && response.StatusCode != HttpStatusCode.InternalServerError && response.StatusCode != HttpStatusCode.MethodNotAllowed && response.StatusCode != HttpStatusCode.RequestTimeout)
                                {
                                    retryRequest = false;
                                }
                                else
                                {
                                    // If it is 500, wait for a while and try again (for example: linear backoff strategy, doubling the delay time each time)
                                    await Task.Delay(2500); // Assume that the interval between each retry is 2.5 seconds, and the specific delay strategy is adjusted according to the actual situation.
                                    if (currentRetry > 0) FMSLOG.Platform("Retransfer JSON : " + item["UNIQUE_KEY"] + $"...{currentRetry}", vsrvinfo.Operation);
                                    currentRetry++;
                                }
                            } while (retryRequest && currentRetry <= maxRetries);
                        }
                        catch (Exception ex)
                        {
                            FMSLOG.Platform("Error JSON : " + item["UNIQUE_KEY"] + $"Exection:{ex.Message}" + PostJson, vsrvinfo.Operation);
                        }

                        Error_Json = await response.Content.ReadAsStringAsync();
                        Sync_StatusCode = ((int)response.StatusCode).ToString();
                        Sync_Message = response.StatusCode.ToString();
                        #endregion

                        response.EnsureSuccessStatusCode();
                        //Console.WriteLine(await response.Content.ReadAsStringAsync());
                        if (response.IsSuccessStatusCode)
                        {
                            //if (PostJson.Contains("'"))
                            //{
                            //    Json_CLOB = PostJson.Replace("'", "''");
                            //}
                            FMSLOG.Platform("Success JSON : " + item["UNIQUE_KEY"] + PostJson, vsrvinfo.Operation);
                            string sql_rtMsg = "UPDATE t_aeqs_to_p88_list SET IS_SYNC='S',SYNC_STATUS_CODE='" + Sync_StatusCode + "',SYNC_DATE=sysdate,SYNC_MESSAGE='" + Sync_Message + "' WHERE UNIQUE_KEY ='" + UniqueKey + "' ";
                            OracleCommand cmd = new OracleCommand(sql_rtMsg, conmes);
                            cmd.CommandType = CommandType.Text;
                            int r = cmd.ExecuteNonQuery();
                            //await POSTAsync(dtsections_f);
                            await POSTAsync_Inline(UniqueKey, dtsections_f);
                            returnMsg = "Success";

                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        FMSLOG.Platform("Error JSON Pivot88: " + Error_Json, vsrvinfo.Operation);
                        string sql_rtMsg = "UPDATE t_aeqs_to_p88_list SET IS_SYNC='N',SYNC_STATUS_CODE='" + Sync_StatusCode + "',SYNC_DATE=sysdate,SYNC_MESSAGE='" + Sync_Message + "' WHERE UNIQUE_KEY ='" + UniqueKey + "' ";
                        OracleCommand cmd = new OracleCommand(sql_rtMsg, conmes);
                        cmd.CommandType = CommandType.Text;
                        int r = cmd.ExecuteNonQuery();
                        returnMsg = "Fail" + ex.ToString();

                    }

                }
            }
            else
            {
                FMSLOG.Platform("No Data to Post SAP", vsrvinfo.Operation);
            }
        }

        //public async Task POSTAsync(DataTable dt_img)
        public async Task POSTAsync_Inline(string UniqueKey, DataTable dt_img1)
        {
            try
            {
                DataTable dt_img = new DataTable();
                string sql = $@"select UNION_ID
                        ,SECTIONS_DEFECTS_PICTURES_TITLE
                        ,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
                        ,SECTIONS_DEFECTS_PICTURES_NUMBER
                        ,SECTIONS_DEFECTS_PICTURES_COMMENT
                        ,SECTION_TYPE
                        ,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID in (select id from  t_aeqs_to_p88_sections where union_id ='" + UniqueKey + "') ";
                OracleCommand cmd = new OracleCommand(sql, conmes);
                cmd.CommandType = CommandType.Text;
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                da.Fill(dt_img);


                if (dt_img.Rows.Count > 0)
                {
                    foreach (DataRow item in dt_img.Rows)
                    {
                        string imgpath = string.Empty;
                        string imgfull_filename = string.Empty;
                        string img_SUFFIX = string.Empty;
                        var client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                        //var request = new HttpRequestMessage(HttpMethod.Post, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload"); // P88 test
                        //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test
                        var request = new HttpRequestMessage(HttpMethod.Post, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload");         // P88 official
                        request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f"); //official


                        var content = new MultipartFormDataContent();
                        imgfull_filename = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                        imgpath = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "B");
                        img_SUFFIX = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "C");
                        //content.Add(new StreamContent(File.OpenRead("E:\\SAP\\MES\\Final_Version_SourceCode\\MES&WMS(SAP Version)\\" +
                        //    "MES&WMS to SAP(Interface)\\Development\\POST_TO_PIVOT88\\POST_TO_SAP\\FMSPlatForm\\FMS\\images\\upload\\20230510170946286.jpg")),
                        //    "file", "20230510170946286.jpg");

                        var webC = new System.Net.WebClient();
                        string url = imgpath.Replace("\\", "/");
                        Image image = new Bitmap(webC.OpenRead(url));
                        MemoryStream stream = new MemoryStream();
                        if (img_SUFFIX == "png") { image.Save(stream, ImageFormat.Png); } else if (img_SUFFIX == "jpg") { image.Save(stream, ImageFormat.Jpeg); }
                        image.Save(stream, ImageFormat.Png);
                        stream.Seek(0, SeekOrigin.Begin); //Need to reset position to 0

                        content.Add(new StreamContent(stream), "file", imgfull_filename);
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        var j = await response.Content.ReadAsStringAsync();
                        response.EnsureSuccessStatusCode();
                        //Console.WriteLine(await response.Content.ReadAsStringAsync());
                    }
                }
                if (dt_img1.Rows.Count > 0)
                {
                    foreach (DataRow item in dt_img1.Rows)
                    {
                        string imgpath = string.Empty;
                        string imgfull_filename = string.Empty;
                        string img_SUFFIX = string.Empty;
                        var client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                        //var request = new HttpRequestMessage(HttpMethod.Post, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload");
                        //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58");
                        var request = new HttpRequestMessage(HttpMethod.Post, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload");         // P88 official
                        request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f"); //official

                        var content = new MultipartFormDataContent();
                        imgfull_filename = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                        imgpath = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "B");
                        img_SUFFIX = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "C");
                        //content.Add(new StreamContent(File.OpenRead("E:\\SAP\\MES\\Final_Version_SourceCode\\MES&WMS(SAP Version)\\" +
                        //    "MES&WMS to SAP(Interface)\\Development\\POST_TO_PIVOT88\\POST_TO_SAP\\FMSPlatForm\\FMS\\images\\upload\\20230510170946286.jpg")),
                        //    "file", "20230510170946286.jpg");

                        var webC = new System.Net.WebClient();
                        string url = imgpath.Replace("\\", "/");
                        Image image = new Bitmap(webC.OpenRead(url));
                        MemoryStream stream = new MemoryStream();
                        if (img_SUFFIX == "png") { image.Save(stream, ImageFormat.Png); } else if (img_SUFFIX == "jpg") { image.Save(stream, ImageFormat.Jpeg); }
                        image.Save(stream, ImageFormat.Png);
                        stream.Seek(0, SeekOrigin.Begin); //Need to reset position to 0

                        content.Add(new StreamContent(stream), "file", imgfull_filename);
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        var j = await response.Content.ReadAsStringAsync();
                        response.EnsureSuccessStatusCode();
                        //Console.WriteLine(await response.Content.ReadAsStringAsync());
                    }
                }

            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show(ex.ToString());
                string errorMsg = ex.Message;
                FMSLOG.Platform("Error JSON Pivot88: " + UniqueKey + " Image upload fail!", null);
                throw;
            }
        }


        #endregion


        #region EndOfLine (TQC)
        //Cls_Return rt = new Cls_Return();
        public async void PostRequestAsync_EndOfLine(SrvInfo vsrvinfo)
        {
            try
            {
                string constrmes = null;
                string filePath;

                filePath = Application.ExecutablePath;
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                string clientEnvConfigFileName = filePath + "database.config";


                if (File.Exists(clientEnvConfigFileName))
                {
                    FileLoader obj = new FileLoader(clientEnvConfigFileName);
                    Hashtable htdblinks = obj.GetDBLinks();
                    if (htdblinks.ContainsKey(vsrvinfo.SDB))
                        constrmes = htdblinks[vsrvinfo.SDB].ToString();

                    conmes = new OracleConnection(constrmes);
                    //OracleTransaction objTrans = null;

                    int updatecount = 0;
                    int errorcount = 0;
                    //await PushNotificationTestAsync();
                    //string errmsg = "";
                    //string sql = "select voucher_no,MAX(CREATE_DATE),SYNC_TIMES from wms_to_sap_list where (is_sync='N' or is_sync is null) and SYNC_TIMES < 3 and  INTERFACE_NO='" + interface_no + "' group by voucher_no,SYNC_TIMES";
                    //OracleCommand cmd = new OracleCommand(sql, conmes);
                    //cmd.CommandType = CommandType.Text;
                    //OracleDataAdapter da = new OracleDataAdapter(cmd);
                    //DataTable dtcheck = new DataTable();
                    //da.Fill(dtcheck);

                    //if (dtcheck.Rows.Count > 0)
                    //{
                    //FMSLOG.Platform("Data Retrived Successfull..(" + dtcheck.Rows.Count + ")Records on " + vsrvinfo.StartDate, vsrvinfo.Operation);
                    FMSLOG.Platform("Data Post Started " + vsrvinfo.StartDate, vsrvinfo.Operation);

                    conmes.Open();
                    //objTrans = conoa.BeginTransaction();

                    await PUTAsync_EndOfLine(vsrvinfo);

                    //System.Threading.Thread.Sleep(30000);
                    //objTrans.Commit();

                    FMSLOG.Platform("Data Send Status : " + returnMsg + "", vsrvinfo.Operation);
                    //SendMailAlert(vsrvinfo);
                    //}
                    //else
                    //{
                    //    FMSLOG.Platform("No Data to Post SAP", vsrvinfo.Operation);
                    //}

                }
                else
                {
                    FMSLOG.Platform("No Databases Exists..", vsrvinfo.Operation);
                }
            }
            catch (Exception ex)
            {
                //string sql = "UPDATE  wms_to_sap_list SET SYNC_DATE=to_date('" + inp.synctime + "','yyyymmdd HH24:MI:SS'),SYNC_TIMES=nvl(sync_times,0)+1   WHERE UUID='" + inp.UUID + "' ";
                //OracleCommand cmd = new OracleCommand(sql, conmes);
                //cmd.CommandType = CommandType.Text;
                //int r = cmd.ExecuteNonQuery();
                FMSLOG.Platform(MethodBase.GetCurrentMethod().Name + " Exception : " + ex.Message, vsrvinfo.Operation);
            }
            finally
            {
                conmes.Close();
                conmes.Dispose();

                GC.Collect();
            }
        }

        public async Task PUTAsync_EndOfLine(SrvInfo vsrvinfo)
        {
            string UniqueKey = string.Empty;
            string Json = string.Empty;
            var Error_Json = string.Empty;
            DataTable dtlist = null;
            try
            {
                string sql = $@"select 
                                UNIQUE_KEY
                                ,l.STATUS
                                , l.DATE_STARTED
                                , l.DEFECTIVE_PARTS
                                , l.PASSFAILS_0_TITLE
                                , l.PASSFAILS_0_TYPE
                                , l.PASSFAILS_0_SUBSECTION
                                , l.PASSFAILS_0_LISTVALUES_VALUE
                                ,l.assignment_items_fields_string_12
                                from t_aeqs_to_p88_list l 
                                where 
                                --to_char(l.date_started,'yyyy/mm/dd')= to_char(sysdate,'yyyy/mm/dd') and 
                                l.assignment_items_assignment_report_type_id=31
--and unique_key in ('apache1_2922')
                --and AEQS_INSERT_DATE < to_date('2024/03/14 23:59:59','yyyy/mm/dd hh24:mi:ss') 

                                and (IS_SYNC IS NULL 
                                   or IS_SYNC='N'
                                ) 
                                order by l.UNIQUE_KEY
                                ";



                OracleCommand cmd = new OracleCommand(sql, conmes);
                cmd.CommandType = CommandType.Text;
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                dtlist = new DataTable();
                da.Fill(dtlist);

            }
            catch (Exception ex)
            {
                FMSLOG.Platform($"Error GetMiddleData: " + ex.Message, vsrvinfo.Operation);
                returnMsg = "Fail" + ex.ToString();
            }

            if (dtlist.Rows.Count > 0)
            {
                foreach (DataRow item in dtlist.Rows)
                {
                    try
                    {

                        UniqueKey = item["UNIQUE_KEY"].ToString();
                        DataTable dtsections_f = new DataTable();
                        string sql1 = $@"select ID
,UNION_ID
,SECTIONS_TYPE
,SECTIONS_TITLE
,SECTIONS_RESULT_ID
,SECTIONS_QTY_INSPECTED
,SECTIONS_SAMPLED_INSPECTED
,SECTIONS_DEFECTIVE_PARTS
,SECTIONS_INSPECTION_LEVEL
,SECTIONS_INSPECTION_METHOD
,SECTIONS_AQL_MINOR
,SECTIONS_AQL_MAJOR
,SECTIONS_AQL_CRITICAL
,SECTIONS_BARCODES_VALUE
,SECTIONS_QTY_TYPE
,SECTIONS_MAX_MINOR_DEFECTS
,SECTIONS_MAX_MAJOR_DEFECTS
,SECTIONS_MAX_MAJOR_A_DEFECTS
,SECTIONS_MAX_MAJOR_B_DEFECTS
,SECTIONS_MAX_CRITICAL_DEFECTS
,SECTIONS_DEFECTS_LABEL
,SECTIONS_DEFECTS_SUBSECTION
,SECTIONS_DEFECTS_CODE
,SECTIONS_DEFECTS_CRITICAL_LEVEL
,SECTIONS_DEFECTS_MAJOR_LEVEL
,SECTIONS_DEFECTS_MINOR_LEVEL
,SECTIONS_DEFECTS_COMMENTS from t_aeqs_to_p88_sections where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd1 = new OracleCommand(sql1, conmes);
                        cmd1.CommandType = CommandType.Text;
                        OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
                        DataTable dtsections = new DataTable();
                        da1.Fill(dtsections);

                        DataRow[] dr_pl = null;
                        DataRow[] dr_p = null;
                        //SECTIONS packaging part
                        dr_pl = dtsections.Select($"SECTIONS_TITLE='packing_packaging_labelling'");
                        //Sections of the product
                        dr_p = dtsections.Select($"SECTIONS_TITLE='product'");
                        List<string> defects_pl = new List<string>();
                        List<string> defects_p = new List<string>();




                        //Product Image
                        foreach (var pl in dr_pl)
                        {
                            List<string> defects_pl_pic = new List<string>();
                            DataTable dtsections_f_pl_pic = new DataTable();

                            string sqlpic = $@"select ID
,UNION_ID
,SECTIONS_DEFECTS_PICTURES_TITLE
,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
,SECTIONS_DEFECTS_PICTURES_NUMBER
,SECTIONS_DEFECTS_PICTURES_COMMENT
,SECTION_TYPE
,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{pl["ID"]}'";
                            OracleCommand cmdpic = new OracleCommand(sqlpic, conmes);
                            cmdpic.CommandType = CommandType.Text;
                            OracleDataAdapter dapic = new OracleDataAdapter(cmdpic);

                            dapic.Fill(dtsections_f_pl_pic);

                            foreach (DataRow itemsections_f_pl_pic in dtsections_f_pl_pic.Rows)
                            {
                                string full_filename = GetImageData(itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                                defects_pl_pic.Add("{\"title\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\": \"" + full_filename + "\",\"number\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");

                            }


                            defects_pl.Add("{\"label\":\"" + pl["SECTIONS_DEFECTS_LABEL"] + "\"," +
                            "\"subsection\":\"" + pl["SECTIONS_DEFECTS_SUBSECTION"] + "\",\"code\":\"" + pl["SECTIONS_DEFECTS_CODE"] + "\",\"critical_level\":\"" + pl["SECTIONS_DEFECTS_CRITICAL_LEVEL"] + "\",\"major_level\":\"" + pl["SECTIONS_DEFECTS_MAJOR_LEVEL"] + "\",\"minor_level\":\"" + pl["SECTIONS_DEFECTS_MINOR_LEVEL"] + "\"," +
                            "\"comments\":\"" + pl["SECTIONS_DEFECTS_COMMENTS"] + "\",\"pictures\":[" + string.Join(",", defects_pl_pic) + "]}");
                        }

                        //Packaging pictures
                        foreach (var p in dr_p)
                        {
                            List<string> defects_p_pic = new List<string>();
                            DataTable dtsections_f_p_pic = new DataTable();
                            string sqlpic = $@"select UNION_ID
,SECTIONS_DEFECTS_PICTURES_TITLE
,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
,SECTIONS_DEFECTS_PICTURES_NUMBER
,SECTIONS_DEFECTS_PICTURES_COMMENT
,SECTION_TYPE
,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{p["ID"]}'";
                            OracleCommand cmdpic = new OracleCommand(sqlpic, conmes);
                            cmdpic.CommandType = CommandType.Text;
                            OracleDataAdapter dapic = new OracleDataAdapter(cmdpic);

                            dapic.Fill(dtsections_f_p_pic);

                            foreach (DataRow itemsections_f_pl_pic in dtsections_f_p_pic.Rows)
                            {
                                string full_filename = GetImageData(itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                                defects_p_pic.Add("{\"title\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\": \"" + full_filename + "\",\"number\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");

                            }


                            defects_p.Add("{\"label\":\"" + p["SECTIONS_DEFECTS_LABEL"] + "\"," +
                            "\"subsection\":\"" + p["SECTIONS_DEFECTS_SUBSECTION"] + "\",\"code\":\"" + p["SECTIONS_DEFECTS_CODE"] + "\",\"critical_level\":\"" + p["SECTIONS_DEFECTS_CRITICAL_LEVEL"] + "\",\"major_level\":\"" + p["SECTIONS_DEFECTS_MAJOR_LEVEL"] + "\",\"minor_level\":\"" + p["SECTIONS_DEFECTS_MINOR_LEVEL"] + "\"," +
                            "\"comments\":\"" + p["SECTIONS_DEFECTS_COMMENTS"] + "\",\"pictures\":[" + string.Join(",", defects_p_pic) + "]}");
                        }

                        List<string> lstsections = new List<string>();


                        lstsections.Add("{\"type\":\"" + dr_pl[0]["SECTIONS_TYPE"] + "\",\"title\":\"" + dr_pl[0]["SECTIONS_TITLE"] + "\",\"section_result_id\":\"" + dr_pl[0]["SECTIONS_RESULT_ID"] + "\",\"qty_inspected\":\"" + dr_pl[0]["SECTIONS_QTY_INSPECTED"] + "\"," +
                    "\"sampled_inspected\":\"" + dr_pl[0]["SECTIONS_SAMPLED_INSPECTED"] + "\",\"defective_parts\":\"" + dr_pl[0]["SECTIONS_DEFECTIVE_PARTS"] + "\",\"inspection_level\":\"" + dr_pl[0]["SECTIONS_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + dr_pl[0]["SECTIONS_INSPECTION_METHOD"] + "\",\"aql_minor\":\"" + dr_pl[0]["SECTIONS_AQL_MINOR"] + "\"," +
                    "\"aql_major\":\"" + dr_pl[0]["SECTIONS_AQL_MAJOR"] + "\",\"aql_critical\":\"" + dr_pl[0]["SECTIONS_AQL_CRITICAL"] + "\",\"barcodes\":[{\"value\":\"001\"}]," +
                    "\"qty_type\":\"" + dr_pl[0]["SECTIONS_QTY_TYPE"] + "\",\"max_minor_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MINOR_DEFECTS"] + "\",\"max_major_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_DEFECTS"] + "\",\"max_major_a_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_A_DEFECTS"] + "\",\"max_major_b_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_B_DEFECTS"] + "\",\"max_critical_defects\":\"" + dr_pl[0]["SECTIONS_MAX_CRITICAL_DEFECTS"] + "\"," +
                    "\"defects\":[" + string.Join(",", defects_pl) + "]}");

                        lstsections.Add("{\"type\":\"" + dr_p[0]["SECTIONS_TYPE"] + "\",\"title\":\"" + dr_p[0]["SECTIONS_TITLE"] + "\",\"section_result_id\":\"" + dr_p[0]["SECTIONS_RESULT_ID"] + "\",\"qty_inspected\":\"" + dr_p[0]["SECTIONS_QTY_INSPECTED"] + "\"," +
                    "\"sampled_inspected\":\"" + dr_p[0]["SECTIONS_SAMPLED_INSPECTED"] + "\",\"defective_parts\":\"" + dr_p[0]["SECTIONS_DEFECTIVE_PARTS"] + "\",\"inspection_level\":\"" + dr_p[0]["SECTIONS_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + dr_p[0]["SECTIONS_INSPECTION_METHOD"] + "\",\"aql_minor\":\"" + dr_p[0]["SECTIONS_AQL_MINOR"] + "\"," +
                    "\"aql_major\":\"" + dr_p[0]["SECTIONS_AQL_MAJOR"] + "\",\"aql_critical\":\"" + dr_p[0]["SECTIONS_AQL_CRITICAL"] + "\"," +
                    "\"qty_type\":\"" + dr_p[0]["SECTIONS_QTY_TYPE"] + "\",\"max_minor_defects\":\"" + dr_p[0]["SECTIONS_MAX_MINOR_DEFECTS"] + "\",\"max_major_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_DEFECTS"] + "\",\"max_major_a_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_A_DEFECTS"] + "\",\"max_major_b_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_B_DEFECTS"] + "\",\"max_critical_defects\":\"" + dr_p[0]["SECTIONS_MAX_CRITICAL_DEFECTS"] + "\"," +
                    "\"defects\":[" + string.Join(",", defects_p) + "]}");


                        //Public pictures section
                        string sql2 = $@"select UNION_ID
                    ,SECTIONS_DEFECTS_PICTURES_TITLE
                    ,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
                    ,SECTIONS_DEFECTS_PICTURES_NUMBER
                    ,SECTIONS_DEFECTS_PICTURES_COMMENT
                    ,SECTION_TYPE
                    ,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd2 = new OracleCommand(sql2, conmes);
                        cmd2.CommandType = CommandType.Text;
                        OracleDataAdapter da2 = new OracleDataAdapter(cmd2);

                        da2.Fill(dtsections_f);
                        List<string> lstsections_f = new List<string>();
                        //if (dtsections_f.Rows.Count > 0)
                        //{
                        foreach (DataRow itemsections_f in dtsections_f.Rows)
                        {
                            string full_filename = GetImageData(itemsections_f["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                            //lstsections_f.Add("{\"type\":\"" + itemsections_f["SECTION_TYPE"] + "\",\"title\":\"" + itemsections_f["SECTION_TITLE"] + "\",\"pictures\":[{\"title\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\":\"" + full_filename + "\",\"number\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}]}");
                            lstsections_f.Add("{\"title\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\":\"" + full_filename + "\",\"number\":\"\",\"comment\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");
                        }
                        //}
                        //else
                        //{
                        //    lstsections_f.Add("");
                        //}


                        //PassFail Section
                        string sql3 = $@"select UNION_ID
,PASSFAILS_TITLE
,PASSFAILS_VALUE
,PASSFAILS_TYPE
,PASSFAILS_SUBSECTION
,PASSFAILS_CHECKLISTSUBSECTION
,PASSFAILS_STATUS
,PASSFAILS_COMMENT
from t_aeqs_to_p88_passfail where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd3 = new OracleCommand(sql3, conmes);
                        cmd3.CommandType = CommandType.Text;
                        OracleDataAdapter da3 = new OracleDataAdapter(cmd3);
                        DataTable dtpassfail = new DataTable();
                        da3.Fill(dtpassfail);
                        List<string> lstpassfail = new List<string>();
                        foreach (DataRow itempassfail in dtpassfail.Rows)
                        {
                            lstpassfail.Add("{\"title\":\"" + itempassfail["PASSFAILS_TITLE"] + "\",\"value\":\"" + itempassfail["PASSFAILS_VALUE"] + "\",\"type\":\"" + itempassfail["PASSFAILS_TYPE"] + "\"," +
                        "\"subsection\":\"" + itempassfail["PASSFAILS_SUBSECTION"] + "\",\"checkListSubsection\":\"" + itempassfail["PASSFAILS_CHECKLISTSUBSECTION"] + "\",\"status\":\"" + itempassfail["PASSFAILS_STATUS"] + "\",\"comment\":\"" + itempassfail["PASSFAILS_COMMENT"] + "\"}");
                        }


                        string sql4 = $@"select UNION_ID
            , ASSIGNMENT_ITEMS_SAMPLED_INSPECTED
            , ASSIGNMENT_ITEMS_INSPECTION_RESULT_ID
            , ASSIGNMENT_ITEMS_INSPECTION_STATUS_ID
            , ASSIGNMENT_ITEMS_QTY_INSPECTED
            , ASSIGNMENT_ITEMS_INSPECTION_COMPLETED_DATE
            , ASSIGNMENT_ITEMS_TOTAL_INSPECTION_MINUTES
            , ASSIGNMENT_ITEMS_SAMPLING_SIZE
            , ASSIGNMENT_ITEMS_QTY_TO_INSPECT
            , ASSIGNMENT_ITEMS_AQL_MINOR
            , ASSIGNMENT_ITEMS_AQL_MAJOR
            , ASSIGNMENT_ITEMS_AQL_MAJOR_A
            , ASSIGNMENT_ITEMS_AQL_MAJOR_B
            , ASSIGNMENT_ITEMS_AQL_CRITICAL
            , ASSIGNMENT_ITEMS_SUPPLIER_BOOKING_MSG
            , ASSIGNMENT_ITEMS_CONCLUSION_REMARKS
            , ASSIGNMENT_ITEMS_ASSIGNMENT_REPORT_TYPE_ID
            , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTOR_USERNAME
            , ASSIGNMENT_ITEMS_ASSIGNMENT_DATE_INSPECTION
            , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_LEVEL
            , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_METHOD
            , ASSIGNMENT_ITEMS_PO_LINE_QTY
            , ASSIGNMENT_ITEMS_PO_LINE_ETD
            , ASSIGNMENT_ITEMS_PO_LINE_ETA
            , ASSIGNMENT_ITEMS_PO_LINE_COLOR
            , ASSIGNMENT_ITEMS_PO_LINE_SIZE
            , ASSIGNMENT_ITEMS_PO_LINE_STYLE
            , ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ID
            , ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ERP_BUSINESS_ID
            , ASSIGNMENT_ITEMS_PO_LINE_PO_NUMBER
            , ASSIGNMENT_ITEMS_PO_LINE_CUSTOMER_PO
            , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ID
            , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ERP_BUSINESS_ID
            , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_PROJECT_ID
            , ASSIGNMENT_ITEMS_PO_LINE_SKU_SKU_NUMBER
            , ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_NAME
            , ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_DESCRIPTION
            , PO_LINE_PROJECT_CODE
            , REPORT_TYPE_NAME
from t_aeqs_to_p88_assignment where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd4 = new OracleCommand(sql4, conmes);
                        cmd4.CommandType = CommandType.Text;
                        OracleDataAdapter da4 = new OracleDataAdapter(cmd4);
                        DataTable dtassignment = new DataTable();
                        da4.Fill(dtassignment);
                        List<string> lstassignment = new List<string>();

                        //Edit on 7/1(PO Change)
                        foreach (DataRow itemassignment in dtassignment.Rows)
                        {
                            //The old data remains unchanged, and the new PO after POChange is transferred to Project Code
                            string id = "\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_PROJECT_ID"] + "\"";
                            string reportTypeKey = "id";
                            string reportTypeValue = itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_REPORT_TYPE_ID"].ToString();
                            if (!string.IsNullOrEmpty(itemassignment["PO_LINE_PROJECT_CODE"].ToString()))
                            {
                                id = "\"project_code\":\"" + itemassignment["PO_LINE_PROJECT_CODE"] + "\"";
                                reportTypeKey = "name";
                                reportTypeValue = itemassignment["REPORT_TYPE_NAME"].ToString();
                            }

                            lstassignment.Add("{\"sampled_inspected\":\"" + itemassignment["ASSIGNMENT_ITEMS_SAMPLED_INSPECTED"] + "\",\"inspection_result_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_INSPECTION_RESULT_ID"] + "\",\"inspection_status_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_INSPECTION_STATUS_ID"] + "\",\"qty_inspected\":\"" + itemassignment["ASSIGNMENT_ITEMS_QTY_INSPECTED"] + "\"," +
                    "\"inspection_completed_date\":\"" + Convert.ToDateTime(itemassignment["ASSIGNMENT_ITEMS_INSPECTION_COMPLETED_DATE"]).ToString("yyyy-MM-ddTHH:mm:ss") + timezone + "\",\"total_inspection_minutes\":\"" + itemassignment["ASSIGNMENT_ITEMS_TOTAL_INSPECTION_MINUTES"] + "\",\"sampling_size\":\"" + itemassignment["ASSIGNMENT_ITEMS_SAMPLING_SIZE"] + "\",\"qty_to_inspect\":\"" + itemassignment["ASSIGNMENT_ITEMS_QTY_TO_INSPECT"] + "\"," +
                    "\"aql_minor\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MINOR"] + "\",\"aql_major\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR"] + "\",\"aql_major_a\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR_A"] + "\",\"aql_major_b\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR_B"] + "\",\"aql_critical\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_CRITICAL"] + "\",\"supplier_booking_msg\":\"" + itemassignment["ASSIGNMENT_ITEMS_SUPPLIER_BOOKING_MSG"] + "\"," +
                    "\"conclusion_remarks\":\"" + itemassignment["ASSIGNMENT_ITEMS_CONCLUSION_REMARKS"] + "\",\"assignment\":{\"report_type\":{\"" + reportTypeKey + "\":\"" + reportTypeValue + "\"},\"inspector\":{\"username\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTOR_USERNAME"] + "\"}," +
                    "\"date_inspection\":\"" + Convert.ToDateTime(itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_DATE_INSPECTION"]).ToString("yyyy-MM-ddTHH:mm:ss") + timezone + "\",\"inspection_level\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_METHOD"] + "\"},\"po_line\":{\"qty\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_QTY"] + "\"," +
                    "\"etd\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_ETD"] + "\",\"eta\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_ETA"] + "\",\"color\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_COLOR"] + "\",\"size\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SIZE"] + "\",\"style\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_STYLE"] + "\",\"po\":{\"exporter\":{\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ID"] + "\"," +
                    "\"erp_business_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ERP_BUSINESS_ID"] + "\"},\"po_number\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_NUMBER"] + "\",\"customer_po\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_CUSTOMER_PO"] + "\",\"importer\":{\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ID"] + "\",\"erp_business_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ERP_BUSINESS_ID"] + "\"}," +
                    "\"project\":{" + id + "}},\"sku\":{\"sku_number\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_SKU_NUMBER"] + "\",\"item_name\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_NAME"] + "\",\"item_description\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_DESCRIPTION"] + "\"}}, \"fields\": {\"string_12\":\"" + item["ASSIGNMENT_ITEMS_FIELDS_STRING_12"] + "\"}}");

                        }

                        List<string> lstpassfail_val = new List<string>();
                        string[] arr = item["PASSFAILS_0_LISTVALUES_VALUE"].ToString().Split('/');
                        //if (arr.Length > 0)
                        //{
                        //    foreach (var item_val in arr)
                        //    {
                        //        lstpassfail_val.Add("{\"value\":\"" + item_val.Trim() + "\"}");
                        //    }

                        //}
                        if (arr[0] != "")
                        {
                            foreach (var item_val in arr)
                            {
                                lstpassfail_val.Add("{\"value\":\"" + item_val.Trim() + "\"}");
                            }

                        }
                        else
                        {
                            lstpassfail_val.Add("{\"value\":\"N/A\"}");
                        }

                        /*var client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                            //var request = new HttpRequestMessage(HttpMethod.Put, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");
                        var request = new HttpRequestMessage(HttpMethod.Put, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");           // P88 Official

                        //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58");//Test

                        request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f");    //official*/

                        string PostJson = "{\"status\":\"" + item["STATUS"] + "\",\"date_started\":\"" + Convert.ToDateTime(item["DATE_STARTED"]).ToString("yyyy-MM-ddTHH:mm:ss") + timezone + "\",\"defective_parts\":" + item["DEFECTIVE_PARTS"] + "," +

                        //sections 
                        //"\"sections\":[" + string.Join(",", lstsections) + "," + string.Join(",", lstsections_f) + "]," +
                        "\"sections\":[" + string.Join(",", lstsections) + ",{\"type\":\"pictures\",\"title\":\"photos\",\"pictures\":[" + string.Join(",", lstsections_f) + "]}]," +
                        //"\"sections\":[" + string.Join(",", lstsections) + "]," +

                        //assignment_items
                        "\"assignment_items\":[" + string.Join(",", lstassignment) + "]," +

                        ////passFails


                        "\"passFails\":[{\"title\":\"" + item["PASSFAILS_0_TITLE"] + "\",\"type\":\"" + item["PASSFAILS_0_TYPE"] + "\",\"subsection\":\"" + item["PASSFAILS_0_SUBSECTION"] + "\",\"listValues\":[" + string.Join(",", lstpassfail_val) + "]}," + string.Join(",", lstpassfail) + "]}";

                        #region Retransmission
                        int maxRetries = 5;
                        int currentRetry = 0;

                        bool retryRequest = true;
                        HttpResponseMessage response = null;



                        try
                        {
                            do
                            {
                                var client = new HttpClient();
                                client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                                //Test
                                var request = new HttpRequestMessage(HttpMethod.Put, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");    // P88 test
                                request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test 
                                //offical
                                //var request = new HttpRequestMessage(HttpMethod.Put, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");           // P88 Official
                                //                                                                                                                                                                       //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58");  //test
                                //request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f");    //official

                                // Create and initialize request content
                                var content = new StringContent(PostJson, null, "application/json");

                                // Set the request content
                                request.Content = content;

                                // send request
                                response = await client.SendAsync(request);

                                // Check the status code, if it is not 500, then jump out of the retry loop
                                if (response.StatusCode != HttpStatusCode.InternalServerError && response.StatusCode != HttpStatusCode.MethodNotAllowed)
                                {
                                    retryRequest = false;
                                }
                                else
                                {
                                    // If it is 500, wait for a while and try again (for example: linear backoff strategy, doubling the delay time each time)
                                    await Task.Delay(2500); // Assume that the interval between each retry is 2.5 seconds, and the specific delay strategy is adjusted according to the actual situation.
                                    if (currentRetry > 0) FMSLOG.Platform("Retransfer JSON : " + item["UNIQUE_KEY"] + $"...{currentRetry}", vsrvinfo.Operation);
                                    currentRetry++;
                                }
                            } while (retryRequest && currentRetry <= maxRetries);
                        }
                        catch (Exception ex)
                        {
                            FMSLOG.Platform("Error JSON : " + item["UNIQUE_KEY"] + $"Exection:{ex.Message}" + PostJson, vsrvinfo.Operation);
                        }

                        Error_Json = await response.Content.ReadAsStringAsync();
                        Sync_StatusCode = ((int)response.StatusCode).ToString();
                        Sync_Message = response.StatusCode.ToString();
                        #endregion





                        /*var content = new StringContent(PostJson, null, "application/json");
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        Json = await response.Content.ReadAsStringAsync();
                        //var jarr = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Json);

                        Sync_StatusCode = ((int)response.StatusCode).ToString();
                        Sync_Message = response.StatusCode.ToString();*/

                        //Sync_StatusCode = jarr["code"].ToString();
                        //Sync_Message = jarr["message"].ToString();
                        response.EnsureSuccessStatusCode();
                        //Console.WriteLine(await response.Content.ReadAsStringAsync());
                        if (response.IsSuccessStatusCode)
                        {
                            //if (PostJson.Contains("'"))
                            //{
                            //    Json_CLOB = PostJson.Replace("'", "''");
                            //}

                            FMSLOG.Platform("Success JSON : " + item["UNIQUE_KEY"] + PostJson, vsrvinfo.Operation);
                            string sql_rtMsg = "UPDATE t_aeqs_to_p88_list SET IS_SYNC='S',SYNC_STATUS_CODE='" + Sync_StatusCode + "',SYNC_DATE=sysdate,SYNC_MESSAGE='" + Sync_Message + "' WHERE UNIQUE_KEY ='" + UniqueKey + "' ";
                            OracleCommand cmd = new OracleCommand(sql_rtMsg, conmes);
                            cmd.CommandType = CommandType.Text;
                            int r = cmd.ExecuteNonQuery();
                            //await POSTAsync(dtsections_f);
                            await POSTAsync_EndOfLine(UniqueKey, dtsections_f);
                            returnMsg = "Success";

                        }
                        //else
                        //{
                        //    string sql_rtMsg = "UPDATE t_aeqs_to_p88_list SET IS_SYNC='N',STATUS_CODE='" + response.StatusCode + "',SYNC_DATE=to_date(sysdate,'yyyymmdd HH24:MI:SS'),SYNC_MESSAGE='" + response.ReasonPhrase + "' WHERE UNIQUE_KEY ='" + item["UNIQUE_KEY"] + "' ";
                        //    cmd = new OracleCommand(sql_rtMsg, conmes);
                        //    cmd.CommandType = CommandType.Text;
                        //    int r = cmd.ExecuteNonQuery();
                        //}


                    }
                    catch (Exception ex)
                    {
                        if (string.IsNullOrEmpty(Json))
                        {
                            Json = "Fail" + ex.ToString();
                        }
                        FMSLOG.Platform("Error JSON Pivot88: " + Json, vsrvinfo.Operation);
                        string sql_rtMsg = "UPDATE t_aeqs_to_p88_list SET IS_SYNC='N',SYNC_STATUS_CODE='" + Sync_StatusCode + "',SYNC_DATE=sysdate,SYNC_MESSAGE='" + Sync_Message + "' WHERE UNIQUE_KEY ='" + UniqueKey + "' ";
                        OracleCommand cmd = new OracleCommand(sql_rtMsg, conmes);
                        cmd.CommandType = CommandType.Text;
                        int r = cmd.ExecuteNonQuery();
                        returnMsg = "Fail" + ex.ToString();

                        //throw;
                    }


                }
            }
            else
            {
                FMSLOG.Platform("No Data to Post SAP", vsrvinfo.Operation);
            }
        }

        //public async Task POSTAsync(DataTable dt_img)
        public async Task POSTAsync_EndOfLine(string UniqueKey, DataTable dt_img1)
        {
            try
            {
                DataTable dt_img = new DataTable();
                string sql = $@"select UNION_ID
                        ,SECTIONS_DEFECTS_PICTURES_TITLE
                        ,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
                        ,SECTIONS_DEFECTS_PICTURES_NUMBER
                        ,SECTIONS_DEFECTS_PICTURES_COMMENT
                        ,SECTION_TYPE
                        ,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID in (select id from  t_aeqs_to_p88_sections where union_id ='" + UniqueKey + "') ";
                OracleCommand cmd = new OracleCommand(sql, conmes);
                cmd.CommandType = CommandType.Text;
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                da.Fill(dt_img);


                if (dt_img.Rows.Count > 0)
                {
                    foreach (DataRow item in dt_img.Rows)
                    {
                        string imgpath = string.Empty;
                        string imgfull_filename = string.Empty;
                        string img_SUFFIX = string.Empty;
                        var client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                        var request = new HttpRequestMessage(HttpMethod.Post, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload"); // P88 test
                      //  var request = new HttpRequestMessage(HttpMethod.Post, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload");         // P88 official
                        request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test
                       // request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f"); //official
                        var content = new MultipartFormDataContent();
                        imgfull_filename = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                        imgpath = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "B");
                        img_SUFFIX = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "C");
                        //content.Add(new StreamContent(File.OpenRead("E:\\SAP\\MES\\Final_Version_SourceCode\\MES&WMS(SAP Version)\\" +
                        //    "MES&WMS to SAP(Interface)\\Development\\POST_TO_PIVOT88\\POST_TO_SAP\\FMSPlatForm\\FMS\\images\\upload\\20230510170946286.jpg")),
                        //    "file", "20230510170946286.jpg");

                        var webC = new System.Net.WebClient();
                        string url = imgpath.Replace("\\", "/");
                        Image image = new Bitmap(webC.OpenRead(url));
                        MemoryStream stream = new MemoryStream();
                        if (img_SUFFIX == "png") { image.Save(stream, ImageFormat.Png); } else if (img_SUFFIX == "jpg") { image.Save(stream, ImageFormat.Jpeg); }
                        image.Save(stream, ImageFormat.Png);
                        stream.Seek(0, SeekOrigin.Begin); //Need to reset position to 0

                        content.Add(new StreamContent(stream), "file", imgfull_filename);
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        //Console.WriteLine(await response.Content.ReadAsStringAsync());
                    }
                }
                if (dt_img1.Rows.Count > 0)
                {
                    foreach (DataRow item in dt_img1.Rows)
                    {
                        string imgpath = string.Empty;
                        string imgfull_filename = string.Empty;
                        string img_SUFFIX = string.Empty;
                        var client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                        var request = new HttpRequestMessage(HttpMethod.Post, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload"); // P88 test
                       // var request = new HttpRequestMessage(HttpMethod.Post, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload");         // P88 official
                        request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test
                       // request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f"); //official
                        var content = new MultipartFormDataContent();
                        imgfull_filename = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                        imgpath = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "B");
                        img_SUFFIX = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "C");
                        //content.Add(new StreamContent(File.OpenRead("E:\\SAP\\MES\\Final_Version_SourceCode\\MES&WMS(SAP Version)\\" +
                        //    "MES&WMS to SAP(Interface)\\Development\\POST_TO_PIVOT88\\POST_TO_SAP\\FMSPlatForm\\FMS\\images\\upload\\20230510170946286.jpg")),
                        //    "file", "20230510170946286.jpg");

                        var webC = new System.Net.WebClient();
                        string url = imgpath.Replace("\\", "/");
                        Image image = new Bitmap(webC.OpenRead(url));
                        MemoryStream stream = new MemoryStream();
                        if (img_SUFFIX == "png") { image.Save(stream, ImageFormat.Png); } else if (img_SUFFIX == "jpg") { image.Save(stream, ImageFormat.Jpeg); }
                        image.Save(stream, ImageFormat.Png);
                        stream.Seek(0, SeekOrigin.Begin); //Need to reset position to 0

                        content.Add(new StreamContent(stream), "file", imgfull_filename);
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        //Console.WriteLine(await response.Content.ReadAsStringAsync());
                    }
                }

            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show(ex.ToString());
                throw;
            }
        }


        #endregion

        #region EndOfLine_ReWork

        public async void PostRequestAsync_EndOfLine_ReWork(SrvInfo vsrvinfo)
        {
            try
            {
                string constrmes = null;
                string filePath;

                filePath = Application.ExecutablePath;
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                string clientEnvConfigFileName = filePath + "database.config";


                if (File.Exists(clientEnvConfigFileName))
                {
                    FileLoader obj = new FileLoader(clientEnvConfigFileName);
                    Hashtable htdblinks = obj.GetDBLinks();
                    if (htdblinks.ContainsKey(vsrvinfo.SDB))
                        constrmes = htdblinks[vsrvinfo.SDB].ToString();

                    conmes = new OracleConnection(constrmes);
                    //OracleTransaction objTrans = null;

                    int updatecount = 0;
                    int errorcount = 0;
                    //await PushNotificationTestAsync();
                    //string errmsg = "";
                    //string sql = "select voucher_no,MAX(CREATE_DATE),SYNC_TIMES from wms_to_sap_list where (is_sync='N' or is_sync is null) and SYNC_TIMES < 3 and  INTERFACE_NO='" + interface_no + "' group by voucher_no,SYNC_TIMES";
                    //OracleCommand cmd = new OracleCommand(sql, conmes);
                    //cmd.CommandType = CommandType.Text;
                    //OracleDataAdapter da = new OracleDataAdapter(cmd);
                    //DataTable dtcheck = new DataTable();
                    //da.Fill(dtcheck);

                    //if (dtcheck.Rows.Count > 0)
                    //{
                    //FMSLOG.Platform("Data Retrived Successfull..(" + dtcheck.Rows.Count + ")Records on " + vsrvinfo.StartDate, vsrvinfo.Operation);
                    FMSLOG.Platform("Data Post Started " + vsrvinfo.StartDate, vsrvinfo.Operation);

                    conmes.Open();
                    //objTrans = conoa.BeginTransaction();

                    await PUTAsync_EndOfLine_ReWork(vsrvinfo);

                    //System.Threading.Thread.Sleep(30000);
                    //objTrans.Commit();

                    FMSLOG.Platform("Data Send Status : " + returnMsg + "", vsrvinfo.Operation);
                    //SendMailAlert(vsrvinfo);
                    //}
                    //else
                    //{
                    //    FMSLOG.Platform("No Data to Post SAP", vsrvinfo.Operation);
                    //}
                }
                else
                {
                    FMSLOG.Platform("No Databases Exists..", vsrvinfo.Operation);
                }
            }
            catch (Exception ex)
            {
                //string sql = "UPDATE  wms_to_sap_list SET SYNC_DATE=to_date('" + inp.synctime + "','yyyymmdd HH24:MI:SS'),SYNC_TIMES=nvl(sync_times,0)+1   WHERE UUID='" + inp.UUID + "' ";
                //OracleCommand cmd = new OracleCommand(sql, conmes);
                //cmd.CommandType = CommandType.Text;
                //int r = cmd.ExecuteNonQuery();
                FMSLOG.Platform(MethodBase.GetCurrentMethod().Name + " Exception : " + ex.Message, vsrvinfo.Operation);
            }
            finally
            {
                conmes.Close();
                conmes.Dispose();

                GC.Collect();
            }
        }

        public async Task PUTAsync_EndOfLine_ReWork(SrvInfo vsrvinfo)
        {
            string UniqueKey = string.Empty;
            var Error_Json = string.Empty;

            string Json = string.Empty;
            try
            {
                string sql = $@"select 
                                UNIQUE_KEY
                                ,l.STATUS
                                , l.DATE_STARTED
                                , l.DEFECTIVE_PARTS
                                , l.PASSFAILS_0_TITLE
                                , l.PASSFAILS_0_TYPE
                                , l.PASSFAILS_0_SUBSECTION
                                , l.PASSFAILS_0_LISTVALUES_VALUE
                                ,l.assignment_items_fields_string_12
                                from t_aeqs_to_p88_list l 
                                where 
                                --to_char(l.date_started,'yyyy/mm/dd')= to_char(sysdate,'yyyy/mm/dd') and 
                                l.assignment_items_assignment_report_type_id=35
--and  l.UNIQUE_KEY in ('apache1_2924')
                                and (IS_SYNC IS NULL or IS_SYNC='N') order by l.UNIQUE_KEY
                                ";



                OracleCommand cmd = new OracleCommand(sql, conmes);
                cmd.CommandType = CommandType.Text;
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataTable dtlist = new DataTable();
                da.Fill(dtlist);
                if (dtlist.Rows.Count > 0)
                {
                    foreach (DataRow item in dtlist.Rows)
                    {
                        UniqueKey = item["UNIQUE_KEY"].ToString();
                        DataTable dtsections_f = new DataTable();
                        string sql1 = $@"select ID
,UNION_ID
,SECTIONS_TYPE
,SECTIONS_TITLE
,SECTIONS_RESULT_ID
,SECTIONS_QTY_INSPECTED
,SECTIONS_SAMPLED_INSPECTED
,SECTIONS_DEFECTIVE_PARTS
,SECTIONS_INSPECTION_LEVEL
,SECTIONS_INSPECTION_METHOD
,SECTIONS_AQL_MINOR
,SECTIONS_AQL_MAJOR
,SECTIONS_AQL_CRITICAL
,SECTIONS_BARCODES_VALUE
,SECTIONS_QTY_TYPE
,SECTIONS_MAX_MINOR_DEFECTS
,SECTIONS_MAX_MAJOR_DEFECTS
,SECTIONS_MAX_MAJOR_A_DEFECTS
,SECTIONS_MAX_MAJOR_B_DEFECTS
,SECTIONS_MAX_CRITICAL_DEFECTS
,SECTIONS_DEFECTS_LABEL
,SECTIONS_DEFECTS_SUBSECTION
,SECTIONS_DEFECTS_CODE
,SECTIONS_DEFECTS_CRITICAL_LEVEL
,SECTIONS_DEFECTS_MAJOR_LEVEL
,SECTIONS_DEFECTS_MINOR_LEVEL
,SECTIONS_DEFECTS_COMMENTS from t_aeqs_to_p88_sections where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd1 = new OracleCommand(sql1, conmes);
                        cmd1.CommandType = CommandType.Text;
                        OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
                        DataTable dtsections = new DataTable();
                        da1.Fill(dtsections);

                        DataRow[] dr_pl = null;
                        DataRow[] dr_p = null;
                        dr_pl = dtsections.Select($"SECTIONS_TITLE='packing_packaging_labelling'");
                        dr_p = dtsections.Select($"SECTIONS_TITLE='product'");
                        List<string> defects_pl = new List<string>();
                        List<string> defects_p = new List<string>();





                        foreach (var pl in dr_pl)
                        {
                            List<string> defects_pl_pic = new List<string>();
                            DataTable dtsections_f_pl_pic = new DataTable();
                            string sqlpic = $@"select ID
,UNION_ID
,SECTIONS_DEFECTS_PICTURES_TITLE
,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
,SECTIONS_DEFECTS_PICTURES_NUMBER
,SECTIONS_DEFECTS_PICTURES_COMMENT
,SECTION_TYPE
,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{pl["ID"]}'";
                            OracleCommand cmdpic = new OracleCommand(sqlpic, conmes);
                            cmdpic.CommandType = CommandType.Text;
                            OracleDataAdapter dapic = new OracleDataAdapter(cmdpic);

                            dapic.Fill(dtsections_f_pl_pic);

                            foreach (DataRow itemsections_f_pl_pic in dtsections_f_pl_pic.Rows)
                            {
                                string full_filename = GetImageData(itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                                defects_pl_pic.Add("{\"title\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\": \"" + full_filename + "\",\"number\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");

                            }


                            defects_pl.Add("{\"label\":\"" + pl["SECTIONS_DEFECTS_LABEL"] + "\"," +
                            "\"subsection\":\"" + pl["SECTIONS_DEFECTS_SUBSECTION"] + "\",\"code\":\"" + pl["SECTIONS_DEFECTS_CODE"] + "\",\"critical_level\":\"" + pl["SECTIONS_DEFECTS_CRITICAL_LEVEL"] + "\",\"major_level\":\"" + pl["SECTIONS_DEFECTS_MAJOR_LEVEL"] + "\",\"minor_level\":\"" + pl["SECTIONS_DEFECTS_MINOR_LEVEL"] + "\"," +
                            "\"comments\":\"" + pl["SECTIONS_DEFECTS_COMMENTS"] + "\",\"pictures\":[" + string.Join(",", defects_pl_pic) + "]}");
                        }
                        foreach (var p in dr_p)
                        {
                            List<string> defects_p_pic = new List<string>();
                            DataTable dtsections_f_p_pic = new DataTable();
                            string sqlpic = $@"select UNION_ID
,SECTIONS_DEFECTS_PICTURES_TITLE
,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
,SECTIONS_DEFECTS_PICTURES_NUMBER
,SECTIONS_DEFECTS_PICTURES_COMMENT
,SECTION_TYPE
,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{p["ID"]}'";
                            OracleCommand cmdpic = new OracleCommand(sqlpic, conmes);
                            cmdpic.CommandType = CommandType.Text;
                            OracleDataAdapter dapic = new OracleDataAdapter(cmdpic);

                            dapic.Fill(dtsections_f_p_pic);

                            foreach (DataRow itemsections_f_pl_pic in dtsections_f_p_pic.Rows)
                            {
                                string full_filename = GetImageData(itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                                defects_p_pic.Add("{\"title\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\": \"" + full_filename + "\",\"number\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\": \"" + itemsections_f_pl_pic["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");

                            }


                            defects_p.Add("{\"label\":\"" + p["SECTIONS_DEFECTS_LABEL"] + "\"," +
                            "\"subsection\":\"" + p["SECTIONS_DEFECTS_SUBSECTION"] + "\",\"code\":\"" + p["SECTIONS_DEFECTS_CODE"] + "\",\"critical_level\":\"" + p["SECTIONS_DEFECTS_CRITICAL_LEVEL"] + "\",\"major_level\":\"" + p["SECTIONS_DEFECTS_MAJOR_LEVEL"] + "\",\"minor_level\":\"" + p["SECTIONS_DEFECTS_MINOR_LEVEL"] + "\"," +
                            "\"comments\":\"" + p["SECTIONS_DEFECTS_COMMENTS"] + "\",\"pictures\":[" + string.Join(",", defects_p_pic) + "]}");
                        }

                        List<string> lstsections = new List<string>();


                        lstsections.Add("{\"type\":\"" + dr_pl[0]["SECTIONS_TYPE"] + "\",\"title\":\"" + dr_pl[0]["SECTIONS_TITLE"] + "\",\"section_result_id\":\"" + dr_pl[0]["SECTIONS_RESULT_ID"] + "\",\"qty_inspected\":\"" + dr_pl[0]["SECTIONS_QTY_INSPECTED"] + "\"," +
                    "\"sampled_inspected\":\"" + dr_pl[0]["SECTIONS_SAMPLED_INSPECTED"] + "\",\"defective_parts\":\"" + dr_pl[0]["SECTIONS_DEFECTIVE_PARTS"] + "\",\"inspection_level\":\"" + dr_pl[0]["SECTIONS_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + dr_pl[0]["SECTIONS_INSPECTION_METHOD"] + "\",\"aql_minor\":\"" + dr_pl[0]["SECTIONS_AQL_MINOR"] + "\"," +
                    "\"aql_major\":\"" + dr_pl[0]["SECTIONS_AQL_MAJOR"] + "\",\"aql_critical\":\"" + dr_pl[0]["SECTIONS_AQL_CRITICAL"] + "\",\"barcodes\":[{\"value\":\"001\"}]," +
                    "\"qty_type\":\"" + dr_pl[0]["SECTIONS_QTY_TYPE"] + "\",\"max_minor_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MINOR_DEFECTS"] + "\",\"max_major_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_DEFECTS"] + "\",\"max_major_a_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_A_DEFECTS"] + "\",\"max_major_b_defects\":\"" + dr_pl[0]["SECTIONS_MAX_MAJOR_B_DEFECTS"] + "\",\"max_critical_defects\":\"" + dr_pl[0]["SECTIONS_MAX_CRITICAL_DEFECTS"] + "\"," +
                    "\"defects\":[" + string.Join(",", defects_pl) + "]}");

                        lstsections.Add("{\"type\":\"" + dr_p[0]["SECTIONS_TYPE"] + "\",\"title\":\"" + dr_p[0]["SECTIONS_TITLE"] + "\",\"section_result_id\":\"" + dr_p[0]["SECTIONS_RESULT_ID"] + "\",\"qty_inspected\":\"" + dr_p[0]["SECTIONS_QTY_INSPECTED"] + "\"," +
                    "\"sampled_inspected\":\"" + dr_p[0]["SECTIONS_SAMPLED_INSPECTED"] + "\",\"defective_parts\":\"" + dr_p[0]["SECTIONS_DEFECTIVE_PARTS"] + "\",\"inspection_level\":\"" + dr_p[0]["SECTIONS_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + dr_p[0]["SECTIONS_INSPECTION_METHOD"] + "\",\"aql_minor\":\"" + dr_p[0]["SECTIONS_AQL_MINOR"] + "\"," +
                    "\"aql_major\":\"" + dr_p[0]["SECTIONS_AQL_MAJOR"] + "\",\"aql_critical\":\"" + dr_p[0]["SECTIONS_AQL_CRITICAL"] + "\"," +
                    "\"qty_type\":\"" + dr_p[0]["SECTIONS_QTY_TYPE"] + "\",\"max_minor_defects\":\"" + dr_p[0]["SECTIONS_MAX_MINOR_DEFECTS"] + "\",\"max_major_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_DEFECTS"] + "\",\"max_major_a_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_A_DEFECTS"] + "\",\"max_major_b_defects\":\"" + dr_p[0]["SECTIONS_MAX_MAJOR_B_DEFECTS"] + "\",\"max_critical_defects\":\"" + dr_p[0]["SECTIONS_MAX_CRITICAL_DEFECTS"] + "\"," +
                    "\"defects\":[" + string.Join(",", defects_p) + "]}");



                        string sql2 = $@"select UNION_ID
                        ,SECTIONS_DEFECTS_PICTURES_TITLE
                        ,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
                        ,SECTIONS_DEFECTS_PICTURES_NUMBER
                        ,SECTIONS_DEFECTS_PICTURES_COMMENT
                        ,SECTION_TYPE
                        ,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd2 = new OracleCommand(sql2, conmes);
                        cmd2.CommandType = CommandType.Text;
                        OracleDataAdapter da2 = new OracleDataAdapter(cmd2);

                        da2.Fill(dtsections_f);
                        List<string> lstsections_f = new List<string>();
                        //if (dtsections_f.Rows.Count > 0)
                        //{
                        foreach (DataRow itemsections_f in dtsections_f.Rows)
                        {
                            string full_filename = GetImageData(itemsections_f["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                            //lstsections_f.Add("{\"type\":\"" + itemsections_f["SECTION_TYPE"] + "\",\"title\":\"" + itemsections_f["SECTION_TITLE"] + "\",\"pictures\":[{\"title\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\":\"" + full_filename + "\",\"number\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}]}");
                            lstsections_f.Add("{\"title\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\":\"" + full_filename + "\",\"number\":\"\",\"comment\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}");
                        }
                        //}
                        //else
                        //{
                        //    lstsections_f.Add("");
                        //}


                        string sql3 = $@"select UNION_ID
,PASSFAILS_TITLE
,PASSFAILS_VALUE
,PASSFAILS_TYPE
,PASSFAILS_SUBSECTION
,PASSFAILS_CHECKLISTSUBSECTION
,PASSFAILS_STATUS
,PASSFAILS_COMMENT
 from t_aeqs_to_p88_passfail where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd3 = new OracleCommand(sql3, conmes);
                        cmd3.CommandType = CommandType.Text;
                        OracleDataAdapter da3 = new OracleDataAdapter(cmd3);
                        DataTable dtpassfail = new DataTable();
                        da3.Fill(dtpassfail);
                        List<string> lstpassfail = new List<string>();
                        foreach (DataRow itempassfail in dtpassfail.Rows)
                        {
                            lstpassfail.Add("{\"title\":\"" + itempassfail["PASSFAILS_TITLE"] + "\",\"value\":\"" + itempassfail["PASSFAILS_VALUE"] + "\",\"type\":\"" + itempassfail["PASSFAILS_TYPE"] + "\"," +
                        "\"subsection\":\"" + itempassfail["PASSFAILS_SUBSECTION"] + "\",\"checkListSubsection\":\"" + itempassfail["PASSFAILS_CHECKLISTSUBSECTION"] + "\",\"status\":\"" + itempassfail["PASSFAILS_STATUS"] + "\",\"comment\":\"" + itempassfail["PASSFAILS_COMMENT"] + "\"}");
                        }


                        string sql4 = $@"select UNION_ID
                , ASSIGNMENT_ITEMS_SAMPLED_INSPECTED
                , ASSIGNMENT_ITEMS_INSPECTION_RESULT_ID
                , ASSIGNMENT_ITEMS_INSPECTION_STATUS_ID
                , ASSIGNMENT_ITEMS_QTY_INSPECTED
                , ASSIGNMENT_ITEMS_INSPECTION_COMPLETED_DATE
                , ASSIGNMENT_ITEMS_TOTAL_INSPECTION_MINUTES
                , ASSIGNMENT_ITEMS_SAMPLING_SIZE
                , ASSIGNMENT_ITEMS_QTY_TO_INSPECT
                , ASSIGNMENT_ITEMS_AQL_MINOR
                , ASSIGNMENT_ITEMS_AQL_MAJOR
                , ASSIGNMENT_ITEMS_AQL_MAJOR_A
                , ASSIGNMENT_ITEMS_AQL_MAJOR_B
                , ASSIGNMENT_ITEMS_AQL_CRITICAL
                , ASSIGNMENT_ITEMS_SUPPLIER_BOOKING_MSG
                , ASSIGNMENT_ITEMS_CONCLUSION_REMARKS
                , ASSIGNMENT_ITEMS_ASSIGNMENT_REPORT_TYPE_ID
                , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTOR_USERNAME
                , ASSIGNMENT_ITEMS_ASSIGNMENT_DATE_INSPECTION
                , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_LEVEL
                , ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_METHOD
                , ASSIGNMENT_ITEMS_PO_LINE_QTY
                , ASSIGNMENT_ITEMS_PO_LINE_ETD
                , ASSIGNMENT_ITEMS_PO_LINE_ETA
                , ASSIGNMENT_ITEMS_PO_LINE_COLOR
                , ASSIGNMENT_ITEMS_PO_LINE_SIZE
                , ASSIGNMENT_ITEMS_PO_LINE_STYLE
                , ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ID
                , ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ERP_BUSINESS_ID
                , ASSIGNMENT_ITEMS_PO_LINE_PO_NUMBER
                , ASSIGNMENT_ITEMS_PO_LINE_CUSTOMER_PO
                , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ID
                , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ERP_BUSINESS_ID
                , ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_PROJECT_ID
                , ASSIGNMENT_ITEMS_PO_LINE_SKU_SKU_NUMBER
                , ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_NAME
                , ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_DESCRIPTION
                , PO_LINE_PROJECT_CODE
                , REPORT_TYPE_NAME
 from t_aeqs_to_p88_assignment where UNION_ID ='{item["UNIQUE_KEY"]}'";
                        OracleCommand cmd4 = new OracleCommand(sql4, conmes);
                        cmd4.CommandType = CommandType.Text;
                        OracleDataAdapter da4 = new OracleDataAdapter(cmd4);
                        DataTable dtassignment = new DataTable();
                        da4.Fill(dtassignment);
                        List<string> lstassignment = new List<string>();
                        foreach (DataRow itemassignment in dtassignment.Rows)
                        {
                            //The old data remains unchanged, and the new PO after POChange is transferred to Project Code
                            string id = "\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_PROJECT_ID"] + "\"";
                            string reportTypeKey = "id";
                            string reportTypeValue = itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_REPORT_TYPE_ID"].ToString();
                            if (!string.IsNullOrEmpty(itemassignment["PO_LINE_PROJECT_CODE"].ToString()))
                            {
                                id = "\"project_code\":\"" + itemassignment["PO_LINE_PROJECT_CODE"] + "\"";
                                reportTypeKey = "name";
                                reportTypeValue = itemassignment["REPORT_TYPE_NAME"].ToString();
                            }

                            lstassignment.Add("{\"sampled_inspected\":\"" + itemassignment["ASSIGNMENT_ITEMS_SAMPLED_INSPECTED"] + "\",\"inspection_result_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_INSPECTION_RESULT_ID"] + "\",\"inspection_status_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_INSPECTION_STATUS_ID"] + "\",\"qty_inspected\":\"" + itemassignment["ASSIGNMENT_ITEMS_QTY_INSPECTED"] + "\"," +
                       "\"inspection_completed_date\":\"" + Convert.ToDateTime(itemassignment["ASSIGNMENT_ITEMS_INSPECTION_COMPLETED_DATE"]).ToString("yyyy-MM-ddTHH:mm:ss") + timezone + "\",\"total_inspection_minutes\":\"" + itemassignment["ASSIGNMENT_ITEMS_TOTAL_INSPECTION_MINUTES"] + "\",\"sampling_size\":\"" + itemassignment["ASSIGNMENT_ITEMS_SAMPLING_SIZE"] + "\",\"qty_to_inspect\":\"" + itemassignment["ASSIGNMENT_ITEMS_QTY_TO_INSPECT"] + "\"," +
                       "\"aql_minor\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MINOR"] + "\",\"aql_major\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR"] + "\",\"aql_major_a\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR_A"] + "\",\"aql_major_b\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_MAJOR_B"] + "\",\"aql_critical\":\"" + itemassignment["ASSIGNMENT_ITEMS_AQL_CRITICAL"] + "\",\"supplier_booking_msg\":\"" + itemassignment["ASSIGNMENT_ITEMS_SUPPLIER_BOOKING_MSG"] + "\"," +
                       "\"conclusion_remarks\":\"" + itemassignment["ASSIGNMENT_ITEMS_CONCLUSION_REMARKS"] + "\",\"assignment\":{\"report_type\":{\"" + reportTypeKey + "\":\"" + reportTypeValue + "\"},\"inspector\":{\"username\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTOR_USERNAME"] + "\"}," +
                       "\"date_inspection\":\"" + Convert.ToDateTime(itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_DATE_INSPECTION"]).ToString("yyyy-MM-ddTHH:mm:ss") + timezone + "\",\"inspection_level\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + itemassignment["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_METHOD"] + "\"},\"po_line\":{\"qty\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_QTY"] + "\"," +
                       "\"etd\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_ETD"] + "\",\"eta\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_ETA"] + "\",\"color\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_COLOR"] + "\",\"size\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SIZE"] + "\",\"style\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_STYLE"] + "\",\"po\":{\"exporter\":{\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ID"] + "\"," +
                       "\"erp_business_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ERP_BUSINESS_ID"] + "\"},\"po_number\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_PO_NUMBER"] + "\",\"customer_po\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_CUSTOMER_PO"] + "\",\"importer\":{\"id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ID"] + "\",\"erp_business_id\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ERP_BUSINESS_ID"] + "\"}," +
                       "\"project\":{" + id + "}},\"sku\":{\"sku_number\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_SKU_NUMBER"] + "\",\"item_name\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_NAME"] + "\",\"item_description\":\"" + itemassignment["ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_DESCRIPTION"] + "\"}}, \"fields\": {\"string_12\":\"" + item["ASSIGNMENT_ITEMS_FIELDS_STRING_12"] + "\"}}");
                        }

                        List<string> lstpassfail_val = new List<string>();
                        string[] arr = item["PASSFAILS_0_LISTVALUES_VALUE"].ToString().Split('/');
                        //if (arr.Length > 0)
                        //{
                        //    foreach (var item_val in arr)
                        //    {
                        //        lstpassfail_val.Add("{\"value\":\"" + item_val.Trim() + "\"}");
                        //    }

                        //}
                        if (arr[0] != "")
                        {
                            foreach (var item_val in arr)
                            {
                                lstpassfail_val.Add("{\"value\":\"" + item_val.Trim() + "\"}");
                            }

                        }
                        else
                        {
                            lstpassfail_val.Add("{\"value\":\"N/A\"}");
                        }

                        //var client = new HttpClient();

                        //Test
                        //var request = new HttpRequestMessage(HttpMethod.Put, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");    // P88 test
                       // request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58");  //test


                        //offical
                        /*var request = new HttpRequestMessage(HttpMethod.Put, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");           // P88 Official
                        request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f");    //official
*/
                        string PostJson = "{\"status\":\"" + item["STATUS"] + "\",\"date_started\":\"" + Convert.ToDateTime(item["DATE_STARTED"]).ToString("yyyy-MM-ddTHH:mm:ss") + "\",\"defective_parts\":" + item["DEFECTIVE_PARTS"] + "," +

                        //sections 
                        //"\"sections\":[" + string.Join(",", lstsections) + "," + string.Join(",", lstsections_f) + "]," +
                        "\"sections\":[" + string.Join(",", lstsections) + ",{\"type\":\"pictures\",\"title\":\"photos\",\"pictures\":[" + string.Join(",", lstsections_f) + "]}]," +
                        //"\"sections\":[" + string.Join(",", lstsections) + "]," +

                        //assignment_items
                        "\"assignment_items\":[" + string.Join(",", lstassignment) + "]," +

                        ////passFails


                        "\"passFails\":[{\"title\":\"" + item["PASSFAILS_0_TITLE"] + "\",\"type\":\"" + item["PASSFAILS_0_TYPE"] + "\",\"subsection\":\"" + item["PASSFAILS_0_SUBSECTION"] + "\",\"listValues\":[" + string.Join(",", lstpassfail_val) + "]}," + string.Join(",", lstpassfail) + "]}";


                        #region Retransmission
                        int maxRetries = 5;
                        int currentRetry = 0;

                        bool retryRequest = true;
                        HttpResponseMessage response = null;

                        do
                        {
                            try
                            {

                                var client = new HttpClient();
                                client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                                //Test
                                var request = new HttpRequestMessage(HttpMethod.Put, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");    // P88 test

                                request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test  

                                //offical
                                //var request = new HttpRequestMessage(HttpMethod.Put, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + item["UNIQUE_KEY"] + " ");           // P88 Official 
                                //request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f");    //official

                                // Create and initialize request content
                                var content = new StringContent(PostJson, null, "application/json");

                                // Set the request content
                                request.Content = content;

                                // send request
                                response = await client.SendAsync(request);

                                // If it is 500, wait for a while and try again (for example: linear backoff strategy, doubling the delay time each time)
                                if (response.StatusCode != HttpStatusCode.InternalServerError && response.StatusCode != HttpStatusCode.MethodNotAllowed)
                                {
                                    retryRequest = false;
                                }
                                else
                                {
                                    // If it is 500, wait for a while and try again (for example: linear backoff strategy, doubling the delay time each time)
                                    await Task.Delay(2500); // Assume that the interval between each retry is 2.5 seconds, and the specific delay strategy is adjusted according to the actual situation.
                                    if (currentRetry > 0) FMSLOG.Platform("Retransfer JSON : " + item["UNIQUE_KEY"] + $"...{currentRetry}", vsrvinfo.Operation);
                                    currentRetry++;
                                }

                            }
                            catch (Exception ex)
                            {
                                FMSLOG.Platform("Error JSON : " + item["UNIQUE_KEY"] + $"Exection:{ex.Message}" + PostJson, vsrvinfo.Operation);
                            }
                        } while (retryRequest && currentRetry <= maxRetries);

                        Error_Json = await response.Content.ReadAsStringAsync();
                        Sync_StatusCode = ((int)response.StatusCode).ToString();
                        Sync_Message = response.StatusCode.ToString();
                        #endregion

                        /*var content = new StringContent(PostJson, null, "application/json");
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        Json = await response.Content.ReadAsStringAsync();
                        //var jarr = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Json);

                        Sync_StatusCode = ((int)response.StatusCode).ToString();
                        Sync_Message = response.StatusCode.ToString();*/

                        //Sync_StatusCode = jarr["code"].ToString();
                        //Sync_Message = jarr["message"].ToString();
                        response.EnsureSuccessStatusCode();
                        //Console.WriteLine(await response.Content.ReadAsStringAsync());
                        if (response.IsSuccessStatusCode)
                        {
                            //if (PostJson.Contains("'"))
                            //{
                            //    Json_CLOB = PostJson.Replace("'", "''");
                            //}
                            FMSLOG.Platform("Success JSON : " + item["UNIQUE_KEY"] + PostJson, vsrvinfo.Operation);
                            string sql_rtMsg = "UPDATE t_aeqs_to_p88_list SET IS_SYNC='S',SYNC_STATUS_CODE='" + Sync_StatusCode + "',SYNC_DATE=sysdate,SYNC_MESSAGE='" + Sync_Message + "' WHERE UNIQUE_KEY ='" + UniqueKey + "' ";
                            cmd = new OracleCommand(sql_rtMsg, conmes);
                            cmd.CommandType = CommandType.Text;
                            int r = cmd.ExecuteNonQuery();
                            //await POSTAsync(dtsections_f);
                            await POSTAsync_EndOfLine_ReWork(UniqueKey, dtsections_f);
                            returnMsg = "Success";

                        }
                        //else
                        //{
                        //    string sql_rtMsg = "UPDATE t_aeqs_to_p88_list SET IS_SYNC='N',STATUS_CODE='" + response.StatusCode + "',SYNC_DATE=to_date(sysdate,'yyyymmdd HH24:MI:SS'),SYNC_MESSAGE='" + response.ReasonPhrase + "' WHERE UNIQUE_KEY ='" + item["UNIQUE_KEY"] + "' ";
                        //    cmd = new OracleCommand(sql_rtMsg, conmes);
                        //    cmd.CommandType = CommandType.Text;
                        //    int r = cmd.ExecuteNonQuery();
                        //}
                    }
                }
                else
                {
                    FMSLOG.Platform("No Data to Post SAP", vsrvinfo.Operation);
                }



            }
            catch (HttpRequestException ex)
            {
                FMSLOG.Platform("Error JSON Pivot88: " + Json, vsrvinfo.Operation);
                string sql_rtMsg = "UPDATE t_aeqs_to_p88_list SET IS_SYNC='N',SYNC_STATUS_CODE='" + Sync_StatusCode + "',SYNC_DATE=sysdate,SYNC_MESSAGE='" + Sync_Message + "' WHERE UNIQUE_KEY ='" + UniqueKey + "' ";
                OracleCommand cmd = new OracleCommand(sql_rtMsg, conmes);
                cmd.CommandType = CommandType.Text;
                int r = cmd.ExecuteNonQuery();
                returnMsg = "Fail" + ex.ToString();
                throw;
            }
        }

        //public async Task POSTAsync(DataTable dt_img)
        public async Task POSTAsync_EndOfLine_ReWork(string UniqueKey, DataTable dt_img1)
        {
            try
            {
                DataTable dt_img = new DataTable();
                string sql = $@"select UNION_ID
                        ,SECTIONS_DEFECTS_PICTURES_TITLE
                        ,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
                        ,SECTIONS_DEFECTS_PICTURES_NUMBER
                        ,SECTIONS_DEFECTS_PICTURES_COMMENT
                        ,SECTION_TYPE
                        ,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID in (select id from  t_aeqs_to_p88_sections where union_id ='" + UniqueKey + "') ";
                OracleCommand cmd = new OracleCommand(sql, conmes);
                cmd.CommandType = CommandType.Text;
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                da.Fill(dt_img);


                if (dt_img.Rows.Count > 0)
                {
                    foreach (DataRow item in dt_img.Rows)
                    {
                        string imgpath = string.Empty;
                        string imgfull_filename = string.Empty;
                        string img_SUFFIX = string.Empty;
                        var client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                        var request = new HttpRequestMessage(HttpMethod.Post, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload"); // P88 test
                        request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test


                        //var request = new HttpRequestMessage(HttpMethod.Post, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload");         // P88 official
                        //request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f"); //official
                        var content = new MultipartFormDataContent();
                        imgfull_filename = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                        imgpath = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "B");
                        img_SUFFIX = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "C");
                        //content.Add(new StreamContent(File.OpenRead("E:\\SAP\\MES\\Final_Version_SourceCode\\MES&WMS(SAP Version)\\" +
                        //    "MES&WMS to SAP(Interface)\\Development\\POST_TO_PIVOT88\\POST_TO_SAP\\FMSPlatForm\\FMS\\images\\upload\\20230510170946286.jpg")),
                        //    "file", "20230510170946286.jpg");

                        var webC = new System.Net.WebClient();
                        string url = imgpath.Replace("\\", "/");
                        Image image = new Bitmap(webC.OpenRead(url));
                        MemoryStream stream = new MemoryStream();
                        if (img_SUFFIX == "png") { image.Save(stream, ImageFormat.Png); } else if (img_SUFFIX == "jpg") { image.Save(stream, ImageFormat.Jpeg); }
                        image.Save(stream, ImageFormat.Png);
                        stream.Seek(0, SeekOrigin.Begin); //Need to reset position to 0

                        content.Add(new StreamContent(stream), "file", imgfull_filename);
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        //Console.WriteLine(await response.Content.ReadAsStringAsync());
                    }
                }
                if (dt_img1.Rows.Count > 0)
                {
                    foreach (DataRow item in dt_img1.Rows)
                    {
                        string imgpath = string.Empty;
                        string imgfull_filename = string.Empty;
                        string img_SUFFIX = string.Empty;
                        var client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(requestTimeout);
                        var request = new HttpRequestMessage(HttpMethod.Post, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload"); // P88 test
                        request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58"); //Test


                        //var request = new HttpRequestMessage(HttpMethod.Post, "https://adidas.pivot88.com/rest/operation/v1/inspection_reports/unique_key:" + UniqueKey + "/images/upload");         // P88 official
                        //request.Headers.Add("api-key", "6f7c5290-1a52-446a-8e8f-3400368b491f"); //official
                        var content = new MultipartFormDataContent();
                        imgfull_filename = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "A");
                        imgpath = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "B");
                        img_SUFFIX = GetImageData(item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"].ToString(), "C");
                        //content.Add(new StreamContent(File.OpenRead("E:\\SAP\\MES\\Final_Version_SourceCode\\MES&WMS(SAP Version)\\" +
                        //    "MES&WMS to SAP(Interface)\\Development\\POST_TO_PIVOT88\\POST_TO_SAP\\FMSPlatForm\\FMS\\images\\upload\\20230510170946286.jpg")),
                        //    "file", "20230510170946286.jpg");

                        var webC = new System.Net.WebClient();
                        string url = imgpath.Replace("\\", "/");
                        Image image = new Bitmap(webC.OpenRead(url));
                        MemoryStream stream = new MemoryStream();
                        if (img_SUFFIX == "png") { image.Save(stream, ImageFormat.Png); } else if (img_SUFFIX == "jpg") { image.Save(stream, ImageFormat.Jpeg); }
                        image.Save(stream, ImageFormat.Png);
                        stream.Seek(0, SeekOrigin.Begin); //Need to reset position to 0

                        content.Add(new StreamContent(stream), "file", imgfull_filename);
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        //Console.WriteLine(await response.Content.ReadAsStringAsync());
                    }
                }

            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show(ex.ToString());
                throw;
            }
        }


        #endregion

        #region Common Methods
        public async Task PushNotificationTestAsync()
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://apc.apachefootwear.com/Platform/message/EscalateAppMessgae");
                request.Headers.Add("Token", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOiJhZG1pbmlzdHJhdG9yIiwibmJmIjoxNjk0NTEwOTQ1LCJleHAiOjE2OTUzNzQ5NDUsImlhdCI6MTY5NDUxMDk0NX0.NB7EWE0sphbfg5JdgjxszwwtUi-EJzWwhPZX7iF6JEM");
                var content = new StringContent("{\r\n     \"id\": null,\n     \"subject\": \"Test 0913\",\n     \"body\": \"Test 0913\",\n     \"sendAll\": 0,\n     \"empnopz\": \"N\",\r\n     \"orgidpz\": \"N\",\r\n     \"deptnopz\": \"N\",\r\n     \"otherspz\": \"N\",\r\n     \"userList\": [\n         \"A54189\"\r\n                 ]   \r\n}", null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }


        public string GetImageData(string guid, string Param)
        {
            string sql = $@"select FILE_URL,SUFFIX from BDM_UPLOAD_FILE_ITEM where GUID ='" + guid + "'";
            OracleCommand cmd = new OracleCommand(sql, conmes);
            cmd.CommandType = CommandType.Text;
            OracleDataAdapter da = new OracleDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            string rtString = string.Empty;
            if (Param == "A") // Get full_filename
            {
                string[] a = dt.Rows[0]["FILE_URL"].ToString().Split('/');
                rtString = a[4];
            }
            else if (Param == "B") //Get file path
            {
                //Client.PicUrl = "http://10.3.0.29:8011";//test APC
                //Client.PicUrl = "http://10.3.0.24:8011";//realtime APC

                //rtString = "http://10.2.171.111:8011" + dt.Rows[0]["FILE_URL"].ToString();// Test imageupload api
                ////rtString = "http://10.1.1.1:8001" + dt.Rows[0]["FILE_URL"].ToString();// offical imageupload api
                ///


                //rtString = "http://10.3.0.29:8011" + dt.Rows[0]["FILE_URL"].ToString();// Test imageupload api
                rtString = "http://10.3.0.24:8011" + dt.Rows[0]["FILE_URL"].ToString();// offical imageupload api

            }
            else if (Param == "C") //Get image suffix
            {
                rtString = dt.Rows[0]["SUFFIX"].ToString();
            }


            return rtString;
        }
        #endregion

    }//class
}
