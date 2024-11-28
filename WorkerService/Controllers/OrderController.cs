using Domain.Model.Requestes;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace WorkerService.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class OrderController(ISendEndpoint _sendEndpoint) : Controller
    {

        [HttpPost("CreateOrder")]
        public async Task CreateOrder(string data)
        {
            await _sendEndpoint.Send(new CreateOrderSagaRequest()
            {
                OrderId = Guid.NewGuid()
            });
        }

        [HttpPost("CancelOrder")]
        public async Task CancelOrder()
        {

        }
    }
}
