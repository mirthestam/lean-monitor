using System;
using GalaSoft.MvvmLight.Messaging;

namespace Monitor.Model.Messages
{
    public class SessionOpenedMessage : MessageBase
    {
        public SessionOpenedMessage(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public string Name { get; set; }
    }
}