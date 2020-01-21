using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using USSD_Proxy.Models;
using USSD_Proxy.Modules;

namespace USSD_Proxy.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TNMServController : ControllerBase
    {
        private static IConfiguration _configuration;

        public TNMServController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult TNMDirect(string ussdrequest)
        {
            if (string.IsNullOrEmpty(ussdrequest))
            {
                string resp = "ENDInvalid parameters passed";
                return Ok(resp);
            }
            if (!ussdrequest.Contains('&'))
            {
                string resp = "ENDInvalid parameters passed";
                return Ok(resp);
            }
            string[] UssdData = ussdrequest.Split('&');
            UssdRequest ussdRequest = new UssdRequest();
            if (UssdData.Length < 4)
            {
                string resp = "ENDInvalid parameters passed";
                return Ok(resp);
            }
            else if(UssdData.Length < 6)
            {
                ussdRequest.sessionid = UssdData[0];
                ussdRequest.msisdn = UssdData[1];
                ussdRequest.shortcode = UssdData[2];
                ussdRequest.inputparams = UssdData[3]; 
            }
            else
            {
                ussdRequest.sessionid = UssdData[0];
                ussdRequest.msisdn = UssdData[1];
                ussdRequest.shortcode = UssdData[2];
                ussdRequest.inputparams = UssdData[3];
                ussdRequest.imsi = UssdData[4];
                ussdRequest.longtitude = UssdData[5];
                ussdRequest.latitude = UssdData[6];
            }
            ussdRequest.sessionid = UssdData[0];
            ussdRequest.msisdn = UssdData[1];
            ussdRequest.shortcode = UssdData[2];
            ussdRequest.inputparams = UssdData[3];
            ussdRequest.imsi = UssdData[4];
            ussdRequest.longtitude = UssdData[5];
            ussdRequest.latitude = UssdData[6]; 
            try
            {
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
                return Ok(ussdResultString);
            }
            catch(Exception ss)
            {
                WriteMessageLog("An error occured: " + ss.Message);
                return Ok("ENDSomething went wrong...");
            } 
        }

        private static object MsgLock = "Locked?";
        private static void WriteMessageLog(String log)
        {
            try
            { 
                lock (MsgLock)
                {
                    var contentRoot = _configuration.GetValue<string>(WebHostDefaults.ContentRootKey);
                    string LogPath = contentRoot + "\\Logs\\";
                    if (!Directory.Exists(LogPath))
                    {
                        Directory.CreateDirectory(LogPath);
                    }
                    string fileName = LogPath + "ussdevent_" + String.Format("{0:yyyy-MM-dd}", DateTime.Now).ToString() + ".log";
                    System.IO.TextWriter ErrHan = new System.IO.StreamWriter(fileName, true);
                    ErrHan.WriteLine(String.Format("{0:yyyy MMM dd:HH:mm:ss}:", DateTime.Now) + log);
                    ErrHan.Flush();
                    ErrHan.Close();
                }
            }
            catch { }
        }
    }
}