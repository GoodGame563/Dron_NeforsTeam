using Newtonsoft.Json;
using UnityEngine;

namespace Models
{
    public class Drone
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public DroneStatus Status { get; set; }

        [JsonProperty("last_coordinates")]
        public Coordinates LastCoordinates { get; set; }
    }
}