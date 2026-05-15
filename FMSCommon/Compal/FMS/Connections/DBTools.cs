using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using Compal.FMS.Connections.DBLoader;
using Oracle.ManagedDataAccess.Client;
using Compal.FMS.Component;
using log4net;

namespace Compal.FMS.Connections
{
    public class DBTools
    {
        private ILog mesLog;
        private string mDBName;//@JC03A
        private ConnectionLoader tempSingleton;
        
        //@JC03A start
        public DBTools(string dbName)
        {
            this.mDBName = dbName;
            this.mesLog = LogManager.GetLogger(FMSLog.DATABASE);
            tempSingleton = new ConnectionLoader();
        }
        //@JC03A end
        

        /// <summary>
        /// Execute Query (Save OralceParameters by Hashtable)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <param name="oraParams"></param>
        /// <returns>string</returns>
        public string ExecuteQueryStrHt(string sqlCommandText, Hashtable oraParams)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand;
            OracleDataReader oraReader;
            string tmpResult = null, result = null;

            try
            {
                mesConn = tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                mesCommand = this.tempSingleton.AddOracleParamsFromHtable(mesCommand, oraParams);
                oraReader = mesCommand.ExecuteReader();
                if (oraReader.Read())
                {
                    if (!oraReader.IsDBNull(0))
                    {
                        tmpResult = (string)oraReader.GetOracleString(0);
                    }
                }
                result = tmpResult;
                oraReader.Dispose();
            }
            catch (Exception ex)
            {
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);//@JC02A
                }
            }
            finally
            {

                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Execute Query (Save OralceParameters by Hashtable)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <param name="oraParams"></param>
        /// <returns>int</returns>
        public int ExecuteQueryIntHt(string sqlCommandText, Hashtable oraParams)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand = null;
            OracleDataReader oraReader = null;
            int cntResult =0;
            int result = 0;

            try
            {
                mesConn = this.tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                mesCommand = this.tempSingleton.AddOracleParamsFromHtable(mesCommand, oraParams);
                oraReader = mesCommand.ExecuteReader();
                if (oraReader.Read())
                {
                    if (!oraReader.IsDBNull(0))
                    {
                        cntResult = (int)oraReader.GetInt32(0);
                    }
                }
                result = cntResult;
                oraReader.Dispose();
            }
            catch (Exception ex)
            {
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);//@JC02A
                }
            }
            finally
            {
                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
                if (oraReader != null)
                {
                    oraReader.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// Execute Query (Save OralceParameters by Hashtable)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <param name="oraParams"></param>
        /// <returns>ExecutionResult (DataSet Save in Anything)</returns>
        public ExecutionResult ExecuteQueryDSHt(string sqlCommandText, Hashtable oraParams)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand =null;
            OracleDataAdapter mesOraDataAdapter =null;
            ExecutionResult result;
            DataSet resultDS;

            result = new ExecutionResult();
            resultDS = new DataSet();
            try
            {
                mesConn = this.tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                mesCommand = this.tempSingleton.AddOracleParamsFromHtable(mesCommand, oraParams);
                mesOraDataAdapter = new OracleDataAdapter(mesCommand);
                mesOraDataAdapter.Fill(resultDS);
                result.Status = true;
                result.Message = "OK";
                result.Anything = resultDS;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);//@JC02A
                }
            }
            finally
            {

                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Execute Update (Save OralceParameters by Hashtable)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <param name="oraParams"></param>
        /// <returns>ExecutionResult</returns>
        public ExecutionResult ExecuteUpdateHt(string sqlCommandText, Hashtable oraParams)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand = null;
            ExecutionResult result;
            
            result = new ExecutionResult();
            try
            {
                mesConn = this.tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                mesCommand = this.tempSingleton.AddOracleParamsFromHtable(mesCommand, oraParams);
                mesCommand.ExecuteNonQuery();
                result.Status = true;
                result.Message = "OK";
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);//@JC02A
                }
            }
            finally
            {

                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Add OracleParameters to MESCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="htParams"></param>
        /// <returns>MESCommand</returns>

        //@JC03A start
        /// <summary>
        /// Execute Stored Procedure (Save OracleParameters by List)
        /// </summary>
        /// <param name="spNdame"></param>
        /// <param name="oraParams"></param>
        /// <returns>ExecutionResult</returns>
        public ExecutionResult ExecuteSP(string spName, List<OracleParameter> oraParams)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand = null;
            ExecutionResult result;
            OracleParameter outputParam = null;

            result = new ExecutionResult();
            try
            {
                mesConn = this.tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.StoredProcedure;
                mesCommand.CommandText = spName;
                foreach (OracleParameter tmpOraParam in oraParams)
                {
                    mesCommand.Parameters.Add(tmpOraParam);
                    if (tmpOraParam.Direction.Equals(ParameterDirection.Output))
                    {
                        outputParam = tmpOraParam;
                    }
                }
                mesCommand.ExecuteNonQuery();
                if (outputParam == null)
                {
                    result.Message = "OK";
                    result.Anything = "OK";
                }
                else
                {
                    result.Message = "OK";
                    result.Anything = outputParam.Value.ToString();

                }
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Message = "DBTools:ExecuteSP," + ex.Message;
                result.Anything = "DBTools:ExecuteSP," + ex.Message;
                result.Status = false;
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
            }
            finally
            {

                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Execute Update (Save OracleParameters by List)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <param name="oraParams"></param>
        /// <returns>ExecutionResult</returns>
        public ExecutionResult ExecuteUpdate(string sqlCommandText, List<OracleParameter> oraParams)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand = null;
            ExecutionResult result;

            result = new ExecutionResult();
            try
            {
                mesConn = this.tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                foreach (OracleParameter tmpOraParam in oraParams)
                {
                    mesCommand.Parameters.Add(tmpOraParam);
                }
                mesCommand.ExecuteNonQuery();
                result.Status = true;
                result.Message = "OK";
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
            }
            finally
            {

                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Execute Query (Save OracleParameters by List)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <param name="oraParams"></param>
        /// <returns>ExecutionResult (DataSet Save in Anything)</returns>
        public ExecutionResult ExecuteQueryDS(string sqlCommandText, List<OracleParameter> oraParams)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand = null;
            OracleDataAdapter mesOraDataAdapter;
            ExecutionResult result;
            DataSet resultDS;
            
            result = new ExecutionResult();
            resultDS = new DataSet();
            try
            {
                mesConn = this.tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                foreach (OracleParameter tmpOraParam in oraParams)
                {
                    mesCommand.Parameters.Add(tmpOraParam);
                }
                mesOraDataAdapter = new OracleDataAdapter(mesCommand);
                mesOraDataAdapter.Fill(resultDS);
                result.Status = true;
                result.Message = "OK";
                result.Anything = resultDS;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
            }
            finally
            {

                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Execute Query (No OracleParameters)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <returns>ExecutionResult (DataSet Save in Anything)</returns>
        public ExecutionResult ExecuteQueryDS(string sqlCommandText)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand = null;
            OracleDataAdapter mesOraDataAdapter;
            ExecutionResult result;
            DataSet resultDS;

            result = new ExecutionResult();
            resultDS = new DataSet();
            try
            {
                mesConn = this.tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                mesOraDataAdapter = new OracleDataAdapter(mesCommand);
                mesOraDataAdapter.Fill(resultDS);
                result.Status = true;
                result.Message = "OK";
                result.Anything = resultDS;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
            }
            finally
            {

                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Execute Query (Save OracleParameters by List)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <param name="oraParams"></param>
        /// <returns>int</returns>
        public int ExecuteQueryInt(string sqlCommandText, List<OracleParameter> oraParams)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand = null;
            OracleDataReader oraReader =null;
            int cntResult = 0, result = 0;
            try
            {
                mesConn = this.tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                foreach (OracleParameter tmpOraParam in oraParams)
                {
                    mesCommand.Parameters.Add(tmpOraParam);
                }
                oraReader = mesCommand.ExecuteReader();
                if (oraReader.Read())
                {
                    if (!oraReader.IsDBNull(0))
                    {
                        cntResult = (int)oraReader.GetInt32(0);
                    }
                }
                result = cntResult;
                oraReader.Dispose();
            }
            catch (Exception ex)
            {
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
            }
            finally
            {
                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Execute Query (No OracleParameters)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <returns>int</returns>
        public int ExecuteQueryInt(string sqlCommandText)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand = null;
            OracleDataReader oraReader =null;
            int cntResult = 0, result = 0;
            try
            {
                mesConn = this.tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                oraReader = mesCommand.ExecuteReader();
                if (oraReader.Read())
                {
                    if (!oraReader.IsDBNull(0))
                    {
                        cntResult = (int)oraReader.GetInt32(0);
                    }
                }
                result = cntResult;
                oraReader.Dispose();
            }
            catch (Exception ex)
            {
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
            }
            finally
            {

                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
                if (oraReader != null)
                {
                    oraReader.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// Execute Query (Save OracleParameters by List)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <param name="oraParams"></param>
        /// <returns>string</returns>
        public string ExecuteQueryStr(string sqlCommandText, List<OracleParameter> oraParams)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand = null;
            OracleDataReader oraReader;
            string tmpResult = null, result = null;

            try
            {
                mesConn = this.tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                foreach (OracleParameter tmpOraParam in oraParams)
                {
                    mesCommand.Parameters.Add(tmpOraParam);
                }
                oraReader = mesCommand.ExecuteReader();
                if (oraReader.Read())
                {
                    if (!oraReader.IsDBNull(0))
                    {
                        tmpResult = (string)oraReader.GetOracleString(0);
                    }
                }
                result = tmpResult;
                oraReader.Dispose();
            }
            catch (Exception ex)
            {
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
            }
            finally
            {

                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Execute Query (No OracleParameters)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <returns>string</returns>
        public string ExecuteQueryStr(string sqlCommandText)
        {
            OracleConnection mesConn = null;
            OracleCommand mesCommand = null;
            OracleDataReader oraReader = null;
            string tmpResult = null, result = null;
            try
            {
                mesConn = this.tempSingleton.GetOraConnection(this.mDBName);
                if (mesConn.State != ConnectionState.Open)
                {
                    mesConn.Open();
                }
                mesCommand = mesConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                oraReader = mesCommand.ExecuteReader();
                if (oraReader.Read())
                {
                    if (!oraReader.IsDBNull(0))
                    {
                        tmpResult = (string)oraReader.GetOracleString(0);
                    }
                }
                result = tmpResult;
                oraReader.Dispose();
            }
            catch (Exception ex)
            {
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
            }
            finally
            {
                if (mesConn != null && mesConn.State != ConnectionState.Closed)
                {
                    mesConn.Close();
                }
                if (oraReader != null)
                {
                    oraReader.Dispose();
                }
            }
            return result;
        }
        //@JC03A end

    }
}
