
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using RabbitMqTools.Conductor.Azure;
using RabbitMqTools.Core;
using RabbitMqTools.Core.Configuration;

namespace RabbitMqTools.Conductor.Commands
{
    public class InitializeCommand
    {
        public static string Name => "init";

        public InitializeCommand(CommandLineApplication command)
        {
            var configPath = command.Option("-c|--config <PATH>", "Required. The path to a config file", CommandOptionType.SingleValue)
                .IsRequired();

            command.OnExecute(async () => await OnExecute(configPath.Value()));
        }

        public async Task OnExecute(string path) 
        {
            Program.SetupLogging();

            var config = ConductorConfiguration.Load(path);

            var datacenterTasks = config.Datacenters.Select(d => new DatacenterFactory(config).CreateAsync(d));
            var results = await Task.WhenAll(datacenterTasks);

            Console.WriteLine($"Creating connection between datacenter-1 and datacenter-2");
            var peering = await results[0].Item1.Peerings.Define(results[0].Item1.Name + "-peering")
                .WithRemoteNetwork(results[1].Item1)
                .CreateAsync();
            Console.WriteLine($"Finished creating connection between datacenter-1 and datacenter-2");

            var fqdns1 = results[0].Item2.Select(p => p.Fqdn);
            var fqdns2 = results[1].Item2.Select(p => p.Fqdn);

            await results[0].Item2.First().FederateAsync(fqdns2);
            await results[1].Item2.First().FederateAsync(fqdns1);

            Console.WriteLine("Initialization Complete");
        }

    }
}