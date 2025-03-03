using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class BossEnemy : MonoBehaviour
{
    public Animator currentAnimator;
    public BossFormManager formManager;
    Rigidbody2D activeRb;
    public float maxHealth = 45f;
    public float currentHealth;
    bool hit = false;

    public EnemyState currentState;

    void Awake()
    {
        formManager = GetComponent<BossFormManager>();
        currentHealth = maxHealth;
    }

    void Start()
    {
       currentAnimator = formManager.GetCurrentAnimator();
    }
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public void Damage(float damage)
    {
        Debug.Log("Damage Received: " + damage); // Debug message
        if (!hit)
        {
            // Decrease health
            currentHealth -= damage;
            PlayHitEffect();
            if (currentHealth <= 0)
            {
                Die();
            }

            hit = true;
            StartCoroutine(ResetHitFlag());
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
}
