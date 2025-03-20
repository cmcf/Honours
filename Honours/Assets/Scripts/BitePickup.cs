using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BitePickup : MonoBehaviour
{
    public BiteModifier[] possibleModifiers;
    SpriteRenderer spriteRenderer;
    BiteModifier selectedBiteModifier;

    public float floatSpeed = 2f;
    public float floatAmount = 0.1f;

    bool playerInRange = false;
    InputAction interactAction;

    void Start()
    {
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        interactAction = playerInput.actions["Interact"];

        // Randomly select a bite modifier
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
        }
    }

    public BiteModifier GetBiteModifier()
    {
        return selectedBiteModifier;
    }
}
