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

    [Header("Speed Increase Settings")]
    [SerializeField] float minRandomInterval = 5f;
    [SerializeField] float maxRandomInterval = 15f;
    [SerializeField] float speedIncreaseDuration = 8f;

    Vector2 moveDirection;
    bool speedIncreased = false;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpeed = defaultSpeed;
        StartCoroutine(RandomizeSpeed());
    }
    void FixedUpdate()
    {
        // Apply movement to player 
        rb.velocity = moveDirection * currentSpeed;
    }


    void Update()
    {

    }

    void OnMove(InputValue value)
    {
        moveDirection = value.Get<Vector2>();
    }

    IEnumerator RandomizeSpeed()
    {
        while (true)
        {
            // Wait for a random interval between min and max value
            float randomInterval = Random.Range(minRandomInterval, maxRandomInterval);
            yield return new WaitForSeconds(randomInterval);

            // Apply speed increase
            IncreaseSpeed();

            // Waits until the speed increase duration has passed, then reset speed
            yield return new WaitForSeconds(speedIncreaseDuration);
            ResetSpeed();
        }
    }

    void IncreaseSpeed()
    {
        currentSpeed = increasedSpeed;
        speedIncreased = true;
        spriteRenderer.color = Color.green;
    }

    void ResetSpeed()
    {
        currentSpeed = defaultSpeed;
        speedIncreased = false;
        spriteRenderer.color = Color.white;
    }

}
