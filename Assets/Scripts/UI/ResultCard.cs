using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultCard : MonoBehaviour
{
    [Header("UI Elements")]
    public Image cardBackground;
    public Image characterImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI rarityText;
    public GameObject newTag;
    
    [Header("Rarity Colors")]
    public Color ssrColor = new Color(1f, 0.84f, 0f);
    public Color srColor = new Color(0.75f, 0f, 1f);
    public Color rColor = new Color(0.5f, 0.5f, 0.5f);
    
    [Header("Animation")]
    public float animationDelay = 0.1f;
    
    public void Setup(GachaCharacter character, bool isNew)
    {
        // Set name
        if (nameText != null)
        {
            nameText.text = character.name;
        }
        
        // Set rarity
        if (rarityText != null)
        {
            rarityText.text = GetRarityStars(character.rarity);
            rarityText.color = GetRarityColor(character.rarity);
        }
        
        // Set background color
        if (cardBackground != null)
        {
            Color bgColor = GetRarityColor(character.rarity);
            bgColor.a = 0.3f;
            cardBackground.color = bgColor;
        }
        
        // Set character image (simplified, should load actual image)
        if (characterImage != null)
        {
            characterImage.color = GetRarityColor(character.rarity);
            // In production, use this to load images:
            // StartCoroutine(LoadImage(character.imageUrl));
        }
        
        // Show NEW tag
        if (newTag != null)
        {
            newTag.SetActive(isNew);
        }
        
        // Play entry animation
        StartCoroutine(PlayEntryAnimation());
    }
    
    private Color GetRarityColor(int rarity)
    {
        switch (rarity)
        {
            case 5: return ssrColor;
            case 4: return srColor;
            case 3: return rColor;
            default: return Color.white;
        }
    }
    
    private string GetRarityStars(int rarity)
    {
        string stars = "";
        for (int i = 0; i < rarity; i++)
        {
            stars += "★";
        }
        return stars;
    }
    
    private System.Collections.IEnumerator PlayEntryAnimation()
    {
        // Initial state
        transform.localScale = Vector3.zero;
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;
        
        yield return new WaitForSeconds(animationDelay);
        
        // Scale and fade-in animation
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Elastic scale
            float scale = EaseOutBack(t);
            transform.localScale = Vector3.one * scale;
            
            // Fade in
            canvasGroup.alpha = t;
            
            yield return null;
        }
        
        transform.localScale = Vector3.one;
        canvasGroup.alpha = 1;
    }
    
    // Elastic easing function
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }
    
    // Load network image (optional implementation)
    private System.Collections.IEnumerator LoadImage(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }
        
        UnityEngine.Networking.UnityWebRequest request = 
            UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
            if (characterImage != null)
            {
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                characterImage.sprite = sprite;
                characterImage.color = Color.white;
            }
        }
        else
        {
            Debug.LogError("Failed to load image: " + url);
        }
    }
}