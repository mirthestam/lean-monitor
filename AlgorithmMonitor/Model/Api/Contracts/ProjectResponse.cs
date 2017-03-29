using System.Collections.Generic;
using Newtonsoft.Json;

namespace Monitor.Model.Api.Contracts
{
    /// <summary>Project list response</summary>
    public class ProjectResponse : RestResponse
    {
        /// <summary>List of projects for the authenticated user</summary>
        [JsonProperty(PropertyName = "projects")]
        public List<Project> Projects;
    }
}