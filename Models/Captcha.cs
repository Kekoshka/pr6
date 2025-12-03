namespace pr6.Models
{
    public class Captcha
    {
        public Guid Id { get; set; }
        public byte[] Image { get; set; }
        public string Value { get; set; }
    }
}
