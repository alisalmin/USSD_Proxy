using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            Console.WriteLine("Attempting to start the listener..");
            AirtelUssdServer airtelUssdServer = new AirtelUssdServer();
            Task.Delay(100).ContinueWith(t => airtelUssdServer.StartListener(Endpoint));
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
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
                    Console.WriteLine("Error on processing request: " + exception.Message);
                }
            }

            public void StartListener(string endPoint)
            {
                try
                {
                    HttpListener listener = new HttpListener();
                    string Ip = GetLocalIPAddress();
                    string url = "http://" + Ip + ":9246/";
                    Console.WriteLine("Set IP to the local " + url);
                    listener.Prefixes.Add(url);
                    listener.Start();
                    Console.WriteLine("listener started on : " + url);
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
                            Console.WriteLine("Error 1 on starting listener: " + exception.Message);
                            continue;
                        }
                    }
                }
                catch (Exception exception2)
                {
                    Console.WriteLine("Error 2 on starting listener: " + exception2.Message);
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
