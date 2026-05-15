using Compal.FMS.Connections.DBLoader;
using Compal.FMS.Kernel.Beans;
using Compal.FMS.Kernel.Operations;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace FMSCommon.Compal.FMS.Kernel.Operations
{
    class Digital_Board
    {
        #region Digital_Board
        public Cls_Return Digital_board_Data_Sync(SrvInfo vsrvinfo)
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
                        string sql = $@"SELECT SLAVE_ID, PROD_LINE
 FROM T_DIGITAL_BOARD_LINES
WHERE IS_ACTIVE = 'Y'";
                        DataTable dt = GetDataFromDatabase(constroa, sql);
                        foreach (DataRow dr in dt.Rows)
                        {
                            DataTable dt1 = new DataTable();
                            string jsonData = string.Empty;
                            string Line = dr["PROD_LINE"].ToString();
                            string ID = dr["SLAVE_ID"].ToString();
                            using (OracleCommand cmd = new OracleCommand("SP_DIGITAL_BOARD", conoa))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.Add("P_SLAVE_ID", OracleDbType.Varchar2).Value = ID;
                                cmd.Parameters.Add("P_PRODLINE", OracleDbType.Varchar2).Value = Line;
                                cmd.Parameters.Add("P_PRODDATE", OracleDbType.Date).Value = DateTime.Now;

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
                            jsonData = ConvertDataTableToJson(dt1);
                            jsonData = jsonData.Trim('[');
                            jsonData = jsonData.Trim(']');

                            string folderPath = @"\\10.3.1.250\sharefolder\Digital Board";
                            Directory.CreateDirectory(folderPath);
                            string filePath2 = Path.Combine(folderPath, dr["PROD_LINE"] + ".json");

                            if (File.Exists(filePath2))
                            {
                                File.Delete(filePath2);
                            }

                            SaveJsonToFile(jsonData, filePath2);
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
        static string ConvertDataTableToJson(DataTable dataTable)
        {
            var jsonResult = dataTable.AsEnumerable().Select(row => new Dictionary<string, object>
  {
      { "ID", row["ID"].ToString() },
      { "TIME", row["TIME"].ToString() },
      { "PLAN", GetValue(row["PLAN"]).ToString() },
      { "INPUT", GetValue(row["INPUT"]).ToString() },
      { "ACTUAL", GetValue(row["ACTUAL"]).ToString() },
      { "BALANCE", GetValue(row["BALANCE"]).ToString() },
      { "RFT", GetValue(row["RFT"]).ToString() },
      { "MP", GetValue(row["MP"]).ToString() },
      { "A.PPH", row["APPH"].ToString() },
      { "T.PPH", row["TPPH"].ToString() },
      { "IE%", row["IE"].ToString() },
      { "IE%BONUS", row["IEBONUS"].ToString()}}
            ).ToList();


            return JsonConvert.SerializeObject(jsonResult, Newtonsoft.Json.Formatting.None);
        }

        private static object GetValue(object value)
        {
            if (value is DBNull)
                return null;
            if (value is decimal || value is double || value is float)
            {
                return Convert.ToInt32(value);
            }
            if (value is int || value is long || value is short)
            {
                return Convert.ToInt32(value);
            }
            return value;
        }
        static void SaveJsonToFile(string jsonData, string filePath)
        {
            File.WriteAllText(filePath, jsonData);
        }
        #endregion
    }
}
