using QuantConnect.Lean.Monitor.Model.Messages;

namespace QuantConnect.Lean.Monitor.Model.Sessions
{
    public interface ISessionHandler
    {
        void HandleResult(Result result);
        void HandleLogMessage(string message, LogItemType type);
    }
}