using RestSharp;
using RestResponse = Monitor.Model.Api.Contracts.RestResponse;

namespace Monitor.Model.Api
{
    public interface IApiConnection
    {
        bool Connected { get; }

        bool TryRequest<T>(RestRequest request, out T result) where T : RestResponse;
    }
}