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
        public CaptchaService(IRandomService randomService,
            IOptions<CaptchaOptions> captchaOptions) 
        {
            _randomService = randomService;
            _co = captchaOptions.Value;
        }
        public Captcha GenerateCaptcha()
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

            return new Captcha()
            {
                Id = Guid.NewGuid(),
                Image = memoryStream.ToArray(),
                Value = captchaText
            };
        }
    }
}
