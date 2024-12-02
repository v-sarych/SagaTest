using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitWrapper.Models
{
    public class RpcResponse<TData>
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccess { get; set; }

        public string? Eror {  get; set; }
        public TData? Data { get; set; }
    }
}
