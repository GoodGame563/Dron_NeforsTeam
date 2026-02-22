using Newtonsoft.Json;

namespace Models
{
    public class PillarMessage
    {
        [JsonProperty("pillar_id")]
        public string PillarId { get; set; }

        [JsonProperty("x")]
        public float X { get; set; }

        [JsonProperty("y")]
        public float Y { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("pillar_station_id")]
        public string PillarStationId { get; set; }
    }
}
