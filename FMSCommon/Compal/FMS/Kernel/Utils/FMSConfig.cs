using System;
using System.Collections.Generic;
using System.Text;
using Compal.FMS.Kernel.Beans;
using System.Collections;

namespace Compal.FMS.Kernel.Utils
{
    public class FMSConfig
    {
        //static List<SrvInfo> mSrvList;
        static public Hashtable mSrvHt;//JC01A

        //@JC01A start
        static public void SetServiceInfos(List<SrvInfo> vSrvInfos)
        {
            if (mSrvHt == null)
                mSrvHt = new Hashtable();
            else
            {
                mSrvHt.Clear();
            }
            foreach (SrvInfo tmpSrvInfo in vSrvInfos)
            {
                if (tmpSrvInfo.ServiceType == "DataBase")
                {
                    mSrvHt.Add(tmpSrvInfo.ServiceCategory + tmpSrvInfo.SDB + tmpSrvInfo.IDB + tmpSrvInfo.MDB + tmpSrvInfo.Operation, tmpSrvInfo);
                }
            }
            //mSrvHt.Values();          
        }

        static public List<SrvInfo> GetServiceInfos()
        {
            List<SrvInfo> result;
            result = new List<SrvInfo>();
            foreach (SrvInfo tmpSrvInfo in mSrvHt.Values)
            {
                result.Add(tmpSrvInfo);
            }
            return result;
        }

        static public bool CheckServiceRunning()
        {
            bool result;
            result = false;
            foreach (SrvInfo tempSrvInfo in mSrvHt.Values)
            {
                if (tempSrvInfo.SrvStatus.ToUpper().Equals("RUNNING"))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        static public void ChangeStatus(SrvInfo vSrvInfo, string newStatus)
        {
            SrvInfo tmpSrvInfo;
            string key;
            if (vSrvInfo.ServiceType == "DataBase")
            {
                key = vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation;
                tmpSrvInfo = (SrvInfo)mSrvHt[key];
                tmpSrvInfo.SrvStatus = newStatus;
                mSrvHt[key] = tmpSrvInfo;
            }



            //@JC02A end
        }

        static public bool RemoveService(SrvInfo vSrvInfo)
        {
            bool result = false;
            string key = "N/A";
            if (vSrvInfo.ServiceType == "DataBase")
                key = vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation;

            if (!key.Equals("N/A"))
            {
                mSrvHt.Remove(key);
                result = true;
            }
            return result;
        }

        static public bool AddService(SrvInfo vSrvInfo)
        {
            bool result = false;
            string key = "N/A";
            if (vSrvInfo.ServiceType == "DataBase")
                key = vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation;

            if (!key.Equals("N/A"))
            {
                if (!mSrvHt.ContainsKey(key))
                {
                    mSrvHt.Add(key, vSrvInfo);
                    result = true;
                }
            }
            return result;
        }

        static public SrvInfo GetServiceInfo(string vKey)
        {
            SrvInfo result = null;
            result = (SrvInfo)mSrvHt[vKey];

            return result;
        }

        static public void UpdServiceInfo(List<SrvInfo> vSrvInfos)
        {
            SrvInfo srvInfo = null;
            string strKey;

            foreach (SrvInfo tempSrvInfo in vSrvInfos)
            {
                if (tempSrvInfo.ServiceType == "DataBase")
                {
                    strKey = tempSrvInfo.ServiceCategory + tempSrvInfo.SDB + tempSrvInfo.IDB + tempSrvInfo.MDB + tempSrvInfo.Operation;
                    srvInfo = (SrvInfo)mSrvHt[strKey];//Current Hashtable
                    srvInfo.SrvStatus = tempSrvInfo.SrvStatus;
                    mSrvHt.Remove(strKey);
                    mSrvHt.Add(strKey, srvInfo);
                }


            }
        }
    }
}
