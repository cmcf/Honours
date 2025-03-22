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
    public int minEnemies => SpawnRateManager.Instance.minAmountOfEnemies;
    public int maxEnemies => SpawnRateManager.Instance.maxAmountOfEnemies;

    public int maxWaves = 2; // Number of waves in a room
    public int currentWave = 0;

    public int currentEnemies = 0;

    public List<SpawnerData> meleeEnemies; // List of melee enemies
    public List<SpawnerData> rangedEnemies; // List of ranged enemies

    public int maxEnemiesPerRoom => SpawnRateManager.Instance.maxAmountOfEnemiesInRoom; // Maximum enemies allowed per room
    private int totalEnemiesSpawnedInRoom = 0; // Tracks the total number of enemies spawned in the current room

    public void StartSpawningEnemies(Room room)
    {
        currentWave = 0;
        totalEnemiesSpawnedInRoom = 0; // Reset enemy count for the new room

        // Ensure only 1 wave in the first few rooms
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
        currentWave = 1;
        currentEnemies = 0;

        // Determine number of enemies, ensuring we don't exceed the room's max limit
        int numEnemies = Random.Range(minEnemies, maxEnemies);
        numEnemies = Mathf.Min(numEnemies, room.enemySpawnPoints.Length); // Limit to available spawn points
        numEnemies = Mathf.Min(numEnemies, maxEnemiesPerRoom - totalEnemiesSpawnedInRoom); // Ensure room max is not exceeded

        for (int i = 0; i < numEnemies; i++)
        {
            SpawnEnemy(room);
        }
    }

    public void StartNextWave(Room room)
    {
        // Stop spawning if the max number of waves is reached or the room limit is hit
        if (currentWave >= maxWaves || totalEnemiesSpawnedInRoom >= maxEnemiesPerRoom) return;

        currentWave++;
        currentEnemies = 0;

        // Determine number of enemies, ensuring we don't exceed the room's max limit
        int numEnemies = Random.Range(minEnemies, maxEnemies + 1);
        numEnemies = Mathf.Min(numEnemies, room.enemySpawnPoints.Length); // Limit to available spawn points
        numEnemies = Mathf.Min(numEnemies, maxEnemiesPerRoom - totalEnemiesSpawnedInRoom); // Ensure room max is not exceeded

        // Start coroutine to spawn enemies with a delay
        StartCoroutine(SpawnEnemiesWithDelay(room, numEnemies));
    }

    IEnumerator SpawnEnemiesWithDelay(Room room, int numEnemies)
    {
        // Make sure to spawn all enemies first
        for (int i = 0; i < numEnemies; i++)
        {
            if (totalEnemiesSpawnedInRoom >= maxEnemiesPerRoom) yield break; // Stop spawning if max room limit is reached

            SpawnEnemy(room); // Spawn a single enemy
            yield return new WaitForSeconds(0.6f); // Delay between spawns
        }

        // Only check for room completion after all enemies are spawned
        if (currentEnemies <= 0 && totalEnemiesSpawnedInRoom == numEnemies)
        {
            room.CheckRoomCompletion(); // Mark the room as completed
        }
    }


    // Spawns a single enemy at a random spawn point
    void SpawnEnemy(Room room)
    {
        if (room.enemySpawnPoints.Length == 0 || totalEnemiesSpawnedInRoom >= maxEnemiesPerRoom) return; // Stop if no spawn points or max enemies reached

        Transform spawnPoint = room.enemySpawnPoints[Random.Range(0, room.enemySpawnPoints.Length)];

        // Check if we there are valid melee or ranged enemies available
        bool hasMeleeEnemies = meleeEnemies.Count > 0;
        bool hasRangedEnemies = rangedEnemies.Count > 0;

        // If no enemies are available, exit
        if (!hasMeleeEnemies && !hasRangedEnemies)
        {
            Debug.LogWarning($"No enemies available to spawn in room ({room.x}, {room.y}).");
            return;
        }

        // Ensure there is valid enemies before trying to spawn one
        SpawnerData enemyToSpawn = null;

        if (hasRangedEnemies && hasMeleeEnemies)
        {
            // Randomly choose between ranged or melee if both are available
            enemyToSpawn = (Random.value > 0.5f) ? rangedEnemies[Random.Range(0, rangedEnemies.Count)] : meleeEnemies[Random.Range(0, meleeEnemies.Count)];
        }
        else if (hasRangedEnemies)
        {
            // Only ranged enemies are available
            enemyToSpawn = rangedEnemies[Random.Range(0, rangedEnemies.Count)];
        }
        else if (hasMeleeEnemies)
        {
            // Only melee enemies are available
            enemyToSpawn = meleeEnemies[Random.Range(0, meleeEnemies.Count)];
        }
        else
        {
            // If no enemies are available, log a warning and return
            Debug.LogWarning($"No enemies available to spawn in room ({room.x}, {room.y}).");
            return;
        }

        // Instantiate enemy at the chosen spawn point
        GameObject enemy = Instantiate(enemyToSpawn.itemToSpawn, spawnPoint.position, Quaternion.identity);
        enemy.transform.SetParent(room.transform);

        // Register enemy death event to track when enemies are defeated
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.OnDeathEvent += () => OnEnemyDefeated(room);
        }

        currentEnemies++; // Increase current wave enemy count
        totalEnemiesSpawnedInRoom++; // Increase total enemies spawned in this room
    }

    // Called when an enemy is defeated
    public void OnEnemyDefeated(Room room)
    {
        currentEnemies--; // Reduce the current enemy count

        // If all enemies are defeated, check if another wave should start
        if (currentEnemies <= 0)
        {
            // If there's still another wave to spawn, do not check for room completion yet
            if (currentWave < maxWaves)
            {
                // Start the next wave if possible
                StartNextWave(room);
            }
            else
            {
                // Ensure we only check completion if no more enemies remain (even from previous waves)
                if (totalEnemiesSpawnedInRoom == currentWave) // Make sure all enemies have been spawned
                {
                    room.CheckRoomCompletion(); // Mark the room as completed only when all enemies are defeated
                }
            }
        }
    }
}

