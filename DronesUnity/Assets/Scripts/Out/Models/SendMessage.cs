using Newtonsoft.Json;
using System.Collections.Generic;

namespace Models
{
    public class SendMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; } = "status";

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public List<object> Message { get; set; }
    }
}