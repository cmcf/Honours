using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class EnemyBullet : MonoBehaviour
{
    float bulletLife = 10f;
    [SerializeField] float damageAmount = 10;

    bool hitPlayer = false;
    void Start()
    {
        Destroy(gameObject, bulletLife);
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
