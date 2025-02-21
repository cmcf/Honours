using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    Rigidbody2D rb;

    public float lifetime = 3f;
    public int damage;
    float pushbackDuration = 0.4f;
    float pushbackAmount = 0.5f;

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
    }

    IEnumerator PushbackEnemy(Transform enemyTransform, Vector3 knifePosition)
    {
        // Store the original position and scale of the enemy
        Vector3 originalPosition = enemyTransform.position;
        Vector3 originalScale = enemyTransform.localScale;

        // Calculate the direction of pushback
        Vector3 pushbackDirection = (enemyTransform.position - knifePosition).normalized;

        // Calculate the target position for pushback
        Vector3 targetPosition = enemyTransform.position + pushbackDirection * pushbackAmount;

        // Initialize time elapsed
        float elapsedTime = 0f;

        // Smoothly move the enemy back over the specified duration
        while (elapsedTime < pushbackDuration)
        {
            // Calculate interpolation factor
            float t = elapsedTime / pushbackDuration;

            if (enemyTransform != null)
            {
                // Interpolate position
                enemyTransform.position = Vector3.Lerp(originalPosition, targetPosition, t);

                // Ensure scale remains unchanged
                enemyTransform.localScale = originalScale;

                // Increment time elapsed
                elapsedTime += Time.deltaTime;
            }

            yield return null;
        }

        // Make sure the enemy ends at the final position with the original scale
        enemyTransform.position = targetPosition;
        enemyTransform.localScale = originalScale;
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
