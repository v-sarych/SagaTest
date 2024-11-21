using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CreateOrderSaga
{
    internal class CreateOrderSagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
    }
}
