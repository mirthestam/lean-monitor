namespace Monitor.Model
{
    public class ResultContext
    {
        public string Project { get; set; }
        public string Name { get; set; }
        public decimal Progress { get; set; }
        public Result Result { get; set; }
        public bool Completed => Progress == 1;
    }
}