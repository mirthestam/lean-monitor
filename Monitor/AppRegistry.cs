using GalaSoft.MvvmLight.Messaging;
using Monitor.Model;
using Monitor.Model.Api;
using Monitor.Model.Sessions;
using Monitor.Model.Statistics;
using Monitor.ViewModel;
using Monitor.ViewModel.NewSession;
using StructureMap;

namespace Monitor
{
    public class AppRegistry : Registry
    {
        public AppRegistry()
        {
            Model();

            // Messaging
            For<IMessenger>().Use(Messenger.Default);

            // Model
        }

        public void Model()
        {
            // Results
            For<IResultConverter>().Singleton().Use<ResultConverter>();
            For<IResultSerializer>().Singleton().Use<ResultSerializer>();
            For<IResultMutator>().Singleton().Use<BenchmarkResultMutator>(); // Implement pipeline pattern if more mutators will exist in the future.
            For<IStatisticsFormatter>().Use<StatisticsFormatter>();

            // Sessions
            For<ISessionService>().Singleton().Use<SessionService>();

            // Api
            For<IApiClient>().Singleton().Use<ApiClient>();             
            
            // ViewModel
            For<ILayoutManager>().Use<LayoutManager>();
            Scan(scanner =>
            {
                scanner.TheCallingAssembly();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf<INewSessionViewModel>();
            });
        }
    }
}
