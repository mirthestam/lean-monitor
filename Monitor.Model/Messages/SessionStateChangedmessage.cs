using GalaSoft.MvvmLight.Messaging;
using Monitor.Model.Sessions;

namespace Monitor.Model.Messages
{
    public class SessionStateChangedMessage : MessageBase
    {
        public SessionStateChangedMessage(SessionState state)
        {
            State = state;
        }

        public SessionState State { get; private set; }
    }
}