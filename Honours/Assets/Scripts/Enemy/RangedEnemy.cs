using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("References")]
    Transform playerLocation;
    public GameObject bulletPrefab;
    public Transform spawnPoint;

    [Header("Bullet Settings")]

    [SerializeField] float minFireRate = 2f;
    [SerializeField] float maxFireRate = 2.5f;
    [SerializeField] float attackRange = 5f;
    [SerializeField] float fireRate = 2f;
    [SerializeField] float projectileSpeed = 15f;
    [SerializeField] float nextFireTime = 0f;
    public float doubleProjectileChance = 0.3f;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        Invoke("SetToActive", 0.3f);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.fireAppear);
        fireRate = Random.Range(minFireRate, maxFireRate);
    }


    void Update()
    {
        // Do not fire projectiles if enemy is not active
        if (!IsActive()) return;

        playerLocation = GameObject.FindGameObjectWithTag("Player").transform;

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

    void SetToActive()
    {
        isActive = true;
        animator.SetTrigger("hasSpawned");
    }

    void FireProjectilesBasedOnDifficulty()
    {
        float chance = Random.value;

        bool isChallenge = false;
        int difficulty = 0; // Default value

        // Check if DifficultyManager is available and set challenge mode and difficulty
        if (DifficultyManager.Instance != null)
        {
            isChallenge = DifficultyManager.Instance.IsHardMode();
            difficulty = DifficultyManager.Instance.currentDifficulty;
        }

        // Difficulty level 3 or above
        if (difficulty >= 3)
        {
            if (isChallenge)
            {
                // In Challenge Mode, increase chance for firing 3 projectiles to 85%
                if (chance <= 0.85f)
                {
                    StartCoroutine(FireThreeProjectiles(spawnPoint.right));
                }
                else
                {
                    // 15% chance for firing 2 projectiles
                    if (chance <= 0.97f)
                    {
                        StartCoroutine(FireTwoProjectiles(spawnPoint.right));
                    }
                    else
                    {
                        // 3% chance for firing 1 projectile
                        FireProjectile(spawnPoint.right);
                    }
                }
            }
            else
            {
                // Normal difficulty level 3+: 75% chance for firing 3 projectiles
                if (chance <= 0.75f)
                {
                    StartCoroutine(FireThreeProjectiles(spawnPoint.right));
                }
                else
                {
                    // 15% chance for firing 2 projectiles
                    if (chance <= 0.90f)
                    {
                        StartCoroutine(FireTwoProjectiles(spawnPoint.right));
                    }
                    else
                    {
                        // 10% chance for firing 1 projectile
                        FireProjectile(spawnPoint.right);
                    }
                }
            }
        }
        // Difficulty level 2
        else if (difficulty >= 2)
        {
            if (isChallenge)
            {
                // In Challenge Mode, 75% chance for firing 2 projectiles
                if (chance <= 0.75f)
                {
                    StartCoroutine(FireTwoProjectiles(spawnPoint.right));
                }
                else
                {
                    // 25% chance for firing 1 projectile
                    FireProjectile(spawnPoint.right);
                }
            }
            else
            {
                // Normal difficulty level 2: 50% chance for firing 2 projectiles
                if (chance <= 0.50f)
                {
                    StartCoroutine(FireTwoProjectiles(spawnPoint.right));
                }
                else
                {
                    // 50% chance for firing 1 projectile
                    FireProjectile(spawnPoint.right);
                }
            }
        }
        // Difficulty level 1 or below: always fire 1 projectile
        else
        {
            FireProjectile(spawnPoint.right);
        }
    }


    void FireProjectile(Vector3 direction)
    {
        if (spawnPoint == null || bulletPrefab == null) return;

        // Use the spawn point's direction to fire the projectile
        direction = spawnPoint.right;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.fireBall);
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
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator FireThreeProjectiles(Vector3 direction)
    {

        for (int i = 0; i < 3; i++)
        {
            // Fire a single bullet in the given direction
            FireProjectile(direction);

            // Wait until the delay has passed before firing again
            yield return new WaitForSeconds(0.3f);
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

