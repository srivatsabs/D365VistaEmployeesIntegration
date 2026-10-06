using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Permissions;
using System.Configuration;
using System.Security.AccessControl;
using System.Net.Mail;
using System.IO;
using System.Web;

namespace D365VistaEmployeesIntegration
{
    internal class SQLData
    {

        bool isInfoLogEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["LogInfo"].ToString());
        bool isProd = Convert.ToBoolean(ConfigurationManager.AppSettings["Prod"].ToString());

        //"Server=Shankara_Home;Database=Delete;TrustServerCertificate=True;MultipleActiveResultSets=true;Integrated Security=true";
        private string connectionString = ConfigurationManager.ConnectionStrings["ExpenseConnectionString"].ToString();
        public List<EmployeeDetails> GetEmployeeRecords()
        {

            if (isInfoLogEnabled)
            {
                Logs objLogs = new Logs
                {
                    Description = $"Entered GetEmployeeRecords method.",
                    ExceptionMessage = ""
                };
                int sucessStatus = AddLogs(objLogs);
            }
            string logoHeader = string.Empty;
            string procedureName = "hrpr.usp_GetVistaEmployees_D365_PrePROD";
            if (isProd)
            {
                procedureName = "hrpr.usp_GetVistaEmployees_D365";
            }

            var records = new List<EmployeeDetails>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(procedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 600;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (reader["ProcessedDate"] == DBNull.Value)
                                    {
                                        var record = new EmployeeDetails
                                        {

                                            PersonalTitle = reader["PersonalTitle"] != DBNull.Value ? reader["PersonalTitle"].ToString() : "",
                                            FirstName = reader["FirstName"] != DBNull.Value ? reader["FirstName"].ToString() : "",
                                            LastName = reader["LastName"] != DBNull.Value ? reader["LastName"].ToString() : "",
                                            MiddleName = reader["MiddleName"] != DBNull.Value ? reader["MiddleName"].ToString() : "",
                                            AnniversaryDateTime = ToD365Date(reader["AnniversaryDateTime"]),
                                            PrimaryContactPhone = reader["PrimaryContactPhone"] != DBNull.Value ? reader["PrimaryContactPhone"].ToString() : "",
                                            // EmployeeId
                                            PersonnelNumber = reader["PersonnelNumber"] != DBNull.Value ? reader["PersonnelNumber"].ToString() : "",
                                            // EmployeeId
                                            //PositionId = reader["PositionId"] != DBNull.Value ? reader["PositionId"].ToString() : "",
                                            ParentPositionId = reader["ParentPositionId"] != DBNull.Value ? reader["ParentPositionId"].ToString() : "",
                                            JobId = reader["JobId"] != DBNull.Value ? reader["JobId"].ToString() : "",
                                            Department = reader["Department"] != DBNull.Value ? reader["Department"].ToString() : "",
                                            // HireDate
                                            ValidFrom = ToD365Date(reader["ValidFrom"]),        // NA
                                            PersonalSuffix = reader["PersonalSuffix"] != DBNull.Value ? reader["PersonalSuffix"].ToString() : "",
                                            // Address
                                            AddressCity = reader["AddressCity"] != DBNull.Value ? reader["AddressCity"].ToString() : "",
                                            LanguageId = reader["LanguageId"] != DBNull.Value ? reader["LanguageId"].ToString() : "US",
                                            // FirstName + LastName
                                            KnownAs = reader["KnownAs"] != DBNull.Value ? reader["KnownAs"].ToString() : "",
                                            // Company ID
                                            LegalEntity = reader["LegalEntity"] != DBNull.Value ? reader["LegalEntity"].ToString() : "",
                                            PositionWorkerAssignment_ValidFrom = ToD365OldDate(reader["PositionWorkerAssignment_ValidFrom"]), //ToD365Date(reader["PositionWorkerAssignment_ValidFrom"]),
                                            PrimaryContactEmail = reader["PrimaryContactEmail"] != DBNull.Value ? reader["PrimaryContactEmail"].ToString() : "",
                                            // 2154-12-31T23:59:59Z
                                            ValidTo = ToD365Date(reader["ValidTo"]),
                                            AddressStreet = reader["AddressStreet"] != DBNull.Value ? reader["AddressStreet"].ToString() : "",
                                            PositionWorkerAssignment_ValidTo = ToD365OldDate(reader["PositionWorkerAssignment_ValidTo"]),// ToD365Date(reader["PositionWorkerAssignment_ValidTo"]),
                                            AddressCountryRegionISOCode = reader["AddressCountryRegionId"] != DBNull.Value ? reader["AddressCountryRegionId"].ToString() : "US",
                                            // Empty
                                            WorkerType = MapWorkerType(reader["WorkerType"]),
                                            AddressState = reader["AddressState"] != DBNull.Value ? reader["AddressState"].ToString() : "",
                                            HourlyBasis = reader["IsHourly"] != DBNull.Value && Convert.ToBoolean(reader["IsHourly"]) ? "Yes" : "No",
                                            Active = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]) ? "Yes" : "No"


                                        };
                                        records.Add(record);
                                    }
                                }
                            }
                        }

                    }
                    return records;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private static string MapWorkerType(object dbValue)
        {
            if (dbValue == DBNull.Value) return "Contractor";

            return dbValue.ToString().Trim().Equals("Y", StringComparison.OrdinalIgnoreCase)
                ? "Employee"
                : "Contractor";
        }
        private string ToD365Date(object dbValue)
        {
            if (dbValue == DBNull.Value || dbValue == null)
                return "2154-12-31T23:59:59Z"; // D365 prefers null over MinValue

            return Convert.ToDateTime(dbValue)
                .ToUniversalTime()
                .ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        }
        private string ToD365OldDate(object dbValue)
        {
            if (dbValue == DBNull.Value || dbValue == null)
                return "1900-01-01T00:00:00Z"; // D365 prefers null over MinValue

            return Convert.ToDateTime(dbValue)
                .ToUniversalTime()
                .ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        }

        public int AddProceddedExpenseHeader(string empId)
        {
            int result = 0;
            string procedureName = "hrpr.usp_UpdateVistaEmployees_D365_PrePROD";
            if (isProd)
            {
                procedureName = "hrpr.usp_UpdateVistaEmployees_D365";
            }
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(procedureName, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        // Input parameter
                        cmd.Parameters.Add("@PersonnelNumber", SqlDbType.NVarChar, 100).Value = empId;
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                            result = 1;   // Success
                        else
                            result = 0;   // No record found
                    }
                }
            }
            catch (Exception ex)
            {
                if (isInfoLogEnabled)
                {
                    Logs objLogs = new Logs
                    {
                        Description = $"Exception in usp_UpdateVistaEmployees_D365 for PersonnelNumber {empId}",
                        ExceptionMessage = ex.Message
                    };

                    AddLogs(objLogs);
                }

                result = -1;  // Failure
            }
            return result;
        }

        public int AddLogs(Logs objLogs)
        {
            int result = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_ExpenseHeadersLogs", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Description", objLogs.Description);
                    cmd.Parameters.AddWithValue("@ExceptionMessage", objLogs.ExceptionMessage);

                    con.Open();
                    object returnValue = cmd.ExecuteScalar();
                    con.Close();
                    result = 1;
                    int newRecordId = Convert.ToInt32(returnValue);
                }
            }
            catch (Exception ex)
            {
                result = -1;
            }
            return result;
        }
    }
}
