using Microsoft.AspNetCore.Mvc;

namespace Service.Controllers
{
    [Route("")]
    [ApiController]
    public class LivecheckController : ControllerBase
    {
        [HttpGet("livecheck")]
        public ActionResult Index()
        {
            return NoContent();
        }
    }
}
