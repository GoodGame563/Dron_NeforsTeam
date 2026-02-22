using NativeWebSocket;
using System;
using System.Net.WebSockets;
using UnityEngine;

public class WsClient : MonoBehaviour
{
    [SerializeField] private string url = "ws://localhost:8080";
    [SerializeField] private float reconnectDelay = 3f;

    private WebSocket _ws;
    private bool _isReconnecting;

    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnMessageReceived;
    public event Action<string> OnError;

    private async void Start()
    {
        await Connect();
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _ws?.DispatchMessageQueue();
#endif
    }

    private async void OnDestroy()
    {
        _isReconnecting = false;
        if (_ws != null)
            await _ws.Close();
    }

    private async void Connect()
    {
        if (_isReconnecting) return;

        _ws = new WebSocket(url);

        _ws.OnOpen += () =>
        {
            Debug.Log("[WsClient] Connected");
            OnConnected?.Invoke();
        };

        _ws.OnClose += (code) =>
        {
            Debug.Log($"[WsClient] Disconnected: {code}");
            OnDisconnected?.Invoke();
            TryReconnect();
        };

        _ws.OnError += (error) =>
        {
            Debug.LogError($"[WsClient] Error: {error}");
            OnError?.Invoke(error);
        };

        _ws.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            OnMessageReceived?.Invoke(message);
        };

        await _ws.Connect();
    }

    public async void Send(string message)
    {
        if (_ws == null || _ws.State != WebSocketState.Open)
        {
            Debug.LogWarning("[WsClient] Cannot send — not connected");
            return;
        }

        await _ws.SendText(message);
    }

    private async void TryReconnect()
    {
        if (_isReconnecting) return;
        _isReconnecting = true;

        Debug.Log($"[WsClient] Reconnecting in {reconnectDelay}s...");
        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(reconnectDelay));

        _isReconnecting = false;
        Connect();
    }
}