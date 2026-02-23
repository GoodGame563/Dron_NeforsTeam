using Newtonsoft.Json;
using UnityEngine;

namespace Models
{
    public class PillarStatusMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; } = "pillar_status";

        [JsonProperty("id_pillar")]
        public string IdPillar { get; set; }
    }

}