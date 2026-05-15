using Compal.FMS.Connections.DBLoader;
using Compal.FMS.Kernel.Beans;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;


namespace Compal.FMS.Compal.FMS.Operations
{
    public class Run_AEQS_Pivot88_Operations
    {
        Cls_Return rt = new Cls_Return();
        OracleConnection conmes = null;

        //public Cls_Return OA_FI013Travel(SrvInfo vsrvinfo)
        //{
        //    OracleConnection conoa = null;
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

        //            conoa = new OracleConnection(constroa);
        //            //OracleTransaction objTrans = null;
        //            int updatecount = 0;
        //            int errorcount = 0;
        //            string errmsg = "";
        //            string passdata = "";
        //            string sql = "select  BUKRS,TO_CHAR(APEFD004.BLDAT,'YYYYMMDD') BLDAT,TO_CHAR(BUDAT,'YYYYMMDD') BUDAT,BLART,WAERS, APEFD004.PROCESSSERIALNUMBER XREF1_HD,BKTXT, 'OA' USERNAME,NUMPG,PAY_TYPE   ,'' REMAKR_1,'' REMAKR_2,'' REMAKR_3,SGTXT, APEFD004.PAY_TYPE,APEFD004.Total,ChangeProcessStateAudit.Currentprocessinstancestate ,      APEFD004.Oid,APEFD004.WEBSERVDATE,APEFD004.Formserialnumber,      decode(ChangeProcessStateAudit.Currentprocessinstancestate,'3','R','') FRGKE ,       decode(ChangeProcessStateAudit.Currentprocessinstancestate,'3','','X') LOEKZ ,           TO_CHAR(SYSDATE,'YYYYMMDD') PSODT,       TO_CHAR(SYSDATE,'HH24MISS') UTIME,WEBSERVDATE ,WEBSERVNOTE from APEFD004 , processinstance,ChangeProcessStateAudit  where APEFD004.PROCESSSERIALNUMBER= processinstance.serialnumber and  ChangeProcessStateAudit.Sourceoid=processinstance.OID    and  ChangeProcessStateAudit.Currentprocessinstancestate IN （'3')     AND    APEFD004.WEBSERVDATE IS NULL";

        //            OracleCommand cmd = new OracleCommand(sql, conoa);
        //            cmd.CommandType = CommandType.Text;

        //            OracleDataAdapter da = new OracleDataAdapter(cmd);

        //            DataTable dtcheck = new DataTable();

        //            da.Fill(dtcheck);

        //            if (dtcheck.Rows.Count > 0)
        //            {
        //                DialogResult result = MessageBox.Show("Would like to Post Retrived (" + dtcheck.Rows.Count + ") Records to SAP?", "Run FMS service",
        //                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        //                if (result.Equals(DialogResult.OK))
        //                {
        //                    conoa.Open();
        //                    //objTrans = conoa.BeginTransaction();
        //                    for (int i = 0; i < dtcheck.Rows.Count; i++)
        //                    {
        //                        string UUID = Guid.NewGuid().ToString();
        //                        string oid = dtcheck.Rows[i]["OID"].ToString();

        //                        string BUKRS = dtcheck.Rows[i]["BUKRS"].ToString();
        //                        string BLDAT = dtcheck.Rows[i]["BLDAT"].ToString();
        //                        string BUDAT = dtcheck.Rows[i]["BUDAT"].ToString();
        //                        string BLART = dtcheck.Rows[i]["BLART"].ToString();
        //                        string WAERS = dtcheck.Rows[i]["WAERS"].ToString();
        //                        //string AUFNR = dtcheck.Rows[i]["AUFNR"].ToString();
        //                        string XREF1_HD = dtcheck.Rows[i]["XREF1_HD"].ToString();
        //                        string BKTXT = dtcheck.Rows[i]["BKTXT"].ToString();

        //                        string NUMPG = dtcheck.Rows[i]["NUMPG"].ToString();
        //                        string PAY_TYPE = dtcheck.Rows[i]["PAY_TYPE"].ToString();

        //                        string REMAKR_1 = dtcheck.Rows[i]["REMAKR_1"].ToString();
        //                        string REMAKR_2 = dtcheck.Rows[i]["REMAKR_2"].ToString();
        //                        string REMAKR_3 = dtcheck.Rows[i]["REMAKR_3"].ToString();
        //                        string SGTXT = dtcheck.Rows[i]["SGTXT"].ToString();
        //                        string TOTAL = dtcheck.Rows[i]["TOTAL"].ToString();

        //                        string username = dtcheck.Rows[i]["USERNAME"].ToString();
        //                        string psodt = dtcheck.Rows[i]["PSODT"].ToString();
        //                        string utime = dtcheck.Rows[i]["UTIME"].ToString();

        //                        string FORMSERIALNUMBER = dtcheck.Rows[i]["FORMSERIALNUMBER"].ToString();

        //                        oaFI013.SI_FI013_OUTService sI_OUTService = new oaFI013.SI_FI013_OUTService();
        //                        sI_OUTService.Credentials = new NetworkCredential("apc_oa", "oa123456");

        //                        oaFI013.DT_FI013_REQ input = new oaFI013.DT_FI013_REQ();
        //                        input.I_REQUEST = new oaFI013.DT_FI013_REQI_REQUEST();

        //                        input.I_REQUEST.Header = new oaFI013.DT_BASEINFO_REQ();

        //                        input.I_REQUEST.Header.BusId = "FI013";
        //                        input.I_REQUEST.Header.UUID = UUID;
        //                        input.I_REQUEST.Header.Sender = "OA";
        //                        input.I_REQUEST.Header.Receiver = "SAP";
        //                        input.I_REQUEST.Header.Date = psodt;
        //                        input.I_REQUEST.Header.Time = utime;

        //                        input.I_REQUEST.Body = new oaFI013.DT_FI013_REQI_REQUESTBody();

        //                        input.I_REQUEST.Body.BUKRS = BUKRS;
        //                        input.I_REQUEST.Body.BLDAT = BLDAT;
        //                        input.I_REQUEST.Body.BUDAT = BUDAT;
        //                        input.I_REQUEST.Body.BLART = BLART;
        //                        input.I_REQUEST.Body.WAERS = WAERS;
        //                        input.I_REQUEST.Body.XREF1_HD = XREF1_HD;
        //                        input.I_REQUEST.Body.BKTXT = BKTXT;
        //                        input.I_REQUEST.Body.NUMPG = NUMPG;
        //                        input.I_REQUEST.Body.USNAM = username;
        //                        input.I_REQUEST.Body.I_REMAKR_1 = REMAKR_1;
        //                        input.I_REQUEST.Body.I_REMAKR_2 = REMAKR_2;
        //                        input.I_REQUEST.Body.I_REMAKR_3 = REMAKR_3;


