using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Reflection;
using Compal.FMS.Connections.DBLoader;
using Oracle.ManagedDataAccess.Client;
using Compal.FMS.Component;
using log4net;

namespace Compal.FMS.Connections
{
    public class DBTXTools
    {
        private ILog mesLog;
        private OracleConnection mMESConn;
        private OracleTransaction mOraTransaction;
        private string strDBName;
        private ConnectionLoader tempSingleton;
         
        public DBTXTools(string dbName)
        {
            this.mesLog =LogManager.GetLogger(FMSLog.DATABASE);
            strDBName = dbName;
            tempSingleton = new ConnectionLoader();
            this.mMESConn = this.tempSingleton.GetOraConnection(this.strDBName);
        }

        /// <summary>
        /// Begins a transaction at the database.
        /// </summary>
        public void BeginTransaction()
        {
            if (this.mMESConn.State != ConnectionState.Open)
            {
                this.mMESConn.Open();
            }
            this.mOraTransaction = this.mMESConn.BeginTransaction();
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        public void Rollback()
        {
            this.mOraTransaction.Rollback();
        }

        /// <summary>
        /// Commits the SQL database transaction.
        /// </summary>
        public void Commit()
        {
            this.mOraTransaction.Commit();
        }

        /// <summary>
        /// Disposes transaction and closes current connection.
        /// </summary>
        public void EndTransaction()
        {
            if (this.mOraTransaction != null)
            {
                this.mOraTransaction.Dispose();            
            }
            if (this.mMESConn != null && this.mMESConn.State != ConnectionState.Closed)
            {
                this.mMESConn.Close();
            }   
        }

        /// <summary>
        /// Execute Stored Procedure by a transaction.
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="oraParams"></param>
        public ExecutionResult ExecuteSP(string spName, List<OracleParameter> oraParams)
        {
            OracleCommand mesCommand;
            ExecutionResult result;
            OracleParameter outputParam = null;
            result = new ExecutionResult();
            try
            {
                mesCommand = this.mMESConn.CreateCommand();
                mesCommand.CommandType = CommandType.StoredProcedure;
                mesCommand.CommandText = spName;
                mesCommand.Transaction = this.mOraTransaction;//Assign Transaction
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
                throw ex;
            }
            return result;
        }

        //@JC01A start
        /// <summary>
        /// Execute Update by a transaction. (Defines a command name)
        /// </summary>
        /// <param name="cmdName"></param>
        /// <param name="sqlCommandText"></param>
        /// <param name="oraParams"></param>
        public void ExecuteUpdate(string cmdName,string sqlCommandText, List<OracleParameter> oraParams)
        {
            OracleCommand mesCommand;
            try
            {
                mesCommand = mMESConn.CreateCommand();                
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                mesCommand.Transaction = this.mOraTransaction;//Assign Transaction
                foreach (OracleParameter tmpOraParam in oraParams)
                {
                    mesCommand.Parameters.Add(tmpOraParam);
                }
                mesCommand.ExecuteNonQuery();
           }
            catch (Exception ex)
           {
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
                throw ex;
            }
        }
        //@JC01A end

        /// <summary>
        /// Execute Update by a transaction. 
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <param name="oraParams"></param>
        public void ExecuteUpdate(string sqlCommandText, List<OracleParameter> oraParams)
        {
            OracleCommand mesCommand;
            try
            {
                mesCommand = mMESConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                mesCommand.Transaction = this.mOraTransaction;//Assign Transaction
                foreach (OracleParameter tmpOraParam in oraParams)
                {
                    mesCommand.Parameters.Add(tmpOraParam);
                }
                mesCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (mesLog.IsErrorEnabled)
                {
                    mesLog.Error(ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
                throw ex;
            }
        }
        /// <summary>
        /// Execute Query (Save OracleParameters by List)
        /// </summary>
        /// <param name="sqlCommandText"></param>
        /// <param name="oraParams"></param>
        /// <returns>ExecutionResult (DataSet Save in Anything)</returns>
        public ExecutionResult ExecuteQueryDS(string sqlCommandText)
        {
            ExecutionResult result;
            OracleDataAdapter mesOraDataAdapter;
            OracleCommand mesCommand;
            DataSet resultDS;

            resultDS = new DataSet();
            result = new ExecutionResult();
            try
            {
                mesCommand = mMESConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                mesCommand.Transaction = this.mOraTransaction;
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
                    mesLog.Error(MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                    mesLog.Error(ex.StackTrace);
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
            ExecutionResult result;
            OracleDataAdapter mesOraDataAdapter;
            OracleCommand mesCommand;
            DataSet resultDS;

            resultDS = new DataSet();
            result = new ExecutionResult();
            try
            {
                mesCommand = mMESConn.CreateCommand();
                mesCommand.CommandType = CommandType.Text;
                mesCommand.CommandText = sqlCommandText;
                mesCommand.Transaction = this.mOraTransaction;
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
                    mesLog.Error(MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                    mesLog.Error(ex.StackTrace);
                }
            }
            return result;
        }

    }
}
