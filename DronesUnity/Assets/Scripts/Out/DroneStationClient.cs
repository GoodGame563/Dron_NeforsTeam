using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

[RequireComponent(typeof(WSServer))]
public class DroneStationClient : MonoBehaviour
{
    private WSServer _ws;

    public Action<string> OnRegisterStationResponse;
    public Action<List<string>> OnRegisterDronsResponse;
    public Action<List<Models.Drone>> OnGetDronsResponse;
    public Action<GetPillarsMessage> OnGetPillarsResponse;
    public Action<string> OnError;
    
    private void Start()
    {
        _ws = this.GetComponent<WSServer>();
    }

    private void OnEnable()
    {
        _ws.OnMessageReceive += OnWsResponse;
    }

    private void OnDisable()
    {
        _ws.OnMessageReceive -= OnWsResponse;
    }

    public async Task RegisterStation(RegisterMessage message)
    {
        var jsonMessage = JsonConvert.SerializeObject(message);
        await _ws.Send(jsonMessage);
    }
    
    public async Task EnterStation(string id)
    {
        var enterMessage = new EnterMessage()
        {
            Event = "enter",
            ClientId = id,
        };
        var jsonMessage = JsonConvert.SerializeObject(enterMessage);
        await _ws.Send(jsonMessage);
    }

    public async Task RegisterDrons()
    {
        await SendEventOnly("register_drons");
    }
    
    public async Task GetDronsRequest()
    {
        await SendEventOnly("get_drons");
    }

    public async Task GetPillarsRequest()
    {
        await SendEventOnly("get_pillars");
    }

    private async Task SendEventOnly(string eventName)
    {
        var message = new EventOnly()
        {
            Event = eventName,
        };

        var jsonMessage = JsonConvert.SerializeObject(message);
        await _ws.Send(jsonMessage);
    }

    private void OnWsResponse(string msg)
    {
        var obj = JsonToModels.Parse(msg);
        switch(obj)
        {
            case GetPillarsMessage getPillarsMessages:
                OnGetPillarsResponse?.Invoke(getPillarsMessages);
                break;
            case StatusMessage statusMessage:
                InterpretStatusMessage(statusMessage);
                break;
            case GetDroneResponse droneResponse:
                OnGetDronsResponse?.Invoke(droneResponse.Drones);
                break;
        }
    }

    private void InterpretStatusMessage(StatusMessage message)
    {
        if (string.Compare(message.Status, "Err", true) == 0)
        {
            OnError?.Invoke(message.Message);
            return;
        }

        if (string.IsNullOrEmpty(message.Message))
            return;

        if (Guid.TryParse(message.Message, out var _))
        {
            OnRegisterStationResponse?.Invoke(message.Message);
            return;
        }

        List<string> ids = JsonConvert.DeserializeObject<List<string>>(message.Message);
        OnRegisterDronsResponse?.Invoke(ids);
    }
}
