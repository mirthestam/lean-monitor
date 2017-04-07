using System;
using GalaSoft.MvvmLight.Messaging;

namespace Monitor.Model.Messages
{
    public class SessionUpdateMessage : MessageBase
    {
        public SessionUpdateMessage(ResultContext resultContext)
        {
            ResultContext = resultContext ?? throw new ArgumentNullException(nameof(resultContext));
        }

        public ResultContext ResultContext { get; private set; }
    }
}