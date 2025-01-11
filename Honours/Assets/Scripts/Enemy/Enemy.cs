using System.Collections;
using UnityEngine;
using static Damage;

public class Enemy : MonoBehaviour, IDamageable
{
    protected Animator animator;
    public float Health { get; set; }
    public float maxHealth = 45f;
    public float currentHealth = 45f;

    bool isActive = false;
    bool hit = false;
    public void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
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
        
        if (isActive && !hit)
        {
            // Subtract damage by received damage
            currentHealth -= damage;  

            // Check if health goes below zero
            if (currentHealth < 0)
            {
                currentHealth = 0;
            }
            animator = GetComponent<Animator>();

            if (animator != null)
            {
                // Reset the "isHurt" trigger to ensure it can be re-triggered
                animator.ResetTrigger("isHurt");

                // Trigger the hurt animation
                animator.SetTrigger("isHurt");
            }
        
            // Enemy dies when health reaches 0
            if (currentHealth <= 0)
            {
                Die();
            }

            StartCoroutine(HitEffect());
            hit = true;
        }
    }

    IEnumerator HitEffect()
    {

        // Wait for a short time before  allowing the next hit
        yield return new WaitForSeconds(0.2f);

        // Reset the hit flag to allow the next damage to be applied
        hit = false;
    }

    void Die()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("isDead");
        }
        RoomController.Instance.StartCoroutine(RoomController.Instance.RoomCoroutine());
        //FindObjectOfType<EnemyManager>().OnEnemyDefeated();
        Invoke("DestroyEnemy", 0.5f);
    }

    void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}
