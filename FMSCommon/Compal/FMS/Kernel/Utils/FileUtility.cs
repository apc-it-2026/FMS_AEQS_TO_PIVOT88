using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Compal.FMS.Component;
using System.Collections;
using System.Net;
using System.Reflection;
using log4net;

namespace Compal.FMS.Kernel.Utils
{
    public class FileUtility
    {
        public static string BAK = "bak";
        public static string ERR = "err";
        public static string TMP = "tmp";
        public static string PRC = "prc";
        private ILog mesLog;

        public FileUtility()
        {
            mesLog = LogManager.GetLogger(FMSLog.PLATFORM);
        }

        public ExecutionResult MoveFile(string action, string sourceFilePath, string targetFileFolder, string srvbck, string bckfolder)
        {
            FileInfo sourceFileInfo;
            ExecutionResult result;
            string strFileDelMsg = "";

            sourceFileInfo = new FileInfo(sourceFilePath);
            result = new ExecutionResult();
            if (!Directory.Exists(targetFileFolder))
                Directory.CreateDirectory(targetFileFolder);
            try
            {
                if (File.Exists(targetFileFolder + sourceFileInfo.Name))
                    File.Delete(targetFileFolder + sourceFileInfo.Name);
                //sourceFileInfo.MoveTo(targetFileFolder + sourceFileInfo.Name);//@JC02D
                File.Copy(sourceFilePath, targetFileFolder + sourceFileInfo.Name);//JC02A     

                if (srvbck == "Y")
                {
                    if (File.Exists(bckfolder + sourceFileInfo.Name))
                        File.Delete(bckfolder + sourceFileInfo.Name);
                    //sourceFileInfo.MoveTo(targetFileFolder + sourceFileInfo.Name);//@JC02D
                    File.Copy(sourceFilePath, bckfolder + sourceFileInfo.Name);//JC02A     

                }

                //File.Delete(sourceFilePath);
                strFileDelMsg = this.LoopingDelete(sourceFilePath);//@JC03A
                if (strFileDelMsg.Equals("OK"))//del server file ok
                {
                    result.Message = action + " successful";
                    result.Status = true;
                    result.Anything = targetFileFolder + sourceFileInfo.Name;
                }
                else
                {
                    result.Message = action + " fail";
                    result.Status = false;
                    result.Anything = targetFileFolder + sourceFileInfo.Name;
                }
                if (mesLog.IsDebugEnabled)
                {
                    mesLog.Debug(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + action + " from " + sourceFilePath + " to " + targetFileFolder + "move time: " + DateTime.Now.ToString("yyyyMMddHH24mmssfff"));
                }
            }
            catch (Exception e)
            {
                result.Status = false;
                result.Message = action + " fail" + e.Message;
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(" throw exception " + e.Message);
                    mesLog.Error(" throw exception " + e.StackTrace);
                }

            }
            return result;
        }

