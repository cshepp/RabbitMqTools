using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nett;
using NLog;
using NLog.Layouts;
using Shepp.RmqTest.Core;
using Shepp.RmqTest.Core.Configuration;
using Shepp.RmqTest.Core.RabbitMq;

namespace Shepp.RmqTest.Consumer
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
            var config = ConsumerConfiguration.Load(configFilePath);
            Logging.SetupSimpleLogging(config.LogPath);

            var containerBuilder = new ServiceCollection();
            containerBuilder.AddSingleton<IConsumerConfiguration>(config);
            containerBuilder.AddSingleton<IRabbitMqConnectionDetails>(config);
            containerBuilder.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
            containerBuilder.AddTransient<Consumer>();

            var container = (IServiceProvider)containerBuilder.BuildServiceProvider();

            var server = new ConsumerHttpServer(container);
            server.Start(46001);
        }
    }
}
