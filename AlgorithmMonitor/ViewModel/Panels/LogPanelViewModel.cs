using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using QuantConnect.Lean.Monitor.Model;
using QuantConnect.Lean.Monitor.Model.Messages;

namespace QuantConnect.Lean.Monitor.ViewModel.Panels
{
    public class LogPanelViewModel : ViewModelBase
    {
        private readonly IMessenger _messenger;

        private ObservableCollection<LogPanelItemViewModel>_logEntries = new ObservableCollection<LogPanelItemViewModel>();

        public LogPanelViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            _messenger.Register<LogEntryReceivedMessage>(this, ParseResult);
            _messenger.Register<SessionClosedMessage>(this, m => Clear());

            if (IsInDesignMode)
            {
                LogEntries.Add(new LogPanelItemViewModel
                {
                    DateTime = DateTime.Now.AddMinutes(-4),
                    EntryType = LogItemType.Log,
                    Message = "This is a log entry"
                });

                LogEntries.Add(new LogPanelItemViewModel
                {
                    DateTime = DateTime.Now.AddMinutes(-2),
                    EntryType = LogItemType.Debug,
                    Message = "This is a debug entry"
                });
            }
        }

        private void Clear()
        {
            LogEntries.Clear();
        }

        public ObservableCollection<LogPanelItemViewModel> LogEntries
        {
            get { return _logEntries; }
            set
            {
                _logEntries = value;
                RaisePropertyChanged();
            }
        }

        private void ParseResult(LogEntryReceivedMessage message)
        {
            LogEntries.Add(new LogPanelItemViewModel
            {
                DateTime = message.DateTime,
                Message = message.Message,
                EntryType = message.EntryType
            });
        }
    }
}
