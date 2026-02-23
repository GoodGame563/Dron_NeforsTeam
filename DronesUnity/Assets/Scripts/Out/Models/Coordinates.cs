using Newtonsoft.Json;
using UnityEngine;

public class Coordinates
{
    [JsonProperty("x")]
    public float X { get; set; }

    [JsonProperty("y")]
    public float Y { get; set; }
}
