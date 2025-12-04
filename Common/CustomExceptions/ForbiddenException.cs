namespace pr6.Common.CustomExceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException() { }
        public ForbiddenException(string message) : base(message) { }
    }
}
