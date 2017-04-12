using System;
using Monitor.Model;

namespace Monitor.ViewModel.Panels
{
    public class LogPanelItemViewModel
    {
        public LogItemType EntryType { get; set; }
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
    }
}