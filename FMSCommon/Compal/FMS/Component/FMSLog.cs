#region Copyright & License
/******************************************************************************
* This document is the property of Compal Electronics Inc, (Compal).
* No exploitation or transfer of any information contained herein is permitted 
* in the absence of an agreement with Compal, 
* and neither the document nor any such information
* may be released without the written consent of Compal
*  
* All right reserved by Compal Electronics Inc.  
*******************************************************************************
* Owner: Jason   
* Version: 1.2.0.4
* FMS.Component: MES File Monitor
* Function Description:*
* Revision / History
*------------------------------------------------------------------------------
* Flag     Date     Who             Changes Description
* -------- -------- --------------- -------------------------------------------
*          20100730 Jason           File created for new simple FMS
* JC01     20101020 Jason           remove FT and DL log         
*------------------------------------------------------------------------------
*/
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace Compal.FMS.Component
{
    public class FMSLog
    {
        public static string PLATFORM = "PlatformLog";
        public static string DATABASE = "DBLog";
        //public static string FT_CLIENT = "FTLog";//@JC01D
        //public static string DL_CLIENT = "DLLog";//@JC01D
    }
}
