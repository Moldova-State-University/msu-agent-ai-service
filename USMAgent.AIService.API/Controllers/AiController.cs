using Microsoft.AspNetCore.Mvc;

namespace USMAgent.AIService.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AiController : ControllerBase
    {
        [Route("/")]
        public IActionResult GetAiResponse(string message)
        {
            return IActionResult.Ok();
        }


    }
}