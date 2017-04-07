using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using Monitor.Model;
using Monitor.Model.Messages;
using Monitor.Model.Sessions;
using Monitor.ViewModel.Charts;
using Monitor.ViewModel.Grids;

namespace Monitor.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ISessionService _sessionService;
        private readonly IMessenger _messenger;

        private SessionState _sessionState = SessionState.Unsubscribed;

        private ObservableCollection<DocumentViewModel> _charts = new ObservableCollection<DocumentViewModel>();

        public RelayCommand ExitCommand { get; private set; }
        public RelayCommand OpenSessionCommand { get; private set; }
        public RelayCommand CloseCommand { get; }
        public RelayCommand ExportCommand { get; }

        public RelayCommand ConnectCommand { get; }
        public RelayCommand DisconnectCommand { get; }

        public ObservableCollection<DocumentViewModel> Charts
        {
            get { return _charts; }
            set
            {
                _charts = value;
                RaisePropertyChanged();
            }
        }

        public bool IsSessionActive => _sessionService.IsSessionActive;

        public SessionState SessionState
        {
            get { return _sessionState; }
            set
            {
                _sessionState = value;
                RaisePropertyChanged();
            }
        }

        public StatusViewModel StatusViewModel { get; }

        public MainWindowViewModel(ISessionService resultService, IMessenger messenger, StatusViewModel statusViewModel)
        {
            StatusViewModel = statusViewModel;
            _sessionService = resultService;
            _messenger = messenger;

            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());
            CloseCommand = new RelayCommand(() => _sessionService.ShutdownSession(), () => IsSessionActive);
            OpenSessionCommand = new RelayCommand(() => _messenger.Send(new ShowNewSessionWindowMessage()));
            ExportCommand = new RelayCommand(Export, () => IsSessionActive);
            ConnectCommand = new RelayCommand(() => _sessionService.IsSessionSubscribed = true, () => _sessionState != SessionState.Subscribed && _sessionService.CanSubscribe);
            DisconnectCommand = new RelayCommand(() => _sessionService.IsSessionSubscribed = false, () => _sessionState != SessionState.Unsubscribed);

            _messenger.Register<SessionOpenedMessage>(this, message => InvalidateCommands());

            _messenger.Register<SessionClosedMessage>(this, message =>
            {
                SessionState = SessionState.Unsubscribed;
                Charts.Clear();
                InvalidateCommands();
            });

            _messenger.Register<SessionStateChangedMessage>(this, message =>
            {
                SessionState = message.State;
                InvalidateCommands();
            });

            _messenger.Register<SessionUpdateMessage>(this, message =>
            {
                try
                {
                    lock (_charts)
                    {
                        ParseResult(message.ResultContext.Result);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            _messenger.Register<GridRequestMessage>(this, message =>
            {
                var chartTableViewModel = new GridPanelViewModel
                {
                    Key = message.Key
                };

                // Calcualte the index for this tab
                var index = Charts.IndexOf(Charts.First(c => c.Key == message.Key));
                Charts.Insert(index, chartTableViewModel);

                chartTableViewModel.IsSelected = true;

                // Get the latest data for this tab and inject it
                var chart = _sessionService.LastResult.Charts[message.Key];
                chartTableViewModel.ParseChart(chart);
            });
        }

        public void Initialize()
        {
            _sessionService.Initialize();
        }

        public void HandleDroppedFileName(string fileName)
        {
            _sessionService.OpenFile(new FileSessionParameters
            {
                FileName = fileName,
                Watch = true
            });
        }

        private void ParseResult(Result messageResult)
        {
            foreach (var chart in messageResult.Charts
                .OrderBy(c => c.Key != "Strategy Equity")
                .ThenBy(c => c.Key != "Benchmark")
                .ThenBy(c => c.Key)
                .ToList())
            {
                if (chart.Value.Series.Count == 0)
                {
                    // This chart has no data.
                    // In example, a StockPlot:SYMBOL chart for which no subscription exists.
                }

                var chartDrawViewModel = Charts.OfType<ChartPanelViewModel>().SingleOrDefault(c => c.Key == chart.Key);
                if (chartDrawViewModel == null)
                {
                    chartDrawViewModel = new ChartPanelViewModel(_messenger)
                    {
                        Key = chart.Key
                    };
                    Charts.Add(chartDrawViewModel);
                }

                var chartTableViewModel = Charts.OfType<GridPanelViewModelBase>().SingleOrDefault(c => c.Key == chart.Key);

                // Some viewmodels need the full result.
                // Others just need a single chart
                try
                {
                    (chartDrawViewModel as IChartView)?.ParseChart(chart.Value);
                    (chartTableViewModel as IChartView)?.ParseChart(chart.Value);
                }
                catch (Exception e)
                {
                    // This individual chart failed to update. Log and continue
                    Console.WriteLine(e);
                }
            }

            RaisePropertyChanged(() => Charts);
        }

        private void Export()
        {           
            var exportDialog = new SaveFileDialog
            {
                FileName = DateTime.Now.ToString("yyyyMMddHHmm") + "_export",
                DefaultExt = ".json",
                Filter = "Json documents (.json)|*.json"
            };

            var dialogResult = exportDialog.ShowDialog();
            if (dialogResult != true) return;
            
            var serializer = new ResultSerializer(new ResultConverter());
            var serialized = serializer.Serialize(_sessionService.LastResult);
            File.WriteAllText(exportDialog.FileName, serialized);
        }

        private void InvalidateCommands()
        {
            RaisePropertyChanged(() => IsSessionActive);
            CloseCommand.RaiseCanExecuteChanged();
            ExportCommand.RaiseCanExecuteChanged();
            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();            
        }
    }
}