        //@JC01A start
        public ExecutionResult MoveFile(string action, string sourceFilePath, string targetFileFolder,string postfixName)
        {
            FileInfo sourceFileInfo;
            ExecutionResult result;
            string strFileDelMsg = "";

            sourceFileInfo = new FileInfo(sourceFilePath);
            result = new ExecutionResult();
            if (!Directory.Exists(targetFileFolder))
                Directory.CreateDirectory(targetFileFolder);
            try
            {
                if (File.Exists(targetFileFolder + sourceFileInfo.Name))
                    File.Delete(targetFileFolder + sourceFileInfo.Name);
                //sourceFileInfo.MoveTo(targetFileFolder + sourceFileInfo.Name.Split('.')[0] + "_" + postfixName + "." +sourceFileInfo.Extension);//JC02D
                File.Copy(sourceFilePath, targetFileFolder + sourceFileInfo.Name.Split('.')[0] + "_" + postfixName + "." + sourceFileInfo.Extension);//JC02A                
                //File.Delete(sourceFilePath);
                strFileDelMsg = this.LoopingDelete(sourceFilePath);//@JC03A
                if (strFileDelMsg.Equals("OK"))//del server file ok
                {
                    result.Message = action + " successful";
                    result.Status = true;
                    result.Anything = targetFileFolder + sourceFileInfo.Name;
                }
                else
                {
                    result.Message = action + " fail";
                    result.Status = false;
                    result.Anything = targetFileFolder + sourceFileInfo.Name;
                }
                if (mesLog.IsDebugEnabled)
                {
                    mesLog.Debug(action + " from " + sourceFilePath + "to" + targetFileFolder);
                }
            }
            catch (Exception e)
            {
                result.Status = false;
                result.Message = action + " fail" + e.Message;
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(MethodBase.GetCurrentMethod().Name +  " throw exception " + e.Message);
                    mesLog.Error(MethodBase.GetCurrentMethod().Name + " " +  e.StackTrace);
                }

            }
            return result;
        }
        //@JC01A end

     
       
        //@JC03A start
        private string LoopingDelete(string filePath)
        {
            string result = "OK";
            int i = 0;
            bool bDeleteFlag = true; ;
            //loop 5 times for issue delete. Thread sleep 1 second.
            while (bDeleteFlag)
            {
                if (File.Exists(filePath))
                {
                    try
                    {   //change file attributes
                        if (File.GetAttributes(filePath) == FileAttributes.ReadOnly)
                            File.SetAttributes(filePath, FileAttributes.Normal);
                        if (mesLog.IsDebugEnabled)
                        {
                            mesLog.Debug(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " try delete file:" + filePath + " in " + Convert.ToString(i) + " times");
                        }
                        //delete
                        File.Delete(filePath);
                        bDeleteFlag = false;
                    }
                    catch //(Exception e)
                    {
                        //if (mesLog.IsErrorEnabled)
                        //{
                        //    mesLog.Error(MethodBase.GetCurrentMethod().Name + " throw exception " + e.Message);
                        //    mesLog.Error(MethodBase.GetCurrentMethod().Name + " " + e.StackTrace);
                        //}
                    }
                }
                i++;
                if (File.Exists(filePath))
                    bDeleteFlag = true;
                else
                    bDeleteFlag = false;

                if (i == 1 && bDeleteFlag)
                {
                    if (mesLog.IsDebugEnabled)
                    {
                        mesLog.Debug("delete file:" + filePath + " with sleep 50ms");
                    }
                    //System.Threading.Thread.Sleep(50);
                    //i = 0;
                    result = "Fail";
                    bDeleteFlag = false;
                }
            }

            return result;
        }

        //@JC03A end
        /// <summary>
        /// Get local IP Address.
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddress()
        {
            string result = "";
            string strHoseName = Dns.GetHostName();
            IPHostEntry ipHose = Dns.GetHostEntry(strHoseName);
            //modified By Jesse.Ji@2014/3/19.
            foreach (IPAddress tempAddr in ipHose.AddressList)
            {
                if (tempAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    result = tempAddr.ToString();
                    break;
                }
            }
            //if (ipHose.AddressList[0].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            //{
            //    result = ipHose.AddressList[0].ToString();
            //}
            //else if (ipHose.AddressList[0].AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            //{
            //    result = ipHose.AddressList[1].ToString();
            //}

            return result;
        }

        /// Jesse.Ji Added@2014/3/19
        public static string GetProcessID()
        {
            string result = "";
            System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();
            result = proc.Id.ToString();

            return result;
        }
        /// <summary>
        /// Get current process ID.
        /// </summary>
        /// <param name="vProcessName"></param>
        /// <returns></returns>
        public static string GetProcessID(string vProcessName)
        {
            string result = "";
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName(vProcessName);
            if (procs != null && procs.Length > 0)
            {
                result = procs[0].Id.ToString();
            }
            return result;
        }
    }
}
