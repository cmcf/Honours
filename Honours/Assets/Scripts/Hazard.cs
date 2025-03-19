using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class Hazard : MonoBehaviour
{
    [SerializeField] int minDamage = 3;
    [SerializeField] int maxDamage = 6;
    int damageAmount;
    PlayerHealth playerHealth;
    void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        damageAmount = Random.Range(minDamage, maxDamage);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerHealth.TakeDamage(damageAmount);
            
        }
    }
}
