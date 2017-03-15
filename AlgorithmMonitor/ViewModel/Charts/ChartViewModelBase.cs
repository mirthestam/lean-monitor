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
    public abstract class ChartViewModelBase : DocumentViewModel
    {
        private int _zoomFrom;
        private int _zoomTo = 1;
        private TimeStamp _lastUpdated;

        public RelayCommand ShowGridCommand { get; private set; }

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

        // We use the index, otherwise missing items will cause blanks in the graph. 
        // The GetXTimeStamp function allows for implementations to relate back to the actual timestamp
        public IPointEvaluator<TimeStampChartPoint> ChartPointMapper { get; } = new CartesianMapper<TimeStampChartPoint>()
            .X((m, index) => index)
            .Y(m => decimal.ToDouble(m.Y));                    

        public IPointEvaluator<TimeStampOhlcChartPoint> OhlcChartPointMapper { get; } = new FinancialMapper <TimeStampOhlcChartPoint>()
            .X((m, index) => index)
            .Open(m => m.Open)
            .Close(m => m.Close)
            .High(m => m.High)
            .Low(m => m.Low);

        protected ChartViewModelBase()
        {
            ShowGridCommand = new RelayCommand(() => Messenger.Default.Send(new GridRequestMessage(Key)));
        }
    }
}