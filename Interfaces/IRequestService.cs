using pr6.Models;

namespace pr6.Interfaces
{
    public interface IRequestService
    {
        Task<string> SaveRequestInCacheAsync(HttpContext context, CancellationToken cancellationToken);
        Task<HttpContextData> GetRequestFromCacheAsync(string requestId, CancellationToken cancellationToken);
        Task ExecuteCachedRequestAsync(string requestId, HttpContext currentContext, CancellationToken cancellationToken);
    }
}
