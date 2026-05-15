using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace Compal.FMS
{
    public class DateSorter : IComparer
    {
        #region IComparer Members
        public int Compare(object x, object y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            FileInfo xInfo = (FileInfo)x;

            FileInfo yInfo = (FileInfo)y;

            //¨Ì¦WºÙ±Æ§Ç  
            return xInfo.FullName.CompareTo(yInfo.FullName);//»¼¼W  
            //return yInfo.FullName.CompareTo(xInfo.FullName);//»¼´î  

            //¨Ì­×§ï¤é´Á±Æ§Ç  
            //return xInfo.LastWriteTime.CompareTo(yInfo.LastWriteTime);//»¼¼W  
            //return yInfo.LastWriteTime.CompareTo(xInfo.LastWriteTime);//»¼´î  
        }
        #endregion
    }
}