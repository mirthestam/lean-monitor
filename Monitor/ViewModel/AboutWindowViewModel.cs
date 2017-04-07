using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Monitor.ViewModel
{
    public class AboutWindowViewModel : ViewModelBase
    {
        public RelayCommand<string> BrowseCommand { get; private set; }

        public AboutWindowViewModel()
        {
            // Bind the command to the browse method
            BrowseCommand = new RelayCommand<string>(Browse);
        }

        private static void Browse(string uri)
        {
            // Use windows to handle the URI.
            // It will start the default web browser application
            Process.Start(uri);
        }
    }
}
