using Microsoft.AspNetCore.Mvc;

namespace Innova.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult GetTest()
        {
            return Ok("Test successful");
        }
    }
}