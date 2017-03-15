namespace QuantConnect.Lean.Monitor.Model.Sessions
{
    public interface ISessionService
    {
        void Initialize();

        void CloseSession();

        void OpenStream(StreamSessionParameters parameters);

        void OpenFile(FileSessionParameters parameters);

        Result LastResult { get; }
    }
}