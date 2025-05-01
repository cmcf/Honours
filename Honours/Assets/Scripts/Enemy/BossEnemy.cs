using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    public Animator currentAnimator;
    public BossFormManager formManager;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;

    public GameObject floatingTextPrefab;
    Rigidbody2D activeRb;
    public float maxHealth = 45f;
    public float currentHealth;
    bool hit = false;

    public EnemyState currentState;

    void Awake()
    {
        formManager = GetComponent<BossFormManager>();

        // Increase health in Hard Mode
        if (DifficultyManager.Instance != null && DifficultyManager.Instance.IsHardMode())
        {
            maxHealth += 50f;
        }

        currentHealth = maxHealth;
    }

    void Start()
    {
       currentAnimator = formManager.GetCurrentAnimator();
       spriteRenderer = GetComponent<SpriteRenderer>();
       rb = GetComponent<Rigidbody2D>();
    }
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public void Damage(int damage)
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

            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyHit);

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
        Invoke("DestroyGameObject", 0.4f);
    }

    void DestroyGameObject()
    {
        Destroy(gameObject);
        PlayerHealth player = FindObjectOfType<PlayerHealth>();
        player.WinGame();
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
