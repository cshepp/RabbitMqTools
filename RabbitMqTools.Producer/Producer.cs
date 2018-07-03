
using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMqTools.Core;
using RabbitMqTools.Core.RabbitMq;

namespace RabbitMqTools.Producer
{
    public class Producer
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IModel _channel;

        public Producer(IRabbitMqConnection connection)
        {
            _connection = connection;
            _channel = _connection.CreateNewChannel();
        }

        public void Produce(string exchange, int count)
        {
            _channel.ExchangeDeclare(exchange, "topic", true, false, null);

            for (var i = 0; i < count; i++)
            {
                var body = Guid.NewGuid().ToString();
                var bytes = Encoding.UTF8.GetBytes(body);
                _channel.BasicPublish(exchange, "", false, null, bytes);
                Logging.Log(new 
                {
                    timestamp = DateTime.Now.ToString("O"),
                    exchange = exchange,
                    message = body
                });
            }
        }
    }
}