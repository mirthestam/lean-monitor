using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Monitor.Model;
using Monitor.Model.Messages;
using Monitor.Model.Sessions;

namespace Monitor.ViewModel
{
    public class StatusViewModel : ViewModelBase
    {
        private readonly IMessenger _messenger;
        private readonly ISessionService _sessionService;

        private bool _isProgressIndeterminate;
        private decimal _progress;
        private string _sessionName;
        private string _projectName;
        private SessionState _sessionState = SessionState.Unsubscribed;

        public StatusViewModel(IMessenger messenger, ISessionService sessionService) : base(messenger)
        {
            _messenger = messenger;
            _sessionService = sessionService;

            _messenger.Register<SessionOpenedMessage>(this, message =>
            {
                Progress = 0;
                IsProgressIndeterminate = false;
                RaisePropertyChanged(() => IsSessionActive);
            });

            _messenger.Register<SessionClosedMessage>(this, message =>
            {
                SessionName = string.Empty;
                ProjectName = string.Empty;
                SessionState = SessionState.Unsubscribed;
                RaisePropertyChanged(() => IsSessionActive);
            });

            _messenger.Register<SessionStateChangedMessage>(this, message =>
            {
                SessionState = message.State;
            });

            _messenger.Register<SessionUpdateMessage>(this, message =>
            {
                Progress = message.ResultContext.Progress;
                SessionName = message.ResultContext.Name;
                ProjectName = message.ResultContext.Project;

                switch (message.ResultContext.Result.ResultType)
                {
                    case ResultType.Backtest:
                        IsProgressIndeterminate = false;
                        return;

                    case ResultType.Live:
                        IsProgressIndeterminate = true;
                        return;
                }
            });
        }

        public decimal Progress
        {
            get { return _progress;  }
            set
            {
                _progress = value;
                RaisePropertyChanged();
            }
        }

        public string SessionName
        {
            get { return _sessionName; }
            set
            {
                if (_sessionName == value) return;
                _sessionName = value;
                RaisePropertyChanged();
            }
        }

        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                if (_projectName == value) return;
                _projectName = value;
                RaisePropertyChanged();
            }
        }

        public bool IsProgressIndeterminate
        {
            get { return _isProgressIndeterminate; }
            set
            {
                if (_isProgressIndeterminate == value) return;
                _isProgressIndeterminate = value;
                RaisePropertyChanged();
            }
        }

        public SessionState SessionState
        {
            get { return _sessionState; }
            set
            {
                _sessionState = value;
                RaisePropertyChanged();
            }
        }

        public bool IsSessionActive => _sessionService.IsSessionActive;
    }
}