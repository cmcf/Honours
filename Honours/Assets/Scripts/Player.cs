using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static Damage;

public class Player : MonoBehaviour, IDamageable
{
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject iceBulletPrefab;
    public Transform spawnPoint;
    public Enemy currentEnemy;
    PlayerAim playerAim;

    SpriteRenderer spriteRenderer;
    PlayerMovement playerMovement;
    Animator animator;

    [Header("Health")]
    [SerializeField] float currentHealth = 50f;
    [SerializeField] float health = 50f;
    public float Health { get; set; }

    [Header("Firing")]
    float lastFireTime = 0f;

    [Header("State")]
    public bool isDead = false; 

    public Weapon currentWeapon;
    bool isAutoFiring = false;
    [SerializeField] int numberOfDeaths = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAim = GetComponentInChildren<PlayerAim>();
        currentHealth = health;
    }

    void OnFire()
    {
        if (Time.time > lastFireTime + currentWeapon.fireDelay)
        {
            lastFireTime = Time.time;
            Vector3 fireDirection;

            if (playerAim.usingController)
            {
                // Use last known aim direction for the controller
                fireDirection = (Vector3)playerAim.aimDirection.normalized;
                if (fireDirection.magnitude < 0.1f)
                {
                    fireDirection = Vector3.left; // Default direction if stick is not moved
                }
            }
            else
            {
                // Mouse aiming
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(
                    Input.mousePosition.x,
                    Input.mousePosition.y,
                    Mathf.Abs(Camera.main.transform.position.z)
                ));
                fireDirection = (mousePosition - spawnPoint.position).normalized;
                FireWeapon(fireDirection);
            }


        }
    }
    
    void FireWeapon(Vector3 direction)
    {
        switch (currentWeapon.weaponType)
        {
            case Weapon.WeaponType.Default:
                FireSingleBullet(direction);
                break;
            case Weapon.WeaponType.RapidFire:
                StartCoroutine(FireRapid(direction));
                break;
            case Weapon.WeaponType.SpreadShot:
                FireSpreadBullets(direction, 5, 30f);
                break;
            case Weapon.WeaponType.Ice:
                FireIceBullet(direction);
                break;
        }
    }

    void FireSingleBullet(Vector3 direction)
    {
        // Ensure the direction is normalized
        direction.Normalize();

        // Calculate the spawn point near the end of the weapon
        Vector3 weaponDirection = direction; // Direction already normalized
        Vector3 bulletSpawnPosition = spawnPoint.position + weaponDirection * 0.5f; 

        // Instantiate the bullet at the adjusted spawn point
        GameObject projectile = Instantiate(bulletPrefab, bulletSpawnPosition, Quaternion.identity);

        // Calculate the rotation angle based on the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply the rotation to the bullet to face the correct direction
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Apply velocity to the projectile based on the direction
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Ensures consistent velocity
            rb.velocity = direction * currentWeapon.bulletSpeed; 
        }
    }


    IEnumerator FireRapid(Vector3 direction)
    {
        // Fire 3 bullets in quick succession
        for (int i = 0; i < 3; i++) 
        {
            // Fire a single bullet in the given direction
            FireSingleBullet(direction); 
            yield return new WaitForSeconds(currentWeapon.fireDelay / 3);
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
            rb.velocity = direction * currentWeapon.bulletSpeed;
        }

        // Sets the ice bullet behaviour
        Bullet bullet = iceBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(currentWeapon.bulletDamage);
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

        // Disable aim ability
        playerAim.enabled = false;

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
