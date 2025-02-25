using System.Collections;
using UnityEngine;
using static Damage;

public class Enemy : MonoBehaviour, IDamageable
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;

    public float maxHealth = 45f;
    public float currentHealth;
    public float moveSpeed;
    public float freezeTimer;
    public int level;

    public bool isActive = false;
    bool hit = false;
    bool isFrozen = false;
    float destroyDelay = 0.6f;

    public EnemyState currentState = EnemyState.Idle;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    // Method to change the state
    public void ChangeState(EnemyState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log($"State changed to: {currentState}");
            switch (currentState)
            {
                case EnemyState.Attacking:
                    break;
                case EnemyState.Appear:
                    break;
                case EnemyState.Idle:                    
                    break;
            }
        }
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
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

    public void SetDifficultyLevel(int difficulty)
    {
        level = Mathf.Min(difficulty, 4);
        // Enemy stats are adjusted based on difficulty setting
        switch (level)
        {
            case 1:
                moveSpeed += 0.1f;
                break;
            case 2:
                moveSpeed += 0.1f;
                break;
            case 3:
                moveSpeed += 0.2f;
                break;
            case 4:
                moveSpeed += 0.2f;
                break;

        }
    }

    public void Damage(float damage)
    {
        // Only deal damage if not already hit and the enemy is active
        if (!hit && isActive)
        {
            // Decrease health
            currentHealth -= damage;

            // Play hurt animation
            PlayHitEffect();

            // healthBar.UpdateHealthBar(currentHealth, maxHealth);

            // If health is zero or below, the enemy dies
            if (currentHealth <= 0)
            {
                isActive = false;
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
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezePosition;
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

    void PlayHitEffect()
    {
        if (animator != null && currentState != EnemyState.Dead)
        {
            animator.SetBool("isHurt", true);
            Invoke("ResetHit", 0.2f);
        }

    }

    void ResetHit()
    {
        if (animator == null) { return; }
        animator.SetBool("isHurt", false);
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

        // Remove position constraints 
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.None;  
        }

    }


    void Die()
    {
        currentState = EnemyState.Dead;
        animator.SetBool("isHurt", false);
        // Start the death animation
        animator.SetTrigger("isDead");
       

        // Call a coroutine to delay destruction of the game object
        StartCoroutine(WaitForDeathAnimation());
    }

    IEnumerator WaitForDeathAnimation()
    {
        // Wait for the death animation to finish
        yield return new WaitForSeconds(destroyDelay);

        Destroy(rb);
        Destroy(gameObject);
    }

}
