using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitWrapper.Abstractions
{
    public interface IRabbitRpcConsumer
    {
        Task Consume<TResponse, TRequest>(string procedureName, Func<TRequest, Task<TResponse>> handler);
    }
}
