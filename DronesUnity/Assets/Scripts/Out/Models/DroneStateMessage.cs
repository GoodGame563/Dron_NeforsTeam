using Newtonsoft.Json;
using System.Collections.Generic;


namespace Models
{
    public class DroneStateMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; } = "dron_state";

        [JsonProperty("drone_id")]
        public string DroneId { get; set; }

        [JsonProperty("drone_status")]
        public string DroneStatus { get; set; }

        [JsonProperty("pilar_id")]
        public string PilarId { get; set; }

        [JsonProperty("last_coordinates")]
        public Dictionary<string, float> LastCoordinates { get; set; }
    }

}