        //                        sql = "SELECT  FORMSERIALNUMBER,  BSCHL,RSTGR,HKONT,KOSTL_TXT ,MAX(AUFNR_TXT) AUFNR_TXT,MAX(PRCTR) PRCTR,sum(NVL(gutax,0)) gutax,SUM(NVL(gtax,0)) gtax FROM APEFD004_GRID0 where Formserialnumber='" + FORMSERIALNUMBER + "' GROUP BY FORMSERIALNUMBER,  BSCHL,RSTGR,HKONT,KOSTL_TXT,AUFNR_TXT,PRCTR";
        //                        cmd = new OracleCommand(sql, conoa);
        //                        cmd.CommandType = CommandType.Text;
        //                        da = new OracleDataAdapter(cmd);
        //                        DataTable dtitem = new DataTable();
        //                        da.Fill(dtitem);

        //                        int ic = 0;
        //                        decimal gutax = 0;
        //                        decimal gtax = 0;
        //                        if (dtitem.Rows.Count > 0)
        //                        {
        //                            input.I_REQUEST.Body.ITEMS = new oaFI013.DT_FI013_REQI_REQUESTBodyITEMS[dtitem.Rows.Count + 5];

        //                            for (int t = 0; t < dtitem.Rows.Count; t++)
        //                            {
        //                                input.I_REQUEST.Body.ITEMS[ic] = new oaFI013.DT_FI013_REQI_REQUESTBodyITEMS();

        //                                input.I_REQUEST.Body.ITEMS[ic].BUZEI = "00" + (ic + 1).ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].BSCHL = dtitem.Rows[t]["BSCHL"].ToString();
        //                                //input.I_REQUEST.Body.ITEMS[ic].RSTGR = dtitem.Rows[t]["RSTGR"].ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].HKONT = dtitem.Rows[t]["HKONT"].ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].KOSTL = dtitem.Rows[t]["KOSTL_TXT"].ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].AUFNR = dtitem.Rows[t]["AUFNR_TXT"].ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].PRCTR = dtitem.Rows[t]["PRCTR"].ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].XREF3 = XREF1_HD;
        //                                input.I_REQUEST.Body.ITEMS[ic].WRBTR = dtitem.Rows[t]["GUTAX"].ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].SGTXT = SGTXT;

        //                                gutax = gutax + Convert.ToDecimal(dtitem.Rows[t]["GUTAX"]);
        //                                gtax = gtax + Convert.ToDecimal(dtitem.Rows[t]["GTAX"]);
        //                                ic++;
        //                            }

        //                            if (gtax > 0)
        //                            {
        //                                input.I_REQUEST.Body.ITEMS[ic] = new oaFI013.DT_FI013_REQI_REQUESTBodyITEMS();
        //                                input.I_REQUEST.Body.ITEMS[ic].BUZEI = "00" + (ic + 1).ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].BSCHL = "40";
        //                                input.I_REQUEST.Body.ITEMS[ic].HKONT = "2221010001";
        //                                input.I_REQUEST.Body.ITEMS[ic].WRBTR = gtax.ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].XREF3 = XREF1_HD;
        //                                input.I_REQUEST.Body.ITEMS[ic].SGTXT = SGTXT;

        //                                ic++;
        //                                input.I_REQUEST.Body.ITEMS[ic] = new oaFI013.DT_FI013_REQI_REQUESTBodyITEMS();
        //                                input.I_REQUEST.Body.ITEMS[ic].BUZEI = "00" + (ic + 1).ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].BSCHL = "50";
        //                                input.I_REQUEST.Body.ITEMS[ic].HKONT = PAY_TYPE;
        //                                input.I_REQUEST.Body.ITEMS[ic].WRBTR = (gutax + gtax).ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].VALUT = BUDAT;
        //                                input.I_REQUEST.Body.ITEMS[ic].RSTGR = "204";
        //                                input.I_REQUEST.Body.ITEMS[ic].XREF3 = XREF1_HD;
        //                                input.I_REQUEST.Body.ITEMS[ic].SGTXT = SGTXT;
        //                            }
        //                            else
        //                            {

        //                                input.I_REQUEST.Body.ITEMS[ic] = new oaFI013.DT_FI013_REQI_REQUESTBodyITEMS();
        //                                input.I_REQUEST.Body.ITEMS[ic].BUZEI = "00" + (ic + 1).ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].BSCHL = "50";
        //                                input.I_REQUEST.Body.ITEMS[ic].HKONT = PAY_TYPE;
        //                                input.I_REQUEST.Body.ITEMS[ic].WRBTR = gutax.ToString();
        //                                input.I_REQUEST.Body.ITEMS[ic].VALUT = BUDAT;
        //                                input.I_REQUEST.Body.ITEMS[ic].RSTGR = "204";
        //                                input.I_REQUEST.Body.ITEMS[ic].XREF3 = XREF1_HD;
        //                                input.I_REQUEST.Body.ITEMS[ic].SGTXT = SGTXT;
        //                            }

        //                        }

        //                        passdata = JsonConvert.SerializeObject(input);

        //                        oaFI013.DT_FI013_RSP rsp = sI_OUTService.SI_FI013_OUT(input);

        //                        if (rsp.E_RESPONSE.Body.MSG_TYPE == "S")
        //                        {
        //                            sql = "UPDATE APEFD004 SET WEBSERVNOTE= '" + rsp.E_RESPONSE.Body.MSG_TEXT + " / BELNR : " + rsp.E_RESPONSE.Body.BELNR + "',WEBSERVDATE= to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-mm-dd HH24:MI:SS') WHERE FORMSERIALNUMBER='" + FORMSERIALNUMBER + "' ";
        //                            cmd = new OracleCommand(sql, conoa);
        //                            cmd.CommandType = CommandType.Text;
        //                            cmd.ExecuteNonQuery();

        //                            updatecount++;
        //                            rt.TYPE = "S";
        //                        }
        //                        else
        //                        {
        //                            sql = "UPDATE APEFD004 SET WEBSERVNOTE= '" + rsp.E_RESPONSE.Body.MSG_TEXT + "' WHERE FORMSERIALNUMBER='" + FORMSERIALNUMBER + "' ";
        //                            cmd = new OracleCommand(sql, conoa);
        //                            cmd.CommandType = CommandType.Text;
        //                            cmd.ExecuteNonQuery();

        //                            rt.TYPE = "E";
        //                            errorcount++;
        //                            errmsg = errmsg + "/ " + rsp.E_RESPONSE.Body.MSG_TEXT + "@ERR DATA:" + FORMSERIALNUMBER + " : " + passdata;
        //                        }
        //                    }
        //                    //objTrans.Commit();

        //                    rt.MESSAGE = "Data Posted and Updated (" + updatecount + ") Success and Error (" + errorcount + ") " + errmsg + "";
        //                }
        //                else
        //                {
        //                    rt.TYPE = "E";
        //                    rt.MESSAGE = "Cancelled to Post SAP";
        //                }

        //            }
        //            else
        //            {
        //                rt.TYPE = "E";
        //                rt.MESSAGE = "No Data to Post SAP";
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

