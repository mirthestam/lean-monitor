using GalaSoft.MvvmLight;
using LiveCharts;
using LiveCharts.Wpf;

namespace QuantConnect.Lean.Monitor.ViewModel.Charts
{
    public class ChildChartViewModel : ViewModelBase
    {
        private SeriesCollection _seriesCollection = new SeriesCollection();
        private AxesCollection _yAxesCollection = new AxesCollection();
        private int _index;

        public ChildChartViewModel(int index, ChartViewModel chart)
        {
            _index = index;
            _parent = chart;
        }

        private ChartViewModel _parent;

        public ChartViewModel Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                RaisePropertyChanged();
            }
        }

        public AxesCollection YAxesCollection
        {
            get { return _yAxesCollection; }
            set
            {
                _yAxesCollection = value;
                RaisePropertyChanged();
            }
        }

        public SeriesCollection SeriesCollection
        {
            get { return _seriesCollection; }
            set
            {
                _seriesCollection = value;
                RaisePropertyChanged();
            }
        }

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                RaisePropertyChanged();
            }
        }

        public bool ShowLabels => Index == 0;

        public bool ShowLegend => false;
    }
}