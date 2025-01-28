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
            // Decrease health
            currentHealth -= damage;

            // healthBar.UpdateHealthBar(currentHealth, maxHealth);

            // If health is zero or below, the enemy dies
            if (currentHealth <= 0)
            {
                Die();
            }

            // Mark as hit so that damage does not process more than once
            hit = true;

            // Allow hit after a short delay
            StartCoroutine(ResetHitFlag());

        }
      
    }

    IEnumerator ResetHitFlag()
    {
        // Wait for a short period before allowing another hit
        yield return new WaitForSeconds(0.3f);
        hit = false;
    }


    public void Freeze(float duration)
    {
        if (!isFrozen)
        {
            isFrozen = true;
            isActive = false;
            freezeTimer = duration;

            // Stop movement
            if (rb != null)
            {
                rb.velocity = Vector2.zero; 
            }

            // Freeze the animator by setting its speed to 0
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = 0;
            }

            // Set colour to cyan to indicate frozen state
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.cyan;
            }

            Invoke("Unfreeze", freezeTimer);
        }
    }

    void Unfreeze()
    {
        isFrozen = false;
        isActive = true;

        // Restore the colour to white to indicate the enemy is no longer frozen
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        // Restore animator speed
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 1;
        }

    }


    void Die()
    {
        RoomController.Instance.StartCoroutine(RoomController.Instance.RoomCoroutine());
        //FindObjectOfType<EnemyManager>().OnEnemyDefeated();
        Destroy(gameObject);
    }


}
