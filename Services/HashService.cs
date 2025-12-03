using pr6.Interfaces;

namespace pr6.Services
{
    public class HashService : IHashService
    {
        private readonly int _workFactor = 14;

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
        }

        public bool Verify(string text, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(text, hash);
        }
    }
}
