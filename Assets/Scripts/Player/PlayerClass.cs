using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float semicircleMoveSpeed = 7f; // ร่างหลัก (ครึ่งวงกลม)
    public float triangleMoveSpeed = 5f;
    public float rectangleMoveSpeed = 3f;
    public float jumpForce = 10f;
    private Rigidbody2D rb;
    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;
    
    public enum PlayerShape { Semicircle, Triangle, Rectangle }
    public PlayerShape currentShape = PlayerShape.Semicircle;
    public int collectedFragments = 0;
    
    // In the PlayerController class, look for these variables
[Header("Transform Energy")]
public float maxTransformEnergy = 100f;
public float currentTransformEnergy = 100f;
public float transformEnergyDrainRate = 5f; // Base drain rate for non-base forms
public float transformEnergyRegenRate = 20f; // Regen rate when in base form

// Add these new variables for form-specific drain rates
private float triangleDrainMultiplier = 1f; // Triangle drains at normal rate
private float rectangleDrainMultiplier = 2f; // Rectangle drains at 2x rate
private float rectangleReflectDrainMultiplier = 4f; // Rectangle reflects at 3x rate
    public Image transformEnergyBar; // อ้างอิงไปยัง UI Image สำหรับแสดงพลังงาน

    [Header("Combat")]
    public int health = 5;
    public int maxHealth = 5;
    public float dashCooldown = 1f;
    private float dashTimer = 0f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    private float dashTimeLeft;
    private bool isDashing = false;
    public int dashDamage = 2;
    public bool isReflecting = false; // สำหรับการสะท้อนความเสียหายของสี่เหลี่ยม
    
    [Header("Visual")]
    public SpriteRenderer playerRenderer;
    public Sprite semicircleSprite;
    public Sprite triangleSprite;
    public Sprite rectangleSprite;
    public Sprite rectangleReflectSprite;
    public GameObject dashEffect;
    public Image healthBar;
    
    // Visual Effects Component
    private PlayerVisualEffects visualEffects;
    private GameManager gameManager;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // ถ้าไม่ได้กำหนด SpriteRenderer ใน Inspector
        if (playerRenderer == null)
        {
            playerRenderer = GetComponent<SpriteRenderer>();
        }
        
        visualEffects = GetComponent<PlayerVisualEffects>();
        gameManager = FindObjectOfType<GameManager>();
        
        UpdatePlayerForm();
        UpdateTransformEnergyBar();
        UpdateHealthBar();
    }
    
    void Update()
    {
        // ตรวจสอบการยืนบนพื้น
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        
        // ไม่ต้องทำการควบคุมถ้ากำลัง dash
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                if (dashEffect != null)
                {
                    dashEffect.SetActive(false);
                }
            }
            return;
        }
        
        // การเคลื่อนที่แนวนอน
        float horizontalInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * GetCurrentSpeed(), rb.linearVelocity.y);
        
        // ปรับการหันหน้าโดยใช้ SpriteRenderer.flipX
        if (horizontalInput > 0)
        {
            playerRenderer.flipX = false;
        }
        else if (horizontalInput < 0)
        {
            playerRenderer.flipX = true;
        }
        
        // การกระโดด - เฉพาะสามเหลี่ยมเท่านั้นที่กระโดดได้
        if (currentShape == PlayerShape.Triangle && Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
        
        // การเปลี่ยนรูปร่าง
        if (Input.GetKeyDown(KeyCode.Alpha1) && currentShape != PlayerShape.Semicircle)
        {
            ChangeShape(PlayerShape.Semicircle);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && currentShape != PlayerShape.Triangle && collectedFragments >= 1)
        {
            if (currentTransformEnergy > 10f) // ต้องมีพลังงานอย่างน้อย 10% จึงจะแปลงร่างได้
            {
                ChangeShape(PlayerShape.Triangle);
            }
            else
            {
                Debug.Log("Not enough transform energy!");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && currentShape != PlayerShape.Rectangle && collectedFragments >= 5)
        {
            if (currentTransformEnergy > 10f) // ต้องมีพลังงานอย่างน้อย 10% จึงจะแปลงร่างได้
            {
                ChangeShape(PlayerShape.Rectangle);
            }
            else
            {
                Debug.Log("Not enough transform energy!");
            }
        }
        
        // ความสามารถพิเศษของแต่ละรูปร่าง
        
        // สามเหลี่ยม - Dash
        if (currentShape == PlayerShape.Triangle && Input.GetKeyDown(KeyCode.E) && dashTimer <= 0 && isGrounded)
        {
            Dash();
        }
        
        // สี่เหลี่ยม - Reflect
        if (currentShape == PlayerShape.Rectangle && Input.GetKeyDown(KeyCode.E))
        {
            ToggleReflect();
        }
        
        // ลดเวลาคูลดาวน์ Dash
        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
        }
        
        // จัดการพลังงานการแปลงร่าง
        ManageTransformEnergy();
    }
    
    void ManageTransformEnergy()
{
    // Regenerate energy when in base form
    if (currentShape == PlayerShape.Semicircle)
    {
        currentTransformEnergy += transformEnergyRegenRate * Time.deltaTime;
        if (currentTransformEnergy > maxTransformEnergy)
        {
            currentTransformEnergy = maxTransformEnergy;
        }
    }
    // Drain energy when in other forms based on multipliers
    else
    {
        float drainRate = transformEnergyDrainRate;
        
        // Apply form-specific drain multipliers
        if (currentShape == PlayerShape.Triangle)
        {
            drainRate *= triangleDrainMultiplier;
        }
        else if (currentShape == PlayerShape.Rectangle)
        {
            // Apply higher drain if reflecting
            if (isReflecting)
            {
                drainRate *= rectangleReflectDrainMultiplier; // 3x drain while reflecting
            }
            else
            {
                drainRate *= rectangleDrainMultiplier; // 2x drain normally
            }
        }
        
        // Apply the calculated drain
        currentTransformEnergy -= drainRate * Time.deltaTime;
        
        // Check if energy is depleted
        if (currentTransformEnergy <= 0)
        {
            currentTransformEnergy = 0;
            ChangeShape(PlayerShape.Semicircle); // Revert to base form when energy is depleted
            Debug.Log("Transform energy depleted! Reverting to base form.");
        }
    }
    
    // Update transform energy bar in UI
    UpdateTransformEnergyBar();
}
    
    void UpdateTransformEnergyBar()
    {
        if (transformEnergyBar != null)
        {
            transformEnergyBar.fillAmount = currentTransformEnergy / maxTransformEnergy;
        }
    }
    
    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)health / maxHealth;
        }
    }
    
    float GetCurrentSpeed()
    {
        switch (currentShape)
        {
            case PlayerShape.Triangle:
                return triangleMoveSpeed;
            case PlayerShape.Rectangle:
                return isReflecting ? 0f : rectangleMoveSpeed; // ไม่เคลื่อนที่ขณะสะท้อน
            default:
                return semicircleMoveSpeed;
        }
    }
    
    public void ChangeShape(PlayerShape newShape)
    {
        // ยกเลิกการสะท้อนเมื่อเปลี่ยนจากสี่เหลี่ยม
        if (currentShape == PlayerShape.Rectangle && isReflecting)
        {
            isReflecting = false;
        }
        
        currentShape = newShape;
        UpdatePlayerForm();
        
        // เล่นเอฟเฟกต์การแปลงร่าง
        if (visualEffects != null)
        {
            visualEffects.PlayTransformEffect(newShape);
        }
    }
    
    private void UpdatePlayerForm()
{
    // Update sprite based on current shape
    switch (currentShape)
    {
        case PlayerShape.Semicircle:
            playerRenderer.sprite = semicircleSprite;
            break;
        case PlayerShape.Triangle:
            playerRenderer.sprite = triangleSprite;
            break;
        case PlayerShape.Rectangle:
            // Use different sprite when reflecting
            playerRenderer.sprite = isReflecting ? rectangleReflectSprite : rectangleSprite;
            break;
    }
}
    
    void Dash()
    {
        // กำหนดทิศทางการพุ่งตามทิศที่ Player หันหน้าอยู่ (ใช้ flipX)
        float dashDirection = playerRenderer.flipX ? -1f : 1f;
        
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashTimer = dashCooldown;
        
        // หยุดความเร็วปัจจุบัน
        rb.linearVelocity = Vector2.zero;
        // กำหนดความเร็วการพุ่ง
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
        
        Debug.Log("Player dashing with direction: " + dashDirection + ", speed: " + dashSpeed);
        
        // แสดงเอฟเฟกต์การพุ่ง
        if (dashEffect != null)
        {
            dashEffect.SetActive(true);
        }
        
        // เพิ่มเอฟเฟกต์ Dash
        if (visualEffects != null)
        {
            visualEffects.StartDashEffect();
        }
        
        // ตรวจสอบการชนกับศัตรูเมื่อพุ่ง (ปรับให้ใช้ flipX)
        // Inside PlayerClass.cs, in the Dash() method

// Check for enemies hit by dash
Vector2 dashCheckPosition = transform.position + new Vector3(dashDirection * 1.5f, 0f, 0f);
Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(dashCheckPosition, new Vector2(3f, 1f), 0f);

foreach (Collider2D enemyCollider in hitEnemies)
{
    if (enemyCollider.CompareTag("Enemy"))
    {
        Debug.Log("Hit enemy during dash: " + enemyCollider.name);
        
        // Get Enemy component and deal damage
        Enemy enemy = enemyCollider.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(dashDamage);
        }
    }
}
    }
    
    void ToggleReflect()
{
    // Toggle the reflection state
    isReflecting = !isReflecting;
    
    // Update visual appearance
    UpdatePlayerForm();
    
    // Play effect
    if (visualEffects != null)
    {
        visualEffects.ToggleReflectEffect(isReflecting);
    }
    
    // Adjust movement during reflection
    if (isReflecting)
    {
        rb.linearVelocity = Vector2.zero;
        
        // Debug message to confirm activation
        Debug.Log("Reflection mode activated");
    }
    else
    {
        Debug.Log("Reflection mode deactivated");
    }
}
    
    public void TakeDamage(int damageAmount)
{
    // Debug to track when damage is received
    Debug.Log("Player received damage: " + damageAmount + ", isReflecting: " + isReflecting);
    
    // Check for reflection (Rectangle form's special ability)
    if (currentShape == PlayerShape.Rectangle && isReflecting)
    {
        // Look for nearby enemies to reflect damage back to
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, 3f);
        bool reflectedToAny = false;
        
        foreach (Collider2D enemy in nearbyEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // Reflect double damage back to enemy
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    enemyComponent.TakeDamage(damageAmount * 2);
                    reflectedToAny = true;
                    Debug.Log("Reflected damage to " + enemy.name);
                }
            }
        }
        
        // Play yellow blink effect when reflecting damage
        if (visualEffects != null && reflectedToAny)
        {
            visualEffects.PlayReflectBlinkEffect();
        }
        
        return; // Don't take damage when reflecting
    }
    
    // For Rectangle form without reflection, reduce damage
    if (currentShape == PlayerShape.Rectangle && !isReflecting)
    {
        // Rectangle takes half damage (rounded up)
        int reducedDamage = Mathf.CeilToInt(damageAmount / 2f);
        Debug.Log("Rectangle form reduced damage from " + damageAmount + " to " + reducedDamage);
        damageAmount = reducedDamage;
    }
    
    // Apply damage
    health -= damageAmount;
    
    // Make sure health doesn't go below 0
    if (health < 0)
    {
        health = 0;
    }
    
    // Play hurt effect
    if (visualEffects != null)
    {
        visualEffects.PlayHurtEffect();
    }
    
    // Update health bar
    UpdateHealthBar();
    
    // Check for death
    if (health <= 0)
    {
        Die();
    }
}
    
    void Die()
    {
        Debug.Log("Player died!");
        
        // ปิดการควบคุม
        this.enabled = false;
        
        // หยุดการเคลื่อนที่
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        // แจ้ง GameManager ว่าเกมจบแล้ว
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }
    
    public void CollectFragment()
    {
        collectedFragments++;
        Debug.Log("Collected Edge Fragment: " + collectedFragments);
        
        // แจ้ง GameManager
        if (gameManager != null)
        {
            gameManager.CollectFragment();
        }
        
        // แจ้งเตือนการปลดล็อกรูปร่าง
        if (collectedFragments == 1)
        {
            Debug.Log("Triangle form unlocked! Press 2 to transform.");
        }
        
        if (collectedFragments == 5)
        {
            Debug.Log("Rectangle form unlocked! Press 3 to transform.");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // ตรวจจับการเก็บ Fragment
        if (other.CompareTag("fragment"))
        {
            CollectFragment();
            Destroy(other.gameObject);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // วาด Gizmo สำหรับแสดงพื้นที่การพุ่งโจมตีของสามเหลี่ยม
        Gizmos.color = Color.red;
        Vector3 dashDirection = playerRenderer.flipX ? Vector3.left : Vector3.right;
        Vector3 dashCenter = transform.position + dashDirection * 1.5f;
        Gizmos.DrawWireCube(dashCenter, new Vector3(3f, 1f, 0f));
        
        // วาด Gizmo แสดงตำแหน่งตรวจสอบพื้น
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }
    }
    
}