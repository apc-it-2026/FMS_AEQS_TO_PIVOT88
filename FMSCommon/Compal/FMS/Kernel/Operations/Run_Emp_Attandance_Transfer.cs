using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Compal.FMS.Connections.DBLoader;
using Compal.FMS.Kernel.Beans;
using Compal.FMS.Kernel.Operations;
using Oracle.ManagedDataAccess.Client;

namespace FMSCommon.Compal.FMS.Kernel.Operations
{
    class Run_Emp_Attandance_Transfer
    {
        #region Emp_Data_Transfer_From_HR_To_MES
        public Cls_Return New_Emp_Data_Transfer(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            OracleConnection conoa = null;
          //  OracleConnection con = new OracleConnection("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS= (COMMUNITY = tcp.world)(PROTOCOL=TCP)(HOST=10.3.3.165)(PORT=1521)))(CONNECT_DATA=(SID = APEDB)));User Id=apctest;Password=apctest;Min Pool Size=10;Max Pool Size=20;Connection Lifetime=60000;Persist Security Info=True;");
          
            string APEXHROracleConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.3.2.184)(PORT=1521)))(CONNECT_DATA=(SID=APEXHR)(SERVICE_NAME=APEXHR)));User Id=APEXPAY;Password=DBAPEXPAY;";
            string MAXHROracleConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=10.3.4.242)(PORT=1521))(CONNECT_DATA=(SID = MAXHR)));User Id=maxpay;Password=dbmaxpay;";
            string APCHROracleConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.3.3.165)(PORT=1521)))(CONNECT_DATA=(SID=APEDB)(SERVICE_NAME=APEDB)));User Id=APCTEST;Password=APCTEST;";
            string HROracleConnectionStringCamphor = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.3.0.46)(PORT=1521)))(CONNECT_DATA=(SID=APEDB)(SERVICE_NAME=APEDB)));User Id=APCTEST;Password=APCTEST;";
            string HROracleConnectionStringSEZ = "Data Source=(DESCRIPTION =(ADDRESS = (COMMUNITY = tcp.world)(PROTOCOL = TCP)(Host = 10.3.0.77)(Port = 1521))(CONNECT_DATA =(SID = SEZ)));User Id=FY00;Password=FY00;";
            OracleConnection APCHR_con = new OracleConnection(APCHROracleConnectionString);
            OracleConnection ApexHR_con = new OracleConnection(APEXHROracleConnectionString);
            OracleConnection CamphorHR_con = new OracleConnection(HROracleConnectionStringCamphor);
            OracleConnection MaxKingHR_con = new OracleConnection(MAXHROracleConnectionString);
            OracleConnection SEZ_con = new OracleConnection(HROracleConnectionStringSEZ);

            APCHR_con.Open();
            string sql1 = $@"Select e.EMP_NO,
       e.NAME_E EMP_NAME,
       e.DEPT_NO,
       d.NAME_E DEPT_NAME,
       e.WORK_NO,
       gg_0002.gf_code_name(e.org_id, 'WORK', e.work_no, 'E') WORK_NAME,
       e.IN_DATE,
       e.OUT_DATE,
       EMP_TYPE,
       e.STATUS,
       e.LAST_DATE
  from EP_MAIN e
  JOIN DP_DEPT d
    ON e.DEPT_NO = d.DEPT_NO
 where e.STATUS =1 and e.org_id='100' and d.org_id = '100'
