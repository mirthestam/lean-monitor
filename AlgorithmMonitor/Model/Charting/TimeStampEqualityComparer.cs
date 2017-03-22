using System.Collections.Generic;
using Monitor.Model;
using Monitor.Utils;

namespace Monitor.ViewModel.Charts
{
    public class TimeStampEqualityComparer : IEqualityComparer<TimeStamp>
    {
        public Resolution Resolution { get; set; }

        public TimeStampEqualityComparer(Resolution resolution = Resolution.Day)
        {
            Resolution = resolution;
        }

        public bool Equals(TimeStamp x, TimeStamp y)
        {
            switch (Resolution)
            {
                case Resolution.Second:
                    return x.ElapsedSeconds.Equals(y.ElapsedSeconds);
                    
                case Resolution.Minute:
                    return x.ElapsedMinutes.Equals(y.ElapsedMinutes);

                case Resolution.Hour:
                    return x.ElapsedHours.Equals(y.ElapsedHours);

                case Resolution.Day:
                    return x.ElapsedDays.Equals(y.ElapsedDays);

                default:
                    return x.Equals(y);
            }
        }

        public int GetHashCode(TimeStamp obj)
        {
            return obj.GetHashCode();
        }
    }
}