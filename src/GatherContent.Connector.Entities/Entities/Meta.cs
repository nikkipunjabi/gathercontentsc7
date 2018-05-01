using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class Meta
    {
        [JsonProperty(PropertyName = "templates")]
        public int Templates { get; set; }
    }
}
