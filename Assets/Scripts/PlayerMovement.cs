using UnityEngine;

public enum ShapeForm
{
    Default,
    Triangle
}

public class PlayerMovement : MonoBehaviour
{
    public ShapeForm currentForm = ShapeForm.Default;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashTime = 0.2f;
    public float dashCooldown = 2f;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float lastDashTime = -Mathf.Infinity;

    [Header("Transformation Settings")]
    public float transformDuration = 10f;
    private float transformTimer = 0f;

    [Header("References")]
    public Sprite defaultSprite;
    public Sprite triangleSprite;
    public GameObject transformEffect;
    public GameObject dashEffect;
    public UnityEngine.UI.Image transformBarFill;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandleInput();

        if (isDashing)
            HandleDashMovement();
        else
            HandleMovement();

        HandleTransformTimer();
        UpdateTransformBar();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2) && currentForm == ShapeForm.Default && FragmentManager.instance.fragmentCount >= 5 && transformTimer > 0f)
        {
            TransformToTriangle();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && currentForm == ShapeForm.Triangle)
        {
            TransformToDefault();
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        if (moveX > 0) spriteRenderer.flipX = false;
        else if (moveX < 0) spriteRenderer.flipX = true;

        if (Input.GetButtonDown("Jump") && currentForm == ShapeForm.Triangle && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            Debug.Log("Jump!");
        }

        if (Input.GetKeyDown(KeyCode.E) && currentForm == ShapeForm.Triangle && Time.time - lastDashTime >= dashCooldown)
        {
            StartDash();
        }
    }

    void HandleDashMovement()
    {
        dashTimer -= Time.deltaTime;

        float direction = spriteRenderer.flipX ? -1 : 1;
        rb.linearVelocity = new Vector2(direction * dashForce, 0f);

        if (dashTimer <= 0f)
        {
            isDashing = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashTime;
        lastDashTime = Time.time;

        if (dashEffect != null)
            Instantiate(dashEffect, transform.position, Quaternion.identity);

        Debug.Log(">> Speed Dash Started!");
    }

    void HandleTransformTimer()
    {
        if (currentForm == ShapeForm.Triangle)
        {
            transformTimer -= Time.deltaTime;
            if (transformTimer <= 0f)
            {
                TransformToDefault();
            }
        }
        else
        {
            transformTimer += Time.deltaTime;
            if (transformTimer > transformDuration)
                transformTimer = transformDuration;
        }
    }

    void TransformToTriangle()
    {
        currentForm = ShapeForm.Triangle;
        spriteRenderer.sprite = triangleSprite;

        if (transformEffect != null)
            Instantiate(transformEffect, transform.position, Quaternion.identity);

        Debug.Log(">> Transformed to Triangle!");
    }

    void TransformToDefault()
    {
        currentForm = ShapeForm.Default;
        spriteRenderer.sprite = defaultSprite;

        if (transformEffect != null)
            Instantiate(transformEffect, transform.position, Quaternion.identity);

        Debug.Log(">> Returned to Default form!");
    }

    void UpdateTransformBar()
    {
        if (transformBarFill != null)
            transformBarFill.fillAmount = transformTimer / transformDuration;
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
}
