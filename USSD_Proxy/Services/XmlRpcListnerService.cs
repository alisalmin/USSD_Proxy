using Horizon.XmlRpc.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using USSD_Proxy.Modules;

namespace USSD_Proxy.Services
{
    public abstract class XmlRpcListenerService : XmlRpcHttpServerProtocol
    {
        public virtual void ProcessRequest(HttpListenerContext RequestContext)
        {
            try
            {
                new XmlRpcListener().ProcessRequest(RequestContext);
            }
            catch(Exception ex) {
                RequestContext.Response.StatusCode = 500;
                RequestContext.Response.StatusDescription = ex.Message;
            }
           /* try
            {
                IHttpRequest req = new XmlRpcListenerRequest(RequestContext.Request);
                IHttpResponse resp = new XmlRpcListenerResponse(RequestContext.Response);
                HandleHttpRequest(req, resp);
            }
            catch (Exception ex)
            {
                // "Internal server error"
                RequestContext.Response.StatusCode = 500;
                RequestContext.Response.StatusDescription = ex.Message;
            } */
            finally
            {
                RequestContext.Response.OutputStream.Close();
            }
        }
    }
}
