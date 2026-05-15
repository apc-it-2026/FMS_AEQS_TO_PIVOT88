using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using Compal.FMS.Connections.DBLoader;
using log4net;
using Compal.FMS.Component;
using System.Diagnostics;

namespace Compal.FMS.Connections.DBLoader
{
    public class DBInfoConfig
    {
        private static string pathField;
        private Hashtable dbInfos;
        private FileLoader loader;
        private ILog mesLog;

        public DBInfoConfig()
        {

            this.mesLog = LogManager.GetLogger(FMSLog.DATABASE);
            this.loader = new FileLoader(DBInfoConfig.PATH);
            this.dbInfos = this.loader.GetDBLinks();
        }
       
        public Hashtable GetDBs()
        {           
            return this.dbInfos;
        }

        public static string PATH
        {
            get
            {
                string path = Process.GetCurrentProcess().MainModule.FileName;
                path = path.Substring(0, path.LastIndexOf("\\"));
                pathField = path + @"\";

                pathField += "database.config";

                return pathField;
            }
        }
    }
}
