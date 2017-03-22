namespace Monitor.Model.Sessions
{
    public interface ISession
    {
        void Open();
        void Close();

        string Name { get; }
    }
}