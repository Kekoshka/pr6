namespace pr6.Models.Options
{
    public class JWTOptions
    {
        public string Issuer { get; set; } = "";
        public string Key { get; set; } = "";
        public int LifeTimeAccessFromMinutes { get; set; } = 0;
        public int LifeTimeRefreshFromMinutes { get; set; } = 0;

    }
}
