using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using static Damage;

public class Player : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject explosiveBulletPrefab;
    public Transform spawnPoint;
    public Enemy currentEnemy;

    SpriteRenderer spriteRenderer;
    PlayerMovement playerMovement;

    [SerializeField] float bulletSpeed = 10f;
    float lastFireTime = 0f;
    float stateTimer = 0f;
    float enhancedStateDuration = 9f;
    float iceBulletDamage = 15f;
    int defaultBulletDamage = 10;
    [SerializeField] float fireDelay = 0.5f;

    public WeaponType currentWeaponType = WeaponType.Default;

    // Weapon Type Enum
    public enum WeaponType
    {
        Default,    // Fires a single bullet
        RapidFire,  // Fires bullets with a faster rate
        SpreadShot, // Fires multiple bullets in a spread
        Ice   // Fires an ice projectile
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        //StartCoroutine(RandomlyChangeState());
    }

    void Update()
    {

    }


    void OnFire()
    {
        // Only fire if the fire delay has passed
        if (Time.time > lastFireTime + fireDelay)
        {
            lastFireTime = Time.time;

            // Determine the fire direction
            Vector3 fireDirection = playerMovement.lastMoveDirection;
            if (fireDirection == Vector3.zero)
            {
                // Default direction
                fireDirection = -spawnPoint.up.normalized; 
            }

            // Handle different weapon types
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
        GameObject iceBullet = Instantiate(explosiveBulletPrefab, spawnPoint.position, Quaternion.identity);
        iceBullet.transform.up = direction;

        Rigidbody2D rb = iceBullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }

        // Configure the ice bullet behaviour
        Bullet bullet = iceBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(iceBulletDamage);
            bullet.isIce = true;
            // Set the freeze duration
            bullet.freezeDuration = 1.5f; 
        }
    }
}
