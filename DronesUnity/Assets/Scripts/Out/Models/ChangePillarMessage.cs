using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine;

namespace Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PillarStatus
    {
        [JsonProperty("death")]
        Death,

        [JsonProperty("alive")]
        Alive,

        [JsonProperty("empty")]
        Empty
    }

    [Serializable]
    public class ChangePillarData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public PillarStatus Status { get; set; }
        
    }

    [Serializable]
    public class ChangePillarMessage
    {
        [JsonProperty("event")]
        public string Event { set; get; }

        [JsonProperty("data")]
        public ChangePillarData Data;
    }
}
