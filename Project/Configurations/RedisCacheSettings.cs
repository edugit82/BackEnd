namespace Project.Configurations
{
    public class RedisCacheSettings
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public int DefaultExpirationMinutes { get; set; }
    }
}