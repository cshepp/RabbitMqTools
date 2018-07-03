
using System.Net;
using System.Text;

namespace RabbitMqTools.Core.Http
{
    public static class HttpExtensions
    {
        public static void Ok(this HttpListenerContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.Close();
        }

        public static void Error(this HttpListenerContext context, string message)
        { 
            context.Response.StatusCode = 400;
            var bytes = Encoding.UTF8.GetBytes(message);
            context.Response.Close(bytes, false);
        }
    }
}