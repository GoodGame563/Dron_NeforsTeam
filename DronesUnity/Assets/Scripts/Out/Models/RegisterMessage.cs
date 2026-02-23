using Newtonsoft.Json;

namespace Models
{
    public class RegisterMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; } = "register";

        [JsonProperty("coordinates")]
        public Coordinates Coordinates { get; set; }

        [JsonProperty("radius")]
        public int Radius { get; set; }

        [JsonProperty("total_drone_count")]
        public int TotalDroneCount { get; set; }

        [JsonProperty("total_lamps_count")]
        public int TotalLampsCount { get; set; }
    }
}