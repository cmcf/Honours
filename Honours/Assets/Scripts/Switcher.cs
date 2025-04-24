using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterInfo
{
    public GameObject character;
}

public enum CharacterState
{
    Player,
    Wolf,
}

public class Switcher : MonoBehaviour
{
    public PlayerInput playerInput; // Reference to PlayerInput component
    public CinemachineVirtualCamera virtualCamera;

    [SerializeField] Slider wolfSlider;

    public List<CharacterInfo> characters = new List<CharacterInfo>();
    int currentCharacterIndex = 0;
    Vector3 previousCharacterPosition = Vector3.zero; // Store the previous character's position
    public  CharacterState currentCharacterState;

    [SerializeField] float switchVFXDuration = 0.2f;

    public GameObject wolfObject;

    // Reference to the GameObject with the player movement script
    public GameObject playerObject;
    public Transform playerTransform;


    float respawnDelay = 0.66f;

    public bool canSwitch = true;
    public bool isSwitching = false;

    public bool canMovePlayer = true;
    public bool canMoveWolf = false;

    public int currentSpawnPointIndex = 0;

    public float wolfFormDuration = 10f; // seconds allowed in wolf form
    public float wolfCooldownTime = 15f; // cooldown after using wolf

    float currentWolfTime = 0f;
    float currentCooldown = 0f;
    bool wolfOnCooldown = false;


    public void UpdateSpawnIndex(int newIndex)
    {
        currentSpawnPointIndex = newIndex;
    }

    public void Respawn()
    {
        playerTransform.position = playerTransform.localPosition;
    }

    void Start()
    {
        // Ensure the first character is active
        if (currentCharacterIndex == 0 && currentCharacterState != CharacterState.Player)
        {
            SwitchCharacter(currentCharacterIndex);
        }

        Respawn();

        PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.canMovePlayer = canMovePlayer;
        }

        Wolf wolfMovement = wolfObject.GetComponent<Wolf>();
        if (wolfMovement != null)
        {
            wolfMovement.canMoveWolf = canMoveWolf;
        }

        playerInput = GetComponentInChildren<PlayerInput>();
        playerInput.currentActionMap.Enable();
        playerInput.actions["Switch"].performed += ctx => OnSwitch(ctx);

