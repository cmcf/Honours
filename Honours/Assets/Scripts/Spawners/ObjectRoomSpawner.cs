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
                SpawnEnemiesInRoom(room);
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


    void SpawnEnemiesInRoom(Room room)
    {
        // Checks if the room has any enemy spawn points
        if (room.enemySpawnPoints.Length == 0)
        {
            Debug.LogWarning("No enemy spawn points available.");
            return;
        }

        // Determine the number of enemies to spawn, based on the min/max values
        int spawnCount = Random.Range(minEnemies, maxEnemies + 1);

        // Get the current difficulty level to apply scaling
        int difficultyLevel = DifficultyManager.Instance.GetCurrentDifficultyLevel();

        // Create a temporary list of available spawn points
        List<Transform> availableSpawns = new List<Transform>(room.enemySpawnPoints);

        // Loop through and spawn enemies up to the determined spawn count
        for (int i = 0; i < spawnCount && availableSpawns.Count > 0; i++)
        {
            // Select a random spawn point from the list
            int randomIndex = Random.Range(0, availableSpawns.Count);
            Transform spawnPoint = availableSpawns[randomIndex];

            // Remove the used spawn point from the list to prevent multiple enemies from spawning at the same location
            availableSpawns.RemoveAt(randomIndex);

            // Ensures there is at least one enemy type available to spawn
            if (spawnerData.Length > 0)
            {
                // Choose a random enemy from the spawner data
                int randomEnemyIndex = Random.Range(0, spawnerData.Length);
                GameObject randomEnemy = spawnerData[randomEnemyIndex].spawnerData.itemToSpawn;

                // Instantiate the enemy at the selected spawn point
                GameObject spawnedEnemy = Instantiate(randomEnemy, spawnPoint.position, Quaternion.identity);

                // Set the spawned enemy's parent to the room
                spawnedEnemy.transform.SetParent(room.transform);

                // Apply difficulty scaling to the enemy 
                Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.SetDifficultyLevel(difficultyLevel);
                }
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
