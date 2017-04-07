using System;
using GalaSoft.MvvmLight;
using NodaTime;
using QuantConnect;

namespace Monitor.ViewModel.Charts
{
    /// <summary>
    /// Chart component responsible for the zoom state
    /// </summary>
    public class ZoomChartComponent : ViewModelBase
    {
        private readonly IChartView _view;

        public Instant StartPoint { get; set; } = Instant.FromUnixTimeSeconds(0);

        private double _zoomFrom;
        private double _zoomTo = 1;
        
        public ZoomChartComponent(IChartView view)
        {
            _view = view;            
        }        

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

        public void AutoZoom()
        {
            if (ZoomTo == 1)
            {
                // Zoom to the known number of values.
                ZoomTo = _view.LastUpdates["Scroll"].ToUnixTimeTicks() / _view.AxisModifier;

                double diff;

                // Determine a default scale
                switch (_view.Resolution)
                {
                    case Resolution.Second:
                    case Resolution.Minute:
                    case Resolution.Hour:
                        // Show approx. a month (actual month length can differ, but for zooming this is acceptable
                        diff = (NodaConstants.TicksPerDay * 31) / _view.AxisModifier; 
                        break;

                    default:
                        // Show approx. two months (actual month length can differ, but for zooming this is acceptable
                        diff = (NodaConstants.TicksPerDay * 60) / _view.AxisModifier;
                        break;
                }

                ZoomFrom = ZoomTo - diff;
            }
            else if (!_view.IsPositionLocked)
            {
                // Scroll to latest data
                var diff = ZoomTo - ZoomFrom;
                ZoomTo = _view.LastUpdates["Scroll"].ToUnixTimeTicks() / _view.AxisModifier;
                ZoomFrom = ZoomTo - diff;
            }
        }

        public void ZoomToFit()
        {
            ZoomFrom = StartPoint.ToUnixTimeTicks() / _view.AxisModifier;
            ZoomTo = Math.Max(1, _view.LastUpdates["Scroll"].ToUnixTimeTicks() / _view.AxisModifier);
        }
    }
}