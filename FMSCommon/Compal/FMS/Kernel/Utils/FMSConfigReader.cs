using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using System.Xml.XPath;
using System.IO;
using Compal.FMS.Kernel.Beans;

namespace Compal.FMS.Kernel.Utils
{
    public class FMSConfigReader
    {
        private string mDLConfigFilePath;
        private string mDatabaseConfigFilePath;//@JC03A

        public FMSConfigReader()
        {

        }

        ~FMSConfigReader()
        {
            GC.Collect();
        }

        public void InitServiceConfig(string vDLConfigFilePath)
        {
            this.mDLConfigFilePath = vDLConfigFilePath;
        }


        //@JC03A start

        public void InitDatabaseConfig(string vDLConfigFilePath)
        {
            this.mDatabaseConfigFilePath = vDLConfigFilePath;
        }

        public List<string> GetDBNames()
        {
            XmlDocument dbConfigDoc;

            XmlNode baseNodes;
            List<string> result;
            string resname = "", restype = ""; ;
            dbConfigDoc = new XmlDocument();

            dbConfigDoc.Load(this.mDatabaseConfigFilePath);
            baseNodes = dbConfigDoc.SelectSingleNode("environment/db_links");
            result = new List<string>();
            foreach (XmlNode tmpNode1 in baseNodes)
            {
                foreach (XmlNode tmpNode2 in tmpNode1.ChildNodes)
                {
                    if (tmpNode2.Name.Equals("name"))
                    {
                        resname = tmpNode2.InnerText;
                    }

                }

                result.Add(resname);

            }
            return result;
        }
        //@JC03A end

