using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Monitor.Model.Charting;
using Monitor.Utils;

namespace Monitor.ViewModel.Charts
{
    public class ChildChartViewModel : ViewModelBase
    {
        private SeriesCollection _seriesCollection = new SeriesCollection();
        private AxesCollection _yAxesCollection = new AxesCollection();
        private VisualElementsCollection _visualElementsCollection = new VisualElementsCollection();

        private int _index;

        public Dictionary<string, TimeStamp> LastUpdates { get; } = new Dictionary<string, TimeStamp>();

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

        public VisualElementsCollection VisualElementsCollection
        {
            get { return _visualElementsCollection; }
            set
            {
                _visualElementsCollection = value;
                RaisePropertyChanged();
            }
        }

        public void CreateTruncatedVisuaLElement(int axisX, TimeStamp x, decimal y)
        {
            _visualElementsCollection.Add(new VisualElement
            {
                X = x.ElapsedTicks / Parent.AxisModifier,
                Y = (double)y,
                UIElement = new Image
                {
                    ToolTip = $"This series is possibly truncated by the lean Engine due to a maximum number of points ({ 8000 }) ",
                    Width = 16,
                    Source = (BitmapImage)Application.Current.Resources["AttentionBitmapImage"],                    
                }
            });
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