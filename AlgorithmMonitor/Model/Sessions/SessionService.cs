using System;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using QuantConnect.Lean.Monitor.Model.Messages;

namespace QuantConnect.Lean.Monitor.Model.Sessions
{
    public class SessionService : ISessionService, ISessionHandler
    {
        private readonly IMessenger _messenger;
        private readonly IResultConverter _resultConverter;
        private readonly IResultSerializer _resultSerializer;

        private ISession _session;

        public Result LastResult { get; private set; }

        public SessionService(IMessenger messenger, IResultConverter resultConverter, IResultSerializer resultSerializer)
        {
            _messenger = messenger;
            _resultConverter = resultConverter;
            _resultSerializer = resultSerializer;
        }

        public void HandleResult(Result result)
        {
            if (result == null) throw new ArgumentException(nameof(result));

            if (_session == null)
            {
                // It might be the case, result has still been sent from an old worker thread before the worker got cancelled.
                // TODO: It might be interesting to use unique ID's for the session
                // so if a new session has been opened meanwhile, we do process new results
                return;
            }

            // Update the last result
            LastResult = result;

            // Send a message indicating the session has been updated
            _messenger.Send(new SessionUpdateMessage(_session.Name, result));
        }

        public void HandleLogMessage(string message, LogItemType type)
        {
            // Live log message, use current DateTime
            _messenger.Send(new LogEntryReceivedMessage(DateTime.Now, message, type));
        }

        public void Initialize()
        {
            // We try to load instructions to load a session from the commandline.
            // This format is a bit obscure because it tries to say compatible with the 'port only' argument as used in the Lean project.

            try
            {
                var arguments = Environment.GetCommandLineArgs();
                var argument = arguments.Last();

                // First try whether it is a port
                if (int.TryParse(argument, out int port))
                {
                    OpenStream(new StreamSessionParameters
                    {
                        Host = "localhost",
                        Port = port
                    });
                    return;
                }
                if (argument.EndsWith(".json"))
                {
                    // Expect it is a fileName
                    OpenFile(new FileSessionParameters
                    {
                        FileName = argument,
                        Watch = true
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                // We were unable to open a session
                throw new Exception($"Invalid command line parameters: {Environment.GetCommandLineArgs()}", ex);
            }

            // Request a session by default
            _messenger.Send(new ShowNewSessionWindowMessage());
        }

        public void CloseSession()
        {
            if (_session == null) throw new Exception("No session exists");

            try
            {
                _session.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Could not close the session", e);
            }
            finally
            {
                _session = null;
                LastResult = null;
                _messenger.Send(new SessionClosedMessage());
            }
        }

        public void OpenStream(StreamSessionParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (string.IsNullOrWhiteSpace(parameters.Host)) throw new ArgumentException(@"Host is required", nameof(parameters));

            if (_session != null)
            {
                // Another session is open.
                // Close the session first before opening this new one
                CloseSession();
            }

            // Fix the parameters
            if (!parameters.Host.StartsWith(">tcp://")) parameters.Host = ">tcp://" + parameters.Host;
            if (!parameters.Host.EndsWith(":")) parameters.Host = parameters.Host + ":";

            // Open a new session and open it
            var session = new StreamSession(this, _resultConverter, parameters);
            OpenSession(session);
        }

        public void OpenFile(FileSessionParameters parameters)
        {
            if (_session != null)
            {
                // Another session is open.
                // Close the session first before opening this new one
                CloseSession();
            }

            var session = new FileSession(this, _resultSerializer, parameters);
            OpenSession(session);
        }

        private void OpenSession(ISession session)
        {
            try
            {
                _session = session;
                session.Open();
            }
            catch (Exception e)
            {
                throw new Exception("Exception occured while opening the session", e);
            }
            finally
            {
                // Notify the app of the new session
                _messenger.Send(new SessionOpenedMessage(_session.Name));
            }
        }
    }
}
