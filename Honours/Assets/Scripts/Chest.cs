using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    Animator animator;
    public Room room;
    PlayerHealth playerHealth;
    public Transform spawnPoint;
    public GameObject weaponPickupPrefab;
    public GameObject healthPrefab;
    public GameObject weaponUpgradePrefab;

    bool playerInRange = false;
    public bool enemiesDefeated = false;

    public bool isRewardRoom = false;
    bool pickupSpawned = false;

    float itemSpawnDelay = 0.6f;

    public GameObject promptImage;

    void Start()
    {
        animator = GetComponent<Animator>();
        room = GetComponentInParent<Room>();
        playerHealth= FindObjectOfType<PlayerHealth>();

        // Hide prompt image
        if (promptImage != null)
        {
            promptImage.gameObject.SetActive(false); 
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Get the Switcher instance to check the character state
        Switcher switcher = FindObjectOfType<Switcher>();

        // Check if the player is in human form
        bool isPlayerInHumanForm = switcher.currentCharacterState == CharacterState.Player;

        // When the player is in range of the chest in the reward room, it automatically opens
        playerInRange = true;

        if (isRewardRoom && isPlayerInHumanForm)
        {
            OpenChest();
        }
        // Chest only opens when all enemies in the room are defeated and player is in range
        else if (room.hasSpawnedEnemies &&  isPlayerInHumanForm && room.AreAllEnemiesDefeated())
        {
            enemiesDefeated = true;
            OpenChest();
        }

        if (!isPlayerInHumanForm && promptImage != null)
        {
            bool shouldShowPrompt =
                (isRewardRoom) ||
                (room.hasSpawnedEnemies && room.AreAllEnemiesDefeated());

            if (shouldShowPrompt)
            {
                promptImage.gameObject.SetActive(true);
            }
        }
    }

    void OpenChest()
    {
        // Chest plays open animation and a pickup is spawned inside the chest
        if (!pickupSpawned)
        {
            animator.SetTrigger("openChest");
            pickupSpawned = true;
            Invoke("SpawnRandomPickup", itemSpawnDelay);
        }
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            promptImage.gameObject.SetActive(false);
        }
    }

    public void SetEnemiesDefeated(bool status)
    {
        enemiesDefeated = status;
        // Only attempt to open chest if player is in range
        if (enemiesDefeated && playerInRange)
        {
            OpenChest();

        }
    }


    void SpawnRandomPickup()
    {
        pickupSpawned = true;
        GameObject pickupPrefabToSpawn = null;
        float randomValue = Random.value;

        // Get player's current and max health
        float playerCurrentHealth = playerHealth.currentHealth;
        float playerMaxHealth = playerHealth.maxHealth;
        bool canSpawnHealth = (playerMaxHealth - playerCurrentHealth) >= 50;

        int roomsCompleted = RoomController.Instance.roomsCompleted;

        bool canSpawnWeaponPickup = roomsCompleted > 4;

        // Get the Player instance to check the weapon max status
        Player player = FindObjectOfType<Player>();
        bool isWeaponMaxedOut = player.IsWeaponMaxedOut();

        // Reward room logic
        if (isRewardRoom)
        {
            if (randomValue < 0.4f && canSpawnHealth) // 40% chance health
            {
                pickupPrefabToSpawn = healthPrefab;
            }
            else if (randomValue < 0.5f && !isWeaponMaxedOut) // 50% chance to spawn weapon upgrade
            {
                pickupPrefabToSpawn = weaponUpgradePrefab;
            }
            else
            {
                pickupPrefabToSpawn = weaponPickupPrefab; // Default to weapon pickup
            }
        }
        else
        {
            // Normal room logic
            if (randomValue < 0.5f && canSpawnWeaponPickup) // 50% chance to spawn weapon pickup if conditions are met
            {
                pickupPrefabToSpawn = weaponPickupPrefab;
            }
            else if (canSpawnHealth) // 20% chance to spawn health
            {
                pickupPrefabToSpawn = healthPrefab;
            }
            else if (randomValue < 0.8f && !isWeaponMaxedOut) // 30% chance to spawn weapon upgrade if no weapon pickup or health is eligible
            {
                pickupPrefabToSpawn = weaponUpgradePrefab;
            }
            else
            {
                pickupPrefabToSpawn = weaponPickupPrefab; // Default to weapon pickup if all else fails
            }
        }

        // If no valid pickup to spawn, exit
        if (pickupPrefabToSpawn == null)
        {
            return;
        }

        // Spawn selected pickup
        GameObject pickup = Instantiate(pickupPrefabToSpawn, spawnPoint.position, Quaternion.identity);
        pickup.transform.SetParent(room.transform);
    }
}
