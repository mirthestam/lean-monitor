using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace Monitor.ViewModel.NewSession
{
    public class NewSessionWindowViewModel : ViewModelBase
    {
        private ObservableCollection<INewSessionViewModel> _newSessionViewModels;
        private INewSessionViewModel _selectedViewModel;

        public ObservableCollection<INewSessionViewModel> NewSessionViewModels
        {
            get
            {
                return _newSessionViewModels;
            }
            set
            {
                _newSessionViewModels = value;
                RaisePropertyChanged();
            }
        }

        public INewSessionViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                _selectedViewModel = value;
                RaisePropertyChanged();
            }
        }

        public NewSessionWindowViewModel(IEnumerable<INewSessionViewModel> newSessionViewModels)
        {
            NewSessionViewModels = new ObservableCollection<INewSessionViewModel>(newSessionViewModels);            
        }
    }
}
