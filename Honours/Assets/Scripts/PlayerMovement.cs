using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    [Header("Speed Settings")]
    [SerializeField] float defaultSpeed = 3f;
    [SerializeField] float currentSpeed;
    [SerializeField] float increasedSpeed = 6f;

    Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpeed = defaultSpeed;
    }

    void FixedUpdate()
    {
        // Apply movement to player 
        rb.velocity = moveDirection * currentSpeed;

        // Flip sprite and children based on movement direction
        RotatePlayer();
    }

    void OnMove(InputValue value)
    {
        // Get movement input
        moveDirection = value.Get<Vector2>();
    }

    void RotatePlayer()
    {
        // Flip left
        if (moveDirection.x < 0) 
        {
            RotateAllChildren(90f); 
        }
        // Flip right
        else if (moveDirection.x > 0) 
        {
            RotateAllChildren(-90); 
        }
        // Flip up
        else if (moveDirection.y > 0) 
        {
            RotateAllChildren(0f); 
        }
        // Flip down
        else if (moveDirection.y < 0) 
        {
            RotateAllChildren(-180); 
        }
    }

    void RotateAllChildren(float rotationZ)
    {
        // Rotate the player's Z rotation
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);

        // Rotate all child objects' Z rotation
        foreach (Transform child in transform)
        {
            if (child != transform)
            {
                child.rotation = Quaternion.Euler(0f, 0f, rotationZ);
            }
        }
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
