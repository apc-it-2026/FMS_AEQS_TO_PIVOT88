#region Copyright & License
/******************************************************************************
* This document is the property of Compal Electronics Inc, (Compal).
* No exploitation or transfer of any information contained herein is permitted 
* in the absence of an agreement with Compal, 
* and neither the document nor any such information
* may be released without the written consent of Compal
*  
* All right reserved by Compal Electronics Inc.  
*******************************************************************************
* Owner: Jason   
* Version: 1.2.0.4
* FMS.Component: MES File Monitor
* Function Description:*
* Revision / History
*------------------------------------------------------------------------------
* Flag     Date     Who             Changes Description
* -------- -------- --------------- -------------------------------------------
*          20100730 Jason           File created for new simple FMS
* JC01     20101028 Jason           local folder monitor mode
*------------------------------------------------------------------------------
*/
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;
using Compal.FMS.Component;
using Compal.FMS.Kernel.Utils;
using Compal.FMS.Kernel.Beans;
using log4net;
using System.IO;


namespace Compal.FMS.Kernel.Threading
{
    public class FileMonitorBuilder
    {
        private List<SrvInfo> mSrvInfos;
        private ILog mesLog;
        private NetDiskConnected netDiskConn;

        public FileMonitorBuilder()
        {
            mesLog = LogManager.GetLogger(FMSLog.PLATFORM);
            netDiskConn = new NetDiskConnected();
        }

        public FileMonitorBuilder(List<SrvInfo> vSrvInfos)
        {
            this.mSrvInfos = vSrvInfos;
            mesLog = LogManager.GetLogger(FMSLog.PLATFORM);
            netDiskConn = new NetDiskConnected();
        }

        ~FileMonitorBuilder()
        {
            mSrvInfos = null;
            mesLog = null;
            netDiskConn = null;

            GC.Collect();
        }

        public ExecutionResult Execute(SrvInfo vSrvInfo)
        {
            ExecutionResult result;
            ThreadStart reqMonitorThreadStart;
            Thread reqMonitorThread;
            FileMonitor requestMointor;
            result = new ExecutionResult();
            try
            {

                requestMointor = new FileMonitor(vSrvInfo);
                reqMonitorThreadStart = new ThreadStart(requestMointor.Start);
                reqMonitorThread = new Thread(reqMonitorThreadStart);
                reqMonitorThread.IsBackground = true;
                reqMonitorThread.Name = vSrvInfo.Operation;
                //start thread.
                reqMonitorThread.Start();
                //add Net IP in logicResult.
                result.Status = true;
                result.Message = vSrvInfo.Operation + ": threading server monitor success.";
                result.Anything = requestMointor;
                if (mesLog.IsInfoEnabled)
                    mesLog.Info(MethodBase.GetCurrentMethod().Name + result.Message);

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = vSrvInfo.Operation + ": threading server monitor exception. Msg:" + ex.Message;
                result.Anything = ex.Message;
                //write log. exceptions
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(MethodBase.GetCurrentMethod().Name + ".Threading server monitor " +
                        vSrvInfo.NetDiskRootPath + " fail. Msg: " + ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
            }
            finally
            {
                //logicResult=null;
                reqMonitorThreadStart = null;
                reqMonitorThread = null;
                requestMointor = null;
                GC.Collect();
            }
            return result;
        }
    }
}
