using System.Collections;
using UnityEngine;
using static Damage;

public class Enemy : MonoBehaviour, IDamageable
{
    public SpriteRenderer spriteRenderer;

    public float Health { get; set; }
    [SerializeField] float maxHealth = 45f;
    float currentHealth;

    bool hit = false;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;     
    }


    public void Damage(float damage)
    {
        // Only deals damage if damage has not already been dealt
        if (!hit)
        {
            currentHealth -= damage;

            //healthBar.UpdateHealthBar(currentHealth, maxHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
            // Change colour of enemy 
            StartCoroutine(HitEffect());
            hit = true;
        }
    }
    IEnumerator HitEffect()
    {
        // Change enemy colour
        spriteRenderer.color = Color.white;

        // Wait until delay has ended
        yield return new WaitForSeconds(0.2f);
        hit = false;

        // Revert enemy colour back to original
        spriteRenderer.color = Color.red;
    }



    void Die()
    {
        Debug.Log("Die");
        Destroy(gameObject);
    }
}
