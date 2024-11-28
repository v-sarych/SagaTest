using Microsoft.AspNetCore.Mvc;

namespace WorkerService.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class TimeOutSimulationController : Controller
    {
        [HttpPost("SendOrderTimeOut")]
        public async Task SendOrderTimeOut()
        {

        }
    }
}