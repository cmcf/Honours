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
    void Start()
    {
        playerLocation = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        // Do not fire projectiles if enemy is not active
        if (!IsActive()) return;

        // Check the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, playerLocation.position);

        RotateTowardsPlayer();

        // If the player is within range and it's time to fire - spawn bullet 
        if (distanceToPlayer <= attackRange && Time.time >= nextFireTime)
        {

            FireProjectile();
            // Update the next fire time
            nextFireTime = Time.time + fireRate;
        }
    }

    void FireProjectile()
    {
        // Get the direction the enemy is facing
        Vector2 direction = transform.up;

        // Instantiate the projectile at the fire point
        GameObject projectile = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);

        // Set the projectile's velocity to move in the same direction as the enemy
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = direction * projectileSpeed;
    }

    void RotateTowardsPlayer()
    {
        if (playerLocation == null) return;

        // Calculates the direction vector from the enemy to the player
        Vector2 direction = (playerLocation.position - transform.position).normalized;

        // Calculates the angle between the enemy's current up direction and the target direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // Rotates enemy towards the player
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

}

