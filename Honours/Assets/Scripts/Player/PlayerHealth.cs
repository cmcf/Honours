using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] float currentHealth;
    private bool isDead = false;

    private Player player;
    private Wolf wolf;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        player = FindObjectOfType<Player>();
        wolf = FindObjectOfType<Wolf>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Reference the Switcher script to check the current character state
        Switcher switcher = FindObjectOfType<Switcher>();
        if (switcher == null)
        {
            return;
        }

        // Check if the player or the wolf is currently active
        if (switcher.currentCharacterState == CharacterState.Player)
        {
            if (switcher.playerObject != null)
            {
                PlayerMovement playerMovement = switcher.playerObject.GetComponent<PlayerMovement>();
                PlayerAim playerAim = switcher.playerObject.GetComponent<PlayerAim>();
                Animator playerAnimator = switcher.playerObject.GetComponent<Animator>();
                Rigidbody2D rb = switcher.playerObject.GetComponent<Rigidbody2D>();

                if (playerMovement != null) playerMovement.enabled = false;
                if (playerAim != null) playerAim.enabled = false;
                if (rb != null) rb.velocity = Vector2.zero;
                if (playerAnimator != null) playerAnimator.SetTrigger("isDead");
            }
        }
        else if (switcher.currentCharacterState == CharacterState.Wolf)
        {
            if (switcher.wolfObject != null)
            {
                Wolf wolfMovement = switcher.wolfObject.GetComponent<Wolf>();
                Animator wolfAnimator = switcher.wolfObject.GetComponent<Animator>();
                Rigidbody2D rb = switcher.wolfObject.GetComponent<Rigidbody2D>();

                if (wolfMovement != null) wolfMovement.enabled = false;
                if (rb != null) rb.velocity = Vector2.zero;
                if (wolfAnimator != null) wolfAnimator.SetTrigger("isDead");
            }
        }
    }

}
