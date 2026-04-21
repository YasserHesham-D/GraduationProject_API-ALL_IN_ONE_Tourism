using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> SignUp([FromBody] string request)
        {
            return Ok();
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> SignIn([FromBody] string request)
        {
            return Ok();
        }

    }
}
