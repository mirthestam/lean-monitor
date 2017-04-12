/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Mon"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Lean.AlgorithmMonitor;
using Lean.AlgorithmMonitor.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Monitor;
using Monitor.ViewModel;
using Monitor.ViewModel.NewSession;
using Monitor.ViewModel.Panels;
using StructureMap;
using System.Collections.Generic;

namespace Lean.AlgorithmMonitor.ViewModel
{
    public class ViewModelLocator
    {
        private static readonly IContainer _container;

        static ViewModelLocator()
        {
            _container = new Container(new AppRegistry());
        }

        public static NewSessionWindowViewModel NewSessionWindow => GetViewModel<NewSessionWindowViewModel>();
        public static AboutWindowViewModel AboutWindow => GetViewModel<AboutWindowViewModel>();
        public static MainWindowViewModel MainWindow => GetViewModel<MainWindowViewModel>();

        public static StatisticsPanelViewModel StatisticsPanel => GetViewModel<StatisticsPanelViewModel>();
        public static RuntimeStatisticsPanelViewModel RuntimeStatisticsPanel => GetViewModel<RuntimeStatisticsPanelViewModel>();
        public static TradesPanelViewModel TradesPanel => GetViewModel<TradesPanelViewModel>();
        public static ProfitLossPanelViewModel ProfitLossPanel => GetViewModel<ProfitLossPanelViewModel>();
        public static LogPanelViewModel LogPanel => GetViewModel<LogPanelViewModel>();       

        private static T GetToolViewModel<T>() where T : ToolPaneViewModel
        {
            return _container.GetInstance<T>();
        }

        private static T GetViewModel<T>()
        {
            // Get all viewmodels as unique instances
            return _container.GetInstance<T>();
        }
    }
}