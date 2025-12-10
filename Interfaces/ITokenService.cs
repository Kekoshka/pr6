using pr6.Models;
using pr6.Models.DTO;

namespace pr6.Interfaces
{
    public interface ITokenService
    {
        TokenPairDTO GetJWTPair(User user);
    }
}
