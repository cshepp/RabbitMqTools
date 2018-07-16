using System;
using System.Collections.Generic;
using RabbitMqTools.Conductor.Azure;
using System.Threading.Tasks;
using System.Linq;
using NLog;
using RabbitMqTools.Core.Configuration;
using McMaster.Extensions.CommandLineUtils;
using RabbitMqTools.Conductor.Commands;

namespace RabbitMqTools.Conductor
{
    static class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.HelpOption();
            app.Command(InitializeCommand.Name, cmd => new InitializeCommand(cmd));
            app.Command(TeardownCommand.Name, cmd => new TeardownCommand(cmd));

            try
            {
                return app.Execute(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }

        public static void SetupLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "C:\\Temp\\rmqtest-log.txt" };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }
    }
}
