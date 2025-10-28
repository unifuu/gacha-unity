using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager Instance { get; private set; }
    
    private string wsURL = "ws://localhost:8080/ws";
    
    private WebSocket websocket;
    private bool isConnected = false;
    private Queue<string> messageQueue = new Queue<string>();
    
    // Message types
    private const string TYPE_SINGLE_PULL = "single_pull";
    private const string TYPE_TEN_PULL = "ten_pull";
    private const string TYPE_GET_USER_INFO = "get_user_info";
    private const string TYPE_GET_INVENTORY = "get_inventory";
    private const string TYPE_GET_POOL = "get_pool";
    private const string TYPE_ADD_CURRENCY = "add_currency";
    private const string TYPE_PING = "ping";
    
    // Response types
    private const string TYPE_GACHA_RESULT = "gacha_result";
    private const string TYPE_USER_INFO = "user_info";
    private const string TYPE_INVENTORY = "inventory";
    private const string TYPE_POOL_INFO = "pool_info";
    private const string TYPE_CURRENCY_UPDATE = "currency_update";
    private const string TYPE_ERROR = "error";
    private const string TYPE_PONG = "pong";
    
    // Events
    public event Action<GachaResult> OnGachaSuccess;
    public event Action<string> OnGachaError;
    public event Action<UserInfo> OnUserInfoReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private async void Start()
    {
        await ConnectWebSocket();
    }
    
    private void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
        #endif
        
        // Process queued messages on main thread
        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            ProcessMessage(message);
        }
    }
    
    // Connect to WebSocket server
    public async System.Threading.Tasks.Task ConnectWebSocket()
    {
        if (isConnected)
        {
            Debug.Log("WebSocket already connected");
            return;
        }
        
        websocket = new WebSocket(wsURL);
        
        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connected!");
            isConnected = true;
            OnConnected?.Invoke();
        };
        
        websocket.OnError += (e) =>
        {
            Debug.LogError("WebSocket error: " + e);
        };
        
        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket closed!");
            isConnected = false;
            OnDisconnected?.Invoke();
        };
        
        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            // Queue message for processing on main thread
            messageQueue.Enqueue(message);
        };
        
        try
        {
            await websocket.Connect();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect WebSocket: {e.Message}");
        }
    }
    
    // Disconnect WebSocket
    public async void DisconnectWebSocket()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
    
    // Send message to server
    private async void SendMessage(string type, object data = null)
    {
        if (!isConnected)
        {
            Debug.LogError("WebSocket not connected");
            return;
        }
        
        WebSocketMessage msg = new WebSocketMessage
        {
            type = type,
            data = data != null ? JsonUtility.ToJson(data) : null
        };
        
        string json = JsonUtility.ToJson(msg);
        
        try
        {
            await websocket.SendText(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send message: {e.Message}");
        }
    }
    
    // Process incoming messages
    private void ProcessMessage(string message)
    {
        try
        {
            WebSocketMessage msg = JsonUtility.FromJson<WebSocketMessage>(message);
            
            switch (msg.type)
            {
                case TYPE_GACHA_RESULT:
                    GachaResult gachaResult = JsonUtility.FromJson<GachaResult>(msg.data);
                    OnGachaSuccess?.Invoke(gachaResult);
                    break;
                    
                case TYPE_USER_INFO:
                    UserInfo userInfo = JsonUtility.FromJson<UserInfo>(msg.data);
                    OnUserInfoReceived?.Invoke(userInfo);
                    break;
                    
                case TYPE_CURRENCY_UPDATE:
                    // Currency update also triggers user info update
                    RequestUserInfo();
                    break;
                    
                case TYPE_ERROR:
                    Debug.LogError($"Server error: {msg.error}");
                    OnGachaError?.Invoke(msg.error);
                    break;
                    
                case TYPE_PONG:
                    Debug.Log("Received pong from server");
                    break;
                    
                default:
                    Debug.Log($"Received message type: {msg.type}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to process message: {e.Message}");
        }
    }
    
    // Public API methods
    
    public void RequestSinglePull()
    {
        SendMessage(TYPE_SINGLE_PULL);
    }
    
    public void RequestTenPull()
    {
        SendMessage(TYPE_TEN_PULL);
    }
    
    public void RequestUserInfo()
    {
        SendMessage(TYPE_GET_USER_INFO);
    }
    
    public void RequestInventory()
    {
        SendMessage(TYPE_GET_INVENTORY);
    }
    
    public void RequestPoolInfo()
    {
        SendMessage(TYPE_GET_POOL);
    }
    
    public void AddCurrency(int amount)
    {
        var request = new { amount = amount };
        SendMessage(TYPE_ADD_CURRENCY, request);
    }
    
    public void SendPing()
    {
        SendMessage(TYPE_PING);
    }
    
    // Check if connected
    public bool IsConnected()
    {
        return isConnected && websocket != null && websocket.State == WebSocketState.Open;
    }
    
    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
    
    private void OnDestroy()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.Close();
        }
    }
}