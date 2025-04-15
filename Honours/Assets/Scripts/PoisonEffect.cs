using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class PoisonEffect : MonoBehaviour
{
    public float poisonDuration = 4f;  // Duration of poison
    public float poisonTickInterval = 1f;  // Time between poison ticks
    public int poisonDamagePerTick = 1;  // Damage applied per tick 

    IDamageable target;

    void Start()
    {
        // Get the IDamageable component from the target
        target = GetComponent<IDamageable>();

        // If the target is valid, start applying poison
        if (target != null)
        {
            StartCoroutine(ApplyPoisonDamage());
        }
    }

    IEnumerator ApplyPoisonDamage()
    {
        float elapsed = 0f;

        // Apply poison damage at regular intervals
        while (elapsed < poisonDuration)
        {
            if (target != null)
            {
                target.Damage(poisonDamagePerTick);  // Apply damage per tick
                Debug.Log("Poison damage applied: " + poisonDamagePerTick);
            }

            // Wait for the next tick
            yield return new WaitForSeconds(poisonTickInterval);
            elapsed += poisonTickInterval;
        }
        // Destroy the PoisonEffect component
        Destroy(this);

        Debug.Log("Poison effect ended.");
    }
}
