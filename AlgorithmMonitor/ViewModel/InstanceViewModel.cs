using Monitor.Model;

namespace Monitor.ViewModel
{
    public class InstanceViewModel
    {
        public ResultType Type { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public decimal Progress { get; set; }
        public bool IsCompleted => Progress == 1;
    }
}