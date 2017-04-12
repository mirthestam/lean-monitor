using System.Collections.Generic;
using System.Threading.Tasks;
using Backtest = Monitor.Model.Api.Contracts.Backtest;
using Project = Monitor.Model.Api.Contracts.Project;

namespace Monitor.Model.Api
{
    public interface IApiClient
    {
        Task<ResultContext> GetResultAsync(int projectId, string instanceId, ResultType type);

        Task<IEnumerable<Project>> GetProjectsAsync();

        Task<IEnumerable<Backtest>> GetBacktestsAsync(int projectId);

        bool Connected { get; }

        void Initialize(int userId, string token, string baseUrl);
    }
}