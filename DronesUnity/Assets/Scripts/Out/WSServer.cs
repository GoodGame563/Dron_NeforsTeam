using NativeWebSocket;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class WSServer : MonoBehaviour
{
    [SerializeField] private string url = "ws://localhost:8080";

    private WebSocket _ws;

    public Action<string> OnMessageReceive;

    private async void Start()
    {
        _ws = new WebSocket(url);

        _ws.OnOpen += () =>
        {
            Debug.Log("[WsClient] Connected");
        };

        _ws.OnClose += (code) =>
        {
            Debug.Log($"[WsClient] Disconnected: {code}");
        };

        _ws.OnError += (error) =>
        {
            Debug.LogError($"[WsClient] Error: {error}");
        };

        _ws.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            OnMessageReceive?.Invoke(message);
        };

        await _ws.Connect();
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _ws?.DispatchMessageQueue();
#endif
    }

    private async void OnDestroy()
    {
        if (_ws != null)
            await _ws.Close();
    }

    public async Task Send(string message)
    {
        if (_ws == null || _ws.State != WebSocketState.Open)
        {
            Debug.LogWarning("[WsClient] Cannot send — not connected");
            return;
        }

        await _ws.SendText(message);
    }
}