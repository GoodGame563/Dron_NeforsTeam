using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Models
{
    [Serializable]
    public class GetPillarsMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; } = "get_pillars";

        [JsonProperty("pillars")]
        public List<Dictionary<string, object>> Pillars { get; set; }
    }
}
