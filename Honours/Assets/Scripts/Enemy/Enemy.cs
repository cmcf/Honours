using System.Collections;
using UnityEngine;
using static Damage;

public class Enemy : MonoBehaviour, IDamageable
{
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    Color originalColour;
    public float Health { get; set; }
    public float maxHealth = 45f;
    public float currentHealth;
    float freezeTimer;

    bool isActive = false;
    bool hit = false;
    bool isFrozen = false;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColour = GetComponent<SpriteRenderer>().color;
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    public void SetActiveState(bool active)
    {
        isActive = active;

        if (!isActive)
        {
            // Stop all movement or actions if inactive
           
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
        // Only deal damage if not already hit and the enemy is active
        if (!hit && isActive)
        {
            Debug.Log("Current Health: " + currentHealth);
            // Debug the incoming damage
            Debug.Log("Damage taken: " + damage);
            

            // Decrease health and make sure it doesn't go below 0
            currentHealth -= damage;

            Debug.Log("Current Health: " + currentHealth);

            // healthBar.UpdateHealthBar(currentHealth, maxHealth);

            // If health is zero or below, the enemy dies
            if (currentHealth <= 0)
            {
                Die();
            }

            // Mark as hit so that we don't process damage again until reset
            hit = true;

            // Reset the 'hit' flag after a small delay (e.g., 1 second)
            StartCoroutine(ResetHitFlag());

        }
      
    }

    // Coroutine to reset the 'hit' flag after a delay
    private IEnumerator ResetHitFlag()
    {
        // Wait for a short period before allowing another hit
        yield return new WaitForSeconds(0.5f);  // Adjust the delay as necessary
        hit = false;
    }


    public void Freeze(float duration)
    {
        if (!isFrozen)
        {
            isFrozen = true;
            freezeTimer = duration;
            // Ensure rb is assigned before using it
            if (rb != null)
            {
                rb.velocity = Vector2.zero; // Stop movement
            }
            else
            {
                Debug.LogWarning("Rigidbody2D not found on " + gameObject.name);
            }
            GetComponent<SpriteRenderer>().color = Color.cyan; // Change colour to indicate freezing
            Invoke("Unfreeze", freezeTimer);
        }
    }

    void Unfreeze()
    {
        isFrozen = false;
        GetComponent<SpriteRenderer>().color = originalColour; // Reset colour
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
        //FindObjectOfType<EnemyManager>().OnEnemyDefeated();
        Destroy(gameObject);
    }
}
