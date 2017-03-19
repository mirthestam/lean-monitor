using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts.Configurations;
using QuantConnect.Lean.Monitor.Model.Charting;
using QuantConnect.Lean.Monitor.Model.Messages;
using QuantConnect.Lean.Monitor.Utils;

namespace QuantConnect.Lean.Monitor.ViewModel.Charts
{
    /// <summary>
    /// Base view model for charts (i.e. generic, Strategy Equity, Benchmark)
    /// </summary>
    public abstract class ChartViewModelBase : DocumentViewModel, ITimeStampSource
    {
        private const string SecondLabelFormat = "yyyy-MM-dd HH:mm:ss";
        private const string MinuteLabelFormat = "yyyy-MM-dd HH:mm";
        private const string HourLabelFormat = "yyyy-MM-dd HH:00";
        private const string DayLabelFormat = "yyyy-MM-dd";

        private int _zoomFrom;
        private int _zoomTo = 1;
        private TimeStamp _lastUpdated;

        /// <summary>
        /// Gets the list of TimeStamps. TimeStamps are our primarily X axis upon which all series indexes are mapped
        /// </summary>
        protected List<TimeStamp> TimeStamps { get; } = new List<TimeStamp>();

        private IPointEvaluator<TimeStampOhlcChartPoint> _ohlcChartPointEvaluator;
        private IPointEvaluator<TimeStampChartPoint> _chartPointEvaluator;

        public RelayCommand ShowGridCommand { get; private set; }

        public ChartResolution Resolution { get; set; } = ChartResolution.Day;

        public int ZoomTo
        {
            get { return _zoomTo; }
            set
            {
                _zoomTo = value;
                RaisePropertyChanged();
            }
        }

        public int ZoomFrom
        {
            get { return _zoomFrom; }
            set
            {
                _zoomFrom = value;
                RaisePropertyChanged();
            }
        }

        public TimeStamp LastUpdated
        {
            get { return _lastUpdated; }
            set
            {
                _lastUpdated = value;
                RaisePropertyChanged();
            }
        }

        public Func<double, string> XFormatter { get; set; }
        
        public IPointEvaluator<TimeStampChartPoint> ChartPointEvaluator => _chartPointEvaluator ?? (_chartPointEvaluator = new TimeStampChartPointMapper(this));

        public IPointEvaluator<TimeStampOhlcChartPoint> OhlcChartPointEvaluator => _ohlcChartPointEvaluator ?? (_ohlcChartPointEvaluator = new OhlcTimeStampChartPointMapper(this));

        protected ChartViewModelBase()
        {
            ShowGridCommand = new RelayCommand(() => Messenger.Default.Send(new GridRequestMessage(Key)));
            XFormatter = val => FormatXLabel((int)val);
        }

        public int IndexOf(TimeStamp item)
        {
            // We switch by resolution, because sometimes series differ a bit from the master series. Find the nearest point based upon the resolution
            // TODO: Investigate whether this is the best approach. Alternative could be to merge all series relative to each other.
            switch (Resolution)
            {
                case ChartResolution.Second:
                    return TimeStamps.FindIndex(ts => ts.ElapsedSeconds == item.ElapsedSeconds);
                    
                case ChartResolution.Minute:
                    return TimeStamps.FindIndex(ts => ts.ElapsedMinutes == item.ElapsedMinutes);

                case ChartResolution.Hour:
                    return TimeStamps.FindIndex(ts => ts.ElapsedHours == item.ElapsedHours);

                case ChartResolution.Day:
                    return TimeStamps.FindIndex(ts => ts.ElapsedDays == item.ElapsedDays);

                default:
                    throw new ArgumentOutOfRangeException();
            }            
        }

        public TimeStamp GetTimeStamp(int index)
        {
            index = Math.Min(index, TimeStamps.Count - 1);
            return TimeStamps[index];
        }

        private string FormatXLabel(int x)
        {
            // When zooming out, It might be the case the chart wants labels for unbound data.
            // Fall back to the first date available
            if (x < 0) { x = 0; }

            // Use a dummy timestamp in design time mode.
            // Otherwise let the derived implementation determine a timestamp for the X index
            var timeStamp = IsInDesignMode ? new TimeStamp() : GetTimeStamp(x);

            // Pick a format string based upon the resolution of the data.
            string format;
            switch (Resolution)
            {
                case ChartResolution.Second:
                    format = SecondLabelFormat;
                    break;

                case ChartResolution.Minute:
                    format = MinuteLabelFormat;
                    break;

                case ChartResolution.Hour:
                    format = HourLabelFormat;
                    break;

                case ChartResolution.Day:
                    format = DayLabelFormat;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return timeStamp.DateTime.ToString(format);
        }
    }
}