using System;
using System.Globalization;
using GalaSoft.MvvmLight;
using QuantConnect.Lean.Monitor.Model.Charting;
using QuantConnect.Lean.Monitor.Utils;

namespace QuantConnect.Lean.Monitor.ViewModel.Charts
{    
    /// <summary>
    /// Abstract view model for document tabs. Documents are based upon chart data (i.e. charts, grids)
    /// </summary>
    public abstract class DocumentViewModel : ViewModelBase
    {
        private string _title;
        private string _key;
        private bool _isSelected;

        private const string SecondLabelFormat = "yyyy-MM-dd HH:mm:ss";
        private const string MinuteLabelFormat = "yyyy-MM-dd HH:mm";
        private const string HourLabelFormat = "yyyy-MM-dd HH:00";
        private const string DayLabelFormat = "yyyy-MM-dd";

        public ChartResolution Resolution { get; set; } = ChartResolution.Day;

        public Func<double, string> XFormatter { get; set; }

        protected DocumentViewModel()
        {
            // Configure the X formatter
            XFormatter = val => FormatXLabel((int)val);
        }

        /// <summary>
        /// The display name for the document tab
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// They key of the document shown (i.e. the Key of the Chart)
        /// </summary>
        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether this DocumentTab is active (selected)
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Implementations can control whether this document tab can be closed
        /// </summary>
        public abstract bool CanClose { get; }

        private string FormatXLabel(int x)
        {
            // When zooming out, It might be the case the chart wants labels for unbound data.
            // Fall back to the first date available
            if (x < 0) { x = 0; }

            // Use a dummy timestamp in design time mode.
            // Otherwise let the derived implementation determine a timestamp for the X index
            var timeStamp = IsInDesignMode ? new TimeStamp() : GetXTimeStamp(x);

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

        /// <summary>
        /// Implementations map this index to a timestamp from their loaded data
        /// </summary>
        protected abstract TimeStamp GetXTimeStamp(int index);
    }
}