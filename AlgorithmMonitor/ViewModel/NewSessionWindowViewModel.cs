using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using QuantConnect.Lean.Monitor.Model;
using QuantConnect.Lean.Monitor.Model.Sessions;

namespace QuantConnect.Lean.Monitor.ViewModel
{
    public class NewSessionWindowViewModel : ViewModelBase
    {
        // tab indexes are a bit of Mvvm violation. 
        // Could have used a property indicating mode is stream of file.
        private const int STREAM_TAB = 0;
        private const int FILE_TAB = 1;

        private readonly ISessionService _sessionService;

        // TODO: Use provider for default / last used parameters
        private readonly FileSessionParameters _fileSessionParameters = new FileSessionParameters
        {
            FileName = "Demo\\DemoAlgorithm.json",
            Watch = true
        };

        private readonly StreamSessionParameters _streamSessionParameters = new StreamSessionParameters
        {
            Host = "localhost",
            Port = 1234
        };

        private int _tabIndex = STREAM_TAB;

        public NewSessionWindowViewModel(ISessionService sessionService)
        {
            _sessionService = sessionService;
            OpenCommand = new RelayCommand(Open, ValidateOpen);
        }

        public RelayCommand OpenCommand { get; private set; }
            
        public string Host
        {
            get { return _streamSessionParameters.Host; }
            set
            {
                _streamSessionParameters.Host = value;
                RaisePropertyChanged();
            }
        }

        public string Port
        {
            get { return _streamSessionParameters.Port.ToString(); }
            set
            {
                int port;
                if (!int.TryParse(value, out port)) return;

                _streamSessionParameters.Port = port;
                RaisePropertyChanged();
            }
        }

        public string FileName
        {
            get { return _fileSessionParameters.FileName; }
            set
            {
                _fileSessionParameters.FileName = value;
                RaisePropertyChanged();
            }
        }

        public bool Watch
        {
            get { return _fileSessionParameters.Watch; }
            set
            {
                _fileSessionParameters.Watch = value;
                RaisePropertyChanged();
            }
        }

        public int TabIndex
        {
            get { return _tabIndex; }
            set
            {
                _tabIndex = value;
                RaisePropertyChanged();
            }
        }

        private void Open()
        {
            switch (TabIndex)
            {
                case STREAM_TAB:
                    _sessionService.OpenStream(_streamSessionParameters);
                    break;

                case FILE_TAB:
                    _sessionService.OpenFile(_fileSessionParameters);                  
                    break;
            }
        }

        private bool ValidateOpen()
        {
            switch (TabIndex)
            {
                case STREAM_TAB:
                    if (string.IsNullOrWhiteSpace(Host)) return false;
                    int port;
                    if (!int.TryParse(Port, out port)) return false;
                    break;

                case FILE_TAB:
                    if (string.IsNullOrWhiteSpace(FileName)) return false;
                    break;
            }

            return true;
        }
    }
}
