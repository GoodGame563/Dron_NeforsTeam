using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Models
{
    [Serializable]
    public class GetPillarsData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("coordinates")]
        public Coordinates Coordinates;

        [JsonProperty("state")]
        public DroneStatus dronState;

        [JsonProperty("pillar_station_id")]
        public string PillarStationId { set; get; }

        [JsonProperty("id_dron_station")]
        public string IdDronStation { set; get; }

        [JsonProperty("last_update")]
        public DateTime UpdatedAt;
    }

    [Serializable]
    public class GetPillarsMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; } = "get_pillars";

        [JsonProperty("data")]
        public List<GetPillarsData> Data;
    }
}
