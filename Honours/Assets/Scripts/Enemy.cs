using System.Collections;
using UnityEngine;
using static Damage;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform playerTransform;
    public Transform spawnPoint;
    SpriteRenderer spriteRenderer;

    [Header("Bullet Settings")]
    [SerializeField] float attackRange = 5f;
    [SerializeField] float fireRate = 2f;
    [SerializeField] float projectileSpeed = 15f;
    [SerializeField] float nextFireTime = 0f;

    
    public float Health { get; set; }
    [SerializeField] float maxHealth = 45f;
    float currentHealth;

    bool hit = false;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }


    void Update()
    {
        // Check the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

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
        Vector2 direction = (playerTransform.position - spawnPoint.position).normalized;

        // Instantiate the projectile at the fire point
        GameObject projectile = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);

        // Set the projectile's velocity to move towards the player
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = direction * projectileSpeed;
    }

    public void Damage(float damage)
    {
        // Only deals damage if damage has not already been dealt
        if (!hit)
        {
            currentHealth -= damage;

            //healthBar.UpdateHealthBar(currentHealth, maxHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
            // Change colour of enemy 
            StartCoroutine(HitEffect());
            hit = true;
        }
    }
    IEnumerator HitEffect()
    {
        // Change enemy colour
        spriteRenderer.color = Color.white;

        // Wait until delay has ended
        yield return new WaitForSeconds(0.2f);
        hit = false;

        // Revert enemy colour back to original
        spriteRenderer.color = Color.red;
    }



    void Die()
    {
        Debug.Log("Die");
        Destroy(gameObject);
    }
}
