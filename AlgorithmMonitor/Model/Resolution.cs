using System;
using System.Globalization;
using System.Windows.Data;
using LiveCharts.Helpers;

namespace Monitor.Model
{
    /// <summary>
    /// Defines the interval on which earch of the charts bars is based
    /// </summary>
    public enum Resolution
    {
        Second,
        Minute,
        Hour,
        Day
    }

    [ValueConversion(typeof(Resolution), typeof(PeriodUnits))]
    public class ResolutionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (targetType != typeof(PeriodUnits))
                throw new InvalidOperationException("The target must be a SeriesResolution");

            switch ((Resolution) value)
            {
                case Resolution.Second:
                    return PeriodUnits.Seconds;

                case Resolution.Minute:
                    return PeriodUnits.Minutes;

                case Resolution.Hour:
                    return PeriodUnits.Hours;

                case Resolution.Day:
                    return PeriodUnits.Days;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (targetType != typeof(Resolution))
                throw new InvalidOperationException("The target must be a Resolution");

            switch ((PeriodUnits) value)
            {
                case PeriodUnits.Seconds:
                    return Resolution.Second;

                case PeriodUnits.Minutes:
                    return Resolution.Minute;

                case PeriodUnits.Hours:
                    return Resolution.Hour;

                case PeriodUnits.Days:
                    return Resolution.Day;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}