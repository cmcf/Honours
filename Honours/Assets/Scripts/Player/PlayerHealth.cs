using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public GameObject floatingTextPrefab;
    public GameObject playerObject;
    public GameObject wolfObject;

    public float maxHealth = 50f;
    public  float currentHealth;
    private bool isDead = false;

    private Player player;
    private Wolf wolf;
    CharacterState currentCharacterState;
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
        ShowFloatingText(amount, Color.red);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void ShowFloatingText(float amount, Color textColour)
    {
        if (floatingTextPrefab != null)
        {
            // Get the correct character's position based on the current character state
            GameObject activeCharacter = null;

            if (currentCharacterState == CharacterState.Player)
            {
                activeCharacter = playerObject;
            }
            else if (currentCharacterState == CharacterState.Wolf)
            {
                activeCharacter = wolfObject;
            }

            if (activeCharacter != null)
            {
                // Spawn the text slightly in front of the active character
                Vector3 spawnPosition = activeCharacter.transform.position + new Vector3(0f, 0f, 2f);

                // Instantiate the floating text prefab
                GameObject textInstance = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity);

                // Set the text instance to be in world space
                textInstance.transform.SetParent(null);

                // Set the text value
                var floatingTextComponent = textInstance.GetComponentInChildren<FloatingText>();

                Destroy(textInstance, 0.5f);

                if (floatingTextComponent != null)
                {
                    floatingTextComponent.SetText(amount.ToString(), textColour);
                }
            }
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        ShowFloatingText(amount, Color.green);
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
