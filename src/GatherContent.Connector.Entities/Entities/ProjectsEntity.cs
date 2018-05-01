using System.Collections.Generic;
using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class ProjectsEntity
    {
        [JsonProperty(PropertyName = "data")]
        public List<Project> Data { get; set; }

    }
}