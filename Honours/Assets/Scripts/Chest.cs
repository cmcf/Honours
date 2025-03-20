using UnityEngine.InputSystem;
using UnityEngine;

public class Chest : MonoBehaviour
{
    Animator animator;
    public Room room;
    public Transform spawnPoint;
    public GameObject weaponPickupPrefab;
    public GameObject biteModifierPrefab;

    bool playerInRange = false;
    public bool enemiesDefeated = false;

    public bool isRewardRoom = false;
    bool pickupSpawned = false;

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
                    Invoke("SpawnRandomPickup", 0.6f);
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
                        Invoke("SpawnRandomPickup", 0.6f);
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

        if (randomValue < 0.5f)
        {
            pickupPrefabToSpawn = weaponPickupPrefab;
        }
        else
        {
            pickupPrefabToSpawn = biteModifierPrefab;
        }
        // Spawns the selected pickup
        GameObject pickup = Instantiate(pickupPrefabToSpawn, spawnPoint.position, Quaternion.identity);

        pickup.transform.SetParent(room.transform);
    }

}
