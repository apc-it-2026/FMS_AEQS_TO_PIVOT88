using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FMSCommon.Compal.FMS.Kernel.Utils
{
    public static class FMSLOG
    {
        public static void Platform(string text, string operation)
        {
            if (!Directory.Exists(Application.StartupPath + "\\Log\\Platform\\" + operation + "\\"))
                Directory.CreateDirectory(Application.StartupPath + "\\Log\\Platform\\" + operation + "\\");

            string path = @"" + Application.StartupPath + "\\Log\\Platform\\" + operation + "\\fmslog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (!File.Exists(path))
                File.Create(path).Dispose();

            //string path = "D:\\IIS_SERVER Applications\\Centralize_Output_System\\MaterialMatchingMailLogs\\MailInfoLog.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                //writer.WriteLine(string.Format("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [" + text + "]"));
                writer.WriteLine(string.Format("[{0}] [{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), text));
                writer.Close();
                
            }

        }
    }
}
