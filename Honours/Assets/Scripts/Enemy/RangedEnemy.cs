using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    Transform playerLocation;
    Rigidbody2D rb;
    Animator animator;
    public GameObject bulletPrefab;
    public Transform spawnPoint;

    [Header("Bullet Settings")]
    [SerializeField] float attackRange = 5f;
    [SerializeField] float fireRate = 2f;
    [SerializeField] float projectileSpeed = 15f;
    [SerializeField] float nextFireTime = 0f;
    void Start()
    {
        playerLocation = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        // Do not fire projectiles if enemy is not active
        if (!IsActive()) return;

        // Check the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, playerLocation.position);

        UpdateEnemyAndSpawnPoint();

        // If the player is within range and it's time to fire - spawn bullet 
        if (distanceToPlayer <= attackRange && Time.time >= nextFireTime)
        {
            animator.SetBool("isAttacking", true);
            FireProjectile();
            // Update the next fire time
            nextFireTime = Time.time + fireRate;
        }
    }

    void FireProjectile()
    {
        if (spawnPoint == null || bulletPrefab == null) return;

        // Use the spawn point's direction to fire the projectile
        Vector2 direction = spawnPoint.right;

        // Instantiate the projectile at the spawn point
        GameObject projectile = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);

        // Set the projectile's velocity to move in the direction of the spawn point
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }

        Invoke("ResetAnimation", 0.3f);
    }

    void ResetAnimation()
    {
        animator.SetBool("isAttacking", false);
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

