using pr6.Models;

namespace pr6.Interfaces
{
    public interface IRequestService
    {
        Task SaveRequestInCacheAsync(HttpContext context);
        Task<HttpContextData> GetRequestFromCacheAsync(string requestId);
        Task ExecuteCachedRequestAsync(string requestId, HttpContext currentContext);
    }
}
