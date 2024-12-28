using System.Collections;
using UnityEngine;
using static Damage;

public class Enemy : MonoBehaviour, IDamageable
{
    public SpriteRenderer spriteRenderer;

    public float Health { get; set; }
    [SerializeField] float maxHealth = 45f;
    float currentHealth;

    bool isActive = false;
    bool hit = false;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;     
    }

    public void SetActiveState(bool active)
    {
        isActive = active;

        if (!isActive)
        {
            // Stop all movement or actions if inactive
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    public bool IsActive()
    {
        return isActive;
    }

    public void Damage(float damage)
    {
        // Only deals damage if damage has not already been dealt and is active
        if (!hit && isActive)
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
        RoomController.Instance.StartCoroutine(RoomController.Instance.RoomCoroutine());
        Debug.Log("Die");
        //FindObjectOfType<EnemyManager>().OnEnemyDefeated();
        Destroy(gameObject);
    }
}
