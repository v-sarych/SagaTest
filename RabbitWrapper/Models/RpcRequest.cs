using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitWrapper.Models
{
    public class RpcRequest<TData>
    {
        public TData Data { get; set; }
    }
}
