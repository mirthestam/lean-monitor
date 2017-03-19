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

    }
}