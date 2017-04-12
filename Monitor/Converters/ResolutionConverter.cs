using System;
using System.Globalization;
using System.Windows.Data;
using LiveCharts.Helpers;
using QuantConnect;

namespace Monitor.Converters
{
    [ValueConversion(typeof(Resolution), typeof(PeriodUnit))]
    public class ResolutionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (targetType != typeof(PeriodUnit))
                throw new InvalidOperationException("The target must be a SeriesResolution");

            switch ((Resolution) value)
            {
                case Resolution.Second:
                    return PeriodUnit.Seconds;

                case Resolution.Minute:
                    return PeriodUnit.Minutes;

                case Resolution.Hour:
                    return PeriodUnit.Hours;

                case Resolution.Daily:
                    return PeriodUnit.Days;

                case Resolution.Tick:
                    return PeriodUnit.Ticks;
               
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (targetType != typeof(Resolution))
                throw new InvalidOperationException("The target must be a Resolution");

            switch ((PeriodUnit) value)
            {
                case PeriodUnit.Seconds:
                    return Resolution.Second;

                case PeriodUnit.Minutes:
                    return Resolution.Minute;

                case PeriodUnit.Hours:
                    return Resolution.Hour;

                case PeriodUnit.Days:
                    return Resolution.Daily;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}