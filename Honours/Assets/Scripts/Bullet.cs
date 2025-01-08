using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class Bullet : MonoBehaviour
{
    [SerializeField] float damageAmount;

    void Start()
    {
        Destroy(gameObject, 10f);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Check if the target has a damageable component
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Deal damage to the target
                damageable.Damage(damageAmount);
            }
        }
    }

    public void SetDamage(float newDamage)
    {
        damageAmount = newDamage;
    }
}
