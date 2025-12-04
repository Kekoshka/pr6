namespace pr6.Common.CustomExceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() { }
        public UnauthorizedException(string message) : base(message) { }
    }
}
