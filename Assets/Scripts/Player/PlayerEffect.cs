using UnityEngine;
using System.Collections;

public class PlayerVisualEffects : MonoBehaviour
{
    [Header("Transform Effects")]
    public ParticleSystem transformParticles;
    public GameObject transformFlash;
    public float flashDuration = 0.2f;
    
    [Header("Hurt Effects")]
    public ParticleSystem hurtParticles;
    public SpriteRenderer playerSprite;
    public Color hurtColor = Color.red;
    public float blinkDuration = 0.1f;
    public int blinkCount = 3;
    
    [Header("Shape-Based Effects")]
    public ParticleSystem triangleDashEffect;
    public ParticleSystem rectangleReflectEffect;
    public TrailRenderer moveTrail;
    
    private Color originalColor;
    private PlayerController playerController;
    
    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (playerSprite == null)
        {
            playerSprite = GetComponent<SpriteRenderer>();
        }
        
        if (playerSprite != null)
        {
            originalColor = playerSprite.color;
        }
    }
    
    public void PlayTransformEffect(PlayerController.PlayerShape newShape)
    {
        // เล่นอนิเมชันการแปลงร่าง
        if (transformParticles != null)
        {
            // ปรับสีอนุภาคตามร่างใหม่
            var main = transformParticles.main;
            switch (newShape)
            {
                case PlayerController.PlayerShape.Semicircle:
                    main.startColor = Color.cyan;
                    break;
                case PlayerController.PlayerShape.Triangle:
                    main.startColor = Color.yellow;
                    break;
                case PlayerController.PlayerShape.Rectangle:
                    main.startColor = Color.green;
                    break;
            }
            
            transformParticles.Play();
        }
        
        // แสดงแฟลชขณะแปลงร่าง
        if (transformFlash != null)
        {
            StartCoroutine(FlashEffect());
        }
    }
    
    private IEnumerator FlashEffect()
    {
        transformFlash.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        transformFlash.SetActive(false);
    }
    
    public void PlayHurtEffect()
    {
        if (hurtParticles != null)
        {
            hurtParticles.Play();
        }
        
        if (playerSprite != null)
        {
            StartCoroutine(BlinkEffect());
        }
    }
    
    private IEnumerator BlinkEffect()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            playerSprite.color = hurtColor;
            yield return new WaitForSeconds(blinkDuration);
            playerSprite.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }
    
    public void StartDashEffect()
    {
        if (triangleDashEffect != null && playerController.currentShape == PlayerController.PlayerShape.Triangle)
        {
            triangleDashEffect.Play();
        }
    }
    
    public void ToggleReflectEffect(bool active)
    {
        if (rectangleReflectEffect != null)
        {
            if (active)
            {
                rectangleReflectEffect.Play();
            }
            else
            {
                rectangleReflectEffect.Stop();
            }
        }
    }
    
    private void Update()
    {
        // อัปเดตเอฟเฟกต์ Trail ตามความเร็วและรูปร่าง
        UpdateTrailEffect();
    }
    public void PlayReflectBlinkEffect()
{
    // Use a yellow color for reflection effect
    Color reflectColor = Color.yellow;
    
    // Create a coroutine for the blink effect
    StartCoroutine(BlinkEffectWithColor(reflectColor));
}

// Custom blinking coroutine that takes a color parameter
private IEnumerator BlinkEffectWithColor(Color blinkColor)
{
    if (playerSprite == null) yield break;
    
    // Store original color
    Color originalColor = playerSprite.color;
    
    for (int i = 0; i < blinkCount; i++)
    {
        playerSprite.color = blinkColor;
        yield return new WaitForSeconds(blinkDuration);
        playerSprite.color = originalColor;
        yield return new WaitForSeconds(blinkDuration);
    }
}
    
    public void UpdateTrailEffect()
    {
        if (moveTrail != null)
        {
            // ปรับความเข้มของเส้นตามความเร็ว
            float speed = Mathf.Abs(GetComponent<Rigidbody2D>().linearVelocity.x);
            Color trailColor = moveTrail.startColor;
            
            // ปรับสีตามร่างปัจจุบัน
            switch (playerController.currentShape)
            {
                case PlayerController.PlayerShape.Semicircle:
                    trailColor = new Color(0f, 0.8f, 1f);
                    break;
                case PlayerController.PlayerShape.Triangle:
                    trailColor = new Color(1f, 0.8f, 0f);
                    break;
                case PlayerController.PlayerShape.Rectangle:
                    trailColor = new Color(0f, 1f, 0.5f);
                    break;
            }
            
            // ปรับความโปร่งใสตามความเร็ว
            trailColor.a = Mathf.Clamp01(speed / 10f);
            
            moveTrail.startColor = trailColor;
            moveTrail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0);
        }
    }
}