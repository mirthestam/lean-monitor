namespace Monitor.Model.Sessions
{
    public class StreamSessionParameters
    {
        public string Host { get; set; }
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets whether this session should close after the last packet has been received. When disabled, New packets will reset the session state.
        /// </summary>
        public bool CloseAfterCompleted { get; set; } = true;
    }
}