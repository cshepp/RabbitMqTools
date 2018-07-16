
using System.Collections.Generic;

namespace RabbitMqTools.Core
{
    public class Datacenter
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public List<RabbitMqNode> RabbitMqNodes { get; set; }
        public ApplicationServer ApplicationServer { get;set;}
}
}