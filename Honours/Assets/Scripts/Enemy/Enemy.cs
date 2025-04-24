using System.Collections;
using UnityEngine;
using static Damage;

public class Enemy : MonoBehaviour, IDamageable
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public GameObject floatingTextPrefab;

    public AudioClip deathSFX;

    public float maxHealth = 45f;
    public float currentHealth;
    public float freezeTimer;
    public int level;

    public bool isActive = true;
    bool hit = false;
    bool isFrozen = false;

    public EnemyState currentState = EnemyState.Idle;

    public event System.Action OnDeathEvent;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        int difficulty = Mathf.Max(DifficultyManager.Instance.currentDifficulty, 1);
        int healthMultiplier = 1 + (difficulty - 1);

        maxHealth *= healthMultiplier;

        if (DifficultyManager.Instance.IsHardMode())
        {
            maxHealth = Mathf.CeilToInt(maxHealth * 1.5f); // +10% boost
        }

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

    public void UpdateHealthScaling(float healthMultiplier)
    {
        currentHealth = maxHealth * healthMultiplier;
        Debug.Log($"{gameObject.name} Health Updated: {currentHealth}");
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

    public void Damage(int damage)
    {
        // Only deal damage if not already hit and the enemy is active
        if (!hit && isActive)
        {
            // Decrease health
            currentHealth -= damage;
            // Show amount of damage taken
            ShowFloatingText(damage, Color.white, gameObject);
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

            // Play hit effect if not hit with ice
            if (!isFrozen)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyHit);
            }

            // Allow hit after a short delay
            StartCoroutine(ResetHitFlag());

        }
      
    }

    void ShowFloatingText(float amount, Color textColour, GameObject target)
    {
        if (floatingTextPrefab != null)
        {
            // Spawn the text slightly in front of the target
            Vector3 spawnPosition = target.transform.position + new Vector3(0f, 0f, 2f);

            // Instantiate the floating text prefab
            GameObject textInstance = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity);

            // Set the text instance to be in world space
            textInstance.transform.SetParent(null);

            // Set the text value and colour
            var floatingTextComponent = textInstance.GetComponentInChildren<FloatingText>();

            if (floatingTextComponent != null)
            {
                floatingTextComponent.SetText(amount.ToString(), textColour);
            }

            Destroy(textInstance, 0.5f);
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
            AudioManager.Instance.PlaySFX(AudioManager.Instance.iceHitEffect);
            // Stop movement
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
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
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

    }

    void Die()
    {
        currentState = EnemyState.Dead;
        animator.SetBool("isHurt", false);
        // Start the death animation
        animator.SetTrigger("isDead");
        AudioManager.Instance.PlaySFX(deathSFX);
        OnDeathEvent?.Invoke();

        // Call a coroutine to delay destruction of the game object
        StartCoroutine(WaitForDeathAnimation());
    }

    IEnumerator WaitForDeathAnimation()
    {
        // Get the current death animation state
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Wait for the animation to finish
        yield return new WaitForSeconds(stateInfo.length);

        Destroy(rb);
        Destroy(gameObject);
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            rb.velocity = Vector2.zero;  // Stop movement
        }
    }

}
