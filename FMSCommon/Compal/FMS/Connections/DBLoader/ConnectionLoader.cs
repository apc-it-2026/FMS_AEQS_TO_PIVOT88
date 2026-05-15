using System;
using System.Collections.Generic;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Collections;
using Compal.FMS.Component;

namespace Compal.FMS.Connections.DBLoader
{
    internal sealed class ConnectionLoader
    {
        private  string connectionString;
        public static Hashtable DBConString = null;
        private DBInfoConfig dbConfig = null;

        public ConnectionLoader()
        {
            if (DBConString == null || DBConString.Count == 0)
            {
                dbConfig = new DBInfoConfig();
                DBConString = dbConfig.GetDBs();
            }
        }

        public  OracleConnection GetOraConnection(string vName)
        {
            OracleConnection oraCon;
            if (DBConString != null && DBConString.ContainsKey(vName))
                connectionString = DBConString[vName].ToString();

            oraCon = new OracleConnection(connectionString);

            return oraCon;
        }

        public OracleCommand AddOracleParamsFromHtable(OracleCommand cmd, Hashtable htParams)
        {
            OracleParameter oParam = null;

            foreach (DictionaryEntry de in htParams)
            {
                oParam = new OracleParameter();
                oParam.ParameterName = ":" + de.Key.ToString();
                switch (de.Value.GetType().Name)
                {
                    case "Double":

                    case "Int32":

                    case "Decimal":
                        oParam.OracleDbType = OracleDbType.Int64;
                        break;

                    case "String":
                        oParam.OracleDbType = OracleDbType.Varchar2;
                        break;

                    case "DateTime":
                        oParam.OracleDbType = OracleDbType.Date;
                        break;
                }
                oParam.Value = de.Value;
                cmd.Parameters.Add(oParam);
            }

            return cmd;
        }

    }
}
