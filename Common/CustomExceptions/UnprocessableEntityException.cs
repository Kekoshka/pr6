namespace pr6.Common.CustomExceptions
{
    public class UnprocessableEntityException : Exception
    {
        public UnprocessableEntityException() { }
        public UnprocessableEntityException(string message) : base(message) { }
    }
}
