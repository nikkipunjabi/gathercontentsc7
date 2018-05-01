using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class Option
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "selected")]
        public bool Selected { get; set; }
    }
}
