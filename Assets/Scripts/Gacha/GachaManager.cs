using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaManager : MonoBehaviour
{
    [Header("UI References")]
    public Button singlePullButton;
    public Button tenPullButton;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI pityCountText;
    public TextMeshProUGUI connectionStatusText;
    public GameObject gachaAnimationPanel;
    public GameObject resultPanel;
    
    [Header("Animation")]
    public GachaAnimation gachaAnimation;
    
    [Header("Result Display")]
    public Transform resultContainer;
    public GameObject resultCardPrefab;
    
    [Header("Settings")]
    public int singlePullCost = 160;
    public int tenPullCost = 1600;
    
    private UserInfo currentUserInfo;
    private bool isProcessing = false;
    
    private void Start()
    {
        // Subscribe to WebSocket events
        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.OnGachaSuccess += OnGachaSuccess;
            WebSocketManager.Instance.OnGachaError += OnGachaError;
            WebSocketManager.Instance.OnUserInfoReceived += OnUserInfoReceived;
            WebSocketManager.Instance.OnConnected += OnWebSocketConnected;
            WebSocketManager.Instance.OnDisconnected += OnWebSocketDisconnected;
        }
        
        // Button events
        if (singlePullButton != null)
            singlePullButton.onClick.AddListener(OnSinglePullClicked);
        
        if (tenPullButton != null)
            tenPullButton.onClick.AddListener(OnTenPullClicked);
        
        // Initialize UI
        UpdateConnectionStatus();
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.OnGachaSuccess -= OnGachaSuccess;
            WebSocketManager.Instance.OnGachaError -= OnGachaError;
            WebSocketManager.Instance.OnUserInfoReceived -= OnUserInfoReceived;
            WebSocketManager.Instance.OnConnected -= OnWebSocketConnected;
            WebSocketManager.Instance.OnDisconnected -= OnWebSocketDisconnected;
        }
        
        if (singlePullButton != null)
            singlePullButton.onClick.RemoveListener(OnSinglePullClicked);
        
        if (tenPullButton != null)
            tenPullButton.onClick.RemoveListener(OnTenPullClicked);
    }
    
    // WebSocket connection events
    private void OnWebSocketConnected()
    {
        Debug.Log("Connected to server!");
        UpdateConnectionStatus();
        RefreshUserInfo();
    }
    
    private void OnWebSocketDisconnected()
    {
        Debug.Log("Disconnected from server!");
        UpdateConnectionStatus();
    }
    
    // Refresh user info
    public void RefreshUserInfo()
    {
        if (WebSocketManager.Instance != null && WebSocketManager.Instance.IsConnected())
        {
            WebSocketManager.Instance.RequestUserInfo();
        }
    }
    
    // Single pull button clicked (PUBLIC so Unity Button can see it)
    public void OnSinglePullClicked()
    {
        Debug.Log("OnSinglePullClicked");
        
        isProcessing = false;

        if (isProcessing) return;
        
        if (WebSocketManager.Instance == null || !WebSocketManager.Instance.IsConnected())
        {
            ShowError("Not connected to server!");
            return;
        }
        
        if (currentUserInfo != null && currentUserInfo.currency < singlePullCost)
        {
            ShowError("Insufficient currency!");
            return;
        }
        
        isProcessing = true;
        if (singlePullButton != null)
            singlePullButton.interactable = false;
        if (tenPullButton != null)
            tenPullButton.interactable = false;
        
        WebSocketManager.Instance.RequestSinglePull();
    }
    
    // Ten pull button clicked (PUBLIC so Unity Button can see it)
    public void OnTenPullClicked()
    {
        if (isProcessing) return;
        
        if (WebSocketManager.Instance == null || !WebSocketManager.Instance.IsConnected())
        {
            ShowError("Not connected to server!");
            return;
        }
        
        if (currentUserInfo != null && currentUserInfo.currency < tenPullCost)
        {
            ShowError("Insufficient currency!");
            return;
        }
        
        isProcessing = true;
        if (singlePullButton != null)
            singlePullButton.interactable = false;
        if (tenPullButton != null)
            tenPullButton.interactable = false;
        
        WebSocketManager.Instance.RequestTenPull();
    }
    
    // Gacha success callback
    private void OnGachaSuccess(GachaResult result)
    {
        Debug.Log("Gacha success! Got " + result.characters.Length + " characters");
        
        // Show gacha animation
        StartCoroutine(PlayGachaSequence(result));
    }
    
    // Gacha error callback
    private void OnGachaError(string error)
    {
        Debug.LogError("Gacha error: " + error);
        ShowError(error);
        isProcessing = false;
        if (singlePullButton != null)
            singlePullButton.interactable = true;
        if (tenPullButton != null)
            tenPullButton.interactable = true;
    }
    
    // User info received callback
    private void OnUserInfoReceived(UserInfo userInfo)
    {
        currentUserInfo = userInfo;
        UpdateUI();
    }
    
    // Update connection status
    private void UpdateConnectionStatus()
    {
        if (connectionStatusText != null)
        {
            bool connected = WebSocketManager.Instance != null && WebSocketManager.Instance.IsConnected();
            connectionStatusText.text = connected ? "Connected" : "Disconnected";
            connectionStatusText.color = connected ? Color.green : Color.red;
        }
    }
    
    // Update UI
    private void UpdateUI()
    {
        if (currentUserInfo != null)
        {
            if (currencyText != null)
                currencyText.text = "Currency: " + currentUserInfo.currency;
            
            if (pityCountText != null)
                pityCountText.text = "Pity Progress: " + currentUserInfo.pityCount + "/90";
            
            bool canPull = WebSocketManager.Instance != null && 
                          WebSocketManager.Instance.IsConnected() && 
                          !isProcessing;
            
            if (singlePullButton != null)
                singlePullButton.interactable = canPull && currentUserInfo.currency >= singlePullCost;
            
            if (tenPullButton != null)
                tenPullButton.interactable = canPull && currentUserInfo.currency >= tenPullCost;
        }
        else
        {
            if (singlePullButton != null)
                singlePullButton.interactable = false;
            if (tenPullButton != null)
                tenPullButton.interactable = false;
        }
    }
    
    // Play gacha sequence
    private IEnumerator PlayGachaSequence(GachaResult result)
    {
        // Show animation panel
        if (gachaAnimationPanel != null)
            gachaAnimationPanel.SetActive(true);
        if (resultPanel != null)
            resultPanel.SetActive(false);
        
        // Play gacha animation
        if (gachaAnimation != null)
        {
            yield return StartCoroutine(gachaAnimation.PlayAnimation(result.characters));
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }
        
        // Hide animation panel
        if (gachaAnimationPanel != null)
            gachaAnimationPanel.SetActive(false);
        
        // Show results
        ShowResults(result);
        
        // Refresh user info
        RefreshUserInfo();
        
        isProcessing = false;
    }
    
    // Show results
    private void ShowResults(GachaResult result)
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);
        
        if (resultContainer == null)
            return;
        
        // Clear previous results
        foreach (Transform child in resultContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create result cards
        if (resultCardPrefab != null)
        {
            for (int i = 0; i < result.characters.Length; i++)
            {
                GameObject card = Instantiate(resultCardPrefab, resultContainer);
                ResultCard resultCard = card.GetComponent<ResultCard>();
                
                if (resultCard != null)
                {
                    resultCard.Setup(result.characters[i], result.isNew[i]);
                }
            }
        }
    }
    
    // Show error message
    private void ShowError(string message)
    {
        Debug.LogError(message);
        // You can display an error UI here
    }
    
    // For testing: Add currency
    public void AddTestCurrency()
    {
        if (WebSocketManager.Instance != null && WebSocketManager.Instance.IsConnected())
        {
            WebSocketManager.Instance.AddCurrency(10000);
        }
        else
        {
            ShowError("Not connected to server!");
        }
    }
    
    // Reconnect button
    public void ReconnectToServer()
    {
        if (WebSocketManager.Instance != null)
        {
            StartCoroutine(ReconnectCoroutine());
        }
    }
    
    private IEnumerator ReconnectCoroutine()
    {
        yield return WebSocketManager.Instance.ConnectWebSocket();
    }
}