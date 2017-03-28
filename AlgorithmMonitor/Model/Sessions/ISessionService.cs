namespace Monitor.Model.Sessions
{
    public interface ISessionService
    {
        bool IsSessionActive { get; }

        Result LastResult { get; }

        void Initialize();

        void ShutdownSession();

        bool IsSessionSubscribed { get; set; }

        void OpenStream(StreamSessionParameters parameters);

        void OpenFile(FileSessionParameters parameters);
    }
}