using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeEnemy : Enemy
{

    Transform playerLocation;
    Rigidbody2D rb;
    Animator animator;
    Room currentRoom;
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
        // Disappear upon reaching the player
        rb.velocity = Vector2.zero; // Stop any residual movement

        // Start the disappearance coroutine
        StartCoroutine(HandleDisappearance());
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
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
        if (RoomController.Instance != null)
        {
            currentRoom = RoomController.Instance.currentRoom;
        }
            
        RespawnEnemy(currentRoom);

        isDisappearing = false;

        // Respawn enemy
        StartCoroutine(HandleAppearance());
    }

    void RespawnEnemy(Room currentRoom)
    {
        if (currentRoom == null) return;

        // Get room boundaries
        Vector3 roomCentre = currentRoom.GetRoomCentre();
        float roomWidth = currentRoom.width;
        float roomHeight = currentRoom.height;

        // Define spawn area bounds
        float margin = 1.5f; 

        float minX = roomCentre.x - roomWidth / 2f + margin;
        float maxX = roomCentre.x + roomWidth / 2f - margin;
        float minY = roomCentre.y - roomHeight / 2f + margin;
        float maxY = roomCentre.y + roomHeight / 2f - margin;

        // Generates a random spawn position within the room bounds
        Vector2 spawnPosition = new Vector2(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY)
        );

        // Update enemy position to the new spawn position
        transform.position = spawnPosition;
    }
}

