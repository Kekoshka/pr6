using pr6.Models;

namespace pr6.Interfaces
{
    public interface ITokenService
    {
        string GetJWT(User user);
    }
}
