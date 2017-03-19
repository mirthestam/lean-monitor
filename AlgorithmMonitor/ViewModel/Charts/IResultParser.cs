using Monitor.Model;

namespace Monitor.ViewModel.Charts
{
    public interface IResultParser
    {
        void ParseResult(Result result);
    }
}