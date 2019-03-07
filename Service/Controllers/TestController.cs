using Microsoft.AspNetCore.Mvc;

namespace Service.Controllers
{
  [Route("")]
  [ApiController]
  public class TestController : ControllerBase
  {

    [HttpGet("test")]
    public ActionResult<string> Test()
    {
      return "Hello thereYYYYY";
    }
  }
}
