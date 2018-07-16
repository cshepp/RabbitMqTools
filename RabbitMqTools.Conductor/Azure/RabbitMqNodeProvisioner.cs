
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Network.Fluent;
using NLog;
using Renci.SshNet;
using Renci.SshNet.Common;
using RabbitMqTools.Core;
using RabbitMqTools.Core.Configuration;

namespace RabbitMqTools.Conductor.Azure
{
    public class RabbitMqNodeProvisioner
    {
        private readonly VirtualMachineFactoryResult _vm;
        private readonly VirtualMachineHelper _vmHelper;
        private readonly IConductorConfiguration _config;
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public string Fqdn => _vm.IpAddress.Fqdn;

        public RabbitMqNodeProvisioner(VirtualMachineFactoryResult vm, IConductorConfiguration config)
        {
            _vm = vm;
            _vmHelper = new VirtualMachineHelper(vm);
            _config = config;
        }

        public async Task InstallAsync()
        {
            Console.WriteLine($"Provisioning {_vm.VmName}");

            var installScript = await File.ReadAllTextAsync("Scripts\\install-rabbitmq.sh");
            installScript = Templater.Replace(installScript, new Dictionary<string, string>()
            {
                { "<Username>", _config.RmqAdminUsername },
                { "<Password>", _config.RmqAdminPassword }
            });
            _vmHelper.UploadFileText(installScript, $"/home/{_vm.Username}/install-rabbitmq.sh");

            await _vmHelper.RunCommandOverSsh("bash ~/install-rabbitmq.sh");
            Console.WriteLine($"Finished provisioning {_vm.VmName}");
        }

        public async Task ClusterAsync()
        {
            Console.WriteLine($"Running cluster script on {_vm.VmName}");
            _vmHelper.UploadFileText(File.ReadAllText("Scripts\\cluster-rabbitmq.sh"), $"/home/{_vm.Username}/cluster-rabbitmq.sh");
            await _vmHelper.RunCommandOverSsh("bash ~/cluster-rabbitmq.sh");
            Console.WriteLine($"Finished running cluster script on {_vm.VmName}");
        }

        public async Task FederateAsync(IEnumerable<string> upstreamFqdns)
        {
            var script = await File.ReadAllTextAsync("Scripts\\federate-rabbitmq.sh");
            var fqdnList = string.Join(", ", upstreamFqdns.Select(f => $"\"amqp://admin:G3733D@{f}\""));
            script = Templater.Replace(script, new Dictionary<string, string>() { { "<FQDN_LIST>", fqdnList } });
            _vmHelper.UploadFileText(script, $"/home/{_vm.Username}/federate-rabbitmq.sh");

            Console.WriteLine($"Running federation script on {_vm.VmName}");
            await _vmHelper.RunCommandOverSsh("bash ~/federate-rabbitmq.sh");
            Console.WriteLine($"Finished running federation script on {_vm.VmName}");
        }
    }
}