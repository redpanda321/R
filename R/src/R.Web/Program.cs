﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

using WebSocketSharp.Server;
using R.Web.Services;

namespace R.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {

            //WebSocket Server
            var wssv = new WebSocketServer("ws://192.168.0.13:51151");
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
