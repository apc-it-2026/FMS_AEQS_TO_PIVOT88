using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Threading;

namespace Compal.FMS.Kernel.Utils
{
    class FileGet
    {
        #region Param Define
        Hashtable hsTmp = new Hashtable();
        private int maxReturnFileCount = -1;
        private int procFileCount = 0;

        public int MaxReturnFileCount
        {
            get { return maxReturnFileCount; }
            set { maxReturnFileCount = Math.Abs(value); }
        }

        object oblock = new object();
        #endregion

        #region Public Function Define
        public Hashtable ReturnFileList(string FolderPath, string FileFilter, out string ProcessResult)
        {
            if (Directory.Exists(FolderPath) == true)
            {
                DirectoryInfo dr = new DirectoryInfo(FolderPath);
                FileInfo[] fi = dr.GetFiles(FileFilter);
                Array.Sort(fi, new DateSorter());
                procFileCount = fi.Length;
                if (maxReturnFileCount == -1)
                    maxReturnFileCount = 10;

                if (procFileCount > maxReturnFileCount * 2)
                    procFileCount = maxReturnFileCount * 2;

                if (procFileCount > 0)
                {
                    Thread[] trlist = new Thread[procFileCount];
                    int trCreateCount = 0;
                    for (int i = 0; i < procFileCount; i++)
                    {
                        if (hsTmp.Count <= MaxReturnFileCount)
                        {
                            trlist[i] = new Thread(new ParameterizedThreadStart(LockCheck));
                            trlist[i].Start(fi[i].FullName);
                            trCreateCount = i;
                        }
                    }

                    for (int k = 0; k < trCreateCount; k++)
                        trlist[k].Join();

                    if (hsTmp.Count > 0)
                    {
                        lock (oblock)
                        {
                            Hashtable hsResult = hsTmp.Clone() as Hashtable;
                            hsTmp.Clear();
                            ProcessResult = "OK";
                            return hsResult;
                        }
                    }
                    else
                    {
                        ProcessResult = "All files is Locked!";
                        return null;
                    }
                }
                else
                {
                    ProcessResult = " NO File Found!";
                    return null;
                }
            }
            else
            {
                ProcessResult = FolderPath + " Folder is not exist!";
                return null;
            }

        }
        #endregion

        #region Private Function Define
        private void LockCheck(object Filepaht)
        {
            if (hsTmp.Count <= MaxReturnFileCount)
            {
                string strFileName = Filepaht as string;
                bool tmpUnlock = true;
                try
                {
                    if (File.Exists(strFileName) == true)
                    {
                        using (FileStream fs = new FileStream(strFileName, FileMode.Open, FileAccess.Write))
                        {
                            if (fs.CanWrite)
                            {
                                tmpUnlock = true;
                            }
                            else
                            {
                                tmpUnlock = false;
                            }
                        }
                    }
                    else
                    {
                        tmpUnlock = false;
                    }
                }
                catch
                {
                    tmpUnlock = false;
                }

                if (tmpUnlock == true)
                {
                    lock (oblock)
                    {
                        if (hsTmp.ContainsKey(strFileName.ToUpper().Trim().ToUpper()) == false)
                            hsTmp.Add(strFileName.ToUpper().Trim().ToUpper(), 1);
                    }
                }
            }
        }
        #endregion
    }
}
