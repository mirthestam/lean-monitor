using Monitor.Model.Statistics;

namespace Monitor.ViewModel.Panels
{
    public class StatisticViewModel
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public StatisticState State { get; set; } = StatisticState.Inconclusive;
    }
}