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

    [SerializeField] float delayBetweenSpawns = 0.6f;

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

        // Determine number of enemies to ensure the room's max limit is not exceeded
        int numEnemies = Random.Range(minEnemies, maxEnemies);
        numEnemies = Mathf.Min(numEnemies, room.enemySpawnPoints.Count); // Limit to available spawn points
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

        int numEnemies = Random.Range(minEnemies, maxEnemies + 1);
        numEnemies = Mathf.Min(numEnemies, room.enemySpawnPoints.Count); // Limit to available spawn points
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
            yield return new WaitForSeconds(delayBetweenSpawns);
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
        if (room.enemySpawnPoints.Count == 0 || totalEnemiesSpawnedInRoom >= maxEnemiesPerRoom) return;

        // Ensure available spawn points list exists and refill if empty
        if (room.availableSpawnPoints == null || room.availableSpawnPoints.Count == 0)
        {
            room.availableSpawnPoints = new List<Transform>(room.enemySpawnPoints);
        }

        if (room.availableSpawnPoints.Count == 0) return;

        // Select and remove a random spawn point
        int randomIndex = Random.Range(0, room.availableSpawnPoints.Count);
        Transform spawnPoint = room.availableSpawnPoints[randomIndex];
        room.availableSpawnPoints.RemoveAt(randomIndex);

        // Enemy selection logic
        SpawnerData enemyToSpawn;
        bool hasMeleeEnemies = meleeEnemies.Count > 0;
        bool hasRangedEnemies = rangedEnemies.Count > 0;

        if (!hasMeleeEnemies && !hasRangedEnemies) return;

        if (hasMeleeEnemies && hasRangedEnemies)
        {
            enemyToSpawn = Random.value > 0.5f ?
                meleeEnemies[Random.Range(0, meleeEnemies.Count)] :
                rangedEnemies[Random.Range(0, rangedEnemies.Count)];
        }
        else
        {
            enemyToSpawn = hasRangedEnemies ?
                rangedEnemies[Random.Range(0, rangedEnemies.Count)] :
                meleeEnemies[Random.Range(0, meleeEnemies.Count)];
        }

        // Instantiate enemy
        GameObject enemy = Instantiate(enemyToSpawn.itemToSpawn, spawnPoint.position, Quaternion.identity);
        enemy.transform.SetParent(room.transform);

        // Register death event
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.OnDeathEvent += () => OnEnemyDefeated(room);
        }

        currentEnemies++;
        totalEnemiesSpawnedInRoom++;
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
                // Ensure room is checked after all enemies have spawned
                if (totalEnemiesSpawnedInRoom == currentWave) 
                {
                    room.CheckRoomCompletion(); 
                }
            }
        }
    }
}

