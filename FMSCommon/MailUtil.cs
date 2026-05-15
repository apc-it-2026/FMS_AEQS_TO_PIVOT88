using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AutoSendEmail
{
    class MailUtil
    {
        /// <summary>
        ///     发送邮件
        /// </summary>
        /// <param name="userEmailAddress">发件人地址</param>
        /// <param name="userName">发件人姓名(可为空)</param>
        /// <param name="password">密码</param>
        /// <param name="host">邮件服务器地址</param>
        /// <param name="port"></param>
        /// <param name="sendToList">收件人(多个电子邮件地址之间必须用逗号字符（“,”）分隔)</param>
        /// <param name="sendCCList">抄送人(多个电子邮件地址之间必须用逗号字符（“,”）分隔)</param>
        /// <param name="subject">主题</param>
        /// <param name="body">内容</param>
        /// <param name="attachmentsPath">附件路径</param>
        /// <param name="errorMessage">错误信息</param>
        public static bool SendMessage(List<string> sendToList, List<string> sendCCList, string subject, string body, string[] attachmentsPath, out string errorMessage)
        {
            
            string userEmailAddress = "IT-announcement@in.apachefootwear.com";
            string userName = "MES Auto Mail Alert";
            string password = "it-123456";
            //string host = "10.2.1.186";
            //string host = "apcmailserver.apachefootwear.com";
            string host = "apcmx1.apachefootwear.com";
            int port = 25;

            //string userEmailAddress = "466910876@qq.com";
            //string userName = "466910876@qq.com";
            ////string userName = "pengtao-xu@apachefootwear.com";
            //string password = "xie198511";
            //string host = "smtp.qq.com";
            //int port = 25;


            //string userEmailAddress = "yintan-cai@apachefootwear.com";
            //string userName = "yintan-cai@apachefootwear.com";
            //string password = "Aa123456";
            //string host = "10.2.19.121";

            errorMessage = string.Empty;
            SmtpClient client = new SmtpClient();
            MailMessage msg = new MailMessage();

            msg.From = new MailAddress(userEmailAddress, userName); //发件人地址
            msg.Subject = subject; //邮件标题   
            msg.Body = body; //邮件内容   
            msg.BodyEncoding = Encoding.UTF8; //邮件内容编码   
            msg.IsBodyHtml = true; //是否是HTML邮件   
            msg.Priority = MailPriority.High; //邮件优先级   

            client.Host = host; //邮件服务器
            client.Port = port; //端口号 非SSL方式，默认端口号为：25
            client.UseDefaultCredentials = false;
            client.EnableSsl = false;
            client.Credentials = new NetworkCredential(userEmailAddress, password); //用户名、密码
            client.DeliveryMethod = SmtpDeliveryMethod.Network; //指定电子邮件发送方式    
            
            //加发件人
            foreach (string send in sendToList)
            {
                msg.To.Add(send);
            }

            //加抄送
            foreach (string cc in sendCCList)
            {
                msg.CC.Add(cc);
            }

            //在有附件的情况下添加附件
            if (attachmentsPath != null && attachmentsPath.Length > 0)
            {
                foreach (string path in attachmentsPath)
                {
                    var attachFile = new Attachment(path);
                    msg.Attachments.Add(attachFile);
                }
            }
            try
            {
                client.Send(msg);
                return true;
            }
            catch (SmtpException ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                msg.Attachments.Dispose();
            }
        }
    }
}