
using Nett;
using Shepp.RmqTest.Core.RabbitMq;

namespace Shepp.RmqTest.Core.Configuration
{
    public interface IProducerConfiguration : IRabbitMqConnectionDetails, ILoggingConfiguration
    {

    }

    public class ProducerConfiguration : IProducerConfiguration
    {
        public string RmqHostName { get; set; }
        public string RmqUserName { get; set; }
        public string RmqPassword { get; set; }
        public string LogPath { get; set; }

        public static IProducerConfiguration Load(string file)
        {
            return (IProducerConfiguration)Toml.ReadFile<ProducerConfiguration>(file);
        }
    }
}