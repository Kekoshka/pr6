using Microsoft.EntityFrameworkCore;
using pr6.Common.CustomExceptions;
using pr6.Models.DTO;
using pr6.Models;
using pr6.Services;

namespace pr6.Interfaces
{
    public interface IRegistrationService
    {
        Task StartRegistrationAsync(UserCredentialsDTO userCredentials, CancellationToken cancellationToken);
        Task EndRegistrationAsync(string code, CancellationToken cancellationToken);

    }
}
