
using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shepp.RmqTest.Core;
using Shepp.RmqTest.Core.RabbitMq;

namespace Shepp.RmqTest.Consumer
{
    public class Consumer : IDisposable
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IModel _channel;

        public Consumer(IRabbitMqConnection connection)
        {
            _connection = connection;
            _channel = connection.CreateNewChannel();
        }

        public void Consume(string exchange)
        { 
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Logging.Log(new 
                {
                    timestamp = DateTime.Now.ToString("O"),
                    exchange = exchange,
                    message = message
                });

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            var queue = "Consumer-" + exchange;
            var isQueueExclusive = false;
            var isQueueAutoAck = false;
            var isDurable = true;
            var isAutoDelete = false;
            IDictionary<string, object> arguments = null;
            _channel.ExchangeDeclare(exchange, ExchangeType.Topic, isDurable, isAutoDelete, arguments);
            _channel.QueueDeclare(queue, isDurable, isQueueExclusive, isAutoDelete, arguments);
            _channel.QueueBind(queue, exchange, "#");
            _channel.BasicConsume(queue, isQueueAutoAck, consumer);
        }

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}