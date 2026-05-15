using System;
using System.Collections.Generic;
using System.Text;

namespace Compal.FMS.Kernel.Utils
{
    public class AlarmMailInfo
    {
        private string strMailFrom;
        private string strMailTo;
        private string strMailExceptionContent;
        private string strMailPriority;
        private string strSMTPHost;
        private string strMailCC;
        private string strMailPassword;
        private string strMailOutOfProcContent;
        private string strMailOutOfProcSubject;
        private string strMailBackupMonitorInterval;
        private string strMailBackupMonitorTimes;

        public string MailFrom
        {
            get { return this.strMailFrom; }
            set { this.strMailFrom = value; }
        }
        public string MailTo
        {
            get { return this.strMailTo; }
            set { this.strMailTo = value; }
        }
        public string MailExceptionSubject
        {
            get { return this.strMailOutOfProcSubject; }
            set { this.strMailOutOfProcSubject = value; }
        }

        public string MailOutOfProcSubject
        {
            get { return this.strMailOutOfProcSubject; }
            set { this.strMailOutOfProcSubject = value; }
        }

        public string MailPriority
        {
            get { return this.strMailPriority; }
            set { this.strMailPriority = value; }
        }

        public string MailExceptionContent
        {
            get { return this.strMailExceptionContent; }
            set { this.strMailExceptionContent = value; }
        }

        public string MailOutOfProcContent
        {
            get { return this.strMailOutOfProcContent; }
            set { this.strMailOutOfProcContent = value; }
        }

        public string SMTPHost
        {
            get { return this.strSMTPHost; }
            set { this.strSMTPHost = value; }
        }
        public string MailCc
        {
            get { return this.strMailCC; }
            set { this.strMailCC = value; }
        }
        public string MailPassword
        {
            get { return this.strMailPassword; }
            set { this.strMailPassword = value; }
        }
        public string BackupMonitorInterval
        {
            get { return this.strMailBackupMonitorInterval; }
            set { this.strMailBackupMonitorInterval = value; }
        }

        public string BackupMonitorTimes
        {
            get { return this.strMailBackupMonitorTimes; }
            set { this.strMailBackupMonitorTimes = value; }
        }
    }
}
