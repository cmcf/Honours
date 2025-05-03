using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class MeleeEnemy : Enemy
{
    Transform playerLocation;
    [Header("State Control")]
    bool reachedPlayer = false;
    bool isDisappearing = false;
    bool isAppearing = false;

    [Header("Movement")]
    [SerializeField] float stoppingDistance = 5f;
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float moveSpeedIncrease = 1f;

    [Header("Attack")]
    [SerializeField] int minDamageAmount = 6;
    [SerializeField] int maxDamageAmount = 15;
    [SerializeField] int hardModeDamageIncrease = 2;
    int damageAmount;
    bool hitPlayer = false;

    [Header("Animation Timing")]
    [SerializeField] float appearDuration = 0.5f; 
    [SerializeField] float disappearDuration = 0.5f; 

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
        
        currentHealth = maxHealth;
        if (DifficultyManager.Instance.IsHardMode())
        {
            minDamageAmount += hardModeDamageIncrease;
            moveSpeed += moveSpeedIncrease;
        }

        damageAmount = Random.Range(minDamageAmount, maxDamageAmount);
        StartCoroutine(UpdatePlayerLocation());
        StartCoroutine(HandleAppearance());
    }

    void Update()
    {
        // Stop movement when not active
        if (isDisappearing || isAppearing) return;

        MoveTowardsPlayer();
    }

    IEnumerator UpdatePlayerLocation()
    {
        while (true)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerLocation = player.transform;
            }

            yield return new WaitForSeconds(0.5f);
        }
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
        if (playerLocation == null || !IsActive()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerLocation.position);

        // Stop movement and trigger disappearance
        if (distanceToPlayer <= stoppingDistance)
        {
            rb.velocity = Vector2.zero;
            reachedPlayer = true;
            AttackPlayer();
        }
        else
        {
            reachedPlayer = false;

            // Raycast to check if there's a wall between enemy and player
            RaycastHit2D hit = Physics2D.Raycast(transform.position, playerLocation.position - transform.position, distanceToPlayer, LayerMask.GetMask("Wall"));

            if (hit.collider == null)
            {
                Vector2 direction = ((Vector2)playerLocation.position - (Vector2)transform.position).normalized;
                rb.velocity = direction * moveSpeed;
            }
            else
            {
                rb.velocity = Vector2.zero;
            }

            animator.SetBool("isAttacking", true);
            lastKnownPosition = transform.position;
        }
    }


    void AttackPlayer()
    {
        if (!reachedPlayer) return;
        // Disappear upon reaching the player
        rb.velocity = Vector2.zero; 

        // Start the disappearance coroutine
        StartCoroutine(HandleDisappearance());
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !hitPlayer)
        {
            // Check if the target has a damageable component
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                hitPlayer = true;
                // Deal damage to the player
                damageable.Damage(damageAmount);
            }


        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        hitPlayer = false;
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

        Room currentRoom = RoomController.Instance.currentRoom;
        RespawnEnemy(currentRoom);

        isDisappearing = false;

        // Respawn enemy
        StartCoroutine(HandleAppearance());
    }

    void RespawnEnemy(Room currentRoom)
    {
        if (currentRoom == null) return;

        Vector3 roomCentre = currentRoom.GetRoomCentre();
        float roomWidth = currentRoom.width;
        float roomHeight = currentRoom.height;
        Vector3 roomPosition = currentRoom.transform.position;

        float margin = 1.5f;

        float minX = roomPosition.x - roomWidth / 2f + margin;
        float maxX = roomPosition.x + roomWidth / 2f - margin;
        float minY = roomPosition.y - roomHeight / 2f + margin;
        float maxY = roomPosition.y + roomHeight / 2f - margin;

        int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 spawnPosition = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            // Check for overlap with walls
            Collider2D hit = Physics2D.OverlapCircle(spawnPosition, 0.3f, LayerMask.GetMask("Wall"));
            if (hit == null)
            {
                transform.position = spawnPosition;
                return;
            }
        }

        // Use room centre if all attempts fail
        transform.position = roomCentre;
    }

}

