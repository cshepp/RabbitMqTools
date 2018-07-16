
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using RabbitMqTools.Core;
using RabbitMqTools.Core.Configuration;

namespace RabbitMqTools.Conductor.Azure
{
    public class DatacenterFactory
    {
        private readonly IConductorConfiguration _config;

        public DatacenterFactory(IConductorConfiguration config)
        {
            _config = config;
        }

        public async Task<Tuple<INetwork, IEnumerable<RabbitMqNodeProvisioner>>> CreateAsync(Datacenter datacenter)
        {
            var vnetName = datacenter.Name + "-vnet";
            var vnetRegion = Region.USEast;
            var vnetResourceGroup = _config.ResourceGroupName;
            var vnetSubnetName = datacenter.Name + "-subnet";

            Console.WriteLine($"Creating VNet {vnetName}");
            var virtualNetwork = await Conductor.Azure.AzureFactory.GetAzure().Networks.Define(vnetName)
                .WithRegion(vnetRegion)
                .WithNewResourceGroup(vnetResourceGroup)
                .WithAddressSpace($"10.{datacenter.Number}1.0.0/16")                // 10.11.0.1   - 10.11.255.254
                .WithSubnet(vnetSubnetName, $"10.{datacenter.Number}1.0.0/24")      // 10.11.0.1   - 10.11.0.254
                .CreateAsync();
            Console.WriteLine($"Finished creating VNet {vnetName}");

            var createVmTasks = datacenter.RabbitMqNodes
                .Select(n => new VirtualMachineFactory(n.Name, _config).CreateAsync(virtualNetwork));

            var createVmTaskResults = await Task.WhenAll(createVmTasks);

            var provisioners = createVmTaskResults.Select(r => new RabbitMqNodeProvisioner(r, _config));

            var installTasks = provisioners
                .Select(f => f.InstallAsync());

            await Task.WhenAll(installTasks);

            // Clustering should happen one machine at a time
            // because the non-masters expect the master to be up.
            foreach (var p in provisioners)
            {
                await p.ClusterAsync();
            }
            
            //Provision App server
            var appVm = await new VirtualMachineFactory(datacenter.ApplicationServer.Name, _config).CreateAsync(virtualNetwork);
            await new ApplicationNodeProvisioner(appVm, createVmTaskResults, _config).InstallAsync();

            return Tuple.Create(virtualNetwork, provisioners);
        }
    }
}