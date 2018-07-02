
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
using Shepp.RmqTest.Core;

namespace Shepp.RmqTest.Conductor.Azure
{
    public class RabbitMqNodeProvisioner
    {
        private readonly VirtualMachineFactoryResult _vm;
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public string Fqdn => _vm.IpAddress.Fqdn;

        public RabbitMqNodeProvisioner(VirtualMachineFactoryResult vm)
        {
            _vm = vm;
        }

        public async Task InstallAsync()
        {
            Console.WriteLine($"Provisioning {_vm.VmName}");

            var installScript = File.ReadAllText("\\Scripts\\install-rabbitmq.sh");
            installScript = Templater.Replace(installScript, new Dictionary<string, string>()
            {
                { "<Username>", Program.Config.RmqAdminUsername },
                { "<Password>", Program.Config.RmqAdminPassword }
            });
            UploadFileText(installScript, $"/home/{_vm.Username}/install-rabbitmq.sh");

            await RunCommandOverSsh("bash ~/install-rabbitmq.sh");
            Console.WriteLine($"Finished provisioning {_vm.VmName}");
        }

        public async Task ClusterAsync()
        {
            Console.WriteLine($"Running cluster script on {_vm.VmName}");
            UploadFileText(File.ReadAllText("Scripts\\cluster-rabbitmq.sh"), $"/home/{_vm.Username}/cluster-rabbitmq.sh");
            await RunCommandOverSsh("bash ~/cluster-rabbitmq.sh");
            Console.WriteLine($"Finished running cluster script on {_vm.VmName}");
        }

        public async Task FederateAsync(IEnumerable<string> upstreamFqdns)
        {
            var script = File.ReadAllText("Scripts\\federate-rabbitmq.sh");
            var fqdnList = string.Join(", ", upstreamFqdns.Select(f => $"\"amqp://admin:G3733D@{f}\""));
            script = Templater.Replace(script, new Dictionary<string, string>() { { "<FQDN_LIST>", fqdnList } });
            UploadFileText(script, $"/home/{_vm.Username}/federate-rabbitmq.sh");

            Console.WriteLine($"Running federation script on {_vm.VmName}");
            await RunCommandOverSsh("bash ~/federate-rabbitmq.sh");
            Console.WriteLine($"Finished running federation script on {_vm.VmName}");
        }

        private void OnUploadProgress(object sender, ScpUploadEventArgs args)
        {
            Console.WriteLine($"Uploading {args.Filename} : {args.Uploaded} of {args.Size}");
        }

        private ConnectionInfo GetConnectionInfo()
        { 
            return new ConnectionInfo(_vm.IpAddress.Fqdn, 
                                      _vm.Username, 
                                      new PasswordAuthenticationMethod(_vm.Username, _vm.Password));
        }

        private async Task RunCommandOverSsh(string command)
        { 
            using (var client = new SshClient(GetConnectionInfo()))
            {
                Console.WriteLine($"Connecting to {_vm.IpAddress.IPAddress} via ssh");

                client.Connect();

                await Task.Run(async () =>
                {
                    var result = client.RunCommand(command);
                    var outputReader = new StreamReader(result.OutputStream);
                    var output = await outputReader.ReadToEndAsync();

                    _logger.Info(output);
                    _logger.Error(result.Error);

                    Console.WriteLine($"Disconnecting from {_vm.IpAddress.IPAddress}");
                }).ConfigureAwait(true);

                client.Disconnect();
            }
        }

        private void UploadFileText(string text, string destinationPath)
        { 
            var connectionInfo = GetConnectionInfo();
            using (var scp = new ScpClient(connectionInfo))
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(text);
                writer.Flush();
                stream.Position = 0;

                scp.Connect();
                scp.Uploading += OnUploadProgress;
                scp.Upload(stream, destinationPath);
                scp.Disconnect();
            }
        }
    }
}