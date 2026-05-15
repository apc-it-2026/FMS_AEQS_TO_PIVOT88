using AutoSendEmail;
using Compal.FMS.Connections.DBLoader;
using Compal.FMS.Kernel.Beans;
using Compal.FMS.Kernel.Operations;
using NewExportExcels;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace FMSCommon.Compal.FMS.Kernel.Operations
{
    class Not_Sync_PO_Alerts
    {
        #region Send_Not_Sync_PO_List
        public async Task Send_Not_Sync_PO_List(SrvInfo vsrvinfo)
        {
            Cls_Return rt = new Cls_Return();
            OracleConnection conoa = null;
            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            string Error_msg = string.Empty;
            string[] listFomat= new string[] { };
            string[] attachList;
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

                    string sql = $@"select distinct a.unique_key,a.aeqs_insert_date as created_date,b.assignment_items_po_line_po_number,b.assignment_items_assignment_inspector_username,a.is_sync
  from t_aeqs_to_p88_list a
 inner join t_aeqs_to_p88_assignment b
    on a.unique_key = b.union_id
 where a.assignment_items_assignment_report_type_id = '9'
   and a.is_sync = 'N'";
                    OracleCommand cmd = new OracleCommand(sql, conoa);
                    cmd.CommandType = CommandType.Text;
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    da.Fill(dt);
                    if(dt.Rows.Count>0)
                    {
                        string sql2 = $@"select RECEIVER_LIST,COPY_LIST,MAIL_SUBJECT from TBL_MAIL_CONFIG where JOBTYPE='Pivot88_Not_Sync_PO_Alert'";
                        OracleCommand cmd2 = new OracleCommand(sql2, conoa);
                        cmd2.CommandType = CommandType.Text;
                        OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
                        da2.Fill(dt2);
                        string recipientEmail = dt2.Rows[0]["receiver_list"].ToString();
                        string ccEmail = dt2.Rows[0]["copy_list"].ToString();
                        string subject = dt2.Rows[0]["mail_subject"].ToString();

                        if (!Directory.Exists(Application.StartupPath + "\\Log\\Platform\\" + vsrvinfo.Operation + "\\"))
                            Directory.CreateDirectory(Application.StartupPath + "\\Log\\Platform\\" + vsrvinfo.Operation + "\\");

                        string _path = Application.StartupPath + "\\Log\\Platform\\" + vsrvinfo.Operation + "\\";

                        string _fileName = "Pivot88_Not_Sync_PO_Alert" + DateTime.Now.ToString("yyyyMMdd");
                        string _filePath = _path + _fileName + ".xlsx";
                        ExportExcels.ExportFomat(dt, _filePath, "sheet1", listFomat);
                        attachList = new string[] { _filePath };
                        string body = "Dear All,\n Please check the above attachment for Not Sync PO List";

                        await SendEmail(recipientEmail, ccEmail, subject, body, attachList);
                        File.Delete(_filePath);
                    }
                        
                }

              conoa.Close();
            }
            catch (Exception e)
            {
               
            }
            finally
            {
                conoa.Close();
                conoa.Dispose();

                GC.Collect();
            }
        }

        public async Task SendEmail(string recipientEmail, string ccEmail, string subject, string body, string[] attachList)
        {
            using (var client = new SmtpClient())
            {
                string userEmailAddress = "IT-announcement@in.apachefootwear.com";
                string userName = "MES Auto Mail Alert";
                string password = "it-123456";
                //string host = "10.3.0.254";
                string host = "apcmx1.apachefootwear.com";
                int port = 25;
                string errorMessage = string.Empty;
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(userEmailAddress, userName);
                msg.Subject = subject;
                msg.Body = body;
                msg.BodyEncoding = Encoding.UTF8;
                msg.IsBodyHtml = true;
                msg.Priority = MailPriority.High;
                client.Host = host;
                client.Port = port;
                client.UseDefaultCredentials = false;
                client.EnableSsl = false;
                client.Credentials = new NetworkCredential(userEmailAddress, password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                string[] Receipient = recipientEmail.Split(',');
                string[] CC = ccEmail.Split(',');

                foreach (string send in Receipient)
                {
                    msg.To.Add(send);
                }
                foreach (string send in CC)
                {
                    msg.CC.Add(send);
                }

                if (attachList != null && attachList.Length > 0)
                {
                    foreach (string path in attachList)
                    {
                        var attachFile = new Attachment(path);
                        msg.Attachments.Add(attachFile);
                    }
                }

                try
                {
                    client.Send(msg);
                }
                catch (SmtpException ex)
                {
                    errorMessage = ex.Message;
                }
                finally
                {
                    msg.Attachments.Dispose();
                }


            }
        }
        #endregion
    }
}
