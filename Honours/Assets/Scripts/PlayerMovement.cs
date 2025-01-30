using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;
    public Transform weapon;

    [Header("Speed Settings")]
    [SerializeField] float defaultSpeed = 3f;
    [SerializeField] float currentSpeed;
    [SerializeField] float increasedSpeed = 6f;

    [Header("Dash Settings")]
    [SerializeField] float dashDuration = 0.8f;
    [SerializeField] float dashSpeed = 7f;
    [SerializeField] float dashCooldown = 2f;

    Vector2 moveDirection;
    public Vector2 lastMoveDirection = Vector2.zero;

    bool isDashing = false;
    bool canDash = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpeed = defaultSpeed;
        animator = GetComponent<Animator>();
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
        float speed = moveDirection.magnitude;

        // Check if the player is moving
        if (speed > 0 && !isDashing)
        {
            // Normalize the direction for consistent facing
            Vector2 normalizedDirection = moveDirection.normalized;

            // Update parameters for the blend tree
            animator.SetFloat("animMoveX", normalizedDirection.x);
            animator.SetFloat("animMoveY", normalizedDirection.y);

            // Save the last direction for idle animations
            lastMoveDirection = normalizedDirection;

            // Flip player
            if (moveDirection.x > 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                weapon.localScale = new Vector3(1, 1, 1); 
            }
            else if (moveDirection.x < 0)
            {
                transform.localScale = new Vector3(1, 1, 1); 
                weapon.localScale = new Vector3(1, 1, 1); 
            }
        }
        else
        {
            // Player is idle; use the last movement direction
            animator.SetFloat("animMoveX", lastMoveDirection.x);
            animator.SetFloat("animMoveY", lastMoveDirection.y);
        }

        // Update Speed parameter for switching between run and idle states
        animator.SetFloat("speed", speed);
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

        Vector2 dashDirection;

        if (moveDirection != Vector2.zero)
        {
            dashDirection = moveDirection;
        }
        else if (lastMoveDirection != Vector2.zero)
        {
            dashDirection = lastMoveDirection;
        }
        else
        {
            dashDirection = Vector2.right; 
        }

        if (dashDirection == Vector2.zero)
        {
            dashDirection = Vector2.right; 
        }

        rb.velocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        rb.velocity = Vector2.zero; 

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
