using System;
using System.Collections.Generic;
using RabbitMqTools.Conductor.Azure;
using System.Threading.Tasks;
using System.Linq;
using NLog;
using RabbitMqTools.Core.Configuration;

namespace RabbitMqTools.Conductor
{
    static class Program
    {
        public static IConductorConfiguration Config;

        static void Main(string[] args)
        {
            if (args.Length != 2 || args[0] != "--config")
            {
                throw new ArgumentException("Make sure you pass --config /path/to/config.toml");
            }

            var configFilePath = args[1];
            var config = ConductorConfiguration.Load(configFilePath);
            Config = config;
            Task.Run(Run).Wait();
        }

        static async Task Run()
        {
            SetupLogging();
            Exception ex = null;

            // TODO - construct this basic on user-provided config
            var datacenters = new List<Datacenter>
            {
                new Datacenter()
                {
                    ResourceGroupName = Program.Config.ResourceGroupName,
                    Name = "datacenter-1",
                    Number = 1,
                    RabbitMqNodes = new List<RabbitMqNode>()
                    {
                        new RabbitMqNode(){ Name = "rmq-11" },
                        new RabbitMqNode(){ Name = "rmq-12" },
                        new RabbitMqNode(){ Name = "rmq-13" },
                    },
                    ApplicationServer = new ApplicationServer() { Name = "app-11" }
                },
                new Datacenter()
                {
                    ResourceGroupName = Program.Config.ResourceGroupName,
                    Name = "datacenter-2",
                    Number = 2,
                    RabbitMqNodes = new List<RabbitMqNode>()
                    {
                        new RabbitMqNode(){ Name = "rmq-21" },
                        new RabbitMqNode(){ Name = "rmq-22" },
                        new RabbitMqNode(){ Name = "rmq-23" },
                    },
                    ApplicationServer = new ApplicationServer() { Name = "app-21" }
                }
            };

            try
            {
                var datacenterTasks = datacenters.Select(d => new DatacenterFactory().CreateAsync(d));
                var results = await Task.WhenAll(datacenterTasks);

                Console.WriteLine($"Creating connection from datacenter-1 <-> datacenter-2");
                var peering = await results[0].Item1.Peerings.Define(results[0].Item1.Name + "-peering")
                    .WithRemoteNetwork(results[1].Item1)
                    .CreateAsync();
                Console.WriteLine($"Finished creating connection from datacenter-1 <-> datacenter-2");

                var fqdns1 = results[0].Item2.Select(p => p.Fqdn);
                var fqdns2 = results[1].Item2.Select(p => p.Fqdn);

                await results[0].Item2.First().FederateAsync(fqdns2);
                await results[1].Item2.First().FederateAsync(fqdns1);

            }
            catch (Exception e)
            {
                ex = e;
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
                Console.WriteLine($"Deleting {Program.Config.ResourceGroupName}...");
                AzureFactory.GetAzure().ResourceGroups.DeleteByName(Program.Config.ResourceGroupName);

                if(ex != null)
                {
                    throw ex;
                }
            }
        }

        static void SetupLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "C:\\Temp\\rmqtest-log.txt" };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }
    }
}
