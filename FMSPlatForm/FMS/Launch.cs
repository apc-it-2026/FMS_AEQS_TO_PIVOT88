using System;
using System.Collections.Generic;
using System.Windows.Forms;
using log4net;
using Compal.FMS.Component;

namespace Compal.FMS.UI
{
    static class Launch
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                //FMSConfigReader cfgReader;
                //PrgInfo prgInfo;
                //SysParam sysParam;
                Type typeofControl;
                Form createdForm;
                //ILog mesLog;
                //mesLog = LogManager.GetLogger(FMSLog.PLATFORM);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                typeofControl = Type.GetType("Compal.FMS.UI.fmFMSMain");
                createdForm = (Form)Activator.CreateInstance(typeofControl);
                createdForm.Text = "Apache FMS " + Application.ProductVersion + "--[" +
                    System.Diagnostics.Process.GetCurrentProcess().ProcessName + "]";

                //處理未捕獲的異常     
                //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                //處理UI線程異常     
                Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                //處理非UI線程異常     
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                Application.Run(createdForm);
            }
            catch (Exception ex)
            {
                WriteErrInfo(ex.Message);
            }
        }


        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            string str = "";
            Exception error = e.Exception as Exception;
            if (error != null)
            {
                str = string.Format("出現應用程序未處理的異常\n異常類型：{0}\n異常消息：{1}\n異常位置：{2}\n",
                     error.GetType().Name, error.Message, error.StackTrace);
            }
            else
            {
                str = string.Format("應用程序線程錯誤:{0}", e);
            }
            WriteErrInfo(str);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            string str = "";

            Exception error = e.ExceptionObject as Exception;
            if (error != null)
            {
                str = string.Format("Application UnhandledException:{0};\n堆疊信息:{1}", error.Message, error.StackTrace);
            }
            else
            {
                str = string.Format("Application UnhandledError:{0}", e);
            }
            WriteErrInfo(str);
        }

        static void WriteErrInfo(string vErrMsg)
        {
            using (System.IO.FileStream fs = new System.IO.FileStream(Application.StartupPath + "\\ThreadLog\\FMS_Exception.log",
                System.IO.FileMode.Append, System.IO.FileAccess.Write))
            {
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(fs,

                     System.Text.Encoding.UTF8))
                {
                    w.WriteLine(vErrMsg); DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                }
            }

        }
    }
}









//public void Process_PO_ORDER_O(SrvInfo vsrvinfo)
//{
//    OracleConnection conerp = null;
//    OracleConnection conMiddle = null;
//    OracleConnection conMES = null;
//    try
//    {
//        string sconstr = null;
//        string iconstr = null;
//        string mconstr = null;



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
//                sconstr = htdblinks[vsrvinfo.SDB].ToString();
//            if (htdblinks.ContainsKey(vsrvinfo.IDB))
//                iconstr = htdblinks[vsrvinfo.IDB].ToString();
//            if (htdblinks.ContainsKey(vsrvinfo.MDB))
//                mconstr = htdblinks[vsrvinfo.MDB].ToString();

//            //string StartDate = "2020-10-13";
//            //string chechdate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");


//            conerp = new OracleConnection(sconstr);
//            conMiddle = new OracleConnection(iconstr);
//            conMES = new OracleConnection(mconstr);

//            conerp.Open();


//            string sql = "select * from PO_ORDER_O where TO_CHAR(last_date, 'YYYY-MM-DD') = '" + vsrvinfo.StartDate + "' or TO_CHAR(insert_date, 'YYYY-MM-DD') = '" + vsrvinfo.StartDate + "'"; // C#

//            OracleCommand cmd = new OracleCommand(sql, conerp);
//            cmd.CommandType = CommandType.Text;

//            OracleDataAdapter da = new OracleDataAdapter(cmd);
//            DataTable dt = new DataTable();
//            da.Fill(dt);
//            conerp.Dispose();
//            conerp.Close();

//            if (dt.Rows.Count > 0)
//            {

