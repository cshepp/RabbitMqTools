
using System;
using System.Net;
using Shepp.RmqTest.Core.Http;
using Shepp.RmqTest.Core.RabbitMq;

namespace Shepp.RmqTest.Producer
{
    public class ProducerHttpServer : HttpServer
    {
        private readonly IServiceProvider _container;

        public ProducerHttpServer(IServiceProvider container)
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
                case("favicon.ico"):
                    context.Ok();
                    break;
                case("Shutdown"):
                    context.Ok();
                    Shutdown();
                    break;
                case("Produce"):
                    HandleProduceRequest(parts, context);
                    break;
                default:
                    context.Error("Route does not exist");
                    break;
            }
        }

        protected override void HandleShutdown()
        {
            var connection = (IRabbitMqConnection) _container.GetService(typeof(IRabbitMqConnection));
            connection.Dispose();
        }

        private void HandleProduceRequest(string[] parts, HttpListenerContext context)
        {
            if(!ValidateProduceRequest(parts, context)) return;

            var exchange = parts[2];
            var count = int.Parse(parts[3]);
            var producer = (Producer)_container.GetService(typeof(Producer));
            producer.Produce(exchange, count);
            context.Ok();
        }

        private static bool ValidateProduceRequest(string[] parts, HttpListenerContext context)
        {
            if (parts.Length != 4) 
            {
                context.Error("format is /Produce/{exchangeName}/{count}");
                return false;
            }

            return true;
        }
    }
}