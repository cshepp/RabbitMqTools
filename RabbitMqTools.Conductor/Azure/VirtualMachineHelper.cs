
using System;
using System.IO;
using System.Threading.Tasks;
using NLog;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace RabbitMqTools.Conductor.Azure
{
    public class VirtualMachineHelper
    {
        private readonly VirtualMachineFactoryResult _vm;
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public VirtualMachineHelper(VirtualMachineFactoryResult vm)
        {
            _vm = vm;
        }

        public async Task RunCommandOverSsh(string command)
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

        public void UploadFileText(string text, string destinationPath)
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

        private ConnectionInfo GetConnectionInfo()
        { 
            return new ConnectionInfo(_vm.IpAddress.Fqdn, 
                                      _vm.Username, 
                                      new PasswordAuthenticationMethod(_vm.Username, _vm.Password));
        }

        private void OnUploadProgress(object sender, ScpUploadEventArgs args)
        {
            Console.WriteLine($"Uploading {args.Filename} : {args.Uploaded} of {args.Size}");
        }
    }
}