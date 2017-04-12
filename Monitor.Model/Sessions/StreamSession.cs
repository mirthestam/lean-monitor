using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using QuantConnect.Orders;
using QuantConnect.Packets;

namespace Monitor.Model.Sessions
{
    public class StreamSession : ISession
    {
        private readonly ISessionHandler _sessionHandler;
        private readonly IResultConverter _resultConverter;

        private readonly BackgroundWorker _eternalQueueListener = new BackgroundWorker();
        private readonly BackgroundWorker _queueReader = new BackgroundWorker();

        private readonly Result _result = new Result();
        private readonly Queue<Packet> _packetQueue = new Queue<Packet>();

        private readonly SynchronizationContext _syncContext;

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);

        private readonly string _host;
        private readonly int _port;
        private SessionState _state = SessionState.Unsubscribed;
        private readonly bool _closeAfterCompleted;

        public string Name => $"{_host}:{_port}";

        public StreamSession(ISessionHandler sessionHandler, IResultConverter resultConverter, StreamSessionParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            _sessionHandler = sessionHandler;
            _resultConverter = resultConverter;            

            _host = parameters.Host;
            _port = parameters.Port;
            _closeAfterCompleted = parameters.CloseAfterCompleted;

            _syncContext = SynchronizationContext.Current;
        }

        public void Initialize()
        {
            //Allow proper decoding of orders.
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = {new OrderJsonConverter()}
            };

            Subscribe();
        }

        public void Shutdown()
        {
            Unsubscribe();
        }

        private void QueueReader(object sender, DoWorkEventArgs e)
        {
            while (!_queueReader.CancellationPending)
            {
                // Check whether we can dequeue
                if (_packetQueue.Count == 0) continue;
                var p = _packetQueue.Dequeue();
                HandlePacket(p);
            }
            _resetEvent.Set();
        }

        private void SocketListener(object sender, DoWorkEventArgs e)
        {
            using (var pullSocket = new PullSocket(_host + _port))
            {
                while (!_eternalQueueListener.CancellationPending)
                {
                    NetMQMessage message = new NetMQMessage();
                    if (!pullSocket.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(500), ref message))
                    {
                        continue;
                    }

                    // There should only be 1 part messages
                    if (message.FrameCount != 1) continue;

                    var payload = message[0].ConvertToString();

                    var packet = JsonConvert.DeserializeObject<Packet>(payload);

                    switch (packet.Type)
                    {
                        case PacketType.LiveResult:
                            var liveResultEventModel = JsonConvert.DeserializeObject<LiveResultPacket>(payload);
                            _packetQueue.Enqueue(liveResultEventModel);
                            break;
                        case PacketType.BacktestResult:
                            var backtestResultEventModel = JsonConvert.DeserializeObject<BacktestResultPacket>(payload);
                            _packetQueue.Enqueue(backtestResultEventModel);
                            break;
                        case PacketType.Log:
                            var logEventModel = JsonConvert.DeserializeObject<LogPacket>(payload);
                            _packetQueue.Enqueue(logEventModel);
                            break;
                    }
                }

                pullSocket.Close();
                _resetEvent.Set();
            }
        }

        private void HandlePacket(Packet packet)
        {
            switch (packet.Type)
            {
                case PacketType.LiveResult:
                    HandleLiveResultPacket(packet);
                    break;
                case PacketType.BacktestResult:
                    HandleBacktestResultPacket(packet);
                    break;
                case PacketType.Log:
                    HandleLogPacket(packet);
                    break;
                case PacketType.Debug:
                    HandleDebugPacket(packet);
                    break;
            }
        }

        private void HandleDebugPacket(Packet packet)
        {
            var debugEventModel = (DebugPacket) packet;
            _syncContext.Send(o => _sessionHandler.HandleLogMessage(debugEventModel.Message, LogItemType.Debug), null);
        }

        private void HandleLogPacket(Packet packet)
        {
            var logEventModel = (LogPacket) packet;
            _syncContext.Send(o => _sessionHandler.HandleLogMessage(logEventModel.Message, LogItemType.Log), null);
        }

        private void HandleBacktestResultPacket(Packet packet)
        {
            var backtestResultEventModel = (BacktestResultPacket) packet;
            var backtestResultUpdate = _resultConverter.FromBacktestResult(backtestResultEventModel.Results);
            _result.Add(backtestResultUpdate);

            var context = new ResultContext
            {
                Name = Name,
                Result = _result,
                Progress = backtestResultEventModel.Progress
            };
            _syncContext.Send(o => _sessionHandler.HandleResult(context), null);

            if (backtestResultEventModel.Progress == 1 && _closeAfterCompleted)
            {
                _syncContext.Send(o => Unsubscribe(), null);
            }
        }

        private void HandleLiveResultPacket(Packet packet)
        {
            var liveResultEventModel = (LiveResultPacket) packet;
            var liveResultUpdate = _resultConverter.FromLiveResult(liveResultEventModel.Results);
            _result.Add(liveResultUpdate);

            var context = new ResultContext
            {
                Name = Name,
                Result = _result
            };

            _syncContext.Send(o => _sessionHandler.HandleResult(context), null);
        }

        public void Subscribe()
        {
            try
            {
                // Configure the worker threads
                _eternalQueueListener.WorkerSupportsCancellation = true;
                _eternalQueueListener.DoWork += SocketListener;
                _eternalQueueListener.RunWorkerAsync();

                _queueReader.WorkerSupportsCancellation = true;
                _queueReader.DoWork += QueueReader;
                _queueReader.RunWorkerAsync();

                State = SessionState.Subscribed;
            }
            catch (Exception e)
            {                
                throw new Exception("Could not subscribe to the stream", e);
            }
        }        

        public void Unsubscribe()
        {
            try
            {
                if (_eternalQueueListener != null)
                {
                    _eternalQueueListener.CancelAsync();
                    _eternalQueueListener.DoWork -= SocketListener;
                    //_resetEvent.WaitOne();
                }

                if (_queueReader != null)
                {
                    _queueReader.CancelAsync();
                    _queueReader.DoWork -= QueueReader;
                    //_resetEvent.WaitOne();
                }

                State = SessionState.Unsubscribed;
            }
            catch (Exception e)
            {
                throw new Exception("Could not unsubscribe from the stream", e);
            }
            
        }

        public SessionState State
        {
            get { return _state; }
            private set
            {
                _state = value;
                _sessionHandler.HandleStateChanged(value);
            }
        }

        public bool CanSubscribe { get; } = true;
    }
}