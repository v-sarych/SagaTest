using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;
using Domain.Model.Enums;
using Domain.Model.Requestes;
using Domain.Model.Responses;
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
        public Event<OrderTimeOut> OrderTimeOut { get; set; }
        public Request<CreateOrderSagaState, CancelOrderRequest, CancelOrderResponse> CancelOrder { get; set; }
        public Request<CreateOrderSagaState, CreatePaymentRequest, CreatePaymentResponse> CreatePaymentRequest { get; set; }
        public Request<CreateOrderSagaState, SendToDeliveryRequest, SendToDeliveryResponse> SendToDelivery { get; set; }
        public Event<DeliveryStatusUpdated> DeliveryStatusUpdated { get; set; }
        public Event<PaymentStatusUpdated> PaymentStatusUpdated { get; set; }


        public CreateOrderSaga(ILogger<CreateOrderSaga> logger)
        {
            _logger = logger;

            Event<CreateOrderSagaRequest>(() => CreateOrder, x => x.CorrelateById(y => y.Message.OrderId));
            Event<PaymentStatusUpdated>(() => PaymentStatusUpdated, x => x.CorrelateById(y => y.Message.OrderId));
            Event<DeliveryStatusUpdated>(() => DeliveryStatusUpdated, x => x.CorrelateById(y => y.Message.OrderId));
            Event<OrderTimeOut>(() => OrderTimeOut, x => x.CorrelateById(y => y.Message.OrderId));
            
            Request(() => CreatePaymentRequest);
            Request(() => CancelOrder);
            Request(() => SendToDelivery);

            InstanceState(x => x.CurrentState);
            
            Initially(
                When(CreateOrder)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation($"Saga created: { context.Message.OrderId }");
                    })
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
                                await context.TransitionToState(PaymentInProgress);

                                _logger.LogInformation("Saga received payment in progress");

                                break;
                        }
                    }),
                When(OrderTimeOut)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("Saga received OrderTimeOut");
                    })
                    .Request(CancelOrder, x => x.Init<CancelOrderRequest>(new { OrderId = x.Message.OrderId }))
                    .Finalize()
                );

            During(PaymentInProgress, 
                When(PaymentStatusUpdated)
                .ThenAsync(async context =>
                {
                    switch (context.Message.PaymentStatus)
                    {
                        case PaymentStatuses.Success:
                            _logger.LogInformation("Saga received payment in progress");

                            await context.RespondAsync(SendToDelivery);
                            await context.TransitionToState(SendedToDelivery);

                            _logger.LogInformation("Saga send to delivery");
                            break;

                        case PaymentStatuses.Failed:

                            _logger.LogInformation("Saga received payment failed");
                            await context.TransitionToState(PaymentInProgress);

                            break;
                    }
                }));

            During(SendedToDelivery,
                When(DeliveryStatusUpdated)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("Saga received delivery aborted");
                        switch (context.Message.DeliveryStatus)
                        {
                            case DeliveryStatuses.InTransit:
                                _logger.LogInformation("DeliveryStatuses.InTransit");

                                await context.TransitionToState(InDelivery);
                                break;

                            case DeliveryStatuses.Aborted:

                                _logger.LogInformation("Saga received DeliveryStatuses.Aborted");

                                //aborted logic

                                break;
                        }
                    }));

            During(InDelivery,
                When(DeliveryStatusUpdated)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("Saga received delivery aborted");
                        switch (context.Message.DeliveryStatus)
                        {
                            case DeliveryStatuses.Delivered:
                                _logger.LogInformation("DeliveryStatuses.InTransit");

                                //finalize job
                                break;

                            case DeliveryStatuses.Aborted:

                                _logger.LogInformation("Saga received DeliveryStatuses.Aborted");
                                
                                //aborted logic

                                break;
                        }
                    }));
        }

    }
}
