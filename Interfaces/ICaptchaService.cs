using pr6.Models;

namespace pr6.Interfaces
{
    public interface ICaptchaService
    {
        Captcha GenerateCaptcha(string requestId);
        bool Verify(string requestId, string value);
    }
}
