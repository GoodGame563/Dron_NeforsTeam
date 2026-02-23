using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class SetDrones
    {
        [JsonProperty("event")]
        public string Event { get; set; } = "set_drons";

        [JsonProperty("drons")]
        public List<Drone> Drons { get; set; }
    }
}
