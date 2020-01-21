
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace USSD_Proxy.Models
{
    public class UssdRequest
    {
        public string msisdn { get; set; }
        public string sessionid { get; set; }
        public string shortcode { get; set; }
        public string inputparams { get; set; }
        public string imsi { get; set; }
        public string longtitude { get; set; }
        public string latitude { get; set; }
    }
}
