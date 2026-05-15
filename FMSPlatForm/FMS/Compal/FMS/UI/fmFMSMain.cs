using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using Compal.FMS.Component;
using Compal.FMS.Kernel.Beans;
using Compal.FMS.Kernel.Utils;
using Compal.FMS.Kernel.Threading;
using log4net;

namespace Compal.FMS.UI
{
    public partial class fmFMSMain : Form
    {
        private FMSConfigReader cfgReader;
        private ComponentResourceManager resources;
        private ILog fmsLog;
        private Hashtable mFileServiceMonitor;

        public fmFMSMain()
        {
            InitializeComponent();
            try
            {
                fmsLog = LogManager.GetLogger(FMSLog.PLATFORM);
                this.mFileServiceMonitor = new Hashtable();
                resources = new ComponentResourceManager(typeof(fmFMSMain));
                //intialize server info.
                this.IntializeSrvInfo();
                //Enable control button
                this.SyncActionButtonStatus();//@JC03A
                if (fmsLog.IsInfoEnabled)
                {
                    fmsLog.Info("FMS start successfully.");
                }
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //may need to stop all thread. then close form
            this.Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //may need to stop all thread. then close form
            this.Close();
        }

        private void IntializeSrvInfo()
        {
            this.cfgReader = new FMSConfigReader();
            this.cfgReader.InitServiceConfig(Application.StartupPath + "\\" + SysParam.SRV_FILE);//point to service.config                
            FMSConfig.SetServiceInfos(this.cfgReader.GetServiceInfos());//FMSConfigReader 取得每一個對應 Server 的設定值//@JC03A
            foreach (SrvInfo tmpSrvInfo in FMSConfig.GetServiceInfos())//@JC03M
            {
                DataGridViewRow newRow = null;
                try
                {
                    if (tmpSrvInfo != null)
                    {
                        newRow = new DataGridViewRow();
                        newRow.CreateCells(this.dgwServer);// #1 
                        newRow.Cells[0].Value = "Add";// #2 this makes it work
                        dgwServer.Rows.Add(newRow);//#3 need this before adding data

                        newRow.Cells[SysParam.DWG_SERVICE_CATEGORY].Value = tmpSrvInfo.ServiceCategory;
                        tmpSrvInfo.SrvStatus = "Stop";//@JC03A
                        newRow.Cells[SysParam.DWG_SERVICE_STATUS].Value = tmpSrvInfo.SrvStatus;
                        newRow.Cells[SysParam.DWG_S_DATABASE].Value = tmpSrvInfo.SDB;//@JC03M
                        newRow.Cells[SysParam.DWG_I_DATABASE].Value = tmpSrvInfo.IDB;

                        newRow.Cells[SysParam.DWG_M_DATABASE].Value = tmpSrvInfo.MDB;
                        newRow.Cells[SysParam.DWG_OPERATION].Value = tmpSrvInfo.Operation;
                        newRow.Cells[SysParam.DWG_SYNCTYPE].Value = tmpSrvInfo.SyncType;
                        newRow.Cells[SysParam.DWG_INTERVAL].Value = tmpSrvInfo.Interval;


                    }
                }
                catch (Exception ex)
                {
                    this.fmsLog.Error(ex.Message);
                    this.fmsLog.Error(ex.StackTrace);
                }
                finally
                {
                    if (newRow != null)
                        newRow = null;
                    GC.Collect();
                }
            }
            this.dgwServer.CurrentCell = null;
            this.tsslbAPServerIP.Text = FileUtility.GetIPAddress();
            this.tsslProcessID.Text = FileUtility.GetProcessID();
            this.tsslStartTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private void RefreshSrvMonitorInfoView(SrvInfo vSrvInfo)
        {
            int iRowIndex = 0;
            if (vSrvInfo != null)
            {
                foreach (DataGridViewRow tempRow in this.dgwServer.Rows)
                {
                    iRowIndex = this.dgwServer.Rows.IndexOf(tempRow);

                    if (vSrvInfo.ServiceCategory.Equals(tempRow.Cells[SysParam.DWG_SERVICE_CATEGORY].Value.ToString()) &&
                        vSrvInfo.SDB.Equals(tempRow.Cells[SysParam.DWG_S_DATABASE].Value.ToString()) &&
                        vSrvInfo.IDB.Equals(tempRow.Cells[SysParam.DWG_I_DATABASE].Value.ToString()) &&
                        vSrvInfo.MDB.Equals(tempRow.Cells[SysParam.DWG_M_DATABASE].Value.ToString()) &&
                        vSrvInfo.Operation.Equals(tempRow.Cells[SysParam.DWG_OPERATION].Value.ToString()) &&
                        tempRow.Cells[SysParam.DWG_SERVICE_STATUS].Value.ToString().ToUpper().Equals("STOP"))
                    {
                        this.dgwServer.Rows[iRowIndex].Cells[SysParam.DWG_SERVICE_CATEGORY].Value = vSrvInfo.ServiceCategory;
                        this.dgwServer.Rows[iRowIndex].Cells[SysParam.DWG_S_DATABASE].Value = vSrvInfo.SDB;
                        this.dgwServer.Rows[iRowIndex].Cells[SysParam.DWG_I_DATABASE].Value = vSrvInfo.IDB;
                        this.dgwServer.Rows[iRowIndex].Cells[SysParam.DWG_M_DATABASE].Value = vSrvInfo.MDB;
                        this.dgwServer.Rows[iRowIndex].Cells[SysParam.DWG_OPERATION].Value = vSrvInfo.Operation;
                        this.dgwServer.Rows[iRowIndex].Cells[SysParam.DWG_SYNCTYPE].Value = vSrvInfo.SyncType;
                        this.dgwServer.Rows[iRowIndex].Cells[SysParam.DWG_INTERVAL].Value = vSrvInfo.Interval;

                    }



                }
            }
        }
        //@JC03M start
        private void AddNewSrvMonitorInfo(SrvInfo vSrvInfo)
        {
            DataGridViewRow newRow = null;
            try
            {
                if (vSrvInfo != null)
                {
                    newRow = new DataGridViewRow();
                    newRow.CreateCells(this.dgwServer);// #1 
                    newRow.Cells[0].Value = "Add";// #2 this makes it work
                    dgwServer.Rows.Add(newRow);//#3 need this before adding data

                    newRow.Cells[SysParam.DWG_SERVICE_CATEGORY].Value = vSrvInfo.ServiceCategory;
                    vSrvInfo.SrvStatus = "Stop";
                    newRow.Cells[SysParam.DWG_SERVICE_STATUS].Value = vSrvInfo.SrvStatus;
                    newRow.Cells[SysParam.DWG_S_DATABASE].Value = vSrvInfo.SDB;
                    newRow.Cells[SysParam.DWG_I_DATABASE].Value = vSrvInfo.IDB;
                    newRow.Cells[SysParam.DWG_M_DATABASE].Value = vSrvInfo.MDB;
                    newRow.Cells[SysParam.DWG_OPERATION].Value = vSrvInfo.Operation;
                    newRow.Cells[SysParam.DWG_SYNCTYPE].Value = vSrvInfo.SyncType;
                    newRow.Cells[SysParam.DWG_INTERVAL].Value = vSrvInfo.Interval;
                    FMSConfig.AddService(vSrvInfo);



                }
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
            }
            finally
            {
                if (newRow != null)
                    newRow = null;
                GC.Collect();
            }
        }
        //@JC03M end

        private void RemoveSrvMonitorInfo(SrvInfo vSrvInfo, DataGridViewRow dgvRow)
        {
            if (vSrvInfo != null && dgvRow != null)
            {

                if (dgvRow.Cells[SysParam.DWG_SERVICE_CATEGORY].Value.ToString().ToUpper().Equals(vSrvInfo.ServiceCategory.ToUpper()) &&
                    dgvRow.Cells[SysParam.DWG_S_DATABASE].Value.ToString().ToUpper().Equals(vSrvInfo.SDB.ToUpper()) &&
                    dgvRow.Cells[SysParam.DWG_I_DATABASE].Value.ToString().ToUpper().Equals(vSrvInfo.IDB.ToUpper()) &&
                    dgvRow.Cells[SysParam.DWG_M_DATABASE].Value.ToString().ToUpper().Equals(vSrvInfo.MDB.ToUpper()) &&
                    dgvRow.Cells[SysParam.DWG_OPERATION].Value.ToString().ToUpper().Equals(vSrvInfo.Operation.ToUpper()))
                {
                    this.dgwServer.Rows.Remove(dgvRow);
                    FMSConfig.RemoveService(vSrvInfo);
                }


            }
        }
        //@JC06A End
        //@JC03A start
        private void DeleteOneMonitor(SrvInfo vSrvInfo)
        {
            if (vSrvInfo.SrvStatus.ToUpper().Equals("STOP"))
            {
                if (MessageBox.Show("Are you sure to delete this service?", "FMS Delete Service Warning",
                           MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    RemoveSrvMonitorInfo(vSrvInfo, this.dgwServer.CurrentRow);
                    this.cfgReader.DeleteServerInfo(vSrvInfo);
                }
                else
                {
                    MessageBox.Show("Service is RUNNING, please STOP it first.", "FMS Delete Service Error.",
                             MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ModifySelectSrvMonitor(object sender, EventArgs e)
        {
            SrvInfo tempSrvInfo = null;
            fmDLConfig fmNewMontor = null;
            List<SrvInfo> serviceInfos;
            serviceInfos = FMSConfig.GetServiceInfos();
            try
            {
                if (this.dgwServer.CurrentRow != null)
                {
                    FMSConfig.SetServiceInfos(this.cfgReader.GetServiceInfos());//FMSConfigReader 取得每一個對應 Server 的設定值//@JC03A
                    FMSConfig.UpdServiceInfo(serviceInfos);

                    //@JC03A start
                    tempSrvInfo = FMSConfig.GetServiceInfo(this.dgwServer.CurrentRow.Cells[SysParam.DWG_SERVICE_CATEGORY].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.S_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.I_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.M_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.OPERATION].Value.ToString());


                    if (tempSrvInfo.SrvStatus.ToUpper().Equals("STOP"))
                    {
                        fmNewMontor = new fmDLConfig(tempSrvInfo);
                        fmNewMontor.ShowDialog();
                        //read config file.
                        //reload server config.
                        this.ReLoadConfigSrvInfo(FMSConfig.GetServiceInfos(), this.cfgReader.GetServiceInfos());
                    }
                    else
                    {
                        MessageBox.Show("Monitor server [" + tempSrvInfo.FileLocation + "] is running, can't modify");
                    }
                }
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (tempSrvInfo != null)
                    tempSrvInfo = null;
                GC.Collect();
            }
        }

        private List<SrvInfo> GetSelectedSrvInfo()
        {
            List<SrvInfo> result;
            SrvInfo tempSrvInfo = null;
            int iSelectedCnt = 0;
            result = new List<SrvInfo>();
            iSelectedCnt = this.dgwServer.SelectedRows.Count;
            if (iSelectedCnt > 0)
            {
                for (int i = 0; i < iSelectedCnt; i++)
                {
                    //tempSrvInfo = this.ReadDGVSrvInfo(this.dgwServer.SelectedRows[i]);//@JC03D
                    //@JC03A start
                    tempSrvInfo = FMSConfig.GetServiceInfo(this.dgwServer.CurrentRow.Cells[SysParam.DWG_SERVICE_CATEGORY].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.S_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.I_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.M_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.OPERATION].Value.ToString());

                    if (tempSrvInfo != null)
                        result.Add(tempSrvInfo);
                }
            }

            return result;
        }
        /*
        private List<SrvInfo> GetAllSrvInfo()
        {
            List<SrvInfo> result;
            SrvInfo tempSrvInfo = null;
            result = new List<SrvInfo>();
            foreach (DataGridViewRow tempDGVRow in this.dgwServer.Rows)
            {
                tempSrvInfo = ReadDGVSrvInfo(tempDGVRow);
                if (tempSrvInfo != null)
                {
                    result.Add(tempSrvInfo);
                }
            }
            return result;
        }*/

        private void StartOneSrvMonitor(SrvInfo vSrvInfo)
        {
            StartOneSrvMonitorByFile(vSrvInfo);
        }

        private void StartOneSrvMonitorByFile(SrvInfo vSrvInfo)
        {
            FileMonitorBuilder srvBuilder = null;
            ExecutionResult execRes = null;
            List<SrvInfo> serviceInfos;
            serviceInfos = FMSConfig.GetServiceInfos();
            try
            {
                execRes = new ExecutionResult();
                srvBuilder = new FileMonitorBuilder();

                //if (mFileServiceMonitor == null)//@JC04D
                //    mFileServiceMonitor = new Hashtable();//@JC04D
                if (vSrvInfo.SrvStatus.ToUpper().Equals("STOP"))
                {

                    if (mFileServiceMonitor != null &&
                        mFileServiceMonitor.ContainsKey(vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation) &&
                        mFileServiceMonitor[vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation] != null)//@JC04M
                    {
                        ((FileMonitor)mFileServiceMonitor[vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation]).Dispose();
                    }


                    execRes = srvBuilder.Execute(vSrvInfo);//start selected server
                    if (execRes.Status)
                    {
                        //this.ChangeServerStatus(vSrvInfo.NetDiskRootPath, vSrvInfo.FileFilter, execRes.Status);//@JC03D
                        this.ChangeServerStatus(vSrvInfo, execRes.Status);//@JC03A
                        if (mFileServiceMonitor != null &&
                            mFileServiceMonitor.ContainsKey(vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation))//@JC04M
                        {
                            mFileServiceMonitor[vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation] = execRes.Anything;//@JC04M
                            execRes.Message = "Thread [" + vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation + "] restart success.";//@JC04M
                        }
                        else
                        {
                            mFileServiceMonitor.Add(vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation, execRes.Anything);//@JC04M
                            execRes.Message = "Thread [" + vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation + "] start success.";////@JC04M
                        }
                        if (fmsLog.IsInfoEnabled)
                            fmsLog.Info(MethodBase.GetCurrentMethod().Name + execRes.Message);

                        //MessageBox.Show(vSrvInfo.Operation + " Service Started Success.");
                    }
                    else
                    {
                        if (fmsLog.IsInfoEnabled)
                            fmsLog.Info(MethodBase.GetCurrentMethod().Name + execRes.Anything.ToString());
                        MessageBox.Show(execRes.Message, "Start Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show(vSrvInfo.Operation + ": Service status is not STOP, can't be start.");
                }
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (srvBuilder != null)
                    srvBuilder = null;
                if (execRes != null)
                    execRes = null;

                GC.Collect();
            }

        }

        private void PauseOneSrvMonitor(SrvInfo vSrvInfo)
        {
            PauseOneSrvMonitorByFile(vSrvInfo);

        }

        private void PauseOneSrvMonitorByFile(SrvInfo vSrvInfo)
        {

            FileMonitorBuilder srvBuilder = null;
            ExecutionResult execRes = null;
            try
            {
                execRes = new ExecutionResult();
                srvBuilder = new FileMonitorBuilder();
                if (vSrvInfo.SrvStatus.ToUpper().Equals("RUNNING"))
                {
                    if (mFileServiceMonitor != null &&
                         mFileServiceMonitor.ContainsKey(vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation) &&
                         mFileServiceMonitor[vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation] != null)//@JC04M
                    {

                        ((FileMonitor)mFileServiceMonitor[vSrvInfo.ServiceCategory + vSrvInfo.SDB + vSrvInfo.IDB + vSrvInfo.MDB + vSrvInfo.Operation]).Stop();

                        this.ChangeServerStatus(vSrvInfo, false);//@JC03A
                        if (fmsLog.IsInfoEnabled)
                            fmsLog.Info(MethodBase.GetCurrentMethod().Name + "Thread [" + vSrvInfo.Operation + "] stop success.");
                    }
                    //MessageBox.Show(vSrvInfo.Operation + " Service Stopped Success.");
                }
                else
                {
                    MessageBox.Show(vSrvInfo.NetDiskIP + ": server is not RUNNING. Not need STOP.", "Information...",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (srvBuilder != null)
                    srvBuilder = null;
                if (execRes != null)
                    execRes = null;

                GC.Collect();
            }

        }

        private void ReLoadConfigSrvInfo(List<SrvInfo> lstSrvInfoDgv, List<SrvInfo> lstSrvInfoCfg)
        {
            bool bAddFlag;
            try
            {
                foreach (SrvInfo tempSrvInfoCfg in lstSrvInfoCfg)
                {
                    bAddFlag = true;
                    foreach (SrvInfo tempSrvInfoDGV in lstSrvInfoDgv)
                    {

                        if (tempSrvInfoCfg.ServiceCategory.ToLower().Equals(tempSrvInfoDGV.ServiceCategory.ToLower()) &&
                            tempSrvInfoCfg.SDB.ToLower().Equals(tempSrvInfoDGV.SDB.ToLower()) &&
                            tempSrvInfoCfg.IDB.ToLower().Equals(tempSrvInfoDGV.IDB.ToLower()) &&
                            tempSrvInfoCfg.MDB.ToLower().Equals(tempSrvInfoDGV.MDB.ToLower()) &&
                            tempSrvInfoCfg.Operation.ToLower().Equals(tempSrvInfoDGV.Operation.ToLower()))
                        {
                            this.RefreshSrvMonitorInfoView(tempSrvInfoCfg);
                            bAddFlag = false;
                            break;//找到相同的,更新後,跳出迴圈, 而且不要新增
                        }

                    }
                    // 找不到有相同的, 就要新增
                    if (bAddFlag)
                        this.AddNewSrvMonitorInfo(tempSrvInfoCfg);
                }
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
                throw ex;
            }
        }

        private bool CheckSrvMonitorStatus()
        {
            List<SrvInfo> lstSrvInfo = null;
            bool result = false;
            try
            {
                lstSrvInfo = new List<SrvInfo>();
                if (lstSrvInfo != null && lstSrvInfo.Count > 0)
                {
                    foreach (SrvInfo tempSrvInfo in lstSrvInfo)
                    {
                        if (tempSrvInfo.SrvStatus.ToUpper().Equals("RUNNING"))
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
                MessageBox.Show("check server monitor status exception. Msg:" + ex.Message);
            }
            finally
            {
                if (lstSrvInfo != null)
                    lstSrvInfo = null;
                GC.Collect();
            }
            return result;
        }

        //@JC03A start
        private void ChangeServerStatus(SrvInfo vSrvInfo, bool status)
        {
            string tmpServiceCategory;
            string tmpSDB;
            string tmpIDB;
            string tmpMDB;
            string tmpOperation;
            bool modifyflag;

            if (status)
            {
                FMSConfig.ChangeStatus(vSrvInfo, "Running");
            }
            else
            {
                FMSConfig.ChangeStatus(vSrvInfo, "Stop");
            }

            foreach (DataGridViewRow tempRow in this.dgwServer.Rows)
            {
                modifyflag = false;

                tmpServiceCategory = tempRow.Cells[SysParam.DWG_SERVICE_CATEGORY].Value.ToString();
                tmpSDB = tempRow.Cells[SysParam.DWG_S_DATABASE].Value.ToString();
                tmpIDB = tempRow.Cells[SysParam.DWG_I_DATABASE].Value.ToString();
                tmpMDB = tempRow.Cells[SysParam.DWG_M_DATABASE].Value.ToString();
                tmpOperation = tempRow.Cells[SysParam.DWG_OPERATION].Value.ToString();

                if (tmpServiceCategory.ToUpper().Equals(vSrvInfo.ServiceCategory.ToUpper()) &&
                    tmpSDB.ToUpper().Equals(vSrvInfo.SDB.ToUpper()) &&
                    tmpIDB.ToUpper().Equals(vSrvInfo.IDB.ToUpper()) &&
                    tmpMDB.ToUpper().Equals(vSrvInfo.MDB.ToUpper()) &&
                    tmpOperation.ToUpper().Equals(vSrvInfo.Operation.ToUpper()))
                    modifyflag = true;


                if (status && modifyflag)
                {
                    this.dgwServer.Rows[this.dgwServer.Rows.IndexOf(tempRow)].Cells["SrvStatus"].Value = "Running";
                    this.dgwServer.Rows[this.dgwServer.Rows.IndexOf(tempRow)].DefaultCellStyle.BackColor = Color.YellowGreen;
                }
                else if (!status && modifyflag)
                {
                    this.dgwServer.Rows[this.dgwServer.Rows.IndexOf(tempRow)].Cells["SrvStatus"].Value = "Stop";
                    this.dgwServer.Rows[this.dgwServer.Rows.IndexOf(tempRow)].DefaultCellStyle.BackColor = Color.WhiteSmoke;
                }
            }
        }
        //@JC03A end

        //@JC03A start
        private void SyncActionButtonStatus()
        {
            bool runningAll;
            bool runningOne;
            int runningCout;
            List<SrvInfo> serviceInfos;
            serviceInfos = FMSConfig.GetServiceInfos();

            runningCout = 0;
            foreach (SrvInfo tmpSrvInfo in serviceInfos)
            {
                if (tmpSrvInfo.SrvStatus.ToUpper().Equals("RUNNING"))
                {
                    runningCout = runningCout + 1;
                }
            }
            if (runningCout == 0 && runningCout == serviceInfos.Count)
            {
                runningAll = false;
                runningOne = false;
            }
            else if (runningCout > 0 && runningCout == serviceInfos.Count)
            {
                runningAll = true;
                runningOne = true;
            }
            else if (runningCout > 0 && runningCout < serviceInfos.Count)
            {
                runningAll = false;
                runningOne = true;
            }
            else
            {
                runningAll = false;
                runningOne = false;
            }

            if (runningAll)
            {
                this.tsActions.Enabled = true;
                this.tsbtnDeleteAll.Enabled = false;
                this.tsbtnDeleteOne.Enabled = false;
                this.tsbtnStartOne.Enabled = false;
                this.tsbtnStartAll.Enabled = false;
                this.tsbtnPauseAll.Enabled = true;
                this.tsbtnPauseOne.Enabled = true;
            }
            else if (!runningAll && !runningOne && serviceInfos.Count > 0)
            {
                this.tsActions.Enabled = true;
                this.tsbtnDeleteAll.Enabled = true;
                this.tsbtnDeleteOne.Enabled = true;
                this.tsbtnStartOne.Enabled = true;
                this.tsbtnStartAll.Enabled = true;
                this.tsbtnPauseAll.Enabled = false;
                this.tsbtnPauseOne.Enabled = false;
            }
            else if (!runningAll && !runningOne && serviceInfos.Count == 0)
            {
                this.tsActions.Enabled = true;
                this.tsbtnDeleteAll.Enabled = false;
                this.tsbtnDeleteOne.Enabled = false;
                this.tsbtnStartOne.Enabled = false;
                this.tsbtnStartAll.Enabled = false;
                this.tsbtnPauseAll.Enabled = false;
                this.tsbtnPauseOne.Enabled = false;
            }
            else if (!runningAll && runningOne)
            {
                this.tsActions.Enabled = true;
                this.tsbtnDeleteAll.Enabled = true;
                this.tsbtnDeleteOne.Enabled = true;
                this.tsbtnStartOne.Enabled = true;
                this.tsbtnStartAll.Enabled = true;
                this.tsbtnPauseAll.Enabled = true;
                this.tsbtnPauseOne.Enabled = true;
            }
        }
        //@JC03A end

        private void tsbtnNew_Click(object sender, EventArgs e)
        {
            List<SrvInfo> lstSrvInfoCfg = null;
            List<SrvInfo> lstSrvInfoDgv = null;
            fmDLConfig fmNewMonitor = null;
            try
            {
                fmNewMonitor = new fmDLConfig();
                fmNewMonitor.ShowDialog();
                //refresh config
                lstSrvInfoCfg = this.cfgReader.GetServiceInfos();
                //reload config server info.
                this.ReLoadConfigSrvInfo(FMSConfig.GetServiceInfos(), lstSrvInfoCfg);//@JC03M
                this.SyncActionButtonStatus();
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (lstSrvInfoCfg != null)
                    lstSrvInfoCfg = null;
                if (lstSrvInfoDgv != null)
                    lstSrvInfoDgv = null;
                if (fmNewMonitor != null)
                    fmNewMonitor = null;

                GC.Collect();
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (this.dgwServer.SelectedRows.Count == 1)
            {
                this.ModifySelectSrvMonitor(sender, e);
            }
            else
            {
                MessageBox.Show("Please Selected One Server.", "FMS System Alarm Info.");
            }
        }

        private void tsbtnStartOne_Click(object sender, EventArgs e)
        {
            List<SrvInfo> serviceInfos;
            serviceInfos = FMSConfig.GetServiceInfos();
            FMSConfig.SetServiceInfos(this.cfgReader.GetServiceInfos());//FMSConfigReader 取得每一個對應 Server 的設定值//@JC03A
            FMSConfig.UpdServiceInfo(serviceInfos);

            SrvInfo selectedServiceInfo;//@JC03A
            try
            {
                //@JC03A start
                if (this.dgwServer.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select one of services to STOP.", "FMS stop service alarm ...",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                else if (this.dgwServer.SelectedRows.Count == 1)
                {


                    selectedServiceInfo = null;
                    selectedServiceInfo = FMSConfig.GetServiceInfo(this.dgwServer.CurrentRow.Cells[SysParam.DWG_SERVICE_CATEGORY].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.S_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.I_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.M_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.OPERATION].Value.ToString());

                    if (selectedServiceInfo != null)
                    {
                        this.StartOneSrvMonitor(selectedServiceInfo);
                        this.SyncActionButtonStatus();
                    }
                }
                else
                {
                    MessageBox.Show("You can only start one monitor. Please select one of services.", "FMS start service alarm ...",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //@JC03A end
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
                MessageBox.Show("Start one monitor thread exception. Msg:" + ex.Message);
            }
        }

        private void tsbtnStartAll_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(" Service Started Successfully.");
            DialogResult dialogResult = MessageBox.Show("Are you Sure you want to start services", "APE FMS", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                List<SrvInfo> serviceInfos;
                serviceInfos = FMSConfig.GetServiceInfos();
                FMSConfig.SetServiceInfos(this.cfgReader.GetServiceInfos());//FMSConfigReader 取得每一個對應 Server 的設定值//@JC03A
                FMSConfig.UpdServiceInfo(serviceInfos);

                List<SrvInfo> lstSrvInfo = null;
                try
                {
                    if (this.dgwServer.Rows.Count > 0)
                    {

                        lstSrvInfo = FMSConfig.GetServiceInfos();//@JC03A
                        if (lstSrvInfo != null &&
                            lstSrvInfo.Count > 0)
                        {
                            foreach (SrvInfo tempSrvInfo in lstSrvInfo)
                            {
                                if (tempSrvInfo.SrvStatus.ToUpper().Equals("STOP"))
                                    this.StartOneSrvMonitor(tempSrvInfo);
                            }
                            this.SyncActionButtonStatus();//@JC03A
                        }
                    }
                    else
                    {
                        MessageBox.Show("No configuration service, please add your server first", "No server information",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);//@JC03M
                    }
                }
                catch (Exception ex)
                {
                    this.fmsLog.Error(ex.Message);
                    this.fmsLog.Error(ex.StackTrace);
                    MessageBox.Show("Start All monitor server exception. Msg:" + ex.Message);
                }
                finally
                {
                    if (lstSrvInfo != null)
                        lstSrvInfo = null;
                    GC.Collect();
                }
            }
            else if (dialogResult == DialogResult.No)
            {
                //do something else
            }

        }

        private void tsbtnPauseOne_Click(object sender, EventArgs e)
        {
            SrvInfo selectedServiceInfo;//@JC03A
            try
            {
                //@JC03A start
                if (this.dgwServer.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select one of services to START.", "FMS start service alarm ...",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                else if (this.dgwServer.SelectedRows.Count == 1)
                {
                    //@JC03A start
                    selectedServiceInfo = null;
                    selectedServiceInfo = FMSConfig.GetServiceInfo(this.dgwServer.CurrentRow.Cells[SysParam.DWG_SERVICE_CATEGORY].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.S_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.I_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.M_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.OPERATION].Value.ToString());

                    //@JC03A end    
                    if (selectedServiceInfo != null)
                    {
                        this.PauseOneSrvMonitor(selectedServiceInfo);
                        this.SyncActionButtonStatus();
                    }
                }
                else
                {
                    MessageBox.Show("You can only stop one monitor. Please select one of services.", "FMS stop service alarm ...",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //@JC03A end
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
                MessageBox.Show("Stop monitor server exception. Msg:" + ex.Message, "Stop error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                GC.Collect();
            }
        }

        private void tsbtnPauseAll_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you Sure you want to Pause All services", "APE FMS", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                List<SrvInfo> lstSrvInfo = null;
                try
                {
                    if (this.dgwServer.Rows.Count > 0)
                    {
                        lstSrvInfo = new List<SrvInfo>();
                        lstSrvInfo = FMSConfig.GetServiceInfos();//@JC03A
                        if (lstSrvInfo != null &&
                            lstSrvInfo.Count > 0)
                        {
                            foreach (SrvInfo tempSrvInfo in lstSrvInfo)
                            {
                                if (tempSrvInfo.SrvStatus.ToUpper().Equals("RUNNING"))
                                    this.PauseOneSrvMonitor(tempSrvInfo);
                            }
                            this.SyncActionButtonStatus();//@JC03A
                        }
                    }
                    else
                    {
                        MessageBox.Show("No config service to STOP, please add your service first", "No service information",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    this.fmsLog.Error(ex.Message);
                    this.fmsLog.Error(ex.StackTrace);
                    MessageBox.Show("STOP All monitor server exception. Msg:" + ex.Message);
                }
                finally
                {
                    if (lstSrvInfo != null)
                        lstSrvInfo = null;
                    GC.Collect();
                }
            }
            else
            {

            }

        }

        private void tsbtnDeleteOne_Click(object sender, EventArgs e)
        {
            SrvInfo selectedServiceInfo;
            //selectedServiceInfo = this.ReadDGVSrvInfo(this.dgwServer.CurrentRow);//@JC03A
            //@JC03A start
            selectedServiceInfo = null;
            selectedServiceInfo = FMSConfig.GetServiceInfo(this.dgwServer.CurrentRow.Cells[SysParam.DWG_SERVICE_CATEGORY].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.S_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.I_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.M_DATABASE].Value.ToString() + this.dgwServer.CurrentRow.Cells[SysParam.OPERATION].Value.ToString());

            if (selectedServiceInfo != null)
            {
                this.DeleteOneMonitor(selectedServiceInfo);//@JC03A
                this.SyncActionButtonStatus();//@JC03A
            }
        }

        private void tsbtnDeleteAll_Click(object sender, EventArgs e)
        {
            List<SrvInfo> lstSrvInfo = null;
            try
            {
                lstSrvInfo = FMSConfig.GetServiceInfos();//@JC03A
                if (lstSrvInfo != null && lstSrvInfo.Count > 0)
                {
                    if (!FMSConfig.CheckServiceRunning())//@JC03M
                    {
                        //delete config info.
                        if (MessageBox.Show("Are you sure to delete All services?", "Delete Warning",
                           MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                        {
                            this.cfgReader.DeleteAllServerInfo();//@JC02M
                            foreach (SrvInfo tmpSrvInfo in lstSrvInfo)
                            {
                                foreach (DataGridViewRow tempRow in this.dgwServer.Rows)
                                {

                                    if ((tempRow.Cells[SysParam.DWG_S_DATABASE].Value.ToString().Equals(tmpSrvInfo.SDB)) && (tempRow.Cells[SysParam.DWG_OPERATION].Value.ToString().Equals(tmpSrvInfo.Operation)))
                                        this.RemoveSrvMonitorInfo(tmpSrvInfo, tempRow);

                                }

                            }
                            this.SyncActionButtonStatus();//@JC03A
                        }
                    }
                    else
                    {
                        MessageBox.Show("Some service is still in running status, please stop all service first.", "FMS delete service warning",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No service to delete.");
                }
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
                MessageBox.Show("Delete monitor server exception. Msg:" + ex.Message);
            }
            finally
            {
                if (lstSrvInfo != null)
                    lstSrvInfo = null;
                GC.Collect();
            }
        }

        private void tsbtnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                this.ReLoadConfigSrvInfo(FMSConfig.GetServiceInfos(), this.cfgReader.GetServiceInfos());
                MessageBox.Show("Service configuration reload successfully.");
                this.SyncActionButtonStatus();
            }
            catch (Exception ex)
            {
                this.fmsLog.Error(ex.Message);
                this.fmsLog.Error(ex.StackTrace);
                MessageBox.Show("Reload service configuration exception. Msg:" + ex.Message, "Error.",
                     MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                GC.Collect();
            }
        }

        private void dgwServer_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            this.ModifySelectSrvMonitor(sender, e);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.dgwServer.Rows.Count > 0)
            {
                if (this.dgwServer.SelectedRows.Count == 1)
                {
                    this.tsbtnDeleteOne_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("You can only remove one stop monitor server.", "Modify Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                }
            }
            else
            {
                MessageBox.Show("No monitor server config.", "Modify Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.tsbtnDeleteAll_Click(sender, e);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tsbtnNew_Click(sender, e);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fmAboutInfo fmAbout = null;
            try
            {
                fmAbout = new fmAboutInfo();
                fmAbout.ShowDialog();
            }
            finally
            {
                if (fmAbout != null)
                    fmAbout = null;

                GC.Collect();
            }
        }

        private void connectionConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fmDBConfig fmDBConfig = null;
            if (!FMSConfig.CheckServiceRunning())//@JC01M
            {
                try
                {
                    fmDBConfig = new fmDBConfig();
                    fmDBConfig.ShowDialog();
                }
                catch (Exception ex)
                {
                    this.fmsLog.Error(ex.Message);
                    this.fmsLog.Error(ex.StackTrace);
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (fmDBConfig != null)
                        fmDBConfig = null;

                    GC.Collect();
                }
            }
            else
            {
                MessageBox.Show("Can't config database connection. please stop all server monitor first.", "Closed Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}