using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitWrapper.Abstractions
{
    public interface IRabbitRpcExecutor
    {
        Task<TResponse> ExecuteAsync<TResponse, TRequest>(string procedureName, TRequest data);
    }
}
