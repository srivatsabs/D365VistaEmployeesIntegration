using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D365VistaEmployeesIntegration
{
    public class EmployeeDetails
    {
        public string PersonalTitle { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string AnniversaryDateTime { get; set; }

        public string PrimaryContactPhone { get; set; }

        // EmployeeId
        public string PersonnelNumber { get; set; }

        // EmployeeId
        //public string PositionId { get; set; }

        public string ParentPositionId { get; set; }

        public string JobId { get; set; }

        public string Department { get; set; }

        // HireDate
        public string ValidFrom { get; set; }

        // NA
        public string PersonalSuffix { get; set; }

        // Address
        public string AddressCity { get; set; }

        public string LanguageId { get; set; }

        // FirstName + LastName
        public string KnownAs { get; set; }

        // Company ID
        public string LegalEntity { get; set; }

        public string PositionWorkerAssignment_ValidFrom { get; set; }

        public string PrimaryContactEmail { get; set; }

        // 2154-12-31T23:59:59Z
        public string ValidTo { get; set; }

        public string AddressStreet { get; set; }

        public string PositionWorkerAssignment_ValidTo { get; set; }

        public string AddressCountryRegionISOCode { get; set; }

        // Empty
        public string WorkerType { get; set; }

        public string AddressState { get; set; }
        public string HourlyBasis { get; set; }
        public string Active { get; set; }
    }
}
