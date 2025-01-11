using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    Transform playerLocation;
    Rigidbody2D rb;
    public GameObject bulletPrefab;
    public Transform spawnPoint;

    [Header("Bullet Settings")]
    [SerializeField] float attackRange = 5f;
    [SerializeField] float fireRate = 2f;
    [SerializeField] float projectileSpeed = 15f;
    [SerializeField] float nextFireTime = 0f;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 2f; // Speed at which the enemy moves

    void Start()
    {
        playerLocation = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        // Ensures the animator is correctly initialized in the base class
        base.Start();

        // Initializes animator in the derived class
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found on this GameObject!");
        }
    }

    void Update()
    {
        // Do not fire projectiles or move if enemy is not active
        if (!IsActive()) return;

        // Check the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, playerLocation.position);

        UpdateEnemyAndSpawnPoint();

        if (distanceToPlayer > attackRange)
        {
            // Move toward the player if out of attack range
            MoveTowardsPlayer();
        }
        else if (Time.time >= nextFireTime)
        {
            FireProjectile();
            nextFireTime = Time.time + fireRate;
        }
    }

    void MoveTowardsPlayer()
    {
        if (playerLocation == null) return;

        // Calculate direction toward the player
        Vector2 direction = (playerLocation.position - transform.position).normalized;

        // Move the enemy using the Rigidbody2D
        rb.velocity = direction * moveSpeed;
    }

    void FireProjectile()
    {
        if (spawnPoint == null || bulletPrefab == null) return;
       
        // Stop movement while attacking
        rb.velocity = Vector2.zero;
        animator.SetBool("isAttacking", true);

        // Use the spawn point's direction to fire the projectile
        Vector2 direction = spawnPoint.right;

        // Instantiate the projectile at the spawn point
        GameObject projectile = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);

        // Set the projectile's velocity to move in the direction of the spawn point
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.velocity = direction * projectileSpeed;
        }

        // Reset attack animation after a short delay
        Invoke("ResetAnimation", 0.3f);
    }

    void ResetAnimation()
    {
        animator.SetBool("isAttacking", false);
        // Ensure the enemy resumes moving if the player is out of attack range
        float distanceToPlayer = Vector2.Distance(transform.position, playerLocation.position);
        if (distanceToPlayer > attackRange)
        {
            // Ensure the enemy is moving again after the attack (no "moving" animation required)
            rb.velocity = (playerLocation.position - transform.position).normalized * moveSpeed;
        }
    }

    void UpdateEnemyAndSpawnPoint()
    {
        if (playerLocation == null) return;

        // Calculates the direction vector from the enemy to the player
        Vector2 direction = (playerLocation.position - transform.position).normalized;

        // Flip the enemy sprite based on the player's position
        FlipEnemySprite(direction);

        // Rotate the projectile spawn point to face the player
        RotateSpawnPoint(direction);
    }

    void FlipEnemySprite(Vector2 direction)
    {
        // Flips sprite to face the player
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x > 0;
        }
    }

    void RotateSpawnPoint(Vector2 direction)
    {
        if (spawnPoint == null) return;

        // Calculates the angle for the spawn point
        float spawnPointAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Rotates only the projectile spawn point to face the player
        spawnPoint.rotation = Quaternion.Euler(0f, 0f, spawnPointAngle);
    }
}


