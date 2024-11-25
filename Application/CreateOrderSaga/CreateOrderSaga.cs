using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;
using MassTransit;

namespace Application.CreateOrderSaga
{
    public class CreateOrderSaga : MassTransitStateMachine<CreateOrderSagaState>
    {
        public State PendingPayment {  get; set; }
        public State PeymentInProgress { get; set; }
        public State SendedToDelivery {  get; set; }
        public State InDelivery { get; set; }

        public Event<CreateOrderSagaRequest> CreateOrder {  get; set; }
        public Request<CreateOrderSagaState, CreatePaymentRequest, CreatePaymentResponse> CreatePaymentRequest { get; set; }

        public CreateOrderSaga()
        {
            Event<CreateOrderSagaRequest>(() => CreateOrder, x => x.CorrelateById(y => y.Message.OrderId));

            InstanceState(x => x.CurrentState);
            
            Initially();
        }

    }
}
