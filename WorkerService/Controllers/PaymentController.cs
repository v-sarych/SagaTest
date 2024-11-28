using Microsoft.AspNetCore.Mvc;

namespace WorkerService.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class PaymentController : Controller
    {
        [HttpPost("Pay")]
        public async Task Pay()
        {

        }

        [HttpPost("AcceptPayment")]
        public async Task AcceptPayment()
        {

        }
    }
}
