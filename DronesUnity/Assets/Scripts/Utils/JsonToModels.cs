using Models;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Utils
{
    public static class JsonToModels
    {
        public static object Parse(string json)
        {
            var jObject = JObject.Parse(json);

            var eventType = jObject["event"]?.ToString();

            return eventType switch
            {
                "register" => jObject.ToObject<RegisterMessage>(),
                "enter" => jObject.ToObject<EnterMessage>(),
                "set_drons" => jObject.ToObject<SetDrones>(),
                "dron_state" => jObject.ToObject<DroneStateMessage>(),
                "status" => jObject.ToObject<StatusMessage>(),
                "pillar_status" => jObject.ToObject<PillarStatusMessage>(),
                "get_pillars" => jObject.ToObject<GetPillarsMessage>(),
                _ => throw new Exception($"Unknown event type: {eventType}")
            };
        }
    }
}

