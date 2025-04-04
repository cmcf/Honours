using UnityEngine.InputSystem;
using UnityEngine;

public class Chest : MonoBehaviour
{
    Animator animator;
    public Room room;
    public Transform spawnPoint;
    public GameObject weaponPickupPrefab;
    public GameObject biteModifierPrefab;
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
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (isRewardRoom)
            {
                animator.SetTrigger("openChest");

                if (!pickupSpawned)
                {
                    pickupSpawned = true;
                    Invoke("SpawnRandomPickup", itemSpawnDelay);
                }
            }
            else if (!isRewardRoom && room.AreAllEnemiesDefeated())
            {
                enemiesDefeated = room.AreAllEnemiesDefeated();
                if (enemiesDefeated && playerInRange)
                {
                    animator.SetTrigger("openChest");
                    if (!pickupSpawned)
                    {
                        pickupSpawned = true;
                        Invoke("SpawnRandomPickup", itemSpawnDelay);
                    }
                }
            }
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
            animator.SetTrigger("openChest");
            
        }
    }


    void SpawnRandomPickup()
    {
        pickupSpawned = true;
        // Selects random pickup
        GameObject pickupPrefabToSpawn;
        float randomValue = Random.value;


        if (isRewardRoom)
        {
            // Higher chance for health pickups in reward rooms
            if (randomValue < 0.4f) // 40% chance health
            {
                pickupPrefabToSpawn = healthPrefab;
            }
            else if (randomValue < 0.7f) // 30% chance weapon
            {
                pickupPrefabToSpawn = weaponPickupPrefab;
            }
            else // 30% chance bite modifier
            {
                pickupPrefabToSpawn = biteModifierPrefab;
            }
        }
        else
        {
            // Normal spawn chances
            if (randomValue < 0.5f) // 50% chance weapon
            {
                pickupPrefabToSpawn = weaponPickupPrefab;
            }
            else if (randomValue < 0.8f) // 30% chance bite modifier
            {
                pickupPrefabToSpawn = biteModifierPrefab;
            }
            else // 20% chance health
            {
                pickupPrefabToSpawn = healthPrefab;
            }
        }
        // Spawns the selected pickup
        GameObject pickup = Instantiate(pickupPrefabToSpawn, spawnPoint.position, Quaternion.identity);

        pickup.transform.SetParent(room.transform);
    }

}
