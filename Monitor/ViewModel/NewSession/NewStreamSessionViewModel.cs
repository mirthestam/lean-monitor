using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Monitor.Model.Sessions;
using Monitor.Properties;

namespace Monitor.ViewModel.NewSession
{
    public class NewStreamSessionViewModel : ViewModelBase, INewSessionViewModel, IDataErrorInfo
    {
        private readonly ISessionService _sessionService;
        private string _host = Settings.Default.StreamHostDefault;
        private string _port = Settings.Default.StreamPortDefault;

        public NewStreamSessionViewModel(ISessionService sessionService)
        {
            _sessionService = sessionService;

            OpenCommand = new RelayCommand(Open, CanOpen);
        }

        private void Open()
        {
            _sessionService.OpenStream(new StreamSessionParameters
            {
                CloseAfterCompleted = true,
                Host = Host,
                Port =  int.Parse(Port)
            });
        }

        private bool CanOpen()
        {
            var fieldsToValidate = new[]
            {
                nameof(Host),
                nameof(Port),
            };

            return fieldsToValidate.All(field => string.IsNullOrEmpty(this[field]));
        }

        public string Host
        {
            get { return _host; }
            set
            {
                _host = value;
                RaisePropertyChanged();
                OpenCommand.RaiseCanExecuteChanged();
            }
        }

        public string Port
        {
            get { return _port; }
            set
            {
                _port = value;
                RaisePropertyChanged();
                OpenCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand OpenCommand { get; }

        public string Header { get; } = "From Stream";

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Host):
                        if (string.IsNullOrWhiteSpace(Host)) return "Host is required";
                        break;

                    case nameof(Port):
                        if (string.IsNullOrWhiteSpace(Port)) return "Port is required";
                        int port;
                        if (!int.TryParse(Port, out port)) return "Port should be numeric";
                        break;
                }

                return string.Empty;
            }
        }

        public string Error { get; } = null;
    }
}