using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using pr6.Interfaces;
using pr6.Models;
using pr6.Models.Options;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Imaging.Effects;

namespace pr6.Services
{
    public class CaptchaService : ICaptchaService
    {
        IRandomService _randomService;
        CaptchaOptions _co;
        IMemoryCache _cache;
        IHttpContextAccessor _context;
        public CaptchaService(IRandomService randomService,
            IOptions<CaptchaOptions> captchaOptions,
            IMemoryCache cache,
            IHttpContextAccessor context) 
        {
            _randomService = randomService;
            _co = captchaOptions.Value;
            _cache = cache;
            _context = context;
        }
        public Captcha GenerateCaptcha(string requestId)
        {
            var captchaText = _randomService.GenerateCaptcha();

            using Bitmap bitmap = new(_co.BitmapWidth, _co.BitmapHeight);
            using Graphics graphics = Graphics.FromImage(bitmap);

            graphics.Clear(Color.White);
            Font font = new(_co.Font, _co.FontSize);
            Brush brush = Brushes.Black;
            graphics.DrawString(captchaText, font, brush, new PointF(_co.BitmapHeight / 2, 10));

            using MemoryStream memoryStream = new();
            bitmap.Save(memoryStream, ImageFormat.Png);

            var captcha = new Captcha()
            {
                Id = Guid.NewGuid(),
                Image = memoryStream.ToArray(),
                Value = captchaText
            };

            _cache.Set(requestId, captcha);

            return captcha;
        }
        public bool Verify(string requestId, string value)
        {
            bool isVerify = _cache.TryGetValue(requestId, out Captcha captcha) && captcha.Value == value;
            if (!isVerify) return false;

            var clientIp = _context.HttpContext.Connection.RemoteIpAddress;
            return true;
        }
    }
}
