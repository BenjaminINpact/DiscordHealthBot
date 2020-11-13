namespace DiscordHealBot
{
    public class EndPointHealthResult
    {
        public string EndpointAddress { get; set; }
        public bool Success { get; set; }
        public double Latency { get; set; }
        public int StatusCode { get; set; }
    }
}