namespace pr6.Common.CustomExceptions
{
    public class InternalServerErrorException : Exception
    {
        public InternalServerErrorException() { }
        public InternalServerErrorException(string message) : base(message) { }
    }
}
