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
    public GameObject weaponPickupPrefab;
    public GameObject biteModifierPrefab;

    public int minEnemies => SpawnRateManager.Instance.minAmountOfEnemies;
    public int maxEnemies => SpawnRateManager.Instance.maxAmountOfEnemies;

    public int maxWaves = 2; // Number of waves in a room
    public int currentWave = 0;

    public int currentEnemies = 0;

    public List<SpawnerData> meleeEnemies; // List of melee enemies
    public List<SpawnerData> rangedEnemies; // List of ranged enemies

    public void StartSpawningEnemies(Room room)
    {
        currentWave = 0;

        // Ensure only 1 wave in the first couple of rooms
        if (RoomController.Instance.roomsCompleted < 3)
        {
            StartSingleWave(room);
        }
        else
        {
            StartNextWave(room);
        }
    }

    public void StartSingleWave(Room room)
    {
        // Only spawn a single wave in the first few rooms
        currentWave = 1;
        currentEnemies = 0;

        // Determine number of enemies for this wave 
        int numEnemies = Random.Range(minEnemies, maxEnemies);
        numEnemies = Mathf.Min(numEnemies, room.enemySpawnPoints.Length); 

        for (int i = 0; i < numEnemies; i++)
        {
            SpawnEnemy(room);
        }
    }

    public void StartNextWave(Room room)
    {
        // Stop spawning if maximum waves have been reached
        if (currentWave >= maxWaves) return;

        currentWave++;
        currentEnemies = 0;

        // Determine the number of enemies to spawn, ensuring it doesn't exceed the number of spawn points
        int numEnemies = Random.Range(minEnemies, maxEnemies + 1);
        numEnemies = Mathf.Min(numEnemies, room.enemySpawnPoints.Length);

        // Start coroutine to spawn enemies with a delay
        StartCoroutine(SpawnEnemiesWithDelay(room, numEnemies));
    }

    // Coroutine to spawn enemies one by one with a delay
    IEnumerator SpawnEnemiesWithDelay(Room room, int numEnemies)
    {
        for (int i = 0; i < numEnemies; i++)
        {
            SpawnEnemy(room); // Spawn a single enemy
            yield return new WaitForSeconds(0.6f); 
        }
    }


    // Spawn a single enemy at a random spawn point
    void SpawnEnemy(Room room)
    {
        if (room.enemySpawnPoints.Length == 0) return;

        Transform spawnPoint = room.enemySpawnPoints[Random.Range(0, room.enemySpawnPoints.Length)];

        // Filter valid enemy lists
        bool hasMeleeEnemies = meleeEnemies.Count > 0;
        bool hasRangedEnemies = rangedEnemies.Count > 0;

        // If no enemies exist, exit
        if (!hasMeleeEnemies && !hasRangedEnemies)
        {
            Debug.LogWarning($"No enemies available to spawn in room ({room.x}, {room.y}).");
            return;
        }

        // Randomly decide whether to spawn a melee or ranged enemy
        SpawnerData enemyToSpawn;
        if (Random.value > 0.5f && hasRangedEnemies)
        {
            enemyToSpawn = rangedEnemies[Random.Range(0, rangedEnemies.Count)];
        }
        else if (hasMeleeEnemies) // If melee enemies exist, spawn one
        {
            enemyToSpawn = meleeEnemies[Random.Range(0, meleeEnemies.Count)];
        }
        else // If melee enemies are not available, but ranged enemies are
        {
            enemyToSpawn = rangedEnemies[Random.Range(0, rangedEnemies.Count)];
        }

        GameObject enemy = Instantiate(enemyToSpawn.itemToSpawn, spawnPoint.position, Quaternion.identity);
        enemy.transform.SetParent(room.transform);

        // Register enemy death event
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.OnDeathEvent += () => OnEnemyDefeated(room);
        }

        currentEnemies++;
    }


    // Called when an enemy is defeated
    public void OnEnemyDefeated(Room room)
    {
        currentEnemies--;

        // If all enemies are defeated, check if another wave should start
        if (currentEnemies <= 0)
        {
            if (currentWave < maxWaves)
            {
                StartNextWave(room);
            }
            else
            {
                room.CheckRoomCompletion(); // Mark the room as completed
            }
        }
    }

    public void StartSpawningPickups(Room room)
    {
        if (room != null)
        {
            SpawnRandomPickup(room);
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

