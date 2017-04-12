using System;
using Monitor.Model.Api.Contracts;
using Newtonsoft.Json;
using QuantConnect;
using QuantConnect.API;
using QuantConnect.Orders;
using RestSharp;
using RestSharp.Authenticators;

namespace Monitor.Model.Api
{
    public class ApiConnection : IApiConnection
    {
        // Class is based upon the actual LEAN ApiConnection, however refactored to supoprt different endpoint addressses, for when people implemented
        // their own servers.

        public RestClient Client;

        // Authorization Credentials
        private readonly string _userId;
        private readonly string _token;

        public ApiConnection(int userId, string token, string endpointAddress)
        {
            _token = token;
            _userId = userId.ToString();
            Client = new RestClient(endpointAddress);
            Client.AddDefaultHeader("Cache-Control", "no-cache");
        }

        public bool Connected
        {
            get
            {
                var request = new RestRequest("authenticate", Method.GET);
                AuthenticationResponse response;
                return TryRequest(request, out response) && response.Success;
            }
        }

        public bool TryRequest<T>(RestRequest request, out T result)
            where T : Contracts.RestResponse
        {
            try
            {
                //Generate the hash each request
                // Add the UTC timestamp to the request header.
                // Timestamps older than 1800 seconds will not work.
                var timestamp = (int)Time.TimeStamp();
                var hash = CreateSecureHash(timestamp, _token);
                request.AddHeader("Timestamp", timestamp.ToString());
                Client.Authenticator = new HttpBasicAuthenticator(_userId, hash);

                // Execute the authenticated REST API Call
                var restsharpResponse = Client.Execute(request);

                // Use custom converter for deserializing live results data
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Converters = { new LiveAlgorithmResultsJsonConverter(), new OrderJsonConverter() }
                };

                //Verify success
                result = JsonConvert.DeserializeObject<T>(restsharpResponse.Content);
                if (!result.Success)
                {
                    //result;
                    return false;
                }
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
            return true;
        }

        private static string CreateSecureHash(int timestamp, string token)
        {
            // Create a new hash using current UTC timestamp.
            // Hash must be generated fresh each time.
            var data = $"{token}:{timestamp}";
            return data.ToSHA256();
        }
    }
}