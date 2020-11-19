namespace DiscordHealBot
{
    public class JobSettings
    {
        /// <summary>
        /// Time between two loops in seconds
        /// </summary>
        public int TimeInterval { get; set; }
        
        public bool FamilyReporting { get; set; }

        public int GetTimeIntervalInMs()
        {
            return TimeInterval * 1000;
        }
    }
}