using System;
using System.Collections.Generic;
using System.Text;

namespace Compal.FMS.Component
{
    //@SJ01 Added Start
    [Serializable]
    //@SJ01 End
    public class ExecutionResult
    {
        private bool statusField;
        private string messageField;
        private object anythingField;

        /// <summary>
        /// Returns true or false
        /// </summary>
        public bool Status
        {
            get { return this.statusField; }
            set { this.statusField = value; }
        }

        /// <summary>
        /// Returns message of string type
        /// </summary>
        public string Message
        {
            get { return this.messageField; }
            set { this.messageField = value; }
        }

        /// <summary>
        /// Returns a object
        /// </summary>
        public object Anything
        {
            get { return this.anythingField; }
            set { this.anythingField = value; }
        }


    }
}
