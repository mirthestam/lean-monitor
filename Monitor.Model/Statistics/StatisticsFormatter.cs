namespace Monitor.Model.Statistics
{
    public class StatisticsFormatter : IStatisticsFormatter
    {
        public StatisticState Format(string key, string value)
        {
            switch (key)
            {
                case "Unrealized":
                case "Net Profit":
                case "Return":
                case "Sharpe Ratio":
                    return FormatNegativePositive(value);

                case "Fees":
                    return StatisticState.Inconclusive;

                default:
                    return FormatOnlyNegative(value);
            }
        }

        private static StatisticState FormatNegativePositive(string value)
        {
            return value.Contains("-") ? StatisticState.Negative : StatisticState.Positive;
        }


        private static StatisticState FormatOnlyNegative(string value)
        {
            return value.Contains("-") ? StatisticState.Negative : StatisticState.Inconclusive;
        }
    }
}
