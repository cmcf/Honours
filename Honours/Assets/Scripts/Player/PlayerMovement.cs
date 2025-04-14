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
    public bool canDash = true;
    public bool canMovePlayer = true;
    bool isFacingRight = false;

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

    void OnEnable()
    {
        canDash = true; // Ensure dashing is available when re-enabling
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
            // If idle, keep the last vertical direction
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

        // Determine the dash direction
        Vector2 dashDirection = moveDirection != Vector2.zero ? moveDirection : playerAim.lastFireDirection;

        // Ensure there's always a valid direction
        if (dashDirection == Vector2.zero)
        {
            dashDirection = Vector2.right;
        }

        // Store last dash direction

        lastMoveDirection = dashDirection; 

        // Make player invincible during the dash
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);

        // Sprite stretch effect and dash effects
        spriteRenderer.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        trailRenderer.emitting = true;

        float elapsedTime = 0f;
        Vector2 startPosition = rb.position;
        Vector2 endPosition = startPosition + (dashDirection.normalized * dashDistance);

        // Move the player smoothly during the dash
        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(startPosition, endPosition, elapsedTime / dashDuration));
            yield return null;
        }

        // Reset sprite and dash effects
        spriteRenderer.transform.localScale = Vector3.one;
        spriteRenderer.color = Color.white;
        trailRenderer.emitting = false;

        // Re-enable collisions
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);

        isDashing = false;

        // Set trail duration to match cooldown
        trailRenderer.time = dashCooldown;

        // Wait until the trail disappears
        yield return new WaitForSeconds(trailRenderer.time);

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

    public void DisableInput()
    {
        moveDirection = Vector2 .zero;
    }
}
