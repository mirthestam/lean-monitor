namespace Monitor.Model.Statistics
{
    public interface IStatisticsFormatter
    {
        StatisticState Format(string key, string value);
    }
}