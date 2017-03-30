using System;
using Newtonsoft.Json;
using QuantConnect.Packets;

namespace Monitor.Model.Api.Contracts
{
    /// <summary>
    /// Backtest response packet from the QuantConnect.com API.
    /// </summary>
    public class Backtest : RestResponse
    {
        /// <summary>Name of the backtest</summary>
        [JsonProperty(PropertyName = "name")]
        public string Name;
        /// <summary>Note on the backtest attached by the user</summary>
        [JsonProperty(PropertyName = "note")]
        public string Note;
        /// <summary>Assigned backtest Id</summary>
        [JsonProperty(PropertyName = "backtestId")]
        public string BacktestId;
        /// <summary>Boolean true when the backtest is completed.</summary>
        [JsonProperty(PropertyName = "completed")]
        public bool Completed;
        /// <summary>Progress of the backtest in percent 0-1.</summary>
        [JsonProperty(PropertyName = "progress")]
        public Decimal Progress;
        /// <summary>Result packet for the backtest</summary>
        [JsonProperty(PropertyName = "result")]
        public BacktestResult Result;
    }
}