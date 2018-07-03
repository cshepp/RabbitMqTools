
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Network.Fluent;
using RabbitMqTools.Core;

namespace RabbitMqTools.Conductor.Azure
{
    public class ApplicationNodeProvisioner 
    {
        private readonly VirtualMachineFactoryResult _vm;
        private readonly VirtualMachineHelper _vmHelper;
        private readonly VirtualMachineFactoryResult[] _rmqVms;

        public ApplicationNodeProvisioner(VirtualMachineFactoryResult vm, VirtualMachineFactoryResult[] rmqVms)  
        {
            _vm = vm;
            _vmHelper = new VirtualMachineHelper(vm);
            _rmqVms = rmqVms;
        }

        public async Task InstallAsync()
        {
            var installScript = await File.ReadAllTextAsync("Scripts\\install-appserver.sh");
            _vmHelper.UploadFileText(installScript, $"/home/{_vm.Username}/install-appserver.sh");

            var startScript = await File.ReadAllTextAsync("Scripts\\start-appserver.sh");
            _vmHelper.UploadFileText(startScript, $"/home/{_vm.Username}/start-appserver.sh");

            await _vmHelper.RunCommandOverSsh($"bash /home/{_vm.Username}/install-appserver.sh");

            // Choose a "random" hostname from the list
            // of RMQ nodes for this datacenter
            var hostname = _rmqVms
                .OrderBy(x => Guid.NewGuid())
                .Take(1)
                .Select(vm => vm.IpAddress.Fqdn)
                .Single();

            var replacementValues = new Dictionary<string, string>()
            {
                { "<RmqUserName>", Program.Config.RmqAdminUsername },
                { "<RmqPassword>", Program.Config.RmqAdminPassword },
                { "<RmqHostName>", hostname },
                { "<VmUserName>", _vm.Username }
            };

            var consumerConfig = await File.ReadAllTextAsync("Scripts\\consumer-config.toml");
            consumerConfig = Templater.Replace(consumerConfig, replacementValues);

            var producerConfig = await File.ReadAllTextAsync("Scripts\\producer-config.toml");
            producerConfig = Templater.Replace(producerConfig, replacementValues);

            _vmHelper.UploadFileText(consumerConfig, $"/home/{_vm.Username}/Consumer/consumer-config.toml");
            _vmHelper.UploadFileText(producerConfig, $"/home/{_vm.Username}/Producer/producer-config.toml");

            await _vmHelper.RunCommandOverSsh($"bash /home/{_vm.Username}/start-appserver.sh");
        }
    }
}