";

            OracleCommand cmd1 = new OracleCommand(sql1, APCHR_con);
            OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
            DataTable APC_Data = new DataTable();
            da1.Fill(APC_Data);
            APCHR_con.Close();


            ApexHR_con.Open();
            string sql2 = $@"Select e.EMP_NO,
       e.NAME_E EMP_NAME,
       e.DEPT_NO,
       d.NAME_E DEPT_NAME,
       e.WORK_NO,
       gg_0002.gf_code_name(e.org_id, 'WORK', e.work_no, 'E') WORK_NAME,
       e.IN_DATE,
       e.OUT_DATE,
       EMP_TYPE,
       e.STATUS,
       e.LAST_DATE
  from EP_MAIN e
  JOIN DP_DEPT d
    ON e.DEPT_NO = d.DEPT_NO
 where e.STATUS =1 and e.org_id='100' and d.org_id = '100'";

            OracleCommand cmd2 = new OracleCommand(sql2, ApexHR_con);
            OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
            DataTable Apex_Data = new DataTable();
            da2.Fill(Apex_Data);
            ApexHR_con.Close();


            CamphorHR_con.Open();
            string sql3 = $@"Select e.EMP_NO,
       e.NAME_E EMP_NAME,
       e.DEPT_NO,
       d.NAME_E DEPT_NAME,
       e.WORK_NO,
       gg_0002.gf_code_name(e.org_id, 'WORK', e.work_no, 'E') WORK_NAME,
       e.IN_DATE,
       e.OUT_DATE,
       EMP_TYPE,
       e.STATUS,
       e.LAST_DATE
  from EP_MAIN e
  JOIN DP_DEPT d
    ON e.DEPT_NO = d.DEPT_NO
 where e.STATUS =1 and e.org_id='115' and d.org_id = '115'
";

            OracleCommand cmd3 = new OracleCommand(sql3, CamphorHR_con);
            OracleDataAdapter da3 = new OracleDataAdapter(cmd3);
            DataTable Camphor_Data = new DataTable();
            da3.Fill(Camphor_Data);
            CamphorHR_con.Close();

            MaxKingHR_con.Open();
            string sql = $@"Select e.EMP_NO,
       e.NAME_E EMP_NAME,
       e.DEPT_NO,
       d.NAME_E DEPT_NAME,
       e.WORK_NO,
       gg_0002.gf_code_name(e.org_id, 'WORK', e.work_no, 'E') WORK_NAME,
       e.IN_DATE,
       e.OUT_DATE,
       EMP_TYPE,
       e.STATUS,
       e.LAST_DATE
  from EP_MAIN e
  JOIN DP_DEPT d
    ON e.DEPT_NO = d.DEPT_NO
 where e.STATUS =1 and e.org_id='100' and d.org_id = '100'
";

            OracleCommand cmd = new OracleCommand(sql, MaxKingHR_con);
            OracleDataAdapter da = new OracleDataAdapter(cmd);
            DataTable MaxKing_Data = new DataTable();
            da.Fill(MaxKing_Data);
            MaxKingHR_con.Close();

            SEZ_con.Open();
            string sql6 = $@"Select e.EMP_NO,
       e.NAME_E EMP_NAME,
       e.DEPT_NO,
       d.NAME_E DEPT_NAME,
       e.WORK_NO,
       gg_0002.gf_code_name(e.org_id, 'WORK', e.work_no, 'E') WORK_NAME,
       e.IN_DATE,
       e.OUT_DATE,
       EMP_TYPE,
       e.STATUS,
       e.LAST_DATE
  from EP_MAIN e
  JOIN DP_DEPT d
    ON e.DEPT_NO = d.DEPT_NO
 where e.STATUS =1 and e.org_id='100' and d.org_id = '100'
