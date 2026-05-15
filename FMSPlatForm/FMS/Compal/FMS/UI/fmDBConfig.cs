using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Oracle.ManagedDataAccess.Client;

namespace Compal.FMS.UI
{
    public partial class fmDBConfig : Form
    {
        XmlDocument mesEnvConfigDoc;
        XmlDocument clientEnvConfigDoc;
        string clientEnvConfigFileName;

        public fmDBConfig()
        {
            InitializeComponent();
        }

        #region Load File
        private void fmConfig_Load(object sender, EventArgs e)
        {
            string filePath;
            filePath = Application.ExecutablePath;
            filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
            clientEnvConfigFileName = filePath + "database.config";
            mesEnvConfigDoc = new XmlDocument();
            clientEnvConfigDoc = new XmlDocument();
            if (File.Exists(clientEnvConfigFileName))
            {
                clientEnvConfigDoc.Load(clientEnvConfigFileName);
                DispDB(true, 0);
            }
            else
            {
                MessageBox.Show("database.config does not exist,PLS check it!!!");
            }
            btDSave.Enabled = false;
            btDCancel.Enabled = false;

            this.Text = "Database Connection Configuration";
        }

        #endregion Load File

        #region Modify DB Items
        private void DispDB(bool update, int index)
        {
            XmlNode baseNode;
            XmlNode subNode;
            XmlNodeList DBList;
            XmlNodeList details;
            clientEnvConfigDoc.Load(clientEnvConfigFileName);
            baseNode = clientEnvConfigDoc.SelectSingleNode("environment/db_links");
            if (update == true)
            {
                if (baseNode.HasChildNodes)
                {
                    // show details in the textbox
                    subNode = baseNode.ChildNodes[index];
                    details = subNode.ChildNodes;
                    tbName.Text = details[0].InnerText;
                    //cmbDBType.SelectedItem = details[1].InnerText;
                    tbHost.Text = details[1].InnerText;
                    tbPort.Text = details[2].InnerText;
                    tbUserName.Text = details[3].InnerText;
                    tbPWD.Text = details[4].InnerText;
                    tbSID.Text = details[5].InnerText;
                    txbMinPool.Text = details[6].InnerText;
                    txbMaxPool.Text = details[7].InnerText;
                    txbLifeTime.Text = details[8].InnerText;
                    //Add items in the list
                    DBList = baseNode.ChildNodes;
                    lbxDB.Items.Clear();
                    lbxDB.BeginUpdate();
                    foreach (XmlNode db in DBList)
                    {
                        lbxDB.Items.Add(db.ChildNodes[0].InnerText);
                    }
                    lbxDB.EndUpdate();
                    lbxDB.SelectedIndex = index;
                }
                else
                {
                    ClearTextBox();
                    lbxDB.Items.Clear();
                }
                btDSave.Enabled = false;
                btDCancel.Enabled = false;
            }
            else
            {
                subNode = baseNode.ChildNodes[index];
                details = subNode.ChildNodes;
                tbName.Text = details[0].InnerText;
                //cmbDBType.SelectedItem = details[1].InnerText;
                tbHost.Text = details[1].InnerText;
                tbPort.Text = details[2].InnerText;
                tbUserName.Text = details[3].InnerText;
                tbPWD.Text = details[4].InnerText;
                tbSID.Text = details[5].InnerText;
                txbMinPool.Text = details[6].InnerText;
                txbMaxPool.Text = details[7].InnerText;
                txbLifeTime.Text = details[8].InnerText;
            }
        }

