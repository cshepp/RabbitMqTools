
using System.Collections.Generic;
using Nett;

namespace RabbitMqTools.Core.Configuration
{
    public interface IConductorConfiguration
    {
        string RmqAdminUsername { get; set; }
        string RmqAdminPassword { get; set; }
        string VmUsername { get; set; }
        string VmPassword { get; set; }
        string ResourceGroupName { get; set; }
        string DnsPrefix { get; set; }
        List<Datacenter> Datacenters { get; set; }
    }

    public class ConductorConfiguration : IConductorConfiguration
    {
        public string RmqAdminUsername { get; set; }
        public string RmqAdminPassword { get; set; }
        public string VmUsername { get; set; }
        public string VmPassword { get; set; }
        public string ResourceGroupName { get; set; }
        public string DnsPrefix { get; set; }
        public List<Datacenter> Datacenters { get; set; }

        public static IConductorConfiguration Load(string file)
        {
            return (IConductorConfiguration)Toml.ReadFile<ConductorConfiguration>(file);
        }
    }
}