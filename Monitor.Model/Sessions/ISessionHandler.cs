namespace Monitor.Model.Sessions
{
    public interface ISessionHandler
    {
        void HandleStateChanged(SessionState state);
        void HandleResult(ResultContext resultContext);
        void HandleLogMessage(string message, LogItemType type);
    }
}