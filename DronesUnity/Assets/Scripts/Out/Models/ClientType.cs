using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ClientType
    {
        [JsonProperty("drone_station")]
        DroneStation,

        [JsonProperty("pillar_station")]
        PillarStation,

        [JsonProperty("frontend")]
        Frontend
    }
}