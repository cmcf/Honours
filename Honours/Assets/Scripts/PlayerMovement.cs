using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;

    [Header("Speed Settings")]
    [SerializeField] float defaultSpeed = 3f;
    [SerializeField] float currentSpeed;
    [SerializeField] float increasedSpeed = 6f;

    Vector2 moveDirection;
    public Vector2 lastMoveDirection = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpeed = defaultSpeed;
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // Apply movement to player 
        rb.velocity = moveDirection * currentSpeed;

        // Updates animations based on movement direction
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
        if (speed > 0)
        {
            // Normalize the direction for consistent facing
            Vector2 normalizedDirection = moveDirection.normalized;

            // Update parameters for the blend tree
            animator.SetFloat("animMoveX", normalizedDirection.x);
            animator.SetFloat("animMoveY", normalizedDirection.y);

            // Save the last direction for idle animations
            lastMoveDirection = normalizedDirection;
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
