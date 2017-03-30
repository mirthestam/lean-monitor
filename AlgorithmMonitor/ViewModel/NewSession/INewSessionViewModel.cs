using GalaSoft.MvvmLight.Command;

namespace Monitor.ViewModel.NewSession
{
    public interface INewSessionViewModel
    {
        RelayCommand OpenCommand { get; }

        string Header { get; }
    }
}