        //public Cls_Return PostRequest(SrvInfo vsrvinfo)
        //{
        //    try
        //    {
        //        DateTime current = DateTime.Now;
        //        string currentString = current.ToString("yyMMddHHmmss");
        //        string url = "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:apache_" + currentString + "";
        //        //string result = string.Empty;
        //        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        //        httpWebRequest.ContentType = "application/json; charset=utf-8";
        //        httpWebRequest.Method = "PUT";
        //        httpWebRequest.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58");
        //        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        //        {

        //            string json = "[  {\"status\":\"Submitted\",\"date_started\":\"2023-05-09T00:00:00\",\"defective_parts\":0,\"sections\":[{\"type\":\"aqlDefects\",\"title\":\"packing_packaging_labelling\",\"section_result_id\":1,\"qty_inspected\":766,\"sampled_inspected\":32,\"defective_parts\":0,\"inspection_level\":\"100%inspection\",\"inspection_method\":\"normal\",\"aql_minor\":4.0,\"aql_major\":2.5,\"aql_major_a\":1,\"aql_major_b\":1,\"aql_critical\":1.0,\"barcodes\":[{\"value\":\"001\"},{\"value\":\"002\"}],\"qty_type\":\"carton\",\"max_minor_defects\":7,\"max_major_defects\":5,\"max_major_a_defects\":0,\"max_major_b_defects\":0,\"max_critical_defects\":0,\"defects\":[{\"label\":\"OUTERCARTONDAMAGED/DIRTY.OFF-CENTER/UN-READABLE/UN-SCANNABLE,STICKER(INCLUDEUCC128LABEL)\",\"subsection\":\"PACKINGANDLABELING\",\"code\":\"FTW100.01\",\"critical_level\":0,\"major_level\":0,\"minor_level\":1,\"comments\":\"testcomments\",\"pictures\":[]}]},{\"type\":\"aqlDefects\",\"title\":\"product\",\"section_result_id\":1,\"defective_parts\":0,\"qty_inspected\":766,\"sampled_inspected\":32,\"inspection_level\":\"100%inspection\",\"inspection_method\":\"normal\",\"aql_minor\":4.0,\"aql_major\":2.5,\"aql_major_a\":1,\"aql_major_b\":1,\"aql_critical\":1.0,\"max_minor_defects\":15,\"max_major_defects\":15,\"max_major_a_defects\":0,\"max_major_b_defects\":0,\"max_critical_defects\":15,\"defects\":[{\"label\":\"OUTERCARTONDAMAGED/DIRTY.OFF-CENTER/UN-READABLE/UN-SCANNABLE,STICKER(INCLUDEUCC128LABEL)\",\"subsection\":\"PACKINGANDLABELING\",\"code\":\"FTW100.01\",\"critical_level\":0,\"major_level\":1,\"minor_level\":0,\"comments\":\"testcomments1\",\"pictures\":[]}]},{\"type\":\"pictures\",\"title\":\"photos\",\"pictures\":[]}],\"assignment_items\":[{\"sampled_inspected\":\"45\",\"inspection_result_id\":1,\"inspection_status_id\":3,\"qty_inspected\":766,\"inspection_completed_date\":\"2023-02-09T00:00:00\",\"total_inspection_minutes\":0,\"sampling_size\":50,\"qty_to_inspect\":766,\"aql_minor\":4.0,\"aql_major\":2.5,\"aql_major_a\":1.0,\"aql_major_b\":1.0,\"aql_critical\":1.0,\"supplier_booking_msg\":\"bookingmessage\",\"conclusion_remarks\":\"Conclusioncomments\",\"assignment\":{\"report_type\":{\"id\":27},\"inspector\":{\"username\":\"chandrasekhar-v\"},\"date_inspection\":\"2023-05-09T00:00:00\",\"inspection_level\":\"100%inspection\",\"inspection_method\":\"normal\"},\"po_line\":{\"qty\":244,\"etd\":\"2023-05-09T00:00:00\",\"eta\":null,\"color\":null,\"size\":null,\"style\":\"style\",\"po\":{\"exporter\":{\"id\":233,\"erp_business_id\":\"011\"},\"po_number\":\"\",\"customer_po\":\"\",\"importer\":{\"id\":215,\"erp_business_id\":\"Adidas001\"},\"project\":{\"id\":2062}},\"sku\":{\"sku_number\":\"GZ3628_3\",\"item_name\":\"GZ3628\",\"item_description\":\"\"}}}],\"passFails\":[{\"title\":\"inspected_carton_numbers\",\"type\":\"list\",\"subsection\":\"actual_inspection\",\"listValues\":[{\"value\":1},{\"value\":2},{\"value\":6}]},{\"title\":\"mcs_confirmed_component_is_available_signature_compliant\",\"value\":\"N/A\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"1_general_compliance\",\"status\":\"na\",\"comment\":\"\"},{\"title\":\"quality_working_instruction_flow_chart\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"2_document_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"operation_all_sops_bpfc\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"2_document_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"broken_needle_procedure_control_record\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"3_metal_detection_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"metal_tools_detection_control_record\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"3_metal_detection_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"metal_detector_calibration_with_test_stick_approved_record\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"3_metal_detection_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"calibration_maintenance_records\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"4_machinery_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"daily_machine_setting_records\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"4_machinery_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"workplace_safety_standards_machinery_chemicals_ppe_etc\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"5_occupational_health_safety_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"machine_condition\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"time\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"temperature\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"pressure\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"energy_level\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"speed\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"programming_automated_processing\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"mold\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"cutting_knife_laser_die_conveyor_board\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"needle\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"gauges\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"last\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"pressing_pad\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"chemical_management\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"3_process\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"process_compliance_to_sop\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"3_process\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"basic_6s\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"3_process\",\"status\":\"pass\",\"comment\":\"\"}]} ]";
        //            //Debug.Write(json);
        //            streamWriter.Write(json);
        //            streamWriter.Flush();
        //            streamWriter.Close();
        //        }
        //        try
        //        {
        //            using (var response = httpWebRequest.GetResponse() as HttpWebResponse)
        //            {
        //                if (httpWebRequest.HaveResponse && response != null)
        //                {
        //                    using (var reader = new StreamReader(response.GetResponseStream()))
        //                    {

        //                        rt.MESSAGE = reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //        }
        //        catch (WebException e)
        //        {
        //            if (e.Response != null)
        //            {
        //                using (var errorResponse = (HttpWebResponse)e.Response)
        //                {
        //                    using (var reader = new StreamReader(errorResponse.GetResponseStream()))
        //                    {
        //                        string error = reader.ReadToEnd();
        //                        rt.MESSAGE = error;
        //                    }
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
        //        GC.Collect();
        //    }
        //}
        public Cls_Return PostRequestAsync(SrvInfo vsrvinfo)
        {
            try
            {
                string constrmes = null;
                string filePath;

                filePath = Application.ExecutablePath;
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                string clientEnvConfigFileName = filePath + "database.config";

                FileLoader obj = new FileLoader(clientEnvConfigFileName);
                Hashtable htdblinks = obj.GetDBLinks();
                if (htdblinks.ContainsKey(vsrvinfo.SDB))
                    constrmes = htdblinks[vsrvinfo.SDB].ToString();
                conmes = new OracleConnection(constrmes);
                conmes.Open();
                //Task t = new Task(PUTAsync);
                //t.Start();

                PUTAsync();

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
                conmes.Close();
                conmes.Dispose();
                GC.Collect();
            }
        }


