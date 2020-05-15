using System;

namespace CSampleServer
{
    public class TimeManager
    {
        private static TimeManager instance;
        
        public static TimeManager I
        {
            get
            {
                if (instance == null)
                {
                    instance = new TimeManager();
                }
                return instance;
            }
        }
        
        private readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly int m_secondsInADay = 86400;
        
        public long UtcTimeStampSeconds => (long)UtcNow.Subtract(UNIX_EPOCH).TotalSeconds;
        public DateTime UtcNow => DateTime.UtcNow;

        public bool IsTimeOver(long time1, long time2)
        {
            return time1 < time2;
        }
    }
}