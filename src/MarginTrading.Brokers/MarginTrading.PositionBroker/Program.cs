﻿using Microsoft.AspNetCore.Hosting;

namespace MarginTrading.PositionBroker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:5008")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}