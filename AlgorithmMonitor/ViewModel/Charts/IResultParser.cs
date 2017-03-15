using QuantConnect.Lean.Monitor.Model;

namespace QuantConnect.Lean.Monitor.ViewModel.Charts
{
    public interface IResultParser
    {
        void ParseResult(Result result);
    }
}