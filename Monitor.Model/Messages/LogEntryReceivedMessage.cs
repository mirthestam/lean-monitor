using System;
using GalaSoft.MvvmLight.Messaging;

namespace Monitor.Model.Messages
{
    public class LogEntryReceivedMessage : MessageBase
    {
        public LogEntryReceivedMessage(DateTime dateTime, string message, LogItemType entryType)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(nameof(message));

            DateTime = dateTime;
            Message = message;
            EntryType = entryType;
        }

        public DateTime DateTime { get; private set; }
        public string Message { get; private set; }
        public LogItemType EntryType { get; private set; }
    }
}