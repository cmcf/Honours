using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    Transform playerLocation;
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
    }

    void Update()
    {
        // Check the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, playerLocation.position);

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
        // Calculate the direction to the player
        Vector2 direction = (playerLocation.position - spawnPoint.position).normalized;

        // Instantiate the projectile at the fire point
        GameObject projectile = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);

        // Set the projectile's velocity to move towards the player
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = direction * projectileSpeed;
    }
}
