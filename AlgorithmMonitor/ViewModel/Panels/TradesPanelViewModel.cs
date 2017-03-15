using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using QuantConnect.Lean.Monitor.Model;
using QuantConnect.Lean.Monitor.Model.Messages;
using QuantConnect.Orders;

namespace QuantConnect.Lean.Monitor.ViewModel.Panels
{
    public class TradesPanelViewModel : ViewModelBase
    {
        private readonly IMessenger _messenger;

        private ObservableCollection<Order> _orders = new ObservableCollection<Order>();

        public ObservableCollection<Order> Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                RaisePropertyChanged();
            }
        }

        public TradesPanelViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            _messenger.Register<SessionUpdateMessage>(this, message =>
            {
                ParseResult(message.Result);
            });
            _messenger.Register<SessionClosedMessage>(this, m => Clear());
        }

        private void Clear()
        {
            Orders.Clear();
        }

        private void ParseResult(Result result)
        {
            Orders = new ObservableCollection<Order>(result.Orders.OrderBy(o => o.Key).Select(p => p.Value));
        }
    }
}