using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using pr6.Context;
using pr6.Interfaces;
using pr6.Models.DTO;
using System.Security.Claims;
using IAuthenticationService = pr6.Interfaces.IAuthenticationService;

namespace pr6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        IAuthenticationService _authenticationService;
        ITokenService _tokenService;
        ApplicationContext _context;
        public AuthenticationController(IAuthenticationService authenticationService,
            ITokenService tokenService,
            ApplicationContext context) 
        {
            _authenticationService = authenticationService;
            _tokenService = tokenService;
            _context = context;
        }
        [HttpPost("StartAuthenticationAsync")]
        public async Task<IActionResult> StartAuthenticationAsync(string mail, string password, CancellationToken cancellationToken)
        {
            UserCredentialsDTO uc = new()
            {
                Mail = mail,
                Password = password
            };
            await _authenticationService.StartAuthenticateAsync(uc, cancellationToken);
            return NoContent();
        }
        [HttpPost("EndAuthenticationAsync")]
        public async Task<IActionResult> EndAuthenticationAsync(string mail, string verifyCode, CancellationToken cancellationToken)
        {
            var tokenPair = await _authenticationService.EndAuthenticateAsync(mail,verifyCode, cancellationToken);
            return Ok(tokenPair);
        }
        [HttpGet("GetNewTokenPairAsync")]
        [Authorize(policy:"RefreshTokenOnly")]
        public async Task<IActionResult> GetNewTokenPairAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var userMail = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            if (userMail is null) return Forbid("Invalid token");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Mail == userMail, cancellationToken);
            if (user is null) return Forbid("Invalid token");

            var tokenPair =  _tokenService.GetJWTPair(user);
            return Ok(tokenPair);
        }
    }
}
