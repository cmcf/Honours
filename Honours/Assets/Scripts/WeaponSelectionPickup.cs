using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSelectionPickup : MonoBehaviour
{
    public Weapon weapon;
    SpriteRenderer spriteRenderer;
    bool playerInRange = false;
    InputAction interactAction;

    public GameObject promptPrefab;
    GameObject controlPromptInstance;

    void Start()
    {
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        interactAction = playerInput.actions["Interact"];

        Player player = FindObjectOfType<Player>();
        Weapon currentWeapon = null;

        if (player != null)
        {
            currentWeapon = player.GetCurrentWeapon();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = weapon.weaponSprite;

        if (promptPrefab != null)
        {
            promptPrefab.SetActive(false);
        }

    }

    void Update()
    {
        if (playerInRange && interactAction.triggered)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                player.PickupWeapon(weapon);
                Destroy(controlPromptInstance); // Remove prompt
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (promptPrefab != null)
            {
                promptPrefab.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (promptPrefab != null)
            {
                promptPrefab.SetActive(false);
            }

            if (controlPromptInstance != null)
            {
                Destroy(controlPromptInstance);
            }
        }
    }

}
