namespace QuantConnect.Lean.Monitor.Model.Sessions
{
    public interface ISessionService
    {
        bool IsSessionActive { get; }

        Result LastResult { get; }

        void Initialize();

        void CloseSession();        

        void OpenStream(StreamSessionParameters parameters);

        void OpenFile(FileSessionParameters parameters);
    }
}