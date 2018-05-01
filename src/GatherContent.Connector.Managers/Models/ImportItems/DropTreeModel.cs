using System.Collections.Generic;
using Newtonsoft.Json;

namespace GatherContent.Connector.Managers.Models.ImportItems
{
    public class DropTreeModel
    {
        public DropTreeModel()
        {
            children = new List<DropTreeModel>();
        }

        [JsonProperty(PropertyName = "title")]
        public string title { get; set; }
        [JsonProperty(PropertyName = "key")]
        public string key { get; set; }
        [JsonProperty(PropertyName = "children")]
        public List<DropTreeModel> children { get; set; }
        [JsonProperty(PropertyName = "isLazy")]
        public bool isLazy { get; set; }
        [JsonProperty(PropertyName = "icon")]
        public string icon { get; set; }
        [JsonProperty(PropertyName = "select")]
        public bool selected { get; set; }
        [JsonProperty(PropertyName = "expand")]
        public bool expanded { get; set; }
    }
}
