using System;
using GalaSoft.MvvmLight.Messaging;

namespace Monitor.Model.Messages
{
    public class GridRequestMessage : MessageBase
    {
        public GridRequestMessage(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            Key = key;
        }

        public string Key { get; private set; }
    }
}