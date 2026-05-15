using Compal.FMS.Kernel.Beans;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FMSCommon.Compal.FMS.Kernel.Operations
{
    class Run_ManDay_Input_Data
    {
        #region ManDay Report
        string objconfig3 = "Data Source=10.3.0.208;Initial Catalog=PMO_ITMANPOWER_INPUT; uid=sa; Password=apc-1234;";
        public class User_data
        {
            public string Region { get; set; }
            public string Department { get; set; }
            public string EmployeeName { get; set; }
            public string EmployeeNumber { get; set; }
            public DateTime? FillDate { get; set; }
            public float? LtH { get; set; }

        }
        //

        //Manday Input Missing Whatsapp API
        public void CheckForMissingInput(SrvInfo vsrvinfo)
        {
            List<string> response = new List<string>();
            try
            {
            
            string region = "APC";

                //PASTE HER
                DateTime today = DateTime.Now;
                DateTime endDate = today.AddDays(-1);

                DateTime startDate = endDate.AddDays(-(int)endDate.DayOfWeek + (int)DayOfWeek.Monday);
                if (endDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    startDate = startDate.AddDays(-7);
                }
                
            // Format the dates as strings
            string startDateStr = startDate.ToString("yyyy-MM-dd");
            string endDateStr = endDate.ToString("yyyy-MM-dd");


            //string startDate = "2024-12-16";
            // string endDate = "2024-12-19";
            string fileName = $@"Manday_Missing_input{DateTime.Now:yyyyMMdd_HHmmss}";
            string message = $@"Dear All,
Kindly find attached the list of missing Manday inputs from {startDateStr} to {endDateStr}. I request that you input the necessary data for those who have not yet submitted their Manday information. For employees currently on leave, please ensure that their deputy fills in the required details
Thank you";
            string body = @"<!DOCTYPE html>

<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>Manday Mis</title>
 <style>
       

        th {
            background-color: #65b2ff;
            color: black;
            text-align: center;
            padding: 10px;
        }

        td {
            text-align: center;
            padding: 8px;
            border: 1px solid #ddd;
        }
    </style>
</head>
<body>";
            //DataTable missingInputEmployees = GetMissingInput_Procedure(startDate, endDate, region);

            // string base64Excel = ConvertDataTableToExcelBase64(missingInputEmployees);
            //List<User_data> employees = GetMissingInput_Procedure1(startDate, endDate, region);
            var employees = GetMissingInput_Procedure1(startDateStr, endDateStr, region);

            if (employees == null || !employees.Any())
            {
                    response.Add("No data available for the specified date range from " + startDateStr + " to " + endDateStr + ".");
                    return;
            }
            body += "<table border='1'>";
            body += "<tr><th>Region</th><th>Department</th><th>Employee Number</th><th>Employee Name</th><th>Fill Date</th><th>LT(h)</th></tr>";
            foreach (var emp in employees)
            {
                string formattedDate = emp.FillDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                body += $"<tr><td>{emp.Region}</td><td>{emp.Department}</td><td>{emp.EmployeeNumber}</td><td>{emp.EmployeeName}</td><td>{formattedDate}</td><td>{emp.LtH}</td></tr>";
            }
            body += "</table></body></html>";


            response = SendMandayReportAsimage(fileName, message, body);

            // string base64Excel = ConvertListToExcelBase64(employees);

            // response = SendWhatsappMessage(message, fileName, base64Excel);
            //string result = string.Join(", ", response);
            //SendWhatsappMessage(result);
            //return response;

            }
            catch(Exception e)
            {
                response.Add(e.Message);
                //string result = string.Join(", ", response);
                //SendWhatsappMessage(result);
            }
            finally
            {
                string result = string.Join(", ", response);
                SendWhatsappMessage(result);
               
            }
           // return response;

        }

        public List<User_data> GetMissingInput_Procedure1(string startDate, string endDate, string region)
        {
            List<User_data> employees = new List<User_data>();

            SqlConnection connection = new SqlConnection(objconfig3);

            connection.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GET_MISSING_MANDAY_DATA";

            cmd.Parameters.AddWithValue("@START_DATE", startDate);
            cmd.Parameters.AddWithValue("@END_DATE", endDate);
            cmd.Parameters.AddWithValue("@REGION", region);


            cmd.ExecuteNonQuery();


            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                employees.Add(new User_data
                {
                    // ID = reader.GetInt32(reader.GetOrdinal("ID")),
                    Region = reader["REGION"] as string,
                    Department = reader["DEPARTMENT"] as string,
                    EmployeeName = reader["EMP_NAME"] as string,
                    EmployeeNumber = reader["EMP_NO"] as string,
                    FillDate = reader.IsDBNull(reader.GetOrdinal("FILL_DATE")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("FILL_DATE")),
                    LtH = reader.IsDBNull(reader.GetOrdinal("TOTAL_LT_H")) ? (float?)null : (float)reader.GetDouble(reader.GetOrdinal("TOTAL_LT_H"))


                });



            }

            return employees;


        }


        public List<string> SendMandayReportAsimage(string fileName, string msg, string htmldata)
        {

            List<string> responseMessages = new List<string>();


            string apiUrl = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendHTML_AS_IMAGE";
            var payload = new
            {
                tagNumbers = new List<string>(),//tag person in group 9550721624(chandra sir)
                numbers = new List<string>(), // Use the fetched phone number
                groups = new[] { "919490996631-1606387197@g.us" },//919490996631-1606387197@g.us//120363347683285873@g.us
                textMsg = msg,
                htmL_Code = htmldata,
                fileName = fileName
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Serialize payload to JSON
                    string jsonPayload = JsonConvert.SerializeObject(payload);

                    // Create HttpContent for the JSON payload
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send POST request
                    HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = response.Content.ReadAsStringAsync().Result;
                        responseMessages.Add(responseData); // Add response to the list
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");

                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");

            }

            return responseMessages;

        }

        public List<string> SendWhatsappMessage(string msg)
        {
            
            List<string> responseMessages = new List<string>();
            string url = "http://10.3.0.208:9090/whatsapp/WhatsappApi/SendMessage";

            // Payload for the API
            var payload = new
            {
                numbers = new List<string> { "8297495918" },
                groups = new List<string>(),
                textMsg = msg,
                mediaurl = "",
                filename = ""
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Serialize payload to JSON
                    string jsonPayload = JsonConvert.SerializeObject(payload);

                    // Create HttpContent for the JSON payload
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send POST request
                    HttpResponseMessage response = client.PostAsync(url, content).Result;

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = response.Content.ReadAsStringAsync().Result;
                        responseMessages.Add(responseData); // Add response to the list
                    }
                    else
                    {
                        responseMessages.Add($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessages.Add($"Exception: {ex.Message}");
            }

            return responseMessages;
        }
        #endregion

    }
}
