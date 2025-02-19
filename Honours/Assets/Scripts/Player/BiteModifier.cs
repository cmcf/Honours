using System.Collections;
using UnityEngine;
using static Damage;

[CreateAssetMenu(fileName = "NewBiteModifier", menuName = "BiteModifiers/New Modifier")]
public class BiteModifier : ScriptableObject
{
    public string effectName;
    public float duration; // Total duration of the effect
    public float tickInterval = 2f; // How often damage is applied
    public float damagePerTick;
    public GameObject effectAnimationPrefab;

    public void ApplyEffect(IDamageable target, MonoBehaviour caller)
    {
        if (target is MonoBehaviour enemy)
        {
            caller.StartCoroutine(ApplyEffectCoroutine(target, enemy));
        }
    }

    private IEnumerator ApplyEffectCoroutine(IDamageable target, MonoBehaviour enemy)
    {
        Transform enemyTransform = enemy.transform;
        GameObject effectInstance = null;

        // Spawn visual effect if available
        if (effectAnimationPrefab != null)
        {
            effectInstance = Instantiate(effectAnimationPrefab, enemyTransform.position, Quaternion.identity, enemyTransform);
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (enemy == null)
            {
                break;
            }

            // Apply damage every tickInterval
            target.Damage(damagePerTick);

            yield return new WaitForSeconds(tickInterval);
            elapsedTime += tickInterval;

            target.Damage(damagePerTick);
        }

        // Destroy visual effect when done
        if (effectInstance != null)
        {
            Destroy(effectInstance);
        }
    }
}
