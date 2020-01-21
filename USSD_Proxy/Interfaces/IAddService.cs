using Horizon.XmlRpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using USSD_Proxy.Modules;
using static USSD_Proxy.Modules.XmlRpcListener;

namespace USSD_Proxy.Interfaces
{
    public interface IAddService
    { 
        [XmlRpcMethod("USSD_MESSAGE")]
        ussdreply ProcessUssdMsg(params ussdmessage[] Data); 
    }
}