//                DialogResult result = MessageBox.Show("Want to Sync Retrived (" + dt.Rows.Count + ") Records on " + vsrvinfo.StartDate + "?", "Run FMS service",
//                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
//                if (result.Equals(DialogResult.OK))
//                {
//                    conMiddle.Open();
//                    conMES.Open();
//                    for (int i = 0; i < dt.Rows.Count; i++)
//                    {

//                        decimal? ORG_ID = null;
//                        if (dt.Rows[i]["ORG_ID"].ToString() != "")
//                            ORG_ID = Convert.ToDecimal(dt.Rows[i]["ORG_ID"]);
//                        string ORDER_NO = dt.Rows[i]["ORDER_NO"].ToString();
//                        decimal? ORDER_SEQ = null;
//                        if (dt.Rows[i]["ORDER_SEQ"].ToString() != "")
//                            ORDER_SEQ = Convert.ToDecimal(dt.Rows[i]["ORDER_SEQ"]);
//                        decimal? LOT_SEQ = null;
//                        if (dt.Rows[i]["LOT_SEQ"].ToString() != "")
//                            LOT_SEQ = Convert.ToDecimal(dt.Rows[i]["LOT_SEQ"]);
//                        string REF_NO = dt.Rows[i]["REF_NO"].ToString();
//                        decimal? REF_SEQ = null;
//                        if (dt.Rows[i]["REF_SEQ"].ToString() != "")
//                            REF_SEQ = Convert.ToDecimal(dt.Rows[i]["REF_SEQ"]);
//                        decimal? ORD_QTY = null;
//                        if (dt.Rows[i]["ORD_QTY"].ToString() != "")
//                            ORD_QTY = Convert.ToDecimal(dt.Rows[i]["ORD_QTY"]);
//                        decimal? CHK_QTY = null;
//                        if (dt.Rows[i]["CHK_QTY"].ToString() != "")
//                            CHK_QTY = Convert.ToDecimal(dt.Rows[i]["CHK_QTY"]);
//                        decimal? RTN_QTY = null;
//                        if (dt.Rows[i]["RTN_QTY"].ToString() != "")
//                            RTN_QTY = Convert.ToDecimal(dt.Rows[i]["RTN_QTY"]);


//                        string COL1 = dt.Rows[i]["COL1"].ToString();
//                        string COL2 = dt.Rows[i]["COL2"].ToString();
//                        decimal? SE_ORG = null;
//                        if (dt.Rows[i]["SE_ORG"].ToString() != "")
//                            SE_ORG = Convert.ToDecimal(dt.Rows[i]["SE_ORG"]);
//                        decimal? BOOK_ORG = null;
//                        if (dt.Rows[i]["BOOK_ORG"].ToString() != "")
//                            BOOK_ORG = Convert.ToDecimal(dt.Rows[i]["BOOK_ORG"]);
//                        decimal? RCPT_QTY = null;
//                        if (dt.Rows[i]["RCPT_QTY"].ToString() != "")
//                            RCPT_QTY = Convert.ToDecimal(dt.Rows[i]["RCPT_QTY"]);
//                        decimal? NG_QTY = null;
//                        if (dt.Rows[i]["NG_QTY"].ToString() != "")
//                            NG_QTY = Convert.ToDecimal(dt.Rows[i]["NG_QTY"]);
//                        decimal? FACT_REQQTY = null;
//                        if (dt.Rows[i]["FACT_REQQTY"].ToString() != "")
//                            FACT_REQQTY = Convert.ToDecimal(dt.Rows[i]["FACT_REQQTY"]);
//                        string INSERT_DATE = null;
//                        if (dt.Rows[i]["INSERT_DATE"].ToString() != "")
//                            INSERT_DATE = Convert.ToDateTime(dt.Rows[i]["INSERT_DATE"]).ToString("yyyy/MM/dd HH:mm:ss");
//                        string LAST_DATE = null;
//                        if (dt.Rows[i]["LAST_DATE"].ToString() != "")
//                            LAST_DATE = Convert.ToDateTime(dt.Rows[i]["LAST_DATE"]).ToString("yyyy/MM/dd HH:mm:ss");


