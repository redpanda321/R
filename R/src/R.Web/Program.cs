using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

using WebSocketSharp.Server;
using R.Web.Services;
using Microsoft.Extensions.Configuration;

namespace R.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {


            //Configuration
            var builder = new ConfigurationBuilder().SetBasePath(System.IO.Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfiguration configuration = builder.Build();

            //WebSocket Server
            var wssv = new WebSocketServer(configuration["ConnectionStrings:WebSocketServer:ConnectionString"]);
            wssv.AddWebSocketService<ResultBehavior>("/ResultBehavior");
            wssv.Start();

            if (wssv.IsListening)
            {
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", wssv.Port);
                foreach (var path in wssv.WebSocketServices.Paths)
                    Console.WriteLine("- {0}", path);
            }

            //Console
             Console.WriteLine("\nPress Enter key to stop the server...");
             Console.ReadLine();

            // MVC
           var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();

           

        }
    }
}
