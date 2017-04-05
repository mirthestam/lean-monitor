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
        Day,
        Ticks
    }

    [ValueConversion(typeof(Resolution), typeof(SeriesResolution))]
    public class ResolutionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (targetType != typeof(SeriesResolution))
                throw new InvalidOperationException("The target must be a SeriesResolution");

            switch ((Resolution) value)
            {
                case Resolution.Second:
                    return SeriesResolution.Second;

                case Resolution.Minute:
                    return SeriesResolution.Minute;

                case Resolution.Hour:
                    return SeriesResolution.Hour;

                case Resolution.Day:
                    return SeriesResolution.Day;

                case Resolution.Ticks:
                    return SeriesResolution.Ticks;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (targetType != typeof(Resolution))
                throw new InvalidOperationException("The target must be a Resolution");

            switch ((SeriesResolution) value)
            {
                case SeriesResolution.Second:
                    return Resolution.Second;

                case SeriesResolution.Minute:
                    return Resolution.Minute;

                case SeriesResolution.Hour:
                    return Resolution.Hour;

                case SeriesResolution.Day:
                    return Resolution.Day;

                case SeriesResolution.Ticks:
                    return Resolution.Ticks;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);

            }
        }
    }
}