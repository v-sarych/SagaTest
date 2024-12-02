using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitWrapper.Configuration
{
    public class RpcConfiguration
    {
        public string RequestExchangeName { get; set; } = "rpc.requests";
        public string ResponseExchangeName { get; set; } = "rpc.responses";
    }
}
