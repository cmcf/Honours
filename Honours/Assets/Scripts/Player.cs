using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using static Damage;

public class Player : MonoBehaviour
{
    public enum PlayerState { Normal, Enhanced, EnemyTransformed }
    public PlayerState currentState = PlayerState.Normal;

    public GameObject bulletPrefab;
    public Transform spawnPoint;
    public Enemy currentEnemy;
    SpriteRenderer spriteRenderer;

    [SerializeField] float bulletSpeed = 10f;
    float lastFireTime = 0f;
    float stateTimer = 0f;
    float enhancedStateDuration = 9f;
    float enhancedBulletDamage = 15f;
    float defaultBulletDamage = 10f;
    [SerializeField] float fireDelay = 0.5f;
    SpriteRenderer enemySpriteRenderer;
    PlayerMovement playerMovement; // Reference to PlayerMovement

    void Start()
    {
        spriteRenderer= GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        //StartCoroutine(RandomlyChangeState());
    }

    void Update()
    {
        // Manage the player state timer
        if (currentState == PlayerState.Enhanced)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                ChangeState(PlayerState.Normal);
            }
        }
    }

    void OnFire()
    {
        // Only fire if the fire delay has passed
        if (Time.time > lastFireTime + fireDelay)
        {
            // Record the time of this shot
            lastFireTime = Time.time;

            // Get the direction to fire in players facing direction
            Vector3 fireDirection = spawnPoint.up.normalized;

            // Instantiate the bullet
            GameObject projectile = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);

            // Rotate the bullet to match the firing direction
            projectile.transform.up = fireDirection;

            // Pass the player's state to the bullet for damage adjustment
            Bullet bullet = projectile.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetDamage(currentState == PlayerState.Enhanced ? enhancedBulletDamage : defaultBulletDamage);
            }

            // Apply velocity to the bullet
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = fireDirection * bulletSpeed;
            }
        }
    }

    public void ChangePlayerState()
    {
        StartCoroutine(RandomlyChangeState());
    }

    IEnumerator RandomlyChangeState()
    {
        Debug.Log("Called");

           // Change to Enhanced state
            ChangeState(PlayerState.Enhanced);
            Debug.Log("Enhanced");

            // Wait until the enhanced state has passed then change back to normal
            yield return new WaitForSeconds(enhancedStateDuration);
            ChangeState(PlayerState.Normal);
            Debug.Log("Normal");
        
    }

    public void ChangeState(PlayerState newState)
    {
        currentState = newState;

        if (newState == PlayerState.Enhanced)
        {
            stateTimer = enhancedStateDuration;
        }

        // Call the PlayerMovement script to change speed based on the new state
        playerMovement.ChangeSpeed(newState == PlayerState.Enhanced);
    }

    // Called when player collides with the trigger
    public void TransformIntoEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            currentEnemy = enemy;
            currentState = PlayerState.EnemyTransformed;

            // Gets enemies sprite renderer
            enemySpriteRenderer = currentEnemy.GetComponent<SpriteRenderer>();

            if (enemySpriteRenderer != null)
            {
                // Change appearance to enemy sprite
                spriteRenderer.sprite = enemySpriteRenderer.sprite; 
                CopyEnemyAbilities();
            }
        }
    }

    void CopyEnemyAbilities()
    {
        // Changes player sprite renderer to the same colour as enemies
        if (currentEnemy != null)
        {
            spriteRenderer.color = enemySpriteRenderer.color;

            // Could also transfer enemy abilities to player here
        }
    }
}
