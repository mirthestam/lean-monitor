using System;

namespace Monitor.Utils
{
    /// <summary>
    /// Represents a moment in time, expressed in units elasped since Thursday, 1 January 1970
    /// </summary>
    public class TimeStamp : IComparable, IComparable<TimeStamp>, IEquatable<TimeStamp>
    {
        private TimeSpan _timeSpan;

        private const long _epochTicks = 621355968000000000;
        private const long _minTicks = -621355968000000000;
        private const long _maxTicks = 3155378975999999999 - _epochTicks;

        public static TimeStamp MinValue => FromTicks(_minTicks);
        public static TimeStamp MaxValue => FromTicks(_maxTicks);

        public static TimeStamp FromSeconds(double elapsedSeconds)
        {
            return From(elapsedSeconds, Model.Resolution.Second);
        }
        public static TimeStamp FromMinutes(double elapsedMinutes)
        {
            return From(elapsedMinutes, Model.Resolution.Minute);
        }
        public static TimeStamp FromHours(double elapsedHours)
        {
            return From(elapsedHours, Model.Resolution.Hour);
        }
        public static TimeStamp FromDays(double elapsedDays)
        {
            return From(elapsedDays, Model.Resolution.Day);
        }
        public static TimeStamp FromTicks(long elapsedTicks)
        {
            var epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            if (elapsedTicks > _maxTicks) throw new ArgumentOutOfRangeException(nameof(elapsedTicks));
            if (elapsedTicks < _minTicks) throw new ArgumentOutOfRangeException(nameof(elapsedTicks));

            var dateTime = epochDateTime.AddTicks(elapsedTicks);
            var timeSpan = dateTime.Subtract(epochDateTime);
            return new TimeStamp(timeSpan);
        }
        public static TimeStamp From(double elapsedUnit, Model.Resolution resolution)
        {
            var epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime dateTime;
            switch (resolution)
            {
                case Model.Resolution.Day:
                    dateTime = epochDateTime.AddDays(elapsedUnit);
                    break;

                case Model.Resolution.Hour:
                    dateTime = epochDateTime.AddHours(elapsedUnit);
                    break;

                case Model.Resolution.Minute:
                    dateTime = epochDateTime.AddMinutes(elapsedUnit);
                    break;

                case Model.Resolution.Second:
                    dateTime = epochDateTime.AddSeconds(elapsedUnit);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(resolution));
            }

            var timeSpan = dateTime.Subtract(epochDateTime);
            return new TimeStamp(timeSpan);
        }

        public static TimeStamp FromDateTime(DateTime dateTime)
        {
            var epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timeSpan = dateTime.Subtract(epochDateTime);
            return new TimeStamp(timeSpan);
        }

        public TimeStamp(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;

            ElapsedDays = (long)Math.Floor(_timeSpan.TotalDays);
            ElapsedSeconds = (long)Math.Floor(_timeSpan.TotalSeconds);
            ElapsedHours = (long)Math.Floor(_timeSpan.TotalHours);
            ElapsedMinutes = (long)Math.Floor(_timeSpan.TotalMinutes);
            ElapsedTicks = _timeSpan.Ticks;
        }

        /// <summary>
        /// Gets or sets the number of days that have elapsed since Thursday, 1 January 1970
        /// </summary>
        public long ElapsedDays { get; private set; }

        /// <summary>
        /// Gets or sets the number of seconds that have elapsed since Thursday, 1 January 1970
        /// </summary>
        public long ElapsedSeconds { get; private set; }

        /// <summary>
        /// Gets or sets the number of hours that have elapsed since Thursday, 1 January 1970
        /// </summary>
        public long ElapsedHours { get; private set; }

        /// <summary>
        /// Gets or sets the number of minutes that have elapsed since Thursday, 1 January 1970
        /// </summary>
        public long ElapsedMinutes { get; private set; }

        /// <summary>
        /// Gets or sets the number of minutes that have elapsed since Thursday, 1 January 1970
        /// </summary>
        public long ElapsedTicks { get; private set; }

        public DateTime DateTime
        {
            get
            {
                var epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epochDateTime.Add(_timeSpan);
            }
        }

        public int CompareTo(TimeStamp other)
        {
            return _timeSpan.CompareTo(other._timeSpan);
        }

        public int CompareTo(object obj)
        {
            return obj == null ? 1 : CompareTo((TimeStamp)obj);
        }

        public bool Equals(TimeStamp other)
        {
            return ElapsedTicks.Equals(other.ElapsedTicks);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TimeStamp && Equals((TimeStamp) obj);
        }

        public override int GetHashCode()
        {
            return ElapsedTicks.GetHashCode();
        }

        public override string ToString()
        {
            return $"{DateTime} - {ElapsedSeconds}";
        }
    }
}