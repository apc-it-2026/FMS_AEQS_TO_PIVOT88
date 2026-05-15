using System;
using System.Collections.Generic;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Compal.FMS.Connections
{
    public class DBParameter
    {
        private List<OracleParameter> oraParams;
        public DBParameter()
        {
            this.oraParams = new List<OracleParameter>();
        }

        /// <summary>
        /// Add OracleParameter to List
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="oraType"></param>
        /// <param name="paramValue"></param>
        public void Add(string paramName,OracleDbType oraType, object paramValue)
        {
            OracleParameter addOraParam;
            addOraParam = new OracleParameter();
            addOraParam.OracleDbType = oraType;
            addOraParam.ParameterName = paramName;
            addOraParam.Value = paramValue;
            oraParams.Add(addOraParam);        
        }

        public void Add(string paramName, OracleDbType oraType)
        {
            OracleParameter addOraParam;
            addOraParam = new OracleParameter();
            addOraParam.OracleDbType = oraType;
            addOraParam.ParameterName = paramName;
            addOraParam.Direction = ParameterDirection.Output;
            addOraParam.Size = 50;
            oraParams.Add(addOraParam);
        }

        /// <summary>
        /// Get OracleParameter List
        /// </summary>
        /// <returns></returns>
        public List<OracleParameter> GetParameters()
        {
            return this.oraParams;
        }

        /// <summary>
        /// Clear OracleParameter List
        /// </summary>
        public void Clear()
        {
            this.oraParams.Clear();
        }
    }
}
