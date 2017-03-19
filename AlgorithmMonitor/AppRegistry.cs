using GalaSoft.MvvmLight.Messaging;
using Monitor.Model;
using Monitor.Model.Sessions;
using StructureMap;

namespace Monitor
{
    public class AppRegistry : Registry
    {
        public AppRegistry()
        {
            // Messaging
            For<IMessenger>().Use(Messenger.Default);

            // Model
            For<ISessionService>().Singleton().Use<SessionService>();
            For<IResultConverter>().Singleton().Use<ResultConverter>();
            For<IResultSerializer>().Singleton().Use<ResultSerializer>();
        }
    }
}
