using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitWrapper.Abstractions;
using RabbitWrapper.Configuration;
using RabbitWrapper.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RabbitWrapper.Implementations
{
    public class RabbitRpcExecutor(IConnectionFactory _factory, RpcConfiguration configuration) : IRabbitRpcExecutor
    {
        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(string procedureName, TRequest request)
        {
            var client = await _factory.CreateConnectionAsync();
            var chanel = await client.CreateChannelAsync();

            var data = new RpcRequest<TRequest>()
            {
                Data = request
            };
            var bytesData = JsonSerializer.SerializeToUtf8Bytes<RpcRequest<TRequest>>(data);

            Guid corelationId = Guid.NewGuid();

            //create response query
            var responseQueueName = await chanel.QueueDeclareAsync(exclusive: true);
            await chanel.QueueBindAsync(responseQueueName, configuration.ResponseExchangeName, corelationId.ToString());

            await chanel.BasicPublishAsync(await _getPublishExchangeName(procedureName, chanel), "", false, new BasicProperties()
            {
                CorrelationId = corelationId.ToString(),
                ReplyTo = configuration.ResponseExchangeName,
            }, bytesData);

            RpcResponse<TResponse> response = new();
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);
            var consumer = new AsyncEventingBasicConsumer(chanel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                response = JsonSerializer.Deserialize<RpcResponse<TResponse>>(bytesData);
                semaphore.Release();
            };
            await chanel.BasicConsumeAsync(responseQueueName, autoAck: true, consumer);

            await semaphore.WaitAsync();

            if (!response.IsSuccess)
            {
                throw new Exception(response.Eror);
            }
            return response.Data;
        }

        private async Task<string> _getPublishExchangeName(string procedureName, IChannel chanel)
        {
            await chanel.ExchangeDeclareAsync(configuration.RequestExchangeName, "direct", false, false);
            await chanel.QueueDeclareAsync(procedureName, exclusive: true);

            return "rpc";
        }
    }
}
