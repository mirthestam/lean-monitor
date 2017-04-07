using QuantConnect.Packets;

namespace Monitor.Model
{
    public interface IResultConverter
    {
        Result FromBacktestResult(BacktestResult backtestResult);
        Result FromLiveResult(LiveResult liveResult);

        BacktestResult ToBacktestResult(Result result);
        LiveResult ToLiveResult(Result result);
    }
}