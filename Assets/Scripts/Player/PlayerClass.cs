using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float semicircleMoveSpeed = 7f; 
    public float triangleMoveSpeed = 5f;
    public float jumpCooldown = 0.5f;
    private float jumpTimer = 0f;
    public float rectangleMoveSpeed = 3f;
    public float jumpForce = 10f;
    private Rigidbody2D rb;
    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;
    
    public enum PlayerShape { Semicircle, Triangle, Rectangle }
    public PlayerShape currentShape = PlayerShape.Semicircle;
    public int collectedFragments = 0;
    
    [Header("Double Dash")]
    public int maxDashCharges = 1;  // Will become 2 when player has 3+ fragments
    public int currentDashCharges = 1;
    private float dashRechargeTimer = 0f;
    public float dashRechargeTime = 1.0f;  // Time to recharge a dash
    
    [Header("Transform Energy")]
    public float maxTransformEnergy = 100f;
    public float energyIncreasePerFragment = 5f;
    public float currentTransformEnergy = 100f;
    public float transformEnergyDrainRate = 5f;
    public float transformEnergyRegenRate = 20f;
    
    private float triangleDrainMultiplier = 1f;
    private float rectangleDrainMultiplier = 2f;
    private float rectangleReflectDrainMultiplier = 4f;
    public Image transformEnergyBar;

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
    public bool isReflecting = false;
    
    [Header("Visual")]
    public SpriteRenderer playerRenderer;
    public Sprite semicircleSprite;
    public Sprite triangleSprite;
    public Sprite rectangleSprite;
    public Sprite rectangleReflectSprite;
    public GameObject dashEffect;
    public Image healthBar;
    
    [Header("UI")]
    public Text fragmentCountText;  // Drag your UI Text here in Inspector
    
    private PlayerVisualEffects visualEffects;
    private GameManager gameManager;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (playerRenderer == null)
        {
            playerRenderer = GetComponent<SpriteRenderer>();
        }
        
        visualEffects = GetComponent<PlayerVisualEffects>();
        gameManager = FindObjectOfType<GameManager>();
        
        // Initialize dash charges
        currentDashCharges = maxDashCharges;
        
        UpdatePlayerForm();
        UpdateTransformEnergyBar();
        UpdateHealthBar();
        UpdateFragmentUI();
    }
    
    void Update()
    {
        // Check if standing on ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        
        // Don't control if dashing
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
        
        // Update dash charge system
        if (collectedFragments >= 3)
        {
            maxDashCharges = 2;  // Allow double dash with 3+ fragments
        }
        else
        {
            maxDashCharges = 1;  // Otherwise just 1 dash
        }
        
        // Recharge dash over time
        if (currentDashCharges < maxDashCharges)
        {
            dashRechargeTimer += Time.deltaTime;
            if (dashRechargeTimer >= dashRechargeTime)
            {
                currentDashCharges++;
                dashRechargeTimer = 0f;
                Debug.Log("Dash recharged: " + currentDashCharges + "/" + maxDashCharges);
            }
        }
        
        // Horizontal movement
        float horizontalInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * GetCurrentSpeed(), rb.linearVelocity.y);
        
        // Adjust facing using SpriteRenderer.flipX
        if (horizontalInput > 0)
        {
            playerRenderer.flipX = false;
        }
        else if (horizontalInput < 0)
        {
            playerRenderer.flipX = true;
        }
        
        // Jump - only triangle can jump
        if (currentShape == PlayerShape.Triangle && Input.GetButtonDown("Jump") && jumpTimer <= 0)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            jumpTimer = jumpCooldown;
        }
        
        // Reduce jump timer
        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
        }
        
        // Shape changing
        if (Input.GetKeyDown(KeyCode.Alpha1) && currentShape != PlayerShape.Semicircle)
        {
            ChangeShape(PlayerShape.Semicircle);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && currentShape != PlayerShape.Triangle && collectedFragments >= 1)
        {
            if (currentTransformEnergy > 10f)
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
            if (currentTransformEnergy > 10f)
            {
                ChangeShape(PlayerShape.Rectangle);
            }
            else
            {
                Debug.Log("Not enough transform energy!");
            }
        }
        
        // Triangle - Dash with charges
        if (currentShape == PlayerShape.Triangle && Input.GetKeyDown(KeyCode.E) && 
            dashTimer <= 0 && currentDashCharges > 0)
        {
            Dash();
            currentDashCharges--;
            Debug.Log("Dashes remaining: " + currentDashCharges);
        }
        
        // Rectangle - Reflect
        if (currentShape == PlayerShape.Rectangle && Input.GetKeyDown(KeyCode.E))
        {
            ToggleReflect();
        }
        
        // Reduce dash cooldown timer
        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
        }
        
        // Manage transform energy
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
                    drainRate *= rectangleReflectDrainMultiplier;
                }
                else
                {
                    drainRate *= rectangleDrainMultiplier;
                }
            }
            
            // Apply the calculated drain
            currentTransformEnergy -= drainRate * Time.deltaTime;
            
            // Check if energy is depleted
            if (currentTransformEnergy <= 0)
            {
                currentTransformEnergy = 0;
                ChangeShape(PlayerShape.Semicircle);
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
            Debug.Log($"Health bar updated: {health}/{maxHealth} = {healthBar.fillAmount}");
        }
    }
    
    void UpdateFragmentUI()
    {
        if (fragmentCountText != null)
        {
            fragmentCountText.text = collectedFragments.ToString();
        }
    }
    
    float GetCurrentSpeed()
    {
        switch (currentShape)
        {
            case PlayerShape.Triangle:
                return triangleMoveSpeed;
            case PlayerShape.Rectangle:
                return isReflecting ? 0f : rectangleMoveSpeed;
            default:
                return semicircleMoveSpeed;
        }
    }
    
    public void ChangeShape(PlayerShape newShape)
    {
        // Cancel reflection when changing from rectangle
        if (currentShape == PlayerShape.Rectangle && isReflecting)
        {
            isReflecting = false;
        }
        
        currentShape = newShape;
        UpdatePlayerForm();
        
        // Play transform effect
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
                playerRenderer.sprite = isReflecting ? rectangleReflectSprite : rectangleSprite;
                break;
        }
    }
    
    void Dash()
    {
        float dashDirection = playerRenderer.flipX ? -1f : 1f;
        
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashTimer = dashCooldown;
        
        // Stop current velocity
        rb.linearVelocity = Vector2.zero;
        // Apply dash velocity
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
        
        Debug.Log("Player dashing with direction: " + dashDirection + ", speed: " + dashSpeed);
        
        // Show dash effect
        if (dashEffect != null)
        {
            dashEffect.SetActive(true);
        }
        
        // Add dash effect
        if (visualEffects != null)
        {
            visualEffects.StartDashEffect();
        }
        
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
        
        // Disable control
        this.enabled = false;
        
        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        // Notify GameManager
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    public void CollectFragment()
    {
        collectedFragments++;
        Debug.Log("Collected Edge Fragment: " + collectedFragments);
        
        // Increase max transform energy
        maxTransformEnergy += energyIncreasePerFragment;
        
        // Optionally also increase current energy by the same amount
        currentTransformEnergy += energyIncreasePerFragment;
        
        // Update the UIs
        UpdateTransformEnergyBar();
        UpdateFragmentUI();
        
        // Notify GameManager
        if (gameManager != null)
        {
            gameManager.CollectFragment();
        }
        
        // Form unlock notifications
        if (collectedFragments == 1)
        {
            Debug.Log("Triangle form unlocked! Press 2 to transform.");
        }
        
        if (collectedFragments == 3)
        {
            Debug.Log("Double dash unlocked!");
        }
        
        if (collectedFragments == 5)
        {
            Debug.Log("Rectangle form unlocked! Press 3 to transform.");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check for fragment collection
        if (other.CompareTag("fragment"))
        {
            CollectFragment();
            Destroy(other.gameObject);
        }
    }
    
    public void IncreaseMaxHealth(int amount)
    {
        // Store old health percentage
        float healthPercentage = (float)health / maxHealth;
        
        // Increase max health
        maxHealth += amount;
        
        // Scale current health to maintain the same percentage
        health = Mathf.RoundToInt(maxHealth * healthPercentage);
        
        // Clamp health to max
        health = Mathf.Clamp(health, 0, maxHealth);
        
        // Update the health bar
        UpdateHealthBar();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw Gizmo for triangle dash attack area
        Gizmos.color = Color.red;
        Vector3 dashDirection = playerRenderer.flipX ? Vector3.left : Vector3.right;
        Vector3 dashCenter = transform.position + dashDirection * 1.5f;
        Gizmos.DrawWireCube(dashCenter, new Vector3(3f, 1f, 0f));
        
        // Draw Gizmo for ground check
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }
    }
}