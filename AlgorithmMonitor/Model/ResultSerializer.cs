using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Orders;
using QuantConnect.Packets;

namespace Monitor.Model
{
    public class ResultSerializer : IResultSerializer
    {
        private readonly IResultConverter _resultConverter;

        public ResultSerializer(IResultConverter resultConverter)
        {
            _resultConverter = resultConverter;

            //Allow proper decoding of orders.
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = { new OrderJsonConverter() }
            };
        }

        public Result Deserialize(string serializedResult)
        {
            if (string.IsNullOrWhiteSpace(serializedResult)) throw new ArgumentNullException(nameof(serializedResult));

            // TODO: It expects BacktestResult. Should have a mechanism to detect the result type
            // i.e. based upon specific live / backtest result known fielts (i.e. Holdings, RollingWindow)
            // It also tries to extract results from a quantconnect download file.

            var json = JObject.Parse(serializedResult);

            // First we try to get thre sults part from a bigger JSON.
            // This can be the case when downloaded from QC.
            try
            {
                JToken resultToken;
                if (json.TryGetValue("results", out resultToken))
                {
                    // Remove the profit-loss part. This is causing problems when downloaded from QC.
                    // TODO: Investigate the problem with the ProfitLoss entry
                    var pl = resultToken.Children().FirstOrDefault(c => c.Path == "results.ProfitLoss");
                    pl?.Remove();

                    // Convert back to string. Our deserializer will get the results part.
                    serializedResult = resultToken.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // We could not parse results from the JSON. Continue and try to parse normally
            }


            var backtestResult = JsonConvert.DeserializeObject<BacktestResult>(serializedResult);
            var result = _resultConverter.FromBacktestResult(backtestResult);
            return result;
        }

        public string Serialize(Result result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            string serialized;

            switch (result.ResultType)
            {
                case ResultType.Backtest:
                    var backtestResult = _resultConverter.ToBacktestResult(result);
                    serialized = JsonConvert.SerializeObject(backtestResult, Formatting.Indented);
                    break;

                case ResultType.Live:
                    var liveResult = _resultConverter.ToLiveResult(result);
                    serialized = JsonConvert.SerializeObject(liveResult, Formatting.Indented);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return serialized;
        }
    }
}