        private void lbDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxDB.SelectedIndex != -1)
            {
                DispDB(false, lbxDB.SelectedIndex);
            }
        }

        private void btDAdd_Click(object sender, EventArgs e)
        {
            ClearTextBox();
            DenyEdit(false);
            btDSave.Enabled = true;
            btDCancel.Enabled = true;
            btDDele.Enabled = false;
            btDMod.Enabled = false;
            btDAdd.Enabled = false;
            btnTestConnection.Enabled = false;
            btDSave.Tag = "ADD";
        }

        private void ClearTextBox()
        {
            tbName.Clear();
            tbHost.Clear();
            tbPort.Clear();
            tbSID.Clear();
            tbUserName.Clear();
            tbPWD.Clear();
            txbLifeTime.Clear();
            txbMaxPool.Clear();
            txbMinPool.Clear();
        }

        private void btDMod_Click(object sender, EventArgs e)
        {
            DenyEdit(false);
            btDSave.Enabled = true;
            btDCancel.Enabled = true;
            btDDele.Enabled = false;
            btDAdd.Enabled = false;
            btDMod.Enabled = false;
            btnTestConnection.Enabled = false;
            btDSave.Tag = "MOD";
        }

        private void DenyEdit(bool flag)
        {
            tbName.ReadOnly = flag;
            tbHost.ReadOnly = flag;
            tbPort.ReadOnly = flag;
            tbSID.ReadOnly = flag;
            tbUserName.ReadOnly = flag;
            tbPWD.ReadOnly = flag;
            txbMinPool.ReadOnly = flag;
            txbMaxPool.ReadOnly = flag;
            txbLifeTime.ReadOnly = flag;
        }

        private void btDSave_Click(object sender, EventArgs e)
        {

            XmlNode baseNode;
            XmlNode subNode;
            XmlNode detail;
            if (tbName.Text.Trim() == "")
            {
                MessageBox.Show("The <DB Name> is empty, PLS check it!!!");
            }
            //else if (cmbDBType.SelectedItem.ToString() == "")
            //{
            //    MessageBox.Show("The <DB Type> is empty, PLS check it!!!");
            //}
            else if (tbHost.Text.Trim() == "")
            {
                MessageBox.Show("The <DB Host> is empty, PLS check it!!!");
            }
            else if (tbPort.Text.Trim() == "")
            {
                MessageBox.Show("The <DB Port> is empty, PLS check it!!!");
            }
            else if (tbSID.Text.Trim() == "")
            {
                MessageBox.Show("The <DB SID> is empty, PLS check it!!!");
            }
            else if (tbUserName.Text.Trim() == "")
            {
                MessageBox.Show("The <DB UserName> is empty, PLS check it!!!");
            }
            else if (tbPWD.Text.Trim() == "")
            {
                MessageBox.Show("The <DB PassWord> is empty, PLS check it!!!");
            }
            else
            {   //modify by dx_ji 2009.01.15
                //clientEnvConfigDoc.Load(mesEnvConfigfileName);
                clientEnvConfigDoc.Load(clientEnvConfigFileName);
                baseNode = clientEnvConfigDoc.SelectSingleNode("environment/db_links");
                switch (btDSave.Tag.ToString())
                {
                    case "ADD":
                        subNode = clientEnvConfigDoc.CreateElement("db");
                        detail = clientEnvConfigDoc.CreateElement("name");
                        detail.InnerText = tbName.Text.Trim();
                        subNode.AppendChild(detail);
                        //detail = clientEnvConfigDoc.CreateElement("type");
                        //detail.InnerText = cmbDBType.SelectedItem.ToString();
                        //subNode.AppendChild(detail);
                        detail = clientEnvConfigDoc.CreateElement("host");
                        detail.InnerText = tbHost.Text.Trim();
                        subNode.AppendChild(detail);
                        detail = clientEnvConfigDoc.CreateElement("port");
                        detail.InnerText = tbPort.Text.Trim();
                        subNode.AppendChild(detail);
                        detail = clientEnvConfigDoc.CreateElement("user");
                        detail.InnerText = tbUserName.Text.Trim();
                        subNode.AppendChild(detail);
                        detail = clientEnvConfigDoc.CreateElement("password");
                        if (tbPWD.Text.IndexOf("{PWD}") != -1)
                        {
                            detail.InnerText = tbPWD.Text.Trim();
                        }
                        else
                        {
                            detail.InnerText = GetPwdString(tbPWD.Text.Trim()) + "{PWD}";
                        }
                        subNode.AppendChild(detail);
                        detail = clientEnvConfigDoc.CreateElement("sid");
                        detail.InnerText = tbSID.Text.Trim();
                        subNode.AppendChild(detail);
                        //minpoolsize
                        detail = clientEnvConfigDoc.CreateElement("minpoolsize");
                        detail.InnerText = txbMinPool.Text.Trim();
                        subNode.AppendChild(detail);
                        //maxpoolsize
                        detail = clientEnvConfigDoc.CreateElement("maxpoolsize");
                        detail.InnerText = txbMaxPool.Text.Trim();
                        subNode.AppendChild(detail);
                        //life time
                        detail = clientEnvConfigDoc.CreateElement("lifetime");
                        detail.InnerText = txbLifeTime.Text.Trim();
                        subNode.AppendChild(detail);
                        baseNode.AppendChild(subNode);
                        clientEnvConfigDoc.Save(clientEnvConfigFileName);
                        DispDB(true, lbxDB.Items.Count);

                        break;
                    case "MOD":
                        subNode = baseNode.ChildNodes[lbxDB.SelectedIndex];
                        subNode.ChildNodes[0].InnerText = tbName.Text.Trim();
                        //subNode.ChildNodes[1].InnerText = cmbDBType.SelectedItem.ToString();
                        subNode.ChildNodes[1].InnerText = tbHost.Text.Trim();
                        subNode.ChildNodes[2].InnerText = tbPort.Text.Trim();
                        subNode.ChildNodes[3].InnerText = tbUserName.Text.Trim();
                        if (tbPWD.Text.IndexOf("{PWD}") != -1)
                        {
                            subNode.ChildNodes[4].InnerText = tbPWD.Text.Trim();
                        }
                        else
                        {
                            subNode.ChildNodes[4].InnerText = GetPwdString(tbPWD.Text.Trim()) + "{PWD}";
                        }

                        subNode.ChildNodes[5].InnerText = tbSID.Text.Trim();
                        subNode.ChildNodes[6].InnerText = txbMinPool.Text.Trim();
                        subNode.ChildNodes[7].InnerText = txbMaxPool.Text.Trim();
                        subNode.ChildNodes[8].InnerText = txbLifeTime.Text.Trim();
                        clientEnvConfigDoc.Save(clientEnvConfigFileName);
                        DispDB(true, lbxDB.SelectedIndex);
                        break;
                }
                DenyEdit(true);
                btDSave.Enabled = false;
                btDCancel.Enabled = false;
                btDDele.Enabled = true;
                btDMod.Enabled = true;
                btDAdd.Enabled = true;
                btnTestConnection.Enabled = true;
            }

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

        private void btDCancel_Click(object sender, EventArgs e)
        {
            if (btDSave.Tag.ToString() == "MOD")
            {
                if (lbxDB.SelectedIndex != -1)
                {
                    DispDB(false, lbxDB.SelectedIndex);
                }
            }
            else
            {
                DispDB(false, 0);
            }
            DenyEdit(true);
            btDSave.Enabled = false;
            btDCancel.Enabled = false;
            btDDele.Enabled = true;
            btDMod.Enabled = true;
            btDAdd.Enabled = true;
            btnTestConnection.Enabled = true;
        }

        private void btDDele_Click(object sender, EventArgs e)
        {
            XmlNode baseNode;
            XmlNode subNode;

            if (lbxDB.SelectedIndex != -1)
            {   //add by dx_ji 2009.01.15
                if (MessageBox.Show("Are you sure to delete it ?", "Delete Confirm ", MessageBoxButtons.OKCancel,
                     MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                {
                    baseNode = clientEnvConfigDoc.SelectSingleNode("environment/db_links");
                    subNode = baseNode.ChildNodes[lbxDB.SelectedIndex];
                    baseNode.RemoveChild(subNode);
                    clientEnvConfigDoc.Save(clientEnvConfigFileName);
                    DispDB(true, 0);

                    btDSave.Enabled = false;
                    btDCancel.Enabled = false;
                }
            }

        }
        #endregion Modify DB Items


        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            OracleConnection orcconn;
            SqlConnection sqlconn;
            string strPWD = tbPWD.Text.Trim();
            if (strPWD.IndexOf("{PWD}") == -1)
            {
                strPWD = this.GetPwdString(tbPWD.Text.Trim());
            }
            else
            {
                strPWD = this.GetPwdString(strPWD.Substring(0, strPWD.IndexOf("{PWD}")));
            }

            //string connString = "SERVER=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + tbHost.Text.Trim();
            //connString = connString + ")(PORT=" + tbPort.Text.Trim() + ")))(CONNECT_DATA=(SID=" + tbSID.Text.Trim() + ")(SERVER=DEDICATED)));";
            //connString = connString + "UID=" + tbUserName.Text.Trim() + ";PWD=" +strPWD + ";";
            try
            {
                
                string connString = "Data Source=(DESCRIPTION=" + "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + tbHost.Text.Trim() + ")(PORT=" + tbPort.Text.Trim() + ")))" + "(CONNECT_DATA=(SERVICE_NAME = " + tbSID.Text.Trim() + ")));" + "User Id=" + tbUserName.Text.Trim() + ";Password=" + strPWD + ";";
                using (orcconn = new OracleConnection(connString))
                {
                    Cursor.Current = Cursors.WaitCursor; // change cursor to hourglass type
                    orcconn.Open();

                    orcconn.Close();
                    MessageBox.Show("Test connected DB[" + tbName.Text + "] OK.");
                    Cursor.Current = Cursors.Default;
                }

                //}

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}