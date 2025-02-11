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
    public float doubleProjectileChance = 0.3f;
    void Start()
    {
        playerLocation = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

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
            FireProjectilesBasedOnDifficulty();
            
            // Update the next fire time
            nextFireTime = Time.time + fireRate;
        }
    }

    void FireProjectilesBasedOnDifficulty()
    {
        // Generates a random number between 0 and 1 for chance-based firing
        float chance = Random.value;

        // Check difficulty level and adjust the chance of firing more projectiles
        if (DifficultyManager.Instance.currentDifficultyLevel >= 3)
        {
            // At level 3 or above, there's a high chance of firing 3 projectiles
            if (chance <= 0.75f) // 75% chance to fire 3 projectiles
            {
                StartCoroutine(FireThreeProjectiles(spawnPoint.right));
            }
            else if (chance <= 0.90f) // 15% chance to fire 2 projectiles
            {
                StartCoroutine(FireTwoProjectiles(spawnPoint.right));
            }
            else
            {
                FireProjectile(spawnPoint.right); // 10% chance to fire 1 projectile
            }
        }
        else if (DifficultyManager.Instance.currentDifficultyLevel >= 2)
        {
            // At level 2, there's a higher chance of firing 2 projectiles
            if (chance <= 0.50f) // 50% chance to fire 2 projectiles
            {
                StartCoroutine(FireTwoProjectiles(spawnPoint.right));
            }
            else
            {
                FireProjectile(spawnPoint.right); // 50% chance to fire 1 projectile
            }
        }
        else
        {
            // Below level 2, fire 1 projectile only
            FireProjectile(spawnPoint.right);
        }
    }

    void FireProjectile(Vector3 direction)
    {
        if (spawnPoint == null || bulletPrefab == null) return;

        // Use the spawn point's direction to fire the projectile
        direction = spawnPoint.right;

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

    IEnumerator FireTwoProjectiles(Vector3 direction)
    {

        for (int i = 0; i < 2; i++)
        {
            // Fire a single bullet in the given direction
            FireProjectile(direction);

            // Wait until the delay has passed before firing again
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator FireThreeProjectiles(Vector3 direction)
    {

        for (int i = 0; i < 3; i++)
        {
            // Fire a single bullet in the given direction
            FireProjectile(direction);

            // Wait until the delay has passed before firing again
            yield return new WaitForSeconds(0.2f);
        }
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

