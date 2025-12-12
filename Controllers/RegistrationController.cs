using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pr6.Interfaces;
using pr6.Models.DTO;

namespace pr6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        IRegistrationService _registrationService;
        public RegistrationController(IRegistrationService registrationService) 
        {
            _registrationService = registrationService;
        }
        [HttpPost("StartRegistrationAsync")]
        public async Task<IActionResult> StartRegistrationAsync(UserCredentialsDTO userCredentials, CancellationToken cancellationToken)
        {
            await _registrationService.StartRegistrationAsync(userCredentials, cancellationToken);
            return NoContent();
        }
        [HttpPost("EndRegistrationAsync")]
        public async Task<IActionResult> EndRegistrationAsync(string code, CancellationToken cancellationToken)
        {
            await _registrationService.EndRegistrationAsync(code, cancellationToken);
            return NoContent();
        }
    }
}
