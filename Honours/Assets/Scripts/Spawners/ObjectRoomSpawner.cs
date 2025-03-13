using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRoomSpawner : MonoBehaviour
{
    [System.Serializable]

    
    public struct RandomSpawner
    {
        public string name;
        public SpawnerData spawnerData;
    }

    public GridController grid;
    public RandomSpawner[] spawnerData;
    public GameObject weaponPickupPrefab;
    public GameObject biteModifierPrefab;

    public int minEnemies => SpawnRateManager.Instance.minAmountOfEnemies;
    public int maxEnemies => SpawnRateManager.Instance.maxAmountOfEnemies;

    void Start()
    {
        grid = GetComponentInChildren<GridController>();
    }

    public void StartSpawningEnemies(Room room)
    {
        // Spawn enemies or objects only for the provided room
        if (room != null)
        {
            GridController gridController = room.GetComponentInChildren<GridController>();
            if (gridController != null && !room.hasSpawnedEnemies)
            {
                SpawnEnemiesForGrid(gridController, room);
                // Mark the room as spawned
                room.hasSpawnedEnemies = true;
            }
        }
    }

    public void StartSpawningPickups(Room room)
    {
        if (room != null)
        {
            GridController gridController = room.GetComponentInChildren<GridController>();
            if (gridController != null)
            {
                SpawnRandomPickup(room);
            }
        }
    }


    void SpawnEnemiesForGrid(GridController gridController, Room room)
    {
        // Check if there are available points in the grid
        if (gridController.availablePoints.Count == 0)
        {
            Debug.LogWarning("No available points to spawn enemies.");
            return;
        }

        // Decide on how many enemies to spawn
        int spawnCount = Random.Range(minEnemies, maxEnemies + 1);

        // Get the current difficulty level
        int difficultyLevel = DifficultyManager.Instance.GetCurrentDifficultyLevel();

        // Loop to spawn a limited number of enemies
        for (int i = 0; i < spawnCount; i++)
        {
            // Choose a random index from the available points
            int randomPointIndex = Random.Range(0, gridController.availablePoints.Count);

            // Get the random grid point
            Vector2 point = gridController.availablePoints[randomPointIndex];

            // Checks if there is any spawner data available
            if (spawnerData.Length > 0)
            {
                // Choose a random index from the spawnerData array
                int randomIndex = Random.Range(0, spawnerData.Length);

                // Get the random GameObject to spawn from the selected spawner data
                GameObject randomEnemy = spawnerData[randomIndex].spawnerData.itemToSpawn;

                // Instantiate the random GameObject at the grid point
                GameObject spawnedEnemy = Instantiate(randomEnemy, point, Quaternion.identity);

                // Set the room as the parent of the spawned enemy to keep the hierarchy clean
                spawnedEnemy.transform.SetParent(room.transform);

                // Reset the enemy's local position to ensure it's correctly placed within the room
                spawnedEnemy.transform.localPosition = point;

                // Apply difficulty-based adjustments to the spawned enemy
                Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.SetDifficultyLevel(difficultyLevel);
                }

                // Remove the used point to avoid spawning multiple enemies at the same location
                gridController.availablePoints.RemoveAt(randomPointIndex);
            }
        }
    }

    void SpawnRandomPickup(Room room)
    {
        if (room.spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points available for pickups.");
            return;
        }

        int randomIndex = Random.Range(0, room.spawnPoints.Length);
        Transform spawnPoint = room.spawnPoints[randomIndex];

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

        GameObject pickup = Instantiate(pickupPrefabToSpawn, spawnPoint.position, Quaternion.identity);

        pickup.transform.SetParent(room.transform);
        pickup.transform.localPosition = spawnPoint.localPosition;
    }



}
