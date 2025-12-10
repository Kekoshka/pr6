using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pr6.Interfaces;

namespace pr6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaptchaController : ControllerBase
    {
        ICaptchaService _captchaService;
        IRequestService _requestService;
        public CaptchaController(ICaptchaService captchaService,
            IRequestService requestService)
        {
            _captchaService = captchaService;
            _requestService = requestService;
        }
        [HttpPost("VerifyCaptcha")]
        public async Task<IActionResult> VerifyCaptcha(string requestId, string code, CancellationToken cancellationToken)
        {
            var isVerify = _captchaService.Verify(requestId, code);
            if (!isVerify) return Forbid("Invalid code");

            await _requestService.ExecuteCachedRequestAsync(requestId, HttpContext, cancellationToken);
            return new EmptyResult();
        }

    }
}
