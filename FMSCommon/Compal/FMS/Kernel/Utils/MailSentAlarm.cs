using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Xml;
using System.IO;
using Compal.FMS.Kernel.Beans;
using System.Windows.Forms;

namespace Compal.FMS.Kernel.Utils
{
    public class MailSentAlarm
    {
        public void SendException(AlarmMailInfo vMailInfo)
        {
            SendException(vMailInfo, true);
        }

        public void SendException(AlarmMailInfo vMailInfo, bool bBodyHTML)
        {
            MailMessage tempMail;
            SmtpClient smtpClient;

            tempMail = new MailMessage();
            smtpClient = new SmtpClient();
            try
            {
                smtpClient.Host = vMailInfo.SMTPHost;
                tempMail.From = new MailAddress(vMailInfo.MailFrom);

                tempMail.Subject = vMailInfo.MailExceptionSubject;
                if (!String.IsNullOrEmpty(vMailInfo.MailTo))
                {
                    foreach (string tmpMailTo in vMailInfo.MailTo.Split(';'))
                    {
                        if (!String.IsNullOrEmpty(tmpMailTo))
                            tempMail.To.Add(tmpMailTo);
                    }
                }

                if (!String.IsNullOrEmpty(vMailInfo.MailCc))
                {
                    foreach (string tmpMailCc in vMailInfo.MailCc.Split(';'))
                    {
                        if (!String.IsNullOrEmpty(tmpMailCc))
                            tempMail.CC.Add(tmpMailCc);
                    }
                }
                tempMail.Body = vMailInfo.MailExceptionContent;
                tempMail.IsBodyHtml = bBodyHTML;
                switch (vMailInfo.MailPriority.ToUpper())
                {
                    case "LOW":
                        tempMail.Priority = MailPriority.Low;
                        break;
                    case "NORMAL":
                        tempMail.Priority = MailPriority.Normal;
                        break;
                    case "HIGH":
                        tempMail.Priority = MailPriority.High;
                        break;
                    default:
                        tempMail.Priority = MailPriority.Normal;
                        break;
                }
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Send(tempMail);
            }
            catch (Exception ex)
            {
                throw new Exception("Sent Mail Fail. Msg: " + ex.Message);
            }
        }

        public void SendOutOfProcess(AlarmMailInfo vMailInfo)
        {
        
        }
         /*
        public void MailSent(bool bBodyHTML)
        {
            try
            {
                MailMessage tempMail = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = this.SMTPHost;
                tempMail.From = new MailAddress(this.MailFrom);
                tempMail.Subject = this.MailExceptionSubject;
                //tempMail.CC = this.MailCc;
                tempMail.Body = this.MailExceptionContent;
                tempMail.IsBodyHtml = bBodyHTML;
                smtpClient.Credentials = new System.Net.NetworkCredential();
                
                #region Mail Priority
                switch (this.MailProprity.ToUpper())
                {
                    case "LOW":
                        tempMail.Priority = MailPriority.Low;
                        break;
                    case "NORMAL":
                        tempMail.Priority = MailPriority.Normal;
                        break;
                    case "HIGH":
                        tempMail.Priority = MailPriority.High;
                        break;
                    default:
                        tempMail.Priority = MailPriority.Normal;
                        break;
                }
                #endregion

                if (this.MailTo.Length != 0)
                {
                    foreach (string mailTo in this.MailTo.Split(';'))
                    {
                        if (mailTo != null && mailTo != "")
                            tempMail.To.Add(mailTo);
                    }
                }

                smtpClient.UseDefaultCredentials = true;
                smtpClient.SendException(tempMail);
            }
            catch (Exception ex)
            {
                throw new Exception("Sent Mail Fail. Msg: " + ex.Message);
            }
        }*/
    }
}