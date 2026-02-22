using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DroneStatus
    {
        [JsonProperty("in_station")]
        InStation,

        [JsonProperty("fly")]
        Fly,

        [JsonProperty("broken")]
        Broken
    }

}