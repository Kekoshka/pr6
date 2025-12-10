using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace pr6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaptchaController : ControllerBase
    {
        public async Task<IActionResult> VerifyCaptcha()
        {

        }

    }
}
