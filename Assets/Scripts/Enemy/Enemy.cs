using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private EnemyHealth healthSystem;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float blinkDuration = 0.1f;
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private Sprite deathSprite; // New death sprite
    [SerializeField] private float deathDuration = 1.0f; // How long to show death sprite
    
    private void Awake()
    {
        healthSystem = GetComponent<EnemyHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (healthSystem == null)
        {
            healthSystem = gameObject.AddComponent<EnemyHealth>();
        }
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage);
            StartCoroutine(BlinkEffect());
        }
    }
    
    private IEnumerator BlinkEffect()
    {
        if (spriteRenderer == null) yield break;
        
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }
    
    // Handle enemy death
    public void OnDeath()
    {
        // Change sprite if we have a death sprite
        if (spriteRenderer != null && deathSprite != null)
        {
            spriteRenderer.sprite = deathSprite;
            // Reset color (in case it was in the middle of a blink)
            spriteRenderer.color = originalColor;
        }
        
        // Disable all colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        // Disable rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }
        
        // Disable scripts except this one
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }
        
        // Destroy after delay
        StartCoroutine(DestroyAfterDelay());
    }
    
    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDuration);
        
        // Fade out
        float fadeTime = 0.5f;
        float startTime = Time.time;
        
        while (Time.time < startTime + fadeTime)
        {
            float alpha = 1 - ((Time.time - startTime) / fadeTime);
            if (spriteRenderer != null)
            {
                Color newColor = spriteRenderer.color;
                newColor.a = alpha;
                spriteRenderer.color = newColor;
            }
            yield return null;
        }
        
        Destroy(gameObject);
    }
}