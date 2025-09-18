using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEN.TextNotificationEN
{    
    public class RequestParameters
    {
        public long msisdn { get; set; }
        public string body_sms { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public Organization organization { get; set; }
        public string type { get; set; }
        public string protocol { get; set; }
        public bool billed { get; set; }
        public bool control { get; set; }
        public int flow { get; set; }
        public int priority { get; set; }
        public int status { get; set; }
        public int weight { get; set; }
        public string shortcodeId { get; set; }
        public string shortcodeType { get; set; }
        public string msisdn { get; set; }
        public string body { get; set; }
        public bool scheduled { get; set; }
        public string scheduledStart { get; set; }
        public string createdBy { get; set; }
        public string createdDate { get; set; }
    }

    public class Organization
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class ResponseParameters
    {
        public int codigo { get; set; }
        public string mensaje { get; set; }
        public Data data { get; set; }
    }    
}
