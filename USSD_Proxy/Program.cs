using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using USSD_Proxy.Modules;
using USSD_Proxy.Services;

namespace USSD_Proxy
{
    public class Program
    {
        static volatile public string menuEndPoint; 
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                  .AddJsonFile("appsettings.json", optional: false)
                  .Build();
            menuEndPoint = config.GetSection("EndPoints").GetSection("MenuEndPoint").Value;
            string airtelEndpoint = config.GetSection("EndPoints").GetSection("AirtelEndPoint").Value;
            startAirtelListener(airtelEndpoint);
            BuildWebHost(args).Run(); 
        }

        private static void startAirtelListener(string Endpoint)
        {
            AirtelUssdServer airtelUssdServer = new AirtelUssdServer();
            Task.Delay(100).ContinueWith(t => airtelUssdServer.StartListener(Endpoint));
        }

        internal class AirtelUssdServer
        {
             
            private void Process(object TheContext)
            {
                HttpListenerContext requestContext = (HttpListenerContext)TheContext;
                try
                {
                    new XmlRpcListener().ProcessRequest(requestContext);
                }
                catch (Exception exception)
                {
                    
                }
            }

            public void StartListener(string endPoint)
            {
                try
                {
                    HttpListener listener = new HttpListener();
                    listener.Prefixes.Add(endPoint);
                    listener.Start();
                    while (true)
                    {
                        try
                        {
                            HttpListenerContext parameter = listener.GetContext();
                            new Thread(new ParameterizedThreadStart(this.Process)).Start(parameter);
                            continue;
                        }
                        catch (Exception exception)
                        { 
                            continue;
                        }
                    }
                }
                catch (Exception exception2)
                { 
                }
            }
        }

        
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseKestrel()
                .UseIISIntegration()
                .Build();
    }
}
