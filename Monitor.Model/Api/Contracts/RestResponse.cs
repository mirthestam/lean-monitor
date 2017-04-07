using System.Collections.Generic;
using Newtonsoft.Json;

namespace Monitor.Model.Api.Contracts
{
    /// <summary>
    /// Base API response class for the QuantConnect API.
    /// </summary>
    public class RestResponse
    {
        /// <summary>
        /// JSON Constructor
        /// </summary>
        public RestResponse()
        {
            Success = false;
            Errors = new List<string>();
        }

        /// <summary>
        /// Indicate if the API request was successful.
        /// </summary>
        [JsonProperty(PropertyName = "success")]
        public bool Success;

        /// <summary>
        /// List of errors with the API call.
        /// </summary>
        [JsonProperty(PropertyName = "errors")]
        public List<string> Errors;
    }
}
