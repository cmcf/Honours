using System.Collections;
using UnityEngine;
using static Damage;

[CreateAssetMenu(fileName = "NewBiteModifier", menuName = "BiteModifiers/New Modifier")]
public class BiteModifier : ScriptableObject
{
    public string effectName;
    public float duration;
    public float tickInterval;
    public float damagePerTick;
    public Color effectColor = Color.white; 

    public void ApplyEffect(IDamageable target, MonoBehaviour caller)
    {
        if (target is MonoBehaviour enemy)
        {
            caller.StartCoroutine(ApplyEffectCoroutine(target, enemy));
        }
    }

    private IEnumerator ApplyEffectCoroutine(IDamageable target, MonoBehaviour enemy)
    {
        SpriteRenderer sprite = enemy.GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            target.Damage(damagePerTick);

            // Flash effect: briefly change color
            sprite.color = effectColor;
            yield return new WaitForSeconds(0.1f);
            sprite.color = originalColor;

            elapsedTime += tickInterval;
        }
    }
}
