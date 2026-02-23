using Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

[RequireComponent(typeof(WSServer))]
public class PillarStationClient : MonoBehaviour
{
    private WSServer _ws;

    public Action<string> OnError;

    private void OnEnable()
    {
        _ws.OnMessageReceive += OnMessageReceive;
    }

    private void OnDisable()
    {
        _ws.OnMessageReceive -= OnMessageReceive;
    }

    public async Task Enter(string id)
    {
        var message = new EnterMessage()
        {
            Event = "enter",
            ClientId = id,
        };
        var jsonMessage = JsonConvert.SerializeObject(message);
        await _ws.Send(jsonMessage);
    }

    public async Task PillarOff(string id)
    {
        var message = new PillarStatusMessage()
        {
            IdPillar = id,
        };
        var jsonMessage = JsonConvert.SerializeObject(message);
        await _ws.Send(jsonMessage);
    }

    private void OnMessageReceive(string msg)
    {
        var obj = JsonToModels.Parse(msg);
        switch (obj)
        {
            case StatusMessage message:
                InterpreterStatusMessage(message);
                break;
        }
    }

    private void InterpreterStatusMessage(StatusMessage msg)
    {
        if (string.Compare(msg.Status, "Err", true) == 0)
        {
            OnError?.Invoke(msg.Message);
            return;
        }
    }
}