";

            OracleCommand cmd6 = new OracleCommand(sql6, SEZ_con);
            OracleDataAdapter da6 = new OracleDataAdapter(cmd6);
            DataTable SEZ_Data = new DataTable();
            da6.Fill(SEZ_Data);
            SEZ_con.Close();



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
                    if (APC_Data.Rows.Count > 0)
                    {
                        //Added on 20240806 starts
                        string sql4 = $@"delete from t_oa_empmain";

                        OracleCommand cmd4 = new OracleCommand(sql4, conoa);

                        cmd4.CommandType = CommandType.Text;
                        cmd4.ExecuteNonQuery();
                        //Added on 20240806 ends

                        foreach (DataRow dr in APC_Data.Rows)
                        {

                            string dept_name = dr["DEPT_NAME"].ToString();
                            dept_name= dept_name.Replace("'", string.Empty);
                            string sql5 = $@"insert into t_oa_empmain( EMP_NO,
       EMP_NAME,
       DEPT_NO,
       DEPT_NAME,        
       WORK_NO,
       WORK_NAME,
       IN_DATE,
       OUT_DATE,        
       EMP_TYPE,
       STATUS,
       LAST_DATE 
        ) VALUES
             (
               '{dr["EMP_NO"]}',
               '{dr["EMP_NAME"]}',
               '{dr["DEPT_NO"]}', 
               '{dept_name}',
               '{dr["WORK_NO"]}',
               '{dr["WORK_NAME"]}',
               TO_DATE('{dr["IN_DATE"]}', 'YYYY/MM/DD HH24:MI:SS'), 
               '{dr["OUT_DATE"]}', 
               '{dr["EMP_TYPE"]}',
               '{dr["STATUS"]}',
                TO_DATE('{dr["LAST_DATE"]}', 'YYYY/MM/DD HH24:MI:SS')
               )";

                            OracleCommand cmd5 = new OracleCommand(sql5, conoa);

                            cmd5.CommandType = CommandType.Text;
                            cmd5.ExecuteNonQuery();
                        }
                    }

                    if (Apex_Data.Rows.Count > 0)
                    {
                    
                        string sql4 = $@"delete from t_oa_apex_emp_data";

                        OracleCommand cmd4 = new OracleCommand(sql4, conoa);

                        cmd4.CommandType = CommandType.Text;
                        cmd4.ExecuteNonQuery();
                       

                        foreach (DataRow dr in Apex_Data.Rows)
                        {

                            string dept_name = dr["DEPT_NAME"].ToString();
                            dept_name = dept_name.Replace("'", string.Empty);
                            string sql5 = $@"insert into t_oa_apex_emp_data( EMP_NO,
       EMP_NAME,
       DEPT_NO,
       DEPT_NAME,        
       WORK_NO,
       WORK_NAME,
       IN_DATE,
       OUT_DATE,        
       EMP_TYPE,
       STATUS,
       LAST_DATE 
        ) VALUES
             (
               '{dr["EMP_NO"]}',
               '{dr["EMP_NAME"]}',
               '{dr["DEPT_NO"]}', 
               '{dept_name}',
               '{dr["WORK_NO"]}',
               '{dr["WORK_NAME"]}',
               TO_DATE('{dr["IN_DATE"]}', 'YYYY/MM/DD HH24:MI:SS'), 
               '{dr["OUT_DATE"]}', 
               '{dr["EMP_TYPE"]}',
               '{dr["STATUS"]}',
                TO_DATE('{dr["LAST_DATE"]}', 'YYYY/MM/DD HH24:MI:SS')
               )";

                            OracleCommand cmd5 = new OracleCommand(sql5, conoa);

                            cmd5.CommandType = CommandType.Text;
                            cmd5.ExecuteNonQuery();
                        }
                    }

                    if (Camphor_Data.Rows.Count > 0)
                    {
                        
                        string sql4 = $@"delete from t_oa_camphor_emp_data";

                        OracleCommand cmd4 = new OracleCommand(sql4, conoa);

                        cmd4.CommandType = CommandType.Text;
                        cmd4.ExecuteNonQuery();
                       

                        foreach (DataRow dr in Camphor_Data.Rows)
                        {

                            string dept_name = dr["DEPT_NAME"].ToString();
                            dept_name = dept_name.Replace("'", string.Empty);
                            string sql5 = $@"insert into t_oa_camphor_emp_data( EMP_NO,
       EMP_NAME,
       DEPT_NO,
       DEPT_NAME,        
       WORK_NO,
       WORK_NAME,
       IN_DATE,
       OUT_DATE,        
       EMP_TYPE,
       STATUS,
       LAST_DATE 
        ) VALUES
             (
               '{dr["EMP_NO"]}',
               '{dr["EMP_NAME"]}',
               '{dr["DEPT_NO"]}', 
               '{dept_name}',
               '{dr["WORK_NO"]}',
               '{dr["WORK_NAME"]}',
               TO_DATE('{dr["IN_DATE"]}', 'YYYY/MM/DD HH24:MI:SS'), 
               '{dr["OUT_DATE"]}', 
               '{dr["EMP_TYPE"]}',
               '{dr["STATUS"]}',
                TO_DATE('{dr["LAST_DATE"]}', 'YYYY/MM/DD HH24:MI:SS')
               )";

                            OracleCommand cmd5 = new OracleCommand(sql5, conoa);

                            cmd5.CommandType = CommandType.Text;
                            cmd5.ExecuteNonQuery();
                        }
                    }

                    if (MaxKing_Data.Rows.Count > 0)
                    {
                        
                        string sql4 = $@"delete from t_oa_maxking_emp_data";

                        OracleCommand cmd4 = new OracleCommand(sql4, conoa);

                        cmd4.CommandType = CommandType.Text;
                        cmd4.ExecuteNonQuery();
                        

                        foreach (DataRow dr in MaxKing_Data.Rows)
                        {

                            string dept_name = dr["DEPT_NAME"].ToString();
                            dept_name = dept_name.Replace("'", string.Empty);
                            string sql5 = $@"insert into t_oa_maxking_emp_data( EMP_NO,
       EMP_NAME,
       DEPT_NO,
       DEPT_NAME,        
       WORK_NO,
       WORK_NAME,
       IN_DATE,
       OUT_DATE,        
       EMP_TYPE,
       STATUS,
       LAST_DATE 
        ) VALUES
             (
               '{dr["EMP_NO"]}',
               '{dr["EMP_NAME"]}',
               '{dr["DEPT_NO"]}', 
               '{dept_name}',
               '{dr["WORK_NO"]}',
               '{dr["WORK_NAME"]}',
               TO_DATE('{dr["IN_DATE"]}', 'YYYY/MM/DD HH24:MI:SS'), 
               '{dr["OUT_DATE"]}', 
               '{dr["EMP_TYPE"]}',
               '{dr["STATUS"]}',
                TO_DATE('{dr["LAST_DATE"]}', 'YYYY/MM/DD HH24:MI:SS')
               )";

                            OracleCommand cmd5 = new OracleCommand(sql5, conoa);

                            cmd5.CommandType = CommandType.Text;
                            cmd5.ExecuteNonQuery();
                        }
                    }

                    if (SEZ_Data.Rows.Count > 0)
                    {

                        string sql4 = $@"delete from t_oa_sez_emp_data";

                        OracleCommand cmd4 = new OracleCommand(sql4, conoa);

                        cmd4.CommandType = CommandType.Text;
                        cmd4.ExecuteNonQuery();


                        foreach (DataRow dr in SEZ_Data.Rows)
                        {

                            string dept_name = dr["DEPT_NAME"].ToString();
                            dept_name = dept_name.Replace("'", string.Empty);
                            string sql5 = $@"insert into t_oa_sez_emp_data( EMP_NO,
       EMP_NAME,
       DEPT_NO,
       DEPT_NAME,        
       WORK_NO,
       WORK_NAME,
       IN_DATE,
       OUT_DATE,        
       EMP_TYPE,
       STATUS,
       LAST_DATE 
        ) VALUES
             (
               '{dr["EMP_NO"]}',
               '{dr["EMP_NAME"]}',
               '{dr["DEPT_NO"]}', 
               '{dept_name}',
               '{dr["WORK_NO"]}',
               '{dr["WORK_NAME"]}',
               TO_DATE('{dr["IN_DATE"]}', 'YYYY/MM/DD HH24:MI:SS'), 
               '{dr["OUT_DATE"]}', 
               '{dr["EMP_TYPE"]}',
               '{dr["STATUS"]}',
                TO_DATE('{dr["LAST_DATE"]}', 'YYYY/MM/DD HH24:MI:SS')
               )";

                            OracleCommand cmd5 = new OracleCommand(sql5, conoa);

                            cmd5.CommandType = CommandType.Text;
                            cmd5.ExecuteNonQuery();
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
        #endregion
    }
}
