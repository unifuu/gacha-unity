using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaAnimation : MonoBehaviour
{
    [Header("Animation Elements")]
    public ParticleSystem starParticles;
    public Image flashImage;
    public Image cardImage;
    public TextMeshProUGUI rarityText;
    public GameObject skipButton;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float flashDuration = 0.3f;
    public float cardRevealDelay = 1.0f;
    public float cardDisplayDuration = 1.5f;
    
    [Header("Rarity Colors")]
    public Color ssrColor = new Color(1f, 0.84f, 0f); // Gold
    public Color srColor = new Color(0.75f, 0f, 1f);  // Purple
    public Color rColor = new Color(0.5f, 0.5f, 0.5f); // Gray
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gachaStartSound;
    public AudioClip ssrRevealSound;
    public AudioClip srRevealSound;
    public AudioClip rRevealSound;
    
    private bool skipRequested = false;
    
    private void Start()
    {
        if (skipButton != null)
        {
            Button btn = skipButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OnSkipClicked);
            }
        }
    }
    
    // Play gacha animation
    public IEnumerator PlayAnimation(GachaCharacter[] characters)
    {
        skipRequested = false;
        
        if (skipButton != null)
            skipButton.SetActive(true);
        
        // Play animation for each character
        for (int i = 0; i < characters.Length; i++)
        {
            if (skipRequested)
            {
                // If skipped, quickly show all results
                break;
            }
            
            yield return StartCoroutine(PlaySingleCharacterAnimation(characters[i]));
            
            // Short pause between characters (if not the last one)
            if (i < characters.Length - 1 && !skipRequested)
            {
                yield return new WaitForSeconds(0.3f);
            }
        }
        
        if (skipButton != null)
            skipButton.SetActive(false);
    }
    
    // Play single character animation
    private IEnumerator PlaySingleCharacterAnimation(GachaCharacter character)
    {
        // 1. Initialize
        if (flashImage != null)
        {
            flashImage.color = new Color(1, 1, 1, 0);
        }
        
        if (cardImage != null)
        {
            cardImage.color = new Color(1, 1, 1, 0);
        }
        
        if (rarityText != null)
        {
            rarityText.gameObject.SetActive(false);
        }
        
        // 2. Play start sound
        if (audioSource != null && gachaStartSound != null && !skipRequested)
        {
            audioSource.PlayOneShot(gachaStartSound);
        }
        
        // 3. Particle effects
        if (starParticles != null && !skipRequested)
        {
            starParticles.Play();
        }
        
        if (!skipRequested)
            yield return new WaitForSeconds(cardRevealDelay);
        
        // 4. Flash effect
        if (flashImage != null && !skipRequested)
        {
            yield return StartCoroutine(FlashEffect());
        }
        
        // 5. Get the ACTUAL character's rarity color (not random!)
        Color rarityColor = GetRarityColor(character.rarity);
        
        // 6. Show card with CORRECT rarity color
        if (cardImage != null)
        {
            // Set the color to match the actual character rarity
            cardImage.color = rarityColor;
            
            if (!skipRequested)
            {
                yield return StartCoroutine(FadeIn(cardImage, fadeInDuration));
            }
            else
            {
                cardImage.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 1);
            }
        }
        
        // 7. Play correct rarity sound based on ACTUAL character
        if (audioSource != null && !skipRequested)
        {
            AudioClip revealSound = GetRaritySound(character.rarity);
            if (revealSound != null)
            {
                audioSource.PlayOneShot(revealSound);
            }
        }
        
        // 8. Show CORRECT rarity text for the actual character
        if (rarityText != null)
        {
            rarityText.text = GetRarityText(character.rarity);
            rarityText.color = rarityColor;
            rarityText.gameObject.SetActive(true);
            
            if (!skipRequested)
            {
                // Text animation effect
                yield return StartCoroutine(ScaleText(rarityText.transform));
            }
        }
        
        // 9. Stop particle effects
        if (starParticles != null)
        {
            starParticles.Stop();
        }
        
        // 10. Keep displaying for a while
        if (!skipRequested)
        {
            yield return new WaitForSeconds(cardDisplayDuration);
        }
    }
    
    // Flash effect
    private IEnumerator FlashEffect()
    {
        if (flashImage == null) yield break;
        
        // Fade in
        float elapsed = 0f;
        while (elapsed < flashDuration / 2)
        {
            if (skipRequested) break;
            
            elapsed += Time.deltaTime;
            float alpha = elapsed / (flashDuration / 2);
            flashImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        
        // Fade out
        elapsed = 0f;
        while (elapsed < flashDuration / 2)
        {
            if (skipRequested) break;
            
            elapsed += Time.deltaTime;
            float alpha = 1 - (elapsed / (flashDuration / 2));
            flashImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        
        flashImage.color = new Color(1, 1, 1, 0);
    }
    
    // Fade in effect
    private IEnumerator FadeIn(Image image, float duration)
    {
        Color startColor = image.color;
        startColor.a = 0;
        Color endColor = startColor;
        endColor.a = 1;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (skipRequested)
            {
                image.color = endColor;
                break;
            }
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            image.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        image.color = endColor;
    }
    
    // Text scale animation
    private IEnumerator ScaleText(Transform textTransform)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (skipRequested)
            {
                textTransform.localScale = endScale;
                break;
            }
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Use elastic curve
            float scale = Mathf.Lerp(0, 1, t);
            if (t < 0.5f)
            {
                scale = 2f * t * t;
            }
            else
            {
                scale = 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
            }
            
            textTransform.localScale = Vector3.one * scale;
            yield return null;
        }
        
        textTransform.localScale = endScale;
    }
    
    // Get rarity color
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
    
    // Get rarity text
    private string GetRarityText(int rarity)
    {
        switch (rarity)
        {
            case 5: return "★★★★★ SSR";
            case 4: return "★★★★ SR";
            case 3: return "★★★ R";
            default: return "★";
        }
    }
    
    // Get rarity sound
    private AudioClip GetRaritySound(int rarity)
    {
        switch (rarity)
        {
            case 5: return ssrRevealSound;
            case 4: return srRevealSound;
            case 3: return rRevealSound;
            default: return null;
        }
    }
    
    // Skip button clicked
    private void OnSkipClicked()
    {
        skipRequested = true;
    }
}