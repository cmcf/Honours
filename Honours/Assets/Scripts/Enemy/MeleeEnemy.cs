using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{

    Transform playerLocation;
    Rigidbody2D rb;
    public int damage;
    public bool notInRoom = false;

    [Header("Movement")]
    [SerializeField] float stoppingDistance = 10f;
    public bool reachedPlayer = false;
    [SerializeField] float moveSpeed;

    [Header("Attack")]
    [SerializeField] float attackCooldown = 2f;
    float attackTimer = 0f; // Tracks time since last attack
    private Vector2 lastPlayerPosition;

    [System.Obsolete]
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerLocation = player.transform;
        }

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!notInRoom)
        {
            MoveTowardsPlayer();
        }
        else
        {
            // Stop all movement if the enemy is not in the current room
            rb.velocity = Vector2.zero;
            return;
        }
    }

    void MoveTowardsPlayer()
    {
        if (playerLocation == null)
            return;

        // Calculate distance between enemy and player
        float distanceToPlayer = Vector2.Distance(transform.position, playerLocation.position);

        if (distanceToPlayer <= stoppingDistance)
        {
            // Stop moving and attack
            rb.velocity = Vector2.zero;
            reachedPlayer = true;
            AttackPlayer();
        }
        else
        {
            // Move toward the player
            reachedPlayer = false;
            Vector2 direction = ((Vector2)playerLocation.position - (Vector2)transform.position).normalized;
            rb.velocity = direction * moveSpeed;
        }
    }

    void AttackPlayer()
    {
        // Check if the attack is ready
        if (attackTimer <= 0f)
        {
            Debug.Log("Attack Player");

            // Reset the attack timer
            attackTimer = attackCooldown;

            // Implement damage logic here
        }

        // Reduce attack cooldown timer
        attackTimer -= Time.deltaTime;
    }
}

