
using System.Collections.Generic;

namespace RabbitMqTools.Conductor
{
    public class Datacenter
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public string ResourceGroupName { get; set; }
        public List<RabbitMqNode> RabbitMqNodes { get; set; }
        public ApplicationServer ApplicationServer { get;set;}
}
}