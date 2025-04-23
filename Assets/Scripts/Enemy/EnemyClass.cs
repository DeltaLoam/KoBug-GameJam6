using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    
    [Header("Stats")]
    [SerializeField] private int damage = 1;
    [SerializeField] private int scoreValue = 100;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField] private float dashDuration = 0.8f;
    [SerializeField] private float dashCooldown = 3f;
    
    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float dashRange = 4f;
    [SerializeField] private float attackCooldown = 1.5f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    
    // Private movement variables
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;
    private bool facingRight = true;
    private bool isGrounded;
    
    // Private state variables
    private enum EnemyState { Idle, Chase, Attack, Dash }
    private EnemyState currentState = EnemyState.Idle;
    private float attackTimer = 0f;
    private float dashTimer = 0f;
    private bool canAttack = true;
    private bool canDash = true;
    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Find player if not set
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        // Create ground check if needed
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = checkObj.transform;
        }
        
    }
    // Add this to your SimpleEnemy or Enemy script in Awake() or Start() method
private void FaceDirection(bool faceRight)
{
    // Only flip if the direction is actually changing
    if (facingRight != faceRight)
    {
        facingRight = faceRight;
        
        // Use this instead of flipping the sprite directly
        transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
        
        // If you're using spriteRenderer.flipX, replace it with this approach:
        // spriteRenderer.flipX = !facingRight;
    }
}
private void FixCollider()
{
    // Replace circle collider with box collider if needed
    CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
    if (circleCollider != null)
    {
        // Store the circle radius
        float radius = circleCollider.radius;
        
        // Remove the circle collider
        Destroy(circleCollider);
        
        // Add a box collider instead
        BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
        
        // Set box collider size (approximate the circle)
        boxCollider.size = new Vector2(radius * 2, radius * 2);
        
        // Adjust offset if needed
        boxCollider.offset = new Vector2(0, 0);
    }
    
    // Adjust rigidbody settings to prevent rolling
    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.freezeRotation = true;  // Prevent rotation
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smoother movement
    }
}
    private void Update()
    {
        // Ground check
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        
        // Reduce timers
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (dashTimer > 0) dashTimer -= Time.deltaTime;
        
        // Reset flags
        if (attackTimer <= 0) canAttack = true;
        if (dashTimer <= 0) canDash = true;
        
        // Dash handling
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero;
            }
            return; // Skip other logic while dashing
        }
        
        // Player detection and state management
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            // Determine state based on distance
            if (distanceToPlayer <= attackRange && canAttack)
            {
                currentState = EnemyState.Attack;
            }
            else if (distanceToPlayer <= dashRange && canDash && distanceToPlayer > attackRange)
            {
                currentState = EnemyState.Dash;
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = EnemyState.Chase;
            }
            else
            {
                currentState = EnemyState.Idle;
            }
            
            // Execute state behavior
            switch (currentState)
            {
                case EnemyState.Idle:
                    IdleBehavior();
                    break;
                case EnemyState.Chase:
                    ChaseBehavior();
                    break;
                case EnemyState.Attack:
                    AttackBehavior();
                    break;
                case EnemyState.Dash:
                    DashBehavior();
                    break;
            }
        }
    }
    
    private void FixedUpdate()
    {
        // Skip movement during dash
        if (isDashing) return;
        
        // Apply movement
        if (currentState == EnemyState.Chase && isGrounded)
        {
            rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
        }
    }
    
    private void IdleBehavior()
    {
        // Simple idle - just stop
        moveDirection = Vector2.zero;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    private void ChaseBehavior()
    {
        // Calculate direction toward player
        moveDirection = (playerTransform.position - transform.position).normalized;
        
        // Face the correct direction
        if (moveDirection.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveDirection.x < 0 && facingRight)
        {
            Flip();
        }
    }
    
    private void AttackBehavior()
    {
        // Stop moving when attacking
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        // Perform attack
        canAttack = false;
        attackTimer = attackCooldown;
        
        // Get player component and deal damage
        PlayerController player = playerTransform.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Debug.Log("Enemy attacked player for " + damage + " damage");
        }
    }
    
    private void DashBehavior()
    {
        // Perform dash
        isDashing = true;
        canDash = false;
        dashTimeLeft = dashDuration;
        dashTimer = dashCooldown;
        
        // Set dash direction (toward player)
        moveDirection = (playerTransform.position - transform.position).normalized;
        
        // Apply dash velocity
        rb.linearVelocity = moveDirection * dashSpeed;
        
        // Face the correct direction
        if (moveDirection.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveDirection.x < 0 && facingRight)
        {
            Flip();
        }
        
        Debug.Log("Enemy dashing toward player!");
    }
    
    private void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
    }
    
    public void OnEnemyDeath()
    {
        // Add score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }
        
        // Chance to drop item (50%)
        if (Random.value > 0.5f)
        {
            DropFragment();
        }
        
        // Disable components
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;
        
        // this will be called by EnemyHealth.Die()
    }
    
    private void DropFragment()
    {
        // Find fragment prefab (assumes it's in Resources folder)
        GameObject fragmentPrefab = Resources.Load<GameObject>("EdgeFragment");
        if (fragmentPrefab != null)
        {
            // Create fragment at enemy position
            GameObject fragment = Instantiate(fragmentPrefab, transform.position, Quaternion.identity);
            
            // Add some force for a nice effect
            Rigidbody2D fragmentRb = fragment.GetComponent<Rigidbody2D>();
            if (fragmentRb != null)
            {
                float randomX = Random.Range(-2f, 2f);
                fragmentRb.AddForce(new Vector2(randomX, 5f), ForceMode2D.Impulse);
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Damage player on contact
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            
            // Stop dashing if collided with player
            if (isDashing)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw detection ranges
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, dashRange);
        
        // Draw ground check
        if (groundCheck != null)
        {
            Gizmos.color = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}