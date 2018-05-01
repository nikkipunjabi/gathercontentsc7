using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class StatusEntity
    {
        [JsonProperty(PropertyName = "data")]
        public GCStatus Data { get; set; }
    }
}