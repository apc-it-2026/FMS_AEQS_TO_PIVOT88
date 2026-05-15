
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Compal.FMS.Kernel.Utils;
using Compal.FMS.Kernel.Beans;
using System.IO;
using System.Reflection;
using Compal.FMS.Kernel.Threading;
using Compal.FMS.Compal.FMS.Operations;
using FMSCommon.Compal.FMS.Kernel.Utils;

namespace Compal.FMS.UI
{
    public partial class fmDLConfig : Form
    {
        private FMSConfigReader cfgReader;
        private NetDiskConnected netConn;
        private SrvInfo tempSrvInfo = null;

        public fmDLConfig()
        {
            InitializeComponent();
            netConn = new NetDiskConnected();
            this.cfgReader = new FMSConfigReader();
            this.cfgReader.InitServiceConfig(Application.StartupPath + "\\" + SysParam.SRV_FILE);
            //add config server info.
            this.SyncConfigToTreeView(this.cfgReader.GetServiceInfos());
            this.IntialDefaultSetting();
        }

        public fmDLConfig(SrvInfo vSrvInfo)
        {
            InitializeComponent();
            tempSrvInfo = vSrvInfo;
            cfgReader = new FMSConfigReader();
            this.cfgReader.InitServiceConfig(Application.StartupPath + "\\" + SysParam.SRV_FILE);
            //add config server info tree view.
            this.SyncConfigToTreeView(this.cfgReader.GetServiceInfos());
            //show the defined server.
            this.ShowDefinedSrvInfo(tempSrvInfo);//@JC04M
            this.cmbServiceCategory.Focus();
        }

        private void IntialDefaultSetting()
        {
            this.cmbSyncType.Text = this.cmbSyncType.Items[0].ToString();
        }


