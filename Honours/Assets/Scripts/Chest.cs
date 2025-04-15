using UnityEngine.InputSystem;
using UnityEngine;

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

    void Start()
    {
        animator = GetComponent<Animator>();
        room = GetComponentInParent<Room>();
        playerHealth= FindObjectOfType<PlayerHealth>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (isRewardRoom)
        {
            OpenChest();
        }
        else if (room.hasSpawnedEnemies && room.AreAllEnemiesDefeated())
        {
            enemiesDefeated = true;
            OpenChest();
        }
    }

    void OpenChest()
    {
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
        // Selects random pickup
        GameObject pickupPrefabToSpawn = null;
        float randomValue = Random.value;

        // Get player's current and max health
        float playerCurrentHealth = playerHealth.currentHealth;
        float playerMaxHealth = playerHealth.maxHealth;
        bool canSpawnHealth = (playerMaxHealth - playerCurrentHealth) >= 50;

        int roomsCompleted = RoomController.Instance.roomsCompleted; 

        // Check if player can spawn weapon pickups based on rooms completed
        bool canSpawnWeaponPickup = roomsCompleted > 4;

        // Reward room logic
        if (isRewardRoom)
        {
            if (randomValue < 0.4f && canSpawnHealth) // 40% chance health
            {
                pickupPrefabToSpawn = healthPrefab;
            }
            else
            {
                if (randomValue < 0.5f && RoomController.Instance.canSpawnUpgrade) // 50% chance to spawn weapon upgrade
                {
                    pickupPrefabToSpawn = weaponUpgradePrefab;
                }
                else
                {
                    pickupPrefabToSpawn = weaponPickupPrefab; // Default to weapon pickup if no health or upgrade chance
                }
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
            else if (randomValue < 0.8f && RoomController.Instance.canSpawnUpgrade) // 30% chance to spawn weapon upgrade if no weapon pickup or health is eligible
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
