using System;
using System.IO;
using System.Threading;

namespace Monitor.Model.Sessions
{
    public class FileSession : ISession
    {
        private readonly IResultSerializer _resultSerializer;
        private readonly ISessionHandler _sessionHandler;
        private readonly SynchronizationContext _syncContext;

        private readonly bool _watchFile;

        public string Name { get; private set; }

        private FileSystemWatcher _watcher;
        private SessionState _state = SessionState.Unsubscribed;

        public FileSession(ISessionHandler resultHandler, IResultSerializer resultSerializer, FileSessionParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (string.IsNullOrWhiteSpace(parameters.FileName)) throw new ArgumentException(@"FileName is required.", nameof(parameters));

            _watchFile = parameters.Watch;
            _syncContext = SynchronizationContext.Current;

            _resultSerializer = resultSerializer;
            _sessionHandler = resultHandler;

            Name = parameters.FileName;
        }

        public void Initialize()
        {
            // Initially open the file
            ReadFromFile();

            // Return when we do not have to configure the file system watcher
            if (!_watchFile) return;

            // Open a monitoring sessionss
            Subscribe();
        }

        public void Shutdown()
        {
            Unsubscribe();
        }

        private void ReadFromFile()
        {
            if (!File.Exists(Name)) throw new Exception($"File '{Name}' does not exist");

            var file = File.ReadAllText(Name);
            var result = _resultSerializer.Deserialize(file);

            var context = new ResultContext
            {
                Name = Name,
                Result = result,
                Progress = 1
            };

            _sessionHandler.HandleResult(context);
        }

        public void Subscribe()
        {
            State = SessionState.Subscribed;

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

        public void Unsubscribe()
        {
            State = SessionState.Unsubscribed;

            if (_watcher == null)
            {
                // This file has no watcher session.
                return;
            }

            _watcher.EnableRaisingEvents = false;

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