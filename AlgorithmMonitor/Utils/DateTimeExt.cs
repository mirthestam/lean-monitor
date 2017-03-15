using System;

namespace QuantConnect.Lean.Monitor.Utils
{
    public static class DateTimeExt
    {
        public static DateTime FromTimeStamp(double unixTimeStamp)
        {
            var epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epochDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}