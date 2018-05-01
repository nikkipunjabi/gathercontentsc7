using System;
using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class ItemDate
    {
        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "timezone_type")]
        public int TimezoneType { get; set; }

        [JsonProperty(PropertyName = "timezone")]
        public string Timezone { get; set; }
    }
}
