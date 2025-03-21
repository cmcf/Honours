using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponPickup : MonoBehaviour
{
    public Weapon[] possibleWeapons;
    SpriteRenderer spriteRenderer;
    Weapon selectedWeapon;

    public float floatSpeed = 2f;
    public float floatAmount = 0.1f;

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

        // Filter out the current weapon from the possible weapons list
        List<Weapon> filteredWeapons = new List<Weapon>(possibleWeapons);
        if (currentWeapon != null)
        {
            filteredWeapons.RemoveAll(w => w.weaponName == currentWeapon.weaponName);
        }

        // Select a random weapon
        if (filteredWeapons.Count > 0)
        {
            selectedWeapon = filteredWeapons[Random.Range(0, filteredWeapons.Count)];
        }
        else
        {
            selectedWeapon = possibleWeapons[Random.Range(0, possibleWeapons.Length)];
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = selectedWeapon.weaponSprite;

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
                player.PickupWeapon(selectedWeapon);
                Destroy(controlPromptInstance); // Remove prompt
                Destroy(gameObject); // Remove pickup
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

    public Weapon GetWeapon()
    {
        return selectedWeapon;
    }
}
