using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using pr6.Interfaces;
using pr6.Models.DTO;
using IAuthenticationService = pr6.Interfaces.IAuthenticationService;

namespace pr6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService authenticationService) 
        {
            _authenticationService = authenticationService;
        }   
        public async Task<IActionResult> StartAuthentication(UserCredentialsDTO userCredentials)
        {
            await _authenticationService.StartAuthenticateAsync(userCredentials);
            return Ok();
        }
        public async Task<IActionResult> EndAuthentication(string mail, string verifyCode)
        {
            var tokenPair = await _authenticationService.EndAuthenticateAsync(mail,verifyCode);
            return Ok(tokenPair);
        }
        public async Task<IActionResult> GetNewTokenPair(string refreshToken)
        {

        }
    }
}