//                        decimal? REF_QTY = null;
//                        if (dt.Rows[i]["REF_QTY"].ToString() != "")
//                            REF_QTY = Convert.ToDecimal(dt.Rows[i]["REF_QTY"]);
//                        string ITEM_NO = dt.Rows[i]["ITEM_NO"].ToString();
//                        string MRP_CODE = dt.Rows[i]["MRP_CODE"].ToString();

//                        decimal? STATUS = null;
//                        if (dt.Rows[i]["STATUS"].ToString() != "")
//                            STATUS = Convert.ToDecimal(dt.Rows[i]["STATUS"]);
//                        string COL3 = dt.Rows[i]["COL3"].ToString();

//                        decimal? COL4 = null;
//                        if (dt.Rows[i]["COL4"].ToString() != "")
//                            COL4 = Convert.ToDecimal(dt.Rows[i]["COL4"]);
//                        decimal? COL5 = null;
//                        if (dt.Rows[i]["COL5"].ToString() != "")
//                            COL5 = Convert.ToDecimal(dt.Rows[i]["COL5"]);
//                        decimal? COL6 = null;
//                        if (dt.Rows[i]["COL6"].ToString() != "")
//                            COL6 = Convert.ToDecimal(dt.Rows[i]["COL6"]);
//                        string MRP_NO = dt.Rows[i]["MRP_NO"].ToString();
//                        string ITEM_LRPZ = dt.Rows[i]["ITEM_LRPZ"].ToString();
//                        string MOVE_DATE = null;
//                        if (dt.Rows[i]["MOVE_DATE"].ToString() != "")
//                            MOVE_DATE = Convert.ToDateTime(dt.Rows[i]["MOVE_DATE"]).ToString("yyyy/MM/dd HH:mm:ss");

//                        Guid value = Guid.NewGuid();
//                        string SID = value.ToString();


//                        sql = "select * from ERP_PO_ORDER_O where ORG_ID='" + ORG_ID + "' and ORDER_NO='" + ORDER_NO + "' and ORDER_SEQ='" + ORDER_SEQ + "' and LOT_SEQ='" + LOT_SEQ + "'";
//                        cmd = new OracleCommand(sql, conMiddle);
//                        cmd.CommandType = CommandType.Text;

//                        da = new OracleDataAdapter(cmd);
//                        DataTable dtcheck = new DataTable();
//                        da.Fill(dtcheck);

