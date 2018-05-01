using System.Collections.Generic;
using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class ItemsEntity
    {

        [JsonProperty(PropertyName = "data")]
        public List<GCItem> Data { get; set; }
    }
}