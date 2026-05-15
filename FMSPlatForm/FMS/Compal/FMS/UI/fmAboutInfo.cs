
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ReadFileStoreDB;
using Compal.FMS.Connections.DBLoader;
using System.Xml;
using System.IO;

namespace Compal.FMS.UI
{
    public partial class fmAboutInfo : Form
    {
        public fmAboutInfo()
        {
            InitializeComponent();
            this.AddInformations();
        }

        private void AddInformations()
        {
            this.rhtxtInformation.Text = "";
            string strInfo = "1. FMS system created for file transfer from SFIS to SWDL.\r\n" +
                                   "2. Used FMS system your can :\r\n" +
                                   "\t1. add new  monitor server.\r\n" +
                                   "\t2. delete one(all)  existed stop monitor server.\r\n" +
                                   "\t3. start one(all) stop monitor server.\r\n" +
                                   "\t4. pause one(all) running monitor server.\r\n" +
                                    "3. Notes:\r\n" +
                                   "\tif you pause a running server and modified it, then you need to restart FMS system while you want to re-start this monitor server.\r\n";
            this.rhtxtInformation.SelectionFont = new Font("PMingLiU", 14, FontStyle.Underline);
            this.rhtxtInformation.SelectionColor = System.Drawing.Color.Red;
            this.rhtxtInformation.Text = strInfo;
            //this.rhtxtInformation. = "3. Notes:\r\n" +
            //                       "\tif you pause a running server and modified it, then you need to restart FMS system while you want to re-start this monitor server.\r\n";

        }

        private void label2_Click(object sender, EventArgs e)
        {
            string filePath;
            filePath = Application.ExecutablePath;
            filePath = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
            string clientEnvConfigFileName = filePath + "database.config";
            XmlDocument clientEnvConfigDoc = new XmlDocument();
            if (File.Exists(clientEnvConfigFileName))
            {
                FileLoader obj = new FileLoader(clientEnvConfigFileName);
                Hashtable ht = obj.GetDBLinks();
            }
            else
            {
                MessageBox.Show("database.config does not exist,PLS check it!!!");
            }
            
        }
    }
}