using System;
using System.Collections.Generic;
using System.Text;

namespace Compal.FMS
{
    public sealed class DBInfo
    {
        private string dbName;
        private string dbType;
        private string dbUser;
        private string dbPwd;
        private string dbHost;
        private string dbSid;
        private string dbPort;
        private string dbMaxPoolSize;//@DX01A
        private string dbMinPoolSize;//@DX01A
        private string dbLifeTime;

        public string Name
        {
            get { return this.dbName; }
            set { this.dbName = value; }
        }
        public string Type
        {
            get { return this.dbType; }
            set { this.dbType = value; }
        }
        public string LoginPwd
        {
            get { return this.dbPwd; }
            set { this.dbPwd = value; }
        }

        public string LoginUser
        {
            get { return this.dbUser; }
            set { dbUser = value; }
        }

        public string Host
        {
            get { return this.dbHost; }
            set { this.dbHost = value; }
        }

        public string Sid
        {
            get { return this.dbSid; }
            set { this.dbSid = value; }
        }

        public string Port
        {
            get { return this.dbPort; }
            set { this.dbPort = value; }
        }

        public string MaxPoolSize
        {
            get { return this.dbMaxPoolSize; }
            set { this.dbMaxPoolSize = value; }
        }

        public string MinPoolSize
        {
            get { return this.dbMinPoolSize; }
            set { this.dbMinPoolSize = value; }
        }

        public string LifeTime
        {
            get { return this.dbLifeTime; }
            set { this.dbLifeTime = value; }
        }
    }
}