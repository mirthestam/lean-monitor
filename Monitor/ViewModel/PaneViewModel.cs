using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace Monitor.ViewModel
{
    public abstract class DocumentPaneViewModel : PaneViewModel
    {
        private bool _canClose;
        private string _key;

        public bool CanClose
        {
            get { return _canClose; }
            set
            {
                if (_canClose == value) return;
                _canClose = value;
                RaisePropertyChanged();
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                RaisePropertyChanged();
            }
        }

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                RaisePropertyChanged();
            }
        }
    }

    public abstract class ToolPaneViewModel : PaneViewModel
    {
        private bool _isVisible = true;

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                RaisePropertyChanged();
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                RaisePropertyChanged();
            }
        }
    }

    /// <summary>
    /// View model for a docking pane
    /// </summary>
    public abstract class PaneViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _isActive;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;

                _isSelected = value;
                RaisePropertyChanged();
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                RaisePropertyChanged();
            }
        }
    }
}
