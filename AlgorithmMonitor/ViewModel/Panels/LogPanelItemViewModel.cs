using System;
using QuantConnect.Lean.Monitor.Model;
using QuantConnect.Lean.Monitor.Model.Messages;

namespace QuantConnect.Lean.Monitor.ViewModel.Panels
{
    public class LogPanelItemViewModel
    {
        public LogItemType EntryType { get; set; }
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
    }
}