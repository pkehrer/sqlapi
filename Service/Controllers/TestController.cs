using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
