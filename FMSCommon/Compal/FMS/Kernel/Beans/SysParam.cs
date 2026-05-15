
using System;
using System.Collections.Generic;
using System.Text;

namespace Compal.FMS.Kernel.Beans
{
    public class SysParam
    {

        public static string FMS_SERVICE_FILE = "file";
        public static string FMS_SERVICE_SOCKET = "socket";
        public static string FMS_SERVICE_LOGIC = "logic";
        public static string FMS_SERVICE_CRAWLER = "crawler";

        // thread log.xml
        public static string THREAD_LOG_FILE = "thread_log.config";
        //mail_config.xml
        public static string MAIL_CONFIG_FILE = "mail.config";
        public static string MAIL_CONTENT_FILE = "exception_content.htm";
        public static string MAIL_CONFIG = "mail_config";
        public static string MAIL_FROM = "mail_from";
        public static string MAIL_TO = "mail_to";
        public static string MAIL_CC = "mail_cc";
        public static string MAIL_SMTP = "smtp_host";
        public static string MAIL_EXCEPTION_SUBJECT = "exception_subject";
        public static string MAIL_OUTOFPROC_SUBJECT = "outofproc_subject";
        public static string MAIL_PRIORITY = "priority";
        public static string MAIL_BACKUP_MONITOR_INTERVAL = "backup_monitor_interval";
        public static string MAIL_BACKUP_MONITOR_TIMES = "backup_monitor_times";

        //service.xml

        public static string SRV_FILE ="service.config"; 
        

        public static string SRV_LIST ="service_list";
        public static string SRV_CATEGORY = "service_category";
        public static string PROCESS_TYPE = "process_type";
        public static string SRV_POINT="service"; 
        public static string SRV_TYPE="type"; 
        public static string SYNC_TYPE="synctype"; 
    
        public static string SRV_NETSERVER_IP = "netserver_ip";
        public static string SRV_NETSERVER_FOLDER = "netserver_folder";

        public static string USR_DATABASE="database";
        public static string S_DATABASE="sdb";
        public static string I_DATABASE="idb";
        public static string M_DATABASE="mdb";
        public static string OPERATION = "operation";
        public static string SRV_LINENAME="netserver_linename"; 
        public static string SRV_SERVERDLL="server_dll"; 
        public static string SRV_SRVLogicMTHD="server_logicmethod"; 
        public static string SRV_DASHBOARDMTHD="server_dashboard"; 
        public static string SRV_PARTMAPEXPT="server_partmapexpt"; 
        public static string SRV_NETDISK_USER= "netdisk_user"; 
        public static string SRV_NETDISK_PWD="netdisk_pwd"; 
        public static string SRV_NETDISK_ROOTPATH="netdisk_rootpath"; 
        public static string SRV_INSTANCE_CLASS="instance_class"; 
        public static string SRV_NETFOLDERCHK="netfolder_check"; 
        public static string SRV_INTERVAL="interval"; 
        public static string SRV_MAXPRCFILES= "maxprcfiles";
        public static string SRV_BACKUPMODE="backup"; 
        public static string SRV_ALARMMAILADDR="alarmmailaddr"; 
        public static string SWDL_REPLY_FILES="swdl_replyfiles"; 
        public static string FILTER_TYPE= "filtertype"; 
        public static string LINE_NAME="line_name";
        public static string SRV_SECTIONNAME = "netserver_sectionname";
        public static string SRV_GROUPNAME = "netserver_groupname";
        public static string SRV_STATIONNAME = "netserver_stationname"; 
        public static string PORT_NUMBER="port_number";
        public static string SECTION_NAME = "section_name";
        public static string GROUP_NAME = "group_name";
        public static string STATION_NAME = "station_name";
        public static string SRV_REPLY = "reply_folder";
        public static string SRV_OTHER_FOLDER1 = "other_folder1";
        public static string SRV_OTHER_FOLDER2 = "other_folder2";
        public static string SRV_FOLDER_TYPE = "folder_type";//@JC02A
        public static string FOLDER_TYPE_LOCAL = "local";//@JC02A
        public static string FOLDER_TYPE_REMOTE = "remote";//@JC02A
        public static string DLL_FILE = "dll_file";//@JC03A
        public static string PLANT_CODE = "plant_code";//@JC04A
        public static string Alert_Mail = "alert_mail";//@JC04A
        public static string Alert_Count = "alert_count";//@JC04A


        //@JC01A start
        public static string DWG_SERVICE_CATEGORY = "ServiceCategory";
        public static string DWG_SERVICE_STATUS = "SrvStatus";
        public static string DWG_S_DATABASE = "sdb";
        public static string DWG_I_DATABASE = "idb";
        public static string DWG_M_DATABASE = "mdb";
        public static string DWG_OPERATION = "operation";
        public static string DWG_SYNCTYPE = "SyncType";
        public static string DWG_INTERVAL = "Interval";
        //@JC01A end


    }
}
