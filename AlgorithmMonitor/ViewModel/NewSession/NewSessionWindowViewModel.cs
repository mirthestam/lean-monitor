using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Monitor.Model.Api;
using Monitor.Model.Sessions;

namespace Monitor.ViewModel.NewSession
{
    public class NewSessionWindowViewModel : ViewModelBase
    {
        // tab indexes are a bit of Mvvm violation. 
        // Could have used a property indicating mode is stream of file.
        private const int API_TAB = 0;
        private const int STREAM_TAB = 1;
        private const int FILE_TAB = 2;

        private readonly ISessionService _sessionService;

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

        public NewApiSessionViewModel NewApiSession { get; }

        private int _tabIndex = API_TAB;

        public NewSessionWindowViewModel(ISessionService sessionService, NewApiSessionViewModel newApiSessionViewModel)
        {
            _sessionService = sessionService;
            NewApiSession = newApiSessionViewModel;
            OpenCommand = new RelayCommand(Open, ValidateOpen);
        }

        public RelayCommand OpenCommand { get; private set; }

        public string StreamHost
        {
            get { return _streamSessionParameters.Host; }
            set
            {
                _streamSessionParameters.Host = value;
                RaisePropertyChanged();
            }
        }

        public string StreamPort
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

        public bool FileWatch
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

                case API_TAB:
                    _sessionService.OpenApi(new ApiSessionParameters
                    {
                        InstanceId = NewApiSession.SelectedInstance.Id,
                        ProjectId = NewApiSession.SelectedProject.ProjectId,
                        InstanceType = NewApiSession.SelectedInstance.Type
                    });
                    break;
            }
        }

        private bool ValidateOpen()
        {
            switch (TabIndex)
            {
                case STREAM_TAB:
                    if (string.IsNullOrWhiteSpace(StreamHost)) return false;
                    int port;
                    if (!int.TryParse(StreamPort, out port)) return false;
                    break;

                case FILE_TAB:
                    if (string.IsNullOrWhiteSpace(FileName)) return false;
                    break;

                case API_TAB:
                    break;
            }

            return true;
        }
    }
}
