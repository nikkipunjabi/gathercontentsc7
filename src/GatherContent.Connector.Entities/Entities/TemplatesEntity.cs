using System.Collections.Generic;
using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class TemplatesEntity
    {
        [JsonProperty(PropertyName = "data")]
        public List<GCTemplate> Data { get; set; }
    }
}