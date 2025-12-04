namespace pr6.Interfaces
{
    public interface IHashService
    {
        string Hash(string password);
        bool Verify(string text, string hash);
    }
}
