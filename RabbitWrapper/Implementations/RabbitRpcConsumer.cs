using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitWrapper.Abstractions;
using RabbitWrapper.Configuration;
using RabbitWrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitWrapper.Implementations
{
    public class RabbitRpcConsumer(IConnectionFactory _factory, RpcConfiguration configuration) : IRabbitRpcConsumer
    {
        public async Task Consume<TRequest, TResponse>(string procedureName, Func<TRequest, Task<TResponse>> handler)
        {
            var client = await _factory.CreateConnectionAsync();
            var chanel = await client.CreateChannelAsync();

            await chanel.ExchangeDeclareAsync(configuration.RequestExchangeName, "direct");
            await chanel.QueueDeclareAsync(procedureName, exclusive: false, autoDelete: false);
            await chanel.QueueBindAsync(procedureName, configuration.RequestExchangeName, "");

            var consumer = new AsyncEventingBasicConsumer(chanel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var request = JsonSerializer.Deserialize<RpcRequest<TRequest>>(ea.Body.ToArray());

                var response = new RpcResponse<TResponse>();
                try
                {
                    response.Data = await handler.Invoke(request.Data);
                    response.IsSuccess = true;
                }
                catch (Exception ex) 
                {
                    response.Eror = ex;
                    response.IsSuccess = false;
                }

                await chanel.BasicAckAsync(ea.DeliveryTag, false);

                await chanel.BasicPublishAsync(ea.BasicProperties.ReplyTo, ea.BasicProperties.CorrelationId, JsonSerializer.SerializeToUtf8Bytes(response));
            };
            await chanel.BasicConsumeAsync(procedureName, autoAck: false, consumer);
        }
    }
}
