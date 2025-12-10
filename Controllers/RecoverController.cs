using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pr6.Interfaces;
using System.Drawing;

namespace pr6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecoverController : ControllerBase
    {
        IRecoverService _recoverService;
        public RecoverController(IRecoverService recoverService)
        {
            _recoverService = recoverService;
        }
        [HttpPost("StartRecoverPasswordAsync")]
        public async Task<IActionResult> StartRecoverPasswordAsync(string mail, CancellationToken cancellationToken)
        {
            await _recoverService.StartRecoverPasswordAsync(mail, cancellationToken);
            return NoContent();
        }
        [HttpPost("EndRecoverPasswordAsync")]
        public async Task<IActionResult> EndRecoverPasswordAsync(string mail, string code, string newPassword, CancellationToken cancellationToken)
        {
            await _recoverService.EndRecoverPasswordAsync(mail, code, newPassword, cancellationToken);
            return NoContent();
        }
    }
}
