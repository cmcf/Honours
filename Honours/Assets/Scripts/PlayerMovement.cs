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
    [SerializeField] float dashDistance = 3f; // How far the dash moves the player
    [SerializeField] float dashDuration = 0.8f;
    [SerializeField] float dashSpeed = 7f;
    [SerializeField] float dashCooldown = 2f;

    [Header("Effects")]
    [SerializeField] Color dashColor = new Color(1, 1, 1, 0.5f); // Semi-transparent during dash
    [SerializeField] ParticleSystem dashEffect; // Optional particle effect

    Vector2 moveDirection;
    public Vector2 lastMoveDirection = Vector2.zero;

    bool isDashing = false;
    bool canDash = true;
    bool isFacingRight = false;

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

        if (speed > 0 && !isDashing)
        {
            // Normalize the movement direction
            Vector2 normalizedDirection = moveDirection.normalized;

            // Update blend tree parameters
            animator.SetFloat("animMoveX", normalizedDirection.x);
            animator.SetFloat("animMoveY", normalizedDirection.y);

            // Save the player's last movement direction
            lastMoveDirection = normalizedDirection;

            // Flip the player based on horizontal movement
            if (moveDirection.x > 0) 
            {
                // Flip player right
                transform.localScale = new Vector3(-1, 1, 1); 
                weapon.localScale = new Vector3(1, 1, 1);
                isFacingRight = true;
            }
            else if (moveDirection.x < 0) 
            {
                // Flip player left 
                transform.localScale = new Vector3(1, 1, 1); 
                weapon.localScale = new Vector3(1, 1, 1); 
                isFacingRight = false; 
            }

            // Rotate the weapon based on movement direction and facing state
            RotateWeaponBasedOnMovement(normalizedDirection);
        }
        else
        {
            // Player is idle; use the last movement direction
            animator.SetFloat("animMoveX", lastMoveDirection.x);
            animator.SetFloat("animMoveY", lastMoveDirection.y);
        }

        // Update the speed parameter for animation states
        animator.SetFloat("speed", speed);
    }

    // Function to handle weapon rotation based on player movement direction
    void RotateWeaponBasedOnMovement(Vector2 normalizedDirection)
    {
        if (isFacingRight) 
        {
            // Rotate weapon for up/down movement when facing right
            if (normalizedDirection.y > 0) // Moving Up
            {
                // Rotates the  weapon upwards
                weapon.rotation = Quaternion.Euler(0, 0, 90f);
                // Sets weapon postion
                weapon.localPosition = new Vector3(-0.075f, 0.208f, 0); 
            }
            // Moving Down
            else if (normalizedDirection.y < 0) 
            {
                // Rotate weapon downwards
                weapon.rotation = Quaternion.Euler(0, 0, -90f); 
                weapon.localPosition = new Vector3(-0.178f, -0.1939f, 0); 
            }

            else
            {
                // Set weapon to default position and rotation
                weapon.rotation = Quaternion.Euler(0, 0, 0); 
                weapon.localPosition = new Vector3(-0.335f, 0, 0); 
            }
        }
        else 
        {
            // If the player is facing left - rotate weapon for up/down movement
            if (normalizedDirection.y > 0) // Moving Up
            {
                // Rotate weapon upwards
                weapon.rotation = Quaternion.Euler(0, 0, -90f); 
                //Set weapon position
                weapon.localPosition = new Vector3(-0.075f, 0.208f, 0); 
            }
            else if (normalizedDirection.y < 0) // Moving Down
            {
                // Rotate weapon downwards
                weapon.rotation = Quaternion.Euler(0, 0, 90f); 
                weapon.localPosition = new Vector3(-0.161f, -0.1856f, 0); 
            }
            else // Neutral horizontal movement
            {
                weapon.rotation = Quaternion.Euler(0, 0, 0); 
                weapon.localPosition = new Vector3(-0.335f, 0, 0); 
            }
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

        // Sprite stretch effect
        spriteRenderer.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
        // Make player semi-transparent
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f); 

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
