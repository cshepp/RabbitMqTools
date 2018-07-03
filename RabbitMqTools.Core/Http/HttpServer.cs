
using System;
using System.Net;
using System.Net.Http;

namespace RabbitMqTools.Core.Http
{

    public abstract class HttpServer
    {
        private bool _process = true;

        protected abstract void HandleRequest(HttpListenerContext context);

        protected abstract void HandleShutdown();

        public void Start(int port)
        {
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://+:{port}/");
            httpListener.Start();

            while (_process)
            {
                HttpListenerContext context = null;
                try
                {
                    context = httpListener.GetContext();
                    HandleRequest(context);
                }
                catch (Exception e)
                {
                    context?.Error(e.Message);
                }
            }

            HandleShutdown();
        }

        public void Shutdown()
        {
            _process = false;
        }
    }
}