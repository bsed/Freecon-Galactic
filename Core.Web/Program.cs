using AutoMapper;
using Nancy.Hosting.Self;
using System;
using Server.Models;
using Freecon.Core.Utils;

namespace Core.Web
{
    class Program
    {
        static void Main(string[] args)
        {           
            ConsoleMover.SetConsolePosition(675, 0);

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<ColonyModel, Colony_HoldingsVM>();
            });

            var config = new NancyConfig();
            
            HostConfiguration hsc = new HostConfiguration();
            hsc.UrlReservations.CreateAutomatically = true;

            var host = new NancyHost(hsc, new Uri("http://localhost:" + config.Port.ToString()));
            
            host.Start();

            Console.WriteLine("NancyHost Succesfully Initialized.");

            
            while(true)
            {
                string c = Console.ReadLine();             

                if(c.ToLower() == "exit")
                    return;
            
            }
        }

    }
}