        public List<SrvInfo> GetServiceInfos()
        {
            List<SrvInfo> result;
            SrvInfo tmpSrvInfo;
            XmlDocument mesSrvConfig;
            XmlNodeList srvListNodes;
            bool bSrvExist = false;

            result = new List<SrvInfo>();
            mesSrvConfig = new XmlDocument();
            try
            {
                if (File.Exists(this.mDLConfigFilePath))
                {
                    mesSrvConfig.Load(this.mDLConfigFilePath);//service.config 
                    srvListNodes = mesSrvConfig.SelectNodes(SysParam.SRV_LIST);//server_list
                    foreach (XmlNode tmpNode1 in srvListNodes)
                    {
                        #region read all nodes.
                        foreach (XmlNode tmpChildNode in tmpNode1.ChildNodes)
                        {
                            tmpSrvInfo = this.ReadConfigSrvInfo(tmpChildNode.ChildNodes);
                            tmpSrvInfo.ServiceType = tmpChildNode.Attributes[SysParam.SRV_TYPE].Value;
                            bSrvExist = this.CheckSrvInfoExistInList(tmpSrvInfo, result);
                            if (!bSrvExist)
                            {//在 XML 裡面沒有重覆才可以加入 list 裡面
                                result.Add(tmpSrvInfo);
                            }
                        }
                        #endregion 
                    }
                }
                else
                {
                    throw new ApplicationException(this.mDLConfigFilePath + " Not Exists.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;

        }

        private SrvInfo ReadConfigSrvInfo(XmlNodeList vNodeList)
        {
            SrvInfo tmpSrvInfo = null;
            try
            {
                tmpSrvInfo = new SrvInfo();
                foreach (XmlNode tmpNode2 in vNodeList)
                {
                    #region read config.

                    if (tmpNode2.Name.ToLower().Equals(SysParam.SRV_CATEGORY))
                    {// <customer>
                        tmpSrvInfo.ServiceCategory = tmpNode2.InnerText;
                    }
                    else if (tmpNode2.Name.ToLower().Equals(SysParam.S_DATABASE))
                    {// <customer>
                        tmpSrvInfo.SDB = tmpNode2.InnerText;
                    }
                    else if (tmpNode2.Name.ToLower().Equals(SysParam.I_DATABASE))
                    {//<netdisk_rootpath>
                        tmpSrvInfo.IDB = tmpNode2.InnerText;
                    }

                    else if (tmpNode2.Name.ToLower().Equals(SysParam.M_DATABASE))
                    {//<interval>
                        tmpSrvInfo.MDB = tmpNode2.InnerText;
                    }
                    else if (tmpNode2.Name.ToLower().Equals(SysParam.OPERATION))
                    {//<filtertype>
                        tmpSrvInfo.Operation = tmpNode2.InnerText;
                    }
                    else if (tmpNode2.Name.ToLower().Equals(SysParam.SYNC_TYPE))
                    {//<bakcup>
                        tmpSrvInfo.SyncType = tmpNode2.InnerText;
                    }
                    else if (tmpNode2.Name.ToLower().Equals(SysParam.SRV_INTERVAL))
                    {//<bakcup>
                        tmpSrvInfo.Interval = tmpNode2.InnerText;
                    }


                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return tmpSrvInfo;
        }

        public bool AddServiceNodeInfo(SrvInfo vSrvInfo)
        {
            bool result = false;
            XmlDocument mesSrvConfig = null;
            XmlNode srvListNodes = null;
            XmlNode srvElement = null;
            XmlNode srvNode = null;
            List<SrvInfo> lstSrvInfo = null;
            XmlAttribute serviceAttr;
            bool bCheckSrv = false;
            try
            {
                //lstSrvInfo = this.GetServiceInfos();
                //bCheckSrv = this.CheckSrvInfoExistInList(vSrvInfo, lstSrvInfo);//檢查是否已經存在
                //if (!bCheckSrv)
                //{
                mesSrvConfig = new XmlDocument();
                if (File.Exists(this.mDLConfigFilePath))
                {
                    mesSrvConfig.Load(this.mDLConfigFilePath);
                    srvListNodes = mesSrvConfig.SelectSingleNode(SysParam.SRV_LIST);
                    #region Add Elements
                    srvNode = mesSrvConfig.CreateElement(SysParam.SRV_POINT);
                    serviceAttr = mesSrvConfig.CreateAttribute(SysParam.SRV_TYPE);
                    serviceAttr.Value = "DataBase";
                    srvNode.Attributes.Append(serviceAttr);

                    //Add Service Category node
                    srvElement = mesSrvConfig.CreateElement(SysParam.SRV_CATEGORY);
                    srvElement.InnerText = vSrvInfo.ServiceCategory;
                    srvNode.AppendChild(srvElement);

                    //Add SDatabase node
                    srvElement = mesSrvConfig.CreateElement(SysParam.S_DATABASE);
                    srvElement.InnerText = vSrvInfo.SDB;
                    srvNode.AppendChild(srvElement);

                    //Add SDatabase node
                    srvElement = mesSrvConfig.CreateElement(SysParam.I_DATABASE);
                    srvElement.InnerText = vSrvInfo.IDB;
                    srvNode.AppendChild(srvElement);

                    //Add SDatabase node
                    srvElement = mesSrvConfig.CreateElement(SysParam.M_DATABASE);
                    srvElement.InnerText = vSrvInfo.MDB;
                    srvNode.AppendChild(srvElement);

                    //Add SDatabase node
                    srvElement = mesSrvConfig.CreateElement(SysParam.OPERATION);
                    srvElement.InnerText = vSrvInfo.Operation;
                    srvNode.AppendChild(srvElement);

                    //Add Timer Interval
                    srvElement = mesSrvConfig.CreateElement(SysParam.SYNC_TYPE);
                    srvElement.InnerText = vSrvInfo.SyncType;
                    srvNode.AppendChild(srvElement);

                    //Add Timer Interval
                    srvElement = mesSrvConfig.CreateElement(SysParam.SRV_INTERVAL);
                    srvElement.InnerText = vSrvInfo.Interval;
                    srvNode.AppendChild(srvElement);


                    #endregion
                    //save roote node.
                    srvListNodes.AppendChild(srvNode);

                    mesSrvConfig.Save(this.mDLConfigFilePath);
                    result = true;
                }
                else
                {
                    throw new Exception(this.mDLConfigFilePath + " Not exists. Please check.");
                }
                /*}
                else
                {
                    result = this.ModifiedServerInfo(vSrvInfo, lstSrvInfo);
                }*/
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (mesSrvConfig != null)
                    mesSrvConfig = null;
                if (srvListNodes != null)
                    srvListNodes = null;
                if (srvElement != null)
                    srvElement = null;
                if (srvNode != null)
                    srvNode = null;

                GC.Collect();
            }
            return result;
        }

        public bool ModifiedServerInfo(SrvInfo vSrvInfo, List<SrvInfo> vLstSrvInfo)
        {
            bool result = false;
            XmlDocument mesSrvConfig = null;
            XmlNode srvRootNodes = null;
            XmlNode tempServiceCategory = null;
            XmlNode tempSDB = null;
            XmlNode tempIDB = null;
            XmlNode tempMDB = null;
            XmlNode tempOperation = null;
            try
            {
                mesSrvConfig = new XmlDocument();
                if (File.Exists(this.mDLConfigFilePath))
                {
                    //if (this.CheckSrvInfoExistInList(vSrvInfo, vLstSrvInfo))
                    //{
                    mesSrvConfig.Load(this.mDLConfigFilePath);
                    srvRootNodes = mesSrvConfig.SelectSingleNode(SysParam.SRV_LIST);
                    foreach (XmlNode tempNode in srvRootNodes)
                    {
                        tempServiceCategory = tempNode.SelectSingleNode(SysParam.SRV_CATEGORY);
                        tempSDB = tempNode.SelectSingleNode(SysParam.S_DATABASE);
                        tempIDB = tempNode.SelectSingleNode(SysParam.I_DATABASE);
                        tempMDB = tempNode.SelectSingleNode(SysParam.M_DATABASE);
                        tempOperation = tempNode.SelectSingleNode(SysParam.OPERATION);

                        if (tempServiceCategory.InnerText.ToLower().Equals(vSrvInfo.ServiceCategory.ToLower()) &&
                            tempSDB.InnerText.ToLower().Equals(vSrvInfo.SDB.ToLower()) &&
                            tempIDB.InnerText.ToLower().Equals(vSrvInfo.IDB.ToLower()) &&
                            tempMDB.InnerText.ToLower().Equals(vSrvInfo.MDB.ToLower()) &&
                            tempOperation.InnerText.ToLower().Equals(vSrvInfo.Operation.ToLower()))
                        {
                            this.ChangeAllModifiedValue(vSrvInfo, tempNode);
                        }



                    }
                    mesSrvConfig.Save(this.mDLConfigFilePath);
                    result = true;
                    /*
                    }
                    else
                    {
                        throw new Exception("Monitor server [" + vSrvInfo.FileLocation + "--" + vSrvInfo.FileFilter + "] not exists, please check server List Config file.");
                    }*/
                }
                else
                {
                    throw new Exception(this.mDLConfigFilePath + " Not exists. Please Check.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (mesSrvConfig != null)
                    mesSrvConfig = null;
                if (srvRootNodes != null)
                    srvRootNodes = null;
            }
            return result;
        }

        public bool DeleteServerInfo(SrvInfo vSrvInfo) //@JC02M
        {
            bool result = false;
            XmlDocument mesSrvConfig = null;
            XmlNode srvNodes = null;
            XmlNodeList srvListNode = null;
            XmlNode tempServiceCategory = null;
            XmlNode tempSDB = null;
            XmlNode tempIDB = null;
            XmlNode tempMDB = null;
            XmlNode tempOperation = null;
            try
            {
                result = this.CheckSrvInfoExistInList(vSrvInfo, this.GetServiceInfos());//@JC02M
                if (result)
                {
                    mesSrvConfig = new XmlDocument();
                    mesSrvConfig.Load(this.mDLConfigFilePath);
                    srvListNode = mesSrvConfig.SelectSingleNode(SysParam.SRV_LIST).ChildNodes;
                    foreach (XmlNode tempNode in srvListNode)
                    {

                        tempServiceCategory = tempNode.SelectSingleNode(SysParam.SRV_CATEGORY);
                        tempSDB = tempNode.SelectSingleNode(SysParam.S_DATABASE);
                        tempIDB = tempNode.SelectSingleNode(SysParam.I_DATABASE);
                        tempMDB = tempNode.SelectSingleNode(SysParam.M_DATABASE);
                        tempOperation = tempNode.SelectSingleNode(SysParam.OPERATION);

                        if (tempServiceCategory.InnerText.ToLower().Equals(vSrvInfo.ServiceCategory.ToLower()) &&
                            tempSDB.InnerText.ToLower().Equals(vSrvInfo.SDB.ToLower()) &&
                            tempIDB.InnerText.ToLower().Equals(vSrvInfo.IDB.ToLower()) &&
                            tempMDB.InnerText.ToLower().Equals(vSrvInfo.MDB.ToLower()) &&
                            tempOperation.InnerText.ToLower().Equals(vSrvInfo.Operation.ToLower()))
                        {
                            mesSrvConfig.SelectSingleNode(SysParam.SRV_LIST).RemoveChild(tempNode);
                            result = true;
                            break;
                        }



                    }
                    mesSrvConfig.Save(this.mDLConfigFilePath);
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (mesSrvConfig != null)
                    mesSrvConfig = null;
                if (srvNodes != null)
                    srvNodes = null;
                if (srvListNode != null)
                    srvListNode = null;

                GC.Collect();
            }

            return result;
        }

        public Hashtable DeleteAllServerInfo()
        {
            Hashtable result = null;
            List<SrvInfo> lstSrvInfo = null;
            try
            {
                lstSrvInfo = this.GetServiceInfos();
                if (lstSrvInfo.Count > 0)
                {
                    result = new Hashtable();
                    foreach (SrvInfo tempSrvInfo in lstSrvInfo)
                    {
                        //result.Add(tempSrvInfo, this.DeleteServerInfo(tempSrvInfo));//@JC02M
                        this.DeleteServerInfo(tempSrvInfo);//@JC02A
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (lstSrvInfo != null)
                    lstSrvInfo = null;

                GC.Collect();
            }
            return result;
        }

        private void ChangeAllModifiedValue(SrvInfo vSrvInfo, XmlNode vSrvNode)
        {
            XmlNodeList srvList = null;
            XmlElement tempNode = null;
            try
            {
                srvList = vSrvNode.ChildNodes;
                foreach (XmlNode tmpNode2 in srvList)
                {
                    tempNode = (XmlElement)tmpNode2;

                    #region
                    if (tmpNode2.Name.ToLower().Equals(SysParam.SRV_CATEGORY))//server ip
                        tempNode.InnerText = vSrvInfo.ServiceCategory;
                    else if (tmpNode2.Name.ToLower().Equals(SysParam.S_DATABASE))//change customer.
                        tempNode.InnerText = vSrvInfo.SDB;
                    else if (tmpNode2.Name.ToLower().Equals(SysParam.I_DATABASE))//change customer.
                        tempNode.InnerText = vSrvInfo.IDB;
                    else if (tmpNode2.Name.ToLower().Equals(SysParam.M_DATABASE))//change customer.
                        tempNode.InnerText = vSrvInfo.MDB;
                    else if (tmpNode2.Name.ToLower().Equals(SysParam.OPERATION))//time interval
                        tempNode.InnerText = vSrvInfo.Operation;
                    else if (tmpNode2.Name.ToLower().Equals(SysParam.SYNC_TYPE))//time interval
                        tempNode.InnerText = vSrvInfo.SyncType;
                    else if (tmpNode2.Name.ToLower().Equals(SysParam.SRV_INTERVAL))//time interval
                        tempNode.InnerText = vSrvInfo.Interval;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        // 檢查配置是否已經存在於 service.config
        public bool CheckSrvInfoExistInList(SrvInfo vSrvInfo, List<SrvInfo> vLstSrvInfo)
        {
            bool result = false;
            if (vLstSrvInfo != null && vLstSrvInfo.Count > 0 && vSrvInfo != null)
            {
                foreach (SrvInfo tempSrvInfo in vLstSrvInfo)
                {

                    if (vSrvInfo.ServiceCategory.Equals(tempSrvInfo.ServiceCategory)
                        && vSrvInfo.SDB.ToLower().Equals(tempSrvInfo.SDB.ToLower())
                        && vSrvInfo.IDB.ToLower().Equals(tempSrvInfo.IDB.ToLower())
                        && vSrvInfo.MDB.ToLower().Equals(tempSrvInfo.MDB.ToLower())
                        && vSrvInfo.Operation.ToLower().Equals(tempSrvInfo.Operation.ToLower())
                        )
                        result = true;

                    if (result)
                        break;
                }

            }

            return result;
        }
    }
}