        //public async Task PUTAsync()
        //{
        //    try
        //    {
        //        var client = new HttpClient();
        //        var request = new HttpRequestMessage(HttpMethod.Put, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:apache_012");
        //        request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58");
        //        //var content = new StringContent("{\"status\":\"Submitted\",\"date_started\":\"2023-07-17T10:09:00\",\"defective_parts\":0,\"sections\":[{\"type\":\"aqlDefects\",\"title\":\"packing_packaging_labelling\",\"section_result_id\":1,\"qty_inspected\":766,\"sampled_inspected\":32,\"defective_parts\":0,\"inspection_level\":\"100%inspection\",\"inspection_method\":\"normal\",\"aql_minor\":4.0,\"aql_major\":2.5,\"aql_major_a\":1,\"aql_major_b\":1,\"aql_critical\":1.0,\"barcodes\":[{\"value\":\"001\"},{\"value\":\"002\"}],\"qty_type\":\"carton\",\"max_minor_defects\":7,\"max_major_defects\":5,\"max_major_a_defects\":0,\"max_major_b_defects\":0,\"max_critical_defects\":0,\"defects\":[{\"label\":\"OUTERCARTONDAMAGED/DIRTY.OFF-CENTER/UN-READABLE/UN-SCANNABLE,STICKER(INCLUDEUCC128LABEL)\",\"subsection\":\"PACKINGANDLABELING\",\"code\":\"FTW100.01\",\"critical_level\":0,\"major_level\":0,\"minor_level\":1,\"comments\":\"testcomments\",\"pictures\":[{\"title\":\"test\",\"full_filename\":\"20230510170946286.jpg\",\"number\":1,\"comment\":\"test\"}]}]},{\"type\":\"aqlDefects\",\"title\":\"product\",\"section_result_id\":1,\"defective_parts\":0,\"qty_inspected\":766,\"sampled_inspected\":32,\"inspection_level\":\"100%inspection\",\"inspection_method\":\"normal\",\"aql_minor\":4.0,\"aql_major\":2.5,\"aql_major_a\":1,\"aql_major_b\":1,\"aql_critical\":1.0,\"max_minor_defects\":15,\"max_major_defects\":15,\"max_major_a_defects\":0,\"max_major_b_defects\":0,\"max_critical_defects\":15,\"defects\":[{\"label\":\"POOR CEMENTING SOCKLINER TO INSOLE BOARD\",\"subsection\":\"INSIDE THE SHOE\",\"code\":\"FTW200.10\",\"critical_level\":0,\"major_level\":1,\"minor_level\":0,\"comments\":\"testcomments1\",\"pictures\":[]},{\"label\":\"LEATHER DEFECTS (LOOSE GRAIN, PEELING, ORANGE PEEL, EDGES NOT DYED AS REQUESTED, ETC.)\",\"subsection\":\"UPPER MATERIALS\",\"code\":\"FTW310.01\",\"critical_level\":1,\"major_level\":0,\"minor_level\":0,\"comments\":\"testcomments2\",\"pictures\":[]}]},{\"type\":\"pictures\",\"title\":\"photos\",\"pictures\":[{\"title\":\"test\",\"full_filename\":\"20230510170946286.jpg\",\"number\":3,\"comment\":\"test\"}]}],\"assignment_items\":[{\"sampled_inspected\":\"45\",\"inspection_result_id\":1,\"inspection_status_id\":3,\"qty_inspected\":766,\"inspection_completed_date\":\"2023-07-17T08:30:13\",\"total_inspection_minutes\":0,\"sampling_size\":50,\"qty_to_inspect\":766,\"aql_minor\":4.0,\"aql_major\":2.5,\"aql_major_a\":1.0,\"aql_major_b\":1.0,\"aql_critical\":1.0,\"supplier_booking_msg\":\"bookingmessage\",\"conclusion_remarks\":\"Conclusioncomments\",\"assignment\":{\"report_type\":{\"id\":27},\"inspector\":{\"username\":\"chandrasekhar-v\"},\"date_inspection\":\"2023-07-17T00:00:00\",\"inspection_level\":\"100%inspection\",\"inspection_method\":\"normal\"},\"po_line\":{\"qty\":766,\"etd\":\"2023-07-17T00:00:00\",\"eta\":null,\"color\":null,\"size\":null,\"style\":\"style\",\"po\":{\"exporter\":{\"id\":233,\"erp_business_id\":\"011\"},\"po_number\":\"0130769668\",\"customer_po\":\"\",\"importer\":{\"id\":215,\"erp_business_id\":\"Adidas001\"},\"project\":{\"id\":2062}},\"sku\":{\"sku_number\":\"GZ3628_3\",\"item_name\":\"GZ3628\",\"item_description\":\"\"}}}],\"passFails\":[{\"title\":\"inspected_carton_numbers\",\"type\":\"list\",\"subsection\":\"actual_inspection\",\"listValues\":[{\"value\":1},{\"value\":2},{\"value\":6}]},{\"title\":\"mcs_confirmed_component_is_available_signature_compliant\",\"value\":\"N/A\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"1_general_compliance\",\"status\":\"na\",\"comment\":\"\"},{\"title\":\"quality_working_instruction_flow_chart\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"2_document_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"operation_all_sops_bpfc\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"2_document_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"broken_needle_procedure_control_record\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"3_metal_detection_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"metal_tools_detection_control_record\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"3_metal_detection_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"metal_detector_calibration_with_test_stick_approved_record\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"3_metal_detection_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"calibration_maintenance_records\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"4_machinery_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"daily_machine_setting_records\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"4_machinery_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"workplace_safety_standards_machinery_chemicals_ppe_etc\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"validation\",\"checkListSubsection\":\"5_occupational_health_safety_compliance\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"machine_condition\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"time\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"temperature\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"pressure\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"energy_level\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"speed\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"programming_automated_processing\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"1_machine_setting_according_to_standard\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"mold\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"cutting_knife_laser_die_conveyor_board\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"needle\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"gauges\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"last\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"pressing_pad\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"2_tools\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"chemical_management\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"3_process\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"process_compliance_to_sop\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"3_process\",\"status\":\"pass\",\"comment\":\"\"},{\"title\":\"basic_6s\",\"value\":\"yes\",\"type\":\"check-list\",\"subsection\":\"checklist\",\"checkListSubsection\":\"3_process\",\"status\":\"pass\",\"comment\":\"\"}]}", null, "application/json");
        //        var content = new StringContent("{\r\n\t\"status\": \"Submitted\",\r\n\t\"date_started\": \"2023-07-15T10:00:00\",\r\n\t\"defective_parts\": 0,\r\n\t\"sections\": [\r\n\t\t{\r\n\t\t\t\"type\": \"aqlDefects\",\r\n\t\t\t\"title\": \"packing_packaging_labelling\",\r\n\t\t\t\"section_result_id\": 1,\r\n\t\t\t\"qty_inspected\": 766,\r\n\t\t\t\"sampled_inspected\": 32,\r\n\t\t\t\"defective_parts\": 0,\r\n\t\t\t\"inspection_level\": \"II\",\r\n\t\t\t\"inspection_method\": \"normal\",\r\n\t\t\t\"aql_minor\": 4.0,\r\n\t\t\t\"aql_major\": 2.5,\r\n\t\t\t\"aql_major_a\": 1,\r\n\t\t\t\"aql_major_b\": 1,\r\n\t\t\t\"aql_critical\": 1.0,\r\n\t\t\t\"barcodes\": [\r\n\t\t\t\t{\r\n\t\t\t\t\t\"value\": \"001\"\r\n\t\t\t\t},\r\n\t\t\t\t{\r\n\t\t\t\t\t\"value\": \"002\"\r\n\t\t\t\t}\r\n\t\t\t],\r\n\t\t\t\"qty_type\": \"carton\",\r\n\t\t\t\"max_minor_defects\": 7,\r\n\t\t\t\"max_major_defects\": 5,\r\n\t\t\t\"max_major_a_defects\": 0,\r\n\t\t\t\"max_major_b_defects\": 0,\r\n\t\t\t\"max_critical_defects\": 0,\r\n\t\t\t\"defects\": [\r\n\t\t\t\t{\r\n\t\t\t\t\t\"label\": \"OUTER CARTON DAMAGED/DIRTY. OFF-CENTER/UN-READABLE/UN-SCANNABLE, STICKER (INCLUDE UCC128 LABEL)\",\r\n\t\t\t\t\t\"subsection\": \"PACKING AND LABELING\",\r\n\t\t\t\t\t\"code\": \"FTW100.01\",\r\n\t\t\t\t\t\"critical_level\": 0,\r\n\t\t\t\t\t\"major_level\": 0,\r\n\t\t\t\t\t\"minor_level\": 1,\r\n\t\t\t\t\t\"comments\": \"test packaging and labeling comment1\",\r\n\t\t\t\t\t\"pictures\": []\r\n\t\t\t\t},\r\n        {\r\n\t\t\t\t\t\"label\": \"ACCESSORIES/ATTACHMENTS MISSING OR IN BAD QUALITY\",\r\n\t\t\t\t\t\"subsection\": \"PACKING AND LABELING\",\r\n\t\t\t\t\t\"code\": \"FTW100.04\",\r\n\t\t\t\t\t\"critical_level\": 0,\r\n\t\t\t\t\t\"major_level\": 1,\r\n\t\t\t\t\t\"minor_level\": 0,\r\n\t\t\t\t\t\"comments\": \"test packaging and labeling comment2\",\r\n\t\t\t\t\t\"pictures\": []\r\n\t\t\t\t},\r\n        {\r\n\t\t\t\t\t\"label\": \"WRONG INNER BOX STICKER IMAGE\",\r\n\t\t\t\t\t\"subsection\": \"PACKING AND LABELING\",\r\n\t\t\t\t\t\"code\": \"FTW100.08\",\r\n\t\t\t\t\t\"critical_level\": 1,\r\n\t\t\t\t\t\"major_level\": 0,\r\n\t\t\t\t\t\"minor_level\": 0,\r\n\t\t\t\t\t\"comments\": \"test packaging and labeling comment3\",\r\n\t\t\t\t\t\"pictures\": []\r\n\t\t\t\t}\r\n\t\t\t]\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"type\": \"aqlDefects\",\r\n\t\t\t\"title\": \"product\",\r\n\t\t\t\"section_result_id\": 1,\r\n\t\t\t\"defective_parts\": 0,\r\n\t\t\t\"qty_inspected\": 5,\r\n\t\t\t\"sampled_inspected\": 5,\r\n\t\t\t\"inspection_level\": \"II\",\r\n\t\t\t\"inspection_method\": \"normal\",\r\n\t\t\t\"aql_minor\": 4,\r\n\t\t\t\"aql_major\": 1,\r\n\t\t\t\"aql_major_a\": 1,\r\n\t\t\t\"aql_major_b\": 1,\r\n\t\t\t\"aql_critical\": 0.01,\r\n\t\t\t\"max_minor_defects\": 0,\r\n\t\t\t\"max_major_defects\": 0,\r\n\t\t\t\"max_major_a_defects\": 0,\r\n\t\t\t\"max_major_b_defects\": 0,\r\n\t\t\t\"max_critical_defects\": 0,\r\n\t\t\t\"defects\": [\r\n\t\t\t\t{\r\n\t\t\t\t\t\"label\": \"LEATHER DEFECTS (LOOSE GRAIN, PEELING, ORANGE PEEL, EDGES NOT DYED AS REQUESTED, ETC.)\",\r\n\t\t\t\t\t\"subsection\": \"UPPER MATERIALS\",\r\n\t\t\t\t\t\"code\": \"FTW310.01\",\r\n\t\t\t\t\t\"critical_level\": 0,\r\n\t\t\t\t\t\"major_level\": 0,\r\n\t\t\t\t\t\"minor_level\": 0,\r\n\t\t\t\t\t\"comments\": \"\",\r\n\t\t\t\t\t\"pictures\": []\r\n\t\t\t\t},\r\n        {\r\n\t\t\t\t\t\"label\": \"WAVY/INCONSISTENT STITCHING\",\r\n\t\t\t\t\t\"subsection\": \"UPPER STITCHING\",\r\n\t\t\t\t\t\"code\": \"FTW320.06\",\r\n\t\t\t\t\t\"critical_level\": 0,\r\n\t\t\t\t\t\"major_level\": 0,\r\n\t\t\t\t\t\"minor_level\": 0,\r\n\t\t\t\t\t\"comments\": \"\",\r\n\t\t\t\t\t\"pictures\": []\r\n\t\t\t\t}\r\n\t\t\t]\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"type\": \"pictures\",\r\n\t\t\t\"title\": \"photos\",\r\n\t\t\t\"pictures\": []\r\n\t\t}\r\n\t],\r\n\t\"assignment_items\": [\r\n\t\t{\r\n\t\t\t\"sampled_inspected\": \"5\",\r\n\t\t\t\"inspection_result_id\": 1,\r\n\t\t\t\"inspection_status_id\": 3,\r\n\t\t\t\"qty_inspected\": 5,\r\n\t\t\t\"inspection_completed_date\": \"2023-07-15T10:30:00\",\r\n\t\t\t\"total_inspection_minutes\": 0,\r\n\t\t\t\"sampling_size\": 5,\r\n\t\t\t\"qty_to_inspect\": 5,\r\n\t\t\t\"aql_minor\": 4.0,\r\n\t\t\t\"aql_major\": 1,\r\n\t\t\t\"aql_major_a\": 1.0,\r\n\t\t\t\"aql_major_b\": 1.0,\r\n\t\t\t\"aql_critical\": 0.01,\r\n\t\t\t\"supplier_booking_msg\": \"booking message\",\r\n\t\t\t\"conclusion_remarks\": \"Conclusion comments\",\r\n\t\t\t\"assignment\": {\r\n\t\t\t\t\"report_type\": {\r\n\t\t\t\t\t\"id\": 9\r\n\t\t\t\t},\r\n\t\t\t\t\"inspector\": {\r\n\t\t\t\t\t\"username\": \"chandrasekhar-v\"\r\n\t\t\t\t},\r\n\t\t\t\t\"date_inspection\": \"2023-07-15T10:00:00\",\r\n\t\t\t\t\"inspection_level\": \"II\",\r\n\t\t\t\t\"inspection_method\": \"normal\"\r\n\t\t\t},\r\n\t\t\t\"po_line\": {\r\n\t\t\t\t\"qty\": 1,\r\n\t\t\t\t\"etd\": \"2023-07-25T00:00:00\",\r\n\t\t\t\t\"eta\": null,\r\n\t\t\t\t\"color\": null,\r\n\t\t\t\t\"size\": null,\r\n\t\t\t\t\"style\": \"style\",\r\n\t\t\t\t\"po\": {\r\n\t\t\t\t\t\"exporter\": {\r\n\t\t\t\t\t\t\"id\": 233,\r\n\t\t\t\t\t\t\"erp_business_id\": \"011\"\r\n\t\t\t\t\t},\r\n\t\t\t\t\t\"po_number\": \"0127859531\",\r\n\t\t\t\t\t\"customer_po\": \"\",\r\n\t\t\t\t\t\"importer\": {\r\n\t\t\t\t\t\t\"id\": 215,\r\n\t\t\t\t\t\t\"erp_business_id\": \"Adidas001\"\r\n\t\t\t\t\t},\r\n\t\t\t\t\t\"project\": {\r\n\t\t\t\t\t\t\"id\": 2062\r\n\t\t\t\t\t}\r\n\t\t\t\t},\r\n\t\t\t\t\"sku\": {\r\n\t\t\t\t\t\"sku_number\": \"IC1303_2\",\r\n\t\t\t\t\t\"item_name\": \"IC1303_2\",\r\n\t\t\t\t\t\"item_description\": \"C LIN ANKLE 3P BLACK/WHITE\"\r\n\t\t\t\t}\r\n\t\t\t}\r\n\t\t}\r\n\t],\r\n\t\"passFails\": [\r\n\t\t{\r\n\t\t\t\"title\": \"mcs_availability_signature_compliance\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"1_general_compliance\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"shas_compliance \",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"1_general_compliance\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"a_01_compliance\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"1_general_compliance\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"cpsia_compliance\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"1_general_compliance\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"customer_country_specific_compliance\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"1_general_compliance\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"production_finish_goods\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"2_metal_detection_compliance\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"warehouse_outer_carton\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"2_metal_detection_compliance\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"finished_goods_testing_pass\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"3_fgt_compliance\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"uv_c_treatment\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"4_mold_prevention\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"anti_mold_wrapping_paper\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"4_mold_prevention\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"moisture_control_box\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"4_mold_prevention\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"moisture_control_product\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"4_mold_prevention\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"exceptional_visual_standard\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"5_exceptional_management\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"factory_disclaimer\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"validation\",\r\n\t\t\t\"checkListSubsection\": \"5_exceptional_management\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"title\": \"slip_on_inspection_pass_step_in_tool\",\r\n\t\t\t\"value\": \"yes\",\r\n\t\t\t\"type\": \"check-list\",\r\n\t\t\t\"subsection\": \"checklist\",\r\n\t\t\t\"checkListSubsection\": \"1_fit\",\r\n\t\t\t\"status\": \"pass\",\r\n\t\t\t\"comment\": \"\"\r\n\t\t}\r\n\t]\r\n}", null, "application/json"); request.Content = content;
        //        var response = await client.SendAsync(request);
        //        response.EnsureSuccessStatusCode();
        //        Console.WriteLine(await response.Content.ReadAsStringAsync());
        //        if (response.IsSuccessStatusCode)
        //        {
        //            //await POSTAsync();
        //            rt.TYPE = "S";
        //            rt.MESSAGE = "Success";
        //        }

        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        throw ex;
        //    }
        //}

