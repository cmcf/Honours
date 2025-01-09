using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeEnemy : Enemy
{

    Transform playerLocation;
    Rigidbody2D rb;
    Animator animator;
    public int damage;

    [Header("State Control")]
    bool reachedPlayer = false;
    bool isDisappearing = false;
    bool isAppearing = false;

    [Header("Movement")]
    [SerializeField] float stoppingDistance = 5f;
    [SerializeField] float moveSpeed;

    [Header("Attack")]
    public float damageAmount = 10f;

    [Header("Animation Timing")]
    [SerializeField] float appearDuration = 0.5f; 
    [SerializeField] float disappearDuration = 0.5f; 
    [SerializeField] float delayBeforeAttack = 0.5f;

    [Header("Respawning")]
    [SerializeField] Vector2 minPosition;
    [SerializeField] Vector2 maxPosition;
    Vector2 lastKnownPosition;

    [System.Obsolete]
    void Start()
    {
        lastKnownPosition = transform.position;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerLocation = player.transform;
        }

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        StartCoroutine(HandleAppearance());
    }

    void Update()
    {
        // Stop movement when not active
        if (isDisappearing || isAppearing || !IsActive()) return;

        MoveTowardsPlayer();
    }

    IEnumerator HandleAppearance()
    {
        // Lock movement during appearance
        isAppearing = true;
        rb.velocity = Vector2.zero;

        // Trigger the appear animation
        animator.SetTrigger("Appear");
        yield return new WaitForSeconds(appearDuration);

        // Allow movement after appearance
        isAppearing = false; 
    }


    void MoveTowardsPlayer()
    {
        if (playerLocation == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerLocation.position);

        if (distanceToPlayer <= stoppingDistance)
        {
            // Stop movement and trigger disappearance
            rb.velocity = Vector2.zero; 
            reachedPlayer = true;
            AttackPlayer();
        }
        else
        {
            reachedPlayer = false;
            Vector2 direction = ((Vector2)playerLocation.position - (Vector2)transform.position).normalized;
            rb.velocity = direction * moveSpeed;

            // Ensure the enemy is in the "attacking" state while moving
            animator.SetBool("isAttacking", true);

            // Update last known position
            lastKnownPosition = transform.position;
    

        }
    }

    void AttackPlayer()
    {
        if (!reachedPlayer) return;
        Debug.Log("Last Known Position: " + lastKnownPosition);
        // Disappear upon reaching the player
        rb.velocity = Vector2.zero; // Stop any residual movement

        // Start the disappearance coroutine
        StartCoroutine(HandleDisappearance());
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("Damage");
        }
    }

    IEnumerator HandleDisappearance()
    {
        isDisappearing = true;

        // Stop movement during disappearance
        rb.velocity = Vector2.zero;

        // Trigger disappear animation
        animator.SetTrigger("Disappear");
        animator.SetBool("isAttacking", false); 
        yield return new WaitForSeconds(disappearDuration); 

        RespawnEnemy(); 
        isDisappearing = false;

        // Respawn
        StartCoroutine(HandleAppearance()); 
    }

    void RespawnEnemy()
    {
        float minRepositionDistance = 2.5f; 
        float maxRepositionDistance = 5.5f; 

        // Generate a random angle
        float randomAngle = Random.Range(0f, Mathf.PI * 2);

        // Generate a random distance between min and max
        float randomDistance = Random.Range(minRepositionDistance, maxRepositionDistance);

        // Calculates the new position based on the random angle and distance
        float offsetX = Mathf.Cos(randomAngle) * randomDistance;
        float offsetY = Mathf.Sin(randomAngle) * randomDistance;

        Vector2 newPosition = lastKnownPosition + new Vector2(offsetX, offsetY);

        // Apply the new position to the enemy
        transform.position = newPosition;
    }
}

