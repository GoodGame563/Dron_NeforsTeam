using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace Models
{
    [Serializable]
    public class GetDroneResponse
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public List<Drone> Drones { get; set; }
    }
}
