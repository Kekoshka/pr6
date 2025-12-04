namespace pr6.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Mail { get; set; }
        public string PasswordHash { get; set; }
        public string CodeHash { get; set; }
    }
}
