using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts.Configurations;
using Monitor.Model;
using Monitor.Model.Charting;
using Monitor.Model.Messages;
using Monitor.Utils;

namespace Monitor.ViewModel.Charts
{
    /// <summary>
    /// Base view model for charts (i.e. generic, Strategy Equity, Benchmark)
    /// </summary>
    public abstract class ChartViewModelBase : DocumentViewModel, IResolutionProvider
    {
        private double _zoomFrom;
        private double _zoomTo = 1;
        private bool _isPositionLocked;

        private IPointEvaluator<TimeStampOhlcChartPoint> _ohlcChartPointEvaluator;
        private IPointEvaluator<TimeStampChartPoint> _chartPointEvaluator;

        public RelayCommand ShowGridCommand { get; private set; }

        public RelayCommand ZoomFitCommand { get; private set; }

        public Resolution Resolution { get; set; } = Resolution.Day;

        public double ZoomTo
        {
            get { return _zoomTo; }
            set
            {
                _zoomTo = value;
                RaisePropertyChanged();
            }
        }

        public double ZoomFrom
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
        
        public IPointEvaluator<TimeStampChartPoint> ChartPointEvaluator => _chartPointEvaluator ?? (_chartPointEvaluator = new TimeStampChartPointMapper(this));

        public IPointEvaluator<TimeStampOhlcChartPoint> OhlcChartPointEvaluator => _ohlcChartPointEvaluator ?? (_ohlcChartPointEvaluator = new OhlcTimeStampChartPointMapper(this));

        protected ChartViewModelBase()
        {
            ShowGridCommand = new RelayCommand(() => Messenger.Default.Send(new GridRequestMessage(Key)));
            ZoomFitCommand = new RelayCommand(ZoomToFit);
        }

        public double AxisModifier
        {
            get
            {
                switch (Resolution)
                {
                    case Resolution.Second:
                        return TimeSpan.TicksPerSecond;

                    case Resolution.Minute:
                        return TimeSpan.TicksPerMinute;

                    case Resolution.Hour:
                        return TimeSpan.TicksPerHour;

                    case Resolution.Day:
                        return TimeSpan.TicksPerDay;

                    case Resolution.Ticks:
                        return 1;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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

        protected abstract void ZoomToFit();       
    }
}