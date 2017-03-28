using Monitor.Model.Statistics;

namespace Monitor.ViewModel.Panels
{
    public class StatisticViewModel
    {
        private string _value;
        public string Name { get; set; }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public StatisticState State { get; set; } = StatisticState.Inconclusive;
    }
}