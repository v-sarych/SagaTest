using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Model.Requestes;
using Domain.Model.Responses;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Consumers
{
    public class CreatePaymentConsumer(ILogger<CreatePaymentConsumer> _logger) : IConsumer<CreatePaymentRequest>
    {
        public async Task Consume(ConsumeContext<CreatePaymentRequest> context)
        {
            _logger.LogInformation("Payment Created");
            await context.RespondAsync<CreatePaymentResponse>(new()
            {
                OrderId = context.Message.OrderId,
            });
        }
    }
}
