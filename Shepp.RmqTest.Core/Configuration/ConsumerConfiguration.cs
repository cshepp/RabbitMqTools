
using Nett;
using Shepp.RmqTest.Core.RabbitMq;

namespace Shepp.RmqTest.Core.Configuration
{
    public interface IConsumerConfiguration : IRabbitMqConnectionDetails, ILoggingConfiguration
    {
    }

    public class ConsumerConfiguration : IConsumerConfiguration
    {
        public string RmqHostName { get; set; }
        public string RmqUserName { get; set; }
        public string RmqPassword { get; set; }
        public string LogPath { get; set; }

        public static IConsumerConfiguration Load(string file)
        {
            return (IConsumerConfiguration)Toml.ReadFile<ConsumerConfiguration>(file);
        }
    }
}