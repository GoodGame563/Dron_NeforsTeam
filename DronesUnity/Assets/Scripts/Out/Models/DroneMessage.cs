using Newtonsoft.Json;
using UnityEngine;

namespace Models
{
    public class DroneMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("last_coordinates")]
        public Coordinates LastCoordinates { get; set; }
    }
}