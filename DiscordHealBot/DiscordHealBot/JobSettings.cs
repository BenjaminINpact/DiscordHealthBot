namespace DiscordHealBot
{
    public class JobSettings
    {
        /// <summary>
        /// Time between two announcements in seconds
        /// </summary>
        public int TimeInterval { get; set; }
        
        /// <summary>
        /// Time between two polling in seconds
        /// </summary>
        public int PollingInterval { get; set; }
        
        
        /// <summary>
        /// Report by family instead of per endpoints
        /// </summary>
        public bool FamilyReporting { get; set; }

        public int GetAnnouncementTimeIntervalInMs()
        {
            return TimeInterval * 1000;
        }

        public int GetPollingTimeIntervalInMs()
        {
            return PollingInterval * 1000;
        }
    }
}