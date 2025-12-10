using pr6.Models.DTO;

namespace pr6.Interfaces
{
    public interface IAuthenticationService
    {
        Task StartAuthenticateAsync(UserCredentialsDTO userCredentials, CancellationToken cancellationToken);
        Task<TokenPairDTO> EndAuthenticateAsync(string mail, string verifyCode, CancellationToken cancellationToken);
    }
}
