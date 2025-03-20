using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BitePickup : MonoBehaviour
{
    public BiteModifier[] possibleModifiers;
    SpriteRenderer spriteRenderer;
    BiteModifier selectedBiteModifier;

    public GameObject promptPrefab; // Assign in Inspector
    private GameObject promptInstance;

    bool playerInRange = false;
    InputAction interactAction;

    void Start()
    {
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        interactAction = playerInput.actions["Interact"];

        selectedBiteModifier = possibleModifiers[Random.Range(0, possibleModifiers.Length)];
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = selectedBiteModifier.biteSprite;
    }

    void Update()
    {
        if (playerInRange && interactAction.triggered)
        {
            Switcher switcher = FindObjectOfType<Switcher>();
            if (switcher != null && switcher.currentCharacterState == CharacterState.Wolf)
            {
                Wolf wolf = FindObjectOfType<Wolf>();
                if (wolf != null)
                {
                    wolf.EquipBiteEffect(selectedBiteModifier);
                    Destroy(promptInstance);
                    Destroy(gameObject);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptInstance != null)
            {
                Destroy(promptInstance);
            }
        }
    }
}
