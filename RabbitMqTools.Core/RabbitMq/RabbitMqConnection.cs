
using System;
using RabbitMQ.Client;

namespace RabbitMqTools.Core.RabbitMq
{
    public interface IRabbitMqConnection : IDisposable
    {
        IModel CreateNewChannel();
    }

    // A single tcp connection to RabbitMq.
    // Should live for the lifetime of the application.
    // Should be fine to share between threads.
    public class RabbitMqConnection : IRabbitMqConnection
    {
        private readonly IConnectionFactory _factory;
        private readonly IConnection _connection;

        public RabbitMqConnection(IRabbitMqConnectionDetails details)
        {
            _factory = new ConnectionFactory()
            {
                HostName = details.RmqHostName,
                Port = 5672,
                UserName = details.RmqUserName,
                Password = details.RmqPassword,
                VirtualHost = "/",
                AutomaticRecoveryEnabled = true,
            };

            _connection = _factory.CreateConnection();
        }

        // Do not share between threads.
        public IModel CreateNewChannel()
        {
            return _connection.CreateModel();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }

    public interface IRabbitMqConnectionDetails
    {
        string RmqHostName { get; set; }
        string RmqUserName { get; set; }
        string RmqPassword { get; set; }
}
}