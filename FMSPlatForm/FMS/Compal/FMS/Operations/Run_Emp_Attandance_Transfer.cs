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

        public Cls_Return Emp_Attandance_Transfer(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            OracleConnection conoa = null;
            OracleConnection con = new OracleConnection("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.3.0.15)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME = APEDB)));User Id=apctest;Password=apctest;Min Pool Size=10;Max Pool Size=20;Connection Lifetime=60000;Persist Security Info=True;");
            con.Open();
            string sql = $@" select
       ORG_ID,
       ATT_DATE,
       EMP_NO,
       A_QTY, 
       WORK_QTYS,
       HOLIDAY_QTYS,
       ABSENT_TIMES 
       from ma_holiday_d where ATT_DATE >=to_char(add_months(trunc(sysdate,'mm'),-2),'yyyyMM')";

            OracleCommand cmd = new OracleCommand(sql, con);
            OracleDataAdapter da = new OracleDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
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
                            string sql2 = $@"insert into t_tsm_ma_holiday_d( ORG_ID,
       ATT_DATE,
       EMP_NO,
       A_QTY,        
       WORK_QTYS,    
       HOLIDAY_QTYS, 
       ABSENT_TIMES ) VALUES
             (
               '{dr["ORG_ID"]}',
               '{dr["ATT_DATE"]}',
               '{dr["EMP_NO"]}', 
               '{dr["A_QTY"]}',
               '{dr["WORK_QTYS"]}',
               '{dr["HOLIDAY_QTYS"]}',
               '{dr["ABSENT_TIMES"]}')";


                            //string sql = "select * from t_aeqs_to_p88_passfail_log a where a.passfails_status='pass'";
                            OracleCommand cmd2 = new OracleCommand(sql2, conoa);

                            cmd2.CommandType = CommandType.Text;
                            cmd2.ExecuteNonQuery();
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
       
    }
}
