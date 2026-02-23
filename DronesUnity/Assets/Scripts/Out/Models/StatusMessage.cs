using Newtonsoft.Json;

namespace Models
{
    public class StatusMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; } = "status";

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = "";
    }
}
