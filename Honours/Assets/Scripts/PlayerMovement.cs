using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;
    TrailRenderer trailRenderer;
    public Transform weapon;
    public Transform bulletSpawn;

    [Header("Speed Settings")]
    [SerializeField] float defaultSpeed = 3f;
    [SerializeField] float currentSpeed;
    [SerializeField] float increasedSpeed = 6f;

    [Header("Dash Settings")]
    [SerializeField] float dashDistance = 3f;
    [SerializeField] float dashDuration = 0.8f;
    [SerializeField] float dashCooldown = 2f;

    public Vector2 moveDirection;
    public Vector2 lastMoveDirection = Vector2.zero;
    bool isDashing = false;
    bool canDash = true;
    bool isFacingRight = false;
    float rotationSpeed = 1f;

    public float weaponDistance = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpeed = defaultSpeed;
        animator = GetComponent<Animator>();
        trailRenderer = GetComponentInChildren<TrailRenderer>();
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.velocity = moveDirection * currentSpeed;
        }

        UpdateAnimation();
    }

    void OnMove(InputValue value)
    {
        // Get movement input
        moveDirection = value.Get<Vector2>();
    }

    void UpdateAnimation()
    {
        // Calculate speed based on the move direction magnitude
        float speed = moveDirection.magnitude;
        Vector2 normalizedDirection = (speed > 0) ? moveDirection.normalized : Vector2.zero;

        // Update blend tree parameters for horizontal movement (animMoveX)
        animator.SetFloat("animMoveX", normalizedDirection.x);

        // Save the player's last movement direction (for idle state)
        lastMoveDirection = normalizedDirection;


        // Update the speed parameter for animation states (run/idle)
        animator.SetFloat("speed", speed);

        // Neutral vertical aim (no up/down aiming)
        if (speed == 0)
        {
            // If idle, keep the last vertical direction (or set to zero if idle)
            animator.SetFloat("animMoveY", lastMoveDirection.y);
        }
        else
        {
            // If moving horizontally, reset vertical aim to zero
            animator.SetFloat("animMoveY", 0f);
        }
    }


    void OnDash()
    {
        if (canDash)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        Vector2 dashDirection = (moveDirection != Vector2.zero) ? moveDirection.normalized : lastMoveDirection;
        if (dashDirection != Vector2.zero)
        {
            lastMoveDirection = dashDirection;
        }

        // Store the current facing direction before the dash
        bool wasFacingRight = isFacingRight;

        // Make player invincible
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);

        // Sprite stretch effect
        spriteRenderer.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
        // Make player semi-transparent
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        // Enable dash trail
        trailRenderer.emitting = true;

        float elapsedTime = 0f;
        Vector2 startPosition = rb.position;
        Vector2 endPosition = startPosition + (dashDirection * dashDistance);

        // Move smoothly over dashDuration
        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(startPosition, endPosition, elapsedTime / dashDuration));
            yield return null;
        }

        // Reset scale
        spriteRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
        // Restore full opacity
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        // Disable dash effect
        trailRenderer.emitting = false;

        // Re-enable collision after dash ends
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);

        // Ensure the player keeps their facing direction after the dash
        isFacingRight = wasFacingRight;
        // Face right
        if (isFacingRight)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        isDashing = false;
        // Enable dash when cooldown has passed
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void ChangeSpeed(bool isEnhanced)
    {
        // Change colour when enhanced
        if (isEnhanced)
        {
            currentSpeed = increasedSpeed;
            spriteRenderer.color = Color.green;
        }
        // Default color
        else
        {
            currentSpeed = defaultSpeed;
            spriteRenderer.color = Color.white;
        }
    }
}
