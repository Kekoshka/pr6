using pr6.Interfaces;
using pr6.Models;

namespace pr6.Services
{
    public class CaptchaService : ICaptchaService
    {
        IRandomService _randomService;
        public CaptchaService(IRandomService randomService) 
        {
            _randomService = randomService;
        }
        public Captcha GenerateCaptcha()
        {
            var captchaText = _randomService.GenerateCaptcha();

        }
    }
}
