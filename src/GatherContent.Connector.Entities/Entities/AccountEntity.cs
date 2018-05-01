using System.Collections.Generic;
using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class AccountEntity
    {
        [JsonProperty(PropertyName = "data")]
        public List<Account> Data { get; set; }

    }
}