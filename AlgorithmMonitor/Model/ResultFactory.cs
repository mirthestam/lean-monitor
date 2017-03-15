using System;
using System.Collections.Generic;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Statistics;

namespace QuantConnect.Lean.Monitor.Model
{
    public class ResultFactory : IResultFactory
    {
        public Result FromBacktestResult(BacktestResult backtestResult)
        {
            return new Result
            {
                ResultType = ResultType.Backtest,
                Charts = new Dictionary<string, Chart>(backtestResult.Charts),
                Orders = new Dictionary<int, Order>(backtestResult.Orders),
                ProfitLoss = new Dictionary<DateTime, decimal>(backtestResult.ProfitLoss),
                Statistics = new Dictionary<string, string>(backtestResult.Statistics),
                RuntimeStatistics = new Dictionary<string, string>(backtestResult.RuntimeStatistics),
                RollingWindow = new Dictionary<string, AlgorithmPerformance>(backtestResult.RollingWindow)
            };
        }

        public Result FromLiveResult(LiveResult liveResult)
        {
            return new Result
            {
                ResultType = ResultType.Live,
                Charts = new Dictionary<string, Chart>(liveResult.Charts),
                Orders = new Dictionary<int, Order>(liveResult.Orders),
                ProfitLoss = new Dictionary<DateTime, decimal>(liveResult.ProfitLoss),
                Statistics = new Dictionary<string, string>(liveResult.Statistics),
                RuntimeStatistics = new Dictionary<string, string>(liveResult.RuntimeStatistics)
            };
        }
    }
}