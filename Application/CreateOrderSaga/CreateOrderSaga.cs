﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace Application.CreateOrderSaga
{
    internal class CreateOrderSaga : MassTransitStateMachine<CreateOrderSagaState>
    {
        public CreateOrderSaga()
        {
                
        }
    }
}
