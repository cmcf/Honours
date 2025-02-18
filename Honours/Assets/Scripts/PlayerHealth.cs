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

        // Disable movement and aiming
        if (player != null)
        {
            player.GetComponent<PlayerMovement>().enabled = false;
            player.GetComponent<PlayerAim>().enabled = false;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
            player.GetComponent<Animator>().SetTrigger("isDead");
        }

        if (wolf != null)
        {
            wolf.GetComponent<PlayerMovement>().enabled = false;

            Rigidbody2D rb = wolf.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;

            wolf.GetComponent<Animator>().SetTrigger("isDead");
        }
    }
}
