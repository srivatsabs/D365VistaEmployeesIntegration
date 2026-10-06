using Azure;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace D365VistaEmployeesIntegration
{
    public class ProcessD365
    {
        SQLData objD = new SQLData();
        bool isInfoLogEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["LogInfo"].ToString());

        public async Task ProcessRecordsAsync()
        {
            if (isInfoLogEnabled)
            {
                Logs objLogs = new Logs
                {
                    Description = $"Entered ProcessRecordsAsync method.",
                    ExceptionMessage = ""
                };
                int sucessStatus = objD.AddLogs(objLogs);
            }
            APService objResponse = new APService();
            try
            {
                string s = objResponse.GetToken();
                List<EmployeeDetails> objEmployeeDetails = objD.GetEmployeeRecords();

                var failed = new ConcurrentBag<string>();
                var semaphore = new SemaphoreSlim(5);

                var tasks = objEmployeeDetails.Select(async emp =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        string payload = JsonSerializer.Serialize(emp);

                        string response = await objResponse.InsertEntityData("REEDVistaEmp", payload, s, emp.PersonnelNumber);
                        var jsonResponse = JsonSerializer.Deserialize<dynamic>(response);
                        if (!jsonResponse.GetProperty("success").GetBoolean())
                        {
                            failed.Add(emp.PersonnelNumber);
                        }                            
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                
            }
            catch (Exception e)
            {
                if (isInfoLogEnabled)
                {
                    Logs objLogs = new Logs
                    {
                        Description = $"Error in processing",
                        ExceptionMessage = e.Message
                    };
                    int sucessStatus = objD.AddLogs(objLogs);
                    SendFailureEmail(e.Message);
                }
                else
                {
                    SendFailureEmail(e.Message);
                }
            }

            if (isInfoLogEnabled)
            {
                Logs objLogs = new Logs
                {
                    Description = $"Exited ProcessRecordsAsync method.",
                    ExceptionMessage = ""
                };
                int sucessStatus = objD.AddLogs(objLogs);
            }
        }

        public void SendFailureEmail(string message)
        {
            try
            {
                if (isInfoLogEnabled)
                {
                    Logs objLogs = new Logs
                    {
                        Description = $"Entered SendFailureEmail method",
                        ExceptionMessage = ""
                    };
                    int sucessStatus = objD.AddLogs(objLogs);
                }

                string toAddress = ConfigurationManager.AppSettings["ToAddress"].ToString();

                string bodyMessage = message;

                string subject = ConfigurationManager.AppSettings["MailSubject"].ToString();

                string footerMessage = ConfigurationManager.AppSettings["FooterMessage"].ToString();
                string[] toAddressList = toAddress.Split(';');

                MailMessage newMessage = new MailMessage();
                SmtpClient mailService = new SmtpClient();
                newMessage.From = new MailAddress(ConfigurationManager.AppSettings["FromAddress"].ToString());

                foreach (var to in toAddressList)
                {
                    newMessage.To.Add(to);
                }
                //foreach (var cc in ccAddressList)
                //{
                //    newMessage.CC.Add(cc);
                //}
                newMessage.Subject = subject;
                StringBuilder mailBody = new StringBuilder();
                mailBody.AppendFormat("<p>" + bodyMessage + "</p>");
                mailBody.AppendFormat("<p>" + footerMessage + "</p>");
                newMessage.Body = mailBody.ToString();
                newMessage.IsBodyHtml = true;
                mailService.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"].ToString());
                mailService.EnableSsl = true;
                mailService.DeliveryMethod = SmtpDeliveryMethod.Network;

                mailService.Host = ConfigurationManager.AppSettings["SMTPHost"].ToString();
                mailService.UseDefaultCredentials = false;
                mailService.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["EmailAdd"].ToString(), ConfigurationManager.AppSettings["EmailPWD"].ToString());
                mailService.Send(newMessage);
                if (isInfoLogEnabled)
                {
                    Logs objLogs = new Logs
                    {
                        Description = $"Email sent to " + toAddress + "  successfully in SendFailureEmail method.",
                        ExceptionMessage = ""
                    };
                    int sucessStatus = objD.AddLogs(objLogs);
                }

            }
            catch (SmtpException ex)
            {
                if (isInfoLogEnabled)
                {
                    Logs objLogs = new Logs
                    {
                        Description = $"Smtp Exception in SendFailureEmail method.",
                        ExceptionMessage = (ex.InnerException == null) ? ex.Message : ex.InnerException?.Message
                    };
                    int sucessStatus = objD.AddLogs(objLogs);
                }


            }
            catch (Exception ex)
            {
                if (isInfoLogEnabled)
                {
                    Logs objLogs = new Logs
                    {
                        Description = $"Exception in SendFailureEmail method.",
                        ExceptionMessage = (ex.InnerException == null) ? ex.Message : ex.InnerException?.Message
                    };
                    int sucessStatus = objD.AddLogs(objLogs);
                }
            }
            //return success;
        }

    }

}