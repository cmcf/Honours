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
    [SerializeField] float projectileLifetime = 1.8f;

    bool hitPlayer = false;
    int damageAmount;

    [SerializeField] float homingDuration = 0.5f;
    float homingTimer;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        damageAmount = Random.Range(minDamage, maxDamage);
        homingTimer = homingDuration;
        StartCoroutine(ShrinkOverTime(projectileLifetime));

        if (DifficultyManager.Instance.IsHardMode())
        {
            speed += 0.5f;
            minDamage += 2;
            homingDuration += 0.5f;
        }
    }

    void FixedUpdate()
    {
        if (target == null) return;

        if (homingTimer > 0f)
        {
            homingTimer -= Time.fixedDeltaTime;
            // // Adjust rotation
            Vector2 toTarget = (target.position - transform.position).normalized;

            // Calculate direction
            float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            Quaternion desiredRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, rotationSpeed * Time.fixedDeltaTime);

        }

        // Move in the direction the projectile is currently facing
        Vector2 forward = transform.right;
        rb.velocity = forward * speed;
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

    IEnumerator ShrinkOverTime(float duration)
    {
        // Projectie reduces in size over time
        Vector3 originalScale = transform.localScale;
        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
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
