using Microsoft.Extensions.Options;
using pr6.Interfaces;
using pr6.Models.Options;
using System.Text;

namespace pr6.Services
{
    public class RandomService : IRandomService
    {
        RandomOptions _randomOptions;
        public RandomService(IOptions<RandomOptions> randomOptions) 
        {
            _randomOptions = randomOptions.Value;
        }
        public string GenerateTempCode() =>
            GenerateCode(_randomOptions.TempCodeLength);
        public string GenerateCaptcha() =>
            GenerateCode(_randomOptions.CaptchaLength);
        private string GenerateCode(int length)
        {
            string tempCode = string.Empty;

            char[] letters = "QWERTYUIOIPASDFGHJKLZXCVBNM".ToCharArray();
            int rndDigit = new Random().Next(0, 9);
            char rndLetter = letters[new Random().Next(0, letters.Length)];
            int rndLetOrDig = new Random().Next(0, 1);

            for (int i = 0; i < length; i++)
                tempCode += rndLetOrDig == 0 ? rndDigit.ToString() : rndLetter;

            return tempCode;
        }
    }
}
