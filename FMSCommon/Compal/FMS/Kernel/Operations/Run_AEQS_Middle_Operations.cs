using Compal.FMS.Kernel.Beans;
using FMSCommon.Compal.FMS.Kernel.Utils;
using SJeMES_Framework.WebAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FMSCommon.Compal.FMS.Kernel.Operations
{
    class Run_AEQS_Middle_Operations
    {
        #region TQC_Middle
        public async void PostRequestAsync_TQC_Middle(SrvInfo vsrvinfo)
        {
            //test
            //string APIURL = "http://localhost:60627/api/CommonCall";
            //string APIURL = "http://10.3.0.29:8092//api/commoncall";
            string APIURL = "http://10.3.0.24:8082//api/commoncall"; 
            //string token = "7f6b1d94-5ff3-44f6-bf7a-69a14590c3bb";  //APC_test 5001
            string token = "1cc13bf3-2f3d-49b2-8ebc-6239c98f55c0";  //APC_real 5001

            //offical
            //string APIURL = "http://10.1.1.1:8090/api/CommonCall";
            //string token = "16A5EA69-5A49-519E-D976-4C34858B37EF";


            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();

                string retdata = Post(APIURL,
                                        "SJ_TQCAPI",//类库名
                                        "SJ_TQCAPI.TQC_Task",//类名
                                        "TransferDataToPivot88",//方法名
                                        token,//token
                                        Newtonsoft.Json.JsonConvert.SerializeObject(dic));

                ResultObject ret = Newtonsoft.Json.JsonConvert.DeserializeObject<ResultObject>(retdata);
                if (ret.IsSuccess)
                {
                    FMSLOG.Platform("Success TQC: ", vsrvinfo.Operation);
                }

            }
            catch (Exception ex)
            {
                string msg = "Error Msg:" + ex.Message;
            }

        }
        #endregion

        #region TQCRework_Middle
        public async void PostRequestAsync_TQCRework_Middle(SrvInfo vsrvinfo)
        {
            //test
            //string APIURL = "http://localhost:60627//api/CommonCall";
            //string APIURL = "http://10.3.0.29:8092/api/CommonCall";//test
            string APIURL = "http://10.3.0.24:8082/api/CommonCall";//real
            //string token = "7f6b1d94-5ff3-44f6-bf7a-69a14590c3bb"; //APC_test  5001
            string token = "1cc13bf3-2f3d-49b2-8ebc-6239c98f55c0";  //APC_real 5001

            /* //offical
             string APIURL = "http://10.30.3.50:8082//api/CommonCall";
             string token = "090ee44c-8ef3-49a9-b613-d20220751712";*/

            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();

                string retdata = Post(APIURL,
                                        "SJ_TQCAPI",//类库名
                                        "SJ_TQCAPI.TQC_Task",//类名
                                        "GenerateTQCReworkTaskByAQL",//方法名
                                        token,//token
                                        Newtonsoft.Json.JsonConvert.SerializeObject(dic));

                ResultObject ret = Newtonsoft.Json.JsonConvert.DeserializeObject<ResultObject>(retdata);
                if (ret.IsSuccess)
                {
                    FMSLOG.Platform("Success TQC: ", vsrvinfo.Operation);
                }

            }
            catch (Exception ex)
            {
                string msg = "Error Msg:" + ex.Message;
            }

        }
        #endregion

        #region Common Methods
        public static string Post(string url, string DllName, string ClassName, string Method, string UserToken, string Data)
        {

            RequestObject obj = new RequestObject(DllName, ClassName, Method,
                string.Empty, string.Empty, false, string.Empty, false, UserToken, Data);

            return Post(url, obj);

        }

        public static string Post(string url, RequestObject obj)
        {
            string body = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

            return HttpPost(url, body);
        }

        private static string HttpPost(string url, string body)
        {

            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.Timeout = 600000;//10min
            request.ContentType = "application/json";
            //request.Headers.Add("lanauage", Language);
            byte[] buffer = encoding.GetBytes(body);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        #endregion
    }
}
