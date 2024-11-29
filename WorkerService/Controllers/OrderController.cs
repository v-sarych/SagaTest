using Domain.Model.Requestes;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace WorkerService.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class OrderController(IBus _bus) : Controller
    {

        [HttpPost("CreateOrder")]
        public async Task CreateOrder(string data)
        {
            await _bus.Publish(new CreateOrderSagaRequest()
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
