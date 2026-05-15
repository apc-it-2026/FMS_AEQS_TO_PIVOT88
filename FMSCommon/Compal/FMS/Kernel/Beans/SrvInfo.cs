
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Compal.FMS.Kernel.Beans
{
    public class SrvInfo
    {

        private string mNetDiskRootPath;
        private string mNetDiskIP;
        private string mNetDiskFolder;
        private string mFileLocation;
        private string mNetDiskUser;
        private string mNetDiskPwd;
        private string mDatabase;
        private string mInstanceClass;
        private string mInterval;
        private int mMaxPrcFiles;
        private string mFileFilter;
        private string mNetFolderCHK;
        private string mLineName;
        private string mSrvStatus;
        private string mLocalProcFolder;
        private string mLocalBakFolder;
        private string mLocalErrFolder;
        private string mLocalFolder;//@JC02A
        private string mServiceType;
        private string mSyncType;
        private string mServiceCategory;
        private string mProcessType;
        private string mSectionName;
        private string mGroupName;
        private string mStationName;
        private string mPortNumber;
        private string mReplyFolder;
        private string mOtherFolder1;
        private string mOtherFolder2;
        private string mFolderType;//@JC03A
        private string mDLLFile;//@JC04
        private string mSrvBackup;
        private string mPlantCode;
        private string mAlertMail;
        private string mAlertCount;


        public string SDB { get; set; }
        public string IDB { get; set; }

        public string MDB { get; set; }
        public string Operation { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }


        public string NetDiskRootPath
        {
            set
            {
                this.mNetDiskRootPath = value;
            }
            get
            {
                return this.mNetDiskRootPath;
            }
        }
        public string NetDiskIP
        {
            set
            {
                this.mNetDiskIP = value;
            }
            get
            {
                return this.mNetDiskIP;
            }
        }
        public string NetDiskFolder
        {
            set
            {
                this.mNetDiskFolder = value;
            }
            get
            {
                return this.mNetDiskFolder;
            }
        }
        public string FileLocation
        {
            set { this.mFileLocation = value; }
            get { return this.mFileLocation; }
        }
        public string NetDiskUser
        {
            set
            {
                this.mNetDiskUser = value;
            }
            get
            {
                return this.mNetDiskUser;
            }
        }
        public string NetDiskPwd
        {
            set
            {
                this.mNetDiskPwd = value;
            }
            get
            {
                return this.mNetDiskPwd;
            }
        }
        public string Database
        {
            get { return this.mDatabase; }
            set { this.mDatabase = value; }
        }

        public string InstanceClass
        {
            set { this.mInstanceClass = value; }
            get { return this.mInstanceClass; }
        }
        public string Interval
        {
            set
            {
                this.mInterval = value;
            }
            get
            {
                return this.mInterval;
            }
        }
        public int MaxPrcFiles
        {
            get { return this.mMaxPrcFiles; }
            set { this.mMaxPrcFiles = value; }
        }
        public string FileFilter
        {
            get { return this.mFileFilter; }
            set
            {
                this.mFileFilter = value;
            }
        }
        public string NetFolderCheck
        {
            get
            {
                return this.mNetFolderCHK;
            }
            set
            {
                this.mNetFolderCHK = value.ToUpper();
            }
        }
        public string LineName
        {
            get { return this.mLineName; }
            set
            {
                if (value == null || value == "")
                {
                    value = "FMSLineA";
                }
                this.mLineName = value;
            }
        }
        public string SrvStatus
        {
            get { return this.mSrvStatus; }
            set { this.mSrvStatus = value; }
        }

        public string LocalProcFolder
        {
            get { return this.mLocalProcFolder; }
            set { this.mLocalProcFolder = value; }
        }
        public string LocalBakFolder
        {
            get { return this.mLocalBakFolder; }
            set { this.mLocalBakFolder = value; }
        }

        public string LocalErrFolder
        {
            get { return this.mLocalErrFolder; }
            set { this.mLocalErrFolder = value; }
        }
        //@JC02A start
        public string LocalFolder
        {
            get { return this.mLocalFolder; }
            set { this.mLocalFolder = value; }
        }
        //@JC02A end

        // private string mServiceType;
        public string ServiceType
        {
            get { return this.mServiceType; }
            set { this.mServiceType = value; }
        }
        public string SyncType
        {
            get { return this.mSyncType; }
            set { this.mSyncType = value; }
        }
        public string ServiceCategory
        {
            get { return this.mServiceCategory; }
            set { this.mServiceCategory = value; }
        }
        public string ProcessType
        {
            get { return this.mProcessType; }
            set { this.mProcessType = value; }
        }
        public string SectionName
        {
            get { return this.mSectionName; }
            set { this.mSectionName = value; }
        }

        public string GroupName
        {
            get { return this.mGroupName; }
            set { this.mGroupName = value; }
        }

        public string StationName
        {
            get { return this.mStationName; }
            set { this.mStationName = value; }
        }

        public string PortNumber
        {
            get { return this.mPortNumber; }
            set { this.mPortNumber = value; }
        }

        public string ReplyFolder
        {
            get { return this.mReplyFolder; }
            set { this.mReplyFolder = value; }
        }

        public string OtherFolder1
        {
            get { return this.mOtherFolder1; }
            set { this.mOtherFolder1 = value; }
        }

        public string OtherFolder2
        {
            get { return this.mOtherFolder2; }
            set { this.mOtherFolder2 = value; }
        }

        //@JC03A start
        public string FolderType
        {
            get { return this.mFolderType; }
            set { this.mFolderType = value; }
        }
        //@JC03A end

        //@JC04A start
        public string DLLFile
        {
            get { return this.mDLLFile; }
            set { this.mDLLFile = value; }
        }
        //@JC04A end

        //@JC05A start
        public string PlantCode
        {
            get { return this.mPlantCode; }
            set { this.mPlantCode = value; }
        }
        //@JC05A end

        public string SrvBackup
        {
            get { return this.mSrvBackup; }
            set { this.mSrvBackup = value; }
        }

        public string AlertMail
        {
            get { return this.mAlertMail; }
            set { this.mAlertMail = value; }
        }

        public string AlertCount
        {
            get { return this.mAlertCount; }
            set { this.mAlertCount = value; }
        }



        public override string ToString()
        {
            string result = "";
            result =
                "ServiceCategory:" + this.ServiceCategory + "\r\n" +
                "SDB:" + this.SDB + "\r\n" +
                "IDB:" + this.IDB + "\r\n" +
                "MDB:" + this.MDB + "\r\n" +
                "Operation:" + this.Operation + "\r\n";

            return result;
        }
    }
}