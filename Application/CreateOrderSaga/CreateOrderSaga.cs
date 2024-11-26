using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;
using Domain.Model.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.CreateOrderSaga
{
    public class CreateOrderSaga : MassTransitStateMachine<CreateOrderSagaState>
    {
        private readonly ILogger<CreateOrderSaga> _logger;

        public State PendingPayment {  get; set; }
        public State PaymentInProgress { get; set; }
        public State SendedToDelivery {  get; set; }
        public State InDelivery { get; set; }

        public Event<CreateOrderSagaRequest> CreateOrder {  get; set; }
        public Request<CreateOrderSagaState, CreatePaymentRequest, CreatePaymentResponse> CreatePaymentRequest { get; set; }
        public Request<CreateOrderSagaState, SendToDeliveryRequest, SendToDeliveryResponse> SendToDelivery { get; set; }
        public Event<InDeliveryRequest> InDeliveryEvent { get; set; }
        public Event<DeliveryStatusUpdated> DeliveryAborted { get; set; }
        public Event<PaymentStatusUpdated> PaymentStatusUpdated { get; set; }

        public CreateOrderSaga(ILogger<CreateOrderSaga> logger)
        {
            _logger = logger;

            Event<CreateOrderSagaRequest>(() => CreateOrder, x => x.CorrelateById(y => y.Message.OrderId));
            Event<PaymentStatusUpdated>(() => PaymentStatusUpdated, x => x.CorrelateById(y => y.Message.OrderId));

            Request(() => CreatePaymentRequest);

            InstanceState(x => x.CurrentState);
            
            Initially(
                When(CreateOrder)
                    .Request(CreatePaymentRequest, x => x.Init<CreatePaymentRequest>(new{OrderId = x.Message.OrderId}))
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("Saga send create payment");
                    })
                    .TransitionTo(PendingPayment)
                );

            During(PendingPayment, 
                When(PaymentStatusUpdated)
                    .ThenAsync(async context =>
                    {
                        switch (context.Message.PaymentStatus)
                        {
                            case PaymentStatuses.Progress:
                                context.TransitionToState(PaymentInProgress);

                                _logger.LogInformation("Saga received payment in progress");

                                break;
                        }
                    }));

            During(PaymentInProgress, 
                When(PaymentStatusUpdated)
                .ThenAsync(async context =>
                {
                    switch (context.Message.PaymentStatus)
                    {
                        case PaymentStatuses.Success:
                            _logger.LogInformation("Saga received payment in progress");

                            await context.RespondAsync(SendToDelivery);
                            context.TransitionToState(SendedToDelivery);

                            _logger.LogInformation("Saga send to delivery");
                            break;

                        case PaymentStatuses.Failed:

                            _logger.LogInformation("Saga received payment failed");
                            context.TransitionToState(PaymentInProgress);

                            break;
                    }
                }));

            During(SendedToDelivery,
                When(InDeliveryEvent),
                When(DeliveryAborted)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("Saga received delivery aborted");
                    }));

            During(InDelivery,
                When(DeliveryAborted)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("Saga received delivery aborted");
                    }));
        }

    }
}
