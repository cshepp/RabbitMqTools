
using Newtonsoft.Json;
using NLog;
using NLog.Layouts;

namespace Shepp.RmqTest.Core
{
    public interface ILoggingConfiguration
    { 
        string LogPath { get; set; }
    }

    public static class Logging
    {
        public static void Log(string message)
        {
            LogManager.GetCurrentClassLogger().Info(message);
        }

        public static void Log(object details)
        {
            var message = JsonConvert.SerializeObject(details);
            Log(message);
        }

        public static void SetupSimpleLogging(string filePath)
        { 
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = filePath,
                Layout = new SimpleLayout() { Text = "${message}" }
            };

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }
    }
}