//                        if (dtcheck.Rows.Count > 0)
//                        {
//                            //Update to Intermediate
//                            sql = "UPDATE ERP_PO_ORDER_O set ORG_ID='" + ORG_ID + "',ORDER_NO='" + ORDER_NO + "',ORDER_SEQ='" + ORDER_SEQ + "',LOT_SEQ='" + LOT_SEQ + "',REF_NO='" + REF_NO + "',REF_SEQ='" + REF_SEQ + "',ORD_QTY='" + ORD_QTY + "',CHK_QTY='" + CHK_QTY + "',RTN_QTY='" + RTN_QTY + "',COL1='" + COL1 + "',COL2='" + COL2 + "',SE_ORG='" + SE_ORG + "',BOOK_ORG='" + BOOK_ORG + "',RCPT_QTY='" + RCPT_QTY + "',NG_QTY='" + NG_QTY + "',FACT_REQQTY='" + FACT_REQQTY + "',INSERT_DATE=TO_DATE('" + INSERT_DATE + "', 'yyyy/mm/dd HH24:MI:SS'),LAST_DATE=TO_DATE('" + LAST_DATE + "', 'yyyy/mm/dd HH24:MI:SS'),REF_QTY='" + REF_QTY + "',ITEM_NO='" + ITEM_NO + "',MRP_CODE='" + MRP_CODE + "',STATUS='" + STATUS + "',COL3='" + COL3 + "',COL4='" + COL4 + "',COL5='" + COL5 + "',COL6='" + COL6 + "',MRP_NO='" + MRP_NO + "',ITEM_LRPZ='" + ITEM_LRPZ + "',MOVE_DATE=TO_DATE('" + MOVE_DATE + "', 'yyyy/mm/dd HH24:MI:SS'),SID='" + SID + "',MES_STATUS='N',ERP_STATUS='Update' where ORG_ID='" + ORG_ID + "' and ORDER_NO='" + ORDER_NO + "' and ORDER_SEQ='" + ORDER_SEQ + "' and LOT_SEQ='" + LOT_SEQ + "'"; // C#
//                            cmd = new OracleCommand(sql, conMiddle);
//                            cmd.CommandType = CommandType.Text;
//                            cmd.ExecuteNonQuery();
//                        }
//                        else
//                        {
//                            //Insert to Intermediate
//                            sql = "INSERT INTO ERP_PO_ORDER_O(ORG_ID,ORDER_NO,ORDER_SEQ,LOT_SEQ,REF_NO,REF_SEQ,ORD_QTY,CHK_QTY,RTN_QTY,COL1,COL2,SE_ORG,BOOK_ORG,RCPT_QTY,NG_QTY,FACT_REQQTY,INSERT_DATE,LAST_DATE,REF_QTY,ITEM_NO,MRP_CODE,STATUS,COL3,COL4,COL5,COL6,MRP_NO,ITEM_LRPZ,MOVE_DATE,SID,MES_STATUS,ERP_STATUS)" +
//                                   "VALUES ('" + ORG_ID + "','" + ORDER_NO + "','" + ORDER_SEQ + "','" + LOT_SEQ + "','" + REF_NO + "','" + REF_SEQ + "','" + ORD_QTY + "','" + CHK_QTY + "','" + RTN_QTY + "','" + COL1 + "','" + COL2 + "','" + SE_ORG + "','" + BOOK_ORG + "','" + RCPT_QTY + "','" + NG_QTY + "','" + FACT_REQQTY + "',TO_DATE('" + INSERT_DATE + "', 'yyyy/mm/dd HH24:MI:SS'),TO_DATE('" + LAST_DATE + "', 'yyyy/mm/dd HH24:MI:SS'),'" + REF_QTY + "','" + ITEM_NO + "','" + MRP_CODE + "','" + STATUS + "','" + COL3 + "','" + COL4 + "','" + COL5 + "','" + COL6 + "','" + MRP_NO + "','" + ITEM_LRPZ + "',TO_DATE('" + MOVE_DATE + "', 'yyyy/mm/dd HH24:MI:SS'),'" + SID + "','N','Insert')"; // C#
//                            cmd = new OracleCommand(sql, conMiddle);
//                            cmd.CommandType = CommandType.Text;
//                            cmd.ExecuteNonQuery();
//                        }


//                        sql = "select * from BDM_PO_ORDER_O where ORG_ID='" + ORG_ID + "' and ORDER_NO='" + ORDER_NO + "' and ORDER_SEQ='" + ORDER_SEQ + "' and LOT_SEQ='" + LOT_SEQ + "'";
//                        cmd = new OracleCommand(sql, conMES);
//                        cmd.CommandType = CommandType.Text;

//                        da = new OracleDataAdapter(cmd);
//                        dtcheck = new DataTable();
//                        da.Fill(dtcheck);

