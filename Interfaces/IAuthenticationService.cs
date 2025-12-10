using pr6.Models.DTO;

namespace pr6.Interfaces
{
    public interface IAuthenticationService
    {
        Task StartAuthenticateAsync(UserCredentialsDTO userCredentials);
        Task<TokenPairDTO> EndAuthenticateAsync(string mail, string verifyCode);
    }
}
