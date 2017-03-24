using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts.Configurations;
using LiveCharts.Geared;
using Monitor.Model;
using Monitor.Model.Charting;
using Monitor.Model.Messages;
using Monitor.Utils;

namespace Monitor.ViewModel.Charts
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
        private bool _isPositionLocked;

        /// <summary>
        /// Gets the list of TimeStamps. TimeStamps are our primarily X axis upon which all series indexes are mapped
        /// </summary>
        protected List<TimeStamp> TimeStamps { get; } = new List<TimeStamp>();
        protected Dictionary<TimeStamp, int> TimeStampIndexes { get; } = new Dictionary<TimeStamp, int>();

        private IPointEvaluator<TimeStampOhlcChartPoint> _ohlcChartPointEvaluator;
        private IPointEvaluator<TimeStampChartPoint> _chartPointEvaluator;

        public RelayCommand ShowGridCommand { get; private set; }

        public RelayCommand ZoomFitCommand { get; private set; }

        public Model.Resolution Resolution { get; set; } = Model.Resolution.Day;

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

        public bool IsPositionLocked
        {
            get { return _isPositionLocked; }
            set
            {
                _isPositionLocked = value;
                RaisePropertyChanged();
            }
        }

        public Func<double, string> XFormatter { get; set; }
        
        public IPointEvaluator<TimeStampChartPoint> ChartPointEvaluator => _chartPointEvaluator ?? (_chartPointEvaluator = new TimeStampChartPointMapper(this));

        public IPointEvaluator<TimeStampOhlcChartPoint> OhlcChartPointEvaluator => _ohlcChartPointEvaluator ?? (_ohlcChartPointEvaluator = new OhlcTimeStampChartPointMapper(this));

        protected ChartViewModelBase()
        {
            ShowGridCommand = new RelayCommand(() => Messenger.Default.Send(new GridRequestMessage(Key)));
            ZoomFitCommand = new RelayCommand(ZoomToFit);
            XFormatter = val => FormatXLabel((int)val);
        }

        public void UpdateExistingOhlcPoints(IList<TimeStampOhlcChartPoint> existingPoints, IList<TimeStampOhlcChartPoint> updatedPoints, Resolution resolution)
        {
            // Check whether we are updating existing points
            if (existingPoints.Count <= 0) return;

            if (resolution != Resolution.Day)
            {
                throw new ArgumentOutOfRangeException($"Resolution {resolution} is not supported. Only Day is supported.");
            }

            // Check whether we have new information for the last ohlc point
            var lastKnownDay = existingPoints.Last().X.ElapsedDays;
            while (updatedPoints.Any() && (updatedPoints.First().X.ElapsedDays <= lastKnownDay)) // We assume we always show ohlc in day groups
            {
                // Update the last ohlc point with this inforrmation
                var refval = updatedPoints.First();

                // find the value matching this day
                var ohlcEquityChartValue = existingPoints.Last();

                // Update ohlc point with highest and lowest, and with the new closing price
                // Update the normal point with the new closing value
                ohlcEquityChartValue.High = Math.Max(refval.High, ohlcEquityChartValue.High);
                ohlcEquityChartValue.Low = Math.Min(refval.Low, ohlcEquityChartValue.Low);
                ohlcEquityChartValue.Close = refval.Close;

                // Remove this value, as it has been parsed into existing chart points
                updatedPoints.RemoveAt(0);
            }
        }

        public int IndexOf(TimeStamp item)
        {
            int index;
            if (!TimeStampIndexes.TryGetValue(item, out index))
            {
                return -1;
            }
            return index;
        }

        public TimeStamp GetTimeStamp(int index)
        {
            index = Math.Min(index, TimeStamps.Count - 1);
            return index < 0 ? TimeStamp.MinValue : TimeStamps[index];
        }

        protected void RebuildTimeStampIndex()
        {
            // The TimeStampIndex is used to quickly find the X index for timestamps.
            TimeStampIndexes.Clear();
            for (var i = 0; i < TimeStamps.Count; i++)
            {
                var ts = TimeStamps[i];
                TimeStampIndexes[ts] = i;
            }
        }

        protected abstract void ZoomToFit();

        private string FormatXLabel(int x)
        {
            // When zooming out, It might be the case the chart wants labels for unbound data.
            // Fall back to the first date available
            if (x < 0) { x = 0; }

            // Use a dummy timestamp in design time mode.
            // Otherwise let the derived implementation determine a timestamp for the X index
            var timeStamp = IsInDesignMode ? new TimeStamp(TimeSpan.Zero) : GetTimeStamp(x);

            // Pick a format string based upon the resolution of the data.
            string format;
            switch (Resolution)
            {
                case Model.Resolution.Second:
                    format = SecondLabelFormat;
                    break;

                case Model.Resolution.Minute:
                    format = MinuteLabelFormat;
                    break;

                case Model.Resolution.Hour:
                    format = HourLabelFormat;
                    break;

                case Model.Resolution.Day:
                    format = DayLabelFormat;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return timeStamp.DateTime.ToString(format);
        }        
    }
}