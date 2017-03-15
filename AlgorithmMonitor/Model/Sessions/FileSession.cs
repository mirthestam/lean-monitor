using System;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Orders;
using QuantConnect.Packets;

namespace QuantConnect.Lean.Monitor.Model.Sessions
{
    public class FileSession : ISession
    {
        private readonly IResultFactory _resultFactory;
        private readonly ISessionHandler _resultHandler;
        private readonly SynchronizationContext _syncContext;

        private readonly bool _watchFile;

        public string Name { get; private set; }

        private FileSystemWatcher _watcher;

        public FileSession(ISessionHandler resultHandler, IResultFactory resultFactory, FileSessionParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (string.IsNullOrWhiteSpace(parameters.FileName)) throw new ArgumentException(@"FileName is required.", nameof(parameters));

            _watchFile = parameters.Watch;
            _syncContext = SynchronizationContext.Current;

            _resultHandler = resultHandler;
            _resultFactory = resultFactory;

            Name = parameters.FileName;

            //Allow proper decoding of orders.
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = { new OrderJsonConverter() }
            };
        }

        public void Open()
        {
            // Initially open the file
            ReadFromFile();

            // Return when we do not have to configure the file system watcher
            if (!_watchFile) return;

            if (!Path.IsPathRooted(Name))
            {
                // Combine with the current directory to allow for the FileSystemWatcher to monitor
                Name = Path.Combine(Environment.CurrentDirectory, Name);
            }

            var directoryName = Path.GetDirectoryName(Name);
            if (directoryName != null)
                _watcher = new FileSystemWatcher(directoryName)
                {
                    EnableRaisingEvents = _watchFile
                };

            _watcher.Changed += (sender, args) =>
            {
                if (args.Name == Path.GetFileName(Name))
                {
                    _syncContext.Post(o => ReadFromFile(), null);
                }
            };
        }

        public void Close()
        {
            if (_watcher == null)
            {
                // This file has no watcher session.
                return;
            }

            _watcher.EnableRaisingEvents = false;
        }

        private void ReadFromFile()
        {
            if (!File.Exists(Name)) throw new Exception($"File '{Name}' does not exist");

            var file = File.ReadAllText(Name);
            var json = JObject.Parse(file);

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
                    file = resultToken.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // We could not parse results from the JSON. Continue and try to parse normally
            }

            Result result;
            try
            {
                var backtestResult = JsonConvert.DeserializeObject<BacktestResult>(file);
                result = _resultFactory.FromBacktestResult(backtestResult);
            }
            catch (Exception ex)
            {
                throw new Exception("The file is no valid Backtest result", ex);
            }

            _resultHandler.HandleResult(result);
        }
    }
}