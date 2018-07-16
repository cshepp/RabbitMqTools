
using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using RabbitMqTools.Conductor.Azure;
using RabbitMqTools.Core.Configuration;

namespace RabbitMqTools.Conductor.Commands
{
    public class TeardownCommand
    {
        public static string Name => "teardown";

        public TeardownCommand(CommandLineApplication command)
        { 
            var configPath = command.Option("-c|--config <PATH>", "Required. The path to a config file", CommandOptionType.SingleValue)
                .IsRequired();

            command.OnExecute(() => OnExecute(configPath.Value()));
        }

        public void OnExecute(string path)
        { 
            var config = ConductorConfiguration.Load(path);
            Console.WriteLine($"Deleting {config.ResourceGroupName}...");
            AzureFactory.GetAzure().ResourceGroups.DeleteByName(config.ResourceGroupName);
        }
    }
}