        if (DifficultyManager.Instance != null) 
        {
            // Switch cooldown is increase and duration is decreased on hard mode
            if (DifficultyManager.Instance.IsHardMode())
            {
                wolfCooldownTime += 5f;
                wolfFormDuration -= 2f;
            }
        }
        
    }


    void Update()
    {
        // Handle input for the current character
        if (currentCharacterState == CharacterState.Player && !isSwitching)
        {
            // Handle input for the player character
            canMoveWolf = true;

        }
        else if (currentCharacterState == CharacterState.Wolf && !isSwitching)
        {
            // Handle input for the wolf character
            canMovePlayer = true;

        }
        // Handles wolf cooldown and UI values

        if (currentCharacterState == CharacterState.Wolf)
        {
            currentWolfTime += Time.deltaTime;
            float timeLeft = Mathf.Clamp(wolfFormDuration - currentWolfTime, 0, wolfFormDuration);

            wolfSlider.gameObject.SetActive(true);
            wolfSlider.maxValue = wolfFormDuration;
            wolfSlider.value = timeLeft;

            if (currentWolfTime >= wolfFormDuration)
            {
                ForceSwitchToPlayer(); 
            }
        }
        else if (wolfOnCooldown)
        {
            currentCooldown -= Time.deltaTime;
            float fillAmount = Mathf.Clamp(wolfCooldownTime - currentCooldown, 0, wolfCooldownTime);

            wolfSlider.gameObject.SetActive(true);
            wolfSlider.maxValue = wolfCooldownTime;
            wolfSlider.value = fillAmount;

            if (currentCooldown <= 0f)
            {
                wolfOnCooldown = false;
            }
        }
        else
        {
            // Wolf ability ready
            wolfSlider.gameObject.SetActive(true);
            wolfSlider.maxValue = 1f;
            wolfSlider.value = 1f;
        }
    }
    void OnSwitch(InputAction.CallbackContext context)
    {
        if (!canSwitch || isSwitching || (currentCharacterState == CharacterState.Player && wolfOnCooldown))
            return; // Prevent switching if cooldown has not passed

        // Disable movement for the current character during switching
        canMovePlayer = false;
        canMoveWolf = false;

        // Play switch SFX
        AudioManager.Instance.PlaySFX(AudioManager.Instance.switchSFX);

        // Store the position of the currently equipped character before deactivating them
        CharacterInfo currentCharacter = characters[currentCharacterIndex];
        previousCharacterPosition = currentCharacter.character.transform.position;

        // Deactivate the currently equipped character
        currentCharacter.character.SetActive(false);

        // Switch to the next character
        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Count;

        // Activate the new character and set their position based on the previous character's position
        SwitchCharacter(currentCharacterIndex);
    }



    void SwitchCharacter(int characterIndex)
    {
        canSwitch = false;
        isSwitching = true;

        Rigidbody2D playerRigidbody = playerObject.GetComponentInChildren<Rigidbody2D>();
        Rigidbody2D wolfRigidbody = wolfObject.GetComponent<Rigidbody2D>();

        // Stop previous and current character movement
        if (currentCharacterState == CharacterState.Player)
        {
            currentWolfTime = 0f; // Reset wolf duration
            PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
            playerMovement.DisableInput();

            wolfRigidbody.velocity = Vector2.zero;
            playerRigidbody.velocity = Vector2.zero;
        }
        else if (currentCharacterState == CharacterState.Wolf)
        {
            // Disable wolf input and stop movement
            Wolf wolfMovement = wolfObject.GetComponent<Wolf>();
            wolfMovement.DisableInput();

            currentCooldown = wolfCooldownTime;
            wolfOnCooldown = true;

            // Destroy wolf's orbiting knives when switching to player
            wolfMovement.DestroyKnives();  

            // Reset velocities
            playerRigidbody.velocity = Vector2.zero;
            wolfRigidbody.velocity = Vector2.zero;
        }

        // Change the current character state
        currentCharacterState = (currentCharacterState == CharacterState.Player) ?
        CharacterState.Wolf : CharacterState.Player;

        if (characterIndex >= 0 && characterIndex < characters.Count)
        {
            CharacterInfo characterInfo = characters[characterIndex];
            characterInfo.character.SetActive(true);

            // Set the position of the character based on the previous character's position
            characterInfo.character.transform.position = previousCharacterPosition;

            // Update the playerTransform reference to the new character's transform
            playerTransform = characterInfo.character.transform;

            playerRigidbody.velocity = Vector2.zero;
            wolfRigidbody.velocity = Vector2.zero;

            // Switch the camera to follow the new character
            SwitchCameraFollow(characterInfo.character.transform);

            Invoke("EnableSwitch", 1f);  
        }
    }

    void ForceSwitchToPlayer()
    {
        if (currentCharacterState != CharacterState.Wolf) return;

        // Deactivate Wolf
        wolfObject.SetActive(false);
        currentCharacterIndex = characters.FindIndex(c => c.character == playerObject);
        currentCharacterState = CharacterState.Player;

        // Activate Player
        playerObject.SetActive(true);
        playerTransform = playerObject.transform;
        playerTransform.position = wolfObject.transform.position;

        // Camera follow
        SwitchCameraFollow(playerTransform);

        currentCooldown = wolfCooldownTime;
        wolfOnCooldown = true;
        currentWolfTime = 0f;
    }


    void SwitchCameraFollow(Transform newTarget)
    {
        // Update camera to follow the new character
        if (virtualCamera != null)
        {
            virtualCamera.Follow = newTarget; 
        }
    }


    void EnableSwitch()
    {
        canSwitch = true;
        isSwitching = false;
    }

    public void EnableActiveCharacter()
    {
        if (currentCharacterState == CharacterState.Player)
        {
            playerObject.SetActive(true);
            ResetCharacterMovement(playerObject);
        }
        else if (currentCharacterState == CharacterState.Wolf)
        {
            wolfObject.SetActive(true);
            ResetCharacterMovement(wolfObject);
        }

        Invoke("UnlockSwitching", 1f);
    }


    void ResetCharacterMovement(GameObject character)
    {
        Rigidbody2D rb = character.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // Stop all movement
            rb.angularVelocity = 0f;
        }

        DisableInputTemporarily();
    }

    public void DisableInputTemporarily()
    {
        canSwitch = false;
        PlayerInput playerInput = GetComponentInChildren<PlayerInput>(); 
        if (playerInput != null)
        {
            playerInput.enabled = false; // Disables input
        }
        Invoke(nameof(ReenableInput), 0.5f); 
    }

    void ReenableInput()
    {
        PlayerInput playerInput = GetComponentInChildren<PlayerInput>(); 
        if (playerInput != null)
        {
            playerInput.enabled = true; 
        }
    }


    public void DisableActiveCharacter()
    {
        canSwitch = false;

        if (currentCharacterState == CharacterState.Player)
        {
            ResetCharacterMovement(playerObject);
            playerObject.SetActive(false);
        }
        else if (currentCharacterState == CharacterState.Wolf)
        {
            ResetCharacterMovement(wolfObject);
            wolfObject.SetActive(false);
        }
    }



    void UnlockSwitching()
    {
        canSwitch = true;
    }

}
