using System;
using System.Threading;
using System.Threading.Tasks;
using Monitor.Model.Api;

namespace Monitor.Model.Sessions
{
    public class ApiSession : ISession
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionHandler _sessionHandler;
        private readonly ApiSessionParameters _parameters;
        private readonly Result _result = new Result();
        private readonly SynchronizationContext _syncContext;

        private SessionState _state = SessionState.Unsubscribed;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task _poller;

        public ApiSession(ISessionHandler sessionHandler, IApiClient apiClient, ApiSessionParameters parameters)
        {
            _apiClient = apiClient;
            _sessionHandler = sessionHandler;
            _parameters = parameters;

            _syncContext = SynchronizationContext.Current;
        }

        public void Initialize()
        {
            Subscribe();
        }

        public void Shutdown()
        {
            Unsubscribe();
        }

        public void Subscribe()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _poller = Task.Factory.StartNew(() =>
            {
                _syncContext.Send(o => State = SessionState.Subscribed, null);

                while (true)
                {
                    Thread.Sleep(500); 
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        _syncContext.Send(o => State = SessionState.Unsubscribed, null);
                        break;
                    }
                    FetchLatestResult().Wait();
                }
            }, _cancellationToken);
        }

        public void Unsubscribe()
        {
            // Request a cancel
            _cancellationTokenSource?.Cancel();
        }

        private async Task FetchLatestResult()
        {
            switch (_parameters.InstanceType)
            {
                case ResultType.Backtest:
                    await FetchBacktestResult();
                    break;

                case ResultType.Live:
                    throw new NotSupportedException();
            }
        }

        private async Task FetchBacktestResult()
        {
            var resultUpdate = await _apiClient.GetResultAsync(_parameters.ProjectId, _parameters.InstanceId,
                ResultType.Backtest);

            _result.Add(resultUpdate.Result);

            var context = new ResultContext
            {
                Result = _result,
                Progress = resultUpdate.Progress, // Somehow current API has NULL in this field
                Project = resultUpdate.Project, // Somehow current API has NULL in this field
                Name = resultUpdate.Name ?? Name
            };

            _syncContext.Send(o => _sessionHandler.HandleResult(context), null);

            if (resultUpdate.Completed)
            {
                // Unsubscribe as this backtest has completed (and we have parsed all data)
                _syncContext.Send(o => Unsubscribe(), null);
            }
        }

        public string Name { get; } = "";

        public SessionState State
        {
            get { return _state; }
            private set
            {
                _state = value;
                _sessionHandler.HandleStateChanged(value);
            }
        }

        public bool CanSubscribe { get; } = false; // Do not allow to resubscribe manually
    }
}