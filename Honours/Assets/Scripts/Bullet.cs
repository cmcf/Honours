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
    public bool isPoison = false;

    [Header("Poison Settings")]
    public float poisonDuration = 4f;
    public float poisonTickInterval = 1f;
    public int poisonDamagePerTick = 1;
    public GameObject poisonEffectPrefab;
    void Start()
    {
        player = FindObjectOfType<Player>();
        Destroy(gameObject, player.currentWeapon.baseBulletLifetime);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Check if the target has a damageable component
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Deal immediate damage to the enemy
                damageable.Damage(damageAmount);
            }
            // If the bullet is ice, apply the ice effect

            if (isIce)
            {
                ApplyIceEffect(collision);
            }

            // If the bullet is poison, apply the poison effect
            if (isPoison)
            {
                ApplyPoisonEffect(collision);
            }

            // Destroy the bullet on impact
            Destroy(gameObject, 0.5f);
        }

        // Destroy bullet if it hits a wall
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    void ApplyPoisonEffect(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Add effect component to the enemy
            PoisonEffect poisonEffect = collision.gameObject.AddComponent<PoisonEffect>();
            poisonEffect.poisonDuration = poisonDuration;
            poisonEffect.poisonTickInterval = poisonTickInterval;
            poisonEffect.poisonDamagePerTick = poisonDamagePerTick;

            // Instantiate a visual effect if provided and make it follow the target
            if (poisonEffectPrefab != null)
            {
                GameObject poisonVisualEffect = Instantiate(poisonEffectPrefab, collision.transform.position, Quaternion.identity);
                poisonVisualEffect.transform.SetParent(collision.transform);  
                // Visual effect ends when the poison duration has passed
                Destroy(poisonVisualEffect, poisonDuration);
            }
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

        Panther pantherEnemy = collision.GetComponent<Panther>();
        if (pantherEnemy != null)
        {
            pantherEnemy.Freeze(freezeDuration);
        }

        Firebird firebirdEnemy = collision.GetComponent<Firebird>();
        if (firebirdEnemy != null)
        {
            firebirdEnemy.Freeze(freezeDuration);
        }
    }

    public void SetDamage(int newDamage)
    {
        damageAmount = newDamage;
    }
}
