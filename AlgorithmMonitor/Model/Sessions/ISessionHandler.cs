namespace Monitor.Model.Sessions
{
    public interface ISessionHandler
    {
        void HandleStateChanged(SessionState state);
        void HandleResult(Result result);
        void HandleLogMessage(string message, LogItemType type);
    }
}