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
    class Run_TempAndHumid_Data_Transfer
    {
        Cls_Return rt = new Cls_Return();
        SqlConnection con = new SqlConnection("Data Source=10.3.0.29;Initial Catalog=EasyMonitor;User ID=sa;Password=apc-1234;");
        SqlCommand cmd;
        SqlDataAdapter adapt;
        public Cls_Return TempAndHumid_Data_Transfer(SrvInfo vsrvinfo)
        {
            con.Open();
            DataTable dt = new DataTable();
            adapt = new SqlDataAdapter("SELECT * FROM tab_historydata a where a.SaveTime >= DATEADD(minute, -30, GETDATE())", con);
            adapt.Fill(dt); 
            con.Close();
            
                OracleConnection conoa = null;
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


                        //string sql = "select * from t_aeqs_to_p88_passfail_log a where a.passfails_status='pass'";
                        OracleCommand cmd = new OracleCommand(sql, conoa);

                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                        }
                    }

                    conoa.Close();
                    //OracleDataAdapter da = new OracleDataAdapter(cmd);

                    //DataTable dtcheck = new DataTable();

                    //da.Fill(dtcheck);

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


