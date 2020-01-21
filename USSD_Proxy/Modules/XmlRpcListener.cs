using Horizon.XmlRpc.Core;
using Horizon.XmlRpc.Server;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using USSD_Proxy.Models;

namespace USSD_Proxy.Modules
{
    public class XmlRpcListener : XmlRpcListenerService
    {
        public XmlRpcListener()
        {

        }

        [XmlRpcMethod("USSD_MESSAGE")]
        public ussdreply ProcessUssdMsg(params ussdmessage[] Data)
        {
            XmlRpcListener.ussdreply ussdreply = new XmlRpcListener.ussdreply();
            try
            {
                WriteMessageLog("Incoming request: " + JsonConvert.SerializeObject(Data[0]));
                string resp = ForwardRequest(Data[0]);
                
                WriteMessageLog("Outgoing response: " + resp);
                if (resp.StartsWith("CON"))
                {
                    ussdreply.END_OF_SESSION = "False";
                    ussdreply.REQUEST_TYPE = "RESPONSE";
                    ussdreply.SESSION_ID = Data[0].SESSION_ID;
                    ussdreply.SEQUENCE = Data[0].SEQUENCE;
                    ussdreply.USSD_BODY = resp.Substring(3);
                }
                else
                {
                    ussdreply.END_OF_SESSION = "True";
                    ussdreply.REQUEST_TYPE = "RESPONSE";
                    ussdreply.SESSION_ID = Data[0].SESSION_ID;
                    ussdreply.SEQUENCE = Data[0].SEQUENCE;
                    ussdreply.USSD_BODY = resp.Substring(3);
                } 
                
            }
            catch(Exception ss) { WriteMessageLog("An err " + ss.Message); }
            return ussdreply;
        }

        [StructLayout(LayoutKind.Sequential), XmlRpcMissingMapping(MappingAction.Ignore)]
        public struct ussdmessage
        {
            public string SERVICE_KEY;
            public string REQUEST_TYPE;
            public string LANGUAGE;
            public string SEQUENCE;
            public string END_OF_SESSION;
            public string MOBILE_NUMBER;
            public string SESSION_ID;
            public string USSD_BODY;
            //public string IMSI;
        }

        [StructLayout(LayoutKind.Sequential), XmlRpcMissingMapping(MappingAction.Ignore)]
        public struct ussdreply
        {
            public string SERVICE_KEY;
            public string REQUEST_TYPE;
            public string LANGUAGE;
            public string SEQUENCE;
            public string END_OF_SESSION;
            public string MOBILE_NUMBER;
            public string SESSION_ID;
            public string USSD_BODY;
            public string COST;
        }

        private static object MsgLock = "Locked?";
        private static void WriteMessageLog(String log)
        {
            try
            {
                lock (MsgLock)
                {
                    string LogPath = @".\Logs\";
                    string fileName = LogPath + "ussd.log" + String.Format("{0:yyyy-MM-dd}", DateTime.Now).ToString(); ;
                    System.IO.TextWriter ErrHan = new System.IO.StreamWriter(fileName, true);
                    ErrHan.WriteLine(String.Format("{0:yyyy MMM dd:HH:mm:ss}:", DateTime.Now) + log);
                    ErrHan.Flush();
                    ErrHan.Close();
                }
            }
            catch { }
        }

        private static string ForwardRequest(ussdmessage ussdmessage)
        {
            UssdRequest ussdRequest = new UssdRequest();
            ussdRequest.msisdn = ussdmessage.MOBILE_NUMBER;
            ussdRequest.sessionid = ussdmessage.SESSION_ID;
            ussdRequest.shortcode = ussdmessage.SERVICE_KEY;
            ussdRequest.inputparams = ussdmessage.USSD_BODY;
            string returnText = string.Empty;
            try
            {
                WriteMessageLog("Forwarding ussd message " + JsonConvert.SerializeObject(ussdRequest) + "\n Endpoint " + Program.menuEndPoint);
                HttpWebRequest ussdAPIRequest = (HttpWebRequest)WebRequest.Create(Program.menuEndPoint); // edit and put an endpoint
                ussdAPIRequest.Method = "POST";
                ussdAPIRequest.ContentType = "application/json";
                byte[] requestBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ussdRequest));
                ussdAPIRequest.ContentLength = requestBytes.Length;
                Stream requestStream = ussdAPIRequest.GetRequestStream();
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                requestStream.Close();
                WebResponse response = ussdAPIRequest.GetResponse();
                string statusDescription = ((HttpWebResponse)response).StatusDescription;
                requestStream = response.GetResponseStream();
                StreamReader streamReader2 = new StreamReader(requestStream);
                string ussdResultString = streamReader2.ReadToEnd();
                WriteMessageLog("Response from menu server: " + ussdResultString);
                return ussdResultString;
            }
            catch(Exception se)
            {
                WriteMessageLog("An error forwarding airtel request: " + se.Message);
                return "ENDSomething went wrong";
            }
        }

    }
}
