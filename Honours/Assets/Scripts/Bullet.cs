using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class Bullet : MonoBehaviour
{
    [SerializeField] float damageAmount;

    void Start()
    {
        Destroy(gameObject, 8f);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Deal damage to the enemy
                damageable.Damage(damageAmount);
            }
        }
    }

    public void SetDamage(float newDamage)
    {
        damageAmount = newDamage;
    }
}
