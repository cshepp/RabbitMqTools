
using System.Threading.Tasks;
using Microsoft.Azure.Management.Network.Fluent;
using RabbitMqTools.Core;

namespace RabbitMqTools.Conductor.Azure
{
    public class ApplicationNodeProvisioner 
    {
        private readonly VirtualMachineFactoryResult _vm;

        public ApplicationNodeProvisioner(VirtualMachineFactoryResult vm)  
        {
            _vm = vm;
        }

        public /*async*/ Task InstallAsync()
        {
            /*
                TODO 
                    - connect via ssh
                    - download producer/consumer archives & untar
                    - start producer/consumer applications
            */
            throw new System.NotImplementedException();
        }
    }
}