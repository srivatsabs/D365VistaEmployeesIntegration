using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace D365VistaEmployeesIntegration
{
    [DataContract]
    public class TokenDC
    {
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string expires_in { get; set; }
        [DataMember]
        public string ext_expires_in { get; set; }
        [DataMember]
        public string expires_on { get; set; }
        [DataMember]
        public string not_before { get; set; }
        [DataMember]
        public string resource { get; set; }
        [DataMember]
        public string access_token { get; set; }
    }
}
