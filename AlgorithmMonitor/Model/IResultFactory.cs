using QuantConnect.Packets;

namespace QuantConnect.Lean.Monitor.Model
{
    public interface IResultFactory
    {
        Result FromBacktestResult(BacktestResult backtestResult);
        Result FromLiveResult(LiveResult liveResult);
    }
}