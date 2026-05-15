using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compal.FMS
{
    public class OracleString
    {
        public OracleString() { }

        public string ToConString(DBInfo vDBInfo)
        {
            string result;

            if (vDBInfo.Type == "SQL SERVER")
            {
                result = "Data Source=" + vDBInfo.Host + ";Initial Catalog=" + vDBInfo.Sid + ";User ID=" + vDBInfo.LoginUser + ";Password=" + vDBInfo.LoginPwd + "";

            }
            else
            {
                result = "Data Source=(DESCRIPTION=" + "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + vDBInfo.Host;
                result = result + ")(PORT=" + vDBInfo.Port + ")))" + "(CONNECT_DATA=(SID = " + vDBInfo.Sid + ")));";
                result = result + "User Id=" + vDBInfo.LoginUser + ";Password=" + vDBInfo.LoginPwd + ";";

                //@Start Add By DX.JI 2010.2.22
                if (vDBInfo.MaxPoolSize != null && vDBInfo.MaxPoolSize != "" &&
                    vDBInfo.MinPoolSize != null && vDBInfo.MinPoolSize != "")
                {
                    result = result + "Min Pool Size=" + vDBInfo.MinPoolSize + ";Max Pool Size=" + vDBInfo.MaxPoolSize + ";";
                    //result = result + "Connection Lifetime=120;Persist Security Info=True;";
                    result = result + "Connection Lifetime=" + vDBInfo.LifeTime + ";Persist Security Info=True;";
                }
            }



            //@End.
            return result;
        }
    }
}