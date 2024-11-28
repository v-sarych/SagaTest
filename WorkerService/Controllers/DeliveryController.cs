using Microsoft.AspNetCore.Mvc;

namespace WorkerService.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class DeliveryController : Controller
    {
        [HttpPost("SetToInDelivery")]
        public async Task SetToInDelivery()
        {

        }
    }
}
