using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Compal.FMS.Component;
using System.Net.NetworkInformation;
using System.Reflection;
using log4net;

namespace Compal.FMS.Kernel.Utils
{
   public class NetDiskConnected
    {
        private ILog mesLog;

        public NetDiskConnected()
        {
            mesLog = LogManager.GetLogger(FMSLog.PLATFORM);
        }

         ~NetDiskConnected()
        {
            mesLog = null;
        }

        private enum ResourceScope
        {
            RESOURCE_CONNECTED = 1,
            RESOURCE_GLOBALNET,
            RESOURCE_REMEMBERED,
            RESOURCE_RECENT,
            RESOURCE_CONTEXT
        };
        private enum ResourceType
        {
            RESOURCETYPE_ANY,
            RESOURCETYPE_DISK,
            RESOURCETYPE_PRINT,
            RESOURCETYPE_RESERVED
        };
        private enum ResourceUsage
        {
            RESOURCEUSAGE_CONNECTABLE = 0x00000001,
            RESOURCEUSAGE_CONTAINER = 0x00000002,
            RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
            RESOURCEUSAGE_SIBLING = 0x00000008,
            RESOURCEUSAGE_ATTACHED = 0x00000010,
            RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
        };
        private enum ResourceDisplayType
        {
            RESOURCEDISPLAYTYPE_GENERIC,
            RESOURCEDISPLAYTYPE_DOMAIN,
            RESOURCEDISPLAYTYPE_SERVER,
            RESOURCEDISPLAYTYPE_SHARE,
            RESOURCEDISPLAYTYPE_FILE,
            RESOURCEDISPLAYTYPE_GROUP,
            RESOURCEDISPLAYTYPE_NETWORK,
            RESOURCEDISPLAYTYPE_ROOT,
            RESOURCEDISPLAYTYPE_SHAREADMIN,
            RESOURCEDISPLAYTYPE_DIRECTORY,
            RESOURCEDISPLAYTYPE_TREE,
            RESOURCEDISPLAYTYPE_NDSCONTAINER
        };
        private enum ResourceFlags
        {
            CONNECT_UPDATE_PROFILE,
            CONNECT_UPDATE_RECENT,
            CONNECT_TEMPORARY,
            CONNECT_INTERACTIVE,
            CONNECT_PROMPT,
            CONNECT_REDIRECT,
            CONNECT_CURRENT_MEDIA,
            CONNECT_COMMANDLINE,
            CONNECT_CMD_SAVECRED,
            CONNECT_CRED_RESET
        };

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public ResourceScope dwScope = 0;
            public ResourceType dwType = 0;
            public ResourceUsage dwUsage = 0;
            public ResourceDisplayType dwDisplayType = 0;
            public string lpLocalName = null;
            public string lpRemoteName = null;
            public string lpComment = null;
            public string lpProvider = null;
        }
        [DllImport("mpr.dll")]
        private static extern IntPtr WNetAddConnection2(NETRESOURCE lpNetResource, string lpPassword,
                             string lpUsername, ResourceFlags dwFlag);

        public string ConnectedNetDisk(string strSrvPath, string strLoginUser, string strLoginPWD)
        {
            string result = "";
            IntPtr netResult;
            NETRESOURCE myResource = new NETRESOURCE();
            myResource.dwScope = ResourceScope.RESOURCE_CONNECTED;
            myResource.dwType = ResourceType.RESOURCETYPE_ANY;
            myResource.dwDisplayType = ResourceDisplayType.RESOURCEDISPLAYTYPE_GENERIC;
            myResource.dwUsage = ResourceUsage.RESOURCEUSAGE_CONNECTABLE;
            myResource.lpComment = null;
            myResource.lpLocalName = null;
            myResource.lpProvider = null;
            myResource.lpRemoteName = strSrvPath.EndsWith("\\") ? strSrvPath.TrimEnd(new char[] { '\\' }) : strSrvPath;

            netResult = WNetAddConnection2(myResource, strLoginPWD, strLoginUser, ResourceFlags.CONNECT_UPDATE_PROFILE);
            if (netResult == IntPtr.Zero)
            {
                result = "OK";
                if (mesLog.IsInfoEnabled)
                {
                    mesLog.Info(MethodBase.GetCurrentMethod().Name +
                        "Connected To Server " + strSrvPath + " success.");
                }
            }
            else
            {
                result = "Connected To Server " + strSrvPath + " Fail. Window ErrorCode:" + netResult.ToString();
                if (mesLog.IsErrorEnabled)
                    mesLog.Error(MethodBase.GetCurrentMethod().Name +
                        "Connected To Server " + strSrvPath + " Fail. Login User: " + strLoginUser + ". Windows Error Code: " + netResult.ToString());
            }
            return result;
        }

        public bool NetIPConnectionCheck(string strNetIP,int timeOut)
        {
            bool result = false;
            Ping ping;
            PingReply netReply = null;
            // int timeOut = 2;

            ping = new Ping();
            try
            {
                netReply = ping.Send(strNetIP, timeOut);
                //add message log.
                if (netReply.Status != IPStatus.Success)
                {
                    if (mesLog.IsErrorEnabled)
                    {
                        mesLog.Error(MethodBase.GetCurrentMethod().Name + 
                            "IP: " + strNetIP + ". time out: " + Convert.ToString(timeOut) + " seconds. status: " +
                            netReply.Status.ToString());
                    }
                }
                else
                {
                    result = true;
                    if (mesLog.IsInfoEnabled)
                    {
                        mesLog.Info(MethodBase.GetCurrentMethod().Name + 
                            "IP: " + netReply.Address.ToString() + ". time out: " + Convert.ToString(timeOut) + " seconds. status: " +
                            netReply.Status.ToString() + ". RoundtripTime: " + netReply.RoundtripTime.ToString() +
                            " ms.");//Time to Live: " + netReply.Options.Ttl + ",Don't fragment:" + netReply.Options.DontFragment.ToString());
                    }
                }
            }
            finally
            {
                if (ping != null)
                    ping = null;
                if (netReply != null)
                    netReply = null;
                GC.Collect();
            }
            return result;
        }

        public string DisconnectedNetDisk(string strSrvPath, string strLoginUser, string strLoginPWD)
        {
            string result = "OK";

            return result;
        }
    }
}
