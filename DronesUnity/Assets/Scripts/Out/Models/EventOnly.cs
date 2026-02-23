using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class EventOnly
    {
        [JsonProperty("event")]
        public string Event { get; set; }
    }
}
