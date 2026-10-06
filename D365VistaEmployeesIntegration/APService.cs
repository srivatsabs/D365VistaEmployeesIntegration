using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Net.Http;
using System.Threading;
using System.IO;
using System.Text.Json;
using System.Configuration;
using Azure;

namespace D365VistaEmployeesIntegration
{
	internal class APService
    {
        private static readonly HttpClient clientRequest = new HttpClient();
        private readonly string baseUrlAddress = ConfigurationManager.AppSettings["BaseUrlAddress"].ToString();
		SQLData objD = new SQLData();
		
		bool isInfoLogEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["LogInfo"].ToString());
		string Domain = ConfigurationManager.AppSettings["Domain"].ToString();
		string ClientId = ConfigurationManager.AppSettings["ClientId"].ToString();
		string ClientSecret = ConfigurationManager.AppSettings["ClientSecret"].ToString();
		string Resource = ConfigurationManager.AppSettings["Resource"].ToString();
		private string Container = ConfigurationManager.AppSettings["ContainerName"].ToString();
		private string ConnectionString = ConfigurationManager.ConnectionStrings["AzureBlobConnectionString"].ToString();
		public string GetToken()
        {
            if (isInfoLogEnabled)
            {
                Logs objLogs = new Logs
                {
                    Description = $"Entered GetToken method.",
                    ExceptionMessage = ""
                };
                int sucessStatus = objD.AddLogs(objLogs);
            }
            try
            {

                string domain = Domain;
				string clientId = ClientId;
				string clientSecret = ClientSecret;
				string resource = Resource;

                HttpClient client = new HttpClient();
                string requestUrl = $"https://login.microsoftonline.com/{domain}/oauth2/token";
                string request_content = $"grant_type=client_credentials&resource={resource}&client_id={clientId}&client_secret={clientSecret}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                try
                {
                    request.Content = new StringContent(request_content, Encoding.UTF8, "application/x-www-form-urlencoded");
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
                HttpResponseMessage response = client.SendAsync(request).Result;
                string responseString = response.Content.ReadAsStringAsync().Result;
                var token = JsonConvert.DeserializeObject<TokenDC>(responseString);
                var accessToken = token.access_token;
                return accessToken;
            }
            catch 
            {
				if (isInfoLogEnabled)
				{
					Logs objLogs = new Logs
					{
						Description = $"Exception in GetToken method",
						ExceptionMessage = "Exception in GetToken method"
					};
					int sucessStatus = objD.AddLogs(objLogs);
				}
				throw;
            }
        }
        
        public async Task<string> InsertEntityData(string dataEntityName, string requestContent, string erpToken, string personalNumber)
        {
            try
            {
                clientRequest.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", erpToken);
                string endpointUrl = $"{baseUrlAddress}{dataEntityName}";

                var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
                {
                    Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
                };

                int retries = 0;
                int maxRetries = 2;

                while (retries < maxRetries)
                {
                    HttpResponseMessage response = await clientRequest.SendAsync(request);                   

                    if (response.IsSuccessStatusCode)
                    {
                        int i = objD.AddProceddedExpenseHeader(personalNumber);
                        return System.Text.Json.JsonSerializer.Serialize(new
                        {
                            success = true,
                            statusCode = (int)response.StatusCode,
                            entity = dataEntityName
                        });
                    }
                    else if ((int)response.StatusCode == 429 && response.Headers.Contains("Retry-After"))
                    {
                        // Extract Retry-After value and wait before retrying
                        int retryAfterSeconds = int.Parse(response.Headers.GetValues("Retry-After").FirstOrDefault() ?? "10");
                        await Task.Delay(TimeSpan.FromSeconds(retryAfterSeconds));
                    }
                    else
                    {
						if (isInfoLogEnabled)
						{
							Logs objLogs = new Logs
							{
								Description = $"Status is not success",
								ExceptionMessage = "Status Code is "+(int)response.StatusCode
							};
							int sucessStatus = objD.AddLogs(objLogs);
						}
						return System.Text.Json.JsonSerializer.Serialize(new
                        {
                            success = false,
                            statusCode = (int)response.StatusCode,
                            entity = dataEntityName,
                        });
                    }

                    retries++;
                }
				if (isInfoLogEnabled)
				{
					Logs objLogs = new Logs
					{
						Description = $"Status is request timeout",
						ExceptionMessage = "Status Code is 408"
					};
					int sucessStatus = objD.AddLogs(objLogs);
				}
				return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    statusCode = 408, // Request Timeout
                    entity = dataEntityName
                });
            }
            catch (Exception ex)
            {
				if (isInfoLogEnabled)
				{
					Logs objLogs = new Logs
					{
						Description = $"Exception in InsertEntityData method.Hence Returning statusCode 500",
						ExceptionMessage = ex.Message
					};
					int sucessStatus = objD.AddLogs(objLogs);
				}
				return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    statusCode = 500,
                    entity = dataEntityName
                });
            }

        }
    }
}
