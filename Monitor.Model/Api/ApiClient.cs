using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monitor.Model.Api.Contracts;
using RestSharp;

namespace Monitor.Model.Api
{
    public class ApiClient : IApiClient
    {        
        private readonly IResultConverter _resultConverter;

        private IApiConnection _connection;

        public ApiClient(IResultConverter resultConverter)
        {
            _resultConverter = resultConverter;
        }

        public async Task<ResultContext> GetResultAsync(int projectId, string instanceId, ResultType type)
        {
            switch (type)
            {
                case ResultType.Backtest:
                    return await GetBacktestResult(projectId, instanceId);
                    
                default:
                    throw new NotSupportedException();
            }
        }

        public void Initialize(int userId, string token, string baseUrl)
        {
            _connection = new ApiConnection(userId, token, baseUrl);
        }

        public bool Connected => _connection != null && _connection.Connected;

        public async Task<IEnumerable<Project>> GetProjectsAsync()
        {
            var taskResponse = await Task.Factory.StartNew(() =>
            {
                var request = new RestRequest("projects/read", Method.GET) {RequestFormat = DataFormat.Json};
                ProjectResponse response;
                _connection.TryRequest(request, out response);

                if (!response.Success)
                {
                    throw new Exception($"Failed to get projects from API: {string.Join(Environment.NewLine, response.Errors.ToArray())}");
                }

                return response.Projects.AsEnumerable();
            });
            return taskResponse;
        }

        public async Task<IEnumerable<Backtest>> GetBacktestsAsync(int projectId)
        {
            var taskResponse = await Task.Factory.StartNew(() =>
            {
                var request = new RestRequest("backtests/read", Method.GET);
                request.AddParameter("projectId", projectId);
                BacktestList response;
                _connection.TryRequest(request, out response);

                if (!response.Success)
                {
                    throw new Exception(
                        $"Failed to get backtests for project '{projectId}' from API: {string.Join(Environment.NewLine, response.Errors.ToArray())}");
                }

                return response.Backtests;
            });
            return taskResponse;
        }

        private async Task<ResultContext> GetBacktestResult(int projectId, string instanceId)
        {
            var taskResponse = await Task.Factory.StartNew(() =>
            {
                var request = new RestRequest("backtests/read", Method.GET);
                request.AddParameter("backtestId", instanceId);
                request.AddParameter("projectId", projectId);
                Backtest response;
                _connection.TryRequest(request, out response);

                if (!response.Success)
                {
                    throw new Exception(
                        $"Failed to get backtest result from API: {string.Join(Environment.NewLine, response.Errors.ToArray())}");

                }

                return new ResultContext
                {
                    Result = _resultConverter.FromBacktestResult(response.Result),
                    Progress = response.Progress
                };
            });

            return taskResponse;
        }
    }
}
