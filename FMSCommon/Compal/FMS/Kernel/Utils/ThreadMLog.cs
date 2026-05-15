using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Compal.FMS.Kernel.Beans;
using log4net;
using System.Threading;
using System.Timers;

namespace Compal.FMS.Kernel.Utils
{
    public class ThreadMLog
    {
        private string mThreadName;
        private string logFilePath;
        private string logFolder;
        private List<string> mMessageList;
        private System.Timers.Timer mLogRunner;
        private bool bStatus;

        public ThreadMLog(string threadName)
        {
            this.mThreadName = threadName;
            this.logFolder = Application.StartupPath + "\\ThreadLog\\" + DateTime.Now.ToString("yyyyMMddHH") + "\\";
            if (!Directory.Exists(this.logFolder))
                Directory.CreateDirectory(this.logFolder);
            this.logFilePath = this.logFolder + DateTime.Now.ToString("yyyyMMddHHmm") + "-" + threadName + ".log";
            mMessageList = new List<string>();
            this.mLogRunner = new System.Timers.Timer();
            this.mLogRunner.Interval = 5000;
            this.mLogRunner.Elapsed += new System.Timers.ElapsedEventHandler(Output);
            this.bStatus = false;
        }

        ~ThreadMLog()
        {
            GC.Collect();
        }

        public void Error(string message)
        {
            string strLogMessage = string.Empty;
            strLogMessage = string.Format("[{0}] [Error] [{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,ffff"), message);
            Monitor.Enter(mMessageList);
            mMessageList.Add(strLogMessage);
            Monitor.Exit(mMessageList);
        }

        public void Info(string message)
        {
            string strLogMessage = string.Empty;
            strLogMessage = string.Format("[{0}] [Info] [{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,ffff"), message);
            Monitor.Enter(mMessageList);
            mMessageList.Add(strLogMessage);
            Monitor.Exit(mMessageList);
        }

        private void Output(object source, ElapsedEventArgs e)
        {
            if (!this.bStatus && this.mMessageList.Count > 0)
            {
                string[] logMessages;
                StringBuilder logBuilder;
                StreamWriter swLog;
                bStatus = true;
                Monitor.Enter(mMessageList);
                logMessages = mMessageList.ToArray();
                mMessageList.Clear();
                Monitor.Exit(mMessageList);
                logBuilder = new StringBuilder();
                foreach (string tmpMessage in logMessages)
                {
                    logBuilder.Append(tmpMessage + "\n");
                }
                try
                {
                    if (!File.Exists(this.logFilePath))
                    {
                        swLog = new StreamWriter(this.logFilePath);
                    }
                    else
                    {
                        swLog = File.AppendText(this.logFilePath);
                    }
                    swLog.Write(logBuilder.ToString());
                    swLog.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
                finally
                {
                    GC.Collect();
                }
                bStatus = false;
            }
        }
    }
}