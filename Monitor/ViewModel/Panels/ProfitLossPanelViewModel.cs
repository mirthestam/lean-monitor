using System;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Monitor.Model;
using Monitor.Model.Messages;

namespace Monitor.ViewModel.Panels
{
    public class ProfitLossPanelViewModel : ToolPaneViewModel
    {
        private readonly IMessenger _messenger;

        private ObservableCollection<ProfitLossItemViewModel> _profitLoss = new ObservableCollection<ProfitLossItemViewModel>();

        public ObservableCollection<ProfitLossItemViewModel> ProfitLoss
        {
            get { return _profitLoss; }
            set
            {
                _profitLoss = value;
                RaisePropertyChanged();
            }
        }

        public ProfitLossPanelViewModel(IMessenger messenger)
        {
            Name = "Profit & Loss";

            _messenger = messenger;
            _messenger.Register<SessionClosedMessage>(this, m => Clear());
            _messenger.Register<SessionUpdateMessage>(this, message =>
            {
                ParseResult(message.ResultContext.Result);
            });

            if (IsInDesignMode)
            {
                ProfitLoss.Add(new ProfitLossItemViewModel
                {
                    DateTime = DateTime.Now,
                    Profit = 1323
                });
            }

        }

        private void Clear()
        {
            ProfitLoss.Clear();
        }

        private void ParseResult(Result result)
        {
            ProfitLoss = new ObservableCollection<ProfitLossItemViewModel>(result.ProfitLoss.OrderBy(o => o.Key).Select(p => new ProfitLossItemViewModel
            {
                DateTime = p.Key,
                Profit = p.Value,
                IsNegative = p.Value < 0
            }));
        }
    }
}
