using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using static Damage;

public class Player : MonoBehaviour, IDamageable
{
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject iceBulletPrefab;
    public Transform spawnPoint;
    public Enemy currentEnemy;

    SpriteRenderer spriteRenderer;
    PlayerMovement playerMovement;
    Animator animator;

    [Header("Health")]
    [SerializeField] float currentHealth = 50f;
    [SerializeField] float health = 50f;
    public float Health { get; set; }

    [Header("Projectile")]
    [SerializeField] float bulletSpeed = 10f;
    float lastFireTime = 0f;
    float iceBulletDamage = 15f;
    int defaultBulletDamage = 10;
    [SerializeField] float fireDelay = 0.5f;

    [Header("State")]
    bool isDead = false; 

    public WeaponType currentWeaponType = WeaponType.Default;
    bool isAutoFiring = false;
    [SerializeField] int numberOfDeaths = 0;

    // Weapon Type Enum
    public enum WeaponType
    {
        Default,    // Fires a single bullet
        RapidFire,  // Fires bullets with a faster rate
        SpreadShot, // Fires multiple bullets in a spread
        Ice,   // Fires an ice projectile
        AutoFire
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        currentHealth = health;
    }

    void OnFire()
    {
        if (currentWeaponType == WeaponType.AutoFire)
        {
            // If AutoFire is enabled, start or ensure the coroutine is running
            if (!isAutoFiring) 
            {
                isAutoFiring = true;
                StartCoroutine(AutoFireCoroutine());
            }
        }
        else
        {
            // Only fire if the fire delay has passed for non-AutoFire weapons
            if (Time.time > lastFireTime + fireDelay)
            {
                lastFireTime = Time.time;

                // Determine the fire direction
                Vector3 fireDirection = playerMovement.lastMoveDirection;
                if (fireDirection == Vector3.zero)
                {
                    fireDirection = -spawnPoint.up.normalized;
                }

                // Handle different weapon type behaviour
                switch (currentWeaponType)
                {
                    case WeaponType.Default:
                        FireSingleBullet(fireDirection);
                        break;
                    case WeaponType.RapidFire:
                        StartCoroutine(FireRapid(fireDirection));
                        break;
                    case WeaponType.SpreadShot:
                        FireSpreadBullets(fireDirection, 5, 30f);
                        break;
                    case WeaponType.Ice:
                        FireIceBullet(fireDirection);
                        break;
                }
            }
        }
    }


    IEnumerator AutoFireCoroutine()
    {
        // Keep firing as long as AutoFire is active
        while (currentWeaponType == WeaponType.AutoFire) 
        {
            if (Time.time > lastFireTime + fireDelay)
            {
                lastFireTime = Time.time;

                Vector3 fireDirection = playerMovement.lastMoveDirection;
                if (fireDirection == Vector3.zero)
                {
                    fireDirection = -spawnPoint.up.normalized;
                }

                FireSingleBullet(fireDirection);
            }

            yield return null;
        }

        // Once AutoFire is no longer active, stop firing
        isAutoFiring = false;
    }


    void FireSingleBullet(Vector3 direction)
    {
        // Instantiate the bullet
        GameObject projectile = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);

        // Calculate the rotation angle based on the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply the rotation
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Apply velocity to the projectile
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }
    }

    IEnumerator FireRapid(Vector3 direction)
    {
        // Fire 3 bullets in quick succession
        for (int i = 0; i < 3; i++) 
        {
            // Fire a single bullet in the given direction
            FireSingleBullet(direction); 
            yield return new WaitForSeconds(fireDelay / 3);
        }
    }

    void FireSpreadBullets(Vector3 direction, int bulletCount, float spreadAngle)
    {
        // Calculate angle between each bullet
        float angleStep = spreadAngle / (bulletCount - 1); 
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + (i * angleStep);
            // Rotate the direction by angle
            Vector3 bulletDirection = Quaternion.Euler(0, 0, angle) * direction; 
            FireSingleBullet(bulletDirection);
        }
    }

    void FireIceBullet(Vector3 direction)
    {

        // Normalise the direction vector to ensure consistent speed
        direction = direction.normalized;

        // Instantiate the ice bullet at the spawn point
        GameObject iceBullet = Instantiate(iceBulletPrefab, spawnPoint.position, Quaternion.identity);

        // Rotate the bullet to face the direction it will travel
        iceBullet.transform.right = direction;

        // Apply velocity to the bullet's Rigidbody2D
        Rigidbody2D rb = iceBullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }

        // Sets the ice bullet behaviour
        Bullet bullet = iceBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(iceBulletDamage);
            bullet.isIce = true;
        }
    }

    public void Damage(float damage)
    {
        if (isDead) { return; }
        // Current health is decreased by the damage received
        currentHealth -= damage;
        // Player dies when current health reaches 0
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        // Only enter death state once
        if (isDead) { return; }

        // Disable player move input
        playerMovement.enabled = false;

        // Stop movement to prevent sliding
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Set player to death state
        isDead = true;

        // Increase death counter
        numberOfDeaths++;

        // Play death animation
        animator.SetTrigger("isDead");
    }

}
