namespace Monitor.Model.Sessions
{
    public interface ISessionService
    {
        bool IsSessionActive { get; }

        Result LastResult { get; }

        void Initialize();

        void ShutdownSession();

        bool IsSessionSubscribed { get; set; }

        bool CanSubscribe { get; }

        void OpenStream(StreamSessionParameters parameters);

        void OpenFile(FileSessionParameters parameters);

        void OpenApi(ApiSessionParameters parameters);
    }
}