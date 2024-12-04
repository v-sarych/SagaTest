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
            var bytesData = JsonSerializer.SerializeToUtf8Bytes(data);

            Guid corelationId = Guid.NewGuid();

            //create response query
            var responseQueue = await chanel.QueueDeclareAsync(exclusive: true, autoDelete: true);
            await chanel.QueueBindAsync(responseQueue.QueueName, configuration.ResponseExchangeName, corelationId.ToString());

            //receiving
            RpcResponse<TResponse> response = new();
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);
            var consumer = new AsyncEventingBasicConsumer(chanel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                response = JsonSerializer.Deserialize<RpcResponse<TResponse>>(ea.Body.ToArray());
                semaphore.Release();
            };
            await chanel.BasicConsumeAsync(responseQueue.QueueName, autoAck: true, consumer);

            //sending
            await chanel.ExchangeDeclareAsync(configuration.RequestExchangeName, "direct", false, false);
            await chanel.BasicPublishAsync(configuration.RequestExchangeName, "", false, new BasicProperties()
            {
                CorrelationId = corelationId.ToString(),
                ReplyTo = configuration.ResponseExchangeName,
            }, bytesData);

            //message received
            await semaphore.WaitAsync();
            await chanel.QueueDeleteAsync(responseQueue.QueueName);

            if (!response.IsSuccess)
            {
                throw response.Eror;
            }
            return response.Data;
        }
    }
}
