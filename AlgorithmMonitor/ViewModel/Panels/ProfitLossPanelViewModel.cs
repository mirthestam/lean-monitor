using System;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using QuantConnect.Lean.Monitor.Model;
using QuantConnect.Lean.Monitor.Model.Messages;

namespace QuantConnect.Lean.Monitor.ViewModel.Panels
{
    public class ProfitLossPanelViewModel : ViewModelBase
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
            _messenger = messenger;
            _messenger.Register<SessionClosedMessage>(this, m => Clear());
            _messenger.Register<SessionUpdateMessage>(this, message =>
            {
                ParseResult(message.Result);
            });

            if (this.IsInDesignMode)
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
                Profit = p.Value
            }));
        }
    }
}