        //@JC02A start
        private bool CheckInputNumberic(string checkType, string checkValue)
        {
            bool result = false;
            Regex regex = null;
            try
            {
                //check Numberic match.
                regex = new Regex("^(-?[0-9]*[.]*[0-9]{0,3})$");
                if (!String.IsNullOrEmpty(checkValue))
                {
                    if (!regex.IsMatch(checkValue))
                    {
                        MessageBox.Show(checkType + " must be a number.", "Setting Error.",
                                                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        result = true;
                    }
                }
                else
                {
                    MessageBox.Show(checkType + " can not be null.", "Setting Error.",
                                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (regex != null)
                    regex = null;

                GC.Collect();
            }
            return result;
        }

        private bool CheckInputNull(string checkType, string checkValue)
        {
            bool result = false;
            if (String.IsNullOrEmpty(checkValue))
            {
                MessageBox.Show(checkType + " can not be null.", "Setting Error.",
                                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = true;
            }
            return result;
        }

        //@JC02A end


        private bool CheckTreeNodeDuplicate(SrvInfo vSrvInfo)
        {
            bool result;
            string key;
            result = false;
            foreach (TreeNode tmpTreeNode in this.tvSrvList.Nodes)
            {
                key = "";
                if (vSrvInfo.ServiceType.Equals(SysParam.FMS_SERVICE_FILE))
                    key = vSrvInfo.ServiceType + vSrvInfo.FileLocation;

                //strKey = tempSrvInfo.NetDiskIP + tempSrvInfo.FileLocation + tempSrvInfo.FileFilter + tempSrvInfo.LineName;
                if (tmpTreeNode.Name.Equals(key))
                {
                    result = true;
                    break;
                }

            }
            return result;
        }

        private void AddSrvInfoInTreeView(SrvInfo vSrvInfo)
        {
            TreeNode tempNode = null;
            TreeNode tempChildNote = null;
            bool bDuplicate;
            try
            {
                if (vSrvInfo != null)
                {
                    bDuplicate = false;
                    if (this.tvSrvList.Nodes.Count > 0 && CheckTreeNodeDuplicate(vSrvInfo))
                    {
                        bDuplicate = true;
                    }
                    if (!bDuplicate)
                    {
                        tempNode = new TreeNode();

                        tempNode.Name = vSrvInfo.Operation;
                        tempNode.Text = vSrvInfo.Operation;
                        //Add SWDL monitor base info.

                        tempChildNote = new TreeNode("ServiceCategory= " + vSrvInfo.ServiceCategory);
                        tempNode.Nodes.Add(tempChildNote);
                        tempChildNote = new TreeNode("Source DB= " + vSrvInfo.SDB);
                        tempNode.Nodes.Add(tempChildNote);
                        tempChildNote = new TreeNode("Intermediate DB= " + vSrvInfo.IDB);
                        tempNode.Nodes.Add(tempChildNote);
                        tempChildNote = new TreeNode("MES DB= " + vSrvInfo.MDB);
                        tempNode.Nodes.Add(tempChildNote);
                        tempChildNote = new TreeNode("Operation= " + vSrvInfo.Operation);
                        tempNode.Nodes.Add(tempChildNote);
                        tempChildNote = new TreeNode("SyncType= " + vSrvInfo.SyncType);
                        tempNode.Nodes.Add(tempChildNote);
                        tempChildNote = new TreeNode("Interval= " + vSrvInfo.Interval);
                        tempNode.Nodes.Add(tempChildNote);

                        //Add tree node to tree view.
                        this.tvSrvList.Nodes.Add(tempNode);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Add Server Info to TreeView exception. Msg:" + ex.Message);
            }
            finally
            {
                if (tempNode != null)
                    tempNode = null;

                GC.Collect();
            }

        }

        private void SyncConfigToTreeView(List<SrvInfo> vlstSrvInfo)
        {
            //List<SrvInfo> lstSrvInfo = null;
            try
            {
                this.tvSrvList.Nodes.Clear();
                //lstSrvInfo = this.cfgReader.GetServiceInfos();
                if (vlstSrvInfo != null && vlstSrvInfo.Count > 0)
                {
                    foreach (SrvInfo tempSrvInfo in vlstSrvInfo)
                    {
                        this.AddSrvInfoInTreeView(tempSrvInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (vlstSrvInfo != null)
                    vlstSrvInfo = null;

                GC.Collect();
            }
        }



        private void btnSave_Click(object sender, EventArgs e)
        {
            SrvInfo tempSrvInfo = null;
            bool bSaveYN = true;
            try
            {
                //if (this.cmbAlertMail.Text.Equals("Y"))
                //{
                //    if (this.CheckInputNumberic("Alert Count", this.txbAlertCount.Text.Trim()))
                //        bSaveYN = false;
                //}

                if (bSaveYN)
                {

                    if (
                        !this.CheckInputNull("ProcessType", this.cmbServiceCategory.Text.Trim()) &&
                        !this.CheckInputNull("Source Database", this.cmbSDB.Text.Trim()) &&
                        !this.CheckInputNull("Operation Table", this.cmbOperations.Text.Trim()) &&
                        !this.CheckInputNull("Operation Table", this.cmbSyncType.Text.Trim()) &&
                        !this.CheckInputNull("Interval", this.txbInterval.Text.Trim())
                        )
                    {//@JC02M
                        tempSrvInfo = new SrvInfo();

                        tempSrvInfo.ServiceCategory = this.cmbServiceCategory.Text.Trim();
                        tempSrvInfo.SDB = this.cmbSDB.Text.Trim();
                        tempSrvInfo.IDB = this.cmbIDB.Text.Trim();
                        tempSrvInfo.MDB = this.cmbMDB.Text.Trim();
                        tempSrvInfo.Operation = this.cmbOperations.Text.Trim();
                        tempSrvInfo.SyncType = this.cmbSyncType.Text.Trim();
                        tempSrvInfo.Interval = this.txbInterval.Text.Trim();

                        if (!this.cfgReader.CheckSrvInfoExistInList(tempSrvInfo, this.cfgReader.GetServiceInfos()))
                        {
                            this.cfgReader.AddServiceNodeInfo(tempSrvInfo);
                            this.SyncConfigToTreeView(this.cfgReader.GetServiceInfos());
                            MessageBox.Show("Add new service configuration successfully.", "Add FMS service",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            this.cfgReader.ModifiedServerInfo(tempSrvInfo, this.cfgReader.GetServiceInfos());
                            this.SyncConfigToTreeView(this.cfgReader.GetServiceInfos());
                            MessageBox.Show("Modify service configuration successfully.", "Modify FMS service",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (tempSrvInfo != null)
                    tempSrvInfo = null;

                GC.Collect();
            }
        }



        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void ShowDefinedSrvInfo(SrvInfo vSrvInfo)//@JC04M
        {
            if (tempSrvInfo != null)
            {
                this.cmbServiceCategory.Text = vSrvInfo.ServiceCategory;
                this.cmbSDB.Text = vSrvInfo.SDB;
                this.cmbIDB.Text = vSrvInfo.IDB;
                this.cmbMDB.Text = vSrvInfo.MDB;
                this.cmbOperations.Text = vSrvInfo.Operation;
                this.cmbSyncType.Text = vSrvInfo.SyncType;
                this.txbInterval.Text = vSrvInfo.Interval;
            }
        }

        private void tvSrvList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode tempNode;
            tempNode = ((TreeView)sender).SelectedNode;

            if (tempNode.Parent == null)
            {
                //this.txbServerIP.Text = this.GetTreeNodeSettingValue(tempNode.Text, '=');
                foreach (TreeNode tempSubNode in tempNode.Nodes)
                {

                    if (tempSubNode.Text.ToUpper().StartsWith("SERVICECATEGORY"))//@JC04M
                    {
                        this.cmbServiceCategory.Text = this.GetTreeNodeSettingValue(tempSubNode.Text, '=');
                    }
                    else if (tempSubNode.Text.ToUpper().StartsWith("SOURCE DB"))//@JC04M
                    {
                        this.cmbSDB.Text = this.GetTreeNodeSettingValue(tempSubNode.Text, '=');
                    }
                    else if (tempSubNode.Text.ToUpper().StartsWith("INTERMEDIATE DB"))//@JC04M
                    {
                        this.cmbIDB.Text = this.GetTreeNodeSettingValue(tempSubNode.Text, '=');
                    }
                    else if (tempSubNode.Text.ToUpper().StartsWith("MES DB"))//@JC04M
                    {
                        this.cmbMDB.Text = this.GetTreeNodeSettingValue(tempSubNode.Text, '=');
                    }
                    else if (tempSubNode.Text.ToUpper().StartsWith("OPERATION"))//@JC04M
                    {
                        this.cmbOperations.Text = this.GetTreeNodeSettingValue(tempSubNode.Text, '=');
                    }
                    else if (tempSubNode.Text.ToUpper().StartsWith("SYNCTYPE"))
                    {
                        this.cmbSyncType.Text = this.GetTreeNodeSettingValue(tempSubNode.Text, '=');
                    }
                    else if (tempSubNode.Text.ToUpper().StartsWith("INTERVAL"))
                    {
                        this.txbInterval.Text = this.GetTreeNodeSettingValue(tempSubNode.Text, '=');
                    }

                }
            }
        }

        private string GetTreeNodeSettingValue(string vNodeText, char vSplitChar)
        {
            string result = "";
            string[] strSplitResult = null;
            if (!string.IsNullOrEmpty(vNodeText))
            {
                strSplitResult = vNodeText.Split(new char[] { vSplitChar });
                if (strSplitResult != null && strSplitResult.Length > 1)
                {
                    result = strSplitResult[1].Trim();
                }
            }

            return result;
        }


        //@JC03A start
        private void fmDLConfig_Load(object sender, EventArgs e)
        {
            FMSConfigReader fmsDBConfigReader;
            fmsDBConfigReader = new FMSConfigReader();
            fmsDBConfigReader.InitDatabaseConfig(Application.StartupPath + "\\" + "database.config");
            cmbSDB.Items.Clear();
            cmbIDB.Items.Clear();
            cmbMDB.Items.Clear();
            foreach (string tmp in fmsDBConfigReader.GetDBNames())
            {
                cmbSDB.Items.Add(tmp);
                cmbIDB.Items.Add(tmp);
                cmbMDB.Items.Add(tmp);
            }
            if (cmbSDB.Items.Count > 0 && FMSConfig.GetServiceInfos().Count == 0)//@JC04M
                cmbSDB.Text = cmbSDB.Items[0].ToString();
            if (cmbIDB.Items.Count > 0 && FMSConfig.GetServiceInfos().Count == 0)//@JC04M
                cmbIDB.Text = cmbIDB.Items[0].ToString();
            if (cmbMDB.Items.Count > 0 && FMSConfig.GetServiceInfos().Count == 0)//@JC04M
                cmbMDB.Text = cmbMDB.Items[0].ToString();
        }

        private void BtnManual_Click(object sender, EventArgs e)
        {
            SrvInfo tempSrvInfo = null;
            bool bSaveYN = true;
            try
            {

                if (bSaveYN)
                {
                    if (
                        !this.CheckInputNull("ProcessType", this.cmbServiceCategory.Text.Trim()) &&
                        !this.CheckInputNull("Source Database", this.cmbSDB.Text.Trim()) &&
                        !this.CheckInputNull("Operation Table", this.cmbOperations.Text.Trim())
                        )
                    {//@JC02M
                        tempSrvInfo = new SrvInfo();

                        tempSrvInfo.ServiceCategory = this.cmbServiceCategory.Text.Trim();
                        tempSrvInfo.SDB = this.cmbSDB.Text.Trim();
                        tempSrvInfo.Operation = this.cmbOperations.Text.Trim();

                        txtStartDate.Text = "";
                        Cursor.Current = Cursors.WaitCursor; // change cursor to hourglass type

                        Cls_Return retob = new Cls_Return();

                        Run_AEQS_Pivot88_Operations rAEQS = new Run_AEQS_Pivot88_Operations();

                        if (tempSrvInfo.Operation == "AQL Outbound")
                            retob = rAEQS.PostRequestAsync(tempSrvInfo);



                        Cursor.Current = Cursors.Default;

                        this.txtStartDate.Text = "Status : " + retob.TYPE + " " + Environment.NewLine + "Message : " + retob.MESSAGE;

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (tempSrvInfo != null)
                    tempSrvInfo = null;

                GC.Collect();
            }
        }

        private void cmbSyncType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSyncType.Text == "Daily")
            {
                labMS.Text = "HH:MM";
            }
            else if (cmbSyncType.Text == "Interval")
            {
                labMS.Text = "MM";
            }

        }

        private void cmbServiceCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbOperations.Items.Clear();
            if (cmbServiceCategory.Text == "OA to SAP")
            {
                cmbOperations.Items.Add("Travel");
                cmbOperations.Items.Add("Payable");
                cmbOperations.Items.Add("Advance");
                cmbOperations.Items.Add("Purchase");
                this.cmbOperations.Text = this.cmbOperations.Items[0].ToString();
            }
            else if (cmbServiceCategory.Text == "MES to SAP")
            {
                cmbOperations.Items.Add("M1");
                cmbOperations.Items.Add("M2");
                cmbOperations.Items.Add("M3");
                cmbOperations.Items.Add("M4");
                this.cmbOperations.Text = this.cmbOperations.Items[0].ToString();
            }
            else if (cmbServiceCategory.Text == "WMS to SAP")
            {
                // cmbOperations.Items.Add("W1");
                cmbOperations.Items.Add("MES029PoReceive");
                cmbOperations.Items.Add("MES039ClaimSingle");
                cmbOperations.Items.Add("MES039ReturnPO");
                cmbOperations.Items.Add("MES033outsourcesupplier");
                cmbOperations.Items.Add("MES029outsourcereceive");
                cmbOperations.Items.Add("MES036deptpick");
                cmbOperations.Items.Add("MES036deptpickWH");
                cmbOperations.Items.Add("MES036deptpickret");
                cmbOperations.Items.Add("MES041_32_pomtrlissue");
                cmbOperations.Items.Add("MES041_32_Vampfeeding");
                cmbOperations.Items.Add("MES041prodsample");
                cmbOperations.Items.Add("MES041prodorder");
                cmbOperations.Items.Add("MES037_38_interwh");
                cmbOperations.Items.Add("MES037_38_interwhsap");
                cmbOperations.Items.Add("MES034_35_inventorygain");
                cmbOperations.Items.Add("MES031_prodorder");
                cmbOperations.Items.Add("MES045_poreceive");
                cmbOperations.Items.Add("MES042Outstanding");
                //
                cmbOperations.Items.Add("MES030_WH_Result");
                cmbOperations.Items.Add("MES040Sales_EX_WH");
                cmbOperations.Items.Add("MES047_Delivery_Order");
                cmbOperations.Items.Add("MES044Production_Report");




                this.cmbOperations.Text = this.cmbOperations.Items[0].ToString();
            }
            else if (cmbServiceCategory.Text == "AEQS to Pivot88")
            {
                cmbOperations.Items.Add("AQL Outbound");
                cmbOperations.Items.Add("Inline");
                cmbOperations.Items.Add("EndOfLine");
                cmbOperations.Items.Add("EndOfLine_Rework");
                this.cmbOperations.Text = this.cmbOperations.Items[0].ToString();
            }
            else if (cmbServiceCategory.Text == "AEQS to Middle")
            {
                cmbOperations.Items.Add("TQC_Middle");
                cmbOperations.Items.Add("TQC_Rework_Middle");
                this.cmbOperations.Text = this.cmbOperations.Items[0].ToString();
            }

        }
        //@JC03A end


    }
}