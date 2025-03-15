using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class Hazard : MonoBehaviour
{
    [SerializeField] float minDamage = 3f;
    [SerializeField] float maxDamage = 6f;
    float damageAmount;
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
