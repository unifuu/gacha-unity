using System;

/// <summary>
/// All data models for the gacha game
/// Place this file at: Assets/Scripts/Models/DataModels.cs
/// </summary>

// Character model - renamed to avoid conflict with Unity's Character class
[Serializable]
public class GachaCharacter
{
    public int id;
    public string name;
    public int rarity;          // 3 = R, 4 = SR, 5 = SSR
    public string imageUrl;
    public float rate;          // Pull rate weight
}

// Gacha result from server
[Serializable]
public class GachaResult
{
    public GachaCharacter[] characters;
    public bool[] isNew;        // Whether each character is newly obtained
    public long timestamp;
}

// User information
[Serializable]
public class UserInfo
{
    public string username;
    public int currency;        // In-game currency for pulls
    public int pityCount;       // Current pity counter (0-90)
}

// WebSocket message wrapper
[Serializable]
public class WebSocketMessage
{
    public string type;         // Message type (e.g., "single_pull", "gacha_result")
    public string data;         // JSON data as string
    public string error;        // Error message if any
}

// Request to add currency (testing only)
[Serializable]
public class AddCurrencyRequest
{
    public int amount;
}

// Currency update response
[Serializable]
public class CurrencyResponse
{
    public int currency;
}

// Pool information response
[Serializable]
public class PoolInfo
{
    public GachaCharacter[] characters;
    public RateInfo rates;
    public string pitySystem;
}

// Rate information
[Serializable]
public class RateInfo
{
    public string ssr;          // "2%"
    public string sr;           // "10%"
    public string r;            // "88%"
}

// Inventory response
[Serializable]
public class InventoryResponse
{
    public GachaCharacter[] inventory;
    public int count;
}