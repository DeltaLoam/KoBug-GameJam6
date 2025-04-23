using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    public PlayerController player; // อ้างอิงไปยัง PlayerController
    public Image healthFill; // รูปสี่เหลี่ยมที่จะลดตามพลังชีวิต
    public Text healthText; // ถ้าต้องการแสดงตัวเลขพลังชีวิต
    
    private int maxHealth;
    
    void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
        
        maxHealth = player.maxHealth;
        UpdateHealthBar();
    }
    
    void Update()
    {
        UpdateHealthBar();
    }
    
    void UpdateHealthBar()
    {
        float healthPercent = (float)player.health / maxHealth;
        healthFill.fillAmount = healthPercent;
        
        // ถ้ามี Text แสดงตัวเลข
        if (healthText != null)
        {
            healthText.text = player.health + " / " + maxHealth;
        }
    }
}