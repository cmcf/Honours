using System.Collections;
using UnityEngine;
using static Damage;

[CreateAssetMenu(fileName = "NewBiteModifier", menuName = "BiteModifiers/New Modifier")]
public class BiteModifier : ScriptableObject
{
    public string effectName;
    public float duration; // Total duration of the effect
    public float tickInterval = 2f; // How often damage is applied
    public int damagePerTick;
    public Sprite biteSprite;
    public GameObject effectAnimationPrefab;

    float leechChance = 0.2f;

    // Health Leech Specific
    public bool isLeechEffect = false; 

    public void ApplyEffect(IDamageable target, MonoBehaviour caller)
    {
        if (target is MonoBehaviour enemy)
        {
            caller.StartCoroutine(ApplyEffectCoroutine(target, enemy, caller));
        }
    }

    IEnumerator ApplyEffectCoroutine(IDamageable target, MonoBehaviour enemy, MonoBehaviour caller)
    {
        Transform enemyTransform = enemy.transform;
        GameObject effectInstance = null;

        // Spawn visual effect if available
        if (effectAnimationPrefab != null)
        {
            effectInstance = Instantiate(effectAnimationPrefab, enemyTransform.position, Quaternion.identity, enemyTransform);
        }

        float elapsedTime = 0f;

        // Find PlayerHealth from GameManager at the start
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();

        while (elapsedTime < duration)
        {
            if (enemy == null || playerHealth == null)
            {
                break;
            }


            if (isLeechEffect)
            {
                // Chance-based healing
                if (Random.value <= leechChance)
                {
                    playerHealth.Heal(damagePerTick);
                }
            }

            yield return new WaitForSeconds(tickInterval);
            elapsedTime += tickInterval;
        }

        // Destroy visual effect when done
        if (effectInstance != null)
        {
            Destroy(effectInstance);
        }
    }

}
