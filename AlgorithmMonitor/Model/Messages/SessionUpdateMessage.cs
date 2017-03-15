using System;
using GalaSoft.MvvmLight.Messaging;

namespace QuantConnect.Lean.Monitor.Model.Messages
{
    public class SessionUpdateMessage : MessageBase
    {
        public SessionUpdateMessage(string name, Result result)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Name = name;

            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        public string Name { get; private set; }

        public Result Result { get; private set; }
    }
}