namespace Monitor.Model.Sessions
{
    public class ApiSessionParameters
    {
        public int ProjectId { get; set; }

        public string InstanceId { get; set; }

        public ResultType InstanceType { get; set; } = ResultType.Backtest;
    }
}