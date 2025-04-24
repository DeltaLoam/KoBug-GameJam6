using UnityEngine;

public class EdgeFragment : MonoBehaviour
{
    public Color defaultColor = Color.yellow;
    public Color blinkColor = Color.white;
    public float blinkSpeed = 2f;
    
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        // ตั้ง Tag ให้ถูกต้อง
        gameObject.tag = "Fragment"; // ใช้ตาม tag ที่คุณตั้ง
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor;
        }
    }
    
    void Update()
    {
        // แค่กระพริบเปลี่ยนสีไม่มีการเคลื่อนไหวอื่นๆ
        if (spriteRenderer != null)
        {
            // สลับระหว่างสองสี
            float t = (Mathf.Sin(Time.time * blinkSpeed) + 1) / 2;
            spriteRenderer.color = Color.Lerp(defaultColor, blinkColor, t);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected, collecting fragment");
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.CollectFragment();
            }
            Destroy(gameObject);
        }
    }
}