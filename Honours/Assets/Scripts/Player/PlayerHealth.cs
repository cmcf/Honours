using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public GameObject floatingTextPrefab;
    public TextMeshProUGUI healthText;
    public Slider healthSlider;
    public GameObject winPanel;

    public float maxHealth = 50f;
    public float currentHealth;

    public bool inTutorial = false;
    bool isDead = false;

    Player player;
    Wolf wolf;
    CharacterState currentCharacterState;

    [SerializeField] float reloadDelay = 0.8f;
 

    void Awake()
    {
        currentHealth = maxHealth;

        Cursor.visible = false;
    }

    void Start()
    {
        player = FindObjectOfType<Player>();
        wolf = FindObjectOfType<Wolf>();
        UpdateHealthText();
    }

    void UpdateHealthText()
    {
        // Clamp health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Update text
        if (healthText != null)
        {
            healthText.text = Mathf.CeilToInt(currentHealth) + " / " + Mathf.CeilToInt(maxHealth);
        }

        // Disable win panel 
        if (winPanel!= null)
        {
            winPanel.SetActive(false);
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }


    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.RegisterDamageTaken(amount);
        }
        
        ShowFloatingText(amount, Color.red);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.playerHit);
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

        AudioManager.Instance.PlaySFX(AudioManager.Instance.playerDeath);

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
        if (!inTutorial)
        {
            DifficultyManager.Instance.ResetDifficultyLevel();
        }
        

        Invoke("ReloadGame", reloadDelay);
    }

    public void ReloadGame()
    {
        if (inTutorial)
        {
            SceneManager.LoadScene("Tutorial");
        }
        else
        {
            SceneManager.LoadScene("GameMain");
        }
        
    }

    public void WinGame()
    {
        winPanel.SetActive(true);
        Cursor.visible = true;
        // Disable player and wolf control
        DisableCharacterControl();
    }

    void DisableCharacterControl()
    {
        if (player != null)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            PlayerAim playerAim = player.GetComponent<PlayerAim>();

            if (playerMovement != null) playerMovement.enabled = false;
            if (playerAim != null) playerAim.enabled = false;
        }

        if (wolf != null)
        {
            Wolf wolfMovement = wolf.GetComponent<Wolf>();
            if (wolfMovement != null) wolfMovement.enabled = false;
        }

        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false; // Disables input
        }
    }
}
