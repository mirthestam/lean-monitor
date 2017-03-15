using GalaSoft.MvvmLight.Messaging;
using QuantConnect.Lean.Monitor.Model;
using QuantConnect.Lean.Monitor.Model.Sessions;
using StructureMap;

namespace QuantConnect.Lean.Monitor
{
    public class AppRegistry : Registry
    {
        public AppRegistry()
        {
            // Messaging
            For<IMessenger>().Use(Messenger.Default);

            // Model
            For<ISessionService>().Singleton().Use<SessionService>();
            For<IResultFactory>().Singleton().Use<ResultFactory>();
        }
    }
}
