namespace pr6.Models.Options
{
    public class JWTOptions
    {
        public string Issuer { get; set; } = "";
        public string Key { get; set; } = "";
        public int LifeTimeFromMinutes { get; set; } = 0;
    }
}
