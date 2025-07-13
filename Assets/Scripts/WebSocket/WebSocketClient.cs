using Meta.Net.NativeWebSocket;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

/*
 * Below shows sample code of how to send/receive data via websocket...
 * 
 * // Somewhere else in your game logic …
    var client = FindObjectOfType<QuestWebSocketClient>();

    // Send data
    client.SendData(new Dictionary<string,string>{
        {"place","Home"},
        {"mood","sad"},
        ...
    });

    // Read the server's last reply later on
    Dictionary<string,string> serverMsg = client.GetLatestData();
    if (serverMsg.TryGetValue("status", out var status))
        Debug.Log($"Server status: {status}");
 * 
 */

public class WebSocketClient : MonoBehaviour
{
    public static WebSocketClient Instance { get; private set; }

    private WebSocket websocket;
    private const string SERVER_URI = "ws://10.0.0.15:2025/quest";

    [Header("When Receives Message")]
    public UnityEvent<string> onReceivedMessage;

    /// <summary>Holds the most-recent message received from the server.</summary>
    public string recentInfo;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    public void SendTestData()
    {
        List<ContextData> contextDatas = new List<ContextData> {
            { new ContextData { name = "place", value = "Home" }},
            { new ContextData { name = "mood", value = "angry" }},
            { new ContextData { name = "time", value = "18:00" }},
            { new ContextData { name = "weather", value = null }},
        };

        SendData(contextDatas);
    }

    async void Start()
    {
        websocket = new WebSocket(SERVER_URI);

        websocket.OnOpen += () => Debug.Log("WebSocket connected");
        websocket.OnError += e => Debug.LogError($"WebSocket error: {e}");
        websocket.OnClose += e => Debug.Log($"WebSocket closed (code {e})");

        websocket.OnMessage += HandleIncomingMessage;

        await websocket.Connect();
    }

    /* ──────────────────────────────  SENDING  ────────────────────────────── */

    /// <summary>Serialises a Dictionary<string,string> to JSON and sends it.</summary>
    public async void SendData(List<ContextData> payload)
    {
        if (websocket.State != WebSocketState.Open) {
            Debug.LogWarning("WebSocket is not connected.");
            return;
        }

        string json = JsonConvert.SerializeObject(payload);
        await websocket.SendText(json);
    }

    /* ─────────────────────────────  RECEIVING  ───────────────────────────── */

    private void HandleIncomingMessage(byte[] bytes)
    {
        recentInfo = Encoding.UTF8.GetString(bytes);
        Debug.Log($"Incoming info: {recentInfo}");        

        // call function.
        onReceivedMessage?.Invoke(recentInfo);
    }    

    /* ──────────────────────────  LIFECYCLE UTILS  ────────────────────────── */

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
            await websocket.Close();
    }
}
