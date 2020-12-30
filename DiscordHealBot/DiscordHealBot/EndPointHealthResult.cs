using System;

namespace DiscordHealBot
{
    public record EndPointHealthResult
    {
        public string EndpointAddress { get; set; }
        public bool Success { get; set; }
        public double Latency { get; set; }
        public int StatusCode { get; set; }
        public string Family { get; set; }
        public DateTime DateRun { get; set; }
    }
}