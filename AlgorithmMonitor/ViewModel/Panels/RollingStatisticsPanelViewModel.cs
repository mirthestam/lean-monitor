using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using QuantConnect.Lean.Monitor.Model;
using QuantConnect.Lean.Monitor.Model.Messages;

namespace QuantConnect.Lean.Monitor.ViewModel.Panels
{
    public class RollingStatisticsPanelViewModel : ViewModelBase
    {
        private readonly IMessenger _messenger;

        public Dictionary<DateTime, RollingStatisticsItemViewModel> Drawdown { get; set; } = new Dictionary<DateTime, RollingStatisticsItemViewModel>();
        public Dictionary<DateTime, RollingStatisticsItemViewModel> AvgWinRate { get; set; } = new Dictionary<DateTime, RollingStatisticsItemViewModel>();

        public RollingStatisticsPanelViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            _messenger.Register<SessionUpdateMessage>(this, message =>
            {
                ParseResult(message.Result);
            });
        }

        private void ParseResult(Result result)
        {
            var windows = result.RollingWindow.GroupBy(v => v.Key.Split('_')[1])
                .Select(g => new
                {
                    Date = DateTime.ParseExact(g.Key, "yyyyMMdd", CultureInfo.InvariantCulture),
                    M1 = g.First(x => x.Key.StartsWith("M1")).Value,
                    M3 = g.First(x => x.Key.StartsWith("M3")).Value,
                    M6 = g.First(x => x.Key.StartsWith("M6")).Value,
                    M12 = g.First(x => x.Key.StartsWith("M12")).Value
                });

            foreach (var window in windows)
            {
                if (!Drawdown.ContainsKey(window.Date)) Drawdown[window.Date] = new RollingStatisticsItemViewModel();
                var drawdown = Drawdown[window.Date];
                drawdown.M1 = window.M1.PortfolioStatistics.Drawdown;
                drawdown.M3 = window.M3.PortfolioStatistics.Drawdown;
                drawdown.M6 = window.M6.PortfolioStatistics.Drawdown;
                drawdown.M12 = window.M12.PortfolioStatistics.Drawdown;

                if (!AvgWinRate.ContainsKey(window.Date)) AvgWinRate[window.Date] = new RollingStatisticsItemViewModel();
                var avgWinRate = AvgWinRate[window.Date];
                avgWinRate.M1 = window.M1.PortfolioStatistics.AverageWinRate;
                avgWinRate.M3 = window.M3.PortfolioStatistics.AverageWinRate;
                avgWinRate.M6 = window.M6.PortfolioStatistics.AverageWinRate;
                avgWinRate.M12 = window.M12.PortfolioStatistics.AverageWinRate;
            }
        }            
    }
}
