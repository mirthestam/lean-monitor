using System;
using Newtonsoft.Json;

namespace Monitor.Model.Api.Contracts
{
    /// <summary>
    /// Response from reading a project by id.
    /// </summary>
    public class Project : RestResponse
    {
        /// <summary>
        /// Project id
        /// </summary>
        [JsonProperty(PropertyName = "projectId")]
        public int ProjectId;

        /// <summary>
        /// Name of the project
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name;

        /// <summary>
        /// Date the project was created
        /// </summary>
        [JsonProperty(PropertyName = "created")]
        public DateTime Created;

        /// <summary>
        /// Modified date for the project
        /// </summary>
        [JsonProperty(PropertyName = "modified")]
        public DateTime Modified;
    }
}