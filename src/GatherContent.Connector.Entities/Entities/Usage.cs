using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class Usage
    {
        [JsonProperty(PropertyName = "item_count")]
        public int Count { get; set; }
    }
}
