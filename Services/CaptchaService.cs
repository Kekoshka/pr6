using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using pr6.Interfaces;
using pr6.Models;
using pr6.Models.Options;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Imaging.Effects;
using System.Reflection;

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
            DrawLinearGradient(graphics);

            DrawDigits(graphics, captchaText);
            DrawLines(graphics);

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
        private void DrawLinearGradient(Graphics graphics)
        {
            // Определение области для градиента
            Rectangle rect = new Rectangle(0, 0, _co.BitmapWidth, _co.BitmapHeight);

            // Создание градиента
            using (LinearGradientBrush brush = new LinearGradientBrush(
                       rect,
                       PickRandomColor(),   // Начальный цвет
                       PickRandomColor(),  // Конечный цвет
                       LinearGradientMode.ForwardDiagonal)) // Направление градиента
            {
                // Заливка области градиентом
                graphics.FillRectangle(brush, rect);
            }
        }
        private void DrawDigits(Graphics graphics,string captchaText)
        {
            int y = _co.BitmapHeight / 2 - 10;
            for (int i = 0;i < captchaText.Length; i++){
                int x = (_co.BitmapWidth - 40) / captchaText.Length*i;
                Font font = new(FontFamily.Families[new Random().Next(0,FontFamily.Families.Length-1)], _co.FontSize);

                graphics.DrawString(captchaText.ToCharArray()[i].ToString(), font, PickRandomBrush(), new PointF(x, y));

            }
        }
        private void DrawLines(Graphics graphics)
        {
            for(int i = 0; i< _co.Lines; i++)
            {
                var point1 = new PointF(new Random().Next(0, _co.BitmapWidth), new Random().Next(0, _co.BitmapHeight));
                var point2 = new PointF(new Random().Next(0, _co.BitmapWidth), new Random().Next(0, _co.BitmapHeight));
                graphics.DrawLine(new Pen(PickRandomBrush(), new Random().Next(1, 6)), point1, point2);
            }
        }
        private Brush PickRandomBrush()
        {
            Brush result = Brushes.Transparent;

            Random rnd = new Random();

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(properties.Length);
            return (Brush)properties[random].GetValue(null, null);
        }
        private Color PickRandomColor()
        {
            Color result = Color.Aqua;

            Random rnd = new Random();

            Type colorsType = typeof(Color);

            PropertyInfo[] properties = colorsType.GetProperties();

            int random = rnd.Next(properties.Length);
            return (Color)properties[random].GetValue(null, null);
        }
    }
}
