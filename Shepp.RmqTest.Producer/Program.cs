using System;
using Microsoft.Extensions.DependencyInjection;
using Shepp.RmqTest.Core;
using Shepp.RmqTest.Core.Configuration;
using Shepp.RmqTest.Core.RabbitMq;

namespace Shepp.RmqTest.Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2 || args[0] != "--config")
            {
                throw new ArgumentException("Make sure you pass --config /path/to/config.toml");
            }

            var configFilePath = args[1];
            var config = ProducerConfiguration.Load(configFilePath);
            Logging.SetupSimpleLogging(config.LogPath);

            var containerBuilder = new ServiceCollection();
            containerBuilder.AddSingleton<IProducerConfiguration>(config);
            containerBuilder.AddSingleton<IRabbitMqConnectionDetails>(config);
            containerBuilder.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
            containerBuilder.AddTransient<Producer>();

            var container = (IServiceProvider)containerBuilder.BuildServiceProvider();

            var server = new ProducerHttpServer(container);
            server.Start(46002);
        }
    }
}
