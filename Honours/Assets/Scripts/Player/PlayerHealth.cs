using UnityEngine;
using TMPro;  // If using TextMeshPro
using UnityEngine.SceneManagement;
public class PlayerHealth : MonoBehaviour
{
    public GameObject floatingTextPrefab;
    public float maxHealth = 50f;
    public float currentHealth;
    private bool isDead = false;

    private Player player;
    private Wolf wolf;
    private CharacterState currentCharacterState;

    [SerializeField] float reloadDelay = 0.8f;

    // Reference to the UI Text element for health display
    public TextMeshProUGUI healthText;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        player = FindObjectOfType<Player>();
        wolf = FindObjectOfType<Wolf>();
        UpdateHealthText();
    }

    void UpdateHealthText()
    {
        // Update the health text to display current health
        healthText.text = "Health: " + currentHealth.ToString("0");
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        ShowFloatingText(amount, Color.red);
        UpdateHealthText();  // Update health text
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ShowFloatingText(float amount, Color textColour)
    {
        if (floatingTextPrefab != null)
        {
            Switcher switcher = FindObjectOfType<Switcher>();
            if (switcher == null)
            {
                return;
            }

            // Get the correct character's position based on the current character state
            GameObject activeCharacter = null;

            if (switcher.currentCharacterState == CharacterState.Player)
            {
                activeCharacter = switcher.playerObject;
            }
            else if (switcher.currentCharacterState == CharacterState.Wolf)
            {
                activeCharacter = switcher.wolfObject;
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
        UpdateHealthText();  // Update health text
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

        Invoke("ReloadGame", 0.8f);
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene("GameMain");
    }
}
