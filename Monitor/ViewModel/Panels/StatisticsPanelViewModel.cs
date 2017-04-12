using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Monitor.Model;
using Monitor.Model.Messages;
using Monitor.Model.Statistics;

namespace Monitor.ViewModel.Panels
{
    public class StatisticsPanelViewModel : ToolPaneViewModel
    {
        private readonly IMessenger _messenger;
        private readonly IStatisticsFormatter _statisticsFormatter;

        private ObservableCollection<StatisticViewModel> _statistics = new ObservableCollection<StatisticViewModel>();

        public ObservableCollection<StatisticViewModel> Statistics
        {
            get { return _statistics; }
            set
            {
                _statistics = value;
                RaisePropertyChanged();
            }
        }

        public StatisticsPanelViewModel(IMessenger messenger, IStatisticsFormatter statisticsFormatter)
        {
            Name = "Statistics";

            _messenger = messenger;
            _statisticsFormatter = statisticsFormatter;
            _messenger.Register<SessionUpdateMessage>(this, message =>
            {
                ParseResult(message.ResultContext.Result);
            });
            _messenger.Register<SessionClosedMessage>(this, m => Clear());
        }

        private void Clear()
        {
            Statistics.Clear();            
        }

        private void ParseResult(Result result)
        {
            Statistics = new ObservableCollection<StatisticViewModel>(result.Statistics.Select(s => new StatisticViewModel
            {
                Name = s.Key,
                Value = s.Value,
                State = _statisticsFormatter.Format(s.Key, s.Value)
            }));
        }
    }
}
