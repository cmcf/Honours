using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using static Damage;

public class Bullet : MonoBehaviour
{
    Enemy enemy;
    [SerializeField] float damageAmount;
    public float freezeDuration = 0.5f;
    public bool isIce = false;
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
            if (isIce)
            {
                enemy = collision.GetComponent<Enemy>();
                ApplyIceEffect(enemy);
            }


            // Destroy the bullet on impact
            Destroy(gameObject, 0.5f);
        }
    }

    void ApplyIceEffect(Enemy enemy)
    {
        enemy.Freeze(freezeDuration);
    }

    public void SetDamage(float newDamage)
    {
        damageAmount = newDamage;
    }
}
