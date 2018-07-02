

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Shepp.RmqTest.Core;

namespace Shepp.RmqTest.Conductor.Azure
{
    public class VirtualMachineFactory
    {
        private readonly string _vmName;

        public VirtualMachineFactory(string name)
        {
            _vmName = name;
        }

        public async Task<VirtualMachineFactoryResult> CreateAsync(INetwork network)
        {
            var vmUsername = Program.Config.VmUsername;
            var vmPassword = Program.Config.VmPassword;
            var vmDnsTag = Program.Config.DnsPrefix + _vmName;

            Console.WriteLine($"Creating VM {_vmName}");

            var vm = await Conductor.Azure.AzureFactory.GetAzure().VirtualMachines.Define(_vmName)
                .WithRegion(network.Region)
                .WithExistingResourceGroup(network.ResourceGroupName)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet(network.Subnets.Values.First().Name)
                .WithPrimaryPrivateIPAddressDynamic()
                .WithNewPrimaryPublicIPAddress(vmDnsTag)
                .WithPopularLinuxImage(KnownLinuxVirtualMachineImage.UbuntuServer16_04_Lts)
                .WithRootUsername(vmUsername)
                .WithRootPassword(vmPassword)
                .WithSize(VirtualMachineSizeTypes.StandardA1v2) // ~15 cents an hour per vm (2 Cores, 7GB RAM)
                .CreateAsync();

            Console.WriteLine($"Finished creating VM {_vmName}");

            Console.WriteLine($"Starting VM {_vmName}");
            vm.Start();
            Console.WriteLine($"{_vmName} has been started");

            return new VirtualMachineFactoryResult
            {
                IpAddress = vm.GetPrimaryPublicIPAddress(),
                Username = vmUsername,
                Password = vmPassword,
                VmName = _vmName,
            };

        }
    }

    public class VirtualMachineFactoryResult
    { 
        public IPublicIPAddress IpAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VmName { get; set; }
    }
}