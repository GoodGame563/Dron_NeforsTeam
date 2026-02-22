using Newtonsoft.Json;
using System;

namespace Models
{
    [Serializable]
    public class EnterMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }
    }
}