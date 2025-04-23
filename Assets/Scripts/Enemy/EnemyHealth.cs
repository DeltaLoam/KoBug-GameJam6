using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Canvas healthBarCanvas;
    
    // Expose current health for other scripts to read
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        // Find health bar components if not assigned
        if (healthBarFill == null)
        {
            Transform healthBar = transform.Find("HealthBarAnchor/Canvas/BG Health Enemy/Filled Health Enemy");
            if (healthBar != null)
            {
                healthBarFill = healthBar.GetComponent<Image>();
            }
        }
        
        if (healthBarCanvas == null)
        {
            Transform canvas = transform.Find("HealthBarAnchor/Canvas");
            if (canvas != null)
            {
                healthBarCanvas = canvas.GetComponent<Canvas>();
            }
        }
        
        // Update health bar initially
        UpdateHealthBar();
        
        // Hide health bar initially if at full health
        if (healthBarCanvas != null && currentHealth >= maxHealth)
        {
            healthBarCanvas.enabled = false;
        }
    }
    
    void Update()
    {
        // Keep health bar facing camera and positioned correctly
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.rotation = Quaternion.identity;
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        
        // Clamp health to valid range
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Show health bar when damaged
        if (healthBarCanvas != null)
        {
            healthBarCanvas.enabled = true;
        }
        
        // Update the health bar fill
        UpdateHealthBar();
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            // Update fill amount based on current health percentage
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }
    
    private void Die()
    {
        // Notify SimpleEnemy script of death
        Enemy enemyScript = GetComponent<Enemy>();
    if (enemyScript != null)
    {
        enemyScript.OnDeath();
    }
    
        
        // Handle death - can be customized based on your game
        Destroy(gameObject, 0.5f);
    }
}