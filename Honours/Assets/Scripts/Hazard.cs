using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class Hazard : MonoBehaviour
{
    [SerializeField] int minDamage = 2;
    [SerializeField] int maxDamage = 6;
    float timeBetweenDamage = 1f; 
    int damageAmount;
    PlayerHealth playerHealth;
    Coroutine damageCoroutine;

    void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        damageAmount = Random.Range(minDamage, maxDamage);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Start the coroutine when the player enters the hazard
        if (collision.CompareTag("Player"))
        {
            if (damageCoroutine == null) 
            {
                damageCoroutine = StartCoroutine(ApplyDamageOverTime(collision));
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // Stop the coroutine when the player leaves the hazard
        if (collision.CompareTag("Player") && damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null; 
        }
    }

    IEnumerator ApplyDamageOverTime(Collider2D player)
    {
        while (true)
        {
            playerHealth.TakeDamage(damageAmount); 
            yield return new WaitForSeconds(timeBetweenDamage); 
        }
    }
}
