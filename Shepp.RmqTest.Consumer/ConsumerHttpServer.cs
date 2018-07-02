
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Abstractions;
using Shepp.RmqTest.Core.Http;
using Shepp.RmqTest.Core.RabbitMq;

namespace Shepp.RmqTest.Consumer
{
    public class ConsumerHttpServer : HttpServer
    {
        private readonly IServiceProvider _container;
        private Dictionary<string, Consumer> _consumers = new Dictionary<string, Consumer>();

        public ConsumerHttpServer(IServiceProvider container)
        {
            _container = container;
        }

        protected override void HandleRequest(HttpListenerContext context)
        {
            var path = context.Request.Url.AbsolutePath;
            var parts = path.Split('/');

            if (parts.Length == 1)
            {
                context.Ok();
                return;
            }

            switch (parts[1])
            {
                case ("favicon.ico"):
                    context.Ok();
                    break;
                case ("ConsumeStart"):
                    HandleConsumeStart(parts, context);
                    break;
                case ("ConsumeStop"):
                    HandleConsumeStop(parts, context);
                    break;
                case ("Shutdown"):
                    context.Ok();
                    Shutdown();
                    break;
                default:
                    context.Error("Route does not exist");
                    break;
            }
        }

        protected override void HandleShutdown()
        {
            foreach (var consumer in _consumers)
            {
                consumer.Value.Dispose();
            }

            var connection = (IRabbitMqConnection)_container.GetService(typeof(IRabbitMqConnection));
            connection.Dispose();
        }

        private void HandleConsumeStart(string[] parts, HttpListenerContext context)
        {
            if (!ValidateConsumeRequest(parts, context)) return;

            var exchange = parts[2];
            var consumer = (Consumer)_container.GetService(typeof(Consumer));
            _consumers.Add(exchange, consumer);
            consumer.Consume(exchange);

            context.Ok();
        }

        private void HandleConsumeStop(string[] parts, HttpListenerContext context)
        { 
            if(!ValidateConsumeRequest(parts, context)) return;

            var exchange = parts[2];
            Consumer consumer;
            if (_consumers.TryGetValue(exchange, out consumer))
            {
                consumer.Dispose();
                _consumers.Remove(exchange);
                context.Ok();
                return;
            }

            context.Error($"No consumer for exchange {exchange} was found");
        }

        private static bool ValidateConsumeRequest(string[] parts, HttpListenerContext context)
        {
            if (parts.Length != 3)
            {
                context.Error("No exchange specified");
                return false;
            }

            return true;
        }

    }
}