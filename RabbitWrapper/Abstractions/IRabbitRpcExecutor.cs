using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitWrapper.Abstractions
{
    public interface IRabbitRpcExecutor
    {
        Task<TResponse> ExecuteAsync<TRequest, TResponse>(string procedureName, TRequest data);
    }
}
