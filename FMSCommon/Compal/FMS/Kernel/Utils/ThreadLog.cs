using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Compal.FMS.Kernel.Beans;
using log4net;

namespace Compal.FMS.Kernel.Utils
{
    public class ThreadLog
    {
        private string mThreadName;
        private string logFilePath;
        private string logFolder;

        public ThreadLog(string threadName)
        {
            this.mThreadName = threadName;
            this.logFolder = Application.StartupPath + "\\ThreadLog\\" + DateTime.Now.ToString("yyyyMMddHH") + "\\";
            if (!Directory.Exists(this.logFolder))
                Directory.CreateDirectory(this.logFolder);
            this.logFilePath = this.logFolder + DateTime.Now.ToString("yyyyMMddHHmm") + "-" + threadName + ".log";
        }

        public void Error(string message)
        {
            string strLogMessage = string.Empty;       
            StreamWriter swLog;
            try
            {
                strLogMessage = string.Format("[{0}] [Error] [{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,ffff"), message);
                if (!File.Exists(this.logFilePath))
                {
                    swLog = new StreamWriter(this.logFilePath);
                }
                else
                {
                    swLog = File.AppendText(this.logFilePath);
                }
                swLog.WriteLine(strLogMessage);
                swLog.WriteLine();
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
        }

        public void Info(string message)
        {
            this.Info(message, false);
        }

        public void Info(string message, bool bDoWrite)
        {
            string strLogMessage = string.Empty;
            StreamWriter swLog;
            string sTime = "", eTime = "";

            if (bDoWrite)
            {
                try
                {
                    strLogMessage = string.Format("[{0}] [Info] [{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,ffff"), message);

                    if (!File.Exists(this.logFilePath))
                    {
                        swLog = new StreamWriter(this.logFilePath);
                    }
                    else
                    {
                        swLog = File.AppendText(this.logFilePath);
                    }
                    swLog.WriteLine(strLogMessage);
                    swLog.WriteLine();
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
            }
        }
    }
}