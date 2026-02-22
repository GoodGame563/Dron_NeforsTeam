using Newtonsoft.Json;
using UnityEngine;

public class Coordinates
{
    [JsonProperty("latitude")]
    public float Latitude { get; set; }

    [JsonProperty("longtiude")]
    public float Longtiude { get; set; }
}
