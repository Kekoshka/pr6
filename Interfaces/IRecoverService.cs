namespace pr6.Interfaces
{
    public interface IRecoverService
    {
        Task StartRecoverPasswordAsync(string mail, CancellationToken cancellationToken);
        Task EndRecoverPasswordAsync(string mail, string code, string newPassword, CancellationToken cancellationToken);
    }
}
