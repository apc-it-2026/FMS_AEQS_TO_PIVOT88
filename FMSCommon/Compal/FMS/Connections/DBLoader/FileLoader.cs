using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using Compal.FMS.Component;
using log4net;

namespace Compal.FMS.Connections.DBLoader
{
    public class FileLoader
    {
        private string filePath;
        private ILog mesLog;
        private OracleString oracleConnString;

        public FileLoader(string vFilePath)
        {
            this.filePath = vFilePath;
            this.mesLog = LogManager.GetLogger(FMSLog.DATABASE);
            oracleConnString = new OracleString();
        }

        public Hashtable GetDBLinks()
        {
            DBInfo authDB;
            Hashtable dbInfos;
            XmlDocument mesEnvConfig;
            XmlNodeList authDBLinkNodes;
            XmlNodeList dbNodes;
            XmlNodeList pluginDetailNodes;
            string authDBName;
            string pwdFlag;

            mesEnvConfig = new XmlDocument();
            dbInfos = new Hashtable();
            try
            {
                mesEnvConfig.Load(this.filePath);
                authDBLinkNodes = mesEnvConfig.SelectNodes("environment/db_links");
                foreach (XmlNode tmpNode in authDBLinkNodes)
                {
                    dbNodes = tmpNode.ChildNodes;
                    foreach (XmlNode tmpChildNode in dbNodes)
                    {
                        authDB = new DBInfo();
                        pluginDetailNodes = tmpChildNode.ChildNodes;
                        authDBName = "N/A";
                        foreach (XmlNode tmpDetailNode in pluginDetailNodes)
                        {
                            if (tmpDetailNode.Name.Equals("name"))
                            {
                                authDB.Name = tmpDetailNode.InnerText;//db/name  
                                authDBName = authDB.Name;
                            }
                            //else if (tmpDetailNode.Name.Equals("type"))
                            //{
                            //    authDB.Type = tmpDetailNode.InnerText;//db/host 
                            //    authDBName = authDBName + "_" + authDB.Type;
                            //}
                            else if (tmpDetailNode.Name.Equals("host"))
                            {
                                authDB.Host = tmpDetailNode.InnerText;//db/host  
                            }
                            else if (tmpDetailNode.Name.Equals("port"))
                            {
                                authDB.Port = tmpDetailNode.InnerText;//db/port  
                            }
                            else if (tmpDetailNode.Name.Equals("user"))
                            {
                                authDB.LoginUser = tmpDetailNode.InnerText;//db/user  
                            }
                            else if (tmpDetailNode.Name.Equals("password"))
                            {
                                authDB.LoginPwd = tmpDetailNode.InnerText;//db/password

                                pwdFlag = "{PWD}";

                                if (authDB.LoginPwd.IndexOf(pwdFlag) == -1)
                                {
                                    tmpDetailNode.InnerText = GetPwdString(authDB.LoginPwd) + pwdFlag;
                                }
                                else
                                {
                                    authDB.LoginPwd = GetPwdString(authDB.LoginPwd.Substring(0, authDB.LoginPwd.IndexOf(pwdFlag)));
                                }
                            }
                            else if (tmpDetailNode.Name.Equals("sid"))
                            {
                                authDB.Sid = tmpDetailNode.InnerText;//db/sid  
                            } //@Add by DX.JI 2010.02.22 Start.
                            else if(tmpDetailNode.Name.ToLower().Equals("minpoolsize"))
                            {
                                authDB.MinPoolSize = tmpDetailNode.InnerText;//db/minpoolsize
                            }
                            else if (tmpDetailNode.Name.ToLower().Equals("maxpoolsize"))
                            {
                                authDB.MaxPoolSize = tmpDetailNode.InnerText;//db/maxpoolsize
                            } //@Add by DX.JI 2010.02.22 End.
                            else if (tmpDetailNode.Name.ToLower().Equals("lifetime"))
                            {
                                authDB.LifeTime = tmpDetailNode.InnerText;
                            }
                        }
                        if (!authDBName.Equals("N/A"))
                        {
                            if (!dbInfos.ContainsKey(authDBName))
                            {
                                dbInfos.Add(authDBName, oracleConnString.ToConString(authDB));
                            }
                        }
                    }
                }
                mesEnvConfig.Save(this.filePath);
            }
            catch (Exception ex)
            {
                if (this.mesLog.IsErrorEnabled)
                {
                    this.mesLog.Error(ex.Message);
                }
            }
            return dbInfos;
        }

        private string GetPwdString(string pwd)
        {
            string sRet = "";
            for (int i = 0; i < pwd.Length; i++)
            {
                sRet = sRet + (char)(((int)(pwd[pwd.Length - 1 - i])) ^ pwd.Length);
            }
            return sRet;
        }
    }
}