        public void PUTAsync()
        {
            try
            {

                string sql = $@"select UNIQUE_KEY
, STATUS
, DATE_STARTED
, DEFECTIVE_PARTS
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
, PASSFAILS_0_TITLE
, PASSFAILS_0_TYPE
, PASSFAILS_0_SUBSECTION
, PASSFAILS_0_LISTVALUES_VALUE
--,INSERT_DATE
--,IS_SYNC
--,STATUS_CODE
from t_aeqs_to_p88_list where to_char(DATE_STARTED,'yyyy/mm/dd')= '2023/07/04' and ASSIGNMENT_ITEMS_ASSIGNMENT_REPORT_TYPE_ID=9"; //
                OracleCommand cmd = new OracleCommand(sql, conmes);
                cmd.CommandType = CommandType.Text;
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataTable dtlist = new DataTable();
                da.Fill(dtlist);


                foreach (DataRow item in dtlist.Rows)
                {
                    string sql1 = $@"select UNION_ID
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
                    //string sections = string.Empty;
                    List<string> lstsections = new List<string>();
                    foreach (DataRow itemsections in dtsections.Rows)
                    {




                        //sections += "\"sections\":[{\"type\":\'" + itemsections["SECTIONS_TYPE"] + "\',\"title\":\'" + itemsections["SECTIONS_TITLE"] + "\',\"section_result_id\":\'" + itemsections["SECTIONS_RESULT_ID"] + "\',\"qty_inspected\":\'" + itemsections["SECTIONS_QTY_INSPECTED"] + "\'," +
                        //"\"sampled_inspected\":\'" + itemsections["SECTIONS_SAMPLED_INSPECTED"] + "\',\"defective_parts\":\'" + itemsections["SECTIONS_DEFECTIVE_PARTS"] + "\',\"inspection_level\":\'" + itemsections["SECTIONS_INSPECTION_LEVEL"] + "\',\"inspection_method\":\'" + itemsections["SECTIONS_INSPECTION_METHOD"] + "\',\"aql_minor\":\'" + itemsections["SECTIONS_AQL_MINOR"] + "\'," +
                        //"\"aql_major\":\'" + itemsections["SECTIONS_AQL_MAJOR"] + "\',\"aql_critical\":\'" + itemsections["SECTIONS_AQL_CRITICAL"] + "\',\"barcodes\":[{\"value\":\"\'" + itemsections["SECTIONS_BARCODES_VALUE"] + "\'\"}]," +
                        //"\"qty_type\":\'" + itemsections["SECTIONS_QTY_TYPE"] + "\',\"max_minor_defects\":\'" + itemsections["SECTIONS_MAX_MINOR_DEFECTS"] + "\',\"max_major_defects\":\'" + itemsections["SECTIONS_MAX_MAJOR_DEFECTS"] + "\',\"max_major_a_defects\":\'" + itemsections["SECTIONS_MAX_MAJOR_A_DEFECTS"] + "\',\"max_major_b_defects\":\'" + itemsections["SECTIONS_MAX_MAJOR_B_DEFECTS"] + "\',\"max_critical_defects\":\'" + itemsections["SECTIONS_MAX_CRITICAL_DEFECTS"] + "\'," +
                        //"\"defects\":[{\"label\":\'" + itemsections["SECTIONS_DEFECTS_LABEL"] + "\'," +
                        //"\"subsection\":\'" + itemsections["SECTIONS_DEFECTS_SUBSECTION"] + "\',\"code\":\'" + itemsections["SECTIONS_DEFECTS_CODE"] + "\',\"critical_level\":\'" + itemsections["SECTIONS_DEFECTS_CRITICAL_LEVEL"] + "\',\"major_level\":\'" + itemsections["SECTIONS_DEFECTS_MAJOR_LEVEL"] + "\',\"minor_level\":\'" + itemsections["SECTIONS_DEFECTS_MINOR_LEVEL"] + "\'," +
                        //"\"comments\":\'" + itemsections["SECTIONS_DEFECTS_COMMENTS"] + "\',";

                        lstsections.Add("{\"type\":\"" + itemsections["SECTIONS_TYPE"] + "\",\"title\":\"" + itemsections["SECTIONS_TITLE"] + "\",\"section_result_id\":\"" + itemsections["SECTIONS_RESULT_ID"] + "\",\"qty_inspected\":\"" + itemsections["SECTIONS_QTY_INSPECTED"] + "\"," +
                        "\"sampled_inspected\":\"" + itemsections["SECTIONS_SAMPLED_INSPECTED"] + "\",\"defective_parts\":\"" + itemsections["SECTIONS_DEFECTIVE_PARTS"] + "\",\"inspection_level\":\"" + itemsections["SECTIONS_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + itemsections["SECTIONS_INSPECTION_METHOD"] + "\",\"aql_minor\":\"" + itemsections["SECTIONS_AQL_MINOR"] + "\"," +
                        "\"aql_major\":\"" + itemsections["SECTIONS_AQL_MAJOR"] + "\",\"aql_critical\":\"" + itemsections["SECTIONS_AQL_CRITICAL"] + "\",\"barcodes\":[{\"value\":\"" + itemsections["SECTIONS_BARCODES_VALUE"] + "\"}]," +
                        "\"qty_type\":\"" + itemsections["SECTIONS_QTY_TYPE"] + "\",\"max_minor_defects\":\"" + itemsections["SECTIONS_MAX_MINOR_DEFECTS"] + "\",\"max_major_defects\":\"" + itemsections["SECTIONS_MAX_MAJOR_DEFECTS"] + "\",\"max_major_a_defects\":\"" + itemsections["SECTIONS_MAX_MAJOR_A_DEFECTS"] + "\",\"max_major_b_defects\":\"" + itemsections["SECTIONS_MAX_MAJOR_B_DEFECTS"] + "\",\"max_critical_defects\":\"" + itemsections["SECTIONS_MAX_CRITICAL_DEFECTS"] + "\"," +
                        "\"defects\":[{\"label\":\"" + itemsections["SECTIONS_DEFECTS_LABEL"] + "\"," +
                        "\"subsection\":\"" + itemsections["SECTIONS_DEFECTS_SUBSECTION"] + "\",\"code\":\"" + itemsections["SECTIONS_DEFECTS_CODE"] + "\",\"critical_level\":\"" + itemsections["SECTIONS_DEFECTS_CRITICAL_LEVEL"] + "\",\"major_level\":\"" + itemsections["SECTIONS_DEFECTS_MAJOR_LEVEL"] + "\",\"minor_level\":\"" + itemsections["SECTIONS_DEFECTS_MINOR_LEVEL"] + "\"," +
                        "\"comments\":\"" + itemsections["SECTIONS_DEFECTS_COMMENTS"] + "\",\"pictures\":[]}]}");
                    }

                    string sql2 = $@"select UNION_ID
,SECTIONS_DEFECTS_PICTURES_TITLE
,SECTIONS_DEFECTS_PICTURES_FULL_FILENAME
,SECTIONS_DEFECTS_PICTURES_NUMBER
,SECTIONS_DEFECTS_PICTURES_COMMENT
,SECTION_TYPE
,SECTION_TITLE from t_aeqs_to_p88_sections_f where UNION_ID ='{item["UNIQUE_KEY"]}'";
                    OracleCommand cmd2 = new OracleCommand(sql2, conmes);
                    cmd1.CommandType = CommandType.Text;
                    OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                    DataTable dtsections_f = new DataTable();
                    da2.Fill(dtsections_f);
                    //string sections_f = string.Empty;
                    List<string> lstsections_f = new List<string>();
                    foreach (DataRow itemsections_f in dtsections_f.Rows)
                    {
                        //sections_f += "\"pictures\":[{\"title\":\'" + item["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\',\"full_filename\":\'" + item["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"] + "\',\"number\":\'" + item["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\',\"comment\":\'" + item["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\'}]}]}," +
                        //"{\"type\":\"aqlDefects\",\"title\":\"product\",\"section_result_id\":1,\"defective_parts\":0,\"qty_inspected\":766,\"sampled_inspected\":32," +
                        //"\"inspection_level\":\"100%inspection\",\"inspection_method\":\"normal\",\"aql_minor\":4.0,\"aql_major\":2.5,\"aql_major_a\":1,\"aql_major_b\":1," +
                        //"\"aql_critical\":1.0,\"max_minor_defects\":15,\"max_major_defects\":15,\"max_major_a_defects\":0,\"max_major_b_defects\":0," +
                        //"\"max_critical_defects\":15,\"defects\":[{\"label\":\"OUTERCARTONDAMAGED/DIRTY.OFF-CENTER/UN-READABLE/UN-SCANNABLE,STICKER(INCLUDEUCC128LABEL)\"," +
                        //"\"subsection\":\"PACKINGANDLABELING\",\"code\":\"FTW100.01\",\"critical_level\":0,\"major_level\":1,\"minor_level\":0,\"comments\":\"testcomments1\"," +
                        //"\"pictures\":[{\"title\":\"test\",\"full_filename\":\"20230510170946286.jpg\",\"number\":2,\"comment\":\"test\"}]}]},{\"type\":\"pictures\"," +
                        //"\"title\":\"photos\",\"pictures\":[{\"title\":\"test\",\"full_filename\":\"20230510170946286.jpg\",\"number\":3,\"comment\":\"test\"}]}],";

                        lstsections_f.Add("{\"type\":\"" + itemsections_f["SECTION_TYPE"] + "\",\"title\":\"" + itemsections_f["SECTION_TITLE"] + "\",\"pictures\":[{\"title\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_TITLE"] + "\",\"full_filename\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_FULL_FILENAME"] + "\",\"number\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_NUMBER"] + "\",\"comment\":\"" + itemsections_f["SECTIONS_DEFECTS_PICTURES_COMMENT"] + "\"}]}");
                    }

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
                    cmd1.CommandType = CommandType.Text;
                    OracleDataAdapter da3 = new OracleDataAdapter(cmd3);
                    DataTable dtpassfail = new DataTable();
                    da3.Fill(dtpassfail);
                    //string sections_f = string.Empty;
                    List<string> lstpassfail = new List<string>();
                    foreach (DataRow itempassfail in dtpassfail.Rows)
                    {
                        lstpassfail.Add("{\"title\":\"mcs_confirmed_component_is_available_signature_compliant\",\"value\":\"N/A\",\"type\":\"check-list\"," +
                    "\"subsection\":\"validation\",\"checkListSubsection\":\"1_general_compliance\",\"status\":\"na\",\"comment\":\"\"}");
                    }
                    //var client = new HttpClient();
                    //var request = new HttpRequestMessage(HttpMethod.Put, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:apache_006");
                    //request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58");
                    string PostJson = "{\"status\":\"" + item["STATUS"] + "\",\"date_started\":\"" + item["DATE_STARTED"] + "\",\"defective_parts\":" + item["DEFECTIVE_PARTS"] + "," +



                    //sections {string.Join(',', sections)}
                    "\"sections\":[" + string.Join(",", lstsections) + "," + string.Join(",", lstsections_f) + "]," +

                    //assignment_items
                    "\"assignment_items\":[{\"sampled_inspected\":\"" + item["ASSIGNMENT_ITEMS_SAMPLED_INSPECTED"] + "\",\"inspection_result_id\":\"" + item["ASSIGNMENT_ITEMS_INSPECTION_RESULT_ID"] + "\",\"inspection_status_id\":\"" + item["ASSIGNMENT_ITEMS_INSPECTION_STATUS_ID"] + "\",\"qty_inspected\":\"" + item["ASSIGNMENT_ITEMS_QTY_INSPECTED"] + "\"," +
                    "\"inspection_completed_date\":\"" + item["ASSIGNMENT_ITEMS_INSPECTION_COMPLETED_DATE"] + "\",\"total_inspection_minutes\":\"" + item["ASSIGNMENT_ITEMS_TOTAL_INSPECTION_MINUTES"] + "\",\"sampling_size\":\"" + item["ASSIGNMENT_ITEMS_SAMPLING_SIZE"] + "\",\"qty_to_inspect\":\"" + item["ASSIGNMENT_ITEMS_QTY_TO_INSPECT"] + "\"," +
                    "\"aql_minor\":\"" + item["ASSIGNMENT_ITEMS_AQL_MINOR"] + "\",\"aql_major\":\"" + item["ASSIGNMENT_ITEMS_AQL_MAJOR"] + "\",\"aql_major_a\":\"" + item["ASSIGNMENT_ITEMS_AQL_MAJOR_A"] + "\",\"aql_major_b\":\"" + item["ASSIGNMENT_ITEMS_AQL_MAJOR_B"] + "\",\"aql_critical\":\"" + item["ASSIGNMENT_ITEMS_AQL_CRITICAL"] + "\",\"supplier_booking_msg\":\"" + item["ASSIGNMENT_ITEMS_SUPPLIER_BOOKING_MSG"] + "\"," +
                    "\"conclusion_remarks\":\"" + item["ASSIGNMENT_ITEMS_CONCLUSION_REMARKS"] + "\",\"assignment\":{\"report_type\":{\"id\":\"" + item["ASSIGNMENT_ITEMS_ASSIGNMENT_REPORT_TYPE_ID"] + "\"},\"inspector\":{\"username\":\"" + item["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTOR_USERNAME"] + "\"}," +
                    "\"date_inspection\":\"" + item["ASSIGNMENT_ITEMS_ASSIGNMENT_DATE_INSPECTION"] + "\",\"inspection_level\":\"" + item["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_LEVEL"] + "\",\"inspection_method\":\"" + item["ASSIGNMENT_ITEMS_ASSIGNMENT_INSPECTION_METHOD"] + "\"},\"po_line\":{\"qty\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_QTY"] + "\"," +
                    "\"etd\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_ETD"] + "\",\"eta\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_ETA"] + "\",\"color\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_COLOR"] + "\",\"size\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_SIZE"] + "\",\"style\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_STYLE"] + "\",\"po\":{\"exporter\":{\"id\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ID"] + "\"," +
                    "\"erp_business_id\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_PO_EXPORTER_ERP_BUSINESS_ID"] + "\"},\"po_number\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_PO_NUMBER"] + "\",\"customer_po\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_CUSTOMER_PO"] + "\",\"importer\":{\"id\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ID"] + "\",\"erp_business_id\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_ERP_BUSINESS_ID"] + "\"}," +
                    "\"project\":{\"id\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_IMPORTER_PROJECT_ID"] + "\"}},\"sku\":{\"sku_number\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_SKU_SKU_NUMBER"] + "\",\"item_name\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_NAME"] + "\",\"item_description\":\"" + item["ASSIGNMENT_ITEMS_PO_LINE_SKU_ITEM_DESCRIPTION"] + "\"}}}]," +





                    ////passFails
                    //"\"passFails\":[{\"title\":\"mcs_confirmed_component_is_available_signature_compliant\",\"value\":\"N/A\",\"type\":\"check-list\"," +
                    //"\"subsection\":\"validation\",\"checkListSubsection\":\"1_general_compliance\",\"status\":\"na\",\"comment\":\"\"}]}";
                    //passFails
                    "\"passFails\":[" + string.Join(",", lstpassfail) + "]}";

                    //var content = new StringContent(PostJson, null, "application/json");
                    //request.Content = content;
                    //var response = await client.SendAsync(request);
                    //response.EnsureSuccessStatusCode();
                    ////Console.WriteLine(await response.Content.ReadAsStringAsync());
                    //if (response.IsSuccessStatusCode)
                    //{
                    //    await POSTAsync();
                    //    rt.TYPE = "S";
                    //    rt.MESSAGE = "Success";
                    //    // returnMsg = "Success";
                    //}
                }
            }
            catch (HttpRequestException ex)
            {
                //returnMsg = "Fail" + ex.ToString();
                throw;
            }
        }

        public async Task POSTAsync()
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://adidasstage4.pivot88.com/rest/operation/v1/inspection_reports/unique_key:apache_005/images/upload");
                request.Headers.Add("api-key", "e1e43ff9-aee0-478c-a447-c2b8ab44ae58");
                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(File.OpenRead("E:\\SAP\\MES\\Final_Version_SourceCode\\MES&WMS(SAP Version)\\MES&WMS to SAP(Interface)\\Development\\POST_TO_PIVOT88\\POST_TO_SAP\\FMSPlatForm\\FMS\\images\\upload\\20230510170946286.jpg")), "file", "20230510170946286.jpg");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException ex)
            {
                throw ex;
            }
        }


    }

}

