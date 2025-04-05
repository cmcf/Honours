using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using static Damage;

public class Bullet : MonoBehaviour
{
    Enemy enemy;
    Player player;
    int damageAmount;
    public float freezeDuration = 0.5f;
    public bool isIce = false;
    void Start()
    {
        player = FindObjectOfType<Player>();
        Destroy(gameObject, player.currentWeapon.bulletLifetime);
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
                ApplyIceEffect(collision);
            }


            // Destroy the bullet on impact
            Destroy(gameObject, 0.5f);
        }

        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    void ApplyIceEffect(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Freeze(freezeDuration);
            return;
        }

        BossEnemy bossEnemy = collision.GetComponent<BossEnemy>();
        if (bossEnemy != null)
        {
            bossEnemy.Freeze(freezeDuration);
        }
    }

    public void SetDamage(int newDamage)
    {
        damageAmount = newDamage;
    }
}
