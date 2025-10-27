using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class Character
{
    public int id;
    public string name;
    public int rarity;
    public string imageUrl;
    public float rate;
}

[Serializable]
public class GachaResult
{
    public Character[] characters;
    public bool[] isNew;
    public long timestamp;
}

[Serializable]
public class UserInfo
{
    public string username;
    public int currency;
    public int pityCount;
}

[Serializable]
public class AddCurrencyRequest
{
    public int amount;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    
    // Change to your backend address
    private string baseURL = "http://localhost:8080/api";
    
    // Events
    public event Action<GachaResult> OnGachaSuccess;
    public event Action<string> OnGachaError;
    public event Action<UserInfo> OnUserInfoReceived;
    
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
    
    // Single pull
    public void RequestSinglePull()
    {
        StartCoroutine(PostRequest("/gacha/pull", null, OnGachaSuccess, OnGachaError));
    }
    
    // Ten pull
    public void RequestTenPull()
    {
        StartCoroutine(PostRequest("/gacha/pull-ten", null, OnGachaSuccess, OnGachaError));
    }
    
    // Get user info
    public void RequestUserInfo()
    {
        StartCoroutine(GetRequest<UserInfo>("/user/info", OnUserInfoReceived, null));
    }
    
    // Add currency (for testing)
    public void AddCurrency(int amount)
    {
        AddCurrencyRequest request = new AddCurrencyRequest { amount = amount };
        string jsonData = JsonUtility.ToJson(request);
        StartCoroutine(PostRequest("/user/add-currency", jsonData, 
            (result) => {
                Debug.Log("Currency added successfully");
                RequestUserInfo();
            }, 
            (error) => Debug.LogError("Failed to add currency: " + error)));
    }
    
    // Generic POST request
    private IEnumerator PostRequest(string endpoint, string jsonData, 
        Action<GachaResult> onSuccess, Action<string> onError)
    {
        string url = baseURL + endpoint;
        
        UnityWebRequest request;
        if (string.IsNullOrEmpty(jsonData))
        {
            request = UnityWebRequest.PostWwwForm(url, "");
        }
        else
        {
            request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        }
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("Response: " + responseText);
            
            try
            {
                GachaResult result = JsonUtility.FromJson<GachaResult>(responseText);
                onSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                Debug.LogError("Parse error: " + e.Message);
                onError?.Invoke("Failed to parse data");
            }
        }
        else
        {
            Debug.LogError("Request error: " + request.error);
            onError?.Invoke(request.error);
        }
        
        request.Dispose();
    }

    // Generic GET request
    private IEnumerator GetRequest<T>(string endpoint, Action<T> onSuccess, Action<string> onError)
    {
        string url = baseURL + endpoint;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Response: " + responseText);

                try
                {
                    T result = JsonUtility.FromJson<T>(responseText);
                    onSuccess?.Invoke(result);
                }
                catch (Exception e)
                {
                    Debug.LogError("Parse error: " + e.Message);
                    onError?.Invoke("Failed to parse data");
                }
            }
            else
            {
                Debug.LogError("Request error: " + request.error);
                onError?.Invoke(request.error);
            }
        }
    }
}