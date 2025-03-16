using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class BossEnemy : MonoBehaviour
{
    public Animator currentAnimator;
    public BossFormManager formManager;
    SpriteRenderer spriteRenderer;

    public GameObject floatingTextPrefab;
    Rigidbody2D activeRb;
    public float maxHealth = 45f;
    public float currentHealth;
    bool hit = false;

    public EnemyState currentState;
    bool isFrozen = false;
    float freezeTimer;
    void Awake()
    {
        formManager = GetComponent<BossFormManager>();
        currentHealth = maxHealth;
    }

    void Start()
    {
       currentAnimator = formManager.GetCurrentAnimator();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public void Damage(float damage)
    {
        Panther panther = GetComponentInChildren<Panther>(); 

        if (panther != null && panther.shieldActive)
        {
            return; // Ignore damage if shield is active
        }

        if (!hit)
        {
            // Decrease health
            currentHealth -= damage;
            PlayHitEffect();
            ShowFloatingText(damage, Color.white);

            if (currentHealth <= 0)
            {
                Die();
            }
 
            hit = true;
            StartCoroutine(ResetHitFlag());
        }
    }


    void ShowFloatingText(float amount, Color textColour)
    {
        if (floatingTextPrefab != null)
        {
            // Get the correct character's position based on the current boss form
            GameObject activeCharacter = null;

            // Get the current active form's position
            if (formManager != null)
            {
                if (formManager.GetCurrentForm() == BossFormManager.BossForm.Firebird)
                {
                    activeCharacter = formManager.firebirdForm;
                }
                else if (formManager.GetCurrentForm() == BossFormManager.BossForm.Panther)
                {
                    activeCharacter = formManager.pantherForm;
                }
            }

            if (activeCharacter != null)
            {
                // Spawn the text slightly in front of the active character
                Vector3 spawnPosition = activeCharacter.transform.position + new Vector3(0f, 0.5f, 2f);

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
    }


    void Die()
    {
        // Get the animator from the active form and play hit animation
        currentAnimator = formManager.GetCurrentAnimator();
        currentState = EnemyState.Dead;
        currentAnimator.SetBool("isHurt", false);
        // Start the death animation
        currentAnimator.SetTrigger("isDead");


        // Call a coroutine to delay destruction of the game object
        StartCoroutine(WaitForDeathAnimation());
    }

    IEnumerator ResetHitFlag()
    {
        // Wait for a short period before allowing another hit
        yield return new WaitForSeconds(0.3f);
        hit = false;
    }

    IEnumerator WaitForDeathAnimation()
    {
        // Wait for the death animation to finish
        yield return new WaitForSeconds(0.6f);
        activeRb = formManager.GetCurrentRb();
        Destroy(activeRb);
        Destroy(gameObject);
    }


    void PlayHitEffect()
    {
        if (currentAnimator != null && currentState != EnemyState.Dead)
        {
            currentAnimator.SetBool("isHurt", true);
            Invoke("ResetHit", 0.2f);
        }

    }
    void ResetHit()
    {
        if (currentAnimator == null) { return; }
        currentAnimator.SetBool("isHurt", false);
    }

    public void Freeze(float duration)
    {
        if (!isFrozen)
        {
            isFrozen = true;
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
}
