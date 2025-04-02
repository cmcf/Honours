using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class HomingProjectile : MonoBehaviour
{
    Transform target;
    Rigidbody2D rb;

    [SerializeField] float speed = 8f;
    [SerializeField] float rotationSpeed = 200f; // Controls how fast it turns
    [SerializeField] int minDamage = 5;
    [SerializeField] int maxDamage = 20;

    bool hitPlayer = false;
    int damageAmount;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        damageAmount = Random.Range(minDamage, maxDamage);
    }

    void FixedUpdate()
    {
        if (target == null) return;

        target = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Calculate direction
        Vector2 direction = (target.position - transform.position).normalized;

        // Rotate smoothly towards the target
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        // Apply movement
        rb.velocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroys bullet if it hits a wall
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }

        if (collision.CompareTag("Player") && !hitPlayer)
        {
            // Deals damage to player if hit
            hitPlayer = true;
            DealDamage(collision.transform);
            Destroy(gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        hitPlayer = false;
    }

    void DealDamage(Transform target)
    {
        // Check if the target has a damageable component
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Deal damage to the player
            damageable.Damage(damageAmount);
        }
    }

}
