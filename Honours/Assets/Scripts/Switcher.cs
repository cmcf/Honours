using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public List<CharacterInfo> characters = new List<CharacterInfo>();
    private int currentCharacterIndex = 0;
    private Vector3 previousCharacterPosition = Vector3.zero; // Store the previous character's position
    private CharacterState currentCharacterState;

    [SerializeField] float switchVFXDuration = 0.2f;

    public GameObject wolfObject;

    // Reference to the GameObject with the player movement script
    public GameObject playerObject;
    public Transform playerTransform;


    float respawnDelay = 0.66f;

    private bool canSwitch = true;
    public bool isSwitching = false;

    public bool canMovePlayer = true;
    public bool canMoveWolf = false;

    public int currentSpawnPointIndex = 0;


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

        // Initialize by activating the first character
        SwitchCharacter(currentCharacterIndex);
        Respawn();

        // Find and store the player movement script component
        PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            // Access to the the canMovePlayer variable
            playerMovement.canMovePlayer = canMovePlayer;
        }

        // Find and store the dog movement script component
        Wolf wolfMovement = wolfObject.GetComponent<Wolf>();
        if (wolfMovement != null)
        {
            wolfMovement.canMoveWolf = canMoveWolf;
        }

        playerInput = GetComponentInChildren<PlayerInput>();
        playerInput.currentActionMap.Enable();


        // Subscribe to the Switch input action
        playerInput.actions["Switch"].performed += ctx => OnSwitch(ctx);
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
            // Handle input for the dog character
            canMovePlayer = true;

        }
    }
    void OnSwitch(InputAction.CallbackContext context)
    {
        // Disable movement for the current character during switching
        canMovePlayer = false;
        canMoveWolf = false;

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
            PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
            playerMovement.DisableInput();

            wolfRigidbody.velocity = Vector2.zero;
            playerRigidbody.velocity = Vector2.zero;

        }
        else if (currentCharacterState == CharacterState.Wolf)
        {
            // Resets move input 
            Wolf wolfMovement = wolfObject.GetComponent<Wolf>();
            wolfMovement.DisableInput();
            // Resets velocity 
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
            Invoke("EnableSwitch", 1f);

        }
    }


    void EnableSwitch()
    {
        canSwitch = true;
        isSwitching = false;
    }

}
