using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    Rigidbody2D rb;

    public float lifetime = 3f;
    public int damage;
    float pushbackDuration = 0.3f;
    float pushbackAmount = 0.4f;

    public float deflectionForce = 10f;

    void Start()
    {
        damage = Random.Range(1, 2);
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); 
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Deal damage to the enemy
                enemy.Damage(damage);

                // Apply knockback
                StartCoroutine(PushbackEnemy(enemy.transform, transform.position));

            }
        }

        if (other.CompareTag("Shield")) 
        {
            DestroyShield(other.gameObject); // Destroy the shield
        }

        else if (other.CompareTag("EnemyProjectile"))
        {
            // Handle deflection of enemy projectiles
            DeflectProjectile(other);
        }
    }

    public void DestroyShield(GameObject shield)
    {

        Panther panther = FindObjectOfType<Panther>();

        if (panther != null)
        {
            panther.OnShieldDestroyed(shield); // Notify Panther that the shield is destroyed
        }

        Destroy(shield); // Destroy the shield GameObject
    }

    IEnumerator PushbackEnemy(Transform enemyTransform, Vector3 knifePosition)
    {
        Rigidbody2D enemyRb = enemyTransform.GetComponent<Rigidbody2D>();
        if (enemyRb == null) yield break;

        Vector2 originalPosition = enemyTransform.position;
        Vector2 pushbackDirection = (enemyTransform.position - knifePosition).normalized;

        // Add slight random variation to direction
        pushbackDirection += new Vector2(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
        pushbackDirection.Normalize();

        // Raycast to prevent passing through walls
        float maxDistance = pushbackAmount;
        RaycastHit2D hit = Physics2D.Raycast(enemyTransform.position, pushbackDirection, maxDistance, LayerMask.GetMask("Wall"));
        if (hit.collider != null)
        {
            // Stop slightly before the wall
            maxDistance = hit.distance - 0.05f; 
        }

        Vector2 targetPosition = originalPosition + pushbackDirection * maxDistance;
        float elapsedTime = 0f;
        float smoothFactor = 2f; 

        // Reduce pushback over time 
        while (elapsedTime < pushbackDuration)
        {
            float t = elapsedTime / pushbackDuration;

            // Improved smoothness: cubic easing-out
            t = 1 - Mathf.Pow(1 - t, 3);

            Vector2 newPosition = Vector2.Lerp(originalPosition, targetPosition, t);
            if (enemyRb != null)
            {
                enemyRb.MovePosition(newPosition);
            }
            
            elapsedTime += Time.fixedDeltaTime / smoothFactor;
            yield return new WaitForFixedUpdate();
        }

        if (enemyRb != null)
        {
            // Ensure final position is correct
            enemyRb.MovePosition(targetPosition);
        }
            
    }

    void DeflectProjectile(Collider2D projectile)
    {
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            // Get the direction to deflect the projectile
            Vector2 deflectionDirection = (projectile.transform.position - transform.position).normalized;

            // Apply force to deflect the projectile
            projectileRb.velocity = deflectionDirection * deflectionForce;

            Destroy(projectile.gameObject);
        }
    }

    public void Fire(Vector3 direction, float speed)
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = direction.normalized * speed;
        }
    }
}
