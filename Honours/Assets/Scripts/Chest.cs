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
        GameObject pickupPrefabToSpawn;
        float randomValue = Random.value;

        // Get player's current and max health
        float playerCurrentHealth = playerHealth.currentHealth;  
        float playerMaxHealth = playerHealth.maxHealth;

        // Condition to only spawn health if current health is 50 less than max health
        bool canSpawnHealth = (playerMaxHealth - playerCurrentHealth) >= 50;

        if (isRewardRoom)
        {
            // Higher chance for health pickups in reward rooms
            if (randomValue < 0.4f && canSpawnHealth) // 40% chance health 
            {
                pickupPrefabToSpawn = healthPrefab;
            }
            else 
            {
                pickupPrefabToSpawn = weaponPickupPrefab;
            }

        }
        else
        {
            if (randomValue < 0.5f) // 50% chance weapon
            {
                pickupPrefabToSpawn = weaponPickupPrefab;
            }

            else if (canSpawnHealth) // 20% chance health 
            {
                pickupPrefabToSpawn = healthPrefab;
            }
            else
            {
                pickupPrefabToSpawn = weaponPickupPrefab;
            }
        }

        // Spawns the selected pickup
        GameObject pickup = Instantiate(pickupPrefabToSpawn, spawnPoint.position, Quaternion.identity);
        pickup.transform.SetParent(room.transform);

    }
}
