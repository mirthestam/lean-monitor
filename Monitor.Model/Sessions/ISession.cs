namespace Monitor.Model.Sessions
{
    public interface ISession
    {
        void Initialize();
        void Shutdown();

        void Subscribe();
        void Unsubscribe();

        bool CanSubscribe { get; }

        SessionState State { get; }
    }
}