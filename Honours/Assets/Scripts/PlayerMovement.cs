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
    PlayerAim playerAim;
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
        playerAim = GetComponentInChildren<PlayerAim>();
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

        // Use lastFireDirection from the PlayerAim script for the dash direction
        Vector2 dashDirection;

        if (playerAim.lastFireDirection != Vector2.zero)
        {
            dashDirection = playerAim.lastFireDirection;
        }
        else
        {
            dashDirection = Vector2.right;
        }

        // If there's still no direction, default to a direction (e.g., right)
        if (dashDirection == Vector2.zero)
        {
            dashDirection = Vector2.right;
        }

        // Update lastMoveDirection to the dash direction
        lastMoveDirection = dashDirection;

        // Store the current facing direction before the dash
        bool wasFacingRight = isFacingRight;

        // Make player invincible during the dash
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);

        // Sprite stretch effect and dash effects
        spriteRenderer.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        trailRenderer.emitting = true;

        float elapsedTime = 0f;
        Vector2 startPosition = rb.position;
        Vector2 endPosition = startPosition + (dashDirection * dashDistance);

        // Move the player smoothly during the dash
        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(startPosition, endPosition, elapsedTime / dashDuration));
            yield return null;
        }

        // Reset sprite and dash effects
        spriteRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        trailRenderer.emitting = false;

        // Re-enable collisions after the dash
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);

        isDashing = false;
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