//                        if (dtcheck.Rows.Count > 0)
//                        {
//                            //Update to MES
//                            sql = "UPDATE BDM_PO_ORDER_O set ORG_ID='" + ORG_ID + "',ORDER_NO='" + ORDER_NO + "',ORDER_SEQ='" + ORDER_SEQ + "',LOT_SEQ='" + LOT_SEQ + "',REF_NO='" + REF_NO + "',REF_SEQ='" + REF_SEQ + "',ORD_QTY='" + ORD_QTY + "',CHK_QTY='" + CHK_QTY + "',RTN_QTY='" + RTN_QTY + "',COL1='" + COL1 + "',COL2='" + COL2 + "',SE_ORG='" + SE_ORG + "',BOOK_ORG='" + BOOK_ORG + "',RCPT_QTY='" + RCPT_QTY + "',NG_QTY='" + NG_QTY + "',FACT_REQQTY='" + FACT_REQQTY + "',INSERT_DATE=TO_DATE('" + INSERT_DATE + "', 'yyyy/mm/dd HH24:MI:SS'),LAST_DATE=TO_DATE('" + LAST_DATE + "', 'yyyy/mm/dd HH24:MI:SS'),REF_QTY='" + REF_QTY + "',ITEM_NO='" + ITEM_NO + "',MRP_CODE='" + MRP_CODE + "',STATUS='" + STATUS + "',COL3='" + COL3 + "',COL4='" + COL4 + "',COL5='" + COL5 + "',COL6='" + COL6 + "',MRP_NO='" + MRP_NO + "',ITEM_LRPZ='" + ITEM_LRPZ + "',MOVE_DATE=TO_DATE('" + MOVE_DATE + "', 'yyyy/mm/dd HH24:MI:SS') where ORG_ID='" + ORG_ID + "' and ORDER_NO='" + ORDER_NO + "' and ORDER_SEQ='" + ORDER_SEQ + "' and LOT_SEQ='" + LOT_SEQ + "'"; // C#
//                            cmd = new OracleCommand(sql, conMES);
//                            cmd.CommandType = CommandType.Text;
//                            cmd.ExecuteNonQuery();
//                        }
//                        else
//                        {
//                            //Insert to MES
//                            sql = "INSERT INTO BDM_PO_ORDER_O(ORG_ID,ORDER_NO,ORDER_SEQ,LOT_SEQ,REF_NO,REF_SEQ,ORD_QTY,CHK_QTY,RTN_QTY,COL1,COL2,SE_ORG,BOOK_ORG,RCPT_QTY,NG_QTY,FACT_REQQTY,INSERT_DATE,LAST_DATE,REF_QTY,ITEM_NO,MRP_CODE,STATUS,COL3,COL4,COL5,COL6,MRP_NO,ITEM_LRPZ,MOVE_DATE)" +
//                                   "VALUES ('" + ORG_ID + "','" + ORDER_NO + "','" + ORDER_SEQ + "','" + LOT_SEQ + "','" + REF_NO + "','" + REF_SEQ + "','" + ORD_QTY + "','" + CHK_QTY + "','" + RTN_QTY + "','" + COL1 + "','" + COL2 + "','" + SE_ORG + "','" + BOOK_ORG + "','" + RCPT_QTY + "','" + NG_QTY + "','" + FACT_REQQTY + "',TO_DATE('" + INSERT_DATE + "', 'yyyy/mm/dd HH24:MI:SS'),TO_DATE('" + LAST_DATE + "', 'yyyy/mm/dd HH24:MI:SS'),'" + REF_QTY + "','" + ITEM_NO + "','" + MRP_CODE + "','" + STATUS + "','" + COL3 + "','" + COL4 + "','" + COL5 + "','" + COL6 + "','" + MRP_NO + "','" + ITEM_LRPZ + "',TO_DATE('" + MOVE_DATE + "', 'yyyy/mm/dd HH24:MI:SS'))"; // C#
//                            cmd = new OracleCommand(sql, conMES);
//                            cmd.CommandType = CommandType.Text;
//                            cmd.ExecuteNonQuery();
//                        }

//                    }

//                    conMiddle.Close();
//                    conMES.Close();

//                    conMiddle.Dispose();
//                    conMES.Dispose();

//                    MessageBox.Show("Data Sync Successfull.." + vsrvinfo.StartDate, "Run FMS service",
//                        MessageBoxButtons.OK, MessageBoxIcon.Information);
//                }
//                else
//                {

//                }


//            }
//            else
//            {
//                MessageBox.Show("No Data Exists.." + vsrvinfo.StartDate, "Run FMS service",
//                    MessageBoxButtons.OK, MessageBoxIcon.Information);
//            }

//        }
//        else
//        {
//            MessageBox.Show("No Databases Exists..", "Run FMS service",
//                    MessageBoxButtons.OK, MessageBoxIcon.Information);
//        }
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show(" Exception : " + ex.Message, "Run FMS service",
//                   MessageBoxButtons.OK, MessageBoxIcon.Information);
//    }
//    finally
//    {
//        conerp.Close();
//        conerp.Dispose();

//        conMiddle.Close();
//        conMiddle.Dispose();

//        conMES.Close();
//        conMES.Dispose();


//        GC.Collect();
//    }

//}
